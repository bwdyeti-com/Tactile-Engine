using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ListExtension;
using TactileRectangleExtension;
using TactileRenderTarget2DExtension;

namespace Tactile
{
    enum Battle_Actions { New_Attack, Prehit_Check, Hit_Check, Start_Attack, Skill_Activation, Defender_Skill_Activation, Animate_Attack,
        Wait_For_Hit, Magic_Wait_For_Hit, Spell_Start, Anima_Effect_End, Spell_Pan, Wait_For_Spell, On_Hit_Skill, Hit, Miss, Wait_For_Shake, Wait_For_Death,
        Death_Quote, Wait_For_Death_Quote, Wait_For_Miss, Wait_For_Pan, No_Damage_Pan, Pan_Hit_Over, Hit_Over, Life_Drain_Pause, Life_Drain_Pan,
        Wait_Life_Drain_Effect1, Wait_Life_Drain_Effect2, Wait_Life_Drain_Heal, Life_Drain_Pan_Back, No_Damage_Hit_Over, Wait_For_Return, Cleanup }
    enum BattleWeathers { Visible, Indoors, Invisible }
    partial class Scene_Action : Scene_Map
    {
        protected int Phase = 0;
        protected int Round = 1;
        protected int Segment = 0;
        protected List<int> Battle_Action = new List<int>();
        protected int Timer = 0;
        protected bool Attack_Active = false;
        protected int Active_Battler = 0;
        protected Game_Unit Battler_1, Battler_2;
        protected Combat_Data Combat_Data;
        protected bool Can_Skip = true;
        protected bool Skipping = false;
        protected int Skip_Timer = 0;
        protected Sprite Skip_Fill;
        protected BattleWeathers Weather_Visible = BattleWeathers.Visible;
        protected float Weather_Opacity = 0;

        protected Vector2 Layer_2_Shake, Layer_3_Shake, Layer_5_Shake;
        protected Vector2 Platform_1_Shake, Platform_2_Shake;
        protected Vector2 Pan_Vector, Effects_Pan_Vector;

        protected List<Vector2> Shake = new List<Vector2>();
        protected bool Shake_Reset = false;
        protected bool Mini_Shaking = false;
        protected List<int> Pan = new List<int>();
        protected List<int> Brightness = new List<int>();
        protected bool Bg_Magic_Darken = false;
        protected bool Magic_Brighten = false;
        protected bool Message_Active = false;
        protected  int Dying_Unit = 0;

        protected bool Reverse = false;
        protected int Distance = 1, Real_Distance = 1;

        protected Window_Battle_HUD HUD;
        protected Battler_Sprite Battler_1_Sprite, Battler_2_Sprite;
        protected Sprite Platform_Effect_1, Platform_Effect_2;
        protected float Platform_Effect_Ratio;
        protected Vector2 Platform_Loc_2;
        protected Matrix Platform_Matrix_1 = Matrix.Identity, Platform_Matrix_2 = Matrix.Identity;
        protected float Platform_Effect_Angle;

        RasterizerState ScissorRasterState = new RasterizerState { ScissorTestEnable = true };

        #region Accessors
        protected ContentManager battle_content { get { return Global.Content; } }

        public Combat_Data combat_data { get { return Combat_Data; } }

        protected override bool has_convo_scene_button { get { return false; } }
        #endregion

        public Scene_Action() { }

        public virtual void initialize_action(int distance) { }

