using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics
{
    class ScreenFade : Sprite
    {
        readonly private int FadeInTime;
        readonly private int FadeHoldTime;
        readonly private int FadeOutTime;
        private int Timer = 0;
        private Color HoldColor = Color.Black;

        private int FadeMidpoint
        {
            get { return FadeInTime + (FadeHoldTime / 2); }
        }

        public bool AtFadeMidPoint
        {
            get { return Timer == this.FadeMidpoint; }
        }

        private int HoldEnd { get { return FadeInTime + FadeHoldTime; } }
        public bool AtHoldEnd
        {
            get { return Timer == this.HoldEnd; }
        }
        public bool FrameOneHoldEnd
        {
            get { return this.HoldEnd == 0 && Timer == 1; }
        }

        public bool BeforeFadeMidPoint
        {
            get { return Timer <= this.FadeMidpoint; }
        }

        public bool Finished
        {
            get { return Timer >= FadeInTime + FadeHoldTime + FadeOutTime; }
        }

        public ScreenFade() : this(15, 15, 15) { }
        public ScreenFade(int fadeTime, int fadeHoldTime)
            : this(fadeTime, fadeHoldTime, fadeTime) { }
        public ScreenFade(int fadeInTime, int fadeHoldTime, int fadeOutTime)
        {
            FadeInTime = Math.Max(0, fadeInTime);
            FadeHoldTime = Math.Max(0, fadeHoldTime);
            FadeOutTime = Math.Max(0, fadeOutTime);

            Texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            tint = new Color(0, 0, 0, 0);
            Dest_Rect = new Rectangle(
                0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);

            Refresh();
        }

        public void Reset()
        {
            Timer = 0;
            Refresh();
        }
        public void SkipIntro()
        {
            Timer = FadeInTime;
            Refresh();
        }

        public void SetHoldColor(Color color)
        {
            HoldColor = color;
            Refresh();
        }

        public override void update()
        {
            Timer++;
            Refresh();

            base.update();
        }

        private void Refresh()
        {
            // Lerp opacity up
            if (Timer < FadeInTime)
            {
                this.tint = Color.Lerp(
                    Color.Transparent,
                    HoldColor,
                    Timer / (float)FadeInTime);
            }
            // Hold on full opacity
            else if (Timer < FadeInTime + FadeHoldTime)
            {
                this.tint = HoldColor;
            }
            // Lerp opacity down
            else if (Timer < FadeInTime + FadeHoldTime + FadeOutTime)
            {
                int timer = Timer - (FadeInTime + FadeHoldTime);
                this.tint = Color.Lerp(
                    HoldColor,
                    Color.Transparent,
                    timer / (float)FadeOutTime);
            }
            else
                this.tint = Color.Transparent;
        }

        public override void draw(SpriteBatch sprite_batch, Texture2D texture, Vector2 draw_offset = default(Vector2))
        {
            if (this.Finished)
                return;

            base.draw(sprite_batch, texture, draw_offset);
        }
    }
}
