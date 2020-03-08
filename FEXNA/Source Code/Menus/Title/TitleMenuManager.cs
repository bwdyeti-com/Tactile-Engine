using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Menus.Title
{
    class TitleMenuManager : InterfaceHandledMenuManager<ITitleMenuHandler>
    {
        private TitleMenuManager(ITitleMenuHandler handler)
            : base(handler) { }

        public bool SoftResetBlocked
        {
            get { return Menus.Peek() is Windows.Command.Window_Config_Options; }
        }
        public bool FullscreenSwitchBlocked
        {
            get { return Menus.Peek() is Windows.Command.Window_Config_Options; }
        }

        public static TitleMenuManager Intro(ITitleMenuHandler handler)
        {
            var menuManager = new TitleMenuManager(handler);
            var titleIntroMenu = new TitleIntroMenu();
            titleIntroMenu.Closed += menuManager.titleIntroMenu_Closed;
            menuManager.AddMenu(titleIntroMenu);
            return menuManager;
        }
        public static TitleMenuManager TitleScreen(ITitleMenuHandler handler)
        {
            var menuManager = new TitleMenuManager(handler);
            menuManager.AddTitleScreenMenu();
            return menuManager;
        }
        public static TitleMenuManager MainMenu(ITitleMenuHandler handler)
        {
            var menuManager = new TitleMenuManager(handler);
            menuManager.AddTitleScreenMenu();
            menuManager.AddMainMenu(true);
            return menuManager;
        }

        void titleIntroMenu_Closed(object sender, EventArgs e)
        {
            RemoveTopMenu();

            AddTitleScreenMenu();

            Global.Audio.PlayBgm(Constants.Audio.Bgm.TITLE_THEME);
        }

        #region Title Screen
        private void AddTitleScreenMenu()
        {
            var titleMenu = new TitleScreenMenu();
            titleMenu.PressedStart += titleMenu_PressedStart;
            titleMenu.ClassReel += titleMenu_ClassReel;
            AddMenu(titleMenu);
        }

        void titleMenu_PressedStart(object sender, EventArgs e)
        {
            AddMainMenu();
        }

        void titleMenu_ClassReel(object sender, EventArgs e)
        {
            Menus.Clear();
            MenuHandler.TitleClassReel();
        }
        #endregion

        #region Main Menu
        private void AddMainMenu(bool skipFadeIn = false)
        {
            var mainMenu = new Window_Title_Main_Menu();
            mainMenu.Opened += mainMenu_Opened;
            mainMenu.Selected += mainMenu_Selected;
            mainMenu.Canceled += mainMenu_Canceled;
            mainMenu.Closed += menu_Closed;
            AddMenu(mainMenu);

            var mainMenuFadeIn = mainMenu.FadeInMenu(skipFadeIn);
            mainMenuFadeIn.Finished += menu_Closed;
            AddMenu(mainMenuFadeIn);
        }

        void mainMenu_Opened(object sender, EventArgs e)
        {
            var titleMenu = (Menus.ElementAt(1) as TitleScreenMenu);
            titleMenu.HideStart();

            CheckMetrics();
        }

        #region First Run Metrics
        private void CheckMetrics()
        {
            if (Global.metrics_allowed &&
                Global.gameSettings.General.Metrics == Metrics_Settings.Not_Set)
            {
                // Show first run metrics setting confirmation window
                var metricsConfirmWindow = new Parchment_Confirm_Window();
                metricsConfirmWindow.set_text(
@"Would you like to provide
anonymous usage data to
the developers for use in
improving this game?");
                metricsConfirmWindow.add_choice("Yes", new Vector2(32, 64));
                metricsConfirmWindow.add_choice("No", new Vector2(80, 64));
                metricsConfirmWindow.size = new Vector2(136, 96);
                metricsConfirmWindow.loc = (new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) - metricsConfirmWindow.size) / 2;
                metricsConfirmWindow.index = 0;
                metricsConfirmWindow.stereoscopic = Config.TITLE_CHOICE_DEPTH - 1;

                var metricsConfirmMenu = new ConfirmationMenu(metricsConfirmWindow);
                metricsConfirmMenu.Confirmed += metricsConfirmMenu_Confirmed;
                metricsConfirmMenu.Canceled += metricsConfirmMenu_Canceled;
                AddMenu(metricsConfirmMenu);
            }
        }

        void metricsConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            menu_Closed(sender, e);
            Global.gameSettings.General.ConfirmSetting(
                FEXNA.Options.GeneralSetting.Metrics, 0, true);
            MenuHandler.TitleSaveConfig();
            AddMetricsSetMenu();
        }

        void metricsConfirmMenu_Canceled(object sender, EventArgs e)
        {
            menu_Closed(sender, e);
            Global.gameSettings.General.ConfirmSetting(
                FEXNA.Options.GeneralSetting.Metrics, 0, false);
            MenuHandler.TitleSaveConfig();
            AddMetricsSetMenu();
        }

        private void AddMetricsSetMenu()
        {
            var metricsSetWindow = new Parchment_Info_Window();
            if (Global.gameSettings.General.Metrics == Metrics_Settings.On)
                metricsSetWindow.set_text(
@"Thank you for participating. Metrics
collection can be turned on or off
at any time from the options menu.");
            else
                metricsSetWindow.set_text(
@"If you change your mind, metrics
collection can be turned on or off
at any time from the options menu.");
            metricsSetWindow.size = new Vector2(184, 64);
            metricsSetWindow.loc = (new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) - metricsSetWindow.size) / 2;
            metricsSetWindow.stereoscopic = Config.TITLE_CHOICE_DEPTH - 1;

            var metricsSetMenu = new ConfirmationMenu(metricsSetWindow);
            metricsSetMenu.Confirmed += metricsSetMenu_Close;
            AddMenu(metricsSetMenu);
        }

        void metricsSetMenu_Close(object sender, EventArgs e)
        {
            RemoveTopMenu();

            Global.game_system.play_se(System_Sounds.Confirm);
        }
        #endregion

        void mainMenu_Canceled(object sender, EventArgs e)
        {
            var mainMenu = sender as Window_Title_Main_Menu;

            var mainMenuFadeOut = mainMenu.FadeOutMenu();
            mainMenuFadeOut.Finished += menu_Closed;
            AddMenu(mainMenuFadeOut);
        }

        void mainMenu_Selected(object sender, EventArgs e)
        {
            var mainMenu = sender as Window_Title_Main_Menu;

            switch (mainMenu.SelectedOption)
            {
                case Main_Menu_Selections.Resume:
                    MenuHandler.TitleResume();
                    break;
                case Main_Menu_Selections.Start_Game:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    mainMenu.HideMenus();
                    AddStartGameMenu();
                    break;
                case Main_Menu_Selections.Options:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    var optionsMenu = new ConfigOptionsMenu();
                    optionsMenu.Stereoscopic = Config.TITLE_OPTIONS_DEPTH;
                    optionsMenu.Canceled += optionsMenu_Canceled;
                    optionsMenu.Closed += menu_Closed;
                    AddMenu(optionsMenu);
                    break;
#if !MONOGAME && DEBUG
                case Main_Menu_Selections.Test_Battle:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Global.game_state.reset();
                    Global.game_map = new Game_Map();
                    Global.game_temp = new Game_Temp();
                    Global.game_battalions = new Game_Battalions();
                    Global.game_actors = new Game_Actors();
                    var testBattleMenu = new Windows.Map.Window_Test_Battle_Setup();
                    testBattleMenu.Confirm += testBattleMenu_Confirm;
                    testBattleMenu.Closed += menu_Closed;
                    AddMenu(testBattleMenu);
                    break;
#endif
                case Main_Menu_Selections.Extras:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    mainMenu.HideMenus();

                    Global.game_state.reset();
                    Global.game_map = new Game_Map();
                    Global.game_temp = new Game_Temp();
                    Global.game_battalions = new Game_Battalions();
                    Global.game_actors = new Game_Actors();
                    var extrasMenu = new ExtrasMenu();
                    extrasMenu.Selected += ExtrasMenu_Selected;
                    extrasMenu.Closed += menu_Closed;
                    extrasMenu.AddToManager(new MenuCallbackEventArgs(this.AddMenu, this.menu_Closed));
                    break;
                case Main_Menu_Selections.Quit:
                    string caption = "Are you sure you\nwant to quit?";

                    var quitWindow = new Window_Confirmation();
                    quitWindow.set_text(caption);
                    quitWindow.add_choice("Yes", new Vector2(16, 32));
                    quitWindow.add_choice("No", new Vector2(56, 32));
                    int text_width = Font_Data.text_width(caption, "FE7_Convo");
                    text_width = text_width + 16 + (text_width % 8 == 0 ? 0 : (8 - text_width % 8));
                    quitWindow.size = new Vector2(Math.Max(88, text_width), 64);
                    quitWindow.loc = (new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) - quitWindow.size) / 2;
                    quitWindow.stereoscopic = Config.TITLE_CHOICE_DEPTH;

                    var quitMenu = new ConfirmationMenu(quitWindow);
                    quitMenu.Confirmed += quitMenu_Confirmed;
                    quitMenu.Canceled += menu_Closed;
                    AddMenu(quitMenu);
                    break;
            }
        }
        
#if !MONOGAME && DEBUG
        void testBattleMenu_Confirm(object sender, EventArgs e)
        {
            var testBattleMenu = sender as Windows.Map.Window_Test_Battle_Setup;
            Menus.Clear();
            MenuHandler.TitleTestBattle(testBattleMenu.distance);
        }
#endif

        void optionsMenu_Canceled(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Cancel);
            MenuHandler.TitleSaveConfig();
        }

        private void ExtrasMenu_Selected(object sender, EventArgs e)
        {
            var extrasMenu = sender as ExtrasMenu;

            switch ((ExtrasSelections)extrasMenu.Index)
            {
                case ExtrasSelections.SupportViewer:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    var supportsMenu = new SupportViewerMenu();
                    supportsMenu.Selected += SupportsMenu_Selected;
                    supportsMenu.Closed += menu_Closed;
                    supportsMenu.AddToManager(new MenuCallbackEventArgs(this.AddMenu, this.menu_Closed));
                    break;
                case ExtrasSelections.Credits:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    var creditsMenu = new CreditsMenu();
                    creditsMenu.OpenFullCredits += CreditsMenu_OpenFullCredits;
                    creditsMenu.Closed += menu_Closed;
                    creditsMenu.AddToManager(new MenuCallbackEventArgs(this.AddMenu, this.menu_Closed));
                    break;
            }
        }

        private void SupportsMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var supportsMenu = sender as SupportViewerMenu;

            var supportActorMenu = new SupportViewerActorMenu(supportsMenu.ActorId, supportsMenu);
            supportActorMenu.SetFieldBase(supportsMenu.IsAtBase);
            supportActorMenu.Selected += SupportActorMenu_Selected;
            supportActorMenu.FieldBaseSwitched += SupportActorMenu_FieldBaseSwitched;
            supportActorMenu.Closed += menu_Closed;
            supportActorMenu.AddToManager(new MenuCallbackEventArgs(this.AddMenu, this.menu_Closed));
        }

        private void SupportActorMenu_FieldBaseSwitched(object sender, EventArgs e)
        {
            var supportActorMenu = sender as SupportViewerActorMenu;
            var supportsMenu = (Menus.ElementAt(1) as SupportViewerMenu);

            Global.game_system.play_se(System_Sounds.Status_Page_Change);
            supportsMenu.SwitchAtBase();
            supportActorMenu.SetFieldBase(supportsMenu.IsAtBase);
        }

        private void SupportActorMenu_Selected(object sender, EventArgs e)
        {
            var supportActorMenu = sender as SupportViewerActorMenu;
            var supportsMenu = (Menus.ElementAt(1) as SupportViewerMenu);

            string key;
            int level;
            if (supportActorMenu.GetSupportConvo(out key, out level))
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                bool atBase = supportsMenu.IsAtBase;
                string background = supportsMenu.ConvoBackground(atBase);
                MenuHandler.TitleSupportConvo(key, level, atBase, background);
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }
        
        private void CreditsMenu_OpenFullCredits(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            MenuHandler.TitleOpenFullCredits();
        }
        void quitMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            MenuHandler.TitleQuit();
        }

        protected void menu_Closed(object sender, EventArgs e)
        {
            RemoveTopMenu();
        }
        #endregion

        #region Start Game
        private void AddStartGameMenu()
        {
            var startGameMenu =
                new Window_Title_Start_Game(Global.latest_save_id);
            startGameMenu.Selected += startGameMenu_Selected;
            startGameMenu.MoveFile += startGameMenu_MoveFile;
            startGameMenu.CopyFile += startGameMenu_CopyFile;
            startGameMenu.Canceled += startGameMenu_Canceled;
            AddMenu(startGameMenu);
        }

        void startGameMenu_Canceled(object sender, EventArgs e)
        {
            menu_Closed(sender, e);
            var mainMenu = Menus.Peek() as Window_Title_Main_Menu;
            mainMenu.RefreshMenuChoices();
        }

        void startGameMenu_Selected(object sender, EventArgs e)
        {
            var startGameMenu = sender as Window_Title_Start_Game;

            Global.game_system.play_se(System_Sounds.Confirm);
            // New Game
            if (Global.save_files_info == null ||
                !Global.save_files_info.ContainsKey(startGameMenu.file_id))
            {
                startGameMenu.HideMenus();
                var styleSelectionMenu = new StyleSelectionMenu();
                styleSelectionMenu.Selected += styleSelectionMenu_Selected;
                styleSelectionMenu.Canceled += menu_Closed;
                AddMenu(styleSelectionMenu);
            }
            // Existing File
            else
            {
                Vector2 loc = startGameMenu.SelectedOptionLoc;
                var fileCommandMenu =
                    new FileSelectedCommandMenu(loc, startGameMenu.file_id);
                fileCommandMenu.Selected += fileCommandMenu_Selected;
                fileCommandMenu.Canceled += menu_Closed;
                AddMenu(fileCommandMenu);
            }
        }

        void styleSelectionMenu_Selected(object sender, EventArgs e)
        {
            var styleSelectionMenu = sender as StyleSelectionMenu;
            var startGameMenu = (Menus.ElementAt(1) as Window_Title_Start_Game);
            styleSelectionMenu.HideMenus();
            var difficultySelectionMenu = new DifficultySelectionMenu();
            difficultySelectionMenu.Selected += difficultySelectionMenu_Selected;
            difficultySelectionMenu.Canceled += menu_Closed;
            AddMenu(difficultySelectionMenu);
        }

        void difficultySelectionMenu_Selected(object sender, EventArgs e)
        {
            var difficultySelectionMenu = sender as DifficultySelectionMenu;
            var styleSelectionMenu = (Menus.ElementAt(1) as StyleSelectionMenu);
            var startGameMenu = (Menus.ElementAt(2) as Window_Title_Start_Game);

            MenuHandler.TitleNewGame(
                startGameMenu.file_id,
                styleSelectionMenu.SelectedStyle,
                difficultySelectionMenu.SelectedDifficulty);
        }

        void startGameMenu_MoveFile(object sender, EventArgs e)
        {
            var startGameMenu = sender as Window_Title_Start_Game;

            if (Global.save_files_info.ContainsKey(startGameMenu.file_id))
                Global.game_system.play_se(System_Sounds.Buzzer);
            else
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Global.start_game_file_id = startGameMenu.move_file_id;
                Global.move_file = true;
                Global.move_to_file_id = startGameMenu.file_id;
                startGameMenu.moving_file = false;

                startGameMenu.waiting_for_io = true;
            }
        }

        void startGameMenu_CopyFile(object sender, EventArgs e)
        {
            var startGameMenu = sender as Window_Title_Start_Game;

            if (Global.save_files_info.ContainsKey(startGameMenu.file_id))
                Global.game_system.play_se(System_Sounds.Buzzer);
            else
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Global.start_game_file_id = startGameMenu.move_file_id;
                Global.copying = true;
                Global.move_to_file_id = startGameMenu.file_id;
                startGameMenu.copying = false;

                startGameMenu.waiting_for_io = true;
            }
        }

        void fileCommandMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var fileCommandMenu = sender as FileSelectedCommandMenu;
            var startGameMenu = (Menus.ElementAt(1) as Window_Title_Start_Game);
            switch (fileCommandMenu.SelectedOption)
            {
                case Start_Game_Options.World_Map:
                    MenuHandler.TitleStartGame(startGameMenu.file_id);
                    break;
                case Start_Game_Options.Load_Suspend:
                    startGameMenu.preview_suspend();
                    var suspendConfirmWindow = FileConfirmWindow(
                        fileCommandMenu,
                        startGameMenu,
                        "Load suspend?");

                    var suspendConfirmMenu = new ConfirmationMenu(suspendConfirmWindow);
                    suspendConfirmMenu.Confirmed += suspendConfirmMenu_Confirmed;
                    suspendConfirmMenu.Canceled += suspendConfirmMenu_Canceled;
                    AddMenu(suspendConfirmMenu);
                    break;
                case Start_Game_Options.Load_Map_Save:
                    startGameMenu.preview_checkpoint();
                    var checkpointConfirmWindow = FileConfirmWindow(
                        fileCommandMenu,
                        startGameMenu,
                        "Load checkpoint?");

                    var checkpointConfirmMenu = new ConfirmationMenu(checkpointConfirmWindow);
                    checkpointConfirmMenu.Confirmed += checkpointConfirmMenu_Confirmed;
                    checkpointConfirmMenu.Canceled += suspendConfirmMenu_Canceled;
                    AddMenu(checkpointConfirmMenu);
                    break;
                case Start_Game_Options.Move:
                    menu_Closed(sender, e);
                    startGameMenu.moving_file = true;
                    break;
                case Start_Game_Options.Copy:
                    menu_Closed(sender, e);
                    startGameMenu.copying = true;
                    break;
                case Start_Game_Options.Delete:
                    var deleteConfirmWindow = FileConfirmWindow(
                        fileCommandMenu,
                        startGameMenu,
                        "Are you sure?");

                    var deleteConfirmMenu = new ConfirmationMenu(deleteConfirmWindow);
                    deleteConfirmMenu.Confirmed += deleteConfirmMenu_Confirmed;
                    deleteConfirmMenu.Canceled += menu_Closed;
                    AddMenu(deleteConfirmMenu);
                    break;
            }
        }

        private Window_Confirmation FileConfirmWindow(
            FileSelectedCommandMenu fileCommandMenu,
            Window_Title_Start_Game startGameMenu,
            string caption)
        {
            var confirmWindow = new Window_Confirmation();
            int height = 64;
            Vector2 loc = fileCommandMenu.SelectedOptionLoc + new Vector2(-8, 0);
            if (loc.Y > Config.WINDOW_HEIGHT - height)
                loc.Y -= height;
            confirmWindow.loc = loc;
            confirmWindow.set_text(caption);
            confirmWindow.add_choice("Yes", new Vector2(16, 16));
            confirmWindow.add_choice("No", new Vector2(56, 16));
            int text_width = Font_Data.text_width(caption, "FE7_Convo");
            text_width = text_width + 16 +
                (text_width % 8 == 0 ? 0 : (8 - text_width % 8));
            confirmWindow.size = new Vector2(Math.Max(88, text_width), 48);
            confirmWindow.index = 1;
            confirmWindow.stereoscopic = Config.TITLE_CHOICE_DEPTH;

            return confirmWindow;
        }

        void suspendConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            var startGameMenu = (Menus.ElementAt(2) as Window_Title_Start_Game);
            MenuHandler.TitleLoadSuspend(startGameMenu.file_id);
        }

        void checkpointConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            var startGameMenu = (Menus.ElementAt(2) as Window_Title_Start_Game);
            MenuHandler.TitleLoadCheckpoint(startGameMenu.file_id);
        }

        void suspendConfirmMenu_Canceled(object sender, EventArgs e)
        {
            menu_Closed(sender, e);
            var startGameMenu = (Menus.ElementAt(1) as Window_Title_Start_Game);
            startGameMenu.close_preview();
        }

        void deleteConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            menu_Closed(sender, e);
            menu_Closed(Menus.Peek(), e);

            var startGameMenu = Menus.Peek() as Window_Title_Start_Game;

            Global.game_system.play_se(System_Sounds.Confirm);
            Global.start_game_file_id = startGameMenu.file_id;
            Global.delete_file = true;

            startGameMenu.waiting_for_io = true;
        }
        #endregion
    }

    interface ITitleMenuHandler : IMenuHandler
    {
        void TitleClassReel();
        void TitleResume();
        void TitleNewGame(int fileId, Mode_Styles style, Difficulty_Modes difficulty);
        void TitleStartGame(int fileId);
        void TitleLoadSuspend(int fileId);
        void TitleLoadCheckpoint(int fileId);
        void TitleSaveConfig();
#if !MONOGAME && DEBUG
        void TitleTestBattle(int distance);
#endif
        void TitleSupportConvo(string supportKey, int level, bool atBase, string background);
        void TitleOpenFullCredits();
        void TitleQuit();
    }
}
