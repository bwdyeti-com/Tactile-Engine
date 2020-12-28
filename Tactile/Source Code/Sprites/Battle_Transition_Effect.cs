using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Battle_Transition_Effect : Sprite
    {
        public Battle_Transition_Effect(Texture2D texture)
        {
            this.texture = texture;
            tint = new Color(96, 96, 96, 96);
        }

        public override Rectangle src_rect
        {
            get
            {
                return new Rectangle(160, 0, 16, 16);
            }
            set
            {
                base.src_rect = value;
            }
        }

        public void draw(SpriteBatch sprite_batch, Rectangle ignore_rect, Rectangle screen_rect)
        {
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Rectangle target_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;

                    // Top
                    target_rect = new Rectangle(0, 0, screen_rect.Width, ignore_rect.Y);
                    sprite_batch.Draw(texture, target_rect,
                        src_rect, tint, angle, offset,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Bottom
                    target_rect = new Rectangle(0, ignore_rect.Y + ignore_rect.Height,
                        screen_rect.Width, screen_rect.Height - (ignore_rect.Y + ignore_rect.Height));
                    sprite_batch.Draw(texture, target_rect,
                        src_rect, tint, angle, offset,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Left
                    target_rect = new Rectangle(0, ignore_rect.Y, ignore_rect.X, ignore_rect.Height);
                    sprite_batch.Draw(texture, target_rect,
                        src_rect, tint, angle, offset,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Right
                    target_rect = new Rectangle(ignore_rect.X + ignore_rect.Width, ignore_rect.Y,
                        screen_rect.Width - (ignore_rect.X + ignore_rect.Width), ignore_rect.Height);
                    sprite_batch.Draw(texture, target_rect,
                        src_rect, tint, angle, offset,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
