using System;
using System.Linq;
using System.Collections.Generic;
using FEXNA.Calculations.Stats;
using FEXNA_Library;

namespace FEXNA
{
    enum WeaponTriangle { Nothing, Advantage, Disadvantage }
    class Combat
    {
        public static bool Ignore_Terrain = false;

        #region Combat Stats
        // Is this needed at all any more // presently it's the only thing that sets units to using magic
        public static List<object> battle_setup(int id1, int id2, int distance)
        {
            return battle_setup(id1, id2, distance, 0);
        }
        public static List<object> battle_setup(int id1, int id2, int distance, int mode)
        {
            List<int?> stats = new List<int?>();
            Game_Unit battler_1 = Global.game_map.units[id1];
            if (mode != 1) // This is probably simply unneeded, due to being set somewhere else too //Yeti
                battler_1.battling = true;
            Game_Actor actor1 = battler_1.actor;
            Game_Unit battler_2 = null;
            Game_Actor actor2 = null;
            if (Global.game_map.units.ContainsKey(id2))
            {
                battler_2 = Global.game_map.units[id2];
                if (mode != 1) // This is probably simply unneeded, due to being set somewhere else too //Yeti
                    battler_2.battling = true;
                actor2 = battler_2.actor;
            }
            Data_Weapon weapon1 = null, weapon2 = null;
            List<object> result;
            switch (mode)
            {
                // Combat
                case 0:
                    // Battler 1
                    weapon1 = actor1.weapon;
                    battler_1.magic_attack = battler_1.check_magic_attack(weapon1, distance);
                    if (battler_2 != null)
                    {
                        weapon2 = actor2.weapon;
                        if (weapon2 != null && !weapon2.is_staff())
                            battler_2.magic_attack = battler_2.check_magic_attack(weapon2, distance);
                    }
                    break;
                // Staff
                case 1:
                    battler_1.magic_attack = true;
                    break;
            }
            return new List<object>();
        }

        // Change this to pass in units directly instead of ids //Yeti
        public static List<int?> combat_stats(int id1, int? id2, int distance)
        {
            if (id2 == null)
                return new List<int?> { null, null, null, null, null, null, null, null };
            else
                return combat_stats(id1, (int)id2, distance);
        }
        public static List<int?> combat_stats(int id1, int id2, int distance)
        {
            return combat_stats(id1, id2, distance, null, false, false);
        }
        public static List<int?> combat_stats(int id1, int id2, int distance, int? item_index)
        {
            return combat_stats(id1, id2, distance, item_index, false, false);
        }
        public static List<int?> combat_stats(int id1, int id2, int distance, int? item_index, bool counter_override)
        {
            return combat_stats(id1, id2, distance, item_index, counter_override, false);
        }
        public static List<int?> combat_stats(int id1, int id2, int distance, int? item_index, bool counter_override, bool ignore_terrain)
        {
            List<int?> result = new List<int?>();
            Game_Unit battler_1 = Global.game_map.units[id1];
            Game_Actor actor1 = battler_1.actor;
            bool is_target_unit = false;
            Combat_Map_Object target = Global.game_map.attackable_map_object(id2);
            Game_Unit battler_2 = null;
            Game_Actor actor2 = null;
            if (target.is_unit())
            {
                is_target_unit = true;
                battler_2 = Global.game_map.units[id2];
                actor2 = battler_2.actor;
            }
            Data_Weapon weapon1 = null, weapon2 = null;
            if (item_index == null)
            {
                weapon1 = actor1.weapon;
                if (weapon1 == null)
                    Print.message("This probably is about to break\nA unit is trying to attack with no weapon");
            }
            else
                weapon1 = Global.data_weapons[battler_1.items[(int)item_index].Id];

            if (is_target_unit)
                weapon2 = actor2.weapon;
            // Staves are not for fighting o:
            if (weapon2 != null)
                if (weapon2.is_staff())
                    weapon2 = null;

            Ignore_Terrain = ignore_terrain;

            if (weapon1.is_staff())
            {
                var stats = new CombatStats(
                    battler_1.id, battler_2.id, weapon1, distance);

                if (weapon1.Heals()) // Healing
                {
                    result.Add(null);
                    result.Add(-stats.dmg());
                    result.Add(null);
                    result.Add(null); //Yeti
                }
                else if (weapon1.is_attack_staff()) // Status effect
                {
                    result.Add(stats.hit());
                    result.Add(null);
                    result.Add(null);
                    result.Add(null); //Yeti
                }
                else // Other, Restore/Hammerne/Barrier //Yeti
                {
                    result.Add(null);
                    result.Add(null);
                    result.Add(null);
                    result.Add(null); //Yeti
                }
                result.Add(null);
                result.Add(null);
                result.Add(null);
                result.Add(null); //Yeti
            }
            else
            {
                if (battler_1.disabled)
                { 
                    result.Add(null);
                    result.Add(null);
                    result.Add(null);
                    result.Add(null); //Yeti
                }
                // Target is unit
                else if (is_target_unit)
                {
                    var stats = new CombatStats(
                        battler_1.id, battler_2.id, weapon1, distance);

                    result.Add(stats.hit());
                    result.Add(stats.dmg());
                    result.Add(stats.crt());
                    result.Add(battler_1.shown_skill_rate(battler_2));
                }
                // Target is attackable terrain
                else
                {
                    result.Add(100);
                    result.Add(Math.Max(0,
                        new BattlerStats(
                            battler_1.id, weapon1.Id, distance).dmg() - target.def));
                    result.Add(0);
                    result.Add(null);
                }

                bool counter = false;
                if (weapon2 != null)
                {
                    if (counter_override || battler_2.can_counter(battler_1, weapon1, distance)) // test for countering //Yeti
                    {
                        var target_stats = new CombatStats(
                            battler_2.id, battler_1.id, distance: distance);

                        counter = true;
                        result.Add(target_stats.hit());
                        result.Add(target_stats.dmg());
                        result.Add(target_stats.crt());
                        result.Add(battler_2.shown_skill_rate(battler_1));
                    }
                }
                if (!counter)
                {
                    result.Add(null);
                    result.Add(null);
                    result.Add(null);
                    result.Add(null); //Yeti
                }
            }
            for (int i = 0; i < result.Count; i++)
            {
                // Skip for dmg
                if (i % 4 == 1)
                    continue;
                // Caps rates at 100
                if (result[i] != null)
                    result[i] = Math.Min(100, (int)result[i]);
            }
            Ignore_Terrain = false;
            return result;
        }

