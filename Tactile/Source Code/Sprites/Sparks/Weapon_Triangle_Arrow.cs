using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TactileWeaponExtension;

namespace Tactile
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

        public virtual void set_effectiveness(int effectiveness, bool halved = false) { }

        private void ResetTriangle()
        {
            set_effectiveness(1);
            this.value = WeaponTriangle.Nothing;
        }
        
        public static void ResetWeaponTriangle(
            Weapon_Triangle_Arrow wta1,
            Weapon_Triangle_Arrow wta2)
        {
            wta1.ResetTriangle();
            if (wta2 != null)
                wta2.ResetTriangle();
        }

        public static void SetWeaponTriangle(
            Weapon_Triangle_Arrow wta1,
            Weapon_Triangle_Arrow wta2,
            Game_Unit attacker,
            Combat_Map_Object target,
            TactileLibrary.Data_Weapon weapon1,
            TactileLibrary.Data_Weapon weapon2,
            int distance)
        {
            //@Yeti: use in all the places weapon triangle arrow is used
            Game_Unit targetUnit = null;
            if (target.is_unit())
                targetUnit = target as Game_Unit;

            ResetWeaponTriangle(wta1, wta2);
            WeaponTriangle tri = WeaponTriangle.Nothing;

            if (targetUnit != null)
            tri = Combat.weapon_triangle(attacker, targetUnit, weapon1, weapon2, distance);
            if (tri != WeaponTriangle.Nothing)
            {
                wta1.value = tri;
                if (wta2 != null && weapon2 != null)
                    wta2.value = Combat.reverse_wta(tri);
            }

            float effectiveness = weapon1.effective_multiplier(attacker, targetUnit, false);
            wta1.set_effectiveness((int)effectiveness,
                targetUnit != null && targetUnit.halve_effectiveness());

            if (wta2 != null && weapon2 != null)
            {
                effectiveness = weapon2.effective_multiplier(targetUnit, attacker, false);
                wta2.set_effectiveness((int)effectiveness, attacker.halve_effectiveness());
            }
        }

        public static void SetWeaponTriangle(
            Weapon_Triangle_Arrow wta,
            Game_Unit attacker,
            Combat_Map_Object target,
            TactileLibrary.Data_Weapon weapon1,
            TactileLibrary.Data_Weapon weapon2,
            int distance)
        {
            SetWeaponTriangle(
                wta, null,
                attacker, target,
                weapon1, weapon2,
                distance);
        }
    }
}
