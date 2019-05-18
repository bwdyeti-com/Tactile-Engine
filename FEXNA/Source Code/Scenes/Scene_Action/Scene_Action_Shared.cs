using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNATexture2DExtension;

namespace FEXNA
{
    partial class Scene_Action
    {
        protected bool Music_Started = false;
        protected Vector2 Battler_1_Map_Loc, Battler_2_Map_Loc;
        protected Vector2 Battler_1_Loc, Battler_2_Loc;
        protected Sprite Background, Black_Backing;
        protected Battle_Platform Platform;
        protected Sprite Black_Fill, White_Screen;
        protected Battle_Text_Spark Miss_Spark, NoDamage_Spark;
        protected Battle_Hit_Spark Hit_Spark;
        private List<Battle_Hit_Number> HitNumbers = new List<Battle_Hit_Number>();
        protected int White_Flash_Time, White_Flash_Timer;
        protected int Exp_Gain;
        protected bool Exp_Sound;
#if DEBUG
        protected Battle_Actions Last_Added_Action, Last_Removed_Action;
#endif

        protected bool is_battle_bg_visible
        {
            get
            {
                return Constants.BattleScene.BATTLE_BG_ALWAYS_VISIBLE ||
                    Global.game_system.Battle_Mode == Constants.Animation_Modes.Full;
            }
        }

        #region Platform/Background
        protected Game_Unit background_battler { get { return Battler_2 == null ? Battler_1 : (Battler_2.is_opposition ? Battler_2 : Battler_1); } }

        protected virtual Texture2D platform(int tag, int distance)
        {
            const string platform_format = @"Graphics/Battlebacks/{0}-{1}";

            string range;
            if (distance == 1)
                range = "Melee";
            else
                range = "Ranged";

            string terrain_name;
            Texture2D result;

            // Check for platform rename
            if (!string.IsNullOrEmpty(Global.data_terrains[tag].Platform_Rename))
            {
                terrain_name = Global.data_terrains[tag].Platform_Rename;
                result = terrain_texture(platform_format, terrain_name, range);
                if (result != null)
                    return result;
            }

            // Check for terrain actual name
            terrain_name = Global.data_terrains[tag].Name;
            result = terrain_texture(platform_format, terrain_name, range);
            if (result != null)
                return result;

            return battle_content.Load<Texture2D>(string.Format(platform_format, "Plains", range));
        }

        protected virtual Texture2D background(Game_Unit battler)
        {
            const string background_format = @"Graphics/Battlebacks/{0}-BG";

            // Checks if the map has a forced background for this tile
            string terrain_name = Global.game_map.forced_battleback(battler.loc);
            if (!string.IsNullOrEmpty(terrain_name))
                if (Global.content_exists(string.Format(background_format, terrain_name)))
                    return battle_content.Load<Texture2D>(string.Format(background_format, terrain_name));

            // Else loads based on terrain type
            int tag = battler.terrain_id();
            Texture2D result;

            // Check for background rename
            if (!string.IsNullOrEmpty(Global.data_terrains[tag].Background_Rename))
            {
                terrain_name = Global.data_terrains[tag].Background_Rename;
                result = terrain_texture(background_format, terrain_name);
                if (result != null)
                    return result;
            }

            // Check for terrain actual name
            terrain_name = Global.data_terrains[tag].Name;
            result = terrain_texture(background_format, terrain_name);
            if (result != null)
                return result;


            return battle_content.Load<Texture2D>(string.Format(background_format, "Plains"));
        }

        private Texture2D terrain_texture(string format, string terrain_name, string range = "")
        {
            if (Global.content_exists(string.Format(format,
                    terrain_name + "_" + Global.game_map.tileset_battleback_suffix, range)))
                return battle_content.Load<Texture2D>(string.Format(format,
                    terrain_name + "_" + Global.game_map.tileset_battleback_suffix, range));
            else if (Global.content_exists(string.Format(format, terrain_name, range)))
                return battle_content.Load<Texture2D>(string.Format(format, terrain_name, range));

            return null;
        }
        #endregion

