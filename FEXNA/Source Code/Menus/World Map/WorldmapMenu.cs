using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Windows.Command;
using FEXNA_Library;

namespace FEXNA.Menus.Worldmap
{
    class WorldmapMenu : BaseMenu, IHasCancelButton
    {
        protected const int WIDTH = 120;

        protected WorldmapMenuData MenuData;
        private IWorldmapHandler MenuHandler;

        protected Window_WorldMap_Data DataWindow;
        protected int ModeSwitchTimer = 0;
        private bool UnitWindowAvailable, RankingWindowAvailable;
        private Dictionary<string, int> StoredPreviousChapterIndices;

        protected Window_Command_Worldmap CommandWindow;
        private Window_Command ChapterCommandWindow;
        private List<ChapterCommands> ActiveChapterCommands;
        private Button_Description CancelButton, DifficultyButton;

        public virtual int Redirect { get { return MenuData.IndexRedirect[CommandWindow.index]; } }

        public bool SelectingChapter { get { return ChapterCommandWindow == null; } }

        public Vector2 CursorLoc
        {
            get
            {
                if (SelectingChapter)
                    return CommandWindow.current_cursor_loc;
                else
                    return ChapterCommandWindow.current_cursor_loc;
            }
        }

        public WorldmapMenu(WorldmapMenuData menuData, IWorldmapHandler menuHandler)
        {
            MenuData = menuData;
            MenuHandler = menuHandler;

            // Command Window
            CreateCommandWindow();
            // Data_Window
            DataWindow = new Window_WorldMap_Data();
            DataWindow.loc = new Vector2(4, 4);

            Refresh();
        }

        protected virtual void CreateCommandWindow()
        {
            List<string> strs = new List<string>();
            foreach (int i in MenuData.IndexRedirect)
                strs.Add(Global.chapter_by_index(i).World_Map_Name);
            CommandWindow = new Window_Command_Worldmap(
                new Vector2(8, 60), WIDTH, 6, strs);
            RefreshRankImages();

            CommandWindow.tint = new Color(224, 224, 224, 224);
            CommandWindow.glow = true;
            CommandWindow.immediate_index =
                MenuData.IndexRedirect.IndexOf(MenuData.Index);
            CommandWindow.refresh_scroll();
        }

        protected virtual void RefreshRankImages()
        {
            var ranks = new List<Tuple<string, Difficulty_Modes>>();
            foreach (int i in MenuData.IndexRedirect)
            {
                if (Global.save_file != null)
                {
                    string chapter = Global.Chapter_List[i];
                    if (Global.save_file.ContainsKey(chapter))
                        ranks.Add(Tuple.Create(
                            Global.save_file.displayed_rank(chapter),
                            Global.save_file.displayed_difficulty(chapter)));
                    else
                        ranks.Add(Tuple.Create("", Difficulty_Modes.Normal));
                }
                else
                    ranks.Add(Tuple.Create("", Difficulty_Modes.Normal));
            }
            CommandWindow.refresh_ranks(ranks);
        }

        private void RefreshInputHelp()
        {
            CancelButton = Button_Description.button(Inputs.B, 16);
            CancelButton.description = "Cancel";
            //CancelButton.stereoscopic = ; //Yeti


            DifficultyButton = Button_Description.button(Inputs.X, 80);
            DifficultyButton.description = "Change Difficulty";
        }

        private void Refresh()
        {
            OnRefreshing(new EventArgs());

            //Global.game_system.Difficulty_Mode = Global.save_file.Difficulty; //Debug
            // If hard mode is not available for this chapter
            if (!MenuData.HardModeEnabled(this.Redirect))
                Global.game_system.Difficulty_Mode = Difficulty_Modes.Normal;

            RefreshDataPanel();
        }

        internal void RefreshPreviousChapters(Dictionary<string, int> previousChapterIndices)
        {
            MenuData.SetPreviousChapterIndices(previousChapterIndices);
            MenuData.PickDefaultUnselectedPreviousChapters();
            DataWindow.set_mode(Global.game_system.Difficulty_Mode, MenuData.MultipleArcs);
            RefreshData(false);
        }

