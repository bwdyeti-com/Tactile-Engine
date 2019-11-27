using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;

namespace FEXNA.Menus
{
    abstract class StandardMenu : BaseMenu, IFadeMenu, IHasCancelButton
    {
        const int BLACK_SCEEN_FADE_IN_TIMER = 16;
        const int BLACK_SCEEN_FADE_OUT_TIMER = 16;
        const int BLACK_SCREEN_FADE_IN_HOLD_TIMER = 8;
        const int BLACK_SCREEN_FADE_OUT_HOLD_TIMER = 4;

        protected bool DataDisplayed { get; private set; }
        private MenuCallbackEventArgs MenuCallback;
        private Button_Description CancelButton;

        public StandardMenu(IHasCancelButton menu = null)
        {
            CreateCancelButton(menu);
        }

        protected virtual int DefaultCancelPosition { get { return Config.WINDOW_WIDTH - 80; } }
        protected virtual void CreateCancelButton(IHasCancelButton menu)
        {
            int position = this.DefaultCancelPosition;
            if (menu != null && menu.HasCancelButton)
            {
                position = (int)menu.CancelButtonLoc.X;
            }

            CreateCancelButton(
                position,
                Config.MAPCOMMAND_WINDOW_DEPTH);
        }
        public void CreateCancelButton(int x, float depth = 0)
        {
            CancelButton = Button_Description.button(Inputs.B, x);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = depth;
        }

        public abstract int Index { get; }

        protected virtual bool CanceledTriggered(bool active)
        {
            bool cancel = false;
            if (CancelButton != null)
            {
                cancel |= CancelButton.consume_trigger(MouseButtons.Left) ||
                    CancelButton.consume_trigger(TouchGestures.Tap);
            }
            return cancel;
        }
        protected abstract bool SelectedTriggered(bool active);

        /// <summary>
        /// Occurs when an element on the menu is selected.
        /// </summary>
        public event EventHandler<EventArgs> Selected;
        private void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        /// <summary>
        /// Occurs when the active element index of the menu changes.
        /// </summary>
        public event EventHandler<EventArgs> IndexChanged;
        protected void OnIndexChanged(EventArgs e)
        {
            if (IndexChanged != null)
                IndexChanged(this, e);
        }

        /// <summary>
        /// Occurs when the menu is canceled and should begin closing.
        /// </summary>
        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled(EventArgs e)
        {
            // If no cancel event is registered, just close
            if (Canceled == null)
            {
                OnClosed(e);
            }
            else
                Canceled(this, e);
        }

        /// <summary>
        /// Occurs when the menu opens and first becomes active.
        /// </summary>
        public event EventHandler<EventArgs> Opened;
        protected void OnOpened(EventArgs e)
        {
            if (Opened != null)
                Opened(this, e);
        }

        /// <summary>
        /// Occurs when the menu closes and is ready to be removed.
        /// </summary>
        public event EventHandler<EventArgs> Closed;
        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
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

        public virtual ScreenFadeMenu FadeInMenu(bool skipFadeIn = false)
        {
            return new ScreenFadeMenu(
                skipFadeIn ? 0 : BLACK_SCEEN_FADE_IN_TIMER,
                BLACK_SCREEN_FADE_IN_HOLD_TIMER,
                BLACK_SCEEN_FADE_IN_TIMER,
                true,
                this);
        }
        public virtual ScreenFadeMenu FadeOutMenu()
        {
            return new ScreenFadeMenu(
                BLACK_SCEEN_FADE_OUT_TIMER,
                BLACK_SCREEN_FADE_OUT_HOLD_TIMER,
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
        
        /// <summary>
        /// Automates the process of adding the menu to a menu manager,
        /// fading in, fading out, and calling the close event.
        /// </summary>
        /// <param name="e">Contains references to the add and close menu methods of the parent menu manager.</param>
        public void AddToManager(MenuCallbackEventArgs e)
        {
            // If menu callbacks are already set
            if (MenuCallback != null)
                return;
            // If canceled events is already set
            if (Canceled != null)
                return;

            MenuCallback = e;

            AddToManager();
        }
        private void AddToManager()
        {
            if (MenuCallback.AddMenuCall != null)
            {
                MenuCallback.AddMenuCall(this);
                // Add an event call for fading out
                this.Canceled += StandardMenu_CanceledFadeOut;

                // Add fade in
                var fadeInMenu = FadeInMenu();
                if (fadeInMenu != null && MenuCallback.MenuClosedCall != null)
                {
                    fadeInMenu.Finished += MenuCallback.MenuClosedCall;
                    MenuCallback.AddMenuCall(fadeInMenu);
                }
                // If not fading in, skip it
                else
                {
                    FadeShow();
                    FadeOpen();
                }
            }
        }

        private void StandardMenu_CanceledFadeOut(object sender, EventArgs e)
        {
            // Add fade out
            var fadeOutMenu = FadeOutMenu();
            if (fadeOutMenu != null && MenuCallback.MenuClosedCall != null)
            {
                fadeOutMenu.Finished += MenuCallback.MenuClosedCall;
                MenuCallback.AddMenuCall(fadeOutMenu);
            }
            // If not fading out, just close
            else
            {
                FadeHide();
                FadeClose();
            }
        }

        #region IMenu
        protected override void UpdateMenu(bool active)
        {
            active = IsActive(active);

            UpdateStandardMenu(active);
            
            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    CreateCancelButton(this);
                CancelButton.Update(active);
            }

            if (CanceledTriggered(active))
            {
                Cancel();
            }
            else if (SelectedTriggered(active))
            {
                SelectItem();
            }
        }
        protected virtual bool IsActive(bool active)
        {
            return active && DataDisplayed;
        }

        protected abstract void UpdateStandardMenu(bool active);

        protected virtual void SelectItem(bool playConfirmSound = false)
        {
            if (playConfirmSound)
                Global.game_system.play_se(System_Sounds.Confirm);
            OnSelected(new EventArgs());
        }
        protected virtual void Cancel()
        {
            Global.game_system.play_se(System_Sounds.Cancel);
            OnCanceled(new EventArgs());
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CancelButton != null && DataDisplayed)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                CancelButton.Draw(spriteBatch);
                spriteBatch.End();
            }
        }
        #endregion

        #region IHasCancelButton
        public bool HasCancelButton { get { return CancelButton != null; } }
        public Vector2 CancelButtonLoc { get { return CancelButton.loc; } }
        #endregion
    }
}
