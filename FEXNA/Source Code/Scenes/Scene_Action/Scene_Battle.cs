using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA_Library;
using ListExtension;
using FEXNAWeaponExtension;

namespace FEXNA
{
    class Scene_Battle : Scene_Action
    {
        protected int Attack_Id = 0;
        protected int Shake_Attacker = 0;
        protected int ActualDmg, Dmg;
        protected bool Dmg_Dealt, Hit, Crit;

        public Scene_Battle() { }

        protected override void initialize_base()
        {
            Scene_Type = "Scene_Battle";
            main_window();
            camera = new Camera(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT, Vector2.Zero);
        }

        public override void initialize_action(int distance)
        {
            Combat_Data = Global.game_state.combat_data;
            Battler_1 = Global.game_map.units[Combat_Data.Battler_1_Id];
            if (Combat_Data.Battler_2_Id != null)
                Battler_2 = Global.game_map.units[(int)Combat_Data.Battler_2_Id];
            if (Battler_2 == null || Battler_2.same_team(Battler_1))
                Reverse = true;
            else
                Reverse = Battler_2.is_opposition;
            Real_Distance = Distance = distance;
            //if (Global.data_weapons[weapon_id(1)].Max_Range <= 2 && Distance > 2)
            if (!Global.data_weapons[weapon_id(1)].Long_Range && Distance > 2) //Debug
                Distance = 2;
            if (Combat_Data.is_ally_killed || Combat_Data.has_death_quote)
                Can_Skip = false;
            initialize_weather_condition();
        }

        protected void initialize_weather_condition()
        {
            if (Weather_Visible == BattleWeathers.Visible)
            {
                // If the terrain of the background costs 1 for fliers in rain, it is indoors; otherwise, outdoors and weather is visible 
                int tag = this.background_battler.terrain_id();
                if (Global.data_terrains[tag].Move_Costs[(int)Weather_Types.Rain][(int)FEXNA_Library.MovementTypes.Flying] == 1)
                    Weather_Visible = BattleWeathers.Indoors;
            }
        }

        #region Screen Shake
        protected override void update_shake()
        {
            base.update_shake();
            if (Shake.Count > 0)
            {
                Vector2 shake = Shake.pop();
                if (Shake_Attacker == 2 ^ !Reverse)
                    shake.X *= -1;
                if (Distance == 1)
                {
                    Layer_2_Shake += shake;
                    Layer_3_Shake += shake * new Vector2(-1, 1);
                }
                else
                {
                    if (Shake_Attacker == 0)
                    {
                        Layer_2_Shake += shake;
                        Layer_3_Shake += shake * new Vector2(-1, 1); // is this how it works? check Kamaitachi in FEXP //Debug
                    }
                    else
                    {
                        if (Mini_Shaking ^ (Shake_Attacker != 1) ^ Reverse) // fiddle with until it works for all situations//Debug
                            Platform_1_Shake += shake * new Vector2(-1, 1);
                        else
                            Platform_2_Shake += shake * new Vector2(-1, 1);
                    }
                }
                if (!Mini_Shaking)
                    Layer_5_Shake += shake;
            }
        }

        protected override void mini_shake(int type)
        {
            Shake_Attacker = Active_Battler;
            // Tests if battler wants both platforms to shake
            Battler_Sprite battler = (Active_Battler == 1 ? Battler_1_Sprite : Battler_2_Sprite);
            if (battler.all_shake && Distance > 1)
                Shake_Attacker = 0;
            base.mini_shake(type);
        }

        protected  void hit_shake()
        {
            Shake_Reset = true;
            Mini_Shaking = false;
            Shake_Attacker = Active_Battler;
            // Tests if battler wants both platforms to shake
            Battler_Sprite battler = (Active_Battler == 1 ? Battler_1_Sprite : Battler_2_Sprite);
            if (battler.all_shake && Distance > 1)
                Shake_Attacker = 0;
            Shake = new List<Vector2> {new Vector2(3,3),new Vector2(-6,-6),new Vector2(3,3),
                new Vector2(3,3),new Vector2(-3,-3),new Vector2(-3,-3),new Vector2(3,3),
                new Vector2(3,3),new Vector2(-3,-3),new Vector2(0,0),new Vector2(-3,-3),
                new Vector2(3,3),new Vector2(0,0),new Vector2(3,3),new Vector2(-3,-3)};
        }

        protected  void crit_shake()
        {
            Shake_Reset = true;
            Mini_Shaking = false;
            Shake_Attacker = Active_Battler;
            // Tests if battler wants both platforms to shake
            Battler_Sprite battler = (Active_Battler == 1 ? Battler_1_Sprite : Battler_2_Sprite);
            if (battler.all_shake && Distance > 1)
                Shake_Attacker = 0;
            // Up left, down right for right attacker
            Shake = new List<Vector2> {new Vector2(-1,-1),new Vector2(1,1),new Vector2(1,1),
                new Vector2(-1,-1),new Vector2(-2,-2),new Vector2(2,2),new Vector2(2,2),
                new Vector2(-2,-2),new Vector2(-2,-2),new Vector2(2,2),new Vector2(2,2),
                new Vector2(-2,-2),new Vector2(-4,-4),new Vector2(4,4),new Vector2(4,4),
                new Vector2(-4,-4),new Vector2(-4,-4),new Vector2(4,4),new Vector2(4,4),
                new Vector2(-4,-4),new Vector2(-4,-4),new Vector2(4,4),new Vector2(-6,-6),
                new Vector2(6,6),new Vector2(6,6),new Vector2(-6,-6),new Vector2(-6,-6),
                new Vector2(6,6),new Vector2(0,0),new Vector2(6,6),new Vector2(-6,-6),
                new Vector2(0,0),new Vector2(-6,-6),new Vector2(6,6),new Vector2(0,0),
                new Vector2(6,6),new Vector2(-6,-6),new Vector2(0,0),new Vector2(0,0),
                new Vector2(-6,-6),new Vector2(6,6)};
        }
        #endregion

        protected virtual bool is_last_attack()
        {
            return Combat_Data.Data.Count == Attack_Id + 1 || hp(1) <= 0 || hp(2) <= 0;
        }

        protected virtual bool is_next_attacker_same()
        {
            if (is_last_attack())
                return true;
            return Combat_Data.Data[Attack_Id].Key.Attacker == Combat_Data.Data[Attack_Id + 1].Key.Attacker;
        }

        protected bool continue_attacking()
        {
            return continue_attacking(false);
        }
        protected bool continue_attacking(bool weapon_broke)
        {
            if (Battler_1.continue_attacking())
                return true;
            if (Battler_2.continue_attacking())
                return true;

            if (Combat_Data == null) return true;
            return (!weapon_broke && hp(1) > 0 && hp(2) > 0);
        }

        protected bool skip_skill_update()
        {
            return (Battler_1.skip_skill_update() || Battler_2.skip_skill_update());
        }

        protected bool is_hud_ready()
        {
            if (Battler_1_Sprite.ignore_hud())
                return true;
            if (Battler_2_Sprite.ignore_hud())
                return true;
            return HUD.is_hp_ready();
        }

