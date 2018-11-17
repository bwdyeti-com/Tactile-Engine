using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Multiplier_Img : Sprite
    {
        protected int Timer = 0;
        protected int Value = 0;

        #region Accessors
        public int timer
        {
            set { Timer = (int)MathHelper.Clamp(value, 0, 63); }
        }

        public int value
        {
            set { Value = (int)MathHelper.Clamp(value, 0, 3); }
        }

        public override Rectangle src_rect
        {
            get
            {
                if (Value == 0)
                    return new Rectangle(0, 0, 0, 0);
                return new Rectangle(16 * (Value - 1), 0, 14, 10);
            }
        }
        #endregion

        public Multiplier_Img()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Multipliers");
            draw_offset = new Vector2(0, -1);
        }

        public int get_multi(Game_Unit unit, Combat_Map_Object target, FEXNA_Library.Data_Weapon weapon, int distance)
        {
            int multi = 0;
            if (!target.is_unit())
                return 0;
            if (weapon.Brave() && !unit.is_brave_blocked())
                multi += 4;
            if (unit.can_double_attack(target, distance))
                multi += 2;
            return multi / 2;
        }

        public override void update()
        {
            switch (Timer)
            {
                case 0:
                case 1:
                    offset = new Vector2(0, 1);
                    break;
                case 2:
                case 3:
                case 4:
                    offset = new Vector2(-1, 1);
                    break;
                case 5:
                case 6:
                case 7:
                    offset = new Vector2(-2, 1);
                    break;
                case 8:
                case 9:
                    offset = new Vector2(-3, 1);
                    break;
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                    offset = new Vector2(-3, 2);
                    break;
                case 15:
                    offset = new Vector2(-4, 2);
                    break;
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                    offset = new Vector2(-3, 3);
                    break;
                case 21:
                case 22:
                    offset = new Vector2(-3, 4);
                    break;
                case 23:
                case 24:
                case 25:
                    offset = new Vector2(-2, 4);
                    break;
                case 26:
                case 27:
                case 28:
                    offset = new Vector2(-1, 4);
                    break;
                case 29:
                case 30:
                case 31:
                    offset = new Vector2(0, 4);
                    break;
                case 32:
                case 33:
                    offset = new Vector2(1, 4);
                    break;
                case 34:
                case 35:
                case 36:
                    offset = new Vector2(2, 4);
                    break;
                case 37:
                case 38:
                case 39:
                    offset = new Vector2(3, 4);
                    break;
                case 40:
                case 41:
                    offset = new Vector2(4, 4);
                    break;
                case 42:
                case 43:
                case 44:
                case 45:
                case 46:
                    offset = new Vector2(4, 3);
                    break;
                case 47:
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                    offset = new Vector2(4, 2);
                    break;
                case 53:
                case 54:
                    offset = new Vector2(4, 1);
                    break;
                case 55:
                case 56:
                case 57:
                    offset = new Vector2(3, 1);
                    break;
                case 58:
                case 59:
                case 60:
                    offset = new Vector2(2, 1);
                    break;
                case 61:
                case 62:
                    offset = new Vector2(1, 1);
                    break;
                case 63:
                    offset = new Vector2(0, 0);
                    break;
            }
            Timer = (Timer + 1) % 64;
        }
    }
}
