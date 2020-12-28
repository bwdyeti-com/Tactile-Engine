using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Menu_Background : Sprite
    {
        public Vector2 vel = Vector2.Zero;
        protected Vector2 Tile = Vector2.One;

        #region Accessors
        public Vector2 tile
        {
            set { Tile = new Vector2(
                Math.Max((int)value.X, 1), Math.Max((int)value.Y, 1));
            }
        }
        #endregion

        public override void update()
        {
            offset -= vel;

            float stereo = Stereo_Offset.ValueOrDefault;
            if (vel.X != 0 && Tile.X > 1)
            {
                while (offset.X >= src_rect.Width + Math.Abs(stereo) * 2)
                    offset.X -= src_rect.Width;
                while (offset.X < Math.Abs(stereo) * 2)
                    offset.X += src_rect.Width;
            }
            if (vel.Y != 0 && Tile.Y > 1)
            {
                while (offset.Y >= src_rect.Height)
                    offset.Y -= src_rect.Height;
                while (offset.Y < 0)
                    offset.Y += src_rect.Height;
            }

            base.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (!(texture == null))
                if (visible)
                {
                    for (int y = 0; y < Tile.Y; y++)
                        for (int x = 0; x < Tile.X; x++)
                        {
                            Rectangle src_rect = this.src_rect;
                            Vector2 offset = this.offset;
                            if (mirrored) offset.X = src_rect.Width - offset.X;
                            sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset + new Vector2(x * src_rect.Width, y * src_rect.Height),
                                src_rect, tint, angle, new Vector2((int)offset.X, (int)offset.Y), scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        }
                }
        }
    }
}
