//Sparring
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Sparring_Exp_Gauge : Stat_Bar
    {
        const int FLASH_DURATION = 64;

        private bool OffsetFlash;

        public int malus_width { get; set; }
        public bool malus_width_has_space { get { return this.bonus_width > 1; } }

        public Sparring_Exp_Gauge(bool offsetFlash = false)
            : base()
        {
            OffsetFlash = offsetFlash;
        }

        public override void draw_fill(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            if (visible)
                if (!(FillTexture == null))
                {
                    Vector2 offset = this.offset;
                    Vector2 temp_loc = new Vector2(2, 1);
                    for (int i = 0; i < fill_width; i++)
                    {
                        sprite_batch.Draw(FillTexture,
                            (loc + draw_vector()) + temp_loc - draw_offset,
                            new Rectangle(0, 0, 1, 2), tint, 0f,
                        offset, 1f, SpriteEffects.None, 0f);
                        temp_loc.X += 1;
                    }

                    float alpha = ((float)Math.Sin(
                        (Global.game_system.total_play_time +
                            (OffsetFlash ? FLASH_DURATION / 2 : 0)) *
                        MathHelper.TwoPi / (float)FLASH_DURATION) + 1) / 2;
                    alpha = (((Global.game_system.total_play_time +
                            (OffsetFlash ? FLASH_DURATION / 2 : 0)) % FLASH_DURATION) /
                        (float)FLASH_DURATION) * 2f - 1f;
                    alpha = Math.Abs(alpha);
                    alpha = (float)Math.Pow(alpha, 0.5f);

                    Color negative_tint = Color.Lerp(
                        this.tint, new Color(144, 128, 144, this.tint.A), 0); //0.5f); //Debug
                    //negative_tint = Color.Lerp( //Debug
                    //    new Color(0, 0, 0, this.tint.A), negative_tint, alpha);
                    Color positive_tint = Color.Lerp(
                        new Color(0, 0, 0, this.tint.A), this.tint, alpha);

                    Vector2 bonus_temp_loc = temp_loc;
                    // If negative bonus
                    if (bonus_width < 0)
                        for (int i = 0; i < -bonus_width; i++)
                        {
                            sprite_batch.Draw(FillTexture,
                                (loc + draw_vector()) + bonus_temp_loc - draw_offset,
                                new Rectangle(0, 2, 1, 2), negative_tint, 0f,
                            offset, 1f, SpriteEffects.None, 0f);
                            bonus_temp_loc.X += 1;
                        }
                    else
                        for (int i = 0; i < bonus_width; i++)
                        {
                            sprite_batch.Draw(FillTexture,
                                (loc + draw_vector()) + bonus_temp_loc - draw_offset,
                                new Rectangle(0, 2, 1, 2), positive_tint, 0f,
                            offset, 1f, SpriteEffects.None, 0f);
                            bonus_temp_loc.X += 1;
                        }

                    bonus_temp_loc = temp_loc;
                    // If negative bonus
                    if (malus_width < 0)
                        for (int i = 0; i < -malus_width; i++)
                        {
                            sprite_batch.Draw(FillTexture,
                                (loc + draw_vector()) + bonus_temp_loc - draw_offset,
                                new Rectangle(0, 2, 1, 2), positive_tint, 0f,
                            offset, 1f, SpriteEffects.None, 0f);
                            bonus_temp_loc.X += 1;
                        }
                    else
                        for (int i = 0; i < malus_width; i++)
                        {
                            sprite_batch.Draw(FillTexture,
                                (loc + draw_vector()) + bonus_temp_loc - draw_offset,
                                new Rectangle(0, 2, 1, 2), negative_tint, 0f,
                            offset, 1f, SpriteEffects.None, 0f);
                            bonus_temp_loc.X += 1;
                        }
                }
        }
    }
}
