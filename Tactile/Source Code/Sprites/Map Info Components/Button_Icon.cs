using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tactile.Graphics.Help
{
    class Button_Icon : Icon_Sprite
    {
        public Button_Icon(Buttons button, Texture2D texture) : this(Button_Description.ButtonIndex(button), texture) { }
        public Button_Icon(int input, Texture2D texture)
        {
            this.texture = texture;
            size = new Vector2(texture.Width, 16);
            columns = 1;
            index = input;
        }
    }
}
