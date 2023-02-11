using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows.UserInterface.Command;

namespace Tactile.Menus
{
    class ConfirmationMenu : BaseMenu
    {
        protected Window_Confirmation Window;

        public ConfirmationMenu(Window_Confirmation window)
        {
            Window = window;
        }

        public Vector2 CurrentCursorLoc { get { return Window.current_cursor_loc; } }

        public event EventHandler<EventArgs> Confirmed;
        protected virtual bool OnConfirmed(EventArgs e)
        {
            if (Confirmed != null)
            {
                Confirmed(this, e);
                return true;
            }

            return false;
        }

        public event EventHandler<EventArgs> Canceled;
        protected virtual bool OnCanceled(EventArgs e)
        {
            if (Canceled != null)
            {
                Canceled(this, e);
                return true;
            }

            return false;
        }
        protected bool HasCancelEvent { get { return Canceled != null; } }

        #region IMenu
        public override bool HidesParent { get { return false; } }

        protected override void UpdateMenu(bool active)
        {
            if (!active)
                return;

            UpdateWindow(active);
        }

        protected virtual void UpdateWindow(bool active)
        {
            Window.update();
            if (Window.is_ready)
            {
                if (Window.is_canceled())
                {
                    if (OnCanceled(new EventArgs()))
                        Global.game_system.play_se(System_Sounds.Cancel);
                }
                else if (Window.is_selected())
                {
                    // If no choices, default to confirm
                    if (Window.choice_count == 0)
                    {
                        OnConfirmed(new EventArgs());
                    }
                    else if (Window.index == 0)
                    {
                        OnConfirmed(new EventArgs());
                    }
                    else
                    {
                        if (OnCanceled(new EventArgs()))
                            Global.game_system.play_se(System_Sounds.Cancel);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Window.draw(spriteBatch);
        }
        #endregion
    }
}
