using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tactile.Graphics.Help;
using Tactile.Menus;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Title;

namespace Tactile
{
#if !MONOGAME && DEBUG
    enum Main_Menu_Selections { Resume, Start_Game, Options, Test_Battle, Extras, Quit, None}
#else
    enum Main_Menu_Selections { Resume, Start_Game, Options, Extras, Quit, None }
#endif
    class Window_Title_Main_Menu : BaseMenu, IFadeMenu
    {
        const int BLACK_SCEEN_FADE_TIMER = 15;
        const int BLACK_SCREEN_HOLD_TIMER = 8;
        readonly static Vector2 MENU_LOC = new Vector2(56, 32);
        const Main_Menu_Selections EXPANDABLE_SELECTION = Main_Menu_Selections.Start_Game;

        private bool MenusHidden = false;
        private Main_Menu_Selections Selection, ExpandedSelection;
        private bool SuspendExists;

        private bool DataDisplayed = false;
        private Menu_Background Background;
        private List<MainMenuChoicePanel> MenuChoices = new List<MainMenuChoicePanel>();
        private UINodeSet<MainMenuChoicePanel> ChoiceNodes;
        private Suspend_Info_Panel SuspendPanel;
        private StartGame_Info_Panel StartGamePanel;
        private Game_Updated_Banner GameUpdated;

        private Button_Description CancelButton;

        #region Accessors
        public override bool HidesParent { get { return DataDisplayed; } }
        #endregion

        public Window_Title_Main_Menu()
        {
            SuspendExists = Global.suspend_file_info != null;
            Selection = SuspendExists ? Main_Menu_Selections.Resume : Main_Menu_Selections.Start_Game;
            initialize();
        }

        protected void initialize()
        {
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Status_Background");
            Background.vel = new Vector2(-0.25f, 0);
            Background.tile = new Vector2(3, 2);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;

            MenuChoices.Add(new MainMenuChoicePanel("RESUME"));
            MenuChoices.Add(new MainMenuChoicePanel("START GAME"));

            MenuChoices.Add(new MainMenuChoicePanel("OPTIONS"));
#if DEBUG && !MONOGAME
            MenuChoices.Add(new MainMenuChoicePanel("TEST BATTLE"));
#endif

            MenuChoices.Add(new MainMenuChoicePanel("EXTRAS"));

            MenuChoices.Add(new MainMenuChoicePanel("QUIT"));

            CreateCancelButton();

            initialize_game_update_banner(true);

            RefreshMenuChoices();
        }

        private void CreateCancelButton()
        {
            CancelButton = Button_Description.button(Inputs.B,
                Config.WINDOW_WIDTH - 64);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = Config.TITLE_MENU_DEPTH;
        }

        private void initialize_game_update_banner(bool startOpened)
        {
            if (Global.is_update_found)
            {
                GameUpdated = new Game_Updated_Banner(startOpened);
                GameUpdated.Clicked += GameUpdated_Clicked;
            }
        }

        private void GameUpdated_Clicked(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            Global.visit_game_site();
        }

        internal void RefreshMenuChoices()
        {
            SuspendExists = Global.suspend_file_info != null;
            MenuChoices[(int)Main_Menu_Selections.Resume].Visible = SuspendExists;
            MenuChoices[(int)Main_Menu_Selections.Extras].Visible = Global.progress.ExtrasAccessible;

            if (SuspendExists)
            {
                SuspendPanel = new Suspend_Info_Panel(true);
                SuspendPanel.loc = new Vector2(-16, 8);
                SuspendPanel.stereoscopic = Config.TITLE_MENU_DEPTH;
            }
            else
            {
                SuspendPanel = null;
            }

            StartGamePanel = new StartGame_Info_Panel(
                Global.latest_save_id, MainMenuChoicePanel.PANEL_WIDTH, true);
            // Show preview of the latest save, if there is one
            StartGamePanel.active = Global.latest_save_id != -1;
            StartGamePanel.loc = new Vector2(-16, 8);
            StartGamePanel.stereoscopic = Config.TITLE_MENU_DEPTH;

            RefreshLocs();

            IEnumerable<MainMenuChoicePanel> nodes = new List<MainMenuChoicePanel>(MenuChoices);
            nodes = nodes.Where(x => x.Visible);
            ChoiceNodes = new UINodeSet<MainMenuChoicePanel>(nodes);
            ChoiceNodes.CursorMoveSound = System_Sounds.Menu_Move1;
            ChoiceNodes.SoundOnMouseMove = true;

            ChoiceNodes.set_active_node(MenuChoices[(int)Selection]);
            RefreshLocs();
        }

