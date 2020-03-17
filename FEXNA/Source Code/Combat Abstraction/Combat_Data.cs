using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNA_Library;
using ListExtension;

namespace FEXNA
{
    internal class Combat_Data
    {
        public int Battler_1_Id, Hp1, MaxHp1, Weapon_1_Id, Weapon_1_Uses = 0, Wexp1, Exp1, Exp_Gain1 = -1, Team1;
        public int? Battler_2_Id;
        public int Hp2, MaxHp2, Weapon_2_Id, Weapon_2_Uses = 0, Wexp2, Exp2, Exp_Gain2 = -1, Team2;
        public string Name1, Name2;
        public int Kill;
        public List<KeyValuePair<Combat_Round_Data, List<Combat_Action_Data>>> Data =
            new List<KeyValuePair<Combat_Round_Data,List<Combat_Action_Data>>>();
        public int Distance;
        protected List<int> Add_Attack = new List<int>();
        protected List<int> Skip_Attack = new List<int>();
        protected bool Full_Exp1 = false, Full_Exp2 = false;
        protected bool Weapon_1_Broke = false, Weapon_2_Broke = false;
        protected bool Attacked_1, Attacked_2;

        protected bool Applied = false;

        #region Accessors
        public bool kill { get { return Kill != 0; } }

        public List<int> skip_attack { get { return Skip_Attack; } }

        public bool weapon_1_broke { get { return Weapon_1_Broke; } }
        public bool weapon_2_broke { get { return Weapon_2_Broke; } }

        protected Data_Weapon weapon1 { get { return Weapon_1_Id <= 0 ? null : Global.data_weapons[Weapon_1_Id]; } }
        protected Data_Weapon weapon2 { get { return Weapon_2_Id <= 0 ? null : Global.data_weapons[Weapon_2_Id]; } }

        public virtual bool MultipleTargets { get { return false; } }
        #endregion

        protected Combat_Data() { }
        public Combat_Data(int battler_1_id, int battler_2_id, int distance)
        {
            initialize(battler_1_id, battler_2_id, distance, new List<int>());
        }
        public Combat_Data(int battler_1_id, int battler_2_id, int distance, List<int> skip_attack)
        {
            initialize(battler_1_id, battler_2_id, distance, skip_attack);
        }

        protected virtual void initialize(int battler_1_id, int battler_2_id, int distance, List<int> skip_attack)
        {
            Battler_1_Id = battler_1_id;
            Battler_2_Id = battler_2_id == -1 ? null : (int?)battler_2_id;
            Distance = distance;
            Skip_Attack = skip_attack;
            setup();
        }

        #region Setup
        protected virtual void setup()
        {
            Game_Unit battler_1 = Global.game_map.units[Battler_1_Id];
            Game_Unit battler_2 = Battler_2_Id == null ? null : Global.game_map.units[(int)Battler_2_Id];
            set_variables(battler_1, battler_2);
            List<int> attack_array = new List<int>();
            battler_1.store_state();
            if (battler_2 != null)
                battler_2.store_state();
            process_attacks(battler_1, battler_2);
            // Set battle end stats
            if (Data.Count == 0)
            {
                throw new NotImplementedException("A battle with no attacks tried to occur");
            }
            end_battle();
            set_exp(battler_1, battler_2);
            battler_1.restore_state();
            if (battler_2 != null)
                battler_2.restore_state();
        }

        protected virtual void set_exp(Game_Unit battler_1, Game_Unit battler_2)
        {
            // Check if a battler has been killed
            if (battler_1.is_dead)
                Kill = 1;
            else if (battler_2 != null && battler_2.is_dead)
                Kill = 2;
            // Wexp
            if (battler_can_gain_wexp(battler_1))
                Weapon_1_Broke = battler_1.is_weapon_broke(Weapon_1_Uses);
            else
            {
                //Exp_Gain1 = 0; //Debug
                Wexp1 = 0;
            }
            if (battler_2 != null)
            {
                if (battler_can_gain_wexp(battler_2))
                    Weapon_2_Broke = battler_2.is_weapon_broke(Weapon_2_Uses);
                else
                {
                    //Exp_Gain2 = 0; //Debug
                    Wexp2 = 0;
                }
            }
            // Exp: battler 1
            get_exp_gain(battler_1, battler_2, 1, ref Exp_Gain1);
            // Exp: battler 2
            if (battler_2 != null)
                get_exp_gain(battler_2, battler_1, 2, ref Exp_Gain2);

            // Cap exp gain to amount that can be gained, and then by the amount that can be given
            cap_exp_gain(battler_1, battler_2, ref Exp_Gain1);
            if (battler_2 != null)
                cap_exp_gain(battler_2, battler_1, ref Exp_Gain2);
            // Reset HP and skills
            battler_1.actor.hp = Hp1;
            battler_1.actor.reset_skills(true);
            if (battler_2 != null)
            {
                battler_2.actor.hp = Hp2;
                battler_2.actor.reset_skills(true);
            }
        }

