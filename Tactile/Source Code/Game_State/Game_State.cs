using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Tactile.State;
using TactileLibrary;
using Tactile.Graphics;
using TactileVersionExtension;

namespace Tactile
{
    internal partial class Game_State
    {
        private Game_Ai_State AiState;
        private Game_Block_State BlockState;
        private Game_Chapter_End_State ChapterEndState;
        private Game_Combat_State CombatState;
        private Game_Exp_State ExpState;
        private Game_Item_State ItemState;
        private Game_Rescue_State RescueState;
        private Game_Shop_Suspend_State ShopState;
        private Game_Skills_State SkillsState;
        private Game_Support_State SupportState;
        private Game_Talk_State TalkState;
        private Game_Visit_State VisitState;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            write_events(writer);
            write_map_stuff(writer);
            new_turn_write(writer);

            BlockState.write(writer);
            ChapterEndState.write(writer);
            ShopState.write(writer);
            AiState.write(writer);
            TalkState.write(writer);
            SupportState.write(writer);
            VisitState.write(writer);
            ExpState.write(writer);
            SkillsState.write(writer);
            ItemState.write(writer);
            RescueState.write(writer);
            CombatState.write(writer);
            //move_write(writer);
        }

        public void read(BinaryReader reader)
        {
            reset();

            read_events(reader);
            read_map_stuff(reader);
            new_turn_read(reader);

            BlockState.read(reader);
            if (!Global.LOADED_VERSION.older_than(0, 6, 3, 0))
                ChapterEndState.read(reader);
            ShopState.read(reader);
            AiState.read(reader);
            TalkState.read(reader);
            SupportState.read(reader);
            VisitState.read(reader);
            ExpState.read(reader);
            SkillsState.read(reader);
            ItemState.read(reader);
            RescueState.read(reader);
            CombatState.read(reader);
            //move_read(reader);
        }
        #endregion

        internal bool ai_active { get { return AiState.ai_active; } }
        internal bool player_ai_active { get { return AiState.player_ai_active; } }
        internal int active_ai_unit { get { return AiState.active_ai_unit; } }
        internal HashSet<Vector2> ai_move_range
        {
            get { return AiState.ai_move_range; }
            set { AiState.ai_move_range = value; }
        }
        internal HashSet<Vector2> ai_enemy_attack_range { get { return AiState.ai_enemy_attack_range; } }
        internal int ai_turn_rn { get { return AiState.ai_turn_rn; } }
        internal Ai_Turn_Skip_State skip_ai_state { get { return AiState.skip_ai_state; } }
        internal bool skip_ai_turn_activating { get { return AiState.skip_ai_state != Ai_Turn_Skip_State.NotSkipping; } }
        internal bool skip_ai_turn_active { get { return AiState.skip_ai_state == Ai_Turn_Skip_State.Skipping; } }
        internal bool switching_ai_skip { get { return AiState.switching_ai_skip; } }
        internal int switching_ai_skip_counter { get { return AiState.switching_ai_skip_counter; } }
        internal bool ai_skipping_allowed { get { return AiState.ai_skipping_allowed; } }

        internal int battler_1_id { get { return CombatState.battler_1_id; } }
        internal int battler_2_id { get { return CombatState.battler_2_id; } }
        internal List<int> aoe_targets { get { return CombatState.aoe_targets; } }
        internal Combat_Data combat_data { get { return CombatState.combat_data; } set { CombatState.combat_data = value; } }
        internal bool transition_to_battle
        {
            get
            {
                if (dance_active)
                    return SkillsState.Dance_State.transition_to_battle;
                if (sacrifice_active)
                    return SkillsState.Sacrifice_State.transition_to_battle;
                if (item_active)
                    return ItemState.transition_to_battle;
                return CombatState.transition_to_battle;
            }
        }
        internal int battle_transition_timer
        {
            get
            {
                if (dance_active)
                    return SkillsState.Dance_State.battle_transition_timer;
                if (sacrifice_active)
                    return SkillsState.Sacrifice_State.battle_transition_timer;
                if (item_active)
                    return ItemState.battle_transition_timer;
                return CombatState.battle_transition_timer;
            }
        }
        internal bool arena { get { return CombatState.arena; } set { CombatState.arena = value; } } //private // on set //Yeti
        internal bool battle_ending { set { CombatState.battle_ending = value; } }

        internal Game_Unit event_caller_unit
        {
            get
            {
                if (VisitState.visitor != null)
                    return VisitState.visitor;
                if (TalkState.talker != null)
                    return TalkState.talker;
                return null;
            }
        }

