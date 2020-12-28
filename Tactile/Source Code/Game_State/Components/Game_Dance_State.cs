using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using TactileVersionExtension;

namespace Tactile.State
{
    class Game_Dance_State : Game_Combat_State_Component
    {
        protected bool Dance_Calling = false;
        protected bool In_Dance = false;
        protected int Dance_Phase = 0;
        protected int Dance_Action = 0;
        protected int Dance_Timer = 0;
        protected int Dancer_Id = -1;
        protected int Dance_Target_Id = -1;
        protected int Dance_Item = -1;

        protected bool Dance_Animated = false;

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            base.write(writer);
            writer.Write(In_Dance);
            writer.Write(Dance_Phase);
            writer.Write(Dance_Action);
            writer.Write(Dance_Timer);
            writer.Write(Dancer_Id);
            writer.Write(Dance_Target_Id);
            writer.Write(Dance_Item);

            writer.Write(Map_Battle);
        }

        internal override void read(BinaryReader reader)
        {
            base.read(reader);
            In_Dance = reader.ReadBoolean();
            Dance_Phase = reader.ReadInt32();
            Dance_Action = reader.ReadInt32();
            Dance_Timer = reader.ReadInt32();
            Dancer_Id = reader.ReadInt32();
            Dance_Target_Id = reader.ReadInt32();
            Dance_Item = reader.ReadInt32();

            if (!Global.LOADED_VERSION.older_than(0, 5, 6, 2)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                Map_Battle = reader.ReadBoolean();
            }
        }
        #endregion

        #region Accessors
        public bool dance_calling
        {
            get { return Dance_Calling; }
            set { Dance_Calling = value; }
        }

        public bool in_dance { get { return In_Dance; } }

        public int dancer_id { get { return Dancer_Id; } }
        public int dance_target_id { get { return Dance_Target_Id; } }
        public int dance_item { get { return Dance_Item; } }

        protected Game_Unit dancer { get { return Dancer_Id == -1 ? null : Units[Dancer_Id]; } }
        protected Game_Unit dance_target { get { return Dance_Target_Id == -1 ? null : Units[Dance_Target_Id]; } }
        #endregion

        internal override void update()
        {
            if (Dance_Calling)
            {
                setup_dance();
            }
            if (In_Dance && !Global.game_state.switching_ai_skip && get_scene_map() != null)
            {
                if (Skip_Battle)
                {
                    update_skipped_dance();
                }
                else if (Map_Battle)
                {
                    update_map_dance();
                }
                else
                {
                    update_scene_dance();
                }
            }
        }

        protected void setup_dance()
        {
            In_Dance = true;
            Dance_Calling = false;
            Skip_Battle = Global.game_state.skip_ai_turn_activating;

            set_animation_mode(Global.game_system.Battler_1_Id, Global.game_system.Battler_2_Id, false, null);
            Map_Battle = Global.game_system.Battle_Mode == Constants.Animation_Modes.Map;
        }

