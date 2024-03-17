using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PleaseHaulPerishables;

public static class DupeUtil
{
    private static void MakeSquareGridBasedOn(IntVec3 origin, Map m, int radius, out List<IntVec3> cellList)
    {
        var list = new List<IntVec3>();
        for (var i = -radius; i <= radius; i++)
        {
            for (var j = -radius; j <= radius; j++)
            {
                var intVec = origin;
                intVec.x += i;
                intVec.z += j;
                if (intVec.InBounds(m))
                {
                    list.Add(intVec);
                }
            }
        }

        cellList = list;
    }

    private static void MakePlusGridBasedOn(IntVec3 origin, Map m, int lineLength, int lineWidth,
        out List<IntVec3> cellList)
    {
        var list = new List<IntVec3>();
        for (var i = -lineLength; i <= lineLength; i++)
        {
            for (var j = -lineWidth; j <= lineWidth; j++)
            {
                var intVec = origin;
                intVec.x += i;
                intVec.z += j;
                if (intVec.InBounds(m))
                {
                    list.Add(intVec);
                }

                intVec = origin;
                intVec.x += j;
                intVec.z += i;
                if (intVec.InBounds(m) && !list.Contains(intVec))
                {
                    list.Add(intVec);
                }
            }
        }

        cellList = list;
    }

    private static int CountDupes(Thing t, List<IntVec3> cellList, Pawn carrier, bool forced, out List<Thing> dupesList)
    {
        var list = new List<Thing>();
        foreach (var cell in cellList)
        {
            var thingList = cell.GetThingList(carrier.Map);
            if (thingList == null || thingList.Count == 0)
            {
                continue;
            }

            foreach (var item in thingList)
            {
                if (item is { def: not null } && item != t && item.def == t.def &&
                    HaulAIUtility.PawnCanAutomaticallyHaulFast(carrier, item, forced))
                {
                    list.Add(item);
                }
            }
        }

        var num = 0;
        foreach (var thing in list)
        {
            num += thing.stackCount;
        }

        dupesList = list;
        return num;
    }

    public static int FindDupesInLine(Thing t, int lineLength, int lineWidth, Pawn carrier, out List<Thing> dupesList)
    {
        MakePlusGridBasedOn(t.Position, carrier.Map, lineLength, lineWidth, out var cellList);
        return CountDupes(t, cellList, carrier, false, out dupesList);
    }

    public static int FindHowManyNearbyDupes(Thing t, int gridRadiusToSearch, Pawn carrier, out List<Thing> dupesList)
    {
        MakeSquareGridBasedOn(t.Position, carrier.Map, gridRadiusToSearch, out var cellList);
        return CountDupes(t, cellList, carrier, false, out dupesList);
    }
}