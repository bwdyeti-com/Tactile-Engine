using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Battle_Platform : Stereoscopic_Graphic_Object
    {
        const float PLATFORM_TO_EDGE_RATIO = 75 / 120f;

        protected bool Ranged;
        protected Texture2D Platform_1, Platform_2;
        protected Vector2 Loc_1, Loc_2;

        #region Accessors
        public Texture2D platform_1
        {
            get { return Platform_1; }
            set { Platform_1 = value; }
        }
        public Texture2D platform_2 { set { Platform_2 = value; } }

        public Vector2 loc_1
        {
            get { return Loc_1; }
            set { Loc_1 = value; } }
        public Vector2 loc_2
        {
            get { return Loc_2; }
            set { Loc_2 = value; }
        }
        #endregion

        public Battle_Platform(bool ranged)
        {
            Ranged = ranged;
            stereoscopic = Config.BATTLE_PLATFORM_BASE_DEPTH;
        }

        public void add_y(int y)
        {
            Loc_1.Y += y;
            Loc_2.Y += y;
        }

        public virtual void draw(SpriteBatch sprite_batch)
        {
            draw(sprite_batch, Vector2.Zero, Vector2.Zero);
        }
        public virtual void draw(SpriteBatch sprite_batch, Vector2 draw_offset_1, Vector2 draw_offset_2)
        {
            if (Platform_1 != null)
            {
                Rectangle src_rect;
                int platform_top_height;
                Matrix matrix, skew;
                Vector2 matrix_offset;

                // Draw Platform 1 Edge
                platform_top_height = (int)(Platform_1.Height * PLATFORM_TO_EDGE_RATIO);
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                src_rect = new Rectangle(0, platform_top_height, Platform_1.Width, Platform_1.Height - platform_top_height);
                sprite_batch.Draw(Platform_1, (Loc_1 + draw_vector() + new Vector2(0, platform_top_height)) - draw_offset_1,
                    src_rect, Color.White, 0f, Vector2.Zero, 1f,
                    SpriteEffects.None, 0f);
                sprite_batch.End();
                // Draw Platform 1 Top
                matrix_offset = (Loc_1 + draw_vector()) - (draw_offset_1 - new Vector2(0, platform_top_height));
                skew = Matrix.Identity;
                skew.M21 = graphic_draw_offset(-Config.BATTLE_PLATFORM_TOP_DEPTH_OFFSET).X / (platform_top_height);
                matrix = Matrix.Identity * Matrix.CreateTranslation(-new Vector3(matrix_offset.X, matrix_offset.Y, 0)) *
                    skew * Matrix.CreateTranslation(new Vector3(matrix_offset.X, matrix_offset.Y, 0));

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, matrix);
                src_rect = new Rectangle(0, 0, Platform_1.Width, platform_top_height);
                sprite_batch.Draw(Platform_1, (Loc_1 + draw_vector()) - draw_offset_1,
                    src_rect, Color.White, 0f, Vector2.Zero, 1f,
                    SpriteEffects.None, 0f);
                sprite_batch.End();


                // Draw Platform 2 Edge
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                src_rect = new Rectangle(0, platform_top_height, Platform_2.Width, Platform_2.Height - platform_top_height);
                sprite_batch.Draw(Platform_2, (Loc_2 + draw_vector() + new Vector2(0, platform_top_height)) - draw_offset_2,
                    src_rect, Color.White, 0f, Vector2.Zero, 1f,
                    SpriteEffects.FlipHorizontally, 0f);
                sprite_batch.End();
                // Draw Platform 2 Top
                matrix_offset = (Loc_2 + draw_vector()) - (draw_offset_2 - new Vector2(0, platform_top_height));
                skew = Matrix.Identity;
                skew.M21 = graphic_draw_offset(-Config.BATTLE_PLATFORM_TOP_DEPTH_OFFSET).X / (platform_top_height);
                matrix = Matrix.Identity * Matrix.CreateTranslation(-new Vector3(matrix_offset.X, matrix_offset.Y, 0)) *
                    skew * Matrix.CreateTranslation(new Vector3(matrix_offset.X, matrix_offset.Y, 0));

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, matrix);
                src_rect = new Rectangle(0, 0, Platform_2.Width, platform_top_height);
                sprite_batch.Draw(Platform_2, (Loc_2 + draw_vector()) - draw_offset_2,
                    src_rect, Color.White, 0f, Vector2.Zero, 1f,
                    SpriteEffects.FlipHorizontally, 0f);
                sprite_batch.End();
            }
        }
    }
}
