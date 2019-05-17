using System;
using FEXNA.Windows.Command;

namespace FEXNA.Menus.Map.Unit.Item
{
    class ItemOptionsMenu : CommandMenu
    {
        public ItemOptionsMenu(Window_Item_Options window, IHasCancelButton menu = null)
            : base(window, menu) { }

        public int SelectedOption
        {
            get
            {
                var itemOptionsWindow = Window as Window_Item_Options;
                return itemOptionsWindow.redirect;
            }
        }

        public bool Unequips {
            get
            {
                var itemOptionsWindow = Window as Window_Item_Options;
                return itemOptionsWindow.unequip;
            }
        }

        public bool CanDiscard
        {
            get
            {
                var itemOptionsWindow = Window as Window_Item_Options;
                return itemOptionsWindow.can_discard;
            }
        }

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = base.CanceledTriggered(active);
            if (active)
            {
                // If right clicked or tapped on nothing in particular
                cancel |= Global.Input.mouse_click(MouseButtons.Right) ||
                    Global.Input.gesture_triggered(TouchGestures.Tap);
            }
            return cancel;
        }
    }
}
