using System;
using System.Collections.Generic;
using System.Linq;

namespace FEXNA.Calculations.LevelUp.Stats
{
    class SemifixedLevelUpStats : LevelUpStats
    {
        public SemifixedLevelUpStats(
            int[] growthRates, int[] statGains, bool[] cappedStats)
                : base(growthRates, statGains, cappedStats) { }

        protected override void RollGrowths(int[] growthRates)
        {
            // Semifixed levels can never be perfect
            PerfectLevel = false;

            // Separate stats into positive and negative
            var positiveStats = Enumerable
                .Range(0, growthRates.Length)
                .Where(x => growthRates[x] > 0);
            var negativeStats = Enumerable
                .Range(0, growthRates.Length)
                .Where(x => growthRates[x] < 0);
#if DEBUG
            positiveStats = positiveStats.ToList();
            negativeStats = negativeStats.ToList();
#endif

            // Get a random stat order, for tie breaking
            List<int> stats = Enumerable.Range(0, growthRates.Length).ToList();
            List<int> randomStatOrder = new List<int>();
            while (stats.Count > 0)
            {
                int index = Global.game_system.get_rng() % stats.Count;
                randomStatOrder.Add(stats[index]);
                stats.RemoveAt(index);
            }
            // Roll for each stat
            foreach (int i in randomStatOrder)
                Rolls[i] = Global.game_system.get_rng();
            

            RollSemifixedGrowths(
                growthRates, positiveStats, randomStatOrder, 1);
            RollSemifixedGrowths(
                growthRates, negativeStats, randomStatOrder, -1);
        }

        private void RollSemifixedGrowths(
            int[] growthRates,
            IEnumerable<int> stats,
            List<int> randomStatOrder,
            int multiplier)
        {
            // Skip if no stats to process
            if (!stats.Any())
                return;

            // Get totals
            float growthTotal = multiplier * stats
                .Select(x => growthRates[x] / (float)Game_Actor.GetStatRatio(x))
                .Sum();

            // Take the roll for each growth rate and get the difference
            var rolls = new List<Tuple<int, int>>();
            foreach (int j in stats)
            {
                rolls.Add(Tuple.Create(j,
                    Rolls[j] - (multiplier * growthRates[j])));
            }
            // Sort by how far under the growth the roll was
            rolls.Sort(delegate(Tuple<int, int> a, Tuple<int, int> b)
            {
                int sortValue = a.Item2 - b.Item2;
                if (sortValue == 0)
                    return randomStatOrder.IndexOf(b.Item1);
                return sortValue;
            });

            // Go through stats in the order they beat the rng by
            while (growthTotal > 0 && rolls.Any())
            {
                int stat = rolls[0].Item1;
                rolls.RemoveAt(0);
                float value = 1 / (float)Game_Actor.GetStatRatio(stat);
                float statWorth = value * 100;

                // IF the remaining points to gain are worth more than the current stat,
                // automatically grant the point
                if (growthTotal >= statWorth)
                {
                    StatGains[stat] += multiplier;
                    growthTotal -= statWorth;
                }
                // Else roll for this stat, with the probability being the percentage
                // of a point remaining
                else
                {
                    if (Global.game_system.get_rng() < growthTotal / value)
                        StatGains[stat] += multiplier;
                    growthTotal = 0;
                }
            }
        }
    }
}