using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FEXNA
{
    class Terrain_Attack_Data : Combat_Data
    {
        public Terrain_Attack_Data(int battler_1_id, int battler_2_id, int distance)
        {
            initialize(battler_1_id, battler_2_id, distance, new List<int>());
        }

        protected override void initialize(int battler_1_id, int battler_2_id, int distance, List<int> skip_attack)
        {
            Battler_1_Id = battler_1_id;
            Battler_2_Id = battler_2_id;
            Distance = distance;
            Skip_Attack = skip_attack;
            setup();
        }

        #region Setup
        protected override void setup()
        {
            Game_Unit battler_1 = Global.game_map.units[Battler_1_Id];
            Combat_Map_Object target = Global.game_map.get_map_object((int)Battler_2_Id) as Combat_Map_Object;
            set_variables(battler_1, target);
            List<int> attack_array = new List<int>();
            battler_1.store_state();
            process_attacks(battler_1, target);
            battler_1.restore_state();
            // Set battle end stats
            if (Data.Count == 0)
            {
                throw new NotImplementedException("A battle with no attacks tried to occur");
            }
            Data[Data.Count - 1].Key.end_battle(Distance, Data[Data.Count - 1].Value);
            // Check if a battler has been killed
            if (battler_1.is_dead)
                Kill = 1;
            else if (target.hp <= Data
                    .Select(x => x.Key)
                    .Where(x => (x.Attacker == 1 ^ x.Result.backfire) && x.Result.hit)
                    .Select(x => x.Result.dmg).Sum()) // target.is_dead) //Debug
                Kill = 2;
            // Exp/Wexp
            Wexp1 = 0;
            Wexp2 = 0;
            Exp_Gain1 = 0;
            Exp_Gain2 = 0;
            // Reset HP
            battler_1.actor.hp = Hp1;
            target.hp = Hp2;
            // Reset skills
            battler_1.actor.reset_skills(true);
        }
        #endregion

        public override void apply_combat(bool immediate_level, bool skip_exp)
        {
            // Prevents data from being applied multiple times
            if (Applied)
                return;
            Applied = true;

            Game_Unit battler_1 = Global.game_map.units[Battler_1_Id];
            Combat_Map_Object battler_2 = Global.game_map.get_map_object((int)Battler_2_Id) as Combat_Map_Object;
            int battler_1_hp = battler_1.hp;
            int battler_2_hp = battler_2 != null ? battler_2.hp : -1;
            for (int i = 0; i < Data.Count; i++)
            {
                Combat_Round_Data data = Data[i].Key;
                data.cause_damage();
            }
            Global.game_state.add_combat_metric(Metrics.CombatTypes.DestroyableTerrain,
                Battler_1_Id, Battler_2_Id, battler_1_hp, battler_2_hp, Weapon_1_Id, Weapon_2_Id,
                Data.Where(x => x.Key.Attacker == 1).Count(), Data.Where(x => x.Key.Attacker != 1).Count());

            use_weapons(battler_1, null);

            battler_1.actor.clear_added_attacks();
            if (skip_exp)
            {
                Exp_Gain1 = 0;
                Exp_Gain2 = 0;
            }
            else
            {
                exp_gain(battler_1.actor, Exp_Gain1);
                if (battler_1.is_player_team)
                    if (Global.game_state.is_battle_map && !Global.game_system.In_Arena)
                        Global.game_system.chapter_exp_gain += Exp_Gain1;
            }
            if (Wexp1 > 0)
            {
                FEXNA_Library.WeaponType type = battler_1.actor.valid_weapon_type_of(Global.data_weapons[Weapon_1_Id]);
                battler_1.actor.wexp_gain(type, Wexp1);
            }
            if (immediate_level)
            {
                if (battler_1.actor.needed_levels > 0)
                    battler_1.actor.level_up();
                battler_1.actor.clear_wlvl_up();
            }
        }
    }
}
