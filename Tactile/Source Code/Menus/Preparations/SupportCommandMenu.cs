using Tactile.Windows.Command;

namespace Tactile.Menus.Preparations
{
    class SupportCommandMenu : CommandMenu
    {
        public SupportCommandMenu(Window_Command_Support window) : base(window)
        {
            CreateCancelButton(Config.WINDOW_WIDTH - 144, Config.MAPCOMMAND_WINDOW_DEPTH);
        }

        public int TargetId
        {
            get { return (Window as Window_Command_Support).TargetId; }
        }
    }
}