        private void get_exp_gain(Game_Unit battler, Game_Unit target,
            int attacker, ref int expGain)
        {
            if (battler_can_gain_exp(battler, target) &&
                battler.actor.can_level() && expGain > -1)
            {
                if (attacker == 1)
                {
                    expGain = exp_gain(battler, target, this.weapon1, Kill == 2);
                    if (!Full_Exp1)
                        expGain = (int)MathHelper.Clamp(expGain / 2, 1, 5);
                }
                else
                {
                    expGain = exp_gain(battler, target, this.weapon1, Kill == 1);
                    if (!Full_Exp2)
                        expGain = (int)MathHelper.Clamp(expGain / 2, 1, 5);
                }
            }
            else
                expGain = 0;
        }
        private void cap_exp_gain(Game_Unit battler, Game_Unit target, ref int expGain)
        {
            if (expGain != 0)
            {
                expGain = (expGain > 0 ?
                    Math.Min(expGain, battler.actor.exp_gain_possible()) :
                    Math.Max(expGain, -battler.actor.exp_loss_possible()));
                // If unit is not playable, don't allow level ups
                if (!battler.is_player_team)
                    expGain = Math.Min(expGain, Constants.Actor.EXP_TO_LVL -
                        (battler.actor.exp + 1));
                // Caps total exp gain per individual unit
                if (expGain > 0 && exp_gain_cap(target))
                    expGain = target.cap_exp_given(expGain);
            }
        }

        protected virtual bool exp_gain_cap(Game_Unit target)
        {
            return Constants.Actor.EXP_PER_ENEMY >= 0 &&
                !(target.is_dead && Constants.Actor.EXP_PER_ENEMY_KILL_EXCEPTION);
        }

        protected virtual bool battler_can_gain_exp(Game_Unit battler, Game_Unit target = null)
        {
            if (!battler.allowed_to_gain_exp())
                return false;
            // If the target exists, either the target needs to be on the opposing team or the attacker needs to not be berserk
            return (target == null || !battler.berserk || battler.is_attackable_team(target));
        }

        protected virtual bool battler_can_gain_wexp(Game_Unit battler)
        {
            return battler.is_ally && !battler.is_dead;
        }

        protected virtual void set_variables(Game_Unit battler_1, Game_Unit battler_2)
        {
            Hp1 = battler_1.actor.hp;
            MaxHp1 = battler_1.actor.maxhp;
            Team1 = battler_1.team;
            Name1 = battler_1.actor.name;
            Exp1 = battler_1.actor.exp;
            Weapon_1_Id = battler_1.actor.weapon_id;
            if (battler_2 != null)
            {
                Hp2 = battler_2.actor.hp;
                MaxHp2 = battler_2.actor.maxhp;
                Team2 = battler_2.team;
                Name2 = battler_2.actor.name;
                Exp2 = battler_2.actor.exp;
                Weapon_2_Id = battler_2.actor.weapon_id;
            }
        }
        protected virtual void set_variables(Game_Unit battler_1, Combat_Map_Object battler_2)
        {
            Hp1 = battler_1.actor.hp;
            MaxHp1 = battler_1.actor.maxhp;
            Team1 = battler_1.team;
            Name1 = battler_1.actor.name;
            Exp1 = battler_1.actor.exp;
            Weapon_1_Id = battler_1.actor.weapon_id;
            if (battler_2 != null)
            {
                Hp2 = battler_2.hp;
                MaxHp2 = battler_2.maxhp;
                Team2 = battler_2.team;
                Name2 = battler_2.name;
                Exp2 = 0;
                Weapon_2_Id = -1;
            }
        }

