using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tactile.Graphics.Text;
using Tactile.Graphics.Help;

namespace Tactile.Windows.UserInterface.Command.Config
{
    class GamepadRemapUINode : ConfigLabeledUINode
    {
        private Button_Description Value;
        private Inputs Input;

        internal GamepadRemapUINode(
                string helpLabel,
                Inputs input,
                string str,
                int width)
            : base(helpLabel, str)
        {
            Input = input;

            RefreshButton();

            Text.draw_offset = new Vector2(8, 0);

            Size = new Vector2(width, 16);
        }

        public void RefreshButton()
        {
            Buttons button = Tactile.Input.PadRedirect(Input);

            Button_Description buttonIcon = Button_Description.button(button);
            buttonIcon.description = "";
            buttonIcon.draw_offset = new Vector2(120, 0);
            buttonIcon.ColonVisible(false);
            Value = buttonIcon;
        }

        protected override void set_label_color(string color) { }

        internal override void set_text_color(string color) { }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);
            Value.Update();
        }

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();
            Value.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();
            Value.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();
            Value.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            Value.Draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
