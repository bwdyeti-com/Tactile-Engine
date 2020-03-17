using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Battle_Face_Sprite : Face_Sprite
    {
        readonly static int[] DEATH_ARY = new int[] {
            0,20,20,20,20,44,44,44,44,64,64,64,64,84,84,84,108,108,108,
            108,128,128,128,128,148,148,148,148,172,172,172,192,192,192,192,212,
            212,212,212,236,236,236,236,255,255,255,0,0,0,0,0,0,256,0,0,0,0,0,0,
            255,0,0,0,0,0,0,255,0,0,0,0,0,0,255,0,0,0,0,0,0,255,0,0,0,0,0,0,255 };
        protected int Death = -1;
        protected int Y = 0;
        private int Frame = 0;
        protected bool Fade = false;

        public Battle_Face_Sprite(Game_Unit unit)
        {
            string filename = unit.actor.face_name;
            initialize(filename, false);
            phase_in();
            if (texture != null)
            {
                if (unit.actor.generic_face)
                    recolor_country(unit.actor.name_full);
                offset = new Vector2(Face_Sprite_Data.BATTLE_FACE_SIZE.X, 0);
                Y = texture.Height - Face_Sprite_Data.FooterHeight;
                set_frame(0);
            }
        }

        public void set_frame(int frame)
        {
            Frame = frame;
            RefreshSrcRect();
        }

        protected override void RefreshSrcRect()
        {
            Src_Rect = new Rectangle(
                this.SrcX,
                Y + Frame * (int)Face_Sprite_Data.BATTLE_FACE_SIZE.Y,
                (int)Face_Sprite_Data.BATTLE_FACE_SIZE.X,
                (int)Face_Sprite_Data.BATTLE_FACE_SIZE.Y);
        }

        public void kill()
        {
            if (Death == -1)
                Death = DEATH_ARY.Length;
        }

        public override void update()
        {
            if (Death > 0)
            {
                Death--;
                if (DEATH_ARY[Death] == 256)
                {
                    Fade = true;
                    tint = new Color(255, 255, 255, 0);
                }
                else
                    tint = new Color(DEATH_ARY[Death], DEATH_ARY[Death], DEATH_ARY[Death], Fade ? 0 : DEATH_ARY[Death]);
            }
        }

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
                    // Body
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
