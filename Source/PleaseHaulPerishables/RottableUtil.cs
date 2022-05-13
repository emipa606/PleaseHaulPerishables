using RimWorld;
using Verse;

namespace PleaseHaulPerishables;

public static class RottableUtil
{
    public static bool ProtectedByEdifice(IntVec3 c, Map map)
    {
        var edifice = c.GetEdifice(map);
        if (edifice != null && edifice.def.building != null)
        {
            return edifice.def.building.preventDeteriorationOnTop;
        }

        return false;
    }

    public static float GetRotDays(Thing t)
    {
        var result = 99999f;
        var compRottable = t.TryGetComp<CompRottable>();
        if (compRottable != null)
        {
            result = compRottable.TicksUntilRotAtCurrentTemp.TicksToDays();
        }

        return result;
    }
}