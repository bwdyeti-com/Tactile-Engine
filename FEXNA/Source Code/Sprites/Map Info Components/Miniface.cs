using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Miniface : Sprite
    {
        const string DEFAULT_FLAGS = "Bern Flags";

        protected Color[] Palette;
        private bool Has_Palette;
        private string Filename;
        protected Texture2D Bg_Texture, Flag_Texture;
        protected Rectangle Bg_Rect, Flag_Rect;

        public Miniface()
        {
            initialize_palette();
            offset.X = Face_Sprite_Data.MINI_FACE_SIZE.X / 2;
        }

        public override bool mirrored
        {
            get { return Mirrored; }
            set
            {
                Mirrored = value;
                RefreshSrcRect();
            }
        }

        protected void initialize_palette()
        {
            Palette = new Color[Palette_Handler.PALETTE_SIZE];
        }

        protected void refresh_palette(string filename)
        {
            //Has_Palette = Global.face_palette_data.ContainsKey(filename); //Debug
            Has_Palette = Global.face_palette_data.ContainsKey(filename) && Global.face_palette_data[filename].Length > 0;
            if (Has_Palette)
            {
                for (int i = 0; i < Global.face_palette_data[filename].Length; i++)
                    Palette[i] = Global.face_palette_data[filename][i];
            }
        }

        public void set_actor(string actor_name)
        {
            reset();
            if (!Global.content_exists(@"Graphics/Faces/" + actor_name))
            {
                if (Global.content_exists(@"Graphics/Faces/" + actor_name.Split(Constants.Actor.ACTOR_NAME_DELIMITER)[0]))
                    actor_name = actor_name.Split(Constants.Actor.ACTOR_NAME_DELIMITER)[0];
                else
                    return;
            }

            SetFilename(actor_name);
        }

        public void set_actor(Game_Actor actor)
        {
            reset();
            if (actor != null)
            {
                string actor_name = actor.face_name;
                if (!Global.content_exists(@"Graphics/Faces/" + actor_name))
                    if (Global.content_exists(@"Graphics/Faces/" + actor_name.Split(Constants.Actor.ACTOR_NAME_DELIMITER)[0]))
                        actor_name = actor_name.Split(Constants.Actor.ACTOR_NAME_DELIMITER)[0];

                if (Global.content_exists(@"Graphics/Faces/" + actor_name))
                {
                    SetFilename(actor_name);
                }

                // Generic background/flags
                if (actor.generic_face)
                {
                    Mirrored = false;
                    Bg_Texture = Global.Content.Load<Texture2D>(@"Graphics/Faces/Countries/Default_Miniface");
                    Bg_Rect = new Rectangle(32 * actor.tier, 0, 32, 32);
                    Flag_Texture = flag_texture(actor);
                    Flag_Rect = new Rectangle(32 * actor.build, 0, 32, 32);
                }
            }
        }

        private void SetFilename(string filename)
        {
            Filename = filename;
            texture = Global.Content.Load<Texture2D>(@"Graphics/Faces/" + Filename);
            RefreshSrcRect();
            refresh_palette(Filename);
        }

        protected void reset()
        {
            Filename = "";
            texture = null;
            Bg_Texture = null;
            Flag_Texture = null;
            Mirrored = true;
        }
        
        private void RefreshSrcRect()
        {
            Src_Rect = new Rectangle(this.SrcX, this.SrcY,
                (int)Face_Sprite_Data.MINI_FACE_SIZE.X, (int)Face_Sprite_Data.MINI_FACE_SIZE.Y);
        }

        private int SrcX
        {
            get
            {
                var faceData = Face_Sprite.get_face_data(Filename);

                int x = 0;
                if (faceData.Asymmetrical)
                {
                    int width = this.texture.Width / 2;
                    x = Mirrored ? width : 0;
                }
                return x;
            }
        }
        private int SrcY
        {
            get
            {
                return this.texture.Height - (int)Face_Sprite_Data.MINI_FACE_SIZE.Y;
            }
        }

        protected Texture2D flag_texture(Game_Actor actor)
        {
            string flag = actor.flag_name;
            string format = @"Graphics/Faces/Countries/{0}";

            if (Global.content_exists(string.Format(format, flag)))
                return Global.Content.Load<Texture2D>(string.Format(format, flag));
#if !DEBUG
            else if (Global.content_exists(string.Format(format, DEFAULT_FLAGS)))
                return Global.Content.Load<Texture2D>(string.Format(format, DEFAULT_FLAGS));
#endif
            return null;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 offset = this.offset;
            // Bg
            if (Bg_Texture != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                sprite_batch.Draw(Bg_Texture, loc + draw_vector() - draw_offset,
                    Bg_Rect, tint, angle, offset, scale,
                    mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                sprite_batch.End();
            }
            // Face
            if (Has_Palette)
            {
                Effect effect = Global.effect_shader();
                if (effect != null)
                {
                    Texture2D palette_texture = Global.palette_pool.get_palette();
                    palette_texture.SetData<Color>(Palette);
#if __ANDROID__
                    // There has to be a way to do this for both
                    effect.Parameters["Palette"].SetValue(palette_texture);
#else
                    sprite_batch.GraphicsDevice.Textures[2] = palette_texture;
#endif
                    sprite_batch.GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
                    effect.CurrentTechnique = effect.Techniques["Palette1"];
                    effect.Parameters["color_shift"].SetValue(new Vector4(0, 0, 0, 0));
                    effect.Parameters["opacity"].SetValue(1f);
                }
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect);
            }
            else
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            base.draw(sprite_batch, draw_offset);

            sprite_batch.End();
#if __ANDROID__
            // There has to be a way to do this for both
            if (Global.effect_shader() != null)
                Global.effect_shader().Parameters["Palette"].SetValue((Texture2D)null);
#else
            sprite_batch.GraphicsDevice.Textures[2] = null;
#endif
            // Flags
            if (Flag_Texture != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                sprite_batch.Draw(Flag_Texture, loc + draw_vector() - draw_offset,
                    Flag_Rect, tint, angle, offset, scale,
                    mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                sprite_batch.End();
            }
        }
    }
}