        internal Game_Unit item_user { get { return ItemState.item_user; } }
        internal int item_used { get { return ItemState.item_used; } }

        internal Game_Unit rescue_moving_unit { get { return RescueState.rescue_moving_unit; } }

        internal int dancer_id { get { return SkillsState.Dance_State.dancer_id; } }
        internal int dance_target_id { get { return SkillsState.Dance_State.dance_target_id; } }
        internal int dance_item { get { return SkillsState.Dance_State.dance_item; } }
        internal int sacrificer_id { get { return SkillsState.Sacrifice_State.sacrificer_id; } }
        internal int stealer_id { get { return SkillsState.Steal_State.stealer_id; } }
        internal int steal_target_id { get { return SkillsState.Steal_State.steal_target_id; } }

        internal IEnumerable<int> SupportGainIds { get { return SupportState.SupportGainIds; } }
        internal IEnumerable<int> SupportGainReadyIds { get { return SupportState.SupportGainReadyIds; } }

        public SpriteParameters SupportGainGfx { get { return SupportState.SupportGainGfx; } }
        public SpriteParameters SupportGainReadyGfx { get { return SupportState.SupportGainReadyGfx; } }

        internal Dictionary<int, HashSet<int>> supports_this_chapter { get { return SupportState.Supports_This_Chapter; } }

        internal bool talk_blocking_support_gain
        {
            get { return this.talk_active && !TalkState.waiting_for_support_gain; }
        }


        public Game_State()
        {
            initialize();
        }

        private void initialize()
        {
            Metrics = new Metrics.Gameplay_Metrics();

            AiState = new Game_Ai_State();
            BlockState = new Game_Block_State();
            ChapterEndState = new Game_Chapter_End_State();
            CombatState = new Game_Combat_State();
            ExpState = new Game_Exp_State();
            ItemState = new Game_Item_State();
            RescueState = new Game_Rescue_State();
            ShopState = new Game_Shop_Suspend_State();
            SkillsState = new Game_Skills_State();
            SupportState = new Game_Support_State();
            TalkState = new Game_Talk_State();
            VisitState = new Game_Visit_State();

            hook_events();
        }

        public void reset()
        {
            Chapter_Id = null;
            hook_events(true);
            initialize();

            reset_events();
            reset_map_stuff();
            reset_new_turn();
        }

        // Redo this so that instead of properties hooking back up to this object, they hook into fellow properties whenever possible
        private void hook_events(bool unhook = false)
        {
            if (unhook)
            {
                AiState.turn_over -= Ai_State_turn_over;
                CombatState.switch_out_of_ai_skip -= AiState.switch_out_of_ai_skip;
            }
            else
            {
                AiState.turn_over += Ai_State_turn_over;
                CombatState.switch_out_of_ai_skip += AiState.switch_out_of_ai_skip;
            }
        }

        void Ai_State_turn_over(object sender, System.EventArgs e)
        {
            change_turn();
        }

        internal void update()
        {
            Scene_Map scene_map = Global.game_map.get_scene_map();
            if (scene_map == null || scene_map.map_transition_running)
            {
                // Timer stuff
                Global.game_map.update_characters_while_waiting();
                return;
            }

            bool cont = Global.game_map.update();
            update_state_functions();
            if (cont)
            {
                if (prev_player_loc != Global.player.loc)
                {
                    if (!Global.game_map.is_off_map(Global.player.loc, false))
                        Global.game_map.highlight_test();
                    prev_player_loc = Global.player.loc;
                }
                update_victory_theme();
                update_main_turn_change();
                if (is_map_ready() && !ai_active && is_battle_map && !scene_map.is_changing_turn() &&
                        !is_menuing && !Global.game_temp.minimap_call)
                    input_handling();
                // Decrement No_Input_Timer
                if (No_Input_Timer > 0 && (is_map_ready() || (
                        (visit_active || support_active || talk_active || combat_active) &&
                        !Global.game_system.is_interpreter_running)))
                    No_Input_Timer--;
            }
        }

        protected void update_state_functions()
        {
            CombatState.update();
            RescueState.update();
            ItemState.update();
            SkillsState.update();
            ExpState.update();
            VisitState.update();
            SupportState.update();
            TalkState.update();
            update_new_turn();
            AiState.update();
            ShopState.update();
            BlockState.update();
            ChapterEndState.update();

            //update_formation();
            //update_dying_units();
        }

