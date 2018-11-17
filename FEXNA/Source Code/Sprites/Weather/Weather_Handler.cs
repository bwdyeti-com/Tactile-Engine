using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    enum Weather_Types { Clear, Rain, Snow }
    internal abstract class Weather_Handler : Sprite
    {
        protected readonly static Random RAND = new Random();

        protected Vector2[] Sprite_Locs;
        protected Rectangle[] Sprite_Rects;
        protected bool Off_Frame;
        protected float Angle;

        protected Weather_Handler()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Weather");
            Sprite_Locs = new Vector2[sprite_count() * 2];
            Sprite_Rects = new Rectangle[sprite_count() * 2];
        }

        protected virtual Vector2 new_sprite_loc(Vector2 draw_offset, bool initial) { return Vector2.Zero; }
        protected int height_slope { get { return (int)(Config.WINDOW_HEIGHT / Angle); } }

        protected virtual int sprite_count()
        {
            return Config.DEFAULT_WEATHER_SPRITE_COUNT;
        }

        public virtual void update(Vector2 draw_offset)
        {
            Off_Frame = !Off_Frame;
        }

        protected override Vector2 stereo_offset()
        {
            if (Stereo_Offset.IsNothing)
                return Vector2.Zero;
            return Stereoscopic_Graphic_Object.graphic_draw_offset(
                Stereo_Offset + (Off_Frame ? Config.MAP_WEATHER_OFF_FRAME_DEPTH_OFFSET : 0));
        }

        public void draw(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix, float opacity)
        {
            draw_lower(sprite_batch, draw_offset, matrix, opacity);
            draw_upper(sprite_batch, draw_offset, matrix, opacity);
        }
        public abstract void draw_upper(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix, float opacity);
        public abstract void draw_lower(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix, float opacity);
    }
}
