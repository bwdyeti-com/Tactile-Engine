//Sparring
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Preparations;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA_Library;

namespace FEXNA
{
    class Window_Sparring_Result
    {
        const int WEXP_INTERVAL = 3;
        const int SPECIAL_OVERSEER_EXP = 100; // Exp to give to the overseer when they are not a staff user, ie when they are the merchant

        private int Timer, Phase, Wexp_Timer;
        private WeaponType Weapon_Type_1, Weapon_Type_2;
        private int Exp1, Exp2, Exp3, Exp_Gain1, Exp_Gain2, Exp_Gain3, Wexp1, Wexp2, Wexp3;
        private int Level_Up_Index = -1;
        private List<int> Promotion_Indices = new List<int>();

        //private Combat_Data Combat_Data;
        private System_Color_Window Battler_1_Panel, Battler_2_Panel, Healer_Panel;
        private Exp_Gauge Battler_1_Gauge, Battler_2_Gauge, Healer_Gauge;
        private Miniface Battler_1_Face, Battler_2_Face;
        private FE_Text Battler_1_Name, Battler_2_Name, Healer_Name;
        private FE_Text Battler_1_Level_Label, Battler_2_Level_Label, Healer_Level_Label;
        private FE_Text Battler_1_Level, Battler_2_Level, Healer_Level;
        private FE_Text Battler_1_Victory, Battler_2_Victory;
        private FE_Text Battler_1_Support, Battler_2_Support;
        private Weapon_Level_Gauge Battler_1_WLvl, Battler_2_WLvl, Healer_WLvl;
        private Sprite Black_Fill;
        private Character_Sprite Healer_Sprite;
        protected Sparring_Gauge Battler_1_Ready, Battler_2_Ready, Healer_Ready;
        protected List<Stat_Up_Spark> Wexp_Sparkles = new List<Stat_Up_Spark>();
        protected List<LevelUp_Map_Spark> Level_Ups = new List<LevelUp_Map_Spark>();

        #region Accessors
        private static Game_Actor battler_1 { get { return Global.game_actors[Window_Sparring.Battler_1_Id]; } }
        private static Game_Actor battler_2 { get { return Global.game_actors[Window_Sparring.Battler_2_Id]; } }
        private static Game_Actor healer { get { return Global.game_actors[Window_Sparring.Healer_Id]; } }

        public bool finished { get { return Phase > 8; } }

        public int level_up_index { get { return Level_Up_Index; } }

        public List<int> promotion_indices { get { return Promotion_Indices; } }
        #endregion

