using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Tactile.State
{
    class Game_Steal_State : Game_State_Component
    {
        protected bool Steal_Calling = false;
        protected bool In_Steal = false;
        protected int Steal_Phase = 0;
        protected int Steal_Action = 0;
        protected int Steal_Timer = 0;
        protected int Stealer_Id = -1;
        protected int Steal_Target_Id = -1;
        protected int Stolen_Item = -1;

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            writer.Write(In_Steal);
            writer.Write(Steal_Phase);
            writer.Write(Steal_Action);
            writer.Write(Steal_Timer);
            writer.Write(Stealer_Id);
            writer.Write(Steal_Target_Id);
            writer.Write(Stolen_Item);
        }

        internal override void read(BinaryReader reader)
        {
            In_Steal = reader.ReadBoolean();
            Steal_Phase = reader.ReadInt32();
            Steal_Action = reader.ReadInt32();
            Steal_Timer = reader.ReadInt32();
            Stealer_Id = reader.ReadInt32();
            Steal_Target_Id = reader.ReadInt32();
            Stolen_Item = reader.ReadInt32();
        }
        #endregion

        #region Accessors
        public bool steal_calling
        {
            get { return Steal_Calling; }
            set { Steal_Calling = value; }
        }

        public bool in_steal { get { return In_Steal; } }

        public int stealer_id { get { return Stealer_Id; } }

        public int steal_target_id { get { return Steal_Target_Id; } }

        public Game_Unit stealer { get { return Stealer_Id == -1 ? null : Units[Stealer_Id]; } }
        public Game_Unit steal_target { get { return Steal_Target_Id == -1 ? null : Units[Steal_Target_Id]; } }
        #endregion

        internal override void update()
        {
            if (Steal_Calling)
            {
                In_Steal = true;
                Steal_Calling = false;
            }
            if (In_Steal && get_scene_map() != null)
            {
                bool cont = false;
                while (!cont)
                {
                    cont = true;
                    switch (Steal_Phase)
                    {
                        case 0:
                            switch (Steal_Timer)
                            {
                                case 0:
                                    if (Global.game_state.is_player_turn)
                                        Global.scene.suspend();
                                    Stealer_Id = Global.game_system.Battler_1_Id;
                                    Steal_Target_Id = Global.game_system.Battler_2_Id;
                                    Stolen_Item = Global.game_system.Stolen_Item;
                                    Global.game_system.Battler_1_Id = -1;
                                    Global.game_system.Battler_2_Id = -1;
                                    Global.game_system.Stolen_Item = -1;
                                    Steal_Timer++;
                                    break;
                                case 1:
                                case 2:
                                case 3:
                                    // Wait until support gain hearts are done
                                    if (Global.game_state.support_gain_active)
                                        break;

                                    if (Global.player.is_on_square)
                                        Steal_Timer++;
                                    break;
                                case 4:
                                    if (!Scrolling)
                                    {
                                        stealer.battling = true;
                                        steal_target.battling = true;
                                        // Turns map sprites toward each other
                                        stealer.face(steal_target);
                                        stealer.frame = 0;
                                        steal_target.facing = 10 - stealer.facing;
                                        steal_target.frame = 0;
                                        Steal_Phase = 1;
                                        Steal_Action = 0;
                                        Steal_Timer = 0;
                                    }
                                    break;
                            }
                            break;
                        // Animation
                        case 1:
                            switch (Steal_Action)
                            {
                                // Waits
                                case 0:
                                    switch (Steal_Timer)
                                    {
                                        case 23:
                                            Steal_Action++;
                                            Steal_Timer = 0;
                                            break;
                                        default:
                                            Steal_Timer++;
                                            break;
                                    }
                                    break;
                                // Stealer moves toward the target
                                case 1:
                                    stealer.attack_move(steal_target);
                                    Steal_Action++;
                                    break;
                                // Waits until stealer has moved
                                case 2:
                                    if (!stealer.is_attack_moving())
                                        Steal_Action++;
                                    break;
                                // Stealer animates
                                case 3:
                                    Global.Audio.play_se("Map Sounds", "Map_Step_FDragon");
                                    stealer.wiggle();
                                    Steal_Action++;
                                    break;
                                // Waits
                                case 4:
                                    if (!stealer.is_wiggle_moving())
                                    {
                                        stealer.attack_move(steal_target, true);
                                        Steal_Action++;
                                    }
                                    break;
                                // Waits
                                case 5:
                                    if (!stealer.is_attack_moving())
                                        Steal_Action++;
                                    break;
                                // Waits
                                case 6:
                                    switch (Steal_Timer)
                                    {
                                        case 47:
                                            Steal_Phase++;
                                            Steal_Action = 0;
                                            Steal_Timer = 0;
                                            break;
                                        default:
                                            Steal_Timer++;
                                            break;
                                    }
                                    break;
                            }
                            stealer.update_attack_graphics();
                            break;
                        // Steal processing
                        case 2:
                            switch (Steal_Action)
                            {
                                case 0:
                                    if (Constants.Gameplay.CANCEL_GREEN_DROP_ON_ANY_STEAL)
                                        steal_target.drops_item = false; // which way are we going on this //Debug
                                    else
                                    {
                                        // If the stolen item is the green drop, cancels green drop status
                                        if (Stolen_Item == steal_target.actor.num_items - 1)
                                            steal_target.drops_item = false;
                                    }
                                    TactileLibrary.Item_Data stolen_item = steal_target.actor.drop_item(Stolen_Item);
                                    stealer.actor.gain_item(stolen_item);
                                    if (!stealer.is_player_allied)
                                    {
                                        Global.game_system.play_se(System_Sounds.Loss);
                                        get_scene_map().set_item_stolen_popup(stolen_item, 113);
                                    }
                                    else
                                    {
                                        Global.game_system.play_se(System_Sounds.Gain);
                                        get_scene_map().set_item_steal_popup(stolen_item, 113);
                                    }
                                    if (stealer.actor.too_many_items && stealer.is_active_player_team) //Multi
                                    {
                                        Global.game_temp.menu_call = true;
                                        Global.game_temp.discard_menu_call = true;
                                        Global.game_system.Discarder_Id = stealer.id;
                                    }
                                    Steal_Action++;
                                    break;
                                case 1:
                                    if (!Global.game_temp.menu_call && !Global.game_state.is_menuing && !get_scene_map().is_map_popup_active())
                                        Steal_Action++;
                                    break;
                                case 2:
                                    switch (Steal_Timer)
                                    {
                                        case 18:
                                            stealer.actor.staff_fix();
                                            Steal_Phase++;
                                            Steal_Action = 0;
                                            Steal_Timer = 0;
                                            break;
                                        default:
                                            Steal_Timer++;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        // Exp gain
                        case 3:
                            switch (Steal_Action)
                            {
                                case 0:
                                    switch (Steal_Timer)
                                    {
                                        case 0:
                                            if (stealer.allowed_to_gain_exp() && stealer.actor.can_level())
                                            {
                                                int exp = (Constants.Combat.STEAL_EXP > 0 ?
                                                    Math.Min(Constants.Combat.STEAL_EXP, stealer.actor.exp_gain_possible()) :
                                                    Math.Max(Constants.Combat.STEAL_EXP, -stealer.actor.exp_loss_possible()));
                                                exp = steal_target.cap_exp_given(exp);
                                                steal_target.add_exp_given(exp);

                                                if (exp != 0)
                                                {
                                                    Global.game_state.exp_gauge_gain = exp;
                                                    Global.game_system.chapter_exp_gain += exp;
                                                    get_scene_map().create_exp_gauge(stealer.actor.exp);
                                                    stealer.actor.exp += exp;
                                                    Steal_Timer++;
                                                }
                                                else
                                                    Steal_Phase++;
                                            }
                                            else
                                                Steal_Phase++;
                                            break;
                                        case 27:
                                            if (Global.game_state.process_exp_gain())
                                            {
                                                Global.game_system.cancel_sound();
                                                Steal_Timer++;
                                            }
                                            break;
                                        // Clears exp window, continues
                                        case 47:
                                            get_scene_map().clear_exp();
                                            Steal_Timer++;
                                            break;
                                        case 78:
                                            if (stealer.actor.needed_levels > 0)
                                                Steal_Action++;
                                            else
                                            {
                                                Steal_Phase++;
                                                Steal_Action = 0;
                                            }
                                            Steal_Timer = 0;
                                            break;
                                        default:
                                            Steal_Timer++;
                                            break;
                                    }
                                    break;
                                // Level up
                                case 1:
                                    switch (Steal_Timer)
                                    {
                                        case 0:
                                            get_scene_map().level_up(Stealer_Id);
                                            Steal_Timer++;
                                            break;
                                        case 1:
                                            if (!get_scene_map().is_leveling_up())
                                                Steal_Timer++;
                                            break;
                                        case 31:
                                            Steal_Phase++;
                                            Steal_Action = 0;
                                            Steal_Timer = 0;
                                            break;
                                        default:
                                            Steal_Timer++;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        // End
                        default:
                            switch (Steal_Timer)
                            {
                                case 0:
                                    if (stealer.cantoing && stealer.is_active_player_team) //Multi
                                    {
                                        Global.player.loc = stealer.loc;
                                        Global.player.instant_move = true;
                                        stealer.open_move_range();
                                    }
                                    else
                                        stealer.start_wait(false);
                                    stealer.battling = false;
                                    steal_target.battling = false;
                                    stealer.queue_move_range_update();
                                    steal_target.queue_move_range_update();
                                    refresh_move_ranges();
                                    Steal_Timer++;
                                    break;
                                case 1:
                                    if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                                    {
                                        Steal_Timer++;
                                    }
                                    break;
                                case 2:
                                    Steal_Calling = false;
                                    In_Steal = false;
                                    Steal_Phase = 0;
                                    Steal_Action = 0;
                                    Steal_Timer = 0;
                                    Stealer_Id = -1;
                                    Steal_Target_Id = -1;
                                    Stolen_Item = -1;
                                    highlight_test();
                                    Global.game_state.any_trigger_events();
                                    break;
                            }
                            break;
                    }
                }
            }
        }
    }
}
