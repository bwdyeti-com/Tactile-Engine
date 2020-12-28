using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Trade_Target_Window_Img : Sprite
    {
        public int rows = 1;

        public Trade_Target_Window_Img()
        {
            this.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Trade_Window");
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (!(texture == null))
                if (visible)
                {
                    Vector2 offset = this.offset;
                    sprite_batch.Draw(texture, loc + new Vector2(4, 4) - draw_offset,
                        new Rectangle(0, Global.game_options.window_color * 41, 105, 20),
                        tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    for(int i = 0; i < rows; i++)
                        sprite_batch.Draw(texture, loc + new Vector2(4, 24 + i*16) - draw_offset,
                            new Rectangle(0, Global.game_options.window_color * 41 + 20, 105, 16),
                            tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    sprite_batch.Draw(texture, loc + new Vector2(4, 24 + rows*16) - draw_offset,
                        new Rectangle(0, Global.game_options.window_color * 41 + 36, 105, 5),
                        tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