        protected virtual void process_attacks(Game_Unit battler_1, Combat_Map_Object battler_2)
        {
            process_attacks(battler_1, battler_2, battler_2 == null ? -1 : (int)Battler_2_Id);
        }
        protected void process_attacks(Game_Unit battler_1, Combat_Map_Object battler_2, int targetId)
        {
            battler_1.start_attack(-1, battler_2);
            if (battler_2 is Game_Unit)
                (battler_2 as Game_Unit).start_attack(-1, battler_1);

            bool ambush = battler_2 != null && battler_2.is_unit() &&
                Global.game_map.units[targetId].counters_first(Global.game_map.units[Battler_1_Id]);

            int num_attacks1, num_attacks2;
            for(int i = 0; i < combat_attacks(
                battler_1, battler_2, out num_attacks1, out num_attacks2); i++)
            {
                if (ambush)
                {
                    if (battler_2 != null && battler_2.is_unit() &&
                            i < num_attacks2 && this.weapon2 != null)
                        add_attacks(battler_1, battler_2, 2, this.weapon2);
                }
                // Add hits for battler_1 in this attack
                if (i < num_attacks1)
                {
                    if (i < num_attacks1)
                        add_attacks(battler_1, battler_2, 1, this.weapon1);
                }
                if (!ambush)
                {
                    if (battler_2 != null && battler_2.is_unit() &&
                            i < num_attacks2 && this.weapon2 != null)
                        add_attacks(battler_1, battler_2, 2, this.weapon2);
                }
            }
        }

        private void add_attacks(
            Game_Unit battler_1, Combat_Map_Object battler_2,
            int i, Data_Weapon weapon)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(i == 1 || i == 2);
#endif
            Game_Unit attacker = i == 2 ? battler_2 as Game_Unit : battler_1;
            Combat_Map_Object target = i == 2 ? battler_1 : battler_2;

            int num_hits = 1;
            if (!attacker.is_brave_blocked())
                // If only single hits against terrain
                if (!(target != null && !target.is_unit() &&
                        Constants.Combat.BRAVE_BLOCKED_AGAINST_DESTROYABLE))
                    num_hits = weapon.HitsPerAttack;
            for (int j = 0; j < num_hits; j++)
                add_attacks(battler_1, battler_2, i);
        }

        private void add_attacks(
            Game_Unit battler_1, Combat_Map_Object battler_2, int i)
        {
            Game_Unit attacker = battler_1;
            Combat_Map_Object target = battler_2;
            if (i == 2)
            {
                if (!battler_2.is_unit())
                    return;
                attacker = battler_2 as Game_Unit;
                target = battler_1;
            }

            bool cont = false;
            while (!cont)
            {
                int attacker_index = 0;
                bool attack_occurs = attackable(attacker, target);
                if (attack_occurs)
                {
                    attacker_index = i;
                    if (Skip_Attack.Contains(attacker_index))
                    {
                        attacker_index = 0;
                        Skip_Attack.pop();
                    }
                }
                if (attacker_index > 0)
                {
                    if (battler_2 == null)
                        add_solo_attack(battler_1, attacker_index);
                    else if (battler_2.is_unit())
                        add_attack(battler_1, battler_2 as Game_Unit, attacker_index);
                    else
                        add_attack(battler_1, battler_2 as Combat_Map_Object, attacker_index);
                }

                cont = true;
                // If an added attack is buffered
                if (Add_Attack.Count > 0)
                {
                    attacker_index = Add_Attack.pop();
                    cont = false;
                }
            }
        }

        protected virtual int combat_attacks(Game_Unit battler_1, Combat_Map_Object battler_2,
            out int numAttacks1, out int numAttacks2)
        {
            numAttacks1 = battler_1.attacks_per_round(battler_2, Distance);
            numAttacks2 = 0;
            if (battler_2 != null && battler_2.is_unit())
                numAttacks2 = (battler_2 as Game_Unit).attacks_per_round(battler_1, Distance);
            return Math.Max(numAttacks1, numAttacks2);
        }

        protected virtual int exp_gain(Game_Unit battler_1, Game_Unit battler_2, Data_Weapon weapon, bool kill)
        {
            return Combat.exp(battler_1, battler_2, kill);
        }