        public static Dictionary<int, List<int?>> combat_stats(int id1, int id2, List<int> distance)
        {
            return combat_stats(id1, id2, distance, null, false, false);
        }
        public static Dictionary<int, List<int?>> combat_stats(int id1, int id2, List<int> distance, int? item_index)
        {
            return combat_stats(id1, id2, distance, item_index, false, false);
        }
        public static Dictionary<int, List<int?>> combat_stats(int id1, int id2, List<int> distance, int? item_index, bool counter_override)
        {
            return combat_stats(id1, id2, distance, item_index, counter_override, false);
        }
        public static Dictionary<int, List<int?>> combat_stats(int id1, int id2, List<int> distance, int? item_index, bool counter_override, bool ignore_terrain)
        {
            Dictionary<int,List<int?>> stats = new Dictionary<int,List<int?>>();
            foreach (int dist in distance)
            {
                stats.Add(dist, Combat.combat_stats(id1, id2, dist, item_index, counter_override, ignore_terrain));
            }
            Ignore_Terrain = false;
            return stats;
        }

        public static float[] damage_per_round(Game_Unit battler_1, Game_Unit battler_2, List<int> atk_distance)
        {
            return damage_per_round(battler_1, battler_2, atk_distance, null, true, false);
        }
        public static float[] damage_per_round(Game_Unit battler_1, Game_Unit battler_2, List<int> atk_distance, int? weapon_index)
        {
            return damage_per_round(battler_1, battler_2, atk_distance, weapon_index, true, false);
        }
        public static float[] damage_per_round(Game_Unit battler_1, Game_Unit battler_2, List<int> atk_distance, int? weapon_index, bool hard_ai)
        {
            return damage_per_round(battler_1, battler_2, atk_distance, weapon_index, hard_ai, false);
        }
        public static float[] damage_per_round(Game_Unit battler_1, Game_Unit battler_2, List<int> atk_distance, int? weapon_index, bool hard_ai, bool ignore_terrain)
        {
            Data_Weapon weapon1, weapon2 = null;
            if (weapon_index == null)
                weapon1 = battler_1.actor.weapon;
            else
                weapon1 = Global.data_weapons[battler_1.items[(int)weapon_index].Id];

            weapon2 = battler_2.actor.weapon;
            // Gets battle_stats
            Dictionary<int, List<int?>> stats_ary = combat_stats(battler_1.id, battler_2.id, atk_distance, weapon_index, true, ignore_terrain);
            float[] result = { 0, 0 };
            foreach (KeyValuePair<int, List<int?>> pair in stats_ary)
            {
                if (pair.Value[0] != null)
                {
                    if (weapon1.is_staff())
                    {
                        int hit = (int)pair.Value[0];
                        float use = Math.Max(0, (hard_ai ? Combat.true_hit(hit) : hit) / 100f);
                        result[0] = Math.Max(result[0], use);
                    }
                    else
                    {
                        int atk = (int)pair.Value[1];
                        int hit = (int)pair.Value[0];
                        int crt = (int)pair.Value[2];
                        float use = Math.Max(0,
                            atk * ((hard_ai ? Combat.true_hit(hit) : hit) / 100f) *
                            (1 + ((crt * (Constants.Combat.CRIT_MULT - 1)) / 100f)));
                        use *= battler_1.is_brave_blocked() ? 1 : weapon1.HitsPerAttack;
                        use *= battler_1.attacks_per_round(battler_2, pair.Key, weapon_index);
                        result[0] = Math.Max(result[0], use);
                    }
                }
                if (pair.Value[4] != null)
                {
                    int atk = (int)pair.Value[5];
                    int hit = (int)pair.Value[4];
                    float crt = (int)pair.Value[6];
                    float use = Math.Max(0,
                        atk * ((hard_ai ? Combat.true_hit(hit) : hit) / 100f) *
                        (1 + ((crt * (Constants.Combat.CRIT_MULT - 1)) / 100f)));
                    use *= battler_2.is_brave_blocked() ? 1 : weapon2.HitsPerAttack;
                    use *= battler_2.attacks_per_round(battler_1, pair.Key);
                    result[1] = Math.Max(result[1], use);
                }
            }
            return result;
        }

