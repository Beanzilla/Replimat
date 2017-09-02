﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace Replimat
{
    public class WorkGiver_Warden_DeliverFoodReplimat : WorkGiver_Warden
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!base.ShouldTakeCareOfPrisoner(pawn, t))
            {
                return null;
            }
            Pawn pawn2 = (Pawn)t;
            if (!pawn2.guest.CanBeBroughtFood)
            {
                return null;
            }
            if (!pawn2.Position.IsInPrisonCell(pawn2.Map))
            {
                return null;
            }
            if (pawn2.needs.food.CurLevelPercentage >= pawn2.needs.food.PercentageThreshHungry + 0.02f)
            {
                return null;
            }
            if (WardenFeedUtility.ShouldBeFed(pawn2))
            {
                return null;
            }
            Thing thing;
            ThingDef def;
            if (!ReplimatUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out thing, out def, false, true, false, false, false))
            {
                return null;
            }
            if (thing.GetRoom(RegionType.Set_Passable) == pawn2.GetRoom(RegionType.Set_Passable))
            {
                return null;
            }
            if (WorkGiver_Warden_DeliverFoodReplimat.FoodAvailableInRoomTo(pawn2))
            {
                return null;
            }
            return new Job(ReplimatDef.deliverFoodReplimatDef, thing, pawn2)
            {
                count = ReplimatUtility.WillIngestStackCountOf(pawn2, def),
                targetC = RCellFinder.SpotToChewStandingNear(pawn2, thing)
            };
        }

        private static bool FoodAvailableInRoomTo(Pawn prisoner)
        {
            if (prisoner.carryTracker.CarriedThing != null && WorkGiver_Warden_DeliverFoodReplimat.NutritionAvailableForFrom(prisoner, prisoner.carryTracker.CarriedThing) > 0f)
            {
                return true;
            }
            float num = 0f;
            float num2 = 0f;
            Room room = prisoner.GetRoom(RegionType.Set_Passable);
            if (room == null)
            {
                return false;
            }
            for (int i = 0; i < room.RegionCount; i++)
            {
                Region region = room.Regions[i];
                List<Thing> list = region.ListerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
                for (int j = 0; j < list.Count; j++)
                {
                    Thing thing = list[j];
                    if (!thing.def.IsIngestible || thing.def.ingestible.preferability > FoodPreferability.DesperateOnly)
                    {
                        num2 += WorkGiver_Warden_DeliverFoodReplimat.NutritionAvailableForFrom(prisoner, thing);
                    }
                }
                List<Thing> list2 = region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                for (int k = 0; k < list2.Count; k++)
                {
                    Pawn pawn = list2[k] as Pawn;
                    if (pawn.IsPrisonerOfColony && pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry + 0.02f && (pawn.carryTracker.CarriedThing == null || !pawn.RaceProps.WillAutomaticallyEat(pawn.carryTracker.CarriedThing)))
                    {
                        num += pawn.needs.food.NutritionWanted;
                    }
                }
            }
            return num2 + 0.5f >= num;
        }

        private static float NutritionAvailableForFrom(Pawn p, Thing foodSource)
        {
            if (foodSource.def.IsNutritionGivingIngestible && p.RaceProps.WillAutomaticallyEat(foodSource))
            {
                return foodSource.def.ingestible.nutrition * (float)foodSource.stackCount;
            }
            if (p.RaceProps.ToolUser && p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                Building_ReplimatTerminal building_ReplimatTerminal = foodSource as Building_ReplimatTerminal;
                if (building_ReplimatTerminal != null && building_ReplimatTerminal.CanDispenseNow)
                {
                    return 99999f;
                }
            }
            return 0f;
        }
    }
}
