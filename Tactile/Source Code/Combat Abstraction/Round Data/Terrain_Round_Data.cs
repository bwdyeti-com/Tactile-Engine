using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tactile
{
    class Terrain_Round_Data : Combat_Round_Data
    {
        private Combat_Map_Object TargetTerrain;

        public Terrain_Round_Data(Game_Unit battler_1, Combat_Map_Object battler_2, int distance)
        {
            Battler1 = battler_1;
            TargetTerrain = battler_2;

            List<int?> ary = combat_stats(distance);
            Stats = ary;
        }

        protected virtual List<int?> combat_stats(int distance)
        {
            return Combat.combat_stats(Battler1.id, TargetTerrain.id, distance);
        }

        public void set_attack(int distance, List<Combat_Action_Data> action_data)
        {
            TactileLibrary.Data_Weapon weapon = Battler1.actor.weapon;
            // Hit test
            //bool[] hit_success = new bool[] { true, false }; //Debug
            //if (Stats[Attacker == 1 ? 0 : 4] != null) // Why even bother to roll RNs for this //Debug
            //    hit_success = Combat.test_hit(battler, target, distance);

            //bool hit = hit_success[0]; //Debug
            //bool crt = hit_success[1];
            List<int?> combat_stats = this.combat_stats(distance);
            int dmg = (int)combat_stats[1];
            bool hit = true;
            bool crt = false;
            Battler1.hit_skill_update();
            // Calculate attack
            Result = calculate_attack(distance, dmg, hit, crt, weapon);
            // Reset skills
            Battler1.reset_skills();
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
            TactileLibrary.Data_Weapon weapon)
        {
            return Combat.set_attack(this.AttackerUnit, TargetTerrain, distance, dmg, hit, crt, weapon);
        }

        public override void cause_damage(bool test = false)
        {
            cause_damage(this.AttackerUnit, TargetTerrain, test);
        }
    }
}
