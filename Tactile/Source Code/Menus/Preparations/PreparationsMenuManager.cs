using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Menus.Map;
using Tactile.Windows;
using Tactile.Windows.Command;
using Tactile.Windows.Map;
using Tactile.Windows.Map.Items;
using Tactile.Windows.UserInterface.Command;

namespace Tactile.Menus.Preparations
{
    enum PreparationsChoices { Pick_Units, Trade, Fortune, Check_Map, Save, None }

    class PreparationsMenuManager : SetupMenuManager<IPreparationsMenuHandler>
    {
        public PreparationsMenuManager(
            IPreparationsMenuHandler handler,
            bool immediateBlackScreen = false,
            bool deployUnits = false)
                : base(handler)
        {
            AddPreparationsMenu(immediateBlackScreen, deployUnits);
        }

        public override void ResumeItemUse()
        {
            var preparationsMenu = Menus.Peek() as Window_Setup;
            preparationsMenu.index = (int)PreparationsChoices.Trade;

            AddItemMenu(true);
        }

        public void CheckMap(bool changingFormation)
        {
            var preparationsMenu = Menus.Peek() as Window_Setup;
            preparationsMenu.skip_black_screen();

            var checkMapMenu = GetCheckMapMenu();
            checkMapMenu.Show();
            if (changingFormation)
                checkMapMenu.index = 1;
            AddMenu(checkMapMenu);
        }

        private void AddPreparationsMenu(
            bool immediateBlackScreen = false,
            bool deployUnits = false)
        {
            var preparationsMenu = new Window_Setup(deployUnits);
            if (immediateBlackScreen)
                preparationsMenu.black_screen();
            preparationsMenu.CheckMap += preparationsMenu_CheckMap;
            preparationsMenu.Selected += homeBaseMenu_Selected;
            preparationsMenu.Start += preparationsMenu_Start;
            preparationsMenu.Closed += preparationsMenu_Closed;
            AddMenu(preparationsMenu);
        }

        private Window_Setup_CheckMap GetCheckMapMenu()
        {
            var checkMapMenu = new Window_Setup_CheckMap();
            checkMapMenu.Selected += checkMapMenu_Selected;
            checkMapMenu.Hidden += checkMapMenu_Hidden;
            checkMapMenu.Closed += checkMapMenu_Closed;
            return checkMapMenu;
        }

        protected override void AddUnitMenuCancelEvent(Window_Unit unitMenu)
        {
            unitMenu.Closed += unitMenu_Closed;
        }

        // Pressed start on preparations menu
        void preparationsMenu_Start(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var preparationsMenu = sender as Window_Setup;
            preparationsMenu.close(true);
        }

        // Preparations menu closed
        void preparationsMenu_Closed(object sender, EventArgs e)
        {
            MenuHandler.PreparationsLeave();
        }

        // Selected an item in the home base menu
        private void homeBaseMenu_Selected(object sender, EventArgs e)
        {
            var homeBaseMenu = sender as Window_Setup;
            switch ((PreparationsChoices)homeBaseMenu.SelectedOption)
            {
                case PreparationsChoices.Pick_Units:
#if DEBUG
                    System.Diagnostics.Debug.Assert(
                        Global.battalion.actors.Count > 0, "Battalion is empty");
#endif
                    Global.game_system.play_se(System_Sounds.Confirm);
                    var pickUnitsMenu = new Window_Prep_PickUnits();
                    pickUnitsMenu.Status += overviewMenu_Status;
                    pickUnitsMenu.Unit += overviewMenu_Unit;
                    pickUnitsMenu.Closing += pickUnitsMenu_Closing;
                    pickUnitsMenu.Closed += pickUnitsMenu_Closed;
                    AddMenu(pickUnitsMenu);
                    break;
                case PreparationsChoices.Trade:
                    if (Global.battalion.actors.Count > 0)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        AddItemMenu();
                    }
                    else
                        // Buzz if battalion is empty
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                case PreparationsChoices.Fortune:
                    if (Global.game_state.augury_event_exists())
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        var auguryMenu = new AuguryMenu();
                        auguryMenu.StartEvent += auguryMenu_StartEvent;
                        auguryMenu.Closed += menu_Closed;
                        AddMenu(auguryMenu);
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                case PreparationsChoices.Check_Map:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    var checkMapMenu = GetCheckMapMenu();
                    AddMenu(checkMapMenu);
                    break;
                case PreparationsChoices.Save:
                    AddSaveMenu();
                    break;
            }
        }

