using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using FEXNA_Library;
using ListExtension;

namespace FEXNA.State
{
    public enum Map_Battle_Actions { Prehit_Check, Hit_Check, Move_Toward_Enemy, Wait_For_Move, Wiggle, Attack,
        Miss, No_Damage, Move_Back, Wait_For_Attack, Wait_After_Attack, Next_Attack }
    public enum Cleanup_Actions { Promotion, Level_Up, Skill_Gain, WLvl_Up, Weapon_Break, Item_Gain, Wait }
    partial class Game_Combat_State : Game_Combat_State_Component
    {
        // Weapons that fire a ballista bolt
        readonly static int[] BALLISTAE = new int[] { 96, 97, 98 };

        protected bool Battle_Calling = false, Staff_Calling = false;
        protected bool In_Battle = false;
        protected bool Battle_Ending = false;
        protected bool Arena = false;
        protected int Combat_Phase = 0;
        protected int Combat_Action = 0;
        protected int Combat_Timer = 0;
        protected List<int> Battle_Action = new List<int>();
        protected List<List<int>> Cleanup_Action = new List<List<int>>();
        protected bool Skip_Attack_Anim = false;

        protected Combat_Data Map_Combat_Data;
        protected int Attack_Id = -1;
        protected int Battler_1_Id = -1, Battler_2_Id = -1;
        protected Vector2 Staff_Target_Loc;
        protected bool Dying = false;
        protected Data_Weapon Weapon1 = null, Weapon2 = null;

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            writer.Write(In_Battle);
            writer.Write(In_Staff_Use);
            writer.Write(Battle_Ending);
            writer.Write(Arena);
            writer.Write(Combat_Phase);
            writer.Write(Combat_Action);
            writer.Write(Combat_Timer);
            Battle_Action.write(writer);
            Cleanup_Action.write(writer);
            writer.Write(Skip_Attack_Anim);

            writer.Write(Map_Battle);

            write_aoe(writer);
        }

        internal override void read(BinaryReader reader)
        {
            In_Battle = reader.ReadBoolean();
            In_Staff_Use = reader.ReadBoolean();
            Battle_Ending = reader.ReadBoolean();
            Arena = reader.ReadBoolean();
            Combat_Phase = reader.ReadInt32();
            Combat_Action = reader.ReadInt32();
            Combat_Timer = reader.ReadInt32();
            Battle_Action.read(reader);
            Cleanup_Action.read(reader);
            Skip_Attack_Anim = reader.ReadBoolean();

            Map_Battle = reader.ReadBoolean();

            read_aoe(reader);
        }
        #endregion

        #region Accessors
        public bool battle_calling
        {
            get { return Battle_Calling; }
            set { Battle_Calling = value; }
        }
        public bool staff_calling
        {
            get { return Staff_Calling; }
            set { Staff_Calling = value; }
        }

        public bool in_battle { get { return In_Battle; } }

        public bool in_staff_use { get { return In_Staff_Use; } }

        public bool battle_ending
        {
            get { return Battle_Ending; }
            set { Battle_Ending = value; }
        }

        public bool arena { get { return Arena; } set { Arena = value; } } //private // on set //Yeti

        public Combat_Data combat_data
        {
            get { return Map_Combat_Data; }
            set { Map_Combat_Data = value; }
        }

        public int battler_1_id { get { return Battler_1_Id; } }
        public int battler_2_id { get { return Battler_2_Id; } }

        private Game_Unit attacker
        {
            get
            {
                if (Battler_1_Id > -1)
                    return Units[Battler_1_Id];
                if (Global.game_system.Battler_1_Id > -1)
                    return Units[Global.game_system.Battler_1_Id];
                return null;
            }
        }
        private Combat_Map_Object combat_target
        {
            get
            {
                if (Battler_2_Id > -1)
                    return attackable_map_object(Battler_2_Id);
                if (Global.game_system.Battler_2_Id > -1)
                    return attackable_map_object(Global.game_system.Battler_2_Id);
                return null;
            }
        }

        internal string battle_theme
        {
            get
            {
                string filename = "";
                if (Global.game_system.In_Arena)
                    filename = Constants.Audio.Bgm.ARENA_BATTLE_THEME;
                else if (Map_Combat_Data != null)
                {
                    // Gets the theme for the current phase
                    filename = Global.data_chapters[Global.game_state.chapter_id].Battle_Themes[Team_Turn];
                    // If battler 1 has a battle theme, switch to that
                    if (Units.ContainsKey(Map_Combat_Data.Battler_1_Id))
                        if (Global.game_state.Unit_Battle_Themes.ContainsKey(Map_Combat_Data.Battler_1_Id))
                        {
                            filename = Global.game_state.Unit_Battle_Themes[Map_Combat_Data.Battler_1_Id];
                            if (Constants.Map.CONTINUE_BOSS_THEME && Units[Map_Combat_Data.Battler_1_Id].boss)
                                Global.game_temp.boss_theme = true;
                        }
                    // If battler 2 has a battle theme, and either battler 1 isn't a boss or battler 2 is a boss and not an enemy
                    if (Map_Combat_Data.Battler_2_Id != null &&
                            (!Units[Map_Combat_Data.Battler_1_Id].boss ||
                            (Units[(int)Map_Combat_Data.Battler_2_Id].boss && !Units[(int)Map_Combat_Data.Battler_2_Id].is_opposition)))
                        if (Units.ContainsKey((int)Map_Combat_Data.Battler_2_Id))
                            if (Global.game_state.Unit_Battle_Themes.ContainsKey((int)Map_Combat_Data.Battler_2_Id))
                            {
                                filename = Global.game_state.Unit_Battle_Themes[(int)Map_Combat_Data.Battler_2_Id];
                                if (Constants.Map.CONTINUE_BOSS_THEME && Units[(int)Map_Combat_Data.Battler_2_Id].boss)
                                    Global.game_temp.boss_theme = true;
                            }

                }
                return filename;
            }
        }
        #endregion

        public Game_Unit enemy_of_dying_unit()
        {
            if (!In_Battle)
            {
                // If an enemy unit is dying outside a battle, return the player lord
                if (Units.ContainsKey(Global.game_temp.dying_unit_id) &&
                        !Units[Global.game_temp.dying_unit_id].is_player_allied &&
                        Global.game_map.allies.Any())
                    // Return the lord, or the closest possible
                    return Units[Global.game_map.highest_priority_unit(Constants.Team.PLAYER_TEAM)];
                else
                    return null;
            }
            if (Map_Combat_Data.kill)
            {
                int id;
                if (In_Aoe)
                {
                    // this doesn't always work maybe //Yeti
                    if (Aoe_Battlers[0] == Battler_1_Id)
                    {
                        id = Aoe_Targets[0];
                        for(int i = 0; i < Aoe_Targets.Count; i++)
                            if (!Units[i].is_dead)
                            {
                                id = i;
                                break;
                            }
                    }
                    else
                        id = Battler_1_Id;
                }
                else
                    id = Map_Combat_Data.surviving_unit_id;
                return Units[id];
            }
            return null;
        }

        internal override void update()
        {
            if (!Aoe_Calling && !In_Aoe)
            {
                if (Battle_Ending && get_scene_map() != null)
                {
                    Battle_Ending = false; // Does this do anything useful anymore //Yeti
                }

                if (Battle_Calling || Staff_Calling)
                    setup_battle();
                if (In_Battle && !Global.game_state.switching_ai_skip &&
                    !Global.player.is_targeting())
                {
                    if (Skip_Battle)
                    {
                        update_skipped_battle();
                    }
                    else if (Map_Battle)
                    {
                        update_map_battle();
                    }
                    else
                    {
                        update_scene_battle();
                    }
                }
            }

            update_aoe();
        }

