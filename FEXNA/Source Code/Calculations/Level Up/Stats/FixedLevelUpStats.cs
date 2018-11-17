using System.Linq;

namespace FEXNA.Calculations.LevelUp.Stats
{
    class FixedLevelUpStats : LevelUpStats
    {
        public FixedLevelUpStats(int[] statGains)
            : base(statGains.Select(x => 0).ToArray(),
                statGains,
                statGains.Select(x => false).ToArray()) { }

        protected override void RollGrowths(int[] growthRates)
        {
            PerfectLevel = false;
        }
    }
}