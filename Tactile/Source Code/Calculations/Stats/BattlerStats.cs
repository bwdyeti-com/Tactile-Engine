using System;
using TactileLibrary;
using EnumExtension;

namespace Tactile.Calculations.Stats
{
    [Flags]
    enum CombatLocationBonuses : byte
    {
        Undefined = 0,
        Defined = 1 << 0,
        NoAttackerBonus = 1 << 1,
        NoDefenderBonus = 1 << 2,
    }
    enum SecondaryStats { Atk, Hit, Crt, Def, Avo, Dod }
    class BattlerStats
    {
        private int AttackerId;
        private int WeaponId;
        protected int Distance;
        private CombatLocationBonuses LocationBonuses;

        internal BattlerStats(
            int attackerId,
            int weaponId = -1,
            Maybe<int> distance = default(Maybe<int>))
        {
            AttackerId = attackerId;
            WeaponId = weaponId;
            Distance = distance.IsNothing ? 1 : (int)distance;
        }
        internal BattlerStats(
            int attackerId,
            Data_Weapon weapon,
            Maybe<int> distance = default(Maybe<int>))
        {
            AttackerId = attackerId;
            WeaponId = weapon == null ? 0 : weapon.Id;
            Distance = distance.IsNothing ? 1 : (int)distance;
        }

        protected Game_Unit attacker { get { return get_unit(AttackerId); } }
        private bool using_actor_weapon { get { return WeaponId == -1; } }
        internal Data_Weapon attacker_weapon
        {
            get
            {
                if (!using_actor_weapon)
                {
                    if (WeaponId == 0)
                        return null;
                    return Global.GetWeapon(WeaponId);
                }
                else
                {
                    return attacker.actor.weapon;
                }
            }
        }
        internal bool has_weapon
        {
            get
            {
                return WeaponId != 0 && (!using_actor_weapon || attacker_weapon != null);
            }
        }
        internal bool has_non_staff_weapon
        {
            get { return has_weapon && !attacker_weapon.is_staff(); }
        }

        internal CombatLocationBonuses location_bonuses
        {
            get { return LocationBonuses; }
            set
            {
                if (!LocationBonuses.HasEnumFlag(CombatLocationBonuses.Defined))
                {
                    LocationBonuses = value;
                    LocationBonuses |= CombatLocationBonuses.Defined;
                }
            }
        }
        protected CombatLocationBonuses inverse_location_bonus
        {
            get
            {
                CombatLocationBonuses result = (CombatLocationBonuses)0;
                if (LocationBonuses.HasEnumFlag(CombatLocationBonuses.NoAttackerBonus))
                    result |= CombatLocationBonuses.NoDefenderBonus;
                if (LocationBonuses.HasEnumFlag(CombatLocationBonuses.NoDefenderBonus))
                    result |= CombatLocationBonuses.NoAttackerBonus;
                return result;
            }
        }

        /* //Debug
        internal int get_value(
            SecondaryStats stat,
            int attackerId,
            int weaponId = -1,
            int distance = 1)
        {
            throw new NotImplementedException();
            var stats = new BattlerStats(attackerId, weaponId, distance);

            switch (stat)
            {
                case SecondaryStats.Atk:
                    return stats.dmg();
                case SecondaryStats.Hit:
                    return stats.hit();
                case SecondaryStats.Crt:
                    return stats.crt();
                case SecondaryStats.Def:
                    throw new NotImplementedException();
                case SecondaryStats.Avo:
                    return stats.avo();
                case SecondaryStats.Dod:
                    return stats.dodge();
            }
            return 0;
        }*/

        protected static Game_Unit get_unit(int id)
        {
            return Global.game_map.units[id];
        }

#if DEBUG
        protected int debug_dmg { get { return dmg(); } }
        protected int debug_hit { get { return hit(); } }
        protected int debug_crt { get { return crt(); } }
#endif

        protected int support_bonus(Combat_Stat_Labels stat)
        {
            if (this.location_bonuses.HasEnumFlag(CombatLocationBonuses.NoAttackerBonus))
                return 0;
            return attacker.support_bonus(stat);
        }

