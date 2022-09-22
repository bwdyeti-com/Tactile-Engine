using System;
using Microsoft.Xna.Framework;
using TactileLibrary;
using TactileWeaponExtension;

namespace Tactile.Calculations.Stats
{
    class CombatStats : BattlerStats
    {
        private int TargetId = -1;

        private Combat_Map_Object target
        {
            get { return Global.game_map.attackable_map_object(TargetId); }
        }

        internal CombatStats(
                int attackerId,
                int targetId,
                int itemIndex = -1,
                Maybe<int> distance = default(Maybe<int>))
            : base(attackerId, weapon_id(attackerId, itemIndex),
                get_distance(attackerId, targetId, distance))
        {
            TargetId = targetId;
        }
        internal CombatStats(
                int attackerId,
                int targetId,
                Data_Weapon weapon,
                Maybe<int> distance = default(Maybe<int>))
            : base(attackerId, weapon,
                get_distance(attackerId, targetId, distance))
        {
            TargetId = targetId;
        }

        private static Data_Weapon weapon_id(int attackerId, int itemIndex)
        {
            var unit = get_unit(attackerId);
            if (itemIndex == -1)
                return unit.actor.weapon;
            return unit.items[itemIndex].to_weapon;
        }
        private static int get_distance(int attackerId, int targetId, Maybe<int> distance)
        {
            if (distance.IsSomething)
                return distance;
            return Global.game_map.combat_distance(attackerId, targetId);
        }

        protected virtual BattlerStats target_stats()
        {
            var result = new CombatStats(target.id, attacker.id, distance: Distance)
            {
                location_bonuses = inverse_location_bonus
            };
            return result;
        }

        internal WeaponTriangle Tri(Game_Unit target)
        {
            if (target == null)
                return WeaponTriangle.Nothing;
            return Tri(target, this.attacker_weapon, target.actor.weapon);
        }
        internal WeaponTriangle Tri(Game_Unit target, Data_Weapon attackerWeapon, Data_Weapon targetWeapon)
        {
            return Combat.weapon_triangle(attacker, target, attackerWeapon, targetWeapon, Distance);
        }

        #region Dmg
        internal override int dmg()
        {
            if (!(this.target is Game_Unit))
                throw new NotImplementedException();
            var target = this.target as Game_Unit;

            if (!has_weapon)
                return 0;
            int unit_weapon_id = attacker.actor.weapon_id;
            attacker.actor.weapon_id = this.attacker_weapon.Id;

            int result = dmg_target(target);

            attacker.actor.weapon_id = unit_weapon_id;
            return result;
        }

        private int dmg_target(Game_Unit target)
        {
            Data_Weapon weapon = this.attacker_weapon;

            // Checks if weapon is magic
            bool magic_attack = attacker.check_magic_attack(weapon, Distance);
            bool imbue = (magic_attack && attacker.actor.power_type() == Power_Types.Strength);
            bool is_staff = weapon.is_staff();
            Game_Actor actor2 = target.actor;
            Data_Weapon weapon2 = actor2.weapon;
            int total_damage, target_def;
            int actor_dmg, weapon_dmg, skill_dmg, support_dmg;
            // Staff
            if (is_staff)
            {
                actor_dmg = attacker.atk_pow(weapon, magic_attack);
                weapon_dmg = weapon.Mgt;
                skill_dmg = 0;
                attacker.dmg_staff_skill(
                    ref skill_dmg, ref weapon_dmg, ref actor_dmg,
                    target, magic_attack, Distance);
                support_dmg = support_bonus(Combat_Stat_Labels.Dmg);
                target_def = 0;
            }
            // Weapon
            else
            {
                var target_stats = this.target_stats();
                target_def = magic_attack ?
                    target_stats.res() : target_stats.def();
                target_def = (weapon.Ignores_Def() ? target_def / 2 : target_def);
                if (weapon.Halves_HP())
                {
                    int total_weapon_damage = target_def + (int)Math.Ceiling(target.actor.hp / 2.0f);
                    actor_dmg = (int)Math.Ceiling(total_weapon_damage / 2.0f);
                    weapon_dmg = (int)Math.Floor(total_weapon_damage / 2.0f);
                }
                else
                {
                    actor_dmg = weapon.Ignores_Pow() ?
                        0 : ((int)(attacker.atk_pow(weapon, magic_attack) *
                            (imbue ? Constants.Combat.MAGIC_WEAPON_STR_RATE : 1)));
                    weapon_dmg = (int)(weapon.Mgt * (imbue ? Constants.Combat.MAGIC_WEAPON_MGT_RATE : 1));
                    if (weapon.Ignores_Def())
                        weapon_dmg += target_def;
                }
                // Weapon triangle
                skill_dmg = 0;
                WeaponTriangle tri = Tri(target, weapon, weapon2);
                if (tri != WeaponTriangle.Nothing)
                    skill_dmg += Weapon_Triangle.DMG_BONUS * (tri == WeaponTriangle.Advantage ? 1 : -1) *
                        Combat.weapon_triangle_mult(attacker, target, weapon, weapon2, Distance);

                float effectiveness = weapon.effective_multiplier(attacker, target);
                skill_dmg += (int)((weapon_dmg + skill_dmg) * (effectiveness - 1));
                support_dmg = support_bonus(Combat_Stat_Labels.Dmg);
                attacker.dmg_skill(
                    ref skill_dmg, ref weapon_dmg, ref actor_dmg, ref support_dmg,
                    ref target_def, weapon, target, weapon2, tri, magic_attack,
                    Distance, effectiveness);
            }
            total_damage = actor_dmg + weapon_dmg + skill_dmg + support_dmg - target_def;
            int result = attacker.dmg_target_skill(target, weapon, Distance, Math.Max(0, total_damage));
            return result;
        }

