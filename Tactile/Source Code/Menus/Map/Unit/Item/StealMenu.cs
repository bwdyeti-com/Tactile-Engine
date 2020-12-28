using System;
using Tactile.Windows.Command.Items;

namespace Tactile.Menus.Map.Unit.Item
{
    class StealMenu : ItemMenu
    {
        public StealMenu(Window_Steal stealWindow, IHasCancelButton menu = null) : base(stealWindow, menu) { }
        
        protected override void SelectItem(bool playConfirmSound = false)
        {
            var stealWindow = (this.Window as Window_Steal);

            if (stealWindow.is_equipped(stealWindow.redirect()))
                stealWindow.equipped_steal_help();
            else if (!stealWindow.can_steal(stealWindow.redirect()))
                stealWindow.cant_steal_help();
            else
            {
                base.SelectItem(playConfirmSound);
            }
        }
    }
}
