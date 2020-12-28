#if DEBUG && WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Calculations.Stats;
using TactileLibrary;

namespace Tactile.Debug_Monitor
{
    class DebugMonitorOverviewPage : DebugMonitorPage
    {
        const int EVENT_QUEUE_COUNT = 11;
        const int EVENT_QUEUE_PREVIOUS = 5;

        public DebugMonitorOverviewPage()
        {
            // Cursor Location
            DebugStringDisplay cursor_loc = new DebugStringDisplay(
                () =>
                {
                    if (!this.on_map || Global.player == null)
                        return "-----";

                    return string.Format("{0}, {1}", Global.player.loc.X, Global.player.loc.Y);
                },
                48, "Cursor Loc", true);
            cursor_loc.loc = new Vector2(0, 0);
            DebugDisplays.Add(cursor_loc);
            // Current team phase
            DebugSwitchDisplay current_phase = new DebugSwitchDisplay(
                () =>
                {
                    return !this.on_map ? -1 : (Global.game_state.team_turn - 1);
                },
                new List<Tuple<int, string, Color>>
                {
                    new Tuple<int, string, Color>(40, "Player", new Color(40, 160, 248)),
                    new Tuple<int, string, Color>(40, "Enemy", new Color(224, 16, 16)),
                    new Tuple<int, string, Color>(40, "Citizen", new Color(24, 208, 16)),
                    new Tuple<int, string, Color>(40, "Intruder", new Color(184, 152, 224)),
                });
            current_phase.loc = new Vector2(0, 16);
            DebugDisplays.Add(current_phase);
            // AI currently processing?
            DebugBooleanDisplay ai_active = new DebugBooleanDisplay(() =>
                !this.on_map ? false : Global.game_state.ai_active,
                "AI Active");
            ai_active.loc = new Vector2(0, 32);
            DebugDisplays.Add(ai_active);
            // Events currently processing?
            DebugBooleanDisplay event_active = new DebugBooleanDisplay(() => Global.game_system.is_interpreter_running, "Event Active");
            event_active.loc = new Vector2(0, 48);
            DebugDisplays.Add(event_active);
            // Current turn
            DebugIntDisplay turn = new DebugIntDisplay(() =>
                !this.on_map ? -1 : Global.game_state.turn, "Turn", 3);
            turn.loc = new Vector2(0, 64);
            DebugDisplays.Add(turn);
            // Difficulty
            DebugStringDisplay difficulty = new DebugStringDisplay(
                () =>
                {
                    if (Global.game_system == null)
                        return "-----";

                    return string.Format("{0}", Global.game_system.Difficulty_Mode);
                },
                48, "Difficulty", false);
            difficulty.loc = new Vector2(80, 64);
            DebugDisplays.Add(difficulty);

            // Ratings
            DebugStringDisplay player_rating = new DebugStringDisplay(
                () =>
                {
                    if (Global.battalion != null)
                    {
                        Maybe<float> rating = Maybe<float>.Nothing;
                        if (Global.scene is Scene_Worldmap)
                        {
                            rating = Global.battalion.average_rating;
                        }
                        else if (this.on_map)
                        {
                            rating = Global.battalion.deployed_average_rating;
                        }
                        if (rating.IsSomething)
                            return string.Format("{0:0.0}", rating.ValueOrDefault);
                    }
                    return "-----";
                }, 48, "Player Rating", text_color: "Blue");
            player_rating.loc = new Vector2(0, 88);
            DebugDisplays.Add(player_rating);
            DebugStringDisplay enemy_rating = new DebugStringDisplay(
                () =>
                {
                    if (!this.on_map || Global.battalion == null)
                        return "-----";
                    return string.Format("{0:0.0}",
                        Global.battalion.enemy_rating);
                }, 48, "Enemy Rating", text_color: "Blue");
            enemy_rating.loc = player_rating.loc + new Vector2(128, 0);
            DebugDisplays.Add(enemy_rating);
            DebugStringDisplay enemy_threat = new DebugStringDisplay(
                () => !this.on_map || Global.battalion == null ?
                    "-----" :
                    string.Format("{0:0.00}x", Global.battalion.enemy_threat),
                48, "Enemy Threat", text_color: "Blue");
            enemy_threat.loc = player_rating.loc + new Vector2(0, 16);
            DebugDisplays.Add(enemy_threat);
            DebugStringDisplay enemy_builds = new DebugStringDisplay(
                () =>
                {
                    if (!this.on_map || Global.battalion == null)
                        return "-----";
                    int builds = Enum_Values.GetEnumCount(typeof(TactileLibrary.Generic_Builds));
                    var build_counts = Global.game_map.units
                        .Where(x => x.Value.is_opposition)
                        .GroupBy(x => x.Value.boss || !x.Value.actor.is_generic_actor ?
                            builds : x.Value.actor.build)
                        .ToDictionary(x => x.Key, x => x.Count());
                    for (int i = 0; i <= builds; i++)
                        if (!build_counts.ContainsKey(i))
                            build_counts[i] = 0;
                    return string.Join(", ", build_counts
                        .OrderBy(x => x.Key)
                        .Select(x => x.Value));
                }, 80, "Enemy Builds", text_color: "Blue");
            enemy_builds.loc = player_rating.loc + new Vector2(0, 32);
            DebugDisplays.Add(enemy_builds);

            //Yeti
            // The following two are very laggy, so they only update every 30 frames
            DebugStringDisplay selected_avg_damage = new DebugStringDisplay(
                () => 
                {
                    if (!this.on_map ||
                            Global.game_map.get_selected_unit() == null)
                        return "-----";
                    var selected_unit = Global.game_map.get_selected_unit();
                    var enemy_units = Global.game_map.units
                        .Values
                        .Where(x => x.is_attackable_team(selected_unit));
                    if (selected_unit.actor.weapon == null ||
                            selected_unit.actor.weapon.is_staff() ||
                            !enemy_units.Any())
                        return "-----";

                    var damages = enemy_units.Select(target =>
                    {
                        var stats = new CombatStats(selected_unit.id, target.id,
                            distance: selected_unit.actor.weapon != null ?
                            selected_unit.actor.weapon.Min_Range : 1)
                        {
                            location_bonuses = CombatLocationBonuses.NoAttackerBonus
                        };
                        return stats.inverse_rounds_to_kill();
                    })
                        .ToList();
                    float average_dmg = damages.Average();

                    return string.Format("{0:0.00}", average_dmg);
                }, 48, "Selected dmg to all", text_color: "Blue");
            selected_avg_damage.loc = player_rating.loc + new Vector2(0, 48);
            selected_avg_damage.set_update_timing(30);
            DebugDisplays.Add(selected_avg_damage);
            DebugStringDisplay selected_taken_damage = new DebugStringDisplay(
                () =>
                {
                    if (!this.on_map ||
                            Global.game_map.get_selected_unit() == null)
                        return "-----";
                    var selected_unit = Global.game_map.get_selected_unit();
                    var enemy_units = Global.game_map.units
                        .Values
                        .Where(x => x.is_attackable_team(selected_unit) &&
                            x.actor.weapon != null && !x.actor.weapon.is_staff());
                    if (!enemy_units.Any())
                        return "-----";

                    var damages = enemy_units.Select(target =>
                    {
                        var stats = new CombatStats(target.id, selected_unit.id,
                            distance: target.actor.weapon != null ?
                            target.actor.weapon.Min_Range : 1)
                        {
                            location_bonuses = CombatLocationBonuses.NoDefenderBonus
                        };
                        return stats.inverse_rounds_to_kill();
                    })
                        .ToList();
                    float average_dmg = damages.Average();

                    return string.Format("{0:0.00}", average_dmg);
                }, 48, "Dmg from all", text_color: "Blue");
            selected_taken_damage.loc = player_rating.loc + new Vector2(136, 48);
            selected_taken_damage.set_update_timing(30);
            DebugDisplays.Add(selected_taken_damage);

            DebugStringDisplay enemies_in_range = new DebugStringDisplay(
                () =>
                {
                    if (!this.on_map)
                        return "-----";
                    var selected_unit = Global.game_map.get_selected_unit();
                    if (selected_unit == null)
                        selected_unit = Global.game_map.get_highlighted_unit();
                    if (selected_unit == null)
                        if (Global.player != null)
                        {
                            selected_unit = Global.game_map.get_unit(Global.player.loc);
                            if (selected_unit != null && !selected_unit.visible_by())
                                selected_unit = null;
                        }
                    IEnumerable<Game_Unit> enemy_units;

                    if (selected_unit == null)
                    {
                        enemy_units = Global.game_map.units
                            .Values
                            .Where(x => !x.is_player_allied);
                        var enemy_ranges = enemy_units
                            .Select(x => Tuple.Create(
                                x, new HashSet<Vector2>(x.attack_range)));
                        int in_range = enemy_ranges
                            .Count(x => x.Item2.Any(y =>
                                {
                                    var target = Global.game_map.get_unit(y);
                                    return target != null && x.Item1.is_attackable_team(target);
                                }));
                        return string.Format("{0} vs {1} PCs",
                            in_range, Global.game_map.allies.Count);
                    }
                    else
                    {
                        enemy_units = Global.game_map.units
                            .Values
                            .Where(x => x.is_attackable_team(selected_unit));
                        var enemy_ranges = enemy_units
                            .Select(x => Tuple.Create(
                                x, new HashSet<Vector2>(x.attack_range)));
                        int in_range = enemy_ranges
                            .Count(x => x.Item2.Contains(selected_unit.loc));
                        return string.Format("{0}", in_range);
                    }
                }, 48, "Enemies in range", text_color: "Blue");
            enemies_in_range.loc = player_rating.loc + new Vector2(0, 64);
            enemies_in_range.set_update_timing(10);
            DebugDisplays.Add(enemies_in_range);


            // Highlit unit
            DebugStringDisplay highlit_unit = new DebugStringDisplay(() =>
                {
                    if (!this.on_map || Global.game_map.get_highlighted_unit() == null)
                    {
                        if (this.on_map && Global.player != null)
                        {
                            // Map objects
                            var mapObject = Global.game_map.get_map_object(Global.player.loc);
                            if (mapObject is Destroyable_Object)
                                return Global.game_map.terrain_data(Global.player.loc).Name;
                            if (mapObject is Siege_Engine)
                                return (mapObject as Siege_Engine).item.name;
                            if (mapObject is LightRune)
                                return "Light Rune";

                            // Visit
                            if (Global.game_map.visit_locations.ContainsKey(Global.player.loc))
                                return Global.game_map.terrain_data(Global.player.loc).Name;
                            if (Global.game_map.door_locations.ContainsKey(Global.player.loc))
                                return Global.game_map.terrain_data(Global.player.loc).Name;
                            if (Global.game_map.chest_locations.ContainsKey(Global.player.loc))
                                return Global.game_map.terrain_data(Global.player.loc).Name;

                            // Seize and escape
                            if (Global.game_map.seize_point_exists(Global.player.loc))
                                return "Seize";
                            var escape = Global.game_map.escape_point_data(-1, -1, Global.player.loc);
                            if (escape.IsSomething)
                            {
                                var escapePoint = escape.ValueOrDefault;
                                if (escapePoint.Team == Constants.Team.PLAYER_TEAM &&
                                        !string.IsNullOrEmpty(escapePoint.EventName))
                                    return "Player Escape";
                                else
                                    return "Escape";
                            }
                            if (Global.game_map.thief_escape_points.ContainsKey(Global.player.loc))
                                return "Thief Escape";
                        }
                        return "(no unit)";
                    }
                    return Global.game_map.get_highlighted_unit().actor.name;
                },
                64, "Highlit Unit", true);
            highlit_unit.loc = new Vector2(0, 176);
            DebugDisplays.Add(highlit_unit);
            DebugStringDisplay highlit_id = new DebugStringDisplay(
                () =>
                {
                    if (!this.on_map || Global.game_map.get_highlighted_unit() == null)
                    {
                        if (this.on_map && Global.player != null)
                        {
                            // Map objects
                            var mapObject = Global.game_map.get_map_object(Global.player.loc);
                            if (mapObject != null)
                            {
                                if (mapObject is Destroyable_Object)
                                    return string.Format("{0}, {1}",
                                        mapObject.id,
                                        (mapObject as Destroyable_Object).event_name);
                                else
                                    return mapObject.id.ToString();
                            }

                            // Visit
                            if (Global.game_map.visit_locations.ContainsKey(Global.player.loc))
                            {
                                var visit = Global.game_map.visit_locations[Global.player.loc];
                                return visit.VisitEvent;
                            }
                            if (Global.game_map.door_locations.ContainsKey(Global.player.loc))
                            {
                                var door = Global.game_map.door_locations[Global.player.loc];
                                return door.VisitEvent;
                            }
                            if (Global.game_map.chest_locations.ContainsKey(Global.player.loc))
                            {
                                var chest = Global.game_map.chest_locations[Global.player.loc];
                                return chest.VisitEvent;
                            }

                            // Seize and escape
                            int team, group;
                            if (Global.game_map.seize_point_exists(Global.player.loc, out team, out group))
                            {
                                if (group != -1)
                                    return string.Format("{0}, Group {1}",
                                        Constants.Team.team_name(team),
                                        group);
                                else
                                    return Constants.Team.team_name(team);
                            }
                            var escape = Global.game_map.escape_point_data(-1, -1, Global.player.loc);
                            if (escape.IsSomething)
                            {
                                var escapePoint = escape.ValueOrDefault;
                                if (escapePoint.Team == Constants.Team.PLAYER_TEAM &&
                                    !string.IsNullOrEmpty(escapePoint.EventName))
                                {
                                    if (escapePoint.LordOnly)
                                        return string.Format("{0} (Lord Only)", escapePoint.EventName);
                                    else
                                        return escapePoint.EventName;
                                }
                                else
                                {
                                    if (escapePoint.Group != -1)
                                        return string.Format("{0}, Group {1}",
                                            Constants.Team.team_name(escapePoint.Team),
                                            escapePoint.Group);
                                    else
                                        return Constants.Team.team_name(escapePoint.Team);
                                }
                            }
                            if (Global.game_map.thief_escape_points.ContainsKey(Global.player.loc))
                                return "-----";
                        }
                        return "-----";
                    }
                    Game_Unit unit = Global.game_map.get_highlighted_unit();

                    string identifier = null;
                    foreach (KeyValuePair<string, int> ident in Global.game_map.unit_identifiers)
                        if (ident.Value == unit.id)
                        {
                            identifier = ident.Key;
                            break;
                        }
                    return string.Format("{0}, {1}", unit.id, string.IsNullOrEmpty(identifier) ? "-----" : identifier);
                },
                112, "ID");
            highlit_id.loc = highlit_unit.loc + new Vector2(highlit_unit.width, 0);
            DebugDisplays.Add(highlit_id);
            DebugStringDisplay highlit_ai = new DebugStringDisplay(
                () =>
                {
                    if (!this.on_map || Global.game_map.get_highlighted_unit() == null)
                        return "-----";
                    Game_Unit unit = Global.game_map.get_highlighted_unit();

                    if (!Game_AI.AI_ENABLED)
                        return "AI Disabled";

                    return string.Format("{0}: {1}", unit.full_ai_mission,
                        Game_AI.MISSION_NAMES.ContainsKey(unit.ai_mission) ?
                        Game_AI.MISSION_NAMES[unit.ai_mission] : "-----");
                },
                144, "Mission");
            highlit_ai.loc = highlit_unit.loc + new Vector2(0, 16);
            DebugDisplays.Add(highlit_ai);

            // Event data
            DebugStringDisplay event_data = new DebugStringDisplay(
                () => Global.game_system.active_event_name, 80, "Active Event");
            event_data.loc = new Vector2(0, 216);
            DebugDisplays.Add(event_data);
            for (int i = 0; i < EVENT_QUEUE_COUNT; i++)
            {
                int j = i - EVENT_QUEUE_PREVIOUS;
                DebugStringDisplay event_queue = new DebugStringDisplay(
                    () => Global.game_system.active_event_command(j), 80, text_color: j == -1 ? "Blue" : (j < 0 ? "Grey" : "White"));
                event_queue.loc = event_data.loc + new Vector2(16, (i + 1) * 16);
                DebugDisplays.Add(event_queue);
            }
        }

        private bool on_map
        {
            get
            {
                return Global.map_exists && !(Global.scene is Scene_Worldmap) &&
                    !(Global.scene is Scene_Save);
            }
        }
    }
}
#endif