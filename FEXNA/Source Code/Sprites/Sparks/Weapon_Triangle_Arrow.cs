using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Weapon_Triangle_Arrow : Sprite
    {
        WeaponTriangle Value = WeaponTriangle.Nothing;
        int Timer = 0;

        #region Accessors
        public int timer { set { Timer = value; } }

        public WeaponTriangle value { set { Value = value; } }

        private int index { get { return (int)Value - 1; } }
        #endregion

        public Weapon_Triangle_Arrow()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Arrows");
            draw_offset = new Vector2(-1, 1);
            offset = new Vector2(-12, -4);
        }

        public Weapon_Triangle_Arrow(Texture2D texture)
        {
            this.texture = texture;
            draw_offset = new Vector2(-1, 1);
            offset = new Vector2(-12, -4);
        }

        public override void update()
        {
            Timer = (Timer + 1) % 24;
        }

        public override Rectangle src_rect
        {
            get
            {
                if (Value == WeaponTriangle.Nothing)
                    return new Rectangle(0, 0, 0, 0);
                return new Rectangle(7 * (Timer / 8), 48 + (index % 2) * 10, 7, 10);
            }
        }
    }
}
