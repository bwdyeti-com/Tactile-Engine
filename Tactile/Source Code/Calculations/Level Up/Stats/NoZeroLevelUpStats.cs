using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tactile.Calculations.LevelUp.Stats
{
    class NoZeroLevelUpStats : LevelUpStats
    {
        public NoZeroLevelUpStats(
                int[] growthRates, int[] statGains, bool[] cappedStats)
            : base(growthRates, statGains, cappedStats)
        {
            // On an empty level, gain a point in whatever stat had the best roll
            if (EmptyLevel)
            {
                // Get a random stat order, for tie breaking
                List<int> stats = Enumerable.Range(0, growthRates.Length).ToList();
                List<int> randomStatOrder = new List<int>();
                while (stats.Count > 0)
                {
                    int index = Global.game_system.get_rng() % stats.Count;
                    if (growthRates[index] != 0)
                        randomStatOrder.Add(stats[index]);
                    stats.RemoveAt(index);
                }

                if (randomStatOrder.Any())
                {
                    // Take the roll for each growth rate and get the difference
                    var rolls = new List<Tuple<int, int>>();
                    foreach (int j in randomStatOrder)
                    {
                        int roll = Rolls[j] - Math.Abs(growthRates[j]);
                        // Halve the chance of getting hp,
                        // since hp is worth half as much
                        rolls.Add(Tuple.Create(j,
                            roll * Game_Actor.GetStatRatio(j)));
                    }
                    // Sort by how close to the growth the roll was
                    rolls.Sort(delegate (Tuple<int, int> a, Tuple<int, int> b)
                    {
                        int sortValue = a.Item2 - b.Item2;
                        if (sortValue == 0)
                            return randomStatOrder.IndexOf(b.Item1);
                        return sortValue;
                    });

                    int freeStat = rolls[0].Item1;
                    StatGains[freeStat] += growthRates[freeStat] > 0 ? 1 : -1;
                }
            }
        }
    }
}
