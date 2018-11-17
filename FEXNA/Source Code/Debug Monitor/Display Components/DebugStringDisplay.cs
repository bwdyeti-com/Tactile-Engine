using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA.Debug_Monitor
{
    class DebugStringDisplay : DebugDisplay
    {
        protected stringFunc UpdateFunction;
        private FE_Text CaptionText;
        private FE_Text Text;
        private int TextWidth;
        private bool Center;
        private string TextColor;

        internal DebugStringDisplay(
            stringFunc update_function, int text_width, string caption = null, bool center = false, string text_color = "White")
        {
            UpdateFunction = update_function;

            int x = 0;
            //Caption
            if (!string.IsNullOrEmpty(caption))
            {
                CaptionText = new FE_Text();
                CaptionText.loc = new Vector2(x + 4, 0);
                CaptionText.Font = "FE7_Text";
                CaptionText.text = caption;
                int caption_width = CaptionText.text_width + 8;
                caption_width = ((caption_width + 7) / 8) * 8;
                x += caption_width;
            }
            // Text
            Text = new FE_Text();
            Text.loc = new Vector2(x + 4, 0);
            Text.Font = "FE7_Text";

            TextWidth = text_width;
            Width = x + TextWidth;
            Center = center;
            TextColor = text_color;
        }

        internal override void update()
        {
            if (this.is_update_frame)
            {
                Text.text = UpdateFunction().ToString();
                if (Center)
                    Text.offset.X = (Text.text_width - (TextWidth - 8)) / 2;
            }
        }

        internal override void draw(SpriteBatch sprite_batch, ContentManager content)
        {
            sprite_batch.Begin();
            if (CaptionText != null)
                CaptionText.draw(sprite_batch, content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow"), -loc);
            Text.draw(sprite_batch, content.Load<Texture2D>(
                string.Format(@"Graphics/Fonts/FE7_Text_{0}", CaptionText == null ? TextColor : "Blue")), -loc);
            sprite_batch.End();
        }
    }
}
