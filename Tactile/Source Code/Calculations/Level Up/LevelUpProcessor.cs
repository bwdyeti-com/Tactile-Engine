using System;
using System.Collections.Generic;
using System.Linq;
using Tactile.Calculations.LevelUp.Stats;

namespace Tactile.Calculations.LevelUp
{
    abstract class LevelUpProcessor
    {
        const int LEVEL_UP_VIABLE_STATS = (int)Stat_Labels.Con; // Stats preceding this can go up on level

        protected Game_Actor Actor;
        private LevelUpStatSet LevelUps;

        internal LevelUpProcessor(Game_Actor actor)
        {
            Actor = actor;
        }
        public LevelUpProcessor(Game_Actor actor, int levelCount = 1)
        {
            Actor = actor;
            SetLevelUpData(levelCount);
         }

        protected void SetLevelUpData(int levelCount)
        {
            LevelUps = GetLevels(levelCount);
        }
        protected virtual LevelUpStatSet GetLevels(int levelCount)
        {
            bool firstLevelUp = Actor.gained_levels() == levelCount;

            var levelUps = new LevelUpStatSet();

            // Track the total stat gains, for checking if a stat caps early
            int[] gainedStats = new int[LEVEL_UP_VIABLE_STATS];

            for (int level = 1; level <= levelCount; level++)
            {
                // Stat points gained this level
                int[] currentLevelStats = new int[LEVEL_UP_VIABLE_STATS];

                int[] growths = GetGrowths(gainedStats, currentLevelStats);

                var stats = GetLevel(growths, gainedStats, currentLevelStats,
                    firstLevelUp && level == 1);

                for (int i = 0; i < gainedStats.Length; i++)
                    gainedStats[i] += stats.StatGains[i];

                levelUps.AddLevel(stats);
            }

            return levelUps;
        }

        protected int[] GetRawGrowths()
        {
            // This should have a parameter for simulating the current level of the
            // actor, so that if they unlock new skills that change growth rates
            // those skills are accounted for? @Debug

            // Actually sometimes the new level is set before the stats are gained,
            // and sometimes after, so this is already inconsistent @Debug

            // Get growths
            int[] rawGrowths = new int[LEVEL_UP_VIABLE_STATS];
            for (int i = 0; i < rawGrowths.Length; i++)
                rawGrowths[i] = Actor.get_growths(i);
            return rawGrowths;
        }

        protected int[] GetGrowths(
            int[] gainedStats, int[] currentLevelStats)
        {
            // Copy raw growths array
            int[] growths = GetRawGrowths();

            GetGrowths(growths, gainedStats, currentLevelStats);

            return growths;
        }
        protected void GetGrowths(
            int[] growths, int[] gainedStats, int[] currentLevelStats)
        {
            // Apply overflow
            if (Global.ActorConfig.ConserveWastedGrowths)
                GetGrowthOverflow(gainedStats, currentLevelStats, growths);
            else
            {
                // Set capped growths to 0 so they don't interfere with anything
                // that uses growth totals
                ClearCappedGrowths(gainedStats, currentLevelStats, growths);
            }
        }

        private void GetGrowthOverflow(
            int[] gainedStats, int[] currentLevelStats, int[] growths)
        {
            int unusedGrowth = 0;
            int uncappedStats = growths.Length;

            // Apply guaranteed growths and get the remainder
            for (int i = 0; i < growths.Length; i++)
            {
                for (; ; )
                {
                    bool guaranteedGain = growths[i] >= 100;
                    bool guaranteedLoss = growths[i] <= -100;
                    bool statCapped = Actor.get_capped(
                        i, gainedStats[i] + currentLevelStats[i]);

                    // If the growth is below -100, add 100 to the growth and subtract 1 from the stat
                    if (guaranteedLoss)
                    {
                        currentLevelStats[i]--;
                        growths[i] += 100;
                    }
                    // If the stat is already capped, add it to the excess growth pool and zero it out
                    else if (statCapped && growths[i] > 0)
                    {
                        unusedGrowth += growths[i] / Game_Actor.GetStatRatio(i);
                        uncappedStats--;
                        growths[i] = 0;
                    }
                    // If the growth is above 100, add 1 to the stat and subtract 100 from the growth
                    else if (guaranteedGain)
                    {
                        currentLevelStats[i]++;
                        growths[i] -= 100;
                    }
                    else
                        break;
                }
            }

            ProcessUnusedGrowths(gainedStats, currentLevelStats, growths, unusedGrowth, uncappedStats);
        }

        protected virtual void ProcessUnusedGrowths(
            int[] gainedStats, int[] currentLevelStats, int[] growths,
            int unusedGrowth, int uncappedStats)
        {
            int maxBonus = MaxOverflowBonus();

            // Divide leftover growths among remaining stats
            if (unusedGrowth > 0 && uncappedStats > 0)
            {
                for (int i = 0; i < growths.Length; i++)
                {
                    bool statCapped = Actor.get_capped(
                        i, gainedStats[i] + currentLevelStats[i]);

                    if (!statCapped)
                    {
                        int bonus = Math.Min(maxBonus, (unusedGrowth / uncappedStats));
                        growths[i] += bonus * Game_Actor.GetStatRatio(i);
                    }
                }
            }
        }

        protected virtual int MaxOverflowBonus()
        {
            return Global.ActorConfig.ConservedGrowthMaxPerStat;
        }

        private void ClearCappedGrowths(
            int[] gainedStats, int[] currentLevelStats, int[] growths)
        {
            for (int i = 0; i < growths.Length; i++)
            {
                if (growths[i] >= 0)
                {
                    // If the stat is already capped
                    if (Actor.get_capped(i, gainedStats[i] + currentLevelStats[i]))
                        growths[i] = 0;
                }
                else
                {
                    // If the stat is already 0
                    if (Actor.stat(i) + gainedStats[i] + currentLevelStats[i] == 0)
                        growths[i] = 0;
                }
            }
        }

        protected abstract LevelUpStats GetLevel(
            int[] growths, int[] gainedStats, int[] currentLevelStats, bool atBaseLevel);

        protected bool[] GetCappedStats(int[] gainedStats)
        {

            // Check if actor capped this stat before this level
            bool[] cappedStats = new bool[LEVEL_UP_VIABLE_STATS];
            for (int i = 0; i < cappedStats.Length; i++)
                cappedStats[i] = Actor.get_capped(i, gainedStats[i]);
            return cappedStats;
        }

        public virtual int StatCount { get { return LEVEL_UP_VIABLE_STATS; } }

        public int StatGain(Stat_Labels stat)
        {
            return LevelUps.StatGains(stat);
        }
        public bool StatChanged(Stat_Labels stat)
        {
            return LevelUps.StatGains(stat) != 0;
        }

        public void Apply()
        {
            for (int i = 0; i < LEVEL_UP_VIABLE_STATS; i++)
                Apply((Stat_Labels)i);
        }

        public void Apply(Stat_Labels stat)
        {
            Actor.gain_stat(stat, LevelUps.StatGains(stat));
        }
    }
}
