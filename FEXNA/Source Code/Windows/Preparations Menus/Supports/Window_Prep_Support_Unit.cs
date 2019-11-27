using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Window_Prep_Support_Unit : Window_Prep_Items_Unit
    {
        public Window_Prep_Support_Unit() { }

        internal void refresh_map_sprites()
        {
            for (int i = 0; i < UnitNodes.Count(); i++)
            {
                refresh_map_sprite(i);
                refresh_font(i);
            }
        }
        
        protected override bool map_sprite_ready(int index)
        {
            return Global.game_actors[Global.battalion.actors[index]].is_support_maxed() ||
                Global.game_actors[Global.battalion.actors[index]].any_supports_ready();
        }

        protected override void refresh_font(int i)
        {
            bool forced = false, available = true;
            if (Global.game_actors[ActorList[i]].is_support_maxed())
                forced = true;
            else if (!Global.game_actors[ActorList[i]].any_supports_ready())
                available = false;
            
            refresh_font(i, forced, available);
        }
    }
}
