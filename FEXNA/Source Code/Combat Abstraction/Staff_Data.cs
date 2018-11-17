using System;
using System.Collections.Generic;

namespace FEXNA
{
    enum Staff_Modes { Heal, Status_Inflict, Torch, Other }
    class Staff_Data : Combat_Data
    {
        readonly static List<Staff_Modes> ATTACK_MODES = new List<Staff_Modes> { Staff_Modes.Status_Inflict };
        protected Staff_Modes Mode;

        #region Accessors
        public Staff_Modes mode { get { return Mode; } }

        public bool attack_staff { get { return ATTACK_MODES.Contains(Mode); } }
        #endregion

        protected Staff_Data() { }
        public Staff_Data(int battler_1_id, int battler_2_id, int distance)
        {
            initialize(battler_1_id, battler_2_id, distance, new List<int>());
        }
        public Staff_Data(int battler_1_id, int battler_2_id, int distance, List<int> skip_attack)
        {
            initialize(battler_1_id, battler_2_id, distance, skip_attack);
        }

        protected override void initialize(int battler_1_id, int battler_2_id, int distance, List<int> skip_attack)
        {
            Full_Exp1 = true;
            base.initialize(battler_1_id, battler_2_id, distance, skip_attack);
        }

        protected override void set_variables(Game_Unit battler_1, Game_Unit battler_2)
        {
            base.set_variables(battler_1, battler_2);
            Mode = get_staff_mode(Weapon_1_Id);
        }
        protected override void set_variables(Game_Unit battler_1, Combat_Map_Object battler_2)
        {
            base.set_variables(battler_1, battler_2);
            Mode = get_staff_mode(Weapon_1_Id);
        }

        internal static Staff_Modes get_staff_mode(int weaponId)
        {
            FEXNA_Library.Data_Weapon weapon = Global.data_weapons[weaponId];

            // Healing
            if (weapon.Heals())
                return Staff_Modes.Heal;
            // Status healing
            else if (!weapon.is_attack_staff() && weapon.Status_Remove.Count > 0)
                return Staff_Modes.Heal;
            // Status infliction
            else if (weapon.is_attack_staff())
                return Staff_Modes.Status_Inflict;
            // Barrier
            else if (weapon.Barrier())
                return Staff_Modes.Heal;
            // Positive Status
            else if (weapon.Status_Inflict.Count > 0)
                return Staff_Modes.Heal;
            // Flare
            else if (weapon.Torch())
                return Staff_Modes.Torch;
            else
                return Staff_Modes.Other;
        }

        protected override bool exp_gain_cap(Game_Unit target)
        {
            return false;
        }

        protected override int exp_gain(Game_Unit battler_1, Game_Unit battler_2, FEXNA_Library.Data_Weapon weapon, bool kill)
        {
            return Combat.staff_exp(battler_1.actor, weapon);
        }

        protected override void add_data(Game_Unit battler_1)
        {
            Data.Add(new KeyValuePair<Combat_Round_Data, List<Combat_Action_Data>>(
                new Staff_Round_Data(battler_1, null, Distance, Mode),
                new List<Combat_Action_Data>()));
        }
        protected override void add_data(Game_Unit battler_1, Game_Unit battler_2)
        {
            Data.Add(new KeyValuePair<Combat_Round_Data, List<Combat_Action_Data>>(
                new Staff_Round_Data(battler_1, battler_2, Distance, Mode),
                new List<Combat_Action_Data>()));
        }

        protected override int combat_attacks(Game_Unit battler_1, Combat_Map_Object battler_2,
            out int numAttacks1, out int numAttacks2)
        {
            // No double casting
            numAttacks1 = 1;
            numAttacks2 = 0;
            if (battler_2 != null && battler_2.is_unit())
                numAttacks2 = 1;
            return Math.Max(numAttacks1, numAttacks2);
        }

        public override void apply_combat(bool immediate_level, bool skip_exp)
        {
            // Prevents data from being applied multiple times
            if (Applied)
                return;

            base.apply_combat(immediate_level, skip_exp);

            if (Battler_2_Id != null)
            {
                apply_staff((int)Battler_2_Id);
            }
        }

        protected void apply_staff(int battler2Id)
        {
            Game_Unit target = Global.game_map.units[battler2Id];
            if (Mode == Staff_Modes.Heal)
            {
                Global.game_map.units[Battler_1_Id].actor.heal_support_gain(target.actor.id);
                target.actor.heal_support_gain(Global.game_map.units[Battler_1_Id].actor.id);
            }

#if DEBUG
            if (this.weapon1.Repair())
            {
                throw new Exception();
            }
#endif
            target.staff_effect(this.weapon1, -1);
        }
    }
}