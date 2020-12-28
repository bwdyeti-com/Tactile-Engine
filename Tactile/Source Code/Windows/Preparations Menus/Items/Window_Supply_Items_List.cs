using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Window_Supply_Items_List : Window_Supply_Items
    {
        public Window_Supply_Items_List(int actor_id, Vector2 loc) : base(actor_id, loc) { }

        protected override void get_items(List<SupplyItem> items)
        {
            if (Global.battalion.has_convoy)
                for (int i = 0; i < Global.game_battalions.active_convoy_data.Count; i++)
                {
                    if (is_item_add_valid(
                            this.type, Global.game_battalions.active_convoy_data[i]))
                        items.Add(new SupplyItem(0, i));
                }

            foreach(Game_Actor actor in Global.battalion.actors
                .Where(x => x != Actor_Id)
                .Select(x => Global.game_actors[x]))
            {
                for (int i = 0; i < actor.num_items; i++)
                {
                    if (is_item_add_valid(
                            this.type, actor.items[i]))
                        items.Add(new SupplyItem(actor.id, i));
                }
            }
            Game_Convoy.sort_supplies(items);
        }
    }
}
