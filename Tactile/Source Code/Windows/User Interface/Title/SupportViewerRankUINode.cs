using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Windows.UserInterface.Command;

namespace Tactile.Windows.UserInterface.Title
{
    enum SupportViewerState { Disabled, Enabled, Capped }

    class SupportViewerRankUINode : TextUINode
    {
        private TextSprite BaseText;
        private bool AtBase;

        internal SupportViewerRankUINode(
                string helpLabel,
                int rank,
                SupportViewerState state,
                bool fieldBaseDifference)
            : base(helpLabel, null, 8)
        {
            string color;
            switch (state)
            {
                case SupportViewerState.Capped:
                    color = "Green";
                    break;
                case SupportViewerState.Enabled:
                    color = "White";
                    break;
                case SupportViewerState.Disabled:
                default:
                    color = "Grey";
#if !DEBUG // Allow all supports in debug
                    _enabled = false;
#endif
                    break;
            }
            
            Text = GetText(rank, color);

            if (fieldBaseDifference)
            {
                BaseText = GetText(rank, color);

                mouse_off_graphic();
            }
        }

        private TextSprite GetText(int rank, string color)
        {
            var text = new TextSprite();
            text.text = Constants.Support.SUPPORT_LETTERS[rank];
            text.SetFont(
                text.text == "-" ? Config.UI_FONT : Config.UI_FONT + "L",
                Global.Content, color, Config.UI_FONT);

            return text;
        }

        public void SetAtBase(bool atBase)
        {
            AtBase = atBase;
            if (this.FieldBaseDifference)
            {
                if (AtBase)
                {
                    Text.draw_offset = new Vector2(-1, 2);
                    BaseText.draw_offset = new Vector2(1, -1);
                }
                else
                {
                    Text.draw_offset = new Vector2(-0, 1);
                    BaseText.draw_offset = new Vector2(1, -2);
                }
            }

            mouse_off_graphic();
        }

        public bool FieldBaseDifference { get { return BaseText != null; } }
        
        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);
            if (BaseText != null)
                BaseText.update();
        }

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();

            SetBackgroundTextTint();
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();

            SetBackgroundTextTint();
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();

            SetBackgroundTextTint();
        }

        private void SetBackgroundTextTint()
        {
            if (BaseText != null)
            {
                BaseText.tint = Text.tint;

                if (AtBase)
                    Text.tint = new Color(128, 128, 128, 255);
                else
                    BaseText.tint = new Color(128, 128, 128, 255);
            }
        }
        
        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (!AtBase && BaseText != null)
                BaseText.draw(sprite_batch, draw_offset - (loc + draw_vector()));

            base.Draw(sprite_batch, draw_offset);

            if (AtBase && BaseText != null)
                BaseText.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
