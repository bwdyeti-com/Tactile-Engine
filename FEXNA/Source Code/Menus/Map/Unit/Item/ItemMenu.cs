using System;
using Microsoft.Xna.Framework;
using FEXNA.Windows.Command.Items;

namespace FEXNA.Menus.Map.Unit.Item
{
    class ItemMenu : CommandMenu
    {
        public ItemMenu(Window_Command_Item window, IHasCancelButton menu = null)
            : base(window, menu)
        {
            HideParent(true);
        }

        public Game_Unit Unit
        {
            get
            {
                var itemWindow = Window as Window_Command_Item;
                return itemWindow.unit;
            }
        }

        public int SelectedItem
        {
            get
            {
                var itemWindow = Window as Window_Command_Item;
                return itemWindow.redirect();
            }
        }

        public bool EquippedSelected
        {
            get
            {
                var itemWindow = Window as Window_Command_Item;
                return itemWindow.equipped - 1 == this.SelectedItem;
            }
        }

        public void RefreshInventory()
        {
            var itemWindow = Window as Window_Command_Item;
            itemWindow.refresh_items();
            itemWindow.index = 0;
        }

        public void RestoreEquipped()
        {
            var itemWindow = Window as Window_Command_Item;
            itemWindow.restore_equipped();
        }

        public void Reset()
        {
            var itemWindow = Window as Window_Command_Item;
            itemWindow.refresh_items();
            itemWindow.index = 0;
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

        #region IMenu
        protected override void UpdateMenu(bool active)
        {
            int index = Window.index;
            Window.update(active);
            if (index != Window.index)
                OnIndexChanged(new EventArgs());

            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    CreateCancelButton(this);
                CancelButton.Update(active);
            }
            bool cancel = CanceledTriggered(active);

            var itemWindow = Window as Window_Command_Item;
            if (itemWindow.is_help_active)
            {
                if (cancel)
                    itemWindow.close_help();
            }
            else
            {
                if (itemWindow.getting_help())
                    itemWindow.open_help();
                else if (cancel)
                {
                    Cancel();
                }
                else if (itemWindow.is_selected())
                {
                    SelectItem();
                }
            }
        }

        protected virtual void SelectItem()
        {
            OnSelected(new EventArgs());
        }

        protected override void Cancel()
        {
            var itemWindow = Window as Window_Command_Item;

            Global.game_system.play_se(System_Sounds.Cancel);
            itemWindow.restore_equipped();
            itemWindow.unit.actor.staff_fix();
            Global.game_map.range_start_timer = 0;
            OnCanceled(new EventArgs());
        }
        #endregion
    }
}
