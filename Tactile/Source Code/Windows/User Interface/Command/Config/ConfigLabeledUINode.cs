using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Windows.UserInterface.Command.Config
{
    abstract class ConfigLabeledUINode : ConfigUINode
    {
        protected TextSprite Text;

        internal ConfigLabeledUINode(
                string helpLabel,
                string str)
            : base(helpLabel)
        {
            Text = new TextSprite();
            Text.SetFont(Tactile.Config.UI_FONT, Global.Content, "White");
            Text.text = str;
        }

        protected override void set_label_color(string color)
        {
            Text.SetColor(Global.Content, color);
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
