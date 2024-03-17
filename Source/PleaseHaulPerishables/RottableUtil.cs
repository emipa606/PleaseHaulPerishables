using RimWorld;
using Verse;

namespace PleaseHaulPerishables;

public static class RottableUtil
{
    public static bool ProtectedByEdifice(IntVec3 c, Map map)
    {
        var edifice = c.GetEdifice(map);
        return edifice?.def.building is { preventDeteriorationOnTop: true };
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