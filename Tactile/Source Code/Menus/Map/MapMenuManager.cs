using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Tactile.Menus.Options;
using Tactile.Windows.Map;
using Tactile.Windows.UserInterface.Command;

namespace Tactile.Menus.Map
{
    enum Map_Menu_Options { Unit, Data, Options, Suspend, End, None }

    class MapMenuManager : InterfaceHandledMenuManager<IMapMenuHandler>
    {
        public MapMenuManager(IMapMenuHandler handler)
            : base(handler)
        {
            var mapMenu = new MapCommandMenu();
            mapMenu.Selected += mapMenu_Selected;
            mapMenu.Canceled += menu_ClosedCanceled;
            AddMenu(mapMenu);
        }

        // Selected an item in the map menu
        private void mapMenu_Selected(object sender, EventArgs e)
        {
            switch ((sender as MapCommandMenu).SelectedOption)
            {
                case Map_Menu_Options.Unit:
                    RemoveTopMenu();

                    var unitMenu = new Window_Unit();
                    unitMenu.Status += unitMenu_Status;
                    unitMenu.Closed += unitMenu_Closed;
                    AddMenu(unitMenu);
                    break;
                case Map_Menu_Options.Data:
                    RemoveTopMenu();

                    var dataMenu = new Window_Data();
                    dataMenu.Closed += menu_ClosedCanceled;
                    AddMenu(dataMenu);
                    break;
                case Map_Menu_Options.Options:
                    RemoveTopMenu();

                    var settingsTopMenu = new GameplaySettingsTopMenu(true);
                    settingsTopMenu.Selected += SettingsTopMenu_Selected;
                    settingsTopMenu.Closed += menu_ClosedCanceled;
                    settingsTopMenu.AddToManager(new MenuCallbackEventArgs(this.AddMenu, this.menu_Closed));
                    break;
                case Map_Menu_Options.Suspend:
                    var suspendConfirmWindow = new Parchment_Confirm_Window();
                    suspendConfirmWindow.set_text("Save and quit?", new Vector2(8, 0));
                    suspendConfirmWindow.add_choice("Yes", new Vector2(16, 16));
                    suspendConfirmWindow.add_choice("No", new Vector2(56, 16));
                    suspendConfirmWindow.size = new Vector2(104, 48);
                    suspendConfirmWindow.loc =
                        new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2 -
                        suspendConfirmWindow.size / 2;

                    var suspendConfirmMenu = new ConfirmationMenu(suspendConfirmWindow);
                    suspendConfirmMenu.Confirmed += suspendConfirmMenu_Confirmed;
                    suspendConfirmMenu.Canceled += menu_Closed;
                    AddMenu(suspendConfirmMenu);
                    break;
                case Map_Menu_Options.End:
                    // If there are no units left to move, just end the turn
                    if (!Global.game_map.ready_movable_units)
                        MenuHandler.MapMenuEndTurn();
                    else
                    {
                        var endTurnConfirmWindow = new Parchment_Confirm_Window();
                        endTurnConfirmWindow.set_text("End your turn?", new Vector2(8, 0));
                        endTurnConfirmWindow.add_choice("Yes", new Vector2(16, 16));
                        endTurnConfirmWindow.add_choice("No", new Vector2(56, 16));
                        endTurnConfirmWindow.size = new Vector2(104, 48);
                        endTurnConfirmWindow.loc =
                            new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2 -
                            endTurnConfirmWindow.size / 2;

                        var endTurnConfirmMenu = new ConfirmationMenu(endTurnConfirmWindow);
                        endTurnConfirmMenu.Confirmed += endTurnConfirmMenu_Confirmed;
                        endTurnConfirmMenu.Canceled += menu_Closed;
                        AddMenu(endTurnConfirmMenu);
                    }
                    break;
            }
        }
        
