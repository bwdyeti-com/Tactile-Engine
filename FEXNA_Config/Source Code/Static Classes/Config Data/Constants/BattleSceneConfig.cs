using System.Collections.Generic;

namespace FEXNA.Constants
{
    public class BattleScene
    {
        public const int HP_COUNTER_VALUES = 2; // Battle scene, map battle
        public const int STATUS_HP_COUNTER_VALUES = 2; // Status screen, unit detail, combat preview
        public const int MAX_HP_ROWS = 2;
        public const int HP_TABS_PER_ROW = 50;
        public const int HPGAUGE_TAB_WIDTH = 3;
        public const int HPGAUGE_TAB_HEIGHT = 6;
        public const float BATTLER_MIN_SCALE = 0.33f; // Battler minimum scale when it zooms in from the map sprite position

        public const int ACTION_BACKGROUND_TONE_WEIGHT = 192;
        public const int ACTION_PLATFORM_TONE_WEIGHT = 160;
        public const int ACTION_BATTLER_TONE_WEIGHT = 64;

        public const int BATTLE_TRANSITION_TIME = 30;
        public const bool BATTLE_BG_ALWAYS_VISIBLE = true;

        public const int ARENA_BG_TIME = 30, ARENA_BG_FRAMES = 3;
    }
}
