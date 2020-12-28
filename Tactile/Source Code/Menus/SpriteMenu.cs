using System;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Menus
{
    class SpriteMenu : StandardMenu
    {
        private Sprite MenuSprite;
        private bool NeedsBatchBeginCall;

        public SpriteMenu(Sprite sprite, bool needsBatchBeginCall = true) : base()
        {
            MenuSprite = sprite;
            NeedsBatchBeginCall = needsBatchBeginCall;
        }

        protected override void CreateCancelButton(IHasCancelButton menu) { }

        #region StandardMenu Abstract
        public override int Index { get { return -1; } }

        protected override bool SelectedTriggered(bool active)
        {
            if (!active)
                return false;

            bool selected = active &&
                (Global.Input.triggered(Inputs.A) ||
                Global.Input.mouse_click(MouseButtons.Left) ||
                Global.Input.gesture_triggered(TouchGestures.Tap));
            return selected;
        }

        protected override void UpdateStandardMenu(bool active)
        {
            MenuSprite.update();
        }
        #endregion

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = active &&
                (Global.Input.triggered(Inputs.B) ||
                Global.Input.mouse_click(MouseButtons.Right) ||
                Global.Input.gesture_triggered(TouchGestures.LongPress));
            return cancel || base.CanceledTriggered(active);
        }

        protected override void Cancel()
        {
            OnClosed(new EventArgs());
        }

        #region IFadeMenu
        public override ScreenFadeMenu FadeInMenu(bool skipFadeIn = false) { return null; }
        public override ScreenFadeMenu FadeOutMenu() { return null; }
        #endregion

        #region IMenu
        public override bool HidesParent { get { return false; } }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DataDisplayed)
            {
                if (NeedsBatchBeginCall)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    MenuSprite.draw(spriteBatch);
                    spriteBatch.End();
                }
                else
                    MenuSprite.draw(spriteBatch);
            }

            base.Draw(spriteBatch);
        }
        #endregion
    }
}
