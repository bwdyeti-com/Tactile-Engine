using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Tactile.State
{
    partial class Game_Combat_State
    {
        protected bool Aoe_Calling = false;
        protected bool In_Aoe = false;
        protected List<int> Aoe_Targets = new List<int>();
        protected List<int> Aoe_Battlers = new List<int>();

        #region Serialization
        private void write_aoe(BinaryWriter writer)
        {
            writer.Write(In_Aoe);
        }

        private void read_aoe(BinaryReader reader)
        {
            In_Aoe = reader.ReadBoolean();
        }
        #endregion

        #region Accessors
        public bool aoe_calling
        {
            get { return Aoe_Calling; }
            set { Aoe_Calling = value; }
        }

        public bool in_aoe { get { return In_Aoe; } }

        public List<int> aoe_targets { get { return Aoe_Targets; } }
        #endregion

        private void update_aoe()
        {
            if (Aoe_Calling)
                setup_aoe_battle();
            if (In_Aoe)
            {
                update_aoe_map_battle();
            }
        }

        protected void update_aoe_map_battle()
        {
            Game_Unit Battler_1 = null, Battler_2 = null;
            if (Battler_1_Id > -1)
                Battler_1 = Units[Battler_1_Id];
            Scene_Map scene_map = get_scene_map();
            if (scene_map == null) return;
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Combat_Phase)
                {
                    case 0:
                        if (Combat_Timer == 0)
                        {
                            Global.scene.suspend();
                            Combat_Phase++;
                        }
                        break;
                    #region 1: Battle Setup
                    case 1:
                        if (!any_behaviors)
                        {
                            add_behavior(camera_to_aoe_attacker());
                            add_behavior(aoe_wait_for_setup_ready());
                            add_behavior(setup_aoe());
                            add_behavior(check_aoe_talk());
                            add_behavior(wait_behavior(10));
                            add_behavior(open_aoe_hud());
                            add_behavior(wait_behavior(12));
                            add_behavior(end_aoe_phase_1());
                        }
                        apply_behaviors();
                        break;
                    #endregion
                    #region 2: Combat Processing
                    case 2:
                        switch (Combat_Action)
                        {
                            case 0:
                                bool attack_cont = false;
                                while (!attack_cont)
                                {
                                    Battler_2_Id = Aoe_Targets[Attack_Id];
                                    Battler_2 = Units[Battler_2_Id];
                                    attack_cont = true;
                                    bool no_attack = false;
                                    Game_Unit battler_1 = null, battler_2 = null;
                                    if (Map_Combat_Data.Data[Attack_Id].Key.Attacker == 1)
                                    {
                                        battler_1 = Battler_1;
                                        battler_2 = Battler_2;
                                    }
                                    else if (Map_Combat_Data.Data[Attack_Id].Key.Attacker == 2)
                                    {
                                        battler_1 = Battler_2;
                                        battler_2 = Battler_1;
                                    }
                                    else
                                    {
                                        no_attack = true;
                                        Combat_Action = 1;
                                    }
                                    if (!no_attack)
                                    {
                                        bool next_attack;
                                        if (In_Staff_Use)
                                            next_attack = staff_use(battler_1, battler_2);
                                        else
                                            next_attack = attack(battler_1, battler_2);
                                        Battler_1.update_attack_graphics();
                                        Battler_2.update_attack_graphics();
                                        if (next_attack)
                                        {
                                            Combat_Timer = 0;
                                            Attack_Id++;
                                            if (Attack_Id >= Map_Combat_Data.Data.Count)
                                            {
                                                // Reset stats for battle ending
                                                for (int i = 0; i < Map_Combat_Data.Data[Map_Combat_Data.Data.Count - 1].Value.Count; i++)
                                                {
                                                    Combat_Action_Data data = Map_Combat_Data.Data[Map_Combat_Data.Data.Count - 1].Value[i];
                                                    if (data.Trigger == (int)Combat_Action_Triggers.End)
                                                    {
                                                        get_scene_map().set_hud_action_id(i);
                                                        get_scene_map().refresh_hud();
                                                    }
                                                }
                                                Combat_Action = 1;
                                                Map_Combat_Data.apply_combat();
                                                Attack_Id = 0;
                                                Aoe_Battlers = new List<int> { battler_1_id };
                                                Aoe_Battlers.AddRange(Aoe_Targets);
                                                Battler_2_Id = -1;
                                            }
                                            else
                                            {
                                                set_attack_index(Attack_Id);
                                                Battler_2_Id = Aoe_Targets[Attack_Id];
                                                attack_cont = false;
                                                refresh_stats(Battler_2.id);
                                                Skip_Attack_Anim = true;
                                            }
                                        }
                                    }
                                }
                                break;
                            // Post Battle Cleanup
                            case 1:
                                Game_Unit battler = null;
                                if (Aoe_Battlers.Count > 0)
                                    battler = Units[Aoe_Battlers[0]];
                                switch (Combat_Timer)
                                {
                                    // Does death quotes if needed
                                    case 0:
                                        if (battler.is_dead)
                                        {
                                            Global.player.force_loc(battler.loc);
                                            get_scene_map().clear_combat();
                                            Global.game_temp.dying_unit_id = battler.id;
                                            if (Global.game_state.get_death_quote(Global.game_temp.dying_unit_id).Length > 0)
                                            {
                                                Global.game_temp.message_text = Global.death_quotes[Global.game_state.get_death_quote(Global.game_temp.dying_unit_id)];
                                                Global.scene.new_message_window();
                                                if (Battler_1.is_opposition ^ Battler_1_Id == battler.id)
                                                    Global.scene.message_reverse();
                                            }
                                            cont = true;
                                        }
                                        else
                                            cont = false;
                                        Combat_Timer++;
                                        break;
                                    // Waits for death quote
                                    case 1:
                                        if (!scene_map.is_message_window_active && !Scrolling)
                                        {
                                            if (battler.is_dead)
                                            {
                                                Global.game_map.add_dying_unit_animation(battler);
                                                Dying = true;
                                            }
                                            battler.actor.staff_fix();
                                            cont = false;
                                            Combat_Timer++;
                                        }
                                        break;
                                    // Waits for dead guys to disappear, then clears HP window
                                    case 2:
                                        if (Dying)
                                        {
                                            battler.update_attack_graphics();
                                            if (!battler.changing_opacity())
                                            {
                                                if (battler.is_dead)
                                                    battler.kill();
                                                Dying = false;
                                                Aoe_Battlers.RemoveAt(0);
                                                if (Aoe_Battlers.Count > 0)
                                                    Combat_Timer = 0;
                                                else
                                                {
                                                    Aoe_Battlers.Clear();
                                                    Combat_Timer++;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Aoe_Battlers.RemoveAt(0);
                                            if (Aoe_Battlers.Count > 0)
                                                Combat_Timer = 0;
                                            else
                                            {
                                                Aoe_Battlers.Clear();
                                                scene_map.clear_combat();
                                                Combat_Timer++;
                                            }
                                        }
                                        break;
                                    // Sets up exp window
                                    case 3:
                                        exp_gauge_gain = 0;
                                        int exp_id = 0;
                                        // Exp gain
                                        if (Battler_1.is_ally && !Battler_1.is_dead && Battler_1.actor.can_level())
                                        {
                                            exp_id = 1;
                                            exp_gauge_gain = Map_Combat_Data.Exp_Gain1;
                                        }
                                        if (exp_gauge_gain > 0)
                                        {
                                            get_scene_map().create_exp_gauge(exp_id == 1 ? Map_Combat_Data.Exp1 : Map_Combat_Data.Exp2);
                                            Combat_Timer++;
                                        }
                                        else
                                        {
                                            Combat_Action = 2;
                                            Combat_Timer = 0;
                                        }
                                        break;
                                    // Waits
                                    case 4:
                                    case 5:
                                    case 6:
                                    case 7:
                                    case 8:
                                    case 9:
                                    case 10:
                                    case 11:
                                    case 12:
                                    case 13:
                                    case 14:
                                    case 15:
                                    case 16:
                                    case 17:
                                    case 18:
                                    case 19:
                                    case 20:
                                    case 21:
                                    case 22:
                                    case 23:
                                    case 24:
                                    case 25:
                                    case 26:
                                    case 27:
                                    case 28:
                                    case 29:
                                    case 30:
                                        Combat_Timer++;
                                        break;
                                    // Unit gains exp
                                    case 31:
                                        if (Global.game_state.process_exp_gain()) //Combat_Timer++; //Debug
                                        {
                                            Global.game_state.cancel_exp_sound();
                                            Global.game_system.cancel_sound();
                                            Combat_Timer++;
                                        }
                                        break;
                                    // Waits
                                    case 32:
                                    case 33:
                                    case 34:
                                    case 35:
                                    case 36:
                                    case 37:
                                    case 38:
                                    case 39:
                                    case 40:
                                    case 41:
                                    case 42:
                                    case 43:
                                    case 44:
                                    case 45:
                                    case 46:
                                    case 47:
                                    case 48:
                                    case 49:
                                    case 50:
                                        Combat_Timer++;
                                        break;
                                    // Clears exp window, continues
                                    case 51:
                                        get_scene_map().clear_exp();
                                        Combat_Action = 2;
                                        Combat_Timer = 0;
                                        break;
                                    default:
                                        Combat_Timer++;
                                        break;
                                }
                                break;
                            // Discern Cleanup Actions
                            case 2:
                                switch (Combat_Timer)
                                {
                                    case 27:
                                        // If Promotion
                                        // If Level Up
                                        if (Battler_1.actor.needed_levels > 0)
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Level_Up, 1 });
                                        // If WLvl Up
                                        if (Battler_1.actor.wlvl_up())
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.WLvl_Up, 1 });
                                        // If Weapon Break
                                        if (Map_Combat_Data.weapon_1_broke)
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Weapon_Break, 1 });
                                        // If Item Gain
                                        bool attacker_killed = Battler_1.is_dead;
                                        if (Map_Combat_Data.has_item_drop)
                                        {
                                            if (attacker_killed)
                                                for (int i = 0; i < Aoe_Targets.Count; i++)
                                                {
                                                    Battler_2 = Units[Aoe_Targets[i]];
                                                    if (!Battler_2.is_dead && Battler_1.drops_item)
                                                    {
                                                        Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Item_Gain, 2, -i });
                                                        break;
                                                    }
                                                }
                                            else
                                                for (int i = 0; i < Aoe_Targets.Count; i++)
                                                {
                                                    Battler_2 = Units[Aoe_Targets[i]];
                                                    if (Battler_2.is_dead && Battler_2.drops_item)
                                                        Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Item_Gain, 1, i });
                                                }
                                        }
                                        cont = false;
                                        Combat_Timer = 0;
                                        Combat_Action = 3;
                                        break;
                                    default:
                                        Combat_Timer++;
                                        break;
                                }
                                break;
                            // Act on Cleanup Actions
                            case 3:
                                if (Cleanup_Action.Count == 0)
                                {
                                    cont = false;
                                    Combat_Timer = 0;
                                    Combat_Action = 0;
                                    Combat_Phase = 3;
                                }
                                else
                                {
                                    cont = update_aoe_cleanup_actions(Battler_1);
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region 3: Post Battle
                    case 3:
                        switch (Combat_Timer)
                        {
                            case 0:
                                Battler_1.end_battle();
                                foreach (int battler_2_id in Aoe_Targets)
                                    Units[battler_2_id].end_battle();
                                //end battle calls used to be here, testing below to let Trample lead to attack canto //Debug
                                // Move this up from below, because it needs to kill the target before testing if canto is possible
                                Battler_1.queue_move_range_update();
                                foreach (int battler_2_id in Aoe_Targets)
                                    Units[battler_2_id].queue_move_range_update();
                                Combat_Timer++;
                                break;
                            case 1:
                                // Attack Canto
                                if (Battler_1.has_attack_canto() && !Battler_1.full_move() && !Battler_1.is_dead)
                                {
                                    Battler_1.cantoing = true;
                                    if (Battler_1.is_active_player_team && !Battler_1.berserk) //Multi
                                    {
                                        Global.player.loc = Battler_1.loc;
                                        Global.player.instant_move = true;
                                        Global.game_system.Selected_Unit_Id = Battler_1_Id;
                                        Battler_1.open_move_range();
                                    }
                                }
                                else
                                    Battler_1.start_wait(false);
                                Battler_1.battling = false;
                                foreach (int battler_2_id in Aoe_Targets)
                                    Units[battler_2_id].battling = false;
                                get_scene_map().update_map_sprite_status(Battler_1_Id);
                                foreach (int battler_2_id in Aoe_Targets)
                                    get_scene_map().update_map_sprite_status(battler_2_id);
                                Battler_1_Id = -1;
                                Aoe_Targets.Clear();
                                Weapon1 = null;
                                Map_Combat_Data = null;
                                Attack_Id = -1;

                                refresh_move_ranges();
                                Combat_Timer++;
                                break;
                            case 2:
                                if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                                {
                                    Combat_Phase = 4;
                                    Combat_Timer = 0;
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Allows suspend to complete, when post-battle suspends are used
                    default:
                        end_battle();
                        In_Aoe = false;
                        break;
                    #endregion
                }
                if (!In_Battle || scene_map.suspend_calling) break;
            }
        }

        private void set_attack_index(int index)
        {
            if (In_Staff_Use)
                (Map_Combat_Data as Aoe_Staff_Data).set_attack(index);
            else
                (Map_Combat_Data as Aoe_Data).set_attack(index);
        }

        #region Behaviors
        private IEnumerable<bool> camera_to_aoe_attacker()
        {
            Global.player.force_loc(Units[Global.game_system.Battler_1_Id].loc);
            Global.game_state.combat_events(true, In_Staff_Use);

            yield return false;
        }
        private IEnumerable<bool> aoe_wait_for_setup_ready()
        {
            while ((Global.game_system.is_interpreter_running || No_Input_Timer > 0 || Global.scene.is_message_window_active) || !Global.player.is_on_square)
                yield return false;

            yield return false;
        }
        private IEnumerable<bool> setup_aoe()
        {
            while (Scrolling)
                yield return false;

            Battler_1_Id = Global.game_system.Battler_1_Id;
            Game_Unit battler_1 = Units[Battler_1_Id];
            Game_Unit battler_2;
            Aoe_Targets = Global.game_system.Aoe_Targets;
            set_aoe_data(Battler_1_Id, Aoe_Targets);
            Attack_Id = 0;
            Global.game_system.Battler_1_Id = -1;
            Global.game_system.Aoe_Targets = new List<int>();
            // Turns map sprites toward each other
            if (In_Staff_Use)
            {
                battler_1.sprite_moving = false;
                battler_1.frame = 0;
                battler_1.facing = 6;
            }
            else
            {
                // this facing should already be set to get the aiming right // actually this is more confusing //Yeti
                battler_1.face(Units[Aoe_Targets[0]]);
                battler_1.frame = 0;
                foreach (int battler_2_id in Aoe_Targets)
                {
                    battler_2 = Units[battler_2_id];
                    battler_2.face(battler_1);
                    battler_2.frame = 0;
                }
            }

            yield return false;
        }
        private IEnumerable<bool> check_aoe_talk()
        {
            
            /*if (!Global.scene.is_message_window_active) //Debug
            {
                if (!get_scene_map().check_talk(Battler_1, Units[Aoe_Targets[Attack_Id]], !Battler_1.is_attackable_team(Config.PLAYER_TEAM)))
                    cont = false;
                Attack_Id++;
                if (Attack_Id == Aoe_Targets.Count)
                {
                    Attack_Id = 0;
                    Combat_Timer++;
                }
            }*/
            for (; ; )
            {
                Game_Unit battler_1 = null;
                if (Battler_1_Id > -1)
                    battler_1 = Units[Battler_1_Id];

                if (!Global.scene.is_message_window_active)
                {
                    bool cont = true;
                    if (!get_scene_map().check_talk(
                            battler_1,
                            Units[Aoe_Targets[Attack_Id]],
                            battler_1.is_player_allied))
                        cont = false;
                    Attack_Id++;

                    if (Attack_Id < Aoe_Targets.Count)
                        yield return cont;
                    else
                    {
                        Attack_Id = 0;
                        break;
                    }
                }
            }
            yield return false;
        }
        private IEnumerable<bool> open_aoe_hud()
        {
            get_scene_map().create_hud(Map_Combat_Data);
            set_attack_index(Attack_Id);
            update_hud_stats();
            yield return false;
        }
        private IEnumerable<bool> end_aoe_phase_1()
        {
            Game_Unit battler_1 = null;
            if (Battler_1_Id > -1)
                battler_1 = Units[Battler_1_Id];
            Combat_Phase = 2;
            Weapon1 = battler_1.actor.weapon;
            Battle_Action.Clear();
            yield break;
        }
        #endregion

        protected bool update_aoe_cleanup_actions(Game_Unit Battler_1)
        {
            switch (Cleanup_Action[0][0])
            {
                case (int)Cleanup_Actions.Item_Gain:
                    switch (Combat_Timer)
                    {
                        case 0:
                            Combat_Timer++;
                            Game_Unit item_receiver = Cleanup_Action[0][1] == 1 ? Battler_1 : Units[Aoe_Targets[Cleanup_Action[0][2] * -1]];
                            Game_Unit item_giver = Cleanup_Action[0][1] == 1 ? Units[Aoe_Targets[Cleanup_Action[0][2]]] : Battler_1;
                            if (item_receiver.can_acquire_drops)
                            {
                                Global.game_system.play_se(System_Sounds.Gain);
                                TactileLibrary.Item_Data item_data = item_giver.actor.drop_item();
                                if (Constants.Gameplay.REPAIR_DROPPED_ITEM)
                                    item_data.repair();
                                item_receiver.actor.gain_item(item_data);
                                get_scene_map().set_item_popup(item_data, 113);
                                if (item_receiver.actor.too_many_items)
                                {
                                    Global.game_temp.menu_call = true;
                                    Global.game_temp.discard_menu_call = true;
                                    Global.game_system.Discarder_Id = item_receiver.id;
                                }
                            }
                            else
                            {
                                Combat_Timer = 0;
                                Cleanup_Action.RemoveAt(0);
                                return false;
                            }
                            break;
                        case 1:
                            if (!Global.game_temp.menu_call && !Global.game_state.is_menuing && !get_scene_map().is_map_popup_active())
                                Combat_Timer++;
                            break;
                        case 21:
                            Cleanup_Action.RemoveAt(0);
                            Combat_Timer = 0;
                            break;
                        default:
                            Combat_Timer++;
                            break;
                    }
                    return true;
            }
            return update_cleanup_actions(Battler_1, null);
        }

        protected void setup_aoe_battle()
        {
            In_Battle = true;
            In_Staff_Use = Staff_Calling;
            In_Aoe = true;
            Aoe_Calling = false;
            Battle_Calling = false;
            Staff_Calling = false;
            Global.game_system.Battle_Mode = Constants.Animation_Modes.Map;

            Skip_Battle = Global.game_state.skip_ai_turn_activating;
            // Check if should stop skipping AI turn
            if (Skip_Battle)
            {
                // Check for talk events
                foreach (int target_id in Global.game_system.Aoe_Targets)
                    if (get_scene_map().check_talk(Units[Global.game_system.Battler_1_Id], Units[target_id], false, true))
                    {
                        switch_out_of_ai_skip(this, new EventArgs());
                        Skip_Battle = false;
                        break;
                    }
                if (Skip_Battle)
                {
                    // Check for death quotes etc
                    Global.game_system.save_rns();
                    set_aoe_data(Global.game_system.Battler_1_Id, Global.game_system.Aoe_Targets);
                    var combat_data = Map_Combat_Data;
                    if (combat_data.is_ally_killed || combat_data.has_death_quote ||
                        combat_data.has_item_drop || combat_data.has_promotion ||
                        (Units.ContainsKey(Global.game_system.Battler_2_Id) && get_scene_map().check_talk(
                        Units[Global.game_system.Battler_1_Id], Units[Global.game_system.Battler_2_Id], false, true)))
                    {
                        switch_out_of_ai_skip(this, new EventArgs());
                        Skip_Battle = false;
                    }
                    Global.game_system.readd_saved_rns();
                }
                if (Skip_Battle)
                {
                    int test = 0;
                }
            }

            Map_Combat_Data = null;
        }

        protected void set_aoe_data(int battler_1_id, List<int> aoe_targets)
        {
            if (!In_Staff_Use || !Map_Battle)
            {
                Units[battler_1_id].battling = true;
                foreach (int battler_2_id in aoe_targets)
                {
                    Units[battler_2_id].battling = true;
                    Combat.battle_setup(battler_1_id, battler_2_id, Global.game_map.unit_distance(battler_1_id, battler_2_id), 0);
                }
            }

            foreach (int battler_2_id in aoe_targets)
                // Still needed at the moment to init magic attacks
                Combat.battle_setup(
                    battler_1_id, battler_2_id,
                    Global.game_map.unit_distance(battler_1_id, battler_2_id),
                    In_Staff_Use ? 1 : 0);

            if (In_Staff_Use)
                Map_Combat_Data = new Aoe_Staff_Data(battler_1_id, aoe_targets);
            else
                Map_Combat_Data = new Aoe_Data(battler_1_id, aoe_targets);
        }
    }
}
