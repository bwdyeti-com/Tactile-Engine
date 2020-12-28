using Microsoft.Xna.Framework.Graphics;
using TactileLibrary;

namespace Tactile
{
    class Restock_Item : Status_Item
    {
        public override void set_image(Game_Actor actor, Item_Data item_data)
        {
            base.set_image(actor, item_data);
            if (!item_data.non_equipment)
            {
                Data_Equipment item = item_data.to_equipment;

                bool can_restock = false;
                if (item_data.Uses != item.Uses)
                    for (int i = 0; i < Global.game_battalions.active_convoy_data.Count; i++)
                        if (item_data.same_item(Global.game_battalions.active_convoy_data[i]))
                        {
                            can_restock = true;
                            break;
                        }
                set_text_color(can_restock);
            }
        }
    }
}
