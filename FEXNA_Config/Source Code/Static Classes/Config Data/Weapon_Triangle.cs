using System.Collections.Generic;
using FEXNA_Library;

namespace FEXNA
{
    public class Weapon_Triangle
    {
        public const bool IN_EFFECT = true; // Weapon triangle is active?
        public const bool UNARMED_DISADVANTAGE = true;
        public const int DMG_BONUS = 3; //1; // Slayers and reavers -1 mgt to account for this; also swift; -2 on swordslayer //Debug
        public const int HIT_BONUS = 15;

        /*public readonly static Dictionary<Weapon_Types, Weapon_Types[][]> DATA = new Dictionary<Weapon_Types, Weapon_Types[][]> //Debug
        {
            // Normal; Reaver
            // Sword => Axe;   Lance
            { Weapon_Types.Sword,   new Weapon_Types[][] { new Weapon_Types[] { Weapon_Types.Axe },
                                                        new Weapon_Types[] { Weapon_Types.Lance }}},
            // Lance => Sword; Axe
            { Weapon_Types.Lance,   new Weapon_Types[][] { new Weapon_Types[] { Weapon_Types.Sword },
                                                        new Weapon_Types[] { Weapon_Types.Axe }}},
            // Axe   => Lance; Sword
            { Weapon_Types.Axe,     new Weapon_Types[][] { new Weapon_Types[] { Weapon_Types.Lance },
                                                        new Weapon_Types[] { Weapon_Types.Sword }}},
            // Bow   =>
            { Weapon_Types.Bow,     new Weapon_Types[][] { new Weapon_Types[] { },
                                                        new Weapon_Types[] { }}},
            // Fire  => Wind,  Light; Thund, Light
            { Weapon_Types.Fire,    new Weapon_Types[][] { new Weapon_Types[] { Weapon_Types.Wind, Weapon_Types.Light },
                                                        new Weapon_Types[] { Weapon_Types.Thunder, Weapon_Types.Light }}},
            // Thund => Fire,  Light; Wind,  Light
            { Weapon_Types.Thunder, new Weapon_Types[][] { new Weapon_Types[] { Weapon_Types.Fire, Weapon_Types.Light },
                                                        new Weapon_Types[] { Weapon_Types.Wind, Weapon_Types.Light }}},
            // Wind  => Thund, Light; Fire,  Light
            { Weapon_Types.Wind,    new Weapon_Types[][] { new Weapon_Types[] { Weapon_Types.Thunder, Weapon_Types.Light },
                                                        new Weapon_Types[] { Weapon_Types.Fire, Weapon_Types.Light }}},
            // Light => Dark
            { Weapon_Types.Light,   new Weapon_Types[][] { new Weapon_Types[] { Weapon_Types.Dark },
                                                        new Weapon_Types[] { Weapon_Types.Dark }}},
            // Dark  => Fire, Thund, Wind
            { Weapon_Types.Dark,    new Weapon_Types[][] { new Weapon_Types[] { Weapon_Types.Fire, Weapon_Types.Thunder, Weapon_Types.Wind },
                                                        new Weapon_Types[] { Weapon_Types.Fire, Weapon_Types.Thunder, Weapon_Types.Wind }}}
        };

        public readonly static Dictionary<Weapon_Types, int[][]> RANGE_DATA = new Dictionary<Weapon_Types, int[][]>
        {
            // WTA range; WTD range
            { Weapon_Types.Sword,   new int[][] { new int[] { },   new int[]{ }}},   // Sword => 
            { Weapon_Types.Lance,   new int[][] { new int[] { },   new int[]{ }}},   // Lance => 
            { Weapon_Types.Axe,     new int[][] { new int[] { },   new int[]{ }}},   // Axe   => 
            { Weapon_Types.Bow,     new int[][] { new int[] { 2 }, new int[]{ 1 }}}, // Bow   => WTA at 2 range; WTD at 1 range
            { Weapon_Types.Fire,    new int[][] { new int[] { },   new int[]{ }}},   // Fire  => 
            { Weapon_Types.Thunder, new int[][] { new int[] { },   new int[]{ }}},   // Thund => 
            { Weapon_Types.Wind,    new int[][] { new int[] { },   new int[]{ }}},   // Wind  => 
            { Weapon_Types.Light,   new int[][] { new int[] { },   new int[]{ }}},   // Light => 
            { Weapon_Types.Dark,    new int[][] { new int[] { },   new int[]{ }}}    // Dark  => 
        };*/
    }
}
