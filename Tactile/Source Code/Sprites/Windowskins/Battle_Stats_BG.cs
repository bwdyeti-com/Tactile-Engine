using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Battle_Stats_BG : Sprite
    {
        readonly static Rectangle STAT_NAMES = new Rectangle(117, 277, 15, 32);
        readonly static Rectangle STAT_BARS = new Rectangle(132, 277, 1, 2);

        int Skill_X;
        int Team;
        Vector2 Skill_Offset;
        Rectangle Bars_Rect, Names_Rect, Skill_Name_Rect;

        public Battle_Stats_BG(Texture2D texture, int team, bool reverse)
        {
            this.texture = texture;
            //int side = reverse ? 0 : 2;
            Team = team;
            //loc.X = ((Config.WINDOW_WIDTH - 40) / 2) * side;
            loc.X = reverse ? 0 : (Config.WINDOW_WIDTH - 40);
            //Skill_Offset = new Vector2((Config.WINDOW_WIDTH - (155 + ((2 - side) * 51) / 2)) - loc.X, 50);
            Skill_Offset = new Vector2((reverse ? 114 : (Config.WINDOW_WIDTH - 155)) - loc.X, 50);
            Bars_Rect = new Rectangle(STAT_BARS.X, STAT_BARS.Y + (team - 1) * STAT_BARS.Height, STAT_BARS.Width, STAT_BARS.Height);
            Names_Rect = new Rectangle(STAT_NAMES.X, STAT_NAMES.Y, STAT_NAMES.Width, STAT_NAMES.Height * 3 / 4);
            Skill_Name_Rect = new Rectangle(STAT_NAMES.X, STAT_NAMES.Y + STAT_NAMES.Height * 3 / 4, STAT_NAMES.Width, STAT_NAMES.Height / 4);
        }

        public override void  draw(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            if (texture != null)
                if (visible)
                {
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    // HitDmgCrt
                    for (int y = 0; y < 3; y++)
                        for (int x = 0; x < 36; x++)
                            sprite_batch.Draw(texture, this.loc + draw_vector() + new Vector2(2 + x, 7 + y * 8) - draw_offset,
                                Bars_Rect, tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    sprite_batch.Draw(texture, this.loc + draw_vector() + new Vector2(2, 3) - draw_offset,
                        Names_Rect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Skl
                    for (int x = 0; x < 36; x++)
                        sprite_batch.Draw(texture, this.loc + draw_vector() + new Vector2(x, 7) + Skill_Offset - draw_offset,
                            Bars_Rect, tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    sprite_batch.Draw(texture, this.loc + draw_vector() + new Vector2(0, 3) + Skill_Offset - draw_offset,
                        Skill_Name_Rect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
