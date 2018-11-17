using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace FEXNA.State
{
    class Game_Sacrifice_State : Game_Combat_State_Component
    {
        protected bool Sacrifice_Calling = false;
        protected bool In_Sacrifice = false;
        protected int Sacrifice_Phase = 0;
        protected int Sacrifice_Action = 0;
        protected int Sacrifice_Timer = 0;
        protected int Sacrificer_Id = -1;
        protected int Sacrifice_Target_Id = -1;

        protected bool Sacrifice_Animated = false;

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            base.write(writer);
            writer.Write(In_Sacrifice);
            writer.Write(Sacrifice_Phase);
            writer.Write(Sacrifice_Action);
            writer.Write(Sacrifice_Timer);
            writer.Write(Sacrificer_Id);
            writer.Write(Sacrifice_Target_Id);
        }

        internal override void read(BinaryReader reader)
        {
            base.read(reader);
            In_Sacrifice = reader.ReadBoolean();
            Sacrifice_Phase = reader.ReadInt32();
            Sacrifice_Action = reader.ReadInt32();
            Sacrifice_Timer = reader.ReadInt32();
            Sacrificer_Id = reader.ReadInt32();
            Sacrifice_Target_Id = reader.ReadInt32();
        }
        #endregion

        #region Accessors
        public bool sacrifice_calling
        {
            get { return Sacrifice_Calling; }
            set { Sacrifice_Calling = value; }
        }

        public bool in_sacrifice { get { return In_Sacrifice; } }

        public int sacrificer_id { get { return Sacrificer_Id; } }
        public int sacrifice_target_id { get { return Sacrifice_Target_Id; } }

        protected Game_Unit sacrificer { get { return Sacrificer_Id == -1 ? null : Units[Sacrificer_Id]; } }
        protected Game_Unit sacrifice_target { get { return Sacrifice_Target_Id == -1 ? null : Units[Sacrifice_Target_Id]; } }
        #endregion

        internal override void update()
        {
            if (Sacrifice_Calling)
            {
                setup_sacrifice();
            }
            if (In_Sacrifice && !Global.game_state.switching_ai_skip && get_scene_map() != null)
            {
                if (Skip_Battle)
                {
                    update_skipped_sacrifice();
                }
                else
                {
                    update_map_sacrifice();
                }
            }
        }

        protected void setup_sacrifice()
        {
            In_Sacrifice = true;
            Sacrifice_Calling = false;
            Skip_Battle = Global.game_state.skip_ai_turn_activating;

            set_animation_mode(Global.game_system.Battler_1_Id, Global.game_system.Battler_2_Id, false, null);
            Map_Battle = Global.game_system.Battle_Mode == Constants.Animation_Modes.Map;
        }

        protected void update_map_sacrifice()
        {
            Scene_Map scene_map = get_scene_map();
            if (scene_map == null)
                return;
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Sacrifice_Phase)
                {
                    #region Setup
                    case 0:
                        switch (Sacrifice_Timer)
                        {
                            case 0:
                                Global.scene.suspend();
                                Sacrificer_Id = Global.game_system.Battler_1_Id;
                                Sacrifice_Target_Id = Global.game_system.Battler_2_Id;
                                Global.game_system.Battler_1_Id = -1;
                                Global.game_system.Battler_2_Id = -1;
                                Sacrifice_Timer++;
                                break;
                            case 1:
                            case 2:
                            case 3:
                                if (Global.player.is_on_square)
                                    Sacrifice_Timer++;
                                break;
                            case 4:
                                if (!Scrolling)
                                {
                                    get_scene_map().create_hud(Sacrificer_Id, Sacrifice_Target_Id);
                                    if (Map_Animations.unit_data(1, sacrificer.actor.class_id) != null)
                                    {
                                        sacrificer.battling = true;
                                        // Turns map sprites toward each other
                                        sacrificer.face(sacrifice_target);
                                        sacrificer.frame = 0;
                                        Sacrifice_Animated = true;
                                    }
                                    else
                                    {
                                        sacrificer.sprite_moving = false;
                                        sacrificer.frame = 0;
                                        sacrificer.facing = 6;// 2;
                                    }
                                    Sacrifice_Timer++;
                                }
                                break;
                            case 5:
                                if (!Sacrifice_Animated)
                                    sacrificer.facing = 6;
                                Sacrifice_Timer++;
                                break;
                            case 27:
                                if (Sacrifice_Animated)
                                    scene_map.set_map_animation(Sacrificer_Id, 1, sacrificer.actor.class_id);
                                Sacrifice_Phase++;
                                Sacrifice_Action = 0;
                                Sacrifice_Timer = 0;
                                break;
                            default:
                                Sacrifice_Timer++;
                                break;
                        }
                        break;
                    #endregion
                    #region Animation
                    case 1:
                        if (Sacrifice_Animated)
                        {
                            if (scene_map.map_animation_finished(Sacrificer_Id))
                            {
                                Sacrifice_Phase++;
                                Sacrifice_Action = 0;
                                Sacrifice_Timer = 0;
                            }
                        }
                        else
                        {
                            switch (Sacrifice_Timer)
                            {
                                case 0:
                                    sacrificer.frame = 1;
                                    Sacrifice_Timer++;
                                    break;
                                case 4:
                                    sacrificer.frame = 2;
                                    Sacrifice_Timer++;
                                    break;
                                case 10:
                                    Sacrifice_Phase++;
                                    Sacrifice_Action = 0;
                                    Sacrifice_Timer = 0;
                                    break;
                                default:
                                    Sacrifice_Timer++;
                                    break;
                            }
                        }
                        break;
                    #endregion
                    #region Heal Animation
                    case 2:
                        switch (Sacrifice_Action)
                        {
                            case 0:
                                // shouldn't be hardcoded //Yeti
                                scene_map.set_map_effect(sacrifice_target.loc, 0,
                                    Map_Animations.item_effect_id(1));
                                Sacrifice_Action++;
                                break;
                            case 1:
                                switch (Sacrifice_Timer)
                                {
                                    case 30:
                                        int heal = sacrificer.sacrifice_heal_amount(sacrifice_target);
                                        sacrificer.actor.hp -= heal;
                                        sacrifice_target.actor.hp += heal;

                                        Sacrifice_Action++;
                                        Sacrifice_Timer = 0;
                                        break;
                                    default:
                                        Sacrifice_Timer++;
                                        break;
                                }
                                break;
                            case 2:
                                {
                                    switch (Sacrifice_Timer)
                                    {
                                        case 0:
                                            if (!scene_map.is_map_effect_active())
                                                if (get_scene_map().combat_hud_ready())
                                                {
                                                    //if (Sacrifice_Animated)
                                                    //{
                                                    //    Sacrifice_Action++;
                                                    //    Sacrifice_Timer = 0;
                                                    //}
                                                    //else
                                                        Sacrifice_Timer++;
                                                }
                                            break;
                                        case 26:
                                            if (!Sacrifice_Animated)
                                                sacrificer.frame = 1;
                                            Sacrifice_Timer++;
                                            break;
                                        case 34:
                                            if (!Sacrifice_Animated)
                                                sacrificer.frame = 0;
                                            Sacrifice_Timer++;
                                            break;
                                        case 46:
                                            Sacrifice_Action++;
                                            Sacrifice_Timer = 0;
                                            break;
                                        default:
                                            Sacrifice_Timer++;
                                            break;
                                    }
                                    break;
                                }
                            case 3:
                                switch (Sacrifice_Timer)
                                {
                                    case 0: //32:
                                        if (!Sacrifice_Animated)
                                        {
                                            sacrificer.sprite_moving = true;
                                            sacrificer.selection_facing();
                                            sacrificer.battling = true;
                                            sacrificer.frame = 0;
                                        }

                                        get_scene_map().clear_combat();
                                        Sacrifice_Phase++;
                                        Sacrifice_Action = 0;
                                        Sacrifice_Timer = 0;
                                        cont = false;
                                        break;
                                    default:
                                        Sacrifice_Timer++;
                                        break;
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Sacrifice processing
                    case 3:
                        scene_map.update_map_sprite_status(Sacrifice_Target_Id);
                        Sacrifice_Phase++;
                        break;
                    #endregion
                    #region Exp gain
                    case 4:
                        switch (Sacrifice_Action)
                        {
                            case 0:
                                switch (Sacrifice_Timer)
                                {
                                    case 0:
                                        // Don't give exp on sacrifice if the target also has sacrifice, or they can infinite loop
                                        if (!sacrifice_target.actor.has_skill("SACRIFICE") &&
                                            sacrificer.is_ally && sacrificer.actor.can_level())
                                        {
                                            //int exp_gain = 10; //Debug
                                            int exp_gain = Combat.exp(sacrificer, sacrifice_target);
                                            // shouldn't be hardcoded //Yeti
                                            exp_gauge_gain = (exp_gain > 0 ?
                                                Math.Min(exp_gain, sacrificer.actor.exp_gain_possible()) :
                                                Math.Max(exp_gain, -sacrificer.actor.exp_loss_possible()));
                                            get_scene_map().create_exp_gauge(sacrificer.actor.exp);
                                            sacrificer.actor.exp += exp_gain;
                                            Sacrifice_Timer++;
                                        }
                                        else
                                            Sacrifice_Phase++;
                                        break;
                                    case 27:
                                        if (Global.game_state.process_exp_gain())
                                        {
                                            Global.game_system.cancel_sound();
                                            Sacrifice_Timer++;
                                        }
                                        break;
                                    // Clears exp window, continues
                                    case 47:
                                        scene_map.clear_exp();
                                        Sacrifice_Timer++;
                                        break;
                                    case 78:
                                        if (sacrificer.actor.needed_levels > 0)
                                            Sacrifice_Action++;
                                        else
                                        {
                                            Sacrifice_Phase++;
                                            Sacrifice_Action = 0;
                                        }
                                        Sacrifice_Timer = 0;
                                        break;
                                    default:
                                        Sacrifice_Timer++;
                                        break;
                                }
                                break;
                            // Level up
                            case 1:
                                switch (Sacrifice_Timer)
                                {
                                    case 0:
                                        scene_map.level_up(Sacrificer_Id);
                                        Sacrifice_Timer++;
                                        break;
                                    case 1:
                                        if (!scene_map.is_leveling_up())
                                            Sacrifice_Timer++;
                                        break;
                                    case 31:
                                        Sacrifice_Phase++;
                                        Sacrifice_Action = 0;
                                        Sacrifice_Timer = 0;
                                        break;
                                    default:
                                        Sacrifice_Timer++;
                                        break;
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region End
                    default:
                        switch (Sacrifice_Timer)
                        {
                            case 0:
                                if (sacrificer.cantoing && sacrificer.is_active_player_team) //Multi
                                {
                                    Global.player.loc = sacrificer.loc;
                                    Global.player.instant_move = true;
                                    sacrificer.open_move_range();
                                }
                                else
                                    sacrificer.start_wait(false);
                                sacrificer.battling = false;
                                sacrificer.queue_move_range_update();
                                sacrifice_target.queue_move_range_update();
                                refresh_move_ranges();
                                Sacrifice_Timer++;
                                break;
                            case 1:
                                if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                                {
                                    Sacrifice_Timer++;
                                }
                                break;
                            case 2:
                                Sacrifice_Calling = false;
                                In_Sacrifice = false;
                                Sacrifice_Phase = 0;
                                Sacrifice_Action = 0;
                                Sacrifice_Timer = 0;
                                Sacrificer_Id = -1;
                                Sacrifice_Target_Id = -1;
                                highlight_test();
                                Global.game_state.any_trigger_events();
                                break;
                        }
                        break;
                    #endregion
                }
            }
        }

        protected void update_skipped_sacrifice()
        {
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Sacrifice_Phase)
                {
                    #region Setup
                    case 0:
                        switch (Sacrifice_Timer)
                        {
                            case 0:
                                //Global.scene.suspend(); // Because skipped
                                Sacrificer_Id = Global.game_system.Battler_1_Id;
                                Sacrifice_Target_Id = Global.game_system.Battler_2_Id;
                                Global.game_system.Battler_1_Id = -1;
                                Global.game_system.Battler_2_Id = -1;
                                Sacrifice_Timer++;
                                cont = false;
                                break;
                            case 1:
                            case 2:
                            case 3:
                                if (Global.player.is_on_square)
                                {
                                    Sacrifice_Timer++;
                                    cont = false;
                                }
                                break;
                            case 4:
                                sacrificer.battling = true;
                                // Turns map sprites toward each other
                                sacrificer.face(sacrifice_target);
                                sacrificer.frame = 0;
                                Sacrifice_Phase++;
                                Sacrifice_Action = 0;
                                Sacrifice_Timer = 0;
                                cont = false;
                                break;
                        }
                        break;
                    #endregion
                    #region Animation
                    case 1:
                        Sacrifice_Phase++;
                        cont = false;
                        break;
                    #endregion
                    #region Sacrifice processing
                    case 2:
                        int heal = sacrificer.sacrifice_heal_amount(sacrifice_target);
                        sacrificer.actor.hp -= heal;
                        sacrifice_target.actor.hp += heal;
                        get_scene_map().update_map_sprite_status(Sacrifice_Target_Id);
                        Sacrifice_Phase++;
                        cont = false;
                        break;
                    #endregion
                    #region Exp gain
                    case 3:
                        switch (Sacrifice_Action)
                        {
                            case 0:
                                switch (Sacrifice_Timer)
                                {
                                    case 0:
                                        if (sacrificer.is_ally && sacrificer.actor.can_level())
                                        {
                                            // shouldn't be hardcoded //Yeti
                                            //int exp_gain = 10; //Debug
                                            int exp_gain = Combat.exp(sacrificer, sacrifice_target);
                                            sacrificer.actor.exp += exp_gain;
                                            if (sacrificer.actor.needed_levels > 0)
                                                sacrificer.actor.level_up();
                                        }
                                        Sacrifice_Phase++;
                                        cont = false;
                                        break;
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region End
                    default:
                        switch (Sacrifice_Timer)
                        {
                            case 0:
                                if (sacrificer.cantoing && sacrificer.is_active_player_team) //Multi
                                {
                                    Global.player.loc = sacrificer.loc;
                                    Global.player.instant_move = true;
                                    sacrificer.open_move_range();
                                }
                                else
                                    sacrificer.start_wait(false);
                                sacrificer.battling = false;
                                sacrificer.queue_move_range_update();
                                sacrifice_target.queue_move_range_update();
                                refresh_move_ranges();
                                Sacrifice_Timer++;
                                cont = false;
                                break;
                            case 1:
                                if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                                {
                                    Sacrifice_Timer++;
                                }
                                break;
                            case 2:
                                Sacrifice_Calling = false;
                                In_Sacrifice = false;
                                Sacrifice_Phase = 0;
                                Sacrifice_Action = 0;
                                Sacrifice_Timer = 0;
                                Sacrificer_Id = -1;
                                Sacrifice_Target_Id = -1;
                                highlight_test();
                                Global.game_state.any_trigger_events();
                                break;
                        }
                        break;
                    #endregion
                }
            }
        }
    }
}