        public static float combat_odds(Game_Unit battler_1, Game_Unit battler_2, int atk_distance, int? weapon_index, bool hard_ai, bool ignore_terrain)
        {
            Data_Weapon weapon1, weapon2 = null;
            if (weapon_index == null)
                weapon1 = battler_1.actor.weapon;
            else
                weapon1 = Global.data_weapons[battler_1.items[(int)weapon_index].Id];

            weapon2 = battler_2.actor.weapon;
            // Gets battle_stats
            List<int?> stats_ary = combat_stats(battler_1.id, battler_2.id, atk_distance, weapon_index, true, ignore_terrain);

            // If opponent has no weapon, victory is guaranteed
            if (stats_ary[4] == null)
                return 1f;

            int atk1 = (int)stats_ary[1];
            int hit1 = (int)stats_ary[0];
            int crt1 = (int)stats_ary[2];
            int atk2 = (int)stats_ary[5];
            int hit2 = (int)stats_ary[4];
            int crt2 = (int)stats_ary[6];

            int hits_per_attack1 = battler_1.is_brave_blocked() ? 1 : weapon1.HitsPerAttack;
            int hits_per_attack2 = battler_2.is_brave_blocked() ? 1 : weapon2.HitsPerAttack;
            int attacks_per_round1 = battler_1.attacks_per_round(battler_2, atk_distance, weapon_index);
            int attacks_per_round2 = battler_2.attacks_per_round(battler_1, atk_distance);

            return combat_odds(
                hard_ai, battler_1.actor.maxhp, battler_2.actor.maxhp,
                atk1, hit1, crt1, atk2, hit2, crt2,
                hits_per_attack1, hits_per_attack2, attacks_per_round1, attacks_per_round2);
        }
        public static float combat_odds(bool hard_ai, int hp1, int hp2, int atk1, int hit1, int crt1, int atk2, int hit2, int crt2,
            int hitsPerAttack1, int hitsPerAttack2,
            int attacksPerRound1, int attacksPerRound2)
        {
            // If neither can damage
            if ((atk1 <= 0 || hit1 <= 0) && (atk2 <= 0 || hit2 <= 0))
                return 0.5f;
            if (atk1 <= 0 || hit1 <= 0)
                return 0f;
            if (atk2 <= 0 || hit2 <= 0)
                return 1f;

            List<float>[] odds = new List<float>[2];
            odds[0] = chance_to_kill(atk1,
                (hard_ai ? Combat.true_hit(hit1) : hit1) / 100f, crt1, hp2,
                hitsPerAttack1, attacksPerRound1, true);
            odds[1] = chance_to_kill(atk2,
                (hard_ai ? Combat.true_hit(hit2) : hit2) / 100f, crt2, hp1,
                hitsPerAttack2, attacksPerRound2, false);

            //odds = new List<float>[2];
            //odds[0] = wah2(50, 10 / 100f, 0, 50, false, false, true);
            //odds[1] = wah2(5, (hard_ai ? Combat.true_hit(40) : 40) / 100f, 50, 50, false, false, false);
            float total_odds = 1f, attacker_odds = 0f;
            int rounds = Math.Min(odds[0].Count, odds[1].Count);
            for (int i = 0; i < rounds; i++)
            {
                attacker_odds += total_odds * odds[0][i];
                total_odds -= total_odds * odds[0][i];
                total_odds -= total_odds * odds[1][i];
            }
            if (odds[0].Count <= odds[1].Count)
                attacker_odds += total_odds;
            if (attacker_odds > 1f)
                attacker_odds = 1f;
            return attacker_odds;
        }

        public static List<float> chance_to_kill(
            int atk, float hit, int crt, int hp,
            int hitsPerAttack, int attacksPerRound, bool first)
        {
            int round = 0;
            int attacks = 1 * hitsPerAttack * attacksPerRound;
            int fatal_rounds = 0;
            float kill_chance = 0f;
            // Kill_Chance is chance the opponent has died, Outcomes is a list of data for each possible outcome
            // Outcomes is set up as chance for this specific outcome, damage so far
            List<Combat_Odds> odds = new List<Combat_Odds>();
            odds.Add(new Combat_Odds(round, 0));
            int index = odds.Count - 1;
            odds[index].Outcomes.Add(new float[] { 1f, 0 });
            round++;
            while (true)
            {
                for (int i = 0; i < attacks; i++)
                {
                    odds.Add(new Combat_Odds(round, 0));
                    index = odds.Count - 1;
                    // Gets the total odds for any things happening this round
                    float total_odds = 0f;
                    foreach (float[] outcome in odds[index - 1].Outcomes)
                    {
                        // If this was already fatal
                        if (outcome[1] >= hp)
                            continue;
                        total_odds += outcome[0];
                    }
                    foreach (float[] outcome in odds[index - 1].Outcomes)
                    {
                        // If this was already fatal
                        if (outcome[1] >= hp)
                            continue;
                        // hit with crit
                        if (crt > 0)
                        {
                            odds[index].Outcomes.Add(new float[] {
                                (outcome[0] * (hit * crt)) / 100,
                                outcome[1] + (atk * Constants.Combat.CRIT_MULT) });
                            if (odds[index].Outcomes[odds[index].Outcomes.Count - 1][1] >= hp)
                            {
                                odds[index].Kill_Chance += ((hit * crt) / 100) * (outcome[0] / total_odds);
                                kill_chance += odds[index].Outcomes[odds[index].Outcomes.Count - 1][0];
                            }
                        }
                        // hit
                        if (crt < 100)
                        {
                            odds[index].Outcomes.Add(new float[] { (outcome[0] * (hit * (100 - crt))) / 100, outcome[1] + atk });
                            if (odds[index].Outcomes[odds[index].Outcomes.Count - 1][1] >= hp)
                            {
                                odds[index].Kill_Chance += ((hit * (100 - crt)) / 100) * (outcome[0] / total_odds);
                                kill_chance += odds[index].Outcomes[odds[index].Outcomes.Count - 1][0];
                            }
                        }
                        // miss
                        if (hit < 1f) //Debug
                            odds[index].Outcomes.Add(new float[] { outcome[0] * (1f - hit), outcome[1] });
                    }
                    odds[index].compress_outcomes();
                    //odds[index].Kill_Chance++;
                    if (kill_chance >= 0.99999f || (fatal_rounds >= 3 && kill_chance > 0.99f))
                    //if (kill_chance >= 1f || (fatal_rounds >= 3 && kill_chance > 0.99f)) //Debug
                    {
                        break;
                    }

                    if (first && attacksPerRound > 1)
                    {
                        if (i + 1 == attacks / 2)
                        {
                            round++;
                            if (kill_chance > 0)
                                fatal_rounds++;
                        }
                    }
                    else if (i + 1 == attacks)
                    {
                        round++;
                        if (kill_chance > 0)
                            fatal_rounds++;
                    }
                }
                if (kill_chance >= 0.99999f || (fatal_rounds >= 3 && kill_chance > 0.99f))
                //if (kill_chance >= 1f || (fatal_rounds >= 3 && kill_chance > 0.99f)) //Debug
                {
                    break;
                }
            }

            List<float> result = new List<float>();
            int result_round = round + 1;
            for (int i = odds.Count - 1; i > 0; i--)
            {
                if (odds[i].Round < result_round)
                {
                    result.Add(odds[i].Kill_Chance);
                    result_round = odds[i].Round;
                }
            }
            result.Reverse();
            return result;
        }
        #endregion

