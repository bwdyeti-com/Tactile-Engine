using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Status_Top_Panel_Bg : Sprite
    {
        public Status_Top_Panel_Bg(List<Texture2D> textures)
        {
            this.textures = textures;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (textures.Count == 2)
                if (visible)
                {
                    Vector2 loc = this.loc + draw_vector();
                    Vector2 offset = this.offset;
                    // Panel Background
                    for (int y = 0; y < 80; y+=8)
                        for (int x = 0; x < 224; x += 8)
                            sprite_batch.Draw(textures[1], loc - draw_offset + new Vector2(96 + x, 0 + y),
                                new Rectangle(8, 8 + 24 * Global.game_options.window_color, 8, 8), tint);
                    // Portrait frame
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(0, 0), new Rectangle(0, 0, 96, 4), tint);
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(0, 4), new Rectangle(0, 4, 8, 72), tint);
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(88, 4), new Rectangle(88, 4, 8, 72), tint);
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(0, 76), new Rectangle(0, 76, 96, 7), tint);
                    // Portrait Emblem
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(0, 61), new Rectangle(
                        Global.game_options.window_color * 24, 83, 24, 24), tint);
                    // Panel Frame
                    for (int i = 0; i < 221; i++)
                        sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(96 + i, 0), new Rectangle(93, 114, 1, 3), tint);
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(317, 0), new Rectangle(93, 114, 3, 3), tint);
                    for (int i = 0; i < 73; i++)
                        sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(317, 3 + i), new Rectangle(93, 116, 3, 1), tint);
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(317, 76), new Rectangle(93, 117, 3, 7), tint);
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(96 + 0, 76), new Rectangle(0, 107, 64, 7), tint);
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(96 + 64, 76), new Rectangle(0, 107, 64, 7), tint);
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(96 + 128, 76), new Rectangle(0, 107, 64, 7), tint);
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(96 + 192, 76), new Rectangle(0, 107, 29, 7), tint);
                    // Battle Stats BG
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(196, 8),
                        new Rectangle(96, 0 + 64 * Global.game_options.window_color, 120, 64), tint);
                    // Name Banner
                    sprite_batch.Draw(textures[0], loc - draw_offset + new Vector2(100, 0),
                        new Rectangle(0, 114 + 25 * Global.game_options.window_color, 93, 25), tint);
                }
        }
    }
}