        protected override void refresh_stats(bool instant)
        {
            if (Combat_Data != null)
            {
                HUD.set_attack_id(Attack_Id);
                HUD.update_battle_stats();
            }
        }

        protected bool shake_wait() // currently unused //Yeti
        {
            if (Battler_1_Sprite.ignore_hud())
                return false;
            if (Battler_2_Sprite.ignore_hud())
                return false;
            return true;
        }

        public override void update()
        {
            base.update();
        }

        protected override void play_battle_theme()
        {
            Global.Audio.PlayBattleTheme(Global.game_state.battle_theme());
        }

        protected override bool test_battle_theme()
        {
            string filename = Global.game_state.battle_theme();
            return Global.Audio.IsTrackPlaying("BattleBgm") &&
                filename == Global.Audio.BgmTrackCueName("BattleBgm");
        }

        #region Phase Updates
        protected override void update_phase_0()
        {
            base.update_phase_0();
        }

        protected override void update_phase_1()
        {
            base_update_phase_1();
            switch (Segment)
            {
                case 0:
                    bool cont = false;
                    while (!cont)
                    {
                        cont = true;
                        bool no_attack = false;
                        Game_Unit battler_1 = null, battler_2 = null;
                        if (Combat_Data.Data[Attack_Id].Key.Attacker == 1)
                        {
                            battler_1 = Battler_1;
                            battler_2 = Battler_2;
                        }
                        else if (Combat_Data.Data[Attack_Id].Key.Attacker == 2)
                        {
                            battler_1 = Battler_2;
                            battler_2 = Battler_1;
                        }
                        else
                        {
                            no_attack = true;
                            Segment = 1;
                        }
                        bool spell_animation = battler_1.spell_animation();
                        if (!no_attack)
                        {
                            if (!Attack_Active)
                            {
                                Active_Battler = Combat_Data.Data[Attack_Id].Key.Attacker;
                                add_battle_action(Battle_Actions.New_Attack);
                            }
                            bool next_attack = attack(battler_1, battler_2, spell_animation);
                            Battler_1.update_attack_graphics();
                            Battler_2.update_attack_graphics();
                            if (next_attack)
                            {
                                Timer = 0;
                                Attack_Id++;
                                if (Attack_Id >= Combat_Data.Data.Count)
                                {
                                    Active_Battler = 0;
                                    Segment = 1;
                                    post_round_skip_deny();
                                }
                                refresh_stats();
                            }
                        }
                    }
                    break;
                case 1:
                    switch (Timer)
                    {
                        case 20:
                            Segment = 2;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                case 2:
                    // Reset stats for battle ending
                    for (int i = 0; i < Combat_Data.Data[Combat_Data.Data.Count - 1].Value.Count; i++)
                    {
                        Combat_Action_Data data = Combat_Data.Data[Combat_Data.Data.Count - 1].Value[i];
                        if (data.Trigger == (int)Combat_Action_Triggers.End)
                        {
                            HUD.set_action_id(i);
                            HUD.update_battle_stats();
                        }
                    }
                    Phase = 2;
                    Segment = 0;
                    apply_combat();
                    break;
            }
        }

        protected virtual void post_round_skip_deny()
        {
            Can_Skip = false;
        }

        protected void base_update_phase_1()
        {
            base.update_phase_1();
        }

        protected virtual void apply_combat()
        {
            Combat_Data.apply_combat();
        }

        protected virtual bool exp_gained()
        {
            Exp_Gain = 0;
            Exp_Sound = false;
            int exp_id = 0;
            // Exp gain
            if (Combat_Data.Exp_Gain1 != 0)
            {
                exp_id = 1;
                Exp_Gain = Combat_Data.Exp_Gain1;
            }
            else if (Combat_Data.Exp_Gain2 != 0)
            {
                exp_id = 2;
                Exp_Gain = Combat_Data.Exp_Gain2;
            }
            if (Exp_Gain != 0)
            {
                create_exp_gauge(exp_id == 1 ? Combat_Data.Exp1 : Combat_Data.Exp2);
                Exp_Gauge.loc.Y = Config.WINDOW_HEIGHT - 24;
                return true;
            }
            return false;
        }

        protected bool exp_gain()
        {
            if (Exp_Gain > 0)
            {
                gain_exp();
                Exp_Gain--;
                if (!Exp_Sound)
                {
                    Global.game_system.play_se(System_Sounds.Exp_Gain);
                    Exp_Sound = true;
                }
            }
            else if (Exp_Gain < 0)
            {
                lose_exp();
                Exp_Gain++;
            }
            else
                return true;
            return false;
        }

        protected override void update_phase_2()
        {
            base.update_phase_2();
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Timer)
                {
                    // Exp gain
                    case 0:
                        if (exp_gained())
                            Timer++;
                        else
                        {
                            Timer = 75;
                            cont = false;
                        }
                        break;
                    // Waits
                    case 1:
                    case 2:
                    case 3:
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
                        Timer++;
                        break;
                    // Unit gains exp
                    case 22:
                        if (exp_gain())
                        {
                            Exp_Sound = false;
                            Global.game_system.cancel_sound();
                            Timer++;
                        }
                        break;
                    // Waits
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                    case 28:
                    case 29:
                    case 30:
                    case 31:
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
                        Timer++;
                        break;
                    // Exp gauge disappears
                    case 42:
                        Exp_Gauge.retract();
                        Timer++;
                        break;
                    // Waits
                    case 43:
                    case 44:
                    case 45:
                    case 46:
                    case 47:
                        Timer++;
                        break;
                    // Clears exp window, continues
                    case 48:
                        clear_exp();
                        Timer++;
                        break;
                    // Waits
                    case 49:
                    case 50:
                    case 51:
                    case 52:
                    case 53:
                    case 54:
                    case 55:
                    case 56:
                    case 57:
                    case 58:
                    case 59:
                    case 60:
                    case 61:
                    case 62:
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 69:
                    case 70:
                    case 71:
                    case 72:
                        Timer++;
                        break;
                    case 73:
                        // If Promotion/Level Up, gives control to the level up window
                        if (Battler_1.actor.needs_promotion)
                        {
                            promote(Battler_1.id, -1);
                            Global.game_system.Class_Changer = Battler_1.id;
                            Global.game_system.Class_Change_To = (int)Battler_1.actor.promotes_to();
                            Timer++;
                        }
                        else if (Battler_1.actor.needed_levels > 0)
                        {
                            level_up(Battler_1.id);
                            if (Battler_1.actor.skills_gained_on_level().Any())
                                skill_gain(Battler_1.id);
                            Timer++;
                        }
                        else if (Battler_2.actor.needs_promotion)
                        {
                            promote(Battler_2.id, -1);
                            Global.game_system.Class_Changer = Battler_2.id;
                            Global.game_system.Class_Change_To = (int)Battler_2.actor.promotes_to();
                            Timer++;
                        }
                        else if (Battler_2.actor.needed_levels > 0)
                        {
                            level_up(Battler_2.id);
                            Timer++;
                        }
                        // Else continue
                        else
                        {
                            cont = false;
                            Timer = 75;
                        }
                        break;
                    // Waits for skill gain
                    case 74:
                        if (!is_leveling_up()) Timer++;
                        break;
                    case 75:
                        // If skill gained
                        if (is_skill_gaining())
                        {
                            Timer++;
                        }
                        // Else continue
                        else
                        {
                            cont = false;
                            Timer = 77;
                        }
                        break;
                    // Waits for level up window
                    case 76:
                        if (!is_skill_gaining()) Timer++;
                        break;
                    case 77:
                        // If weapon level up
                        if (Battler_1.actor.wlvl_up())
                        {
                            wlvl_up(Battler_1.id);
                            Timer++;
                        }
                        else if (Battler_2.actor.wlvl_up())
                        {
                            wlvl_up(Battler_2.id);
                            Timer++;
                        }
                        // Else continue
                        else
                        {
                            cont = false;
                            Timer = 79;
                        }
                        break;
                    // Waits for weapon level up
                    case 78:
                        if (!is_wlvling_up()) Timer++;
                        break;
                    case 79:
                        // If weapon broke
                        if (weapon_broke(1))
                        {
                            wbreak(new int[] { 0, weapon_id(1) });
                            Timer++;
                        }
                        else if (weapon_broke(2))
                        {
                            wbreak(new int[] { 0, weapon_id(2) });
                            Timer++;
                        }
                        // Else continue
                        else
                        {
                            cont = false;
                            Timer = 81;
                        }
                        break;
                    // Waits for weapon break
                    case 80:
                        if (!is_wbreaking()) Timer++;
                        break;
                    // Sits around
                    case 81:
                    case 82:
                    case 83:
                    case 84:
                    case 85:
                    case 86:
                    case 87:
                    case 88:
                    case 89:
                    case 90:
                    case 91:
                    case 92:
                    case 93:
                    case 94:
                    case 95:
                    case 96:
                    case 97:
                    case 98:
                    case 99:
                    case 100:
                    case 101:
                    case 102:
                    case 103:
                    case 104:
                    case 105:
                    case 106:
                    case 107:
                    case 108:
                    case 109:
                    case 110:
                    case 111:
                    case 112:
                    case 113:
                    case 114:
                    case 115:
                    case 116:
                    case 117:
                    case 118:
                    case 119:
                    case 120:
                    case 121:
                    case 122:
                    case 123:
                    case 124:
                    case 125:
                    case 126:
                    case 127:
                    case 128:
                    case 129:
                        Timer++;
                        break;
                    case 130:
                        Phase = 3;
                        Timer = 0;
                        break;
                }
            }
        }