        #region Action Callers
        internal void call_battle(int id1, int id2)
        {
            CombatState.battle_calling = true;
            Global.game_system.Battler_1_Id = id1;
            Global.game_system.Battler_2_Id = id2;
        }
        internal void call_battle(int id1, List<int> targets)
        {
            CombatState.aoe_calling = true;

            CombatState.battle_calling = true;
            Global.game_system.Battler_1_Id = id1;
            Global.game_system.Aoe_Targets = targets;
        }

        internal void call_staff(int id1, int id2,
            Maybe<Vector2> target_loc = default(Maybe<Vector2>))
        {
            Game_Unit attacker = Global.game_map.units[id1];
            if (attacker.actor.weapon.Hits_All_in_Range())
            {
                var targets = attacker.units_in_staff_range(attacker.actor.weapon);
                call_staff(id1, targets, target_loc);
            }
            else
            {
                CombatState.staff_calling = true; //Yeti
                // Is Staff_User_Id even necessary anymore //Yeti
                Global.game_system.Staff_User_Id = Global.game_system.Battler_1_Id = id1;
                Global.game_system.Staff_Target_Id = Global.game_system.Battler_2_Id = id2;
                if (target_loc.IsSomething)
                    Global.game_system.Staff_Target_Loc = target_loc;
            }
        }
        internal void call_staff(int id1, List<int> targets,
            Maybe<Vector2> target_loc = default(Maybe<Vector2>))
        {
            CombatState.aoe_calling = true;

            CombatState.staff_calling = true; //Yeti
            // Is Staff_User_Id even necessary anymore //Yeti
            Global.game_system.Staff_User_Id = Global.game_system.Battler_1_Id = id1;
            Global.game_system.Aoe_Targets = targets;
            if (target_loc.IsSomething)
                Global.game_system.Staff_Target_Loc = target_loc;
        }

        internal void call_block(int Id)
        {
            BlockState.block_calling = true;
            BlockState.blocked_id = Id;
        }

        internal void call_chapter_end(bool showRankings, bool sendMetrics, bool supportPoints)
        {
            ChapterEndState.end_chapter(showRankings, sendMetrics, supportPoints);
        }

        internal void call_item(int id, int item_index)
        {
            ItemState.item_calling = true;
            Global.game_system.Item_User = id;
            Global.game_system.Item_Used = item_index;
        }
        internal void call_item(int id, int item_index, Vector2 itemLoc)
        {
            call_item(id, item_index);
            Global.game_system.ItemTargetLoc = itemLoc;
        }
        internal void call_item(int id, int item_index, int promotionId)
        {
            call_item(id, item_index);
            Global.game_system.ItemPromotionId = promotionId;
        }

        internal void call_talk(string event_name, int initiator_id)
        {
            TalkState.talk_calling = true;
            TalkState.talk_event_name = event_name;
            Global.game_system.Visitor_Id = initiator_id;
        }

        internal void call_shop_suspend()
        {
            ShopState.suspend_shop();
        }

        internal void call_dance(int id1, int id2, int dance_item_index)
        {
            SkillsState.Dance_State.dance_calling = true;
            Global.game_system.Battler_1_Id = id1;
            Global.game_system.Battler_2_Id = id2;
            Global.game_system.Dance_Item = dance_item_index;
        }
        internal void call_sacrifice(int id1, int id2)
        {
            SkillsState.Sacrifice_State.sacrifice_calling = true;
            Global.game_system.Battler_1_Id = id1;
            Global.game_system.Battler_2_Id = id2;
        }
        internal void call_steal(int id1, int id2, int steal_item_index)
        {
            SkillsState.Steal_State.steal_calling = true;
            Global.game_system.Battler_1_Id = id1;
            Global.game_system.Battler_2_Id = id2;
            Global.game_system.Stolen_Item = steal_item_index;
        }

        internal void call_support(int id1, int id2)
        {
            SupportState.support_calling = true;
            Global.game_system.Rescuer_Id = id1;
            Global.game_system.Rescuee_Id = id2;
        }

        internal void call_support_gain(int id1, int id2)
        {
            SupportState.support_gain_calling = true;
            Global.game_system.SupportGainId = id1;
            Global.game_system.SupportGainTargets = new List<int> { id2 };
        }
        internal void call_support_gain(int id1, IEnumerable<int> targets)
        {
            SupportState.support_gain_calling = true;
            Global.game_system.SupportGainId = id1;
            Global.game_system.SupportGainTargets = new List<int>(targets);
        }

#if DEBUG
        internal void remove_support_this_chapter(int id1, int id2)
        {
            SupportState.remove_support_this_chapter(id1, id2);
        }
#endif

