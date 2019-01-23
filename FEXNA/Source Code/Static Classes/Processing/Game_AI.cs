using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNA.Calculations.Stats;
using FEXNA.Pathfinding;
using FEXNA_Library;
using FEXNAWeaponExtension;

namespace FEXNA
{
    enum Search_For_Ally_Modes { Anyone, Looking_To_Heal, Looking_For_Healing, Looking_For_Healing_Item, Attack_In_Range_Healing_Item, Looking_To_Dance }
    class Game_AI
    {
#if DEBUG
        public static bool AI_ENABLED = true;
#endif

        public readonly static int[] ATTACK_MISSIONS = new int[] { 0, 1, 2, 3, 15 };
        public readonly static int[] STATUS_MISSIONS = new int[] { 0, 1, 2, 3, 7 };
        public readonly static int[] STAFF_MISSIONS = new int[] { 7 };
        public readonly static int[] HEALING_MISSIONS = new int[] { 7 };
        public readonly static int[] IMMOBILE_MISSIONS = new int[] { 0, 10 };
        public readonly static int[] UNMOVING_MISSIONS = new int[] { 0, 1, 10 };
        public readonly static int[] MOVING_MISSIONS = new int[] { 2, 3, 4, 7 };
        public readonly static int[] MOVE_TO_TILE_MISSIONS = new int[] { 9 };
        // Missions where the unit's goal can dramatically change the map state (escaping, talking to a PC), and
        //      thus they shouldn't be sidetracked by anything that could cause their mission to fail, like healing
        public readonly static int[] DETERMINED_MISSIONS = new int[] { 4, 8, 15 };

        public const int MAIN_MISSION_COUNT = 20;
        public const int ATTACK_IN_PLACE_MISSION = 0;
        public const int ATTACK_IN_RANGE_MISSION = 1;
        public const int BERSERK_MISSION = 2;
        public const int PILLAGE_OVER_MISSION = 2;
        public const int DO_NOTHING_MISSION = 10;
        public const int SENTRY_MISSION = 13;
        public const int SAVIOR_MISSION = 14;
        public const int ESCAPE_MISSION = 15;
        public const int STEAL_MISSION = 24;
        public const int HEALING_MISSION = 25;
        public const int RETREAT_MISSION = 27;
        public const int USE_ITEM_MISSION = 28;
        public const int RESCUE_MISSION = 29;
        public const int RESCUE_DROP_MISSION = 30;
        public const int RETREATING_ATTACK_MISSION = 31;
        public const int SAFE_ATTACK_MISSION = 32;
        public const int ATTACK_IN_RANGE_HEAL_SELF = 33;
        public const int MISSION_COUNT = 100;
        const int ATTACK_WEIGHT_PRECISION = 10000;
        const int HEALING_TERRAIN_SEARCH_RANGE = 2; // How many turns to search outward for healing terrain (forts)

        internal readonly static Dictionary<int, string> MISSION_NAMES = new Dictionary<int, string> {
            {  0, "Still" },
            {  1, "Attack in range" },
            {  2, "Seek and Attack (any)" },
            {  3, "Seek and Attack (weakest)" },
            {  4, "Pillage" },
            {  5, "Thief" },
            {  6, "Defend Area" },
            {  7, "Staff User" },
            {  8, "Seek Unit to Talk" },
            {  9, "Seek Tile" },
            { 10, "Do nothing" },
            { 13, "FoW Sentry" },
            { 14, "Savior" },
            { 15, "Escape" },
            { 16, "Dancer" }
        };

        #region Selecting target
        public static int[] get_atk_target(Game_Unit attacker, List<int> targets, List<int> weapons, bool can_move = false, bool best = true)
        {
            // Check the viability of each target
            List<int[]> target_ary = check_targets(attacker, targets, weapons, can_move, best, damage_test: false);
            if (!target_ary.Any())
                return null;

            target_ary.Sort(delegate(int[] a, int[] b) { return b[2] - a[2]; });
            int best_use = target_ary[0][2];
            List<int> valid_targets = new List<int>();
            for (int i = 0; i < target_ary.Count; i++)
                if (target_ary[i][2] == best_use)
                    valid_targets.Add(i);
            return target_ary[valid_targets[(int)((Global.game_state.ai_turn_rn / 100f) * valid_targets.Count)]];
        }

        public static int[] get_retreating_atk_target(Game_Unit attacker, List<int> targets, List<int> weapons, bool can_move = false, bool best = true)
        {
            // Check the viability of each target
            List<int[]> target_ary = check_targets(attacker, targets, weapons, can_move, best, retreating: true, damage_test: false);
            if (target_ary.Count == 0)
                return null;
            target_ary.Sort(delegate(int[] a, int[] b) { return b[2] - a[2]; });
            int best_use = target_ary[0][2];
            List<int> valid_targets = new List<int>();
            for (int i = 0; i < target_ary.Count; i++)
                if (target_ary[i][2] == best_use)
                    valid_targets.Add(i);
            return target_ary[valid_targets[(int)((Global.game_state.ai_turn_rn / 100f) * valid_targets.Count)]];
        }

        public static int[] get_en_route_atk_target(Game_Unit attacker, List<int> targets, List<int> weapons, bool can_move = false, bool best = true)
        {
            /*// Find the closest pillage location //Debug
            //HashSet<Vector2> target_locs = new HashSet<Vector2>(); //Debug
            //foreach (KeyValuePair<Vector2, string[]> pair in Global.game_map.visit_locations)
            //    if (pair.Value[1].Length > 0)
            //        target_locs.Add(pair.Key);
            List<LocationDistance> target_distances = Game_AI.distance_to_locations(attacker, target_locs);
            target_distances.Sort(delegate(LocationDistance a, LocationDistance b)
            {
                return a.dist - b.dist;
            });
            int min_distance = target_distances[0].dist;
            // Remove targets that are further from the closest pillage location
            List<int> new_targets = new List<int>();
            foreach (int id in targets)
            {
                Game_Unit target_unit = Global.game_map.units[id];
                List<LocationDistance> distances = distance_to_locations(attacker, target_locs, true, target_unit.loc);
                if (distances.Count > 0)
                {
                    distances.Sort(delegate(LocationDistance a, LocationDistance b)
                    {
                        return a.dist - b.dist;
                    });
                    if (distances[0].dist < min_distance)
                        new_targets.Add(id);
                }
            }*/
            // Check the viability of each target
            List<int[]> target_ary = check_targets(attacker, targets, weapons, can_move, best, damage_test: false);
            if (target_ary.Count == 0)
                return null;
            target_ary.Sort(delegate(int[] a, int[] b) { return b[2] - a[2]; });
            return target_ary[0];
        }

        public static bool can_kill_target(Game_Unit attacker, List<int> targets, List<int> weapons, bool can_move)
        {
            // Check the viability of each target
            List<int[]> target_ary = check_targets(attacker, targets, weapons, can_move, true, damage_test: true);
            if (target_ary.Count == 0) return false;
            foreach (int[] ary in target_ary)
                if (ary[2] >= ATTACK_WEIGHT_PRECISION)
                    return true;
            return false;
        }

