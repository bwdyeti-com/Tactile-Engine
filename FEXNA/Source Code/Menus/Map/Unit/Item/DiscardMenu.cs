using System;
using FEXNA.Windows.Command.Items;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Menus.Map.Unit.Item
{
    class DiscardMenu : ItemMenu
    {
        private bool WaitingForPopup = false;

        public DiscardMenu(Window_Command_Item_Discard discardWindow, IHasCancelButton menu = null) : base(discardWindow, menu) { }
        
        public void WaitForPopup()
        {
            WaitingForPopup = true;
        }

        public string DropText
        {
            get
            {
                var discardWindow = (this.Window as Window_Command_Item_Discard);
                var item = this.Unit.actor.whole_inventory[discardWindow.redirect()];
                string text = string.Format("{0} {1} {2}?\n",
                    discardWindow.drop_text(),
                    item.to_equipment.article,
                    item.name);
                return text;
            }
        }

        protected override void SelectItem()
        {
            var discardWindow = (this.Window as Window_Command_Item_Discard);

            bool canDiscard = Global.battalion.convoy_ready_for_sending ||
                this.Unit.actor.CanDiscard(this.SelectedItem);
            if (!canDiscard)
                Global.game_system.play_se(System_Sounds.Buzzer);
            else
            {
                OnSelected(new EventArgs());
            }
        }

        protected override void Cancel()
        {
            Global.game_system.play_se(System_Sounds.Buzzer);
        }

        #region IMenu
        protected override void UpdateMenu(bool active)
        {
            if (WaitingForPopup)
            {
                if (!Global.scene.is_map_scene ||
                    !(Global.scene as Scene_Map).is_map_popup_active())
                {
                    Global.game_system.play_se(System_Sounds.Open);
                    WaitingForPopup = false;
                }
            }

            base.UpdateMenu(active && !WaitingForPopup);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!WaitingForPopup)
                base.Draw(spriteBatch);
        }
        #endregion
    }
}
