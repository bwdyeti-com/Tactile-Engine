using Microsoft.Xna.Framework;

namespace Tactile.Windows.Target
{
    abstract class Window_Target_Unit : Window_Target<int>
    {
        public override int target { get { return Targets.Count == 0 ? -1 : Targets[this.index]; } }

        internal override Vector2 target_loc(int target)
        {
            Combat_Map_Object unit1 = Global.game_map.attackable_map_object(target);
            return unit1.loc_on_map();
        }

        protected override int distance_to_target(int target)
        {
            return Global.game_map.unit_distance(Unit_Id, target);
        }

        protected override void reset_cursor()
        {
            if (Manual_Targeting)
                return;
            Game_Unit target = Global.game_map.units[Targets[Temp_Index]];
            cursor_move_to(target);
        }

        protected virtual void cursor_move_to(Map_Object target)
        {
            if (Manual_Targeting)
                return;
            Vector2 loc = target.loc_on_map();

            cursor_move_to(loc);
        }
    }
}
