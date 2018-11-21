//Sparring
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Preparations
{
    class Sparring_Gauge : Sprite
    {
        const int POINT_WIDTH = 10, POINT_HEIGHT = 5;
        private int Points;
        private int StaffPoints;
        private bool Vertical;
        private bool Support = false;
        private int ActivePips = 0;
        private float ActivePipOpacity = 1f;

        #region Accessors
        public int points { set { Points = value; } }
        public int staffPoints { set { StaffPoints = value; } }

        public bool support { set { Support = value; } }

        public int active_pips { set { ActivePips = value; } }

        public float active_pip_opacity { set { ActivePipOpacity = value; } }
        #endregion

        public Sparring_Gauge(bool vertical)
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Sparring_Components");
            Vertical = vertical;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;

                    Color pip_tint = new Color(ActivePipOpacity, ActivePipOpacity, ActivePipOpacity, ActivePipOpacity);

                    if (Vertical)
                    {
                        sprite_batch.Draw(texture, (loc + new Vector2(0, 8) + draw_vector()) - draw_offset,
                            new Rectangle(16, 8, 8, 24), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        for (int y = 0; y < Points; y++)
                            sprite_batch.Draw(texture, (loc + new Vector2(0, 24 - y * POINT_HEIGHT) + draw_vector()) - draw_offset,
                                new Rectangle(0, 24, 8, 8), (Points - y) <= ActivePips ? pip_tint : tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        if (Support)
                            sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset,
                                new Rectangle(8, 24, 8, 8), tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                    else
                    {
                        sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset,
                            new Rectangle(0, 0, 40, 8), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        for (int x = 0; x < Points; x++)
                            sprite_batch.Draw(texture, (loc + new Vector2(x * POINT_WIDTH, 0) + draw_vector()) - draw_offset,
                                new Rectangle(0, 8, 16, 8), (Points - x) <= ActivePips ? pip_tint : tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);

                        for (int x = 0; x < StaffPoints; x++)
                            sprite_batch.Draw(texture, (loc + new Vector2(29 + (StaffPoints - (x + 1)) * 4, 1) + draw_vector()) - draw_offset,
                                new Rectangle(24, 8, 8, 8), tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                }
        }
    }
}
