using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEXNA
{
    class Animation_Setup_Data_Processor : Animation_Setup_Data
    {
        public Animation_Setup_Data_Processor(BattlerSpriteData battler, int distance)
        {
            Key = string.Format("{0}-{1}", battler.ClassNameFull,
                battler.UsedWeaponType);
            Distance = distance;
            Class_Id = battler.ClassId;
            Gender = battler.ActualGender;

            FEXNA_Library.Data_Weapon weapon = battler.Weapon;
            Equipped = weapon != null;
            if (Equipped)
            {
                Weapon_Main_Type = weapon.Main_Type;
                Weapon_Thrown = weapon.Thrown(); // Factor out to actor or something to make weapons throwable by skills??? //Debug
                Weapon_Is_Staff = weapon.is_staff();
                MWeapon = (distance >= 2 && weapon.is_ranged_magic()) || weapon.is_always_magic();
            }
            Skill = battler.SkillEffect;

            // Skills
            Astra_Activated = battler.Unit.actor.astra_activated;
            Astra_Missed = battler.Unit.actor.astra_missed;
            Astra_Count = battler.Unit.actor.astra_count;
            Swoop_Activated = battler.Unit.swoop_activated;
        }
    }
}
