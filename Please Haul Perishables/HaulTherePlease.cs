//using System;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PleaseHaulPerishables
{
	public static class HaulTherePlease
	{
		private static string ForbiddenLowerTrans = "ForbiddenLower".Translate();

		private static string ForbiddenOutsideAllowedAreaLowerTrans = "ForbiddenOutsideAllowedAreaLower".Translate();

		private static string PrisonerRoomLowerTrans = "PrisonerRoomLower".Translate();

		private static string BurningLowerTrans = "BurningLower".Translate();

		private static string NoEmptyPlaceLowerTrans = "NoEmptyPlaceLower".Translate();



		public static Job HaulToStorageJob(Pawn p, Thing t)
		{
			var currentPriority = HaulAIUtility.StoragePriorityAtFor(t.Position, t);
			IntVec3 storeCell;
			if (!StoreUtility.TryFindBestBetterStoreCellFor(t, p, p.Map, currentPriority, p.Faction, out storeCell, true))
			{
				JobFailReason.Is(NoEmptyPlaceLowerTrans);
				return null;
			}
			return TestHaulMaxNumToCellJob(p, t, storeCell, false);
		}

		public static Job TestHaulMaxNumToCellJob(Pawn p, Thing t, IntVec3 storeCell, bool fitInStoreCell)
		{
            bool debug = false;

            var startTime = Time.timeSinceLevelLoad;
            var endTime = 0f;


            var job = new Job(JobDefOf.HaulToCell, t, storeCell);
			var slotGroup = p.Map.slotGroupManager.SlotGroupAt(storeCell);
			if (slotGroup != null)
			{
				var thing = p.Map.thingGrid.ThingAt(storeCell, t.def);
				if (thing != null)
				{
					if (!StoreUtility.IsGoodStoreCell(storeCell, p.Map, t, p, p.Faction))
					{
						return null;
					}

					job.count = t.def.stackLimit;
					if (fitInStoreCell)
					{
						job.count -= thing.stackCount;
					}
				}
				else
				{
					job.count = 99999;
				}
				int num = 0;
				var statValue = p.GetStatValue(StatDefOf.CarryingCapacity, true);
				List<IntVec3> cellsList = slotGroup.CellsList;
				for (int i = 0; i < cellsList.Count; i++)
				{
					if (StoreUtility.IsGoodStoreCell(cellsList[i], p.Map, t, p, p.Faction))
					{

						if (!cellsList[i].InAllowedArea(p))
						{
							continue;
						}

						if (!p.CanReach(cellsList[i], PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
						{
							continue;
						}

						var thing2 = p.Map.thingGrid.ThingAt(cellsList[i], t.def);
						if (thing2 != null && thing2 != t)
						{
							num += Mathf.Max(t.def.stackLimit - thing2.stackCount, 0);
						}
						else
						{
							num += t.def.stackLimit;
						}
						if (num >= job.count || num >= statValue)
						{
							break;
						}
					}
				}
				job.count = Mathf.Min(job.count, num);

				if (job.count == 0)
				{



                    endTime = Time.timeSinceLevelLoad;

                    if (debug) Log.Message("TestHaulMaxNumToCellJob spent " + (endTime - startTime).ToString("0.0000") + " seconds making a job to haul " + t.Label + " but got a count of 0.");


                    return null;
				}
			}
			else
			{
				job.count = 99999;
			}
			job.haulOpportunisticDuplicates = true;
			job.haulMode = HaulMode.ToCellStorage;


            endTime = Time.timeSinceLevelLoad;

            if (debug) Log.Message("TestHaulMaxNumToCellJob made a job to haul " + t.Label + " in " + (endTime - startTime).ToString("0.0000") + " seconds.");



            return job;
		}


	}//End of class
}//End of namespace