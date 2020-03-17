using System;
using System.Collections.Generic;
using System.Linq;
using FEXNA_Library;

namespace FEXNAWeaponExtension
{
    public static class Extension
    {
        internal static bool can_heal(this Data_Weapon weapon, FEXNA.Game_Unit target)
        {
            if (weapon.Heals())
                if (!target.actor.is_full_hp())
                    return true;
            if (healable_statuses(weapon, target).Count > 0)
                return true;
            return false;
        }
        internal static List<int> healable_statuses(this Data_Weapon weapon, FEXNA.Game_Unit target)
        {
            return weapon.Status_Remove.Intersect(target.actor.negative_states).ToList();
        }

        public static bool has_anima_start(this Data_Weapon weapon)
        {
            return (FEXNA_Library.Data_Weapon.ANIMA_TYPES.Contains((int)weapon.Main_Type) && new List<int> { 1, 2, 3 }.Contains((int)weapon.Rank)) ||
                (FEXNA_Library.Data_Weapon.ANIMA_TYPES.Contains((int)weapon.Scnd_Type) && new List<int> { 1, 2, 3, 4, 5 }.Contains((int)weapon.Rank)); //Debug
        }

        public static int anima_type(this Data_Weapon weapon)
        {
            return Data_Weapon.ANIMA_TYPES.Contains((int)weapon.Main_Type) ? weapon.Main_Type : weapon.Scnd_Type;
        }

        internal static float effective_multiplier(
            this Data_Weapon weapon,
            FEXNA.Game_Unit unit,
            FEXNA.Game_Unit target,
            bool halveOnHealingTerrain = true)
        {
            int effectiveness = 1;
            if (target == null)
                return effectiveness;
            // Skills: Nullify
            if (target.actor.has_skill("NULL"))
            {
                return effectiveness;
            }

            foreach (FEXNA_Library.ClassTypes type in target.actor.actor_class.Class_Types)
            {
                if (weapon.Effectiveness[(int)type] > effectiveness)
                    effectiveness = weapon.Effectiveness[(int)type];
            }
            // Skills: Smite
            if (unit != null && unit.actor.has_skill("SMITE"))
            {
                if (target != null && !unit.nihil(target))
                    if (target.actor.weapon != null && !weapon.is_staff() && (
                        target.actor.weapon.main_type().Name == "Dark" ||
                        target.actor.weapon.scnd_type().Name == "Dark"))
                        //target.actor.weapon.Main_Type == FEXNA_Library.Weapon_Types.Dark || //Debug
                        //target.actor.weapon.Scnd_Type == FEXNA_Library.Weapon_Types.Dark))
                effectiveness = Math.Max(2, effectiveness);
            }
            
            float result = effectiveness;
            if (halveOnHealingTerrain)
            {
                if (target.halve_effectiveness() && effectiveness > 1)
                    result = 1 + (effectiveness - 1) / 2f;
            }
            return result;
        }

        public static bool is_siege(this Data_Weapon weapon)
        {
            return weapon.Long_Range; // weapon.Max_Range >= 10 || weapon.Mag_Range; //Debug
        }

        public static bool range_blocked_by_walls(this Data_Weapon weapon)
        {
            return FEXNA.Constants.Gameplay.BLOCK_FIRE_THROUGH_WALLS_DEFAULT && !weapon.is_siege();
        }
    }
}

