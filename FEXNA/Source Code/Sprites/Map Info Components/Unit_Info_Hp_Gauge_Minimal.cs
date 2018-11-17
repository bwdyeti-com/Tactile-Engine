using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    class Unit_Info_Hp_Gauge_Minimal : Unit_Info_Hp_Gauge
    {
        public Unit_Info_Hp_Gauge_Minimal()
        {
            init_images();
            set_constants();
        }

        protected virtual void set_constants()
        {
            GAUGE_WIDTH = 40;
            TAG_LOC = new Vector2(0, 32);
            TEXT_LOC = new Vector2(31, 28);
            GAUGE_LOC = new Vector2(51 - 32, 34);
            MAX_OFFSET = new Vector2(20, 0);
        }

        protected override void draw_text(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2)) { }
    }
}
