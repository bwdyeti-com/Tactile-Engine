using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    public class Message_Arrow : Sprite
    {
        const int CLEAR_TIME = 5;
        protected int Timer = 0;
        protected bool Clear = false;
        protected int Clear_Timer = CLEAR_TIME;

        #region Accessors
        public bool cleared { get { return Clear && Clear_Timer == 0; } }
        #endregion

        public Message_Arrow()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Arrows");
            Src_Rect = new Rectangle(21, 48, 8, 16);
        }

        public override void update()
        {
            if (Clear_Timer == 0) return;
            if (Clear_Timer == 2)
                opacity = 0;
            Timer = (Timer + 1) % 32;
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
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                    offset.Y = 0;
                    break;
                case 20:
                case 21:
                    offset.Y = -1;
                    break;
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                    offset.Y = -2;
                    break;
                case 30:
                case 31:
                    offset.Y = -1;
                    break;
            }
            if (Clear && Clear_Timer > 0) Clear_Timer -= 1;
        }

        public void clear()
        {
            Clear = true;
        }
    }
}
