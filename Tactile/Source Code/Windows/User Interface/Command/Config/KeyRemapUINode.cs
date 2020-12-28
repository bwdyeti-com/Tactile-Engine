using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tactile.Graphics.Text;
using Tactile.Graphics.Help;

namespace Tactile.Windows.UserInterface.Command.Config
{
    class KeyRemapUINode : ConfigUINode
    {
        private Button_Description Text;
        private Keyboard_Icon Value;
        private Inputs Input;
        private string Description;

        internal KeyRemapUINode(
                string helpLabel,
                Inputs input,
                string str,
                int width)
            : base(helpLabel)
        {
            Input = input;
            Description = str;

            RefreshButton();

            Value = new Keyboard_Icon(
                input,
                Global.Content.Load<Texture2D>(
                    @"Graphics/Pictures/Buttons_Keyboard"), false);
            Value.refresh();
            Value.draw_offset = new Vector2(120, 0);

            Size = new Vector2(width, 16);
        }

        public void RefreshButton()
        {
            Buttons button = Tactile.Input.PadRedirect(Input);

            Button_Description buttonIcon = Button_Description.button(button);
            buttonIcon.description = Description;
            buttonIcon.draw_offset = new Vector2(8, 0);
            Text = buttonIcon;
        }

        internal override void set_text_color(string color) { }

        internal void set_key(Keys key)
        {
            Value.SetKey(key);
        }

        protected override void update_graphics(bool activeNode)
        {
            Text.Update();
            Value.refresh();
            Value.update();
        }

        protected override void mouse_off_graphic()
        {
            Text.tint = Color.White;
            Value.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            Text.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
            Value.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            Text.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
            Value.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Text.Draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Value.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