        protected void add_attack(Game_Unit battler_1, Game_Unit battler_2, int attacker)
        {
            battler_1.start_attack(Data.Count(x => x.Key.Attacker == attacker), battler_2);
            add_data(battler_1, battler_2);
            Data[Data.Count - 1].Key.Attacker = attacker;
            // Checks if this is the first attack of this unit
            for (int i = 0; i < Data.Count - 1; i++)
            {
                if (Data[i].Key.Attacker == Data[Data.Count - 1].Key.Attacker)
                {
                    Data[Data.Count - 1].Key.First_Attack = false;
                    break;
                }
            }
            if (attacker == 1)
                Attacked_1 = true;
            else
                Attacked_2 = true;
            Data[Data.Count - 1].Key.set_attack(Distance, Data[Data.Count - 1].Value);
            // Add Attacks
            if (battler_1.actor.added_attacks.Count > 0)
                foreach (int attack in battler_1.actor.added_attacks)
                    Add_Attack.Add(attack == 1 ? 1 : 2);
            battler_1.actor.clear_added_attacks();
            if (battler_2.actor.added_attacks.Count > 0)
                foreach (int attack in battler_2.actor.added_attacks)
                    Add_Attack.Add(attack == 1 ? 2 : 1);
            battler_2.actor.clear_added_attacks();
            exp_gain_test(battler_1, battler_2, attacker, Data[Data.Count - 1]);
        }
        protected void add_attack(Game_Unit battler_1, Combat_Map_Object battler_2, int attacker)
        {
            battler_1.start_attack(Data.Count(x => x.Key.Attacker == attacker), battler_2);
            add_data(battler_1, battler_2);
            Data[Data.Count - 1].Key.Attacker = attacker;
            // Checks if this is the first attack of this unit
            for (int i = 0; i < Data.Count - 1; i++)
            {
                if (Data[i].Key.Attacker == Data[Data.Count - 1].Key.Attacker)
                {
                    Data[Data.Count - 1].Key.First_Attack = false;
                    break;
                }
            }
            if (attacker == 1)
                Attacked_1 = true;
            else
                Attacked_2 = true;
            ((Terrain_Round_Data)Data[Data.Count - 1].Key).set_attack(Distance, Data[Data.Count - 1].Value);
            // Add Attacks
            if (battler_1.actor.added_attacks.Count > 0)
                foreach (int attack in battler_1.actor.added_attacks)
                    Add_Attack.Add(attack == 1 ? 1 : 2);
            battler_1.actor.clear_added_attacks();

            Exp_Gain1 = Math.Max(0, Exp_Gain1);
            int uses = battler_1.weapon_use_count(Data[Data.Count - 1].Key.Result.hit);
            add_weapon_uses(ref Weapon_1_Uses, uses, this.weapon1);
        }
        protected void add_solo_attack(Game_Unit battler_1, int attacker)
        {
            battler_1.start_attack(Data.Count(x => x.Key.Attacker == attacker), null);
            add_data(battler_1);
            Data[Data.Count - 1].Key.Attacker = attacker;
            // Checks if this is the first attack of this unit
            for (int i = 0; i < Data.Count - 1; i++)
            {
                if (Data[i].Key.Attacker == Data[Data.Count - 1].Key.Attacker)
                {
                    Data[Data.Count - 1].Key.First_Attack = false;
                    break;
                }
            }
            if (attacker == 1)
                Attacked_1 = true;
            else
                Attacked_2 = true;
            Data[Data.Count - 1].Key.set_attack(Distance, Data[Data.Count - 1].Value);
            // Add Attacks
            if (battler_1.actor.added_attacks.Count > 0)
                foreach (int attack in battler_1.actor.added_attacks)
                    Add_Attack.Add(attack == 1 ? 1 : 2);
            battler_1.actor.clear_added_attacks();

            Exp_Gain1 = Math.Max(0, Exp_Gain1);
            Full_Exp1 = true;
            int uses = battler_1.weapon_use_count(Data[Data.Count - 1].Key.Result.hit);
            add_weapon_uses(ref Weapon_1_Uses, uses, this.weapon1);
            Wexp1 += Data[Data.Count - 1].Key.Result.wexp;
            Data[Data.Count - 1].Key.cause_damage();
        }

