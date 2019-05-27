using FEXNA.Windows.Command.Items;

namespace FEXNA.Menus.Map.Unit.Item
{
    class AttackItemMenu : ItemMenu
    {
        public AttackItemMenu(Window_Command_Item_Attack window, IHasCancelButton menu = null)
            : base(window, menu) { }

        public string Skill
        {
            get
            {
                var attackItemWindow = Window as Window_Command_Item_Attack;
                return attackItemWindow.skill;
            }
        }
    }
}
