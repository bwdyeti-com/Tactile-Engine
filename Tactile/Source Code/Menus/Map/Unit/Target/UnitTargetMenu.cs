﻿using Tactile.Windows.Target;

namespace Tactile.Menus.Map.Unit.Target
{
    class UnitTargetMenu : TargetMenu<int>
    {
        public UnitTargetMenu(Window_Target_Unit window, IHasCancelButton menu = null)
            : base(window, menu) { }

        public int SelectedUnitId
        {
            get { return Window.targets[Window.selected_index().Index]; }
        }

        public int Target { get { return Window.target; } }
    }
}