        protected override void update_phase_3()
        {
            base.update_phase_3();
        }
        #endregion

        protected virtual bool weapon_broke(int id)
        {
            switch (id)
            {
                case 1:
                    return Combat_Data.weapon_1_broke;
                case 2:
                    return Combat_Data.weapon_2_broke;
            }
            return false;
        }

        protected virtual int weapon_id(int id)
        {
            switch (id)
            {
                case 1:
                    return Combat_Data.Weapon_1_Id;
                case 2:
                    return Combat_Data.Weapon_2_Id;
            }
            return -1;
        }

        protected virtual Data_Equipment equipment(int index)
        {
            switch (index)
            {
                case 1:
                    return Global.data_weapons[weapon_id(1)];
                case 2:
                    return Global.data_weapons[weapon_id(2)];
            }
            return null;
        }

        /// <summary>
        /// Gets the HP a battler will have after the current attack
        /// </summary>
        /// <param name="index">Index of the battler. 1 is the first battler, 2 is the second, etc</param>
        /// <returns>Returns the HP value</returns>
        protected virtual int hp(int index)
        {
            switch (index)
            {
                case 1:
                    return Combat_Data.Hp1;
                case 2:
                    return Combat_Data.Hp2;
            }
            return -1;
        }

        protected virtual bool kill()
        {
            return Combat_Data.Data[Attack_Id].Key.Result.kill;
        }

