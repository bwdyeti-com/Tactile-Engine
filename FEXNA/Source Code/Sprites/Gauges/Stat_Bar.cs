using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Stat_Bar : Sprite
    {
        public int bar_width { get; set; }
        public int fill_width { get; protected set; }
        public int bonus_width { protected get; set; }
        protected int Color_Override = -1;

        protected Texture2D FillTexture;

        public Stat_Bar()
        {
            this.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Stat_Bar");
            FillTexture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Stat_Bar_Fill");
        }

        #region Accessors
        public int color_override
        {
            set
            {
                Color_Override = (int)MathHelper.Clamp(value, -1, Constants.Team.NUM_TEAMS - 1);
            }
        }

        private int color { get { return Color_Override != -1 ? Color_Override : Global.game_options.window_color; } }
        #endregion

        public void SetFillWidth(int value)
        {
            this.fill_width = value;
        }
        public void SetFillWidth(int totalWidth, int value, int min, int max)
        {
            this.fill_width = (int)Math.Round(totalWidth * (value - min) / ((float)max - min));
            if (totalWidth > 1 && value < max)
                this.fill_width = Math.Min(this.fill_width, totalWidth - 1);
            if (value - min > 0 && totalWidth > 0)
                this.fill_width = Math.Max(this.fill_width, 1);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            draw_bg(sprite_batch, draw_offset);
            draw_fill(sprite_batch, draw_offset);
        }

        public void draw_bg(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            if (visible)
                if (!(texture == null))
                {
                    Vector2 offset = this.offset;
                    Vector2 temp_loc = Vector2.Zero;
                    // Left Side
                    sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset + temp_loc,
                        new Rectangle(0, 0 + 5 * color, 2, 5), tint, 0f,
                        offset, 1f, SpriteEffects.None, 0f);
                    temp_loc.X += 2;
                    // Center
                    for (int i = 0; i < bar_width; i++)
                    {
                        sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset + temp_loc,
                            new Rectangle(2, 0 + 5 * color, 1, 5), tint, 0f,
                        offset, 1f, SpriteEffects.None, 0f);
                        temp_loc.X += 1;
                    }
                    // Right Side
                    sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset + temp_loc,
                        new Rectangle(3, 0 + 5 * color, 2, 5), tint, 0f,
                        offset, 1f, SpriteEffects.None, 0f);
                }
        }

        public virtual void draw_fill(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            if (visible)
                if (!(FillTexture == null))
                {
                    Vector2 offset = this.offset;
                    Vector2 temp_loc = new Vector2(2, 1);
                    for (int i = 0; i < fill_width; i++)
                    {
                        sprite_batch.Draw(FillTexture, (loc + draw_vector()) - draw_offset + temp_loc,
                            new Rectangle(0, 0, 1, 2), tint, 0f,
                        offset, 1f, SpriteEffects.None, 0f);
                        temp_loc.X += 1;
                    }
                    // If negative bonus
                    if (bonus_width < 0)
                        for (int i = 0; i < -bonus_width; i++)
                        {
                            sprite_batch.Draw(FillTexture, (loc + draw_vector()) - draw_offset + temp_loc,
                                new Rectangle(0, 4, 1, 2), tint, 0f,
                            offset, 1f, SpriteEffects.None, 0f);
                            temp_loc.X += 1;
                        }
                    else
                        for (int i = 0; i < bonus_width; i++)
                        {
                            sprite_batch.Draw(FillTexture, (loc + draw_vector()) - draw_offset + temp_loc,
                                new Rectangle(0, 2, 1, 2), tint, 0f,
                            offset, 1f, SpriteEffects.None, 0f);
                            temp_loc.X += 1;
                        }
                }
        }
    }
}
