using System;
using System.Collections.Generic;
using Tactile.Graphics.Text;
using Tactile.Windows.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Windows.UserInterface.Options
{
    class SettingUINode : UINode
    {
        protected TextSprite Label;

        protected override IEnumerable<Inputs> ValidInputs
        {
            get
            {
                yield break;
            }
        }
        protected override bool RightClickActive { get { return false; } }

        protected SettingUINode() { }
        internal SettingUINode(string label)
        {
            Label = new TextSprite(
                Config.UI_FONT, Global.Content, "Grey",
                new Vector2(0, 0),
                label);

            Size = new Vector2(Math.Max(12, Label.text_width), 16);
        }

        protected override void update_graphics(bool activeNode)
        {
            Label.update();
        }

        internal void refresh_active(bool active)
        {
            if (active)
                Label.SetColor(Global.Content, "Blue");
            else
                Label.SetColor(Global.Content, "Grey");
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
