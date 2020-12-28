using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ListExtension;

namespace Tactile
{
    class Scene_Arena : Scene_Battle
    {
        protected int Arena_Timer = 0;
        protected int Arena_Combat_Round = 0;
        protected bool Retreat = false;

        public Scene_Arena() { }

        protected override void initialize_base()
        {
            Scene_Type = "Scene_Arena";
            main_window();
            camera = new Camera(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT, Vector2.Zero);
            Weather_Visible = BattleWeathers.Invisible;

            Global.game_state.arena_load();
        }

        public override void initialize_action(int distance)
        {
            Battler_1 = Global.game_map.units[Global.game_system.Battler_1_Id];
            Battler_2 = Global.game_map.units[Global.game_system.Battler_2_Id];

            Battler_1.preload_animations(distance);
            Battler_2.preload_animations(distance);

            Combat.battle_setup(Battler_1.id, Battler_2.id, distance, 0);
            new_combat_data(distance);
            Reverse = Battler_2.is_opposition;
            Real_Distance = Distance = distance;
            //if (Global.data_weapons[Combat_Data.Weapon_1_Id].Max_Range <= 2 && Distance > 2)
            if (!Global.data_weapons[weapon_id(1)].Long_Range && Distance > 2) //Debug
                Distance = 2;
            if (Distance == 2)
                Pan_Vector = Effects_Pan_Vector = new Vector2(16 * (Reverse ? -1 : 1), 0);
            Can_Skip = false;
        }

        #region Platform/Background
        protected override Texture2D platform(int tag, int distance)
        {
            return null;
        }

        protected override Texture2D background(Game_Unit battler)
        {
            return battle_content.Load<Texture2D>(@"Graphics/Battlebacks/" + "Arena-BG");
        }

        protected void update_bg()
        {
            if (Background != null)
                ((Arena_Background)Background).update();
        }
        #endregion

        protected override void update_pan()
        {
            if (Pan.Count > 0)
            {
                int val = Pan.pop() * (Reverse ? 1 : -1);
                if (Distance != 2)
                    Pan_Vector.X += val;
                else
                    Effects_Pan_Vector.X -= val;
            }
        }

        protected override bool is_last_attack()
        {
            return false;
        }

        protected override bool is_next_attacker_same()
        {
            if (Combat_Data.Data.Count == Attack_Id + 1)
                return Combat_Data.Data[Attack_Id].Key.Attacker == Combat_Data.Data[0].Key.Attacker;
            else
                return Combat_Data.Data[Attack_Id].Key.Attacker == Combat_Data.Data[Attack_Id + 1].Key.Attacker;
        }

        public override void update()
        {
            update_bg();
            base.update();
        }

        protected override void battle_start_text() {}

        protected override void play_battle_theme()
        {
            base.play_battle_theme();
        }

        protected virtual void crowd_cheer()
        {
            Global.Audio.play_bgs("Crowd Cheering");
        }

        protected virtual void end_crowd_cheer()
        {
            Global.Audio.stop_bgs();
            Global.Audio.play_se("Battle Sounds", "Crowd_Cheer_End");
        }