        public Window_Sparring_Result(Combat_Data data)
        {
            //Combat_Data = data;
            set_exp(data);

            Black_Fill = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            Black_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Fill.tint = new Color(0, 0, 0, 0);

            #region Panels
            Battler_1_Panel = new System_Color_Window();
            Battler_1_Panel.width = 144;
            Battler_1_Panel.height = 72;
            Battler_1_Panel.loc = new Vector2(Config.WINDOW_WIDTH / 2 - Battler_1_Panel.width, 48) + new Vector2(-Config.WINDOW_WIDTH / 2, 0);
            Battler_1_Panel.stereoscopic = Config.RANKING_WINDOW_DEPTH;

            Battler_2_Panel = new System_Color_Window();
            Battler_2_Panel.width = 144;
            Battler_2_Panel.height = 72;
            Battler_2_Panel.loc = new Vector2(Config.WINDOW_WIDTH / 2, 48) + new Vector2(Config.WINDOW_WIDTH / 2, 0);
            Battler_2_Panel.stereoscopic = Config.RANKING_WINDOW_DEPTH;

            Healer_Panel = new System_Color_Window();
            Healer_Panel.width = 144;
            Healer_Panel.height = 56;
            Healer_Panel.loc = new Vector2(Config.WINDOW_WIDTH / 2 - Healer_Panel.width / 2, Config.WINDOW_HEIGHT - 64) + new Vector2(0, 64);
            Healer_Panel.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            #endregion

            #region Exp Gauges
            Battler_1_Gauge = new Exp_Gauge(battler_1.exp);
            Battler_1_Gauge.skip_appear();
            Battler_1_Gauge.loc = new Vector2(4, Battler_1_Panel.height - 28);
            Battler_1_Gauge.stereoscopic = Config.RANKING_WINDOW_DEPTH;

            Battler_2_Gauge = new Exp_Gauge(battler_2.exp);
            Battler_2_Gauge.skip_appear();
            Battler_2_Gauge.loc = new Vector2(4, Battler_2_Panel.height - 28);
            Battler_2_Gauge.stereoscopic = Config.RANKING_WINDOW_DEPTH;

            Healer_Gauge = new Exp_Gauge(healer.exp);
            Healer_Gauge.skip_appear();
            Healer_Gauge.loc = new Vector2(4, Healer_Panel.height - 28);
            Healer_Gauge.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            #endregion

            #region Minifaces
            Battler_1_Face = new Miniface();
            Battler_1_Face.set_actor(battler_1);
            Battler_1_Face.loc = new Vector2(32, 8);
            Battler_1_Face.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            Battler_1_Face.mirrored = true;

            Battler_2_Face = new Miniface();
            Battler_2_Face.set_actor(battler_2);
            Battler_2_Face.loc = new Vector2(32, 8);
            Battler_2_Face.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            Battler_2_Face.mirrored = false;
            #endregion

            #region Names
            Battler_1_Name = new FE_Text();
            Battler_1_Name.loc = new Vector2(48, 8);
            Battler_1_Name.Font = "FE7_Text";
            Battler_1_Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Battler_1_Name.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            Battler_1_Name.text = battler_1.name;

            Battler_2_Name = new FE_Text();
            Battler_2_Name.loc = new Vector2(48, 8);
            Battler_2_Name.Font = "FE7_Text";
            Battler_2_Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Battler_2_Name.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            Battler_2_Name.text = battler_2.name;

            Healer_Name = new FE_Text();
            Healer_Name.loc = new Vector2(32, 8);
            Healer_Name.Font = "FE7_Text";
            Healer_Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Healer_Name.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            Healer_Name.text = healer.name;
            #endregion

            #region Level
            Battler_1_Level_Label = new FE_Text();
            Battler_1_Level_Label.loc = new Vector2(48, 24);
            Battler_1_Level_Label.Font = "FE7_TextL";
            Battler_1_Level_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Battler_1_Level_Label.text = "LV";
            Battler_1_Level = new FE_Text_Int();
            Battler_1_Level.loc =  Battler_1_Level_Label.loc + new Vector2(32, 0);
            Battler_1_Level.Font = "FE7_Text";
            Battler_1_Level.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Battler_1_Level.text = battler_1.level.ToString();

            Battler_2_Level_Label = new FE_Text();
            Battler_2_Level_Label.loc = new Vector2(48, 24);
            Battler_2_Level_Label.Font = "FE7_TextL";
            Battler_2_Level_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Battler_2_Level_Label.text = "LV";
            Battler_2_Level = new FE_Text_Int();
            Battler_2_Level.loc = Battler_2_Level_Label.loc + new Vector2(32, 0);
            Battler_2_Level.Font = "FE7_Text";
            Battler_2_Level.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Battler_2_Level.text = battler_2.level.ToString();

            Healer_Level_Label = new FE_Text();
            Healer_Level_Label.loc = new Vector2(Healer_Panel.width - 72, 8);
            Healer_Level_Label.Font = "FE7_TextL";
            Healer_Level_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Healer_Level_Label.text = "LV";
            Healer_Level = new FE_Text_Int();
            Healer_Level.loc = Healer_Level_Label.loc + new Vector2(32, 0);
            Healer_Level.Font = "FE7_Text";
            Healer_Level.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Healer_Level.text = healer.level.ToString();
            #endregion

            #region Victory Text
            Battler_1_Victory = new FE_Text();
            Battler_1_Victory.loc = new Vector2(112, 8);
            Battler_1_Victory.Font = "FE7_Text";
            Battler_1_Victory.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Battler_1_Victory.stereoscopic = Config.RANKING_WINDOW_DEPTH;

            Battler_2_Victory = new FE_Text();
            Battler_2_Victory.loc = new Vector2(112, 8);
            Battler_2_Victory.Font = "FE7_Text";
            Battler_2_Victory.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Battler_2_Victory.stereoscopic = Config.RANKING_WINDOW_DEPTH;

            if (!data.kill)
            {
                Battler_1_Victory.text = "DRAW";
                Battler_2_Victory.text = "DRAW";
            }
            else if (data.Kill == 2)
            {
                Battler_1_Victory.text = "WIN";
                Battler_2_Victory.text = "LOSE";
                Battler_2_Victory.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
            }
            else
            {
                Battler_1_Victory.text = "LOSE";
                Battler_1_Victory.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
                Battler_2_Victory.text = "WIN";
            }
            #endregion

            #region Support
            Battler_1_Support = new FE_Text();
            Battler_1_Support.loc = new Vector2(40, 40);
            Battler_1_Support.Font = "FE7_Text";
            Battler_1_Support.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Battler_1_Support.stereoscopic = Config.RANKING_WINDOW_DEPTH;

            Battler_2_Support = new FE_Text();
            Battler_2_Support.loc = new Vector2(40, 40);
            Battler_2_Support.Font = "FE7_Text";
            Battler_2_Support.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Battler_2_Support.stereoscopic = Config.RANKING_WINDOW_DEPTH;

            Battler_1_Support.text = Battler_2_Support.text = battler_1.support_possible(battler_2.id) ? "Support+" : "  ---";
            #endregion

            #region Weapon Level
            Weapon_Type_1 = battler_1.determine_sparring_weapon_type();
            Battler_1_WLvl = new Weapon_Level_Gauge(Weapon_Type_1.Key);
            Battler_1_WLvl.loc = new Vector2(72, 16);
            refresh_wexp(Weapon_Type_1, battler_1, Battler_1_WLvl);

            Weapon_Type_2 = battler_2.determine_sparring_weapon_type();
            Battler_2_WLvl = new Weapon_Level_Gauge(Weapon_Type_2.Key);
            Battler_2_WLvl.loc = new Vector2(72, 16);
            refresh_wexp(Weapon_Type_2, battler_2, Battler_2_WLvl);

            if (healer.can_staff())
            {
                Healer_WLvl = new Weapon_Level_Rank(healer.staff_type().Key);
                refresh_wexp(healer.staff_type(), healer, Healer_WLvl);
            }
            else
            {
                WeaponType item_bag_type = Global.weapon_types[0];
                Healer_WLvl = new Weapon_Level_Rank(item_bag_type.Key);
                refresh_wexp(item_bag_type, healer, Healer_WLvl);
            }
            Healer_WLvl.loc = new Vector2(Healer_Panel.width - 48, 0);
            #endregion

            #region Readiness Gauges
            // Spar points
            Battler_1_Ready = new Sparring_Gauge(true);
            Battler_1_Ready.support = battler_1.support_possible(battler_2.id);
            Battler_1_Ready.loc = new Vector2(8, 8);

            Battler_2_Ready = new Sparring_Gauge(true);
            Battler_2_Ready.support = battler_1.support_possible(battler_2.id);
            Battler_2_Ready.loc = new Vector2(8, 8);

            Healer_Ready = new Sparring_Gauge(true);
            Healer_Ready.support = false;
            Healer_Ready.loc = new Vector2(8, -4);

            Battler_1_Ready.points = Global.battalion.sparring_readiness(battler_1.id) + Global.battalion.spar_expense(battler_1.id, false);
            Battler_2_Ready.points = Global.battalion.sparring_readiness(battler_2.id) + Global.battalion.spar_expense(battler_2.id, false);
            Healer_Ready.points = Global.battalion.sparring_readiness(healer.id) + Global.battalion.spar_expense(healer.id, true);
            #endregion

            Healer_Sprite = new Character_Sprite();
            Healer_Sprite.facing_count = 3;
            Healer_Sprite.frame_count = 3;
            Healer_Sprite.loc = Healer_Name.loc + new Vector2(-8, 16);
            Healer_Sprite.stereoscopic = Config.DATA_LEADER_DEPTH;

            Healer_Sprite.texture = Scene_Map.get_team_map_sprite(Constants.Team.PLAYER_TEAM, healer.map_sprite_name);
            if (Healer_Sprite.texture != null)
                Healer_Sprite.offset = new Vector2(
                    (Healer_Sprite.texture.Width / Healer_Sprite.frame_count) / 2,
                    (Healer_Sprite.texture.Height / Healer_Sprite.facing_count) - 8);
            Healer_Sprite.mirrored = Constants.Team.flipped_map_sprite(Constants.Team.PLAYER_TEAM);
        }

