#if !MONOGAME && DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Menus.Map;
using Tactile.Menus.Options;
using Tactile.Windows.Command;
using Tactile.Windows.Map;
using Tactile.Windows.UserInterface.Command;
using TactileLibrary;

namespace Tactile.Menus.Map
{
    enum Unit_Editor_Options
        { Unit, Add_Unit, Paste_Unit, Reinforcements, Options,
        Clear_Units, Mirror_Units, Playtest, Revert, Save, Quit, None }

    class UnitEditorMapMenuManager : InterfaceHandledMenuManager<IUnitEditorMapMenuHandler>
    {
        List<Data_Unit> ReinforcementData;

        public UnitEditorMapMenuManager(
            IUnitEditorMapMenuHandler handler,
            List<Data_Unit> reinforcementData)
                : base(handler)
        {
            ReinforcementData = reinforcementData;

            var mapMenu = new UnitEditorMapCommandMenu();
            mapMenu.Selected += mapMenu_Selected;
            mapMenu.Canceled += menu_ClosedCanceled;
            AddMenu(mapMenu);
        }

        // Selected an item in the map menu
        private void mapMenu_Selected(object sender, EventArgs e)
        {
            var mapMenu = (sender as UnitEditorMapCommandMenu);

            switch (mapMenu.SelectedOption)
            {
                case Unit_Editor_Options.Unit:
                    RemoveTopMenu();

                    var unitMenu = new Window_Unit_Team();
                    unitMenu.Status += unitMenu_Status;
                    unitMenu.Closed += unitMenu_Closed;
                    AddMenu(unitMenu);
                    break;
                case Unit_Editor_Options.Add_Unit:
                    MenuHandler.UnitEditorMapMenuAddUnit();
                    break;
                case Unit_Editor_Options.Paste_Unit:
                    MenuHandler.UnitEditorMapMenuPasteUnit();
                    break;
                case Unit_Editor_Options.Reinforcements:
                    RemoveTopMenu();

                    var reinforcementsMenu = new ReinforcementsMenu(ReinforcementData);
                    reinforcementsMenu.Selected += reinforcementsMenu_Selected;
                    reinforcementsMenu.Canceled += menu_ClosedCanceled;
                    AddMenu(reinforcementsMenu);
                    break;
                case Unit_Editor_Options.Options:
                    RemoveTopMenu();

                    var settingsTopMenu = new GameplaySettingsTopMenu(false);
                    settingsTopMenu.Selected += SettingsTopMenu_Selected;
                    settingsTopMenu.Closed += menu_ClosedCanceled;
                    settingsTopMenu.AddToManager(new MenuCallbackEventArgs(this.AddMenu, this.menu_Closed));
                    break;
                case Unit_Editor_Options.Clear_Units: // Clear Units
                    Vector2 optionLocation = mapMenu.SelectedOptionLocation;
                    
                    var clearUnitsWindow = new Window_Command(
                        optionLocation + new Vector2(8, 24),
                        80, new List<string> { "Confirm", "Cancel" });
                    clearUnitsWindow.immediate_index = 1;

                    var clearUnitsCommandMenu = new CommandMenu(clearUnitsWindow);
                    clearUnitsCommandMenu.Selected += clearUnitsCommandMenu_Selected;
                    clearUnitsCommandMenu.Canceled += confirmMenu_Canceled;
                    AddMenu(clearUnitsCommandMenu);
                    break;
                case Unit_Editor_Options.Mirror_Units: // Mirror Units
                    optionLocation = mapMenu.SelectedOptionLocation;

                    var mirrorUnitsWindow = new Window_Command(
                        optionLocation + new Vector2(8, 24),
                        80, new List<string> { "Confirm", "Cancel" });
                    mirrorUnitsWindow.immediate_index = 1;

                    var mirrorUnitsCommandMenu = new CommandMenu(mirrorUnitsWindow);
                    mirrorUnitsCommandMenu.Selected += mirrorUnitsCommandMenu_Selected;
                    mirrorUnitsCommandMenu.Canceled += confirmMenu_Canceled;
                    AddMenu(mirrorUnitsCommandMenu);
                    break;
                case Unit_Editor_Options.Playtest:
                    MenuHandler.UnitEditorMapMenuPlaytest();
                    break;
                case Unit_Editor_Options.Revert:
                    MenuHandler.UnitEditorMapMenuRevert();
                    break;
                case Unit_Editor_Options.Save:
                    MenuHandler.UnitEditorMapMenuSave();
                    break;
                case Unit_Editor_Options.Quit: // Quit
                    optionLocation = mapMenu.SelectedOptionLocation;

                    var quitConfirmWindow = new Window_Confirmation();
                    int height = 48;
                    quitConfirmWindow.loc = optionLocation + new Vector2(0, 24);
                    if (quitConfirmWindow.loc.Y + height > Config.WINDOW_HEIGHT)
                        quitConfirmWindow.loc = optionLocation + new Vector2(0, -40);
                    quitConfirmWindow.set_text("Are you sure?");
                    quitConfirmWindow.add_choice("Yes", new Vector2(16, 16));
                    quitConfirmWindow.add_choice("No", new Vector2(56, 16));
                    quitConfirmWindow.size = new Vector2(88, height);
                    quitConfirmWindow.index = 1;

                    var quitConfirmMenu = new ConfirmationMenu(quitConfirmWindow);
                    quitConfirmMenu.Confirmed += quitConfirmMenu_Confirmed;
                    quitConfirmMenu.Canceled += confirmMenu_Canceled;
                    AddMenu(quitConfirmMenu);
                    break;
            }
        }

