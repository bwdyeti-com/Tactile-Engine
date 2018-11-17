using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA
{
    class Unit_Info_Hp_Gauge_Damage : Unit_Info_Hp_Gauge_Minimal
    {
        const int FLASH_DURATION = 64;
        private int DamageWidth;

        public Unit_Info_Hp_Gauge_Damage()
        {
            init_images();
            
            set_constants();
        }

        protected override void set_constants()
        {
            GAUGE_WIDTH = 32;
            TAG_LOC = new Vector2(4, 32);
            TEXT_LOC = new Vector2(31 + 8, 28);
            //TEXT_LOC = new Vector2(31 + 16 + GAUGE_WIDTH, 28); //Debug
            GAUGE_LOC = new Vector2(51 - 48, 34 + 8);
            MAX_OFFSET = new Vector2(20, 0);
        }

        public override void set_val(int hp, int maxhp)
        {
            set_val(hp, maxhp, default(Maybe<float>));
        }
        public void set_val(int hp, int maxhp, Maybe<float> ratio)
        {
            base.set_val(hp, maxhp);
            //float ratio = damage.ValueOrDefault / (float)maxhp; //Debug
            DamageWidth = 0;
            if (ratio.ValueOrDefault != 0)
                 DamageWidth = (int)MathHelper.Clamp(ratio * GAUGE_WIDTH, GAUGE_MIN, GAUGE_WIDTH);
            DamageWidth = Math.Min(DamageWidth, Width);
            if (ratio.IsSomething)
            {
                int damage = Math.Min((int)(ratio * 100), 999);
                Hp.text = string.Format(damage >= 100 ? "{0:0}" : "{0:0}%", damage);
            }
            else
                Hp.text = "---";
        }

        protected override void draw_tag(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector() - draw_offset;
            sprite_batch.Draw(texture, loc + TAG_LOC,
                new Rectangle(16, 64, 16, 8), tint, angle, offset, scale,
                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
        }

        protected override void draw_gauge(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector() - draw_offset;
            // Hp Gauge
            draw_gauge_bg(sprite_batch, loc);
            // Hp Gauge Fill
            draw_gauge_fill(sprite_batch, loc, Width - DamageWidth, this.tint);
            // Draw potential damage
            float alpha = ((float)Math.Sin(Global.game_system.total_play_time *
                MathHelper.TwoPi / (float)FLASH_DURATION) + 1) / 2;
            alpha = ((Global.game_system.total_play_time % FLASH_DURATION) /
                (float)FLASH_DURATION) * 2f - 1f;
            alpha = Math.Abs(alpha);
            alpha = (float)Math.Pow(alpha, 0.2f);

            Color tint = new Color(this.tint.R, this.tint.G / 4, this.tint.B / 4);
            tint *= alpha;
            tint.A = this.tint.A;
            draw_gauge_fill(sprite_batch,
                loc + new Vector2(Width - DamageWidth, 0), DamageWidth,tint);
        }

        protected override void draw_text(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector() - draw_offset;
            Hp.draw(sprite_batch, -(loc + TEXT_LOC - offset));
        }
    }
}
