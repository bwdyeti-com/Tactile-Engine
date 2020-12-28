using Microsoft.Xna.Framework;

namespace Tactile.Windows.Target
{
    abstract class Window_Target_Location : Window_Target<Vector2>
    {
        public override Vector2 target
        {
            get { return Targets[this.index]; }
        }

        internal override Vector2 target_loc(Vector2 target)
        {
            return target;
        }

        protected override int distance_to_target(Vector2 target)
        {
            return Global.game_map.distance(get_unit().loc, target);
        }

        protected override void reset_cursor()
        {
            Vector2 loc = Targets[Temp_Index];
            cursor_move_to(loc);
        }
    }
}