        public override void update()
        {
            if (update_soft_reset())
                return;
            update_skip();
            if (!is_message_window_active && Message_Active)
            {
                HUD.move_on();
                Message_Active = false;
            }
            base.update();
            update_white_flash();
            if (!Skipping)
            {
                // Hit/Crit Spark
                update_hit_spark();
                if (Battler_1_Sprite != null)
                {
                    // Update battler 1's sprite
                    Battler_1_Sprite.update();
                    // Causes HUD effects when battler 1 activates a skill
                    /*if (Battler_1_Sprite.battler.actor.skill_flash) //Yeti
                    {
                        HUD.skill_flash(1);
                        HUD.add_message(Battler_1_Sprite.battler.actor.skill_name(), Battler_1_Sprite.battler.team, 90, "Yellow");
                        Battler_1_Sprite.battler.actor.skill_flash = false;
                    }*/
                    int battler_shake = Battler_1_Sprite.shake();
                    if (battler_shake > 0)
                        mini_shake(battler_shake);
                }
                if (Battler_2_Sprite != null)
                {
                    // Update battler 2's sprite
                    Battler_2_Sprite.update();
                    // Causes HUD effects when battler 2 activates a skill
                    /*if (Battler_2_Sprite.battler.actor.skill_flash) //Yeti
                    {
                        HUD.skill_flash(2);
                        HUD.add_message(Battler_2_Sprite.battler.actor.skill_name(), Battler_2_Sprite.battler.team, 90, "Yellow");
                        Battler_2_Sprite.battler.actor.skill_flash = false;
                    }*/
                    int battler_shake = Battler_2_Sprite.shake();
                    if (battler_shake > 0)
                        mini_shake(battler_shake);
                }
                switch (Phase)
                {
                    case 0:
                        update_phase_0();
                        break;
                    case 1:
                        update_phase_1();
                        break;
                    case 2:
                        update_phase_2();
                        break;
                    case 3:
                        update_phase_3();
                        break;
                }
                if (Shake_Reset)
                {
                    Layer_2_Shake = Vector2.Zero;
                    Layer_3_Shake = Vector2.Zero;
                    Layer_5_Shake = Vector2.Zero;
                    Platform_1_Shake = Vector2.Zero;
                    Platform_2_Shake = Vector2.Zero;
                    Shake_Reset = false;
                }
                update_shake();
                update_pan();
                if (Brightness.Count > 0)
                {
                    Black_Fill.TintA = (byte)Brightness.pop();
                }
                if (Weather_Visible == BattleWeathers.Visible && Phase >= 1 && Phase <= 2)
                {
                    float minimum = 0.25f;
                    if (Bg_Magic_Darken && Weather_Opacity > minimum)
                        Weather_Opacity = Math.Max(minimum, Weather_Opacity - 1 / 24f);
                    else if (!Bg_Magic_Darken && Weather_Opacity < 1)
                        Weather_Opacity = Math.Min(1, Weather_Opacity + 1 / 24f);
                }
                if (HUD != null)
                    HUD.update();
                // Miss effect
                update_miss_text();
                // No Damage effect
                update_nodamage_text();
            }
        }

        protected virtual void update_skip()
        {
            if (can_skip() && (Global.Input.triggered(Inputs.Start) || EventSkip))
            {
                Skipping = true;
                Can_Skip = false;
            }
            if (Skipping)
            {
                switch (Skip_Timer)
                {
                    case 0:
                        skip_audio_fade();
                        Skip_Fill = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                        Skip_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                        Skip_Fill.tint = new Color(0, 0, 0, (Skip_Timer + 1) * 32);
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        Skip_Fill.tint = new Color(0, 0, 0, (Skip_Timer + 1) * 32);
                        break;
                    case 7:
                        Skip_Fill.tint = new Color(0, 0, 0, 255);
                        Weather_Opacity = 0;
                        break;
                    case 24:
                        null_sprites_for_skip();
                        Skip_Fill.tint = new Color(0, 0, 0, 256 - (Skip_Timer - 23) * 16);
                        break;
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
                        Skip_Fill.tint = new Color(0, 0, 0, 256 - (Skip_Timer - 23) * 16);
                        break;
                    case 39:
                        end_skip();
                        Global.game_state.skip_battle_scene();
                        return;
                }
                Skip_Timer++;
            }
        }

        private void null_sprites_for_skip()
        {
            Battler_1_Sprite = null;
            Battler_2_Sprite = null;
            Platform = null;
            HUD = null;
            Background = null;
            Black_Backing = null;
            Hit_Spark = null;
            HitNumbers.Clear();
            White_Screen = null;
            Miss_Spark = null;
            NoDamage_Spark = null;
        }

