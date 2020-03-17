using FEXNA.Windows.Command;

namespace FEXNA.Menus.Preparations
{
    class SupportCommandMenu : CommandMenu
    {
        public SupportCommandMenu(Window_Command_Support window) : base(window) { }

        public int TargetId
        {
            get { return (Window as Window_Command_Support).TargetId; }
        }
    }
}