        #region Map Battle
        protected void update_map_battle()
        {
            Game_Unit Battler_1 = null, Battler_2 = null;
            Combat_Map_Object target = null;
            if (Battler_1_Id > -1) Battler_1 = Units[Battler_1_Id];
            if (Battler_2_Id > -1)
                target = attackable_map_object(battler_2_id);
            bool is_target_unit = false;
            if (target != null && target.is_unit())
            {
                is_target_unit = true;
                Battler_2 = (Game_Unit)target;
            }

            Scene_Map scene_map = get_scene_map();
            if (scene_map == null) return;
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Combat_Phase)
                {
                    case 0:
                        if (!Global.game_temp.scripted_battle)
                            Global.scene.suspend();
                        Combat_Phase++;

                        Combat_Timer++;
                        break;
                    #region 1: Battle Setup
                    case 1:

                        switch (Combat_Timer)
                        {
                            case 1:
                                // Wait until support gain hearts are done
                                if (Global.game_state.support_gain_active)
                                    break;

                                Global.player.force_loc(Units[Global.game_system.Battler_1_Id].loc);
                                // If the battle is scripted it should be handling its own events
                                if (!Global.game_temp.scripted_battle)
                                    Global.game_state.combat_events(true, In_Staff_Use);
                                Combat_Timer++;
                                break;
                            case 2:
                            case 3:
                                if (!Global.game_system.is_battle_interpreter_running &&
                                        (No_Input_Timer <= 0 || Global.game_system.is_interpreter_running) &&
                                        !Global.scene.is_message_window_active)
                                //if (!(Global.game_system.is_battle_interpreter_running || No_Input_Timer > 0 || Global.scene.is_message_window_active)) //Debug
                                    if (Global.player.is_on_square)
                                        Combat_Timer++;
                                break;
                            case 4:
                                if (!Scrolling)
                                {
                                    Battler_1_Id = Global.game_system.Battler_1_Id;
                                    Battler_2_Id = Global.game_system.Battler_2_Id;
                                    Staff_Target_Loc = Global.game_system.Staff_Target_Loc;
                                    Battler_1 = Units[Battler_1_Id];
                                    target = attackable_map_object(battler_2_id);
                                    if (target != null && target.is_unit())
                                    {
                                        is_target_unit = true;
                                        Battler_2 = (Game_Unit)target;
                                    }
                                    set_combat_data(Battler_1_Id, Battler_2_Id);

                                    Attack_Id = 0;
                                    Global.game_system.Battler_1_Id = -1;
                                    Global.game_system.Battler_2_Id = -1;
                                    Global.game_system.Staff_Target_Loc = new Vector2(-1, -1);
                                    // Turns map sprites toward each other
                                    if (In_Staff_Use)
                                    {
                                        Battler_1.sprite_moving = false;
                                        Battler_1.frame = 0;
                                        Battler_1.facing = 6;
                                    }
                                    else
                                    {
                                        Battler_1.face(target);
                                        Battler_1.frame = 0;
                                        if (is_target_unit)
                                        {
                                            Battler_2.facing = 10 - Battler_1.facing;
                                            Battler_2.frame = 0;
                                        }
                                    }
                                    Combat_Timer++;
                                }
                                break;
                            case 5:
                                if (is_target_unit)
                                    get_scene_map().check_talk(Battler_1, Battler_2,
                                        Battler_1.is_player_allied);
                                Combat_Timer++;
                                break;
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
                                if (!Global.scene.is_message_window_active)
                                    Combat_Timer++;
                                break;
                            case 16:
                                if (target != null)
                                    scene_map.create_hud(Map_Combat_Data);
                                update_hud_stats();
                                Combat_Timer++;
                                break;
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
                                Combat_Timer++;
                                break;
                            case 29:
                                Combat_Timer = 0;
                                Combat_Phase = 2;
                                Weapon1 = Battler_1.actor.weapon;
                                if (is_target_unit)
                                    Weapon2 = Battler_2.actor.weapon;
                                Battle_Action.Clear();
                                break;
                        }
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
                                    attack_cont = true;
                                    bool no_attack = false;
                                    Combat_Map_Object battler_1 = null, battler_2 = null;
                                    if (Map_Combat_Data.Data[Attack_Id].Key.Attacker == 1)
                                    {
                                        battler_1 = Battler_1;
                                        battler_2 = target;
                                    }
                                    else if (Map_Combat_Data.Data[Attack_Id].Key.Attacker == 2)
                                    {
                                        battler_1 = target;
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
                                            next_attack = staff_use((Game_Unit)battler_1, (Game_Unit)battler_2);
                                        else
                                            next_attack = attack((Game_Unit)battler_1, battler_2);
                                        Battler_1.update_attack_graphics();
                                        if (is_target_unit)
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
                                            }
                                            else
                                            {
                                                attack_cont = false;
                                                refresh_stats(Battler_2.id);
                                            }
                                        }
                                    }
                                }
                                break;
                            // Post Battle Cleanup
                            case 1:
                                switch (Combat_Timer)
                                {
                                    // Does death quotes if needed
                                    case 0:
                                        Game_Unit dying_unit = null;
                                        if (Map_Combat_Data.Kill == 1) // Battler_1.is_dead) //Debug
                                        {
                                            Global.player.force_loc(Battler_1.loc);
                                            Global.game_temp.dying_unit_id = Battler_1.id;
                                            if (Global.game_state.get_death_quote(Global.game_temp.dying_unit_id).Length > 0)
                                            {
                                                dying_unit = Battler_1;
                                                get_scene_map().clear_combat();
                                                Global.game_temp.message_text = Global.death_quotes[Global.game_state.get_death_quote(Global.game_temp.dying_unit_id)];
                                                Global.scene.new_message_window();
                                                if (!Battler_1.is_opposition)
                                                    Global.scene.message_reverse();
                                            }
                                            cont = true;
                                        }
                                        else if (target != null && Map_Combat_Data.Kill == 2) // target.is_dead) //Debug
                                        {
                                            Global.player.force_loc(target.loc);
                                            //Global.game_temp.dying_unit_id = Battler_2.id; //Debug
                                            Global.game_temp.dying_unit_id = target.id;
                                            if (is_target_unit && Global.game_state.get_death_quote(Global.game_temp.dying_unit_id).Length > 0)
                                            {
                                                dying_unit = Battler_2;
                                                get_scene_map().clear_combat();
                                                Global.game_temp.message_text = Global.death_quotes[Global.game_state.get_death_quote(Global.game_temp.dying_unit_id)];
                                                Global.scene.new_message_window();
                                                if (Battler_1.is_opposition)
                                                    Global.scene.message_reverse();
                                            }
                                            cont = true;
                                        }
                                        else
                                            cont = false;
                                        if (dying_unit != null && dying_unit.is_player_team) //Multi
                                        {
                                            Global.Audio.BgmFadeOut(15);
                                            if (dying_unit.loss_on_death)
                                            {
                                                Global.Audio.PlayBattleTheme(Constants.Audio.Bgm.GAME_OVER_THEME);
                                            }
                                            else
                                            {
                                                Global.Audio.PlayBattleTheme(Constants.Audio.Bgm.ALLY_DEATH_THEME);
                                            }
                                        }

                                        Combat_Timer++;
                                        break;
                                    // Waits for death quote
                                    case 1:
                                        if (!scene_map.is_message_window_active && !Scrolling)
                                        {
                                            if (Map_Combat_Data.Kill == 1) // Battler_1.is_dead) //Debug
                                            {
                                                Global.game_map.add_dying_unit_animation(Battler_1);
                                                Dying = true;
                                            }
                                            if (is_target_unit && Map_Combat_Data.Kill == 2) // target.is_dead) //Debug
                                            {
                                                Global.game_map.add_dying_unit_animation(Battler_2);
                                                Dying = true;
                                            }
                                            Battler_1.actor.staff_fix();
                                            if (is_target_unit)
                                                Battler_2.actor.staff_fix();
                                            cont = false;
                                            Combat_Timer++;
                                        }
                                        break;
                                    // Waits for dead guys to disappear, then clears HP window
                                    case 2:
                                        if (Dying)
                                        {
                                            Battler_1.update_attack_graphics();
                                            Battler_2.update_attack_graphics();
                                            if (!Battler_1.changing_opacity() && !Battler_2.changing_opacity())
                                            {
                                                if (Map_Combat_Data.Kill == 1) // Battler_1.is_dead) //Debug
                                                    Battler_1.kill(!Global.game_temp.scripted_battle);
                                                if (Map_Combat_Data.Kill == 2) // Battler_2.is_dead) //Debug
                                                    Battler_2.kill(!Global.game_temp.scripted_battle);
                                                Dying = false;
                                                scene_map.clear_combat();
                                                Combat_Timer++;
                                            }
                                        }
                                        else
                                        {
                                            scene_map.clear_combat();
                                            Combat_Timer++;
                                        }
                                        break;
                                    // Sets up exp window
                                    case 3:
                                        exp_gauge_gain = 0;
                                        Global.game_state.cancel_exp_sound();
                                        int exp_id = 0;
                                        // Exp gain
                                        if (Map_Combat_Data.Exp_Gain1 != 0)
                                        {
                                            exp_id = 1;
                                            exp_gauge_gain = Map_Combat_Data.Exp_Gain1;
                                        }
                                        else if (target != null && Map_Combat_Data.Exp_Gain2 != 0)
                                        {
                                            exp_id = 2;
                                            exp_gauge_gain = Map_Combat_Data.Exp_Gain2;
                                        }

                                        if (exp_gauge_gain != 0)
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
                                        if (Global.game_state.process_exp_gain())
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
                                        if (Battler_1.actor.needs_promotion)
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Promotion, 1 });
                                        else if (is_target_unit && Battler_2.actor.needs_promotion)
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Promotion, 2 });
                                        // If Level Up
                                        if (Battler_1.actor.needed_levels > 0)
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Level_Up, 1 });
                                        else if (is_target_unit && Battler_2.actor.needed_levels > 0)
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Level_Up, 2 });
                                        // If WLvl Up
                                        if (Battler_1.actor.wlvl_up() && !Battler_1.actor.needs_promotion)
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.WLvl_Up, 1 });
                                        else if (is_target_unit && Battler_2.actor.wlvl_up() && !Battler_2.actor.needs_promotion)
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.WLvl_Up, 2 });
                                        // If Weapon Break
                                        if (Map_Combat_Data.weapon_1_broke)// && !Battler_1.using_siege_engine) //Debug
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Weapon_Break, 1 });
                                        else if (is_target_unit && Map_Combat_Data.weapon_2_broke)// && !Battler_2.using_siege_engine) //Debug
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Weapon_Break, 2 });
                                        // If Item Gain
                                        if (is_target_unit)
                                        {
                                            if (Map_Combat_Data.has_item_drop)
                                            {
                                                if (!Battler_1.is_dead && Battler_2.is_dead)
                                                    Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Item_Gain, 1 });
                                                else if (!Battler_2.is_dead && Battler_1.is_dead)
                                                    Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Item_Gain, 2 });
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
                                if (Cleanup_Action.Any())
                                    cont = update_cleanup_actions(Battler_1, Battler_2);
                                else
                                {
                                    cont = false;
                                    Combat_Timer = 0;
                                    Combat_Action = 0;
                                    Combat_Phase = 3;
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
                                if (target != null && !is_target_unit && Map_Combat_Data.Kill == 2) // target.is_dead) //Debug
                                {
                                    if (target is LightRune)
                                        Global.game_map.remove_light_rune(target.id);
                                    else if (target is Destroyable_Object)
                                        Global.game_map.remove_destroyable(target.id, true);
#if DEBUG
                                    else
                                        throw new InvalidCastException();
#endif
                                }
                                Combat_Timer++;
                                break;
                            case 1:
                                if (!Global.game_system.is_battle_interpreter_running && !Global.scene.is_message_window_active)
                                {
                                    //end battle calls used to be here, testing below to let Trample lead to attack canto //Debug
                                    // Move this up from below, because it needs to kill the target before testing if canto is possible
                                    if (is_target_unit)
                                    {
                                        Battler_1.queue_move_range_update();
                                        Battler_2.queue_move_range_update();
                                    }
                                    else
                                    {
                                        if (target != null && Map_Combat_Data.Kill == 2) // target.is_dead) //Debug
                                        {
                                            if (target is LightRune)
                                                Global.game_map.remove_light_rune(target.id);
                                            else if (target is Destroyable_Object)
                                                Global.game_map.remove_destroyable(target.id);
#if DEBUG
                                            else
                                                throw new InvalidCastException();
#endif
                                        }
                                    }
                                    Combat_Timer++;
                                }
                                break;
                            case 2:
                                if (!Global.game_temp.scripted_battle)
                                {
                                    // Attack Canto
                                    if (Battler_1.has_attack_canto() && Battler_1.can_canto_move() && !Battler_1.full_move() && Map_Combat_Data.Kill != 1) // !Battler_1.is_dead) //Debug
                                    {
                                        Battler_1.cantoing = true;
                                        if (Battler_1.is_active_player_team && !Battler_1.berserk) //Multi
                                        {
                                            Global.player.loc = Battler_1.loc;
                                            Global.player.instant_move = true;
                                            Global.game_system.Selected_Unit_Id = Battler_1_Id;
                                            Battler_1.update_move_range();
                                            Battler_1.open_move_range();
                                        }
                                    }
                                    else
                                        Battler_1.start_wait(false);
                                }
                                Battler_1.end_battle();
                                if (is_target_unit)
                                    Battler_2.end_battle();

                                Battler_1.battling = false;
                                if (is_target_unit)
                                    Battler_2.battling = false;
                                get_scene_map().update_map_sprite_status(Battler_1_Id);
                                if (is_target_unit)
                                    get_scene_map().update_map_sprite_status(Battler_2_Id);
                                Battler_1_Id = -1;
                                Battler_2_Id = -1;
                                Weapon1 = null;
                                Weapon2 = null;
                                Map_Combat_Data = null;
                                Attack_Id = -1;

                                refresh_move_ranges();
                                Combat_Timer++;
                                break;
                            case 3:
                                if (!Global.game_system.is_battle_interpreter_running && !Global.scene.is_message_window_active)
                                {
                                    var selected = Global.game_map.get_selected_unit();
                                    if (selected == null || !selected.ready || !selected.cantoing)
                                    {
                                        Global.game_system.Selected_Unit_Id = -1;
                                    }
                                    Combat_Phase = 4;
                                    Combat_Timer = 0;
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region 4: Allows suspend to complete, when post-battle suspends are used
                    default:
                        if (!Global.game_system.is_battle_interpreter_running)
                            end_battle();
                        break;
                    #endregion
                }
                if (!In_Battle || scene_map.suspend_calling) break;
            }
        }

        protected bool update_cleanup_actions(Game_Unit Battler_1, Game_Unit Battler_2)
        {
            switch (Cleanup_Action[0][0])
            {
                case (int)Cleanup_Actions.Promotion:
                    switch (Combat_Timer)
                    {
                        case 0:
                            Global.Audio.play_se("System Sounds", "Level_Up_Fanfare", duckBgm: true);
                            get_scene_map().create_promotion_spark(
                                Global.game_map.units[Cleanup_Action[0][1] == 1 ? Battler_1_Id : Battler_2_Id].pixel_loc);
                            Combat_Timer++;
                            break;
                        case 1:
                            if (!get_scene_map().levelup_spark_active)
                                Combat_Timer++;
                            break;
                        case 2:
                            Global.game_system.Class_Changer = Cleanup_Action[0][1] == 1 ? Battler_1_Id : Battler_2_Id;
                            Global.game_system.Class_Change_To = (int)(Cleanup_Action[0][1] == 1 ? Battler_1 : Battler_2).actor.promotes_to();
                            //Item_Used = -1; // shouldn't do anything, but... //Debug
                            Transition_To_Battle = true;
                            Battle_Transition_Timer = Constants.BattleScene.BATTLE_TRANSITION_TIME;
                            Combat_Timer++;
                            break;
                        case 3:
                            Battle_Transition_Timer--;
                            if (Battle_Transition_Timer == 0)
                            {
                                Combat_Timer++;
                            }
                            break;
                        case 4:
                            Global.game_system.Battle_Mode = Global.game_options.animation_mode ==
                                (byte)Constants.Animation_Modes.Map ?
                                Constants.Animation_Modes.Map : Constants.Animation_Modes.Full; //Yeti
                            Global.scene_change("Scene_Promotion");
                            Combat_Timer++;
                            break;
                        case 7:
                            Transition_To_Battle = false;
                            Battle_Transition_Timer = Constants.BattleScene.BATTLE_TRANSITION_TIME;
                            Combat_Timer++;
                            break;
                        case 8:
                            if (Battle_Transition_Timer > 0)
                                Battle_Transition_Timer--;
                            if (Battle_Transition_Timer == 0)
                            {
                                // this was handled during promotion, make sure it works here
                                Global.game_system.Class_Changer = -1;

                                Cleanup_Action.RemoveAt(0);
                                Combat_Timer = 0;
                            }
                            break;
                        default:
                            Combat_Timer++;
                            break;
                    }
                    break;
                case (int)Cleanup_Actions.Level_Up:
                    switch (Combat_Timer)
                    {
                        case 0:
                            int battler_id = Cleanup_Action[0][1] == 1 ? Battler_1_Id : Battler_2_Id;
                            get_scene_map().level_up(battler_id);
                            if (Units[battler_id].actor.skills_gained_on_level().Any())
                            {
                                Cleanup_Action.Insert(1, new List<int> { (int)Cleanup_Actions.Skill_Gain, Cleanup_Action[0][1] });
                                get_scene_map().skill_gain(battler_id);
                            }

                            Combat_Timer++;
                            break;
                        case 1:
                            if (!get_scene_map().is_leveling_up())
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
                    break;
                case (int)Cleanup_Actions.Skill_Gain:
                    switch (Combat_Timer)
                    {
                        case 0:
                            if (!get_scene_map().is_skill_gaining())
                                Combat_Timer++;
                            break;
                        case 20:
                            Cleanup_Action.RemoveAt(0);
                            Combat_Timer = 0;
                            break;
                        default:
                            Combat_Timer++;
                            break;
                    }
                    break;
                case (int)Cleanup_Actions.WLvl_Up:
                    switch (Combat_Timer)
                    {
                        case 0:
                            get_scene_map().wlvl_up(Cleanup_Action[0][1] == 1 ? Battler_1_Id : Battler_2_Id);
                            Combat_Timer++;
                            break;
                        case 1:
                            if (!get_scene_map().is_wlvling_up())
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
                    break;
                case (int)Cleanup_Actions.Weapon_Break:
                    switch (Combat_Timer)
                    {
                        case 0:
                            get_scene_map().wbreak(new int[] {
                                0, Cleanup_Action[0][1] == 1 ? Map_Combat_Data.Weapon_1_Id : Map_Combat_Data.Weapon_2_Id });
                            Combat_Timer++;
                            break;
                        case 1:
                            if (!get_scene_map().is_wbreaking())
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
                    break;
                case (int)Cleanup_Actions.Item_Gain:
                    switch (Combat_Timer)
                    {
                        case 0:
                            Combat_Timer++;
                            Game_Unit item_receiver = Cleanup_Action[0][1] == 1 ? Battler_1 : Battler_2;
                            Game_Unit item_giver = Cleanup_Action[0][1] == 1 ? Battler_2 : Battler_1;
                            if (item_receiver.can_acquire_drops)
                            {
                                Global.game_system.play_se(System_Sounds.Gain);
                                Item_Data item_data = item_giver.actor.drop_item();
                                if (item_data != null)
                                {
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
                    break;
                case (int)Cleanup_Actions.Wait:
                    switch (Combat_Timer)
                    {
                        case 8:
                            Cleanup_Action.RemoveAt(0);
                            Combat_Timer = 0;
                            break;
                        default:
                            Combat_Timer++;
                            break;
                    }
                    break;
                default:
                    Cleanup_Action.RemoveAt(0);
                    return false;
            }
            return true;
        }

        protected void cancel_battle()
        {

        }

        protected void refresh_stats(int target_id)
        {
            refresh_stats(target_id, true);
        }
        protected void refresh_stats(int target_id, bool instant)
        {
            update_hud_stats(instant);
        }

        protected void update_hud_stats()
        {
            update_hud_stats(true);
        }
        protected void update_hud_stats(bool instant)
        {
            Scene_Map scene_map = get_scene_map();
            if (scene_map == null) return;
            scene_map.set_hud_attack_id(Attack_Id);
        }

        protected bool continue_attacking()
        {
            return continue_attacking(false);
        }
        protected bool continue_attacking(bool weapon_broke)
        {
            if (Units[Battler_1_Id].continue_attacking())
                return true;
            if (Units.ContainsKey(Battler_2_Id) && Units[Battler_2_Id].continue_attacking())
                return true;

            if (In_Aoe)
                return (!weapon_broke && Map_Combat_Data.Hp1 > 0 && Map_Combat_Data.Hp2 > 0);
            else
                return (!weapon_broke && Map_Combat_Data.Hp1 > 0 && Map_Combat_Data.Hp2 > 0);
        }

        protected bool skip_skill_update()
        {
            return (Units[Battler_1_Id].skip_skill_update() || (Units.ContainsKey(Battler_2_Id) && Units[Battler_2_Id].skip_skill_update()));
        }
        #endregion

        #region Attack
        protected bool attack(Game_Unit battler_1, Combat_Map_Object target)
        {
            Game_Unit battler_2 = null;
            bool is_target_unit = false;
            if (target.is_unit())
            {
                is_target_unit = true;
                battler_2 = (Game_Unit)target;
            }
            Data_Weapon weapon;
            bool weapon_broke;
            int target_id;
            int[] ids;
            if (battler_1.id == Battler_1_Id)
            {
                weapon = Weapon1;
                target_id = target.id;
                weapon_broke = Map_Combat_Data.weapon_1_broke;
                ids = new int[] { 0, 1 };
            }
            else
            {
                weapon = Weapon2;
                target_id = battler_1.id;
                weapon_broke = Map_Combat_Data.weapon_2_broke;
                ids = new int[] { 1, 0 };
            }
            bool cont = false;
            while (!cont)
            {
                cont = true;
                if (Battle_Action.Count == 0)
                {
                    if (continue_attacking())
                    {
                        Battle_Action.Add((int)Map_Battle_Actions.Prehit_Check);
                        Battle_Action.Add((int)Map_Battle_Actions.Hit_Check);
                        if (!Skip_Attack_Anim) Battle_Action.Add((int)Map_Battle_Actions.Move_Toward_Enemy);
                        Battle_Action.Add((int)Map_Battle_Actions.Attack);
                    }
                    else
                        return true;
                }
                else
                {
                    switch (Battle_Action[0])
                    {
                        case (int)Map_Battle_Actions.Prehit_Check:
                            // Defender Prehit Skill Check
                            for (int i = 0; i < Map_Combat_Data.Data[Attack_Id].Value.Count; i++)
                            {
                                Combat_Action_Data data = Map_Combat_Data.Data[Attack_Id].Value[i];
                                if (data.Trigger == (int)Combat_Action_Triggers.Attack && data.Battler_Index == 2)
                                {
                                    get_scene_map().set_hud_action_id(i);
                                    Game_Unit battler = data.Battler_Index == 1 ? battler_1 : battler_2;
                                    foreach (string skill_id in data.Skill_Ids)
                                        battler.actor.activate_battle_skill(skill_id, true);
                                }
                            }
                            Battle_Action.Remove((int)Map_Battle_Actions.Prehit_Check);
                            break;
                        case (int)Map_Battle_Actions.Hit_Check:
                            // Attacker Prehit Skill Check
                            for (int i = 0; i < Map_Combat_Data.Data[Attack_Id].Value.Count; i++)
                            {
                                Combat_Action_Data data = Map_Combat_Data.Data[Attack_Id].Value[i];
                                if (data.Trigger == (int)Combat_Action_Triggers.Attack && data.Battler_Index == 1)
                                {
                                    get_scene_map().set_hud_action_id(i);
                                    Game_Unit battler = data.Battler_Index == 1 ? battler_1 : battler_2;
                                    foreach (string skill_id in data.Skill_Ids)
                                        battler.actor.activate_battle_skill(skill_id);
                                    battler.mastery_hit_confirm(Map_Combat_Data.Data[Attack_Id].Key.Result.hit);
                                }
                            }
                            // Attacker Posthit Skill Check
                            for (int i = 0; i < Map_Combat_Data.Data[Attack_Id].Value.Count; i++)
                            {
                                Combat_Action_Data data = Map_Combat_Data.Data[Attack_Id].Value[i];
                                if (data.Trigger == (int)Combat_Action_Triggers.Skill)
                                {
                                    get_scene_map().set_hud_action_id(i);
                                    Game_Unit battler = data.Battler_Index == 1 ? battler_1 : battler_2;
                                    foreach (string skill_id in data.Skill_Ids)
                                        battler_1.actor.activate_battle_skill(skill_id);
                                }
                            }
                            battler_1.hit_skill_update();
                            if (battler_1.skill_activated)
                                battler_1.skill_map_effect();
                            //refresh_stats(target_id, false); //Debug

                            Battle_Action.Remove((int)Map_Battle_Actions.Hit_Check);
                            break;
                        case (int)Map_Battle_Actions.Move_Toward_Enemy:
                            if (!get_scene_map().is_map_effect_active())
                            {
                                Global.player.instant_move = true;
                                Global.player.loc = battler_1.loc;
                                if (battler_1.trample_activated)
                                {
                                    battler_1.trample_move();
                                }
                                else
                                {
                                    battler_1.attack_move(target);
                                }
                                    Battle_Action.Remove((int)Map_Battle_Actions.Move_Toward_Enemy);
                                    Battle_Action.Insert(0, (int)Map_Battle_Actions.Wait_For_Move);
                            }
                            break;
                        case (int)Map_Battle_Actions.Wait_For_Move:
                            // For trample this should probably check if on one frame the battler is on one side and the next on the other //Yeti
                            // Also should be a called Game_Unit method?
                            if (battler_1.trample_activated ?
                                (battler_1.pixel_loc / (Constants.Map.TILE_SIZE) ==
                                    battler_2.loc || !battler_1.is_in_motion()) :
                                !battler_1.is_attack_moving())
                            {
                                Battle_Action.Remove((int)Map_Battle_Actions.Wait_For_Move);
                                if (!battler_1.trample_activated)
                                    Battle_Action.Insert(0, (int)Map_Battle_Actions.Wiggle);
                            }
                            break;
                        case (int)Map_Battle_Actions.Wiggle:
                            battler_1.wiggle();
                            Battle_Action.Remove((int)Map_Battle_Actions.Wiggle);
                            if (BALLISTAE.Contains(weapon.Id))
                            {
                                get_scene_map().set_ballista_effect(battler_1.loc, target.loc);
                            }
                            break;
                        case (int)Map_Battle_Actions.Attack:
                            Global.player.instant_move = true;
                            Global.player.loc = target.loc;
                            if (get_scene_map().is_map_effect_hit())
                            {
                                // ON-HIT SKILL ACTIVATION GOES HERE (Determination) //Yeti
                                for (int i = 0; i < Map_Combat_Data.Data[Attack_Id].Value.Count; i++)
                                {
                                    Combat_Action_Data data = Map_Combat_Data.Data[Attack_Id].Value[i];
                                    if (data.Trigger == (int)Combat_Action_Triggers.Hit)
                                    {
                                        get_scene_map().set_hud_action_id(i);
                                        Game_Unit battler = data.Battler_Index == 1 ? battler_1 : battler_2;
                                        foreach (string skill_id in data.Skill_Ids)
                                            battler_1.actor.activate_battle_skill(skill_id);
                                    }
                                }
                                map_attack(battler_1, target, is_target_unit, Map_Combat_Data.Data[Attack_Id].Key, weapon);
                                if (!is_target_unit)
                                {
                                    get_scene_map().set_map_effect(target.loc, 4, Map_Combat_Data.Data[Attack_Id].Key.Result.kill ? 2 : 1);
                                }
                                int dmg = Map_Combat_Data.Data[Attack_Id].Key.Result.dmg;
                                // If a crit and damaged, shake the HUD // should it shake on no damage as an indicator? //Yeti
                                if (Map_Combat_Data.Data[Attack_Id].Key.Result.crt && ((dmg > 0) || Map_Combat_Data.Data[Attack_Id].Key.Result.kill))
                                    get_scene_map().hud_crit_shake();
                                Battle_Action.Remove((int)Map_Battle_Actions.Attack);
                                // If miss
                                if (!Map_Combat_Data.Data[Attack_Id].Key.Result.hit)
                                    Battle_Action.Add((int)Map_Battle_Actions.Miss);
                                // Else if no damage
                                else if (dmg == 0 && !Map_Combat_Data.Data[Attack_Id].Key.Result.kill)
                                    Battle_Action.Add((int)Map_Battle_Actions.No_Damage);
                                Battle_Action.Add((int)Map_Battle_Actions.Move_Back);
                            }
                            break;
                        case (int)Map_Battle_Actions.Miss:
                            get_scene_map().create_miss_map_spark(battler_2.pixel_loc);
                            Battle_Action.Remove((int)Map_Battle_Actions.Miss);
                            break;
                        case (int)Map_Battle_Actions.No_Damage:
                            get_scene_map().create_nodamage_map_spark(battler_2.pixel_loc);
                            Battle_Action.Remove((int)Map_Battle_Actions.No_Damage);
                            break;
                        case (int)Map_Battle_Actions.Move_Back:
                            if ((get_scene_map().combat_hud_ready() || continue_attacking(true)) && !battler_1.is_wiggle_moving())
                            {
                                if (!Skip_Attack_Anim)
                                    battler_1.attack_move(target, true);
                                Battle_Action.Remove((int)Map_Battle_Actions.Move_Back);
                                Battle_Action.Add((int)Map_Battle_Actions.Wait_For_Attack);
                            }
                            break;
                        case (int)Map_Battle_Actions.Wait_For_Attack:
                            if (!battler_1.is_attack_moving())
                            {
                                Battle_Action.Remove((int)Map_Battle_Actions.Wait_For_Attack);
                                Battle_Action.Add((int)Map_Battle_Actions.Wait_After_Attack);
                                Combat_Timer = 0;
                            }
                            break;
                        case (int)Map_Battle_Actions.Wait_After_Attack:
                            Combat_Timer++;
                            if (!get_scene_map().is_map_effect_active() && (Combat_Timer > 23 || continue_attacking(true)))
                            {
                                Battle_Action.Remove((int)Map_Battle_Actions.Wait_After_Attack);
                                Battle_Action.Add((int)Map_Battle_Actions.Next_Attack);
                                Combat_Timer = 0;
                                // Status effect animation
                                if (battler_2 != null &&
                                        Map_Combat_Data.Data[Attack_Id].Key.Result.state_change.Count > 0 &&
                                        !is_battler_dead(ids[1]) &&
                                        !skip_skill_update() &&
                                        Map_Combat_Data.Data[Attack_Id].Key.Result.status_inflict_map_id() > 0)
                                    get_scene_map().set_map_effect(battler_2.loc, 3, Map_Combat_Data.Data[Attack_Id].Key.Result.status_inflict_map_id());
                            }
                            break;
                        case (int)Map_Battle_Actions.Next_Attack:
                            if (!get_scene_map().is_map_effect_active())
                            {
                                //# ATTACK END SKILL ACTIVATION GOES HERE (Adept) //Yeti
                                for (int i = 0; i < Map_Combat_Data.Data[Attack_Id].Value.Count; i++)
                                {
                                    Combat_Action_Data data = Map_Combat_Data.Data[Attack_Id].Value[i];
                                    if (data.Trigger == (int)Combat_Action_Triggers.Return)
                                    {
                                        get_scene_map().set_hud_action_id(i);
                                        Game_Unit battler = data.Battler_Index == 1 ? battler_1 : battler_2;
                                        foreach (string skill_id in data.Skill_Ids)
                                            battler_1.actor.activate_battle_skill(skill_id);
                                    }
                                }
                                if (!skip_skill_update())
                                {
                                    battler_1.reset_skills();
                                    if (is_target_unit)
                                        battler_2.reset_skills();
                                }
                                Skip_Attack_Anim = false;
                                Battle_Action.Remove((int)Map_Battle_Actions.Next_Attack);
                                return true;
                            }
                            break;
                    }
                }
            }
            return false;
        }

        protected bool is_battler_dead(int id)
        {
            if (id == 0)
                return Map_Combat_Data.Hp1 <= 0;
            else
                return Map_Combat_Data.Hp2 <= 0;
        }

        protected bool staff_use(Game_Unit battler_1, Game_Unit battler_2)
        {
            switch (Combat_Timer)
            {
                // Staff user animates
                case 0:
                    battler_1.frame = 1;
                    Combat_Timer++;
                    break;
                case 4:
                    battler_1.frame = 2;
                    Combat_Timer++;
                    break;
                case 11:
                    if (!get_scene_map().is_map_effect_active())
                    {
                        get_scene_map().set_map_effect(battler_2 == null ? Staff_Target_Loc : battler_2.loc,
                            1, Map_Animations.weapon_effect_id(Map_Combat_Data.Weapon_1_Id));
                    }
                    if (get_scene_map().is_map_effect_hit())
                    {
                        map_attack_dmg(battler_1, battler_2, Map_Combat_Data.Data[Attack_Id].Key, Weapon1);
                        if (!Map_Combat_Data.Data[Attack_Id].Key.Result.hit && battler_2 != null)
                            get_scene_map().create_miss_map_spark(battler_2.pixel_loc);
                        Combat_Timer++;
                    }
                    break;
                case 12:
                    if (get_scene_map().combat_hud_ready() && !get_scene_map().is_map_effect_active())
                    {
                        Combat_Timer++;
                    }
                    break;
                case 25:
                    battler_1.frame = 1;
                    Combat_Timer++;
                    break;
                case 33:
                    battler_1.frame = 0;
                    Combat_Timer++;
                    break;
                case 57:
                    battler_1.selection_facing();
                    battler_1.sprite_moving = true;
                    battler_1.battling = true;
                    Combat_Timer = 0;
                    return true;
                default:
                    Combat_Timer++;
                    break;
            }
            return false;
        }

        protected void map_attack(Game_Unit battler_1, Combat_Map_Object target, bool is_target_unit, Combat_Round_Data data, Data_Weapon weapon)
        {
            map_attack_dmg(battler_1, is_target_unit ? (Game_Unit)target : null, data, weapon);
            Combat.map_attack(battler_1, target, is_target_unit, data, weapon);
            (data.Result.backfire ? battler_1 : target).hit_rumble(data.Result.dmg <= 0, data.Result.dmg, data.Result.crt);
        }

        protected void map_attack_dmg(Game_Unit battler_1, Game_Unit battler_2, Combat_Round_Data data, Data_Weapon weapon)
        {
            if (battler_1.id == Map_Combat_Data.Battler_1_Id)
            {
                if (!data.Result.backfire)
                {
                    Map_Combat_Data.Hp2 -= data.Result.dmg;
                    Map_Combat_Data.Hp1 = Math.Min(Map_Combat_Data.Hp1 + data.Result.immediate_life_steal, Map_Combat_Data.MaxHp1);
                }
                else
                    Map_Combat_Data.Hp1 = Math.Min(Map_Combat_Data.Hp1 - (data.Result.dmg - data.Result.immediate_life_steal), Map_Combat_Data.MaxHp1);

                if (data.Result.delayed_life_steal && Map_Combat_Data.Hp1 > 0)
                    Map_Combat_Data.Hp1 = Math.Min(Map_Combat_Data.Hp1 + data.Result.delayed_life_steal_amount, Map_Combat_Data.MaxHp1);
                    //Map_Combat_Data.Hp1 += data.Result.delayed_life_steal_amount; //Debug
            }
            else
            {
                if (!data.Result.backfire)
                {
                    Map_Combat_Data.Hp1 -= data.Result.dmg;
                    Map_Combat_Data.Hp2 = Math.Min(Map_Combat_Data.Hp2 + data.Result.immediate_life_steal, Map_Combat_Data.MaxHp2);
                }
                else
                    Map_Combat_Data.Hp2 = Math.Min(Map_Combat_Data.Hp2 - (data.Result.dmg - data.Result.immediate_life_steal), Map_Combat_Data.MaxHp2);

                if (data.Result.delayed_life_steal && Map_Combat_Data.Hp2 > 0)
                    Map_Combat_Data.Hp2 = Math.Min(Map_Combat_Data.Hp2 + data.Result.delayed_life_steal_amount, Map_Combat_Data.MaxHp2);
                    //Map_Combat_Data.Hp2 += data.Result.delayed_life_steal_amount; //Debug
            }
            if (weapon.Torch())
                Global.game_map.add_torch_staff(Staff_Target_Loc);
        }
        #endregion

        protected void update_scene_battle()
        {
            Game_Unit Battler_1 = null, Battler_2 = null;
            if (Battler_1_Id > -1) Battler_1 = Units[Battler_1_Id];
            if (Battler_2_Id > -1) Battler_2 = Units[Battler_2_Id];
            Scene_Map scene_map = get_scene_map();
            Scene_Battle scene_battle = Global.game_map.get_scene_battle();
            if (scene_map == null && scene_battle == null) return;
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Combat_Phase)
                {
                    case 0:
                        Skipping_Battle_Scene = false;

                        if (!Global.game_temp.scripted_battle)
                            Global.scene.suspend();
                        Combat_Phase++;

                        Combat_Timer++;
                        break;
                    #region 1: Battle Setup
                    case 1:
                        switch (Combat_Timer)
                        {
                            case 0:
                                if (!Global.game_temp.scripted_battle)
                                    Global.scene.suspend();
                                Combat_Timer++;
                                break;
                            case 1:
                                // Wait until support gain hearts are done
                                if (Global.game_state.support_gain_active)
                                    break;

                                // If the battle is scripted it should be handling its own events
                                if (!Global.game_temp.scripted_battle)
                                    Global.game_state.combat_events(true, In_Staff_Use);
                                Combat_Timer++;
                                break;
                            case 2:
                            case 3:
                                if (!Global.game_system.is_battle_interpreter_running &&
                                        (No_Input_Timer <= 0 || Global.game_system.is_interpreter_running) &&
                                        !Global.scene.is_message_window_active)
                                    //if (!(Global.game_system.is_battle_interpreter_running || No_Input_Timer > 0 || Global.scene.is_message_window_active)) //Debug
                                    if (Global.player.is_on_square)
                                        Combat_Timer++;
                                break;
                            case 4:
                                if (!Scrolling)
                                {
                                    Transition_To_Battle = true;
                                    Battle_Transition_Timer = Constants.BattleScene.BATTLE_TRANSITION_TIME;
                                    Battler_1_Id = Global.game_system.Battler_1_Id;
                                    Battler_2_Id = Global.game_system.Battler_2_Id;
                                    Battler_1 = Units[Battler_1_Id];
                                    Battler_2 = Units[Battler_2_Id];
                                    set_combat_data(Battler_1_Id, Battler_2_Id);
                                    Attack_Id = 0;
                                    Global.game_system.Battler_1_Id = -1;
                                    Global.game_system.Battler_2_Id = -1;
                                    // Turns map sprites toward each other
                                    Battler_1.face(Battler_2);
                                    Battler_2.facing = 10 - Battler_1.facing;
                                    Battler_1.frame = 0;
                                    Battler_2.frame = 0;

                                    Battler_1.preload_animations(combat_distance(Battler_1_Id, Battler_2_Id));
                                    Battler_2.preload_animations(combat_distance(Battler_1_Id, Battler_2_Id));
                                    Combat_Timer++;
                                }
                                break;
                            case 5:
                                if (!Global.game_state.is_menuing)
                                {
                                    if (Battle_Transition_Timer > 0) Battle_Transition_Timer--;
                                    if (scene_map == null)
                                        Combat_Timer++;
                                    else if (Battle_Transition_Timer == 0)
                                    {
                                        Combat_Timer++;
                                    }
                                }
                                break;
                            case 6:
                                int distance;
                                if (Arena)
                                {
                                    distance = Global.game_system.Arena_Distance;
                                    Global.scene_change("Scene_Arena");
                                }
                                else
                                {
                                    distance = combat_distance(Battler_1_Id, Battler_2_Id);
                                    if (In_Staff_Use)
                                        Global.scene_change("Scene_Staff");
                                    else
                                        Global.scene_change("Scene_Battle");
                                }
                                Global.battle_scene_distance = distance;
                                Combat_Phase = 2;
                                Combat_Timer = 0;
                                break;
                        }
                        break;
                    #endregion
                    #region 2: Combat ends
                    case 2:
                        if (scene_map != null)
                        {
                            Combat_Phase = 3;
                            Global.game_system.Class_Changer = -1;
                            if (Arena)
                            {
                                Units[Global.game_system.Battler_1_Id].battling = false;
                                scene_map.resume_arena();

                                end_battle();
                            }
                        }
                        break;
                    #endregion
                    #region 3: Map brightens, units reappear
                    case 3:
                        switch (Combat_Timer)
                        {
                            case 1:
                                if (!Global.game_state.is_menuing) // if shop does exist, previous step needs to skip the transition return
                                {
                                    Transition_To_Battle = false;
                                    Battle_Transition_Timer = Constants.BattleScene.BATTLE_TRANSITION_TIME;
                                    Combat_Timer++;
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
                                        Combat_Phase = 2;
                                        Combat_Action = 1;
                                        Map_Combat_Data.apply_combat();
                                    }
                                    else
                                        Combat_Phase = 4;
                                    Combat_Timer = 0;
                                }
                                break;
                            default:
                                Combat_Timer++;
                                break;
                        }
                        break;
                    #endregion
                    #region 4: Defeated units clear, popups, end stuff
                    case 4:
                        switch (Combat_Action)
                        {
                            // Post Battle Cleanup
                            case 0:
                                if (Map_Combat_Data.Kill == 1) // Battler_1.is_dead) //Debug
                                {
                                    Global.game_map.add_dying_unit_animation(Battler_1);
                                    Dying = true;
                                }
                                if (Map_Combat_Data.Kill == 2) // Battler_2.is_dead) //Debug
                                {
                                    Global.game_map.add_dying_unit_animation(Battler_2);
                                    Dying = true;
                                }
                                Battler_1.actor.staff_fix();
                                Battler_2.actor.staff_fix();
                                cont = false;
                                Combat_Action++;
                                break;
                            // Waits for dead guys to die
                            case 1:
                                if (Dying)
                                {
                                    Battler_1.update_attack_graphics();
                                    Battler_2.update_attack_graphics();
                                    if (!Battler_1.changing_opacity() && !Battler_2.changing_opacity())
                                    {
                                        if (Map_Combat_Data.Kill == 1) // Battler_1.is_dead) //Debug
                                            Battler_1.kill(!Global.game_temp.scripted_battle);
                                        if (Map_Combat_Data.Kill == 2) // Battler_2.is_dead) //Debug
                                            Battler_2.kill(!Global.game_temp.scripted_battle);
                                        Dying = false;
                                        Combat_Action++;
                                    }
                                }
                                else
                                {
                                    Combat_Action++;
                                }
                                break;
                            // Discern Cleanup Actions
                            case 2:
                                Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Wait, 1 });
                                // If Item Gain
                                if (Map_Combat_Data.has_item_drop)
                                {
                                    if (!Battler_1.is_dead && Battler_2.is_dead)
                                        Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Item_Gain, 1 });
                                    else if (!Battler_2.is_dead && Battler_1.is_dead)
                                        Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Item_Gain, 2 });
                                }
                                cont = false;
                                Combat_Timer = 0;
                                Combat_Action++;
                                break;
                            // Act on Cleanup Actions
                            case 3:
                                if (Cleanup_Action.Count == 0)
                                {
                                    cont = false;
                                    Combat_Timer = 0;
                                    Combat_Action++;
                                }
                                else
                                {
                                    cont = update_cleanup_actions(Battler_1, Battler_2);
                                }
                                break;
                            default:
                                switch (Combat_Timer)
                                {
                                    case 0:
                                        //end battle calls used to be here, testing below to let Trample lead to attack canto //Debug
                                        // Move this up from below, because it needs to kill the target before testing if canto is possible
                                        Battler_1.queue_move_range_update();
                                        Battler_2.queue_move_range_update();
                                        Combat_Timer++;
                                        break;
                                    case 1:
                                        //end battle calls used to be here, testing below to let Trample lead to attack canto //Debug
                                        if (!Global.game_temp.scripted_battle)
                                        {
                                            // Attack Canto
                                            if (Battler_1.has_attack_canto() && Battler_1.can_canto_move() && !Battler_1.full_move() && Map_Combat_Data.Kill != 1) // !Battler_1.is_dead) //Debug
                                            {
                                                Battler_1.cantoing = true;
                                                if (Battler_1.is_active_player_team && !Battler_1.berserk) //Multi
                                                {
                                                    Global.player.loc = Battler_1.loc;
                                                    Global.player.instant_move = true;
                                                    Global.game_system.Selected_Unit_Id = Battler_1_Id;
                                                    Battler_1.update_move_range();
                                                    Battler_1.open_move_range();
                                                }
                                            }
                                            else
                                                Battler_1.start_wait(false);
                                        }
                                        Battler_1.end_battle();
                                        Battler_2.end_battle();

                                        Battler_1.battling = false;
                                        Battler_2.battling = false;
                                        get_scene_map().update_map_sprite_status(Battler_1_Id);
                                        get_scene_map().update_map_sprite_status(Battler_2_Id);
                                        Battler_1_Id = -1;
                                        Battler_2_Id = -1;
                                        Weapon1 = null;
                                        Weapon2 = null;
                                        Map_Combat_Data = null;
                                        Attack_Id = -1;

                                        refresh_move_ranges();
                                        Combat_Timer++;
                                        break;
                                    case 2:
                                        if (!Global.game_system.is_battle_interpreter_running && !Global.scene.is_message_window_active)
                                        {
                                            var selected = Global.game_map.get_selected_unit();
                                            if (selected == null || !selected.ready || !selected.cantoing)
                                            {
                                                Global.game_system.Selected_Unit_Id = -1;
                                            }
                                            Combat_Phase = 5;
                                            Combat_Timer = 0;
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region 5: Allows suspend to complete, when post-battle suspends are used
                    default:
                        end_battle();
                        break;
                    #endregion
                }
                // If the battle is over, or the game is trying to suspend, break from the loop so those things can happen
                if (!In_Battle || scene_map.suspend_calling)
                    break;
            }
        }

        protected void update_skipped_battle()
        {
            Game_Unit Battler_1 = null, Battler_2 = null;
            Combat_Map_Object target = null;
            if (Battler_1_Id > -1) Battler_1 = Units[Battler_1_Id];
            if (Battler_2_Id > -1) target = attackable_map_object(battler_2_id);
            bool is_target_unit = false;
            if (target != null && target.is_unit())
            {
                is_target_unit = true;
                Battler_2 = (Game_Unit)target;
            }

            Scene_Map scene_map = get_scene_map();
            Scene_Battle scene_battle = Global.game_map.get_scene_battle();
            if (scene_map == null && scene_battle == null) return;
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Combat_Phase)
                {
                    case 0:
                        // No suspend for skipped battles
                        Combat_Phase++;
                        break;
                    #region 1: Battle Setup
                    case 1:
                        switch (Combat_Timer)
                        {
                            case 0:
                                Combat_Timer++;
                                break;
                            case 1:
                                // Wait until support gain hearts are done
                                if (Global.game_state.support_gain_active)
                                    break;

                                // If the battle is scripted it should be handling its own events
                                if (!Global.game_temp.scripted_battle)
                                    Global.game_state.combat_events(true, In_Staff_Use);
                                Combat_Timer++;
                                break;
                            case 2:
                                if (!Global.game_system.is_battle_interpreter_running &&
                                    (No_Input_Timer <= 0 || Global.game_system.is_interpreter_running) &&
                                    !Global.scene.is_message_window_active)
                                //if (!(Global.game_system.is_battle_interpreter_running || No_Input_Timer > 0 || Global.scene.is_message_window_active)) //Debug
                                {
                                    Combat_Timer++;
                                    cont = false;
                                }
                                break;
                            case 3:
                                Battler_1_Id = Global.game_system.Battler_1_Id;
                                Battler_2_Id = Global.game_system.Battler_2_Id;
                                Staff_Target_Loc = Global.game_system.Staff_Target_Loc;
                                Battler_1 = Units[Battler_1_Id];
                                target = attackable_map_object(battler_2_id);
                                if (target != null && target.is_unit())
                                {
                                    is_target_unit = true;
                                    Battler_2 = (Game_Unit)target;
                                }
                                set_combat_data(Battler_1_Id, Battler_2_Id);

                                Attack_Id = 0;
                                Global.game_system.Battler_1_Id = -1;
                                Global.game_system.Battler_2_Id = -1;
                                Global.game_system.Staff_Target_Loc = new Vector2(-1, -1);
                                // Turns map sprites toward each other
                                if (In_Staff_Use)
                                {
                                    Battler_1.sprite_moving = false;
                                    Battler_1.frame = 0;
                                    Battler_1.facing = 6;
                                }
                                else
                                {
                                    Battler_1.face(target);
                                    Battler_1.frame = 0;
                                    if (is_target_unit)
                                    {
                                        Battler_2.facing = 10 - Battler_1.facing;
                                        Battler_2.frame = 0;
                                    }
                                }

                                Weapon1 = Battler_1.actor.weapon;
                                if (is_target_unit)
                                    Weapon2 = Battler_2.actor.weapon;

                                Combat_Phase = 2;
                                Combat_Timer = 0;
                                cont = false;
                                break;
                        }
                        break;
                    #endregion
                    #region 2: Combat ends
                    case 2:
                        if (scene_map != null)
                        {
                            while (Attack_Id < Map_Combat_Data.Data.Count)
                            {
                                if (Map_Combat_Data.Data[Attack_Id].Key.Attacker == 1)
                                {
                                    if (Weapon1.Torch())
                                        Global.game_map.add_torch_staff(Staff_Target_Loc);
                                }
                                else if (Map_Combat_Data.Data[Attack_Id].Key.Attacker == 2)
                                {
                                    if (Weapon2.Torch())
                                        Global.game_map.add_torch_staff(Staff_Target_Loc);
                                }
                                Attack_Id++;
                            }

                            Map_Combat_Data.apply_combat(true);
                            Combat_Phase = 3;
                            cont = false;
                        }
                        break;
                    #endregion
                    #region 3: Defeated units clear, popups, end stuff
                    case 3:
                        switch (Combat_Action)
                        {
                            // Post Battle Cleanup
                            case 0:
                                if (Map_Combat_Data.Kill == 1) // Battler_1.is_dead) //Debug
                                    Dying = true;
                                if (is_target_unit && Map_Combat_Data.Kill == 2) // Battler_2.is_dead) //Debug
                                    Dying = true;
                                Battler_1.actor.staff_fix();
                                if (is_target_unit)
                                    Battler_2.actor.staff_fix();
                                cont = false;
                                Combat_Action++;
                                break;
                            // Waits for dead guys to die
                            case 1:
                                if (Dying)
                                {
                                    Battler_1.update_attack_graphics();
                                    Battler_2.update_attack_graphics();
                                    if (!Battler_1.changing_opacity() && !Battler_2.changing_opacity())
                                    {
                                        if (Map_Combat_Data.Kill == 1) // Battler_1.is_dead) //Debug
                                            Battler_1.kill(!Global.game_temp.scripted_battle);
                                        if (Map_Combat_Data.Kill == 2) // Battler_2.is_dead) //Debug
                                            Battler_2.kill(!Global.game_temp.scripted_battle);
                                        Dying = false;
                                        Combat_Action++;
                                        cont = false;
                                    }
                                }
                                else
                                {
                                    Combat_Action++;
                                    cont = false;
                                }
                                break;
                            // Discern Cleanup Actions
                            case 2:
                                // If Item Gain //Yeti
                                // It shouldn't be possible to get here if someone is dropping an item
                                // If Item Gain
                                if (is_target_unit)
                                {
                                    if (Map_Combat_Data.has_item_drop)
                                    {
                                        if (!Battler_1.is_dead && Battler_2.is_dead)
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Item_Gain, 1 });
                                        else if (!Battler_2.is_dead && Battler_1.is_dead)
                                            Cleanup_Action.Add(new List<int> { (int)Cleanup_Actions.Item_Gain, 2 });
                                    }
                                }
                                cont = false;
                                Combat_Timer = 0;
                                Combat_Action++;
                                break;
                            // Act on Cleanup Actions
                            case 3:
                                if (Cleanup_Action.Count == 0)
                                {
                                    cont = false;
                                    Combat_Timer = 0;
                                    Combat_Action++;
                                }
                                else
                                {
                                    cont = update_cleanup_actions(Battler_1, Battler_2);
                                }
                                break;
                            default:
                                switch (Combat_Timer)
                                {
                                    case 0:
                                        if (target != null && !is_target_unit && Map_Combat_Data.Kill == 2) // target.is_dead) //Debug
                                        {
                                            if (target is LightRune)
                                                Global.game_map.remove_light_rune(target.id);
                                            else if (target is Destroyable_Object)
                                                Global.game_map.remove_destroyable(target.id, true);
#if DEBUG
                                            else
                                                throw new InvalidCastException();
#endif
                                        }
                                        Combat_Timer++;
                                        cont = false;
                                        break;
                                    case 1:
                                        if (!Global.game_system.is_battle_interpreter_running && !Global.scene.is_message_window_active)
                                        {
                                            //end battle calls used to be here, testing below to let Trample lead to attack canto //Debug
                                            // Move this up from below, because it needs to kill the target before testing if canto is possible
                                            if (is_target_unit)
                                            {
                                                Battler_1.queue_move_range_update();
                                                Battler_2.queue_move_range_update();
                                            }
                                            else
                                            {
                                                if (target != null && Map_Combat_Data.Kill == 2) // target.is_dead) //Debug
                                                {
                                                    if (target is LightRune)
                                                        Global.game_map.remove_light_rune(target.id);
                                                    else if (target is Destroyable_Object)
                                                        Global.game_map.remove_destroyable(target.id);
#if DEBUG
                                                    else
                                                        throw new InvalidCastException();
#endif
                                                }
                                            }
                                            Combat_Timer++;
                                        }
                                        break;
                                    case 2:
                                        //end battle calls used to be here, testing below to let Trample lead to attack canto //Debug
                                        if (!Global.game_temp.scripted_battle)
                                        {
                                            // Attack Canto
                                            if (Battler_1.has_attack_canto() && Battler_1.can_canto_move() && !Battler_1.full_move() && Map_Combat_Data.Kill != 1) // !Battler_1.is_dead) //Debug
                                            {
                                                Battler_1.cantoing = true;
                                                if (Battler_1.is_active_player_team && !Battler_1.berserk) //Multi
                                                {
                                                    Skip_Battle = false;
                                                    Global.player.loc = Battler_1.loc;
                                                    Global.player.instant_move = true;
                                                    Global.game_system.Selected_Unit_Id = Battler_1_Id;
                                                    Battler_1.update_move_range();
                                                    Battler_1.open_move_range();
                                                }
                                            }
                                            else
                                                Battler_1.start_wait(false);
                                        }
                                        Battler_1.end_battle();
                                        if (is_target_unit)
                                            Battler_2.end_battle();

                                        Battler_1.battling = false;
                                        if (is_target_unit)
                                            Battler_2.battling = false;
                                        get_scene_map().update_map_sprite_status(Battler_1_Id);
                                        if (is_target_unit)
                                            get_scene_map().update_map_sprite_status(Battler_2_Id);
                                        Battler_1_Id = -1;
                                        Battler_2_Id = -1;
                                        Weapon1 = null;
                                        Weapon2 = null;
                                        Map_Combat_Data = null;
                                        Attack_Id = -1;

                                        refresh_move_ranges();
                                        Combat_Timer++;
                                        break;
                                    case 3:
                                        if (!Global.game_system.is_battle_interpreter_running && !Global.scene.is_message_window_active)
                                        {
                                            var selected = Global.game_map.get_selected_unit();
                                            if (selected == null || !selected.ready || !selected.cantoing)
                                            {
                                                Global.game_system.Selected_Unit_Id = -1;
                                            }
                                            Combat_Phase = 4;
                                            Combat_Timer = 0;
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Allows suspend to complete, when post-battle suspends are used
                    default:
                        end_battle();
                        break;
                    #endregion
                }
                if (!In_Battle || scene_map.suspend_calling) break;
            }
        }

        internal event EventHandler switch_out_of_ai_skip;

        protected void end_battle()
        {
            Global.game_system.Battle_Mode = Constants.Animation_Modes.Map;
            Map_Battle = true;
            if (Global.game_system.Battler_1_Id != -1)
            {
                Units[Global.game_system.Battler_1_Id].battling = false;
                Units[Global.game_system.Battler_1_Id].sprite_moving = false;
            }
            if (Global.game_system.Battler_2_Id != -1)
            {
                Units[Global.game_system.Battler_2_Id].battling = false;
                Units[Global.game_system.Battler_2_Id].sprite_moving = false;
            }
            In_Battle = false;
            bool staff = In_Staff_Use;
            In_Staff_Use = false;
            Combat_Phase = 0;
            Combat_Action = 0;
            Combat_Timer = 0;
            highlight_test();
            // If the battle is scripted it should be handling its own events
            if (!Global.game_temp.scripted_battle)
                Global.game_state.combat_events(false, staff);
            Global.game_temp.scripted_battle = false;
        }

        protected void setup_battle()
        {
            Arena = false;
            In_Battle = true;
            In_Staff_Use = Staff_Calling;
            Battle_Calling = false;
            Staff_Calling = false;

            // Preserves RNs while simulating battle, to ensure the outcome in the actual battle is the same
            Global.game_system.save_rns();
            set_combat_data(Global.game_system.Battler_1_Id, Global.game_system.Battler_2_Id);
            bool fighting = !In_Staff_Use || ((Staff_Data)Map_Combat_Data).attack_staff;
            // Readds preserved RNs to the RN queue
            Global.game_system.readd_saved_rns();

            // Test if skipping AI turn/if the skip should be canceled
            Skip_Battle = Global.game_state.skip_ai_turn_activating;
            if (this.attacker != null)
            {
                if (this.combat_target != null)
                {
                    if (this.combat_target.is_unit())
                    {
                        if (!this.attacker.visible_by() &&
                            !(this.combat_target as Game_Unit).visible_by())
                        {
                            Skip_Battle = true;
                        }
                    }
                }
                else
                {
                    if (!this.attacker.visible_by())
                        // Don't skip untargeted staff use
                        if (!In_Staff_Use)
                            Skip_Battle = true;
                }
            }
            // If AI turn skip needs canceled, for various reasons
            if (Map_Combat_Data.is_ally_killed || Map_Combat_Data.has_death_quote ||
                Map_Combat_Data.has_item_drop || Map_Combat_Data.has_promotion ||
                (Units.ContainsKey(Global.game_system.Battler_2_Id) &&
                    get_scene_map().check_talk(
                        Units[Global.game_system.Battler_1_Id],
                        Units[Global.game_system.Battler_2_Id], false, true)))
            {
                switch_out_of_ai_skip(this, new EventArgs());
                Skip_Battle = false;
            }

            set_animation_mode(
                Global.game_system.Battler_1_Id,
                Global.game_system.Battler_2_Id,
                fighting, Map_Combat_Data);
            Map_Combat_Data = null;
            Map_Battle = Global.game_system.Battle_Mode == Constants.Animation_Modes.Map;
        }

        protected void set_combat_data(int battler_1_id, int battler_2_id)
        {
            if (!In_Staff_Use || !Map_Battle)
            {
                Units[battler_1_id].battling = true;
                if (Units.ContainsKey(battler_2_id))
                    Units[battler_2_id].battling = true;
            }

            if (Global.game_temp.scripted_battle)
            {
                Combat.battle_setup(battler_1_id, battler_2_id, combat_distance(battler_1_id, battler_2_id), 0); // Still needed at the moment to init magic attacks
                Map_Combat_Data = new Scripted_Combat_Data(
                    battler_1_id, battler_2_id, combat_distance(battler_1_id, battler_2_id));
            }
            else
            {
                if (In_Staff_Use)
                {
                    Combat.battle_setup(battler_1_id, battler_2_id, battler_2_id == -1 ? 0 : combat_distance(battler_1_id, battler_2_id), 1); // Still needed at the moment to init magic attacks
                    Map_Combat_Data = new Staff_Data(
                        battler_1_id, battler_2_id, battler_2_id == -1 ? 0 : combat_distance(battler_1_id, battler_2_id));
                }
                else if (!attackable_map_object(battler_2_id).is_unit())
                {
                    Combat.battle_setup(battler_1_id, battler_2_id, combat_distance(battler_1_id, battler_2_id), 0); // Still needed at the moment to init magic attacks
                    Map_Combat_Data = new Terrain_Attack_Data(
                        battler_1_id, battler_2_id, combat_distance(battler_1_id, battler_2_id));
                }
                else
                {
                    Combat.battle_setup(battler_1_id, battler_2_id, combat_distance(battler_1_id, battler_2_id), 0); // Still needed at the moment to init magic attacks
                    Map_Combat_Data = new Combat_Data(
                        battler_1_id, battler_2_id, combat_distance(battler_1_id, battler_2_id));
                }
            }
        }

        public void to_arena()
        {
            refresh_move_ranges();
            wait_for_move_update();
            Arena = true;
            In_Battle = true;
            Map_Battle = false;
            Skip_Battle = false;
            Combat_Phase = 1;
            Combat_Timer = 5;
            Global.game_system.In_Arena = true;

            Units[Global.game_system.Battler_1_Id].preload_animations(Global.game_system.Arena_Distance);
            Units[Global.game_system.Battler_2_Id].preload_animations(Global.game_system.Arena_Distance);
            // This should never be scripted but just in case
            if (!Global.game_temp.scripted_battle && !Global.game_system.home_base)
                Global.scene.suspend();
        }

        public void arena_load()
        {
            Combat_Phase = 2;
            Combat_Timer = 0;
        }
    }
}
