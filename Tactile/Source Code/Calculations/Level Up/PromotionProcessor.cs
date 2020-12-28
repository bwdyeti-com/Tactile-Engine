using System.Collections.Generic;
using System.Linq;
using Tactile.Calculations.LevelUp.Stats;

namespace Tactile.Calculations.LevelUp
{
    class PromotionProcessor : LevelUpProcessor
    {
        private int OldClassId;
        private int _StatCount;

        public PromotionProcessor(Game_Actor actor, int oldClassId)
            : base(actor)
        {
            OldClassId = oldClassId;
            SetLevelUpData(0);
        }

        protected override Stats.LevelUpStats GetLevel(
            int[] growths, int[] gainedStats, int[] currentLevelStats, bool atBaseLevel)
        {
            return null;
        }

        protected override LevelUpStatSet GetLevels(int levelCount)
        {
            int[] promotionGains = Game_Actor.promotion_bonuses(
                OldClassId, Actor.class_id)
                    .ToArray();
            _StatCount = promotionGains.Length;

            var levelUps = new LevelUpStatSet();
            levelUps.AddLevel(new FixedLevelUpStats(promotionGains));
            return levelUps;
        }

        public override int StatCount { get { return _StatCount; } }
    }
}
