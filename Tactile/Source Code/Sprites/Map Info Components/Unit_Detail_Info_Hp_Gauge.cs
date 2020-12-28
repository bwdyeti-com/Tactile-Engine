using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Unit_Detail_Info_Hp_Gauge : Unit_Info_Hp_Gauge
    {
        protected TextSprite Slash;

        public Unit_Detail_Info_Hp_Gauge()
        {
            init_images();
            Slash = new TextSprite();
            Slash.SetFont(Config.INFO_FONT, Global.Content);
            Slash.text = "/";
            set_constants();
        }

        protected virtual void set_constants()
        {
            GAUGE_WIDTH = 40;
            TAG_LOC = new Vector2(0, 32);
            TEXT_LOC = new Vector2(31, 28);
            GAUGE_LOC = new Vector2(51, 34);
            MAX_OFFSET = new Vector2(20, 0);
        }

        public override void set_val(int hp, int maxhp)
        {
            base.set_val(hp, maxhp);
            if (hp >= Math.Pow(10, Global.BattleSceneConfig.StatusHpCounterValues))
            {
                Hp.text = "--";
            }
            else
                Hp.text = hp.ToString();
        }

        protected override void draw_text(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            draw_slash(sprite_batch, draw_offset);
            base.draw_text(sprite_batch, draw_offset);
        }

        protected virtual void draw_slash(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector() - draw_offset;
            Slash.draw(sprite_batch, -(loc + TEXT_LOC + new Vector2(1, 0) - offset));
        }
    }
}
