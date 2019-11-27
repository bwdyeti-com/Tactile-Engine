using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Menus.Title
{
    class CreditsMenu : StandardMenu
    {
        private Menu_Background Background;

        public CreditsMenu()
        {
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Pictures\Preparation_Background");
            (Background as Menu_Background).vel = new Vector2(0, -1 / 4f);
            (Background as Menu_Background).tile = new Vector2(1, 2);
            Background.tint = new Color(160, 160, 160, 255);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
        }

        #region StandardMenu Abstract
        public override int Index
        {
            get { return -1; }
        }

        protected override bool SelectedTriggered(bool active)
        {
            return false;
        }

        protected override void UpdateStandardMenu(bool active)
        {
            Background.update();
        }
        #endregion

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = base.CanceledTriggered(active);
            if (active)
            {
                cancel |= Global.Input.triggered(Inputs.B);
                cancel |= Global.Input.mouse_click(MouseButtons.Right);
            }
            return cancel;
        }
        
        #region IMenu
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DataDisplayed)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Background.draw(spriteBatch);
                spriteBatch.End();
            }

            base.Draw(spriteBatch);
        }
        #endregion
    }
}
