using System;
using System.Collections.Generic;
using System.Linq;

namespace FEXNA.Calculations.LevelUp.Stats
{
    class LevelUpStats
    {
        protected int[] Rolls;
        public int[] StatGains { get; protected set; }
        public bool PerfectLevel { get; protected set; }
        public bool EmptyLevel { get; private set; }

        public override string ToString()
        {
            string result = string.Format(
                "LevelUpStats: {0} point change", Enumerable
                    .Range(0, StatGains.Length)
                    .Sum(i => StatGains[i] / (float)Game_Actor.get_stat_ratio(i)));
            if (PerfectLevel)
                result += "; Perfect level";
            else if (EmptyLevel)
                result += "; Empty level";

            return result;
        }

        public LevelUpStats(int[] growthRates, int[] statGains, bool[] cappedStats)
        {
            growthRates = growthRates
                .Select(x => x)
                .ToArray();

            Rolls = new int[statGains.Length];
            StatGains = statGains
                .Select(x => x)
                .ToArray();

            PerfectLevel = true;

            ApplyGuaranteedGrowths(growthRates);
            RollGrowths(growthRates);

            // Unless a stat isn't capped and has a nonzero change, there were no gains
            EmptyLevel = true;
            for (int i = 0; i < StatGains.Length; i++)
            {
                if (!cappedStats[i] && StatGains[i] != 0)
                {
                    EmptyLevel = false;
                    break;
                }
            }
        }

        private void ApplyGuaranteedGrowths(int[] growthRates)
        {
            for (int i = 0; i < StatGains.Length; i++)
            {
                // Positive growth
                if (growthRates[i] >= 0)
                    // While rate is 100 or greater
                    while (growthRates[i] >= 100)
                    {
                        StatGains[i]++;
                        growthRates[i] -= 100;
                    }
                // Negative growth
                else
                {
                    // If any stat has negative growths,
                    // the actor can't get "perfect" levels
                    PerfectLevel = false;

                    // While rate is -100 or less
                    while (growthRates[i] <= -100)
                    {
                        StatGains[i]--;
                        growthRates[i] += 100;
                    }
                }
            }
        }

        protected virtual void RollGrowths(int[] growthRates)
        {
            // Roll for each growth rate to change stats
            for (int i = 0; i < StatGains.Length; i++)
            {
#if DEBUG
                // In the unit editor, random level up stats are only used for the
                // leftover value, and use a fixed roll
                if (Global.scene.scene_type == "Scene_Map_Unit_Editor")
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
                else
#endif
                {
                    int rate = Math.Abs(growthRates[i]);
                    int roll;
                    if (Global.game_system.roll_rng(growthRates[i], out roll))
                    {
                        if (growthRates[i] >= 0)
                            StatGains[i]++;
                        else
                            StatGains[i]--;
                    }
                    else
                    {
                        // Ignore 0 growths when checking for perfect levels I guess
                        if (growthRates[i] != 0)
                            PerfectLevel = false;
                    }
                    // Save the rolled rn
                    Rolls[i] = roll;
                }
            }
        }
    }
}