        #region Attack
        protected bool attack(Game_Unit battler_1, Game_Unit battler_2, bool magic_attack)
        {
            Data_Equipment weapon;
            Battler_Sprite battler_1_sprite, battler_2_sprite;
            bool reverse;
            Battler_Sprite dying_unit;
            int[] ids;
            if (battler_1.id == Battler_1.id)
            {
                weapon = equipment(1);
                battler_1_sprite = Battler_1_Sprite;
                battler_2_sprite = Battler_2_Sprite;
                reverse = true;
                ids = new int[] { 0, 1 };
            }
            else
            {
                weapon = equipment(2);
                battler_1_sprite = Battler_2_Sprite;
                battler_2_sprite = Battler_1_Sprite;
                reverse = false;
                ids = new int[] { 1, 0 };
            }
            // Brighten after magic
            if (Magic_Brighten)
            {
                if (battler_1_sprite.spell_effect == null || battler_1_sprite.spell_effect.brighten_duration <= 0) // This used to check if hud was ready //Yeti
                {
                    brighten();
                    Magic_Brighten = false;
                }
            }
            bool cont = false;
            while (!cont)
            {
                cont = true;
                Battle_Actions action = (Battle_Actions)Battle_Action[0];
                switch (action)
                {
                    #region Initiates new attack
                    case Battle_Actions.New_Attack:
                        remove_battle_action(Battle_Actions.New_Attack);
                        add_battle_action(Battle_Actions.Prehit_Check);
                        cont = false;
                        break;
                    #endregion
                    #region Battle_Actions.Prehit_Check:
                    case Battle_Actions.Prehit_Check:
                        remove_battle_action(Battle_Actions.Prehit_Check);
                        // Defender Prehit Skill Check
                        if (Combat_Data != null)
                            for(int i = 0; i < Combat_Data.Data[Attack_Id].Value.Count; i++)
                            {
                                Combat_Action_Data data = Combat_Data.Data[Attack_Id].Value[i];
                                if (data.Trigger == (int)Combat_Action_Triggers.Attack && data.Battler_Index == 2)
                                {
                                    HUD.set_action_id(i);
                                    Game_Unit battler = data.Battler_Index == 1 ? battler_1_sprite.battler : battler_2_sprite.battler;
                                    foreach (string skill_id in data.Skill_Ids)
                                        battler.actor.activate_battle_skill(skill_id, true);
                                }
                            }
                        if (battler_2.actor.skill_activated)
                            battler_2_sprite.skill_animation(Distance);
                        //refresh_stats(false); // Skills don't use this anymore I guess //Debug
                        add_battle_action(Battle_Actions.Hit_Check);
                        Attack_Active = true;
                        cont = false;
                        break;
                    #endregion
                    #region Determine outcome of attack
                    case Battle_Actions.Hit_Check:
                        if (battler_2_sprite.skill_effect == null || battler_2_sprite.skill_effect.duration <= 2)
                        {
                            remove_battle_action(Battle_Actions.Hit_Check);
                            insert_battle_action(0, Battle_Actions.Start_Attack);
                            determine_hit(battler_1_sprite, battler_2_sprite);
                            cont = false;
                        }
                        break;
                    #endregion
                    #region Begin attack
                    case Battle_Actions.Start_Attack:
                        remove_battle_action(Battle_Actions.Start_Attack);
                        add_battle_action(Battle_Actions.Animate_Attack);
                        cont = false;
                        break;
                    #endregion
                    #region Battle_Actions.Skill_Activation
                    case Battle_Actions.Skill_Activation: // Why doesn't this do anything anymore //Yeti
                        remove_battle_action(Battle_Actions.Skill_Activation);
                        cont = false;
                        break;
                    #endregion
                    #region Battle_Actions.Defender_Skill_Activation
                    case Battle_Actions.Defender_Skill_Activation:
                        break;
                    #endregion
                    #region Attack Animation
                    case Battle_Actions.Animate_Attack:
                        attack_anim(battler_1_sprite);
                        HUD.face_set(ids[0], 1);
                        remove_battle_action(Battle_Actions.Animate_Attack);
                        add_battle_action(magic_attack ? Battle_Actions.Magic_Wait_For_Hit : Battle_Actions.Wait_For_Hit);
                        break;
                    #endregion
                    #region Battle_Actions.Wait_For_Hit
                    case Battle_Actions.Wait_For_Hit:
                        if (battler_1.actor.skill_activated)
                            if (battler_1_sprite.is_skill_time(Crit))
                                battler_1_sprite.skill_animation(Distance);
                        if (battler_1_sprite.duration <= 2)
                        {
                            remove_battle_action(Battle_Actions.Wait_For_Hit);
                            // ON-HIT SKILL ACTIVATION GOES HERE (Determination) //Yeti
                            if (Combat_Data != null)
                                for (int i = 0; i < Combat_Data.Data[Attack_Id].Value.Count; i++)
                                {
                                    Combat_Action_Data data = Combat_Data.Data[Attack_Id].Value[i];
                                    if (data.Trigger == (int)Combat_Action_Triggers.Hit)
                                    {
                                        HUD.set_action_id(i);
                                        Game_Unit battler = data.Battler_Index == 1 ? battler_1_sprite.battler : battler_2_sprite.battler;
                                        foreach (string skill_id in data.Skill_Ids)
                                            battler.actor.activate_battle_skill(skill_id);
                                    }
                                }
                            if (battler_2.actor.skill_activated)
                            {
                                battler_2_sprite.skill_animation(Distance);
                                add_battle_action(Battle_Actions.On_Hit_Skill);
                                cont = false;
                            }
                            else
                            {
                                if (Hit)
                                {
                                    add_battle_action(Battle_Actions.Hit);
                                    if (Dmg_Dealt)
                                        hit_flash(battler_1_sprite, battler_2_sprite);
                                }
                                else
                                    add_battle_action(Battle_Actions.Miss);
                            }
                        }
                        break;
                    #endregion
                    #region Battle_Actions.Magic_Wait_For_Hit
                    case Battle_Actions.Magic_Wait_For_Hit:
                        if (battler_1.actor.skill_activated)
                            if (battler_1_sprite.is_skill_time(Crit))
                                battler_1_sprite.skill_animation(Distance);
                        if (battler_1_sprite.duration <= 2)
                        {
                            remove_battle_action(Battle_Actions.Magic_Wait_For_Hit);
                            add_battle_action(Battle_Actions.Spell_Start);
                        }
                        break;
                    #endregion
                    #region Battle_Actions.Spell_Start
                    case Battle_Actions.Spell_Start:
                        hit_freeze(battler_1_sprite);
                        if (weapon != null && weapon.is_weapon)
                            darken();
                        if (weapon != null && weapon.is_weapon && ((Data_Weapon)weapon).has_anima_start())
                        {
                            battler_1_sprite.anima_start(Distance);
                            add_battle_action(Battle_Actions.Anima_Effect_End);
                        }
                        else
                            attack_spell(battler_1_sprite);
                        remove_battle_action(Battle_Actions.Spell_Start);
                        add_battle_action(Battle_Actions.Spell_Pan);
                        break;
                    #endregion
                    #region Battle_Actions.Anima_Effect_End
                    case Battle_Actions.Anima_Effect_End:
                        if (battler_1_sprite.anima_ready)
                        {
                            attack_spell(battler_1_sprite);
                            remove_battle_action(Battle_Actions.Anima_Effect_End);
                        }
                        break;
                    #endregion
                    #region Battle_Actions.Spell_Pan
                    case Battle_Actions.Spell_Pan:
                        if (Distance > 1)
                        {
                            if (battler_1_sprite.spell_pan)
                            {
                                pan(reverse ? -1 : 1);
                                remove_battle_action(Battle_Actions.Spell_Pan);
                                add_battle_action(Battle_Actions.Wait_For_Spell);
                            }
                        }
                        else
                        {
                            cont = false;
                            remove_battle_action(Battle_Actions.Spell_Pan);
                            add_battle_action(Battle_Actions.Wait_For_Spell);
                        }
                        break;
                    #endregion
                    #region Battle_Actions.Wait_For_Spell
                    case Battle_Actions.Wait_For_Spell:
                        if (battler_1_sprite.spell_ready)
                        {
                            remove_battle_action(Battle_Actions.Wait_For_Spell);
                            // ON-HIT SKILL ACTIVATION GOES HERE (Determination) //Yeti
                            if (Combat_Data != null)
                                for (int i = 0; i < Combat_Data.Data[Attack_Id].Value.Count; i++)
                                {
                                    Combat_Action_Data data = Combat_Data.Data[Attack_Id].Value[i];
                                    if (data.Trigger == (int)Combat_Action_Triggers.Hit)
                                    {
                                        HUD.set_action_id(i);
                                        foreach (string skill_id in data.Skill_Ids)
                                            switch (skill_id)
                                            {
                                                case "DETER":
                                                    //battler_1_sprite.battler.actor.activate_deter();
                                                    break;
                                            }
                                    }
                                }
                            if (Hit)
                            {
                                add_battle_action(Battle_Actions.Hit);
                                int time = 8;
                                if (weapon != null && weapon.is_weapon && On_Hit.SPELL_WHITEN_TIME.ContainsKey(weapon.Id))
                                    time = On_Hit.SPELL_WHITEN_TIME[weapon.Id];
                                hit_flash(battler_1_sprite, battler_2_sprite, time);
                            }
                            else
                                add_battle_action(Battle_Actions.Miss);
                        }
                        break;
                    #endregion
                    #region Battle_Actions.On_Hit_Skill
                    case Battle_Actions.On_Hit_Skill:
                        if (battler_2_sprite.duration <= 2 && (battler_2_sprite.skill_effect == null || battler_2_sprite.skill_effect.duration <= 2))
                        {
                            remove_battle_action(Battle_Actions.On_Hit_Skill);
                            if (Hit)
                            {
                                add_battle_action(Battle_Actions.Hit);
                                // How does this interact with the above Wait_For_Spell? Can they flash twice? //Yeti
                                if (Dmg_Dealt)
                                    hit_flash(battler_1_sprite, battler_2_sprite);
                            }
                            else
                                add_battle_action(Battle_Actions.Miss);
                        }
                        break;
                    #endregion
                    #region Processes hit
                    case Battle_Actions.Hit:
                        if (hit_effects(battler_1_sprite, battler_2_sprite, reverse, magic_attack))
                        {
                            remove_battle_action(Battle_Actions.Hit);
                            Timer = 0;
                            cont = false;
                        }
                        else
                        {
                            if (Timer == 0)
                                if (Dmg_Dealt)
                                HUD.face_set(ids[Combat_Data.Data[Attack_Id].Key.Result.backfire ? 0 : 1], 2);
                            Timer++;
                        }
                        break;
                    #endregion
                    #region Processes miss
                    case Battle_Actions.Miss:
                        if (miss_effects(battler_1_sprite, battler_2_sprite, reverse, magic_attack))
                        {
                            remove_battle_action(Battle_Actions.Miss);
                            Timer = 0;
                            cont = false;
                        }
                        else
                            Timer++;
                        break;
                    #endregion
                    #region Waits for screen to stop shaking
                    // It doesn't really wait, at least not for magic
                    case Battle_Actions.Wait_For_Shake:
                        bool ready = is_hud_ready(); //Yeti
                        //shake_over = ((@shake.length == 0) or !shake_wait?) //Yeti
                        if (magic_attack)
                            ready &= battler_1_sprite.spell_effect_duration <= 3;
                        if (ready)
                        {
                            remove_battle_action(Battle_Actions.Wait_For_Shake);
                            if ((is_battler_dead(ids[0]) || is_battler_dead(ids[1])) && !continue_attacking())
                                insert_battle_action((Combat_Data.Data[Attack_Id].Key.Result.delayed_life_steal ? 1 : 0), Battle_Actions.Wait_For_Death);
                            cont = false;
                        }
                        break;
                    #endregion
                    #region Battle_Actions.Wait_For_Death
                    case Battle_Actions.Wait_For_Death:
                        if (Timer >= 1)
                        {
                            remove_battle_action(Battle_Actions.Wait_For_Death);
                            if (is_battler_dead(ids[1]))
                            {
                                if (Global.game_state.get_death_quote(
                                    battler_2_sprite.battler.id).Length > 0 &&
                                    !is_test_battle)
                                {
                                    Dying_Unit = 2;
                                    battler_2_sprite.get_hit(0, false);
                                    insert_battle_action(0, Battle_Actions.Death_Quote);
                                }
                                else
                                {
                                    HUD.face_set(ids[0], 3);
                                    HUD.face_kill(ids[1]);
                                    battler_kill(battler_2_sprite);
                                }
                            }
                            else if (is_battler_dead(ids[0]))
                            {
                                if (Global.game_state.get_death_quote(
                                    battler_1_sprite.battler.id).Length > 0 &&
                                    !is_test_battle)
                                {
                                    Dying_Unit = 1;
                                    insert_battle_action(0, Battle_Actions.Death_Quote);
                                }
                                else
                                {
                                    HUD.face_set(ids[1], 3);
                                    HUD.face_kill(ids[0]);
                                    battler_kill(battler_1_sprite);
                                }
                            }
                            cont = false;
                            Timer = 0;
                        }
                        else
                            Timer++;
                        break;
                    #endregion
                    #region Battle_Actions.Death_Quote
                    case Battle_Actions.Death_Quote:
                        // Earlier this could skip the end of long hit animations sometimes, so now it waits for hit animation to finish //Debug
                        if (hit_over(magic_attack, battler_1_sprite))
                        {
                            new_message_window();
                            if ((Dying_Unit == (reverse ? 1 : 2)) ^ !Reverse)
                                message_reverse();
                            dying_unit = Dying_Unit == 1 ? battler_1_sprite : battler_2_sprite;
                            Global.game_temp.dying_unit_id = dying_unit.battler.id;
                            Global.game_temp.message_text = Global.death_quotes[Global.game_state.get_death_quote(Global.game_temp.dying_unit_id)];
                            remove_battle_action(Battle_Actions.Death_Quote);
                            insert_battle_action(0, Battle_Actions.Wait_For_Death_Quote);
                            return_anim(battler_1_sprite);
                            reset_faces(ids);
                            if (dying_unit.battler.is_player_team) //Multi
                            {
                                Global.Audio.BgmFadeOut(15);
                                if (dying_unit.battler.loss_on_death)
                                {
                                    Global.Audio.PlayBattleTheme(Constants.Audio.Bgm.GAME_OVER_THEME);
                                }
                                else
                                {
                                    Global.Audio.PlayBattleTheme(Constants.Audio.Bgm.ALLY_DEATH_THEME);
                                }
                            }
                        }
                        break;
                    #endregion
                    #region Battle_Actions.Wait_For_Death_Quote
                    case Battle_Actions.Wait_For_Death_Quote:
                        if (!is_message_window_active && HUD.is_ready && !Message_Active)
                        {
                            if (Dying_Unit != 0)
                            {
                                dying_unit = Dying_Unit == 1 ? battler_1_sprite : battler_2_sprite;
                                // Sets face on victory
                                HUD.face_set(ids[2 - Dying_Unit], 3);
                                HUD.face_kill(ids[Dying_Unit - 1]);
                                battler_kill(dying_unit);
                            }
                            remove_battle_action(Battle_Actions.Wait_For_Death_Quote);
                        }
                        break;
                    #endregion
                    #region Battle_Actions.Wait_For_Miss
                    case Battle_Actions.Wait_For_Miss:
                        if (magic_attack)
                        {
                            if (battler_1_sprite.spell_effect_duration <= 4)
                            {
                                remove_battle_action(Battle_Actions.Wait_For_Miss);
                                if (is_last_attack() && Distance > 1)
                                    insert_battle_action(0, Battle_Actions.Pan_Hit_Over);
                                else
                                {
                                    insert_battle_action(0, Battle_Actions.Hit_Over);
                                    if (Distance > 1 && is_next_attacker_same())
                                        insert_battle_action(0, Battle_Actions.Wait_For_Pan);
                                }
                                Timer = 0;
                                cont = false;
                            }
                        }
                        else
                        {
                            switch (Timer)
                            {
                                case 24:
                                    remove_battle_action(Battle_Actions.Wait_For_Miss);
                                    if (is_last_attack() && Distance > 1)
                                        insert_battle_action(0, Battle_Actions.Pan_Hit_Over);
                                    else
                                    {
                                        insert_battle_action(0, Battle_Actions.Hit_Over);
                                        if (Distance > 1 && is_next_attacker_same())
                                            insert_battle_action(0, Battle_Actions.Wait_For_Pan);
                                    }
                                    Timer = 0;
                                    cont = false;
                                    break;
                                default:
                                    Timer++;
                                    break;
                            }
                        }
                        break;
                    #endregion
                    #region Battle_Actions.Wait_For_Pan
                    case Battle_Actions.Wait_For_Pan:
                        if ((hp(1) > 0 || battler_1_sprite.is_dead()) && (hp(2) > 0 || battler_2_sprite.is_dead()))
                        {
                            switch (Timer)
                            {
                                case 5:
                                    pan(reverse ? 1 : -1);
                                    Timer++;
                                    break;
                                case 14:
                                    remove_battle_action(Battle_Actions.Wait_For_Pan);
                                    Timer = 0;
                                    break;
                                default:
                                    Timer++;
                                    break;
                            }
                        }
                        break;
                    #endregion
                    #region Battle_Actions.No_Damage_Pan
                    case Battle_Actions.No_Damage_Pan:
                        switch (Timer)
                        {
                            case 24:
                                pan(reverse ? 1 : -1);
                                Timer++;
                                break;
                            case 33:
                                remove_battle_action(Battle_Actions.No_Damage_Pan);
                                Timer = 0;
                                break;
                            default:
                                Timer++;
                                break;
                        }
                        break;
                    #endregion
                    #region Battle_Actions.Pan_Hit_Over
                    case Battle_Actions.Pan_Hit_Over:
                        if (!Hit && Miss_Spark != null)
                        {
                            Miss_Spark.remove();
                        }
                        if (!Dmg_Dealt && NoDamage_Spark != null)
                        {
                            NoDamage_Spark.remove();
                        }
                        if (Timer == (!Dmg_Dealt ? 16 : 0))
                        {
                            if (hit_over(magic_attack, battler_1_sprite) && is_hud_ready())
                            {
                                if (hp(2) > 0 || battler_2_sprite.is_dead())
                                {
                                    if (is_battler_dead(ids[1]))
                                        if (!continue_attacking())
                                            HUD.add_message("Defeated", battler_2_sprite.battler.team, 90, "Grey");
                                    if (is_battler_dead(ids[0]))
                                    {
                                        if (!continue_attacking())
                                            HUD.add_message("Defeated", battler_1_sprite.battler.team, 90, "Grey");
                                    }
                                    //else if (Dying_Unit == 0) //Debug
                                    if (Dying_Unit == 0)
                                    {
                                        return_anim(battler_1_sprite);
                                        if (Hit && !is_battler_dead(ids[1]))
                                            battler_2_sprite.idle_anim();
                                        reset_faces(ids);
                                    }
                                    Dying_Unit = 0;
                                    if (!Hit)
                                        battler_2_sprite.avoid_return();
                                    Timer++;
                                }
                            }
                        }
                        else if (Timer == (!Dmg_Dealt ? 26 : 10))
                        {
                            pan(reverse ? 1 : -1);
                            Timer = 0;
                            remove_battle_action(Battle_Actions.Pan_Hit_Over);
                            add_battle_action(Battle_Actions.Wait_For_Return);
                            add_battle_action(Battle_Actions.Cleanup);
                            cont = false;
                        }
                        else
                            Timer++;
                        break;
                    #endregion
                    #region Battle_Actions.Hit_Over
                    case Battle_Actions.Hit_Over:
                        if (!Hit && Miss_Spark != null)
                        {
                            Miss_Spark.remove(Distance > 1 ? 8 : 23);
                        }
                        if (hit_over(magic_attack, battler_1_sprite) && is_hud_ready())
                        {
                            if (hp(2) > 0 || battler_2_sprite.is_dead())
                            {
                                if (is_battler_dead(ids[1]))
                                    if (!continue_attacking())
                                        HUD.add_message("Defeated", battler_2_sprite.battler.team, 90, "Grey");
                                if (is_battler_dead(ids[0]))
                                {
                                    if (!continue_attacking())
                                        HUD.add_message("Defeated", battler_1_sprite.battler.team, 90, "Grey");
                                }
                                //else if (Dying_Unit == 0) //Debug
                                if (Dying_Unit == 0)
                                {
                                    return_anim(battler_1_sprite);
                                    if (Hit && !is_battler_dead(ids[1]))
                                        battler_2_sprite.idle_anim();
                                    reset_faces(ids);
                                }
                                Dying_Unit = 0;
                                if (!Hit)
                                    battler_2_sprite.avoid_return();
                                remove_battle_action(Battle_Actions.Hit_Over);
                                add_battle_action(Battle_Actions.Wait_For_Return);
                                add_battle_action(Battle_Actions.Cleanup);
                                cont = false;
                            }
                        }
                        break;
                    #endregion
                    case Battle_Actions.Life_Drain_Pause:
                        if (hit_over(magic_attack, battler_1_sprite) && is_hud_ready())
                        {
                            battler_1_sprite.life_drain_spell_1(Hit, Crit, Distance);
                            remove_battle_action(Battle_Actions.Life_Drain_Pause);
                            insert_battle_action(0, Battle_Actions.Wait_Life_Drain_Effect1);
                            if (Distance > 1)
                                insert_battle_action(0, Battle_Actions.Life_Drain_Pan);
                        }
                        break;
                    case Battle_Actions.Life_Drain_Pan:
                        if (battler_1_sprite.spell_pan)
                        {
                            pan(reverse ? 1 : -1);
                            remove_battle_action(Battle_Actions.Life_Drain_Pan);
                            cont = false;
                        }
                        break;
                    case Battle_Actions.Wait_Life_Drain_Effect1:
                        if (hit_over(magic_attack, battler_1_sprite))
                        {
                            battler_1_sprite.life_drain_spell_2(Hit, Crit, Distance);
                            attack_dmg_delayed_life_steal(battler_1_sprite.battler, battler_2_sprite.battler, Combat_Data.Data[Attack_Id].Key);
                            remove_battle_action(Battle_Actions.Wait_Life_Drain_Effect1);
                            insert_battle_action(0, Battle_Actions.Wait_Life_Drain_Effect2);
                            if (magic_attack)
                                Magic_Brighten = true;
                        }
                        break;
                    case Battle_Actions.Wait_Life_Drain_Effect2:
                        if (hit_over(magic_attack, battler_1_sprite))
                        {
                            remove_battle_action(Battle_Actions.Wait_Life_Drain_Effect2);
                            insert_battle_action(0, Battle_Actions.Wait_Life_Drain_Heal);
                            cont = false;
                        }
                        break;
                    case Battle_Actions.Wait_Life_Drain_Heal:
                        if (is_hud_ready())
                        {
                            Timer++;
                            if (Timer >= 20)
                            {
                                Timer = 0;
                                remove_battle_action(Battle_Actions.Wait_Life_Drain_Heal);
                                if (Distance > 1 && !is_next_attacker_same())
                                    insert_battle_action(0, Battle_Actions.Life_Drain_Pan_Back);
                            }
                        }
                        break;
                    case Battle_Actions.Life_Drain_Pan_Back:
                        if (is_hud_ready())
                        {
                            switch (Timer)
                            {
                                case 5:
                                    pan(reverse ? -1 : 1);
                                    Timer++;
                                    break;
                                case 18:
                                    Timer = 0;
                                    remove_battle_action(Battle_Actions.Life_Drain_Pan_Back);
                                    break;
                                default:
                                    Timer++;
                                    break;
                            }
                        }
                        break;
                    #region On no damage, sets attacker to return
                    case Battle_Actions.No_Damage_Hit_Over:
                        if (NoDamage_Spark != null)
                            NoDamage_Spark.remove();
                        if (hit_over(magic_attack, battler_1_sprite))
                        {
                            if (is_battler_dead(ids[0]))
                            {
                                if (!continue_attacking())
                                    HUD.add_message("Defeated", battler_1_sprite.battler.team, 90, "Grey");
                            }
                            //else if (Dying_Unit == 0) //Debug
                            if (Dying_Unit == 0)
                            {
                                return_anim(battler_1_sprite);
                                if (Hit && !is_battler_dead(ids[1]))
                                    battler_2_sprite.idle_anim();
                                reset_faces(ids);
                            }
                            Dying_Unit = 0;
                            remove_battle_action(Battle_Actions.No_Damage_Hit_Over);
                            add_battle_action(Battle_Actions.Wait_For_Return);
                            add_battle_action(Battle_Actions.Cleanup);
                            cont = false;
                        }
                        break;
                    #endregion
                    #region Waits for attacker return animation
                    case Battle_Actions.Wait_For_Return:
                        if (true) // if !message_window_active and @hud.ready? and !@message_active //Yeti
                        {
                            if (Distance > 1 && Timer < 5)
                                Timer++;
                            else if (Distance > 1 ? battler_1_sprite.duration <= 3 : battler_1_sprite.duration <= 2)
                            {
                                remove_battle_action(Battle_Actions.Wait_For_Return);
                                // ATTACK END SKILL ACTIVATION GOES HERE (Adept) //Yeti
                                if (Combat_Data != null)
                                    for (int i = 0; i < Combat_Data.Data[Attack_Id].Value.Count; i++)
                                    {
                                        Combat_Action_Data data = Combat_Data.Data[Attack_Id].Value[i];
                                        if (data.Trigger == (int)Combat_Action_Triggers.Return)
                                        {
                                            HUD.set_action_id(i);
                                            Game_Unit battler = data.Battler_Index == 1 ? battler_1_sprite.battler : battler_2_sprite.battler;
                                            foreach (string skill_id in data.Skill_Ids)
                                                battler.actor.activate_battle_skill(skill_id);
                                        }
                                    }
                                if (!skip_skill_update())
                                {
                                    battler_1_sprite.battler.reset_skills();
                                    battler_2_sprite.battler.reset_skills();
                                }
                                Timer = 0;
                                cont = false;
                            }
                        }
                        break;
                    #endregion
                    #region Ends attack and readies next attack
                    case Battle_Actions.Cleanup:
                        if (Bg_Magic_Darken) // this boolean was slightly different before make sure it works //Yeti
                        {
                            brighten();
                            Magic_Brighten = false;
                        }
                        else if (!Bg_Magic_Darken && Brightness.Count == 0)
                        {
                            if (Timer >= 0)
                            {
                                Attack_Active = false;
                                remove_battle_action(Battle_Actions.Cleanup);
                                Timer = 0;
                                return true;
                            }
                            Timer++;
                        }
                        break;
                    #endregion
                }
            }
            // Updates screen flash
            battler_1_sprite.update_flash();
            if (magic_attack)
                update_spell_flash(battler_1_sprite); //Yeti
            return false;
        }