        #region Dmg
        internal virtual int dmg()
        {
            if (!has_weapon)
                return 0;
            Data_Weapon weapon = this.attacker_weapon;

            // Checks if weapon is magic
            bool magic_attack = attacker.check_magic_attack(weapon, Distance);
            bool imbue = (magic_attack && attacker.actor.power_type() == Power_Types.Strength);
            bool is_staff = weapon.is_staff();
            int actor_dmg, weapon_dmg, skill_dmg, support_dmg;
            if (is_staff)
            {
                return 0; //Yeti
            }
            else
            {
                actor_dmg = weapon.Ignores_Pow() ?
                    0 : ((int)(attacker.atk_pow(weapon, magic_attack) *
                        (imbue ? Constants.Combat.MAGIC_WEAPON_STR_RATE : 1)));
                weapon_dmg = (int)(weapon.Mgt * (imbue ? Constants.Combat.MAGIC_WEAPON_MGT_RATE : 1));
                skill_dmg = 0;
                support_dmg = support_bonus(Combat_Stat_Labels.Dmg);
                attacker.dmg_skill(
                    ref skill_dmg, ref weapon_dmg, ref actor_dmg, ref support_dmg,
                    weapon, magic_attack, Distance);
            }
            return Math.Max(0, actor_dmg + weapon_dmg + skill_dmg + support_dmg);
        }
        #endregion

        #region Hit
        internal int base_hit(Data_Weapon weapon, bool magic)
        {
            return attacker.base_hit_skl(weapon, magic) +
                attacker.stat(Stat_Labels.Lck) / 2;
        }

        internal virtual int hit()
        {
            if (!has_weapon)
                return 0;
            Data_Weapon weapon = this.attacker_weapon;

            // Checks if weapon is magic
            bool magic_attack = attacker.check_magic_attack(weapon, Distance);
            bool is_staff = weapon.is_staff();
            int actor_hit, weapon_hit, skill_hit, support_hit, s_bonus;
            if (is_staff)
            {
                actor_hit = attacker.atk_pow(weapon, magic_attack) * 5 +
                    attacker.stat(Stat_Labels.Skl);
                weapon_hit = weapon.Hit;
                skill_hit = 0;
                attacker.hit_skill(
                    ref skill_hit, ref weapon_hit, ref actor_hit,
                    weapon, null, magic_attack, Distance);
                support_hit = support_bonus(Combat_Stat_Labels.Hit);
                s_bonus = 0;
            }
            else
            {
                actor_hit = base_hit(weapon, magic_attack);
                weapon_hit = weapon.Hit;
                skill_hit = 0;
                attacker.hit_skill(
                    ref skill_hit, ref weapon_hit, ref actor_hit,
                    weapon, null, magic_attack, Distance);
                support_hit = support_bonus(Combat_Stat_Labels.Hit);
                s_bonus = attacker.actor.s_rank_bonus(weapon);
            }
            return Math.Max(0, actor_hit + weapon_hit + skill_hit + support_hit + s_bonus);
        }
        #endregion

        #region Crt
        internal int base_crt()
        {
            return attacker.stat(Stat_Labels.Skl) / 2;
        }

        internal virtual int crt()
        {
            if (!has_weapon)
                return 0;
            Data_Weapon weapon = this.attacker_weapon;

            if (weapon.is_staff())
                return 0;

            if (!can_crit())
                return 0;

            // Checks if weapon is magic
            bool magic_attack = attacker.check_magic_attack(weapon);
            int actor_crt, weapon_crt, skill_crt, support_crt, s_bonus;
            actor_crt = base_crt();
            weapon_crt = weapon.Crt;
            skill_crt = 0;
            attacker.crt_skill(
                ref skill_crt, ref weapon_crt, ref actor_crt,
                weapon, null, magic_attack, 1);
            support_crt = support_bonus(Combat_Stat_Labels.Crt);
            s_bonus = attacker.actor.s_rank_bonus(weapon);
            return Math.Max(0, actor_crt + weapon_crt + skill_crt + support_crt + s_bonus);
        }

