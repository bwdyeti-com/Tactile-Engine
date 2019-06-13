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
                refresh_map_sprite(i);
        }
        
        protected override bool map_sprite_ready(int index)
        {
            return Global.game_actors[Global.battalion.actors[index]].is_support_maxed() ||
                Global.game_actors[Global.battalion.actors[index]].any_supports_ready();
        }

        protected override void refresh_font(int i)
        {
            int actor_id = Global.battalion.actors[i];

            string textColor = "White";
            if (Global.game_actors[actor_id].is_support_maxed())
                textColor = "Green";
            else if (!Global.game_actors[actor_id].any_supports_ready())
                textColor = "Grey";

            UnitNodes[i].set_name_texture(textColor);
        }
    }
}