        protected virtual void exp_gain_test(
            Game_Unit battler_1, Game_Unit battler_2,
            int attacker,
            KeyValuePair<Combat_Round_Data, List<Combat_Action_Data>> data)
        {
            if (attacker == 1)
            {
                Exp_Gain1 = Math.Max(0, Exp_Gain1);
                int uses = battler_1.weapon_use_count(data.Key.Result.hit);
                add_weapon_uses(ref Weapon_1_Uses, uses, this.weapon1);
                Wexp1 += data.Key.Result.wexp;
                if (data.Key.is_successful_hit(this.weapon1))
                    Full_Exp1 = true;
            }
            else
            {
                Exp_Gain2 = Math.Max(0, Exp_Gain2);
                int uses = battler_2.weapon_use_count(data.Key.Result.hit);
                add_weapon_uses(ref Weapon_2_Uses, uses, this.weapon2);
                Wexp2 += data.Key.Result.wexp;
                if (data.Key.is_successful_hit(this.weapon2))
                    Full_Exp2 = true;
            }
        }

        protected virtual void add_weapon_uses(ref int totalUses, int uses, Data_Weapon weapon)
        {
            if (weapon.Hits_All_in_Range())
                totalUses = Math.Max(totalUses, uses);
            else
                totalUses += uses;
        }

        protected virtual void add_data(Game_Unit battler_1)
        {
            Data.Add(new KeyValuePair<Combat_Round_Data, List<Combat_Action_Data>>(
                new Combat_Round_Data(battler_1, null, Distance), new List<Combat_Action_Data>()));
        }
        protected virtual void add_data(Game_Unit battler_1, Game_Unit battler_2)
        {
            Data.Add(new KeyValuePair<Combat_Round_Data, List<Combat_Action_Data>>(
                new Combat_Round_Data(battler_1, battler_2, Distance), new List<Combat_Action_Data>()));
        }
        protected virtual void add_data(Game_Unit battler_1, Combat_Map_Object battler_2)
        {
            Data.Add(new KeyValuePair<Combat_Round_Data, List<Combat_Action_Data>>(
                new Terrain_Round_Data(battler_1, battler_2, Distance), new List<Combat_Action_Data>()));
        }

        protected virtual void end_battle()
        {
            Data[Data.Count - 1].Key.end_battle(Distance, Data[Data.Count - 1].Value);
        }
        #endregion

        protected bool attackable()
        {
            return attackable(Global.game_map.units[Battler_1_Id]);
        }
        protected bool attackable(Game_Unit battler_1)
        {
            return attackable(battler_1, Battler_2_Id == null ? null : Global.game_map.attackable_map_object((int)Battler_2_Id));
        }
        protected virtual bool attackable(Game_Unit attacker, Combat_Map_Object target)
        {
            // If target doesn't exist because it's a flare use/etc
            if (target == null)
                return true;

            // If attacker is dead return
            if (attacker.is_dead)
                return false;

            bool is_target_unit = false;
            // If the target is already dead, stop attacking
            if (target.hp <= 0)
                // Don't return if either battler wants the attacks to continue
                if (!attacker.continue_attacking() &&
                    !(target.is_unit() && (target as Game_Unit).continue_attacking()))
                return false;
            Game_Unit target_unit = null;
            if (target.is_unit())
            {
                is_target_unit = true;
                target_unit = (Game_Unit)target;
            }

            var weapon = attacker.id == Battler_1_Id ? this.weapon1 : this.weapon2;
            if (weapon == null)
                return false;

            // If target is berserk ally and attacker isn't berserk, don't hurt your friend >:
            if (is_target_unit && attacker.id != Battler_1_Id &&
                    !attacker.is_attackable_team(target_unit) && !can_counter_ally(attacker))
                return false;

            int hit, uses;
            if (attacker.id == Battler_1_Id)
            {
                List<int?> ary = Combat.combat_stats(attacker.id, target.id, Distance);
                hit = 100;
                uses = Weapon_1_Uses;
                if (ary[0] == null && !weapon.is_staff())
                    return false; // This is newly added to account for the attacker being put to sleep, did any other cases make hit null?
                if (ary[0] != null)
                    hit = (int)ary[0];
            }
            else
            {
                List<int?> ary = Combat.combat_stats(target.id, attacker.id, Distance);
                if (ary[4] == null)
                    return false;
                hit = (int)ary[4];
                uses = Weapon_2_Uses;
            }

            // Breaks if the attacker can't fight/broke their weapon
            if (hit < 0 || attacker.is_weapon_broke(uses))
                return false;

            return true;
        }

