using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.Title;
using System;

namespace FEXNA.Menus.Title
{
    class SupportViewerMenu : StandardMenu
    {
        private WindowSupportViewerActorList Window;
        private Menu_Background Background;

        public SupportViewerMenu() : base()
        {
            // Window
            Window = new WindowSupportViewerActorList();
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Pictures\Preparation_Background");
            (Background as Menu_Background).vel = new Vector2(0, -1 / 3f);
            (Background as Menu_Background).tile = new Vector2(1, 2);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
        }

        #region StandardMenu Abstract
        public override int Index
        {
            get { return Window.index; }
        }

        protected override bool SelectedTriggered(bool active)
        {
            if (!active)
                return false;

            var selected = Window.consume_triggered(Inputs.A, MouseButtons.Left, TouchGestures.Tap);
            return selected.IsSomething;
        }

        protected override void UpdateStandardMenu(bool active)
        {
            // Needed to animate map sprites
            Global.game_system.update_timers();

            Window.update(active);
            Background.update();
        }
        #endregion

        public int ActorId { get { return Window.actor_id; } }
        
        protected override int DefaultCancelPosition { get { return 16; } }

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = active && Global.Input.triggered(Inputs.B);
            return cancel || base.CanceledTriggered(active);
        }

        #region IMenu
        public override bool HidesParent { get { return DataDisplayed; } }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DataDisplayed)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Background.draw(spriteBatch);
                spriteBatch.End();

                Window.draw(spriteBatch);
            }

            base.Draw(spriteBatch);
        }
        #endregion
    }
}
