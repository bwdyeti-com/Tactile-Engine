using System;
using Tactile.ConfigData;
using Tactile.Graphics.Text;
using Tactile.Windows.UserInterface.Command;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Windows.UserInterface.Options
{
    class UnitHeaderUINode : CommandUINode
    {
        protected Sprite Label;

        internal UnitHeaderUINode(
                string helpLabel,
                UnitScreenData config)
            : base(helpLabel)
        {
            if (config.WeaponIcon >= 0)
            {
                var icon = new Weapon_Type_Icon();
                icon.index = config.WeaponIcon;

                Label = icon;
                Size = icon.size;
            }
            else
            {
                var text = new TextSprite(
                    Config.UI_FONT, Global.Content, "White",
                    Vector2.Zero,
                    config.Name);

                Label = text;
                Size = new Vector2(Math.Max(12, text.text_width), 16);
            }
        }

        internal override void set_text_color(string color) { }

        protected override void update_graphics(bool activeNode)
        {
            Label.update();
        }

        protected override void mouse_off_graphic()
        {
            Label.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            Label.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            Label.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Label.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