        public static void test_hit(int hit, int crt, int distance, out bool is_hit, out bool is_crt)
        {
            is_hit = false;
            is_crt = false;

            if (Global.game_system.roll_dual_rng(hit))
                is_hit = true;
            if (is_hit)
                if (crt > -1)
                {
                    if (Global.game_system.roll_rng(crt))
                        is_crt = true;
                }
        }
        static void test_hit(Game_Unit battler_1, Combat_Map_Object battler_2, int distance, out bool is_hit, out bool is_crt)
        {
            List<int?> ary = Combat.combat_stats(battler_1.id, battler_2.id, distance);
            int hit = (int)ary[0];
            int crt = ary[2] == null ? -1 : (int)ary[2];

            test_hit(hit, crt, distance, out is_hit, out is_crt);
        }

        /// <summary>
        /// Gets an Attack_Result of one unit attacking another.
        /// </summary>
        /// <param name="battler_1">Attacking unit</param>
        /// <param name="battler_2">Defending unit</param>
        /// <param name="distance">Combat distance</param>
        /// <param name="actual_dmg">Damage of the atack</param>
        /// <param name="hit">hit flag</param>
        /// <param name="crt">Crit flag</param>
        /// <param name="weapon">Attacker's weapon</param>
        public static Attack_Result set_attack(
            Game_Unit battler_1, Game_Unit battler_2, int distance, int actual_dmg, bool hit, bool crt, FEXNA_Library.Data_Weapon weapon)
        {
            Attack_Result result = new Attack_Result { state_change = new List<KeyValuePair<int, bool>>() };

            result.hit = hit;
            result.crt = crt;
            int wexp = battler_1.actor.wexp_from_weapon(weapon);

            if (result.hit)
            {
                if (result.crt)
                    actual_dmg = (int)Math.Floor(actual_dmg * Constants.Combat.CRIT_MULT);
                if (weapon.Cursed())
                    result.backfire = Global.game_system.roll_rng(
                        Constants.Actor.LUCK_CAP - battler_1.stat(Stat_Labels.Lck) +
                        Constants.Combat.CURSE_BACKFIRE_RATE); // unit or actor lck? //Yeti
                int dmg;
                if (result.backfire)
                {
                    battler_1.skill_effects(ref actual_dmg, battler_2, ref result);
                    int hp = battler_1.actor.hp + result.immediate_life_steal; //Debug
                    if (battler_1.actor.fatality)
                        actual_dmg = hp;
                    dmg = Math.Max(Math.Min(actual_dmg, hp), hp - battler_1.actor.maxhp);
                    result.kill = dmg >= hp;
                    state_change(weapon, ref result);
                    // Can't delayed heal after killing yourself
                    if (weapon.Drains_HP() && !result.kill)
                        result.delayed_life_steal = dmg > 0;
                }
                else
                {
                    int hp = battler_2.actor.hp;
                    if (battler_1.actor.fatality)
                        actual_dmg = hp;
                    battler_1.skill_effects(ref actual_dmg, battler_2, ref result);
                    dmg = Math.Max(Math.Min(actual_dmg, hp), hp - battler_2.actor.maxhp);
                    result.kill = dmg >= hp;
                    state_change(weapon, ref result);
                    result.delayed_life_steal = weapon.Drains_HP() && dmg > 0;
                }
                result.dmg = dmg;
                result.actual_dmg = actual_dmg;
                // WExp Gain
                if (result.kill) wexp *= 2;
            }
            else
            {
                // WExp Gain
                wexp = Math.Max(1, wexp / 2);
            }
            result.wexp = battler_1.is_ally ? wexp : 0;
            return result;
        }
        /// <summary>
        /// Gets an Attack_Result of one unit attacking another in scripted combat.
        /// </summary>
        /// <param name="battler_1">Attacking unit</param>
        /// <param name="battler_2">Defending unit</param>
        /// <param name="distance">Combat distance</param>
        /// <param name="weapon">Attacker's weapon</param>
        /// <param name="stats">Scripted combat stats</param>
        public static Attack_Result set_attack(
            Game_Unit battler_1, Game_Unit battler_2, int distance, FEXNA_Library.Data_Weapon weapon, Scripted_Combat_Stats stats)
        {
            Attack_Result result = new Attack_Result { state_change = new List<KeyValuePair<int, bool>>() };

            int actual_dmg = stats.Damage;
            result.hit = stats.Result != Attack_Results.Miss;
            result.crt = result.hit && stats.Result == Attack_Results.Crit;
            int wexp = battler_1.actor.wexp_from_weapon(weapon);

            if (result.hit)
            {
                if (weapon.Cursed())
                    result.backfire = false;
                int dmg;
                if (result.backfire)
                {
                    battler_1.skill_effects(ref actual_dmg, battler_2, ref result);
                    int hp = battler_1.actor.hp + result.immediate_life_steal; //Debug
                    if (battler_1.actor.fatality)
                        actual_dmg = hp;
                    dmg = Math.Max(Math.Min(actual_dmg, hp), hp - battler_1.actor.maxhp);
                    result.kill = dmg >= hp;
                    state_change(weapon, ref result);
                    if (weapon.Drains_HP() && !result.kill)
                        result.delayed_life_steal = dmg > 0;
                }
                else
                {
                    int hp = battler_2.actor.hp;
                    if (battler_1.actor.fatality)
                        actual_dmg = hp;
                    battler_1.skill_effects(ref actual_dmg, battler_2, ref result);
                    dmg = Math.Max(Math.Min(actual_dmg, hp), hp - battler_2.actor.maxhp);
                    result.kill = dmg >= hp;
                    state_change(weapon, ref result);
                    result.delayed_life_steal = weapon.Drains_HP() && dmg > 0;
                }
                result.dmg = dmg;
                result.actual_dmg = actual_dmg;
                // WExp Gain
                if (result.kill) wexp *= 2;
            }
            else
            {
                // WExp Gain
                wexp = Math.Max(1, wexp / 2);
            }
            result.wexp = battler_1.is_ally ? wexp : 0;
            return result;
        }
        /// <summary>
        /// Gets an Attack_Result of one unit attacking a non-unit object.
        /// </summary>
        /// <param name="battler_1">Attacking unit</param>
        /// <param name="battler_2">Targetted terrain</param>
        /// <param name="distance">Combat distance</param>
        /// <param name="actual_dmg">Damage of the atack</param>
        /// <param name="hit">hit flag</param>
        /// <param name="crt">Crit flag</param>
        /// <param name="weapon">Attacker's weapon</param>
        public static Attack_Result set_attack(
            Game_Unit battler_1, Combat_Map_Object battler_2, int distance, int actual_dmg, bool hit, bool crt, FEXNA_Library.Data_Weapon weapon)
        {
            Attack_Result result = new Attack_Result { state_change = new List<KeyValuePair<int, bool>>() };

            result.hit = hit;
            result.crt = crt;
            int wexp = battler_1.actor.wexp_from_weapon(weapon);

            if (result.hit)
            {
                if (result.crt)
                    actual_dmg = (int)Math.Floor(actual_dmg * Constants.Combat.CRIT_MULT);
                int hp = battler_2.hp;
                int dmg = Math.Min(actual_dmg, hp);
                result.kill = dmg >= hp;
                state_change(weapon, ref result);
                result.dmg = dmg;
                result.actual_dmg = actual_dmg;
            }
            else { }
            result.wexp = 0;
            return result;
        }

