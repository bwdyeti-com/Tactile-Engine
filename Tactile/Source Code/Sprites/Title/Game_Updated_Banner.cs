using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Windows.UserInterface;

namespace Tactile
{
    class Game_Updated_Banner : Sprite, IUIObject
    {
        const int FADE_IN_TIME = 16;
        const int SCROLL_SPEED = -1;
        const int HEIGHT = 20;
        readonly static string SPACES_GAP = "        ";

        private int FadeInTimer;
        private int ScissorHeight;
        private int TextWidth, TextPosition;
        private TextSprite Text;
        private static RasterizerState ScissorState = new RasterizerState { ScissorTestEnable = true };

        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Text.stereoscopic = value;
            }
        }

        public Game_Updated_Banner(bool open_already_opened)
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            loc = new Vector2(0, Config.WINDOW_HEIGHT - 32);
            tint = new Color(0, 0, 0, 128);
            scale = new Vector2(Config.WINDOW_WIDTH, HEIGHT) / 16f;

            FadeInTimer = open_already_opened ? FADE_IN_TIME : 0;
            ScissorHeight = open_already_opened ? HEIGHT : 0;

            Text = new TextSprite();
            Text.draw_offset = new Vector2(0, (HEIGHT - 16) / 2);
            Text.SetFont(Config.CONVO_FONT);
            // Set message
            // Version
            Text.SetTextFontColor(0, "White");
            Text.text = "New version is available!";
            Text.text += SPACES_GAP;
            // Download link
            Text.text += string.Format("Download v{0} ({1}-{2}-{3}) at ",
                Global.UpdateVersion, Global.UpdateDate.Year, Global.UpdateDate.Month, Global.UpdateDate.Day);
            Text.SetTextFontColor("Blue");
            Text.text += Global.UpdateUri;
            Text.text += SPACES_GAP;
            // Description
            if (!string.IsNullOrEmpty(Global.UpdateDescription))
            {
                Text.SetTextFontColor("White");
                Text.text += Global.UpdateDescription;
                Text.text += SPACES_GAP;
            }

            TextWidth = Text.text_width;
            TextWidth = TextWidth + (TextWidth % 8 == 0 ? 0 : (8 - TextWidth % 8));
            TextPosition = (SCROLL_SPEED < 0 ? -1 : 1) * (TextWidth - 80);
        }

        public override void update()
        {
            base.update();
            if (FadeInTimer < FADE_IN_TIME)
            {
                FadeInTimer++;
                ScissorHeight = (int)(HEIGHT * (FadeInTimer / (float)FADE_IN_TIME));
            }
            TextPosition += SCROLL_SPEED;
            while (Math.Abs(TextPosition) >= TextWidth)
                TextPosition += (SCROLL_SPEED < 0 ? 1 : -1) * TextWidth;
        }

        public EventHandler Clicked;

        public void UpdateInput(Vector2 drawOffset = default(Vector2))
        {
            if (Input.ControlScheme == ControlSchemes.Buttons || !visible)
                return;

            Rectangle rect = OnScreenBounds(drawOffset);

            Text.tint = Color.White;

            // Mouse triggered
            if (Global.Input.mouse_clicked_rectangle(MouseButtons.Left,
                    rect, loc, this.offset, this.angle, mirrored))
            {
                if (Clicked != null)
                    Clicked(this, new EventArgs());
            }
            // Tapped
            else if (Global.Input.gesture_rectangle(TouchGestures.Tap,
                rect, loc, this.offset, this.angle, mirrored))
            {
                if (Clicked != null)
                    Clicked(this, new EventArgs());
            }
            else if (Global.Input.mouse_in_rectangle(
                    rect, loc, this.offset, this.angle, mirrored) ||
                Global.Input.touch_rectangle(
                    Services.Input.InputStates.Pressed,
                    rect, loc, this.offset, this.angle, mirrored, false))
            {
                // Pressed
                if (Global.Input.mouse_down_rectangle(
                        MouseButtons.Left,
                        rect, loc, this.offset, this.angle, mirrored, false) ||
                    Global.Input.touch_rectangle(
                        Services.Input.InputStates.Pressed,
                        rect, loc, this.offset, this.angle, mirrored, false))
                {
                    Text.tint = new Color(0.6f, 0.65f, 0.7f, 1f);
                }
                // Highlight
                else
                {
                    Text.tint = new Color(0.8f, 0.8f, 0.8f, 1f);
                }
            }
        }

        public Rectangle OnScreenBounds(Vector2 drawOffset)
        {
            return new Rectangle(
                (int)loc.X, (int)loc.Y, Config.WINDOW_WIDTH, HEIGHT);
        }

        public bool MouseOver(Vector2 drawOffset = default(Vector2))
        {
            Rectangle objectRect = OnScreenBounds(drawOffset);
            return Global.Input.mouse_in_rectangle(
                objectRect, loc - drawOffset, this.offset, this.angle, mirrored);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Rectangle scissor_rect = new Rectangle(
                (int)loc.X, (int)loc.Y + (HEIGHT / 2) - (ScissorHeight / 2),
                Config.WINDOW_WIDTH, ScissorHeight);

            sprite_batch.GraphicsDevice.ScissorRectangle = scissor_rect;
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, ScissorState);
            base.draw(sprite_batch, texture, draw_offset);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, null, null, ScissorState);
            for (int x = 0; (x - 1) * TextWidth < Config.WINDOW_WIDTH; x++)
                Text.draw_multicolored(sprite_batch, -(loc + new Vector2(TextPosition + x * TextWidth, 0)));
            sprite_batch.End();
        }
    }
}
