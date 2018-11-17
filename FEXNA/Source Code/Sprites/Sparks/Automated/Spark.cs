using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Spark : Sprite
    {
        protected int[] Timer_Maxes = { };
        const int Timer_Max = 10;
        protected Vector2 Frames = new Vector2(1, 1);
        protected bool Loop = false;

        protected int Frame = 0;
        protected int Timer = 0;

        public virtual bool completed()
        {
            if (Loop)
                return false;
            return Frame >= (Frames.X * Frames.Y) || Frame >= Timer_Maxes.Length;
        }

        public override void update()
        {
            base.update();
            if (completed())
                return;
            update_spark();
        }

        protected void update_spark()
        {
            if (completed()) return;
            if (Timer_Maxes.Length > 0 ? Timer_Maxes[Frame] == -1 : Timer_Max == -1) return;
            if (Timer_Maxes.Length > 0 ? Timer >= Timer_Maxes[Frame] : Timer > Timer_Max)
            {
                Frame++;
                if (Frame >= Timer_Maxes.Length && Loop)
                    Frame = 0;
                Timer = 0;
            }
            Timer++;
        }

        public override Rectangle src_rect
        {
            get
            {
                return new Rectangle((Frame % (int)Frames.X) * (int)(texture.Width / Frames.X),
                    (Frame / (int)Frames.X) * (int)(texture.Height / Frames.Y),
                    (int)(texture.Width / Frames.X), (int)(texture.Height / Frames.Y));
            }
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(texture, this.loc + draw_vector() - draw_offset,
                        src_rect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
