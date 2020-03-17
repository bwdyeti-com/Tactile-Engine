using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Status_Face : Face_Sprite
    {
        public Status_Face(Game_Unit unit) : this(unit.actor) { }
        public Status_Face(Game_Actor actor)
        {
            string filename = actor.face_name;
            initialize(filename, false);
            phase_in();
            if (texture != null)
            {
                if (actor.generic_face)
                    recolor_country(actor.name_full);
                offset = new Vector2(Face_Sprite_Data.STATUS_FACE_SIZE.X / 2, Face_Sprite_Data.STATUS_FACE_SIZE.Y);

                RefreshSrcRect();
            }
        }

        protected override void RefreshSrcRect()
        {
            int x = (int)status_offset.X + this.SrcX;
            //@Debug: Maybe just set Expression instead, and then use this.SrcY?
            int srcY = Face_Sprite_Data.EmotionHeight(this.face_data, texture.Height) *
                (status_frame >= Emotion_Count ? 0 : status_frame);
            int y = (int)status_offset.Y + srcY;
            int width = (int)Face_Sprite_Data.STATUS_FACE_SIZE.X;
            int height = (int)Face_Sprite_Data.STATUS_FACE_SIZE.Y;
            Src_Rect = new Rectangle(x, y, width, height);
        }

        public override void update() { }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    if (palette_exists)
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

                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    
                    if (Dest_Rect != null)
                        sprite_batch.Draw(texture, (Rectangle)Dest_Rect,
                            src_rect, tint, angle, offset,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    else
                        sprite_batch.Draw(texture, this.loc + draw_vector() - draw_offset,
                            src_rect, tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);

                    sprite_batch.End();
#if __ANDROID__
                    // There has to be a way to do this for both
                    if (Global.effect_shader() != null)
                        Global.effect_shader().Parameters["Palette"].SetValue((Texture2D)null);
#else
                    sprite_batch.GraphicsDevice.Textures[2] = null;
#endif
                }
        }
    }
}
