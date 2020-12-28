using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Tactile.Windows.Command;

namespace Tactile.Menus.Title
{
    enum Start_Game_Options { World_Map, Load_Suspend, Load_Map_Save, Move, Copy, Delete }
    class FileSelectedCommandMenu : CommandMenu
    {
        private List<int> StartGameOptionRedirect = new List<int>();

        public FileSelectedCommandMenu(Vector2 loc, int fileId)
            : base()
        {
            OpenWindow(loc, fileId);
            CreateCancelButton(Config.WINDOW_WIDTH - 64, Config.TITLE_MENU_DEPTH);
        }

        private void OpenWindow(Vector2 loc, int fileId)
        {
            List<string> strs = new List<string>();
            int width;

            StartGameOptionRedirect.Clear();
            strs.Add(string.IsNullOrEmpty(
                Global.save_files_info[fileId].chapter_id) ?
                "Start Game" : "Select Chapter");
            StartGameOptionRedirect.Add((int)Start_Game_Options.World_Map);
            if (Global.save_files_info[fileId].suspend_exists)
            {
                strs.Add("Continue");
                StartGameOptionRedirect.Add((int)Start_Game_Options.Load_Suspend);
            }
            if (Global.save_files_info[fileId].map_save_exists)
            {
                strs.Add("Checkpoint");
                StartGameOptionRedirect.Add((int)Start_Game_Options.Load_Map_Save);
            }
            if (Global.save_files_info.Count < Config.SAVES_PER_PAGE * Config.SAVE_PAGES)
            {
                strs.Add("Move");
                StartGameOptionRedirect.Add((int)Start_Game_Options.Move);
                strs.Add("Copy");
                StartGameOptionRedirect.Add((int)Start_Game_Options.Copy);
            }
            strs.Add("Delete");
            StartGameOptionRedirect.Add((int)Start_Game_Options.Delete);
            width = 80;

            loc += new Vector2(-width, 0);
            loc.Y = Math.Min(loc.Y, (Config.WINDOW_HEIGHT - 16) - (strs.Count + 1) * 16);

            Window = new Window_Command(loc, width, strs);
            Window.stereoscopic = Config.TITLE_CHOICE_DEPTH;
        }

        public Start_Game_Options SelectedOption
        {
            get
            {
                return (Start_Game_Options)StartGameOptionRedirect[this.SelectedIndex];
            }
        }
    }
}
