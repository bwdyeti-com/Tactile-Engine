using System.Collections.Generic;

namespace Tactile
{
    class Arena_Combat_Data : Combat_Data
    {
        public Arena_Combat_Data() : base() { }
        public Arena_Combat_Data(int battler_1_id, int battler_2_id, int distance) : base(battler_1_id, battler_2_id, distance) { }

        protected override bool exp_gain_cap(Game_Unit target)
        {
            return false;
        }

        protected override void add_data(Game_Unit battler_1)
        {
            Data.Add(new KeyValuePair<Combat_Round_Data, List<Combat_Action_Data>>(
                new Arena_Combat_Round_Data(battler_1, null, Distance), new List<Combat_Action_Data>()));
        }
        protected override void add_data(Game_Unit battler_1, Game_Unit battler_2)
        {
            Data.Add(new KeyValuePair<Combat_Round_Data, List<Combat_Action_Data>>(
                new Arena_Combat_Round_Data(battler_1, battler_2, Distance), new List<Combat_Action_Data>()));
        }
    }
}
