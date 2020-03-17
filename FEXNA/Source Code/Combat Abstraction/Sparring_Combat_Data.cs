//Sparring
using System;

namespace FEXNA
{
    class Sparring_Combat_Data : Arena_Combat_Data
    {
        public Sparring_Combat_Data(int battler_1_id, int battler_2_id, int distance) : base(battler_1_id, battler_2_id, distance) { }

        protected override bool battler_can_gain_exp(Game_Unit battler, Game_Unit target = null)
        {
            return true;
        }
        protected override bool battler_can_gain_wexp(Game_Unit battler)
        {
            return true;
        }

        protected override int exp_gain(Game_Unit battler_1, Game_Unit battler_2, FEXNA_Library.Data_Weapon weapon, bool kill)
        {
            return Combat.training_exp(battler_1, battler_2, kill);
        }
    }
}
