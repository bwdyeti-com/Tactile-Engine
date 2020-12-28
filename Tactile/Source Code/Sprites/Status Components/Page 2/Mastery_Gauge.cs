using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Mastery_Gauge : Sprite
    {
        const int WIDTH = 12;

        private int Width;

        #region Accessors
        public float percent
        {
            set
            {
                scale.X = value;
                refresh();
            }
        }
        public float height
        {
            set
            {
                scale.Y = Math.Max(2, value);
            }
        }

        Vector2 gauge_offset { get { return new Vector2((Config.SKILL_ICON_SIZE - Width) / 2, Config.SKILL_ICON_SIZE - 5); } }
        Vector2 bar_scale { get { return new Vector2(scale.X == 0 ? 0 : Math.Max(1 / 16f, Width * scale.X / 16), (scale.Y - 1) / 16f); } }
        Vector2 bg_scale { get { return new Vector2(Width / (float)16, scale.Y / 16f); } }
        #endregion

        public Mastery_Gauge(float percent, int width = WIDTH)
        {
            Width = width;
            texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            charge(percent);
        }

        private void refresh()
        {
            if (scale.X >= 1f)
                tint = new Color(128, 240, 208, 255);
            else
                tint = new Color(255, (int)(255 * scale.X), 0, 255);
        }

        internal void charge(float percent)
        {
            scale = new Vector2(percent, 2);
            refresh();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            //mastery_charge_percent
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    // Gauge background
                    sprite_batch.Draw(texture, (this.loc + draw_vector() + gauge_offset) - draw_offset,
                        src_rect, Color.Black, angle, offset, bg_scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Gauge
                    sprite_batch.Draw(texture, (this.loc + draw_vector() + gauge_offset) - draw_offset,
                        src_rect, tint, angle, offset, bar_scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Gauge
                    //sprite_batch.Draw(texture, (this.loc + draw_vector() + OFFSET + new Vector2(0, 1)) - draw_offset,
                    //    src_rect, new Color(tint.R * 3 / 4, tint.G * 3 / 4, tint.B * 3 / 4, 255), angle, offset, scale,
                    //    mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
