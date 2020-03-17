using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Map
{
    class Character_Hp_Gauge : Matrix_Position_Sprite
    {
        private Texture2D Advanced_Team_Texture;
        private float Hp_Percent = -1;
        private byte HpGaugesMode = byte.MaxValue;
        private Color Hp_Hue;
        private int Team;

        public Character_Hp_Gauge()
        {
            Advanced_Team_Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Team_Hp_Gauge");
            texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            draw_offset = new Vector2(-WIDTH / 2, Constants.Map.TILE_SIZE / 2 - 3);
        }

        private static float WIDTH
        {
            get
            {
                return Constants.Map.TILE_SIZE;
            }
        }

        internal void update(Game_Unit unit)
        {
            Game_Actor actor = unit.actor;

            float hp_percent = (float)actor.hp / actor.maxhp;
            if (Hp_Percent != hp_percent ||
                HpGaugesMode != Global.game_options.hp_gauges)
            {
                Hp_Percent = hp_percent;
                HpGaugesMode = Global.game_options.hp_gauges;

                float hue;
                // Simple mode
                if (HpGaugesMode == (int)Constants.Hp_Gauge_Modes.Basic)
                    hue = Hp_Percent * Config.SIMPLE_HPGAUGE_MAX_HUE;
                // Advanced mode
                else
                    hue = (Math.Min(actor.hp, Constants.Actor.MAX_HP) /
                        (float)Constants.Actor.MAX_HP) *
                            Config.ADVANCED_HPGAUGE_MAX_HUE;
                Hp_Hue = color_from_hue(hue);
                if (Hp_Percent > 0 && Hp_Percent < 1f / (WIDTH - 3))
                    Hp_Percent = 1f / (WIDTH - 3);
            }

            Team = unit.team;
            visible = !actor.is_full_hp() ||
                Global.game_options.hp_gauges != (int)Constants.Hp_Gauge_Modes.Injured;
        }

        internal static Color color_from_hue(float hue)
        {
            float r = 0, g = 0, b = 0;
            int i = ((int)hue) / 60;
            float f = (((int)hue) % 60) / 60f;
            switch (i)
            {
                // Red - yellow
                case 0:
                    r = 1;
                    g = f;
                    break;
                // Yellow - green
                case 1:
                    g = 1;
                    r = 1 - f;
                    break;
                // Green - cyan
                case 2:
                    g = 1;
                    b = f;
                    break;
                // Cyan - blue
                case 3:
                    b = 1;
                    g = 1 - f;
                    break;
                // Blue - magenta
                case 4:
                    b = 1;
                    r = f;
                    break;
                // Magenta - red
                default:
                    r = 1;
                    b = 1 - f;
                    break;
            }
            return new Color(r, g, b);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix)
        {
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    sprite_batch.Draw(texture, Vector2.Transform((this.loc + draw_vector()) - draw_offset, matrix),
                        src_rect, new Color(24, 24, 24, 240), angle, offset, new Vector2((WIDTH - 1) / texture.Width, 3f / texture.Height),
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Simple mode
                    if (Global.game_options.hp_gauges == (int)Constants.Hp_Gauge_Modes.Basic)
                    {
                        sprite_batch.Draw(texture, Vector2.Transform((this.loc + draw_vector()) - draw_offset, matrix) + new Vector2(1, 1),
                            src_rect, Hp_Hue, angle, offset, new Vector2(((WIDTH - 3) * Hp_Percent) / texture.Width, 1f / texture.Height),
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                    // Advanced mode
                    else
                    {
                        sprite_batch.Draw(texture, Vector2.Transform((this.loc + draw_vector()) - draw_offset, matrix) + new Vector2(1, 1),
                            src_rect, Hp_Hue, angle, offset, new Vector2(((WIDTH - 3) * Hp_Percent) / texture.Width, 1f / texture.Height),
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        sprite_batch.Draw(Advanced_Team_Texture, Vector2.Transform((this.loc + draw_vector()) - draw_offset, matrix) + new Vector2(1, 1),
                            new Rectangle(0, 2 * Team, Advanced_Team_Texture.Width, 1),
                            Color.White, angle, offset, new Vector2(((WIDTH - 3) * Hp_Percent) / Advanced_Team_Texture.Width, 1f),
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                }
        }
    }
}
