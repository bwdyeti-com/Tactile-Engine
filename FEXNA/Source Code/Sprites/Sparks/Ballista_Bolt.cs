using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Map
{
    class Ballista_Bolt : Map_Effect
    {
        const int FORCE = 5;//4;//70;
        const float GRAVITY = 9.8f;
        const float MAX_DIST = (FORCE * FORCE) / GRAVITY;
        const float SCALE = MAX_DIST / (5120f);
        const float MAX_HEIGHT_SCALE_MULT = 2f;

        protected float Velocity;
        protected float Angle;
        protected float Distance, Total_Distance = 256;
        protected float Shadow_Angle, Shadow_Length;
        protected Vector2 Src_Loc, Dest_Loc;
        protected bool Falling_Sound_Played = false;
        protected Vector2 Scroll;

        #region Accessors
        protected float height
        {
            get
            {
                return (float)(((Distance * SCALE * Math.Tan(Angle) -
                    ((GRAVITY * Math.Pow(Distance * SCALE, 2)) / (2 * Math.Pow(FORCE * Math.Cos(Angle), 2)))) / 1) / SCALE) * HEIGHT_SCALE;
            }
        }

        protected Vector2 map_loc
        {
            get
            {
                Vector2 direction = (Dest_Loc - Src_Loc);
                direction.Normalize();
                return ((Src_Loc + direction * Distance) * Constants.Map.TILE_SIZE) /
                    Constants.Map.UNIT_TILE_SIZE;
            }
        }

        protected Vector2 bolt_loc { get { return map_loc + new Vector2(0, -height); } }
        #endregion

        //static float HEIGHT_SCALE { get { return (2 / 3f) / (256f / Config.TILE_SIZE); } }
        static float HEIGHT_SCALE { get { return (2 * Constants.Map.TILE_SIZE) / (768f); } }
        static int FALL_SOUND_HEIGHT { get { return (int)(1600 * HEIGHT_SCALE); } }

        static float max_height;
        public Ballista_Bolt(Vector2 src_loc, Vector2 dest_loc)
        {
            max_height = 0;
            Texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/Ballista_Bolt");
            Global.Audio.play_se("Map Sounds", "Ballista_Fire");
            //Texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Src_Loc = src_loc * Constants.Map.UNIT_TILE_SIZE +
                new Vector2(Constants.Map.UNIT_TILE_SIZE) / 2;
            Dest_Loc = dest_loc * Constants.Map.UNIT_TILE_SIZE +
                new Vector2(Constants.Map.UNIT_TILE_SIZE) / 2;

            if (Src_Loc == Dest_Loc)
            {
                Angle = 0;
                Velocity = 0;
                Total_Distance = 0;
            }
            else
            {
                Total_Distance = (Dest_Loc - Src_Loc).Length() * SCALE;
                offset = new Vector2(texture.Width / 2, texture.Height / 2);
                //Angle = (float)Math.Asin(FORCE * GRAVITY / (Math.Pow(Total_Distance, 2))) / 2;
                Angle = (float)Math.Atan(
                    (Math.Pow(FORCE, 2) + Math.Pow(Math.Pow(FORCE, 4) - (Math.Pow(GRAVITY * Total_Distance, 2)), 0.5f)) /
                        (GRAVITY * Total_Distance));
                if (float.IsNaN(Angle))
                {
                    Angle = MathHelper.PiOver4;
                }
                Shadow_Angle = -(MathHelper.PiOver4 * 4) + (float)Math.Atan2(Src_Loc.Y - Dest_Loc.Y, Src_Loc.X - Dest_Loc.X);
                Velocity = (float)((Math.Cos(Angle) * (FORCE / (float)Config.FRAME_RATE)) / SCALE);
            }

            Scroll = Global.game_map.scroll_dist(dest_loc);
            int time = (int)Math.Ceiling((Total_Distance / SCALE) / Velocity);
            Scroll /= time;

            refresh();
        }

        public override Rectangle src_rect
        {
            get
            {
                return new Rectangle(0, 0, texture.Width, texture.Width);
            }
        }

        public override void update()
        {
            update_map_scroll();
            Distance += (Velocity);
            refresh();
            max_height = Math.Max(height, max_height);
            if (Distance >= Total_Distance / SCALE)
                Finished = true;
        }

        protected void update_map_scroll()
        {
            float temp_dist = Distance;
            Distance += (Velocity);
            Vector2 temp_loc = map_loc;
            Distance = temp_dist;
            //Global.game_map.scroll_loc(temp_loc - map_loc);
            Global.game_map.scroll_loc(Scroll);
        }

        protected void refresh()
        {
            float temp_dist = Distance;
            Distance += (Velocity);
            Vector2 temp_loc = bolt_loc;
            Vector2 temp_map_loc = map_loc;
            float temp_height = height;
            Distance = temp_dist;
            // Fix angle
            angle = -(MathHelper.PiOver4 * 4) + (float)Math.Atan2(bolt_loc.Y - temp_loc.Y, bolt_loc.X - temp_loc.X);
            // Fix scale
            float scale_factor = (float)Math.Atan2(Src_Loc.Y - Dest_Loc.Y, Src_Loc.X - Dest_Loc.X);
            if (scale_factor > 0)
                scale_factor -= MathHelper.TwoPi;
            scale_factor =  Math.Abs(scale_factor + MathHelper.Pi);
            scale_factor = Math.Abs(scale_factor - MathHelper.PiOver2);
            float vel = (temp_loc - bolt_loc).Length();
            scale = Vector2.One * 0.5f;
            scale.X *= MathHelper.Clamp((scale_factor / MathHelper.PiOver2) + (vel / FORCE) * (1 - (scale_factor / MathHelper.PiOver2)), 4 / 16f, 1f);
            // Shadow length
            scale_factor = (float)Math.Atan2(height - temp_height, -Velocity);
            if (scale_factor > 0)
                scale_factor -= MathHelper.TwoPi;
            scale_factor = Math.Abs(scale_factor + MathHelper.Pi);
            scale_factor = Math.Abs(scale_factor - MathHelper.PiOver2);
            Shadow_Length = 0.25f + 0.25f * MathHelper.Clamp(scale_factor / MathHelper.PiOver2, 4 / 16f, 1f);
            if (!Falling_Sound_Played && temp_height < height && height < FALL_SOUND_HEIGHT)
            {
                Global.Audio.play_se("Map Sounds", "Ballista_Fall");
                Falling_Sound_Played = true;
            }
            float ratio = ((Distance < (Total_Distance / SCALE) / 2) ?
                Distance  :
                ((Total_Distance / SCALE) - Distance)) /
                    ((Total_Distance / SCALE) / 2);
            stereoscopic = ratio * Config.MAP_BALLISTA_HIGHEST_DEPTH + (1 - ratio) * Config.MAP_MAP_DEPTH;
            scale *= ratio * MAX_HEIGHT_SCALE_MULT + (1 - ratio) * 1f;
        }

        protected Vector2 shadow_stereo_offset()
        {
            return Stereoscopic_Graphic_Object.graphic_draw_offset(Config.MAP_MAP_DEPTH);
        }
        protected Vector2 shadow_draw_vector()
        {
            return draw_offset + shadow_stereo_offset();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix)
        {
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;

                    // shouldn't the shadow draw before the bolt //Debug
                    Vector2 shadow_scale = new Vector2(Shadow_Length, 0.5f);
                    int alpha = 16 + Math.Min(48, 48 - (int)(height / 3));
                    sprite_batch.Draw(texture, Vector2.Transform(
                        (map_loc + shadow_draw_vector() + new Vector2(-height * (0 / 4), 0)) - draw_offset, matrix), //Debug
                        src_rect, new Color(0, 0, 0, alpha), Shadow_Angle, offset, shadow_scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);

                    sprite_batch.Draw(texture, Vector2.Transform(
                        (bolt_loc + draw_vector()) - draw_offset, matrix),
                        src_rect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
