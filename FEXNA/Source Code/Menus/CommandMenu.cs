using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Windows.Command;
using FEXNA_Library;

namespace FEXNA.Menus
{
    class CommandMenu : BaseMenu, IHasCancelButton
    {
        protected Window_Command Window;
        protected Button_Description CancelButton;
        private bool _HidesParent = false;

        protected CommandMenu() { }
        public CommandMenu(Window_Command window, IHasCancelButton menu = null)
        {
            Window = window;

            CreateCancelButton(menu);
        }

        public Vector2 WindowLoc { get { return Window.loc; } }

        public Vector2 SelectedOptionLoc
        {
            get { return this.WindowLoc + new Vector2(0, 24 + this.SelectedIndex * 16); }
        }

        public int Index { get { return Window.index; } }

        internal void SetTextColor(int index, string color)
        {
            Window.set_text_color(index, color);
        }

        public Maybe<int> SelectedIndex
        {
            get
            {
                return Window.selected_index();
            }
        }

        protected void CreateCancelButton(IHasCancelButton menu)
        {
            if (menu != null && menu.HasCancelButton)
            {
                CreateCancelButton(
                    (int)menu.CancelButtonLoc.X,
                    Config.MAPCOMMAND_WINDOW_DEPTH);
            }
        }
        public void CreateCancelButton(int x, float depth = 0)
        {
            CancelButton = Button_Description.button(Inputs.B, x);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = depth;
        }

        protected virtual bool CanceledTriggered(bool active)
        {
            bool cancel = Window.is_canceled();
            if (CancelButton != null)
            {
                cancel |= CancelButton.consume_trigger(MouseButtons.Left) ||
                    CancelButton.consume_trigger(TouchGestures.Tap);
            }
            return cancel;
        }

        protected virtual bool HideCursorWhileInactive { get { return true; } }

        public void HideParent(bool value)
        {
            _HidesParent = value;
        }

        public event EventHandler<EventArgs> Selected;
        private void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        public event EventHandler<EventArgs> IndexChanged;
        protected void OnIndexChanged(EventArgs e)
        {
            if (IndexChanged != null)
                IndexChanged(this, e);
        }

        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled(EventArgs e)
        {
            if (Canceled != null)
                Canceled(this, e);
        }

        #region IMenu
        public override bool HidesParent { get { return _HidesParent; } }

        protected override void UpdateMenu(bool active)
        {
            if (this.HideCursorWhileInactive)
                Window.active = active;

            int index = Window.index;
            Window.update(active);
            if (index != Window.index)
                OnIndexChanged(new EventArgs());

            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    CreateCancelButton(this);
                CancelButton.Update(active);
            }
            bool cancel = CanceledTriggered(active);

            if (cancel)
            {
                Cancel();
            }
            else if (Window.is_selected())
            {
                SelectItem();
            }
        }

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
            Window.draw(spriteBatch);
            if (CancelButton != null)
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
