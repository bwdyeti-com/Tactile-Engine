using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Effective_WT_Arrow : Weapon_Triangle_Arrow
    {
        readonly static Vector2 EFFECTIVE_OFFSET = new Vector2(-15, -5);

        protected Texture2D Effectiveness_Texture;
        private int Effectiveness;
        private bool HalvedEffectiveness;

        public Effective_WT_Arrow()
        {
            Effectiveness_Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Multipliers");
            set_effectiveness(1);
        }

        public override void set_effectiveness(int effectiveness, bool halved = false)
        {
            Effectiveness = effectiveness;
            HalvedEffectiveness = halved;
        }

        private Rectangle effectiveness_rect
        {
            get
            {
                return new Rectangle(
                    16 * (Effectiveness - 2), 16 + (HalvedEffectiveness ? 8 : 0), 16, 8);
            }
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.draw(sprite_batch, draw_offset);
            if (texture != null)
                if (visible && Effectiveness > 1)
                {
                    Rectangle src_rect = this.effectiveness_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(Effectiveness_Texture, (this.loc + draw_vector() + EFFECTIVE_OFFSET) - draw_offset,
                        src_rect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
