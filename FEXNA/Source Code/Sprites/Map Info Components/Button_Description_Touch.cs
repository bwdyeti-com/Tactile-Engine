using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Help
{
    class Button_Description_Touch : Button_Description
    {
        readonly static int[] Offsets = new int[] { 0, 0, 0, 0, 19, 19, 19, 19, 25, 25, 38, 34 };

        #region Accessors
        protected override Vector2 button_offset
        {
            get
            {
                var icon = Button as Keyboard_Icon;
                return new Vector2(
                    icon.ButtonWidth -
                    (icon.minimum_width - icon.letter_offset), -2);
            }
        }

        protected override Texture2D description_texture
        {
            get { return Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Keys"); }
        }
        #endregion

        public Button_Description_Touch(Inputs input) : base(input) { }
        public Button_Description_Touch(Inputs input, Texture2D description_texture, Rectangle description_rect) : base(input, description_texture, description_rect) { }

        protected override void set_button(Inputs input)
        {
            var icon = new TouchIcon(input,
                Global.Content.Load<Texture2D>(@"Graphics/Pictures/Buttons_Touch"),
                false);
            icon.letter = "";
            Button = icon;
        }

        protected override void refresh_size(Vector2 descriptionSize)
        {
            base.refresh_size(descriptionSize);
            (Button as Keyboard_Icon).TextWidth = (int)descriptionSize.X;
        }
    }
}
