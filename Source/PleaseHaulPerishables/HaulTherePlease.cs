using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace PleaseHaulPerishables;

public static class HaulTherePlease
{
    private static string ForbiddenLowerTrans = "ForbiddenLower".Translate();

    private static string ForbiddenOutsideAllowedAreaLowerTrans = "ForbiddenOutsideAllowedAreaLower".Translate();

    private static string PrisonerRoomLowerTrans = "PrisonerRoomLower".Translate();

    private static string BurningLowerTrans = "BurningLower".Translate();

    private static readonly string NoEmptyPlaceLowerTrans = "NoEmptyPlaceLower".Translate();

    public static Job Compat_PickUpAndHaul(Pawn pawn, Thing thing, bool forced)
    {
        try
        {
            var typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("PickUpAndHaul.WorkGiver_HaulToInventory");
            if (typeInAnyAssembly == null)
            {
                return null;
            }

            if (!typeof(WorkGiver_HaulGeneral).IsAssignableFrom(typeInAnyAssembly))
            {
                throw new Exception("Expected work giver to extend WorkGiver_HaulGeneral");
            }

            if (typeInAnyAssembly.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new Exception("Expected work giver to have parameterless constructor");
            }

            var workGiver_HaulGeneral = (WorkGiver_HaulGeneral)Activator.CreateInstance(typeInAnyAssembly);
            if (workGiver_HaulGeneral.ShouldSkip(pawn, forced))
            {
                return null;
            }

            if (Perishables_Loader.settings.debug)
            {
                Log.Message($"PHP: {pawn} Got a Job, Hauling: {thing}from Haul There Please/PUAH");
            }

            return workGiver_HaulGeneral.JobOnThing(pawn, thing, forced);
        }
        catch (Exception ex)
        {
            Log.Error($"Please Haul Perishables: Caught exception when trying to get Pick Up and Haul job. {ex}");
            return null;
        }
    }

    public static Job HaulToStorageJob(Pawn p, Thing t)
    {
        var currentPriority = StoreUtility.StoragePriorityAtFor(t.Position, t);
        if (!StoreUtility.TryFindBestBetterStoreCellFor(t, p, p.Map, currentPriority, p.Faction, out var foundCell))
        {
            JobFailReason.Is(NoEmptyPlaceLowerTrans);
            return null;
        }

        if (Perishables_Loader.settings.debug)
        {
            Log.Message($"PHP: {p} Got a Job, Hauling: {t}from Haul There Please");
        }

        return HaulAIUtility.HaulToCellStorageJob(p, t, foundCell, false);
    }
}