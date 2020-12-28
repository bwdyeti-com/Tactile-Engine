using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TactileLibrary;

namespace Tactile.Graphics.Preparations
{
    class ChapterLabel : Sprite
    {
        private ChapterLabels Label;
        private string ChapterNumber = "";

        public ChapterLabel()
        {
            this.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/ChapterLabel");
        }

        public void SetChapter(Data_Chapter chapter)
        {
            Label = chapter.Label;
            ChapterNumber = chapter.LabelString;
        }

        public override void draw(SpriteBatch sprite_batch, Texture2D texture, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    RenderLabel(sprite_batch, texture, draw_offset - new Vector2(2, 2), Color.Black);
                    RenderLabel(sprite_batch, texture, draw_offset, tint);
                }
        }

        private void RenderLabel(SpriteBatch spriteBatch, Texture2D texture, Vector2 draw_offset, Color tint)
        {
            Vector2 offset = this.offset;
            int x = 0;

            // Label
            int labelWidth = Data_Chapter.LabelWidth(Label);
            spriteBatch.Draw(texture, (loc + new Vector2(x, 0) + draw_vector()) - draw_offset,
                new Rectangle(0, (int)Label * 16 + 16, labelWidth, 16), tint, angle, offset, scale,
                SpriteEffects.None, Z);
            x += labelWidth;

            // Number
            Font_Data font = Font_Data.Data["ChapterLabel"];
            font.RenderText(spriteBatch, texture, ChapterNumber,
                (loc + new Vector2(x, 0) + draw_vector()) - draw_offset,
                tint, angle, offset, scale, Z);
            x += font.TextWidth(ChapterNumber);

            // Right edge
            spriteBatch.Draw(texture, (loc + new Vector2(x, 0) + draw_vector()) - draw_offset,
                new Rectangle(0, 0, 8, 16), tint, angle, offset, scale,
                SpriteEffects.None, Z);
        }
    }
}
