using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Unit_Info_Exp_Gauge : Unit_Detail_Info_Hp_Gauge
    {
        protected bool No_Exp_Gauge;

        protected override void set_constants()
        {
            GAUGE_MIN = 0;
            GAUGE_WIDTH = 40;
            TAG_LOC = new Vector2(0, 40);
            TEXT_LOC = new Vector2(31, 36);
            GAUGE_LOC = new Vector2(51, 42);
            MAX_OFFSET = new Vector2(20, 0);
        }

        public void set_val(int exp, int level, bool no_gauge)
        {
            No_Exp_Gauge = no_gauge;
            if (No_Exp_Gauge)
            {
                MaxHp = new TextSprite();
                MaxHp.SetFont(Config.INFO_FONT, Global.Content);
                MaxHp.text = "---";
                MaxHp.offset = new Vector2(-9, 0);
            }
            else
            {
                float ratio = exp / (float)Global.ActorConfig.ExpToLvl;
                Width = (int)MathHelper.Clamp(ratio * GAUGE_WIDTH, GAUGE_MIN, GAUGE_WIDTH);
                MaxHp = new RightAdjustedText();
                MaxHp.SetFont(Config.INFO_FONT, Global.Content);
                MaxHp.text = exp.ToString();
            }
            Hp.text = level.ToString();
        }

        protected override void draw_tag(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector() - draw_offset;
            sprite_batch.Draw(texture, loc + TAG_LOC,
                new Rectangle(0, 48, 16, 8), tint, angle, offset, scale,
                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
        }

        protected override void draw_gauge(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector() - draw_offset;
            if (No_Exp_Gauge)
                return;
            // Exp Gauge
            sprite_batch.Draw(texture, loc + GAUGE_LOC,
                new Rectangle(16, 48, 2 + GAUGE_WIDTH, 5), tint, angle, offset, scale,
                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
            sprite_batch.Draw(texture, loc + GAUGE_LOC + new Vector2(2 + GAUGE_WIDTH, 0),
                new Rectangle(60, 48, 2, 5), tint, angle, offset, scale,
                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
            // Exp Gauge Fill
            for (int x = 0; x < Width; x++)
                sprite_batch.Draw(texture, loc + GAUGE_LOC + new Vector2(2 + x, 1),
                    new Rectangle(62, 48, 1, 2), tint, angle, offset, scale,
                    mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
        }

        protected override void draw_slash(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector() - draw_offset;
            sprite_batch.Draw(texture, loc + TEXT_LOC + new Vector2(0, 4),
                new Rectangle(64, 48, 8, 8), tint, angle, offset, scale,
                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
        }

        protected override void draw_text(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            draw_slash(sprite_batch, draw_offset);

            Vector2 loc = this.loc + draw_vector() - draw_offset;
            Hp.draw(sprite_batch, -(loc + TEXT_LOC - offset));
            if (Gauge_Visible)
                MaxHp.draw(sprite_batch, -(loc + TEXT_LOC + MAX_OFFSET - offset));
        }
    }
}
