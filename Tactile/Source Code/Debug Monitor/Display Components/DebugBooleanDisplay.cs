using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile.Debug_Monitor
{
    class DebugBooleanDisplay : DebugSwitchDisplay
    {
        private TextSprite CaptionText;

        internal DebugBooleanDisplay(boolFunc update_function, string caption,
                Maybe<int> caption_width = default(Maybe<int>))
            : base()
        {
            UpdateFunction = () => update_function() ? 0 : 1;
            BgFadeTimes = new List<int> { 0, 0 };
            BgColors = new List<Color> { new Color(40, 200, 40), new Color(200, 40, 40) };

            int x = 0;
            //Caption
            CaptionText = new TextSprite();
            CaptionText.loc = new Vector2(x + 4, 0);
            CaptionText.SetFont(Config.UI_FONT);
            CaptionText.text = caption;
            if (caption_width.IsNothing)
            {
                caption_width = CaptionText.text_width + 8;
                caption_width = ((caption_width + 7) / 8) * 8;
            }
            x += caption_width;
            // On
            TextSprite text = new TextSprite();
            text.loc = new Vector2(x + 4, 0);
            text.SetFont(Config.UI_FONT);
            text.text = "On";
            Texts.Add(text);
            Sprite bg = new Sprite();
            bg.loc = new Vector2(x, 0);
            bg.scale = new Vector2(24 / 16f, 1f);
            bg.tint = BgColors[0];
            Bgs.Add(bg);
            x += 24;
            // Off
            text = new TextSprite();
            text.loc = new Vector2(x + 4, 0);
            text.SetFont(Config.UI_FONT);
            text.text = "Off";
            Texts.Add(text);
            bg = new Sprite();
            bg.loc = new Vector2(x, 0);
            bg.scale = new Vector2(24 / 16f, 1f);
            bg.tint = BgColors[0];
            Bgs.Add(bg);

            Width = x + 24;
        }

        internal override void draw(SpriteBatch sprite_batch, ContentManager content)
        {
            base.draw(sprite_batch, content);
            sprite_batch.Begin();
            CaptionText.draw(sprite_batch, content.Load<Texture2D>(
                string.Format(@"Graphics/Fonts/{0}_Yellow", Config.UI_FONT)), -loc);
            sprite_batch.End();
        }
    }
}
