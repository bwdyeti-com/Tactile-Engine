using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Scene_Promotion : Scene_Action
    {
        protected List<int> Unit_Ids = null;
        protected int OldClass, OldLevel;
        protected bool Combat_Scene_Fade_In = false, Preparations_Fade_In = false;
        protected bool Arena_Background = false, Arena_Scene = false, Preparations_Scene = false;

        public Scene_Promotion() { }

        #region Platform/Background
        protected override Texture2D platform(int tag, int distance)
        {
            if (Global.game_system.preparations)
                return battle_content.Load<Texture2D>(@"Graphics/Battlebacks/" + "Floor" + "-Melee"); //Debug
            else
                return base.platform(tag, distance);
        }

        protected override Texture2D background(Game_Unit battler)
        {
            if (Global.game_system.preparations)
                return battle_content.Load<Texture2D>(@"Graphics/Battlebacks/" + "Floor-BG"); //Debug
            else
                return base.background(battler);
        }
        #endregion

        protected override void initialize_base()
        {
            Scene_Type = "Scene_Promotion";
            main_window();
            camera = new Camera(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT, Vector2.Zero);

            if (Global.scene.is_action_scene)
            {
                Combat_Data = ((Scene_Action)Global.scene).combat_data;
                Combat_Scene_Fade_In = true;
                //Sparring
                if (Global.scene.scene_type == "Scene_Sparring")
                {
                    Arena_Scene = true;
                    Unit_Ids = ((Scene_Level_Up)Global.scene).level_up_battler_ids;
                }
                else if (Global.scene.scene_type == "Scene_Arena")
                {
                    Arena_Scene = true;
                    Arena_Background = true;
                }

                if (Arena_Scene)
                    Weather_Visible = BattleWeathers.Invisible;
                else if (Combat_Scene_Fade_In)
                    Weather_Opacity = 1f;
            }
            else if (Global.game_system.preparations)
            {
                Preparations_Fade_In = true;
                Preparations_Scene = true;
                Weather_Visible = BattleWeathers.Invisible;
            }
        }

        public override void initialize_action(int distance)
        {
            Phase = 0;
            Segment = 0;
            Timer = 0;
            if (Unit_Ids != null)
                Battler_1 = Global.game_map.units[Unit_Ids[0]];
            else
                Battler_1 = Global.game_map.units[Global.game_system.Class_Changer];
            if (Global.game_system.Class_Change_To == -1)
                Global.game_system.Class_Change_To = (int)Battler_1.actor.promotes_to();
            Reverse = true;
            Real_Distance = Distance = 1;
            Can_Skip = false;
        }

        protected override void setup_hud()
        {
            HUD = new Window_Battle_HUD(Battler_1.id, !Reverse, Distance);
        }

        protected override void play_battle_theme()
        {
            Global.game_state.play_promotion_theme();
        }

        protected override bool level_up_layer_resort()
        {
            return Level_Up_Action >= 3;
        }

        #region Phase Updates
        protected override void update_phase_0()
        {
            if (Combat_Scene_Fade_In || Preparations_Fade_In)
            {
                bool cont = false;
                while (!cont)
                {
                    cont = true;
                    int alpha;
                    if (Preparations_Fade_In)
                        switch (Timer)
                        {
                            case 0:
                                promotion_fade_in_initialize();
                                Skip_Fill.tint = new Color(0, 0, 0, 1f);
                                Timer++;
                                break;
                            case 34:
                                Skip_Fill = null;
                                Preparations_Fade_In = false;
                                Timer = 36;
                                break;
                            default:
                                if (Timer > 18)
                                {
                                    alpha = (34 - Timer) * 16;
                                    Skip_Fill.tint = new Color(0, 0, 0, alpha);
                                }
                                Timer++;
                                break;
                        }
                    else
                        switch (Timer)
                        {
                            case 0:
                                promotion_fade_in_initialize();
                                Skip_Fill.tint = new Color(1f, 1f, 1f, 1f);
                                Timer++;
                                break;
                            case 40:
                                Skip_Fill = null;
                                Combat_Scene_Fade_In = false;
                                Timer = 36;
                                break;
                            default:
                                if (Timer > 8)
                                {
                                    alpha = (40 - Timer) * 8;
                                    Skip_Fill.tint = new Color(alpha, alpha, alpha, alpha);
                                }
                                Timer++;
                                break;
                        }
                }
            }
            else
                base.update_phase_0();
        }

        protected override void update_phase_1()
        {
            base.update_phase_1();
            switch (Segment)
            {
                case 0:
                    Battler_1_Sprite.class_change_start();
                    darken();
                    Segment = 1;
                    break;
                case 1:
                    if (Battler_1_Sprite.duration <= 1 && Battler_1_Sprite.spell_effect_duration <= 1)
                    {
                        OldClass = Battler_1.actor.class_id;
                        OldLevel = Battler_1.actor.level;
                        Battler_1.actor.promotion_class_id = Global.game_system.Class_Change_To;
                        var battler1Data = new BattlerSpriteData(Battler_1);
                        Battler_1_Sprite = new Battler_Sprite(battler1Data, !Reverse, Distance, Reverse);
                        Battler_1_Sprite.start_battle();
                        Battler_1_Sprite.loc = Battler_1_Loc = new Vector2((!Reverse ? 184 + (Distance == 1 ? 0 : 56) :
                            136 - (Distance == 1 ? 0 : 56)) +
                            (Distance > 2 ? (Reverse ? -270 : 270) : 0), 176);
                        Battler_1_Sprite.reset_pose();
                        Battler_1_Sprite.update_animation_position();
                        Battler_1_Sprite.visible = true;
                        Battler_1_Sprite.class_change_end();
                        Battler_1_Sprite.stereoscopic = Config.BATTLE_BATTLERS_DEPTH;
                        Segment = 2;
                    }
                    break;
                case 2:
                    if (Battler_1_Sprite.duration >= 163)
                        brighten();
                    if (Battler_1_Sprite.duration <= 1 && Battler_1_Sprite.spell_effect_duration <= 1)
                        Segment = 3;
                    break;
                case 3:
                    switch (Timer)
                    {
                        case 104:
                            Phase = 2;
                            Segment = 0;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
            }
            // Updates screen flash
            Battler_1_Sprite.update_flash();
            update_spell_flash(Battler_1_Sprite);
        }

        protected override void update_phase_2()
        {
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Segment)
                {
                    // Promotion Window
                    case 0:
                        switch (Timer)
                        {
                            case 0:
                                promote(Battler_1.id, OldClass);
                                Timer++;
                                break;
                            case 1:
                                if (!is_leveling_up())
                                {
                                    Timer = 0;
                                    Segment++;
                                }
                                break;
                        }
                        break;
                    // If gained a skill
                    case 1:
                        switch (Timer)
                        {
                            case 0:
#if DEBUG
                                if (Battler_1.actor.level == 5)
                                    throw new System.Exception();
#endif
                                // If gained a skill
                                if (!promotion_skill_gain(Battler_1.id, OldClass, OldLevel))
                                    cont = true;
                                Timer++;
                                break;
                            case 1:
                                if (!is_skill_gaining())
                                {
                                    Timer++;
                                    cont = false;
                                }
                                break;
                            case 2:
                                Timer = 0;
                                Segment++;
                                break;
                        }
                        break;
                    // If weapon level up
                    case 2:
                        switch (Timer)
                        {
                            case 0:
                                // If weapon level up
                                if (Battler_1.actor.wlvl_up())
                                {
                                    wlvl_up(Battler_1.id);
                                }
                                else
                                    cont = true;
                                Timer++;
                                break;
                            case 1:
                                if (!is_wlvling_up())
                                {
                                    Timer++;
                                    cont = false;
                                }
                                break;
                            case 2:
                                Timer = 0;
                                Segment++;
                                break;
                        }
                        break;
                    // If weapon broke
                    case 3:
                        switch (Timer)
                        {
                            case 0:
                                // If weapon broke
                                if (Combat_Data != null && Combat_Data.weapon_1_broke)
                                {
                                    wbreak(new int[] { 0, Combat_Data.Weapon_1_Id });
                                }
                                else if (Combat_Data != null && Combat_Data.weapon_2_broke)
                                {
                                    wbreak(new int[] { 0, Combat_Data.Weapon_2_Id });
                                }
                                // Else continue
                                else
                                    cont = true;
                                Timer++;
                                break;
                            case 1:
                                if (!is_wbreaking())
                                {
                                    Timer++;
                                    cont = false;
                                }
                                break;
                            case 2:
                                Timer = 0;
                                Segment++;
                                break;
                        }
                        break;
                    // Wait before clearing
                    case 4:
                        switch (Timer)
                        {
                            case 61:
                                Global.game_system.Class_Change_To = -1;
                                Timer = 0;
                                Phase = 3;
                                break;
                            default:
                                Timer++;
                                break;
                        }
                        break;
                }
            }
            base.update_phase_2();
        }

        protected override void update_phase_3()
        {
            if (Arena_Scene || Preparations_Scene)
            {
                bool cont = false;
                while (!cont)
                {
                    cont = true;
                    switch (Timer)
                    {
                        // Darken background
                        case 0:
                            if (Unit_Ids != null)
                            {
                                Unit_Ids.RemoveAt(0);
                                if (Unit_Ids.Count == 0)
                                    Unit_Ids = null;
                            }
                            if (Unit_Ids == null)
                                Global.Audio.BgmFadeOut(45);
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
                            Skip_Fill.tint = new Color(0, 0, 0, Timer * 16);
                            Timer++;
                            break;
                        default:
                            if (Unit_Ids != null && Unit_Ids.Any())
                            {
                                Global.game_system.Class_Change_To = -1;
                                initialize_action(Distance);
                                Preparations_Fade_In = true;
                                return;
                            }
                            if ((Arena_Scene && Timer == 34) || (Preparations_Scene && Timer == 47))
                                battle_end();
                            Timer++;
                            break;
                    }
                }
            }
            else
                base.update_phase_3();
        }
        #endregion

        private void promotion_fade_in_initialize()
        {
            initialize_battle_sprites();
            Battler_1_Sprite.loc = new Vector2(Battler_1_Loc.X, 176);
            Battler_1_Sprite.scale = Vector2.One;
            Battler_1_Sprite.reset_pose();
            Battler_1_Sprite.update_animation_position();
            Battler_1_Sprite.visible = true;

            // HUD
            setup_hud();
            HUD.complete_move();
            // Create platforms
            create_platforms();
            Platform.add_y(-110);
            // White screen flash
            White_Screen = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
            White_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            White_Screen.visible = false;

            Black_Fill = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
            Black_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Fill.tint = new Color(0, 0, 0, 0);
            // Create background
            if (is_battle_bg_visible)
            {
                Background = new Sprite(background(this.background_battler));
                Background.stereoscopic = Config.BATTLE_BG_DEPTH;
                Black_Backing = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                Black_Backing.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                Black_Backing.tint = new Color(0, 0, 0, 255);
            }

            Skip_Fill = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
            Skip_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
        }

        protected override void update_promotion()
        {
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Level_Up_Action)
                {
                    // Darken screen
                    case 0:
                        switch (Level_Up_Timer)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                                //Black_Fill.tint = new Color(0, 0, 0, (Level_Up_Timer + 1) * 32);
                                Level_Up_Timer++;
                                break;
                            case 4:
                                Level_Up_Action++;
                                Level_Up_Timer = 0;
                                break;
                        }
                        break;
                    // Wait
                    case 1:
                        switch (Level_Up_Timer)
                        {
                            case 4:
                                Level_Up_Timer = 0;
                                Level_Up_Action = 3;
                                Level_Window = new Window_Promotion(level_up_content(), Global.game_map.units[Level_Up_Battler_Ids[0]].actor.id, Promotion_Old_Class_Id);
                                break;
                            default:
                                Level_Up_Timer++;
                                break;
                        }
                        break;
                    // Brings window onscreen
                    case 3:
                        switch (Level_Up_Timer)
                        {
                            case 0:
                                Level_Window.move_on();
                                Black_Fill.tint = new Color(0, 0, 0, (Level_Up_Timer + 1) * 16);
                                Level_Up_Timer++;
                                break;
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                Black_Fill.tint = new Color(0, 0, 0, (Level_Up_Timer + 1) * 16);
                                Level_Up_Timer++;
                                break;
                            case 12:
                                Level_Up_Action++;
                                Level_Up_Timer = 0;
                                break;
                            default:
                                Level_Up_Timer++;
                                break;
                        }
                        break;
                    // Waits before running
                    case 4:
                        switch (Level_Up_Timer)
                        {
                            case 31:
                                Level_Up_Action++;
                                Level_Up_Timer = 0;
                                Level_Window.execute = true;
                                break;
                            default:
                                Level_Up_Timer++;
                                break;
                        }
                        break;
                    // Continues when ready
                    case 5:
                        if (Level_Window.is_ready())
                        {
                            Level_Window.finish();
                            Level_Up_Action++;
                            Level_Up_Timer = 0;
                        }
                        break;
                    // Waits
                    case 6:
                        switch (Level_Up_Timer)
                        {
                            case 58:
                                Level_Up_Action++;
                                Level_Up_Timer = 0;
                                break;
                            default:
                                Level_Up_Timer++;
                                break;
                        }
                        break;
                    // Moves window offscreen
                    case 7:
                        switch (Level_Up_Timer)
                        {
                            case 0:
                                Level_Window.move_off();
                                Black_Fill.tint = new Color(0, 0, 0, (7 - Level_Up_Timer) * 16);
                                Level_Up_Timer++;
                                break;
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                Black_Fill.tint = new Color(0, 0, 0, (7 - Level_Up_Timer) * 16);
                                Level_Up_Timer++;
                                break;
                            case 12:
                                Level_Up_Action++;
                                Level_Up_Timer = 0;
                                cont = false;
                                break;
                            default:
                                Level_Up_Timer++;
                                break;
                        }
                        break;
                    // Brightens
                    case 8:
                        switch (Level_Up_Timer)
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                //Black_Fill.tint = new Color(0, 0, 0, (4 - Level_Up_Timer) * 32);
                                Level_Up_Timer++;
                                break;
                            case 18:
                                Level_Up_Action++;
                                Level_Up_Timer = 0;
                                break;
                            default:
                                Level_Up_Timer++;
                                break;
                        }
                        break;
                    // Finishes up
                    case 9:
                        Promoting = false;
                        Leveling_Up = false;
                        Level_Up_Action = 0;
                        Level_Up_Timer = 0;
                        Level_Up_Battler_Ids.RemoveAt(0);
                        Promotion_Old_Class_Id = -1;
                        if (Level_Window != null)
                            Level_Window = null;
                        break;
                }
            }
        }

        protected override void draw_scene(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            if (!Global.game_system.preparations)
                base.draw_scene(sprite_batch, device, render_targets);
        }
    }
}
