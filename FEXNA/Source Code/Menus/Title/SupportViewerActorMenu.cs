using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Preparations;
using FEXNA.Windows.Command;

namespace FEXNA.Menus.Title
{
    class SupportViewerActorMenu : StandardMenu
    {
        private int ActorId;
        private WindowCommandSupportViewerActor Window;

        public SupportViewerActorMenu(int actorId, IHasCancelButton menu) : base(menu)
        {
            // Skip fade
            FadeShow();

            ActorId = actorId;
            Window = new WindowCommandSupportViewerActor(
                ActorId,
                new Vector2(Config.WINDOW_WIDTH - 152, 16));
        }

        #region StandardMenu Abstract
        public override int Index { get { return -1; } }

        protected override bool SelectedTriggered(bool active)
        {
            return false;
        }

        protected override void UpdateStandardMenu(bool active)
        {
            Window.update(active);
        }
        #endregion

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = Window.is_canceled();
            return cancel || base.CanceledTriggered(active);
        }

        #region IFadeMenu
        public override ScreenFadeMenu FadeInMenu(bool skipFadeIn = false) { return null; }
        public override ScreenFadeMenu FadeOutMenu() { return null; }
        #endregion

        #region IMenu
        public override bool HidesParent { get { return false; } }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Window.draw(spriteBatch);

            base.Draw(spriteBatch);
        }
        #endregion
    }
}
