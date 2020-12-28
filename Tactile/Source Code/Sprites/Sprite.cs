using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    enum Blend_Modes { Normal, Additive }
    class Sprite : Stereoscopic_Graphic_Object
    {
        public Texture2D Texture;
        public List<Texture2D> textures;

        protected List<Rectangle> frames = new List<Rectangle>();
        protected List<Vector2> frame_offsets = new List<Vector2>();
        protected Rectangle? Src_Rect, Dest_Rect;
        protected List<int[]> animation_data;
        protected int animation_frame = 0;
        //protected int frame_width = 0;
        //protected int frame_height = 0;
        protected int current_frame = 0;
        protected int frame_time = 1;

        protected Color Tint = Color.White;
        protected int Blend_Mode;
        public Vector2 scale = Vector2.One;
        protected bool Mirrored = false;
        public bool visible = true;
        protected float rotation = 0.0f;
        protected Dictionary<int, int> bitmap_data = new Dictionary<int, int>();

        protected int z = 0;

        #region Accessors
        public virtual Texture2D texture
        {
            get { return Texture; }
            set { Texture = value; }
        }

        public virtual float Z
        {
            get { return (1.0f / (z + 1)); }
            set { z = (int)Math.Max(0, value); }
        }

        public int blend_mode
        {
            get { return Blend_Mode; }
            set { Blend_Mode = (int)MathHelper.Clamp(value, 0, 1); } // 2? //Yeti
        }

        public virtual bool mirrored
        {
            get { return Mirrored; }
            set { Mirrored = value; }
        }

        public float angle
        {
            get { return rotation; }
            set { rotation = value % MathHelper.TwoPi; }
        }

        public virtual int frame
        {
            get { return current_frame; }
            set
            {
                current_frame = (int)MathHelper.Clamp(value, -1, frames.Count - 1);
                if (current_frame >= 0 && bitmap_data.Count > 0)
                {
                    Texture = textures[(int)bitmap_data[current_frame] - 1];
                }
                if (current_frame < 0)
                    offset = new Vector2(0, 0);
                else
                    offset = frame_offsets[current_frame];
            }
        }

        public virtual Color tint
        {
            get { return Tint; }
            set { Tint = value; }
        }
        public virtual byte TintA
        {
            get { return Tint.A; }
            set { Tint.A = value; }
        }
        public virtual int opacity
        {
            get { return this.tint.R; }
            set
            {
                byte opacity = (byte)MathHelper.Clamp(value, 0, 255);
                this.tint = new Color(opacity, opacity, opacity, Blend_Mode == (int)Blend_Modes.Additive ? 0 : opacity);
            }
        }
        #endregion

        public Sprite() { }

        public Sprite(Texture2D texture)
        {
            this.texture = texture;
        }

        public Sprite(Texture2D texture, Rectangle initial_frame, Vector2 offset)
        {
            initialize(texture, initial_frame, offset);
        }

        public Sprite(List<Texture2D> src_textures, Rectangle initial_frame, Vector2 offset, Dictionary<int, int> bitmap_data)
        {
            this.bitmap_data = bitmap_data;
            textures = src_textures;
            initialize(src_textures[0], initial_frame, offset);
        }

        public Sprite(Texture2D texture, Rectangle initial_frame)
        {
            initialize(texture, initial_frame, Vector2.Zero);
        }

        protected virtual void initialize(Texture2D texture, Rectangle initial_frame, Vector2 offset)
        {
            this.texture = texture;
            frames.Add(initial_frame);
            frame_offsets.Add(offset);
            animation_data = new List<int[]>() { };
            //frame_width = initial_frame.Width;
            //frame_height = initial_frame.Height;
            frame = 0;
        }

        public virtual void frame_set(int val)
        {
            frame_time = 0;
            animation_frame = (val) % animation_data.Count;
            frame = animation_data[animation_frame][0];
        }

        public void frame_set(int val, int time)
        {
            frame_set(val);
            frame_time = (time % animation_data[animation_frame][1]);
        }

        public void add_frame(Rectangle rect, Vector2 offset)
        {
            frames.Add(rect);
            frame_offsets.Add(offset);
        }

        public virtual Rectangle src_rect
        {
            get
            {
                if (Src_Rect != null)
                {
                    return (Rectangle)Src_Rect;
                }
                else if (frames.Count == 0)
                {
                    if (Texture == null)
                        return Rectangle.Empty;
                    return Texture.Bounds; // new Rectangle(0, 0, texture.Width, texture.Height); //Debug
                }
                else if (current_frame < 0)
                    return new Rectangle(0, 0, 0, 0);
                else
                    return frames[current_frame];
            }
            set { Src_Rect = value; }
        }

        public virtual Rectangle? dest_rect
        {
            set { Dest_Rect = value; }
        }

        public virtual void update()
        {
            if (animation_data != null)
            {
                // If using an animation
                if (animation_data.Count > 0)
                {
                    // Unless current frame is infinite
                    if (animation_data[animation_frame][1] > -1)
                    {
                        frame_time++;
                        if (frame_time >= animation_data[animation_frame][1])
                        {
                            next_frame();
                        }
                    }
                }
            }
        }

        protected virtual void next_frame()
        {
            frame_time = 0;
            animation_frame = (animation_frame + 1) % animation_data.Count;
            frame = animation_data[animation_frame][0];
        }

        public void set_animation(List<int[]> animation_data)
        {
            this.animation_data = animation_data;
            frame_time = 0;
            animation_frame = 0;
            if (animation_data.Count > 0)
                frame = animation_data[animation_frame][0];
        }

        public virtual void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            draw(sprite_batch, this.texture, draw_offset);
        }

        public virtual void draw(SpriteBatch sprite_batch, Texture2D texture, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    if (this.texture == null && src_rect == Rectangle.Empty)
                        src_rect = texture.Bounds;

                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    if (Dest_Rect != null)
                        sprite_batch.Draw(texture, (Rectangle)Dest_Rect,
                            src_rect, Tint, angle, Vector2.Zero,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    else
                        sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset,
                            src_rect, Tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }

    class Matrix_Position_Sprite : Sprite
    {
        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            draw(sprite_batch, draw_offset, Matrix.Identity);
        }
        public virtual void draw(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix)
        {
            if (this.texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    if (Dest_Rect != null)
                        sprite_batch.Draw(this.texture, (Rectangle)Dest_Rect,
                            src_rect, Tint, angle, Vector2.Zero,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    else
                        sprite_batch.Draw(this.texture, Vector2.Transform((loc + draw_vector()) - draw_offset, matrix),
                            src_rect, Tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
