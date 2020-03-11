using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface;

namespace FEXNA.Graphics.Help
{
    class Button_Description : UINode
    {
        readonly static int[] Offsets = new int[] { 0, 0, 0, 0, 19, 19, 19, 19, 22, 22, 38, 41 };

        protected Sprite Button;
        protected Sprite Description;

        #region Accessors
        public string description
        {
            set
            {
                if (Description is FE_Text)
                {
                    (Description as FE_Text).text = value;

                    refresh_size(Description as FE_Text);
                }
            }
        }

        protected virtual Vector2 button_offset
        {
            get { return new Vector2(Offsets[(Button as Button_Icon).index], 0); }
        }

        protected virtual Texture2D description_texture
        {
            get { return Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White"); }
        }

        public int width
        {
            get
            {
                return (int)button_offset.X + (Description is FE_Text ?
                    (Description as FE_Text).text_width : Description.src_rect.Width);
            }
        }

        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Button.stereoscopic = value;
                Description.stereoscopic = value;
            }
        }

        protected override IEnumerable<Inputs> ValidInputs
        {
            get
            {
                yield break;
            }
        }
        protected override bool RightClickActive { get { return false; } }

        internal override bool Enabled { get { return true; } }

        internal Color tint
        {
            get { return Button.tint; }
            set
            {
                Button.tint = value;
                Description.tint = value;
            }
        }
        #endregion

        public static Button_Description button(Inputs input, int x)
        {
            return button(input, new Vector2(x, Config.WINDOW_HEIGHT - 16));
        }
        public static Button_Description button(Inputs input, Vector2 loc)
        {
            Button_Description result;
            if (Input.ControlScheme == ControlSchemes.Touch)
                result = new Button_Description_Touch(input);
            else if (!Input.Controller_Active)
                result = new Button_Description_Keyboard(input);
            else
            {
                if (Global.gameSettings.Controls.IconSet == Options.ButtonIcons.Xbox360)
                    result = new Button_Description_360(input);
                else
                    result = new Button_Description(input);
            }
            result.loc = loc;
            return result;
        }
        public static Button_Description button(Inputs input, Texture2D description_texture, Rectangle description_rect)
        {
            return button(input, description_texture, description_rect, Vector2.Zero);
        }
        public static Button_Description button(Inputs input, Texture2D description_texture, Rectangle description_rect, int x)
        {
            return button(input, description_texture, description_rect, new Vector2(x, Config.WINDOW_HEIGHT - 16));
        }
        public static Button_Description button(Inputs input, Texture2D description_texture, Rectangle description_rect, Vector2 loc)
        {
            Button_Description result;
            if (Input.ControlScheme == ControlSchemes.Touch)
                result = new Button_Description_Touch(input, description_texture, description_rect);
            else if (!Input.Controller_Active)
                result = new Button_Description_Keyboard(input, description_texture, description_rect);
            else
            {
                if (Global.gameSettings.Controls.IconSet == Options.ButtonIcons.Xbox360)
                    result = new Button_Description_360(input, description_texture, description_rect);
                else
                    result = new Button_Description(input, description_texture, description_rect);
            }
            result.loc = loc;
            return result;
        }

        public Button_Description(Inputs input)
        {
            set_button(input);

            FE_Text description = new FE_Text();
            description.loc = this.button_offset;
            //description.loc = new Vector2(Offsets[(int)input], 0);
            description.Font = "FE7_Text";
            description.texture = description_texture;
            Description = description;

            refresh_size(description);
        }
        protected Button_Description(Inputs input, Texture2D description_texture, Rectangle description_rect)
        {
            set_button(input);

            Description = new Sprite(description_texture);
            Description.loc = this.button_offset;
            Description.src_rect = description_rect;

            refresh_size(new Vector2(description_rect.Width, description_rect.Height));
        }

        protected virtual void set_button(Inputs input)
        {
            Button = new Button_Icon(input, Global.Content.Load<Texture2D>(@"Graphics/Pictures/Buttons"));
        }

        private void refresh_size(FE_Text description)
        {
            refresh_size(new Vector2(description.text_width, description.CharHeight));
        }
        protected virtual void refresh_size(Vector2 descriptionSize)
        {
            Size = new Vector2(
                button_offset.X + descriptionSize.X,
                Math.Max(16, descriptionSize.Y));
        }

        protected override void update_graphics(bool activeNode)
        {
            Button.update();
            Description.update();
        }

        protected override void mouse_off_graphic()
        {
            Button.tint = Color.White;
            Description.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            Button.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
            Description.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            Button.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
            Description.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Button.draw(sprite_batch, draw_offset + offset - (loc + this.draw_offset));
            Description.draw(sprite_batch, draw_offset + offset - (loc + this.draw_offset));
        }
    }
}
