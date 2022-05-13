using RimWorld;
using UnityEngine;
using Verse;

namespace PleaseHaulPerishables;

public static class CapacityUtil
{
    public static int GetMinStackSizeToCarry(Pawn pawn, float carryCapacityThresh, int maxValue = 1)
    {
        if (!pawn.def.StatBaseDefined(StatDefOf.CarryingCapacity))
        {
            return maxValue;
        }

        var num = (int)pawn.GetStatValue(StatDefOf.CarryingCapacity);
        var num2 = (int)pawn.def.GetStatValueAbstract(StatDefOf.CarryingCapacity);
        return Mathf.Min(num, (int)(num2 * carryCapacityThresh), maxValue);
    }
}