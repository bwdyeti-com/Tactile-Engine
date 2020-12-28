using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Status_Heal_Effect : Matrix_Position_Sprite
    {
        public const int MAX_TIME = 63;
        protected int Timer = 0;

        #region Accessors
        public bool is_finished { get { return Timer >= MAX_TIME; } }
        #endregion

        public Status_Heal_Effect()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Status_Heal");
            offset = new Vector2(texture.Width / 2, texture.Height - 32);
        }

        public override void update()
        {
            int alpha;
            switch (Timer)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                    alpha = (Timer + 1) * 16;
                    tint = new Color(alpha, alpha, alpha, 0);
                    break;
                case 15:
                    tint = new Color(255, 255, 255, 0);
                    break;
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                case 58:
                case 59:
                case 60:
                case 61:
                case 62:
                case 63:
                    alpha = (63 - Timer) * 16;
                    tint = new Color(alpha, alpha, alpha, 0);
                    break;
            }
            Timer++;
            offset += new Vector2(0, 1);
            if (offset.Y >= texture.Height)
                offset -= new Vector2(0, 32);
        }
    }
}
