using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile.Windows.UserInterface.Title
{
    class Mode_Style_Info_Panel : Title_Info_Panel
    {
        internal const int WIDTH = 240;
        protected bool Active;
        TextSprite Style, Description;

        #region Accessors
        public bool active
        {
            set
            {
                Active = value;
                int alpha;
                if (Active)
                    alpha = 255;
                else
                    alpha = 160;
                Window.tint = new Color(alpha, alpha, alpha, 255);
                Style.tint = new Color(alpha, alpha, alpha, 255);
                Description.tint = new Color(alpha, alpha, alpha, 255);
            }
        }

        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Style.stereoscopic = value;
                Description.stereoscopic = value;
            }
        }
        #endregion

        public Mode_Style_Info_Panel(Mode_Styles style)
        {
            var window = new System_Color_Window();
            window.width = WIDTH;
            window.height = 48;
            window.color_override = Constants.Difficulty.STYLE_COLOR_REDIRECT[style];
            window.small = true;
            Window = window;

            Size = new Vector2(WIDTH, Window.height);

            Style = new TextSprite();
            Style.loc = new Vector2(24, -8);
            Style.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Style.text = style.ToString();
            Description = new TextSprite();
            Description.loc = new Vector2(16, 8);
            Description.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Description.text = Global.system_text["Style " + style.ToString()];

            active = false;
        }

        //Debug
        protected override void mouse_off_graphic() { }
        protected override void mouse_highlight_graphic() { }
        protected override void mouse_click_graphic() { }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = (draw_offset + this.offset) -
                (this.loc + this.draw_offset + stereo_offset());

            Window.draw(sprite_batch, loc);
            Style.draw(sprite_batch, loc);
            Description.draw(sprite_batch, loc);
        }
    }
}
