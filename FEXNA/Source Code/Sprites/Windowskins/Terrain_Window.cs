using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Terrain_Window : Sprite
    {
        protected bool Is_Destroyable_Object = false;
        protected Texture2D Destroyable_Object_Texture;

        #region Accessors
        public bool is_destroyable_object { set { Is_Destroyable_Object = value; } }
        #endregion

        public Terrain_Window()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Terrain_Info");
            Destroyable_Object_Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Attackable_Terrain_Icon");
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = new Rectangle(0, 53 * Global.game_options.window_color, 48, 53);
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    if (Dest_Rect != null)
                        sprite_batch.Draw(texture, (Rectangle)Dest_Rect,
                            src_rect, tint, angle, Vector2.Zero,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    else
                        sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset,
                            src_rect, tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    if (Is_Destroyable_Object)
                    {
                        sprite_batch.Draw(Destroyable_Object_Texture, (this.loc + draw_vector() + new Vector2(8, 30)) - draw_offset,
                            new Rectangle(0, 0, 16, 16), Color.White, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                    else
                    {
                        // Def
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(8, 31)) - draw_offset,
                            new Rectangle(0, 212, 16, 7), Color.White, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        // Avo
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(8, 39)) - draw_offset,
                            new Rectangle(0, 220, 16, 7), Color.White, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                }
        }
    }
}