        public static Attack_Result set_heal(Game_Unit battler_1, Game_Unit battler_2, int distance, FEXNA_Library.Data_Weapon weapon)
        {
            Attack_Result result = new Attack_Result { state_change = new List<KeyValuePair<int, bool>>() };

            List<int?> ary = Combat.combat_stats(battler_1.id, battler_2.id, distance);
            int dmg = 0, actual_dmg = 0;
            if (weapon.Heals())
            {
                if (weapon.Cursed())
                    result.backfire = Global.game_system.roll_rng(
                        Constants.Actor.LUCK_CAP - battler_1.stat(Stat_Labels.Lck) +
                        Constants.Combat.CURSE_BACKFIRE_RATE); // unit or actor lck? //Yeti
                // Hard-coding this because I don't see much other solution
                if (result.backfire)
                    dmg = actual_dmg = battler_2.actor.maxhp - (battler_2.actor.hp - 1);
                else
                {
                    actual_dmg = Math.Max((int)ary[1], -battler_2.actor.maxhp);
                    dmg = Math.Max(actual_dmg, -(battler_2.actor.maxhp - battler_2.actor.hp));
                }
            }
            result.hit = true;
            result.crt = false;
            //battler_1.actor.backfire = false; //Debug
            int wexp = staff_wexp(battler_1.actor, weapon);

            state_change(weapon, ref result);
            result.dmg = dmg;
            result.actual_dmg = actual_dmg;

            result.wexp = battler_1.is_ally ? wexp : 0;
            return result;
        }

        public static Attack_Result set_status_staff(Game_Unit battler_1, Game_Unit battler_2, int distance, bool hit, FEXNA_Library.Data_Weapon weapon)
        {
            Attack_Result result = new Attack_Result { state_change = new List<KeyValuePair<int, bool>>() };

            result.hit = hit;
            result.crt = false;
            //battler_1.actor.backfire = false; //Debug
            int wexp = staff_wexp(battler_1.actor, weapon);

            if (hit)
                state_change(weapon, ref result);
            result.dmg = 0;
            result.actual_dmg = 0;

            result.wexp = battler_1.is_ally ? wexp : 0;
            return result;
        }

