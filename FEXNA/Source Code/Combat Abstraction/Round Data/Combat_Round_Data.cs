using System;
using System.Collections.Generic;
using System.Linq;

namespace FEXNA
{
    internal class Combat_Round_Data
    {
        public int Attacker;
        public List<int?> Stats;
        public Attack_Result Result;
        public bool First_Attack = true;
        protected Game_Unit Battler1, Battler2;

        protected Game_Unit AttackerUnit { get { return Attacker == 1 ? Battler1 : Battler2; } }
        protected Game_Unit TargetUnit { get { return Attacker == 2 ? Battler1 : Battler2; } }

        protected Combat_Round_Data() { }
        internal Combat_Round_Data(Game_Unit battler_1, Game_Unit battler_2, int distance)
        {
            Battler1 = battler_1;
            Battler2 = battler_2;

            List<int?> ary = combat_stats(distance);
            Stats = ary;
        }
        internal Combat_Round_Data(Game_Unit battler_1, Game_Unit battler_2, Scripted_Combat_Stats stats)
        {
            Battler1 = battler_1;
            Battler2 = battler_2;

            Stats = new List<int?>();
            for (int i = 0; i < 4; i++)
                Stats.Add(stats.Stats_1.Count < i + 1 ? null : (int?)stats.Stats_1[i]);
            for (int i = 0; i < 4; i++)
                Stats.Add(stats.Stats_2.Count < i + 1 ? null : (int?)stats.Stats_2[i]);
        }

        protected virtual List<int?> combat_stats(int distance)
        {
            return Combat.combat_stats(Battler1.id, Battler2 == null ? null : (int?)Battler2.id, distance);
        }

        internal void set_attack(int distance, List<Combat_Action_Data> action_data)
        {
            Game_Unit battler = Attacker == 1 ? Battler1 : Battler2;
            Game_Unit target = Attacker == 2 ? Battler1 : Battler2;
            // In case of skills like Astra that turn skill_activated back on at the end of reset_skills()
            battler.skill_activated = false;
            if (target != null)
                target.skill_activated = false;

            List<int?> combat_stats;
            int hit = 100, dmg = 0;
            bool hit_skill_changed = false;
            if (target != null)
            {
                combat_stats = Combat.combat_stats(battler.id, target.id, distance);
                if (combat_stats[0] != null)
                    hit = (int)combat_stats[0];
                if (combat_stats[1] != null)
                    dmg = (int)combat_stats[1];
                hit_skill_changed = false;
            }


            FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
            // Pre hit skill check, Defender (Bastion)
            prehit_def_skill_check(distance, action_data, hit, ref hit_skill_changed);
            // Pre hit skill check, Attcker (Spiral Dive?)
            prehit_skill_check(distance, action_data, hit, ref hit_skill_changed);

            bool is_hit = true, is_crt = false;
            if (target != null)
            {
                combat_stats = Combat.combat_stats(battler.id, target.id, distance);
                // Hit hasn't changed
                hit = attack_hit(hit, combat_stats[0] ?? hit, hit_skill_changed);
                int crt = combat_stats[2] ?? -1;
                // Hit test
                is_hit = true;
                is_crt = false;

                if (Stats[Attacker == 1 ? 0 : 4] != null)
                    Combat.test_hit(hit, crt, distance, out is_hit, out is_crt);

#if DEBUG
                //Cheat codes
                if (
                    false &&
                    Global.scene.scene_type != "Scene_Test_Battle" &&
                    !(this is Staff_Round_Data))
                {
                    // Only for PCs
                    if (true)
                    {
                        is_hit = battler.is_player_team ||
                            (is_hit && (!target.is_ally ||
                                combat_stats[1] * (is_crt ? Constants.Combat.CRIT_MULT : 1) < target.hp)); //Debug
                        if (battler.is_player_team)
                            is_crt = Global.game_system.roll_rng(50);
                    }
                    // For NPCs also
                    else
                    {
                        is_hit = battler.is_player_allied ||
                            (is_hit && (!target.is_ally || combat_stats[1] < target.hp)); //Debug
                        is_crt = battler.is_player_allied || !is_hit;
                        is_crt = is_crt && Global.game_system.roll_rng(50);//!battler.is_attackable_team(Config.PLAYER_TEAM); //Debug
                    }

                    //is_hit = !battler.is_attackable_team(Config.PLAYER_TEAM); //Debug
                    //is_crt = !battler.is_attackable_team(Config.PLAYER_TEAM);
                    //is_crt = Global.game_system.roll_rng(50);//!battler.is_attackable_team(Config.PLAYER_TEAM); //Debug
                }
#endif
            }
            // Post hit skill activation (Astra)
            hit_skill_check(distance, action_data, is_hit, is_crt);

            battler.hit_skill_update();
            // On hit skill activation (Determination) //Yeti
            onhit_skill_check(distance, action_data, is_hit);

            // Calculate attack
            if (target != null)
            {
                combat_stats = Combat.combat_stats(battler.id, target.id, distance);
                if (combat_stats[1] != null)
                    dmg = attack_dmg(dmg, (int)combat_stats[1]);
            }

            Result = calculate_attack(distance, dmg, is_hit, is_crt, weapon);

            if (Battler2 != null)
                cause_damage(true);
            // Attack end skill activation (Adept) //Yeti
            posthit_skill_check(distance, action_data);
            // Reset skills
            if (!skip_skill_update())
            {
                battler.reset_skills();
                if (target != null)
                    target.reset_skills();
            }
        }
        internal void set_attack(int distance, List<Combat_Action_Data> action_data, Scripted_Combat_Stats stats)
        {
            Game_Unit battler = Attacker == 1 ? Battler1 : Battler2;
            Game_Unit target = Attacker == 2 ? Battler1 : Battler2;
            // In case of skills like Astra that turn skill_activated back on at the end of reset_skills()
            battler.skill_activated = false;
            if (target != null)
                target.skill_activated = false;

            FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
            // Pre hit skill check, Defender (Bastion)
            //prehit_def_skill_check(distance, action_data, hit, ref hit_skill_changed); //Yeti
            // Pre hit skill check, Attcker (Spiral Dive?)
            //prehit_skill_check(distance, action_data, hit, ref hit_skill_changed); //Yeti

            // Hit test
            //bool hit = stats.Result != Attack_Results.Miss; //Debug
            //bool crt = hit && stats.Result == Attack_Results.Crit;

            // Post hit skill activation (Astra)
            //hit_skill_check(distance, action_data, is_hit, is_crt); //Yeti

            battler.hit_skill_update();
            // On hit skill activation (Determination) //Yeti
            //onhit_skill_check(distance, action_data, is_hit); //Yeti

            // Calculate attack
            Result = calculate_attack(distance, weapon, stats);

            if (Battler2 != null)
                cause_damage(true);
            // Attack end skill activation (Adept) //Yeti
            //posthit_skill_check(distance, action_data); //Yeti
            // Reset skills
            if (!skip_skill_update())
            {
                battler.reset_skills();
                if (target != null)
                    target.reset_skills();
            }
        }

