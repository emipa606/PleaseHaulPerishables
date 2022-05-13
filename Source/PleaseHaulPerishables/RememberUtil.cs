using System.Collections.Generic;
using Verse;

namespace PleaseHaulPerishables;

public static class RememberUtil
{
    public static bool TryRetrieveThingListFromMapCache(Map map, Dictionary<Map, int> tickOfLastUpdate,
        Dictionary<Map, List<Thing>> data, out List<Thing> returnList, int updateInterval = 240)
    {
        if (!tickOfLastUpdate.ContainsKey(map))
        {
            tickOfLastUpdate.Add(map, GenTicks.TicksGame);
        }
        else if (data.ContainsKey(map) && GenTicks.TicksGame < tickOfLastUpdate[map] + updateInterval)
        {
            returnList = data[map];
            return true;
        }

        returnList = null;
        return false;
    }

    public static void TryUpdateListInMapCache(Map map, Dictionary<Map, int> tickOfLastUpdate,
        Dictionary<Map, List<Thing>> data, List<Thing> newValue)
    {
        if (tickOfLastUpdate.ContainsKey(map))
        {
            tickOfLastUpdate[map] = GenTicks.TicksGame;
        }
        else
        {
            Log.Warning($"tickOfLastUpdate did not contain {map}");
        }

        if (data.ContainsKey(map))
        {
            data[map] = newValue;
        }
        else
        {
            data.Add(map, newValue);
        }
    }

    public static void FlushDataOfOldMaps(Dictionary<Map, int> dictToCheck)
    {
        foreach (var key in dictToCheck.Keys)
        {
            if (key == null)
            {
                dictToCheck.Remove(key);
            }
        }
    }

    public static void FlushDataOfOldMaps(Dictionary<Map, List<Thing>> dictToCheck)
    {
        foreach (var key in dictToCheck.Keys)
        {
            if (key == null)
            {
                dictToCheck.Remove(key);
            }
        }
    }
}