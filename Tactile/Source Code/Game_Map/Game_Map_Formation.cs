using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tactile
{
    partial class Game_Map
    {
        protected bool Changing_Formation = false;
        protected int Formation_Unit_Id1 = -1;
        protected int Formation_Unit_Id2 = -1;

        #region Accessors
        public bool changing_formation { get { return Changing_Formation; } }

        public int formation_unit_id1 { get { return Formation_Unit_Id1; } }
        public int formation_unit_id2 { get { return Formation_Unit_Id2; } }
        #endregion

        protected void update_formation()
        {
            if (Changing_Formation)
            {
                if (!this.units[Formation_Unit_Id1].is_changing_formation)
                {
                    this.units[Formation_Unit_Id1].queue_move_range_update();
                    if (Formation_Unit_Id2 != -1)
                        this.units[Formation_Unit_Id2].queue_move_range_update();
                    refresh_move_ranges();

                    Changing_Formation = false;
                    Formation_Unit_Id1 = -1;
                    Formation_Unit_Id2 = -1;

                    Move_Range_Visible = true;
                    highlight_test();
                    range_start_timer = 0;
                }
            }
        }
    }
}