        protected virtual void skip_audio_fade()
        {
            if (Global.Audio.IsTrackPlaying("BattleBgm"))
                if ((!Global.game_temp.boss_theme || Combat_Data.kill) && !Global.game_system.In_Arena)
                    Global.Audio.BgmFadeOut(30);
            Global.game_state.resume_turn_theme(true);
            Global.Audio.sfx_fade(30);
        }

        protected void end_skip()
        {
            Skip_Fill.tint = new Color(0, 0, 0, 0);
            Phase = 3;
            Timer = 34;
            Skipping = false;
        }

        protected virtual bool can_skip()
        {
            if (Phase == 0 || Message_Active || Global.game_temp.scripted_battle)
                return false;
            return Can_Skip;
        }

        protected virtual void update_shake() { }

        protected virtual void update_pan()
        {
            if (Pan.Count > 0)
            {
                int val = Pan.pop() * (Reverse ? 1 : -1);
                Pan_Vector.X += val;
            }
        }

        protected void update_white_flash()
        {
            if (White_Flash_Time > 0 && White_Screen != null)
            {
                White_Flash_Timer--;
                if (White_Flash_Timer <= 0)
                {
                    White_Flash_Time = 0;
                    White_Flash_Timer = 0;
                    White_Screen.visible = false;
                    White_Screen.opacity = 255;
                }
                else
                    White_Screen.opacity = 255 * White_Flash_Timer / White_Flash_Time;
            }
        }

        protected virtual void battle_end()
        {
            Global.scene_change("Scene_Map");
            Global.game_state.battle_ending = true;
            Global.game_map.ShowUnits();
        }

        protected virtual bool level_up_layer_resort()
        {
            return !Promoting && Level_Up_Action >= 3;
        }

        private Tuple<Battler_Sprite, bool> active_battler_sprite
        {
            get
            {
                switch (Active_Battler)
                {
                    case 1:
                        if (Battler_1_Sprite == null)
                            return null;
                        return new Tuple<Battler_Sprite, bool>(Battler_1_Sprite, false);
                    case 2:
                        if (Battler_2_Sprite == null)
                            return null;
                        return new Tuple<Battler_Sprite, bool>(Battler_2_Sprite, true);
                    default:
                        return null;
                }
            }
        }
        private IEnumerable<Tuple<Battler_Sprite, bool>> inactive_battler_sprites
        {
            get
            {
                // Last one drawn is on top, so draw the attacker last
                if (Active_Battler != 2 && Battler_2_Sprite != null)
                    yield return new Tuple<Battler_Sprite, bool>(Battler_2_Sprite, true);
                if (Active_Battler != 1 && Battler_1_Sprite != null)
                    yield return new Tuple<Battler_Sprite, bool>(Battler_1_Sprite, false);
            }
        }

        #region Overrides
        public override void create_levelup_spark(Vector2 loc)
        {
            create_action_levelup_spark(loc);
        }
        public override void create_promotion_spark(Vector2 loc)
        {
            create_action_promotion_spark(loc);
        }

        protected override void create_wlvl_popup(int weaponType, int newRank)
        {
            WLvl_Popup = new Weapon_Level_Popup(weaponType, newRank);
            WLvl_Popup.loc = new Vector2((Config.WINDOW_WIDTH - WLvl_Popup.Width) / 2, 64);
        }

        protected override void create_wbreak_popup()
        {
            WBreak_Popup = new Item_Break_Popup(WBreak_Item[1], WBreak_Item[0] == 1);
            WBreak_Popup.loc = new Vector2(96, 64);
        }

        protected override void draw_map_level_up(SpriteBatch sprite_batch) { }


        public override void new_message_window()
        {
            if (Message_Window == null)
                return;
            Message_Window.reset(true);
            HUD.go_away();
            Message_Active = true;
        }

        protected override void update_message()
        {
            update_message_skip_buttons();

            if (Message_Window != null)
            {
                bool active = is_message_window_active;
                Message_Window.update();
            }
        }

        protected override bool skip_convo_button_active
        {
            get
            {
                if (can_skip())
                    return true;

                return base.skip_convo_button_active;
            }
        }
        #endregion

