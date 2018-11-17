using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA.Constants
{
    public class WorldMap
    {
        public const int WORLDMAP_CONTROLS_FADE_TIME = 20;
        public const int WORLDMAP_FADE_TIME = 60;
        public const int WORLDMAP_ZOOM_FADE_TIME = 60;
        public const int WORLDMAP_SCROLL_SPEED = 16;
        public const int WORLDMAP_EVENT_SCROLL_SPEED = 8;
        public const int WORLDMAP_MODE_SWITCH_DELAY = 10;

        public const int WORLDMAP_TEXT_BOX_WIDTH = 280;

        public readonly static Vector2 WORLDMAP_EVENT_OFFSET = new Vector2(0, -24);
        public readonly static Vector2 WORLDMAP_MAP_OFFSET = new Vector2(0, 0);
        public readonly static Vector2 WORLDMAP_MINIMAP_SCALE = new Vector2(0.03f, 0.03f);
        public readonly static Vector2 WORLDMAP_MINIMAP_OFFSET = new Vector2(0, 0);
        public readonly static Vector2 WORLDMAP_MAP_SPRITE_OFFSET = new Vector2(16, 8);

        public readonly static List<string> HARD_MODE_BLOCKED = new List<string> { };

        public const string WORLDMAP_THEME = "A Hint of Things to Come";

        public const bool SEPARATE_CHAPTERS_INTO_ARCS = true;

        public readonly static List<string> GAME_ARCS = new List<string>
        {
            "Trial Maps",
        };
    }
}
