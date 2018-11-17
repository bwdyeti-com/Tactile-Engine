using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Debug_Monitor
{
    class DebugIntDisplay : DebugDisplay
    {
        protected intFunc UpdateFunction;
        private FE_Text CaptionText;
        private FE_Text_Int Text;
        private int Places;

        internal DebugIntDisplay(intFunc update_function, string caption, int places,
            Maybe<int> caption_width = default(Maybe<int>))
        {
            UpdateFunction = update_function;
            Places = places;

            int x = 0;
            //Caption
            CaptionText = new FE_Text();
            CaptionText.loc = new Vector2(x + 4, 0);
            CaptionText.Font = "FE7_Text";
            CaptionText.text = caption;
            if (caption_width.IsNothing)
            {
                caption_width = CaptionText.text_width + 8;
                caption_width = ((caption_width + 7) / 8) * 8;
            }
            x += caption_width;
            // Counter
            Text = new FE_Text_Int();
            Text.loc = new Vector2(x + Places * 8 + 4, 0);
            Text.Font = "FE7_Text";

            Width = caption_width + (Places + 1) * 8;
        }

        internal override void update()
        {
            if (this.is_update_frame)
            {
                Text.text = Math.Min(Math.Pow(10, Places) - 1,
                    Math.Max(1 - Math.Pow(10, Places - 1), UpdateFunction())).ToString();
            }
        }

        internal override void draw(SpriteBatch sprite_batch, ContentManager content)
        {
            sprite_batch.Begin();
            CaptionText.draw(sprite_batch, content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow"), -loc);
            Text.draw(sprite_batch, content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue"), -loc);
            sprite_batch.End();
        }
    }
}