        public static Attack_Result set_torch(Game_Unit battler_1, FEXNA_Library.Data_Weapon weapon)
        {
            Attack_Result result = new Attack_Result { state_change = new List<KeyValuePair<int, bool>>() };

            result.hit = true;
            result.crt = false;
            //battler_1.actor.backfire = false; //Debug
            int wexp = staff_wexp(battler_1.actor, weapon);
            result.dmg = 0;
            result.actual_dmg = 0;

            result.wexp = battler_1.is_ally ? wexp : 0;
            return result;
        }

        public static int staff_wexp(Game_Actor actor, FEXNA_Library.Data_Weapon staff)
        {
            return actor.wexp_from_weapon(staff);
        }

        public static void state_change(FEXNA_Library.Data_Weapon weapon, ref Attack_Result result)
        {
            foreach (int i in weapon.Status_Remove)
            {
                result.state_change.Add(new KeyValuePair<int, bool>(i, false));
            }
            foreach (int i in weapon.Status_Inflict)
            {
                result.state_change.Add(new KeyValuePair<int, bool>(i, true));
            }
        }

        // Reuse this method for hazardous terrain, to make units flash/etc //Yeti
        public static void map_attack(Game_Unit battler_1, Combat_Map_Object target, bool is_target_unit, Combat_Round_Data data, FEXNA_Library.Data_Weapon weapon)
        {
            Game_Unit battler_2 = is_target_unit ? (Game_Unit)target : null;
            if (data.Result.hit)
            {
                Game_Unit hit_battler;
                if (data.Result.backfire)
                {
                    hit_battler = battler_1;
                }
                else
                {
                    hit_battler = battler_2;
                }
                // Determines what hit sounds to play
                bool no_damage = (data.Result.dmg == 0 && !data.Result.kill);
                if (data.Result.kill)
                {
                    if (data.Result.crt)
                        Global.Audio.play_se("Map Sounds", "Critical");
                    Global.Audio.play_se("Map Sounds", is_target_unit ? "Hit_Kill" : "Snag_Kill");
                }
                else if (!no_damage)
                {
                    if (data.Result.crt)
                        Global.Audio.play_se("Map Sounds", "Critical");
                    Global.Audio.play_se("Map Sounds", is_target_unit ? "Hit" : "Snag_Hit");
                }
                else
                {
                    Global.Audio.play_se("Map Sounds", "Hit_NoDamage");
                }
                // Hit flash
                List<int> types = new List<int> { (int)weapon.Main_Type, (int)weapon.Scnd_Type };
                if (!no_damage && hit_battler != null)
                {
                    if (data.Result.crt)
                        hit_battler.crit_color(Game_Unit.hit_flash_color(types));
                    else
                        hit_battler.hit_color(Game_Unit.hit_flash_color(types));
                }
            }
            else
            {
                Global.Audio.play_se("Map Sounds", "Miss");
            }
        }

        #region Exp
        public static int exp(Game_Unit battler_1, Game_Unit battler_2, bool kill = false)
        {
            int diff = level_difference(battler_1, battler_2);
            return exp(diff, battler_2.boss, kill);
        }

        public static int exp(int diff, bool boss = false, bool kill = false)
        {
            // Attack exp
            // 0.5x diff for non-kills
            int exp_gain = (Constants.Combat.BASE_COMBAT_EXP * 2 + 1 + diff) / 2;
            exp_gain = Math.Max(1, exp_gain);

            // Kill exp
            if (kill)
            {
                int kill_gain = Constants.Combat.BASE_COMBAT_EXP * 3; // Exp for a kill
                // 3x diff when above target level
                if (diff < 0)
                    kill_gain = kill_gain + (diff * 3);
                // 1.5x diff when below target level
                else
                    kill_gain = kill_gain + (diff * 3); //kill_gain + (diff * 3 / 2);
                kill_gain = Math.Max(exp_gain, kill_gain);

                exp_gain = kill_gain;
            }
            exp_gain = exp_difficulty_adjustment(exp_gain, kill);
            // Boss bonus
            if (kill)
            {
                if (boss)
                    exp_gain += 30;
            }
            exp_gain = Math.Max(1, exp_gain);

            return Math.Min(Constants.Actor.EXP_TO_LVL, exp_gain);

            /*
            // Attack exp
            int exp_gain = Math.Max(1, (21 + diff) / 2);
            // Kill bonus
            if (kill)
            {
                exp_gain += Math.Max(1, diff + 20);
                if (boss)
                    exp_gain += 30;
            }
            return Math.Min(Constants.Actor.EXP_TO_LVL, exp_gain);*/
        }

        private static int exp_difficulty_adjustment(float exp_gain, bool kill)
        {
            // Kill adjustment
            if (kill)
            {
                if (Constants.Combat.KILL_EXP_MULTIPLIER
                    .ContainsKey(Global.game_system.Difficulty_Mode))
                {
                    float multiplier =
                        Constants.Combat.KILL_EXP_MULTIPLIER[
                            Global.game_system.Difficulty_Mode];
                    exp_gain = exp_gain * multiplier;
                }
            }
            // Non-kill adjustment
            else
            {
                if (Constants.Combat.NON_KILL_EXP_MULTIPLIER
                    .ContainsKey(Global.game_system.Difficulty_Mode))
                {
                    float multiplier =
                        Constants.Combat.NON_KILL_EXP_MULTIPLIER[
                            Global.game_system.Difficulty_Mode];
                    exp_gain = exp_gain * multiplier;
                }
            }

            // Result adjustment
            if (Constants.Combat.EXP_MULTIPLIER
                .ContainsKey(Global.game_system.Difficulty_Mode))
            {
                float multiplier =
                    Constants.Combat.EXP_MULTIPLIER[
                        Global.game_system.Difficulty_Mode];
                exp_gain = exp_gain * multiplier;
            }

            return (int)Math.Ceiling(exp_gain);
        }

