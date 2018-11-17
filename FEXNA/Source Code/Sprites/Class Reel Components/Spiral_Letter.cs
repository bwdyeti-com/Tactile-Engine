using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Text
{
    class Spiral_Letter : FE_Text
    {
        public const int DELAY = 6;
        public const int FLASH_TIME = 40;
        public const int TOTAL_WAIT = 104;

        protected int Index;
        protected int Timer = 0;
        protected int Delay;
        protected int Flash_Delay;
        protected Color Flash = new Color(255, 255, 255, 0);

        public Color flash { get { return Flash; } }

        public Spiral_Letter(char letter, int index, int length)
        {
            text = "" + letter;
            Index = index;
            Delay = (length - 1) * DELAY + TOTAL_WAIT;
            Flash_Delay = (length - Index) * DELAY + FLASH_TIME;
            Font = "FE7_Reel";
            texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Reel_Black");
            offset = new Vector2(DELAY, 16);
            opacity = 0;
        }

        public override void update()
        {
            // Letter slides on
            if (Timer >= 0 && Timer < DELAY)
            {
                offset.X--;
                opacity = 64 / (DELAY - Timer);
            }
            // Letter flashes
            else if (Timer >= Flash_Delay && Timer < Flash_Delay + 8)
            {
                Flash.A = (byte)MathHelper.Clamp(Flash.A + 24, 0, 255);
                opacity += 24;
            }
            // Letter switches from engraved texture to extruded texture
            else if (Timer == Flash_Delay + 8)
            {
                texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Reel_Gold");
                stereoscopic = Config.REEL_NAME_LETTERS_DEPTH;
                opacity = 192;
            }
            else if (Timer >= Flash_Delay + 16 && Timer < Flash_Delay + 24)
            {
                Flash.A = (byte)MathHelper.Clamp(Flash.A - 32, 0, 255);
            }
            // Letter kind of sits around
            else if (Timer >= DELAY && Timer < Delay) { }
            // Whoosh
            else
            {
                float angle = ((Timer - Delay + Index) * 10 - 135) / 360f * MathHelper.TwoPi;
                loc.X += (float)Math.Cos(angle) * (Timer - Delay + Index * 2) / 2;
                loc.Y += (float)Math.Sin(angle) * (Timer - Delay + Index * 2) / 2;
            }
            Timer++;
        }
    }
}
