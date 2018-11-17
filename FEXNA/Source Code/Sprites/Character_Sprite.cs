using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Map
{
    internal class Character_Sprite : Matrix_Position_Sprite
    {
        public int facing_count = 4, frame_count = 4;
        //protected Map_Status_Effect Status_Effect;
        protected List<int> Statuses = new List<int>();
        protected int Status_Index = 0;
        protected Character_Hp_Gauge Hp_Gauge;
        protected Map_Unit_Animation_Data Animation;
        protected int Animation_Frame, Animation_Frame_Timer, Animation_Total_Time;
        protected bool Hold_Animation = true;

        protected static int Status_Timer = 0;

        #region Accessors
        protected int? status_id { get { return Statuses.Count == 0 ? null : (int?)Statuses[Status_Index]; } }

        public bool animation_active { get { return Animation != null; } }
        #endregion

        public Character_Sprite()
        {
            initialize(null);
        }

        public Character_Sprite(Texture2D texture)
        {
            initialize(texture);
        }

        protected virtual void initialize(Texture2D texture)
        {
            Hp_Gauge = new Character_Hp_Gauge();
            if (!(texture == null))
                this.texture = texture;
            animation_data = new List<int[]>() { };
            frame = 0;
        }

        public void update_status(List<int> states)
        {
            Status_Index = 0;
            List<int> statuses = new List<int>();
            //Status_Effect = null;
            foreach (int id in states)
                if (Map_Status_Effect.status_ids.Contains(id))
                    statuses.Add(id);
            Statuses = statuses;
        }

        public override void update()
        {
            update(null);
        }
        public void update(Map_Object unit)
        {
            base.update();
            if (Status_Timer == 0 && Statuses.Count > 1)
                Status_Index = (Status_Index + 1) % Statuses.Count;
            if (unit != null && unit is Game_Unit)
            {
                Hp_Gauge.update(unit as Game_Unit);
                if (Animation != null)
                    update_animation(unit as Game_Unit);
                else if (!Hold_Animation)
                    (unit as Game_Unit).update_sprite_frame(this);
            }
            else if (Animation != null)
                update_animation(null);
            //if (Status_Effect != null)
            //    Status_Effect.update();
        }

        public static void update_status_timer()
        {
            Status_Timer = (Status_Timer + 1) % Config.MAP_STATUS_EFFECT_TIME;
        }

        #region Animation
        internal void set_animation(Game_Unit unit, Map_Unit_Animation_Data animation)
        {
            Animation = animation;
            Animation_Frame = 0;
            Animation_Frame_Timer = 0;
            Animation_Total_Time = 0;
            process_animation_frame(unit);
        }

        protected void process_animation_frame(Game_Unit unit)
        {
            process_animation_frame(unit, Animation_Frame);
        }
        protected void process_animation_frame(Game_Unit unit, int frame)
        {
            // Change image
            if (Animation.data[frame].Change_Image)
            {
                if (unit != null)
                    unit.refresh_sprite(unit.actual_map_sprite_name + Animation.data[frame].Image_Name, Animation.data[frame].Moving);
#if DEBUG
                else
                    throw new NotImplementedException("Map object animation isn't set up for not-units yet");
#endif
                if ((int)Animation.data[frame].Image_Cells.X != 0 && (int)Animation.data[frame].Image_Cells.Y != 0)
                {
                    facing_count = (int)Animation.data[frame].Image_Cells.Y;
                    frame_count = (int)Animation.data[frame].Image_Cells.X;
                    offset = new Vector2(
                        (texture.Width / frame_count) / 2,
                        Math.Min((texture.Height / facing_count),
                            (texture.Height / (facing_count * 2)) + 20));
                }
            }
            this.frame = Animation.data[frame].Frame_Index;
        }

        protected void update_animation(Game_Unit unit)
        {
            if (Animation_Frame_Timer >= Animation.data[Animation_Frame].Time)
            {
                if (Animation.data.Count <= Animation_Frame + 1)
                {
                    end_animation(unit);
                    return;
                }
                else
                {
                    Animation_Frame++;
                    process_animation_frame(unit);
                    Animation_Frame_Timer = 0;
                }
            }
            Animation_Frame_Timer++;
            // Update sounds/effects
            Animation_Total_Time++;
            Map_Effect.process_timing(Animation_Total_Time, Animation.processing_data, null, loc);
        }

        protected void end_animation(Game_Unit unit)
        {
            Animation = null;
            Hold_Animation = true;
        }

        public void finish_animation()
        {
            if (Animation == null)
                Hold_Animation = false;
        }
        #endregion

        public override int frame
        {
            get { return current_frame; }
            set
            {
                current_frame = (int)MathHelper.Clamp(value, 0, frame_count * facing_count - 1);
            }
        }

        public override Rectangle src_rect
        {
            get
            {
                return new Rectangle(
                    current_frame % frame_count * (texture.Width / frame_count),
                    current_frame / frame_count * (texture.Height / facing_count),
                    texture.Width / frame_count,
                    texture.Height / facing_count);
            }
        }

        public void draw_status(SpriteBatch sprite_batch, Dictionary<int, Map_Status_Effect> effects, Vector2 draw_offset, Matrix matrix)
        {
            if (status_id != null && effects[(int)status_id] != null)
                effects[(int)status_id].draw(
                    sprite_batch,
                    draw_offset - (loc + new Vector2(Constants.Map.TILE_SIZE / 2)),
                    matrix);
        }

        public virtual void draw_hp(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix)
        {
            if (Global.game_options.hp_gauges != (int)Constants.Hp_Gauge_Modes.Off)
                Hp_Gauge.draw(
                    sprite_batch,
                    draw_offset - (loc + new Vector2(Constants.Map.TILE_SIZE / 2)),
                    matrix);
        }
    }
}
