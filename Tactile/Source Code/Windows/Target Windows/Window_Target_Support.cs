using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Tactile.Windows.Target
{
    class Window_Target_Support : Window_Target_Talk
    {
        public Window_Target_Support(int unit_id, Vector2 loc)
        {
            initialize(loc);
            Right_X = Config.WINDOW_WIDTH - this.window_width;
            Unit_Id = unit_id;
            List<int> targets = get_unit().support_targets();
            Targets = sort_targets(targets);
            this.index = 0;
            Temp_Index = this.index;
            Game_Unit target = Global.game_map.units[this.target];
            cursor_move_to(target);

            Global.player.instant_move = true;
            Global.player.update_movement();
            initialize_images();
            refresh();
            index = this.index;
        }
    }
}
