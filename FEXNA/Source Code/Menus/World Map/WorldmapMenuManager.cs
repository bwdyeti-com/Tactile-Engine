using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Menus.Map;
using FEXNA.Menus.Preparations;
using FEXNA.Windows;
using FEXNA.Windows.Command;
using FEXNA.Windows.Map;
using FEXNA.Windows.Map.Items;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Menus.Worldmap
{
    class WorldmapMenuManager : SetupMenuManager<IWorldmapMenuHandler>, IWorldmapHandler
    {
        protected WorldmapMenuData MenuData;

        public WorldmapMenuManager(
            IWorldmapMenuHandler handler, WorldmapMenuData menuData)
                : base(handler)
        {
            MenuData = menuData;

            AddWorldmapMenu();
        }

        protected virtual void AddWorldmapMenu()
        {
            var worldmapMenu = new WorldmapMenu(MenuData, this);
            worldmapMenu.ChapterSelected += worldmapMenu_ChapterSelected;
            worldmapMenu.Canceled += worldmapMenu_Canceled;
            worldmapMenu.Refreshing += worldmapMenu_Refreshing;
            worldmapMenu.ChapterCommandSelected += worldmapMenu_ChapterCommandSelected;
            AddMenu(worldmapMenu);

            MenuHandler.WorldmapChapterChanged(MenuData.Chapter);
        }

        #region World Map Menu
        // Chapter selected
        void worldmapMenu_ChapterSelected(object sender, EventArgs e)
        {
            var worldmapMenu = sender as WorldmapMenu;

            // Select chapter
            if (Constants.WorldMap.HARD_MODE_BLOCKED.Contains(
                MenuData.ChapterId) &&
                Global.game_system.hard_mode)
            {
                Global.game_system.Difficulty_Mode = Difficulty_Modes.Normal;
                worldmapMenu.RefreshDataPanel();

                if (Global.game_system.Style != Mode_Styles.Classic)
                {
                    Global.game_system.play_se(System_Sounds.Cancel);

                    var hardModeBlockWindow = new Parchment_Info_Window();
                    hardModeBlockWindow.set_text(@"This chapter does not yet have
hard mode data, and must be
loaded in normal mode. Sorry!");
                    hardModeBlockWindow.size = new Vector2(160, 64);
                    hardModeBlockWindow.loc =
                        new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2 -
                        hardModeBlockWindow.size / 2;

                    var hardModeBlockedMenu = new ConfirmationMenu(hardModeBlockWindow);
                    hardModeBlockedMenu.Confirmed += hardModeBlockedMenu_Close;
                    AddMenu(hardModeBlockedMenu);

                    return;
                }
            }

            if (MenuData.IsSkippingGaiden(MenuData.ChapterId))
            {
                Global.game_system.play_se(System_Sounds.Confirm);

                var gaidenSkippedWindow = new Parchment_Info_Window();
                gaidenSkippedWindow.set_text(@"Proceeding with this chapter
will skip available sidequests
or alternate routes.");
                gaidenSkippedWindow.size = new Vector2(152, 64);
                gaidenSkippedWindow.loc =
                    new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2 -
                    gaidenSkippedWindow.size / 2;

                var gaidenSkippedMenu = new ConfirmationMenu(gaidenSkippedWindow);
                gaidenSkippedMenu.Confirmed += gaidenSkippedMenu_Close;
                AddMenu(gaidenSkippedMenu);

                return;
            }

            Global.game_system.play_se(System_Sounds.Confirm);
            SelectChapter(worldmapMenu);
        }

        private void SelectChapter(WorldmapMenu worldmapMenu)
        {
            if (worldmapMenu.PreviousChapterSelectionIncomplete())
            {
                // A lone prior requirement of a chapter from
                // another battalion can make this crash //@Yeti
                AddPreviousChapterSelectionMenu(worldmapMenu);
            }
            else
            {
                worldmapMenu.SelectChapter();
            }
        }

        #region Previous Chapter Selection
        private void AddPreviousChapterSelectionMenu(WorldmapMenu worldmapMenu)
        {
            worldmapMenu.StorePreviousChapters();

            // Open previous chapter selection
            var previousChapterSelectionWindow =
                new PreviousChapterSelectionMenu(
                    new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2,
                    MenuData.ChapterId,
                    MenuData,
                    worldmapMenu);
            previousChapterSelectionWindow.activate(worldmapMenu.CursorLoc);
            previousChapterSelectionWindow.Selected += previousChapterSelectionWindow_Selected;
            previousChapterSelectionWindow.PreviousChapterChanged += previousChapterSelectionWindow_PreviousChapterChanged;
            previousChapterSelectionWindow.Canceled += previousChapterSelectionWindow_Canceled;
            AddMenu(previousChapterSelectionWindow);
        }

        void previousChapterSelectionWindow_Selected(object sender, EventArgs e)
        {
            var previousChapterSelectionWindow = sender as PreviousChapterSelectionMenu;

            RemoveTopMenu();

            var worldmapMenu = Menus.Peek() as WorldmapMenu;
            worldmapMenu.RefreshPreviousChapters(
                previousChapterSelectionWindow.previous_chapter_indices);
            if (worldmapMenu.SelectingChapter)
                worldmapMenu.SelectChapter();
        }

        void previousChapterSelectionWindow_PreviousChapterChanged(object sender, EventArgs e)
        {
            var previousChapterSelectionWindow = sender as PreviousChapterSelectionMenu;
            var worldmapMenu = Menus.ElementAt(1) as WorldmapMenu;

            worldmapMenu.RefreshPreviousChapters(
                previousChapterSelectionWindow.previous_chapter_indices);
        }

        void previousChapterSelectionWindow_Canceled(object sender, EventArgs e)
        {
            menu_Closed(sender, e);

            var worldmapMenu = Menus.Peek() as WorldmapMenu;
            if (!worldmapMenu.SelectingChapter)
            {
                worldmapMenu.RestorePreviousChapters();
            }
        }
        #endregion

        void hardModeBlockedMenu_Close(object sender, EventArgs e)
        {
            RemoveTopMenu();

            Global.game_system.play_se(System_Sounds.Confirm);
        }

        void gaidenSkippedMenu_Close(object sender, EventArgs e)
        {
            RemoveTopMenu();

            Global.game_system.play_se(System_Sounds.Confirm);
            var worldmapMenu = (Menus.Peek() as WorldmapMenu);
            SelectChapter(worldmapMenu);
        }

        void worldmapMenu_Canceled(object sender, EventArgs e)
        {
            var leaveConfirmWindow = new Parchment_Confirm_Window();
            leaveConfirmWindow.set_text("Return to title?");
            leaveConfirmWindow.add_choice("Yes", new Vector2(16, 16));
            leaveConfirmWindow.add_choice("No", new Vector2(56, 16));
            leaveConfirmWindow.index = 1;
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
            MenuHandler.WorldmapExit();
        }

        // Refreshing
        void worldmapMenu_Refreshing(object sender, EventArgs e)
        {
            var worldmapMenu = sender as WorldmapMenu;
            MenuHandler.WorldmapChapterChanged(MenuData.Chapter);
        }

        // Chapter option selected
        void worldmapMenu_ChapterCommandSelected(object sender, EventArgs e)
        {
            var worldmapMenu = sender as WorldmapMenu;
            switch (worldmapMenu.SelectedChapterCommand())
            {
                case ChapterCommands.StartChapter:
                    // Open suspend deletion confirm menu, or start chapter
                    if (worldmapMenu.PreviousChapterSelectionIncomplete())
                    {
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    }
                    else
                    {
                        // If a map save or suspend already exists
#if DEBUG
                        if (Global.save_files_info != null &&
                            (Global.current_save_info.map_save_exists ||
                            Global.current_save_info.suspend_exists))
#else
                        if (Global.current_save_info.map_save_exists ||
                            Global.current_save_info.suspend_exists)
#endif
                        {
                            var deleteSuspendWindow = new Parchment_Confirm_Window();
                            deleteSuspendWindow.loc = new Vector2(Config.WINDOW_WIDTH - 152, Config.WINDOW_HEIGHT - 64) / 2;
                            deleteSuspendWindow.set_text("Temporary saves for this file\nwill be deleted. Proceed?");
                            deleteSuspendWindow.add_choice("Yes", new Vector2(32, 32));
                            deleteSuspendWindow.add_choice("No", new Vector2(88, 32));
                            deleteSuspendWindow.size = new Vector2(152, 64);
                            deleteSuspendWindow.index = 1;

                            var deleteSuspendMenu = new ConfirmationMenu(deleteSuspendWindow);
                            deleteSuspendMenu.Confirmed += deleteSuspendMenu_Confirmed;
                            deleteSuspendMenu.Canceled += menu_Closed;
                            AddMenu(deleteSuspendMenu);
                        }
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            MenuHandler.WorldmapStartChapter();
                        }
                    }
                    break;
                case ChapterCommands.SelectPrevious:
                    // Open select previous chapter menu
                    Global.game_system.play_se(System_Sounds.Confirm);
                    AddPreviousChapterSelectionMenu(worldmapMenu);
                    break;
                case ChapterCommands.Options:
                    // Open options menu
                    Global.game_system.play_se(System_Sounds.Confirm);
                    AddOptionsMenu(false);
                    break;
                case ChapterCommands.Unit:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Global.game_map = new Game_Map();
                    // Open unit menu
                    var unitMenu = new Window_Unit();
                    unitMenu.Status += unitMenu_Status;
                    unitMenu.Closing += unitMenu_Closing;
                    unitMenu.Closed += unitMenu_Closed;
                    AddMenu(unitMenu);
                    break;
                case ChapterCommands.Manage:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Global.game_state.reset();
                    Global.game_map = new Game_Map();
                    // Open item management menu
                    AddItemManageMenu();
                    break;
                case ChapterCommands.Ranking:
                    // Open ranking menu
                    Global.game_system.play_se(System_Sounds.Confirm);
                    var rankingsMenu = new WindowRankingsOverview(
                        MenuData.ChapterId,
                        Global.game_system.Difficulty_Mode);
                    rankingsMenu.Closed += menu_Closed;
                    AddMenu(rankingsMenu);
                    break;
            }
        }
        #endregion

        #region Chapter Command Menus
        void deleteSuspendMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            RemoveTopMenu();
            Global.delete_map_save = true;
            MenuHandler.WorldmapStartChapter();
        }

        void unitMenu_Closed(object sender, EventArgs e)
        {
            menu_Closed(sender, e);
            Global.game_map = null;
        }

        protected override void optionsMenu_Closed(object sender, EventArgs e)
        {
            Global.game_temp.menuing = false;
            menu_Closed(sender, e);

            MenuHandler.SetupSave();
        }

        private void AddItemManageMenu()
        {
            var itemManageMenu = new Window_Manage_Items();
            itemManageMenu.UnitSelected += ItemsMenu_UnitSelected;
            itemManageMenu.Status += preparationsMenu_Status;
            itemManageMenu.TradeSelected += ItemsMenu_TradeSelected;
            itemManageMenu.Trade += itemsMenu_Trade;
            itemManageMenu.Convoy += itemsMenu_Convoy;
            itemManageMenu.List += itemsMenu_List;
            itemManageMenu.Closed += itemManageMenu_Closed;
            AddMenu(itemManageMenu);
        }

        void itemManageMenu_Closed(object sender, EventArgs e)
        {
            menu_Closed(sender, e);
            Global.game_map = null;
            MenuHandler.SetupSave();
        }
        #endregion

        #region IWorldmapHandler
        public void LoadData(string chapterId, Dictionary<string, string> previousChapters)
        {
            MenuHandler.WorldmapLoadData(chapterId, previousChapters);
        }
        #endregion
    }

    interface IWorldmapMenuHandler : ISetupMenuHandler
    {
        void WorldmapStartChapter();
        void WorldmapChapterChanged(FEXNA_Library.Data_Chapter chapter);
        void WorldmapLoadData(string chapterId, Dictionary<string, string> previousChapters);
        void WorldmapExit();
    }
}
