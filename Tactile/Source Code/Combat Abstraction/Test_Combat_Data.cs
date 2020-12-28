#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tactile
{
    class Test_Combat_Data : Combat_Data
    {
        public Test_Combat_Data(int battler_1_id, int battler_2_id, int distance) :
            base(battler_1_id, battler_2_id, distance) { }

        protected override void fix_unusable_items(Game_Unit battler_1, Game_Unit battler_2) { }
    }
}
#endif