        private void set_exp(Combat_Data data)
        {
            Exp1 = Exp_Gain1 = data.Exp_Gain1;
            Exp2 = Exp_Gain2 = data.Exp_Gain2;

            int overseerExp;
            if (healer.can_staff() || !healer.can_oversee_sparring_normally())
            {
                overseerExp = Combat.training_exp(healer, Combat.staff_exp(
                    healer, Global.data_weapons[Scene_Sparring.HEALER_STAFF_ID]));
                overseerExp = Math.Min(Constants.Actor.EXP_TO_LVL, overseerExp);
            }
            else
                overseerExp = SPECIAL_OVERSEER_EXP;
            overseerExp = Math.Min(overseerExp, healer.exp_gain_possible());
            Exp3 = Exp_Gain3 = overseerExp;

            Wexp1 = data.Wexp1 == 0 ? 0 : Math.Max(1, data.Wexp1 / 2);
            Wexp2 = data.Wexp2 == 0 ? 0 : Math.Max(1, data.Wexp2 / 2);
            Wexp3 = Combat.staff_wexp(healer, Global.data_weapons[Scene_Sparring.HEALER_STAFF_ID]);
        }

        private bool exp_ready()
        {
            return Exp1 == 0 && Exp2 == 0 && Exp3 == 0 && Wexp1 == 0 && Wexp2 == 0 && Wexp3 == 0;
        }