        protected virtual void mini_shake(int type)
        {
            Shake_Reset = true;
            Mini_Shaking = true;
            switch(type)
            {
                // Merc jumps
                case 1:
                    Global.Rumble.add_rumble(TimeSpan.FromSeconds(0.2f), 0.2f, 0.5f);
                    Shake = new List<Vector2> {new Vector2(0,1),new Vector2(-1,-2),new Vector2(1,1),
                        new Vector2(0,1),new Vector2(0,-1),new Vector2(0,-1),new Vector2(0,1),
                        new Vector2(0,1),new Vector2(0,-1)};
                    break;
                // General crit hit apparently
                case 2:
                    Global.Rumble.add_rumble(TimeSpan.FromSeconds(0.5f), 0.5f, 0.5f);
                    Shake = new List<Vector2> {new Vector2(3,3),new Vector2(-6,-6),new Vector2(3,3),
                        new Vector2(3,3),new Vector2(-3,-3),new Vector2(-3,-3),new Vector2(3,3),
                        new Vector2(3,3),new Vector2(-3,-3),new Vector2(0,0),new Vector2(-3,-3),
                        new Vector2(3,3),new Vector2(0,0),new Vector2(3,3),new Vector2(-3,-3),
                        new Vector2(0,0),new Vector2(0,0),new Vector2(-3,-3),new Vector2(3,3)};
                    break;
            }
        }

        protected void pan(int direction)
        {
            if (Distance == 2)
                Pan = new List<int> { 2, 3, 5, 6, 6, 5, 3, 2 };
            else
                Pan = new List<int> { 2, 7, 12, 17, 22, 27, 32, 32, 32, 32, 27, 22, 17, 12, 7, 2 };
            for (int i = 0; i < Pan.Count; i++)
                Pan[i] = Pan[i] * direction;
        }

        protected void darken()
        {
            if (Bg_Magic_Darken)
                return;
            Brightness = new List<int> { 128, 96, 64, 32 };
            Bg_Magic_Darken = true;
        }

        protected void brighten()
        {
            if (!Bg_Magic_Darken)
                return;
            Brightness = new List<int> { 0, 32, 64, 86 };
            Bg_Magic_Darken = false;
        }

        #region HUD and Stats
        protected virtual void setup_hud()
        {
            HUD = new Window_Battle_HUD(Combat_Data, !Reverse, Distance);
        }

        protected void refresh_stats()
        {
            refresh_stats(true);
        }
        protected virtual void refresh_stats(bool instant)
        {
            HUD.update_battle_stats();
        }
        #endregion

        protected void create_platforms()
        {
            bool right_battler_is_battler_1 = Reverse;
            Platform = new Battle_Platform(Distance > 1);

            var battler1 = Battler_1;
            var battler2 = Battler_2;
            
            // Skills: Swoop
            if (battler2 != null && battler1.swoop_activated)
            {
                // Use the opponent's platform for swooping attackers
                battler1 = battler2;
            }

            Game_Unit right_battler = battler1, left_battler = battler1;
            if (battler2 != null)
            {
                right_battler = Reverse ? battler1 : battler2;
                left_battler = !Reverse ? battler1 : battler2;
            }
            
            int rightPlatformTerrain = right_battler.terrain_id();
            Platform.platform_2 = platform(rightPlatformTerrain, Distance);
            Platform.loc_2 = (right_battler_is_battler_1 ? Battler_1_Loc : Battler_2_Loc) + new Vector2(Distance == 1 ? -24 : -37, 94);

            int leftPlatformTerrain = left_battler.terrain_id();
            Platform.platform_1 = platform(leftPlatformTerrain, Distance);
            Platform.loc_1 = Platform.loc_2 + new Vector2(Distance == 1 ? -87 : -(154 + (Distance == 2 ? 0 : 270)), 0);
        }