        private static int level_difference(Game_Unit battler_1, Game_Unit battler_2)
        {
            // Get levels of both battlers
            bool tier_0s = true; //Global.game_system.has_tier_0s //Yeti
            int level_1 = battler_1.actor.level;
            int level_2 = battler_2.actor.level;
            if (tier_0s)
            {
                if (battler_1.actor.tier > 0)
                    level_1 += Constants.Actor.TIER0_LVL_CAP;
                if (battler_2.actor.tier > 0)
                    level_2 += Constants.Actor.TIER0_LVL_CAP;
            }
            level_1 += Constants.Actor.LVL_CAP * Math.Max(0, battler_1.actor.tier - 1);
            level_2 += Constants.Actor.LVL_CAP * Math.Max(0, battler_2.actor.tier - 1);

            return level_2 - level_1;
        }

        public static int staff_exp(Game_Actor actor, Data_Weapon weapon)
        {
            int exp_gain;
            if (actor.tier <= 0)
                exp_gain = (int)(weapon.Staff_Exp * 1.5);
            else
                exp_gain = (int)(weapon.Staff_Exp / (Math.Pow(2, actor.tier - 1)));
            return Math.Min(exp_gain, Constants.Actor.EXP_TO_LVL);
        }
        #endregion

        public static WeaponTriangle weapon_triangle(
            Game_Unit battler_1, Game_Unit battler_2,
            Data_Weapon weapon_1, Data_Weapon weapon_2, int distance)
        {
            // Before anything else if unarmed (and can use weapons), WTD
            bool targetUnarmed = weapon_2 == null;
            bool targetCanArm = battler_2.actor.weapon_types().Count > 0;
            if (Weapon_Triangle.IN_EFFECT && Weapon_Triangle.UNARMED_DISADVANTAGE)
                if (targetUnarmed && targetCanArm)
                    return WeaponTriangle.Advantage;

            // Check for override
            var tri_override = battler_1.weapon_triangle_override(
                battler_2, weapon_1, weapon_2, distance);
            if (tri_override.IsSomething)
                return tri_override;
            
            // Return if weapon triangle disabled
            if (!Weapon_Triangle.IN_EFFECT)
                return WeaponTriangle.Nothing;

            int advantage1 = 0;
            int advantage2 = 0;

            // Get all weapon types
            HashSet<WeaponType> weapon_1_types, weapon_2_types;
            weapon_1_types = weapon_1.main_type().type_and_parents(Global.weapon_types);
            weapon_1_types.UnionWith(weapon_1.scnd_type().type_and_parents(Global.weapon_types));

            if (weapon_2 != null)
            {
                weapon_2_types = weapon_2.main_type().type_and_parents(Global.weapon_types);
                weapon_2_types.UnionWith(weapon_2.scnd_type().type_and_parents(Global.weapon_types));
            }
            else
                weapon_2_types = new HashSet<WeaponType>();

            // Compares weapon types, if target is armed
            if (!targetUnarmed)
            {
                // Determines what each weapon can beat
                HashSet<int> wpn_1_advs, wpn_2_advs;
                // If reaver
                if (weapon_1.Reaver() ^ weapon_2.Reaver())
                {
                    wpn_1_advs = new HashSet<int>(weapon_1_types.SelectMany(x => x.WtaReaverTypes));
                    wpn_2_advs = new HashSet<int>(weapon_2_types.SelectMany(x => x.WtaReaverTypes));
                }
                else
                {
                    wpn_1_advs = new HashSet<int>(weapon_1_types.SelectMany(x => x.WtaTypes));
                    wpn_2_advs = new HashSet<int>(weapon_2_types.SelectMany(x => x.WtaTypes));
                }
                // Determines net weapon advantage
                foreach (int type in wpn_1_advs)
                    if (weapon_2_types.Contains(Global.weapon_types[type]))
                        advantage1++;
                foreach (int type in wpn_2_advs)
                    if (weapon_1_types.Contains(Global.weapon_types[type]))
                        advantage2++;
            }

            // Modifies advantage based on range
            if (!(Global.game_system.In_Arena || Global.game_system.home_base) || Global.scene.is_test_battle)
            {
                if (weapon_1_types.Any(x => x.WtaRanges.Contains(distance)))
                    advantage1++;
                if (weapon_1_types.Any(x => x.WtdRanges.Contains(distance)))
                    advantage2++;

                if (weapon_2_types.Any(x => x.WtaRanges.Contains(distance)))
                    advantage2++;
                if (weapon_2_types.Any(x => x.WtdRanges.Contains(distance)))
                    advantage1++;
            }

            // Calculates result
            if (advantage1 > advantage2)
                return WeaponTriangle.Advantage;
            else if (advantage2 > advantage1)
                return WeaponTriangle.Disadvantage;
            return WeaponTriangle.Nothing;
        }
        
        public static WeaponTriangle reverse_wta(WeaponTriangle tri)
        {
            switch (tri)
            {
                case WeaponTriangle.Advantage:
                    return WeaponTriangle.Disadvantage;
                case WeaponTriangle.Disadvantage:
                    return WeaponTriangle.Advantage;
                case WeaponTriangle.Nothing:
                    return WeaponTriangle.Nothing;
#if DEBUG
                default:
                    throw new Exception();
#endif
            }
            return WeaponTriangle.Nothing;
        }