        private void RefreshLocs()
        {
            RefreshExpandedSelection();
            // Resume
            if (ExpandedSelection == Main_Menu_Selections.Resume)
            {
                MenuChoices[0].Size = new Vector2(
                    MenuChoices[0].Size.X, SuspendPanel == null ? 16 : SuspendPanel.height);
                MenuChoices[0].BgVisible = false;
            }
            else
            {
                MenuChoices[0].Size = new Vector2(MenuChoices[0].Size.X, 16);
                MenuChoices[0].BgVisible = true;
            }
            // Start Game
            if (ExpandedSelection == Main_Menu_Selections.Start_Game)
            {
                MenuChoices[1].Size = new Vector2(
                    MenuChoices[1].Size.X, StartGamePanel.height);
                MenuChoices[1].BgVisible = false;
            }
            else
            {
                MenuChoices[1].Size = new Vector2(MenuChoices[1].Size.X, 16);
                MenuChoices[1].BgVisible = true;
            }

            Vector2 loc = MENU_LOC + new Vector2(4, -12);
            for (int i = 0; i < MenuChoices.Count; i++)
            {
                MenuChoices[i].ResetOffset();
                MenuChoices[i].RefreshWidth(i == (int)Selection);

                if (i != (int)Selection && i != (int)ExpandedSelection)
                    MenuChoices[i].offset += new Vector2(-12, 0);

                if (MenuChoices[i].Visible)
                {
                    MenuChoices[i].loc = loc;
                    loc += new Vector2(0, MenuChoices[i].Size.Y);
                }
                MenuChoices[i].RefreshBg();
            }

            // Increases the top of the start game hitbox over the resume one somewhat
            if (SuspendExists && ExpandedSelection == Main_Menu_Selections.Resume)
            {
                int heightAdjustment = SuspendPanel.height - StartGamePanel.height;
                if (heightAdjustment > 0)
                {
                    MenuChoices[0].Size.Y -= heightAdjustment;
                    MenuChoices[1].Size.Y += heightAdjustment;
                    MenuChoices[1].loc.Y -= heightAdjustment;
                    MenuChoices[1].offset.Y -= heightAdjustment;
                }
            }
        }

        private void RefreshExpandedSelection()
        {
            bool changeExpanded =
                Input.ControlScheme == ControlSchemes.Buttons ||
                Selection <= EXPANDABLE_SELECTION;

            if (changeExpanded)
            {
                // Resume
                if (Selection == Main_Menu_Selections.Resume)
                {
                    if (SuspendExists)
                        ExpandedSelection = Selection;
                }
                else if (Selection <= EXPANDABLE_SELECTION)
                    ExpandedSelection = Selection;
                else
                    ExpandedSelection = Main_Menu_Selections.None;
            }
        }

        public Main_Menu_Selections SelectedOption
        {
            get
            {
                return Selection;
            }
        }

        #region Events
        public event EventHandler<EventArgs> Opened;
        protected void OnOpened(EventArgs e)
        {
            if (Opened != null)
                Opened(this, e);
        }

        public event EventHandler<EventArgs> Selected;
        protected void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled(EventArgs e)
        {
            if (Canceled != null)
                Canceled(this, e);
        }

