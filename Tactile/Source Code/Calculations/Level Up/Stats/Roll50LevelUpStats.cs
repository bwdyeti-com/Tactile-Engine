#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tactile.Calculations.LevelUp.Stats
{
    class Roll50LevelUpStats : LevelUpStats
    {
        public Roll50LevelUpStats(int[] growthRates, int[] statGains, bool[] cappedStats)
            : base(growthRates, statGains, cappedStats) { }

        protected override void RollGrowths(int[] growthRates)
        {
            // Roll for each growth rate to change stats
            for (int i = 0; i < StatGains.Length; i++)
            {
                Rolls[i] = 49;
                if (growthRates[i] >= 0)
                {
                    if (growthRates[i] >= 50)
                        StatGains[i]++;
                    else
                        PerfectLevel = false;
                }
                else
                {
                    if (-growthRates[i] >= 50)
                        StatGains[i]--;
                }
            }
        }
    }
}
#endif