        protected virtual void prehit_def_skill_check(int distance, List<Combat_Action_Data> action_data, int hit, ref bool hit_skill_changed)
        {
            if (!skip_skill_update() && this.TargetUnit != null)
            {
                var attacker = this.AttackerUnit;
                var target = this.TargetUnit;

                target.prehit_def_skill_check(attacker);
                if (target.skill_activated)
                {
                    action_data.Add(new Combat_Action_Data
                    {
                        Battler_Index = 2,
                        Trigger = (int)Combat_Action_Triggers.Attack,
                        Skill_Ids = target.hit_skill_ids(),
                        combat_stats = this.combat_stats(distance)
                    });
                    if (hit != (int)(Attacker == 1 ? action_data.Last().Hit1 : action_data.Last().Hit2))
                        hit_skill_changed = true;
                    // Turns skills activated at all back off so further checks can be accurate
                    target.skill_activated = false;
                }
            }
        }
        protected virtual void prehit_skill_check(int distance, List<Combat_Action_Data> action_data, int hit, ref bool hit_skill_changed)
        {
            if (!skip_skill_update())
            {
                var attacker = this.AttackerUnit;
                var target = this.TargetUnit;

                if (First_Attack)
                    attacker.activate_masteries();
                attacker.prehit_skill_check(target, distance);
                if (attacker.skill_activated)
                {
                    action_data.Add(new Combat_Action_Data
                    {
                        Battler_Index = 1,
                        Trigger = (int)Combat_Action_Triggers.Attack,
                        Skill_Ids = attacker.hit_skill_ids(),
                        combat_stats = this.combat_stats(distance)
                    });
                    if (hit != (int)(Attacker == 1 ? action_data.Last().Hit1 : action_data.Last().Hit2))
                        hit_skill_changed = true;
                    // Turns skills activated at all back off so further checks can be accurate
                    attacker.skill_activated = false;
                }
            }
        }
        protected virtual void hit_skill_check(int distance, List<Combat_Action_Data> action_data, bool is_hit, bool is_crt)
        {
            if (!skip_skill_update())
            {
                var attacker = this.AttackerUnit;
                var target = this.TargetUnit;

                if (First_Attack)
                    attacker.mastery_hit_confirm(is_hit);
                attacker.hit_skill_check(is_hit, is_crt, target, distance);
                if (attacker.skill_activated)
                {
                    action_data.Add(new Combat_Action_Data
                    {
                        Battler_Index = 1,
                        Trigger = (int)Combat_Action_Triggers.Skill,
                        Skill_Ids = attacker.hit_skill_ids(),
                        combat_stats = this.combat_stats(distance)
                    });
                    // Turns 'skills activated at all' back off so further checks can be accurate
                    attacker.skill_activated = false;
                }
            }
        }
        protected virtual void onhit_skill_check(int distance, List<Combat_Action_Data> action_data, bool is_hit)
        {
            if (this.TargetUnit != null && !skip_skill_update()) //yeti
            {
                var attacker = this.AttackerUnit;
                var target = this.TargetUnit;

                target.onhit_skill_check(is_hit, target, distance);
                if (target.skill_activated)
                {
                    action_data.Add(new Combat_Action_Data
                    {
                        Battler_Index = 2,
                        Trigger = (int)Combat_Action_Triggers.Hit,
                        Skill_Ids = target.onhit_skill_ids(),
                        combat_stats = this.combat_stats(distance)
                    });
                    // Turns skills activated at all back off so further checks can be accurate
                    target.skill_activated = false;
                }
            }
        }
        protected virtual void posthit_skill_check(int distance, List<Combat_Action_Data> action_data)
        {
            if (!skip_skill_update())
            {
                var attacker = this.AttackerUnit;
                var target = this.TargetUnit;

                attacker.posthit_skill_check(target);
                if (attacker.skill_activated)
                {
                    action_data.Add(new Combat_Action_Data
                    {
                        Battler_Index = 1,
                        Trigger = (int)Combat_Action_Triggers.Return,
                        Skill_Ids = attacker.posthit_skill_ids(),
                        combat_stats = this.combat_stats(distance)
                    });
                    // Turns skills activated at all back off so further checks can be accurate
                    attacker.skill_activated = false;
                }
            }
        }