        #region Hit Sparks
        protected void create_hit_spark(bool reverse)
        {
            Hit_Spark = new Battle_Hit_Spark();
            Hit_Spark.mirrored = !reverse;
            if (Distance > 1)
                Hit_Spark.loc.X += (reverse ? 24 : -24);
            Hit_Spark.stereoscopic = Config.BATTLE_HITSPARK_DEPTH;
        }

        protected void create_crit_spark(bool reverse)
        {
            Hit_Spark = new Battle_Crit_Spark();
            Hit_Spark.mirrored = reverse;
            Hit_Spark.stereoscopic = Config.BATTLE_CRITSPARK_DEPTH;
        }

        protected void add_damage_number(int value, bool right_side, Vector2 loc)
        {
            if (!Global.game_temp.scripted_battle)
            {
                var number = new Battle_Hit_Number(value, right_side ? 1 : -1);
                number.loc = loc + new Vector2(8 * (right_side ? 1 : -1), 24);
                number.stereoscopic = Config.BATTLE_HITSPARK_DEPTH;
                HitNumbers.Add(number);
            }
        }

        protected void update_hit_spark()
        {
            if (Hit_Spark != null)
            {
                Hit_Spark.update();
                if (Hit_Spark.finished)
                    Hit_Spark = null;
            }
            foreach (var number in HitNumbers)
                number.update();
            HitNumbers.RemoveAll(x => x.finished);
        }

        protected void create_miss_battle_spark(int team, bool reverse)
        {
            Miss_Spark = new Miss_Battle_Spark();
            Miss_Spark.team = team;
            Miss_Spark.loc.X = 140 + 28 * (reverse ? -1 : 1);
            if (Distance > 1)
            {
                if (!Reverse)
                    Miss_Spark.loc.X += (Distance > 2 ? 302 : 32);
                Miss_Spark.loc.X += (reverse ? -56 : 24);
                if (Distance > 2 && reverse)
                    Miss_Spark.loc.X += -270;
            }
            Miss_Spark.stereoscopic = Config.BATTLE_MISSNODAMAGE_DEPTH;
        }

        protected void update_miss_text()
        {
            if (Miss_Spark != null)
            {
                Miss_Spark.update();
                if (Miss_Spark.finished)
                    Miss_Spark = null;
            }
        }

        protected void create_nodamage_battle_spark(int team, bool reverse)
        {
            NoDamage_Spark = new No_Damage_Battle_Spark();
            NoDamage_Spark.team = team;
            NoDamage_Spark.loc.X = 120 + 28 * (reverse ? -1 : 1);
            if (Distance > 1)
            {
                if (!Reverse)
                    NoDamage_Spark.loc.X += (Distance > 2 ? 302 : 32);
                NoDamage_Spark.loc.X += (reverse ? -56 : 24);
                if (Distance > 2 && reverse)
                    NoDamage_Spark.loc.X += -270;
            }
            NoDamage_Spark.stereoscopic = Config.BATTLE_MISSNODAMAGE_DEPTH;
        }

        protected void update_nodamage_text()
        {
            if (NoDamage_Spark != null)
            {
                NoDamage_Spark.update();
                if (NoDamage_Spark.finished)
                    NoDamage_Spark = null;
            }
        }
        #endregion

        #region Screen Flash
        public void screen_flash(int time)
        {
            if (time > 0)
            {
                White_Flash_Time = time;
                White_Flash_Timer = time;
                White_Screen.visible = true;
                White_Screen.opacity = 255;
            }
        }

        protected void update_spell_flash(Battler_Sprite battler_sprite)
        {
            if (White_Flash_Time <= 0)
            {
                White_Screen.visible = false;
                if (battler_sprite.flash)
                    White_Screen.visible = true;
            }
        }
        #endregion

