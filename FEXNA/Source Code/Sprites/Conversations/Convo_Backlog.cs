using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    class Convo_Backlog
    {
        public bool Active { get; private set; }
        private int Index = -1;
        private FE_Text[] Text = new FE_Text[Config.CONVO_BACKLOG_LINES];
        private Sprite Black_Fill;
        private Vector2 Offset = Vector2.Zero;
        private int FadeTimer = 0, PanTimer = 0;
        private float Scroll_Speed = 0;
        private bool ScrollWheel = false;
        private bool Fading_In = false;
        private string LastColorSet;
        protected Page_Arrow UpArrow, DownArrow;

        #region Accessors
        public bool ready { get { return FadeTimer == 0 && PanTimer == 0; } }
        
        private int min_offset { get { return Config.WINDOW_HEIGHT - (Text.Length * Window_Message.FontData.CharHeight); } }
        private int max_offset { get { return ((Index + 2) - Text.Length) * Window_Message.FontData.CharHeight; } }

        public float stereoscopic
        {
            set
            {
                for (int i = 0; i < Text.Length; i++)
                    Text[i].stereoscopic = value;
            }
        }

        public bool started { get { return Index >= 0; } }
        #endregion

        public Convo_Backlog()
        {
            Active = false;
            for (int i = 0; i < Text.Length; i++)
            {
                Text[i] = new FE_Text();
                Text[i].loc = new Vector2(24, LineY(i));
                Text[i].Font = Window_Message.FONT;
                Text[i].stereoscopic = Config.CONVO_BACKLOG_DEPTH;
            }
            Black_Fill = new Sprite();
            Black_Fill.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Fill.tint = new Color(0, 0, 0, 0);

            UpArrow = new Page_Arrow();
            UpArrow.loc = new Vector2(Config.WINDOW_WIDTH / 2 + 8, 4);
            UpArrow.angle = MathHelper.PiOver2;
            DownArrow = new Page_Arrow();
            DownArrow.loc = new Vector2(Config.WINDOW_WIDTH / 2 + 8, Config.WINDOW_HEIGHT - 4);
            DownArrow.mirrored = true;
            DownArrow.angle = MathHelper.PiOver2;
        }

        private int LineY(int index)
        {
            return (index - Text.Length) * Window_Message.FontData.CharHeight +
                Config.WINDOW_HEIGHT + 8;
        }
        
        public void add_text(char c)
        {
            if (!started)
            {
                if (Global.scene.is_map_scene)
                    throw new IndexOutOfRangeException("First convo backlog line not started");
                else
                    new_line();
            }
            Text[Index].text += c;
        }
        public void add_text(string str)
        {
            if (!started)
            {
                if (Global.scene.is_map_scene)
                    throw new IndexOutOfRangeException("First convo backlog line not started");
                else
                    new_line();
            }
            Text[Index].text += str;
        }

        public void new_speaker_line()
        {
            new_line();
            if (Index >= 1)
                new_line();
            Text[Index].offset.X = 8;
            LastColorSet = null;
        }
        public void new_line()
        {
            Index++;
            if (Index >= Text.Length)
                remove_lines((Text.Length + 1) - Index);
            Text[Index].text = "";
            Text[Index].clear_text_colors();
            if (!string.IsNullOrEmpty(LastColorSet))
            {
                Text[Index].text_colors[0] =
                    string.Format("{0}_{1}", Window_Message.FONT, LastColorSet);
            }
            Text[Index].offset = Vector2.Zero;
            // Scroll to the bottom
            Offset.Y = Math.Max(this.min_offset, this.max_offset);
        }

        public void set_color(string color)
        {
            if (!started)
                throw new IndexOutOfRangeException("First convo backlog line not started");
            Text[Index].text_colors[Text[Index].text.Length] =
                string.Format("{0}_{1}", Window_Message.FONT, color);
            LastColorSet = color;
        }

        public void reset()
        {
            Index = -1;
            LastColorSet = null;
        }

        //@Debug: this is never called?
        private void add_line(List<string> lines, List<string> colors)
        {
            if (lines.Count + Index > Text.Length)
                remove_lines(Text.Length - (lines.Count + Index));
            for (int i = Math.Max(lines.Count - Text.Length, 0); i < lines.Count; i++)
            {
                Index++;
                Text[Index].text = lines[i];
                Text[Index].text_colors[0] =
                    string.Format("{0}_{1}", Window_Message.FONT, colors[i]);
            }
            // Scroll to the bottom
            Offset.Y = Math.Max(this.min_offset, this.max_offset);
        }

        private void remove_lines(int count)
        {
            if (count > Index)
            {
                Index = -1;
                return;
            }

            // Move the oldest lines to the end
            FE_Text[] removed = new FE_Text[count];
            for (int i = 0; i < count; i++)
            {
                removed[i] = Text[i];
                removed[i].text = "";
                removed[i].clear_text_colors();
            }
            for (int i = 0; i < (Text.Length - count); i++)
            {
                Text[i] = Text[i + count];
                Text[i].loc.Y = LineY(i);
            }
            for (int i = 0; i < count; i++)
            {
                Text[Text.Length + i - count] = removed[i];
                Text[Text.Length + i - count].loc.Y = LineY(Text.Length + i - count);
            }
            Index -= count;
        }

        public void fade_in(bool swipeIn = false)
        {
            Active = true;
            Scroll_Speed = 0;
            FadeTimer = Config.CONVO_BACKLOG_FADE_TIME;
            Fading_In = true;
            PanTimer = swipeIn ? Config.CONVO_BACKLOG_PAN_IN_TIME : 0;

            foreach (var text in Text)
            {
                if (swipeIn)
                    text.tint = Color.Transparent;
                text.draw_offset = Vector2.Zero;
            }
        }

        public void fade_out(bool swipeOut = false)
        {
            Active = false;
            Scroll_Speed = 0;
            FadeTimer = Config.CONVO_BACKLOG_FADE_TIME;
            Fading_In = false;
            PanTimer = swipeOut ? Config.CONVO_BACKLOG_PAN_OUT_TIME : 0;

            if (swipeOut)
                foreach (var text in Text)
                    text.tint = Color.White;
        }

        public void update()
        {
            UpArrow.update();
            DownArrow.update();

            float max_speed;
            if (Input.ControlScheme == ControlSchemes.Buttons)
                max_speed = (Global.Input.speed_up_input() ? 2 : 1) *
                    Config.CONVO_BACKLOG_MAX_SCROLL_SPEED;
            else if (Input.ControlScheme == ControlSchemes.Mouse)
                max_speed = 5f * Config.CONVO_BACKLOG_MAX_SCROLL_SPEED;
            else
                max_speed = Config.WINDOW_HEIGHT;

            // Pan out
            if (PanTimer > 0 && !Fading_In)
            {
                PanTimer--;
                int pan = Config.CONVO_BACKLOG_PAN_OUT_TIME - PanTimer;
                // Set full alpha if the pan is over
                int alpha = 255;
                if (PanTimer > 0)
                    alpha = Math.Min(255, PanTimer * 256 / Config.CONVO_BACKLOG_PAN_OUT_TIME);
                Color tint = new Color(alpha, alpha, alpha, alpha);
                foreach (var text in Text)
                {
                    text.draw_offset = new Vector2(
                        -(int)Math.Pow(pan, 1.8f), 0);
                    text.tint = tint;
                }
            }
            else if (FadeTimer > 0)
            {
                FadeTimer--;
                Black_Fill.tint = new Color(0, 0, 0,
                    ((Fading_In ? (Config.CONVO_BACKLOG_FADE_TIME - FadeTimer) : FadeTimer) *
                        Config.CONVO_BACKLOG_BG_OPACITY) /
                    Config.CONVO_BACKLOG_FADE_TIME);
            }
            // Pan in
            else if (PanTimer > 0)
            {
                PanTimer--;
                int pan = Config.CONVO_BACKLOG_PAN_IN_TIME - PanTimer;
                int alpha = Math.Min(255, pan * 256 / Config.CONVO_BACKLOG_PAN_IN_TIME);
                Color tint = new Color(alpha, alpha, alpha, alpha);
                foreach (var text in Text)
                {
                    text.draw_offset = new Vector2(
                        -(int)Math.Pow(PanTimer, 1.8f), 0);
                    text.tint = tint;
                }
            }
            if (FadeTimer == 0 && PanTimer == 0)
                Fading_In = false;

            if (ready)
            {
                update_input(max_speed);
            }
            Scroll_Speed = MathHelper.Clamp(Scroll_Speed, -max_speed, max_speed);
            Offset.Y = MathHelper.Clamp(Offset.Y + Scroll_Speed, this.min_offset, this.max_offset);

            UpArrow.visible = Offset.Y > this.min_offset;
            DownArrow.visible = Offset.Y < this.max_offset;
        }

        private void update_input(float max_speed)
        {
            if (Active)
            {
                if (Global.Input.pressed(Inputs.Up))
                {
                    if (Scroll_Speed > 0)
                        Scroll_Speed = 0;
                    if (Scroll_Speed > -max_speed)
                        Scroll_Speed--;
                    return;
                }
                else if (Global.Input.pressed(Inputs.Down))
                {
                    if (Scroll_Speed < 0)
                        Scroll_Speed = 0;
                    if (Scroll_Speed < max_speed)
                        Scroll_Speed++;
                    return;
                }
                else if (Global.Input.mouseScroll < 0)
                {
                    Scroll_Speed += max_speed / 5;
                    ScrollWheel = true;
                    return;
                }
                else if (Global.Input.mouseScroll > 0)
                {
                    Scroll_Speed += -max_speed / 5;
                    ScrollWheel = true;
                    return;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse && UpArrow.MouseOver())
                {
                    Scroll_Speed = -Config.CONVO_BACKLOG_MAX_SCROLL_SPEED;
                    ScrollWheel = false;
                    return;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse && DownArrow.MouseOver())
                {
                    Scroll_Speed = Config.CONVO_BACKLOG_MAX_SCROLL_SPEED;
                    ScrollWheel = false;
                    return;
                }
                else if (Global.Input.gesture_triggered(TouchGestures.VerticalDrag))
                {
                    Scroll_Speed = -(int)Global.Input.verticalDragVector.Y;
                    return;
                }
            }

            if (Scroll_Speed != 0)
            {
                if (Input.ControlScheme == ControlSchemes.Buttons)
                {
                    Scroll_Speed = (float)Additional_Math.double_closer(
                        Scroll_Speed, 0, 1);
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse)
                {
                    if (ScrollWheel)
                        Scroll_Speed *= (float)Math.Pow(
                            Config.CONVO_BACKLOG_TOUCH_SCROLL_FRICTION, 2f);
                    else
                        Scroll_Speed = (float)Additional_Math.double_closer(
                            Scroll_Speed, 0, 1);
                }
                else
                {
                    Scroll_Speed *= Config.CONVO_BACKLOG_TOUCH_SCROLL_FRICTION;
                }

                if (Math.Abs(Scroll_Speed) < 0.1f)
                {
                    Scroll_Speed = 0;
                    ScrollWheel = false;
                }
            }
        }

        public void draw(SpriteBatch sprite_batch)
        {
            Vector2 int_offset = new Vector2((int)Offset.X, (int)Offset.Y);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Black_Fill.draw(sprite_batch);
            sprite_batch.End();

            //sprite_batch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend); //@Debug
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (PanTimer > 0 || (Fading_In ? FadeTimer == 0 : Active))
            {
                for (int i = 0; i <= Index; i++)
                    Text[i].draw_multicolored(sprite_batch, int_offset);

                UpArrow.draw(sprite_batch);
                DownArrow.draw(sprite_batch);
            }
            sprite_batch.End();
        }
    }
}
