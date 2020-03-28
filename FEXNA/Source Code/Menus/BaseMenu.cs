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

        /// <summary>
        /// Updates menu processing.
        /// Only called if the menu <see cref="IsVisible"/>.
        /// </summary>
        /// <param name="active"></param>
        protected abstract void UpdateMenu(bool active);

        /// <summary>
        /// Updates parts of the menu that don't involve user input.
        /// Called every frame, even when the menu isn't visible.
        /// </summary>
        protected virtual void UpdateAncillary() { }

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
            // Things to always update, whether visible or not
            UpdateAncillary();

            // Don't update unless visible
            if (this.IsVisible)
            {
                UpdateActive(active);

                UpdateMenu(Active);
            }
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
