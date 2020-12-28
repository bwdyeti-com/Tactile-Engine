using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows;
using Tactile.Windows.UserInterface.Command;

namespace Tactile.Menus.Save
{
    class SaveMenuManager : InterfaceHandledMenuManager<ISaveMenuHandler>
    {
        public SaveMenuManager(ISaveMenuHandler handler)
            : base(handler)
        {
            var saveComparisonMenu = new SaveComparisonMenu();
            saveComparisonMenu.Opened += saveComparisonMenu_Opened;
            saveComparisonMenu.Closed += saveComparisonMenu_Closed;
            AddMenu(saveComparisonMenu);

            var saveMenuFadeIn = saveComparisonMenu.FadeInMenu(false);
            saveMenuFadeIn.Finished += menu_Closed;
            AddMenu(saveMenuFadeIn);
        }

        void saveComparisonMenu_Opened(object sender, EventArgs e)
        {
            var window = create_save_overwrite_window();

            var saveOverwriteMenu = new ConfirmationMenu(window);
            saveOverwriteMenu.Confirmed += saveOverwriteMenu_Confirmed;
            saveOverwriteMenu.Canceled += saveOverwriteMenu_Canceled;
            AddMenu(saveOverwriteMenu);
        }

        void saveComparisonMenu_Closed(object sender, EventArgs e)
        {
            Menus.Clear();
        }

        protected Window_Confirmation create_save_overwrite_window()
        {
            var saveOverwriteWindow = new Parchment_Confirm_Window();
            saveOverwriteWindow.set_text("Overwrite save?");
            saveOverwriteWindow.add_choice("Yes", new Vector2(16, 16));
            saveOverwriteWindow.add_choice("No", new Vector2(56, 16));
            saveOverwriteWindow.size = new Vector2(112, 48);
            saveOverwriteWindow.loc = new Vector2(
                (Config.WINDOW_WIDTH - saveOverwriteWindow.size.X) / 2, 24);

            string chapter_name = Global.data_chapters[Global.game_state.chapter_id].World_Map_Name;

            return saveOverwriteWindow;
        }

        void saveOverwriteMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            MenuHandler.SaveOverwriteConfirm();

            RemoveTopMenu();

            var saveComparisonMenu = Menus.Peek() as SaveComparisonMenu;
            var saveMenuFadeOut = saveComparisonMenu.FadeOutMenu();
            saveMenuFadeOut.Finished += menu_Closed;
            AddMenu(saveMenuFadeOut);
        }

        void saveOverwriteMenu_Canceled(object sender, EventArgs e)
        {
            MenuHandler.SaveOverwriteCancel();

            RemoveTopMenu();

            var saveComparisonMenu = Menus.Peek() as SaveComparisonMenu;
            var saveMenuFadeOut = saveComparisonMenu.FadeOutMenu();
            saveMenuFadeOut.Finished += menu_Closed;
            AddMenu(saveMenuFadeOut);
        }

        protected void menu_Closed(object sender, EventArgs e)
        {
            RemoveTopMenu();
        }
    }

    interface ISaveMenuHandler : IMenuHandler
    {
        void SaveOverwriteConfirm();
        void SaveOverwriteCancel();
    }
}
