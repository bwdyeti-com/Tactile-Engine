using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Graphics.Text
{
    class Quick_Stat_Up_Num : Stat_Up_Num
    {
        public Quick_Stat_Up_Num(List<Texture2D> textures)
            : base(textures)
        {
            Timer = -13;
            visible = false;
        }

        public override void update(int glow)
        {
            switch (Image)
            {
                case 0:
                    Timer++;
                    if (Timer > 1)
                    {
                        Timer = 0;
                        Image = 2;
                        base.update(glow);
                    }
                    else if (Timer == 1)
                        visible = true;
                    break;
                default:
                    base.update(glow);
                    break;
            }
        }
    }
}
