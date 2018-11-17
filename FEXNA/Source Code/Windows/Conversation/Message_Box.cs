using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Message_Box : Window_Message
    {
        protected bool Window_Shown;
        protected Face_Sprite Speaker_Face;
        protected int Lines;
        protected bool Finished = false;
        protected bool Silenced, Skip_Blocked;

        #region Accessors
        public Face_Sprite speaker { set { Speaker_Face = value; } }

        public bool finished { set { Finished = value; } }

        public Constants.Message_Speeds text_speed { set { Text_Speed = value; } }
        #endregion

        public Message_Box(int x, int y, int width, int lines, bool window_shown, string text_color)
        {
            Lines = lines;
            reset(false, new Vector2(x, y), width, Lines * 16 + 16);
            Window_Shown = window_shown;
            Default_Text_Color = text_color;
            setup_images(Width, Height);
            Text = new List<string>();
        }

        public void set_text(string text)
        {
            Window_Img.text_clear();
            Window_Img.text_color = Default_Text_Color;
            Text.Clear();
            foreach (string line in text.Split('\n'))
                Text.Add(line);
            Text_Loc = Vector2.Zero;
            Text_End = !(Text.Count > 0 && Text[0].Length > 0);
        }

        public void silence()
        {
            Silenced = true;
        }
        public void block_skip()
        {
            Skip_Blocked = true;
        }

        protected override void play_talk_sound()
        {
            if (!Silenced)
                Global.game_system.play_se(System_Sounds.Talk_Boop, false);
        }

        protected override void speaker_talk()
        {
            if (Speaker_Face != null)
                Speaker_Face.talk();
        }

        protected override void update_phase_1()
        {
            Phase_Timer = 0;
            Phase = 2;
        }

        protected override void create_arrow()
        {
            Arrow = new Message_Arrow();
            Arrow.loc = new Vector2(Window_Img.loc.X + Text_Loc.X + 12, this.loc.Y + Text_Loc.Y * 16 + 8);
        }

        public override void update()
        {
            update_images();
            // If there is no text of not waiting on an arrow
            if (Text == null || !Wait)
            {
                if (Wait_Timer > 0)
                    Wait_Timer--;
                else if (Phase != 2 || Text_Speed != Constants.Message_Speeds.Max)
                    refresh();
            }

            // Confirm
            if (Phase == 2 && !Skip_Blocked &&
                (Global.Input.triggered(Inputs.A) ||
                Global.Input.triggered(Inputs.B) ||
                Global.Input.mouse_click(MouseButtons.Left) ||
                Global.Input.gesture_triggered(TouchGestures.Tap)))
            {
                if ((Text == null || !Wait) && !Scroll)
                {
                    // Jump to the end of the current box
                    Wait_Timer = 0;
                    Skipping = true;
                    while (refresh() && (Text == null || Text_Loc.Y != Lines)) { }
                    Skipping = false;
                }
                else if (Wait)
                {
                    // Done waiting at an arrow
                    Wait = false;
                    Arrow.clear();
                    Was_Waiting = true;
                }
                else if (Wait_Timer > 0)
                    Wait_Timer = Math.Max(Wait_Timer - 1, 0);
            }
            // Max text speed
            else if (Text_Speed == Constants.Message_Speeds.Max && Phase == 2 && !Wait)
            {
                Skipping = true;
                bool result = false;
                for (; ; )
                {
                    if (Text.Count > 0 && Text[0].Length == 0 && Was_Waiting)
                        result = refresh();
                    else
                    {
                        result = refresh();
                        if (Was_Waiting && result && !Wait)
                            Was_Waiting = false;
                    }
                    if (!result || Wait || !(Text == null || Text_Loc.Y != Face_Sprite_Data.MESSAGE_LINES))
                        break;
                }
                if (Text.Count == 0)
                {
                    int test = 0;
                }
                // Plays boops at the end of a text spam
                if ((!Finished || Wait) && Phase != 3 && (Text.Count != 0 || Text_End) &&
                    (Wait || (Scroll ? !Was_Waiting && ((Timer + 1) % SCROLL_TIME == 0) : Text_End ? !Was_Waiting : is_arrow_cleared())))
                    play_talk_sound();
                Text_Shown = false;
                Skipping = false;
            }
        }

        protected override void add_backlog(char c) { }

        protected override bool update_images()
        {
            if (Window_Img != null) Window_Img.update();
            if (Arrow != null) Arrow.update();
            return false;
        }
    }
}