        #region Attack Animations
        protected virtual void attack_anim(Battler_Sprite battler_sprite)
        {
            battler_sprite.attack(Crit, Distance, !Dmg_Dealt, kill(), Hit);
        }
        
        protected virtual void hit_freeze(Battler_Sprite battler_sprite)
        {
            battler_sprite.hit_freeze(Crit, Distance, !Dmg_Dealt, kill(), Hit);
        }

        protected virtual void return_anim(Battler_Sprite battler_sprite)
        {
            battler_sprite.return_anim(Crit, Distance, !Dmg_Dealt, kill(), Hit);
        }

        protected virtual void attack_spell(Battler_Sprite battler_sprite)
        {
            battler_sprite.attack_spell(Hit, Crit, Distance);
        }
        #endregion

        protected void reset_faces(int[] ids)
        {
            if (hp(1) > 0 && hp(2) > 0)
                foreach (int id in ids)
                    HUD.face_set(id, 0);
        }

        protected bool hit_over(bool magic, Battler_Sprite battler_sprite)
        {
            if (magic)
            {
                if (battler_sprite.spell_effect != null)
                    return battler_sprite.spell_effect_duration <= 1;
                return battler_sprite.duration <= 1;
            }
            else
                return battler_sprite.duration <= 1;
        }

