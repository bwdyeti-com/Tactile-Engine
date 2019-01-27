using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using FEXNA.AI;
using FEXNA_Library;
using HashSetExtension;
using ListExtension;

namespace FEXNA.State
{
    public enum Ai_Actions { Idle, Selecting_Target, Move_To_Target, Wait_For_Move, Move_Wait, Attack_Target, Wait_For_Combat,
        Search_For_Targets, Move_In, Wait_For_Move_In, Finish_Movement, Pillage_Target, Open_Target, Wait_For_Visit,
        Wait_For_Move_Escape, Move_Wait_Escape, Escape, Wait_For_Steal, Wait_For_Item, Rescue_Target, Wait_For_Rescue,
        Talk_Target, Wait_For_Talk, Wait_For_Dance, Full_Retreat,
        Seize }
    public enum Ai_Turn_Skip_State { NotSkipping, SkipStart, Skipping, SkipEnd }
    enum NextAIUnitModes { Seize, StatusStaff, HealthyAttacker, Healer, Attacker,
        Other, Savior }
    class Game_Ai_State : Game_State_Component
    {
        private int Active_Ai_Unit_Id = -1;
        private HashSet<Vector2> Ai_Move_Range = new HashSet<Vector2>();
        private int Ai_Phase = 0;
        private Ai_Actions Ai_Action = Ai_Actions.Idle;
        private int Ai_Timer = 0;
        private List<int> Ai_Team = new List<int>(), Attack_Ai_Team = new List<int>();
        private HashSet<Vector2> Ai_Enemy_Attack_Range = new HashSet<Vector2>();
        private int Ai_Turn_Rn = 0;
        private bool Player_Ai_Active = false;

        private bool Ai_Healing = false;
        private int[] Temp_Ai_Target = null;
        private Vector2 Temp_Ai_Loc;
        private HashSet<Vector2> Temp_Ai_Locs = new HashSet<Vector2>();
        private HashSet<Vector2> Ai_Defend_Area = new HashSet<Vector2>();
        private List<int> Useable_Weapons = new List<int>();
        private List<int>[] Enemy_Target_Ary;
        private bool Defending_Area = false, Limited_Access_En_Route = false;

        private bool Skip_Ai_Turn = false;
        private int Skip_Ai_Turn_Counter = 0;
        private bool Switching_Ai_Skip = false;
        private int Switching_Ai_Skip_Counter = 0;
        private int SwitchCancelUnitId = -1;

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            writer.Write(Active_Ai_Unit_Id);
            Ai_Move_Range.write(writer);
            writer.Write(Ai_Phase);
            writer.Write((int)Ai_Action);
            writer.Write(Ai_Timer);
            Ai_Team.write(writer);
            Attack_Ai_Team.write(writer);
            Ai_Enemy_Attack_Range.write(writer);
            writer.Write(Ai_Turn_Rn);
            writer.Write(Player_Ai_Active);
        }

        internal override void read(BinaryReader reader)
        {
            Active_Ai_Unit_Id = reader.ReadInt32();
            Ai_Move_Range.read(reader);
            Ai_Phase = reader.ReadInt32();
            Ai_Action = (Ai_Actions)reader.ReadInt32();
            Ai_Timer = reader.ReadInt32();
            Ai_Team.read(reader);
            Attack_Ai_Team.read(reader);
            Ai_Enemy_Attack_Range.read(reader);
            Ai_Turn_Rn = reader.ReadInt32();
            Player_Ai_Active = reader.ReadBoolean();
        }
        #endregion

        #region Accessors
        public bool ai_active { get { return !is_player_turn || Player_Ai_Active; } }
        public bool player_ai_active
        {
            get { return Player_Ai_Active; }
            set { Player_Ai_Active = value; }
        }

        internal int active_ai_unit { get { return Active_Ai_Unit_Id; } }

        internal HashSet<Vector2> ai_move_range
        {
            get { return Ai_Move_Range; }
            set { Ai_Move_Range = value; }
        }

        internal HashSet<Vector2> ai_enemy_attack_range { get { return Ai_Enemy_Attack_Range; } }

        internal int ai_turn_rn { get { return Ai_Turn_Rn; } }

        internal Ai_Turn_Skip_State skip_ai_state
        {
            get
            {
                if (Switching_Ai_Skip)
                    return Skip_Ai_Turn ? Ai_Turn_Skip_State.SkipEnd : Ai_Turn_Skip_State.SkipStart;
                else
                    return Skip_Ai_Turn ? Ai_Turn_Skip_State.Skipping : Ai_Turn_Skip_State.NotSkipping;
            }
        }
        internal bool switching_ai_skip { get { return Switching_Ai_Skip; } }

        internal int switching_ai_skip_counter { get { return Switching_Ai_Skip_Counter; } }

        protected int ai_wait_time
        {
            get
            {
                return Skip_Ai_Turn ? -1 : Constants.Map.AI_WAIT_TIME;
            }
        }

#if DEBUG
        private List<Game_Unit> ai_team_units { get { return Ai_Team.Select(x => Units[x]).ToList(); } }
        private List<Game_Unit> attack_ai_team_units { get { return Attack_Ai_Team.Select(x => Units[x]).ToList(); } }
#endif
        #endregion

        internal void refresh_ai_defend_area()
        {
            Ai_Defend_Area.Clear();

            if (Active_Ai_Unit_Id > -1)
                Ai_Defend_Area.UnionWith(defend_area(
                    Units[Active_Ai_Unit_Id], Team_Turn));
        }
        internal static IEnumerable<Vector2> defend_area(Game_Unit unit, int team)
        {
            var defend_area = Global.game_map.get_defend_area(
                team, unit.group);

            if (defend_area != null && defend_area.Any())
                foreach (Rectangle rect in defend_area)
                    for (int y = rect.Y; y < rect.Bottom; y++)
                        for (int x = rect.X; x < rect.Right; x++)
                            yield return new Vector2(x, y);
        }

        internal override void update()
        {
            bool busy = (!ai_active || is_changing_turns || !is_map_ready() || No_Input_Timer > 0);
            if (Switching_Ai_Skip)
            {
                if (!busy || is_skip_ai_blocked())
                    refresh_ai_skip_switch();
            }
            else if (!busy)
            {
                bool cont = false;
                while (!cont && Waiting_Units.Count == 0)
                {
                    cont = !Skip_Ai_Turn;
                    Game_Unit unit;
                    switch (Ai_Phase)
                    {
                        #region 0: Sets up AI turn
                        case 0:
#if DEBUG
                            if (!Game_AI.AI_ENABLED)
                            {
                                Ai_Phase = 4;
                            }
                            else
#endif
                            if (false)//ally_turn_end_check(false)) // I don't know that this actually does anything //Yeti
                            {
                                Ai_Phase = 4;
                            }
                            else
                            {
                                Ai_Turn_Rn = Global.game_system.get_rng();
                                Ai_Team.Clear();
                                Ai_Team.AddRange(Global.game_map.teams[Team_Turn]);
                                // Remove rescued units
                                int i = 0;
                                while (i < Ai_Team.Count)
                                {
                                    Game_Unit ai_unit = Units[Ai_Team[i]];
                                    if (!ai_unit.ready || !ai_unit_ready_to_act(ai_unit))
                                        Ai_Team.RemoveAt(i);
                                    else
                                        i++;
                                }
                                // Ready AI units exist, so set them to work
                                if (Ai_Team.Count > 0)
                                {
                                    sort_ai_team();
                                    Ai_Phase = 1;
                                }
                                // Skip AI processing
                                else
                                    Ai_Phase = 4;
                            }
                            cont = false;
                            break;
                        #endregion
                        #region 1: Cycles through each enemy to give it something to do
                        case 1:
                            Ai_Healing = false;
                            Ai_Enemy_Attack_Range.Clear();
                            // Decides the current unit to act
                            Attack_Ai_Team = Attack_Ai_Team.Intersect(Ai_Team).ToList();
                            int? active_id = next_ai_unit(Ai_Team, Attack_Ai_Team);
                            // If no units were viable, check again if any units are berserk/uncontrollable
                            if (active_id == null)
                                active_id = next_ai_unit(Ai_Team, Attack_Ai_Team, true);

                            if (active_id == null)
                                Ai_Phase = 4;
                            else
                            {
                                Active_Ai_Unit_Id = (int)active_id;
                                if (Units[Active_Ai_Unit_Id].is_rescued || Units[Active_Ai_Unit_Id].disabled) // unit is prevented from moving, eg sleeping(?) //Yeti
                                    Ai_Phase = 3;
                                else
                                {
                                    refresh_ai_defend_area();
                                    //refresh_terrain_tags(); // only if tags are in a table, not referenced in real time //Yeti
                                    Ai_Phase = 2;
                                    Global.game_state.any_trigger_events();
                                    unit = Units[Active_Ai_Unit_Id];
                                    Global.game_map.check_update_unit_move_range(unit);
                                    if (Ai_Healing)
                                        unit.mission = Game_AI.HEALING_MISSION;
                                    else if (unit.mission == -1)
                                        unit.mission = unit.ai_mission;
                                    // If the unit thinks it's on healing terrain but it's actually not for whatever reason, correct that
                                    if (unit.ai_terrain_healing && !unit.terrain_heals())
                                        unit.ai_terrain_healing = false;
                                    // Determine the enemies from attackable teams, and adds their attack ranges
                                    foreach(int i in unit.attackable_teams())
                                    {
                                        foreach (int enemy_id in Global.game_map.teams[i])
                                        {
                                            if (enemy_id == unit.id)
                                                continue;
                                            Ai_Enemy_Attack_Range.UnionWith(Units[enemy_id].attack_range);
                                            //Enemy_Attack_Range = Enemy_Attack_Range.Distinct().ToList(); //ListOrEquals //HashSet
                                            
                                        }
                                    }
                                    // if suspending after each enemy unit is selected
                                    if (Config.SUSPEND_AFTER_AI_SELECTION)
                                        Global.scene.suspend();
                                }
                            }
                            break;
                        #endregion
                        #region 2: Makes AI act
                        case 2:
                            // Unit killed itself
                            if (!Units.ContainsKey(Active_Ai_Unit_Id))
                            {
                                Ai_Timer = 0;
                                Ai_Phase = 3;
                                cont = false;
                                continue;
                            }

                            unit = Units[Active_Ai_Unit_Id];
                            // Unit crashed into something moving through fog
                            if (!unit.ready && unit.mission == -1) //Debug //unit.blocked //Yeti
                            {
                                Ai_Timer = 0;
                                Ai_Phase = 3;
                                cont = false;
                            }
                            else
                                cont = update_ai_unit(unit, cont);
                            break;
                        #endregion
                        #region 3: Restarts for next unit or goes to cleanup
                        case 3:
                            // If the unit still exists
                            if (Units.ContainsKey(Active_Ai_Unit_Id))
                            {
                                unit = Units[Active_Ai_Unit_Id];
                                // If the unit is not cantoing, end its turn now
                                if (!unit.cantoing)
                                {
                                    cont = false;
                                    reset_for_next_unit();
                                }
                                else
                                {
                                    // If the cantoing unit used up all its movement
                                    if (!unit.has_canto() || unit.full_move() || Game_AI.UNMOVING_MISSIONS.Contains(unit.ai_mission) ||
                                            Game_AI.IMMOBILE_MISSIONS.Contains(unit.mission) || Game_AI.MOVE_TO_TILE_MISSIONS.Contains(unit.mission))
                                        // After this the phase will still be the same so we will return to this block when the unit has finished waiting
                                        unit.start_wait();
                                    // The unit has some movement left, so continue moving a bit
                                    else
                                    {
                                        Ai_Phase = 2;
                                        Ai_Action = Ai_Actions.Idle;
                                        Ai_Timer = 0;
                                        unit.mission = Game_AI.SENTRY_MISSION;
                                    }
                                }
                            }
                            // Skips if unit killed itself :F
                            else
                            {
                                cont = false;
                                reset_for_next_unit();
                            }
                            break;
                        #endregion
                        #region 4: Cleanup
                        case 4:
                            Global.game_state.any_trigger_events();
                            Active_Ai_Unit_Id = -1;
                            Ai_Phase = 5;
                            Ai_Action = Ai_Actions.Idle;
                            Ai_Timer = 0;
                            break;
                        #endregion
                        #region 5: New Turn
                        case 5:
                            switch (Ai_Timer)
                            {
                                case 0:
                                    refresh_move_ranges(true);
                                    Ai_Timer++;
                                    if (switch_out_of_ai_skip())
                                        cont = true;
                                    break;
                                case 1:
                                    Ai_Phase = 0;
                                    Ai_Timer = 0;
                                    Player_Ai_Active = false;
                                    turn_over(this, new EventArgs());
                                    Skip_Ai_Turn = false;
                                    Skip_Ai_Turn_Counter = 0;
                                    cont = true;
                                    break;
                            }
                            break;
                        #endregion
                    }
                    if (Refresh_Move_Ranges)
                        cont = true;
                }
            }
        }

        internal event EventHandler turn_over;