        public event EventHandler<EventArgs> Closed;
        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }
        #endregion

        protected override void UpdateMenu(bool active)
        {
            if (Background != null)
                Background.update();
            if (GameUpdated != null)
            {
                GameUpdated.update();
                GameUpdated.UpdateInput();
            }

            int index = ChoiceNodes.ActiveNodeIndex;
            ChoiceNodes.Update(active);
            if (index != ChoiceNodes.ActiveNodeIndex)
            {
                ChangeSelection(ChoiceNodes.ActiveNodeIndex);
            }

            CancelButton.Update(active);

            if (active)
            {
                if (GameUpdated == null)
                    initialize_game_update_banner(false);

                var selected = ChoiceNodes.consume_triggered(
                    Inputs.A, MouseButtons.Left, TouchGestures.Tap);

                if (selected.IsSomething ||
                    Global.Input.KeyPressed(Keys.Enter) ||
                    Global.Input.triggered(Inputs.Start))
                {
                    if (selected.IsSomething)
                        ChangeSelection(selected.Index);
                    OnSelected(new EventArgs());
                }
                else if (Global.Input.triggered(Inputs.B) ||
                    CancelButton.consume_trigger(MouseButtons.Left) ||
                    CancelButton.consume_trigger(TouchGestures.Tap))
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    OnCanceled(new EventArgs());
                }
            }
        }

        protected override void UpdateAncillary()
        {
            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    CreateCancelButton();
            }
        }

        private void ChangeSelection(int index)
        {
            index = MenuChoices.IndexOf(ChoiceNodes[index]);
            Selection = (Main_Menu_Selections)index;
            RefreshLocs();
        }

        protected override void Activate()
        {
            MenusHidden = false;
        }

        public void HideMenus()
        {
            MenusHidden = true;
        }

        #region IFadeMenu
        public void FadeShow()
        {
            DataDisplayed = true;
        }
        public void FadeHide()
        {
            DataDisplayed = false;
        }

        public ScreenFadeMenu FadeInMenu(bool skipFadeIn)
        {
            return new ScreenFadeMenu(
                skipFadeIn ? 0 : BLACK_SCEEN_FADE_TIMER,
                BLACK_SCREEN_HOLD_TIMER,
                BLACK_SCEEN_FADE_TIMER,
                true,
                this);

        }
        public ScreenFadeMenu FadeOutMenu()
        {
            //@Debug: fade in over Config.TITLE_GAME_START_TIME instead
            return new ScreenFadeMenu(
                BLACK_SCEEN_FADE_TIMER,
                BLACK_SCREEN_HOLD_TIMER,
                false,
                this);
        }

        public void FadeOpen()
        {
            OnOpened(new EventArgs());
        }
        public void FadeClose()
        {
            OnClosed(new EventArgs());
        }
        #endregion

        #region Draw
        public override void Draw(SpriteBatch sprite_batch)
        {
            if (DataDisplayed)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                if (Background != null)
                    Background.draw(sprite_batch);
                sprite_batch.End();

                if (!MenusHidden)
                {
                    if (GameUpdated != null)
                        GameUpdated.draw(sprite_batch);

                    if (ExpandedSelection == Main_Menu_Selections.Resume)
                    {
                        SuspendPanel.Draw(sprite_batch,
                            -(MenuChoices[0].loc - MenuChoices[0].offset));
                    }

                    if (ExpandedSelection == Main_Menu_Selections.Start_Game)
                    {
                        sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                        StartGamePanel.Draw(sprite_batch,
                            -(MenuChoices[1].loc - MenuChoices[1].offset));
                        sprite_batch.End();
                    }

                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    ChoiceNodes.Draw(sprite_batch);
                    sprite_batch.End();

                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    CancelButton.Draw(sprite_batch);
                    sprite_batch.End();
                }
            }
        }
        #endregion
    }
}