        #region Draw
        public override void draw(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            base.draw(sprite_batch, device, render_targets);

            draw_battle(sprite_batch, device,
                render_targets[0], render_targets[1], render_targets[2],
                render_targets[3]);

            // Copy battle to render target 0
            render_targets[1].raw_copy_render_target(sprite_batch, device, render_targets[0]);

            // Menus (promotion choice menu in particular) draw under skip
            draw_menus(sprite_batch, device, render_targets);

            draw_skip(sprite_batch);
            draw_level_up(sprite_batch, Vector2.Zero);

            draw_message(sprite_batch, device, render_targets);
            //draw_skip(sprite_batch); //Debug
        }

        protected override void draw_scene(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            // Don't skip drawing the map if the battle scene doesn't cover the whole screen //Debug
            /*
            // If the background is covering up the map anyway, just return
            if (is_battle_bg_visible)
                if (Black_Backing  != null && Black_Backing.TintA == 255)
                    return;
            */

            base.draw_scene(sprite_batch, device, render_targets);
        }

        protected override void draw_units(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets, bool roof, HashSet<Vector2> roof_tiles)
        {
            // The map fades out to hide the units anyway, so we're just wasting processing
            return;
        }
        protected override void draw_units(SpriteBatch sprite_batch, bool rotated = false, bool roof = false, HashSet<Vector2> roof_tiles = null)
        {
            // The map fades out to hide the units anyway, so we're just wasting processing
            return;
        }
        protected override void draw_active_units(SpriteBatch sprite_batch)
        {
            // The map fades out to hide the units anyway, so we're just wasting processing
            return;
        }

        protected virtual void draw_skip(SpriteBatch sprite_batch)
        {
            if (Skip_Fill != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Skip_Fill.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        protected void draw_battle(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D mapRender,
            RenderTarget2D finalRender, RenderTarget2D tempRender, RenderTarget2D effectRender)
        {
            Effect battle_shader = Global.effect_shader();
            // Map is on render target 0, so don't use it for anything
            // Render target 1 will act as the 'result' target

            // Draws to target 1
            device.SetRenderTarget(finalRender);
            device.Clear(Color.Transparent);

            // If not doing level up foreground darken, draw the background now
            // Otherwise it happens at the end
            if (!level_up_layer_resort())
            {
                draw_background_render(sprite_batch, device, tempRender, mapRender, effectRender);

                // Draw map and background to final render
                effectRender.raw_copy_render_target(sprite_batch, device, finalRender);
            }

            draw_battle(sprite_batch, device, finalRender, tempRender, effectRender, battle_shader);

            // If doing level up foreground darken, now it's time to draw the background
            if (level_up_layer_resort())
            {
                draw_background_render(sprite_batch, device, finalRender, mapRender, tempRender);
            }

            // Draw map and background to final render
            device.SetRenderTarget(tempRender);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, null, null);
            sprite_batch.Draw(effectRender, Vector2.Zero, Color.White);
            sprite_batch.End();

            // Draw render result
            device.SetRenderTarget(finalRender);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            sprite_batch.Draw(mapRender, Vector2.Zero, Color.White);
            sprite_batch.End();

            //Rectangle scissor_rect = new Rectangle(16, 16,
            //    Config.WINDOW_WIDTH - 16 * 2,
            //    Config.WINDOW_HEIGHT - 16 * 2);
            Rectangle scissor_rect = new Rectangle(0, 0,
                Config.WINDOW_WIDTH,
                Config.WINDOW_HEIGHT);
            sprite_batch.GraphicsDevice.ScissorRectangle = scissor_rect;

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, ScissorRasterState);
            sprite_batch.Draw(tempRender, Vector2.Zero, Color.White);
            sprite_batch.End();
        }