        protected virtual bool can_counter_ally(Game_Unit unit)
        {
            if (Global.game_system.In_Arena)
                return true;
            if (unit.berserk)
                return true;
            return false;
        }

        protected bool counter_doubling_disabled()
        {
            if (Global.game_system.In_Arena)
                return false;
            return true;
        }

        /// <summary>
        /// Applies the effects of this round of combat, such as damage taken, weapon uses, exp gain, etc.
        /// </summary>
        /// <param name="immediate_level">Immediately applies level ups from exp gain, instead of letting the combat routine detect a level up is needed and display it.
        /// Used for battles during skipped AI turns.</param>
        /// <param name="skip_exp">Skips exp and wexp gain for this round. Used by the arena.</param>
        public virtual void apply_combat(bool immediate_level = false, bool skip_exp = false)
        {
            // Prevents data from being applied multiple times
            if (Applied)
                return;
            Applied = true;
            
            Game_Unit battler_1 = Global.game_map.units[Battler_1_Id];
            Game_Unit battler_2 = Battler_2_Id == null ? null : Global.game_map.units[(int)Battler_2_Id];
            int battler_1_hp = battler_1.hp;
            int battler_2_hp = battler_2 != null ? battler_2.hp : -1;

            for(int i = 0; i < Data.Count; i++)
            {
                Combat_Round_Data data = Data[i].Key;
                data.cause_damage();
            }

            // shouldn't really use 'this is' //Yeti
            Global.game_state.add_combat_metric(
                this is Staff_Data ?
                    Metrics.CombatTypes.Staff :
                    Metrics.CombatTypes.Battle,
                Battler_1_Id, Battler_2_Id, battler_1_hp, battler_2_hp, Weapon_1_Id, Weapon_2_Id,
                Data.Where(x => x.Key.Attacker == 1).Count(), Data.Where(x => x.Key.Attacker != 1).Count());

            // Charge Masteries
            if (Attacked_1 && battler_2 != null && battler_2.different_team(battler_1))
                battler_1.charge_masteries(Game_Unit.MASTERY_RATE_BATTLE_END);
            if (Attacked_2 && battler_2 != null && battler_2.different_team(battler_1))
                battler_2.charge_masteries(Game_Unit.MASTERY_RATE_BATTLE_END);

            use_weapons(battler_1, battler_2);

            battler_1.actor.clear_added_attacks();
            if (battler_2 != null)
                battler_2.actor.clear_added_attacks();

            if (skip_exp)
            {
                //Exp_Gain1 = 0; //Debug
                //Exp_Gain2 = 0;
            }
            else
            {
                exp_gain(battler_1.actor, Exp_Gain1);
                if (battler_2 != null)
                    exp_gain(battler_2.actor, Exp_Gain2);

                // Increase exp amounts given out from each unit
                if (Exp_Gain2 > 0 && exp_gain_cap(battler_1))
                    battler_1.add_exp_given(Exp_Gain2);
                if (battler_2 != null && Exp_Gain1 > 0 && exp_gain_cap(battler_2))
                    battler_2.add_exp_given(Exp_Gain1);

                if (battler_1.is_player_team)
                    if (Global.game_state.is_battle_map && !Global.game_system.In_Arena)
                        Global.game_system.chapter_exp_gain += Exp_Gain1;
                if (battler_2 != null && battler_2.is_player_team)
                    if (Global.game_state.is_battle_map && !Global.game_system.In_Arena)
                        Global.game_system.chapter_exp_gain += Exp_Gain2;
            }
            if (!skip_exp)
            {
                if (Wexp1 > 0)
                {
                    WeaponType type = battler_1.actor.valid_weapon_type_of(Global.data_weapons[Weapon_1_Id]);
                    battler_1.actor.wexp_gain(type, Wexp1);
                }
                if (Wexp2 > 0)
                {
                    WeaponType type = battler_2.actor.valid_weapon_type_of(Global.data_weapons[(int)Weapon_2_Id]);
                    battler_2.actor.wexp_gain(type, Wexp2);
                }
            }
            if (immediate_level)
            {
                if (battler_1.actor.needed_levels > 0)
                    battler_1.actor.level_up();
                battler_1.actor.clear_wlvl_up();
                if (battler_2 != null)
                {
                    if (battler_2.actor.needed_levels > 0)
                        battler_2.actor.level_up();
                    battler_2.actor.clear_wlvl_up();
                }
            }
        }

