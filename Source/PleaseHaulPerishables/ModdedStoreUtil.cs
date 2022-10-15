using RimWorld;
using UnityEngine;
using Verse;

namespace PleaseHaulPerishables;

public static class ModdedStoreUtil
{
    private const float IceBoxTempThresh = 9f;

    private const string ExtendedStorageStatName = "ES_StorageFactor";

    private static bool IsColderEnoughAt(IntVec3 storeCell, Thing t, Map m)
    {
        var temperature = storeCell.GetTemperature(m);
        var temperature2 = t.Position.GetTemperature(m);
        var num = temperature2 - temperature;
        if (temperature <= 9f && num >= -3f)
        {
            return temperature2 > 0f;
        }

        return false;
    }

    private static bool ContainsPartialThingWithSameDef(Thing t, IntVec3 c, Map m)
    {
        var stackFull = false;
        var hasEsStorageFactor = false;
        var thereIsRoom = false;
        var num = 0;
        var thingList = c.GetThingList(m);
        var num2 = t.def.stackLimit;
        if (c.GetFirstBuilding(m) is Building_Storage thing &&
            DefDatabase<StatDef>.GetNamedSilentFail("ES_StorageFactor") != null)
        {
            var statValue = thing.GetStatValue(StatDef.Named("ES_StorageFactor"));
            num2 = (int)(num2 * statValue);
            hasEsStorageFactor = true;
        }

        foreach (var item in thingList)
        {
            if (item is not { def: { } } || item == t)
            {
                continue;
            }

            if (item.def == t.def && !hasEsStorageFactor)
            {
                stackFull |= item.stackCount + t.stackCount <= num2;
            }

            if (item.def == t.def && hasEsStorageFactor)
            {
                num += item.stackCount;
            }
        }

        if (hasEsStorageFactor && num < num2)
        {
            thereIsRoom = true;
        }

        return stackFull || thereIsRoom;
    }

    private static bool ContainsThingWithSameDef(Thing t, IntVec3 c, Map m)
    {
        var hasSameDef = false;
        foreach (var thing in c.GetThingList(m))
        {
            if (thing is { def: { } } && thing != t)
            {
                hasSameDef |= thing.def == t.def;
            }
        }

        return hasSameDef;
    }

    public static bool TryFindBestBetterStoreCellFor(Thing t, Pawn carrier, Map map, StoragePriority currentPriority,
        Faction faction, out IntVec3 foundCell, bool needAccurateResult = true, bool mustBeColder = false,
        bool mustProtectFromDeterioration = true)
    {
        var allGroupsListInPriorityOrder = map.haulDestinationManager.AllGroupsListInPriorityOrder;
        if (allGroupsListInPriorityOrder.Count == 0)
        {
            foundCell = IntVec3.Invalid;
            return false;
        }

        var foundPriority = currentPriority;
        var closestDistSquared = 2.1474836E+09f;
        var closestSlot = IntVec3.Invalid;
        var count = allGroupsListInPriorityOrder.Count;
        for (var i = 0; i < count; i++)
        {
            var slotGroup = allGroupsListInPriorityOrder[i];
            var priority = slotGroup.Settings.Priority;
            if ((int)priority < (int)foundPriority || (int)priority <= (int)currentPriority)
            {
                break;
            }

            if (t.Map != allGroupsListInPriorityOrder[i].parent.Map)
            {
                continue;
            }

            if (mustProtectFromDeterioration)
            {
                var room = allGroupsListInPriorityOrder[i].parent.Position.GetRoom(map);

                if (room is { UsesOutdoorTemperature: true } &&
                    !RottableUtil.ProtectedByEdifice(allGroupsListInPriorityOrder[i].parent.Position, map))
                {
                    continue;
                }
            }

            if (!mustBeColder || IsColderEnoughAt(allGroupsListInPriorityOrder[i].parent.Position, t, map))
            {
                TryFindBestBetterStoreCellForWorker(t, carrier, map, faction, slotGroup, needAccurateResult,
                    ref closestSlot, ref closestDistSquared, ref foundPriority);
            }
        }

        if (!closestSlot.IsValid)
        {
            foundCell = IntVec3.Invalid;
            return false;
        }

        foundCell = closestSlot;
        return true;
    }

    private static void TryFindBestBetterStoreCellForWorker(Thing t, Pawn carrier, Map map, Faction faction,
        SlotGroup slotGroup, bool needAccurateResult, ref IntVec3 closestSlot, ref float closestDistSquared,
        ref StoragePriority foundPriority)
    {
        if (!slotGroup.parent.Accepts(t))
        {
            return;
        }

        var intVec = !t.SpawnedOrAnyParentSpawned ? carrier.PositionHeld : t.PositionHeld;
        var cellsList = slotGroup.CellsList;
        var count = cellsList.Count;
        var num = needAccurateResult ? Mathf.FloorToInt(count * Rand.Range(0.005f, 0.018f)) : 0;
        for (var i = 0; i < count; i++)
        {
            var intVec2 = cellsList[i];
            float num2 = (intVec - intVec2).LengthHorizontalSquared;
            if (!(num2 <= closestDistSquared) || !StoreUtility.IsGoodStoreCell(intVec2, map, t, carrier, faction))
            {
                continue;
            }

            closestSlot = intVec2;
            closestDistSquared = num2;
            foundPriority = slotGroup.Settings.Priority;
            if (i >= num)
            {
                break;
            }
        }
    }
}