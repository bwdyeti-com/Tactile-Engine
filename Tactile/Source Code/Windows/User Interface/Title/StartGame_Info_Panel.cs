using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile.Windows.UserInterface.Title
{
    class StartGame_Info_Panel : Title_Info_Panel
    {
        protected int Save_Id;
        protected bool Active, New_Game;
        TextSprite Save_Label;
        TextSprite Chapter, Short_Chapter, Save, Mode, Style, Map_Save, Suspend;
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

            Save_Label = new TextSprite();
            Save_Label.loc = new Vector2(0, 0);
            Save_Label.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Save_Label.text = "File";

            Chapter = new TextSprite();
            Chapter.loc = new Vector2(width / 2 - 56, 0);
            Chapter.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Short_Chapter = new TextSprite();
            Short_Chapter.loc = new Vector2(width / 2 - 56, 0);
            Short_Chapter.SetFont(Config.UI_FONT, Global.Content, "Yellow");

            Save = new RightAdjustedText();
            Save.loc = new Vector2(40, 0);
            Save.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Mode = new TextSprite();
            Mode.loc = new Vector2(width / 2 - 56, 16);
            Mode.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Style = new TextSprite();
            Style.loc = new Vector2(width / 2 - 16, 16);
            Style.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Map_Save = new TextSprite();
            Map_Save.loc = new Vector2(width / 2 - 56, 32);
            Map_Save.SetFont(Config.UI_FONT);
            Map_Save.text = "Checkpoint";
            Suspend = new TextSprite();
            Suspend.loc = new Vector2(width / 2, 32);
            Suspend.SetFont(Config.UI_FONT);
            Suspend.text = "Suspend";

            set_data(save_id);
        }

        private void create_window(int width, bool mainMenu)
        {
            Window = new System_Color_Window();
            Window.height = 32;
            Window.offset = new Vector2(16, 8);

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

                Tactile.IO.Save_Info info = Global.save_files_info[Save_Id];
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
                Map_Save.SetColor(Global.Content, info.map_save_exists ? "White" : "Grey");
                Suspend.SetColor(Global.Content, info.suspend_exists ? "White" : "Grey");
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
