using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;
using Tactile.Menus.Title;

namespace Tactile.Windows.UserInterface.Title
{
    class CommunityUINode : UINode
    {
        protected Sprite Panel;
        protected TextSprite Name;

        protected override IEnumerable<Inputs> ValidInputs
        {
            get
            {
                yield return Inputs.A;
            }
        }
        protected override bool RightClickActive { get { return true; } }

        internal CommunityUINode(CommunityEntry entry)
        {
            Name = new TextSprite();
            Name.draw_offset = new Vector2(36, 8);
            Name.SetFont(Config.UI_FONT, Global.Content, "White");
            Name.text = entry.Name;

            Panel = new Sprite();
            if (Global.content_exists(@"Graphics/Titles/" + entry.Texture))
                Panel.texture = Global.Content.Load<Texture2D>(@"Graphics/Titles/" + entry.Texture);
            Panel.draw_offset = new Vector2(0, 0);

            Size = new Vector2(128, 32);
        }

        protected override void update_graphics(bool activeNode)
        {
            Name.update();
            Panel.update();
        }

        protected override void mouse_off_graphic()
        {
            Name.tint = Color.White;
            Panel.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            Name.tint = Color.White;
            Panel.tint = Color.White;
        }
        protected override void mouse_click_graphic()
        {
            Name.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
            Panel.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Panel.draw(sprite_batch, draw_offset - loc);
            Name.draw(sprite_batch, draw_offset - loc);
        }
    }
}
