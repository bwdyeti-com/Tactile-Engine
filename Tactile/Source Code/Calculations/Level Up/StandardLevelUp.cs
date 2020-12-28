using System.Linq;
using Tactile.Calculations.LevelUp.Stats;

namespace Tactile.Calculations.LevelUp
{
    /// <summary>
    /// Rolls for each stat
    /// </summary>
    class StandardLevelUp : LevelUpProcessor
    {
        public StandardLevelUp(Game_Actor actor, int levelCount = 1)
            : base(actor, levelCount) { }

        protected override LevelUpStats GetLevel(
            int[] growths, int[] gainedStats, int[] currentLevelStats, bool atBaseLevel)
        {
            bool[] cappedStats = GetCappedStats(gainedStats);

            if (atBaseLevel && Global.ActorConfig.SemifixedFirstLevelUp)
            {
                var stats = new SemifixedLevelUpStats(
                    growths, currentLevelStats, cappedStats);
                return stats;
            }
            else
            {
#if DEBUG
                // In the unit editor, random level up stats are only used for the
                // leftover value, and use a fixed roll
                if (Global.scene.scene_type == "Scene_Map_Unit_Editor" && Actor.is_generic_actor)
                {
                    var stats = new Roll50LevelUpStats(growths, currentLevelStats, cappedStats);
                    return stats;
                }
                else
#endif
                if (Global.ActorConfig.NoEmptyLevels)
                {
                    var stats = new NoZeroLevelUpStats(growths, currentLevelStats, cappedStats);
                    return stats;
                }
                else
                {
                    var stats = new LevelUpStats(growths, currentLevelStats, cappedStats);
                    return stats;
                }
            }
        }
    }
}
