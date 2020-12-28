using System.Linq;
using Tactile.Calculations.LevelUp.Stats;

namespace Tactile.Calculations.LevelUp
{
    /// <summary>
    /// Multiplies growth rate by levels gained, then rolls randomly on remainder
    /// </summary>
    class FixedLevelUp : StandardLevelUp
    {
        public FixedLevelUp(Game_Actor actor, int levelCount = 1)
            : base(actor, levelCount) { }

        protected override LevelUpStatSet GetLevels(int levelCount)
        {
            // Growth points gained in each stat; divide this by 100 for how many points to add
            int[] statPoints = GetGrowthPoints(levelCount);

            int[] growths = new int[statPoints.Length];
            int[] gainedStats = new int[statPoints.Length];
            int[] currentLevelStats = new int[statPoints.Length];
            int[] zeros = new int[statPoints.Length];

            // Get final growths and gains
            for (int i = 0; i < growths.Length; i++)
            {
                growths[i] += statPoints[i] % 100;
                gainedStats[i] = statPoints[i] / 100;
            }

            var levelUps = new LevelUpStatSet();

            // Add definite gains
            var definiteStats = new FixedLevelUpStats(gainedStats);
            levelUps.AddLevel(definiteStats);
            // Add leftover random gains
            var randomStats = GetLevel(growths, gainedStats, zeros, false);
            levelUps.AddLevel(randomStats);

            return levelUps;
        }

        private int[] GetGrowthPoints(int levelCount)
        {
            // Growth points gained in each stat; divide this by 100 for how many points to add
            int[] statPoints = new int[GetRawGrowths().Length];

            for (int level = 1; level <= levelCount; level++)
            {
                // Get growths
                int[] growths = GetRawGrowths();
                // Total stat gains
                int[] gainedStats = new int[statPoints.Length];
                for (int i = 0; i < growths.Length; i++)
                {
                    growths[i] += statPoints[i] % 100;
                    gainedStats[i] = statPoints[i] / 100;
                }

                // Stat points gained this level
                int[] currentLevelStats = new int[statPoints.Length];
                GetGrowths(growths, gainedStats, currentLevelStats);

                for (int i = 0; i < growths.Length; i++)
                    statPoints[i] = (gainedStats[i] + currentLevelStats[i]) * 100 + growths[i];
            }

            return statPoints;
        }

        protected override int MaxOverflowBonus()
        {
            int maxBonus = Global.ActorConfig.ConservedGrowthMaxPerStat;
            // Ignore overflow max for generics so min-maxed classes get a more fair result
            if (Actor.is_generic_actor)
                maxBonus = int.MaxValue;
            return maxBonus;
        }
    }
}