        internal void StorePreviousChapters()
        {
            StoredPreviousChapterIndices = MenuData.UsablePreviousChapterIndices;
        }

        internal void RestorePreviousChapters()
        {
            RefreshPreviousChapters(StoredPreviousChapterIndices);
        }

        internal virtual void RefreshDataPanel()
        {
            DataWindow.set_mode(Global.game_system.Difficulty_Mode, MenuData.MultipleArcs);
            RefreshData();
        }

        protected virtual void RefreshData(bool resetData = true)
        {
            if (resetData)
                MenuData.RefreshPreviousChapters(MenuData.ChapterId);

            Difficulty_Modes difficulty = Global.game_system != null ?
                Global.game_system.Difficulty_Mode : Difficulty_Modes.Normal;
            if (Global.chapter_by_index(this.Redirect).Standalone ||
                MenuData.ValidPreviousChapters.Count == 0)
            {
                Global.game_system = new Game_System();
                Global.game_battalions = new Game_Battalions();
                Global.game_actors = new Game_Actors();

                DataWindow.set(
                    Global.chapter_by_index(this.Redirect).World_Map_Name,
                    Global.chapter_by_index(this.Redirect).World_Map_Lord_Id,
                    Global.chapter_by_index(this.Redirect).Preset_Data);
            }
            else
            {
                LoadData();
                Global.game_battalions.current_battalion =
                    Global.chapter_by_index(this.Redirect).Battalion;
                Global.game_actors.heal_battalion();
                // Not sure why this happens //Yeti
                // what conflicts are caused by using the loaded system //Yeti
                var system = Global.game_system;
                Global.game_system = system.copy();
                // For now, setting the event data so it shows up in the monitor //Yeti
                Global.game_system.set_event_data(system.SWITCHES, system.VARIABLES);

                DataWindow.set(
                    Global.chapter_by_index(this.Redirect).World_Map_Name,
                    Global.chapter_by_index(this.Redirect).World_Map_Lord_Id,
                    new FEXNA_Library.Preset_Chapter_Data
                    {
                        Lord_Lvl = Global.game_actors[Global.chapter_by_index(this.Redirect).World_Map_Lord_Id].level,
                        Units = Global.chapter_by_index(this.Redirect).Preset_Data.Units + Global.battalion.actors.Count,
                        Gold = Global.chapter_by_index(this.Redirect).Preset_Data.Gold + Global.battalion.gold,
                        Playtime = system.total_play_time
                    });
            }
            Global.game_system.Difficulty_Mode = difficulty;

            UnitWindowAvailable = Global.game_system.Style != Mode_Styles.Classic &&
                Global.battalion != null && Global.battalion.actors.Any();
            // Block ranking window if this chapter is unranked
            RankingWindowAvailable = 
                !Global.data_chapters[MenuData.ChapterId].Unranked &&
                Global.save_file.ContainsKey(MenuData.ChapterId);

            RefreshInputHelp();
            RefreshPreviousChapterSelected();
        }

        private void LoadData()
        {
            LoadData(MenuData.Chapter.Prior_Chapters);
        }
        private void LoadData(List<string> prior_chapters)
        {
            Dictionary<string, string> previous_chapters =
                MenuData.GetSelectedPreviousChapters();
            MenuHandler.LoadData(MenuData.ChapterId, previous_chapters);
        }

        protected virtual bool HardModeEnabled(int index)
        {
            var chapter = Global.chapter_by_index(index);
            if (chapter.Standalone && chapter.Prior_Chapters.Count == 0)
                return true;

            return Global.save_file.chapter_available(Global.Chapter_List[index]);
        }

