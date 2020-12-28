using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{

    class Status_Bonus_Background : StatusWindowDivider
    {
        private Texture2D BonusPanelTexture;

        public Status_Bonus_Background()
        {
            SetWidth(128);
            BonusPanelTexture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Portrait_bg");
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.draw(sprite_batch, draw_offset + new Vector2(0, -4));
            if (BonusPanelTexture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored)
                        offset.X = src_rect.Width - offset.X;
                    // Bonus stats panel
                    sprite_batch.Draw(BonusPanelTexture, this.loc + draw_vector() + new Vector2(4, 32) - draw_offset,
                        new Rectangle(96, Global.game_options.window_color * 64, 120, 8), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    sprite_batch.Draw(BonusPanelTexture, this.loc + draw_vector() + new Vector2(4, 40) - draw_offset,
                        new Rectangle(96, Global.game_options.window_color * 64 + 24, 120, 40), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }

    class Status_Support_Background : StatusWindowDivider
    {
        public Status_Support_Background()
        {
            SetWidth(128);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.draw(sprite_batch, draw_offset + new Vector2(0, 3));
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(texture, this.loc + draw_vector() + new Vector2(3, 0) - draw_offset,
                        new Rectangle(94, Global.game_options.window_color * 3 + 256, 122, 3), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
