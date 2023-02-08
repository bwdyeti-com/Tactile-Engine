using Tactile.Windows.Command;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Menus.Map
{
    class DialoguePromptMenu : CommandMenu
    {
        private bool WaitingForMessage = true;
        internal readonly int VariableId;

        public DialoguePromptMenu(
            Window_Command discardWindow,
            int variableId,
            IHasCancelButton menu = null) : base(discardWindow, menu)
        {
            VariableId = variableId;
        }
        
        protected override void Cancel()
        {
            Global.game_system.play_se(System_Sounds.Buzzer);
        }

        #region IMenu
        protected override void UpdateMenu(bool active)
        {
            if (WaitingForMessage)
            {
                if (!Global.scene.is_message_window_active ||
                    Global.scene.is_message_window_waiting)
                {
                    Global.game_system.play_se(System_Sounds.Open);
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