        public void SelectChapter()
        {
            List<string> commands = new List<string> { "Start Chapter" };
            ActiveChapterCommands = new List<ChapterCommands> { ChapterCommands.StartChapter };
            if (MenuData.ValidPreviousChapters.Any(x => x.Value.Count > 1))
            {
                commands.Add("Load Data");
                ActiveChapterCommands.Add(ChapterCommands.SelectPrevious);
            }
            commands.Add("Options");
            ActiveChapterCommands.Add(ChapterCommands.Options);
            if (UnitWindowAvailable)
            {
                commands.Add("Unit");
                ActiveChapterCommands.Add(ChapterCommands.Unit);
                // Manage screen blocked in Classic
                if (!MenuData.AutoSelectChapter)
                {
                    commands.Add("Manage");
                    ActiveChapterCommands.Add(ChapterCommands.Manage);
                }
            }
            if (RankingWindowAvailable)
            {
                commands.Add("Ranking");
                ActiveChapterCommands.Add(ChapterCommands.Ranking);
            }

            ChapterCommandWindow = new Window_Command(
                CommandWindow.loc + new Vector2(8, 0),
                WIDTH - (16 + 16), commands);
            ChapterCommandWindow.tint = new Color(224, 224, 224, 224);
            ChapterCommandWindow.text_offset = new Vector2(4, 0);
            CommandWindow.active = false;
            CommandWindow.visible = false;
        }

        public ChapterCommands SelectedChapterCommand()
        {
            return ActiveChapterCommands[ChapterCommandWindow.selected_index()];
        }

        private void CloseChapterCommands()
        {
            CommandWindow.active = true;
            CommandWindow.visible = true;
            ChapterCommandWindow = null;
        }

        // Previous Chapter
        internal bool PreviousChapterSelectionIncomplete()
        {
            return MenuData.PreviousChapterSelectionIncomplete();
        }

        private void RefreshPreviousChapterSelected()
        {
            if (ChapterCommandWindow != null)
            {
                if (PreviousChapterSelectionIncomplete())
                {
                    ChapterCommandWindow.set_text_color(0, "Grey");
                    ChapterCommandWindow.set_text_color(1, "Yellow");
                }
                else
                {
                    ChapterCommandWindow.set_text_color(0, "White");
                    if (ActiveChapterCommands.Contains(ChapterCommands.SelectPrevious))
                        ChapterCommandWindow.set_text_color(1, "White");
                }
            }
        }

        // Difficulty
        protected bool CanChangeDifficulty()
        {
            return ModeSwitchTimer <= 0 &&
                Global.save_file.Style != Mode_Styles.Classic &&
                HardModeEnabled(this.Redirect); // Hard mode enabled
        }

        protected bool DifficultyChangeButtonVisible()
        {
            return Global.save_file.Style != Mode_Styles.Classic &&
                HardModeEnabled(this.Redirect);
        }

        protected void SwitchDifficulty(bool increase)
        {
            if (CanChangeDifficulty())
            {
                Global.game_system.play_se(System_Sounds.Status_Page_Change);
                int difficulties = Enum_Values.GetEnumCount(typeof(Difficulty_Modes));
                if (increase)
                    Global.game_system.Difficulty_Mode =
                        (Difficulty_Modes)(((int)Global.game_system.Difficulty_Mode + 1) % difficulties);
                else
                    Global.game_system.Difficulty_Mode =
                        (Difficulty_Modes)(((int)Global.game_system.Difficulty_Mode - 1 + difficulties) % difficulties);

                //@Debug
                // Now that you can switch difficulty freely this actually shouldn't be needed?
                RefreshDataPanel();
                ModeSwitchTimer = Constants.WorldMap.WORLDMAP_MODE_SWITCH_DELAY;
            }
        }

        protected override void Activate()
        {
            CommandWindow.active = true;
            CommandWindow.greyed_cursor = false;
            if (ChapterCommandWindow != null)
            {
                ChapterCommandWindow.active = true;
                ChapterCommandWindow.greyed_cursor = false;
            }
        }
        protected override void Deactivate()
        {
            CommandWindow.active = false;
            CommandWindow.greyed_cursor = true;
            if (ChapterCommandWindow != null)
            {
                ChapterCommandWindow.active = false;
                ChapterCommandWindow.greyed_cursor = true;
            }
        }

        #region IHasCancelButton
        public bool HasCancelButton { get { return CancelButton != null; } }
        public Vector2 CancelButtonLoc { get { return CancelButton.loc; } }
        #endregion

