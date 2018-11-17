using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Help
{
    class Button_Description_360 : Button_Description
    {
        readonly static int[] Offsets = new int[] { 0, 0, 0, 0, 19, 19, 19, 19, 25, 25, 38, 34 };

        #region Accessors
        protected override Vector2 button_offset
        {
            get { return new Vector2(Offsets[(Button as Button_Icon).index], 0); } 
        }
        #endregion

        public Button_Description_360(Inputs input) : base(input) { }
        public Button_Description_360(Inputs input, Texture2D description_texture, Rectangle description_rect) : base(input, description_texture, description_rect) { }

        protected override void set_button(Inputs input)
        {
            Button = new Button_Icon(input, Global.Content.Load<Texture2D>(@"Graphics/Pictures/Buttons_360"));
        }
    }
}
