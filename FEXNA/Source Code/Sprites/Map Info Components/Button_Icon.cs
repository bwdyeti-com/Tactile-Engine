using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Help
{
    class Button_Icon : Icon_Sprite
    {
        public Button_Icon(Inputs input, Texture2D texture) : this((int)input, texture) { }
        public Button_Icon(int input, Texture2D texture)
        {
            this.texture = texture;
            size = new Vector2(texture.Width, 16);
            columns = 1;
            index = input;
        }
    }
}
