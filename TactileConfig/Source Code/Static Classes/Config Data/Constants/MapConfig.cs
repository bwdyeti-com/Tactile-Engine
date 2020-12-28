using System.Collections.Generic;
using TactileLibrary;

namespace Tactile.Constants
{
    public class Map
    {
        public readonly static int TILE_SIZE = 16;
        public readonly static int TILESET_TILE_SIZE = 16; // Total size of tiles on the tileset images, including gutters around them
        public const int UNIT_PIXEL_SIZE = 16; // Unit logical steps per pixel
        public readonly static int UNIT_TILE_SIZE = TILE_SIZE * UNIT_PIXEL_SIZE; // Unit logical steps per tile, should be larger than TILE_SIZE

#if XBOX
        public const int ALPHA_GRANULARITY = 1;
#else
        public const int ALPHA_GRANULARITY = 2;
#endif
        public const int ALPHA_MAX = 12 * ALPHA_GRANULARITY;

        public const bool FOW_TERRAIN_DATA = false; // Is terrain info data visible when moving the cursor on fog tiles, generally turn off only if doing Thracia fog

        public const int AI_WAIT_TIME = 20;
        public const int SKIP_AI_TURN_HOLD_TIME = 8;
        public const int SKIP_AI_SWTICH_TIME = 8;

        public const int SUSPEND_FADE_TIME = 20;

        public const float ENEMY_RANGE_TINT_MULT = 0.5f;
        public const int PASSIVE_MOVE_RANGE_OPACITY = 80;
        public const int ACTIVE_MOVE_RANGE_OPACITY = 144;
        public const int PASSIVE_FORMATION_MOVE_RANGE_OPACITY = 144;
        public const int ACTIVE_FORMATION_MOVE_RANGE_OPACITY = 255;

        public const int MAP_SPELL_DARKEN_MIN = 168;
        public const int MAP_SPELL_DARKEN_TIME = 16;

        public const bool RESCUED_TERRAIN_HEAL = true; // Can terrain (forts) heal rescued units?
        public const bool RESCUER_TERRAIN_HEAL_FULL_HP_DISPLAY = true; // If terrain is healing rescued and the rescuer is at full, should their data be displayed?

        public const int TURN_SKIP_TEXT_FLASH_TIME = 60;

        public const int MAP_TRANSITION_TIME = 20;

        public const int DESTROYABLE_OBJECT_TEAM = Constants.Team.INTRUDER_TEAM;

        public const bool FORCE_BOSS_ANIMATIONS = true;
#if DEBUG
        public const bool CONTINUE_BOSS_THEME = false; // Boss themes continue on the map after combat?
#else
        public const bool CONTINUE_BOSS_THEME = false; // Boss themes continue on the map after combat?
#endif
    }
}