        internal int dmg_per_round()
        {
            int hits_per_round = attacker.attacks_per_round(
                target, Distance, attacker_weapon);
            hits_per_round *= attacker_weapon.HitsPerAttack;

            return dmg() * hits_per_round;
        }
        internal float avg_dmg_per_round()
        {
            float dmg = this.dmg();
            float hit = Combat.true_hit(this.hit()) / 100f;
            float crt = this.crt() / 100f;

            // Apply crit
            dmg = MathHelper.Lerp(dmg, dmg * Constants.Combat.CRIT_MULT, crt);
            dmg *= hit;

            int hits_per_round = attacker.attacks_per_round(
                target, Distance, attacker_weapon);
            hits_per_round *= attacker_weapon.HitsPerAttack;

            return dmg * hits_per_round;
        }

        internal float hits_to_kill()
        {
            float dmg = this.dmg();
            if (dmg == 0)
                return 0;
            float hit = Combat.true_hit(this.hit()) / 100f;
            float crt = this.crt() / 100f;
            if (hit == 0)
                return 0;

            int hits_to_kill = (int)Math.Ceiling(target.maxhp / dmg);
            float attacks_to_kill = hits_to_kill / hit;

            float result = MathHelper.Lerp(attacks_to_kill,
                attacks_to_kill / Constants.Combat.CRIT_MULT, crt);
            float simplified_hit_result = target.maxhp / (dmg * hit);

            // If simplified result suggests less hits are needed
            if (Math.Ceiling(simplified_hit_result) < Math.Ceiling(result) &&
                    (result - simplified_hit_result) >= 0.5f)
                return (simplified_hit_result + result) / 2f;
                //return result; //Debug
#if DEBUG
            if (result < simplified_hit_result)
            {
                return result;
            }
#endif
            return Math.Min(result, simplified_hit_result);

#if DEBUG
            // Odds of each outcome of each attack
            float miss = 1f - hit;
            crt = crt * hit;
            hit -= crt;

            float total_odds = miss + crt + hit;
            System.Diagnostics.Debug.Assert(Math.Abs(total_odds - 1f) < 0.001f);
#endif
        }
        internal float rounds_to_kill()
        {
            int hits_per_round = attacker.attacks_per_round(
                target, Distance, attacker_weapon);
            hits_per_round *= attacker_weapon.HitsPerAttack;

            return hits_to_kill() / hits_per_round;
        }
        internal float inverse_rounds_to_kill()
        {
            float result = rounds_to_kill();
            if (result == 0)
                return 0;
            return 1 / result;
        }
        #endregion
        
        #region Hit
        internal override int hit()
        {
            if (!(this.target is Game_Unit))
                throw new NotImplementedException();
            var target = this.target as Game_Unit;

            if (!has_weapon)
                return 0;
            int unit_weapon_id = attacker.actor.weapon_id;
            attacker.actor.weapon_id = this.attacker_weapon.Id;

            int result = hit_target(target);

            attacker.actor.weapon_id = unit_weapon_id;
            return result;
        }

