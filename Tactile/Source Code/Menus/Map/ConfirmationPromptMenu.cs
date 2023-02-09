using Tactile.Windows.UserInterface.Command;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Menus.Map
{
    class ConfirmationPromptMenu : ConfirmationMenu
    {
        private bool WaitingForMessage = true;
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
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!WaitingForMessage)
                base.Draw(spriteBatch);
        }
        #endregion
    }
}