        // Open status screen from unit menu
        void unitMenu_Status(object sender, EventArgs e)
        {
            Global.game_temp.menu_call = false;
            Global.game_temp.status_menu_call = false;

            var unitWindow = (Menus.Peek() as Window_Unit);
            List<int> team = unitWindow.team;
            int index = team.IndexOf(Global.game_temp.status_unit_id);
            index = Math.Max(0, index);

            var statusMenu = new Window_Status(team, index);
            statusMenu.Closed += statusMenu_Closed;
            AddMenu(statusMenu);
        }

        // Unit menu canceled
        void unitMenu_Closed(object sender, EventArgs e)
        {
            var unitMenu = Menus.Peek() as Window_Unit;
            if (unitMenu.unit_selected)
                Global.player.force_loc(Global.game_map.units[unitMenu.unit_index].loc);

            menu_ClosedCanceled(sender, e);
        }

        private void SettingsTopMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var settingsTopMenu = (sender as GameplaySettingsTopMenu);

            int index = settingsTopMenu.Index;
            switch (index)
            {
                case 0:
                    var optionsMenu = new Window_Options(settingsTopMenu.SoloAnimAllowed);
                    optionsMenu.SoloAnim += optionsMenu_SoloAnim;
                    optionsMenu.Closed += menu_Closed;
                    AddMenu(optionsMenu);
                    break;
                case 1:
                    OpenSettingsMenu(Global.gameSettings.General, settingsTopMenu);
                    break;
                case 2:
                    OpenSettingsMenu(Global.gameSettings.Graphics, settingsTopMenu);
                    break;
                case 3:
                    OpenSettingsMenu(Global.gameSettings.Audio, settingsTopMenu);
                    break;
                case 4:
                    OpenSettingsMenu(Global.gameSettings.Controls, settingsTopMenu);
                    break;
            }
        }

        private void OpenSettingsMenu(Tactile.Options.ISettings settings, IHasCancelButton parent)
        {
            SettingsMenu settingsMenu;
            settingsMenu = new SettingsMenu(settings, parent);
            settingsMenu.OpenSubMenu += SettingsMenu_OpenSubMenu;
            settingsMenu.Canceled += settingsMenu_Canceled;
            AddMenu(settingsMenu);
        }

        private void SettingsMenu_OpenSubMenu(object sender, EventArgs e)
        {
            var parentMenu = (sender as SettingsMenu);
            var settings = parentMenu.GetSubSettings();
            OpenSettingsMenu(settings, parentMenu);
        }

        void settingsMenu_Canceled(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Cancel);
            MenuHandler.MapSaveConfig();

            menu_Closed(sender, e);
        }

        // Open unit screen from options to change solo anims
        void optionsMenu_SoloAnim(object sender, EventArgs e)
        {
            var soloAnimMenu = new Window_SoloAnim();
            soloAnimMenu.Status += unitMenu_Status;
            soloAnimMenu.Closed += soloAnimMenu_Closed;
            AddMenu(soloAnimMenu);
        }

        void soloAnimMenu_Closed(object sender, EventArgs e)
        {
            RemoveTopMenu();
        }

        // Suspend confirmed
        private void suspendConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            MenuHandler.MapMenuSuspend();
        }

        // End turn confirmed
        private void endTurnConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            MenuHandler.MapMenuEndTurn();
        }

        // Status menu canceled
        void statusMenu_Closed(object sender, EventArgs e)
        {
            var statusMenu = Menus.Peek() as Window_Status;
            int currentUnit = statusMenu.current_unit;

            Global.game_temp.status_team = 0;
            statusMenu.close();
            RemoveTopMenu();

            var unitMenu = Menus.Peek() as Window_Unit;
            unitMenu.unit_index = currentUnit;
        }

        protected void menu_Closed(object sender, EventArgs e)
        {
            RemoveTopMenu();
        }

        // Close the menu
        void menu_ClosedCanceled(object sender, EventArgs e)
        {
            Menus.Clear();
            Global.game_temp.menuing = false;
            Global.game_map.highlight_test();
        }
    }

    interface IMapMenuHandler : IMenuHandler
    {
        void MapSaveConfig();
        void MapMenuSuspend();
        void MapMenuEndTurn();
    }
}
