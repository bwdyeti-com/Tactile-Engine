using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;

namespace FEXNA.Windows.UserInterface.Title
{
    class StartGame_Info_Panel : Title_Info_Panel
    {
        protected int Save_Id;
        protected bool Active, New_Game;
        FE_Text Save_Label;
        FE_Text Chapter, Short_Chapter, Save, Mode, Style, Map_Save, Suspend;
        private bool MainMenu;

        #region Accessors
        public bool active
        {
            set
            {
                Active = value;
                if (Active)
                    Window.height = 64;
                else
                    Window.height = 32;

                //FEGame
                if (MainMenu)
                    Window.height += 8;

                Size = new Vector2(Size.X, Window.height - 8);
            }
        }

        public Color tint { set { Window.tint = value; } }

        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Save_Label.stereoscopic = value;
                Chapter.stereoscopic = value;
                Short_Chapter.stereoscopic = value;
                Save.stereoscopic = value;
                Mode.stereoscopic = value;
                Style.stereoscopic = value;
                Map_Save.stereoscopic = value;
                Suspend.stereoscopic = value;
            }
        }
        #endregion

        public StartGame_Info_Panel(int save_id, int width, bool mainMenu = false)
        {
            this.offset = new Vector2(-12, -4);

            create_window(width, mainMenu);

            Size = new Vector2(width - 8, Window.height);

            Save_Label = new FE_Text();
            Save_Label.loc = new Vector2(0, 0);
            Save_Label.Font = "FE7_Text";
            Save_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Save_Label.text = "File";

            Chapter = new FE_Text();
            Chapter.loc = new Vector2(width / 2 - 56, 0);
            Chapter.Font = "FE7_Text";
            Chapter.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Short_Chapter = new FE_Text();
            Short_Chapter.loc = new Vector2(width / 2 - 56, 0);
            Short_Chapter.Font = "FE7_Text";
            Short_Chapter.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");

            Save = new FE_Text_Int();
            Save.loc = new Vector2(40, 0);
            Save.Font = "FE7_Text";
            Save.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Mode = new FE_Text();
            Mode.loc = new Vector2(width / 2 - 56, 16);
            Mode.Font = "FE7_Text";
            Mode.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Style = new FE_Text();
            Style.loc = new Vector2(width / 2 - 16, 16);
            Style.Font = "FE7_Text";
            Style.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Map_Save = new FE_Text();
            Map_Save.loc = new Vector2(width / 2 - 56, 32);
            Map_Save.Font = "FE7_Text";
            Map_Save.text = "Checkpoint";
            Suspend = new FE_Text();
            Suspend.loc = new Vector2(width / 2, 32);
            Suspend.Font = "FE7_Text";
            Suspend.text = "Suspend";

            set_data(save_id);
        }

        private void create_window(int width, bool mainMenu)
        {
            //FEGame
            if (mainMenu)
            {
                MainMenu = true;
                //Yeti
                //Window = new System_Color_Window(Global.Content.Load<Texture2D>(
                //    "Graphics/Windowskins/MainMenuWindow"),
                //    128, 8, 24, 16, 8, 16);
                Window = new WindowPanel(Global.Content.Load<Texture2D>(
                    "Graphics/Windowskins/MainMenuWindow"), Vector2.Zero,
                    128, 8, 24, 16, 8, 16);
                Window.height = 40;
                Window.offset = new Vector2(16, 16);
            }
            else
            {
                Window = new System_Color_Window();
                Window.height = 32;
                Window.offset = new Vector2(16, 8);
            }

            Window.width = width;
        }

        public void set_data(int save_id)
        {
            Save_Id = save_id;
            // If no save exists
            if (Save_Id == -1 || Global.save_files_info == null || !Global.save_files_info.ContainsKey(Save_Id))
            {
                if (Window is System_Color_Window)
                {
                    var window = Window as System_Color_Window;
                    if (Global.save_files_info == null || !Global.save_files_info.ContainsKey(Save_Id))
                        window.color_override = Constants.Difficulty.DIFFICULTY_COLOR_REDIRECT[Difficulty_Modes.Normal];
                    else
                        window.color_override = Constants.Difficulty.DIFFICULTY_COLOR_REDIRECT[Global.save_files_info[Save_Id].difficulty];
                }
                New_Game = true;
                Chapter.text = Short_Chapter.text = "-----";
                Short_Chapter.text = "-----";
            }
            else
            {
                New_Game = false;

                FEXNA.IO.Save_Info info = Global.save_files_info[Save_Id];
                if (Window is System_Color_Window)
                    (Window as System_Color_Window).color_override =
                        Constants.Difficulty.DIFFICULTY_COLOR_REDIRECT[info.difficulty];
                if (string.IsNullOrEmpty(info.chapter_id))
                    Chapter.text = Short_Chapter.text = "New Game";
                else
                {
                    string actual_chapter_id = !Global.data_chapters.ContainsKey(info.chapter_id) ?
                        info.chapter_id.Substring(0, info.chapter_id.Length - 1) : info.chapter_id;

                    if (Global.data_chapters.ContainsKey(info.chapter_id))
                    {
                        Chapter.text = Global.data_chapters[info.chapter_id].FileSelectName;
                        Short_Chapter.text = Global.data_chapters[info.chapter_id].ShortName;
                    }
                    else
                    {
                        Chapter.text = info.chapter_id;
                        Short_Chapter.text = info.chapter_id;
                    }
                }
                Mode.text = info.difficulty.ToString();
                Style.text = info.style.ToString();
                Map_Save.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + (info.map_save_exists ? "White" : "Grey"));
                Suspend.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + (info.suspend_exists ? "White" : "Grey"));
            }
            if (Save_Id != -1)
            {
                Save.text = Save_Id.ToString();
            }
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = (draw_offset + this.offset) -
                (this.loc + this.draw_offset + stereo_offset());

            Window.draw(sprite_batch, loc);
            if (Active)
                Chapter.draw(sprite_batch, loc);
            else
                Short_Chapter.draw(sprite_batch, loc);
            if (Save_Id != -1)
            {
                Save_Label.draw(sprite_batch, loc);
                Save.draw(sprite_batch, loc);
            }
            if (!New_Game)
            {
                if (!Active)
                    Style.draw(sprite_batch, loc - new Vector2(72, -16));
                else
                {
                    Mode.draw(sprite_batch, loc);
                    Style.draw(sprite_batch, loc);
                    Map_Save.draw(sprite_batch, loc);
                    Suspend.draw(sprite_batch, loc);
                }
            }
        }
    }
}
