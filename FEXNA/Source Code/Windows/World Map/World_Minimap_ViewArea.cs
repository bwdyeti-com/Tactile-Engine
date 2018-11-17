using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class World_Minimap_ViewArea : Sprite
    {
        const int VIEW_TIME_MAX = 64;
        protected int View_Timer = 0;

        public World_Minimap_ViewArea(Vector2 size)
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            scale = size;
        }

        public override void update()
        {
            // View box flash
            byte alpha;
            if (View_Timer < VIEW_TIME_MAX / 2)
                alpha = (byte)Math.Min(192 - View_Timer * (256 / VIEW_TIME_MAX), 255);
            else
                alpha = (byte)Math.Min(192 - ((VIEW_TIME_MAX - View_Timer) * (256 / VIEW_TIME_MAX)), 255);
            this.tint = new Color(alpha, alpha, alpha, this.tint.A);
            View_Timer = (View_Timer + 1) % VIEW_TIME_MAX;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    // Top
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset,
                        src_rect, tint, angle, offset, new Vector2(scale.X / texture.Width, 1f / texture.Height),
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Left
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset,
                        src_rect, tint, angle, offset, new Vector2(1f / texture.Width, scale.Y / texture.Height),
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Right
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + new Vector2(scale.X - 1, 0),
                        src_rect, tint, angle, offset, new Vector2(1f / texture.Width, scale.Y / texture.Height),
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Bottom
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + new Vector2(0, scale.Y - 1),
                        src_rect, tint, angle, offset, new Vector2(scale.X / texture.Width, 1f / texture.Height),
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
