using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Windows.UserInterface.Title
{
    enum SupportViewerState { Disabled, Enabled, Capped }

    class SupportViewerRankUINode : TextUINode
    {
        internal SupportViewerRankUINode(
                string helpLabel,
                int rank,
                SupportViewerState state)
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
                    _enabled = false;
                    break;
            }

            var text = new FE_Text();
            text.texture = Global.Content.Load<Texture2D>(
                string.Format(@"Graphics\Fonts\FE7_Text_{0}", color));
            text.text = Constants.Support.SUPPORT_LETTERS[rank];
            text.Font = text.text == "-" ? "FE7_Text" : "FE7_TextL";

            Text = text;
        }
    }
}