        private int hit_target(Game_Unit target)
        {
            Data_Weapon weapon = this.attacker_weapon;

            // Checks if weapon is magic
            bool magic_attack = attacker.check_magic_attack(weapon, Distance);
            bool is_staff = weapon.is_staff();
            Game_Actor actor2 = target.actor;
            Data_Weapon weapon2 = actor2.weapon;
            bool boss_staff = false;
            int actor_hit, weapon_hit, skill_hit, support_hit, s_bonus;
            int hit_rate, target_avo;
            if (is_staff)
            {
                actor_hit = attacker.atk_pow(weapon, magic_attack) * 5 +
                    attacker.stat(Stat_Labels.Skl);
                weapon_hit = weapon.Hit;
                skill_hit = 0;
                attacker.hit_skill(
                    ref skill_hit, ref weapon_hit, ref actor_hit,
                    weapon, target, magic_attack, Distance);
                support_hit = support_bonus(Combat_Stat_Labels.Hit);
                s_bonus = 0;
                hit_rate = Math.Max(0, actor_hit + weapon_hit + skill_hit + support_hit + s_bonus);

                target_avo = this.target_stats().staff_avo();
                boss_staff = target.boss;
            }
            else
            {
                actor_hit = base_hit(weapon, magic_attack);

                weapon_hit = weapon.Hit;
                skill_hit = 0;
                attacker.hit_skill(
                    ref skill_hit, ref weapon_hit, ref actor_hit,
                    weapon, target, magic_attack, Distance);
                support_hit = support_bonus(Combat_Stat_Labels.Hit);
                s_bonus = attacker.actor.s_rank_bonus(weapon);
                // Weapon triangle
                WeaponTriangle tri = Tri(target, weapon, weapon2);
                if (tri != WeaponTriangle.Nothing)
                    weapon_hit += Weapon_Triangle.HIT_BONUS * (tri == WeaponTriangle.Advantage ? 1 : -1) *
                        Combat.weapon_triangle_mult(attacker, target, weapon, weapon2, Distance);

                hit_rate = Math.Max(0, actor_hit + weapon_hit + skill_hit + support_hit + s_bonus);

                target_avo = this.target_stats().avo(Combat.reverse_wta(tri));
            }
            int total_hit = hit_rate - target_avo;
            int result = attacker.hit_target_skill(
                target, weapon, Distance, Math.Max(total_hit, 0) / (boss_staff ? 2 : 1));
            return result;
        }
        #endregion

        #region Crt
        internal override int crt()
        {
            if (!(this.target is Game_Unit))
                throw new NotImplementedException();
            var target = this.target as Game_Unit;

            if (!has_weapon)
                return 0;
            int unit_weapon_id = attacker.actor.weapon_id;
            attacker.actor.weapon_id = this.attacker_weapon.Id;

            int result = crt_target(target);

            attacker.actor.weapon_id = unit_weapon_id;
            return result;
        }

        private int crt_target(Game_Unit target)
        {
            Data_Weapon weapon = this.attacker_weapon;

            // Staves can't crit
            if (weapon.is_staff())
                return 0;
            if (weapon.Crt < 0)
                return 0;

            // Checks if weapon is magic
            bool magic_attack = attacker.check_magic_attack(weapon, Distance);
            bool is_staff = weapon.is_staff();
            Game_Actor actor2 = target.actor;
            Data_Weapon weapon2 = actor2.weapon;
            int target_dod;
            int actor_crt, weapon_crt, skill_crt, support_crt, s_bonus;

            actor_crt = base_crt();
            // Bonus to crit for hit > 100
            if (Constants.Combat.HIT_OVERFLOW)
            {
                int hit_bonus = (int)((hit() - 100) *
                    Constants.Combat.HIT_OVERFLOW_RATE);
                actor_crt += Math.Max(0, hit_bonus);
            }
            weapon_crt = weapon.Crt;
            skill_crt = 0;
            attacker.crt_skill(
                ref skill_crt, ref weapon_crt, ref actor_crt,
                weapon, target, magic_attack, Distance);
            support_crt = support_bonus(Combat_Stat_Labels.Crt);
            s_bonus = attacker.actor.s_rank_bonus(weapon);

            target_dod = this.target_stats().dodge();
            int crit_rate = actor_crt + weapon_crt + skill_crt + support_crt + s_bonus;
            int total_crt = Math.Max(0, crit_rate - target_dod);
            if (magic_attack && attacker.actor.power_type() == Power_Types.Strength)
                total_crt = (int)(total_crt * Constants.Combat.MAGIC_WEAPON_CRT_RATE);
            int result = attacker.crt_target_skill(target, weapon, Distance, total_crt);
            return result;
        }
        #endregion

        #region Def/Res
        protected override Maybe<int> terrain_def_bonus()
        {
            return attacker.terrain_def_bonus(this.target as Game_Unit);
        }

        protected override Maybe<int> terrain_res_bonus()
        {
            return attacker.terrain_res_bonus(this.target as Game_Unit);
        }
        #endregion

        #region Avo
        internal override int avo(WeaponTriangle tri)
        {
            return avo(this.target as Game_Unit, tri);
        }

        internal override int staff_avo()
        {
            return staff_avo(this.target as Game_Unit);
        }
        #endregion
    }
}
