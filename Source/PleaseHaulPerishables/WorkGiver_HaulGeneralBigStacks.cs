using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PleaseHaulPerishables;

public class WorkGiver_HaulGeneralBigStacks : WorkGiver_Haul
{
    private const float CarryCapacityThresh = 0.6f;

    private const int GridRadiusToSearch = 5;

    private const float ValueThresh = 350f;

    private const int LineDupeLength = 9;

    private const int LineDupeWidth = 1;

    private const int bigStacksUpdateInterval = 304;

    private static readonly Dictionary<Map, List<Thing>> allBigStacks = new Dictionary<Map, List<Thing>>();

    private static readonly Dictionary<Map, int> tickOfLastUpdate = new Dictionary<Map, int>();

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling();
    }

    private List<Thing> GetBigStacks(Pawn pawn)
    {
        if (RememberUtil.TryRetrieveThingListFromMapCache(pawn.Map, tickOfLastUpdate, allBigStacks, out var returnList,
                304))
        {
            return returnList;
        }

        var list = pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling();
        var list2 = new List<Thing>();
        var list3 = new List<Thing>();
        foreach (var item in list)
        {
            if (item is Corpse || list2.Contains(item) || list3.Contains(item) || item.def.stackLimit == 1)
            {
                continue;
            }

            if (item.def.BaseMarketValue * item.def.stackLimit > 350f)
            {
                list2.Add(item);
                continue;
            }

            var minStackSizeToCarry = CapacityUtil.GetMinStackSizeToCarry(pawn, 0.6f, 40);
            if (item.stackCount < minStackSizeToCarry)
            {
                var num = DupeUtil.FindHowManyNearbyDupes(item, 5, pawn, out var dupesList);
                if (item.stackCount + num >= minStackSizeToCarry)
                {
                    list2.Add(item);
                    list2.AddRange(dupesList);
                    continue;
                }

                dupesList.Clear();
                num = DupeUtil.FindDupesInLine(item, 9, 1, pawn, out dupesList);
                if (item.stackCount + num >= minStackSizeToCarry)
                {
                    list2.Add(item);
                    list2.AddRange(dupesList);
                }
                else
                {
                    list3.Add(item);
                    list3.AddRange(dupesList);
                }
            }
            else
            {
                list2.Add(item);
            }
        }

        RememberUtil.TryUpdateListInMapCache(pawn.Map, tickOfLastUpdate, allBigStacks, list2);
        RememberUtil.FlushDataOfOldMaps(tickOfLastUpdate);
        RememberUtil.FlushDataOfOldMaps(allBigStacks);
        return list2;
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        return pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling().Count == 0;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced))
        {
            return null;
        }

        var bigStacks = GetBigStacks(pawn);
        if (bigStacks == null)
        {
            if (Perishables_Loader.settings.debug)
            {
                JobFailReason.Is("Can't haul as big stack, list is null.");
            }

            return null;
        }

        if (!bigStacks.Contains(t))
        {
            if (Perishables_Loader.settings.debug)
            {
                JobFailReason.Is("Can't haul as big stack, not on list.");
            }

            return null;
        }

        if (Perishables_Loader.settings.compatPickUpAndHaul)
        {
            if (Perishables_Loader.settings.debug)
            {
                Log.Message("PHP: Got a Job from Haul BigStacks/PUAH");
            }

            return HaulTherePlease.Compat_PickUpAndHaul(pawn, t, forced);
        }

        if (Perishables_Loader.settings.debug)
        {
            Log.Message("PHP: Got a Job from Haul BigStacks");
        }

        return HaulAIUtility.HaulToStorageJob(pawn, t);
    }
}