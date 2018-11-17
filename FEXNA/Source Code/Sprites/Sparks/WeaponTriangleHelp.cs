using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    abstract class WeaponTriangleHelp : Sprite
    {
        protected Sprite Icon1, Icon2, Icon3;
        private Weapon_Triangle_Arrow Arrow1, Arrow2, Arrow3;

        public WeaponTriangleHelp()
        {
            Arrow1 = advantage_arrow(MathHelper.TwoPi * 3 / 12);
            Arrow2 = advantage_arrow(MathHelper.TwoPi * 7 / 12);
            Arrow3 = advantage_arrow(MathHelper.TwoPi * 11 / 12);
        }

        protected virtual Weapon_Triangle_Arrow advantage_arrow(float angle)
        {
            var result = new Weapon_Triangle_Arrow();
            result.value = WeaponTriangle.Disadvantage;
            result.offset = new Vector2(
                result.src_rect.Width / 2, result.src_rect.Height / 2);
            result.angle = angle;
            result.loc = new Vector2(0, 8) + 12 * new Vector2(
                (float)Math.Cos(angle), (float)Math.Sin(angle));
            return result;
        }

        public override void update()
        {
            if (Icon1 != null)
            {
                Icon1.update();
                if (Icon2 != null)
                    Icon2.update();
                if (Icon3 != null)
                    Icon3.update();
            }

            Arrow1.update();
            Arrow2.update();
            Arrow3.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (Icon1 != null)
            {
                Vector2 offset = this.loc + draw_vector();

                Icon1.draw(sprite_batch, draw_offset - offset);
                if (Icon2 != null)
                {
                    Icon2.draw(sprite_batch, draw_offset - offset);
                    Arrow3.draw(sprite_batch, draw_offset - offset);
                }
                if (Icon3 != null)
                {
                    Icon3.draw(sprite_batch, draw_offset - offset);
                    Arrow2.draw(sprite_batch, draw_offset - offset);

                    if (Icon2 != null)
                        Arrow1.draw(sprite_batch, draw_offset - offset);
                }
            }
        }
    }
}