        protected bool is_battler_dead(int id)
        {
            if (id == 0)
                return hp(1) <= 0;
            else
                return hp(2) <= 0;
        }

        protected virtual void battler_kill(Battler_Sprite battler_sprite)
        {
            battler_sprite.kill();
        }

        protected virtual void determine_hit(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite)
        {
            // Attacker Prehit Skill Check (Spiral Dive)
            for (int i = 0; i < Combat_Data.Data[Attack_Id].Value.Count; i++)
            {
                Combat_Action_Data data = Combat_Data.Data[Attack_Id].Value[i];
                if (data.Trigger == (int)Combat_Action_Triggers.Attack && data.Battler_Index == 1)
                {
                    HUD.set_action_id(i);
                    Game_Unit battler = data.Battler_Index == 1 ? battler_1_sprite.battler : battler_2_sprite.battler;
                    foreach (string skill_id in data.Skill_Ids)
                        battler.actor.activate_battle_skill(skill_id);
                    battler.mastery_hit_confirm(Combat_Data.Data[Attack_Id].Key.Result.hit);
                }
            }
            ActualDmg = Combat_Data.Data[Attack_Id].Key.Result.actual_dmg;
            Dmg = Combat_Data.Data[Attack_Id].Key.Result.dmg;
            Dmg_Dealt = Combat_Data.Data[Attack_Id].Key.Result.dmg > 0 || kill();
            Hit = Combat_Data.Data[Attack_Id].Key.Result.hit;
            Crit = Combat_Data.Data[Attack_Id].Key.Result.crt;
            // Attacker Posthit Skill Check (Luna)
            for(int i = 0; i < Combat_Data.Data[Attack_Id].Value.Count; i++)
            {
                Combat_Action_Data data = Combat_Data.Data[Attack_Id].Value[i];
                if (data.Trigger == (int)Combat_Action_Triggers.Skill)
                {
                    HUD.set_action_id(i);
                    Game_Unit battler = data.Battler_Index == 1 ? battler_1_sprite.battler : battler_2_sprite.battler;
                    foreach (string skill_id in data.Skill_Ids)
                        battler.actor.activate_battle_skill(skill_id);
                }
            }
            battler_1_sprite.battler.hit_skill_update();
        }

