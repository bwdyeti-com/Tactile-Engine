using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Preparations;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface.Command;
using Tactile.Graphics.Help;

namespace Tactile.Menus.Preparations
{
    abstract class PreparationsBaseMenu : Windows.Map.Map_Window_Base, IFadeMenu
    {
        const int BLACK_SCEEN_FADE_TIMER = 8;
        const int BLACK_SCREEN_HOLD_TIMER = 4;

        protected Window_Prep_Items_Unit UnitWindow;
        protected WindowPanel ChooseUnitWindow;
        protected Pick_Units_Items_Header ItemHeader;
        protected Button_Description RButton;

        protected TextSprite ChooseUnitLabel;

        public int ActorId
        {
            get
            {
                return UnitWindow.actor_id;
            }
            set
            {
                UnitWindow.actor_id = value;
                UnitWindow.refresh_scroll();
                refresh();
            }
        }
        public Game_Actor actor { get { return Global.game_actors[ActorId]; } }
        
        public PreparationsBaseMenu()
        {
            InitializeSprites();
            update_black_screen();
        }

        protected override void set_black_screen_time()
        {
            Black_Screen_Fade_Timer = BLACK_SCEEN_FADE_TIMER;
            Black_Screen_Hold_Timer = BLACK_SCREEN_HOLD_TIMER;
            base.set_black_screen_time();
        }

        protected virtual void InitializeSprites()
        {
            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(
                @"Graphics\White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 255);
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Pictures\Preparation_Background");
            (Background as Menu_Background).vel = new Vector2(0, -1 / 3f);
            (Background as Menu_Background).tile = new Vector2(1, 2);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
            // Unit Window
            SetUnitWindow();
            UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DEPTH;
            UnitWindow.IndexChanged += UnitWindow_IndexChanged;
            // Choose Unit Window
            ChooseUnitWindow = new WindowPanel(Global.Content.Load<Texture2D>(
                @"Graphics\Windowskins\Preparations_Item_Options_Window"));
            ChooseUnitWindow.loc = new Vector2(Config.WINDOW_WIDTH - 120, Config.WINDOW_HEIGHT - 84);
            ChooseUnitWindow.width = 80;
            ChooseUnitWindow.height = 40;
            ChooseUnitWindow.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;

            ChooseUnitLabel = new TextSprite();
            ChooseUnitLabel.loc = new Vector2(12, 4);
            ChooseUnitLabel.SetFont(Config.UI_FONT, Global.Content, "White");
            SetLabel("Choose unit");
            ChooseUnitLabel.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;

            RefreshInputHelp();

            refresh();
        }

        protected void RefreshInputHelp()
        {
            RButton = Button_Description.button(Inputs.R,
                Global.Content.Load<Texture2D>(
                    @"Graphics\Windowskins\Preparations_Screen"),
                new Rectangle(126, 122, 24, 16));
            RButton.loc = new Vector2(216, 172) + new Vector2(60, -16);
            RButton.offset = new Vector2(-1, -1);
            RButton.stereoscopic = Config.PREPITEM_FUNDS_DEPTH;
        }

        protected void SetLabel(string str)
        {
            ChooseUnitLabel.text = str;
        }
        protected void LabelVisible(bool visible)
        {
            ChooseUnitWindow.visible = visible;
            ChooseUnitLabel.visible = visible;
        }

        protected virtual void SetUnitWindow()
        {
            UnitWindow = new Window_Prep_Items_Unit();
        }

        public abstract void refresh();
        public event EventHandler<EventArgs> UnitSelected;
        protected void OnUnitSelected(EventArgs e)
        {
            if (UnitSelected != null)
                UnitSelected(this, e);
        }

        public event EventHandler<EventArgs> Status;
        protected void OnStatus(EventArgs e)
        {
            if (Status != null)
                Status(this, e);
        }

        #region IFadeMenu
        public virtual void FadeShow() { }
        public virtual void FadeHide() { }

        public ScreenFadeMenu FadeInMenu(bool skipFadeIn)
        {
            return new ScreenFadeMenu(
                BLACK_SCEEN_FADE_TIMER,
                BLACK_SCREEN_HOLD_TIMER,
                BLACK_SCEEN_FADE_TIMER,
                true,
                this);
        }
        public ScreenFadeMenu FadeOutMenu()
        {
            return new ScreenFadeMenu(
                BLACK_SCEEN_FADE_TIMER,
                BLACK_SCREEN_HOLD_TIMER,
                false,
                this);
        }

        public void FadeOpen() { }
        public void FadeClose() { }
        #endregion

        #region Update
        protected override void UpdateMenu(bool active)
        {
            UpdateUnitWindow(active);
            
            base.UpdateMenu(active);
        }

        protected override void UpdateAncillary()
        {
            if (Input.ControlSchemeSwitched)
                RefreshInputHelp();
        }

        private void UnitWindow_IndexChanged(object sender, EventArgs e)
        {
            refresh();
        }

        public abstract void CancelUnitSelecting();
        public abstract bool SelectUnit(int index);

        protected abstract void UpdateUnitWindow(bool active);
        
        // Update input modes
        protected override void update_input(bool active)
        {
            if (active && this.ready_for_inputs)
            {
                // Close this window
                if (Global.Input.triggered(Inputs.B))
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    CancelUnitSelecting();
                    return;
                }

                // Select unit
                var selectedIndex = UnitWindow.consume_triggered(
                    Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                if (selectedIndex.IsSomething)
                {
                    SelectUnit(selectedIndex.Index);
                    return;
                }

                // Status screen
                var statusIndex = UnitWindow.consume_triggered(
                    Inputs.R, MouseButtons.Right, TouchGestures.LongPress);
                if (statusIndex.IsSomething)
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    OnStatus(new EventArgs());
                    return;
                }
            }
        }
        #endregion

        #region Draw
        protected override void draw_window(SpriteBatch sprite_batch)
        {
            UnitWindow.draw(sprite_batch);
            DrawStatsWindow(sprite_batch);

            DrawHeader(sprite_batch);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            ChooseUnitWindow.draw(sprite_batch);
            ChooseUnitLabel.draw(sprite_batch, -ChooseUnitWindow.loc);
            sprite_batch.End();
            
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            RButton.Draw(sprite_batch, -new Vector2(0, 20));
            sprite_batch.End();
        }

        protected abstract void DrawStatsWindow(SpriteBatch spriteBatch);

        protected virtual void DrawHeader(SpriteBatch spriteBatch)
        {
            if (ItemHeader != null)
                ItemHeader.draw(spriteBatch);
        }
        #endregion
    }
}
