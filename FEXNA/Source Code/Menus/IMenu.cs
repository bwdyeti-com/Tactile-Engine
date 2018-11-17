using System;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Menus
{
    interface IMenu
    {
        bool HidesParent { get; }
        bool IsVisible { get; set; }

        void UpdateActive(bool active);

        void Update(bool active);
        void Draw(SpriteBatch spriteBatch);
    }
}
