using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNARenderTarget2DExtension;

namespace FEXNA
{
    public enum BattlerRenderCommands { NormalDraw, DistortionDraw, Finish }

    class BattleSpriteRenderer
    {
        private Vector2 P1Shake, P2Shake;

        public BattleSpriteRenderer(bool reverse, Vector2 offset, Vector2 platform_1_shake, Vector2 platform_2_shake)
        {
            P1Shake = offset + (reverse ? platform_2_shake : platform_1_shake);
            P2Shake = offset + (reverse ? platform_1_shake : platform_2_shake);
        }

        public void draw(SpriteBatch sprite_batch, GraphicsDevice device,
            Tuple<Battler_Sprite, bool> active_battler_sprite,
            IEnumerable<Tuple<Battler_Sprite, bool>> inactive_battler_sprites,
            RenderTarget2D final_render, RenderTarget2D temp_render, RenderTarget2D effect_render)
        {
            Effect battle_shader = Global.effect_shader();

            // Draw to a temporary target
            device.SetRenderTarget(temp_render);
            device.Clear(Color.Transparent);

            BattlerRenderCommands current_command = BattlerRenderCommands.NormalDraw;
            foreach (BattlerRenderCommands command in draw_battlers(
                sprite_batch, active_battler_sprite, inactive_battler_sprites))
            {
                if (command != current_command)
                {
                    // Apply distortion effect
                    if (current_command == BattlerRenderCommands.DistortionDraw && command != current_command)
                    {
                        // Start drawing to effect_render
                        device.SetRenderTarget(effect_render);
                        device.Clear(Color.Transparent);

                        // Draw final_render, using temp_render as a distortion map
                        if (battle_shader != null)
                        {
                            battle_shader.CurrentTechnique = battle_shader.Techniques["Distortion"];
                            float larger_dimension = Math.Max(temp_render.Width, temp_render.Height);
                            Vector2 mask_size_ratio = new Vector2(temp_render.Width / larger_dimension,
                                temp_render.Height / larger_dimension);
                            battle_shader.Parameters["mask_size_ratio"].SetValue(mask_size_ratio);

#if __ANDROID__
                            // There has to be a way to do this for both
                            battle_shader.Parameters["Map_Alpha"].SetValue(temp_render);
#else
                            sprite_batch.GraphicsDevice.Textures[1] = temp_render;
#endif
                            sprite_batch.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp; //Yeti

                            //battle_shader.CurrentTechnique = battle_shader.Techniques["Tone"];
                            //battle_shader.Parameters["tone"].SetValue(Global.game_state.screen_tone.to_vector_4(Config.ACTION_BATTLER_TONE_WEIGHT / 255f));
                        }
                        sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, battle_shader);
                        sprite_batch.Draw(final_render, Vector2.Zero, Color.White); // on promotion (and level up?) darken this tint //Yeti
                        sprite_batch.End();

                        // Copy effect_render to final_render
                        effect_render.raw_copy_render_target(sprite_batch, device, final_render);

                        // Start drawing to temp_render
                        device.SetRenderTarget(temp_render);
                        device.Clear(Color.Transparent);
                    }

                    if (command == BattlerRenderCommands.NormalDraw) { }
                    else if (command == BattlerRenderCommands.DistortionDraw)
                    {
                        // Copy to final target with tone
                        draw_battler_tone(sprite_batch, device, final_render, temp_render, battle_shader);

                        // Start drawing to temp_render
                        device.SetRenderTarget(temp_render);
                        device.Clear(Color.Transparent);
                    }
                    else if (command == BattlerRenderCommands.Finish)
                    {
                        break;
                        //draw_battler_tone(sprite_batch, device, final_render, temp_render, battle_shader);
                    }
                    current_command = command;
                }
            }

            // Copy to final target with tone
            draw_battler_tone(sprite_batch, device, final_render, temp_render, battle_shader);
        }

        private void draw_battler_tone(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D final_render, RenderTarget2D temp_render, Effect battle_shader)
        {
            device.SetRenderTarget(final_render);
            if (battle_shader != null)
            {
                battle_shader.CurrentTechnique = battle_shader.Techniques["Tone"];
                battle_shader.Parameters["tone"].SetValue(
                    Global.game_state.screen_tone.to_vector_4(
                        Constants.BattleScene.ACTION_BATTLER_TONE_WEIGHT / 255f));
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, battle_shader);
            sprite_batch.Draw(temp_render, Vector2.Zero, Color.White); // on promotion (and level up?) darken this tint //Yeti
            sprite_batch.End();
        }

        protected IEnumerable<BattlerRenderCommands> draw_battlers(SpriteBatch sprite_batch,
            Tuple<Battler_Sprite, bool> active_battler_sprite,
            IEnumerable<Tuple<Battler_Sprite, bool>> inactive_battler_sprites)
        {
            Effect battle_shader = Global.effect_shader();

            // Draw avoiding targets on the bottom
                if (inactive_battler_sprites != null)
                foreach (var battler in inactive_battler_sprites)
                    if (battler.Item1.avoiding)
                    {
                        Vector2 shake = battler.Item2 ? P2Shake : P1Shake;
                        foreach (BattlerRenderCommands command in battler.Item1.draw_lower(sprite_batch, shake, battle_shader))
                            yield return command;
                    }
            // Draw active battler lower frames
            if (active_battler_sprite != null)
            {
                Vector2 shake = active_battler_sprite.Item2 ? P2Shake : P1Shake;
                foreach (BattlerRenderCommands command in active_battler_sprite.Item1.draw_lower(sprite_batch, shake, battle_shader))
                    yield return command;
            }
            // Draw targets in the middle
            if (inactive_battler_sprites != null)
                foreach (var battler in inactive_battler_sprites)
                    if (!battler.Item1.avoiding)
                    {
                        Vector2 shake = battler.Item2 ? P2Shake : P1Shake;
                        foreach (BattlerRenderCommands command in battler.Item1.draw_lower(sprite_batch, shake, battle_shader))
                            yield return command;
                        foreach (BattlerRenderCommands command in battler.Item1.draw_upper(sprite_batch, shake, battle_shader))
                            yield return command;
                    }
            // Draw active battler lower frames
            if (active_battler_sprite != null)
            {
                Vector2 shake = active_battler_sprite.Item2 ? P2Shake : P1Shake;
                foreach (BattlerRenderCommands command in active_battler_sprite.Item1.draw_upper(sprite_batch, shake, battle_shader))
                    yield return command;
            }
            // Draw avoiding targets on the top
            if (inactive_battler_sprites != null)
                foreach (var battler in inactive_battler_sprites)
                    if (battler.Item1.avoiding)
                    {
                        Vector2 shake = battler.Item2 ? P2Shake : P1Shake;
                        foreach (BattlerRenderCommands command in battler.Item1.draw_upper(sprite_batch, shake, battle_shader))
                            yield return command;
                    }
            yield return BattlerRenderCommands.Finish;
        }
    }
}
