using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA.Windows.Target
{
    class Window_Target_Steal : Window_Target_Trade
    {
        public Window_Target_Steal(int unit_id, Vector2 loc)
        {
            initialize_trade(unit_id, loc);
        }

        protected override List<int> get_targets()
        {
            return get_unit().steal_targets();
        }

        protected override void draw_item(Game_Unit target, int i)
        {
            Items.Add(new Steal_Target_Item());
            ((Steal_Item)Items[i]).set_image(get_unit(), target, i);
            Items[i].draw_offset = new Vector2(8, 24 + i * 16);
        }
    }
}