        public static int weapon_triangle_mult(Game_Unit battler_1, Game_Unit battler_2, Data_Weapon weapon_1, Data_Weapon weapon_2, int distance)
        {
            // If target is unequipped
            if (weapon_2 == null)
                return 1;

            // Skills
            var skill_mult = battler_1.weapon_triangle_mult_skill(
                battler_2, weapon_1, weapon_2, distance);
            if (skill_mult.IsSomething)
                return skill_mult;
            skill_mult = battler_2.weapon_triangle_mult_skill(
                battler_1, weapon_2, weapon_1, distance);
            if (skill_mult.IsSomething)
                return skill_mult;

            // Weapon doubles weapon triangle; traditionally part of reavers
            if (weapon_1.DoubledWeaponTri() || weapon_2.DoubledWeaponTri())
                return 2;
            return 1;
        }

        public static float true_hit(int hit)
        {
            float t_hit;
            if (hit < 50)
                t_hit = hit * (hit + 0.5f) * 2.0f;
            else
            {
                t_hit = 1 - (1 - (hit / 100.0f)) * (1 - ((hit / 100.0f) + 0.005f)) * 2.0f;
                t_hit *= 10000;
            }
            return t_hit / 100.0f;
        }

        public static bool can_use_item(Game_Unit unit, int item_id, bool is_weapon = false)
        {
            return can_use_item(unit.actor, item_id, is_weapon, unit);
        }
        public static bool can_use_item(Game_Actor actor, int item_id, bool is_weapon, Game_Unit unit = null)
        {
            // this probably shouldn't check with weapons, at least for now //Yeti
            if (is_weapon)
                return false;
            Data_Item item = Global.data_items[item_id];
            if (!actor.is_useable(item))
                return false;
            // Torch
            if (item.Torch_Radius > 0)
                if (Global.game_map.fow && !Global.game_system.preparations)
                    if (unit == null || !unit.vision_penalized())
                        return true;
            if (item.is_placeable())
            {
                if (unit != null)
                {
                    if (unit.placeable_targets().Any())
                        return true;
                }
            }
            // Promotion Items
            if (item.Promotes.Contains(actor.class_id) && actor.promotes_to() != null)
                return true;
            // Healing Items
            if (item.can_heal_hp() && !actor.is_full_hp())
            {
                if (!Global.game_system.preparations)
                    return true;
            }
            else if (item.Status_Remove.Intersect(actor.negative_states).ToList().Count > 0)
                return true;
            // Stat Buffs
            if (item.is_stat_buffer())
                if (!Global.game_system.preparations)
                    return true;
            // Stat Boosters
            if (item.is_stat_booster())
            {
                if (item.Stat_Boost[(int)Boosts.MaxHp] > 0 && !actor.get_capped(Stat_Labels.Hp))
                    return true;
                if (item.Stat_Boost[(int)Boosts.Pow] > 0 && !actor.get_capped(Stat_Labels.Pow))
                    return true;
                if (item.Stat_Boost[(int)Boosts.Skl] > 0 && !actor.get_capped(Stat_Labels.Skl))
                    return true;
                if (item.Stat_Boost[(int)Boosts.Spd] > 0 && !actor.get_capped(Stat_Labels.Spd))
                    return true;
                if (item.Stat_Boost[(int)Boosts.Lck] > 0 && !actor.get_capped(Stat_Labels.Lck))
                    return true;
                if (item.Stat_Boost[(int)Boosts.Def] > 0 && !actor.get_capped(Stat_Labels.Def))
                    return true;
                if (item.Stat_Boost[(int)Boosts.Res] > 0 && !actor.get_capped(Stat_Labels.Res))
                    return true;
                if (item.Stat_Boost[(int)Boosts.Mov] > 0 && !actor.get_capped(Stat_Labels.Mov))
                    return true;
                if (item.Stat_Boost[(int)Boosts.Con] > 0 && !actor.get_capped(Stat_Labels.Con))
                    return true;
                if (item.Stat_Boost[(int)Boosts.WLvl] > 0 || item.Stat_Boost[(int)Boosts.WExp] > 0)
                    if (actor.equipped > 0)
                    {
                        WeaponType weapon_type = actor.valid_weapon_type_of(actor.items[actor.equipped - 1].to_weapon);
                        return !actor.is_weapon_level_capped(weapon_type);
                    }
            }
            // Growth Boosters
            if (item.is_growth_booster())
            {
                if (actor.can_level())
                    return true;
            }
            // Repair Kit, anything else that can target own items?
            foreach (Item_Data item_data in actor.items)
                if (item.can_target_item(item_data))
                    return true;
            return false;
        }
    }

    class Combat_Odds
    {
        public int Round;
        public float Kill_Chance;
        public List<float[]> Outcomes = new List<float[]>();

        public Combat_Odds(int round, float kill_chance)
        {
            Round = round;
            Kill_Chance = kill_chance;
        }

        public void compress_outcomes()
        {
            List<float[]> outcomes = new List<float[]>();
            Dictionary<float, float> outcome_dictionary = new Dictionary<float, float>();
            foreach(float[] outcome in Outcomes)
            {
                if (!outcome_dictionary.ContainsKey(outcome[1]))
                    outcome_dictionary[outcome[1]] = 0;
                outcome_dictionary[outcome[1]] += outcome[0];
            }
            foreach (KeyValuePair<float, float> pair in outcome_dictionary)
                outcomes.Add(new float[] { pair.Value, pair.Key });
            Outcomes = outcomes;
        }
    }
}
