﻿using System.Linq;
using Microsoft.Xna.Framework;

namespace Tactile
{
    class Item_Steal_Popup : Item_Gain_Popup
    {
        public Item_Steal_Popup(int item_id, bool is_item, int time)
        {
            initialize(item_id, is_item, false);
            Timer_Max = time;
            loc = new Vector2((Config.WINDOW_WIDTH - Width) / 2, 80);
        }

        protected override string got_text()
        {
            return "Stole ";
        }
    }
}