        private void update_exp(ref int exp, Exp_Gauge gauge)
        {
            if (exp != 0)
            {
                if (exp > 0)
                {
                    gauge.exp++;
                    exp--;
                }
                else
                {
                    gauge.exp--;
                    exp++;
                }
            }
        }

        private void update_wexp(ref int wexp, WeaponType type, Game_Actor actor, Weapon_Level_Gauge gauge)
        {
            if (wexp != 0)
            {
                if (actor.is_weapon_level_capped(type))
                    wexp = 0;
                else if (wexp > 0)
                {
                    actor.wexp_gain(type, 1);
                    refresh_wexp(type, actor, gauge);
                    wexp--;
                }
                else
                {
                    actor.wexp_gain(type, -1);
                    refresh_wexp(type, actor, gauge);
                    wexp++;
                }
            }
        }

        private void refresh_wexp(WeaponType type, Game_Actor actor, Weapon_Level_Gauge gauge)
        {
            string letter = actor.weapon_level_letter(type);
            gauge.set_data(actor.weapon_level_percent(type),
                letter == Data_Weapon.WLVL_LETTERS[Data_Weapon.WLVL_LETTERS.Length - 1] ? "Green" : "Blue",
                letter);
        }

        public void update()
        {
            int alpha;

            Battler_1_Gauge.update();
            Battler_2_Gauge.update();
            Healer_Gauge.update();
            update_sparks();

            Healer_Sprite.frame = Global.game_system.unit_anim_idle_frame;

            switch (Phase)
            {
                #region 0: Fade In
                case 0:
                    switch (Timer)
                    {
                        case 15:
                            Black_Fill.tint = new Color(0, 0, 0, (Timer + 1) * 10);
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Black_Fill.tint = new Color(0, 0, 0, (Timer + 1) * 10);
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 1: Wait
                case 1:
                    switch (Timer)
                    {
                        case 15:
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 2: Slide Windows On
                case 2:
                    Battler_1_Panel.loc += new Vector2(Config.WINDOW_WIDTH / 16f, 0);
                    Battler_2_Panel.loc += new Vector2(-Config.WINDOW_WIDTH / 16f, 0);
                    Healer_Panel.loc += new Vector2(0, -8);
                    Timer++;
                    if (Timer == 8)
                    {
                        Phase++;
                        Timer = 0;
                    }
                    break;
                #endregion
                #region 3: Wait
                case 3:
                    switch (Timer)
                    {
                        case 31:
                            Battler_1_Ready.points = Global.battalion.sparring_readiness(battler_1.id);
                            Battler_2_Ready.points = Global.battalion.sparring_readiness(battler_2.id);
                            Healer_Ready.points = Global.battalion.sparring_readiness(healer.id);
                            Global.game_system.play_se(System_Sounds.Exp_Gain);
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 4: Exp Gain
                case 4:
                    if (exp_ready())
                    {
                        Global.game_system.cancel_sound();
                        battler_1.clear_wlvl_up();
                        battler_2.clear_wlvl_up();
                        healer.clear_wlvl_up();

                        Combat_Data.exp_gain(battler_1, Exp_Gain1);
                        Combat_Data.exp_gain(battler_2, Exp_Gain2);
                        Combat_Data.exp_gain(healer, Exp_Gain3);

                        battler_1.new_turn_support_gain(battler_2.id, -1, true);
                        battler_2.new_turn_support_gain(battler_1.id, -1, true);
                        Phase++;
                    }
                    else
                    {
                        bool levelup_sound = false;
                        if (Exp1 > 0 && Battler_1_Gauge.exp == Constants.Actor.EXP_TO_LVL - 1)
                        {
                            levelup_sound = true;
                            Level_Ups.Add(new LevelUp_Map_Spark());
                            Level_Ups[Level_Ups.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + LevelUp_Map_Spark.FILENAME);
                            Level_Ups[Level_Ups.Count - 1].loc = Battler_1_Panel.loc + Battler_1_Gauge.loc + new Vector2(36, 0);
                        }
                        if (Exp2 > 0 && Battler_2_Gauge.exp == Constants.Actor.EXP_TO_LVL - 1)
                        {
                            levelup_sound = true;
                            Level_Ups.Add(new LevelUp_Map_Spark());
                            Level_Ups[Level_Ups.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + LevelUp_Map_Spark.FILENAME);
                            Level_Ups[Level_Ups.Count - 1].loc = Battler_2_Panel.loc + Battler_2_Gauge.loc + new Vector2(36, 0);
                        }
                        if (Exp3 > 0 && Healer_Gauge.exp == Constants.Actor.EXP_TO_LVL - 1)
                        {
                            levelup_sound = true;
                            Level_Ups.Add(new LevelUp_Map_Spark());
                            Level_Ups[Level_Ups.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + LevelUp_Map_Spark.FILENAME);
                            Level_Ups[Level_Ups.Count - 1].loc = Healer_Panel.loc + Healer_Gauge.loc + new Vector2(36, 0);
                        }
                        if (levelup_sound)
                            Global.Audio.play_se("System Sounds", "Level_Up_Level");

                        update_exp(ref Exp1, Battler_1_Gauge);
                        update_exp(ref Exp2, Battler_2_Gauge);
                        update_exp(ref Exp3, Healer_Gauge);

                        if (Wexp_Timer == 0)
                        {
                            int weapon_rank_1 = battler_1.get_weapon_level(Weapon_Type_1);
                            int weapon_rank_2 = battler_2.get_weapon_level(Weapon_Type_2);
                            int weapon_rank_3 = healer.get_weapon_level(healer.staff_type());

                            update_wexp(ref Wexp1, Weapon_Type_1, battler_1, Battler_1_WLvl);
                            update_wexp(ref Wexp2, Weapon_Type_2, battler_2, Battler_2_WLvl);
                            update_wexp(ref Wexp3, healer.staff_type(), healer, Healer_WLvl);

                            bool wlvl_sound = false;
                            if (battler_1.get_weapon_level(Weapon_Type_1) > weapon_rank_1)
                            {
                                wlvl_sound = true;
                                Wexp_Sparkles.Add(new Stat_Up_Spark());
                                Wexp_Sparkles[Wexp_Sparkles.Count - 1].loc = Battler_1_Panel.loc + Battler_1_WLvl.loc + new Vector2(20, -8);
                            }
                            if (battler_2.get_weapon_level(Weapon_Type_2) > weapon_rank_2)
                            {
                                wlvl_sound = true;
                                Wexp_Sparkles.Add(new Stat_Up_Spark());
                                Wexp_Sparkles[Wexp_Sparkles.Count - 1].loc = Battler_2_Panel.loc + Battler_2_WLvl.loc + new Vector2(20, -8);
                            }
                            if (healer.get_weapon_level(healer.staff_type()) > weapon_rank_3)
                            {
                                wlvl_sound = true;
                                Wexp_Sparkles.Add(new Stat_Up_Spark());
                                Wexp_Sparkles[Wexp_Sparkles.Count - 1].loc = Healer_Panel.loc + Healer_WLvl.loc + new Vector2(12, -8);
                            }
                            if (wlvl_sound)
                                Global.Audio.play_se("System Sounds", "Level_Up_Stat");

                        }
                        Wexp_Timer = (Wexp_Timer + 1) % WEXP_INTERVAL;
                    }
                    break;
                #endregion
                #region 5: Wait
                case 5:
                    switch (Timer)
                    {
                        case 59:
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            if (Level_Ups.Count == 0)
                                Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 6: Slide Windows Off
                case 6:
                    Battler_1_Panel.loc += new Vector2(-Config.WINDOW_WIDTH / 16f, 0);
                    Battler_2_Panel.loc += new Vector2(Config.WINDOW_WIDTH / 16f, 0);
                    Healer_Panel.loc += new Vector2(0, 8);
                    Timer++;
                    if (Timer == 8)
                    {
                        Phase++;
                        Timer = 0;
                    }
                    break;
                #endregion
                #region 7: Fade In
                case 7:
                    switch (Timer)
                    {
                        case 15:
                            Black_Fill.tint = new Color(0, 0, 0, (15 - Timer) * 10);
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Black_Fill.tint = new Color(0, 0, 0, (15 - Timer) * 10);
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 8: Level Up
                case 8:
                    switch (Timer)
                    {
                        case 0:
                            if (!battler_1.needs_promotion && battler_1.needed_levels > 0)
                                Level_Up_Index = 1;
                            else if (!battler_2.needs_promotion && battler_2.needed_levels > 0)
                                Level_Up_Index = 2;
                            else if (!healer.needs_promotion && healer.needed_levels > 0)
                                Level_Up_Index = 3;
                            else
                            {
                                if (battler_1.needs_promotion)
                                    Promotion_Indices.Add(1);
                                if (battler_2.needs_promotion)
                                    Promotion_Indices.Add(2);
                                if (healer.needs_promotion)
                                    Promotion_Indices.Add(3);
                                Phase++;
                            }
                            break;
                        case 1:
                            if (Level_Up_Index == -1)
                                Timer++;
                            break;
                        case 16:
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 9: Wait
                case 9:
                    switch (Timer)
                    {
                        case 15:
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
            }
        }

        private void update_sparks()
        {
            int i = 0;
            while (i < Wexp_Sparkles.Count)
            {
                Wexp_Sparkles[i].update();
                if (Wexp_Sparkles[i].completed())
                    Wexp_Sparkles.RemoveAt(i);
                else
                    i++;
            }
            i = 0;
            while (i < Level_Ups.Count)
            {
                Level_Ups[i].update();
                if (Level_Ups[i].completed())
                    Level_Ups.RemoveAt(i);
                else
                    i++;
            }
        }

        public void level_up_finished()
        {
            Level_Up_Index = -1;
        }

        public void draw(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Black_Fill.draw(sprite_batch);
            Battler_1_Panel.draw(sprite_batch);
            Battler_2_Panel.draw(sprite_batch);
            Healer_Panel.draw(sprite_batch);

            Battler_1_WLvl.draw_bg(sprite_batch, -Battler_1_Panel.loc);
            Battler_2_WLvl.draw_bg(sprite_batch, -Battler_2_Panel.loc);
            Healer_WLvl.draw_bg(sprite_batch, -Healer_Panel.loc);
            sprite_batch.End();

            Battler_1_Face.draw(sprite_batch, -Battler_1_Panel.loc);
            Battler_2_Face.draw(sprite_batch, -Battler_2_Panel.loc);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Battler_1_Ready.draw(sprite_batch, -Battler_1_Panel.loc);
            Battler_2_Ready.draw(sprite_batch, -Battler_2_Panel.loc);
            Healer_Ready.draw(sprite_batch, -Healer_Panel.loc);

            Battler_1_WLvl.draw(sprite_batch, -Battler_1_Panel.loc);
            Battler_2_WLvl.draw(sprite_batch, -Battler_2_Panel.loc);
            Healer_WLvl.draw(sprite_batch, -Healer_Panel.loc);

            Battler_1_Level_Label.draw(sprite_batch, -Battler_1_Panel.loc);
            Battler_1_Level.draw(sprite_batch, -Battler_1_Panel.loc);
            Battler_2_Level_Label.draw(sprite_batch, -Battler_2_Panel.loc);
            Battler_2_Level.draw(sprite_batch, -Battler_2_Panel.loc);
            Healer_Level_Label.draw(sprite_batch, -Healer_Panel.loc);
            Healer_Level.draw(sprite_batch, -Healer_Panel.loc);

            Battler_1_Victory.draw(sprite_batch, -Battler_1_Panel.loc);
            Battler_2_Victory.draw(sprite_batch, -Battler_2_Panel.loc);

            //Battler_1_Support.draw(sprite_batch, -Battler_1_Panel.loc);
            //Battler_2_Support.draw(sprite_batch, -Battler_2_Panel.loc);

            Battler_1_Name.draw(sprite_batch, -Battler_1_Panel.loc);
            Battler_2_Name.draw(sprite_batch, -Battler_2_Panel.loc);
            Healer_Name.draw(sprite_batch, -Healer_Panel.loc);

            Battler_1_Gauge.draw(sprite_batch, -Battler_1_Panel.loc);
            Battler_2_Gauge.draw(sprite_batch, -Battler_2_Panel.loc);
            Healer_Gauge.draw(sprite_batch, -Healer_Panel.loc);
            Healer_Sprite.draw(sprite_batch, -Healer_Panel.loc);

            foreach (LevelUp_Map_Spark spark in Level_Ups)
                spark.draw(sprite_batch);
            foreach (Stat_Up_Spark spark in Wexp_Sparkles)
                spark.draw(sprite_batch);
            sprite_batch.End();
        }
    }
}
