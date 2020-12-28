using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Graphics.Map
{
    class Torch_Flash : Map_Effect
    {
        readonly static int[] OPACITY = new int[] { 1, 1, 2, 2, 2, 3, 3, 4, 4, 4, 5, 5, 6, 6, 6, 7, 7, 8, 8, 8, 9, 9, 0xA, 0xA,
            0xA, 0xB, 0xB, 0xC, 0xC, 0xC, 0xD, 0xD, 0xE, 0xE, 0xE, 0xF, 0xF, 0x10, 0x10, 0xF, 0xF, 0xE, 0xE, 0xD, 0xD, 0xC, 0xC,
            0xB, 0xB, 0xA, 0xA, 9, 8, 8, 7, 7, 6, 6, 5, 5, 4, 4, 3, 3, 2, 2, 1 };
        readonly static int[] SIZE = new int[] { 13, 19, 24, 29, 35, 40, 45, 49, 54, 58, 63, 67, 71, 75, 79, 83, 86, 90, 93, 97,
            100, 103, 106, 109, 112, 114, 117, 119, 122, 124, 126, 128, 130, 132, 134, 136, 137, 139, 141, 142, 143, 145, 146, 147,
            148, 149, 150, 151, 152, 153, 154, 154, 155, 156, 156, 157, 157, 158, 158, 158, 159, 159, 159, 159, 160, 160, 160 };

        public Torch_Flash()
        {
            Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + "Torch_Flash");
            Blend_Mode = 1;
            refresh();
            offset = new Vector2(Texture.Width / 2, Texture.Height / 2);
        }

        public override Rectangle src_rect
        {
            get
            {
                return new Rectangle(0, 0, texture.Width, texture.Width);
            }
        }

        public override void update()
        {
            Timer++;
            if (Timer >= OPACITY.Length)
                Finished = true;
            else
                refresh();
        }

        protected void refresh()
        {
            opacity = Math.Min(255, OPACITY[Timer] * 16);
            scale = new Vector2(SIZE[Timer] / 160f);
        }
    }
}