        protected void initialize_battle_sprites()
        {
            Battler_1_Sprite = new Battler_Sprite(Battler_1, Reverse, Distance, Reverse);
            if (Battler_2 != null)
                Battler_2_Sprite = new Battler_Sprite(Battler_2, !Reverse, Distance, Reverse);
            // Initial battle sprite positions to match map sprites: battler 1
            Battler_1_Sprite.loc = Battler_1_Map_Loc =
                (Battler_1_Sprite.battler.loc * Constants.Map.TILE_SIZE -
                    Global.game_map.display_loc) +
                new Vector2(1, 2) * (Constants.Map.TILE_SIZE / 2);
            // Target battle sprite locations: battler 1
            Battler_1_Loc = new Vector2((Reverse ? 184 + (Distance == 1 ? 0 : 24) :
                136 - (Distance == 1 ? 0 : 24)) +
                (Distance > 2 ? (Reverse ? 0 : 0) : 0), 104);
            Battler_1_Sprite.scale = new Vector2(Constants.BattleScene.BATTLER_MIN_SCALE);
            Battler_1_Sprite.stereoscopic = Config.BATTLE_BATTLERS_DEPTH;

            // Target battle sprite locations: battler 2
            Battler_2_Loc = new Vector2((!Reverse ? 184 + (Distance == 1 ? 0 : 56) :
                136 - (Distance == 1 ? 0 : 56)) +
                (Distance > 2 ? (Reverse ? -270 : 270) : 0), 104);
            if (Battler_2 != null)
            {
                // Initial battle sprite positions to match map sprites: battler 2
                Battler_2_Sprite.loc = Battler_2_Map_Loc =
                    (Battler_2_Sprite.battler.loc * Constants.Map.TILE_SIZE -
                        Global.game_map.display_loc) +
                        new Vector2(1, 2) * (Constants.Map.TILE_SIZE / 2);
                Battler_2_Sprite.scale = new Vector2(Constants.BattleScene.BATTLER_MIN_SCALE);
                Battler_2_Sprite.stereoscopic = Config.BATTLE_BATTLERS_DEPTH;

                if (Battler_1_Map_Loc.Y == Battler_2_Map_Loc.Y)
                    Platform_Effect_Angle = (Battler_1_Map_Loc.X < Battler_2_Map_Loc.X ^ Reverse) ? 0 :
                        (Distance == 2 ? -MathHelper.Pi : MathHelper.Pi);
                else
                    Platform_Effect_Angle = (Battler_1_Map_Loc.Y < Battler_2_Map_Loc.Y ^ Reverse) ?
                        MathHelper.PiOver2 : -MathHelper.PiOver2;
            }
            else
                Platform_Effect_Angle = -MathHelper.PiOver2;
            if (ranged_defender_first())
            {
                pan(-1);
                while (Pan.Count > 0)
                    update_pan();
                //Battler_1_Loc += new Vector2(32 + (Distance > 2 ? 270 : 0), 0) * (Reverse ? 1 : -1);
                //Battler_2_Loc += new Vector2(32 + (Distance > 2 ? 270 : 0), 0) * (Reverse ? 1 : -1);
            }
        }

        protected bool ranged_defender_first()
        {
            return Distance > 1 && Combat_Data != null && Combat_Data.Data[0].Key.Attacker != 1;
        }

        protected void battle_sprite_setup(int timer)
        {
            // sure looks like 11 should be a constant here //Yeti

            timer = Math.Max(timer, 0);
            Platform_Effect_Ratio = timer / 11f;
            Battler_1_Sprite.loc = (Battler_1_Map_Loc + Pan_Vector) * ((11 - (timer)) / 11.0f) +
                Battler_1_Loc * ((timer) / 11.0f);
            //Battler_1_Sprite.loc = Battler_1_Map_Loc * ((11 - (timer)) / 11.0f) +
            //    Battler_1_Loc * ((timer) / 11.0f);
            Battler_1_Sprite.scale = new Vector2(Constants.BattleScene.BATTLER_MIN_SCALE) * ((11 - (timer)) / 11.0f) +
                Vector2.One * ((timer) / 11.0f);
            Battler_1_Sprite.update_animation_position();
            if (Battler_2_Sprite != null)
            {
                Battler_2_Sprite.loc = (Battler_2_Map_Loc + Pan_Vector) * ((11 - (timer)) / 11.0f) +
                    Battler_2_Loc * ((timer) / 11.0f);
                //Battler_2_Sprite.loc = Battler_2_Map_Loc * ((11 - (timer)) / 11.0f) +
                //    Battler_2_Loc * ((timer) / 11.0f);
                Battler_2_Sprite.scale = new Vector2(Constants.BattleScene.BATTLER_MIN_SCALE) * ((11 - (timer)) / 11.0f) +
                    Vector2.One * ((timer) / 11.0f);
                Battler_2_Sprite.update_animation_position();
            }
            Platform_Loc_2 = Battler_2 == null ? (Battler_1_Map_Loc + Pan_Vector) * ((11 - (timer)) / 11.0f) +
                Battler_2_Loc * ((timer) / 11.0f) : Battler_2_Sprite.loc;
        }

