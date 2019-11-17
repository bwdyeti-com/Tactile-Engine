using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;

namespace FEXNA.Windows.UserInterface.Title
{
    class Suspend_Info_Panel : Title_Info_Panel
    {
        Miniface Lord_Mini;
        FE_Text Turn_Label, Units_Label, Save_Label;
        FE_Text Chapter, Turn, Units, Gold, Save, Mode, Style;
        Play_Time_Counter Counter;
        Sprite Gold_G;
        Sprite Screenshot;

        #region Accessors
        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Lord_Mini.stereoscopic = value;
                Turn_Label.stereoscopic = value;
                Units_Label.stereoscopic = value;
                Save_Label.stereoscopic = value;
                Chapter.stereoscopic = value;
                Turn.stereoscopic = value;
                Units.stereoscopic = value;
                Gold.stereoscopic = value;
                Save.stereoscopic = value;
                Mode.stereoscopic = value;
                Style.stereoscopic = value;
                Counter.stereoscopic = value;
                Gold_G.stereoscopic = value;
            }
        }
        #endregion

        public Suspend_Info_Panel(bool mainMenu = false)
            : this(mainMenu, Global.suspend_file_info) { }
        public Suspend_Info_Panel(bool mainMenu, IO.Suspend_Info suspend_file_info)
        {
            this.offset = new Vector2(-12, -4);

            create_window(mainMenu, suspend_file_info);

            Lord_Mini = new Miniface();
            Lord_Mini.set_actor(suspend_file_info.lord_actor_face);
            Lord_Mini.loc = new Vector2(24, 0);

            Turn_Label = new FE_Text();
            Turn_Label.loc = new Vector2(48, 16);
            Turn_Label.Font = "FE7_Text";
            Turn_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Units_Label = new FE_Text();
            Units_Label.loc = new Vector2(112, 16);
            Units_Label.Font = "FE7_Text";
            Units_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Units_Label.text = "Units";
            Save_Label = new FE_Text();
            Save_Label.loc = new Vector2(0, 48);
            Save_Label.Font = "FE7_Text";
            Save_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Save_Label.text = "File";

            Chapter = new FE_Text();
            Chapter.loc = new Vector2(48, 0);
            Chapter.Font = "FE7_Text";
            Chapter.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Chapter.text = !Global.data_chapters.ContainsKey(suspend_file_info.chapter_id) ?
                suspend_file_info.chapter_id :
                Global.data_chapters[suspend_file_info.chapter_id].FileSelectName;
            Turn = new FE_Text_Int();
            Turn.loc = new Vector2(88, 16);
            Turn.Font = "FE7_Text";
            Turn.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Units = new FE_Text_Int();
            Units.loc = new Vector2(152, 16);
            Units.Font = "FE7_Text";
            Units.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Units.text = suspend_file_info.units.ToString();
            Gold = new FE_Text_Int();
            Gold.loc = new Vector2(140, 32);
            Gold.Font = "FE7_Text";
            Gold.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Gold.text = suspend_file_info.gold.ToString();
            Save = new FE_Text_Int();
            Save.loc = new Vector2(40, 48);
            Save.Font = "FE7_Text";
            Save.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Save.text = suspend_file_info.save_id.ToString();
            Mode = new FE_Text();
            Mode.loc = new Vector2(80, 48);
            Mode.Font = "FE7_Text";
            Mode.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Mode.text = suspend_file_info.difficulty.ToString();
            Style = new FE_Text();
            Style.loc = new Vector2(120, 48);
            Style.Font = "FE7_Text";
            Style.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Style.text = suspend_file_info.style.ToString();//.Substring(0, Math.Min(3, suspend_file_info.style.ToString().Length)); //Debug

            if (suspend_file_info.home_base)
            {
                Turn_Label.text = "Home Base";
                Units_Label.visible = false;
                Units.visible = false;
            }
            else if (suspend_file_info.preparations)
                Turn_Label.text = "Preparations";
            else
            {
                Turn_Label.text = "Turn";
                Turn.text = suspend_file_info.turn.ToString();
            }

            Counter = new Play_Time_Counter(suspend_file_info.playtime);
            Counter.loc = new Vector2(-4, 32);

            Gold_G = new Sprite();
            Gold_G.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Gold_G.loc = new Vector2(140, 32);
            Gold_G.src_rect = new Rectangle(0, 160, 16, 16);

            Screenshot = new Sprite();
            Screenshot.texture = suspend_file_info.Screenshot;
            Screenshot.loc = new Vector2(0, 0);
            Screenshot.scale = new Vector2(0.25f, 0.25f);
        }

        private void create_window(bool mainMenu, IO.Suspend_Info suspend_file_info)
        {
            Window = new System_Color_Window();
            Window.height = 80;
            Window.offset = new Vector2(16, 8);

            Window.width = 208;
            if (Window is System_Color_Window)
                (Window as System_Color_Window).color_override =
                    Constants.Difficulty.DIFFICULTY_COLOR_REDIRECT[suspend_file_info.difficulty];
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = (draw_offset + this.offset) -
                (this.loc + this.draw_offset + stereo_offset());

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Window.draw(sprite_batch, loc);
            Turn_Label.draw(sprite_batch, loc);
            Units_Label.draw(sprite_batch, loc);
            Save_Label.draw(sprite_batch, loc);

            Chapter.draw(sprite_batch, loc);
            Turn.draw(sprite_batch, loc);
            Units.draw(sprite_batch, loc);
            Gold.draw(sprite_batch, loc);
            Save.draw(sprite_batch, loc);
            Mode.draw(sprite_batch, loc);
            Style.draw(sprite_batch, loc);
            Counter.draw(sprite_batch, loc);
            Gold_G.draw(sprite_batch, loc);
            //Screenshot.draw(sprite_batch, loc); //Yeti
            sprite_batch.End();

            Lord_Mini.draw(sprite_batch, loc);
        }
    }
}
