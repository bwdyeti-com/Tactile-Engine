using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Steal_Item : Status_Item
    {
        public void set_image(Game_Unit unit, Game_Unit target, int i)
        {
            TactileLibrary.Item_Data item_data = target.actor.items[i];
            if (item_data.non_equipment)
            {
                Icon.texture = null;
                Name.text = "";
                Uses.text = "";
                Slash.text = "";
                Use_Max.text = "";
            }
            else
            {
                bool can_steal = unit.can_steal_item(target, i);
                TactileLibrary.Data_Equipment item = item_data.to_equipment;
                // Icon
                Icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + item.Image_Name);
                Icon.index = item.Image_Index;
                // Name
                Name.text = item.Name;
                // Uses
                Uses.text = item_data.Uses < 0 ? "--" : item_data.Uses.ToString();
                Slash.text = "/";
                Use_Max.text = item.Uses < 0 ? "--" : item.Uses.ToString();

                set_text_color(can_steal);
            }
        }
    }
}
