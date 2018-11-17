using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class SoloAnim_Button : Spark
    {
        public readonly static string FILENAME = "SoloAnim_Button_Icon";

        public SoloAnim_Button()
        {
            Loop = true;
            Timer_Maxes = new int[] { 8, 8 };
            Frames = new Vector2(2, 1);
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + FILENAME);
        }
    }
}
