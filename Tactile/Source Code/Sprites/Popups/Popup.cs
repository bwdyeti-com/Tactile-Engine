using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile
{
    internal class Popup : Sprite
    {
        protected WindowPanel Window;
        protected int Timer = 0;
        protected int Timer_Max = 97;
        TextSprite Text;
        public int Width { get; protected set; }

        #region Accessors
        public bool finished { get { return Timer > Timer_Max; } }

        protected bool skip_input
        {
            get
            {
                return (Global.Input.triggered(Inputs.A) ||
                    Global.Input.mouse_click(MouseButtons.Left) ||
                    Global.Input.gesture_triggered(TouchGestures.Tap));
            }
        }
        #endregion

        public Popup() { }
        public Popup(string str, int time, int width)
        {
            Window = new System_Color_Window();
            Window.width = width;
            Window.height = 32;
            Text = new TextSprite();
            Text.loc = new Vector2(8, 8);
            Text.SetFont(Config.UI_FONT, Global.Content, "White");
            Text.text = str;
            Timer_Max = time;
            Width = width;
            loc = new Vector2((Config.WINDOW_WIDTH - width) / 2, 80);
        }

        public override void update()
        {
            Timer++;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                if (Window != null)
                    Window.draw(sprite_batch, -(loc + draw_vector()));
                else
                    draw_panel(sprite_batch, Width);
                Text.draw(sprite_batch, -(loc + draw_vector()));
                sprite_batch.End();
            }
        }

        protected void draw_panel(SpriteBatch sprite_batch, int width)
        {
            if (texture != null)
            {
                width = Math.Max(24, width);
                // Left side
                sprite_batch.Draw(texture, this.loc + draw_vector() + new Vector2(0, 5),
                    new Rectangle(0, 0, 8, 22), tint, angle, offset, scale,
                    mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                // Middle
                int x = 0;
                while (x < width - 16)
                {
                    sprite_batch.Draw(texture, this.loc + draw_vector() + new Vector2(0, 5),
                        new Rectangle(Math.Min((width - 16) - x, 8), 0, (width - 16) - x, 22), tint, angle, offset - new Vector2((x + 8), 0), scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    x += 8;
                }
                // Right side
                sprite_batch.Draw(texture, this.loc + draw_vector() + new Vector2(0, 5),
                    new Rectangle(16, 0, 8, 22), tint, angle, offset - new Vector2(width - 8, 0), scale,
                    mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
            }
        }
    }
}