        #region Events
        public event EventHandler<EventArgs> ChapterSelected;
        protected void OnChapterSelected(EventArgs e)
        {
            if (ChapterSelected != null)
                ChapterSelected(this, e);
        }

        public event EventHandler<EventArgs> Refreshing;
        protected void OnRefreshing(EventArgs e)
        {
            if (Refreshing != null)
                Refreshing(this, e);
        }

        public event EventHandler<EventArgs> ChapterCommandSelected;
        protected void OnChapterCommandSelected(EventArgs e)
        {
            if (ChapterCommandSelected != null)
                ChapterCommandSelected(this, e);
        }

        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled(EventArgs e)
        {
            if (Canceled != null)
                Canceled(this, e);
        }
        #endregion

        #region Update
        protected override void UpdateMenu(bool active)
        {
            if (ModeSwitchTimer > 0)
                ModeSwitchTimer--;

            MenuData.Index = this.Redirect;
            CommandWindow.update(active && this.SelectingChapter);
            if (MenuData.Index != this.Redirect)
            {
                MenuData.Index = this.Redirect;
                Refresh();
            }

            if (ChapterCommandWindow != null)
                ChapterCommandWindow.update(active);

            CancelButton.Update(active);
            DifficultyButton.Update(active && CanChangeDifficulty());

            UpdateInput(active);

            DataWindow.update();
        }

        private void UpdateInput(bool active)
        {
            bool cancel =
                CancelButton.consume_trigger(MouseButtons.Left) ||
                CancelButton.consume_trigger(TouchGestures.Tap);

            bool changeDifficulty =
                active && Global.Input.triggered(Inputs.X);
            changeDifficulty |=
                DifficultyButton.consume_trigger(MouseButtons.Left) ||
                DifficultyButton.consume_trigger(TouchGestures.Tap);

            // Selecting chapter
            if (this.SelectingChapter)
            {
                cancel |= CommandWindow.is_canceled();
                bool left = false, right = false;
                if (active && MenuData.MultipleArcs && ModeSwitchTimer <= 0)
                {
                    if (Global.Input.triggered(Inputs.Left) ||
                            Global.Input.gesture_triggered(TouchGestures.SwipeRight))
                        left = true;
                    else if (Global.Input.triggered(Inputs.Right) ||
                            Global.Input.gesture_triggered(TouchGestures.SwipeLeft))
                        right = true;
                }

                if (cancel)
                {
                    OnCanceled(new EventArgs());
                }
                else if (changeDifficulty)
                {
                    SwitchDifficulty(true);
                }
                else if (left || right)
                {
                    Global.game_system.play_se(System_Sounds.Status_Page_Change);
                    MenuData.ChangeArc(right);
                    MenuData.Index = MenuData.IndexRedirect[0];
                    CreateCommandWindow();
                    Refresh();
                    ModeSwitchTimer = Constants.WorldMap.WORLDMAP_MODE_SWITCH_DELAY;
                }
                else if (CommandWindow.is_selected())
                {
                    OnChapterSelected(new EventArgs());
                }
            }
            // Chapter selected
            else
            {
                cancel |= ChapterCommandWindow.is_canceled();

                if (cancel)
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    CloseChapterCommands();
                }
                else if (changeDifficulty)
                {
                    SwitchDifficulty(true);
                }
                else if (ChapterCommandWindow.is_selected())
                {
                    OnChapterCommandSelected(new EventArgs());
                }
            }
        }
        #endregion

        #region Draw
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw window
            spriteBatch.Begin(
                SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, null, null);
            DataWindow.draw(spriteBatch);

            if (Active)
            {
                CancelButton.Draw(spriteBatch);
                if (DifficultyChangeButtonVisible())
                    DifficultyButton.Draw(spriteBatch);
            }

            spriteBatch.End();

            // Command window
            CommandWindow.draw(spriteBatch);
            if (ChapterCommandWindow != null)
                ChapterCommandWindow.draw(spriteBatch);
        }
        #endregion
    }

    interface IWorldmapHandler
    {
        void LoadData(string chapterId, Dictionary<string, string> previousChapters);
    }
}