        private void draw_battle(SpriteBatch sprite_batch, GraphicsDevice device,
            RenderTarget2D finalRender, RenderTarget2D tempRender, RenderTarget2D effectRender, Effect battle_shader)
        {
            device.SetRenderTarget(finalRender);

            if (Weather_Visible == BattleWeathers.Visible)
                draw_lower_weather(sprite_batch, Weather_Opacity);

            draw_battler_bg_effects(sprite_batch);

            // Platform
            device.SetRenderTarget(tempRender);
            device.Clear(Color.Transparent);
            draw_platform(sprite_batch);
            device.SetRenderTarget(finalRender);
            if (battle_shader != null)
            {
                battle_shader.CurrentTechnique = battle_shader.Techniques["Tone"];
                battle_shader.Parameters["tone"].SetValue(
                    Global.game_state.screen_tone.to_vector_4(
                        Global.BattleSceneConfig.ActionPlatformToneWeight / 255f));
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, battle_shader);
            sprite_batch.Draw(tempRender, Vector2.Zero, Color.White); // on promotion (and level up?) darken this tint //Yeti
            sprite_batch.End();

            // Platform effect
            device.SetRenderTarget(tempRender);
            device.Clear(Color.Transparent);

            platform_effect_setup();
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Platform_Matrix_1);
            if (Platform_Effect_1 != null)
                Platform_Effect_1.draw(sprite_batch, Vector2.Zero);
            //Platform_Effect_1.draw(sprite_batch, (Layer_2_Shake + Pan_Vector) + Platform_1_Shake);
            sprite_batch.End();
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Platform_Matrix_2);
            if (Platform_Effect_2 != null)
                Platform_Effect_2.draw(sprite_batch, Vector2.Zero);
            //Platform_Effect_2.draw(sprite_batch, (Layer_2_Shake + Pan_Vector) + Platform_2_Shake);
            sprite_batch.End();

            device.SetRenderTarget(finalRender);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            sprite_batch.Draw(tempRender, Vector2.Zero, new Color(160, 160, 160, 160));
            sprite_batch.End();

            // Battlers
            draw_battler_lower_effects(sprite_batch);

            BattleSpriteRenderer battler_renderer = new BattleSpriteRenderer(
                Reverse, Layer_3_Shake + Pan_Vector, Platform_1_Shake, Platform_2_Shake);
            battler_renderer.draw(sprite_batch, device,
                active_battler_sprite, inactive_battler_sprites,
                finalRender, tempRender, effectRender);

            device.SetRenderTarget(finalRender);

            if (Weather_Visible == BattleWeathers.Visible)
                draw_upper_weather(sprite_batch, Weather_Opacity);

            draw_battler_upper_effects(sprite_batch);

            draw_hud(sprite_batch);

            if (White_Screen != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                White_Screen.draw(sprite_batch);
                sprite_batch.End();
            }

            // If level up, darken parts of the screen
            // Always switch to render target 2 for this, it needs to act as a temporary final target for a bit
            device.SetRenderTarget(effectRender);
            device.Clear(Color.Transparent);