        /// <summary>
        /// Performs AI actions for a unit.
        /// </summary>
        /// <param name="unit">The AI unit being updated.</param>
        /// <param name="cont">False if the AI is not finished, and this should be called again on the same frame.</param>
        protected bool update_ai_unit(Game_Unit unit, bool cont)
        {
            List<int> useable_weapons, useable_staves, useable_ally_staves, useable_enemy_staves;
            List<int>[] ally_target_ary, enemy_target_ary, untargeted_target_ary;
            List<LocationDistance> pillage_targets;
            List<LocationDistance> chest_targets;
            List<LocationDistance> escape_targets;
            List<LocationDistance> defend_targets;
            List<int> steal_targets, dance_targets;
            Vector2 actual_target_loc;
            int unit_index;

#if DEBUG
            if (false) // Skip all AI action to test things //Debug
            {
                Ai_Phase = 3;
                return cont;
            }
#endif
            switch (unit.mission)
            {
                #region 0: Still
                case 0:
                    switch (Ai_Action)
                    {
                        // Look for things to hit
                        case Ai_Actions.Idle:
                            unit.actor.sort_items();
                            if (unit.actor.staff_fix() || unit.actor.weapon == null)
                                unit.mission = 10;
                            else
                            {
                                useable_weapons = new List<int>();
                                enemy_target_ary = unit.enemies_in_range(new HashSet<Vector2> { unit.loc }, true);
                                foreach (int i in enemy_target_ary[1])
                                {
                                    int? weapon_index = unit.actor.weapon_index(i);
                                    if (weapon_index == null)
                                        weapon_index = Constants.Actor.NUM_ITEMS;
                                    if (!useable_weapons.Contains((int)weapon_index))
                                        useable_weapons.Add((int)weapon_index);
                                }
                                if (useable_weapons.Count > 0)
                                    Ai_Action = Ai_Actions.Selecting_Target;
                                else
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                cont = false;
                            }
                            break;
                        // Picks/attacks a target
                        case Ai_Actions.Selecting_Target:
                            switch (Ai_Timer)
                            {
                                case 0:
                                    Ai_Timer++;
                                    break;
                                default:
                                    Ai_Timer = 0;
                                    useable_weapons = new List<int>();
                                    enemy_target_ary = unit.enemies_in_range(new HashSet<Vector2> { unit.loc }, true);
                                    foreach (int i in enemy_target_ary[1])
                                    {
                                        int? weapon_index = unit.actor.weapon_index(i);
                                        if (weapon_index == null)
                                            weapon_index = Constants.Actor.NUM_ITEMS;
                                        if (!useable_weapons.Contains((int)weapon_index))
                                            useable_weapons.Add((int)weapon_index);
                                    }
                                    Temp_Ai_Target = Game_AI.get_atk_target(unit, enemy_target_ary[0], useable_weapons);
                                    if (Temp_Ai_Target == null)
                                    {
                                        // Use items if needed
                                        unit_use_item(unit);
                                        cont = false;
                                    }
                                    else
                                    {
                                        Temp_Ai_Loc = unit.loc;
                                        unit.ai_move_to(Temp_Ai_Loc);
                                        Ai_Action = Ai_Actions.Wait_For_Move;
                                    }
                                    break;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Vector2 target_loc = Units[Temp_Ai_Target[0]].loc;
                                if (!is_off_screen(target_loc))
                                    Ai_Timer = Constants.Map.AI_WAIT_TIME;
                                if (Ai_Timer < ai_wait_time)
                                    Ai_Timer++;
                                else
                                {
                                    Ai_Timer = 0;
                                    if (!Skip_Ai_Turn)
                                    {
                                        Global.player.target_tile(target_loc);
                                        Global.player.force_loc(target_loc);
                                    }
                                    Ai_Action = Ai_Actions.Attack_Target;
                                }
                            }
                            else
                                cont = true;
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Attack_Target:
                            cont |= attack_target(unit);
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Combat:
                            if (!Global.game_state.combat_active)
                                Ai_Phase = 3;
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 1/32: Attack in range (normal/stay out of enemy range)
                case 1:
                case 32:
                    switch (Ai_Action)
                    {
                        // Look for things to hit
                        case Ai_Actions.Idle:
                            unit.actor.sort_items();
                            if (unit.actor.staff_fix() || unit.actor.weapon == null)
                                unit.mission = 10;
                            else
                            {
                                Ai_Move_Range = update_ai_move_range(unit);
                                if (unit.mission == Game_AI.SAFE_ATTACK_MISSION)
                                {
                                    if (Defending_Area && Ai_Move_Range.Intersect(Ai_Defend_Area).Any())
                                        Ai_Move_Range.IntersectWith(Ai_Defend_Area);
                                    if (Ai_Move_Range.Except(Ai_Enemy_Attack_Range).Any())
                                        Ai_Move_Range.ExceptWith(Ai_Enemy_Attack_Range);
                                }
                                useable_weapons = new List<int>();
                                enemy_target_ary = unit.enemies_in_range(Ai_Move_Range, true);
                                foreach (int i in enemy_target_ary[1])
                                {
                                    int? weapon_index = unit.actor.weapon_index(i);
                                    if (weapon_index == null)
                                        weapon_index = Constants.Actor.NUM_ITEMS;
                                    if (!useable_weapons.Contains((int)weapon_index))
                                        useable_weapons.Add((int)weapon_index);
                                }

                                // Healing~
                                if (!unit.berserk)
                                {
                                    if (unit.actor.has_critical_health())
                                    {
                                        bool can_kill = false;
                                        if (useable_weapons.Count > 0)
                                        {
                                            can_kill = Game_AI.can_kill_target(unit, enemy_target_ary[0], useable_weapons, true);
                                        }

                                        // If not too busy trying to finish a target, and not berserk
                                        if (!can_kill)
                                        {
                                            bool wants_healing = ai_unit_seeks_healing(unit, true);
                                            if (wants_healing)
                                            {
                                                unit.mission = Game_AI.ATTACK_IN_RANGE_HEAL_SELF;
                                                Ai_Action = Ai_Actions.Idle;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                    }
                                }
                                unit.ai_terrain_healing = false;

                                if (useable_weapons.Count > 0)
                                    Ai_Action = Ai_Actions.Selecting_Target;
                                else
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                cont = false;
                            }
                            break;
                        // Picks/attacks a target
                        case Ai_Actions.Selecting_Target:
                            switch (Ai_Timer)
                            {
                                case 0:
                                    Ai_Timer++;
                                    break;
                                default:
                                    Ai_Timer = 0;
                                    useable_weapons = new List<int>();
                                    enemy_target_ary = unit.enemies_in_range(Ai_Move_Range, true);
                                    foreach (int i in enemy_target_ary[1])
                                    {
                                        int? weapon_index = unit.actor.weapon_index(i);
                                        if (weapon_index == null)
                                            weapon_index = Constants.Actor.NUM_ITEMS;
                                        if (!useable_weapons.Contains((int)weapon_index))
                                            useable_weapons.Add((int)weapon_index);
                                    }
                                    Temp_Ai_Target = Game_AI.get_atk_target(unit, enemy_target_ary[0], useable_weapons, true);
                                    if (Temp_Ai_Target == null)
                                    {
                                        // Use items if needed
                                        unit_use_item(unit);
                                        cont = false;
                                    }
                                    else
                                    {
                                        Ai_Action = Ai_Actions.Move_To_Target;
                                    }
                                    break;
                            }
                            break;
                        // Moves to enemy
                        case Ai_Actions.Move_To_Target:
                            switch (Ai_Timer)
                            {
                                case 0:
                                    Ai_Timer++;
                                    // If target data included a location
                                    if (Temp_Ai_Target[3] != Config.OFF_MAP.X && Temp_Ai_Target[4] != Config.OFF_MAP.Y)
                                    {
                                        Temp_Ai_Loc = new Vector2(Temp_Ai_Target[3], Temp_Ai_Target[4]);
                                    }
                                    else
                                    {
                                        HashSet<Vector2> target_ary = Game_AI.move_to_hit( //Debug
                                            Units[Temp_Ai_Target[0]], Ai_Move_Range, Units[Active_Ai_Unit_Id], Temp_Ai_Target[1] - 1);
                                        // I switched to this version for some reason, but it crashed when a magic user tried to attack with a siege tome //Yeti
                                        // First item was a normal tome, second was siege, so normal tome wasn't in range
                                        // Why did I change it in the past?
                                        //List<Vector2> target_ary = Game_AI.move_to_hit(
                                        //    Units[Ai_Target[0]], Ai_Move_Range, Units[Active_Ai_Unit_Id], 0);

                                        int target_index = (int)((Ai_Turn_Rn / 100.0f) * target_ary.Count);
                                        Temp_Ai_Loc = target_ary.ToArray()[target_index]; //HashSet
                                        // Tests for doors in the way // Not sure if this one works //Yeti
                                        if (unit.can_open_door())
                                        {
                                            Vector2? door_target = Game_AI.door_target(unit, Temp_Ai_Loc);
                                            // Doors are in the way, head toward them with intent of opening
                                            if (door_target != null)
                                            {
                                                Temp_Ai_Loc = (Vector2)door_target;
                                                unit.mission = 22;
                                                Ai_Action = Ai_Actions.Idle;
                                                Ai_Timer = 0;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                    }
                                    Global.player.force_loc(unit.loc);
                                    break;
                                default:
                                    if (!Scrolling || Skip_Ai_Turn)
                                    {
                                        Ai_Timer = 0;
                                        unit.ai_move_to(Temp_Ai_Loc);
                                        Ai_Action = Ai_Actions.Wait_For_Move;
                                    }
                                    break;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Vector2 target_loc = Units[Temp_Ai_Target[0]].loc;
                                if (!is_off_screen(target_loc))
                                    Ai_Timer = Constants.Map.AI_WAIT_TIME;
                                if (Ai_Timer < ai_wait_time)
                                    Ai_Timer++;
                                else
                                {
                                    Ai_Timer = 0;
                                    if (!Skip_Ai_Turn)
                                    {
                                        Global.player.target_tile(target_loc);
                                        Global.player.force_loc(target_loc);
                                    }
                                    Ai_Action = Ai_Actions.Attack_Target;
                                }
                            }
                            else
                                cont = true;
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Attack_Target:
                            cont |= attack_target(unit);
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Combat:
                            if (!Global.game_state.combat_active)
                                Ai_Phase = 3;
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 2/3/31: Seek and Attack (any/weakest/while retreating)
                case 2:
                case 3:
                case 31:
                    switch (Ai_Action)
                    {
                        #region Look for things to hit
                        case Ai_Actions.Idle:
                            unit.actor.sort_items();
                            // If can't actually fight, retreat
                            if (!unit.berserk && (unit.actor.staff_fix() || unit.actor.weapon == null))
                            {
                                // If rescuing, need to offload this guy before retreating
                                if (!unit.berserk && unit.is_rescuing)
                                    unit.mission = Game_AI.RESCUE_DROP_MISSION;
                                else
                                    unit.mission = Game_AI.SENTRY_MISSION;
                            }
                            else
                            {
                                Ai_Move_Range = update_ai_move_range(unit);
                                var seize_points = Global.game_map.get_seize_points(unit.team, unit.group);
                                // If seizing is possible and a seize point is in range, get it
                                if (unit.mission != Game_AI.RETREATING_ATTACK_MISSION &&
                                        !unit.uncontrollable && seize_points.Any())
                                    if (Ai_Move_Range.Intersect(seize_points).Any())
                                    {
                                        Ai_Action = Ai_Actions.Seize;
                                        cont = false;
                                        return cont;
                                    }
                                // Check which weapons are usable and which targets they can hit
                                Useable_Weapons.Clear();
                                // If this unit is retreat attacking, and there are no tiles in its move range that the enemy can't hit
                                // This shouldn't be necessary anymore, due to some changes where retreat attacks can be anywhere along the route //Debug
                                //if (unit.mission == Game_AI.RETREATING_ATTACK_MISSION && !unit.move_range.Except(Ai_Enemy_Attack_Range).Any())
                                //{
                                //    // Only check attacks in place
                                //    Enemy_Target_Ary = unit.enemies_in_range(new HashSet<Vector2> { unit.loc }, true);
                                //}
                                if (Defending_Area)
                                {
                                    HashSet<Vector2> move_range = new HashSet<Vector2>(Ai_Move_Range);
                                    move_range.IntersectWith(Ai_Defend_Area);
                                    Enemy_Target_Ary = unit.enemies_in_range(move_range, true);
                                }
                                else
                                    Enemy_Target_Ary = unit.enemies_in_range(Ai_Move_Range, true);
                                foreach (int i in Enemy_Target_Ary[1])
                                {
                                    int? weapon_index = unit.actor.weapon_index(i);
                                    if (weapon_index == null)
                                        weapon_index = Constants.Actor.NUM_ITEMS;
                                    if (!Useable_Weapons.Contains((int)weapon_index))
                                        Useable_Weapons.Add((int)weapon_index);
                                }

                                // If we want to attack but rescuing and we ended up in enemy range, whoops need to offload this guy
                                if (Useable_Weapons.Count > 0 && !unit.berserk &&
                                        unit.is_rescuing && Ai_Enemy_Attack_Range.Contains(unit.loc))
                                    // And also we're not on an escape mission, don't drop your friend
                                    if (unit.ai_mission != Game_AI.ESCAPE_MISSION)
                                    {
                                        // If drop would fail, we obviously don't want to attempt it
                                        Temp_Ai_Target = Game_AI.search_for_rescue_drop(unit);
                                        if (Temp_Ai_Target != null)
                                        {
                                            unit.mission = Game_AI.RESCUE_DROP_MISSION;
                                            Ai_Action = Ai_Actions.Idle;
                                            Ai_Timer = 0;
                                            cont = false;
                                            return cont;
                                        }
                                    }
                                // Healing~/Rescue drop
                                if (!unit.berserk && unit.mission != Game_AI.RETREATING_ATTACK_MISSION)
                                {
                                    if ((unit.is_rescuing && unit.is_weighted_by_ally) || unit.actor.has_critical_health())
                                    {
                                        bool can_kill = false;
                                        if (Useable_Weapons.Count > 0)
                                        {
                                            if (unit.mission == 2)
                                                can_kill = true;
                                            else
                                                can_kill = Game_AI.can_kill_target(unit, Enemy_Target_Ary[0], Useable_Weapons, true);
                                        }

                                        // If not too busy trying to finish a target, and not berserk
                                        if (!can_kill)
                                        {
                                            if (unit.is_rescuing)
                                            {
                                                Ai_Action = Ai_Actions.Search_For_Targets;
                                                cont = false;
                                                return cont;
                                            }
                                            bool wants_healing = ai_unit_seeks_healing(unit);
                                            if (wants_healing)
                                            {
                                                unit.mission = Game_AI.HEALING_MISSION;
                                                Ai_Action = Ai_Actions.Idle;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                    }

                                    // If the unit is on a fort waiting to heal up, keep healing
                                    if (!unit.actor.is_full_hp() && unit.terrain_heals() && unit.ai_terrain_healing)
                                    {
                                        // Check if any enemies can be hit without moving; if so, switch to attack in place mission instead
                                        enemy_target_ary = unit.enemies_in_range(new HashSet<Vector2> { unit.loc }, true);
                                        if (enemy_target_ary[0].Count > 0)
                                        {
                                            unit.mission = 0;
                                            Ai_Action = Ai_Actions.Idle;
                                            cont = false;
                                            return cont;
                                        }
                                        // If any enemies in attack range can hit at ranges this unit can't, skip this and attack them pre-emptively
                                        bool should_attack = false;
                                        foreach (int unit_id in Enemy_Target_Ary[0])
                                        {
                                            if (Units[unit_id].min_range_absolute() < unit.min_range_absolute() ||
                                                Units[unit_id].max_range_absolute() > unit.max_range_absolute() ||
                                                Units[unit_id].has_uncounterable())
                                            {
                                                should_attack = true;
                                                break;
                                            }
                                        }
                                        if (!should_attack)
                                        {
                                            unit.mission = Game_AI.HEALING_MISSION;
                                            Ai_Action = Ai_Actions.Idle;
                                            cont = false;
                                            return cont;
                                        }
                                    }
                                }
                                unit.ai_terrain_healing = false;

                                if (Useable_Weapons.Count > 0)
                                    Ai_Action = Ai_Actions.Selecting_Target;
                                else
                                {
                                    if (unit.mission == Game_AI.RETREATING_ATTACK_MISSION)
                                    {
                                        if (Ai_Enemy_Attack_Range.Contains(unit.loc))
                                            unit.mission = Game_AI.SENTRY_MISSION;
                                        else
                                            unit_use_item(unit);
                                    }
                                    else
                                        Ai_Action = Ai_Actions.Search_For_Targets;
                                }
                                cont = false;
                            }
                            break;
                        #endregion
                        #region Picks/attacks a target
                        case Ai_Actions.Selecting_Target:
                            // Determines target
                            if (unit.mission == Game_AI.RETREATING_ATTACK_MISSION)
                            {
                                // If this unit is retreat attacking, and there are no tiles in its move range that the enemy can't hit
                                // As above with checking the base square only, this shouldn't be necessary anymore //Debug
                                //bool only_check_without_moving = !unit.move_range.Except(Ai_Enemy_Attack_Range).Any();
                                Temp_Ai_Target = Game_AI.get_retreating_atk_target(unit, Enemy_Target_Ary[0], Useable_Weapons, true);
                            }
                            else if (unit.mission == 2)
                                Temp_Ai_Target = Game_AI.get_atk_target(unit, Enemy_Target_Ary[0], Useable_Weapons, true, false);
                            else
                                Temp_Ai_Target = Game_AI.get_atk_target(unit, Enemy_Target_Ary[0], Useable_Weapons, true);
                            // If no target was found, move closer to an enemy instead of attacking
                            if (Temp_Ai_Target == null)
                            {
                                // Unless this unit is trying to do a strafing attack
                                if (unit.mission == Game_AI.RETREATING_ATTACK_MISSION)
                                {
                                    // Most of this was debugged out at some point, why though? //Debug
                                    // In which case run away if an enemy can hit them
                                    if (Ai_Enemy_Attack_Range.Contains(unit.loc))
                                    {
                                        unit.mission = Game_AI.RETREAT_MISSION;
                                        //unit.mission = Game_AI.SENTRY_MISSION; //Debug
                                        Ai_Action = Ai_Actions.Idle;
                                    }
                                    // Or just stay still and maybe use an item
                                    else
                                        unit_use_item(unit);
                                }
                                else
                                    Ai_Action = Ai_Actions.Search_For_Targets;
                            }
                            else
                            {
                                Ai_Action = Ai_Actions.Move_To_Target;
                            }
                            break;
                        #endregion
                        #region Moves to enemy
                        case Ai_Actions.Move_To_Target:
                            switch (Ai_Timer)
                            {
                                case 0:
                                    Ai_Timer++;
                                    // If target data included a location
                                    if (Temp_Ai_Target.Length > 3 && Temp_Ai_Target[3] != Config.OFF_MAP.X && Temp_Ai_Target[4] != Config.OFF_MAP.Y)
                                    {
                                        Temp_Ai_Loc = new Vector2(Temp_Ai_Target[3], Temp_Ai_Target[4]);
                                    }
                                    else
                                    {
                                        //List<Vector2> target_ary = Game_AI.move_to_hit( //Debug
                                        //    Units[Ai_Target[0]], Ai_Move_Range, Units[Active_Ai_Unit_Id], Ai_Target[1] - 1);
                                        HashSet<Vector2> target_ary = Game_AI.move_to_hit(
                                            Units[Temp_Ai_Target[0]], Ai_Move_Range, Units[Active_Ai_Unit_Id], 0);

                                        int target_index = (int)((Ai_Turn_Rn / 100.0f) * target_ary.Count);
                                        Temp_Ai_Loc = target_ary.ToArray()[target_index]; //HashSet
                                        // Tests for doors in the way // Not sure if this one works //Yeti
                                        if (unit.can_open_door())
                                        {
                                            Vector2? door_target = Game_AI.door_target(unit, Temp_Ai_Loc);
                                            // Doors are in the way, head toward them with intent of opening
                                            if (door_target != null)
                                            {
                                                Temp_Ai_Loc = (Vector2)door_target;
                                                unit.mission = 22;
                                                Ai_Action = Ai_Actions.Idle;
                                                Ai_Timer = 0;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                    }
                                    Global.player.force_loc(unit.loc);
                                    break;
                                default:
                                    if (!Scrolling || Skip_Ai_Turn)
                                    {
                                        Ai_Timer = 0;
                                        unit.ai_move_to(Temp_Ai_Loc);
                                        Ai_Action = Ai_Actions.Wait_For_Move;
                                    }
                                    break;
                            }
                            break;
                        #endregion
                        #region Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Vector2 target_loc = Global.game_map
                                    .attackable_map_object(Temp_Ai_Target[0])
                                    .loc;
                                if (!is_off_screen(target_loc))
                                    Ai_Timer = Constants.Map.AI_WAIT_TIME;
                                if (Ai_Timer < ai_wait_time)
                                    Ai_Timer++;
                                else
                                {
                                    Ai_Timer = 0;
                                    if (!Skip_Ai_Turn)
                                    {
                                        Global.player.target_tile(target_loc);
                                        Global.player.force_loc(target_loc);
                                    }
                                    Ai_Action = Ai_Actions.Attack_Target;
                                }
                            }
                            else
                                cont = true;
                            break;
                        #endregion
                        #region Attacks target
                        case Ai_Actions.Attack_Target:
                            cont |= attack_target(unit);
                            break;
                        #endregion
                        #region Goes to next unit
                        case Ai_Actions.Wait_For_Combat:
                            if (!Global.game_state.combat_active)
                                Ai_Phase = 3;
                            else
                                cont = true;
                            break;
                        #endregion
                        #region Capture a seize point in range
                        case Ai_Actions.Seize:
                            Maybe<Vector2>[] search_loc = null;
                            if (Global.game_map.get_seize_points(unit.team, unit.group).Any())
                                search_loc = Game_AI.search_for_seize(unit, false);
                            // No direct routes found
                            if (search_loc[1].IsNothing)
                            {
                                unit.mission = Game_AI.SENTRY_MISSION;
                                Ai_Action = Ai_Actions.Idle;
                                Ai_Timer = 0;
                                return true;
                            }
                            // Direct Route found, continue as normal
                            actual_target_loc = search_loc[0];
                            Temp_Ai_Loc = search_loc[1];
                            if (!unit.move_range.Contains(Temp_Ai_Loc))
                            {
                                throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                            }
                            if (is_blocked(Temp_Ai_Loc, Active_Ai_Unit_Id, false) || Temp_Ai_Loc == unit.loc)
                            {
                                // Use items if needed
                                unit_use_item(unit);
                                cont = false;
                            }
                            else
                            {
                                // Tests for doors in the way
                                if (unit.can_open_door())
                                {
                                    Vector2? door_target = Game_AI.door_target(unit, actual_target_loc, Temp_Ai_Loc, -1);
                                    if (door_target != null && Global.game_map.distance((Vector2)door_target, Temp_Ai_Loc) != 1)
                                        door_target = null;
                                    // Doors are in the way, head toward them with intent of opening
                                    if (door_target != null)
                                    {
                                        Temp_Ai_Loc = (Vector2)door_target;
                                        unit.mission = 22;
                                        Ai_Action = Ai_Actions.Idle;
                                        Ai_Timer = 0;
                                        cont = false;
                                        return cont;
                                    }
                                }
                                Ai_Action = Ai_Actions.Move_In;
                                Global.player.force_loc(unit.loc);
                            }
                            break;
                        #endregion
                        case Ai_Actions.Search_For_Targets:
                            search_loc = null;
                            bool seizing = false;
                            // If seize points are a thing for this unit's group,
                            // move toward those instead of enemies
                            if (!unit.uncontrollable && Global.game_map.get_seize_points(unit.team, unit.group).Any() && !Defending_Area)
                                search_loc = Game_AI.search_for_seize(unit, true);

                            if (search_loc == null || search_loc[1].IsNothing)
                            {
                                if (Defending_Area)
                                    search_loc = Game_AI.search_for_enemy(unit, Ai_Defend_Area);
                                else if (unit.mission == 2)
                                    search_loc = Game_AI.search_for_enemy(unit, 0, false);
                                else
                                    search_loc = Game_AI.search_for_enemy(unit);
                            }
                            else
                                seizing = true;
                            // No direct routes found
                            if (search_loc[1].IsNothing)// && !Defending_Area) //Debug
                            {
                                if (Defending_Area)
                                    search_loc = Game_AI.search_attack_through_walls(unit, Ai_Defend_Area);
                                else
                                    search_loc = Game_AI.search_attack_through_walls(unit);
                                // No indirect routes found either
                                if (search_loc[1].IsNothing)
                                {
                                    unit.mission = Game_AI.SENTRY_MISSION;
                                    Ai_Action = Ai_Actions.Idle;
                                    Ai_Timer = 0;
                                    return true;
                                }
                            }
                            // Direct Route found, continue as normal
                            actual_target_loc = search_loc[0];
                            Temp_Ai_Loc = search_loc[1];
                            if (!unit.move_range.Contains(Temp_Ai_Loc))
                            {
                                throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                            }
                            if (is_blocked(Temp_Ai_Loc, Active_Ai_Unit_Id, false) || Temp_Ai_Loc == unit.loc)
                            {
                                if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                            }
                            else
                            {
                                // Tests for doors in the way
                                if (unit.can_open_door())
                                {
                                    Vector2? door_target = Game_AI.door_target(unit, actual_target_loc, Temp_Ai_Loc, -1);
                                    if (door_target != null && Global.game_map.distance((Vector2)door_target, Temp_Ai_Loc) != 1)
                                        door_target = null;
                                    // Doors are in the way, head toward them with intent of opening
                                    if (door_target != null)
                                    {
                                        Temp_Ai_Loc = (Vector2)door_target;
                                        unit.mission = 22;
                                        Ai_Action = Ai_Actions.Idle;
                                        Ai_Timer = 0;
                                        cont = false;
                                        return cont;
                                    }
                                }
                                // Rescuing~
                                if (!unit.berserk && unit.mission == 3)
                                    // And also we're not on an escape mission, don't drop your friend/rescue people
                                    if (unit.ai_mission != Game_AI.ESCAPE_MISSION)
                                    {
                                        if (unit.is_rescuing)
                                        {
                                            Temp_Ai_Target = Game_AI.search_for_rescue_drop(unit, Temp_Ai_Loc, actual_target_loc);
                                            if (Temp_Ai_Target != null)
                                            {
                                                if (!Ai_Enemy_Attack_Range.Contains(new Vector2(Temp_Ai_Target[2], Temp_Ai_Target[3])))
                                                { }
                                                unit.mission = Game_AI.RESCUE_DROP_MISSION;
                                                Ai_Action = Ai_Actions.Idle;
                                                Ai_Timer = 0;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                        else
                                        {
                                            Temp_Ai_Target = Game_AI.search_for_rescue(unit, actual_target_loc, seizing);
                                            if (Temp_Ai_Target != null)
                                            {
                                                // Edit the bit that figures out where to move to when rescuing so it doesn't move on top of allies
                                                unit.mission = Game_AI.RESCUE_MISSION;
                                                Ai_Action = Ai_Actions.Idle;
                                                Ai_Timer = 0;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                    }
                                Ai_Action = Ai_Actions.Move_In;
                                Global.player.force_loc(unit.loc);
                            }
                            break;
                        case Ai_Actions.Move_In:
                            if (!unit.move_range.Contains(Temp_Ai_Loc))
                            {
                                throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                            }
                            else if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move_In;
                            }
                            break;
                        case Ai_Actions.Wait_For_Move_In:
                            if (unit.loc == Temp_Ai_Loc)
                                Ai_Action = Ai_Actions.Finish_Movement;
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (!unit.cantoing)
                                {
                                    // If not cantoing and at a seize point, seize it
                                    if (Global.game_map.get_seize_points(unit.team, unit.group).Contains(unit.loc))
                                        Global.game_map.seize_point(unit.team, unit.loc);
                                    else
                                    {
                                        // Use items if needed
                                        unit_use_item(unit);
                                        cont = false;
                                    }
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Wait_For_Combat;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 4: Pillage
                case 4:
                    switch (Ai_Action)
                    {
                        // Look for houses to burn
                        case Ai_Actions.Idle:
                            // If nothing left to pillage, switch to attack any
                            if (!Global.game_map.any_pillage)
                            {
                                unit.mission = Game_AI.PILLAGE_OVER_MISSION;

                                // This permanently changes mission instead of temporarily, is this wanted? //Debug
                                //unit.ai_mission = (unit.ai_priority) * Game_AI.MISSION_COUNT + unit.mission;
                                cont = false;
                            }
                            else
                            {
                                unit.actor.sort_items();
                                Ai_Move_Range = update_ai_move_range(unit);
                                // Gets the closest village to attack
                                pillage_targets = Game_AI.pillage_targets(unit);

                                // If there are targets to move to
                                if (pillage_targets.Count > 0)
                                {
                                    //target_sort(pillage_targets); //Debug

                                    // Village locations that are within one move and not blocked
                                    var in_range_pillaging = pillage_targets
                                        .Where(x => x.dist <= unit.mov &&
                                            !Global.game_map.is_blocked(x.loc, unit.id));

                                    if (in_range_pillaging.Any())
                                    {
                                        Temp_Ai_Loc = in_range_pillaging.First().loc; //pillage_targets[0].loc; //Debug

                                        // Tests for doors in the way // Not sure if this one works //Yeti
                                        if (unit.can_open_door())
                                        {
                                            Vector2? door_target = Game_AI.door_target(unit, Temp_Ai_Loc);
                                            // Doors are in the way, head toward them with intent of opening
                                            if (door_target != null)
                                            {
                                                Temp_Ai_Loc = (Vector2)door_target;
                                                unit.mission = 22;
                                                Ai_Action = Ai_Actions.Idle;
                                                Ai_Timer = 0;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                        cont = Skip_Ai_Turn;
                                        switch_out_of_ai_skip();
                                        Ai_Action = Ai_Actions.Move_To_Target;
                                        Global.player.force_loc(unit.loc);

                                    }
                                    // If we can't get to the village this turn, switch to the mission that handles moving closer
                                    else
                                    {
                                        target_sort(pillage_targets);
                                        Temp_Ai_Loc = pillage_targets[0].loc;
                                        Temp_Ai_Locs = new HashSet<Vector2>(pillage_targets.Select(x => x.loc));
                                        unit.mission = 23;
                                        Limited_Access_En_Route = true;
                                        Ai_Action = Ai_Actions.Idle;
                                    }
                                }
                                else
                                {
                                    unit.mission = 2;
                                    Ai_Action = Ai_Actions.Idle;
                                    cont = false;
                                }
                            }
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until pillager has moved
                        case Ai_Actions.Wait_For_Move:
                            if (unit.loc == Temp_Ai_Loc)
                                Ai_Action = Ai_Actions.Move_Wait;
                            else
                                cont = true;
                            break;
                        // Finishes movement, waits a few ticks
                        case Ai_Actions.Move_Wait:
                            switch (Ai_Timer)
                            {
                                case 19:
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Pillage_Target;
                                    break;
                                default:
                                    Ai_Timer++;
                                    break;
                            }
                            break;
                        // Pillages target
                        case Ai_Actions.Pillage_Target:
                            // Unit movement locked in
                            unit.moved();
                            Global.game_state.call_visit(Visit_Modes.Visit, unit.id, Temp_Ai_Loc, true);
                            Ai_Action = Ai_Actions.Wait_For_Visit;
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Visit:
                            if (!Global.game_state.visit_active)
                                Ai_Phase = 3;
                            break;
                    }
                    break;
                #endregion

                #region 5: Thief routine
                case 5:
                    switch (Ai_Action)
                    {
                        // Look for chests
                        case Ai_Actions.Idle:
                            // No chests left, switch to escaping
                            if (!Global.game_map.chest_locations.Any() || !unit.can_open_chest() || unit.actor.is_full_items)
                            {
                                Ai_Timer = 0;
                                unit.mission = 21;
                                cont = false;
                            }
                            else
                            {
                                unit.actor.sort_items();
                                Ai_Move_Range = update_ai_move_range(unit);
                                HashSet<Vector2> target_array = new HashSet<Vector2>(Global.game_map.chest_locations.Keys);
                                chest_targets = Game_AI.distance_to_locations(unit, target_array);

                                if (chest_targets.Count > 0)
                                {
                                    target_sort(chest_targets);
                                    // If all chests are out of range or have someone standing on them
                                    if (chest_targets.All(x => x.dist > unit.mov ||
                                         is_blocked(x.loc, Active_Ai_Unit_Id, true)))
                                    {
                                        // If the unit can't steal, use standard move toward target code, and thus attack things along the way
                                        if (!unit.can_steal())
                                        {
                                            Temp_Ai_Loc = chest_targets[0].loc;
                                            Temp_Ai_Locs = new HashSet<Vector2>(chest_targets.Select(x => x.loc));
                                            unit.mission = 23;
                                            Limited_Access_En_Route = true;
                                            Ai_Action = Ai_Actions.Idle;
                                            return cont;
                                        }

                                        Temp_Ai_Loc = chest_targets[0].loc;
                                        Ai_Action = Ai_Actions.Search_For_Targets;
                                        cont = false;
                                    }
                                    else
                                    {
                                        // Gets the first unoccupied chest
                                        Temp_Ai_Loc = chest_targets
                                            .Where(x => !is_blocked(x.loc, Active_Ai_Unit_Id, true))
                                            .First().loc;
                                        // Tests for doors in the way
                                        if (unit.can_open_door())
                                        {
                                            Vector2? door_target = Game_AI.door_target(unit, Temp_Ai_Loc);
                                            // Doors are in the way, head toward them with intent of opening
                                            if (door_target != null)
                                            {
                                                Temp_Ai_Loc = (Vector2)door_target;
                                                unit.mission = 22;
                                                Ai_Action = Ai_Actions.Idle;
                                                Ai_Timer = 0;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                        Ai_Action = Ai_Actions.Move_To_Target;
                                        cont = Skip_Ai_Turn;
                                        switch_out_of_ai_skip();
                                        Global.player.force_loc(unit.loc);
                                    }
                                }
                                // No way to go to any chests, steal (was retreat, but then they might leave for temporary blocks?)
                                else
                                {
                                    unit.mission = Game_AI.STEAL_MISSION; // 21; //Debug
                                    Ai_Action = Ai_Actions.Idle;
                                    cont = false;
                                }
                            }
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until thief has moved
                        case Ai_Actions.Wait_For_Move:
                            if (unit.loc == Temp_Ai_Loc)
                                Ai_Action = Ai_Actions.Move_Wait;
                            else
                                cont = true;
                            break;
                        // Finishes movement, waits a few ticks
                        case Ai_Actions.Move_Wait:
                            switch (Ai_Timer)
                            {
                                case 19:
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Open_Target;
                                    break;
                                default:
                                    Ai_Timer++;
                                    break;
                            }
                            break;
                        // Opens chest
                        case Ai_Actions.Open_Target:
                            // Unit movement locked in
                            unit.moved();
                            Global.game_state.call_visit(Visit_Modes.Chest, unit.id, Temp_Ai_Loc);
                            Ai_Action = Ai_Actions.Wait_For_Visit;
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Visit:
                            if (!Global.game_state.visit_active)
                                Ai_Phase = 3;
                            break;
                        // Moves toward target
                        case Ai_Actions.Search_For_Targets:
                            actual_target_loc = Temp_Ai_Loc;
                            Maybe<Vector2> search_loc = Game_AI.path_to_target(unit, Temp_Ai_Loc);
                            if (search_loc.IsNothing)
                            {
                                Ai_Phase = 3;
                            }
                            else
                            {
                                Temp_Ai_Loc = search_loc;
                                if (is_blocked(Temp_Ai_Loc, Active_Ai_Unit_Id, false))
                                {
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Idle;
                                    unit.mission = 21;
                                    cont = false;
                                }
                                else
                                {
                                    if (!unit.move_range.Contains(Temp_Ai_Loc))
                                    {
                                        throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                                    }
                                    if (is_blocked(Temp_Ai_Loc, Active_Ai_Unit_Id, false) || Temp_Ai_Loc == unit.loc)
                                    {
                                        // Use items if needed
                                        unit_use_item(unit);
                                        cont = false;
                                    }
                                    else
                                    {
                                        // Tests for doors in the way
                                        if (unit.can_open_door())
                                        {
                                            Vector2? door_target = Game_AI.door_target(unit, actual_target_loc, Temp_Ai_Loc, -1);
                                            if (door_target != null && Global.game_map.distance((Vector2)door_target, Temp_Ai_Loc) != 1)
                                                door_target = null;
                                            // Doors are in the way, head toward them with intent of opening
                                            if (door_target != null)
                                            {
                                                Temp_Ai_Loc = (Vector2)door_target;
                                                unit.mission = 22;
                                                Ai_Action = Ai_Actions.Idle;
                                                Ai_Timer = 0;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                        Ai_Action = Ai_Actions.Move_In;
                                        Global.player.force_loc(unit.loc);
                                    }
                                }
                            }
                            break;
                        // Waits for scrolling to stop
                        case Ai_Actions.Move_In:
                            if (!unit.move_range.Contains(Temp_Ai_Loc))
                            {
                                throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                            }
                            else if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move_In;
                            }
                            break;
                        // Waits until units has moved
                        case Ai_Actions.Wait_For_Move_In:
                            if (unit.loc == Temp_Ai_Loc)
                                Ai_Action = Ai_Actions.Finish_Movement;
                            else
                                cont = true;
                            break;
                        // Finishes movement, waits a few ticks
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 6: Defend Area
                case 6:
                    switch (Ai_Action)
                    {
                        // Look for enemies in the defended area
                        case Ai_Actions.Idle:
                            bool enemy_found = false;
                            // If there is no defend area for this team, then short-circuit to seek mission
                            if (Ai_Defend_Area.Count == 0)
                                enemy_found = true;
                            // Should check if the unit has a siege tome or access to a siege engine or anything here //Yeti
                            else if (false)
                                enemy_found = true;
                            // Check if any enemies are in the defended area, and if so switch to seek and attack one
                            else
                                foreach (int team in unit.attackable_teams())
                                {
                                    foreach (int other_unit_id in Global.game_map.teams[team])
                                        if (Ai_Defend_Area.Contains(Units[other_unit_id].loc))
                                        {
                                            enemy_found = true;
                                            break;
                                        }
                                    if (enemy_found)
                                        break;
                                }

                            if (enemy_found)
                            {
                                Defending_Area = true;
                                unit.mission = 3;
                            }
                            else
                            {
                                defend_targets = null;
                                // If not in the defend zone, try to move back into it
                                if (!Ai_Defend_Area.Contains(unit.loc))
                                    defend_targets = Game_AI.distance_to_locations(unit, Ai_Defend_Area);
                                // If moving back in is possible
                                if (defend_targets != null && defend_targets.Count > 0)
                                {
                                    Temp_Ai_Loc = defend_targets[0].loc;
                                    Temp_Ai_Locs = new HashSet<Vector2>(Ai_Defend_Area);
                                    unit.mission = 23;
                                }
                                // Else no enemies to attack so stay out of the way
                                else
                                {
                                    Defending_Area = true;
                                    unit.mission = Game_AI.SENTRY_MISSION;
                                }
                            }
                            cont = false;
                            break;
                    }
                    break;
                #endregion

                #region 7: Staff User
                case 7:
                    switch (Ai_Action)
                    {
                        // Look for things to heal
                        case Ai_Actions.Idle:
                            unit.actor.sort_items();

                            Ai_Move_Range = update_ai_move_range(unit);
                            // Allies
                            useable_staves = new List<int>();
                            ally_target_ary = unit.allies_in_staff_range(Ai_Move_Range);
                            foreach (int i in ally_target_ary[1])
                            {
                                if (!useable_staves.Contains((int)unit.actor.weapon_index(i)))
                                    useable_staves.Add((int)unit.actor.weapon_index(i));
                            }
                            // Enemies
                            enemy_target_ary = unit.enemies_in_staff_range(Ai_Move_Range);
                            foreach (int i in enemy_target_ary[1])
                            {
                                if (!useable_staves.Contains((int)unit.actor.weapon_index(i)))
                                    useable_staves.Add((int)unit.actor.weapon_index(i));
                            }
                            // Other
                            untargeted_target_ary = unit.untargeted_staff_range(Ai_Move_Range);
                            foreach (int i in untargeted_target_ary[1])
                            {
                                if (!useable_staves.Contains((int)unit.actor.weapon_index(i)))
                                    useable_staves.Add((int)unit.actor.weapon_index(i));
                            }
                            if (useable_staves.Count > 0)
                                Ai_Action = Ai_Actions.Selecting_Target;
                            else
                                Ai_Action = Ai_Actions.Search_For_Targets;
                            cont = false;
                            break;
                        // Picks a target
                        case Ai_Actions.Selecting_Target:
                            // Healing staves
                            useable_ally_staves = new List<int>();
                            ally_target_ary = unit.allies_in_staff_range(Ai_Move_Range);
                            foreach (int i in ally_target_ary[1])
                            {
                                if (!useable_ally_staves.Contains((int)unit.actor.weapon_index(i)))
                                    useable_ally_staves.Add((int)unit.actor.weapon_index(i));
                            }
                            // Attack staves
                            useable_enemy_staves = new List<int>();
                            enemy_target_ary = unit.enemies_in_staff_range(Ai_Move_Range);
                            foreach (int i in enemy_target_ary[1])
                            {
                                if (!useable_enemy_staves.Contains((int)unit.actor.weapon_index(i)))
                                    useable_enemy_staves.Add((int)unit.actor.weapon_index(i));
                            }
                            // Untargeted staves
                            useable_staves = new List<int>();
                            untargeted_target_ary = unit.untargeted_staff_range(Ai_Move_Range);
                            foreach (int i in untargeted_target_ary[1])
                            {
                                if (!useable_staves.Contains((int)unit.actor.weapon_index(i)))
                                    useable_staves.Add((int)unit.actor.weapon_index(i));
                            }
                            // Determines target
                            Temp_Ai_Target = Game_AI.get_heal_target(
                                unit, ally_target_ary[0], useable_ally_staves, true);
                            // If everyone moderately healthy, check for enemies to afflict
                            if (Temp_Ai_Target == null)
                                Temp_Ai_Target = Game_AI.get_attack_staff_target(
                                    unit, enemy_target_ary[0], useable_enemy_staves, true);
                            // If no one to afflict, check for untargeted staves
                            if (Temp_Ai_Target == null)
                            {
                                Temp_Ai_Target = Game_AI.get_untargeted_staff_target(
                                    unit, useable_staves, true);
                                if (Temp_Ai_Target != null)
                                {
                                    Global.game_system.Staff_Target_Loc = new Vector2(
                                        Temp_Ai_Target[0] % Global.game_map.width,
                                        Temp_Ai_Target[0] / Global.game_map.width);
                                    Temp_Ai_Target[0] = -1;
                                }
                            }
                            // If nothing else to do, check if anyone is injured
                            if (Temp_Ai_Target == null)
                                Temp_Ai_Target = Game_AI.get_heal_target(
                                    unit, ally_target_ary[0], useable_ally_staves, true, false);

                            // If nothing at all to do, look for a target
                            if (Temp_Ai_Target == null)
                                Ai_Action = Ai_Actions.Search_For_Targets;
                            else
                            {
                                Ai_Action = Ai_Actions.Move_To_Target;
                            }
                            break;
                        // Moves to enemy
                        case Ai_Actions.Move_To_Target:
                            switch (Ai_Timer)
                            {
                                case 0:
                                    Ai_Timer++;
                                    // If target data included a location
                                    if (Temp_Ai_Target.Length > 3 && Temp_Ai_Target[3] != Config.OFF_MAP.X && Temp_Ai_Target[4] != Config.OFF_MAP.Y)
                                    {
                                        Temp_Ai_Loc = new Vector2(Temp_Ai_Target[3], Temp_Ai_Target[4]);
                                    }
                                    else
                                    {
                                        HashSet<Vector2> target_ary = Game_AI.move_to_hit( //Debug
                                            Units[Temp_Ai_Target[0]], Ai_Move_Range, Units[Active_Ai_Unit_Id], Temp_Ai_Target[1] - 1);
                                        //List<Vector2> target_ary = Game_AI.move_to_hit(
                                        //    Units[Ai_Target[0]], Ai_Move_Range, Units[Active_Ai_Unit_Id], 0);

                                        int target_index = (int)((Ai_Turn_Rn / 100.0f) * target_ary.Count);
                                        Temp_Ai_Loc = target_ary.ToArray()[target_index]; //HashSet
                                        // Tests for doors in the way // Not sure if this one works //Yeti
                                        if (unit.can_open_door())
                                        {
                                            Vector2? door_target = Game_AI.door_target(unit, Temp_Ai_Loc);
                                            // Doors are in the way, head toward them with intent of opening
                                            if (door_target != null)
                                            {
                                                Temp_Ai_Loc = (Vector2)door_target;
                                                unit.mission = 22;
                                                Ai_Action = Ai_Actions.Idle;
                                                Ai_Timer = 0;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                    }
                                    Global.player.force_loc(unit.loc);
                                    break;
                                default:
                                    if (!Scrolling || Skip_Ai_Turn)
                                    {
                                        Ai_Timer = 0;
                                        unit.ai_move_to(Temp_Ai_Loc);
                                        Ai_Action = Ai_Actions.Wait_For_Move;
                                    }
                                    break;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Vector2 target_loc = Temp_Ai_Target[0] == -1 ? Global.game_system.Staff_Target_Loc : Units[Temp_Ai_Target[0]].loc;
                                if (!is_off_screen(target_loc))
                                    Ai_Timer = Constants.Map.AI_WAIT_TIME;
                                if (Ai_Timer < ai_wait_time)
                                    Ai_Timer++;
                                else
                                {
                                    Ai_Timer = 0;
                                    if (!Skip_Ai_Turn)
                                    {
                                        Global.player.target_tile(target_loc);
                                        Global.player.force_loc(target_loc);
                                    }
                                    Ai_Action = Ai_Actions.Attack_Target;
                                }
                            }
                            else
                                cont = true;
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Attack_Target:
                            if (!Global.player.is_targeting())
                            {
                                unit.equip(Temp_Ai_Target[1]);
                                unit.actor.organize_items();
                                unit.using_siege_engine = Temp_Ai_Target[1] - 1 ==
                                    Siege_Engine.SIEGE_INVENTORY_INDEX;
                                // Unit movement locked in
                                unit.moved();
                                Global.game_state.call_staff(unit.id, Temp_Ai_Target[0]);
                                Ai_Action = Ai_Actions.Wait_For_Combat;
                            }
                            else
                                cont = true;
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Combat:
                            if (!Global.game_state.staff_active)
                                Ai_Phase = 3;
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Search_For_Targets:
                            // Healing~
                            bool wants_healing = ai_unit_seeks_healing(unit);
                            if (wants_healing)
                            {
                                unit.mission = Game_AI.HEALING_MISSION;
                                Ai_Action = Ai_Actions.Idle;
                                cont = false;
                                return cont;
                            }

                            Maybe<Vector2>[] search_loc = null;
                            if (unit.actor.useable_healing_staves().Count > 0)
                            {
                                // Find someone to heal, to move closer to
                                search_loc = Game_AI.search_for_ally(
                                    unit, Search_For_Ally_Modes.Looking_To_Heal);
                            }
                            // If no one to heal, look for status staff usage
                            if (search_loc == null || search_loc[1].IsNothing)
                                if (unit.actor.useable_attack_staves().Count > 0)
                                {
                                    // Move closer to enemies without entering their range
                                    search_loc = Game_AI.search_for_enemy(unit);
                                }

                            // If nothing to do, retreat
                            if (search_loc == null || search_loc[1].IsNothing)
                            {
                                unit.mission = Game_AI.SENTRY_MISSION;// 27; // For now stay still if no one to move toward and not in enemy range //Yeti
                                Ai_Action = Ai_Actions.Idle;
                                Ai_Timer = 0;
                            }
                            else
                            {
                                actual_target_loc = search_loc[0];
                                Temp_Ai_Loc = search_loc[1];
                                if (!unit.move_range.Contains(Temp_Ai_Loc))
                                {
                                    throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                                }
                                if (is_blocked(Temp_Ai_Loc, Active_Ai_Unit_Id, false) || Temp_Ai_Loc == unit.loc)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    // Tests for doors in the way
                                    if (unit.can_open_door())
                                    {
                                        Vector2? door_target = Game_AI.door_target(unit, actual_target_loc, Temp_Ai_Loc, -1);
                                        if (door_target != null && Global.game_map.distance((Vector2)door_target, Temp_Ai_Loc) != 1)
                                            door_target = null;
                                        // Doors are in the way, head toward them with intent of opening
                                        if (door_target != null)
                                        {
                                            Temp_Ai_Loc = (Vector2)door_target;
                                            unit.mission = 22;
                                            Ai_Action = Ai_Actions.Idle;
                                            Ai_Timer = 0;
                                            cont = false;
                                            return cont;
                                        }
                                    }
                                    Ai_Action = Ai_Actions.Move_In;
                                    Global.player.force_loc(unit.loc);
                                }
                            }
                            break;
                        case Ai_Actions.Move_In:
                            if (!unit.move_range.Contains(Temp_Ai_Loc))
                            {
                                throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                            }
                            else if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move_In;
                            }
                            break;
                        case Ai_Actions.Wait_For_Move_In:
                            if (unit.loc == Temp_Ai_Loc)
                                Ai_Action = Ai_Actions.Finish_Movement;
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Wait_For_Combat;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 8: Seek unit to talk
                case 8:
                    switch (Ai_Action)
                    {
                        // Looks for someone to talk to
                        case Ai_Actions.Idle:
                            Temp_Ai_Target = Game_AI.search_for_talk(unit);
                            if (Temp_Ai_Target == null)
                            {
                                if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                            }
                            else
                            {
                                Temp_Ai_Loc = new Vector2(Temp_Ai_Target[1], Temp_Ai_Target[2]);
                                Ai_Action = Ai_Actions.Move_To_Target;
                                Global.player.force_loc(unit.loc);
                            }
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                if (Temp_Ai_Target[0] == -1)
                                {
                                    unit.moved();
                                    Ai_Action = Ai_Actions.Finish_Movement;
                                }
                                else
                                {
                                    Vector2 target_loc = Units[Temp_Ai_Target[0]].loc;
                                    unit.face(target_loc);
                                    if (!is_off_screen(target_loc))
                                        Ai_Timer = Constants.Map.AI_WAIT_TIME;
                                    if (Ai_Timer < ai_wait_time)
                                        Ai_Timer++;
                                    else
                                    {
                                        Ai_Timer = 0;
                                        if (!Skip_Ai_Turn)
                                        {
                                            if (unit.visible_by() || Units[Temp_Ai_Target[0]].visible_by())
                                                Global.player.target_tile(target_loc, 20);
                                            Global.player.force_loc(target_loc);
                                        }
                                        Ai_Action = Ai_Actions.Talk_Target;
                                    }
                                }
                            }
                            else
                                cont = true;
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Talk_Target:
                            if (!Global.player.is_targeting())
                            {
                                // Unit movement locked in
                                unit.moved();

                                Global.game_state.activate_talk(unit.id, Temp_Ai_Target[0]);
                                unit.cantoing = false;
                                if (Constants.Gameplay.TALKING_IS_FREE_ACTION)
                                    unit.cantoing = true;
                                else if (unit.has_canto() && !unit.full_move())
                                    unit.cantoing = true;

                                Ai_Action = Ai_Actions.Wait_For_Talk;
                            }
                            else
                                cont = true;
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Talk:
                            if (!Global.game_state.talk_active)
                            {
                                Ai_Action = Ai_Actions.Finish_Movement;
                                Temp_Ai_Target = new int[] { 0 };
                            }
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (!unit.ready)
                                {
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                                // If didn't actually talk yet, or can do things after talking
                                else if (Constants.Gameplay.TALKING_IS_FREE_ACTION || Temp_Ai_Target == null || Temp_Ai_Target[0] == -1)
                                {
                                    // And they're still on the active team
                                    if (unit.is_active_team)
                                    {
                                        // Use items if needed
                                        unit_use_item(unit);
                                        cont = false;
                                    }
                                    else
                                    {
                                        Ai_Timer = 0;
                                        Ai_Phase = 3;
                                    }
                                }
                                else if (unit.has_canto())
                                {
                                    unit.mission = Game_AI.SENTRY_MISSION;
                                    Ai_Action = Ai_Actions.Idle;
                                    Ai_Timer = 0;
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 9: Seek tile
                case 9:
                    switch (Ai_Action)
                    {
                        // Looks for tiles to move to
                        case Ai_Actions.Idle:
                            Temp_Ai_Target = Game_AI.search_for_tile(unit);
                            if (Temp_Ai_Target == null)
                            {
                                if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                            }
                            else
                            {
                                Temp_Ai_Loc = new Vector2(Temp_Ai_Target[0], Temp_Ai_Target[1]);
                                Ai_Action = Ai_Actions.Move_To_Target;
                                Global.player.force_loc(unit.loc);
                            }
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                unit.moved();
                                Ai_Action = Ai_Actions.Finish_Movement;
                            }
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (!unit.ready)
                                {
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                                else
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 10: Do nothing
                case 10:
                    Ai_Phase = 3;
                    break;
                #endregion

                #region 13: FoW Sentry
                case 13:
                    switch (Ai_Action)
                    {
                        case Ai_Actions.Idle:
                            unit.actor.sort_items();
                            // If this unit can retreat after attacking, and has a weapon
                            if (!unit.cantoing && unit.has_attack_canto() && !(unit.actor.staff_fix() || unit.actor.weapon == null))
                            {
                                Ai_Move_Range = update_ai_move_range(unit);
                                Enemy_Target_Ary = unit.enemies_in_range(Ai_Move_Range, true);
                                // If this unit can't hit anything, and enemies can hit it, it needs to retreat even though it can fight
                                if (!Enemy_Target_Ary[0].Any() && Ai_Enemy_Attack_Range.Contains(unit.loc))
                                    unit.mission = Game_AI.RETREAT_MISSION;
                                else
                                    unit.mission = Game_AI.RETREATING_ATTACK_MISSION;
                            }
                            // Retreats if in enemy attack range
                            else if (Ai_Enemy_Attack_Range.Contains(unit.loc))
                            {
                                // If can heal self and has canto, heal now, move later
                                if (unit.has_canto() && !unit.cantoing)
                                    unit.mission = 26;
                                else
                                    unit.mission = Game_AI.RETREAT_MISSION;
                            }
                            else if (!unit.cantoing)
                            {
                                // Use items if needed
                                //unit_use_item(unit); //Debug
                                // Use a modified version for this, that tries to stay out of potential enemy range //Yeti
                                unit.mission = Game_AI.SAFE_ATTACK_MISSION;
                            }
                            // If cantoing, just retreat to do something
                            else
                            {
                                unit.mission = Game_AI.RETREAT_MISSION;
                            }
                            cont = false;
                            break;
                    }
                    break;
                #endregion

                #region 14: Savior
                case 14:
                    switch (Ai_Action)
                    {
                        // Looks for someone to cover
                        case Ai_Actions.Idle:
                            if (!unit.has_cover() || !Constants.Map.RESCUED_TERRAIN_HEAL)
                            {
                                unit.mission = Game_AI.ATTACK_IN_RANGE_MISSION;
                                cont = false;
                            }
                            else
                            {
                                if (unit.is_rescuing)
                                    Temp_Ai_Target = Game_AI.search_for_savior_drop(unit);
                                else
                                    Temp_Ai_Target = Game_AI.search_for_savior(unit);
                                if (Temp_Ai_Target == null)
                                {
                                    if (!unit.cantoing)
                                    {
                                        // If no one to rescue and not rescuing, maybe attack someone in range? //Yeti
                                        if (!unit.is_rescuing)
                                            unit.mission = Game_AI.ATTACK_IN_RANGE_MISSION;
                                        else
                                            // Use items if needed
                                            unit_use_item(unit);
                                        cont = false;
                                    }
                                    else
                                    {
                                        Ai_Timer = 0;
                                        Ai_Phase = 3;
                                        unit.start_wait();
                                    }
                                }
                                else
                                {
                                    if (unit.is_rescuing)
                                    {
                                        Temp_Ai_Loc = new Vector2(Temp_Ai_Target[0], Temp_Ai_Target[1]);
                                        Ai_Action = Ai_Actions.Move_To_Target;
                                        Global.player.force_loc(unit.loc);
                                    }
                                    else
                                    {
                                        Temp_Ai_Loc = new Vector2(Temp_Ai_Target[1], Temp_Ai_Target[2]);
                                        Ai_Action = Ai_Actions.Move_To_Target;
                                        Global.player.force_loc(unit.loc);
                                    }
                                }
                            }
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Vector2 target_loc;
                                if (unit.is_rescuing)
                                    target_loc = new Vector2(Temp_Ai_Target[2], Temp_Ai_Target[3]);
                                else
                                    target_loc = Units[Temp_Ai_Target[0]].loc;
                                unit.face(target_loc);
                                if (!is_off_screen(target_loc))
                                    Ai_Timer = Constants.Map.AI_WAIT_TIME;
                                if (Ai_Timer < ai_wait_time)
                                    Ai_Timer++;
                                else
                                {
                                    Ai_Timer = 0;
                                    if (!Skip_Ai_Turn)
                                    {
                                        if (unit.visible_by() || Units[Temp_Ai_Target[0]].visible_by())
                                            Global.player.target_tile(target_loc, 20);
                                        Global.player.force_loc(target_loc);
                                    }
                                    Ai_Action = Ai_Actions.Rescue_Target;
                                }
                            }
                            else
                                cont = true;
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Rescue_Target:
                            if (!Global.player.is_targeting())
                            {
                                // Unit movement locked in
                                unit.moved();
                                if (unit.is_rescuing)
                                    unit.drop_ally(new Vector2(Temp_Ai_Target[2], Temp_Ai_Target[3]));
                                else
                                    unit.cover_ally(Temp_Ai_Target[0]);
                                Ai_Action = Ai_Actions.Wait_For_Rescue;
                            }
                            else
                                cont = true;
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Rescue:
                            if (!Global.game_state.rescue_active)
                                Ai_Action = Ai_Actions.Finish_Movement;
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (!unit.ready)
                                {
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                                else if (unit.has_canto())
                                {
                                    unit.mission = Game_AI.SENTRY_MISSION;
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Idle;
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 15: Escape
                case 15:
                    switch (Ai_Action)
                    {
                        // Look for somewhere to escape
                        case Ai_Actions.Idle:
                            // If there's nowhere to escape, switch to attack in range
                            if (!Global.game_map.escape_point_locations(unit.team, unit.group).Any())
                            {
                                unit.mission = Game_AI.ATTACK_IN_RANGE_MISSION;

                                // This permanently changes mission instead of temporarily, is this wanted? //Debug
                                //unit.ai_mission = (unit.ai_priority) * Game_AI.MISSION_COUNT + unit.mission;
                                cont = false;
                            }
                            else
                            {
                                unit.actor.sort_items();
                                Ai_Move_Range = update_ai_move_range(unit);
                                // Gets the closest escape point
                                HashSet<Vector2> escape_locations = Global.game_map.escape_point_locations(unit.team, unit.group);
                                escape_targets = Game_AI.distance_to_locations(unit, escape_locations);

                                // If there are targets to move to
                                if (escape_targets.Any())
                                {
                                    target_sort(escape_targets);
                                    Temp_Ai_Loc = escape_targets[0].loc;
                                    // If we can't get to the escape point this turn, switch to the mission that handles moving closer
                                    if (escape_targets[0].dist > unit.mov)
                                    {
                                        Temp_Ai_Locs = new HashSet<Vector2>(escape_targets.Select(x => x.loc));
                                        unit.mission = 23;
                                        Ai_Action = Ai_Actions.Idle;
                                    }
                                    else
                                    {
                                        Temp_Ai_Locs = new HashSet<Vector2>(escape_targets.Select(x => x.loc));
                                        bool assist_others = false;
                                        // Check if other units are retreating and whether to assist them instead of leaving now
                                        if (Units.Values.Where(other_unit => other_unit != unit &&
                                            other_unit.ai_mission == 15).Any())
                                        {
                                            List<int> enemies_en_route = Game_AI.get_enemies_toward_target(
                                                unit, Temp_Ai_Locs, unit.get_attackable_units());
                                            enemy_target_ary = unit.enemies_in_range(Ai_Move_Range, true);
                                            if (enemies_en_route.Intersect(enemy_target_ary[0]).Any())
                                                assist_others = true;
                                        }

                                        if (assist_others)
                                        {
                                            unit.mission = 23;
                                            Ai_Action = Ai_Actions.Idle;
                                        }
                                        else
                                        {
                                            Temp_Ai_Locs.Clear();
                                            // Tests for doors in the way // Not sure if this one works //Yeti
                                            if (unit.can_open_door())
                                            {
                                                Vector2? door_target = Game_AI.door_target(unit, Temp_Ai_Loc);
                                                // Doors are in the way, head toward them with intent of opening
                                                if (door_target != null)
                                                {
                                                    Temp_Ai_Loc = (Vector2)door_target;
                                                    unit.mission = 22;
                                                    Ai_Action = Ai_Actions.Idle;
                                                    Ai_Timer = 0;
                                                    cont = false;
                                                    return cont;
                                                }
                                            }
                                            cont = Skip_Ai_Turn;
                                            switch_out_of_ai_skip();
                                            Ai_Action = Ai_Actions.Move_To_Target;
                                            Global.player.force_loc(unit.loc);
                                        }
                                    }
                                }
                                else
                                {
                                    unit.mission = 3;
                                    Ai_Action = Ai_Actions.Idle;
                                    cont = false;
                                }
                            }
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until escapee has moved
                        case Ai_Actions.Wait_For_Move:
                            if (unit.loc == Temp_Ai_Loc)
                                Ai_Action = Ai_Actions.Move_Wait;
                            else
                                cont = true;
                            break;
                        // Finishes movement, waits a few ticks
                        case Ai_Actions.Move_Wait:
                            switch (Ai_Timer)
                            {
                                case 11:
                                    Ai_Timer = 0;
                                    unit.evented_move_to(Global.game_map.escape_point_data(unit, Temp_Ai_Loc).EscapeToLoc);
                                    Ai_Action = Ai_Actions.Wait_For_Move_Escape;
                                    break;
                                default:
                                    Ai_Timer++;
                                    break;
                            }
                            break;
                        // Waits until escapee has moved off map
                        case Ai_Actions.Wait_For_Move_Escape:
                            if (unit.loc == Global.game_map.escape_point_data(unit, Temp_Ai_Loc).EscapeToLoc)
                                Ai_Action = Ai_Actions.Move_Wait_Escape;
                            else
                                cont = true;
                            break;
                        // Finishes movement off map, waits a few ticks
                        case Ai_Actions.Move_Wait_Escape:
                            switch (Ai_Timer)
                            {
                                case 19:
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Escape;
                                    break;
                                default:
                                    Ai_Timer++;
                                    break;
                            }
                            break;
                        // Opens chest
                        case Ai_Actions.Escape:
                            // Unit movement locked in
                            unit.escape();

                            if (unit.is_rescuing)
                                Global.game_map.remove_unit(unit.rescuing);
                            Global.game_map.remove_unit(unit.id);
                            Ai_Phase = 3;
                            break;
                    }
                    break;
                #endregion

                #region 16: Dancer
                case 16:
                    switch (Ai_Action)
                    {
                        // Look for things to heal
                        case Ai_Actions.Idle:
                            // If this unit can't actually dance
                            if (!unit.can_dance())
                            {
                                unit.mission = 3;
                                cont = false;
                            }
                            else
                            {
                                Ai_Move_Range = update_ai_move_range(unit);
                                dance_targets = new List<int>();
                                foreach (int id in unit.check_range(1, 1, Ai_Move_Range, attackable: false))
                                    if (Units[id].same_team(unit) && ai_unit_ready_to_act(Units[id])
                                            && !Units[id].ready && Units[id].actor.weapon != null)
                                        dance_targets.Add(id);
                                // There is a target to dance for
                                if (dance_targets.Count > 0)
                                    Ai_Action = Ai_Actions.Selecting_Target;
                                else
                                    Ai_Action = Ai_Actions.Search_For_Targets;
                                cont = false;
                            }
                            break;
                        // Picks a target
                        case Ai_Actions.Selecting_Target:
                            dance_targets = new List<int>();
                            foreach (int id in unit.check_range(1, 1, Ai_Move_Range, attackable: false))
                                if (Units[id].same_team(unit) && ai_unit_ready_to_act(Units[id]) &&
                                        !Units[id].ready && Units[id].actor.weapon != null)
                                    dance_targets.Add(id);
                            if (dance_targets.Count > 0)
                            {
                                Temp_Ai_Target = new int[] { Game_AI.get_dance_target(unit, dance_targets, Ai_Move_Range) };
                                if (Temp_Ai_Target != null)
                                    Ai_Action = Ai_Actions.Move_To_Target;
                                else
                                    Ai_Action = Ai_Actions.Search_For_Targets;
                            }
                            break;
                        // Moves to enemy
                        case Ai_Actions.Move_To_Target:

                            switch (Ai_Timer)
                            {
                                case 0:
                                    Ai_Timer++;
                                    HashSet<Vector2> target_ary = Game_AI.move_to_one_range(
                                        Units[Temp_Ai_Target[0]], Ai_Move_Range, Units[Active_Ai_Unit_Id]);
                                    int target_index = (int)((Ai_Turn_Rn / 100.0f) * target_ary.Count);
                                    Temp_Ai_Loc = target_ary.ToArray()[target_index]; //HashSet
                                    // Tests for doors in the way // Not sure if this one works //Yeti
                                    if (unit.can_open_door())
                                    {
                                        Vector2? door_target = Game_AI.door_target(unit, Temp_Ai_Loc);
                                        // Doors are in the way, head toward them with intent of opening
                                        if (door_target != null)
                                        {
                                            Temp_Ai_Loc = (Vector2)door_target;
                                            unit.mission = 22;
                                            Ai_Action = Ai_Actions.Idle;
                                            Ai_Timer = 0;
                                            cont = false;
                                            return cont;
                                        }
                                    }
                                    Global.player.force_loc(unit.loc);
                                    break;
                                default:
                                    if (!Scrolling || Skip_Ai_Turn)
                                    {
                                        Ai_Timer = 0;
                                        unit.ai_move_to(Temp_Ai_Loc);
                                        Ai_Action = Ai_Actions.Wait_For_Move;
                                    }
                                    break;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Vector2 target_loc = Units[Temp_Ai_Target[0]].loc;
                                if (!is_off_screen(target_loc))
                                    Ai_Timer = Constants.Map.AI_WAIT_TIME;
                                if (Ai_Timer < ai_wait_time)
                                    Ai_Timer++;
                                else
                                {
                                    Ai_Timer = 0;
                                    if (!Skip_Ai_Turn)
                                    {
                                        Global.player.target_tile(target_loc);
                                        Global.player.force_loc(target_loc);
                                    }
                                    Ai_Action = Ai_Actions.Attack_Target;
                                }
                            }
                            else
                                cont = true;
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Attack_Target:
                            if (!Global.player.is_targeting())
                            {
                                // Unit movement locked in
                                unit.moved();
                                Game_Unit target = Units[Temp_Ai_Target[0]];
                                Global.game_state.call_dance(unit.id, Temp_Ai_Target[0], -1); // id -1 for now //Debug
                                Global.player.force_loc(target.loc);
                                Ai_Action = Ai_Actions.Wait_For_Dance;
                            }
                            else
                                cont = true;
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Dance:
                            if (!Global.game_state.dance_active)
                                Ai_Phase = 3;
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Search_For_Targets:
                            // Healing~
                            bool wants_healing = ai_unit_seeks_healing(unit);
                            if (wants_healing)
                            {
                                unit.mission = Game_AI.HEALING_MISSION;
                                Ai_Action = Ai_Actions.Idle;
                                cont = false;
                                return cont;
                            }

                            Maybe<Vector2>[] search_loc;
                            // Find someone to heal, to move closer to
                            search_loc = Game_AI.search_for_ally(unit, Search_For_Ally_Modes.Looking_To_Dance);
                            // If no one to dance for, retreat
                            if (search_loc[1].IsNothing)
                            {
                                unit.mission = Game_AI.SENTRY_MISSION;// 27; // For now stay still if no one to move toward and not in enemy range //Yeti
                                Ai_Action = Ai_Actions.Idle;
                                Ai_Timer = 0;
                            }
                            else
                            {
                                actual_target_loc = search_loc[0];
                                Temp_Ai_Loc = search_loc[1];
                                if (!unit.move_range.Contains(Temp_Ai_Loc))
                                {
                                    throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                                }
                                if (is_blocked(Temp_Ai_Loc, Active_Ai_Unit_Id, false) || Temp_Ai_Loc == unit.loc)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    // Tests for doors in the way
                                    if (unit.can_open_door())
                                    {
                                        Vector2? door_target = Game_AI.door_target(unit, actual_target_loc, Temp_Ai_Loc, -1);
                                        if (door_target != null && Global.game_map.distance((Vector2)door_target, Temp_Ai_Loc) != 1)
                                            door_target = null;
                                        // Doors are in the way, head toward them with intent of opening
                                        if (door_target != null)
                                        {
                                            Temp_Ai_Loc = (Vector2)door_target;
                                            unit.mission = 22;
                                            Ai_Action = Ai_Actions.Idle;
                                            Ai_Timer = 0;
                                            cont = false;
                                            return cont;
                                        }
                                    }
                                    Ai_Action = Ai_Actions.Move_In;
                                    Global.player.force_loc(unit.loc);
                                }
                            }
                            break;
                        case Ai_Actions.Move_In:
                            if (!unit.move_range.Contains(Temp_Ai_Loc))
                            {
                                throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                            }
                            else if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move_In;
                            }
                            break;
                        case Ai_Actions.Wait_For_Move_In:
                            if (unit.loc == Temp_Ai_Loc)
                                Ai_Action = Ai_Actions.Finish_Movement;
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Wait_For_Combat;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                // Sub-missions (called from other missions) //
                #region 21: Thief escape
                case 21:
                    switch (Ai_Action)
                    {
                        // Look for somewhere to escape
                        case Ai_Actions.Idle:
                            // No place to escape, switch to stealing? (FEXP used attack weakest, and permanently instead of temporarily) //Yeti
                            if (Global.game_map.thief_escape_points.Count == 0)
                            {
                                unit.mission = 24;
                                cont = false;
                            }
                            else
                            {
                                unit.actor.sort_items();
                                Ai_Move_Range = update_ai_move_range(unit);
                                HashSet<Vector2> target_array = new HashSet<Vector2>();
                                foreach (Vector2 loc in Global.game_map.thief_escape_points.Keys)
                                    target_array.Add(loc);
                                escape_targets = Game_AI.distance_to_locations(unit, target_array);

                                if (escape_targets.Count > 0)
                                {
                                    target_sort(escape_targets);
                                    Temp_Ai_Loc = escape_targets[0].loc;
                                    if (escape_targets[0].dist > unit.mov)
                                    {
                                        Ai_Action = Ai_Actions.Search_For_Targets;
                                        cont = false;
                                    }
                                    else
                                    {
                                        // Tests for doors in the way // Not sure if this one works //Yeti
                                        if (unit.can_open_door())
                                        {
                                            Vector2? door_target = Game_AI.door_target(unit, Temp_Ai_Loc);
                                            // Doors are in the way, head toward them with intent of opening
                                            if (door_target != null)
                                            {
                                                Temp_Ai_Loc = (Vector2)door_target;
                                                unit.mission = 22;
                                                Ai_Action = Ai_Actions.Idle;
                                                Ai_Timer = 0;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                        cont = Skip_Ai_Turn;
                                        switch_out_of_ai_skip();
                                        Ai_Action = Ai_Actions.Move_To_Target;
                                        Global.player.force_loc(unit.loc);
                                    }
                                }
                                // Escape point is blocked off, ruin somebody's day
                                else
                                {
                                    unit.mission = 24;
                                    Ai_Action = Ai_Actions.Idle;
                                    cont = false;
                                }
                            }
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until escapee has moved
                        case Ai_Actions.Wait_For_Move:
                            if (unit.loc == Temp_Ai_Loc)
                                Ai_Action = Ai_Actions.Move_Wait;
                            else
                                cont = true;
                            break;
                        // Finishes movement, waits a few ticks
                        case Ai_Actions.Move_Wait:
                            switch (Ai_Timer)
                            {
                                case 11:
                                    Ai_Timer = 0;
                                    unit.evented_move_to(Global.game_map.thief_escape_points[Temp_Ai_Loc]);
                                    Ai_Action = Ai_Actions.Wait_For_Move_Escape;
                                    break;
                                default:
                                    Ai_Timer++;
                                    break;
                            }
                            break;
                        // Waits until escapee has moved off map
                        case Ai_Actions.Wait_For_Move_Escape:
                            if (unit.loc == Global.game_map.thief_escape_points[Temp_Ai_Loc])
                                Ai_Action = Ai_Actions.Move_Wait_Escape;
                            else
                                cont = true;
                            break;
                        // Finishes movement off map, waits a few ticks
                        case Ai_Actions.Move_Wait_Escape:
                            switch (Ai_Timer)
                            {
                                case 19:
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Escape;
                                    break;
                                default:
                                    Ai_Timer++;
                                    break;
                            }
                            break;
                        // Opens chest
                        case Ai_Actions.Escape:
                            // Unit movement locked in
                            unit.escape();

                            if (unit.is_rescuing)
                                Global.game_map.remove_unit(unit.rescuing);
                            Global.game_map.remove_unit(unit.id);
                            Ai_Phase = 3;
                            break;
                        // Moves toward target
                        case Ai_Actions.Search_For_Targets:
                            actual_target_loc = Temp_Ai_Loc;
                            Maybe<Vector2> search_loc = Game_AI.path_to_target(unit, Temp_Ai_Loc);
                            if (search_loc.IsNothing)
                            {
                                // Use items if needed
                                unit_use_item(unit);
                                cont = false;
                            }
                            else
                            {
                                Temp_Ai_Loc = search_loc;
                                if (is_blocked(Temp_Ai_Loc, Active_Ai_Unit_Id, false))
                                {
                                    // isn't this an infinite loop //Yeti
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Idle;
                                    unit.mission = 21;
                                    cont = false;
                                }
                                else
                                {
                                    if (!unit.move_range.Contains(Temp_Ai_Loc))
                                    {
                                        throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                                    }
                                    if (is_blocked(Temp_Ai_Loc, Active_Ai_Unit_Id, false) || Temp_Ai_Loc == unit.loc)
                                    {
                                        // Use items if needed
                                        unit_use_item(unit);
                                        cont = false;
                                    }
                                    else
                                    {
                                        // Tests for doors in the way
                                        if (unit.can_open_door())
                                        {
                                            Vector2? door_target = Game_AI.door_target(unit, actual_target_loc, Temp_Ai_Loc, -1);
                                            if (door_target != null && Global.game_map.distance((Vector2)door_target, Temp_Ai_Loc) != 1)
                                                door_target = null;
                                            // Doors are in the way, head toward them with intent of opening
                                            if (door_target != null)
                                            {
                                                Temp_Ai_Loc = (Vector2)door_target;
                                                unit.mission = 22;
                                                Ai_Action = Ai_Actions.Idle;
                                                Ai_Timer = 0;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                        Ai_Action = Ai_Actions.Move_In;
                                        Global.player.force_loc(unit.loc);
                                    }
                                }
                            }
                            break;
                        // Waits for scrolling to stop
                        case Ai_Actions.Move_In:
                            if (!unit.move_range.Contains(Temp_Ai_Loc))
                            {
                                throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                            }
                            else if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move_In;
                            }
                            break;
                        // Waits until units has moved
                        case Ai_Actions.Wait_For_Move_In:
                            if (unit.loc == Temp_Ai_Loc)
                                Ai_Action = Ai_Actions.Finish_Movement;
                            else
                                cont = true;
                            break;
                        // Finishes movement, waits a few ticks
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 22: Door open
                case 22:
                    switch (Ai_Action)
                    {
                        // Look for chests
                        case Ai_Actions.Idle:
                            foreach(Vector2 offset in new Vector2[] {
                                    new Vector2(0, -1), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(1, 0) })
                                if (Ai_Move_Range.Contains(Temp_Ai_Loc + offset))
                                {
                                    Global.game_system.Visit_Loc = Temp_Ai_Loc;
                                    Temp_Ai_Loc = Temp_Ai_Loc + offset;
                                    Ai_Action = Ai_Actions.Move_To_Target;
                                    cont = Skip_Ai_Turn;
                                    switch_out_of_ai_skip();
                                    Global.player.force_loc(unit.loc);
                                    return cont;
                                }
                            // Couldn't get to the door, search for an open tile instead // This should never happen
                            Ai_Action = Ai_Actions.Search_For_Targets;
                            cont = false;
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until door opener has moved
                        case Ai_Actions.Wait_For_Move:
                            if (unit.loc == Temp_Ai_Loc && unit.is_on_square)
                            {
                                if (unit.cantoing)
                                {
                                    unit.moved();
                                    Ai_Action = Ai_Actions.Wait_For_Visit;
                                }
                                else
                                {
                                    unit.face(Global.game_system.Visit_Loc);
                                    Global.player.target_tile(Global.game_system.Visit_Loc, 24);
                                    Ai_Action = Ai_Actions.Open_Target;
                                }
                            }
                            else
                                cont = true;
                            break;
                        // Opens door
                        case Ai_Actions.Open_Target:
                            if (!Global.player.is_targeting())
                            {
                                // Unit movement locked in
                                unit.moved();
                                Global.game_state.call_visit(Visit_Modes.Door, unit.id, Global.game_system.Visit_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Visit;
                            }
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Visit:
                            if (!Global.game_state.visit_active)
                                Ai_Phase = 3;
                            break;
                    }
                    break;
                #endregion

                #region 23: Move to target - moving closer/attacking
                case 23:
                    switch (Ai_Action)
                    {
                        // Look for things to hit
                        case Ai_Actions.Idle:
                            unit.actor.sort_items();
                            // If the unit can't fight, retreat instead of doing things // This should probably be removed/changed! //Debug
                            if (false) //unit.actor.staff_fix() || unit.actor.weapon == null) //Debug
                                unit.mission = Game_AI.SENTRY_MISSION;
                            else
                            {
                                cont = false;
                                Ai_Move_Range = update_ai_move_range(unit);
                                // If the unit is a dancer
                                if (unit.can_dance())
                                {
                                    dance_targets = new List<int>();
                                    foreach (int id in unit.check_range(1, 1, Ai_Move_Range, attackable: false))
                                        if (Units[id].same_team(unit) && ai_unit_ready_to_act(Units[id])
                                                && !Units[id].ready && Units[id].actor.weapon != null)
                                            dance_targets.Add(id);
                                    if (dance_targets.Count > 0)
                                    {
                                        unit.mission = 16;
                                        return cont;
                                    }
                                }

                                // If the unit can fight
                                if (!(unit.actor.staff_fix() || unit.actor.weapon == null))
                                {
                                    // Get all enemies that are closer/etc to the target location than this unit
                                    List<int> enemies_en_route = Game_AI.get_enemies_toward_target(
                                        unit, Temp_Ai_Locs, unit.get_attackable_units(), limited_access: Limited_Access_En_Route);
                                    if (!enemies_en_route.Any())
                                        Ai_Action = Ai_Actions.Search_For_Targets;
                                    else
                                    {
                                        useable_weapons = new List<int>();
                                        enemy_target_ary = unit.enemies_in_range(Ai_Move_Range, true);
                                        // Removes enemies to ignore
                                        if (!unit.is_player_allied)
                                        {
                                            unit_index = 0;
                                            while (unit_index < enemy_target_ary[0].Count)
                                            {
                                                if (!Units[enemy_target_ary[0][unit_index]].is_ally)
                                                    enemy_target_ary[0].RemoveAt(unit_index);
                                                else
                                                    unit_index++;
                                            }
                                        }
                                        if (enemy_target_ary[0].Any())
                                        {
                                            foreach (int weapon_id in enemy_target_ary[1])
                                            {
                                                int? weapon_index = unit.actor.weapon_index(weapon_id);
                                                if (weapon_index == null)
                                                    weapon_index = Constants.Actor.NUM_ITEMS;
                                                if (!useable_weapons.Contains((int)weapon_index))
                                                    useable_weapons.Add((int)weapon_index);
                                            }
                                            if (useable_weapons.Count > 0)
                                                Ai_Action = Ai_Actions.Selecting_Target;
                                            else
                                                Ai_Action = Ai_Actions.Search_For_Targets;
                                        }
                                        else
                                            Ai_Action = Ai_Actions.Search_For_Targets;
                                    }
                                }
                                else
                                    Ai_Action = Ai_Actions.Search_For_Targets;
                            }
                            break;
                        // Picks/attacks a target
                        case Ai_Actions.Selecting_Target:
                            useable_weapons = new List<int>();
                            enemy_target_ary = unit.enemies_in_range(Ai_Move_Range, true);
                            // Removes enemies to ignore
                            // If the unit can attack the player team, only attack player units? what? //Debug
                            // Oh because only player units can enter villages, not citizens
                            // And this mission is used by pillaging brigands, so they want to stop the player
                            // But green units should still be attacked, sometimes...? //Yeti
                            if (!unit.is_player_allied)
                            {
                                unit_index = 0;
                                while (unit_index < enemy_target_ary[0].Count)
                                {
                                    if (!Units[enemy_target_ary[0][unit_index]].is_ally)
                                        enemy_target_ary[0].RemoveAt(unit_index);
                                    else
                                        unit_index++;
                                }
                            }
                            if (enemy_target_ary[0].Count == 0)
                                Ai_Action = Ai_Actions.Search_For_Targets;
                            else
                            {
                                foreach (int weapon_id in enemy_target_ary[1])
                                {
                                    int? weapon_index = unit.actor.weapon_index(weapon_id);
                                    if (weapon_index == null)
                                        weapon_index = Constants.Actor.NUM_ITEMS;
                                    if (!useable_weapons.Contains((int)weapon_index))
                                        useable_weapons.Add((int)weapon_index);
                                }
                                // Determines target
                                List<int> enemies_en_route = Game_AI.get_enemies_toward_target(
                                    unit, Temp_Ai_Locs, unit.get_attackable_units(), limited_access: Limited_Access_En_Route)
                                    .Intersect(enemy_target_ary[0]).ToList();

                                Temp_Ai_Target = Game_AI.get_en_route_atk_target(unit, enemies_en_route, useable_weapons, true);
                                if (Temp_Ai_Target == null)
                                    Ai_Action = Ai_Actions.Search_For_Targets;
                                else
                                {
                                    Ai_Action = Ai_Actions.Move_To_Target;
                                }
                            }
                            cont = false;
                            break;
                        // Moves to enemy
                        case Ai_Actions.Move_To_Target:
                            switch (Ai_Timer)
                            {
                                case 0:
                                    Ai_Timer++;
                                    // If target data included a location
                                    if (Temp_Ai_Target[3] != Config.OFF_MAP.X && Temp_Ai_Target[4] != Config.OFF_MAP.Y)
                                    {
                                        Temp_Ai_Loc = new Vector2(Temp_Ai_Target[3], Temp_Ai_Target[4]);
                                    }
                                    else
                                    {
                                        HashSet<Vector2> target_ary = Game_AI.move_to_hit(
                                            Units[Temp_Ai_Target[0]], Ai_Move_Range, Units[Active_Ai_Unit_Id], 0);

                                        int target_index = (int)((Ai_Turn_Rn / 100.0f) * target_ary.Count);
                                        Temp_Ai_Loc = target_ary.ToArray()[target_index]; //HashSet
                                        // Tests for doors in the way // Not sure if this one works //Yeti
                                        if (unit.can_open_door())
                                        {
                                            Vector2? door_target = Game_AI.door_target(unit, Temp_Ai_Loc);
                                            // Doors are in the way, head toward them with intent of opening
                                            if (door_target != null)
                                            {
                                                Temp_Ai_Loc = (Vector2)door_target;
                                                unit.mission = 22;
                                                Ai_Action = Ai_Actions.Idle;
                                                Ai_Timer = 0;
                                                cont = false;
                                                return cont;
                                            }
                                        }
                                    }
                                    Global.player.force_loc(unit.loc);
                                    break;
                                default:
                                    if (!Scrolling || Skip_Ai_Turn)
                                    {
                                        Ai_Timer = 0;
                                        unit.ai_move_to(Temp_Ai_Loc);
                                        Ai_Action = Ai_Actions.Wait_For_Move;
                                    }
                                    break;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Vector2 target_loc = Units[Temp_Ai_Target[0]].loc;
                                if (!is_off_screen(target_loc))
                                    Ai_Timer = Constants.Map.AI_WAIT_TIME;
                                if (Ai_Timer < ai_wait_time)
                                    Ai_Timer++;
                                else
                                {
                                    Ai_Timer = 0;
                                    if (!Skip_Ai_Turn)
                                    {
                                        Global.player.target_tile(target_loc);
                                        Global.player.force_loc(target_loc);
                                    }
                                    Ai_Action = Ai_Actions.Attack_Target;
                                }
                            }
                            else
                                cont = true;
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Attack_Target:
                            cont |= attack_target(unit);
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Combat:
                            if (!Global.game_state.combat_active)
                                Ai_Phase = 3;
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Search_For_Targets:
                            actual_target_loc = Temp_Ai_Loc;
                            Maybe<Vector2> search_loc = Game_AI.path_to_target(unit, Temp_Ai_Loc, ignore_blocking: true);
                            if (search_loc.IsNothing)
                            {
                                // Use items if needed
                                unit_use_item(unit);
                                cont = false;
                            }
                            else
                            {
                                Temp_Ai_Loc = search_loc;
                                if (!unit.move_range.Contains(Temp_Ai_Loc))
                                {
                                    throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                                }
                                if (is_blocked(Temp_Ai_Loc, Active_Ai_Unit_Id, false) || Temp_Ai_Loc == unit.loc)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    // Tests for doors in the way
                                    if (unit.can_open_door())
                                    {
                                        Vector2? door_target = Game_AI.door_target(unit, actual_target_loc, Temp_Ai_Loc, -1);
                                        if (door_target != null && Global.game_map.distance((Vector2)door_target, Temp_Ai_Loc) != 1)
                                            door_target = null;
                                        // Doors are in the way, head toward them with intent of opening
                                        if (door_target != null)
                                        {
                                            Temp_Ai_Loc = (Vector2)door_target;
                                            unit.mission = 22;
                                            Ai_Action = Ai_Actions.Idle;
                                            Ai_Timer = 0;
                                            cont = false;
                                            return cont;
                                        }
                                    }
                                    Ai_Action = Ai_Actions.Move_In;
                                    Global.player.force_loc(unit.loc);
                                }
                            }
                            break;
                        case Ai_Actions.Move_In:
                            if (!unit.move_range.Contains(Temp_Ai_Loc))
                            {
                                throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                            }
                            else if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move_In;
                            }
                            break;
                        case Ai_Actions.Wait_For_Move_In:
                            if (unit.loc == Temp_Ai_Loc)
                                Ai_Action = Ai_Actions.Finish_Movement;
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Wait_For_Combat;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 24: Thief steal
                case 24:
                    switch (Ai_Action)
                    {
                        // Look for somewhere to steal from
                        case Ai_Actions.Idle:
                            // Inventory full/shouldn't be in this mission
                            if (unit.actor.is_full_items || !unit.can_steal())
                            {
                                unit.mission = 3;
                                cont = false;
                            }
                            else
                            {
                                Ai_Move_Range = update_ai_move_range(unit);
                                steal_targets = new List<int>();
                                foreach (int id in unit.check_range(1, 1, Ai_Move_Range))
                                    if (unit.can_steal_from(Units[id]))
                                        steal_targets.Add(id);
                                // There is a target to steal from
                                if (steal_targets.Count > 0)
                                {
                                    cont = false;
                                    Ai_Action = Ai_Actions.Selecting_Target;
                                }
                                // No one to steal from, stab them instead
                                else
                                    unit.mission = 3;
                            }
                            break;
                        // Picks/attacks a target
                        case Ai_Actions.Selecting_Target:
                            steal_targets = new List<int>();
                            foreach (int id in unit.check_range(1, 1, Ai_Move_Range))
                                if (unit.can_steal_from(Units[id]))
                                    steal_targets.Add(id);
                            Temp_Ai_Target = Game_AI.get_steal_target(unit, steal_targets, Ai_Move_Range);
                            if (Temp_Ai_Target != null)
                                Ai_Action = Ai_Actions.Move_To_Target;
                            else
                            {
                                unit.mission = 3;
                                Ai_Action = Ai_Actions.Idle;
                            }
                            break;
                        // Moves to enemy
                        case Ai_Actions.Move_To_Target:
                            switch (Ai_Timer)
                            {
                                case 0:
                                    Ai_Timer++;
                                    HashSet<Vector2> target_ary = Game_AI.move_to_one_range(
                                        Units[Temp_Ai_Target[0]], Ai_Move_Range, Units[Active_Ai_Unit_Id]);
                                    int target_index = (int)((Ai_Turn_Rn / 100.0f) * target_ary.Count);
                                    Temp_Ai_Loc = target_ary.ToArray()[target_index]; //HashSet
                                    // Tests for doors in the way // Not sure if this one works //Yeti
                                    if (unit.can_open_door())
                                    {
                                        Vector2? door_target = Game_AI.door_target(unit, Temp_Ai_Loc);
                                        // Doors are in the way, head toward them with intent of opening
                                        if (door_target != null)
                                        {
                                            Temp_Ai_Loc = (Vector2)door_target;
                                            unit.mission = 22;
                                            Ai_Action = Ai_Actions.Idle;
                                            Ai_Timer = 0;
                                            cont = false;
                                            return cont;
                                        }
                                    }
                                    Global.player.force_loc(unit.loc);
                                    break;
                                default:
                                    if (!Scrolling || Skip_Ai_Turn)
                                    {
                                        Ai_Timer = 0;
                                        unit.ai_move_to(Temp_Ai_Loc);
                                        Ai_Action = Ai_Actions.Wait_For_Move;
                                    }
                                    break;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Vector2 target_loc = Units[Temp_Ai_Target[0]].loc;
                                if (!is_off_screen(target_loc))
                                    Ai_Timer = Constants.Map.AI_WAIT_TIME;
                                if (Ai_Timer < ai_wait_time)
                                    Ai_Timer++;
                                else
                                {
                                    Ai_Timer = 0;
                                    if (!Skip_Ai_Turn)
                                    {
                                        Global.player.target_tile(target_loc);
                                        Global.player.force_loc(target_loc);
                                    }
                                    Ai_Action = Ai_Actions.Attack_Target;
                                }
                            }
                            else
                                cont = true;
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Attack_Target:
                            if (!Global.player.is_targeting())
                            {
                                // Unit movement locked in
                                unit.moved();
                                Game_Unit target = Units[Temp_Ai_Target[0]];
                                Global.game_state.call_steal(unit.id, Temp_Ai_Target[0], Temp_Ai_Target[1]);
                                Global.player.force_loc(target.loc);
                                Ai_Action = Ai_Actions.Wait_For_Steal;
                            }
                            else
                                cont = true;
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Steal:
                            if (!Global.game_state.steal_active)
                                Ai_Phase = 3;
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 25/33: Find healing
                case 25:
                case 33:
                    switch (Ai_Action)
                    {
                        // Moves to ally
                        case Ai_Actions.Idle:
                            Maybe<Vector2>[] search_loc;
                            if (unit.mission == Game_AI.ATTACK_IN_RANGE_HEAL_SELF)
                            {
                                if (unit.can_heal_self())
                                    search_loc = new Maybe<Vector2>[] { default(Maybe<Vector2>), default(Maybe<Vector2>) };
                                else
                                    search_loc = Game_AI.search_for_ally(unit, Search_For_Ally_Modes.Attack_In_Range_Healing_Item);
                            }
                            else
                                search_loc = Game_AI.search_for_healing(unit);
                            if (search_loc[1].IsNothing)
                            {
                                if (unit.mission == Game_AI.ATTACK_IN_RANGE_HEAL_SELF)
                                    unit_use_item(unit);
                                else
                                {
                                    if (unit.can_heal_self())
                                        unit.mission = Game_AI.SENTRY_MISSION;
                                    else
                                        unit.mission = unit.ai_mission;
                                    Ai_Action = Ai_Actions.Idle;
                                    Ai_Timer = 0;
                                }
                            }
                            else
                            {
                                actual_target_loc = search_loc[0];
                                Temp_Ai_Loc = search_loc[1];
                                if (!unit.move_range.Contains(Temp_Ai_Loc))
                                {
                                    throw new IndexOutOfRangeException("whoops ai unit can't move where it's trying to");
                                }
                                if (is_blocked(Temp_Ai_Loc, Active_Ai_Unit_Id, false) || Temp_Ai_Loc == unit.loc)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    // Tests for doors in the way
                                    if (unit.can_open_door())
                                    {
                                        Vector2? door_target = Game_AI.door_target(unit, actual_target_loc, Temp_Ai_Loc, -1);
                                        if (door_target != null && Global.game_map.distance((Vector2)door_target, Temp_Ai_Loc) != 1)
                                            door_target = null;
                                        // Doors are in the way, head toward them with intent of opening
                                        if (door_target != null)
                                        {
                                            Temp_Ai_Loc = (Vector2)door_target;
                                            unit.mission = 22;
                                            Ai_Action = Ai_Actions.Idle;
                                            Ai_Timer = 0;
                                            cont = false;
                                            return cont;
                                        }
                                    }
                                    Ai_Action = Ai_Actions.Move_To_Target;
                                    Global.player.force_loc(unit.loc);
                                }
                            }
                            cont = false;
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Ai_Action = Ai_Actions.Finish_Movement;
                            }
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (unit.actor.has_critical_health() && unit.terrain_heals())
                                    unit.ai_terrain_healing = true;
                                if (unit.can_heal_self() && !unit.cantoing)
                                {
                                    unit.mission = 26;
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Idle;
                                    cont = false;
                                }
                                else if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 26: Use healing item
                case 26:
                    switch (Ai_Action)
                    {
                        // Determine and use item
                        case Ai_Actions.Idle:
                            if (get_scene_map() == null)
                                return cont;

                            if (!get_scene_map().is_map_popup_active() && (!Scrolling || Skip_Ai_Turn))
                            {
                                if (Ai_Timer > 0)
                                    Ai_Timer--;
                                else
                                {
                                    if (unit.actor.has_critical_health() && unit.can_heal_self())
                                    {
                                        Ai_Action = Ai_Actions.Move_To_Target;
                                        Temp_Ai_Loc = unit.loc;
                                        Global.player.force_loc(unit.loc);
                                    }
                                    else
                                    {
                                        // Use items if needed
                                        unit_use_item(unit);
                                        cont = false;
                                    }
                                }
                            }
                            break;

                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                int item = unit.heal_self_item();
                                // Unit movement locked in
                                unit.moved();
                                Global.game_state.call_item(unit.id, item);
                                Ai_Action = Ai_Actions.Wait_For_Item;
                            }
                            else
                                cont = true;
                            break;

                        // Goes to next unit
                        case Ai_Actions.Wait_For_Item:
                            if (!Global.game_state.item_active)
                                Ai_Phase = 3;
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 27: Retreat
                case 27:
                    switch (Ai_Action)
                    {
                        // Moves to ally
                        case Ai_Actions.Idle:
                        case Ai_Actions.Full_Retreat:
                            Maybe<Vector2> retreat_loc = Game_AI.retreat(unit, full_retreat: Ai_Action == Ai_Actions.Full_Retreat);
                            if (retreat_loc.IsNothing || retreat_loc == unit.loc)
                            {
                                if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                    unit.start_wait();
                                }
                            }
                            else
                            {
                                Temp_Ai_Loc = retreat_loc;
                                Ai_Action = Ai_Actions.Move_To_Target;
                                // Tests for doors in the way // Not sure if this one works //Yeti
                                if (unit.can_open_door() && !unit.cantoing)
                                {
                                    Vector2? door_target = Game_AI.door_target(unit, Temp_Ai_Loc);
                                    // Doors are in the way, head toward them with intent of opening
                                    if (door_target != null)
                                    {
                                        Temp_Ai_Loc = (Vector2)door_target;
                                        unit.mission = 22;
                                        Ai_Action = Ai_Actions.Idle;
                                        Ai_Timer = 0;
                                        cont = false;
                                        return cont;
                                    }
                                }
                                Global.player.force_loc(unit.loc);
                            }
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Ai_Action = Ai_Actions.Finish_Movement;
                            }
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (unit.can_heal_self() && !unit.cantoing)
                                {
                                    unit.mission = 26;
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Idle;
                                    cont = false;
                                }
                                else if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 28: Use items
                case 28:
                    switch (Ai_Action)
                    {
                        // Determine and use item
                        case Ai_Actions.Idle:
                            switch (Ai_Timer)
                            {
                                case 0:
                                    if (get_scene_map() == null)
                                        return cont;
                                    // If something to trade for was found
                                    if (Game_AI.healing_trade_target(unit).IsSomething)
                                        Global.player.force_loc(unit.loc);
                                    Ai_Timer++;
                                    cont = false;
                                    break;
                                case 1:
                                    if (!Scrolling || Skip_Ai_Turn)
                                    {
                                        if (get_scene_map() == null)
                                            return cont;

                                        var trade_target = Game_AI.healing_trade_target(unit);
                                        // If something to trade for was found
                                        if (trade_target.IsSomething)
                                        {
                                            int trade_target_id = trade_target.ValueOrDefault.Item1;
                                            int trade_item_index = trade_target.ValueOrDefault.Item2;

                                            if (!Skip_Ai_Turn)
                                                get_scene_map().set_item_received_popup(
                                                    Units[trade_target_id].actor
                                                        .items[trade_item_index], 60);

                                            unit.actor.trade(Units[trade_target_id].actor,
                                                unit.actor.num_items,
                                                trade_item_index);
                                            /* @Debug: setting into .items doesn't affect the underlying data
                                            unit.actor.items[unit.actor.num_items] =
                                                Units[trade_target_id].actor.items[trade_item_index];
                                            Units[trade_target_id].actor.items[trade_item_index] = new Item_Data();

                                            unit.actor.sort_items();
                                            Units[trade_target_id].actor.sort_items();*/

                                            if (!unit.sprite_moving)
                                                unit.sprite_moving = true;
                                            unit.face(Units[trade_target_id]);

                                            unit.mission = 26;
                                            Ai_Timer = 8;
                                            Ai_Action = Ai_Actions.Idle;
                                            cont = false;
                                        }
                                        else
                                        {
                                            // Torch
                                            if (unit.could_use_torch() && unit.has_torch())
                                            {
                                                int item = unit.torch_item();
                                                // Unit movement locked in
                                                unit.moved();
                                                Global.game_state.call_item(unit.id, item);
                                                Global.player.force_loc(unit.loc);
                                                Ai_Timer++;
                                            }
                                            // Healing
                                            else if (unit.actor.has_critical_health() && unit.can_heal_self())
                                            {
                                                Global.player.force_loc(unit.loc);
                                                unit.mission = 26;
                                                Ai_Timer = 0;
                                                Ai_Action = Ai_Actions.Idle;
                                            }
                                            // Wait
                                            else
                                            {
                                                if (unit.turn_start_loc != unit.loc || unit.cantoing)
                                                    unit.start_wait();
                                                else
                                                    cont = false;
                                                Ai_Timer = 0;
                                                Ai_Phase = 3;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    if (!Scrolling || Skip_Ai_Turn)
                                    {
                                        Ai_Timer = 0;
                                        Ai_Action = Ai_Actions.Wait_For_Item;
                                    }
                                    break;
                            }
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Item:
                            if (!Global.game_state.item_active)
                                Ai_Phase = 3;
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 29: Rescue
                case 29:
                    switch (Ai_Action)
                    {
                        // Moves to ally
                        case Ai_Actions.Idle:
                            // Insurance against crashing
                            if (Temp_Ai_Target == null || unit.is_rescuing)
                            {
                                if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                    unit.start_wait();
                                }
                            }
                            else
                            {
                                Temp_Ai_Loc = new Vector2(Temp_Ai_Target[1], Temp_Ai_Target[2]);
                                Ai_Action = Ai_Actions.Move_To_Target;
                                Global.player.force_loc(unit.loc);
                            }
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Vector2 target_loc = Units[Temp_Ai_Target[0]].loc;
                                unit.face(target_loc);
                                if (!is_off_screen(target_loc))
                                    Ai_Timer = Constants.Map.AI_WAIT_TIME;
                                if (Ai_Timer < ai_wait_time)
                                    Ai_Timer++;
                                else
                                {
                                    Ai_Timer = 0;
                                    if (!Skip_Ai_Turn)
                                    {
                                        if (unit.visible_by() || Units[Temp_Ai_Target[0]].visible_by())
                                            Global.player.target_tile(target_loc, 20);
                                        Global.player.force_loc(target_loc);
                                    }
                                    Ai_Action = Ai_Actions.Rescue_Target;
                                }
                            }
                            else
                                cont = true;
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Rescue_Target:
                            if (!Global.player.is_targeting())
                            {
                                // Unit movement locked in
                                unit.moved();
                                unit.rescue_ally(Temp_Ai_Target[0]);
                                Ai_Action = Ai_Actions.Wait_For_Rescue;
                            }
                            else
                                cont = true;
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Rescue:
                            if (!Global.game_state.rescue_active)
                                Ai_Action = Ai_Actions.Finish_Movement;
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (!unit.ready)
                                {
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                                else if (unit.has_canto())
                                {
                                    unit.mission = 3;
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Search_For_Targets;
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                #region 30: Rescue Drop
                case 30:
                    switch (Ai_Action)
                    {
                        // Moves to ally
                        case Ai_Actions.Idle:
                            // Insurance against crashing
                            if (!unit.is_rescuing)
                            {
                                if (!unit.cantoing)
                                {
                                    // Use items if needed
                                    unit_use_item(unit);
                                    cont = false;
                                }
                                else
                                {
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                    unit.start_wait();
                                }
                            }
                            else
                            {
                                if (Temp_Ai_Target == null)
                                    Temp_Ai_Target = Game_AI.search_for_rescue_drop(unit);
                                Temp_Ai_Loc = new Vector2(Temp_Ai_Target[0], Temp_Ai_Target[1]);
                                Ai_Action = Ai_Actions.Move_To_Target;
                                Global.player.force_loc(unit.loc);
                            }
                            break;
                        // Moves to target
                        case Ai_Actions.Move_To_Target:
                            if (!Scrolling || Skip_Ai_Turn)
                            {
                                unit.ai_move_to(Temp_Ai_Loc);
                                Ai_Action = Ai_Actions.Wait_For_Move;
                            }
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Wait_For_Move:
                            if (!unit.is_in_motion() && Temp_Ai_Loc == unit.loc)
                            {
                                Vector2 target_loc = new Vector2(Temp_Ai_Target[2], Temp_Ai_Target[3]);
                                unit.face(target_loc);
                                if (!is_off_screen(target_loc))
                                    Ai_Timer = Constants.Map.AI_WAIT_TIME;
                                if (Ai_Timer < ai_wait_time)
                                    Ai_Timer++;
                                else
                                {
                                    Ai_Timer = 0;
                                    if (!Skip_Ai_Turn)
                                    {
                                        if (!Global.game_map.fow || (unit.visible_by() ||
                                                Global.game_map.fow_visibility[Constants.Team.PLAYER_TEAM].Contains(target_loc)))
                                            Global.player.target_tile(target_loc, 20);
                                        Global.player.force_loc(target_loc);
                                    }
                                    Ai_Action = Ai_Actions.Rescue_Target;
                                }
                            }
                            else
                                cont = true;
                            break;
                        // Waits until attacker has moved
                        case Ai_Actions.Rescue_Target:
                            if (!Global.player.is_targeting())
                            {
                                // Unit movement locked in
                                unit.moved();
                                unit.drop_ally(new Vector2(Temp_Ai_Target[2], Temp_Ai_Target[3]));
                                Ai_Action = Ai_Actions.Wait_For_Rescue;
                            }
                            else
                                cont = true;
                            break;
                        // Goes to next unit
                        case Ai_Actions.Wait_For_Rescue:
                            if (!Global.game_state.rescue_active)
                                Ai_Action = Ai_Actions.Finish_Movement;
                            else
                                cont = true;
                            break;
                        case Ai_Actions.Finish_Movement:
                            if (!unit.is_in_motion())
                            {
                                if (!unit.ready)
                                {
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                                else if (unit.has_canto())
                                {
                                    unit.mission = 3;
                                    Ai_Timer = 0;
                                    Ai_Action = Ai_Actions.Search_For_Targets;
                                    cont = false;
                                }
                                else
                                {
                                    unit.start_wait();
                                    Ai_Timer = 0;
                                    Ai_Phase = 3;
                                }
                            }
                            else
                                cont = true;
                            break;
                    }
                    break;
                #endregion

                // Unlisted
                default:
                    Ai_Phase = 3;
                    break;
            }
            return cont;
        }

        private bool attack_target(Game_Unit unit)
        {
            if (!Global.player.is_targeting())
            {
                unit.equip(Temp_Ai_Target[1]);
                unit.actor.organize_items();
                unit.using_siege_engine =
                    Temp_Ai_Target[1] - 1 == Siege_Engine.SIEGE_INVENTORY_INDEX;
                // Unit movement locked in
                unit.moved();
                var target = Global.game_map.attackable_map_object(Temp_Ai_Target[0]);
                if (target.is_unit())
                {
                    Game_Unit targetUnit = Units[Temp_Ai_Target[0]];
                    foreach (string skill in unit.ready_masteries())
                    {
                        if (unit.valid_mastery_target(skill, targetUnit, Global.game_map.unit_distance(unit.id, target.id)))
                            if (!Game_Unit.HEALING_MASTERIES.Contains(skill) || !unit.actor.is_full_hp())
                            {
                                unit.call_mastery(skill);
                                break;
                            }
                    }
                    targetUnit.target_unit(unit, unit.actor.weapon, Global.game_map.combat_distance(unit.id, target.id));
                    targetUnit.accept_targeting();
                }
                Global.game_state.call_battle(unit.id, Temp_Ai_Target[0]);
                Ai_Action = Ai_Actions.Wait_For_Combat;
            }
            else
                return true;
            return false;
        }

        #region AI turn skip
        public void update_ai_skip()
        {
            if (Active_Ai_Unit_Id > -1)
            {
                // If skipped/canceled skipping, have to release Start before changing state again
                if (Skip_Ai_Turn_Counter < 0)
                {
                    if (!Global.Input.pressed(Inputs.Start) && !Switching_Ai_Skip)
                        Skip_Ai_Turn_Counter = 0;
                    return;
                }
            }

            if (this.ai_skipping_allowed)
            {
                if (Global.Input.pressed(Inputs.Start))
                    Skip_Ai_Turn_Counter++;
                else
                    Skip_Ai_Turn_Counter = 0;

                // If Start has been held long enough
                if (Skip_Ai_Turn_Counter > Constants.Map.SKIP_AI_TURN_HOLD_TIME ||
                    Global.scene.message_skip_button_pressed)
                {
                    switch_ai_skip();
                }
            }
        }

        public bool ai_skipping_allowed
        {
            get
            {
                if (Active_Ai_Unit_Id > -1 &&
                    Skip_Ai_Turn_Counter >= 0 && is_map_ready(true))
                {
                    if (Skip_Ai_Turn)
                        return true;
                    // If a unit's AI canceled skipping, don't allow skipping again
                    if (SwitchCancelUnitId != -1 &&
                            SwitchCancelUnitId == Active_Ai_Unit_Id)
                        return false;
                    return !is_skip_ai_blocked();
                }
                return false;
            }
        }

        protected void refresh_ai_skip_switch()
        {
            Switching_Ai_Skip_Counter--;
            if (Switching_Ai_Skip_Counter <= 0)
            {
                if (get_scene_map() != null)
                    get_scene_map().reset_skip_timer();
                Skip_Ai_Turn = !Skip_Ai_Turn;
                Switching_Ai_Skip = false;
            }
        }

        protected bool is_skip_ai_blocked()
        {
            if (Global.game_state.combat_active)
                return true;
            if (Global.game_system.is_interpreter_running)
                return true;
            if (Global.scene.is_message_window_active)
                return true;
            return false;
        }

        protected void switch_ai_skip()
        {
            Switching_Ai_Skip = true;
            Switching_Ai_Skip_Counter = Constants.Map.SKIP_AI_SWTICH_TIME;
            Skip_Ai_Turn_Counter = -1;
        }

        internal void switch_out_of_ai_skip(object sender, EventArgs e)
        {
            switch (skip_ai_state)
            {
                case Ai_Turn_Skip_State.SkipStart:
                    cancel_ai_skip();
                    break;
                case Ai_Turn_Skip_State.Skipping:
                    switch_out_of_ai_skip();
                    break;
                // If not skipping, or skip is already ending, nothing to worry about
                case Ai_Turn_Skip_State.NotSkipping:
                case Ai_Turn_Skip_State.SkipEnd:
                    break;
            }
        }
        public bool switch_out_of_ai_skip(bool preventSkipForActive = true)
        {
            if (Skip_Ai_Turn)
            {
                if (!Switching_Ai_Skip)
                {
                    // If cancelled AI skip while a unit is active, don't allow skipping
                    // again until that unit is done
                    if (preventSkipForActive)
                    {
                        SwitchCancelUnitId = Active_Ai_Unit_Id;
                    }
                    switch_ai_skip();
                }
                return true;
            }
            return false;
        }

        public bool cancel_ai_skip()
        {
            if (!Skip_Ai_Turn && Switching_Ai_Skip)
            {
                Skip_Ai_Turn = true;
                Switching_Ai_Skip_Counter =
                    Constants.Map.SKIP_AI_SWTICH_TIME - Switching_Ai_Skip_Counter;
                Skip_Ai_Turn_Counter = -1;
                return true;
            }
            return false;
        }
        #endregion

        protected void unit_use_item(Game_Unit unit)
        {
            unit.mission = Game_AI.USE_ITEM_MISSION;
            Ai_Timer = 0;
            Ai_Action = Ai_Actions.Idle;
        }

        private void target_sort(List<LocationDistance> targets)
        {
            targets.Sort(delegate(LocationDistance a, LocationDistance b)
            {
                return a.dist - b.dist;
            });
        }

        protected void sort_ai_team()
        {
            Ai_Team.Sort(delegate(int a, int b)
            {
                // Mission priority
                if (Units[b].ai_priority != Units[a].ai_priority)
                    return Units[b].ai_priority - Units[a].ai_priority;
                // Mission
                if (Units[b].ai_mission != Units[a].ai_mission)
                    return Units[b].ai_mission - Units[a].ai_mission;
                // Range
                int range_b = Units[b].max_range_absolute();
                int range_a = Units[a].max_range_absolute();
                if (range_b != 0 && range_a != 0 && range_b != range_a)
                    return range_b - range_a;

                return 0;
            });

            // Sets up the subset of the team that are on attack missions
            Attack_Ai_Team.Clear();
            //Attack_Ai_Team.AddRange(Ai_Team); //Debug

            // Makes all units try to equip something, if they aren't
            foreach (int id in Ai_Team)
            {
                Game_Unit unit = Units[id];
                if (!unit.actor.is_equipped)
                    unit.actor.sort_items();
            }
            // Removes non-attack units, and attack units with no enemies in range
            Attack_Ai_Team.AddRange(Ai_Team.Where(id => ai_unit_attack_mission(Units[id])));
        }

        private bool ai_unit_ready_to_act(Game_Unit ai_unit)
        {
            return !(ai_unit.is_rescued || ai_unit.disabled || Global.game_map.is_off_map(ai_unit.loc) || (is_player_turn && !ai_unit.uncontrollable));
        }
        private bool ai_unit_attack_mission(Game_Unit ai_unit)
        {
            // If not on an attack mission
            if (!Game_AI.ATTACK_MISSIONS.Contains(ai_unit.ai_mission))
                return false;
            // If no enemies are in range
            if (ai_unit.enemies_in_range(
                    Game_AI.IMMOBILE_MISSIONS.Contains(ai_unit.ai_mission) ?
                    new HashSet<Vector2> { ai_unit.loc } : ai_unit.move_range, true)[1].Count == 0)
                return false;
            return true;
        }

        private bool ai_unit_seeks_healing(Game_Unit unit, bool only_attack_in_range_units = false)
        {
            if (unit.actor.has_critical_health())
            {
                // If able to heal self, break out of this mission and switch to self heal mission
                if (unit.can_heal_self())
                    return true;
                // If this unit attacks in range, minimize movement in search of healing because attacking in range is paramount
                if (only_attack_in_range_units)
                {
                    // If the unit is generic, it can't trade for healing items
                    if (unit.actor.is_generic_actor)
                        return false;
                    // If there's a healer nearby, break out of this mission and switch to self heal mission
                    Maybe<Vector2> healer_loc = Game_AI.search_for_ally(unit, Search_For_Ally_Modes.Attack_In_Range_Healing_Item)[1];
                    if (healer_loc.IsSomething)
                        // If the healer has a healing item to trade, ignore enemy range
                        return true;
                }
                else
                {
                    // If there's a healer nearby, break out of this mission and switch to self heal mission
                    // This doesn't currently check if the ally is within one move //Yeti
                    Maybe<Vector2> healer_loc = Game_AI.search_for_ally(unit, Search_For_Ally_Modes.Looking_For_Healing_Item)[1];
                    if (healer_loc.IsSomething)
                        // If the healer has a healing item to trade, ignore enemy range
                        return true;
                    // Look for healing terrain or a healer
                    healer_loc = Game_AI.search_for_healing(unit)[1];
                    if (healer_loc.IsSomething && (!unit.ai_terrain_healing || healer_loc != unit.loc))
                    {
                        // If the healer location is not in enemy range, move to the healer
                        if (!Ai_Enemy_Attack_Range.Contains(healer_loc))
                            return true;
                    }
                }
            }
            return false;
        }

        //enum NextAIUnitModes { Seize, StatusStaff, HealthyAttacker, Healer, Attacker,
        //    Other, Savior }

        protected int? next_ai_unit(List<int> ai_team, List<int> attack_ai_team)
        {
            return next_ai_unit(ai_team, attack_ai_team, false);
        }
        protected int? next_ai_unit(List<int> ai_team, List<int> attack_ai_team, bool uncontrollable)
        {
            List<int> attack_ai_team_dup = new List<int>(attack_ai_team);
            //attack_ai_team_dup.AddRange(attack_ai_team);
            int active_ai_unit;
            Game_Unit unit;
            int ai_mission;

            int modes = Enum_Values.GetEnumCount(typeof(NextAIUnitModes));
            for (int mode = 0; mode <= modes; mode++)
            {
                switch (mode)
                {
                    #region Seize: Looking for anyone who can seize
                    case (int)NextAIUnitModes.Seize:
                        // If no seize points for this team, continue
                        if (!Global.game_map.get_seize_points(Team_Turn).Any())
                            continue;
                        for (int i = 0; i < ai_team.Count; i++)
                        {
                            active_ai_unit = ai_team[i];
                            unit = Units[active_ai_unit];
                            unit.mission = -1;
                            if (unit.uncontrollable)
                            {
                                continue;
                            }
                            ai_mission = unit.berserk ? Game_AI.BERSERK_MISSION : unit.ai_mission;
                            // If on an attack mission and not an immobile mission
                            if (Game_AI.ATTACK_MISSIONS.Contains(ai_mission) &&
                                    !Game_AI.IMMOBILE_MISSIONS.Contains(ai_mission))
                                // If there is a seize point within move range
                                if (update_ai_move_range(unit).Intersect(
                                        Global.game_map.get_seize_points(unit.team, unit.group)).Any())
                                    return active_ai_unit;
                        }
                        break;
                    #endregion
                    #region StatusStaff: Looking for anyone who can use status staves (to better enable allies who are just attacking)
                    case (int)NextAIUnitModes.StatusStaff:
                        for (int i = 0; i < ai_team.Count; i++)
                        {
                            active_ai_unit = ai_team[i];
                            unit = Units[active_ai_unit];
                            unit.mission = -1;
                            if (unit.uncontrollable && !uncontrollable)
                            {
                                continue;
                            }
                            ai_mission = unit.berserk ? Game_AI.BERSERK_MISSION : unit.ai_mission;
                            // If on an attack staff mission
                            if (Game_AI.STATUS_MISSIONS.Contains(ai_mission))
                            {
                                List<int>[] targets = (unit.enemies_in_staff_range(
                                    Game_AI.IMMOBILE_MISSIONS.Contains(
                                    ai_mission) ? new HashSet<Vector2> { unit.loc } : update_ai_move_range(unit), true));
                                // If any targets are in range
                                if (targets[1].Count > 0)
                                {
                                    return active_ai_unit;
                                }
                            }
                        }
                        break;
                    #endregion
                    #region HealthyAttacker: Looking for anyone healthy who can attack
                    case (int)NextAIUnitModes.HealthyAttacker:
                        for (int i = 0; i < attack_ai_team_dup.Count; i++)
                        {
                            active_ai_unit = attack_ai_team_dup[i];
                            unit = Units[active_ai_unit];
                            unit.mission = -1;
                            if (unit.uncontrollable && !uncontrollable)
                            {
                                continue;
                            }
                            ai_mission = unit.berserk ? Game_AI.BERSERK_MISSION : unit.ai_mission;
                            // If on an attack mission
                            if (Game_AI.ATTACK_MISSIONS.Contains(ai_mission))
                            {
                                // Healing~
                                // Attack in place/attack in move range missions can't self heal? //Debug
                                if (!Game_AI.UNMOVING_MISSIONS.Contains(ai_mission) && !unit.berserk)
                                    //if (!Game_AI.IMMOBILE_MISSIONS.Contains(ai_mission) && !unit.berserk) //Debug
                                    if (unit.actor.has_critical_health())
                                    {
                                        continue;
                                    }
                                // n/m not healing self
                                List<int>[] targets = (unit.enemies_in_range(
                                    Game_AI.IMMOBILE_MISSIONS.Contains(
                                    ai_mission) ? new HashSet<Vector2> { unit.loc } : update_ai_move_range(unit), true));
                                // If any targets are in range
                                if (targets[1].Count > 0)
                                {
                                    if (false) // this checks if the targets can be damaged or something >:? //Yeti
                                    {

                                    }
                                    else
                                    {
                                        return active_ai_unit;
                                        // this checks if the targets can be damaged or something >:? //Yeti
                                        foreach (int target_id in targets[0])
                                        {
                                            foreach (int weapon_id in targets[1].Distinct().ToList()) //ListOrEquals
                                            {
                                                var weapon = Global.data_weapons[weapon_id];
                                                // If can damage or inflict status, and a non-zero hit rate
                                                var stats = new Calculations.Stats.CombatStats(
                                                    unit.id, target_id, weapon, weapon.Min_Range);
                                                if ((stats.dmg() > 0 || weapon.Status_Inflict.Any()) && stats.hit() > 0)
                                                    return active_ai_unit;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    #endregion
                    #region Healer: Looking for healers before the rest, since they'll want to heal the wounded
                    case (int)NextAIUnitModes.Healer:
                        for (int i = 0; i < ai_team.Count; i++)
                        {
                            active_ai_unit = ai_team[i];
                            unit = Units[active_ai_unit];
                            unit.mission = -1;
                            if (unit.uncontrollable && !uncontrollable)
                            {
                                continue;
                            }
                            ai_mission = unit.berserk ? Game_AI.BERSERK_MISSION : unit.ai_mission;
                            // Healing others
                            if (Game_AI.HEALING_MISSIONS.Contains(ai_mission))
                            {
                                Ai_Move_Range = Game_AI.IMMOBILE_MISSIONS.Contains(
                                    ai_mission) ? new HashSet<Vector2> { unit.loc } : update_ai_move_range(unit);
                                List<int>[] ally_target_ary = unit.allies_in_staff_range(Ai_Move_Range);
                                List<int> useable_ally_staves = new List<int>();
                                foreach (int j in ally_target_ary[1])
                                {
                                    if (!useable_ally_staves.Contains((int)unit.actor.weapon_index(j)))
                                        useable_ally_staves.Add((int)unit.actor.weapon_index(j));
                                }
                                int[] heal_targets = Game_AI.get_heal_target(unit, ally_target_ary[0], useable_ally_staves,
                                    !Game_AI.IMMOBILE_MISSIONS.Contains(ai_mission), true);
                                if (heal_targets != null)
                                    return active_ai_unit;
                            }
                        }
                        break;
                    #endregion
                    #region Attacker: Looking for injured attackers
                    case (int)NextAIUnitModes.Attacker:
                        for (int i = 0; i < attack_ai_team_dup.Count; i++)
                        {
                            active_ai_unit = attack_ai_team_dup[i];
                            unit = Units[active_ai_unit];
                            unit.mission = -1;
                            if (unit.uncontrollable && !uncontrollable)
                            {
                                continue;
                            }
                            ai_mission = unit.berserk ? Game_AI.BERSERK_MISSION : unit.ai_mission;
                            // If on an attack mission
                            if (Game_AI.ATTACK_MISSIONS.Contains(ai_mission))
                            {
                                /*// Healing~
                                // Attack in place/attack in move range missions can't self heal? //Debug
                                if (!Game_AI.UNMOVING_MISSIONS.Contains(ai_mission) && !unit.berserk)
                                //if (!Game_AI.IMMOBILE_MISSIONS.Contains(ai_mission) && !unit.berserk) //Debug
                                {
                                    if (unit.actor.has_critical_health())
                                    {
                                        if (unit.can_heal_self())
                                        {
                                            Ai_Healing = true;
                                            return active_ai_unit;
                                        }
                                        Vector2? healer_loc = Game_AI.search_for_ally(unit, 2)[1];
                                        if (healer_loc != null)
                                        {
                                            if (healer_loc == unit.loc)
                                            {
                                                i++;
                                                continue;
                                            }
                                            else
                                            {
                                                Ai_Healing = true;
                                                return active_ai_unit;
                                            }
                                        }
                                    }
                                }*/
                                List<int>[] targets = (unit.enemies_in_range(
                                    Game_AI.IMMOBILE_MISSIONS.Contains(
                                    ai_mission) ? new HashSet<Vector2> { unit.loc } : update_ai_move_range(unit), true));
                                // If any targets are in range
                                if (targets[1].Count > 0)
                                    return active_ai_unit;
                            }
                        }
                        break;
                    #endregion
                    #region Other: Otherwise just proceed with the first unit found unless the unit never moves normally
                    case (int)NextAIUnitModes.Other:
                        for (int i = 0; i < ai_team.Count; i++)
                        {
                            active_ai_unit = ai_team[i];
                            unit = Units[active_ai_unit];
                            unit.mission = -1;
                            if (unit.uncontrollable && !uncontrollable)
                            {
                                continue;
                            }
                            ai_mission = unit.berserk ?
                                Game_AI.BERSERK_MISSION : unit.ai_mission;
                            // If on attack mission, not rooted in place, and not doing something too important to heal
                            if (Game_AI.ATTACK_MISSIONS.Contains(ai_mission) &&
                                !Game_AI.DETERMINED_MISSIONS.Contains(ai_mission) &&
                                // Attack in place/attack in move range missions can't self heal? //Debug
                                !Game_AI.UNMOVING_MISSIONS.Contains(ai_mission))
                            //!Game_AI.IMMOBILE_MISSIONS.Contains(ai_mission)) //Debug
                            {
                                // Healing~
                                if (unit.actor.has_critical_health())
                                {
                                    if (unit.can_heal_self())
                                    {
                                        Ai_Healing = true;
                                        return active_ai_unit;
                                    }
                                    Maybe<Vector2> healer_loc = Game_AI.search_for_healing(unit)[1];
                                    if (healer_loc.IsSomething)
                                    {
                                        Ai_Healing = true;
                                        return active_ai_unit;
                                    }
                                }
                                // Torches/etc
                                else if ((unit.could_use_torch() && unit.has_torch()) || false) // put other things here //Yeti
                                {
                                    unit.mission = 28;
                                    return active_ai_unit;
                                }
                            }
                            // If unit does things without enemy interaction
                            if (ai_mission != Game_AI.SAVIOR_MISSION && !Game_AI.UNMOVING_MISSIONS.Contains(ai_mission))
                            {
                                // Skip if on an attack mission and all tiles blocked
                                if (Game_AI.MOVING_MISSIONS.Contains(ai_mission))
                                {
                                    foreach (Vector2 loc in unit.move_range)
                                        if (Global.game_map.get_unit(loc) == null)
                                            return active_ai_unit;
                                }
                                else
                                    return active_ai_unit;
                            }
                        }
                        break;
                    #endregion
                    #region Savior: Looking for saviors
                    case (int)NextAIUnitModes.Savior:
                        for (int i = 0; i < ai_team.Count; i++)
                        {
                            active_ai_unit = ai_team[i];
                            unit = Units[active_ai_unit];
                            unit.mission = -1;
                            if (unit.uncontrollable && !uncontrollable)
                            {
                                continue;
                            }
                            ai_mission = unit.berserk ? Game_AI.BERSERK_MISSION : unit.ai_mission;
                            // If unit does things without enemy interaction
                            if (!Game_AI.UNMOVING_MISSIONS.Contains(ai_mission))
                            {
                                // Skip if on an attack mission and all tiles blocked
                                if (Game_AI.MOVING_MISSIONS.Contains(ai_mission))
                                {
                                    if (!unit.move_range
                                        .Any(x => Global.game_map.get_unit(x) == null))
                                        return active_ai_unit;
                                }
                                else
                                    return active_ai_unit;
                            }
                        }
                        // If everyone doesn't want to move, end turn
                        if (uncontrollable)
                            ai_team.Clear();
                        break;
                    #endregion
                }
            }

            return null;
        }

        protected void reset_for_next_unit()
        {
            Ai_Move_Range.Clear();
            Temp_Ai_Target = null;
            Temp_Ai_Locs.Clear();
            Useable_Weapons.Clear();
            Defending_Area = false;
            Limited_Access_En_Route = false;
            SwitchCancelUnitId = -1;

            Ai_Team.Remove(Active_Ai_Unit_Id);
            if (Ai_Team.Count == 0)
                Ai_Phase = 4;
            else
            {
                Ai_Phase = 1;
                Ai_Action = Ai_Actions.Idle;
                Ai_Timer = 0;
            }
        }

        internal void remove_ai_unit(int id)
        {
            Ai_Team.Remove(id);
            Attack_Ai_Team.Remove(id);
        }

        internal void refresh_ai_unit(int id)
        {
            if (Global.game_state.ai_active && !Ai_Team.Contains(id))
            {
                Ai_Team.Insert(0, id);
                if (ai_unit_attack_mission(Units[id]))
                    Attack_Ai_Team.Insert(0, id);
            }
        }

        protected HashSet<Vector2> update_ai_move_range(Game_Unit unit)
        {
            HashSet<Vector2> move_range = unit.move_range;
            // Remove tiles blocked by light runes, etc
            move_range = new HashSet<Vector2>(move_range
                .Where(x => unit.can_move_to(x, move_range)));
            List<int> units = new List<int>();
            //foreach(int[] group in Config.TEAM_GROUPS)
            //    foreach(int team in group)
            //        foreach (int other_unit_id in Global.game_map.teams[team])
            // Goes through other units and removes their locations from move range (since can't walk on other units)
            foreach(int other_unit_id in Units.Keys)
                units.Add(other_unit_id);
            units.Remove(unit.id);
            foreach (int i in units)
                move_range.Remove(Units[i].loc);
            return move_range;
        }
    }
}
