using System.Collections.Generic;

namespace FEXNA
{
    class Staff_Round_Data : Combat_Round_Data
    {
        private Staff_Modes StaffMode;

        public Staff_Round_Data(
                Game_Unit battler_1, Game_Unit battler_2, int distance, Staff_Modes mode) :
            base(battler_1, battler_2, distance)
        {
            StaffMode = mode;
        }

        protected override void prehit_def_skill_check(int distance, List<Combat_Action_Data> action_data, int hit, ref bool hit_skill_changed)
        {
        }
        protected override void prehit_skill_check(int distance, List<Combat_Action_Data> action_data, int hit, ref bool hit_skill_changed)
        {
        }
        protected override void hit_skill_check(int distance, List<Combat_Action_Data> action_data, bool is_hit, bool is_crt)
        {
        }
        protected override void onhit_skill_check(int distance, List<Combat_Action_Data> action_data, bool is_hit)
        {
        }
        protected override void posthit_skill_check(int distance, List<Combat_Action_Data> action_data)
        {
        }

        protected override Attack_Result calculate_attack(int distance, int dmg, bool hit, bool crt,
            FEXNA_Library.Data_Weapon weapon)
        {
            // Healing, Status Healing, Barrier, Positive Status
            if (StaffMode == Staff_Modes.Heal)
                return Combat.set_heal(this.AttackerUnit, this.TargetUnit, distance, weapon);
            // Status infliction
            else if (StaffMode == Staff_Modes.Status_Inflict)
                return Combat.set_status_staff(this.AttackerUnit, this.TargetUnit, distance, hit, weapon);
            // Flare
            else if (StaffMode == Staff_Modes.Torch)
                return Combat.set_torch(this.AttackerUnit, weapon);
            else
                return new Attack_Result { state_change = new List<KeyValuePair<int, bool>>() }; // Additional results add on after here //Yeti
        }

        public override bool is_successful_hit(FEXNA_Library.Data_Weapon weapon)
        {
            if (weapon.Barrier())
                return true;
            // If the attacker didn't hit themself (how does this happen with staves),
            // returns true when damage or status effects happened
            return !Result.backfire && (Result.dmg != 0 || Result.state_change.Count > 0);
        }
    }
}