        protected virtual bool hit_effects(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite, bool reverse, bool magic)
        {
            if (Timer == 0)
            {
                bool life_drain = Combat_Data.Data[Attack_Id].Key.Result.delayed_life_steal;
                attack_dmg(battler_1_sprite.battler, battler_2_sprite.battler, Combat_Data.Data[Attack_Id].Key);
                // White screen flash
                if (!magic && Dmg_Dealt && !Crit)
                    White_Screen.visible = true;
                if (magic)
                    battler_1_sprite.end_spell(Hit, Crit, Distance);
                else
                    hit_freeze(battler_1_sprite);
                // Crit shake
                if (Crit && Dmg_Dealt)
                {
                    crit_shake();
                    add_battle_action(Battle_Actions.Wait_For_Shake);
                    if (!magic)
                        create_crit_spark((reverse ^ Reverse));
                    if (!Combat_Data.Data[Attack_Id].Key.Result.backfire)
                        battler_2_sprite.get_hit(Dmg, Crit);
                }
                // Rumble
                if (Hit)
                    (Combat_Data.Data[Attack_Id].Key.Result.backfire ? battler_1_sprite : battler_2_sprite)
                        .battler.hit_rumble(!Dmg_Dealt, Dmg, Crit);
                if (!Dmg_Dealt)
                {
                    HUD.add_message("No Damage!", battler_1_sprite.battler.team, 60);
                    create_nodamage_battle_spark(battler_1_sprite.battler.team, !(reverse ^ Reverse));
                    battler_1_sprite.hit_sound(Dmg_Dealt, Hit, magic);
                    battler_2_sprite.no_damage();
                }
                else
                {
                    battler_1_sprite.hit_sound(Dmg_Dealt, Hit, magic, Crit, kill());
                    if (Crit)
                        HUD.add_message("Critical Hit!", battler_1_sprite.battler.team, 90, "Yellow");
                    if (life_drain)
                        HUD.drain_hit((battler_1_sprite == Battler_1_Sprite) ^ (
                            Combat_Data.Data[Attack_Id].Key.Result.backfire) ? 2 : 1);
                    else
                        HUD.hit((battler_1_sprite == Battler_1_Sprite) ^ (
                            Combat_Data.Data[Attack_Id].Key.Result.backfire) ? 2 : 1);
                }
                if (life_drain)
                {
                    // No damage, and pan back if needed
                    if (!Dmg_Dealt)
                    {
                        if (Distance > 1 && is_next_attacker_same())
                            add_battle_action(Battle_Actions.No_Damage_Pan);
                        if (magic) Magic_Brighten = true;
                        add_battle_action(Battle_Actions.No_Damage_Hit_Over);
                    }
                    // Life drain
                    else
                    {
                        add_battle_action(Battle_Actions.Life_Drain_Pause);
                        add_battle_action(Battle_Actions.Hit_Over);
                    }
                }
                else
                {
                    // Pan back on last hit
                    if (Distance > 1 && is_last_attack())
                    {
                        if (magic) Magic_Brighten = true;
                        add_battle_action(Battle_Actions.Pan_Hit_Over);
                    }
                    else
                    {
                        if (magic) Magic_Brighten = true;
                        // No damage, and pan back if needed
                        if (!Dmg_Dealt)
                        {
                            if (Distance > 1 && is_next_attacker_same())
                                add_battle_action(Battle_Actions.No_Damage_Pan);
                            add_battle_action(Battle_Actions.No_Damage_Hit_Over);
                        }
                        //Hit over, and pan back if needed
                        else
                        {
                            if (Distance > 1 && is_next_attacker_same())
                                add_battle_action(Battle_Actions.Wait_For_Pan);
                            add_battle_action(Battle_Actions.Hit_Over);
                        }

                    }
                }
            }
            // Finished with hit
            else if (Timer > 3)
            {
                if (!magic)
                    White_Screen.visible = false;
                if (Dmg_Dealt)
                {
                    // Normal hit shake
                    if (!Crit)
                    {
                        insert_battle_action(0, Battle_Actions.Wait_For_Shake);
                        hit_shake();
                        if (!Combat_Data.Data[Attack_Id].Key.Result.backfire)
                            battler_2_sprite.get_hit(Dmg, Crit);
                        if (!magic)
                            create_hit_spark((reverse ^ Reverse));
                    }
                    add_damage_number(ActualDmg, reverse ^ Reverse, battler_2_sprite.loc + new Vector2(0, -Config.BATTLER_SIZE / 2));
                }
                return true;
            }
            // Pan from attacker to target
            else if (!magic && Timer == 3)
            {
                if (Distance > 1)
                    pan(reverse ? -1 : 1);
            }
            return false;
        }

