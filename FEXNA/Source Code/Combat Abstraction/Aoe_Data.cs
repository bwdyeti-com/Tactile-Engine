using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FEXNA
{
    class Aoe_Data : Combat_Data
    {
        public List<int> Battler_2_Ids;
        public int[] Hp2s, MaxHp2s, Team2s;
        protected string[] Name2s;
        protected int[] Kills;

        public override bool MultipleTargets { get { return true; } }

        public Aoe_Data(int battler_1_id, List<int> battler_2_ids)
        {
            initialize(battler_1_id, battler_2_ids);
        }

        protected void initialize(int battler_1_id, List<int> battler_2_ids)
        {
            Battler_1_Id = battler_1_id;
            Battler_2_Ids = battler_2_ids;
            Kills = new int[Battler_2_Ids.Count];
            Hp2s = new int[Battler_2_Ids.Count];
            MaxHp2s = new int[Battler_2_Ids.Count];
            Team2s = new int[Battler_2_Ids.Count];
            Name2s = new string[Battler_2_Ids.Count];
            setup();
        }

        public void set_attack(int val)
        {
            Hp2 = Hp2s[val];
            MaxHp2 = MaxHp2s[val];
            Name2 = Name2s[val];
        }

        #region Setup
        protected override void setup()
        {
            Game_Unit battler_1 = Global.game_map.units[Battler_1_Id];
            set_variables(battler_1, Battler_2_Ids);
            List<int> attack_array = new List<int>();
            for (int i = 0; i < Battler_2_Ids.Count; i++)
            {
                Game_Unit battler_2 = Global.game_map.units[Battler_2_Ids[i]];
                Hp2 = battler_2.actor.hp;
                battler_1.store_state();
                battler_2.store_state();
                process_attacks(battler_1, battler_2, Battler_2_Ids[i]); // why was there a breakpoint here? //Test
                battler_1.restore_state();
                battler_2.restore_state();
                // Set battle end stats
                if (Data.Count == 0)
                {
                    throw new NotImplementedException("A battle with no attacks tried to occur");
                }
                Data[Data.Count - 1].Key.end_battle(Distance, Data[Data.Count - 1].Value);
                // Check if a battler has been killed
                if (battler_1.is_dead)
                {
                    Kills[i] = 1;
                    Kill = Kills[i];
                }
                else if (battler_2.is_dead)
                {
                    Kills[i] = 2;
                    Kill = Kills[i];
                }
                // Exp/Wexp: battler 1
                if (battler_can_gain_wexp(battler_1))
                    Weapon_1_Broke = battler_1.is_weapon_broke(Weapon_1_Uses);
                else
                {
                    Exp_Gain1 = 0;
                    Wexp1 = 0;
                }
                if (battler_can_gain_exp(battler_1, battler_2) && Exp_Gain1 > -1)
                {
                    Exp_Gain1 += exp_gain(battler_1, battler_2, this.weapon1, Kill == 2);
                    if (!Full_Exp1)
                        Exp_Gain1 = (int)MathHelper.Clamp(Exp_Gain1 / 2, 1, 5);
                }
                else
                    Exp_Gain1 = 0;

                Exp_Gain2 = 0;
                // Reset HP
                battler_1.actor.hp = Hp1;
                battler_2.actor.hp = Hp2;
                Hp2 = 0;
                // Reset skills
                battler_1.actor.reset_skills(true);
                battler_2.actor.reset_skills(true);
            }
            Exp_Gain1 = (Exp_Gain1 > 0 ?
                Math.Min(Exp_Gain1, battler_1.actor.exp_gain_possible()) :
                Math.Max(Exp_Gain1, -battler_1.actor.exp_loss_possible()));
        }

        protected override bool exp_gain_cap(Game_Unit target)
        {
            return false;
        }

        protected void set_variables(Game_Unit battler_1, List<int> battler_2_ids)
        {
            Hp1 = battler_1.actor.hp;
            MaxHp1 = battler_1.actor.maxhp;
            Team1 = battler_1.team;
            Name1 = battler_1.actor.name;
            Exp1 = battler_1.actor.exp;
            Weapon_1_Id = battler_1.actor.weapon_id;
            for(int i = 0; i < battler_2_ids.Count; i++)
            {
                Game_Unit battler_2 = Global.game_map.units[battler_2_ids[i]];
                Hp2s[i] = battler_2.actor.hp;
                MaxHp2s[i] = battler_2.actor.maxhp;
                Team2s[i] = battler_2.team;
                Name2s[i] = battler_2.actor.name;
            }
        }

        protected override int exp_gain(Game_Unit battler_1, Game_Unit battler_2, FEXNA_Library.Data_Weapon weapon, bool kill)
        {
            //return Math.Max(Exp_Gain1, base.exp_gain(battler_1, battler_2, weapon, kill) / 2);
            return Math.Min(
                Math.Max(
                    Exp_Gain1,
                    base.exp_gain(battler_1, battler_2, weapon, kill)),
                Constants.Combat.AOE_EXP_GAIN); //Debug
        }

        protected override int combat_attacks(Game_Unit battler_1, Combat_Map_Object battler_2,
            out int numAttacks1, out int numAttacks2)
        {
            // Only one attack per target for area of effect
            numAttacks1 = 1;
            numAttacks2 = 0;
            return Math.Max(numAttacks1, numAttacks2);
        }
        #endregion

        public override void apply_combat(bool immediate_level, bool skip_exp)
        {
            // Prevents data from being applied multiple times
            if (Applied)
                return;
            Applied = true;

            Game_Unit battler_1 = Global.game_map.units[Battler_1_Id];
            int battler_1_hp = battler_1.hp;
            List<int> battler_2_hps = Battler_2_Ids.Select(x =>
                {
                    var map_object = Global.game_map.attackable_map_object(x);
                    return map_object == null ? map_object.hp : -1;
                }).ToList();
            for (int i = 0; i < Data.Count; i++)
            {
                Combat_Round_Data data = Data[i].Key;
                data.cause_damage();
            }
            Global.game_state.add_combat_metric(Metrics.CombatTypes.Battle, Battler_1_Id, Battler_2_Ids, battler_1_hp, battler_2_hps,
                Weapon_1_Id, Data.Where(x => x.Key.Attacker == 1).Count());

            use_weapons(battler_1, null);

            battler_1.actor.clear_added_attacks();
            if (skip_exp)
            {
                Exp_Gain1 = 0;
            }
            else
            {
                exp_gain(battler_1.actor, Exp_Gain1);
                if (battler_1.is_player_team)
                    if (Global.game_state.is_battle_map && !Global.game_system.In_Arena)
                        Global.game_system.chapter_exp_gain += Exp_Gain1;
            }
            if (Constants.Combat.AOE_WEXP_GAIN)
                if (Wexp1 > 0)
                {
                    FEXNA_Library.WeaponType type =
                        battler_1.actor.valid_weapon_type_of(Global.data_weapons[Weapon_1_Id]);
                    battler_1.actor.wexp_gain(type, Wexp1);
                }
            if (immediate_level)
            {
                if (battler_1.actor.needed_levels > 0)
                    battler_1.actor.level_up();
                battler_1.actor.clear_wlvl_up();
            }
        }

        public override bool is_ally_killed
        {
            get
            {
                if (Kill != 0)
                    for (int i = 0; i < Battler_2_Ids.Count; i++)
                        if ((Global.game_map.units[Kills[i] == 1 ?
                                Battler_1_Id : Battler_2_Ids[i]]).is_player_team)
                            return true;
                return false;
            }
        }

        public override bool game_over
        {
            get
            {
                if (Kill != 0)
                    for (int i = 0; i < Battler_2_Ids.Count; i++)
                        if ((Global.game_map.units[Kills[i] == 1 ?
                                Battler_1_Id : Battler_2_Ids[i]]).loss_on_death)
                            return true;
                return false;
            }
        }

        public override bool has_death_quote
        {
            get
            {
                if (Kill != 0)
                    for (int i = 0; i < Battler_2_Ids.Count; i++)
                        if (Global.game_state.get_death_quote(Kills[i] == 1 ?
                                Battler_1_Id : Battler_2_Ids[i]).Length > 0)
                            return true;
                return false;
            }
        }

        public override bool has_item_drop
        {
            get
            {
                if (Kill != 0)
                    for (int i = 0; i < Battler_2_Ids.Count; i++)
                    {
                        Game_Unit killed_unit = Global.game_map.units[Kills[i] == 1 ?
                            Battler_1_Id : Battler_2_Ids[i]];
                        Game_Unit winning_unit = Global.game_map.units[Kills[i] == 1 ?
                            Battler_2_Ids[i] : Battler_1_Id];
                        if (killed_unit.drops_item && killed_unit.actor.has_items &&
                                winning_unit.can_acquire_drops)
                            return true;
                    }
                return false;
            }
        }
    }
}
