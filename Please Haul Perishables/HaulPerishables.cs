using RimWorld;
//using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PleaseHaulPerishables
{

	public class WorkGiver_HaulPerishables : WorkGiver_Haul
	{
		const int GridRadiusToSearch = 5;

		//const float DeteriorateThresh = 5f;

		const int DeteriorateDaysThresh = 10;

		const float IceBoxTempThresh = 9f;

		const float StackThreshPercent = 0.4f;

        const int DefaultDetDays = 99999;

		float GetDetDays(Thing detAble, Map map)
		{

            

			List<StatModifier> statBases = detAble.def.statBases;


			StatDef detRateStat = StatDefOf.DeteriorationRate;

            if (!statBases.StatListContains(detRateStat)) return DefaultDetDays;

            float detRate = statBases.GetStatValueFromList(detRateStat, 0f);
            if (detAble.def.thingCategories.Contains(ThingCategoryDefOf.Weapons)) detRate *= 10f;

            float rainRate = map.weatherManager.RainRate;
			detRate *= Mathf.Lerp(1f, 5f, rainRate);
			float detDays = 0f;

			if (detRate > 0f)
			{
				detDays = detAble.HitPoints / detRate;
			}
			else
			{
				detDays = DefaultDetDays;
			}

			return detDays;
		}

		List<Thing> ListPerishables(Pawn pawn)
		{


			var initialList = new List<Thing>();
			foreach (Thing potential in pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling())
			{

				float rotDays = RottableUtil.GetRotDays(potential);
				float detDays = GetDetDays(potential, pawn.Map);


				bool canDetOrRotSoon = !(detDays > DeteriorateDaysThresh && rotDays > GenDate.DaysPerYear);
				if (canDetOrRotSoon)
				{
					IntVec3 position = potential.Position;
					RoofDef roof = position.GetRoof(pawn.Map);
					bool canDetSoon = false || detDays <= DeteriorateDaysThresh;
					if (canDetSoon)
					{
						bool isRoofed = false || roof != null;
						if (!isRoofed)
						{
							initialList.Add(potential);
						}
					}
					else
					{
						bool canRotSoon = rotDays <= GenDate.DaysPerYear;
						if (canRotSoon)
						{
							initialList.Add(potential);
						}
					}
				}
			}


			var finalList = new List<Thing>();


			foreach (Thing rotOrDetHaulable in initialList)
			{
				if (finalList.Contains(rotOrDetHaulable)) continue;

				bool stacksToOne = rotOrDetHaulable.def.stackLimit == 1;

				if (stacksToOne)
				{
					finalList.Add(rotOrDetHaulable);
				}
				else
				{
					//finalList.Add(rotOrDetHaulable);
					float detDays = GetDetDays(rotOrDetHaulable, pawn.Map);
					var minNumToHaul = (int)(rotOrDetHaulable.def.stackLimit * StackThreshPercent);
					bool stackBelowHaulThresh = rotOrDetHaulable.stackCount < minNumToHaul;
					if (stackBelowHaulThresh && detDays > DeteriorateDaysThresh)
					{
						var dupeList = new List<Thing>();
						int numDupes = rotOrDetHaulable.stackCount;
						numDupes += DupeUtil.FindHowManyNearbyDupes(rotOrDetHaulable, GridRadiusToSearch, pawn,
																	out dupeList);
						bool stillBelowHaulThresh = numDupes < minNumToHaul;
						if (!stillBelowHaulThresh)
						{
							finalList.Add(rotOrDetHaulable);
							finalList.AddRange(dupeList);
						}
					}
					else
					{
						finalList.Add(rotOrDetHaulable);
					}
				}
			}


			return finalList;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			//return Pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling();
			return ListPerishables(pawn);
		}

		public override bool ShouldSkip(Pawn pawn)
		{
			return pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling().Count == 0;
		}

		bool TestStoreInColderPlace(Pawn p, Thing t, out IntVec3 storeCell)
		{
			storeCell = IntVec3.Invalid;
			Map map = p.Map;
			StoragePriority currentPriority = HaulAIUtility.StoragePriorityAtFor(t.Position, t);
            bool isThereBetterStoreCell = ModdedStoreUtil.TryFindBestBetterStoreCellFor(t, p, map,
                                                                                        currentPriority,
                                                                                        p.Faction, out IntVec3 searchCell,
                                                                                        true, true, true);

            if (!isThereBetterStoreCell)
			{
				return false;
			}

			storeCell = searchCell;
			return true;
		}




		bool TestStoreUnderRoof(Pawn p, Thing t, out IntVec3 storeCell)
		{
			storeCell = IntVec3.Invalid;
			Map map = p.Map;
			StoragePriority currentPriority = HaulAIUtility.StoragePriorityAtFor(t.Position, t);
			IntVec3 searchCell;
			bool isThereBetterStoreCell = ModdedStoreUtil.TryFindBestBetterStoreCellFor(t, p, map,
			                                                                             currentPriority,
			                                                                             p.Faction, out searchCell,
			                                                                             true, false, true);
			if (!isThereBetterStoreCell)
			{
				return false;
			}

			storeCell = searchCell;
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced)
		{
			bool canBeHauled = HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced);

			if (!canBeHauled) return null;

			Job result = null;

			bool invalidStackCount = t.stackCount == 0;

			if (invalidStackCount) return null;


			IntVec3 iceBoxCell = IntVec3.Invalid;
			IntVec3 roofedCell = IntVec3.Invalid;
			bool canFindColderPlace = false;
			bool canFindRoofedPlace = false;
			float rotDays = RottableUtil.GetRotDays(t);

			if (rotDays <= GenDate.DaysPerYear)
			{
				canFindColderPlace = TestStoreInColderPlace(pawn, t, out iceBoxCell);
			}

			canFindRoofedPlace = TestStoreUnderRoof(pawn, t, out roofedCell);

			if (canFindColderPlace) result = HaulTherePlease.TestHaulMaxNumToCellJob(pawn, t, iceBoxCell, true);

			if (canFindRoofedPlace) result = HaulTherePlease.TestHaulMaxNumToCellJob(pawn, t, roofedCell, false);

			//result = HaulTherePlease.HaulToStorageJob(pawn, t);

			return result;
		}





	}//End of class.


}//End of namespace.