        // Open status screen from unit menu
        void unitMenu_Status(object sender, EventArgs e)
        {
            Global.game_temp.menu_call = false;
            Global.game_temp.status_menu_call = false;
            // @Debug: team should be generated from the units shown in the Unit screen
            List<int> team = new List<int>();
            {
                // Only list units that are on the map or rescued (rescued units can be off map)
                team.AddRange(Global.game_map.teams[Global.game_temp.status_team]
                    .Where(x => x == Global.game_temp.status_unit_id ||
                        !Global.game_map.is_off_map(Global.game_map.units[x].loc) ||
                        Global.game_map.units[x].is_rescued));
            }
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

        // Selected an item in the reinforcements menu
        void reinforcementsMenu_Selected(object sender, EventArgs e)
        {
            var reinforcementsMenu = sender as ReinforcementsMenu;

            List<string> commands =
                new List<string> { "Edit", "Add", "Paste", "Delete" };
            var reinforcementsOptionsWindow = new Window_Command(
                reinforcementsMenu.WindowLoc +
                    new Vector2(WindowCommandReinforcements.WIDTH + 8, 0),
                80, commands);

            if (ReinforcementData.Count == 0)
            {
                reinforcementsOptionsWindow.set_text_color(0, "Grey");
                reinforcementsOptionsWindow.set_text_color(3, "Grey");
            }

            var reinforcementsOptionsMenu = new CommandMenu(reinforcementsOptionsWindow);
            reinforcementsOptionsMenu.Selected += reinforcementsOptionsMenu_Selected;
            reinforcementsOptionsMenu.Canceled += reinforcementsOptionsMenu_Canceled;
            AddMenu(reinforcementsOptionsMenu);
        }

        void reinforcementsOptionsMenu_Selected(object sender, EventArgs e)
        {
            var reinforcementsOptionsMenu = (sender as CommandMenu);

            Global.game_system.play_se(System_Sounds.Confirm);
            switch (reinforcementsOptionsMenu.SelectedIndex)
            {
                    // Edit
                case 0:
                    RemoveTopMenu();
                    var reinforcementsMenu = (Menus.Peek() as ReinforcementsMenu);
                    int index = reinforcementsMenu.Index - 1;

                    Menus.Clear();

                    MenuHandler.UnitEditorMapMenuEditReinforcement(index);
                    break;
                // Add
                case 1:
                // Paste
                case 2:
                    RemoveTopMenu();
                    reinforcementsMenu = (Menus.Peek() as ReinforcementsMenu);

                    index = reinforcementsMenu.Index;
                    if (ReinforcementData.Count == 0)
                        index = 0;

                    if (reinforcementsOptionsMenu.SelectedIndex == 1)
                        // Add
                        MenuHandler.UnitEditorMapMenuAddReinforcement(index);
                    else
                        // Paste
                        MenuHandler.UnitEditorMapMenuPasteReinforcement(index);

                    reinforcementsMenu.Refresh(index + 1);
                    break;
                // Delete
                case 3:
                    var deleteReinforcementWindow = new Window_Confirmation();
                    deleteReinforcementWindow.loc = new Vector2(Config.WINDOW_WIDTH - 96, 32);
                    deleteReinforcementWindow.set_text("Delete this\nreinforcement?");
                    deleteReinforcementWindow.add_choice("Yes", new Vector2(8, 32));
                    deleteReinforcementWindow.add_choice("No", new Vector2(48, 32));
                    deleteReinforcementWindow.size = new Vector2(88, 64);
                    deleteReinforcementWindow.index = 1;

                    var deleteReinforcementMenu = new ConfirmationMenu(deleteReinforcementWindow);
                    deleteReinforcementMenu.Confirmed += deleteReinforcementMenu_Confirmed;
                    deleteReinforcementMenu.Canceled += confirmMenu_Canceled;
                    AddMenu(deleteReinforcementMenu);
                    break;
            }
        }

        void deleteReinforcementMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);