            draw_masked_screen_darken(sprite_batch, battle_shader, finalRender);
        }

        private void draw_masked_screen_darken(SpriteBatch sprite_batch, Effect battle_shader, Texture2D render)
        {
            // Draws the render normally
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            sprite_batch.Draw(render, Vector2.Zero, Color.White);
            sprite_batch.End();

            if (Black_Fill == null || !level_up_layer_resort())
                return;

            // Uses the render as a mask and draws a black screen
            if (battle_shader != null)
            {
                battle_shader.CurrentTechnique = battle_shader.Techniques["Mask"];
                battle_shader.Parameters["mask_rect"].SetValue(new Vector4(0, 0, render.Width, render.Height) /
                    new Vector4(render.Width, render.Height,
                    render.Width, render.Height));
                Vector2 mask_size_ratio = new Vector2(Config.WINDOW_WIDTH / (float)render.Width,
                    Config.WINDOW_HEIGHT / (float)render.Height);
                battle_shader.Parameters["mask_size_ratio"].SetValue(Vector2.One / mask_size_ratio);
                Vector2 sprite_offset = Vector2.Zero; //(Map_Sprites[Global.game_map.new_turn_unit_id].offset +
                //new Vector2(Map_Sprites[Global.game_map.new_turn_unit_id].src_rect.X, Map_Sprites[Global.game_map.new_turn_unit_id].src_rect.Y)) /
                //new Vector2(render.Width, render.Height);
                Vector2 effect_offset = Vector2.Zero; //Status_Heal.offset /
                //new Vector2(Status_Heal.src_rect.Width, Status_Heal.src_rect.Height);
                battle_shader.Parameters["alpha_offset"].SetValue(-(effect_offset * mask_size_ratio - sprite_offset));

#if __ANDROID__
                // There has to be a way to do this for both
                battle_shader.Parameters["Map_Alpha"].SetValue(render);
#else
                sprite_batch.GraphicsDevice.Textures[1] = render;
#endif
                sprite_batch.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp; //Yeti
            }

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, null, null, battle_shader);
            Black_Fill.draw(sprite_batch);
            sprite_batch.End();
        }

        private void draw_background_render(SpriteBatch sprite_batch, GraphicsDevice device,
            RenderTarget2D tempRender, RenderTarget2D mapRender, RenderTarget2D backgroundRender)
        {
            Effect battle_shader = Global.effect_shader();

            // Draw background to temporary target
            device.SetRenderTarget(tempRender);
            device.Clear(Color.Transparent);
            draw_background(sprite_batch);

            // Draw map to temp render
            device.SetRenderTarget(backgroundRender);
            device.Clear(Color.Transparent);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            sprite_batch.Draw(mapRender, Vector2.Zero, Color.White);
            sprite_batch.End();

            // Draw background to map with tone
            if (battle_shader != null)
            {
                battle_shader.CurrentTechnique = battle_shader.Techniques["Tone"];
                battle_shader.Parameters["tone"].SetValue(
                    Global.game_state.screen_tone.to_vector_4(
                        Global.BattleSceneConfig.ActionBackgroundToneWeight / 255f));
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, null, null, battle_shader);
            sprite_batch.Draw(tempRender, Vector2.Zero, Color.White);
            sprite_batch.End();
        }

        protected void draw_background(SpriteBatch sprite_batch)
        {
            if (Background != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Black_Backing.draw(sprite_batch);
                Background.draw(sprite_batch);
                sprite_batch.End();
            }
            if (Black_Fill != null && !level_up_layer_resort())
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Black_Fill.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        protected void draw_battler_bg_effects(SpriteBatch sprite_batch)
        {
            Effect battle_shader = Global.effect_shader();
            if (Battler_1_Sprite != null) Battler_1_Sprite.draw_bg_effects(sprite_batch, battle_shader, Pan_Vector);
            if (Battler_2_Sprite != null) Battler_2_Sprite.draw_bg_effects(sprite_batch, battle_shader, Pan_Vector);
        }

        protected void draw_platform(SpriteBatch sprite_batch)
        {
            Vector2 offset = Layer_2_Shake + Pan_Vector;

            if (Platform == null)
                return;
            Platform.draw(sprite_batch, offset + Platform_1_Shake, offset + Platform_2_Shake);
        }

        /*protected void draw_layer_battlers(SpriteBatch sprite_batch, int layer) //Debug
        {

            if (Active_Battler == 2)
            {
                if (Battler_1_Sprite != null && Battler_1_Sprite.layer == layer) Battler_1_Sprite.draw_lower(sprite_batch, offset + p1_shake, battle_shader);
                if (Battler_2_Sprite != null && Battler_2_Sprite.layer == layer) Battler_2_Sprite.draw_lower(sprite_batch, offset + p2_shake, battle_shader);
                if (Battler_1_Sprite != null && Battler_1_Sprite.layer == layer) Battler_1_Sprite.draw(sprite_batch, offset + p1_shake, battle_shader);
                if (Battler_2_Sprite != null && Battler_2_Sprite.layer == layer) Battler_2_Sprite.draw(sprite_batch, offset + p2_shake, battle_shader);
                if (Battler_1_Sprite != null && Battler_1_Sprite.layer == layer) Battler_1_Sprite.draw_upper(sprite_batch, offset + p1_shake, battle_shader);
                if (Battler_2_Sprite != null && Battler_2_Sprite.layer == layer) Battler_2_Sprite.draw_upper(sprite_batch, offset + p2_shake, battle_shader);
            }
            else
            {
                if (Battler_2_Sprite != null && Battler_2_Sprite.layer == layer) Battler_2_Sprite.draw_lower(sprite_batch, offset + p2_shake, battle_shader);
                if (Battler_1_Sprite != null && Battler_1_Sprite.layer == layer) Battler_1_Sprite.draw_lower(sprite_batch, offset + p1_shake, battle_shader);
                if (Battler_2_Sprite != null && Battler_2_Sprite.layer == layer) Battler_2_Sprite.draw(sprite_batch, offset + p2_shake, battle_shader);
                if (Battler_1_Sprite != null && Battler_1_Sprite.layer == layer) Battler_1_Sprite.draw(sprite_batch, offset + p1_shake, battle_shader);
                if (Battler_2_Sprite != null && Battler_2_Sprite.layer == layer) Battler_2_Sprite.draw_upper(sprite_batch, offset + p2_shake, battle_shader);
                if (Battler_1_Sprite != null && Battler_1_Sprite.layer == layer) Battler_1_Sprite.draw_upper(sprite_batch, offset + p1_shake, battle_shader);
            }
        }*/

        protected void draw_battler_lower_effects(SpriteBatch sprite_batch)
        {
            Effect battle_shader = Global.effect_shader();
            if (Battler_1_Sprite != null) Battler_1_Sprite.draw_lower_effects(sprite_batch, battle_shader, Pan_Vector, Effects_Pan_Vector);
            if (Battler_2_Sprite != null) Battler_2_Sprite.draw_lower_effects(sprite_batch, battle_shader, Pan_Vector, Effects_Pan_Vector);
        }

        protected void draw_battler_upper_effects(SpriteBatch sprite_batch)
        {
            Effect battle_shader = Global.effect_shader();
            if (Battler_1_Sprite != null) Battler_1_Sprite.draw_upper_effects(sprite_batch, battle_shader, Pan_Vector, Effects_Pan_Vector);
            if (Battler_2_Sprite != null) Battler_2_Sprite.draw_upper_effects(sprite_batch, battle_shader, Pan_Vector, Effects_Pan_Vector);
        }

        protected void draw_hud(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            if (Hit_Spark != null)
                Hit_Spark.draw(sprite_batch);
            Vector2 number_vector = (Layer_3_Shake + Platform_1_Shake + Platform_2_Shake) * 0.5f + Pan_Vector;
            foreach (var number in HitNumbers)
                number.draw(sprite_batch, new Vector2((int)number_vector.X, (int)number_vector.Y));
            sprite_batch.End();
            if (HUD != null)
                HUD.draw(sprite_batch, Layer_5_Shake);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            if (Miss_Spark != null)
                Miss_Spark.draw(sprite_batch, Pan_Vector);
            if (NoDamage_Spark != null)
                NoDamage_Spark.draw(sprite_batch, Pan_Vector);
            sprite_batch.End();
            if (Exp_Gauge != null)
            {
                sprite_batch.GraphicsDevice.ScissorRectangle = fix_rect_to_screen(Exp_Gauge.scissor_rect());
                if (sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0 && sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0)
                {
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Exp_Gauge.raster_state);
                    Exp_Gauge.draw(sprite_batch);
                    sprite_batch.End();
                }
            }
            Effect battle_shader = Global.effect_shader();
            if (Battler_1_Sprite != null) Battler_1_Sprite.draw_fg_effects(sprite_batch, battle_shader, Pan_Vector);
            if (Battler_2_Sprite != null) Battler_2_Sprite.draw_fg_effects(sprite_batch, battle_shader, Pan_Vector);

            draw_skill_gain_popup(sprite_batch);
            draw_wlvl_popup(sprite_batch);
            draw_wbreak_popup(sprite_batch);
        }

        protected override void draw_map_combat(SpriteBatch sprite_batch) { }
        #endregion

        #region Dispose
        public override void dispose()
        {
            base.dispose();
        }
        #endregion
    }
}
