using Microsoft.Xna.Framework;

namespace Tactile.Windows.Command.Items
{
    class Window_Command_Item_Send : Window_Command_Item_Discard
    {
        public Window_Command_Item_Send(int unit_id, Vector2 loc) : base(unit_id, loc) { }

        public override string drop_text()
        {
            return "Send";
        }
    }
}
