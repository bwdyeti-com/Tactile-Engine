using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Exp_Gauge : Sprite
    {
        const int HEIGHT = 24;

        int Timer = 0;
        const int TIMER_MAX = 8;
        int Exp;
        RightAdjustedText Exp_Counter;
        RasterizerState Raster_State = new RasterizerState { ScissorTestEnable = true };
        bool Retracting = false;

        #region Accessors
        public RasterizerState raster_state { get { return Raster_State; } }

        public int exp
        {
            get { return Exp; }
            set { Exp = value % Global.ActorConfig.ExpToLvl; }
        }
        #endregion

        public Exp_Gauge(int base_exp)
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Exp_Window");
            Exp = base_exp;
            loc = new Vector2(88, (Config.WINDOW_HEIGHT - 32) / 2);
            Exp_Counter = new RightAdjustedText();
            Exp_Counter.SetFont(Config.UI_FONT, Global.Content, "Exp");
        }

        public Rectangle scissor_rect()
        {
            if (Retracting)
                return new Rectangle(0, (int)loc.Y + HEIGHT / 2 - ((TIMER_MAX - Timer) * 2), 320, (TIMER_MAX - Timer) * 4);
            else
                return new Rectangle(0, (int)loc.Y + HEIGHT / 2 - (Timer * 2), 320, Timer * 4);
        }

        public void skip_appear()
        {
            if (!Retracting)
                while (Timer < TIMER_MAX)
                    Timer++;
        }

        public void retract()
        {
            Timer = 2;
            Retracting = true;
        }

        public override void update()
        {
            if (Timer < TIMER_MAX) Timer++;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    // Window
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset,
                        new Rectangle(0, 0, 136, HEIGHT), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Counter
                    Exp_Counter.loc = this.loc + new Vector2(24, 8);
                    Exp_Counter.text = Exp.ToString();
                    Exp_Counter.draw(sprite_batch, draw_offset - draw_vector());
                    // Gauge
                    if (Exp > 0)
                    {
                        int width = Exp;
                        Vector2 bar_offset = new Vector2(24, 9);
                        sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + bar_offset,
                            new Rectangle(0, 24, 3, 7), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        int temp_x = 0;
                        while (temp_x < width - 1)
                        {
                            sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + bar_offset + new Vector2(3 + temp_x, 0),
                                new Rectangle(3, 24, 1, 7), tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                            temp_x++;
                        }
                        if (width < 99)
                            sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + bar_offset + new Vector2(2 + width, 0),
                                new Rectangle(5, 24, 1, 7), tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        else
                            sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset + bar_offset + new Vector2(2 + width, 0),
                                new Rectangle(4, 24, 2, 7), tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                }
        }
    }
}
