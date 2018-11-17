namespace FEXNA
{
    class Item_Repair_Popup : Item_Gain_Popup
    {
        public Item_Repair_Popup(int item_id, bool is_item, int time) : base(item_id, is_item, time) { }

        public override void update()
        {
            if (Timer > 0 && !Global.game_system.preparations && this.skip_input)
                Timer = Timer_Max;
            base.update();
        }

        protected override string got_text()
        {
            return "Repaired ";
        }
    }
}
