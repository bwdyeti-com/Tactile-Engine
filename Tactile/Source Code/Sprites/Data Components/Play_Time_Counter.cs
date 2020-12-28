using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Play_Time_Counter : Stereoscopic_Graphic_Object
    {
        protected int Time = -1;
        protected Sprite Label;
        protected RightAdjustedText Hour, Minute, Second;
        protected TextSprite Separator_1, Separator_2;

        public Play_Time_Counter()
        {
            initialize();
        }
        public Play_Time_Counter(int time)
        {
            Time = time;
            initialize();
        }

        protected void initialize()
        {
            // Label
            Label = new Sprite();
            Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Label.loc = new Vector2(0 + (Time >= 0 ? 4 : 0), 0);
            Label.src_rect = new Rectangle(0, 144, 24, 16);
            // Hour
            Hour = new RightAdjustedText();
            Hour.loc = new Vector2(40, 0);
            Hour.SetFont(Config.UI_FONT, Global.Content, "Blue");
            // Minute
            Minute = new RightAdjustedText();
            Minute.loc = new Vector2(64, 0);
            Minute.SetFont(Config.UI_FONT, Global.Content, "Blue");
            // Second
            Second = new RightAdjustedText();
            Second.loc = new Vector2(88, 0);
            Second.SetFont(Config.UI_FONT + "S", Global.Content, "Blue", Config.UI_FONT);
            // Separators
            Separator_1 = new RightAdjustedText();
            Separator_1.loc = new Vector2(40 + 4, 0);
            Separator_1.SetFont(Config.UI_FONT + "S", Global.Content, "Blue", Config.UI_FONT);
            Separator_1.text = ":";
            Separator_2 = new RightAdjustedText();
            Separator_2.loc = new Vector2(64 + 4, 0);
            Separator_2.SetFont(Config.UI_FONT + "S", Global.Content, "Blue", Config.UI_FONT);
            Separator_2.text = ".";
            update();
        }

        public void update()
        {
            int time = Time;
            if (Time < 0)
                time = Global.game_system.total_play_time;
            if (Time == 0)
            {
                Hour.text = "--";
                Minute.text = "--";
                Second.text = "--";
            }
            else
            {
                int total_seconds = time / Config.FRAME_RATE;
                int hours = total_seconds / 60 / 60;
                int minutes = total_seconds / 60 % 60;
                int seconds = total_seconds % 60;

                Hour.text = hours.ToString();
                Minute.text = (minutes < 10 ? "0" : "") + minutes.ToString();
                Second.text = (seconds < 10 ? "0" : "") + seconds.ToString();
            }

            Separator_1.visible = (Global.game_system.total_play_time % Config.FRAME_RATE) < Config.FRAME_RATE / 2;
            Separator_2.visible = Separator_1.visible;
        }

        public void draw(SpriteBatch sprite_batch)
        {
            draw(sprite_batch, Vector2.Zero);
        }
        public void draw(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            Vector2 loc = this.loc + draw_vector();
            Label.draw(sprite_batch, -loc + draw_offset);
            Separator_1.draw(sprite_batch, -loc + draw_offset);
            Separator_2.draw(sprite_batch, -loc + draw_offset);
            Hour.draw(sprite_batch, -loc + draw_offset);
            Minute.draw(sprite_batch, -loc + draw_offset);
            Second.draw(sprite_batch, -loc + draw_offset);
        }
    }
}
