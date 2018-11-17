using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.UserInterface.Status
{
    class StatusTextUINode : StatusUINode
    {
        protected Func<Game_Unit, string> TextFormula;
        protected FE_Text Text;
        protected bool Center { get; private set; }

        internal StatusTextUINode(
                string helpLabel,
                Func<Game_Unit, string> textFormula,
                bool center = false)
            : base(helpLabel)
        {
            TextFormula = textFormula;
            Center = center;

            initialize_text(new Vector2(0, 0));
        }

        protected virtual void initialize_text(Vector2 draw_offset)
        {
            Text = new FE_Text();
            Text.draw_offset = draw_offset;
            Text.Font = "FE7_Text";
            set_color("White");
        }

        internal void set_color(string color)
        {
            Text.texture = Global.Content.Load<Texture2D>(
                string.Format(@"Graphics/Fonts/FE7_Text_{0}", color));
        }

        internal override void refresh(Game_Unit unit)
        {
            Text.text = TextFormula(unit);
            if (Center)
                Text.offset.X = Font_Data.text_width(Text.text) / 2;
            else
                Text.offset.X = 0;
        }

        protected override void update_graphics(bool activeNode)
        {
            Text.update();
        }

        protected override Vector2 HitBoxLoc(Vector2 drawOffset)
        {
            Vector2 loc = base.HitBoxLoc(drawOffset);
            if (Center)
                loc.X -= Size.X / 2;
            return loc;
        }

        protected override void mouse_off_graphic()
        {
            Text.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            Text.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            Text.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Text.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