        // Pick units menu closed
        void pickUnitsMenu_Closed(object sender, EventArgs e)
        {
            var pickUnitsMenu = sender as Window_Prep_PickUnits;

            if (pickUnitsMenu.pressed_start)
                preparationsMenu_Closed(sender, e);
            else
                menu_Closed(sender, e);
        }

        // Check map menu closed
        void checkMapMenu_Closed(object sender, EventArgs e)
        {
            var checkMapMenu = sender as Window_Setup_CheckMap;

            if (checkMapMenu.starting_map)
                preparationsMenu_Closed(sender, e);
            else
                menu_Closed(sender, e);
        }

        // Pick units menu closing
        void pickUnitsMenu_Closing(object sender, EventArgs e)
        {
            var pickUnitsMenu = sender as Window_Prep_PickUnits;

            var preparationsMenu = (Menus.ElementAt(1) as Window_Setup);
            // Execute the deployment changes the player chose
            preparationsMenu.refresh_deployed_units(
                pickUnitsMenu.unit_changes());
            preparationsMenu.refresh();
        }

        // Unit menu closed
        void unitMenu_Closed(object sender, EventArgs e)
        {
            menu_Closed(sender, e);

            if (Menus.Peek() is Window_Prep_PickUnits)
                Global.game_map.clear_preparations_unit_team();
        }

        // Augury menu faded out
        void auguryMenu_StartEvent(object sender, EventArgs e)
        {
            Global.game_state.activate_augury_event();
        }

        // Pressed B on preparations menu
        private void preparationsMenu_CheckMap(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Cancel);
            var checkMapMenu = GetCheckMapMenu();
            AddMenu(checkMapMenu);
        }

        // Selected an item in the check map menu
        void checkMapMenu_Selected(object sender, EventArgs e)
        {
            var checkMapMenu = sender as Window_Setup_CheckMap;

            switch (checkMapMenu.SelectedOption)
            {
                case PrepCheckMapResults.ViewMap:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Global.game_map.highlight_test();
                    checkMapMenu.HideToViewMap();
                    break;
                case PrepCheckMapResults.Formation:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    checkMapMenu.HideToChangeFormation();
                    break;
                case PrepCheckMapResults.Options:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    AddOptionsMenu();
                    break;
                case PrepCheckMapResults.Save:
                    AddSaveMenu();
                    break;
                case PrepCheckMapResults.StartChapter:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    checkMapMenu.close(true);
                    break;
                case PrepCheckMapResults.Cancel:
                    Global.game_system.play_se(System_Sounds.Cancel);
                    checkMapMenu.close();
                    break;
                case PrepCheckMapResults.Info:
                    break;
            }
        }

        // Check map menu finished hiding
        void checkMapMenu_Hidden(object sender, EventArgs e)
        {
            var checkMapMenu = sender as Window_Setup_CheckMap;

            switch (checkMapMenu.SelectedOption)
            {
                case PrepCheckMapResults.ViewMap:
                    MenuHandler.PreparationsViewMap();
                    break;
                case PrepCheckMapResults.Formation:
                    MenuHandler.PreparationsChangeFormation();
                    break;
                default:
                    checkMapMenu.Show();
                    //throw new ArgumentException(); //@Debug
                    break;
            }
        }
    }

    interface IPreparationsMenuHandler : ISetupMenuHandler
    {
        void PreparationsViewMap();
        void PreparationsChangeFormation();
        void PreparationsLeave();
    }
}
