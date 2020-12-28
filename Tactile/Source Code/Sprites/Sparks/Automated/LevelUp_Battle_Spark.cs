using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class LevelUp_Battle_Spark : Spark
    {
        const int Timer_Max = 74;
        
        protected virtual string filename { get { return "LevelUp_Battle"; } }

        public LevelUp_Battle_Spark(ContentManager content)
        {
            texture = content.Load<Texture2D>(@"Graphics/Pictures/" + filename);
            Frames = new Vector2(5, 11);
        }

        public override bool completed()
        {
            return Timer > Timer_Max;
        }

        public override void update()
        {
            if (completed()) return;
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
                    Frame = Timer;
                    break;
                case 29:
                case 30:
                case 31:
                case 32:
                case 33:
                case 34:
                    Frame = Timer - 9;
                    break;
                case 35:
                case 36:
                case 37:
                case 38:
                case 39:
                case 40:
                case 41:
                case 42:
                case 43:
                case 44:
                case 45:
                case 46:
                case 47:
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
                    if (Timer % 2 == 1)
                        Frame = ((Timer - 1) / 2) + 9;
                    break;
                case 62:
                    tint = new Color(255, 255, 255, 0);
                    Frame = Timer - 22;
                    break;
                case 63:
                case 64:
                case 65:
                case 66:
                case 67:
                case 68:
                case 69:
                case 70:
                case 71:
                case 72:
                case 73:
                    Frame = Timer - 22;
                    break;
            }
            Timer++;
        }
    }
}