        public bool can_crit()
        {
            Data_Weapon weapon = this.attacker_weapon;
            if (weapon == null)
                return false;

            if (weapon.is_staff())
                return false;

            return weapon.Crt >= 0;
        }
        #endregion

        #region Def/Res
        internal int def()
        {
            int bonus = 0;
            if (!this.location_bonuses.HasEnumFlag(CombatLocationBonuses.NoAttackerBonus))
            {
                // Terrain defense bonus
                var terrain_bonus = terrain_def_bonus();
                if (terrain_bonus.IsSomething)
                    bonus += terrain_bonus;
                // Support defense bonus
                bonus += support_bonus(Combat_Stat_Labels.Def);
            }
            return attacker.stat(Stat_Labels.Def) + bonus;
        }

        internal int res()
        {
            int bonus = 0;
            if (!this.location_bonuses.HasEnumFlag(CombatLocationBonuses.NoAttackerBonus))
            {
                // Terrain defense bonus
                var terrain_bonus = terrain_res_bonus();
                if (terrain_bonus.IsSomething)
                    bonus += terrain_bonus;
                // Support defense bonus
                bonus += support_bonus(Combat_Stat_Labels.Def);
            }
            return attacker.stat(Stat_Labels.Res) + bonus;
        }

        protected virtual Maybe<int> terrain_def_bonus()
        {
            return attacker.terrain_def_bonus(null);
        }

        protected virtual Maybe<int> terrain_res_bonus()
        {
            return attacker.terrain_res_bonus(null);
        }
        #endregion

        #region Avo
        internal int base_avo()
        {
            //int spd = this.spd(weapon_id); //Debug
            int spd = attacker.atk_spd(Distance, this.attacker_weapon == null ?
                null : new Item_Data(0, attacker_weapon.Id, -1));
            int lck = attacker.stat(Stat_Labels.Lck);
            attacker.base_avo_skill(ref spd, ref lck);

            //Debug
            //return spd * 2 + lck; // Default GBAFE avoid
            //return spd + lck * 2; // Lck weighted
            return ((spd + lck) * 3) / 2; // Spd and lck averaged
        }

        public int avo()
        {
            return avo(null, 0);
        }
        internal virtual int avo(WeaponTriangle tri)
        {
            return avo(null, tri);
        }
        internal int avo(Game_Unit target, WeaponTriangle tri)
        {
            return avo(target, false, tri);
        }
        internal int avo(Game_Unit target, bool staff = false, WeaponTriangle tri = WeaponTriangle.Nothing)
        {
            int actor_avo, bonus = 0, skill_avo;
            // Terrain avoid bonus
            if (Global.scene.is_map_scene)
            {
                bool magic_attack = target != null &&
                    target.check_magic_attack(target.actor.weapon, Distance);
                var terrain_bonus = attacker.terrain_avo_bonus(target, magic_attack);
                if (terrain_bonus.IsSomething)
                    bonus += terrain_bonus;
            }
            if (staff)
            {
                actor_avo = res() * 5;
                bonus += Distance * 2;
            }
            else
                actor_avo = base_avo();
            skill_avo = 0;
            attacker.avo_skill(ref skill_avo, ref actor_avo, ref bonus,
                attacker_weapon, target, tri);
            // Support avoid bonus
            bonus += support_bonus(Combat_Stat_Labels.Avo);
            return Math.Max(0, actor_avo + bonus + skill_avo);
        }

        internal virtual int staff_avo()
        {
            return staff_avo(null);
        }
        internal int staff_avo(Game_Unit target)
        {
            return avo(target, true);
        }
        #endregion

        #region Dod
        public int dodge()
        {
            return attacker.stat(Stat_Labels.Lck) +
                support_bonus(Combat_Stat_Labels.Dod) +
                attacker.dodge_skill();
        }
        #endregion
    }
}
