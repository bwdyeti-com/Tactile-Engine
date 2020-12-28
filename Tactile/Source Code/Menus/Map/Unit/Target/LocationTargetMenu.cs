using Microsoft.Xna.Framework;
using Tactile.Windows.Target;

namespace Tactile.Menus.Map.Unit.Target
{
    class LocationTargetMenu : TargetMenu<Vector2>
    {
        public LocationTargetMenu(Window_Target_Location window, IHasCancelButton menu = null)
            : base(window, menu) { }

        public Vector2 SelectedLoc
        {
            get { return Window.targets[Window.selected_index()]; }
        }

        public Vector2 Target { get { return Window.target; } }
    }
}