        internal void call_visit(Visit_Modes mode, int id, Vector2 visit_loc, bool pillaging = false)
        {
            VisitState.visit_calling = true;
            VisitState.visit_mode = (int)mode;
            Global.game_system.Visitor_Id = id;
            Global.game_system.Visit_Loc = visit_loc;
            VisitState.Pillaging = pillaging;
        }
        #endregion

        #region Actions Active
        internal bool combat_active { get { return CombatState.battle_calling || CombatState.in_battle; } }
        internal bool battle_active { get { return combat_active && !staff_active; } }
        internal bool staff_active { get { return CombatState.staff_calling || CombatState.in_staff_use; } } // || CombatState.in_battle; } } //Debug
        internal bool aoe_active { get { return CombatState.aoe_calling || CombatState.in_aoe; } }

        internal bool block_active { get { return BlockState.block_calling || BlockState.in_block; } }
        internal bool chapter_end_active { get { return ChapterEndState.Active; } }
        internal bool exp_active { get { return ExpState.exp_calling || ExpState.in_exp_gain; } }
        internal bool item_active { get { return ItemState.item_calling || ItemState.in_item_use; } }
        internal bool rescue_active { get { return RescueState.rescue_calling != Rescue_Modes.None || RescueState.in_rescue; } }
        internal bool shop_suspend_active { get { return ShopState.in_shop_suspend; } }
        internal bool skills_active { get { return !SkillsState.is_skill_ready(); } }
        internal bool support_active { get { return this.support_convo_active; } }
        //internal bool support_active { get { return this.support_convo_active || // @Debug
        //    SupportState.support_gain_calling || SupportState.in_support_gain; } }
        internal bool support_convo_active { get { return SupportState.support_calling || SupportState.in_support; } }
        internal bool support_gain_active { get { return SupportState.in_support_gain; } }
        internal bool talk_active { get { return TalkState.talk_calling || TalkState.in_talk; } }

        internal bool visit_active { get { return VisitState.visit_calling || VisitState.in_visit; } }
        internal Visit_Modes visit_mode { get { return (Visit_Modes)VisitState.visit_mode; } }
        internal bool pillaging { get { return VisitState.Pillaging; } }
        internal Vector2 visit_loc { get { return VisitState.visit_loc; } }

        internal bool dance_active { get { return SkillsState.dance_active; } }
        internal bool sacrifice_active { get { return SkillsState.sacrifice_active; } }
        internal bool steal_active { get { return SkillsState.steal_active; } }
        #endregion

        #region Stuff that should probably be events
        internal void refresh_ai_defend_area()
        {
            AiState.refresh_ai_defend_area();
        }

        internal void activate_player_ai()
        {
            AiState.player_ai_active = true;
        }
        #endregion

        private Scene_Map get_scene_map()
        {
            return Global.game_map.get_scene_map();
        }

        internal void update_ai_skip()
        {
            AiState.update_ai_skip();
        }
        internal void switch_out_of_ai_skip()
        {
            AiState.switch_out_of_ai_skip(false);
        }
        internal void cancel_ai_skip()
        {
            AiState.cancel_ai_skip();
        }

        internal void remove_ai_unit(int id)
        {
            AiState.remove_ai_unit(id);
        }
        internal void refresh_ai_unit(int id)
        {
            AiState.refresh_ai_unit(id);
        }

        internal void skip_battle_scene()
        {
            CombatState.skip_battle_scene();
            SkillsState.skip_battle_scene();
        }
        internal Game_Unit enemy_of_dying_unit()
        {
            return CombatState.enemy_of_dying_unit();
        }
        internal void arena_load()
        {
            CombatState.arena_load();
        }

        internal void to_arena()
        {
            CombatState.to_arena();
        }

        internal void gain_exp(int unit_id, int exp_gain)
        {
            ExpState.gain_exp(unit_id, exp_gain);
        }
        internal bool process_exp_gain()
        {
            return ExpState.process_exp_gain();
        }
        internal void cancel_exp_sound()
        {
            ExpState.cancel_exp_sound();
        }

        internal void call_rescue(Rescue_Modes mode, int id, int ally_id, Vector2 player_loc)
        {
            Global.player.loc = player_loc;
            Global.player.instant_move = true;
            RescueState.rescue_calling = mode;
            Global.game_system.Rescuer_Id = id;
            Global.game_system.Rescuee_Id = ally_id;
        }

        internal void reset_support_data()
        {
            SupportState.reset_support_data();
        }

        internal int exp_gauge_gain
        {
            get { return ExpState.Exp_Gauge_Gain; }
            set { ExpState.Exp_Gauge_Gain = value; }
        }
    }
}
