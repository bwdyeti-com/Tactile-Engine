using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Graphics.Text
{
    class Battle_Hit_Number : TextSprite
    {
        const int MIN_TIME = 16;
        private int Value;
        private int Timer;
        private Vector2 Vel;

        #region Accessors
        private int max_time
        {
            get
            {
                return MIN_TIME + Math.Min(Global.ActorConfig.HpMax, Math.Max(40, Value));
            }
        }
        #endregion

        public Battle_Hit_Number(int value, float vel)
        {
            Value = value;
            Timer = max_time;
            Vel = new Vector2(1f * vel, -1);

            SetFont(Config.UI_FONT, Global.Content, "Red");
            text = value.ToString();
            if (font_data.IsSomething)
            {
                Font_Data data = font_data;
                offset = new Vector2(text_width / 2, data.CharHeight / 2);
            }
            opacity = 0;
            scale = new Vector2(1.0f);
            update();
        }

        public override void update()
        {
            /*float timer_progress = 1 - (Timer * 2f / max_time); //Debug
            float bluh = (float)(Math.Asin(timer_progress) / Math.PI) + 0.5f;
            bluh = (float)(Math.Asin(bluh) / Math.PI) + 0.5f;
            draw_offset.X = (float)Math.Pow(Math.Abs(max_time / 2 - Timer) / 5f, 3) *
                ((Timer < max_time / 2) ? 1 : -1);
            //this.loc += Vel;
            //Vel += new Vector2(0, 0.04f * 4);*/

            Timer--;
            // Jump at the start
            if (Timer > max_time - 16)
            {
                if (Timer > max_time - 8)
                    draw_offset.Y = -(max_time - Timer) * 2;
                else
                    draw_offset.Y = -(Timer - (max_time - 16)) * 2;
            }
            // Fade out
            if (Timer < 10)
            {
                opacity = Timer * 255 / 10;
            }
            // Fade in
            else if (Timer > max_time - 8)
            {
                opacity = (max_time - Timer) * 255 / 8;
            }
        }

        public bool finished { get { return Timer < 0; } }
    }
}
