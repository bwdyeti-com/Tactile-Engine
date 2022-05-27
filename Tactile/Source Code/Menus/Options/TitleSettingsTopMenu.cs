using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Menus.Options
{
    class TitleSettingsTopMenu : SettingsTopMenu
    {
        private TextSprite VersionNumber;

        public TitleSettingsTopMenu() : base()
        {
            // Version Number
            VersionNumber = new TextSprite();
            VersionNumber.loc = new Vector2(8, Config.WINDOW_HEIGHT - 16);
            VersionNumber.SetFont(Config.UI_FONT, Global.Content, "White");
            VersionNumber.text = string.Format("v {0}", Global.RUNNING_VERSION);
#if PRERELEASE || DEBUG
            VersionNumber.text += " - Private Beta";
#endif
            VersionNumber.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
        }

        #region StandardMenu Abstract
        protected override void UpdateStandardMenu(bool active)
        {
            base.UpdateStandardMenu(active);

            VersionNumber.update();
        }
        #endregion

        protected override void DrawData(SpriteBatch spriteBatch)
        {
            base.DrawData(spriteBatch);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            VersionNumber.draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