        protected virtual bool miss_effects(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite, bool reverse, bool magic)
        {
            // Finished with miss
            if (Timer > 3)
            {
                if (magic) Magic_Brighten = true;
                return true;
            }
            else if (Timer == 0)
            {
                HUD.add_message("Miss!", battler_1_sprite.battler.team, 60);
                create_miss_battle_spark(battler_1_sprite.battler.team, !(reverse ^ Reverse));
                if (magic)
                    battler_1_sprite.end_spell(Hit, Crit, Distance);
                else
                    hit_freeze(battler_1_sprite);
                battler_1_sprite.hit_sound(false, Hit, magic);
                battler_2_sprite.avoid();
            }
            // Pan back
            else if (Timer == 3)
            {
                add_battle_action(Battle_Actions.Wait_For_Miss);
                if (!magic && Distance > 1)
                    pan(reverse ? -1 : 1);
            }
            return false;
        }

        protected virtual void hit_flash(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite, int time = 8)
        {
            (Combat_Data.Data[Attack_Id].Key.Result.backfire ? battler_1_sprite : battler_2_sprite).whiten(time);
        }

        protected void attack_dmg(Game_Unit battler_1, Game_Unit battler_2, Combat_Round_Data data)
        {
            if (battler_1.id == Combat_Data.Battler_1_Id)
            {
                if (!data.Result.backfire)
                {
                    Combat_Data.Hp2 -= data.Result.dmg;
                    Combat_Data.Hp1 = Math.Min(Combat_Data.Hp1 + data.Result.immediate_life_steal, Combat_Data.MaxHp1);
                }
                else
                {
                    Combat_Data.Hp1 = Math.Min(Combat_Data.Hp1 - (data.Result.dmg - data.Result.immediate_life_steal), Combat_Data.MaxHp1);
                }

                //if (data.Result.delayed_life_steal && Combat_Data.Hp1 > 0) //Debug
                //    Combat_Data.Hp1 += data.Result.dmg;
            }
            else
            {
                if (!data.Result.backfire)
                {
                    Combat_Data.Hp1 -= data.Result.dmg;
                    Combat_Data.Hp2 = Math.Min(Combat_Data.Hp2 + data.Result.immediate_life_steal, Combat_Data.MaxHp2);
                }
                else
                {
                    Combat_Data.Hp2 = Math.Min(Combat_Data.Hp2 - (data.Result.dmg - data.Result.immediate_life_steal), Combat_Data.MaxHp2);
                }

                //if (data.Result.delayed_life_steal && Combat_Data.Hp2 > 0) //Debug
                //    Combat_Data.Hp2 += data.Result.dmg;
            }
            // Status effects
            if (!data.Result.backfire)
                battler_2.state_change(data.Result.state_change);
            else
                battler_1.state_change(data.Result.state_change);
        }

        protected void attack_dmg_delayed_life_steal(Game_Unit battler_1, Game_Unit battler_2, Combat_Round_Data data)
        {
            if (battler_1.id == Combat_Data.Battler_1_Id)
            {
                if (data.Result.delayed_life_steal && Combat_Data.Hp1 > 0)
                    Combat_Data.Hp1 = Math.Min(Combat_Data.Hp1 + data.Result.delayed_life_steal_amount, Combat_Data.MaxHp1);
                    //Combat_Data.Hp1 += data.Result.delayed_life_steal_amount; //Debug
            }
            else
            {
                if (data.Result.delayed_life_steal && Combat_Data.Hp2 > 0)
                    Combat_Data.Hp2 = Math.Min(Combat_Data.Hp2 + data.Result.delayed_life_steal_amount, Combat_Data.MaxHp2);
                    //Combat_Data.Hp2 += data.Result.delayed_life_steal_amount; //Debug
            }
        }
        #endregion
    }
}
