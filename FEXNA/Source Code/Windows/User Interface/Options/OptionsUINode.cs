using System.Collections.Generic;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Windows.UserInterface.Options
{
    class OptionsUINode : UINode
    {
        internal Constants.Options Option { get; private set; }
        private Icon_Sprite OptionsIcon;
        private FE_Text Label;

        protected override IEnumerable<Inputs> ValidInputs
        {
            get
            {
                yield break;
            }
        }
        protected override bool RightClickActive { get { return false; } }

        internal OptionsUINode(int optionIndex)
        {
            Option = (Constants.Options)optionIndex;

            OptionsIcon = new Icon_Sprite();
            OptionsIcon.size = new Vector2(16, 16);
            OptionsIcon.draw_offset = new Vector2(-16, 0);
            OptionsIcon.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Options_Icons");
            OptionsIcon.index = optionIndex;

            Label = new FE_Text("FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White"),
                new Vector2(0, 0),
                Constants.OptionsConfig.OPTIONS_DATA[optionIndex].Label);

            Size = new Vector2(80, 16);
        }

        protected override void update_graphics(bool activeNode)
        {
            Label.update();
            OptionsIcon.update();
        }

        protected override Vector2 HitBoxLoc(Vector2 drawOffset)
        {
            return base.HitBoxLoc(drawOffset + new Vector2(16, 0));
        }

        protected override void mouse_off_graphic()
        {
            OptionsIcon.tint = Color.White;
            Label.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            OptionsIcon.tint = Color.White;
            Label.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            OptionsIcon.tint = Color.White;
            Label.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
            //OptionsIcon.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR; //Debug
            //Label.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            OptionsIcon.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Label.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
