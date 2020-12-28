using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Windows.UserInterface.Command
{
    class ItemUINode : CommandUINode
    {
        protected Status_Item Text;
        protected TextSprite EquippedTag;

        internal ItemUINode(
                string helpLabel,
                Status_Item text,
                int width)
            : base(helpLabel)
        {
            Text = text;
            Size = new Vector2(width, 16);

            EquippedTag = new TextSprite();
            EquippedTag.draw_offset = new Vector2(width - 8, 0);
            EquippedTag.SetFont(Tactile.Config.UI_FONT, Global.Content, "White");
            EquippedTag.text = "$";
            EquippedTag.visible = false;
        }

        internal override void set_text_color(string color)
        {
            Text.change_text_color(color);
            EquippedTag.SetColor(Global.Content, color);
        }

        internal void equip(bool value)
        {
            EquippedTag.visible = value;
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
            EquippedTag.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