        protected virtual int attack_hit(int original_hit, int current_hit, bool hit_skill_changed)
        {
            return current_hit;
        }
        protected virtual int attack_dmg(int original_dmg, int current_dmg)
        {
            return current_dmg;
        }

        protected virtual Attack_Result calculate_attack(int distance, int dmg, bool hit, bool crt,
            FEXNA_Library.Data_Weapon weapon)
        {
            return Combat.set_attack(this.AttackerUnit, this.TargetUnit, distance, dmg, hit, crt, weapon);
        }
        private Attack_Result calculate_attack(int distance,
            FEXNA_Library.Data_Weapon weapon, Scripted_Combat_Stats stats)
        {
            return Combat.set_attack(this.AttackerUnit, this.TargetUnit, distance, weapon, stats);
        }

        public virtual void end_battle(int distance, List<Combat_Action_Data> action_data)
        {
            action_data.Add(new Combat_Action_Data
            {
                Battler_Index = 0,
                Trigger = (int)Combat_Action_Triggers.End,
                combat_stats = this.combat_stats(distance)
            });
        }
        internal void end_battle(Scripted_Combat_Stats stats, List<Combat_Action_Data> action_data)
        {
            List<int?> list = new List<int?>();
            for (int i = 0; i < 4; i++)
                list.Add(stats.Stats_1.Count < i + 1 ? null : (int?)stats.Stats_1[i]);
            for (int i = 0; i < 4; i++)
                list.Add(stats.Stats_2.Count < i + 1 ? null : (int?)stats.Stats_2[i]);
            action_data.Add(new Combat_Action_Data
            {
                Battler_Index = 0,
                Trigger = (int)Combat_Action_Triggers.End,
                combat_stats = list
            });
        }

        protected bool skip_skill_update()
        {
            return (Battler1.skip_skill_update() || (Battler2 != null && Battler2.skip_skill_update()));
        }

        public virtual bool is_successful_hit(FEXNA_Library.Data_Weapon weapon)
        {
            // If the attacker didn't hit themself, returns true when damage was successfully caused/the opponent was slain
            return !Result.backfire && (Result.dmg > 0 || Result.kill);

            if (!Result.backfire) //Debug
            {
                // Technically checks if attacks miss, since missed attacks do no damage
                return (Result.dmg > 0 || Result.kill); //Debug
                if (Attacker == 1 && Stats[1] > 0)
                {
                    return true;
                }
                else if (Attacker == 2 && Stats[5] > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void cause_damage(bool test = false)
        {
            if (Battler2 == null)
            {

            }
            else
                cause_damage(this.AttackerUnit, this.TargetUnit, test);
        }
        protected void cause_damage(Combat_Map_Object attacker, Combat_Map_Object target, bool test)
        {
            // If the attack didn't backfire, cause damage as normal then apply on-hit healing here
            if (!Result.backfire)
            {
                target.combat_damage(Result.dmg, attacker, Result.state_change, Result.backfire, test);
                attacker.hp += Result.immediate_life_steal;
            }
            // Else it backfired, and damage is caused
            else
                attacker.combat_damage(Result.dmg - Result.immediate_life_steal, target, Result.state_change, Result.backfire, test);

            // Then apply is delayed life gain, such as from Resire or Nosferatu
            if (Result.delayed_life_steal && !attacker.is_dead)
            {
                attacker.hp += Result.delayed_life_steal_amount;
            }
        }

        public void cause_status()
        {
            if (!Result.backfire)
            {
                this.TargetUnit.state_change(Result.state_change);
            }
            else
                this.AttackerUnit.state_change(Result.state_change);
        }

        public Game_Unit hit_unit()
        {
            if (!Result.backfire)
                return this.TargetUnit;
            else
                return this.AttackerUnit;
        }
    }
}
