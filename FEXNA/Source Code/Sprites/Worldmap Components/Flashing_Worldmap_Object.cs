using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Flashing_Worldmap_Object : Sprite
    {
        const int FLASH_TIME = 54;
        static int Flash_Timer;
        protected static int Flash_Frame;
        protected int Team;

        public Flashing_Worldmap_Object()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Worldmap_Sprites");
        }

        public static void update_flash()
        {
            Flash_Timer = (Flash_Timer + 1) % FLASH_TIME;
            switch(Flash_Timer)
            {
                case 0:
                    Flash_Frame = 0;
                    break;
                case 10:
                case 48:
                    Flash_Frame = 1;
                    break;
                case 16:
                case 42:
                    Flash_Frame = 2;
                    break;
                case 22:
                    Flash_Frame = 3;
                    break;
            }
        }
    }
}