        protected void platform_effect_setup()
        {
            if (Platform_Effect_1 != null)
            {
                // If at range, the battler 1 platform will be attached to battle 1
                if (Distance > 1)
                    Platform_Effect_1.loc = Battler_1_Sprite.loc;
                // Otherwise it is shared between the two battlers and thus needs to be averaged between them
                else
                {
                    Platform_Effect_1.loc = (Battler_1_Sprite.loc + Platform_Loc_2) / 2;
                }
                Platform_Effect_1.loc -= new Vector2(0, 8);
                Platform_Effect_1.loc -= Pan_Vector;
                Platform_Effect_1.loc += Stereoscopic_Graphic_Object.graphic_draw_offset(Battler_1_Sprite.battler_stereo_offset());
                Platform_Matrix_1 = Matrix.Identity *
                    Matrix.CreateTranslation(new Vector3(-Platform_Effect_1.loc.X, -Platform_Effect_1.loc.Y, 0)) *
                    platform_effect_matrix(Distance > 1, true ^ !Reverse, Platform_Effect_Ratio) *
                    Matrix.CreateRotationZ((1 - Platform_Effect_Ratio) * Platform_Effect_Angle) *
                    Matrix.CreateTranslation(new Vector3(
                            Platform_Effect_1.loc.X,
                            Platform_Effect_1.loc.Y,
                            0) +
                        new Vector3((Battler_2 == null || Distance == 1) ? 0 : Platform_Effect_Ratio * 4, Platform_Effect_Ratio * 2, -0.5f)) *
                    Matrix.CreateScale(new Vector3(1, 1, 0.001f));
                if (Platform_Effect_2 != null)
                {
                    Platform_Effect_2.loc = Battler_2_Sprite.loc - new Vector2(0, 8);
                    Platform_Effect_2.loc -= Pan_Vector;
                    Platform_Effect_2.loc += Stereoscopic_Graphic_Object.graphic_draw_offset(Battler_1_Sprite.battler_stereo_offset());
                    Platform_Matrix_2 = Matrix.Identity *
                        Matrix.CreateTranslation(new Vector3(-Platform_Effect_2.loc.X, -Platform_Effect_2.loc.Y, 0)) *
                        platform_effect_matrix(Distance > 1, false ^ !Reverse, Platform_Effect_Ratio) *
                        Matrix.CreateRotationZ((1 - Platform_Effect_Ratio) * Platform_Effect_Angle) *
                        Matrix.CreateTranslation(new Vector3(
                                Platform_Effect_2.loc.X,
                                Platform_Effect_2.loc.Y,
                                0) +
                            new Vector3(Platform_Effect_Ratio * -4, Platform_Effect_Ratio * 2, -0.5f)) *
                        Matrix.CreateScale(new Vector3(1, 1, 0.001f));
                }
            }
        }