            RemoveTopMenu();
            RemoveTopMenu();
            var reinforcementsMenu = (Menus.Peek() as ReinforcementsMenu);

            int index = reinforcementsMenu.Index - 1;

            MenuHandler.UnitEditorMapMenuDeleteReinforcement(index);

            reinforcementsMenu.Refresh(index + 1);
        }

        void reinforcementsOptionsMenu_Canceled(object sender, EventArgs e)
        {
            RemoveTopMenu();
            var reinforcementsMenu = Menus.Peek() as ReinforcementsMenu;
            reinforcementsMenu.Enable();
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
            settingsMenu.OpenSettingList += SettingsMenu_OpenSettingList;
            settingsMenu.Canceled += settingsMenu_Canceled;
            AddMenu(settingsMenu);
        }

        private void SettingsMenu_OpenSubMenu(object sender, EventArgs e)
        {
            var parentMenu = (sender as SettingsMenu);
            var settings = parentMenu.GetSubSettings();
            OpenSettingsMenu(settings, parentMenu);
        }

        private void SettingsMenu_OpenSettingList(object sender, EventArgs e)
        {
            var parentMenu = (sender as SettingsMenu);
            var settingListWindow = parentMenu.GetSettingListWindow();

            var settingListCommandMenu = new CommandMenu(settingListWindow, parentMenu);
            settingListCommandMenu.Selected += SettingListCommandMenu_Selected;
            settingListCommandMenu.Canceled += SettingListCommandMenu_Canceled;
            AddMenu(settingListCommandMenu);
        }

        void SettingListCommandMenu_Selected(object sender, EventArgs e)
        {
            var settingListCommandMenu = sender as CommandMenu;
            var settingsMenu = (Menus.ElementAt(1) as SettingsMenu);

            int settingIndex = settingListCommandMenu.SelectedIndex.ValueOrDefault;
            if (settingsMenu.SelectSettingListItem(settingIndex))
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                RemoveTopMenu();
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        void SettingListCommandMenu_Canceled(object sender, EventArgs e)
        {
            RemoveTopMenu();
        }

        void settingsMenu_Canceled(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Cancel);
            MenuHandler.UnitEditorMapMenuSaveConfig();

            menu_Closed(sender, e);
        }

        // Selected a choice in the clear units window
        void clearUnitsCommandMenu_Selected(object sender, EventArgs e)
        {
            if ((sender as CommandMenu).SelectedIndex.ValueOrDefault == 0)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                MenuHandler.UnitEditorMapMenuClearUnits();
            }
            else
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                confirmMenu_Canceled(sender, e);
            }
        }

        // Selected a choice in the mirror units window
        void mirrorUnitsCommandMenu_Selected(object sender, EventArgs e)
        {
            if ((sender as CommandMenu).SelectedIndex.ValueOrDefault == 0)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                MenuHandler.UnitEditorMapMenuMirrorUnits();
            }
            else
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                confirmMenu_Canceled(sender, e);
            }
        }

        // Quit game confirmed
        void quitConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            MenuHandler.UnitEditorMapMenuQuit();
        }

        // Confirmation menu canceled
        void confirmMenu_Canceled(object sender, EventArgs e)
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

        protected void menu_Closed(object sender, EventArgs e)
        {
            RemoveTopMenu();
        }
    }

    interface IUnitEditorMapMenuHandler : IMenuHandler
    {
        void UnitEditorMapMenuAddUnit();
        void UnitEditorMapMenuPasteUnit();

        void UnitEditorMapMenuEditReinforcement(int index);
        void UnitEditorMapMenuAddReinforcement(int index);
        void UnitEditorMapMenuPasteReinforcement(int index);
        void UnitEditorMapMenuDeleteReinforcement(int index);

        void UnitEditorMapMenuSaveConfig();
        void UnitEditorMapMenuClearUnits();
        void UnitEditorMapMenuMirrorUnits();
        void UnitEditorMapMenuPlaytest();
        void UnitEditorMapMenuRevert();
        void UnitEditorMapMenuSave();
        void UnitEditorMapMenuQuit();
    }
}
#endif
