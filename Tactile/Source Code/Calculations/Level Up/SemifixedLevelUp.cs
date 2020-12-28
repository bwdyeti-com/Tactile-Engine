using System.Linq;
using Tactile.Calculations.LevelUp.Stats;

namespace Tactile.Calculations.LevelUp
{
    /// <summary>
    /// Rolls for each stat, then increases a subset that were closest to succeeding
    /// </summary>
    class SemifixedLevelUp : LevelUpProcessor
    {
        public SemifixedLevelUp(Game_Actor actor, int levelCount = 1)
            : base(actor, levelCount) { }

        protected override LevelUpStats GetLevel(
            int[] growths, int[] gainedStats, int[] currentLevelStats, bool atBaseLevel)
        {
            bool[] cappedStats = GetCappedStats(gainedStats);

            var stats = new SemifixedLevelUpStats(growths, currentLevelStats, cappedStats);
            return stats;
        }
    }
}
