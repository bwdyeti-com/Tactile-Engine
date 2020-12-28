using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Graphics.Preparations
{
    class Pick_Units_Header : Sprite
    {
        const string FILENAME = @"Graphics/Windowskins/Pick_Units_Components";
        protected int Width;

        public Pick_Units_Header(int width)
        {
            texture = Global.Content.Load<Texture2D>(FILENAME);
            Width = width;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    int x = 0;
                    // Left
                    sprite_batch.Draw(texture, (loc + draw_vector() + new Vector2(x, 0)) - draw_offset,
                        new Rectangle(0, 0, 16, 32), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    x += 16;
                    // Center
                    while (x + 16 <= Width)
                    {
                        sprite_batch.Draw(texture, (loc + draw_vector() + new Vector2(x, 0)) - draw_offset,
                            new Rectangle(16, 0, 8, 32), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        x += 8;
                    }
                    // Right
                    sprite_batch.Draw(texture, (loc + draw_vector() + new Vector2(x, 0)) - draw_offset,
                        new Rectangle(24, 0, 8, 32), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
