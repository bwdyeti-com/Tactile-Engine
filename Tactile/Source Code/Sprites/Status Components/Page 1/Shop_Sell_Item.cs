using TactileLibrary;

namespace Tactile
{
    class Shop_Sell_Item : Shop_Item
    {
        public override void set_image(
            Game_Actor actor, Item_Data item_data, int stock, int price)
        {
            base.set_image(actor, item_data, -1, price);
            Uses.text = stock < 0 ? "--" : stock.ToString();
        }

        protected override bool buyable_text(int price, TactileLibrary.Item_Data item_data)
        {
            return item_data.to_equipment.Can_Sell;
        }
    }
}
