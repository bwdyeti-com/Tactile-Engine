using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.Command.Items;

namespace FEXNA.Menus.Preparations
{
    class ItemRepairMenu : Menus.Map.Unit.Item.ItemMenu
    {
        private int ActorId, SelectedItemIndex;

        public ItemRepairMenu(
            int actorId,
            int selectedItemIndex,
            Window_Command_Item_Preparations_Repair window,
            IHasCancelButton menu = null)
            : base(window, menu)
        {
            ActorId = actorId;
            SelectedItemIndex = selectedItemIndex;
        }

        public Game_Actor Actor { get { return Global.game_actors[ActorId]; } }

        protected override void Activate()
        {
            Window.greyed_cursor = false;
            Window.active = true;
            base.Activate();
        }
        protected override void Deactivate()
        {
            Window.greyed_cursor = true;
            Window.active = false;
            base.Deactivate();
        }

        public override bool HidesParent { get { return false; } }

        protected override void SelectItem(bool playConfirmSound = false)
        {
            if (ValidInventoryTarget())
            {
                base.SelectItem(playConfirmSound);
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        #region Item Use
        public bool ValidInventoryTarget()
        {
            return this.Actor.items[SelectedItemIndex].to_item.can_target_item(
                this.Actor.items[this.SelectedItem]);
        }
        #endregion

        public override void Draw(SpriteBatch spriteBatch)
        {
            var itemWindow = Window as Window_Command_Item_Preparations;
            
            base.Draw(spriteBatch);
            
            itemWindow.draw_help(spriteBatch);
        }
    }
}
