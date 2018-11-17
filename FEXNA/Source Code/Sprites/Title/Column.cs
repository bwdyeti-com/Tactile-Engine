using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Title_Column : Sprite
    {
        const float START_DISTANCE = 500;
        public const bool SINGLE_PILLAR = true;
        protected const float SWITCH_ANGLE = 8f;//14.5f;
        protected const int ANGLE_GAP = 2;
        protected const int BRIGHT_ALPHA_MULT = 30;//18;
        protected const int DARK_ALPHA_MULT = 6;
        protected const int SCALE = 4;
        protected const int OFFSET = (int)(240 * 2.0f);

        protected float Distance = START_DISTANCE;
        protected int Opacity = 255;
        protected Color Color = new Color(232, 224, 200, 0);

        public Title_Column(Texture2D texture)
        {
            initialize(texture);
        }

        protected void initialize(Texture2D texture)
        {
            this.texture = texture;
            offset = new Vector2(texture.Width / 2, 945);//texture.Height / 2);
            loc.Y = 125;// Config.WINDOW_HEIGHT / 2;
            //offset = new Vector2(texture.Width / 2, texture.Height * 48 / 60); //when we get a new, dramatic angle background //Yeti
            //loc.Y = Config.WINDOW_HEIGHT * 2 / 3;
        }

        #region Accessors
        public float distance
        {
            get { return Distance; }
            set { Distance = value; }
        }

        public override int opacity
        {
            set
            {
                Opacity = (int)MathHelper.Clamp(value, 0, 255);
                tint = new Color(Opacity, Opacity, Opacity, Opacity);
            }
        }

        public Color color
        {
            get { return Color; }
        }
        #endregion

        public override void update()
        {
            opacity = Opacity + 16;
            if (Distance <= 0) return;
            if (Distance > 10000) return;
            float x = Distance / 100f;
            // Arcexsecant lol
            float angle, base_angle;
            angle = base_angle = (float)(MathHelper.PiOver2 - Math.Atan(Math.Pow(Math.Pow(x, 2) + 2 * x, 0.5f)));
            float zoom = (float)Math.Tan(angle);
            scale = new Vector2(zoom, zoom) * SCALE;
            set_x(zoom);
            if ((SWITCH_ANGLE + ANGLE_GAP) - angle * 360f / MathHelper.TwoPi > 0)
                Color.A = (byte)(Math.Max(SWITCH_ANGLE - angle * 360f / MathHelper.TwoPi, 0) * BRIGHT_ALPHA_MULT);
            else
            {
                Color = new Color(88, 56, 32, (byte)MathHelper.Clamp(-((SWITCH_ANGLE - ANGLE_GAP) - angle * 360f / MathHelper.TwoPi), 0, 255));
            }
        }

        protected virtual void set_x(float zoom)
        {
            if (SINGLE_PILLAR)
                loc.X = Config.WINDOW_WIDTH / 2;
            else
                loc.X = Config.WINDOW_WIDTH / 2 - (zoom * OFFSET);
        }

        protected override Vector2 stereo_offset()
        {
            float x = Distance / 100f;
            // Arcexsecant lol
            float angle, base_angle;
            angle = base_angle = (float)(MathHelper.PiOver2 - Math.Atan(Math.Pow(Math.Pow(x, 2) + 2 * x, 0.5f)));
            float zoom = (float)Math.Tan(angle) * SCALE;
            return base.stereo_offset() - (base.stereo_offset() * zoom);// (Distance / START_DISTANCE);

            return base.stereo_offset() * (Distance / START_DISTANCE);
        }
    }

    class Title_Column_Other : Title_Column
    {
        public Title_Column_Other(Texture2D texture) : base(texture)
        {
            mirrored = true;
        }

        protected override void set_x(float zoom)
        {
            loc.X = Config.WINDOW_WIDTH / 2 + (zoom * OFFSET);
        }
    }
}
