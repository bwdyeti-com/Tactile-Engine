using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Text
{
    class Stat_Up_Num : FE_Text
    {
        public int value;
        protected int Image = 0;
        protected int Timer = -1;

        public Stat_Up_Num(List<Texture2D> textures)
        {
            this.textures = textures;
            texture = textures[0];
            Font = "FE7_Text_Stat2";
            offset = new Vector2(6, 3);
            text = "-";
        }

        public virtual void update(int glow)
        {
            switch (Image)
            {
                case 0:
                    Timer++;
                    if (Timer > 1)
                    {
                        Timer = 0;
                        Image = (value < 0 ? 2 : 1);
                        text = "";
                    }
                    break;
                case 1:
                    switch (Timer)
                    {
                        case 0:
                            text = value.ToString();
                            offset = new Vector2(7, 6);
                            scale = new Vector2(2);
                            Timer++;
                            break;
                        case 4:
                            scale = new Vector2(1.8f);
                            Timer++;
                            break;
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                            scale = new Vector2(0.2f * (14 - Timer));
                            Timer++;
                            break;
                        case 14:
                            scale = new Vector2(1);
                            text = "";
                            Timer = 0;
                            Image = 2;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                case 2:
                    if (Timer == 0)
                    {
                        texture = textures[1];
                        Font = "FE7_Text_Stat1";
                        offset = new Vector2(11, 13);
                        // Negative numbers already have signs, so - doesn't need added
                        //text = (value > 0 ? "+" : (value < 0 ? "-" : "")) + value.ToString(); //Debug
                        text = (value > 0 ? "+" : "") + value.ToString();
                        Timer++;
                    }
                    break;
            }
        }
    }
}