        protected Matrix platform_effect_matrix(bool ranged, bool facing, float ratio)
        {
            float taper_ratio = Platform_Effect_Angle == -MathHelper.Pi ? Math.Max(0, ratio - 0.5f) * 2 : ratio;
            if (ranged)
                return new Matrix(1 + ratio * 7.2f, 0, 0, 0,
                               taper_ratio * 2.4f * (facing ? 1 : -1), 1 + ratio * 1.9f, 0, taper_ratio * -0.048f,
                               0, 0, 1 + ratio, 0,
                               0, 0, 0, 1 + ratio);
            else
                return new Matrix(Battler_2 == null ? 1 + ratio * 15.6f : 2 + ratio * 14.6f, 0, 0, 0,
                               0, 1 + ratio * 1.9f, 0, taper_ratio * -0.045f,
                               0, 0, 1 + ratio, 0,
                               0, 0, 0, 1 + ratio);
        }

        protected virtual void battle_start_text()
        {
            HUD.add_message("Battle Start", Battler_1.team);
        }

        protected virtual void play_battle_theme() { }

        protected virtual bool test_battle_theme()
        {
            return false;
        }

        protected void add_battle_action(Battle_Actions action)
        {
            Battle_Action.Add((int)action);
#if DEBUG
            Last_Added_Action = action;
#endif
        }
        protected void insert_battle_action(int index, Battle_Actions action)
        {
            Battle_Action.Insert(index, (int)action);
#if DEBUG
            Last_Added_Action = action;
#endif
        }
        protected void remove_battle_action(Battle_Actions action)
        {
            Battle_Action.Remove((int)action);
#if DEBUG
            Last_Removed_Action = action;
#endif
        }

