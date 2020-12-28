using System;
using Microsoft.Xna.Framework;
using TactileLibrary;

namespace Tactile
{
    class WeaponTriangleHelpReaverTypes : WeaponTriangleHelpTypes
    {
        public WeaponTriangleHelpReaverTypes(
            WeaponType type, WeaponType advType, WeaponType disType)
            : base(type, advType, disType) { }

        protected override Weapon_Triangle_Arrow advantage_arrow(float angle)
        {
            var result = new Weapon_Triangle_Arrow();
            result.value = WeaponTriangle.Advantage;
            result.offset = new Vector2(
                result.src_rect.Width / 2, result.src_rect.Height / 2);
            result.angle = angle + MathHelper.Pi;
            result.loc = new Vector2(0, 8) + 12 * new Vector2(
                (float)Math.Cos(angle), (float)Math.Sin(angle));
            return result;
        }
    }
}