        protected void update_map_dance()
        {
            Scene_Map scene_map = get_scene_map();
            if (scene_map == null)
                return;
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Dance_Phase)
                {
                    #region Setup
                    case 0:
                        switch (Dance_Timer)
                        {
                            case 0:
                                Global.scene.suspend();
                                Dancer_Id = Global.game_system.Battler_1_Id;
                                Dance_Target_Id = Global.game_system.Battler_2_Id;
                                Dance_Item = Global.game_system.Dance_Item;
                                Global.game_system.Battler_1_Id = -1;
                                Global.game_system.Battler_2_Id = -1;
                                Global.game_system.Dance_Item = -1;
                                Dance_Timer++;
                                break;
                            case 1:
                            case 2:
                            case 3:
                                if (Global.player.is_on_square)
                                    Dance_Timer++;
                                break;
                            case 4:
                                if (!Scrolling)
                                {
                                    dancer.battling = true;
                                    // Turns map sprites toward each other
                                    dancer.face(dance_target);
                                    dancer.frame = 0;
                                    if (Map_Animations.unit_data(1, dancer.actor.class_id) != null)
                                    {
                                        Dance_Animated = true;
                                        scene_map.set_map_animation(Dancer_Id, 1, dancer.actor.class_id);
                                    }
                                    Dance_Phase++;
                                    Dance_Action = 0;
                                    Dance_Timer = 0;
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Animation
                    case 1:
                        if (Dance_Animated)
                        {
                            if (scene_map.map_animation_finished(Dancer_Id))
                            {
                                Dance_Phase++;
                                Dance_Action = 0;
                                Dance_Timer = 0;
                            }
                        }
                        else
                        {
                            switch (Dance_Action)
                            {
                                // Waits
                                case 0:
                                    switch (Dance_Timer)
                                    {
                                        case 23:
                                            Dance_Action++;
                                            Dance_Timer = 0;
                                            break;
                                        default:
                                            Dance_Timer++;
                                            break;
                                    }
                                    break;
                                // Dancer moves toward the target //Yeti
                                case 1:
                                    dancer.attack_move(dance_target);
                                    Dance_Action++;
                                    break;
                                // Waits until dancer has moved //Yeti
                                case 2:
                                    if (!dancer.is_attack_moving())
                                        Dance_Action++;
                                    break;
                                // Dancer animates
                                case 3:
                                    Global.Audio.play_se("Map Sounds", "Map_Step_FDragon");
                                    dancer.wiggle();
                                    Dance_Action++;
                                    break;
                                // Waits
                                case 4:
                                    if (!dancer.is_wiggle_moving())
                                    {
                                        dancer.attack_move(dance_target, true);
                                        Dance_Action++;
                                    }
                                    break;
                                // Waits
                                case 5:
                                    if (!dancer.is_attack_moving())
                                        Dance_Action++;
                                    break;
                                // Waits
                                case 6:
                                    switch (Dance_Timer)
                                    {
                                        case 47:
                                            Dance_Phase++;
                                            Dance_Action = 0;
                                            Dance_Timer = 0;
                                            break;
                                        default:
                                            Dance_Timer++;
                                            break;
                                    }
                                    break;
                            }
                        dancer.update_attack_graphics();
                        }
                        break;
                    #endregion
                    #region Ring Animation
                    case 2:
                        switch (Dance_Action)
                        {
                            case 0:
                                if (Dance_Item > -1)
                                {
                                    scene_map.set_map_effect(dance_target.loc, 0,
                                        Map_Animations.item_effect_id(dancer.items[Dance_Item].Id));
                                    Dance_Action++;
                                }
                                else
                                {
                                    Dance_Phase++;
                                    cont = false;
                                }
                                break;
                            case 1:
                                {
                                    if (!scene_map.is_map_effect_active())
                                        Dance_Action++;
                                    break;
                                }
                            case 2:
                                switch (Dance_Timer)
                                {
                                    case 32:
                                        Dance_Phase++;
                                        Dance_Action = 0;
                                        Dance_Timer = 0;
                                        cont = false;
                                        break;
                                    default:
                                        Dance_Timer++;
                                        break;
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Dance processing
                    case 3:
                        dancer.apply_dance(dance_target, Dance_Item);
                        scene_map.update_map_sprite_status(Dance_Target_Id);
                        Dance_Phase++;
                        break;
                    #endregion
                    #region Exp gain
                    case 4:
                        switch (Dance_Action)
                        {
                            case 0:
                                switch (Dance_Timer)
                                {
                                    case 0:
                                        if (dancer.allowed_to_gain_exp() && dancer.actor.can_level())
                                        {
                                            int exp = dancer.actor.exp;
                                            int exp_gain = dancer.dance_exp();
                                            if (exp_gain > 0 || dancer.actor.needed_levels > 0)
                                            {
                                                scene_map.create_exp_gauge(exp);
                                                exp_gauge_gain = exp_gain;
                                            }
                                            else
                                                Dance_Timer = 47;
                                            Dance_Timer++;
                                        }
                                        else
                                            Dance_Phase++;
                                        break;
                                    case 27:
                                        if (Global.game_state.process_exp_gain())
                                        {
                                            Global.game_system.cancel_sound();
                                            Dance_Timer++;
                                        }
                                        break;
                                    // Clears exp window, continues
                                    case 47:
                                        scene_map.clear_exp();
                                        Dance_Timer++;
                                        break;
                                    case 78:
                                        if (dancer.actor.needed_levels > 0)
                                            Dance_Action++;
                                        else
                                        {
                                            Dance_Phase++;
                                            Dance_Action = 0;
                                        }
                                        Dance_Timer = 0;
                                        break;
                                    default:
                                        Dance_Timer++;
                                        break;
                                }
                                break;
                            // Level up
                            case 1:
                                switch (Dance_Timer)
                                {
                                    case 0:
                                        scene_map.level_up(Dancer_Id);
                                        Dance_Timer++;
                                        break;
                                    case 1:
                                        if (!scene_map.is_leveling_up())
                                            Dance_Timer++;
                                        break;
                                    case 31:
                                        Dance_Phase++;
                                        Dance_Action = 0;
                                        Dance_Timer = 0;
                                        break;
                                    default:
                                        Dance_Timer++;
                                        break;
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region End
                    default:
                        switch (Dance_Timer)
                        {
                            case 0:
                                dance_cleanup();
                                break;
                            case 1:
                                if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                                {
                                    Dance_Timer++;
                                }
                                break;
                            case 2:
                                Dance_Calling = false;
                                In_Dance = false;
                                Dance_Phase = 0;
                                Dance_Action = 0;
                                Dance_Timer = 0;
                                Dancer_Id = -1;
                                Dance_Target_Id = -1;
                                Dance_Item = -1;
                                highlight_test();
                                Global.game_state.any_trigger_events();
                                break;
                        }
                        break;
                    #endregion
                }
            }
        }

        protected void update_scene_dance()
        {
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Dance_Phase)
                {
                    #region Setup
                    case 0:
                        switch (Dance_Timer)
                        {
                            case 0:
                                Skipping_Battle_Scene = false;

                                Global.scene.suspend();
                                Dancer_Id = Global.game_system.Battler_1_Id;
                                Dance_Target_Id = Global.game_system.Battler_2_Id;
                                Dance_Item = Global.game_system.Dance_Item;
                                Global.game_system.Battler_1_Id = -1;
                                Global.game_system.Battler_2_Id = -1;
                                Global.game_system.Dance_Item = -1;

                                dancer.preload_animations(combat_distance(Dancer_Id, Dance_Target_Id));
                                dance_target.preload_animations(combat_distance(Dancer_Id, Dance_Target_Id));
                                Dance_Timer++;
                                break;
                            case 1:
                            case 2:
                            case 3:
                                if (Global.player.is_on_square)
                                    Dance_Timer++;
                                break;
                            case 4:
                                if (!Scrolling)
                                {
                                    Transition_To_Battle = true;
                                    Battle_Transition_Timer = Global.BattleSceneConfig.BattleTransitionTime;
                                    dancer.battling = true;
                                    // Turns map sprites toward each other
                                    dancer.face(dance_target);
                                    dancer.frame = 0;
                                    Dance_Timer++;
                                }
                                break;
                            case 5:
                                if (!Global.game_state.is_menuing)
                                {
                                    if (Battle_Transition_Timer > 0) Battle_Transition_Timer--;
                                    if (Battle_Transition_Timer == 0)
                                        Dance_Timer++;
                                }
                                break;
                            case 6:
                                int distance = combat_distance(Dancer_Id, Dance_Target_Id);
                                Global.scene_change("Scene_Dance");
                                Global.battle_scene_distance = distance;
                                Dance_Phase++;
                                Dance_Action = 0;
                                Dance_Timer = 0;
                                break;
                        }
                        break;
                    #endregion
                    #region Dance ends
                    case 1:
                        if (get_scene_map() != null)
                        {
                            Dance_Phase++;
                        }
                        break;
                    #endregion
                    #region Map brightens, units reappear
                    case 2:
                        switch (Dance_Timer)
                        {
                            case 1:
                                if (!Global.game_state.is_menuing) // if shop does exist, previous step needs to skip the transition return
                                {
                                    Transition_To_Battle = false;
                                    Battle_Transition_Timer = Global.BattleSceneConfig.BattleTransitionTime;
                                    Dance_Timer++;
                                }
                                break;
                            case 2:
                                if (Battle_Transition_Timer > 0) Battle_Transition_Timer--;
                                if (Battle_Transition_Timer == 0)
                                {
                                    // If scene was skipped, jump to map battle processing
                                    if (Skipping_Battle_Scene)
                                    {
                                        Skipping_Battle_Scene = false;
                                        Global.game_system.Battle_Mode = Constants.Animation_Modes.Map;
                                        Map_Battle = true;
                                        Dance_Phase = 3;
                                        Dance_Action = 0;
                                    }
                                    else
                                        Dance_Phase++;
                                    Dance_Timer = 0;
                                }
                                break;
                            default:
                                Dance_Timer++;
                                break;
                        }
                        break;
                    #endregion
                    #region Dance processing
                    case 3:
                        dancer.apply_dance(dance_target, Dance_Item);
                        get_scene_map().update_map_sprite_status(Dance_Target_Id);
                        Dance_Phase++;
                        break;
                    #endregion
                    #region End
                    default:
                        switch (Dance_Timer)
                        {
                            case 0:
                                dance_cleanup();
                                break;
                            case 1:
                                if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                                {
                                    Dance_Timer++;
                                }
                                break;
                            case 2:
                                Dance_Calling = false;
                                In_Dance = false;
                                Dance_Phase = 0;
                                Dance_Action = 0;
                                Dance_Timer = 0;
                                Dancer_Id = -1;
                                Dance_Target_Id = -1;
                                Dance_Item = -1;
                                highlight_test();
                                Global.game_state.any_trigger_events();
                                break;
                        }
                        break;
                    #endregion
                }
            }
        }

        protected void update_skipped_dance()
        {
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Dance_Phase)
                {
                    #region Setup
                    case 0:
                        switch (Dance_Timer)
                        {
                            case 0:
                                //Global.scene.suspend(); // Because skipped
                                Dancer_Id = Global.game_system.Battler_1_Id;
                                Dance_Target_Id = Global.game_system.Battler_2_Id;
                                Dance_Item = Global.game_system.Dance_Item;
                                Global.game_system.Battler_1_Id = -1;
                                Global.game_system.Battler_2_Id = -1;
                                Global.game_system.Dance_Item = -1;
                                Dance_Timer++;
                                cont = false;
                                break;
                            case 1:
                            case 2:
                            case 3:
                                if (Global.player.is_on_square)
                                {
                                    Dance_Timer++;
                                    cont = false;
                                }
                                break;
                            case 4:
                                dancer.battling = true;
                                // Turns map sprites toward each other
                                dancer.face(dance_target);
                                dancer.frame = 0;
                                Dance_Phase++;
                                Dance_Action = 0;
                                Dance_Timer = 0;
                                cont = false;
                                break;
                        }
                        break;
                    #endregion
                    #region Animation
                    case 1:
                        Dance_Phase++;
                        cont = false;
                        break;
                    #endregion
                    #region Dance processing
                    case 2:
                        dancer.apply_dance(dance_target, Dance_Item);
                        get_scene_map().update_map_sprite_status(Dance_Target_Id);
                        Dance_Phase++;
                        cont = false;
                        break;
                    #endregion
                    #region Exp gain
                    case 3:
                        switch (Dance_Action)
                        {
                            case 0:
                                switch (Dance_Timer)
                                {
                                    case 0:
                                        if (dancer.allowed_to_gain_exp() && dancer.actor.can_level())
                                        {
                                            dancer.dance_exp();
                                            if (dancer.actor.needed_levels > 0)
                                                dancer.actor.level_up();
                                        }
                                        Dance_Phase++;
                                        cont = false;
                                        break;
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region End
                    default:
                        switch (Dance_Timer)
                        {
                            case 0:
                                dance_cleanup();
                                cont = false;
                                break;
                            case 1:
                                if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                                {
                                    Dance_Timer++;
                                }
                                break;
                            case 2:
                                Dance_Calling = false;
                                In_Dance = false;
                                Dance_Phase = 0;
                                Dance_Action = 0;
                                Dance_Timer = 0;
                                Dancer_Id = -1;
                                Dance_Target_Id = -1;
                                Dance_Item = -1;
                                highlight_test();
                                Global.game_state.any_trigger_events();
                                break;
                        }
                        break;
                    #endregion
                }
            }
        }

        private void dance_cleanup()
        {
            if (!dance_target.ready && (Dance_Item == -1 || Constants.Combat.RING_REFRESH))
            {
                dance_target.refresh_unit();
                // If this unit is on an AI team and was refreshed, it needs to be re-added to the list of AI units doing things
                Global.game_state.refresh_ai_unit(dance_target.id);
            }
            if (dancer.cantoing && dancer.is_active_player_team) //Multi
            {
                Global.player.loc = dancer.loc;
                Global.player.instant_move = true;
                dancer.open_move_range();
            }
            else
                dancer.start_wait(false);
            dancer.battling = false;
            dancer.queue_move_range_update();
            dance_target.queue_move_range_update();
            refresh_move_ranges();
            Dance_Timer++;
        }
    }
}
