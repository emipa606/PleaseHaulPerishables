using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PleaseHaulPerishables;

public class WorkGiver_HaulFood : WorkGiver_Haul
{
    private const bool DisableStorageSearch = true;

    private const int FoodUpdateInterval = 272;

    private static readonly Dictionary<Map, List<Thing>> allFood = new();

    private static readonly Dictionary<Map, int> tickOfLastUpdate = new();

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling();
    }

    private static List<Thing> getFood(Pawn pawn)
    {
        if (RememberUtil.TryRetrieveThingListFromMapCache(pawn.Map, tickOfLastUpdate, allFood, out var returnList,
                FoodUpdateInterval))
        {
            return returnList;
        }

        var list = new List<Thing>();
        foreach (var item in pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling())
        {
            if (item.def.IsIngestible && !item.def.IsDrug)
            {
                list.Add(item);
            }
        }

        RememberUtil.TryUpdateListInMapCache(pawn.Map, tickOfLastUpdate, allFood, list);
        RememberUtil.FlushDataOfOldMaps(tickOfLastUpdate);
        RememberUtil.FlushDataOfOldMaps(allFood);
        return list;
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

        var food = getFood(pawn);
        if (food == null)
        {
            if (Perishables_Loader.settings.debug)
            {
                JobFailReason.Is("Can't haul as food, list is null.");
            }

            return null;
        }

        if (!food.Contains(t))
        {
            if (Perishables_Loader.settings.debug)
            {
                JobFailReason.Is("Can't haul as food, not on list.");
            }

            return null;
        }

        if (Perishables_Loader.settings.compatPickUpAndHaul)
        {
            if (Perishables_Loader.settings.debug)
            {
                Log.Message("PHP: Got a Job from Haul Haul Food/PUAH");
            }

            return HaulTherePlease.Compat_PickUpAndHaul(pawn, t, forced);
        }

        if (Perishables_Loader.settings.debug)
        {
            Log.Message("PHP: Got a Job from Haul Food");
        }

        return HaulAIUtility.HaulToStorageJob(pawn, t, forced);
    }
}