        protected void use_weapons(Game_Unit battler_1, Game_Unit battler_2)
        {
            // Weapon Use
            for (int i = 0; i < Weapon_1_Uses; i++)
            {
                battler_1.weapon_use();
            }
            for (int i = 0; i < Weapon_2_Uses; i++)
            {
                battler_2.weapon_use();
            }

            if (battler_1.using_siege_engine)
            {
                // Put siege engine in reload state
                if (Constants.Gameplay.SIEGE_RELOADING)
                    Global.game_map.get_siege(battler_1.loc).fire();
            }
            else if (battler_1.is_weapon_broke())
                battler_1.actor.discard_weapon();

            if (battler_2 != null)
            {
                if (battler_2.using_siege_engine)
                {
                    // Put siege engine in reload state
                    if (Constants.Gameplay.SIEGE_RELOADING)
                        Global.game_map.get_siege(battler_2.loc).fire();
                }
                else if (battler_2.is_weapon_broke())
                    battler_2.actor.discard_weapon();
            }

            fix_unusable_items(battler_1, battler_2);
        }

        protected virtual void fix_unusable_items(Game_Unit battler_1, Game_Unit battler_2)
        {
            if (battler_1.actor.weapon != null &&
                    !battler_1.actor.is_equippable_as_siege(battler_1.actor.weapon))
                battler_1.actor.setup_items(false);
            if (battler_2 != null)
            {
                if (battler_2.actor.weapon != null &&
                        !battler_2.actor.is_equippable_as_siege(battler_2.actor.weapon))
                    battler_2.actor.setup_items(false);
            }
        }

        internal static void exp_gain(Game_Actor actor, int gain)
        {
            if (actor != null)
                while (gain != 0)
                {
                    if (gain > 0)
                    {
                        actor.exp++;
                        if (actor.can_level())
                            gain--;
                        else
                            gain = 0;
                    }
                    else if (gain > 0)
                    {
                        actor.exp--;
                        gain++;
                    }
                }
        }

        public virtual bool is_ally_killed
        {
            get
            {
                if (Kill != 0)
                {
                    int id = Kill == 1 ? Battler_1_Id : (int)Battler_2_Id;
                    if (!Global.game_map.units.ContainsKey(id))
                        return false;
                    if ((Global.game_map.units[id]).is_ally)
                        return true;
                }
                return false;
            }
        }

        public virtual bool game_over
        {
            get
            {
                if (Kill != 0)
                {
                    int id = Kill == 1 ? Battler_1_Id : (int)Battler_2_Id;
                    if (!Global.game_map.units.ContainsKey(id))
                        return false;
                    if ((Global.game_map.units[id]).loss_on_death)
                        return true;
                }
                return false;
            }
        }

        public int surviving_unit_id
        {
            get
            {
                if (Kill == 1 && Battler_2_Id != null)
                    return (int)Battler_2_Id;
                if (Kill == 2)
                    return Battler_1_Id;
                return -1;
            }
        }

        public virtual bool has_death_quote
        {
            get
            {
                if (Kill != 0)
                {
                    int id = Kill == 1 ? Battler_1_Id : (int)Battler_2_Id;
                    if (!Global.game_map.units.ContainsKey(id))
                        return false;
                    if (Global.game_state.get_death_quote(id).Length > 0)
                        return true;
                }
                return false;
            }
        }

        public virtual bool has_item_drop
        {
            get
            {
                if (Kill != 0)
                {
                    // Id of the killed unit
                    int id = Kill == 1 ? Battler_1_Id : (int)Battler_2_Id;
                    if (!Global.game_map.units.ContainsKey(id))
                        return false;
                    if (Global.game_map.units[id].drops_item &&
                            Global.game_map.units[id].actor.has_items &&
                            Global.game_map.units[Kill == 1 ?
                                (int)Battler_2_Id : Battler_1_Id].can_acquire_drops)
                        return true;
                }
                return false;
            }
        }

        public bool has_promotion
        {
            get
            {
                if (Exp_Gain1 > 0)
                {
                    if (Global.game_map.units[Battler_1_Id].actor.would_promote(Exp_Gain1))
                        return true;
                }
                else if (Exp_Gain2 > 0)
                {
                    if (Global.game_map.units[(int)Battler_2_Id].actor.would_promote(Exp_Gain2))
                        return true;
                }
                return false;
            }
        }
    }
}
