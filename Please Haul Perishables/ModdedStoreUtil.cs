//using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace PleaseHaulPerishables
{


	public static class RottableUtil
	{

		public static float GetRotDays(Thing t)
		{
			
			float result = 99999f;
			CompRottable compRottable = t.TryGetComp<CompRottable>();
			bool hasRottableComp = compRottable != null;
			if (hasRottableComp)
			{
				result = compRottable.TicksUntilRotAtCurrentTemp.TicksToDays();
			}
			return result;
		}
	}//End of class

	public static class DupeUtil
	{
		static void MakeSquareGridBasedOn(IntVec3 origin, Map m, int radius, out List<IntVec3> cellList)
		{
			var foundCellList = new List<IntVec3>();
			for (int i = -radius; i <= radius; i++)
			{
				for (int j = -radius; j <= radius; j++)
				{
					IntVec3 intVec = origin;
					intVec.x += i;
					intVec.z += j;

					bool isInMapBounds = intVec.InBounds(m);
					if (isInMapBounds)
					{
						foundCellList.Add(intVec);
					}
				}
			}
			cellList = foundCellList;
		}

        static void MakePlusGridBasedOn(IntVec3 origin, Map m, int lineLength, int lineWidth, out List<IntVec3> cellList)
        {
            var foundCellList = new List<IntVec3>();
            for (int i = -lineLength; i<= lineLength; i++)
            {
                for (int j = -lineWidth; j <= lineWidth; j++)
                {
                    IntVec3 intVec = origin;
                    intVec.x += i;
                    intVec.z += j;

                    bool isInMapBounds = intVec.InBounds(m);
                    if (isInMapBounds)
                    {
                        foundCellList.Add(intVec);
                    }

                    intVec = origin;
                    intVec.x += j;
                    intVec.z += i;

                    isInMapBounds = intVec.InBounds(m);
                    if (isInMapBounds && !foundCellList.Contains(intVec))
                    {
                        foundCellList.Add(intVec);
                    }
                }
            }

            cellList = foundCellList;
        }


        static int CountDupes(Thing t, List<IntVec3> cellList, Pawn carrier, bool forced, out List<Thing> dupesList)
        {
            var foundDupeList = new List<Thing>();
            foreach (IntVec3 c in cellList)
            {
                List<Thing> thingList = c.GetThingList(carrier.Map);
                if (thingList != null && thingList.Count != 0)
                {
                    foreach (Thing q in thingList)
                    {
                        if (q != null && q.def != null && q != t && q.def == t.def &&
                            HaulAIUtility.PawnCanAutomaticallyHaulFast(carrier, q, forced))
                        {
                            foundDupeList.Add(q);
                        }
                    }
                }
            }
            int totalStack = 0;

            for (int i = 0; i < foundDupeList.Count; i++)
            {
                totalStack += foundDupeList[i].stackCount;
            }

            dupesList = foundDupeList;

            return totalStack;
        }

        public static int FindDupesInLine(Thing t, int lineLength, int lineWidth, Pawn carrier, out List<Thing> dupesList)
        {
            
            var cellList = new List<IntVec3>();
            MakePlusGridBasedOn(t.Position, carrier.Map, lineLength, lineWidth, out cellList);
            var forced = false;

            int count = CountDupes(t, cellList, carrier, forced, out dupesList);

            return count;
        }

		public static int FindHowManyNearbyDupes(Thing t, int gridRadiusToSearch, Pawn carrier, out List<Thing> dupesList)
		{

			var cellList = new List<IntVec3>();
			MakeSquareGridBasedOn(t.Position, carrier.Map, gridRadiusToSearch, out cellList);
			var forced = false;

            int count = CountDupes(t, cellList, carrier, forced, out dupesList);

            return count;

        }
	}

	public static class ModdedStoreUtil
	{
		const float IceBoxTempThresh = 9f;
        
        const string ExtendedStorageStatName = "ES_StorageFactor";


        static bool IsColderEnoughAt(IntVec3 storeCell, Thing t, Map m)
		{
			float thereTemp = storeCell.GetTemperature(m);
			float hereTemp = t.Position.GetTemperature(m);
			float tempDiff = hereTemp - thereTemp;
			bool isColderEnough = thereTemp <= IceBoxTempThresh && tempDiff >= -3f && hereTemp > 0f;

			return isColderEnough;
		}

        // Attempted to add Extended Storage compatibility.
		static bool ContainsPartialThingWithSameDef(Thing t, IntVec3 c, Map m)
		{
			bool matchingStack = false;
            bool extendedStorageCell = false;
            bool matchingExtendedStorage = false;
            int totalExtendedStack = 0;
			var thingListDest = c.GetThingList(m);
			var tLimit = t.def.stackLimit;

            var storageExtended = c.GetFirstBuilding(m) as Building_Storage;

            if (storageExtended != null && StatDef.Named(ExtendedStorageStatName) != null)
            {
                var capacity = storageExtended.GetStatValue(StatDef.Named(ExtendedStorageStatName));

                tLimit = (int)(tLimit * capacity);
                extendedStorageCell = true;

            }

            foreach (Thing q in thingListDest)
			{
				if (q == null) continue;
				if (q.def == null) continue;
				if (q == t) continue;



				if (q.def == t.def && !extendedStorageCell)
				{
					matchingStack |= q.stackCount + t.stackCount <= tLimit;
				}

                if (q.def == t.def && extendedStorageCell)
                {
                    totalExtendedStack += q.stackCount;
                }

			}

            if (extendedStorageCell && totalExtendedStack < tLimit)
            {
                matchingExtendedStorage = true;
            }


			if (matchingStack || matchingExtendedStorage)
			{
				return true;
			}

			return false;
		}

		static bool ContainsThingWithSameDef(Thing t, IntVec3 c, Map m)
		{
			bool matchingStack = false;
			var thingListDest = c.GetThingList(m);

			foreach (Thing q in thingListDest)
			{
				if (q == null) continue;
				if (q.def == null) continue;
				if (q == t) continue;

				matchingStack |= q.def == t.def;
			}

			if (matchingStack)
			{
				return true;
			}

			return false;
		}

		public static bool TryFindBestBetterStoreCellFor(Thing t, Pawn carrier, Map map,
		                                                 StoragePriority currentPriority, Faction faction,
		                                                 out IntVec3 foundCell, bool needAccurateResult = true,
		                                                 bool mustBeColder = false, bool mustBeRoofed = true)
		{

			List<SlotGroup> allGroupsListInPriorityOrder = map.slotGroupManager.AllGroupsListInPriorityOrder;
			if (allGroupsListInPriorityOrder.Count == 0)
			{
				foundCell = IntVec3.Invalid;
				return false;
			}

            var searchSlots = new List<SlotGroup>();

            searchSlots = allGroupsListInPriorityOrder;

            IntVec3 a = (t.MapHeld == null) ? carrier.PositionHeld : t.PositionHeld;
			StoragePriority storagePriority = currentPriority;
			float maxDist = 2.14748365E+09f;
			IntVec3 bestCell = default(IntVec3);
			IntVec3 matchingCell = IntVec3.Invalid;
			bool cellDetected = false;
			int count = searchSlots.Count;
			for (int i = 0; i < count; i++)
			{
				SlotGroup slotGroup = searchSlots[i];
				StoragePriority priority = slotGroup.Settings.Priority;
				if (priority < storagePriority || priority <= currentPriority)
				{
					break;
				}

                if (t.Map != searchSlots[i].parent.Map) continue;

				if (slotGroup.Settings.AllowedToAccept(t))
				{
					var cells = slotGroup.CellsList;
					IEnumerable<IntVec3> cellsList = cells.InRandomOrder();

					int numSlotsToSearch;
					if (needAccurateResult)
					{
						numSlotsToSearch = Mathf.FloorToInt(cells.Count * Rand.Range(0.012f, 0.024f));
					}
					else
					{
						numSlotsToSearch = 0;
					}
					foreach (IntVec3 cell in cellsList)
					{

						if (mustBeColder && !IsColderEnoughAt(cell, t, map)) continue;

						if (mustBeRoofed && cell.GetRoof(map) == null) continue;

						float lengthHorizontalSquared = (a - cell).LengthHorizontalSquared;
						if (lengthHorizontalSquared <= maxDist)
						{
							if (StoreUtility.IsGoodStoreCell(cell, map, t, carrier, faction))
							{
								

								if (ContainsPartialThingWithSameDef(t, cell, map))
								{
									matchingCell = cell;
								}

								cellDetected = true;
								bestCell = cell;
								maxDist = lengthHorizontalSquared;
								storagePriority = priority;



								numSlotsToSearch--;

								if (numSlotsToSearch <= 0)
								{
									break;
								}
							}
						}
					}
				}
			}
			if (!cellDetected)
			{
				foundCell = IntVec3.Invalid;

                return false;

                

			}

			if (matchingCell.IsValid)
			{
				foundCell = matchingCell;
			}
			else
			{
				foundCell = bestCell;
			}


            return true;
		}

	}
}