        #region Phase Updates
        protected override void update_phase_0()
        {
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Timer)
                {
                    // Initialize battler sprites
                    case 0:
                        initialize_battle_sprites();
                        Battler_1_Sprite.loc = new Vector2(Battler_1_Loc.X, 176);
                        Battler_1_Sprite.scale = Vector2.One;
                        Battler_1_Sprite.reset_pose();
                        Battler_1_Sprite.visible = true;
                        if (Battler_2_Sprite != null)
                        {
                            Battler_2_Sprite.loc = new Vector2(Battler_2_Loc.X, 176);
                            Battler_2_Sprite.scale = Vector2.One;
                            Battler_2_Sprite.reset_pose();
                            Battler_2_Sprite.visible = true;
                        }
                        Skip_Fill = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                        Skip_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                        Skip_Fill.tint = new Color(0, 0, 0, 255);
                        Timer++;
                        break;
                    case 1:
                    case 2:
                    case 3:
                        Timer++;
                        break;
                    case 4:
                        Timer++;
                        break;
                    case 5:
                        Timer++;
                        break;
                    case 6:
                        // hud
                        setup_hud();
                        while (!HUD.is_ready)
                            HUD.update();
                        // Create platforms
                        create_platforms();
                        // White screen flash
                        White_Screen = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                        White_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                        White_Screen.visible = false;
                        // Create background
                        Background = new Arena_Background(background(this.background_battler));
                        Black_Backing = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                        Black_Backing.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                        Black_Backing.tint = new Color(0, 0, 0, 255);
                        Timer++;
                        break;
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
                        Skip_Fill.tint = new Color(0, 0, 0, (23 - Timer) * 16);
                        Timer++;
                        break;
                    case 24:
                        Skip_Fill = null;
                        Black_Fill = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                        Black_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                        Black_Fill.tint = new Color(0, 0, 0, 0);
                        if (Combat_Data != null)
                        {
                            Battler_1_Sprite.initialize_animation();
                            if (Battler_2_Sprite != null)
                                Battler_2_Sprite.initialize_animation();
                        }
                        Timer++;
                        break;
                    case 36:
                        Global.Audio.BgmFadeOut(60);
                        crowd_cheer();
                        battle_start_text();
                        Timer++;
                        break;
                    case 72:
                        play_battle_theme();
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

        protected override void update_phase_1()
        {
            if (Arena_Timer <= 0)
            {
                update_yield();
                base.update_phase_1();
            }
            else
            {
                Arena_Timer--;
                if (Arena_Timer == 0)
                {
                    // Load new combat data
                    new_combat_data();
                }
            }
        }

        protected virtual void update_yield()
        {
            bool yield = Global.Input.triggered(Inputs.B);
            if (yield && !Retreat)
                if (Combat_Data.Hp1 > 0 && Combat_Data.Hp2 > 0)
                    HUD.add_message("Yield", Battler_1.team, 120, "Grey");
            Retreat |= yield;
        }

        protected virtual void new_combat_data()
        {
            new_combat_data(Combat_Data.Distance, false);
        }
        protected virtual void new_combat_data(int distance)
        {
            new_combat_data(distance, true);
        }
        protected virtual void new_combat_data(int distance, bool initial)
        {
            Arena_Combat_Round++;
            int wexp_1 = 0, wexp_2 = 0;
            if (Combat_Data != null)
            {
                wexp_1 = Combat_Data.Wexp1;
                wexp_2 = Combat_Data.Wexp2;
            }
            Combat_Data = new Arena_Combat_Data(Battler_1.id, Battler_2.id, distance);
            Combat_Data.Wexp1 += wexp_1;
            Combat_Data.Wexp2 += wexp_2;
            Global.game_state.combat_data = Combat_Data;
            if (!initial)
            {
                Attack_Id = 0;
                HUD.combat_data = Combat_Data;
                refresh_stats();
            }
        }

        protected override void apply_combat()
        {
            if (Combat_Data.Hp1 > 0 && Combat_Data.Hp2 > 0)
            {
                Combat_Data.apply_combat(false, true);
                if (Retreat)
                    Global.game_system.Arena_Retreat = true;
                else
                {
                    // should Cancel propagate between rounds? //Yeti
                    Phase = 1;
                    Segment = 0;
                    Timer = 0;
                    Arena_Timer = 30;
                    suspend();
                }
            }
            else
                Combat_Data.apply_combat();
        }

        protected override bool exp_gained()
        {
            if (Global.game_system.Arena_Retreat)
                return false;
            else
                return base.exp_gained();
        }

        protected override void update_phase_3()
        {
            if (Timer == 0)
            {
                if (!Global.game_system.is_loss())
                    Global.Audio.BgmFadeOut(45);
                //Audio.bgs_fade(60); //Yeti
            }
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Timer)
                {
                    // Darken background
                    case 0:
                        Skip_Fill = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                        Skip_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                        Skip_Fill.tint = new Color(0, 0, 0, 0);
                        Timer++;
                        break;
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
                        if (!Skipping)
                            Skip_Fill.tint = new Color(0, 0, 0, Timer * 16);
                        Timer++;
                        break;
                    case 34:
                        battle_end();
                        Timer++;
                        break;
                    default:
                        Timer++;
                        break;
                }
            }
        }
        #endregion

        protected override void battle_end()
        {
            arena_weapon_reset();
            base.battle_end();
        }

        protected virtual void arena_weapon_reset()
        {
            Battler_1.actor.equip(Battler_1.actor.equipped);
            Global.game_state.combat_data = null;
        }

        protected override void change_to_promotion()
        {
            arena_weapon_reset();
            base.change_to_promotion();
        }

        protected override void battler_kill(Battler_Sprite battler_sprite)
        {
            base.battler_kill(battler_sprite);
            end_crowd_cheer();
        }
    }
}
