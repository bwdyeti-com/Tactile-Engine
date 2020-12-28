using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Graphics.Help
{
    class TouchIcon : Keyboard_Icon
    {
        const int LETTER_OFFSET = 3;
        const int WIDTH_ADDITION = 7;

        internal override int letter_offset { get { return LETTER_OFFSET; } }
        protected override int width_addition { get { return WIDTH_ADDITION; } }

        public TouchIcon(Inputs input, Texture2D texture, bool colon = true)
            : base(input, texture, colon) { }
    }
}
