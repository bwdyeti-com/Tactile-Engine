using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Menus.Map;
using FEXNA.Windows;
using FEXNA.Windows.Command;
using FEXNA.Windows.Map;
using FEXNA.Windows.Map.Items;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Menus.Preparations
{
    enum HomeBaseChoices { Manage, Talk, Codex, Options, Save, None }
    //Sparring
    enum HomeBaseManageChoices { Organize, Trade, Training, Support, None }

    class HomeBaseMenuManager : SetupMenuManager<IHomeBaseMenuHandler>
    {
        public HomeBaseMenuManager(
            IHomeBaseMenuHandler handler,
            bool immediateBlackScreen = false)
                : base(handler)
        {
            var homeBaseMenu = new Window_Home_Base();
            if (immediateBlackScreen)
                homeBaseMenu.black_screen();
            homeBaseMenu.Selected += homeBaseMenu_Selected;
            homeBaseMenu.Start += homeBaseMenu_Start;
            homeBaseMenu.Closed += homeBaseMenu_Closed;
            AddMenu(homeBaseMenu);
        }

        //Sparring
        public void ResumeSparring()
        {
            // Manage
            var homeBaseMenu = Menus.Peek() as Window_Home_Base;
            homeBaseMenu.index = (int)HomeBaseChoices.Manage;
            Vector2 optionLocation = homeBaseMenu.SelectedOptionLocation;

            var manageCommandMenu = GetManageMenu(optionLocation,
                (int)HomeBaseManageChoices.Training);
            manageCommandMenu.IndexChanged += manageCommandMenu_IndexChanged;
            AddMenu(manageCommandMenu);

            homeBaseMenu.refresh_manage_text(
                (HomeBaseManageChoices)manageCommandMenu.Index);

            // Training
            AddSparringMenu(true);
        }

        public override void ResumeItemUse()
        {
            // Manage
            var homeBaseMenu = Menus.Peek() as Window_Home_Base;
            homeBaseMenu.index = (int)HomeBaseChoices.Manage;
            Vector2 optionLocation = homeBaseMenu.SelectedOptionLocation;

            var manageCommandMenu = GetManageMenu(optionLocation,
                (int)HomeBaseManageChoices.Trade);
            manageCommandMenu.IndexChanged += manageCommandMenu_IndexChanged;
            AddMenu(manageCommandMenu);

            homeBaseMenu.refresh_manage_text(
                (HomeBaseManageChoices)manageCommandMenu.Index);

            AddItemMenu(true);
        }

        private CommandMenu GetManageMenu(Vector2 optionLocation,
            int index = 0)
        {
            // Command Window
            Vector2 manage_loc = optionLocation + new Vector2(48, 8);
            //Sparring
            var manageCommandWindow = new Window_Command(manage_loc, 72,
                new List<string> { "Organize", "Items", "Training", "Support" });
            manageCommandWindow.text_offset = new Vector2(8, 0);
            manageCommandWindow.glow = true;
            manageCommandWindow.bar_offset = new Vector2(-8, 0);
            manageCommandWindow.stereoscopic = Config.PREPMAIN_WINDOW_DEPTH;

            manageCommandWindow.index = index;

            //Sparring
            if (Scene_Map.debug_chapter_options_blocked())
                manageCommandWindow.set_text_color(
                    (int)HomeBaseManageChoices.Training, "Grey");

            var manageCommandMenu = new CommandMenu(manageCommandWindow);
            manageCommandMenu.CreateCancelButton(
                Config.WINDOW_WIDTH - 128, Config.PREPMAIN_INFO_DEPTH);
            manageCommandMenu.Selected += manageCommandMenu_Selected;
            manageCommandMenu.Canceled += menu_Closed;
            return manageCommandMenu;
        }

        // Selected an item in the home base menu
        private void homeBaseMenu_Selected(object sender, EventArgs e)
        {
            var homeBaseMenu = sender as Window_Home_Base;
            switch ((HomeBaseChoices)homeBaseMenu.SelectedOption)
            {
                case HomeBaseChoices.Manage:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Vector2 optionLocation = homeBaseMenu.SelectedOptionLocation;

                    var manageCommandMenu = GetManageMenu(optionLocation);
                    manageCommandMenu.IndexChanged += manageCommandMenu_IndexChanged;
                    AddMenu(manageCommandMenu);

                    homeBaseMenu.refresh_manage_text(
                        (HomeBaseManageChoices)manageCommandMenu.Index);
                    break;
                case HomeBaseChoices.Talk:
                    if (!homeBaseMenu.talk_events_exist)
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        var talkMenu = new BaseConvoMenu(new Vector2(48,
                            homeBaseMenu.SelectedOption * 16 + 32));
                        talkMenu.CreateCancelButton(
                            Config.WINDOW_WIDTH - 128, Config.PREPMAIN_INFO_DEPTH);
                        talkMenu.Selected += talkMenu_Selected;
                        talkMenu.StartEvent += talkMenu_StartEvent;
                        talkMenu.EventStopped += talkMenu_EventStopped;
                        talkMenu.EndEvent += menu_Closed;
                        talkMenu.Canceled += menu_Closed;
                        AddMenu(talkMenu);
                    }
                    break;
                case HomeBaseChoices.Codex:
                    Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                case HomeBaseChoices.Options:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    AddOptionsMenu();
                    break;
                case HomeBaseChoices.Save:
                    AddSaveMenu();
                    break;
            }
        }

        void homeBaseMenu_Start(object sender, EventArgs e)
        {
            var leaveConfirmWindow = new Parchment_Confirm_Window();
            leaveConfirmWindow.set_text("Leave base?", new Vector2(8, 0));
            leaveConfirmWindow.add_choice("Yes", new Vector2(16, 16));
            leaveConfirmWindow.add_choice("No", new Vector2(56, 16));
            leaveConfirmWindow.size = new Vector2(96, 48);
            leaveConfirmWindow.loc =
                new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2 -
                leaveConfirmWindow.size / 2;

            var leaveConfirmMenu = new ConfirmationMenu(leaveConfirmWindow);
            leaveConfirmMenu.Confirmed += leaveConfirmMenu_Confirmed;
            leaveConfirmMenu.Canceled += menu_Closed;
            AddMenu(leaveConfirmMenu);
        }

        void leaveConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            RemoveTopMenu();

            var homeBaseMenu = (Menus.Peek() as Window_Home_Base);
            homeBaseMenu.close(true);
        }

        void homeBaseMenu_Closed(object sender, EventArgs e)
        {
            MenuHandler.HomeBaseLeave();
        }

        #region Manage
        void manageCommandMenu_IndexChanged(object sender, EventArgs e)
        {
            var manageCommandMenu = sender as CommandMenu;
            (Menus.ElementAt(1) as Window_Home_Base)
                .refresh_manage_text((HomeBaseManageChoices)manageCommandMenu.Index);
        }

        // Selected an item in the manage menu
        void manageCommandMenu_Selected(object sender, EventArgs e)
        {
            var manageMenu = sender as CommandMenu;

            switch ((HomeBaseManageChoices)manageMenu.SelectedIndex.ValueOrDefault)
            {
                case HomeBaseManageChoices.Organize:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    var organizeMenu = new Window_Prep_Organize();
                    organizeMenu.Status += overviewMenu_Status;
                    organizeMenu.Unit += overviewMenu_Unit;
                    organizeMenu.Closed += menu_Closed;
                    AddMenu(organizeMenu);
                    break;
                case HomeBaseManageChoices.Trade:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    AddItemMenu();
                    break;
                //Sparring
                case HomeBaseManageChoices.Training:
                    if (Scene_Map.debug_chapter_options_blocked())
                    {
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    }
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        AddSparringMenu();
                    }
                    break;
                case HomeBaseManageChoices.Support:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    var supportMenu = new Window_Base_Support();
                    supportMenu.UnitSelected += SupportMenu_UnitSelected;
                    supportMenu.Support += supportMenu_Support;
                    supportMenu.SupportEnded += SupportMenu_SupportEnded;
                    supportMenu.Status += preparationsMenu_Status;
                    supportMenu.Closed += menu_Closed;
                    AddMenu(supportMenu);
                    break;
            }
        }

        //Sparring
        #region Sparring
        private void AddSparringMenu(bool resuming = false)
        {
            Window_Sparring.reset();
            var sparringMenu = new Window_Sparring();
            if (resuming)
                sparringMenu.black_screen();
            sparringMenu.Spar += sparringMenu_Spar;
            sparringMenu.BattlersSelected += SparringMenu_BattlersSelected;
            sparringMenu.Status += preparationsMenu_Status;
            sparringMenu.Closed += menu_Closed;
            AddMenu(sparringMenu);
        }

        void sparringMenu_Spar(object sender, EventArgs e)
        {
            var sparringWindow = (sender as Window_Sparring);

            menu_Closed(Menus.Peek(), e); // Screen Fade
            menu_Closed(Menus.Peek(), e); // Confirmation Menu

            MenuHandler.HomeBaseTraining(
                Window_Sparring.Healer_Id,
                Window_Sparring.Battler_1_Id,
                Window_Sparring.Battler_2_Id);
        }
        
        private void SparringMenu_BattlersSelected(object sender, EventArgs e)
        {
            var sparringWindow = (sender as Window_Sparring);

            var sparringConfirmWindow = sparringWindow.ConfirmWindow();
            var sparringConfirmMenu = new ConfirmationMenu(sparringConfirmWindow);
            sparringConfirmMenu.Confirmed += SparringConfirmMenu_Confirmed;
            sparringConfirmMenu.Canceled += SparringConfirmMenu_Canceled;
            AddMenu(sparringConfirmMenu);
        }
        
        private void SparringConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            var sparringWindow = (Menus.ElementAt(1) as Window_Sparring);

            if (sparringWindow.AcceptBattle())
            {
                var sparringSceneFadeIn = sparringWindow.SparringFadeIn();
                sparringSceneFadeIn.Finished += menu_Closed;
                AddMenu(sparringSceneFadeIn);
            }
        }

        private void SparringConfirmMenu_Canceled(object sender, EventArgs e)
        {
            menu_Closed(sender, e);
            var sparringWindow = (Menus.Peek() as Window_Sparring);
            sparringWindow.CancelUnitSelecting();
        }
        #endregion

        private void SupportMenu_UnitSelected(object sender, EventArgs e)
        {
            var supportMenu = (sender as Window_Base_Support);
            var commandWindow = supportMenu.GetCommandWindow();

            var supportPartnerMenu = new SupportCommandMenu(commandWindow);
            supportPartnerMenu.Selected += SupportPartnerMenu_Selected;
            supportPartnerMenu.Canceled += menu_Closed;
            AddMenu(supportPartnerMenu);
        }

        private void SupportPartnerMenu_Selected(object sender, EventArgs e)
        {
            var supportPartnerMenu = (sender as SupportCommandMenu);
            var supportMenu = (Menus.ElementAt(1) as Window_Base_Support);
            int targetId = supportPartnerMenu.TargetId;

            if (supportMenu.TrySelectPartner(targetId))
            {
                Global.game_system.play_se(System_Sounds.Confirm);

                var supportConfirmWindow = new Preparations_Confirm_Window();
                supportConfirmWindow.set_text(string.Format("Speak to {0}?",
                    Global.game_actors[targetId].name));
                supportConfirmWindow.add_choice("Yes", new Vector2(16, 12));
                supportConfirmWindow.add_choice("No", new Vector2(64, 12));
                supportConfirmWindow.size = new Vector2(112, 40);
                supportConfirmWindow.loc = new Vector2(32, 24);
                supportConfirmWindow.index = 1;

                var supportConfirmMenu = new ConfirmationMenu(supportConfirmWindow);
                supportConfirmMenu.Confirmed += SupportConfirmMenu_Confirmed;
                supportConfirmMenu.Canceled += menu_Closed;
                AddMenu(supportConfirmMenu);
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        private void SupportConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var supportConfirmMenu = (sender as ConfirmationMenu);
            var supportMenu = (Menus.ElementAt(2) as Window_Base_Support);

            supportMenu.AcceptSupport();

            var supportSceneFadeIn = supportMenu.SupportFadeIn();
            supportSceneFadeIn.Finished += menu_Closed;
            AddMenu(supportSceneFadeIn);
        }

        void supportMenu_Support(object sender, EventArgs e)
        {
            var supportWindow = (sender as Window_Base_Support);
            var supportPartnerMenu = (Menus.ElementAt(2) as SupportCommandMenu);
            int targetId = supportPartnerMenu.TargetId;

            menu_Closed(Menus.Peek(), e); // Screen Fade
            menu_Closed(Menus.Peek(), e); // Confirmation Menu
            menu_Closed(Menus.Peek(), e); // Command Menu

            MenuHandler.HomeBaseSupport(
                supportWindow.ActorId, targetId);
        }

        private void SupportMenu_SupportEnded(object sender, EventArgs e)
        {
            var supportMenu = (sender as Window_Base_Support);

            var supportSceneFadeIn = supportMenu.SupportFadeOut();
            supportSceneFadeIn.Finished += menu_Closed;
            AddMenu(supportSceneFadeIn);
        }
        #endregion

        #region Talk
        void talkMenu_Selected(object sender, EventArgs e)
        {
            var talkMenu = (Menus.Peek() as BaseConvoMenu);
            if (talkMenu.SelectEvent())
                Global.game_system.play_se(System_Sounds.Confirm);
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        void talkMenu_StartEvent(object sender, EventArgs e)
        {
            var talkMenu = (Menus.Peek() as BaseConvoMenu);
            Global.game_state.activate_base_event(talkMenu.Index);
        }

        void talkMenu_EventStopped(object sender, EventArgs e)
        {
            (Menus.ElementAt(1) as Window_Home_Base).refresh_talk_ready();
        }
        #endregion
    }

    interface IHomeBaseMenuHandler : ISetupMenuHandler
    {
        //Sparring
        void HomeBaseTraining(int healerId, int battlerId1, int battlerId2);
        void HomeBaseSupport(int actorId1, int actorId2);
        void HomeBaseLeave();
    }
}