namespace FEXNABattleFrameDataExtension
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    public static class Extension
    {
        public static List<string> get_sounds(this Battle_Frame_Data frame, int tick)
        {
            List<string> sound_names = new List<string>();
            foreach(Sound_Data sound in frame.sounds)
            {
                if (sound.Key == tick)
                    sound_names.Add(sound.Value);
            }
            return sound_names;
        }

        #region Draw
        public static Maybe<Rectangle> src_rect(this Battle_Frame_Data frame, Texture2D texture, int frame_id, Frame_Data frame_data)
        {
            Rectangle src_rect;
            if (frame_data != null)
            {
                if (frame_id >= frame_data.offsets.Count)
                    return default(Maybe<Rectangle>);
                src_rect = frame_data.src_rects[frame_id];
            }
            else
            {
                src_rect = new Rectangle(FEXNA.Config.BATTLER_SIZE * (frame_id % 5), FEXNA.Config.BATTLER_SIZE * (frame_id / 5),
                    FEXNA.Config.BATTLER_SIZE, FEXNA.Config.BATTLER_SIZE);
            }
            return src_rect;
        }

        public static IEnumerable<BattleFrameRenderData> draw_lower(this Battle_Frame_Data frame,
            Texture2D texture, Frame_Data frame_data, SpriteBatch sprite_batch,
            Battle_Animation_Data animation_data, int animation_frame_id, int tick,
            Vector2 draw_offset, Matrix matrix, Vector2 offset,
            Vector2 scale, bool mirrored, bool reverse, int opacity, int blend_mode,
            Maybe<Color> tint = default(Maybe<Color>))
        {
            if (tint.IsNothing)
                tint = Color.White;
            for (int layer = 0; layer < frame.Lower_Frames.Count; layer++)
            {
                var render = frame.draw(texture, frame_data, sprite_batch,
                    animation_data, animation_frame_id, tick,
                    draw_offset, matrix, offset, scale, mirrored, reverse, opacity, blend_mode, tint,
                    layer, frame.Lower_Frames[layer]);
                if (render.IsSomething)
                    yield return render.ValueOrDefault;
            }
        }


        public static IEnumerable<BattleFrameRenderData> draw_upper(this Battle_Frame_Data frame,
            Texture2D texture, Frame_Data frame_data, SpriteBatch sprite_batch,
            Battle_Animation_Data animation_data, int animation_frame_id, int tick,
            Vector2 draw_offset, Matrix matrix, Vector2 offset,
            Vector2 scale, bool mirrored, bool reverse, int opacity, int blend_mode,
            Maybe<Color> tint = default(Maybe<Color>))
        {
            if (tint.IsNothing)
                tint = Color.White;
            for (int layer = 0; layer < frame.Upper_Frames.Count; layer++)
            {
                var render = frame.draw(texture, frame_data, sprite_batch,
                    animation_data, animation_frame_id, tick,
                    draw_offset, matrix, offset, scale, mirrored, reverse, opacity, blend_mode, tint,
                    layer + frame.Lower_Frames.Count, frame.Upper_Frames[layer]);
                if (render.IsSomething)
                    yield return render.ValueOrDefault;
            }
        }

        public static Maybe<BattleFrameRenderData> draw(this Battle_Frame_Data frame,
            Texture2D texture, Frame_Data frame_data, SpriteBatch sprite_batch,
            Battle_Animation_Data animation_data, int animation_frame_id, int tick,
            Vector2 draw_offset, Matrix matrix, Vector2 offset,
            Vector2 scale, bool mirrored, bool reverse, int opacity, int blend_mode, Color tint,
            int layer, Battle_Frame_Image_Data data)
        {
            int frame_id = animation_data.image_index(animation_frame_id, tick, layer);
            if (frame_id < 0 || (frame_data != null && frame_id >= frame_data.offsets.Count))
                return default(Maybe<BattleFrameRenderData>);
            Maybe<Rectangle> src_rect = frame.src_rect(texture, frame_id, frame_data);
            if (src_rect.IsNothing)
                return default(Maybe<BattleFrameRenderData>);

            draw_offset -= (offset - new Vector2(FEXNA.Config.BATTLER_SIZE / 2));
            offset = Vector2.Zero;
            Vector2 data_loc = animation_data.image_location(animation_frame_id, tick, layer);
            Vector2 data_scale = animation_data.image_scale(animation_frame_id, tick, layer);
            float frame_opacity = (animation_data.image_opacity(animation_frame_id, tick, layer) / 255f) * (opacity / 255f);
            float rotation = (animation_data.image_rotation(animation_frame_id, tick, layer) / 180.0f) * MathHelper.Pi;

            // Blend mode
            blend_mode = blend_mode != 0 ? blend_mode : data.blend_mode;
            float frame_alpha = frame_opacity / 255f;
            switch (blend_mode)
            {
                case 0: // Normal
                case 2: // Distortion
                    tint = new Color(tint.R * frame_alpha, tint.G * frame_alpha, tint.B * frame_alpha, tint.A * frame_alpha);
                    break;
                // Additive
                case 1:
                    tint = new Color(tint.R * frame_alpha, tint.G * frame_alpha, tint.B * frame_alpha, 0);
                    break;
                default:
                    tint = new Color(tint.R * frame_alpha, tint.G * frame_alpha, tint.B * frame_alpha, tint.A * frame_alpha);
                    break;
            }
            Vector2 frame_vector = Vector2.Zero;
            if (frame_data != null)
            {
                frame_vector = new Vector2(frame_data.offsets[frame_id].X, frame_data.offsets[frame_id].Y);
                //draw_offset -= frame_vector;
                offset = frame_vector;// new Vector2(FEXNA.Config.BATTLER_SIZE / 2) - frame_vector;
                //frame_vector.X = (reverse ^ mirrored) ? (FEXNA.Config.BATTLER_SIZE - (frame_data.src_rects[frame_id].Width + frame_vector.X)) : frame_vector.X;
                //if (reverse ^ mirrored)
                //{ }//draw_offset.X += (FEXNA.Config.BATTLER_SIZE - (frame_data.src_rects[frame_id].Width + frame_vector.X));
            }
            else
            {
                offset = new Vector2(FEXNA.Config.BATTLER_SIZE / 2);
                //draw_offset -= frame_vector;
            }
            if (mirrored ^ data.flipped)
            {
                offset.X = ((Rectangle)src_rect).Width - offset.X;
                rotation *= -1;
            }
            return new BattleFrameRenderData(texture, Vector2.Transform((data_loc * scale * new Vector2(mirrored ? -1 : 1, 1)) + draw_offset, matrix),
            //sprite_batch.Draw(texture, Vector2.Transform((data_loc * scale * new Vector2((mirrored ^ data.flipped) ? -1 : 1, 1)) + draw_offset, matrix),
            
            //sprite_batch.Draw(texture, Vector2.Transform((data_loc * new Vector2(reverse ? -1 : 1, 1)) + draw_offset, matrix),
                src_rect, tint, rotation, offset, data_scale * scale,
                mirrored ^ data.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0, blend_mode);

            /*sprite_batch.Draw(texture, Vector2.Transform((data_loc * scale * new Vector2(mirrored ? -1 : 1, 1)) + draw_offset, matrix), //Debug
            //sprite_batch.Draw(texture, Vector2.Transform((data_loc * scale * new Vector2((mirrored ^ data.flipped) ? -1 : 1, 1)) + draw_offset, matrix),
            
            //sprite_batch.Draw(texture, Vector2.Transform((data_loc * new Vector2(reverse ? -1 : 1, 1)) + draw_offset, matrix),
                (Rectangle)src_rect, tint, rotation, offset, data_scale * scale,
                mirrored ^ data.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);*/
        }
        #endregion
    }
}