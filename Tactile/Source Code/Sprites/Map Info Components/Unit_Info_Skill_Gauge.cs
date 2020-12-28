using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Unit_Info_Skill_Gauge : Unit_Info_Exp_Gauge
    {
        public void set_val(float skill_percent, int level, bool no_gauge)
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
                Width = (int)MathHelper.Clamp(skill_percent * GAUGE_WIDTH, GAUGE_MIN, GAUGE_WIDTH);
            Hp.text = level.ToString();
        }

        protected override void draw_slash(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector() - draw_offset;
            sprite_batch.Draw(texture, loc + TEXT_LOC + new Vector2(0, 4),
                new Rectangle(0, 56, 24, 8), tint, angle, offset, scale,
                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
        }

        protected override void draw_text(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            draw_slash(sprite_batch, draw_offset);

            Vector2 loc = this.loc + draw_vector() - draw_offset;
            Hp.draw(sprite_batch, -(loc + TEXT_LOC - offset));
            if (No_Exp_Gauge)
                MaxHp.draw(sprite_batch, -(loc + TEXT_LOC + MAX_OFFSET - offset));
        }
    }
}
