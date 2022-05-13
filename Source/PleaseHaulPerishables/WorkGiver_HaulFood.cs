using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PleaseHaulPerishables;

public class WorkGiver_HaulFood : WorkGiver_Haul
{
    private const bool DisableStorageSearch = true;

    private const int FoodUpdateInterval = 272;

    private static readonly Dictionary<Map, List<Thing>> allFood = new Dictionary<Map, List<Thing>>();

    private static readonly Dictionary<Map, int> tickOfLastUpdate = new Dictionary<Map, int>();

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling();
    }

    public List<Thing> GetFood(Pawn pawn)
    {
        if (RememberUtil.TryRetrieveThingListFromMapCache(pawn.Map, tickOfLastUpdate, allFood, out var returnList, 272))
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

        var food = GetFood(pawn);
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

        return HaulAIUtility.HaulToStorageJob(pawn, t);
    }

    private bool CanAnyoneEatThisIfPawnMovesIt(Thing t, Pawn p)
    {
        var preferability = t.def.ingestible.preferability;
        if (preferability != FoodPreferability.DesperateOnly && preferability != FoodPreferability.RawBad &&
            preferability != 0 && preferability != FoodPreferability.NeverForNutrition)
        {
            return true;
        }

        var movePetFood = false;
        var outcells = new List<IntVec3>();
        var list = p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction);
        var list2 = new List<Pawn>();
        foreach (var pawn in list)
        {
            if (CanPawnEatAnimalFeed(t, pawn))
            {
                list2.Add(pawn);
            }
        }

        foreach (var animal in list2)
        {
            if (!ShouldPetFoodBeMoved(t, animal, p, out outcells))
            {
                continue;
            }

            movePetFood = true;
            break;
        }

        if (!movePetFood)
        {
            return false;
        }

        outcells.TryRandomElement(out var result);
        if (result.IsValid)
        {
            return true;
        }

        return false;
    }

    private bool CanPawnEatAnimalFeed(Thing t, Pawn animal)
    {
        if (animal.health.State != PawnHealthState.Mobile || !animal.RaceProps.Animal ||
            !animal.RaceProps.Eats(t.def.ingestible.foodType))
        {
            return false;
        }

        return true;
    }

    private bool ShouldPetFoodBeMoved(Thing t, Pawn animal, Pawn colonist, out List<IntVec3> outcells)
    {
        var foundCell = false;
        var list = new List<IntVec3>();
        var allGroupsListInPriorityOrder = colonist.Map.haulDestinationManager.AllGroupsListInPriorityOrder;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < allGroupsListInPriorityOrder.Count; i++)
        {
            var slotGroup = allGroupsListInPriorityOrder[i];
            if (!slotGroup.Settings.AllowedToAccept(t) ||
                !CellsThatCanTakeThingForAnimal(slotGroup.CellsList, t, animal, out var outcells2))
            {
                continue;
            }

            var storagePriority = StoragePriority.Unstored;
            if (t.GetSlotGroup() != null)
            {
                storagePriority = t.GetSlotGroup().Settings.Priority;
            }

            var priority = slotGroup.Settings.Priority;
            if (animal.CanReach((LocalTargetInfo)t, PathEndMode.OnCell, Danger.None) &&
                (int)storagePriority >= (int)priority)
            {
                continue;
            }

            foreach (var intVec3 in outcells2)
            {
                if (!colonist.CanReserveAndReach(intVec3, PathEndMode.OnCell, Danger.None))
                {
                    continue;
                }

                list.Add(intVec3);
                foundCell = true;
                break;
            }

            if (foundCell)
            {
                break;
            }
        }

        outcells = list;
        if (foundCell)
        {
            return true;
        }

        return false;
    }

    private bool CellsThatCanTakeThingForAnimal(List<IntVec3> SlotCellList, Thing t, Pawn animal,
        out List<IntVec3> outcells)
    {
        var list = new List<IntVec3>();
        var map = t.Map;
        for (var i = 0; i < SlotCellList.Count; i++)
        {
            if (SlotCellList[i].ContainsStaticFire(map))
            {
                continue;
            }

            var thingList = SlotCellList[i].GetThingList(map);
            foreach (var thing in thingList)
            {
                if (!thing.def.EverHaulable || thing.def != t.def)
                {
                    continue;
                }

                if (thing.stackCount >= thing.def.stackLimit)
                {
                    break;
                }

                if (animal.CanReach(SlotCellList[i], PathEndMode.OnCell, Danger.None))
                {
                    list.Add(SlotCellList[i]);
                }
            }
        }

        outcells = list;
        if (list.Count > 0)
        {
            return true;
        }

        return false;
    }
}