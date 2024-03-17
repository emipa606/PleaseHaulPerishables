using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PleaseHaulPerishables;

public class WorkGiver_HaulPerishables : WorkGiver_Haul
{
    private const bool DisableFreezerSearch = true;

    private const int GridRadiusToSearch = 5;

    private const int DeteriorateDaysThresh = 10;

    private const float IceBoxTempThresh = 9f;

    private const float CarryCapacityThresh = 0.4f;

    private const int DefaultDetDays = 99999;

    private const int PerishablesUpdateInterval = 240;

    private static readonly Dictionary<Map, List<Thing>> allPerishables = new Dictionary<Map, List<Thing>>();

    private static readonly Dictionary<Map, int> tickOfLastUpdate = new Dictionary<Map, int>();

    private float GetDetDays(Thing detAble, Map map)
    {
        var statBases = detAble.def.statBases;
        if (RottableUtil.ProtectedByEdifice(detAble.Position, map))
        {
            return 99999f;
        }

        var deteriorationRate = StatDefOf.DeteriorationRate;
        if (!statBases.StatListContains(deteriorationRate))
        {
            return 99999f;
        }

        var num = statBases.GetStatValueFromList(deteriorationRate, 0f);
        if (num <= 0f)
        {
            return 99999f;
        }

        if (detAble.def.IsWeapon)
        {
            num *= 10f;
        }

        if (detAble.def.IsApparel)
        {
            num *= 10f;
        }

        var rainRate = map.weatherManager.RainRate;
        var num2 = 0.5f;
        if (!detAble.Position.Roofed(map))
        {
            num *= Mathf.Lerp(1f, 5f, rainRate);
            num2 += 0.5f;
        }

        var terrain = detAble.Position.GetTerrain(map);
        if (terrain is { extraDeteriorationFactor: > 0f })
        {
            num2 += terrain.extraDeteriorationFactor;
        }

        num *= num2;
        if (num > 0f)
        {
            return detAble.HitPoints / num;
        }

        return 99999f;
    }

    private bool GetIsPerishable(Thing t, Map m)
    {
        var rotDays = RottableUtil.GetRotDays(t);
        var detDays = GetDetDays(t, m);
        if (detDays > 10f && rotDays > 60f)
        {
            return false;
        }

        var usesOutdoorTemperature = false;
        var invalid = t.Position;
        if (invalid.GetRoom(m) != null)
        {
            usesOutdoorTemperature = invalid.GetRoom(m).UsesOutdoorTemperature;
        }

        if (detDays <= 10f)
        {
            if (usesOutdoorTemperature)
            {
                return true;
            }
        }
        else if (rotDays <= 60f)
        {
            return true;
        }

        return false;
    }

    private List<Thing> ListPerishables(Pawn pawn)
    {
        if (RememberUtil.TryRetrieveThingListFromMapCache(pawn.Map, tickOfLastUpdate, allPerishables,
                out var returnList))
        {
            return returnList;
        }

        var list = new List<Thing>();
        foreach (var item in pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling())
        {
            if (item is not Corpse && GetIsPerishable(item, pawn.Map))
            {
                list.Add(item);
            }
        }

        var list2 = new List<Thing>();
        foreach (var item2 in list)
        {
            if (list2.Contains(item2))
            {
                continue;
            }

            if (item2.def.stackLimit == 1)
            {
                list2.Add(item2);
                continue;
            }

            var detDays = GetDetDays(item2, pawn.Map);
            var minStackSizeToCarry = CapacityUtil.GetMinStackSizeToCarry(pawn, 0.4f, 40);
            if (item2.stackCount < minStackSizeToCarry && detDays > 10f)
            {
                if (item2.stackCount +
                    DupeUtil.FindHowManyNearbyDupes(item2, GridRadiusToSearch, pawn, out var dupesList) <
                    minStackSizeToCarry)
                {
                    continue;
                }

                list2.Add(item2);
                list2.AddRange(dupesList);
            }
            else
            {
                list2.Add(item2);
            }
        }

        RememberUtil.TryUpdateListInMapCache(pawn.Map, tickOfLastUpdate, allPerishables, list2);
        RememberUtil.FlushDataOfOldMaps(tickOfLastUpdate);
        RememberUtil.FlushDataOfOldMaps(allPerishables);
        return list2;
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling();
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        return pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling().Count == 0;
    }

    private bool TestStoreInColderPlace(Pawn p, Thing t, out IntVec3 storeCell)
    {
        storeCell = IntVec3.Invalid;
        var map = p.Map;
        var currentPriority = StoreUtility.StoragePriorityAtFor(t.Position, t);
        if (!ModdedStoreUtil.TryFindBestBetterStoreCellFor(t, p, map, currentPriority, p.Faction, out var foundCell,
                true, true))
        {
            return false;
        }

        storeCell = foundCell;
        return true;
    }

    private bool TestStoreInside(Pawn p, Thing t, out IntVec3 storeCell)
    {
        storeCell = IntVec3.Invalid;
        var map = p.Map;
        var currentPriority = StoreUtility.StoragePriorityAtFor(t.Position, t);
        if (!ModdedStoreUtil.TryFindBestBetterStoreCellFor(t, p, map, currentPriority, p.Faction, out var foundCell))
        {
            return false;
        }

        storeCell = foundCell;
        return true;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is Corpse)
        {
            if (Perishables_Loader.settings.debug)
            {
                JobFailReason.Is("Can't haul as perishable, corpses are handled by another WorkGiver.");
            }

            return null;
        }

        if (!HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced))
        {
            return null;
        }

        Job job = null;
        if (t.stackCount == 0)
        {
            return null;
        }

        var list = ListPerishables(pawn);
        if (list == null)
        {
            if (Perishables_Loader.settings.debug)
            {
                JobFailReason.Is("Can't haul as perishable, list is null.");
            }

            return null;
        }

        if (!list.Contains(t))
        {
            if (Perishables_Loader.settings.debug)
            {
                JobFailReason.Is("Can't haul as perishable, not on list.");
            }

            return null;
        }

        if (Perishables_Loader.settings.compatPickUpAndHaul)
        {
            if (Perishables_Loader.settings.debug)
            {
                Log.Message("PHP: Got a Job from Haul Perishables/PUAH");
            }

            return HaulTherePlease.Compat_PickUpAndHaul(pawn, t, forced);
        }

        RottableUtil.GetRotDays(t);
        var num = TestStoreInside(pawn, t, out var storeCell);

        if (num)
        {
            job = HaulAIUtility.HaulToCellStorageJob(pawn, t, storeCell, false);
        }

        if (job == null && Perishables_Loader.settings.debug)
        {
            JobFailReason.Is("Can't haul as perishable, no suitable indoor place.");
        }

        return job;
    }
}