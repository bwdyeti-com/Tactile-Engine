using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Window_Convo_Location : Stereoscopic_Graphic_Object
    {
        public const int WAIT_TIME = 120;
        const int FADE_TIME = 16;
        protected int Timer = 0;
        protected TextSprite Text;
        protected Sprite Window;

        #region Accessors
        public bool is_finished { get { return Timer >= WAIT_TIME; } }
        #endregion

        public Window_Convo_Location(string text)
        {
            Text = new TextSprite();
            Text.SetFont(Config.UI_FONT, Global.Content, "White");
            Text.text = text;
            Text.offset.X = Font_Data.text_width(text) / 2;
            Text.opacity = 0;
            Window = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Convo_Location_Window"));
            Window.loc = new Vector2(Config.WINDOW_WIDTH - 8, 12);
            Text.loc = Window.loc + new Vector2(-Window.texture.Width / 2, 8);
            Window.offset.X = Window.texture.Width;
            Window.opacity = 0;
        }

        public void update()
        {
            Timer++;
            if (Timer <= FADE_TIME)
            {
                Text.opacity = (byte)Math.Min(255, Timer * 256 / FADE_TIME);
                Window.opacity = (byte)Math.Min(255, Timer * 256 / FADE_TIME);
            }
            else if ((WAIT_TIME - Timer) < FADE_TIME)
            {
                Text.opacity = (byte)Math.Min(255, (WAIT_TIME - Timer) * 256 / FADE_TIME);
                Window.opacity = (byte)Math.Min(255, (WAIT_TIME - Timer) * 256 / FADE_TIME);
            }
        }

        public void draw(SpriteBatch sprite_batch)
        {
            Window.draw(sprite_batch, -draw_vector());
            Text.draw(sprite_batch, -draw_vector());
        }
    }
}
