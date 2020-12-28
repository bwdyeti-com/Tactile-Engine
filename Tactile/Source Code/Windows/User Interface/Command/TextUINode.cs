using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Windows.UserInterface.Command
{
    class TextUINode : CommandUINode
    {
        protected TextSprite Text;

        internal TextUINode(
                string helpLabel,
                TextSprite text,
                int width)
            : base(helpLabel)
        {
            Text = text;
            Size = new Vector2(width, 16);
        }

        internal override void set_text_color(string color)
        {
            Text.SetColor(Global.Content, color);
        }

        internal void set_text(string text)
        {
            Text.text = text;
        }

        protected override void update_graphics(bool activeNode)
        {
            Text.update();
        }

        protected override void mouse_off_graphic()
        {
            Text.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            Text.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            Text.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Text.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
