using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows.UserInterface.Command;

namespace Tactile.Menus
{
    class ConfirmationMenu : BaseMenu
    {
        Window_Confirmation Window;

        public ConfirmationMenu(Window_Confirmation window)
        {
            Window = window;
        }

        public Vector2 CurrentCursorLoc { get { return Window.current_cursor_loc; } }

        public event EventHandler<EventArgs> Confirmed;

        public event EventHandler<EventArgs> Canceled;
        protected bool OnCanceled(EventArgs e)
        {
            if (Canceled != null)
            {
                Canceled(this, e);
                return true;
            }

            return false;
        }

        #region IMenu
        public override bool HidesParent { get { return false; } }

        protected override void UpdateMenu(bool active)
        {
            if (!active)
                return;

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
                        Confirmed(this, new EventArgs());
                    }
                    else if (Window.index == 0)
                    {
                        Confirmed(this, new EventArgs());
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
