using System.Collections.Generic;

namespace FEXNA
{
    public enum Anim_Types { None, Sword, Lance, Axe, Bow, ThrowAxe, Magic, Staff, Unique }
    public class Class_Battler_Frames
    {
        public static bool avoids_forward(int class_id)
        {
            switch (class_id)
            {
                case 4: // Skywatcher
                    return true;
                case 7: // Scout
                    return true;
                case 17: // Thief
                    return true;
                case 19: // Archer
                    return true;
                case 22: // Soldier
                    return true;
                case 24: // Fighter
                    return false;
                case 35: // Zweihander
                    return true;
                case 40: // Mage
                    return false;
                case 41: // Troubadour
                    return false;
                case 55: // Longbowman
                    return true;
                case 65: // Hero
                    return true;
                case 81: // Valkyrie
                    return false;
                case 96: // Lord, Uther
                    return false;
            }
            return false;
        }
    }
}
