//using System;
using System.Collections.Generic;
//using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;


namespace PleaseHaulPerishables
{
	public class WorkGiver_HaulFood : WorkGiver_Haul
    {

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{

            var thingsToHaul = new List<Thing>();
            foreach (Thing t in pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling())
            {

                // We want to know if this item is food.
                if (!t.def.IsIngestible)
                {
                    continue;
                }
                // No beer or drugs please.
                if (t.def.IsDrug)
                {
                    continue;
                }
                if (!CanAnyoneEatThisIfPawnMovesIt(t, pawn))
                {
                    continue;
                }

                thingsToHaul.Add(t);

            }

            return thingsToHaul;


            //return pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling();
		}

		public override bool ShouldSkip(Pawn pawn)
		{
			return pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling().Count == 0;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced)
		{
			if (!HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced))
			{
				return null;
			}

            return HaulTherePlease.HaulToStorageJob(pawn, t);

		}

        bool CanAnyoneEatThisIfPawnMovesIt(Thing t, Pawn p)
        {
            // Is this a nice human meal?
            bool canHumanLikeThis = false;
            var foodPreferability = t.def.ingestible.preferability;
            canHumanLikeThis =
                foodPreferability != FoodPreferability.DesperateOnly
                && foodPreferability != FoodPreferability.RawBad
                && foodPreferability != FoodPreferability.Undefined
                && foodPreferability != FoodPreferability.NeverForNutrition;

            // We don't need to look through the whole list of animals if humans
            // like this food.
            if (canHumanLikeThis)
            {

                //If there's somewhere for this food to be, let's move it.
                return true;

            }

            // We want to know if there are any tame animals on the map
            // which can eat food t.
            //
            // So we look through the list of colony pawns and build a list
            // of animals that can eat food t.
            bool canPetsEatThis = false;
            var petFoodHaulCells = new List<IntVec3>();
            var colonyPawns = p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction);
            var animalPawns = new List<Pawn>();
            for (int a = 0; a < colonyPawns.Count; a++)
            {
                // And whether they will eat this as food.
                var pet = colonyPawns[a];
                if (CanPawnEatAnimalFeed(t, pet))
                {
                    animalPawns.Add(pet);
                }
            }

            // Now we cycle through the animal list and see if animals can eat this.
            // We only need to find one animal which eats this, hence the break.
            for (int n = 0; n < animalPawns.Count; n++)
            {
                var pet = animalPawns[n];
                if (ShouldPetFoodBeMoved(t, pet, p, out petFoodHaulCells))
                {
                    canPetsEatThis = true;
                    break;
                }

            }


            if (canPetsEatThis)
            {
                petFoodHaulCells.TryRandomElement(out IntVec3 randCell);
                // If there's a valid cell then haul there, saves doing a search twice.
                // Pet food doesn't need to all fit in the storage cell.
                if (randCell.IsValid)
                {
                    return true;
                }

            }

            return false;
        }
    

		bool CanPawnEatAnimalFeed(Thing t, Pawn animal)
		{
			// If this pawn is not mobile, they cannot feed themselves.
			// If this pawn is not an animal, then this is not applicable to them.
			// If this pawn does not eat this food type, this is not applicable to them.
			if (animal.health.State != PawnHealthState.Mobile
				|| !animal.RaceProps.Animal
				|| !animal.RaceProps.Eats(t.def.ingestible.foodType))
			{
				return false;
			}
			return true;
		}

		bool ShouldPetFoodBeMoved(Thing t, Pawn animal, Pawn colonist, out List<IntVec3> outcells)
		{
			// First establish whether the pawn a can actually reach food t at the moment.
			bool canReachNow = animal.CanReach(t, PathEndMode.OnCell, Danger.None, false, TraverseMode.ByPawn);
			bool couldReachNewStorage = false;

			var validDest = new List<IntVec3>();
			var dest = new List<IntVec3>();
			var slots = colonist.Map.slotGroupManager.AllGroupsListInPriorityOrder;

			// Now determine whether there is a new place which could take thing t, but
			// Pawn animal can still reach, and is a higher priority than the current
			// storage priority.
			for (int s = 0; s < slots.Count; s++)
			{
				var thisIsASlotGroup = slots[s];
				if (thisIsASlotGroup.Settings.AllowedToAccept(t))
				{
					if (CellsThatCanTakeThingForAnimal(thisIsASlotGroup.CellsList, t, animal, out dest))
					{
						var curPriority = StoragePriority.Unstored;
						if (t.GetSlotGroup() != null)
						{
							curPriority = t.GetSlotGroup().Settings.Priority;
						}

						var slotPriority = thisIsASlotGroup.Settings.Priority;
						if (canReachNow && curPriority >= slotPriority)
						{
							continue;
						}

						for (int ds = 0; ds < dest.Count; ds++)
						{
							// If animal can currently reach thing t, only move t
							// from low priority to higher priority storage.
							//
							// Implication: if animal can't reach thing t in low
							// priority storage, then higher priority and reachable
							// is good.


							// We should check that this is a place that Pawn colonist
							// can reserve and reach. 
							if (!colonist.CanReserveAndReach(dest[ds], PathEndMode.OnCell, Danger.None, 1))
							{
								continue;
							}

							validDest.Add(dest[ds]);
							couldReachNewStorage = true;
							break;
						}
						if (couldReachNewStorage)
						{
							break;
						}
					}
				}
			}
			outcells = validDest;

			// If there is a new cell to store t which animal can reach, return that cell.
			if (couldReachNewStorage)
			{
				return true;
			}
			
			return false;

		}//End of ShouldPetFoodBeMoved

		bool CellsThatCanTakeThingForAnimal(List<IntVec3> SlotCellList, Thing t, Pawn animal, out List<IntVec3> outcells)
		{
			var foundCells = new List<IntVec3>();
			var curMap = t.Map;
			// Cycle through all cells in SlotCellList to look for a valid cell.
			for (int e = 0; e < SlotCellList.Count; e++)
			{

				// Is there fire in this cell? Then continue iterating.
				if (SlotCellList[e].ContainsStaticFire(curMap))
				{
					continue;
				}
				// Cycle through thingList in this cell to see if there is
				// a Thing with a matching ThingDef.
				List<Thing> thingList = SlotCellList[e].GetThingList(curMap);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing t2 = thingList[i];
					if (t2.def.EverStoreable)
					{
						// If t does not have the same ThingDef as t2, keep iterating.
						if (t2.def != t.def)
						{
							continue;
						}
						// If t2 has a maxed stack, iterate next cell (hence the break).
						if (t2.stackCount >= t2.def.stackLimit)
						{
							break;
						}
						// If the animal can reach this cell, then it's a valid place
						// to drop off Thing t.
						// var targ = SlotCellList[e];
						if (animal.CanReach(SlotCellList[e], PathEndMode.OnCell, Danger.None, false, TraverseMode.ByPawn))
						{
							// Set return value and break out of iterating.
							foundCells.Add(SlotCellList[e]);
						}
					}
				}
			}

			outcells = foundCells;
			if (foundCells.Count > 0)
			{
				return true;
			}
			return false;
		}//End of CellsThatCanTakeThingForAnimal

	}//End of class
}//End of namespace

