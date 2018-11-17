using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class FE_Arena_Target_Window : FE_Combat_Target_Window
    {
        public FE_Arena_Target_Window(Texture2D texture) : base(texture) { }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (!(texture == null))
                if (visible)
                {
                    Vector2 offset = this.offset;
                    int team, y;
                    // Attacker //
                    team = team1 - 1;
                    // Top
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + new Vector2(0, 0),
                        new Rectangle(0, 0 + team * IMAGE_HEIGHT, 73, 20),
                        tint, 0f, offset, 1f, SpriteEffects.None, 0f);
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + new Vector2(24, 20),
                        new Rectangle(24, 20 + team * IMAGE_HEIGHT, 49, 8),
                        tint, 0f, offset, 1f, SpriteEffects.None, 0f);
                    // Rows
                    y = 28;
                    for (int i = 0; i < rows - 1; i++)
                    {
                        sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + new Vector2(24, y),
                            new Rectangle(24, 28 + team * IMAGE_HEIGHT, 49, 16),
                            tint, 0f, offset, 1f, SpriteEffects.None, 0f);
                        y += 16;
                    }
                    // Bottom
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + new Vector2(32, y),
                        new Rectangle(32, 44 + team * IMAGE_HEIGHT, 41, 8),
                        tint, 0f, offset, 1f, SpriteEffects.None, 0f);
                    // Target //
                    team = team2 - 1;
                    // Top
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + new Vector2(0, 20),
                        new Rectangle(0, 20 + team * IMAGE_HEIGHT, 24, 8),
                        tint, 0f, offset, 1f, SpriteEffects.None, 0f);
                    // Rows
                    y = 28;
                    for (int i = 0; i < rows - 1; i++)
                    {
                        sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + new Vector2(0, y),
                            new Rectangle(0, 28 + team * IMAGE_HEIGHT, 24, 16),
                            tint, 0f, offset, 1f, SpriteEffects.None, 0f);
                        y += 16;
                    }
                    // Bottom
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + new Vector2(0, y),
                        new Rectangle(0, 44 + team * IMAGE_HEIGHT, 32, 8),
                        tint, 0f, offset, 1f, SpriteEffects.None, 0f);
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + new Vector2(0, y + 8),
                        new Rectangle(0, 52 + team * IMAGE_HEIGHT, 73, 16),
                        tint, 0f, offset, 1f, SpriteEffects.None, 0f);
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + new Vector2(0, y + 24),
                        new Rectangle(0, 84 + team * IMAGE_HEIGHT, 73, 12),
                        tint, 0f, offset, 1f, SpriteEffects.None, 0f);
                }
        }
    }
}
