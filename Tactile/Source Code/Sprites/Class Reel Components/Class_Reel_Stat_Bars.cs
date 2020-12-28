using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Class_Reel_Stat_Bars : Sprite
    {
        protected int Cap = 0, Stat = 0;

        #region Accessors
        public int cap { set { Cap = Math.Min(value, Global.ActorConfig.PrimaryStatMax); } }
        public int stat { set { Stat = value; } }
        #endregion

        public Class_Reel_Stat_Bars()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Class_Reel_Window");
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    // Cap
                    sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset,
                        new Rectangle(0, 48, 48, 8), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    for (int i = 0; i < Cap; i++)
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(48 + i * 2, 0)) - draw_offset,
                            new Rectangle(48, 48, 2, 8), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(48 + Cap * 2, 0)) - draw_offset,
                        new Rectangle(64, 48, 8, 8), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Stat
                    for (int i = 0; i < Stat; i++)
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(49 + i * 2, 0)) - draw_offset,
                            new Rectangle(72, 48, 8, 8), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