        public static List<int[]> check_targets(Game_Unit attacker, List<int> targets,
            List<int> weapons, bool can_move, bool best, bool retreating = false,
            bool damage_test = false)
        {
            List<int[]> test_ary = new List<int[]>();
            // Makes an array for each weapon and enemy combination, and figures out the
            // usefulness of attacking there
            // [enemy, weapon, usefulness, x, y]
            List<int[]> target_ary = new List<int[]>();
            Game_Actor actor1 = attacker.actor;

            targets = check_targets_target_list(attacker, targets, best);

            Maybe<Vector2> retreat_loc = default(Maybe<Vector2>);
            if (retreating)
                retreat_loc = retreat(attacker, no_move_okay: false);

            // Cycle through targets
            foreach (int id in targets)
            {
                // This method doesn't check destroyable terrain, just units, so break on non-units
                if (!Global.game_map.attackable_map_object(id).is_unit())
                    continue;
                Game_Unit target = Global.game_map.units[id];
                // Next if off map or target dead???
                if (Global.game_map.is_off_map(target.loc) || target.is_dead)
                    continue;

                Game_Actor actor2 = target.actor;
                // Cycle through weapons
                foreach (int weapon_index in weapons)
                {
                    int distance = Global.game_map.unit_distance(attacker.id, id);
                    Data_Weapon weapon1 = null, weapon2 = null;
                    int min_range, max_range;
                    bool ever_counter;

                    #region Regular Weapons
                    if (weapon_index != Constants.Actor.NUM_ITEMS)
                    {
                        // Gets range
                        if (Global.data_weapons.ContainsKey(actor1.items[weapon_index].Id))
                            weapon1 = Global.data_weapons[actor1.items[weapon_index].Id];
                        min_range = attacker.min_range(weapon_index);
                        max_range = attacker.max_range(weapon_index);

                        attacker.reset_ai_loc();
                        HashSet<Vector2> attack_move_range = new HashSet<Vector2> { attacker.loc };
                        // If just checking damage as a percent of target health
                        if (damage_test)
                        {
                            ever_counter = true;
                            weapon2 = actor2.weapon;
                        }
                        else
                        {
                            if (can_move)
                            {
                                // If unable to hit the target, continue
                                if (!can_move_to_hit(target, Global.game_state.ai_move_range, attacker, weapon_index))
                                    continue;
                                attack_move_range = attacker.hit_from_loc(target.loc, Global.game_state.ai_move_range, weapon_index, "");
                                // If retreat attacking
                                if (retreating)
                                {
                                    if (retreat_loc.IsSomething)
                                    {
                                        // Remove tiles from the attacking range that are not already on the way to the retreat point
                                        attack_move_range = new HashSet<Vector2>(attack_move_range.Where(x =>
                                        {
                                            var move_cost = Pathfind.get_distance(x, attacker.id, attacker.mov, false, attacker.loc);
                                            // If somehow can't move to the target tile (what), break
                                            if (move_cost.IsNothing)
                                                return false;
                                            else
                                                return Pathfind.get_distance(retreat_loc, attacker.id, attacker.mov - move_cost, false, x).IsSomething;
                                        }));
                                    }

                                    /*// Remove tiles from the attacking range that the attacker cannot then retreat to safety from //Debug
                                    attack_move_range = new HashSet<Vector2>(attack_move_range.Where(x =>
                                    {
                                        var move_cost = Pathfinding.get_distance(x, attacker.id, attacker.mov, false, attacker.loc);
                                        // If somehow can't move to the target tile (what), break
                                        if (!move_cost.Key)
                                            return false;
                                        var post_attack_move_range = Pathfinding.get_range(x, attacker.mov - move_cost.Value, attacker.id, x);
                                        return post_attack_move_range.Except(Global.game_state.ai_enemy_attack_range).Any();
                                    }));*/
                                }
                            }
                            else
                            {
                                // If the target can't be hit from this tile, continue
                                if (distance > max_range || distance < min_range ||
                                        !attacker.get_weapon_range(new List<int> { weapon_index }, new HashSet<Vector2> { attacker.loc }, "").Contains(target.loc))
                                    continue;
                            }

                            // For the target to ever counter, this weapon can't disallow counters and the target's attack range has to have overlap
                            ever_counter = !weapon1.No_Counter &&
                                !(target.min_range_absolute() > attacker.max_range(weapon_index) ||
                                target.max_range_absolute() < attacker.min_range(weapon_index));
                            if (weapon1.No_Counter)
                            {
                                // Pares attack range to best terrain types
                                // This allows selecting for the best damage output as well, not just for best defensive terrain
                                best_terrain(ref attack_move_range);
                            }
                        }

                        // Cycle through locations that can be attacked from
                        foreach (Vector2 loc in attack_move_range)
                        {
                            attacker.reset_ai_loc();
                            if (loc.X != Config.OFF_MAP.X)
                            {
                                Maybe<int> move_distance = Pathfind.get_distance(loc, attacker.id, attacker.canto_mov, false, attacker.loc);
                                if (move_distance.IsSomething)
                                {
                                    attacker.set_ai_base_loc(loc, move_distance);
                                    distance = (int)(Math.Abs(loc.X - target.loc.X) + Math.Abs(loc.Y - target.loc.Y));
                                }
                            }

                            target.target_unit(attacker, weapon1, distance);

                            //test_ary.push blah blah
                            var use = attack_use(attacker, target, weapon_index, weapon1, loc, ever_counter, distance, best, damage_test);
                            if (use.IsNothing)
                                continue;
                            target_ary.Add(new int[] { id, weapon_index + 1, (int)use, (int)loc.X, (int)loc.Y });
                            target.cancel_targeted();
                        }
                    }
                    #endregion
                    #region Siege Weapons
                    else
                    {
                        attacker.reset_ai_loc();
                        HashSet<Vector2> siege_move_range = can_move ? Global.game_map.remove_blocked(
                            Global.game_state.ai_move_range, attacker.id) : new HashSet<Vector2> { attacker.loc };
                        foreach (Siege_Engine siege in Global.game_map.siege_engines.Values)
                        {
                            // If siege engine has uses and is in the move range and can be equipped
                            if (siege.is_ready && siege_move_range.Contains(siege.loc) &&
                                actor1.is_equippable_as_siege(Global.data_weapons[siege.item.Id]))
                            {
                                Maybe<int> move_distance = Pathfind.get_distance(siege.loc, attacker.id, attacker.canto_mov, false, attacker.loc);
                                if (move_distance.IsSomething)
                                {
                                    // Set location to the siege engine and fix distance
                                    attacker.set_ai_base_loc(siege.loc, move_distance);
                                    distance = (int)(Math.Abs(siege.loc.X - target.loc.X) + Math.Abs(siege.loc.Y - target.loc.Y));

                                    if (Global.data_weapons.ContainsKey(attacker.items[weapon_index].Id))
                                        weapon1 = Global.data_weapons[attacker.items[weapon_index].Id];
                                    min_range = attacker.min_range(weapon_index);
                                    max_range = attacker.max_range(weapon_index);

                                    // If just checking damage as a percent of target health
                                    if (damage_test)
                                    {
                                        ever_counter = true;
                                        weapon2 = actor2.weapon;
                                    }
                                    else
                                    {
                                        if (distance > max_range || distance < min_range ||
                                            !attacker.get_weapon_range(new List<int> { weapon_index }, new HashSet<Vector2> { siege.loc }, "").Contains(target.loc))
                                        {
                                            attacker.reset_ai_loc();
                                            continue;
                                        }

                                        // For the target to ever counter, this weapon can't disallow counters and the target's attack range has to have overlap
                                        ever_counter = !weapon1.No_Counter &&
                                            !(target.min_range_absolute() > attacker.max_range(weapon_index) ||
                                            target.max_range_absolute() < attacker.min_range(weapon_index));
                                    }

                                    target.target_unit(attacker, weapon1, distance);

                                    var use = attack_use(attacker, target, weapon_index, weapon1, siege.loc, ever_counter, distance, best, damage_test);
                                    if (use.IsNothing)
                                    {
                                        attacker.reset_ai_loc();
                                        continue;
                                    }
                                    target_ary.Add(new int[] { id, weapon_index + 1, (int)use, (int)siege.loc.X, (int)siege.loc.Y });
                                    target.cancel_targeted();

                                    attacker.reset_ai_loc();
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
            attacker.reset_ai_loc();
            return target_ary;
        }

        private static List<int> check_targets_target_list(Game_Unit attacker, List<int> targets, bool best)
        {
            if (targets.Count > 0)
            {
                // If attacker is CRAZY attack closest target regardless, then decide best weapon as normal
                if (attacker.berserk)
                {
                    int temp_target = 0;
                    int distance = Global.game_map.unit_distance(attacker.id, targets[temp_target]);
                    for (int i = 1; i < targets.Count; i++)
                    {
                        int dist = Global.game_map.unit_distance(attacker.id, targets[i]);
                        if (dist < distance)
                        {
                            distance = dist;
                            temp_target = i;
                        }
                    }
                    targets = new List<int> { targets[temp_target] };
                }
                // Else if not searching for best target, randomly select a target, then decide the best weapon as normal
                else if (!best)
                {
                    int temp_target = (int)((Global.game_state.ai_turn_rn / 100f) * targets.Count);
                    targets = new List<int> { targets[temp_target] };
                }
            }
            return targets;
        }

        protected static Maybe<int> attack_use(
            Game_Unit attacker, Game_Unit target, int weapon_index, Data_Weapon weapon1,
            Vector2 loc, bool ever_counter, int distance, bool best, bool damage_test)
        {
            Game_Actor actor1 = attacker.actor;
            Game_Actor actor2 = target.actor;
            Data_Weapon weapon2 = actor2.weapon;
            bool counter = target.can_counter(attacker, weapon1, distance);

            float[] use = Combat.damage_per_round(attacker, target, new List<int> { distance }, weapon_index, Global.game_system.hard_mode);
            if (weapon1.is_staff())
            {
                use[0] *= target.threat();
                use[0] *= weapon1.Staff_Exp;
            }
            else
            {
                // Tests if unit causes no damage but can inflict status effects
                if (use[0] <= 0 && best)
                {
                    /*if (weapon1.Status_Inflict.Count > 0)
                    {
                        int hit1 = 0;
                        List<int?> stats = (Combat.combat_stats(attacker.id, target.id, distance, weapon_index));
                        // If hit is not null
                        if (stats[0] != null)
                            hit1 = Math.Max(hit1, (int)stats[0]);
                        if (hit1 <= 0)
                            return Maybe<int>.Nothing;
                        use[0] = 0.05f * hit1;
                    }
                    else
                        return Maybe<int>.Nothing;*/

                    if (!weapon1.Status_Inflict.Any())
                        return Maybe<int>.Nothing;
                }
                bool target_can_kill = false;
                var stats = new CombatStats(
                    attacker.id, target.id, weapon1, distance);
                var target_stats = new CombatStats(
                    target.id, attacker.id, weapon2, distance);

                if (!counter)
                    use[1] = 0;
                // If target could die after the first attack 
                else if (use[0] / attacker.attacks_per_round(target, distance, weapon_index) >= actor2.hp && !target.counters_first(attacker))
                {
                    // Multiply target's use value by their chance of surviving
                    int hit = Math.Min(100, stats.hit());
                    use[1] *= (100 - (Global.game_system.hard_mode ? Combat.true_hit(hit) : hit)) / 100f;
                }
                // Else if attacker could die on first counter
                // This doesn't take crits into account, might need to modify //Debug
                // Also doesn't account for brave //Debug
                else if (target_stats.dmg() >= actor1.hp) // This needs to account for Trooper Anticipation //it should now //Debug
                {
                    target_can_kill = true;
                    // Reduce use to reflect overkill being meaningless; thus if all targets can kill, damage will at least be maximized
                    int hit = Math.Min(100, target_stats.hit());
                    use[1] = (Global.game_system.hard_mode ? Combat.true_hit(hit) : hit) * actor1.hp / 100f *
                        target.attacks_per_round(attacker, distance) * 3;
                    // If target will attack first
                    if (target.counters_first(attacker))
                    {
                        // Multiply attackers's use value by their chance of surviving
                        use[0] *= (100 - (Global.game_system.hard_mode ? Combat.true_hit(hit) : hit)) / 100f;
                    }
                }
                // If just checking damage as a percent of target health
                if (damage_test)
                { }
                else
                {
                    if (use[0] >= actor2.hp)
                        use[0] *= 3;
                    if (use[1] >= actor1.hp && !target_can_kill)
                        use[1] *= 3;

                    // Add a bonus to use if inflicting a status
                    if (weapon1.Status_Inflict.Count > 0)
                    {
                        float hit1 = 0;
                        // If hit is not null
                        if (stats.has_weapon)
                            hit1 = Math.Max(hit1, stats.hit());
                        use[0] += Math.Min(0.2f, actor2.maxhp * 0.004f) * hit1;
                    }
                }
            }
            float total_use;
            // If just checking damage as a percent of target health
            if (damage_test)
            {
                total_use = use[0] / actor2.hp;
            }
            else
            {
                // If can't see tile being moved to, halve use
                if (Global.game_map.fow && !Global.game_map.fow_visibility[attacker.team].Contains(loc))
                    use[0] /= 2;
                // These two might be better after combining the use values? //Yeti
                // If the target can't counter now
                if (!counter) use[0] *= 1.5f;
                // If the target can't counter ever
                if (!ever_counter) use[0] *= 1.5f;

                // If can attack player team
                if (!attacker.is_player_allied)
                {
                    // If the target's death would cause the player to lose
                    if (target.loss_on_death)
                        use[0] *= 1.5f;
                }
                // If on allied team
                else
                {
                    // If the target's death would cause the player to lose
                    if (target.loss_on_death)
                        use[0] /= 1.5f;
                }

                if (weapon1.is_staff())
                    total_use = use[0];
                else
                {
                    if (Global.game_system.hard_mode) // Less aggressive version was always used before //Debug
                        total_use = (float)(Math.Pow(use[0] / actor2.hp, 2) - Math.Pow(use[1] / actor1.hp, 2)); // Less aggressive when no good target
                    else
                        total_use = (float)(Math.Pow(use[0] / actor2.hp, 0.5f) - Math.Pow(use[1] / actor1.hp, 0.5f)); // More aggressive when no good target //Debug
                }
            }
            return (int)(total_use * ATTACK_WEIGHT_PRECISION);
        }

        protected static Maybe<int> healing_staff_use(
            Game_Unit staff_user, Game_Unit target, int staff_index, Data_Weapon staff,
            List<int> distances)
        {
            // Count each status removed as healing 20 hp
            const int NEGATIVE_STATE_REMOVAL_VALUE = 20;
            Game_Actor actor2 = target.actor;

            int heal_amount = 0;
            if (staff.Heals())
            {
                if (actor2.is_full_hp() && actor2.negative_states.Count == 0)
                    return Maybe<int>.Nothing;

                Dictionary<int, List<int?>> stats = Combat.combat_stats(
                    staff_user.id, target.id, distances, staff_index);
                int pow = 0;
                foreach (KeyValuePair<int, List<int?>> pair in stats)
                {
                    if (pair.Value[1] != null)
                        pow = Math.Max(pow, -(int)pair.Value[1]);
                }
                // Amount using this staff will heal
                heal_amount += Math.Min(actor2.hp + pow, actor2.maxhp) - actor2.hp;
            }
            if (staff.Status_Remove.Count > 0)
            {
                int healed_states = staff.Status_Remove
                    .Intersect(actor2.negative_states).ToList().Count;
                heal_amount += NEGATIVE_STATE_REMOVAL_VALUE * healed_states;
            }
            // If applies any positive states
            if (staff.Status_Inflict.Count > 0 &&
                staff.Status_Inflict.Any(x => !Global.data_statuses[x].Negative))
            {
                // If the target already has all the statuses this staff applies,
                // and they still have more than half the turns left,
                // don't reapply them
                bool should_apply_status = false;
                if (staff.Status_Inflict.Except(actor2.states)
                        .Any(x => !Global.data_statuses[x].Negative))
                    should_apply_status = true;

                if (!should_apply_status)
                    foreach (var existing_status in actor2.states.Intersect(staff.Status_Inflict))
                    {
                        int status_turns = Global.data_statuses[existing_status].Turns;
                        if (status_turns > 0 &&
                            actor2.state_turns_left(existing_status) <= status_turns / 2)
                        {
                            should_apply_status = true;
                            break;
                        }
                    }

                if (should_apply_status)
                {
                    // Use exp value of the staff as an analogue for the power
                    // of the status it applies converted into healing value
                    heal_amount += staff.Staff_Exp;
                }
            }

            float total_use;
            if (staff.Heals())
            {
                int missing_hp = actor2.maxhp - actor2.hp;
                missing_hp += NEGATIVE_STATE_REMOVAL_VALUE * actor2.negative_states.Count;

                // Reduce the value of cases that would not heal fully
                float use = (float)Math.Pow(heal_amount, 2);
                total_use = use / missing_hp;
            }
            else
            {
                total_use = heal_amount;
            }

            if (total_use <= 0)
                return Maybe<int>.Nothing;

            // Multiply the result by target threat,
            // effectively meaning the AI will prefer to boost their strongest allies
            total_use *= target.threat();

            // Prefer helping the player
            if (target.is_player_team)
                total_use *= 2;
            else
            {
                // Prefer combat units
                if (ATTACK_MISSIONS.Contains(target.ai_mission))
                    total_use *= 2;
                // Ignore immobile units
                if (UNMOVING_MISSIONS.Contains(target.ai_mission))
                    total_use /= 2;
                // Ignore immobile units
                if (IMMOBILE_MISSIONS.Contains(target.ai_mission))
                    total_use /= 2;
            }

            return (int)(total_use * ATTACK_WEIGHT_PRECISION);
        }

        public static int[] get_heal_target(Game_Unit staff_user, List<int> targets,
            List<int> staves, bool can_move = false, bool weak_only = true)
        {
            // Makes an array for each weapon and enemy combination, and figures out the
            // usefulness of attacking there
            // [enemy, weapon, usefulness]
            List<int[]> target_ary = new List<int[]>();
            Game_Actor actor1 = staff_user.actor;
            foreach (int id in targets)
            {
                Game_Unit target = Global.game_map.units[id];
                if (Global.game_map.is_off_map(target.loc))
                    continue;
                Game_Actor actor2 = target.actor;
                if (weak_only && actor2.negative_states.Count == 0 && !actor2.has_critical_health())
                    continue;
                int distance = Global.game_map.unit_distance(staff_user.id, id);
                foreach (int staff_index in staves)
                {
                    Data_Weapon staff = Global.data_weapons[actor1.items[staff_index].Id];
                    // Get ranges to attack from
                    List<int> atk_distance = new List<int>();
                    int max_range = staff_user.max_range(staff_index);
                    for (int i = staff_user.min_range(staff_index); i <= max_range; i++)
                        atk_distance.Add(i);
                    // Skip if can't actually get into range for this target
                    if (can_move)
                    {
                        bool can_hit = can_move_to_hit(
                            target, Global.game_state.ai_move_range,
                            staff_user, staff_index, false, avoidEnemyRange: false);
                        if (!can_hit)
                            continue;
                    }
                    else
                    {
                        if (!atk_distance.Contains(distance))
                            continue;
                    }
                    var use =  healing_staff_use(
                        staff_user, target, staff_index, staff, atk_distance);
                    if (use.IsNothing)
                        continue;

                    target_ary.Add(new int[] { id, staff_index + 1, (int)use });
                }
            }
            if (target_ary.Count == 0)
                return null;
            sort_use(ref target_ary);
            return target_ary[0];
        }

        public static int[] get_attack_staff_target(Game_Unit staff_user, List<int> targets, List<int> staves, bool can_move)
        {
            // Makes an array for each weapon and enemy combination, and figures out the
            // usefulness of attacking there
            // [enemy, weapon, usefulness]
            List<int[]> target_ary = new List<int[]>();
            Game_Actor actor1 = staff_user.actor;
            foreach (int id in targets)
            {
                Game_Unit target = Global.game_map.units[id];
                if (Global.game_map.is_off_map(target.loc))
                    continue;
                Game_Actor actor2 = target.actor;
                int distance = Global.game_map.unit_distance(staff_user.id, id);
                foreach (int staff_index in staves)
                {
                    Data_Weapon staff = Global.data_weapons[actor1.items[staff_index].Id];
                    // Ignore immobile non-combat units
                    if (target.actor.mov == 0 && target.actor.is_non_combat()) //not 100% sure about how this handles //Yeti
                        continue;
                    // If there are no states to remove
                    if (!staff.Status_Remove.Except(actor2.states).Any())
                    {
                        // And there are no states worth adding
                        bool not_worth_casting = true;
                        for (int i = 0; i < staff.Status_Inflict.Count; i++)
                        {
                            if (actor2.state_turns_left(staff.Status_Inflict[i]) <= 1)
                                if (Global.data_statuses[staff.Status_Inflict[i]].No_Magic ? target.has_magic() : true)
                                {
                                    not_worth_casting = false;
                                    break;
                                }
                        }
                        if (not_worth_casting)
                            continue;
                    }
                    // Gets attack range
                    List<int> atk_distance = new List<int>();
                    int min_range = staff_user.min_range(staff_index);
                    int max_range = staff_user.max_range(staff_index);
                    HashSet<Vector2> attack_range = new HashSet<Vector2> { staff_user.loc };

                    if (can_move)
                    {
                        bool can_hit = can_move_to_hit(target, Global.game_state.ai_move_range, staff_user, staff_index);
                        if (!can_hit) continue;
                        attack_range = staff_user.hit_from_loc(target.loc, Global.game_state.ai_move_range, staff_index, "");
                    }
                    else
                    {
                        if (distance > max_range || distance < min_range ||
                            !staff_user.get_weapon_range(
                                new List<int> { staff_index },
                                new HashSet<Vector2> { staff_user.loc },
                                "").Contains(target.loc)) continue;
                    }
                    // Pares attack range to best terrain types
                    // This allows selecting for the best damage output as well, not just for best defensive terrain
                    best_terrain(ref attack_range);

                    // Cycle through locations that can be attacked from
                    foreach (Vector2 loc in attack_range)
                    {
                        staff_user.reset_ai_loc();
                        if (loc.X != Config.OFF_MAP.X)
                        {
                            Maybe<int> move_distance = Pathfind.get_distance(loc, staff_user.id, staff_user.canto_mov, false, staff_user.loc);
                            if (move_distance.IsSomething)
                            {
                                staff_user.set_ai_base_loc(loc, move_distance);
                                distance = (int)(Math.Abs(loc.X - target.loc.X) + Math.Abs(loc.Y - target.loc.Y));
                            }
                        }

                        target.target_unit(staff_user, staff, distance);

                        var use = attack_use(staff_user, target, staff_index, staff, loc, false, distance, true, false);
                        if (use.IsNothing)
                            continue;
                        target_ary.Add(new int[] { id, staff_index + 1, (int)use, (int)loc.X, (int)loc.Y });
                        target.cancel_targeted();
                    }



                    /*Dictionary<int, List<int?>> stats = Combat.combat_stats(staff_user.id, id, atk_distance, staff_index);
                    int pow = 0;
                    foreach (KeyValuePair<int, List<int?>> pair in stats)
                    {
                        if (pair.Value[1] != null)
                            pow = Math.Max(pow, -(int)pair.Value[1]);
                    }
                    int heal_hp = (Math.Min(actor2.hp + pow, actor2.maxhp) - actor2.hp) +
                        (20 * (staff.Status_Remove.Intersect(actor2.negative_states).ToList().Count));
                    int missing_hp = actor2.maxhp - actor2.hp + 20 * actor2.negative_states.Count;
                    float use = (float)Math.Pow(heal_hp, 2);
                    use /= missing_hp;
                    target_ary.Add(new int[] { id, staff_index + 1, (int)(use * ATTACK_WEIGHT_PRECISION) });*/
                }
            }
            staff_user.reset_ai_loc();

            if (target_ary.Count == 0) return null;
            sort_use(ref target_ary);
            return target_ary[0];
        }

        public static int[] get_untargeted_staff_target(Game_Unit staff_user, List<int> staves, bool can_move)
        {
            // Makes an array for each weapon and enemy combination, and figures out the
            // usefulness of attacking there
            // [enemy, weapon, usefulness]
            List<int[]> target_ary = new List<int[]>();
            Game_Actor actor1 = staff_user.actor;
            int distance;
            HashSet<int> targets = new HashSet<int>();
            foreach (int team in staff_user.attackable_teams())
                targets.UnionWith(Global.game_map.teams[team]); //HashSet

            foreach (int staff_index in staves)
            {
                Data_Weapon staff = Global.data_weapons[actor1.items[staff_index].Id];
                // Gets attack range
                List<int> atk_distance = new List<int>();
                int min_range = staff_user.min_range(staff_index);
                int max_range = staff_user.max_range(staff_index);
                HashSet<Vector2> move_range = can_move ? Global.game_state.ai_move_range : new HashSet<Vector2> { staff_user.loc };

                if (staff.Torch())
                {
                    List<int[]> torch_target_ary = new List<int[]>();
                    bool anyone_in_range = false;
                    HashSet<Vector2> hit_range = staff_user.get_weapon_range(new List<int> { staff_index }, move_range, "");
                    HashSet<Vector2> torch_range = Pathfind.get_range_around(
                        hit_range, Map.Torch_Staff_Point.initial_vision(), 0,
                        Constants.Gameplay.BLOCK_VISION_THROUGH_WALLS);
                    foreach (int id in targets)
                        if (torch_range.Contains(Global.game_map.units[id].loc))
                        {
                            anyone_in_range = true;
                            break;
                        }
                    if (!anyone_in_range)
                        continue;
                    foreach(Vector2 hit in hit_range)
                    //for (int i = 0; i < hit_range.Count; i++) //HashSet
                    {
                        torch_range = Pathfind.get_range_around(
                            new HashSet<Vector2> { hit }, Map.Torch_Staff_Point.initial_vision(), 0,
                            Constants.Gameplay.BLOCK_VISION_THROUGH_WALLS);
                        int in_range = 0;
                        foreach (int id in targets)
                            if (torch_range.Contains(Global.game_map.units[id].loc))
                            {
                                in_range++;
                            }
                        if (in_range == 0)
                            continue;
                        torch_target_ary.Add(new int[] {
                            (int)hit.X + (int)hit.Y * Global.game_map.width,
                            staff_index + 1,
                            in_range });
                    }
                    sort_use(ref torch_target_ary);
                    // Gets just the 8 best targets
                    torch_target_ary = torch_target_ary.GetRange(0, Math.Min(torch_target_ary.Count, 8));
                    int target_index = (int)((Global.game_state.ai_turn_rn / 100f) * torch_target_ary.Count);
                    Vector2 target = new Vector2(
                        torch_target_ary[target_index][0] % Global.game_map.width,
                        torch_target_ary[target_index][0] / Global.game_map.width);
                    int target_count = torch_target_ary[target_index][2];

                    if (can_move)
                        move_range = staff_user.hit_from_loc(target, Global.game_state.ai_move_range, staff_index, "");

                    // Pares attack range to best terrain types
                    // This allows selecting for the best damage output as well, not just for best defensive terrain
                    best_terrain(ref move_range);

                    // Cycle through locations that can be attacked from
                    foreach (Vector2 loc in move_range)
                    {
                        staff_user.reset_ai_loc();
                        if (loc.X != Config.OFF_MAP.X)
                        {
                            Maybe<int> move_distance = Pathfind.get_distance(loc, staff_user.id, staff_user.canto_mov, false, staff_user.loc);
                            if (move_distance.IsSomething)
                                staff_user.set_ai_base_loc(loc, move_distance);
                        }
                        target_ary.Add(new int[] {
                            (int)target.X + (int)target.Y * Global.game_map.width,
                            staff_index + 1,
                            (int)target_count * 10,
                            (int)loc.X,
                            (int)loc.Y });
                    }
                }
                else continue;
            }
            staff_user.reset_ai_loc();

            if (target_ary.Count == 0)
                return null;
            sort_use(ref target_ary);
            return target_ary[0];
        }

        public static int[] get_steal_target(Game_Unit stealer, List<int> targets, HashSet<Vector2> move_range)
        {
            int highest_cost = -1;
            int best_target = -1, item_id = -1;
            foreach (int id in targets)
            {
                Game_Unit target = Global.game_map.units[id];
                if (Pathfind.hit_from_loc(target.loc, move_range, 1, 1).Count == 0)
                    continue;

                for (int i = 0; i < target.actor.items.Count; i++)
                {
                    // Tests if the thief can actually steal this item
                    if (!stealer.can_steal_item(target, i))
                        continue;
                    Item_Data item_data = target.actor.items[i];
                    //if (item_data[0] == 0) // Have a constant flag for what items can/can't be stolen (weapons, equipped things, etc) //Yeti
                    //    continue;
                    Data_Equipment item = item_data.to_equipment;
                    int cost = Math.Max(item.Cost * (item.Uses <= 0 ? 1 : item_data.Uses), 1);
                    if (cost > highest_cost)
                    {
                        highest_cost = cost;
                        best_target = id;
                        item_id = i;
                    }
                }
            }
            return new int[] { best_target, item_id };
        }

        public static int get_dance_target(Game_Unit dancer, List<int> targets, HashSet<Vector2> move_range)
        {
            int highest_priority = -1;
            int best_target = -1;
            foreach (int id in targets)
            {
                Game_Unit target = Global.game_map.units[id];
                if (Pathfind.hit_from_loc(target.loc, move_range, 1, 1).Count == 0)
                    continue;

                int priority = (target.actor.rating() * (target.actor.hp + target.actor.maxhp)) / (target.actor.maxhp * 2);
                priority *= ATTACK_WEIGHT_PRECISION;
                // If the target is only a healer
                if (target.is_player_team ? target.actor.is_staff_only() : target.actor.staff_fix())
                {
                    HashSet<Vector2> target_move_range = target.move_range;
                    int target_count = target.allies_in_staff_range(target_move_range)[0]
                        .Union(target.enemies_in_staff_range(target_move_range)[0])
                        .Where(unit_id => target.attack_range.Contains(Global.game_map.units[unit_id].loc)).Count();
                    priority = (int)(priority * Math.Sqrt(target_count + 1));
                }
                // If the target can fight
                else
                {
                    int target_count = target.get_attackable_units()
                        .Where(unit_id => target.attack_range.Contains(Global.game_map.units[unit_id].loc)).Count();
                    priority = (int)(priority * Math.Sqrt(target_count + 1));
                }
                if (priority > highest_priority)
                {
                    highest_priority = priority;
                    best_target = id;
                }
            }
            return best_target;
        }

        protected static void sort_use(ref List<int[]> target_ary)
        {
            target_ary.Sort(delegate(int[] a, int[] b) { return b[2] - a[2]; });
        }

        public static List<int> get_enemies_toward_target(Game_Unit unit, Vector2 target_loc, List<int> targets, bool limited_access = false)
        {
            return get_enemies_toward_target(unit, new HashSet<Vector2> { target_loc }, targets, limited_access);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="target_locs"></param>
        /// <param name="targets"></param>
        /// <param name="limited_access">If true, access to the target locations is limited and getting into any of them is paramount.
        /// If false, just head to the closest one.</param>
        /// <returns></returns>
        public static List<int> get_enemies_toward_target(Game_Unit unit, HashSet<Vector2> target_locs, List<int> targets, bool limited_access = false)
        {
            List<LocationDistance> target_distances = Game_AI.distance_to_locations(unit, target_locs, true);
            target_distances.Sort(delegate(LocationDistance a, LocationDistance b)
            {
                return a.dist - b.dist;
            });
            int min_distance = target_distances[0].dist - 1; // (int)Math.Ceiling(unit.mov / 2.0f); //Debug
            Pathfind.ignore_units = true;
            List<Vector2> path = unit.actual_move_route(Pathfind.get_route(target_distances[0].loc, -1, unit.id, unit.loc, true));
            Pathfind.reset();
            // Remove targets that are further from the closest pillage location
            return targets.Where(id =>
                {
                    Game_Unit target_unit = Global.game_map.units[id];
                    // If the unit is just straight up in the way of the path to the target, take 'em out
                    if (path.Contains(target_unit.loc))
                        return true;
                    List<LocationDistance> distances = distance_to_locations(unit,
                        limited_access ? target_locs : new HashSet<Vector2> { target_distances[0].loc }, true, target_unit.loc);
                    if (distances.Count > 0)
                    {
                        distances.Sort(delegate(LocationDistance a, LocationDistance b)
                        {
                            return a.dist - b.dist;
                        });
                        if (distances[0].dist < min_distance || (limited_access && distances[0].dist <= target_unit.mov))
                            return true;
                    }
                    return false;
                }).ToList();
        }
        #endregion

        #region Searching for targets
        /// <summary>
        /// Returns locations this unit might want to go to, based on their ai_mission.
        /// Can be sorted for viability later.
        /// </summary>
        public static HashSet<Vector2> target_locations(Game_Unit unit)
        {
            switch (unit.ai_mission)
            {
                case 4: // Pillage
                    // Valid villages
                    return new HashSet<Vector2>(
                        pillage_targets(unit)
                         .Select(x => x.loc));
                case 5: // Thief
                    // Chest Locations
                    if (Global.game_map.chest_locations.Any())
                        return new HashSet<Vector2>(
                            Global.game_map.chest_locations
                                .Select(x => x.Key));
                    // No chests, so thief escape points
                    else
                        return new HashSet<Vector2>(
                            Global.game_map.thief_escape_points
                                .Select(x => x.Key));
                case 6: // Defend Area
                    var defend_area = new HashSet<Vector2>(
                        State.Game_Ai_State.defend_area(unit, unit.team));
                    // Enemies in the defend area
                    var enemies_in_defend_area = Global.game_map.units
                        .Where(x => unit.is_attackable_team(x.Value))
                        .Where(x => defend_area.Contains(x.Value.loc));
                    if (enemies_in_defend_area.Any())
                    {
                        return new HashSet<Vector2>(
                            enemies_in_defend_area
                                .Select(x => x.Value.loc));
                    }
                    // Any part of the defend area
                    else
                        return defend_area;
                case 8: // Talk
                    var potential_talkers = Global.game_state.talk_targets(unit.id);
                    return new HashSet<Vector2>(
                        potential_talkers
                        .Select(x => Global.game_map.units[x].loc));
                case 9: // Seek tile
                    // Look for places this specific unit wants to move to
                    if (Global.game_map.unit_seek_locs.ContainsKey(unit.id))
                        return new HashSet<Vector2> {
                            Global.game_map.unit_seek_locs[unit.id] };
                    // Look for places this unit's team group wants to move to
                    if (Global.game_map.team_seek_locs.ContainsKey(unit.team) &&
                            Global.game_map.team_seek_locs[unit.team].ContainsKey(unit.group))
                        return new HashSet<Vector2> {
                            Global.game_map.team_seek_locs[unit.team][unit.group] };
                    break;
                case 10: // Do nothing
                    break;
                case 13: // FoW Sentry
                    return  new HashSet<Vector2>(
                        Enumerable.Range(0, Global.game_map.width)
                            .SelectMany(x => Enumerable.Range(0, Global.game_map.height)
                                .Select(y => new Vector2(x, y)))
                            .Where(loc => !Global.game_state
                                .ai_enemy_attack_range.Contains(loc)));
                case 15: // Escape
                    return Global.game_map.escape_point_locations(unit.team, unit.group);
                case 7: // Staff user
                case 14: // Savior
                case 16: // Dancer
                    // Allies
                    return  new HashSet<Vector2>(
                        Global.game_map.units
                            .Where(x => !unit.is_attackable_team(x.Value))
                            .Select(x => x.Value.loc));
                default:
                case 0: // Still
                case 1: // Attack in range
                case 2: // Seek and attack any
                case 3: // Seek and attack
                    // Enemies
                    return  new HashSet<Vector2>(
                        Global.game_map.units
                            .Where(x => unit.is_attackable_team(x.Value))
                            .Select(x => x.Value.loc));

            }

            return null;
        }

        public static List<HashSet<Vector2>> group_locations(Game_Unit unit, IEnumerable<Vector2> locs)
        {
            var temp_locs = locs.ToList();
            List<HashSet<Vector2>> result = new List<HashSet<Vector2>>();

            var unit_map = new UnitMovementMap(unit.id, ignoreUnits: true, throughDoors: true);
            while (temp_locs.Any())
            {
                Vector2 loc = temp_locs[0];
                temp_locs.RemoveAt(0);
                HashSet<Vector2> group = new HashSet<Vector2>();
                group.Add(loc);
                // Group any locations that can be reached from this location
                for (int i = 0; i < temp_locs.Count;)
                {
                    if (unit_map.get_distance(temp_locs[i], -1, loc).IsSomething)
                    {
                        group.Add(temp_locs[i]);
                        temp_locs.RemoveAt(i);
                    }
                    else
                        i++;
                }
                result.Add(group);
            }
            return result;
        }

        private static List<Vector2> closest_group_targets(Vector2 vector2, List<HashSet<Vector2>> locGroups)
        {
            return locGroups.Select(x =>
            {
                return x.OrderBy(delegate(Vector2 group_loc)
                {
                    return Global.game_map.distance(vector2, group_loc);
                })
                .First();
            })
            .ToList();
        }

        public static Maybe<Vector2>[] search_for_enemy(Game_Unit unit)
        {
            return search_for_enemy(unit, 0, true, null);
        }
        public static Maybe<Vector2>[] search_for_enemy(Game_Unit unit, HashSet<Vector2> enemy_tiles)
        {
            return search_for_enemy(unit, 0, true, enemy_tiles);
        }
        public static Maybe<Vector2>[] search_for_enemy(Game_Unit unit, int mode, bool cares_about_damage)
        {
            return search_for_enemy(unit, mode, cares_about_damage, null);
        }
        private static Maybe<Vector2>[] search_for_enemy(Game_Unit unit, int mode, bool cares_about_damage, HashSet<Vector2> enemy_tiles)
        {
            // add defending area hashset
            // Gets targets that can be reached and their distance
            List<UnitDistance> searched_targets = search_for_target(unit);
            bool ignore_doors = false;
            // If no targets were found, try again looking through doors that this unit can't open; moving closer to doors would still be useful
            if (searched_targets.Count == 0)
            {
                if (Global.game_map.door_locations.Count > 0)
                {
                    searched_targets = search_for_target(unit, ignore_doors: true);
                    ignore_doors = true;
                }
            }
            if (searched_targets.Count == 0)
                return new Maybe<Vector2>[] { default(Maybe<Vector2>), default(Maybe<Vector2>) };
            // If defending an area, only check the ones in the area (unless that's nobody)
            if (enemy_tiles != null)
            {
                List<UnitDistance> defend_targets = new List<UnitDistance>();
                foreach (UnitDistance i in searched_targets)
                    if (enemy_tiles.Contains(i.unit.loc))
                        defend_targets.Add(i);
                if (defend_targets.Count > 0)
                    searched_targets = defend_targets;
                else
                    return new Maybe<Vector2>[] { default(Maybe<Vector2>), default(Maybe<Vector2>) };
            }
            // If only wanting to attack things that can be hurt
            if (cares_about_damage)
            {
                HashSet<UnitDistance> no_damage = new HashSet<UnitDistance>();
                foreach (UnitDistance i in searched_targets)
                {
                    // Temporarily adds the value to a list of targets that cannot be damaged
                    no_damage.Add(i);
                    for (int j = 0; j < Constants.Actor.NUM_ITEMS; j++)
                        if (unit.actor.items[j].is_weapon)
                        {
                            var weapon = unit.actor.items[j].to_weapon;
                            if (unit.actor.is_equippable(weapon))
                            {
                                var stats = new CombatStats(
                                    unit.id, i.UnitId, weapon, weapon.Min_Range);

                                if ((stats.dmg() > 0 || weapon.Status_Inflict.Any()) &&
                                    stats.hit() > 0)
                                {
                                    // Removes the target if they actually can be damaged
                                    no_damage.Remove(i);
                                    break;
                                }
                            }
                        }
                }
                // Removes all targets that can't be damaged
                searched_targets = searched_targets.Except(no_damage).ToList();
                // If that means the list is empty because no one can be damaged
                if (searched_targets.Count == 0)
                    return new Maybe<Vector2>[] { default(Maybe<Vector2>), default(Maybe<Vector2>) };
            }
            // Sorts the target units by toughness and distance
            searched_targets.Sort(delegate(UnitDistance a, UnitDistance b)
            {
                return sort_by_toughness_distance(unit, a, b);
            });

            // Selects a target
            Game_Unit target = searched_targets[0].unit;
            bool offensive = unit.actor.can_attack();
            // Try finding a path around enemies in the way first
            Maybe<Vector2> path_loc = path_to_target(
                unit, target.loc, offensive : offensive,
                no_move_okay: unit.cantoing, ignore_doors: ignore_doors,
                ignore_blocking: false);
            if (path_loc.IsNothing)
            {
                path_loc = path_to_target(
                    unit, target.loc, offensive: offensive,
                    no_move_okay: unit.cantoing, ignore_doors: ignore_doors,
                    ignore_blocking: true);
            }
            // If the found route requires going through a door but we don't actually have a key
            if (ignore_doors)
            {
                // If true this probably means this unit is standing right in front of the door, and should move out of the way
                if (path_loc.IsNothing) //Yeti
                {

                }
                else
                {
                    Vector2? door_target = Game_AI.door_target(unit, target.loc, path_loc, -1);
                    // If a door is in the way, but the door tile is not adjacent to the tile we would normally end our move on
                    if (door_target != null && Global.game_map.distance((Vector2)door_target, path_loc) != 1)
                        door_target = null;
                    // Doors are in the way this turn, don't move because we don't actually have a key
                    if (door_target != null)
                        return new Maybe<Vector2>[] { default(Maybe<Vector2>), default(Maybe<Vector2>) };
                }
            }
            return new Maybe<Vector2>[] { target.loc, path_loc };
        }

        public static Maybe<Vector2>[] search_for_seize(Game_Unit unit, bool ignore_blocking)
        {
            List<int[]> searched_points = search_for_seize_target(unit, ignore_blocking);
            if (searched_points.Count == 0)
                return new Maybe<Vector2>[] { default(Maybe<Vector2>), default(Maybe<Vector2>) };
            // Sorts the target units by toughness and distance
            searched_points.Sort(delegate(int[] a, int[] b)
            {
                return (
                    (100 * (a[2] / unit.mov + 1)) -
                    (100 * (b[2] / unit.mov + 1)));
            });
            // Selects a target
            Vector2 target_loc = new Vector2(searched_points[0][0], searched_points[0][1]);
            return new Maybe<Vector2>[] { target_loc, path_to_target(unit, target_loc, ignore_blocking: ignore_blocking) };
        }

        public static Maybe<Vector2>[] search_attack_through_walls(Game_Unit unit)
        {
            return search_attack_through_walls(unit, null);
        }
        public static Maybe<Vector2>[] search_attack_through_walls(Game_Unit unit, HashSet<Vector2> enemy_tiles)
        {
            // This method could probably be modified to have this as a HashSet //Yeti
            var pathfinder = new Pathfinding.UnitMovementMap(unit.id).Pathfind();
            List<Vector2> total_move_range = new List<Vector2>(
                pathfinder.get_range(unit.loc, unit.loc, -1));
            /* //Debug
            List<Vector2> total_move_range = new List<Vector2>(
                Pathfind.get_range(unit.loc, -1, unit.id));*/
            if (enemy_tiles != null)
            {
                total_move_range = total_move_range.Intersect(enemy_tiles).ToList();
                if (total_move_range.Count == 0)
                    total_move_range = new List<Vector2> { unit.loc };
            }
            // Get a list of all range distances the unit can attack from
            HashSet<int> ranges = new HashSet<int>(), siege_ranges = new HashSet<int>();
            foreach (int useable_weapon in unit.actor.useable_weapons())
            {
                int min = unit.min_range(useable_weapon);
                int max = unit.max_range(useable_weapon);
                for (int i = min; i <= max; i++)
                {
                    ranges.Add(i);
                    if (Global.data_weapons[unit.actor.items[useable_weapon].Id].is_siege())
                        siege_ranges.Add(i);
                }
            }

            // Sort enemies by distance
            List<int> target_team = target_units(unit, true);
            if (target_team.Count == 0)
                return new Maybe<Vector2>[] { default(Maybe<Vector2>), default(Maybe<Vector2>) };

            List<UnitDistance> searched_team = target_team
                .Where(unit_id => enemy_tiles == null || enemy_tiles.Contains(Global.game_map.units[unit_id].loc))
                .Select(unit_id => new UnitDistance(unit_id, Global.game_map.unit_distance(unit.id, unit_id)))
                .ToList();
            if (searched_team.Count == 0)
                return new Maybe<Vector2>[] { default(Maybe<Vector2>), default(Maybe<Vector2>) };
            searched_team.Sort(delegate(UnitDistance a, UnitDistance b)
            {
                return sort_by_toughness_distance(unit, a, b);
            });
            // Test each enemy; if they're in range of any tile in the move range, move closer
            foreach (UnitDistance unit_id in searched_team)
            {
                Game_Unit target = unit_id.unit;
                bool can_hit = false;
                // If defending an area, always move closer to something if they're in the area
                if (enemy_tiles != null)
                    can_hit = true;
                else
                    for (int i = 0; i < total_move_range.Count; i++)
                    {
                        int attack_distance = Global.game_map.distance(total_move_range[i], target.loc);
                        if (ranges.Contains(attack_distance) &&
                            (siege_ranges.Contains(attack_distance) || Pathfind.clear_firing_line(total_move_range[i], target.loc)))
                        {
                            can_hit = true;
                            break;
                        }
                    }
                if (can_hit)
                {
                    // Sort the move range to get the closest tile to the target
                    total_move_range.Sort(delegate(Vector2 a, Vector2 b)
                    {
                        return (
                            Global.game_map.distance(a, target.loc) -
                            Global.game_map.distance(b, target.loc));
                    });
                    foreach (Vector2 loc in total_move_range)
                        if (ranges.Contains(Global.game_map.distance(loc, target.loc)))
                        {
                            // Ignore if enemy can't be hurt, because running up to a wall toward them is silly
                            unit.set_ai_base_loc(loc, unit.mov);
                            int distance = (int)(Math.Abs(loc.X - target.loc.X) + Math.Abs(loc.Y - target.loc.Y));
                            foreach (int weapon_index in unit.actor.useable_weapons())
                            {
                                Data_Weapon weapon = Global.data_weapons[unit.actor.items[weapon_index].Id];
                                int min = unit.min_range(weapon_index);
                                int max = unit.max_range(weapon_index);

                                target.target_unit(unit, weapon, distance);

                                float[] use = Combat.damage_per_round(
                                    unit, target, new List<int> { distance }, weapon_index, Global.game_system.hard_mode);
                                // Tests if unit causes no damage but can inflict status effects
                                if (use[0] <= 0)
                                {
                                    if (weapon.Status_Inflict.Count > 0)
                                    {
                                        int hit1 = 0;
                                        List<int?> stats = (Combat.combat_stats(unit.id, target.id, distance, weapon_index));
                                        // If hit is not null
                                        if (stats[0] != null)
                                            hit1 = Math.Max(hit1, (int)stats[0]);
                                        if (hit1 <= 0)
                                        {
                                            unit.reset_ai_loc();
                                            target.cancel_targeted();
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        unit.reset_ai_loc();
                                        target.cancel_targeted();
                                        continue;
                                    }
                                }
                                target.cancel_targeted();
                                unit.reset_ai_loc();

                                return new Maybe<Vector2>[] { target.loc, path_to_target(unit, loc, ignore_blocking: true) };
                            }
                        }
                    // Return at least something
                    if (enemy_tiles != null)
                        return new Maybe<Vector2>[] { target.loc, path_to_target(unit, total_move_range[0], ignore_blocking: true) };
                }
            }

            return new Maybe<Vector2>[] { default(Maybe<Vector2>), default(Maybe<Vector2>) };
        }

        private static int sort_by_toughness_distance(Game_Unit unit, UnitDistance a, UnitDistance b)
        {
            int comparison = toughness_distance(unit, a) - toughness_distance(unit, b);
            return comparison;
        }
        private static int toughness_distance(Game_Unit unit, UnitDistance target)
        {
            // test how this works with targets on impassable terrain //Yeti
            // The cost of moving onto the target's tile
            int target_tile_cost = unit.move_cost(target.unit.loc);
            // Gets the total move cost to move up to the target
            int route_cost = target.dist - target_tile_cost;

            int turns_to_reach = (int)Math.Ceiling(route_cost / (float)unit.mov);
            return (int)(target.unit.toughness() *
                Math.Pow(turns_to_reach, 1.5f));
        }

        public static Maybe<Vector2>[] search_for_ally(Game_Unit unit)
        {
            return search_for_ally(unit, Search_For_Ally_Modes.Anyone);
        }
        public static Maybe<Vector2>[] search_for_ally(Game_Unit unit, Search_For_Ally_Modes mode)
        {
            // [0] is destination location, [1] is where this unit wants to move to
            List<UnitDistance> searched_targets = search_for_allies(unit, mode);
            if (!searched_targets.Any())
                return new Maybe<Vector2>[] { default(Maybe<Vector2>), default(Maybe<Vector2>) };
            // Selects a target
            Game_Unit target = searched_targets[0].unit;

            bool no_move_okay;
            switch(mode)
            {
                case Search_For_Ally_Modes.Looking_For_Healing:
                case Search_For_Ally_Modes.Looking_For_Healing_Item:
                case Search_For_Ally_Modes.Attack_In_Range_Healing_Item:
                    no_move_okay = true;
                    break;
                default:
                    no_move_okay = false;
                    break;
            }
            return new Maybe<Vector2>[] { target.loc, path_to_target(unit, target.loc, no_move_okay: no_move_okay) };
        }

        private static List<UnitDistance> search_for_allies(Game_Unit unit, Search_For_Ally_Modes mode)
        {
            // If looking for an item to heal with, but this unit drops an item, don't because the traded for item would drop instead
            // Or if inventory is already full, that's important
            bool looking_for_item;
            switch(mode)
            {
                case Search_For_Ally_Modes.Looking_For_Healing_Item:
                case Search_For_Ally_Modes.Attack_In_Range_Healing_Item:
                    looking_for_item = true;
                    break;
                default:
                    looking_for_item = false;
                    break;
            }
            if (looking_for_item && (unit.no_ai_item_trading || unit.actor.is_full_items))
                return new List<UnitDistance>();

            List<UnitDistance> searched_targets;
            switch (mode)
            {
                case Search_For_Ally_Modes.Looking_To_Heal:
                    searched_targets = search_for_target(unit, false, true);
                    break;
                default:
                    searched_targets = search_for_target(unit, false);
                    break;
            }
            if (searched_targets.Count == 0)
                return searched_targets;

            int i = 0;
            switch (mode)
            {
                case Search_For_Ally_Modes.Anyone:
                    searched_targets.Sort(delegate(UnitDistance a, UnitDistance b)
                    {
                        return ((a.dist / unit.mov + 1) - (b.dist / unit.mov + 1));
                    });
                    break;
                case Search_For_Ally_Modes.Looking_To_Heal:
                    /*while (i < searched_targets.Count) //This is embedded in search_for_target() now //Debug
                    {
                        if (!unit.actor.can_heal(Global.game_map.units[searched_targets[i][0]]))
                            searched_targets.RemoveAt(i);
                        else
                            i++;
                    }*/

                    // If trying to heal, ensure the target can be healed
                    //if (healing_target && !unit.actor.can_heal(target))
                    //    continue;

                    // Gets the distance from each heal target to the nearest enemy unit
                    var distance_to_enemies = searched_targets.ToDictionary(
                        x => x.UnitId, x =>
                            {
                                var targets = search_for_target(x.unit, true);
                                // If no enemies can be reached, return an arbitrarily high value
                                if (!targets.Any())
                                    return 2 * (Global.game_map.width + Global.game_map.height);
                                return targets.Min(y => y.dist);
                            });
                    /* //Debug
                    // Don't heal allies who can't reach any enemies...?
                    foreach (int unit_id in distance_to_enemies
                            .Where(x => x.Value == -1)
                            .Select(x => x.Key)
                            .ToList())
                        distance_to_enemies.Remove(unit_id);*/

                    // Sorts the target units by hp needed and distance
                    searched_targets.Sort(delegate(UnitDistance a, UnitDistance b)
                    {
                        return (int)(ATTACK_WEIGHT_PRECISION * (
                            heal_distance(unit, a, distance_to_enemies[a.UnitId]) -
                            heal_distance(unit, b, distance_to_enemies[b.UnitId])));
                    });
                    break;
                case Search_For_Ally_Modes.Looking_For_Healing:
                    // Remove units that aren't on a healing mission or can't heal
                    while (i < searched_targets.Count)
                    {
                        if (!HEALING_MISSIONS.Contains(searched_targets[i].unit.ai_mission) ||
                                !searched_targets[i].unit.actor.can_heal(unit))
                            searched_targets.RemoveAt(i);
                        else
                            i++;
                    }
                    searched_targets.Sort(delegate(UnitDistance a, UnitDistance b)
                    {
                        return ((a.dist / unit.mov + 1) - (b.dist / unit.mov + 1));
                    });
                    break;
                case Search_For_Ally_Modes.Looking_For_Healing_Item:
                case Search_For_Ally_Modes.Attack_In_Range_Healing_Item:
                    // Remove units that aren't on the exact same team or don't have a healing item,
                    // or units that don't want to give up their items
                    searched_targets = searched_targets
                        .Where(x =>
                            // Changed this to only work within on move, otherwise there was infinite looping // Yeti
                            // Maybe change it later
                            // I don't actually understand how this is infinite looping though //Debug
                            x.dist <= unit.canto_mov && unit.same_team(x.unit) &&
                            !x.unit.no_ai_item_trading &&
                            x.unit.can_heal_self(unit))
                        .ToList();
                    if (mode == Search_For_Ally_Modes.Attack_In_Range_Healing_Item)
                        // If on attack in range, only trade with other nongeneric attack in range units
                        searched_targets = searched_targets
                            .Where(x =>
                                x.unit.ai_mission == ATTACK_IN_RANGE_MISSION &&
                                !x.unit.actor.is_generic_actor)
                            .ToList();
                    /*while (i < searched_targets.Count) //Debug
                    {
                        // Changed this to only work within on move, otherwise there was infinite looping // Yeti
                        // Maybe change it later
                        // I don't actually understand how this is infinite looping though //Debug
                        if (searched_targets[i][1] > unit.canto_mov || unit.different_team(Global.game_map.units[searched_targets[i][0]]) ||
                                Global.game_map.units[searched_targets[i][0]].drops_item || !Global.game_map.units[searched_targets[i][0]].can_heal_self(unit))
                            searched_targets.RemoveAt(i);
                        else
                            i++;
                    }*/
                    searched_targets.Sort(delegate(UnitDistance a, UnitDistance b)
                    {
                        return ((a.dist / unit.mov + 1) - (b.dist / unit.mov + 1));
                    });
                    break;
                case Search_For_Ally_Modes.Looking_To_Dance:
                    // Remove units that aren't on the exact same team, haven't moved yet already, or can't fight
                    searched_targets = new List<UnitDistance>(
                        searched_targets.Where(x => unit.same_team(x.unit) &&
                            !x.unit.ready && x.unit.actor.weapon != null));
                    /*while (i < searched_targets.Count) //Debug
                    {
                        if (unit.different_team(Global.game_map.units[searched_targets[i][0]]) ||
                                Global.game_map.units[searched_targets[i][0]].ready || Global.game_map.units[searched_targets[i][0]].actor.weapon != null)
                            searched_targets.RemoveAt(i);
                        else
                            i++;
                    }*/
                    searched_targets.Sort(delegate(UnitDistance a, UnitDistance b)
                    {
                        return ((a.dist / unit.mov + 1) - (b.dist / unit.mov + 1));
                    });
                    break;
            }
            return searched_targets;
        }

        private static double heal_distance(
            Game_Unit unit, UnitDistance target, int distanceToEnemy)
        {
            int turns_to_reach = target.dist / unit.mov + 1;
            float health_percent = target.unit.actor.hp / (float)target.unit.actor.maxhp;
            // Will prefer healing targets with the lowest result of this function
            // Higher hp, further away from the healer, and further away from any enemies increase the result
            return Math.Pow(health_percent, 2) * turns_to_reach * distanceToEnemy;
        }

        public static Maybe<Vector2>[] search_for_healing(Game_Unit unit)
        {
            // First gets a list of all healing terrain that is close enough and not occupied
            List<LocationDistance> healing_terrain = new List<LocationDistance>(),
                near_healing_terrain = new List<LocationDistance>();
            foreach (Vector2 loc in Global.game_map.healing_terrain())
            {
                if (!Global.game_map.is_blocked(loc, unit.id))
                {
                    Maybe<int> check = Pathfind.get_distance(loc, unit.id, -1, true);
                    if (check.IsSomething)
                    {
                        if (check <= unit.mov)
                            near_healing_terrain.Add(new LocationDistance(loc, check));
                        else if (check <= unit.mov * HEALING_TERRAIN_SEARCH_RANGE && !Global.game_state.ai_enemy_attack_range.Contains(loc))
                            healing_terrain.Add(new LocationDistance(loc, check));
                    }
                }
            }
            // Next get allies who can trade healing items, if the unit can't heal themself and has inventory space
            List<UnitDistance> allies;
            if (unit.can_heal_self() || unit.actor.is_full_items)
                allies = allies = new List<UnitDistance>();
            else
                allies = search_for_allies(unit, Search_For_Ally_Modes.Looking_For_Healing_Item);
            int i = 0;
            // Remove the allies that aren't within one turn of movement
            while (i < allies.Count)
            {
                if (allies[i].dist - unit.move_cost(allies[i].unit.loc) <= unit.mov)
                    i++;
                else
                    allies.RemoveAt(i);
            }

            // If there is any healing terrain within one move, move to one of those tiles
            if (near_healing_terrain.Count > 0)
            {
                bool any_outside_enemy_range = false;
                foreach (LocationDistance pair in near_healing_terrain)
                {
                    // If any of those healing tiles have an ally next to them, prefer that tile
                    foreach (UnitDistance ally in allies)
                        if ((Math.Abs(ally.unit.loc.X - pair.loc.X) == 1 && ally.unit.loc.Y == pair.loc.Y) ||
                                (ally.unit.loc.X == pair.loc.X && Math.Abs(ally.unit.loc.Y - pair.loc.Y) == 1))
                            return new Maybe<Vector2>[] { pair.loc, path_to_target(unit, pair.loc, offensive: false, retreat: false, no_move_okay: true) };
                    any_outside_enemy_range |= !Global.game_state.ai_enemy_attack_range.Contains(pair.loc);
                }

                // Else if all healing terrain within range is inside enemy attack range, move to an ally to prioritize immediate healing
                if (allies.Count == 0 || any_outside_enemy_range)
                {
                    // If any healing terrain within range is outside enemy range, remove the ones inside enemy range
                    if (any_outside_enemy_range)
                    {
                        i = 0;
                        while (i < near_healing_terrain.Count)
                        {
                            if (!Global.game_state.ai_enemy_attack_range.Contains(near_healing_terrain[i].loc))
                                i++;
                            else
                                near_healing_terrain.RemoveAt(i);
                        }
                    }

                    // Else just pick one
                    int index = (int)((Global.game_state.ai_turn_rn / 100f) * near_healing_terrain.Count);
                    return new Maybe<Vector2>[] { near_healing_terrain[index].loc, path_to_target(unit, near_healing_terrain[index].loc,
                        offensive: false, retreat: false, no_move_okay: true) };
                }
            }

            // Else if any allies are in range move to one
            if (allies.Count > 0)
            {
                Game_Unit target = allies[(int)((Global.game_state.ai_turn_rn / 100f) * allies.Count)].unit;
                return new Maybe<Vector2>[] { target.loc, path_to_target(unit, target.loc, offensive: false, retreat: false, no_move_okay: true) };
            }
            // If there is any further out healing terrain, move toward one of those tiles
            else if (healing_terrain.Count > 0)
            {
                int index = (int)((Global.game_state.ai_turn_rn / 100f) * healing_terrain.Count);
                return new Maybe<Vector2>[] { healing_terrain[index].loc, path_to_target(unit, healing_terrain[index].loc, offensive: false, retreat: false, no_move_okay: true) };
            }
            // Else move toward a staff bot
            return search_for_ally(unit, Search_For_Ally_Modes.Looking_For_Healing);
        }

        /// <summary>
        /// Performs AI rescue planning for the given unit.
        /// If the unit is very far from its target, it activates a flag for allies to detect that it wants help.
        /// Also checks for allies who have the rescue flag set to see if this unit can help speed them up.
        /// Returns an array containing the id of a unit to rescue, the x position to move to, and the y position to move to.
        /// Returns null if no unit is to be rescued.
        /// </summary>
        /// <param name="unit">The AI unit to plan rescuing.</param>
        /// <param name="actual_target_loc">Where the unit would like to move, if it doesn't rescue.</param>
        /// <param name="heading_to_seize">True if the unit is trying to move toward a seize point.</param>
        public static int[] search_for_rescue(Game_Unit unit, Vector2 actual_target_loc, bool heading_to_seize = false)
        {
            if (unit.boss || unit.is_rescue_blocked())
                return null;
            Maybe<int> distance_test = Pathfind.get_distance(actual_target_loc, unit.id, -1, false);
            // Check for rescuing
            // If a route to the target is found and it will take more than 2 turns to get there, we have time to care about rescuing
            if (distance_test.IsSomething && distance_test >= unit.mov * 2)
            {
                // If a route to the target is found and it will take more than 4 turns to get there
                if (distance_test >= unit.mov * 4)
                    // This unit would like to be rescued to move faster
                    unit.ai_wants_rescue = true;

                // If cantoing (ie just dropped someone off?), don't allow rescuing
                if (unit.cantoing)
                    return null;

                int distance = distance_test;
                List<int> rescue_targets = new List<int>();
                // Get units that are not this unit and can be rescued and not berserk
                // And also not bosses (bosses want to always be on the field, and also don't want to lose stats/turns from rescuing)
                // If an already moved unit wants rescued (and can be rescued) and it's slower than this unit
                var potential_rescuees = new HashSet<int>(Global.game_map.teams[unit.team]
                    .Where(unit_id => unit_id != unit.id &&
                        unit.can_rescue(Global.game_map.units[unit_id]) &&
                        !Global.game_map.units[unit_id].berserk &&
                        !Global.game_map.units[unit_id].boss &&
                        !Global.game_map.units[unit_id].ready &&
                        Global.game_map.units[unit_id].ai_wants_rescue));
                foreach (int unit_id in potential_rescuees)
                //foreach (int unit_id in Global.game_map.teams[unit.team]) //Debug
                {
                    Game_Unit target = Global.game_map.units[unit_id];
                    // If the target has canto, this unit needs to have canto and at least 2 more move
                    // If the rescuer doesn't have canto they need at least 2 more move than the other unit
                    if (target.has_canto() ?
                            (unit.has_canto() && unit.mov >= target.mov + 2) :
                            (unit.mov >= target.mov + (unit.has_canto() ? 1 : 2)))
                    {
                        // If this unit is trying to head to a seize point, not to attack a unit
                        if (heading_to_seize)
                            // And the target is under enemy attack range, don't bother picking them up it'll probably slow this unit down and get it attacked
                            if (Global.game_state.ai_enemy_attack_range.Contains(target.loc))
                                continue;

                        distance_test = Pathfind.get_distance(actual_target_loc, unit.id, -1, false, target.loc);
                        // If the unit doesn't have to backtrack to rescue
                        if (distance_test.IsSomething && (distance_test - (unit.has_canto() ? (unit.mov - 1) / 2 : 0)) < distance)
                            // Check if the unit can move to any tiles adjacent to the other unit
                            foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                            {
                                // Check if an ally is blocking the tile though
                                if (!Global.game_map.is_off_map(target.loc + offset) && !Global.game_map.is_blocked(target.loc + offset, unit.id))
                                {
                                    distance_test = Pathfind.get_distance(target.loc + offset, unit.id, -1, false);
                                    if (distance_test.IsSomething && distance_test <= unit.mov)
                                    {
                                        rescue_targets.Add(unit_id);
                                        break;
                                    }
                                }
                            }
                    }
                }
                if (rescue_targets.Count > 0)
                {
                    int target_id = rescue_targets[(int)((Global.game_state.ai_turn_rn / 100.0f) * rescue_targets.Count)];
                    Game_Unit target = Global.game_map.units[target_id];
                    List<LocationDistance> target_locs = new List<LocationDistance>();
                    // Check the tiles around the target
                    foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                    {
                        // Check if an ally is blocking the tile though
                        if (!Global.game_map.is_off_map(target.loc + offset) && !Global.game_map.is_blocked(target.loc + offset, unit.id))
                        {
                            distance_test = Pathfind.get_distance(target.loc + offset, unit.id, -1, false);
                            // If the tile can be reached
                            if (distance_test.IsSomething && distance_test <= unit.mov)
                            {
                                distance = distance_test;
                                distance_test = Pathfind.get_distance(actual_target_loc, unit.id, -1, false, target.loc + offset);
                                if (distance_test.IsSomething)
                                {
                                    target_locs.Add(new LocationDistance(target.loc + offset, distance_test + distance));
                                }
                            }
                        }
                    }
                    if (target_locs.Count == 0)
                        return null;
                    target_locs.Sort(delegate(LocationDistance a, LocationDistance b) { return a.dist - b.dist; });
                    return new int[] { target_id, (int)target_locs[0].loc.X, (int)target_locs[0].loc.Y };
                }
            }
            return null;
        }
        /// <summary>
        /// Searches for somewhere for the given unit to drop its rescued ally.
        /// Returns an array containing x and y positions to move to, followed by the x and y positions to drop on.
        /// Returns null if nowhere to drop.
        /// </summary>
        /// <param name="unit">The AI unit trying to find somewhere to drop.</param>
        public static int[] search_for_rescue_drop(Game_Unit unit)
        {
            return search_for_rescue_drop(unit, default(Maybe<Vector2>), default(Maybe<Vector2>));
        }
        /// <summary>
        /// Searches for somewhere for the given unit to drop its rescued ally.
        /// Returns an array containing x and y positions to move to, followed by the x and y positions to drop on.
        /// Returns null if nowhere to drop.
        /// </summary>
        /// <param name="unit">The AI unit trying to find somewhere to drop.</param>
        /// <param name="actual_target_loc">
        /// Where the unit would like to move.
        /// If it is close to this location, it will drop the rescued unit.
        /// If null, defaults to the unit's retreat location.
        /// </param>
        public static int[] search_for_rescue_drop(Game_Unit unit, Maybe<Vector2> move_loc, Maybe<Vector2> actual_target_loc)
        {
            if (!unit.is_rescuing)
                throw new ArgumentException(string.Format("AI unit \"{0}\" that is not rescuing is trying to drop.", unit));

            // If cantoing (ie probably picked someone up this turn), don't allow drop
            if (unit.cantoing)
                return null;

            List<Vector2> targets;
            if (actual_target_loc.IsSomething)
                targets = new List<Vector2> { actual_target_loc };
            else
            {
                var locs = target_locations(unit.rescuing_unit);
                if (locs.Any())
                {
                    var loc_groups = group_locations(unit.rescuing_unit, locs);
                    targets = closest_group_targets(unit.loc, loc_groups);
                }
                else
                {
                    actual_target_loc = retreat(unit);
                    if (actual_target_loc.IsNothing)
                        actual_target_loc = unit.loc;
                    targets = new List<Vector2> { actual_target_loc };
                }
            }

            var map = new UnitMovementMap(unit.id, throughDoors: false);
            targets = targets.Where(target =>
                {
                    var distance = map.get_distance(target, -1);
                    return distance.IsSomething && distance <= unit.mov * 2;
                })
                .ToList();
            // Check for rescuing
            if (targets.Any() ||
                (move_loc.IsSomething && Global.game_state.ai_enemy_attack_range.Contains(move_loc)))
            {
                var unit_map = new UnitMovementMap(unit.id, throughDoors: true);
                var rescuee_map = new UnitMovementMap(unit.rescuing, ignoreUnits: true, throughDoors: true);
                Game_Unit rescued_unit = Global.game_map.units[unit.rescuing];
                // Gets the distance from all tiles in the move range to the target
                HashSet<Vector2> move_range = unit.move_range;
                // Remove blocked
                // Testing enumerable.Where()
                move_range = new HashSet<Vector2>(move_range.Where(v => !Global.game_map.is_blocked(v, unit.id, false)));
                // First try to get tiles outside the enemy attack range
                int min_dist;
                List<LocationDistance> target_locs = new List<LocationDistance>();
                // Switching back and forth in the pathfinder between two units introduces a lot of lag
                // So check all the tiles for the rescuer first, then check if they're open for the rescuee
                List<Vector2> rescuer_tiles = move_range
                    .Where(x => targets.Any(target => unit_map.get_distance(target, -1, x).IsSomething))
                    .ToList();

                if (Global.game_system.Difficulty_Mode >= Difficulty_Modes.Hard)
                {
                    foreach (Vector2 loc in rescuer_tiles)
                    {
                        min_dist = -1;
                        foreach (Vector2 potential_target in rescuee_map.AdjacentLocations(loc))
                            if (!Global.game_state.ai_enemy_attack_range.Contains(potential_target) &&
                                !Global.game_map.is_off_map(potential_target) &&
                                !Global.game_map.is_blocked(potential_target, unit.id) &&
                                rescuee_map.Passable(potential_target))
                            {
                                var distances = targets.Select(target =>
                                        rescuee_map.get_distance(target, -1, potential_target))
                                    .Where(x => x.IsSomething)
                                    .OrderBy(x => x.ValueOrDefault);
                                if (distances.Any())
                                {
                                    var check = distances.First();
                                    if (check.IsSomething && (min_dist == -1 || check < min_dist))
                                        min_dist = check;
                                }
                            }
                        if (min_dist != -1)
                            target_locs.Add(new LocationDistance(loc, min_dist));
                    }
                }
                // Remove any locations that are more than two turns from the target
                target_locs = target_locs
                    .Where(x => x.dist <= Math.Max(unit.mov, unit.rescuing_unit.mov * 2))
                    .ToList();
                // If that doesn't work then just get closest to the target
                if (target_locs.Count == 0)
                {
                    foreach (Vector2 loc in rescuer_tiles)
                    {
                        min_dist = -1;
                        foreach (Vector2 potential_target in rescuee_map.AdjacentLocations(loc))
                        {
                            // Don't need to check is_off_map(), because is_blocked() already does
                            if (!Global.game_map.is_off_map(potential_target) &&
                                !Global.game_map.is_blocked(potential_target, unit.id) &&
                                rescuee_map.Passable(potential_target))
                            {
                                var distances = targets.Select(target =>
                                        rescuee_map.get_distance(target, -1, potential_target))
                                    .Where(x => x.IsSomething)
                                    .OrderBy(x => x.ValueOrDefault);
                                if (distances.Any())
                                {
                                    var check = distances.First();
                                    if (check.IsSomething && (min_dist == -1 || check < min_dist))
                                        min_dist = check;
                                }
                            }
                        }
                        if (min_dist != -1)
                            target_locs.Add(new LocationDistance(loc, min_dist));
                    }
                    // Remove any locations that are more than two turns from the target
                    target_locs = target_locs
                        .Where(x => x.dist <= Math.Max(unit.mov, unit.rescuing_unit.mov * 2))
                        .ToList();
                }
                if (target_locs.Count == 0)
                    return null;
                target_locs.Sort(delegate(LocationDistance a, LocationDistance b) { return a.dist - b.dist; });
                int min_distance = target_locs[0].dist;
                target_locs = target_locs.Where(x => x.dist <= min_distance).ToList();

                Vector2 target_loc = target_locs[(int)((Global.game_state.ai_turn_rn / 100.0f) * target_locs.Count)].loc;

                target_locs.Clear();
                rescuee_map = new UnitMovementMap(unit.rescuing, ignoreUnits: true, throughDoors: false);
                // Check the tiles around the target
                foreach (Vector2 potential_target in rescuee_map.AdjacentLocations(target_loc))
                {
                    if (!Global.game_map.is_off_map(potential_target) &&
                        !Global.game_map.is_blocked(potential_target, unit.id) &&
                        rescuee_map.Passable(potential_target))
                    {
                                var distances = targets.Select(target =>
                                        rescuee_map.get_distance(target, -1, potential_target))
                                    .Where(x => x.IsSomething)
                                    .OrderBy(x => x.ValueOrDefault);
                                if (distances.Any())
                                {
                                    var distance_test = distances.First();
                                    if (distance_test.IsSomething)
                                        target_locs.Add(new LocationDistance(potential_target, distance_test));
                                }
                    }
                }
                if (target_locs.Count == 0)
                    return null;
                target_locs.Sort(delegate(LocationDistance a, LocationDistance b)
                {
                    if (Global.game_state.ai_enemy_attack_range.Contains(a.loc) != Global.game_state.ai_enemy_attack_range.Contains(b.loc))
                        return (Global.game_state.ai_enemy_attack_range.Contains(a.loc) ? 1 : -1);
                    if (a.dist != b.dist)
                        return a.dist - b.dist;
                    int def_a = Global.game_map.terrain_def_bonus(a.loc);
                    int def_b = Global.game_map.terrain_def_bonus(b.loc);
                    if (def_a != def_b)
                        return def_b - def_a;
                    return Global.game_map.terrain_avo_bonus(b.loc) - Global.game_map.terrain_avo_bonus(a.loc);
                });
                // First two values are where to move to, second two are drop location
                return new int[] { (int)target_loc.X, (int)target_loc.Y, (int)target_locs[0].loc.X, (int)target_locs[0].loc.Y };
            }
            return null;
        }

        /// <summary>
        /// Searches for injured units on healing terrain for the given unit to cover, protecting them until they recover.
        /// Returns an array containing the id of the unit to cover, the x position to move to, and the y position to move to.
        /// Returns null if no unit is to be covered.
        /// </summary>
        /// <param name="unit">The AI unit trying to find someone to cover. This unit must have the cover ability.</param>
        public static int[] search_for_savior(Game_Unit unit)
        {
            if (!unit.has_cover())
                throw new ArgumentException(string.Format("AI unit \"{0}\" is trying to cover but doesn't have the ability.", unit));
            if (unit.is_rescue_blocked())
                return null;
            Maybe<int> distance_test;

            List<int> rescue_targets = new List<int>();
            // If a unit can be rescued
            // Get units that are not this unit and not berserk
            // And also not bosses (bosses want to always be on the field, and also don't want to lose stats/turns from rescuing)
            var potential_rescuees = new HashSet<int>(Global.game_map.teams[unit.team]
                .Where(unit_id => unit_id != unit.id &&
                    unit.can_rescue(Global.game_map.units[unit_id]) &&
                    !Global.game_map.units[unit_id].berserk &&
                    !Global.game_map.units[unit_id].boss));
            foreach (int unit_id in potential_rescuees)
            //foreach (int unit_id in Global.game_map.teams[unit.team]) //Debug
            {
                Game_Unit target = Global.game_map.units[unit_id];
                // If a unit is healing on terrain (and can be rescued) and the unit will take more than one more turn to heal and within enemy range
                if (target.ai_terrain_healing && target.terrain_heals() &&
                    (target.actor.hp + (target.actor.maxhp * Global.game_map.terrain_healing_amount(target.loc)) / 100) < target.actor.maxhp &&
                    unit.can_rescue(target) && Global.game_state.ai_enemy_attack_range.Contains(target.loc) && Pathfind.passable(unit, target.loc))
                {
                        // Check if the unit can move to any tiles adjacent to the other unit
                        foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                        {
                            // Check if an ally is blocking the tile though
                            if (!Global.game_map.is_off_map(target.loc + offset) && !Global.game_map.is_blocked(target.loc + offset, unit.id))
                            {
                                distance_test = Pathfind.get_distance(target.loc + offset, unit.id, -1, false);
                                if (distance_test.IsSomething && distance_test <= unit.mov)
                                {
                                    rescue_targets.Add(unit_id);
                                    break;
                                }
                            }
                        }
                }
            }
            if (rescue_targets.Count > 0)
            {
                int target_id = rescue_targets[(int)((Global.game_state.ai_turn_rn / 100.0f) * rescue_targets.Count)];
                Game_Unit target = Global.game_map.units[target_id];
                List<LocationDistance> target_locs = new List<LocationDistance>();
                // Check the tiles around the target
                foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                {
                    // Check if an ally is blocking the tile though
                    if (!Global.game_map.is_off_map(target.loc + offset) && !Global.game_map.is_blocked(target.loc + offset, unit.id))
                    {
                        distance_test = Pathfind.get_distance(target.loc + offset, unit.id, -1, false);
                        // If the tile can be reached
                        if (distance_test.IsSomething && distance_test <= unit.mov)
                            target_locs.Add(new LocationDistance(target.loc + offset, distance_test));
                    }
                }
                if (target_locs.Count == 0)
                    return null;
                target_locs.Sort(delegate(LocationDistance a, LocationDistance b) { return a.dist - b.dist; });
                return new int[] { target_id, (int)target_locs[0].loc.X, (int)target_locs[0].loc.Y };
            }
            return null;
        }
        /// <summary>
        /// Searches for somewhere for the given unit to drop its covered ally, usually the healing terrain the unit is already on.
        /// Returns an array containing x and y positions to move to, followed by the x and y positions to drop on.
        /// Returns null if nowhere to drop.
        /// </summary>
        /// <param name="unit">The AI unit trying to find somewhere to drop.</param>
        public static int[] search_for_savior_drop(Game_Unit unit)
        {
            if (!unit.is_rescuing)
                throw new ArgumentException(string.Format("AI unit \"{0}\" that is not rescuing is trying to drop.", unit));
            Game_Unit rescued_unit = Global.game_map.units[unit.rescuing];
            // If the rescuee is still healing and the unit is within enemy range, stay put and keep healing
            if (unit.terrain_heals() && Global.game_state.ai_enemy_attack_range.Contains(unit.loc) &&
                    (rescued_unit.actor.hp + (rescued_unit.actor.maxhp * Global.game_map.terrain_healing_amount(unit.loc)) / 100) < rescued_unit.actor.maxhp)
                return null;
            Maybe<int> distance_test;

            // Gets the distance from all tiles in the move range to the target
            HashSet<Vector2> move_range = unit.move_range;
            // Remove blocked
            // Testing enumerable.Where()
            move_range = new HashSet<Vector2>(move_range.Where(v => !Global.game_map.is_blocked(v, unit.id, false)));
            // First try to get tiles outside the enemy attack range
            int min_dist;
            List<LocationDistance> target_locs = new List<LocationDistance>();
            if (Global.game_system.Difficulty_Mode >= Difficulty_Modes.Hard)
                foreach (Vector2 loc in move_range)
                {
                    min_dist = -1;
                    foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                        // If the terrain heals or if the unit is done healing
                        if ((Global.game_map.terrain_heals(loc + offset) || rescued_unit.actor.is_full_hp()) &&
                            !Global.game_state.ai_enemy_attack_range.Contains(loc + offset) && !Global.game_map.is_off_map(loc + offset) &&
                            !Global.game_map.is_blocked(loc + offset, unit.id) && Pathfind.passable(rescued_unit, loc + offset))
                        {
                            distance_test = Pathfind.get_distance(loc, unit.id, -1, false);
                            if (distance_test.IsSomething && (min_dist == -1 || distance_test < min_dist))
                                min_dist = distance_test;
                        }
                    if (min_dist != -1)
                        target_locs.Add(new LocationDistance(loc, min_dist));
                }
            // If that doesn't work then just get closest to the target
            if (target_locs.Count == 0)
            {
                foreach (Vector2 loc in move_range)
                {
                    min_dist = -1;
                    foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                        // If the terrain heals or if the unit is done healing
                        if ((Global.game_map.terrain_heals(loc + offset) || rescued_unit.actor.is_full_hp()) &&
                            !Global.game_map.is_off_map(loc + offset) &&
                            !Global.game_map.is_blocked(loc + offset, unit.id) && Pathfind.passable(rescued_unit, loc + offset))
                        {
                            distance_test = Pathfind.get_distance(loc, unit.id, -1, false);
                            if (distance_test.IsSomething && (min_dist == -1 || distance_test < min_dist))
                                min_dist = distance_test;
                        }
                    if (min_dist != -1)
                        target_locs.Add(new LocationDistance(loc, min_dist));
                }
            }
            if (target_locs.Count == 0)
                return search_for_rescue_drop(unit);
            target_locs.Sort(delegate(LocationDistance a, LocationDistance b) { return a.dist - b.dist; });
            int min_distance = target_locs[0].dist;
            for (int i = 0; i < target_locs.Count; i++)
                if (target_locs[i].dist > min_distance)
                {
                    target_locs.RemoveRange(i, target_locs.Count - i);
                    break;
                }

            Vector2 target_loc = target_locs[(int)((Global.game_state.ai_turn_rn / 100.0f) * target_locs.Count)].loc;
            target_locs.Clear();
            // Check the tiles around the target
            foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
            {
                if ((Global.game_map.terrain_heals(target_loc + offset) || rescued_unit.actor.is_full_hp()) && 
                    !Global.game_map.is_off_map(target_loc + offset) &&
                    !Global.game_map.is_blocked(target_loc + offset, unit.id) && Pathfind.passable(rescued_unit, target_loc + offset))
                {
                    distance_test = Pathfind.get_distance(target_loc, unit.id, -1, false);
                    if (distance_test.IsSomething)
                        target_locs.Add(new LocationDistance(target_loc + offset, distance_test));
                }
            }
            if (target_locs.Count == 0)
                return null;
            target_locs.Sort(delegate(LocationDistance a, LocationDistance b)
            {
                if (Global.game_state.ai_enemy_attack_range.Contains(a.loc) != Global.game_state.ai_enemy_attack_range.Contains(b.loc))
                    return (Global.game_state.ai_enemy_attack_range.Contains(a.loc) ? 1 : -1);
                if (a.dist != b.dist)
                    return a.dist - b.dist;
                int def_a = Global.game_map.terrain_def_bonus(a.loc);
                int def_b = Global.game_map.terrain_def_bonus(b.loc);
                if (def_a != def_b)
                    return def_b - def_a;
                return Global.game_map.terrain_avo_bonus(b.loc) - Global.game_map.terrain_avo_bonus(a.loc);
            });
            return new int[] { (int)target_loc.X, (int)target_loc.Y, (int)target_locs[0].loc.X, (int)target_locs[0].loc.Y };
        }

        public static List<LocationDistance> pillage_targets(Game_Unit unit)
        {
            HashSet<Vector2> target_array = new HashSet<Vector2>(Global.game_map.visit_locations
                .Where(x => !string.IsNullOrEmpty(x.Value.PillageEvent))
                .Select(x => x.Key));
            return Game_AI.distance_to_locations(unit, target_array);
        }

        public static int[] search_for_talk(Game_Unit unit)
        {
            var potential_talkers = Global.game_state.talk_targets(unit.id);
            if (potential_talkers != null && potential_talkers.Count == 0)
                return null;

            List<UnitDistance> talk_targets = new List<UnitDistance>();
            #region Find a unit to talk to this turn
            foreach (int target_id in potential_talkers)
            {
                Game_Unit target = Global.game_map.units[target_id];
                // Check if the unit can move to any tiles adjacent to the other unit
                foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                {
                    // Check if an ally is blocking the tile though
                    if (!Global.game_map.is_off_map(target.loc + offset) && !Global.game_map.is_blocked(target.loc + offset, unit.id))
                    {
                        Maybe<int> distance_test = Pathfind.get_distance(target.loc + offset, unit.id, -1, false);
                        if (distance_test.IsSomething && distance_test <= unit.mov)
                        {
                            talk_targets.Add(new UnitDistance(target_id, distance_test));
                        }
                    }
                }
            }

            if (talk_targets.Count > 0)
            {
                talk_targets.Sort(delegate(UnitDistance a, UnitDistance b) { return a.dist - b.dist; });

                int target_id = talk_targets[0].UnitId;
                Game_Unit target = Global.game_map.units[target_id];
                List<KeyValuePair<Vector2, int>> target_locs = new List<KeyValuePair<Vector2, int>>();
                // Check the tiles around the target
                foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                {
                    // Check if an ally is blocking the tile though
                    if (!Global.game_map.is_off_map(target.loc + offset) && !Global.game_map.is_blocked(target.loc + offset, unit.id))
                    {
                        Maybe<int> distance_test = Pathfind.get_distance(target.loc + offset, unit.id, -1, false);
                        // If the tile can be reached ever
                        if (distance_test.IsSomething && distance_test <= unit.mov)
                        {
                            target_locs.Add(new KeyValuePair<Vector2, int>(target.loc + offset, distance_test));
                        }
                    }
                }
                if (target_locs.Count == 0)
                    return null;
                target_locs.Sort(delegate(KeyValuePair<Vector2, int> a, KeyValuePair<Vector2, int> b) { return a.Value - b.Value; });
                return new int[] { target_id, (int)target_locs[0].Key.X, (int)target_locs[0].Key.Y };
            }
            #endregion
            talk_targets.Clear();
            #region Find a unit to move toward
            find_tile_near_talk(unit, potential_talkers, talk_targets);
            if (talk_targets.Count == 0)
                find_tile_near_talk(unit, potential_talkers, talk_targets, true);

            if (talk_targets.Count > 0)
            {
                talk_targets.Sort(delegate(UnitDistance a, UnitDistance b) { return a.dist - b.dist; });

                int target_id = talk_targets[0].UnitId;
                Game_Unit target = Global.game_map.units[target_id];
                List<LocationDistance> target_locs = new List<LocationDistance>();
                // Check the tiles around the target
                find_tile_near_talk(unit, target, target_locs);
                if (target_locs.Count == 0)
                    find_tile_near_talk(unit, target, target_locs, true);

                if (target_locs.Count == 0)
                    return null;
                target_locs.Sort(delegate(LocationDistance a, LocationDistance b) { return a.dist - b.dist; });
                return new int[] { -1, (int)target_locs[0].loc.X, (int)target_locs[0].loc.Y };
            }
            #endregion
            return null;
        }

        private static void find_tile_near_talk(
            Game_Unit unit, HashSet<int> potentialTalkers,
            List<UnitDistance> talkTargets,
            bool ignoreUnits = false)
        {
            foreach (int target_id in potentialTalkers)
            {
                Game_Unit target = Global.game_map.units[target_id];
                // Check if the unit can move to any tiles adjacent to the other unit
                foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                {
                    if (!Global.game_map.is_off_map(target.loc + offset))
                    {
                        Pathfind.ignore_units = ignoreUnits;
                        Maybe<int> distance_test = Pathfind.get_distance(target.loc + offset, unit.id, -1, false);
                        if (distance_test.IsSomething)
                        {
                            talkTargets.Add(new UnitDistance(target_id, distance_test));
                        }
                    }
                }
            }
        }
        private static void find_tile_near_talk(
            Game_Unit unit,
            Game_Unit target,
            List<LocationDistance> targetLocs,
            bool ignoreUnits = false)
        {
            // Check if the unit can move to any tiles adjacent to the other unit
            foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
            {
                // Check if an ally is blocking the tile though //Debug
                //if (!Global.game_map.is_off_map(target.loc + offset) && !Global.game_map.is_blocked(target.loc + offset, unit.id))
                // Don't worry about the tile being blocked, we're just trying to move closer
                if (!Global.game_map.is_off_map(target.loc + offset))
                {
                    Pathfind.ignore_units = ignoreUnits;
                    Maybe<int> distance_test = Pathfind.get_distance(target.loc + offset, unit.id, -1, false);
                    // If the tile can be reached ever
                    if (distance_test.IsSomething)
                    {
                        // Pathfind as close to it as possible
                        Maybe<Vector2> target_loc = path_to_target(unit, target.loc + offset, offensive: false);
                        if (target_loc.IsSomething)
                            targetLocs.Add(new LocationDistance(target_loc, distance_test));
                    }
                }
            }
        }

        public static int[] search_for_tile(Game_Unit unit)
        {
            // Look for places this specific unit wants to move to
            if (Global.game_map.unit_seek_locs.ContainsKey(unit.id))
            {
                Vector2 seekLoc = Global.game_map.unit_seek_locs[unit.id];
                Maybe<int> check = Pathfind.get_distance(seekLoc, unit.id, -1, true);
                Pathfind.reset();
                if (check.IsSomething)
                {
                    Maybe<Vector2> target_loc = path_to_target(unit, seekLoc, offensive: true);
                    if (target_loc.IsSomething)
                        return new int[] { (int)((Vector2)target_loc).X, (int)((Vector2)target_loc).Y };
                }
            }
            // Look for places this unit's team group wants to move to
            if (Global.game_map.team_seek_locs.ContainsKey(unit.team) && Global.game_map.team_seek_locs[unit.team].ContainsKey(unit.group))
            {
                Maybe<int> check = Pathfind.get_distance(Global.game_map.team_seek_locs[unit.team][unit.group], unit.id, -1, true);
                Pathfind.reset();
                if (check.IsSomething)
                {
                    Maybe<Vector2> target_loc = path_to_target(unit, Global.game_map.team_seek_locs[unit.team][unit.group], offensive: false);
                    if (target_loc.IsSomething)
                        return new int[] { (int)((Vector2)target_loc).X, (int)((Vector2)target_loc).Y };
                }
            }

            return null;
        }

        public static Maybe<Vector2> retreat(Game_Unit unit, bool no_move_okay = false, bool full_retreat = false)
        {
            return path_to_target(unit, unit.loc,
                offensive: false, retreat: true, no_move_okay: no_move_okay, full_retreat: full_retreat);
        }

        public static List<UnitDistance> search_for_target(Game_Unit unit,
            bool searching_for_enemies = true, bool healing_target = false,
            bool ignore_doors = false)
        {
            List<int> target_team = target_units(unit, searching_for_enemies);
            // Gets targets that can be reached and their distance
            List<UnitDistance> searched_team = new List<UnitDistance>();
            Pathfind.reset();
            foreach (int unit_id in target_team)
            {
                // I sure hope nothing that calls this wants a unit to find a path to itself! //Debug
                if (unit_id == unit.id)
                    continue;

                Game_Unit target = Global.game_map.units[unit_id];

                // Check if the target can be reached and get the distance
                Maybe<int> check = Pathfind.get_distance(target.loc, unit.id, -1, true, ignore_doors);
                if (check.IsSomething)
                    searched_team.Add(new UnitDistance(unit_id, check));
            }
            Pathfind.reset();
            return searched_team;
        }

        public static List<int[]> search_for_seize_target(Game_Unit unit, bool ignore_units)
        {
            HashSet<Vector2> seize_points = new HashSet<Vector2>(
                Global.game_map.get_seize_points(unit.team, unit.group));
            if (seize_points.Intersect(Global.game_state.ai_move_range).Any())
                seize_points = new HashSet<Vector2>(seize_points.Intersect(Global.game_state.ai_move_range));
            // Gets seizable points that can be reached and their distance
            List<int[]> searched_points = new List<int[]>();
            foreach (Vector2 seize_point in seize_points)
            {
                Pathfind.ignore_units = ignore_units;
                Maybe<int> check = Pathfind.get_distance(seize_point, unit.id, -1, true);
                if (check.IsSomething)
                    searched_points.Add(new int[] { (int)seize_point.X, (int)seize_point.Y, check });
            }
            Pathfind.reset();
            return searched_points;
        }

        protected static List<int> target_units(Game_Unit unit, bool searching_for_enemies)
        {
            // Makes a list of possible targets
            List<int> target_team = new List<int>();
            if (searching_for_enemies)
            {
                foreach (int enemy_team in unit.attackable_teams())
                    foreach (int unit_id in Global.game_map.teams[enemy_team])
                        target_team.Add(unit_id);
            }
            else
            {
                foreach (int ally_team in unit.friendly_teams())
                    foreach (int unit_id in Global.game_map.teams[ally_team])
                        target_team.Add(unit_id);
            }
            if (target_team.Count == 0)
                return new List<int>();
            // Removes rescued targets
            List<int> temp_team = new List<int>();
            foreach (int unit_id in target_team)
                if (Global.game_map.units[unit_id].is_rescued)
                    temp_team.Add(unit_id);
            target_team = target_team.Except(temp_team).ToList();
            return target_team;
        }

        public static List<LocationDistance> distance_to_locations(
            Game_Unit unit, HashSet<Vector2> target_array,
            bool ignore_units = false, Vector2? base_loc = null)
        {
            Vector2 loc;
            if (base_loc == null)
                loc = unit.loc;
            else
                loc = (Vector2)base_loc;
            List<LocationDistance> targets = new List<LocationDistance>();
            foreach(Vector2 target_loc in target_array)
            {
                Pathfind.ignore_units = ignore_units;
                Maybe<int> path = Pathfind.get_distance(target_loc, unit.id, -1, true, loc); // through doors? and should also use the ignore_units bool //Yeti
                if (path.IsSomething)
                    targets.Add(new LocationDistance(target_loc, path));
            }
            Pathfind.reset();
            return targets;
        }

        /// <summary>
        /// Given a unit and a location to move toward, returns a closer location for the unit to move onto within its move range.
        /// Returns null if no valid location is found.
        /// </summary>
        /// <param name="unit">The unit trying to move to a target location.</param>
        /// <param name="target_loc">The location that the unit eventually wants to reach.</param>
        /// <param name="offensive">If true, the unit is unafraid to engage in combat and will enter enemy attack range.</param>
        /// <param name="retreat">If true, the unit is retreating and just wants to move away from all enemies, and wants to stay outside their attack range.</param>
        /// <param name="no_move_okay">If true, the unit's current location is a valid result.</param>
        /// <param name="full_retreat">If true, always retreat even if the unit can fight.</param>
        /// <param name="ignore_doors">If true, pathfind through doors with the intent of opening them.</param>
        /// <param name="ignore_blocking">If true, the unit is eager to engage in combat and will pathfind through enemies.</param>
        public static Maybe<Vector2> path_to_target(Game_Unit unit, Vector2 target_loc,
            bool offensive = true, bool retreat = false, bool no_move_okay = false, bool full_retreat = false,
            bool ignore_doors = false, bool ignore_blocking = false)
        {
            // Can't open doors if cantoing
            if (unit.cantoing)
                ignore_doors = false;
            // Gets the distance from all tiles in the move range to the target
            List<LocationDistance> target_locs = new List<LocationDistance>();
            int dist = -1;
            // Remove blocked
            HashSet<Vector2> move_range = new HashSet<Vector2>(unit.move_range.Where(v => !Global.game_map.is_blocked(v, unit.id, false)));
            bool attack = unit.actor.can_attack();
            bool critical = unit.actor.has_critical_health();
            bool attack_retreat = false;
            // If retreating
            if (retreat)
            {
                // Get own seize points
                var own_seize_points = Global.game_map.get_seize_points(unit.team, unit.group);
                // For teams in the same team group as this unit
                var ally_seize_points = Constants.Team.allied_teams(unit.team)
                    // Get their seize points
                    .SelectMany(x => Global.game_map.get_seize_points(x))
                    .Distinct();
                // Remove own seize points from the set of allied points
                ally_seize_points = ally_seize_points.Except(own_seize_points);
                // If any allies have a seize point at the same tile as this unit's loc
                bool blocking_ally_seize = ally_seize_points.Contains(unit.loc);
                if (blocking_ally_seize)
                {
                    move_range.ExceptWith(ally_seize_points);
                }
                // If there are any tiles to move to without being attacked
                if (move_range.Except(Global.game_state.ai_enemy_attack_range).Any())
                {
                    // If one of those safe tiles is the starting location
                    if (!Global.game_state.ai_enemy_attack_range.Contains(unit.loc))
                    {
                        // If not blocking seize, and can attack and not injured and no one can attack AND not cantoing, you don't really need to retreat just chill
                        if (!blocking_ally_seize && (!unit.cantoing && retreat && attack && !critical))
                        {
                            // If we have to move, instead just work as if we don't have to move and if things still pan out we won't move
                            if (!no_move_okay)
                            {
                                full_retreat = false;
                                no_move_okay = true;
                            }
                            else
                                return default(Maybe<Vector2>);
                        }
                        else if (!unit.cantoing)
                            no_move_okay = false;
                    }

                    // If can attack and not injured and can escape enemy range, only just retreat outside the enemy range
                    if (retreat && attack && !critical)
                        // And not in full retreat
                        if (!full_retreat)
                        {
                            retreat = false;
                            attack_retreat = true;
                        }
                    // Don't move into tiles enemies can attack (if any)
                    move_range.ExceptWith(Global.game_state.ai_enemy_attack_range);
                }
            }
            // Else if can't attack, and in either critical situation or not being offensive, don't move into tiles enemies can attack (if any)
            else if (!attack && (critical || !offensive))
                if (move_range.Except(Global.game_state.ai_enemy_attack_range).Any())
                    move_range.ExceptWith(Global.game_state.ai_enemy_attack_range);

            // Check if the route to the target is blocked, because if so we need to get fancy
            Pathfind.reset();
            Maybe<int> check = Pathfind.get_distance(target_loc, unit.id, -1, true, unit.loc, ignore_doors);
            bool path_blocked = check.IsNothing, path_blocked_by_unit = false;
            if (path_blocked)
            {
#if DEBUG
                Console.WriteLine(string.Format(
                    "\nPathfinding for a unit is blocked,\nchecking if it's just units in the way\nUnit: {0}\n", unit));
#endif
                Pathfind.ignore_units = true;
                check = Pathfind.get_distance(target_loc, unit.id, -1, true, unit.loc, ignore_doors);
                path_blocked_by_unit = check.IsSomething;
#if DEBUG
                if (!path_blocked_by_unit)
                    Print.message("Pathfinding for a unit is completely blocked,\nthis is probably about to crash");
#endif
            }

            // Get the distance from the base tile to each move_range tile

            // At least that's the sensible thing this would do, it was checking the
            //     distance from the target to where the unit is standing, which
            //     crashes when the unit is on impassable terrain //Debug
            foreach (Vector2 loc in move_range)
            {
                Pathfind.ignore_units = ignore_blocking || path_blocked_by_unit;
                //check = Pathfinding.get_distance(target_loc, unit.id, -1, true, loc, ignore_doors); //Debug
                check = Pathfind.get_distance(loc, unit.id, -1, true, target_loc, ignore_doors);
                if (check.IsSomething)
                {
                    target_locs.Add(new LocationDistance(loc, check));
                }
            }
            Pathfind.reset();
            // Raise an error if target_locs.Count == 0 ? //Yeti
            if (target_locs.Count == 0)
                throw new IndexOutOfRangeException("No valid locations to move to");
            int i;
            List<int> target_team = target_units(unit, true);
            // Get a rough idea of the attack ranges of enemies if they were unblocked, and try to stay outside that range too
            if (attack_retreat)
            {
                int[] enemy_distance = new int[target_locs.Count];
                // Ignore enemies with more move than this unit, they'll catch us anyway
                var attack_retreat_enemies = target_team
                    .Where(target_id => Global.game_map.units[target_id].mov <= unit.mov);
#if DEBUG
                attack_retreat_enemies = attack_retreat_enemies.ToList();
#endif
                // If this unit has less move than all enemies, this isn't going to work
                if (attack_retreat_enemies.Count() > 0)
                {
                    // Get the location of each enemy, and their move score + their maximum attack range
                    HashSet<LocationDistance> enemy_ranges = new HashSet<LocationDistance>( // This is kind of a mess //Yeti
                        attack_retreat_enemies.Select(target_id =>
                        {
                            Game_Unit target = Global.game_map.units[target_id];
                            // Don't bother with enemies that have no weapons
                            if (!target.actor.can_attack())
                                return new LocationDistance(Vector2.Zero, -1);
                            return new LocationDistance(target.loc, target.mov + target.max_range_absolute());
                        }).Where(x => x.dist != -1));
                    for (int j = 0; j < enemy_distance.Length; j++)
                    {
                        LocationDistance pair = target_locs[j];
                        enemy_distance[j] = enemy_ranges.Select(x =>
                            {
                                return Global.game_map.distance(pair.loc, x.loc) - x.dist;
                            }).Min();
                    }
                }
                // If any of the tiles to move to are outside the theoretical range of the enemies, only check those tiles
                var safe_tile_indices = Enumerable.Range(0, enemy_distance.Length).Where(x => enemy_distance[x] > 0);
                if (safe_tile_indices.Any())
                {
                    target_locs = new List<LocationDistance>(safe_tile_indices.Select(x => target_locs[x]));
                }
                // Otherwise try to run as far from all enemies as possible
                else
                    retreat = true;
            }
            if (retreat)
                dist = target_locs.Select(x => x.dist).Max();
            else
                dist = target_locs.Select(x => x.dist).Min();
            // If retreating, find the closest enemy to each tile and choose the tile with the enemy furthest away
            if (retreat)
            {
                int[] enemy_distance = new int[target_locs.Count];
                for (int j = 0; j < enemy_distance.Length; j++)
                {
                    LocationDistance pair = target_locs[j];
                    enemy_distance[j] = target_team.Select(target_id =>
                    {
                        Game_Unit target = Global.game_map.units[target_id];
                        return Global.game_map.distance(pair.loc, target.loc);
                    }).Min();
                }
                // Get tile with the closest enemy the furthest away
                int min_distance = enemy_distance[0];
                i = 0;
                for (int j = 1; j < enemy_distance.Length; j++)
                    if (min_distance < enemy_distance[j])
                    {
                        i = j;
                        min_distance = enemy_distance[j];
                    }
            }
            // Else not retreating, just pick a tile at optimal distance
            else
            {
                // Pares list down to tiles at the optimal distance
                int index = 0;
                while (index < target_locs.Count)
                {
                    if (target_locs[index].dist != dist)
                        target_locs.RemoveAt(index);
                    else
                        index++;
                }
                // If one of the valid locations is the current location, why move //Debug
                if (no_move_okay)
                    foreach (LocationDistance pair in target_locs)
                        if (pair.loc == unit.loc)
                            return unit.loc;

                // If some tiles are visible and some aren't, remove invisible ones
                if (Global.game_map.fow)
                {
                    List<LocationDistance> visible_tiles = new List<LocationDistance>();
                    foreach (LocationDistance pair in target_locs)
                        if (!Global.game_map.fow_visibility[unit.team].Contains(pair.loc))
                            visible_tiles.Add(pair);
                    if (visible_tiles.Count > 0)
                        target_locs = visible_tiles;
                }
                // If more than two turns away from the target,
                // cast a ray to the target, and if any locations are on that
                // ray's path, remove any not on it
                if (target_locs[0].dist > unit.mov * 2)
                {
                    // This would act more intelligently if, instead of using
                    // target_loc as the end point for the ray, the pathfinding
                    // determined all chokepoints along the path that every
                    // target_loc would send the unit through, and then cast a
                    // ray to the first point beyond one move range from the
                    // chokepoint before it //@Debug
                    var rayPath = new HashSet<Vector2>(
                        Pathfind.bresenham_supercover(unit.loc, target_loc));
                    if (rayPath.Intersect(target_locs.Select(x => x.loc)).Any())
                    {
                        target_locs = target_locs
                            .Where(x => rayPath.Contains(x.loc))
                            .ToList();
                    }
                }
                
                i = (int)((Global.game_state.ai_turn_rn / 100f) * target_locs.Count);
            }
            // If we have to move, but the selected location is the current one
            if (!no_move_okay && target_locs[i].loc == unit.loc)
                // Returns null to inform callers that no valid location was found, instead of suggesting the current location is valid
                return default(Maybe<Vector2>);
            else
            {
                //something about doors //Yeti
                return target_locs[i].loc;
            }
        }
        #endregion

        public static Vector2? door_target(Game_Unit unit, Vector2 target_loc)
        {
            return door_target(unit, target_loc, unit.loc, -1);
        }
        public static Vector2? door_target(Game_Unit unit, Vector2 target_loc, Vector2 loc, int range)
        {
            List<Vector2> route = unit.actual_move_route(loc, Pathfind.get_route(target_loc, range, unit.id, loc, true, true));
            Pathfind.reset();
            if (route.Count == 0)
                return null;
            foreach(Vector2 move_loc in route)
            {
                if (unit.move_cost(move_loc) >= 0)
                    continue;
                if (Global.game_map.door_locations.ContainsKey(move_loc))
                    return move_loc;
            }
            /*for(int i = route.Count - 1; i >= 0; i--) //Debug
            {
                loc += route[i];
                if (unit.move_cost(loc) >= 0)
                    continue;
                if (Global.game_map.door_locations.ContainsKey(loc))
                    return loc;
            }*/
            return null;
        }
        
        protected static bool can_move_to_hit(
            Game_Unit target, HashSet<Vector2> move_range, Game_Unit attacker,
            int weapon_index, bool offense = true, bool avoidEnemyRange = true)
        {
            int min_range = attacker.min_range(weapon_index);
            int max_range = attacker.max_range(weapon_index);
            HashSet<Vector2> temp_move_range = new HashSet<Vector2>(move_range);

            if (avoidEnemyRange)
                if (!attacker.actor.can_attack())
                    if (offense || !target.actor.has_critical_health())
                        if (temp_move_range.Except(Global.game_state.ai_enemy_attack_range).Any())
                            temp_move_range.ExceptWith(Global.game_state.ai_enemy_attack_range);
            if (attacker.get_weapon_range(new List<int> { weapon_index }, temp_move_range, "").Contains(target.loc))
                return true;
            return false;
        }
        protected static bool can_move_to_hit(Vector2 target, HashSet<Vector2> move_range, Game_Unit attacker, int weapon_index)
        {
            int min_range = attacker.min_range(weapon_index);
            int max_range = attacker.max_range(weapon_index);
            HashSet<Vector2> temp_move_range = new HashSet<Vector2>(move_range); // Why not just use move range, this isn't modified //Yeti
            if (attacker.get_weapon_range(new List<int> { weapon_index }, temp_move_range, "").Contains(target))
                return true;
            return false;
        }

        internal static Maybe<Tuple<int, int>> healing_trade_target(Game_Unit unit)
        {
            if (unit.no_ai_item_trading)
                return default(Maybe<Tuple<int, int>>);
            int trade_target_id = 0, trade_item_index = -1, cost = -1;

            if (unit.actor.has_critical_health() && !unit.can_heal_self())
                // Check allies within 1 tile to trade for healing items
                foreach (int target_id in unit.allies_in_range(1))
                {
                    Game_Unit target = Global.game_map.units[target_id];
                    if (target.no_ai_item_trading)
                        continue;
                    if (unit.same_team(target) && target.can_heal_self(unit))
                    {
                        int item_index = target.heal_self_item(unit);
                        if (trade_item_index == -1 || cost < target.actor.items[item_index].to_item.Cost)
                        {
                            trade_target_id = target_id;
                            trade_item_index = item_index;
                            cost = target.actor.items[item_index].to_item.Cost;
                        }
                    }
                }
            if (trade_item_index != -1)
                return new Tuple<int, int>(trade_target_id, trade_item_index);
            else
                return default(Maybe<Tuple<int, int>>);
        }

        #region Move to target
        // Returns a list of locations that can be used to attack a target
        public static HashSet<Vector2> move_to_hit(Game_Unit target, HashSet<Vector2> move_range, Game_Unit unit, int weapon_index)
        {
            return move_to_hit(target, move_range, unit, weapon_index, true);
        }
        public static HashSet<Vector2> move_to_hit(Game_Unit target, HashSet<Vector2> move_range, Game_Unit unit, int weapon_index, bool offensive)
        {
            HashSet<Vector2> result = unit.hit_from_loc(target.loc, move_range, weapon_index, "");
            int min_range = unit.min_range(weapon_index);
            int max_range = unit.max_range(weapon_index);

            return move_to_target(result, target, min_range, max_range, offensive);
        }

        // Returns a list of locations that can be used to steal from a target
        public static HashSet<Vector2> move_to_one_range(Game_Unit target, HashSet<Vector2> move_range, Game_Unit unit)
        {
            HashSet<Vector2> result = Pathfind.hit_from_loc(target.loc, move_range, 1, 1);
            return move_to_target(result, target, 1, 1, false);
        }

        public static HashSet<Vector2> move_to_target(HashSet<Vector2> result, Game_Unit target, int min_range, int max_range, bool offensive)
        {
            HashSet<Vector2> temp_range = new HashSet<Vector2>();

            // Checks if there are tiles the target can't counter at
            if (offensive)
            {
                if (target.actor.weapon != null)
                {
                    int target_max_range = target.max_range();
                    int target_min_range = target.min_range();
                    if (max_range > target_max_range || min_range < target_min_range)
                    {
                        int tile_dist;
                        foreach (Vector2 tile in result)
                        {
                            tile_dist = (int)(Math.Abs(tile.X - target.loc.X) + Math.Abs(tile.Y - target.loc.Y));
                            if (tile_dist > target_max_range || tile_dist < target_min_range)
                                temp_range.Add(tile);
                        }
                        if (result.Intersect(temp_range).Any())
                            result.IntersectWith(temp_range);
                    }
                }
            }

            best_terrain(ref result);
            return result;
        }

        protected static void best_terrain(ref HashSet<Vector2> result)
        {
            HashSet<Vector2> temp_range = new HashSet<Vector2>();
            // Pares down to def boosting terrain, if any exist
            foreach (Vector2 loc in result)
                if (Global.game_map.terrain_def_bonus(loc) > 0 || Global.game_map.terrain_res_bonus(loc) > 0)
                    temp_range.Add(loc);
            if (result.Intersect(temp_range).Any())
                result.IntersectWith(temp_range);
            temp_range.Clear();
            // Pares down to avo boosting terrain, if any exist
            foreach (Vector2 loc in result)
                if (Global.game_map.terrain_avo_bonus(loc) > 0)
                    temp_range.Add(loc);
            if (result.Intersect(temp_range).Any())
                result.IntersectWith(temp_range);
            //temp_range.Clear();
        }
        #endregion
    }

    /// <summary>
    /// Represents a location on a map, and the travel distance to get there.
    /// </summary>
    struct LocationDistance
    {
        private Vector2 Loc;
        private int Dist;

        #region Accessors
        public Vector2 loc { get { return Loc; } }
        public int dist { get { return Dist; } }
        #endregion

        public override string ToString()
        {
            return string.Format("Loc: {0}; Dist: {1}", Loc, Dist);
        }

        public LocationDistance(Vector2 loc, int dist)
        {
            Loc = loc;
            Dist = dist;
        }
    }

    /// <summary>
    /// Represents a unit on a map, and the travel distance to get there.
    /// </summary>
    struct UnitDistance
    {
        private int _unitId;
        private int Dist;

        #region Accessors
        public Game_Unit unit { get { return Global.game_map.units[_unitId]; } }
        public int UnitId { get { return _unitId; } }
        public int dist { get { return Dist; } }
        #endregion

        public override string ToString()
        {
            return string.Format("Unit: {0}; Dist: {1}", unit, Dist);
        }

        public UnitDistance(int unitId, int dist)
        {
            _unitId = unitId;
            Dist = dist;
        }
    }
}
