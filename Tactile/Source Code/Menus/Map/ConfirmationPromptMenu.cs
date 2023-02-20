using System;
using Tactile.Windows.UserInterface.Command;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Menus.Map
{
    class ConfirmationPromptMenu : ConfirmationMenu
    {
        private bool WaitingForMessage = true;
        private bool ConfirmWhenReady = false;
        private bool CancelWhenReady = false;
        internal readonly int SwitchId;

        public ConfirmationPromptMenu(
            Window_Confirmation confirmationWindow,
            int switchId) : base(confirmationWindow)
        {
            SwitchId = switchId;
        }

        #region IMenu
        protected override void UpdateMenu(bool active)
        {
            if (WaitingForMessage)
            {
                if (!Global.scene.is_message_window_active ||
                    Global.scene.is_message_window_waiting)
                {
                    WaitingForMessage = false;
                }
            }

            base.UpdateMenu(active && !WaitingForMessage);

            // This menu waits until the window shrinks before calling events
            if (Window.finished_moving)
            {
                if (ConfirmWhenReady)
                {
                    OnConfirmed(new EventArgs());
                    ConfirmWhenReady = false;
                }
                if (CancelWhenReady)
                {
                    OnCanceled(new EventArgs());
                    CancelWhenReady = false;
                }
            }
        }

        protected override void UpdateWindow(bool active)
        {
            Window.update();
            if (Window.is_ready && !ConfirmWhenReady && !CancelWhenReady)
            {
                if (Window.is_canceled())
                {
                    if (this.HasCancelEvent)
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        CancelWhenReady = true;
                        Window.close();
                    }
                }
                else if (Window.is_selected())
                {
                    // If no choices, default to confirm
                    if (Window.choice_count == 0)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        ConfirmWhenReady = true;
                        Window.close();
                    }
                    else if (Window.index == 0)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        ConfirmWhenReady = true;
                        Window.close();
                    }
                    else
                    {
                        if (this.HasCancelEvent)
                        {
                            Global.game_system.play_se(System_Sounds.Cancel);
                            CancelWhenReady = true;
                            Window.close();
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!WaitingForMessage)
                base.Draw(spriteBatch);
        }
        #endregion
    }
}