        #region Phase Updates
        protected virtual void update_phase_0()
        {
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Timer)
                {
                    case 0:
                        initialize_battle_sprites();
                        Timer++;
                        break;
                    case 1:
                        Platform_Effect_1 = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                        Platform_Effect_1.offset = new Vector2(8, 8);
                        if (Battler_2 != null && Distance > 1)
                        {
                            Platform_Effect_2 = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                            Platform_Effect_2.offset = new Vector2(8, 8);
                        }
                        battle_sprite_setup(0);
                        Timer++;
                        break;
                    case 2:
                    case 3:
                        Battler_1_Sprite.visible = false;
                        if (Battler_2_Sprite != null)
                            Battler_2_Sprite.visible = false;
                        battle_sprite_setup(Timer - 3);
                        Timer++;
                        break;
                    case 4:
                        Battler_1_Sprite.visible = true;
                        Battler_1_Sprite.offset.Y = 120;
                        Battler_1_Sprite.update_animation_position();
                        if (Battler_2_Sprite != null)
                        {
                            Battler_2_Sprite.visible = true;
                            Battler_2_Sprite.offset.Y = 120;
                            Battler_2_Sprite.update_animation_position();
                        }
                        battle_sprite_setup(Timer - 3);
                        Timer++;
                        break;
                    case 5:
                        battle_sprite_setup(Timer - 3);
                        Timer++;
                        break;
                    case 6:
                        battle_sprite_setup(Timer - 3);
                        // HUD
                        setup_hud();
                        // Create platforms
                        create_platforms();
                        // White screen flash
                        White_Screen = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                        White_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                        White_Screen.visible = false;
                        Timer++;
                        break;
                    case 7:
                    case 8:
                        battle_sprite_setup(Timer - 3);
                        // 110 total platform movement
                        Platform.add_y(-(Timer + 3));
                        Timer++;
                        break;
                    case 9:
                    case 10:
                    case 11:
                        battle_sprite_setup(Timer - 3);
                        Platform.add_y(-(Timer + 6));
                        Timer++;
                        break;
                    case 12:
                    case 13:
                        battle_sprite_setup(Timer - 3);
                        Platform.add_y(-(Timer + 8));
                        Timer++;
                        break;
                    case 14:
                        battle_sprite_setup(Timer - 3);
                        Timer++;
                        Platform_Effect_1 = null;
                        Platform_Effect_2 = null;
                        break;
                    case 15:
                        Battler_1_Sprite.loc = new Vector2(Battler_1_Loc.X, 176);
                        Battler_1_Sprite.scale = Vector2.One;
                        Battler_1_Sprite.reset_pose();
                        Battler_1_Sprite.update_animation_position();
                        if (Battler_2_Sprite != null)
                        {
                            Battler_2_Sprite.loc = new Vector2(Battler_2_Loc.X, 176);
                            Battler_2_Sprite.scale = Vector2.One;
                            Battler_2_Sprite.reset_pose();
                            Battler_2_Sprite.update_animation_position();
                        }
                        Battler_1_Sprite.initialize_animation();
                        if (Battler_2_Sprite != null)
                            Battler_2_Sprite.initialize_animation();

                        Platform_Effect_1 = null;
                        Platform_Effect_2 = null;
                        Timer++;
                        break;
                    case 16:
                        Black_Fill = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                        Black_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                        if (is_battle_bg_visible)
                            Black_Fill.tint = new Color(0, 0, 0, 32);
                        else
                            Black_Fill.tint = new Color(0, 0, 0, 0);
                        Timer++;
                        break;
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                        if (is_battle_bg_visible)
                        {
                            Black_Fill.tint = new Color(0, 0, 0, (int)MathHelper.Min((Timer - 15) * 32, 255));
                            if (Timer > 19)
                                Weather_Opacity = (Timer - 19) / 16f;
                        }
                        Timer++;
                        break;
                    case 24:
                    case 25:
                        if (is_battle_bg_visible)
                            Weather_Opacity = (Timer - 19) / 16f;
                        Timer++;
                        break;
                    case 26:
                        // Create background
                        if (is_battle_bg_visible)
                        {
                            Background = new Sprite(background(this.background_battler));
                            Background.stereoscopic = Config.BATTLE_BG_DEPTH;
                            Black_Backing = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                            Black_Backing.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                            Black_Backing.tint = new Color(0, 0, 0, 255);

                            Weather_Opacity = (Timer - 19) / 16f;
                        }
                        else
                            cont = false;
                        Timer++;
                        break;
                    case 27:
                    case 28:
                    case 29:
                    case 30:
                    case 31:
                    case 32:
                    case 33:
                    case 34:
                    case 35:
                        if (is_battle_bg_visible)
                        {
                            Black_Fill.tint = new Color(0, 0, 0, (int)MathHelper.Min((35 - Timer) * 32, 255));
                            Weather_Opacity = (Timer - 19) / 16f;
                            //Weather_Opacity = (Timer - 27) / 8f;
                        }
                        else
                            cont = false;
                        Timer++;
                        break;
                    case 36:
                        battle_start_text();
                        if (!Music_Started && (!test_battle_theme() || !Global.game_temp.boss_theme))
                            Global.Audio.BgmFadeOut(60);
                        Global.game_temp.boss_theme = false;
                        Timer++;
                        break;
                    case 72:
                        if (!Music_Started)
                        {
                            play_battle_theme();
                            Music_Started = true;
                        }
                        Battler_1_Sprite.start_battle();
                        if (Battler_2_Sprite != null)
                            Battler_2_Sprite.start_battle();
                        Timer++;
                        break;
                    case 73:
                        if (Battler_1_Sprite.duration <= 2 && (Battler_2_Sprite == null || Battler_2_Sprite.duration <= 2))
                        {
                            check_talk(Battler_1, Battler_2, Reverse);
                            Timer++;
                        }
                        break;
                    case 74:
                        if (!is_message_window_active && HUD.is_ready && !Message_Active)
                        {
                            Timer = 0;
                            Phase = 1;
                        }
                        break;
                    default:
                        Timer++;
                        break;
                }
            }
        }

        protected virtual void update_phase_1() { }

        protected virtual void update_phase_2() { }

        protected virtual void update_phase_3()
        {
            if (!Global.game_system.is_loss() && (Combat_Data == null || !Combat_Data.game_over))
            {
                if (Timer == 0)
                    if (!Global.game_temp.boss_theme || Combat_Data.kill)
                    {
                        Global.Audio.BgmFadeOut(45);
                        Global.game_temp.boss_theme = false;
                    }
                if (Timer == 34)
                {
                    if (Global.game_temp.boss_theme)
                        Global.game_temp.boss_theme = false;
                    else
                        Global.game_state.resume_turn_theme(true);
                }
            }
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Timer)
                {
                    // Darken background
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        if (is_battle_bg_visible)
                        {
                            Black_Fill.tint = new Color(0, 0, 0, (int)MathHelper.Min((Timer + 1) * 32, 255));
                            Weather_Opacity = (15 - Timer) / 16f;
                        }
                        else
                            cont = false;
                        Timer++;
                        break;
                    // Dispose background
                    case 7:
                        White_Screen = null;
                        if (is_battle_bg_visible)
                        {
                            Black_Fill.tint = new Color(0, 0, 0, 255);
                            Background = null;
                            Black_Backing = null;

                            Weather_Opacity = (15 - Timer) / 16f;
                        }
                        else
                            cont = false;
                        Timer++;
                        break;
                    case 8:
                    case 9:
                        if (is_battle_bg_visible)
                            Weather_Opacity = (15 - Timer) / 16f;
                        else
                            cont = false;
                        Timer++;
                        break;
                    // Lighten background
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                        if (is_battle_bg_visible)
                        {
                            Black_Fill.tint = new Color(0, 0, 0, (int)MathHelper.Min((17 - Timer) * 32, 255));
                            if (Timer <= 15)
                                Weather_Opacity = (15 - Timer) / 16f;
                        }
                        else
                            cont = false;
                        Timer++;
                        break;
                    // Battlers move back to map sprites
                    case 17:
                        Black_Fill = null;
                        Battler_1_Sprite.loc.Y = 104;
                        Battler_1_Sprite.offset.Y = 120;
                        Battler_1_Loc = Battler_1_Sprite.loc;
                        if (Battler_2_Sprite != null)
                        {
                            Battler_2_Sprite.loc.Y = 104;
                            Battler_2_Sprite.offset.Y = 120;
                            Battler_2_Loc = Battler_2_Sprite.loc;
                        }
                        
                        Platform_Effect_1 = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                        Platform_Effect_1.offset = new Vector2(8, 8);
                        if (Battler_2 != null && Distance > 1)
                        {
                            Platform_Effect_2 = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                            Platform_Effect_2.offset = new Vector2(8, 8);
                        }

                        battle_sprite_setup(27 - Timer);
                        Timer++;
                        break;
                    // HUD moves off
                    case 18:
                        battle_sprite_setup(27 - Timer);
                        HUD.go_away();
                        Timer++;
                        break;
                    case 19:
                        battle_sprite_setup(27 - Timer);
                        Timer++;
                        break;
                    // Platforms start to move off
                    case 20:
                    case 21:
                        battle_sprite_setup(27 - Timer);
                        Platform.add_y(41 - Timer);
                        Timer++;
                        break;
                    case 22:
                    case 23:
                    case 24:
                        battle_sprite_setup(27 - Timer);
                        Platform.add_y(39 - Timer);
                        Timer++;
                        break;
                    case 25:
                    case 26:
                        battle_sprite_setup(27 - Timer);
                        Platform.add_y(37 - Timer);
                        Timer++;
                        break;
                    // Dispose platforms
                    case 27:
                        battle_sprite_setup(27 - Timer);
                        Platform = null;
                        Platform_Effect_1 = null;
                        Platform_Effect_2 = null;
                        Timer++;
                        break;
                    case 28:
                        Battler_1_Sprite.visible = false;
                        if (Battler_2_Sprite != null)
                            Battler_2_Sprite.visible = false;
                        Timer++;
                        break;
                    case 29:
                    case 30:
                    case 31:
                    case 32:
                    case 33:
                        Timer++;
                        break;
                    case 34:
                        battle_end();
                        Timer++;
                        break;
                }
            }
        }
        #endregion
    }
}
