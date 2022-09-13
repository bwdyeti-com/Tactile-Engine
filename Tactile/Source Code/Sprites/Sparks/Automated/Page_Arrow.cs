using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows.UserInterface;

namespace Tactile
{
    class Page_Arrow : Spark, IUIObject
    {
        public readonly static string FILENAME = "Page_Arrows";

        private int TwirlingTime = 0;

        public Page_Arrow()
        {
            Loop = true;
            Timer_Maxes = new int[] { 8, 8, 8, 8, 8, 8 };
            Frames = new Vector2(6, 1);
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + FILENAME);
        }

        internal void twirl()
        {
            TwirlingTime = 20;
        }

        internal void twirling_update()
        {
            for (int i = 0; i < 6; i++)
                base.update();
        }

        public override void update()
        {
            if (TwirlingTime > 0)
            {
                twirling_update();
                TwirlingTime--;
            }
            base.update();
        }

        public EventHandler ArrowClicked;
        //@Debug: ArrowPressed event?

        public void UpdateInput(Vector2 drawOffset = default(Vector2))
        {
            if (Input.ControlScheme == ControlSchemes.Buttons || !visible)
                return;

            Rectangle arrow_rect = OnScreenBounds(drawOffset);

            tint = Color.White;

            // Mouse triggered
            if (Global.Input.mouse_clicked_rectangle(MouseButtons.Left,
                    arrow_rect, loc - drawOffset, this.offset, this.angle, mirrored))
            {
                if (ArrowClicked != null)
                    ArrowClicked(this, new EventArgs());
            }
            // Tapped
            else if (Global.Input.gesture_rectangle(TouchGestures.Tap,
                arrow_rect, loc - drawOffset, this.offset, this.angle, mirrored))
            {
                if (ArrowClicked != null)
                    ArrowClicked(this, new EventArgs());
            }
            else if (MouseOver(drawOffset) ||
                Global.Input.touch_rectangle(
                    Services.Input.InputStates.Pressed,
                    arrow_rect, loc - drawOffset, this.offset, this.angle, mirrored, false))
            {
                tint = new Color(0.6f, 0.7f, 0.8f, 1f);
            }
        }

        public Rectangle OnScreenBounds(Vector2 drawOffset)
        {
            Vector2 loc = (this.loc + this.draw_offset) - drawOffset;
            return new Rectangle(
                (int)loc.X, (int)loc.Y, src_rect.Width, src_rect.Height);
        }

        public bool MouseOver(Vector2 drawOffset = default(Vector2))
        {
            Rectangle objectRect = OnScreenBounds(drawOffset);
            return Global.Input.mouse_in_rectangle(
                objectRect, loc - drawOffset, this.offset, this.angle, mirrored);
        }
    }
}
