using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Menus
{
    abstract class MenuManager
    {
        protected Stack<IMenu> Menus = new Stack<IMenu>();

        public bool Finished { get { return Menus.Count == 0; } }

        protected void AddMenu(IMenu menu)
        {
            // Deactivate top menu
            if (Menus.Count > 0)
                Menus.Peek().UpdateActive(false);

            Menus.Push(menu);
            RefreshVisibility();
        }

        /// <summary>
        /// Remove the topmost menu of the menu stack.
        /// </summary>
        protected void RemoveTopMenu()
        {
            Menus.Pop();
            // Reactivate new top menu
            if (Menus.Count > 0)
                Menus.Peek().UpdateActive(true);
            RefreshVisibility();
        }
        /// <summary>
        /// Remove the topmost menu of the menu stack, if it is the passed menu.
        /// </summary>
        protected bool RemoveTopMenu(IMenu menu)
        {
            if (Menus.Any() && Menus.Peek() == menu)
            {
                RemoveTopMenu();
                return true;
            }

            return false;
        }

        private void RefreshVisibility()
        {
            bool visible = true;
            // Iterating over a stack naturally yields the newest item first
            foreach (var menu in Menus)
            {
                menu.IsVisible = visible;
                if (menu.HidesParent)
                    visible = false;
            }
        }

        public void Update(bool active = true)
        {
            // Update menus under the top menu, starting from the bottom
            foreach (var menu in Menus
                .Skip(1)
                .Reverse())
            {
                menu.Update(false);
            }

            // Update the menu on top of the stack
            if (Menus.Count > 0)
            {
                Menus.Peek().Update(active);
                RefreshVisibility();
            }
        }

        public void Draw(
            SpriteBatch spriteBatch,
            GraphicsDevice device,
            RenderTarget2D[] renderTargets)
        {
            // Stack needs reversed to draw the newest item last
            foreach (var menu in Menus.Reverse())
            {
                if (menu.IsVisible)
                    menu.Draw(spriteBatch, device, renderTargets);
            }
        }
    }
}
