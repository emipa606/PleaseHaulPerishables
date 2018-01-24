//using System;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PleaseHaulPerishables
{
	public class WorkGiver_HaulGeneralBigStacks : WorkGiver_Haul
	{
		const float StackThreshPercent = 0.6f;
		const int GridRadiusToSearch = 5;
        const float ValueThresh = 350;
        const int LineDupeLength = 9;
        const int LineDupeWidth = 1;

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
            var initiaList = pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling();
            var topOfList = new List<Thing>();
            var bottomOfList = new List<Thing>();
            foreach (Thing t in initiaList)
            {
                if (t is Corpse) continue;

                if (topOfList.Contains(t)) continue;

                if (bottomOfList.Contains(t)) continue;

                if (t.def.stackLimit == 1) continue;

                var valueFullStack = t.def.BaseMarketValue * t.def.stackLimit;

                if (valueFullStack > ValueThresh)
                {
                    topOfList.Add(t);
                    continue;
                }

                var desiredNumToHaul = t.def.stackLimit * StackThreshPercent;
                if (t.stackCount < desiredNumToHaul)
                {
                    var dupeList = new List<Thing>();
                    var numDupes = DupeUtil.FindHowManyNearbyDupes(t, GridRadiusToSearch, pawn, out dupeList);

                    if (t.stackCount + numDupes >= desiredNumToHaul)
                    {
                        topOfList.Add(t);
                        topOfList.AddRange(dupeList);
                        continue;
                    }

                    dupeList.Clear();
                    numDupes = DupeUtil.FindDupesInLine(t, LineDupeLength, LineDupeWidth, pawn, out dupeList);

                    if (t.stackCount + numDupes >= desiredNumToHaul)
                    {
                        topOfList.Add(t);
                        topOfList.AddRange(dupeList);
                    }
                    else
                    {
                        bottomOfList.Add(t);
                        bottomOfList.AddRange(dupeList);
                    }
                }
                else
                {
                    topOfList.Add(t);
                }
            }


            return topOfList;

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
    }
}
