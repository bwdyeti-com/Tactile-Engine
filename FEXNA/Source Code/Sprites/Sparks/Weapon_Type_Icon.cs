using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Weapon_Type_Icon : Icon_Sprite
    {
        public Weapon_Type_Icon()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Weapon Level Icons");
            columns = 1;
            size = new Vector2(16, 16);
        }
    }
}
