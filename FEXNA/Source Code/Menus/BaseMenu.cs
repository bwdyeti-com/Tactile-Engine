using System;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Menus
{
    abstract class BaseMenu : IMenu
    {
        protected bool Visible = true;
        protected bool Active { get; private set; }

        protected BaseMenu()
        {
            Active = true;
        }

        protected virtual void Activate() { }
        protected virtual void Deactivate() { }

        protected abstract void UpdateMenu(bool active);

        #region IMenu
        public virtual bool HidesParent { get { return true; } }
        public bool IsVisible
        {
            get { return Visible; }
            set { Visible = value; }
        }

        public void UpdateActive(bool active)
        {
            if (Active != active)
            {
                // Active needs to be tracked for calling Activate() and Deactivate()
                if (active)
                    Activate();
                else
                    Deactivate();

                Active = active;
            }
        }

        public void Update(bool active)
        {
            UpdateActive(active);

            UpdateMenu(Active);
        }
        public abstract void Draw(SpriteBatch spriteBatch);
        public virtual void Draw(
            SpriteBatch spriteBatch,
            GraphicsDevice device,
            RenderTarget2D[] renderTargets)
        {
            Draw(spriteBatch);
        }
        #endregion
    }
}
