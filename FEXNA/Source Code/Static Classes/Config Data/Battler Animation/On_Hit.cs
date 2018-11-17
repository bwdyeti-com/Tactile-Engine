using System;
using System.Collections.Generic;
using IntExtension;

namespace FEXNA
{
    class On_Hit
    {
        public static bool BOTH_PLATFORM_SHAKE(int anim_id)
        {
            if (anim_id == Global.animation_group("General-Sword") + 23) // General Lance Kamaitachi
                return true;
            if (anim_id == Global.animation_group("General-Sword") + 26) // General Lance Kamaitachi (crit)
                return true;
            if (anim_id == Global.animation_group("General-Lance") + 23) // General Sword Kamaitachi
                return true;
            if (anim_id == Global.animation_group("General-Lance") + 26) // General Sword Kamaitachi (crit)
                return true;
            return false;
        }

        public readonly static Dictionary<int, int> SPELL_HIT_SOUNDS = new Dictionary<int, int>
        {
            { 12, 3 }, // Light Brand (Lightning)
            { 15, 3 }, // Wind Sword (Wind)
            { 18, 3 }, // Runesword (Nosferatu)

            { 101, 3 }, // Fire
            { 103, 3 }, // Arcfire
            { 111, 3 }, // Thunder
            { 112, 3 }, // Elthunder
            { 113, 3 }, // Arcthunder
            { 115, 3 }, // Bolting
            { 121, 3 }, //2 // Wind
            { 131, 3 }, // Lightning
            { 132, 3 }, // Shine
            { 133, 3 }, // Divine
            { 135, 3 }, // Purge
            { 142, 3 }, // Flux
            { 144, 3 } // Hekat
        };

        public readonly static Dictionary<int, int> SPELL_WHITEN_TIME = new Dictionary<int, int>
        {
            { 101, 8 }, //16 // Fire
            { 103, 8 }, // Arcfire
            { 115, 8 }, // Bolting
            { 131, 8 }, // Lightning
            { 132, 8 }, // Shine
            { 133, 4 }, // Divine
            { 142, 8 }, // Flux
            { 144, 6 } // Hekat
        };

        public static int? SPELL_BRIGHTEN_OFFSET(int anim_id)
        {
            int anim_offset = Global.animation_group("Spells");
            if (anim_id == anim_offset + 10) // Arcfire
                return 3 + 2;
            if (anim_id == anim_offset + 124) // Wind
                return 10;
            if (anim_id == anim_offset + 182) // Lightning
                return 0; //10
            if (anim_id == anim_offset + 186) // Shine
                return 53 + 13;
            if (anim_id == anim_offset + 190) // Divine
                return 32 + 2;
            if (anim_id == anim_offset + 196) // Purge
                return 10;
            if (anim_id == anim_offset + 257) // Nosferatu (miss)
                return 36;
            if (anim_id == anim_offset + 312) // Silence
                return 7;
            if (anim_id == anim_offset + 318) // Sleep
                return 15;
            if (anim_id == anim_offset + 331) // Barrier
                return 13;
            return null;
        }

        public static List<Tuple<int, List<int>>> ADDED_EFFECTS(int anim_id)
        {
            // Brigand
            if (anim_id == Global.animation_group("Brigand-Axe") + 11) // Brigand Attack
                return new List<Tuple<int, List<int>>> {
                    Tuple.Create(13, Global.animation_group("Effects").list_add(new List<int> { 1 })) // Dust
            };

            // Pirate
            if (anim_id == Global.animation_group("Pirate-Axe") + 11) // Pirate Attack
                return new List<Tuple<int, List<int>>> {
                    Tuple.Create(23, Global.animation_group("Effects").list_add(new List<int> { 1 })) // Dust
            };
            if (anim_id == Global.animation_group("Pirate-Axe") + 14) // Pirate Crit Attack
                return new List<Tuple<int, List<int>>> {
                    Tuple.Create(21, Global.animation_group("Pirate-Axe").list_add(new List<int> { 22 })) // Crit Dust
            };
            if (anim_id == Global.animation_group("Pirate-Axe") + 17) // Pirate Hold (miss)
                return new List<Tuple<int, List<int>>> {
                    Tuple.Create(1, Global.animation_group("Effects").list_add(new List<int> { 1 })) // Dust
            };

            // Corsair
            if (anim_id == Global.animation_group("Corsair-Axe") + 11) // Corsair Attack
                return new List<Tuple<int, List<int>>> {
                    Tuple.Create(26, Global.animation_group("Effects").list_add(new List<int> { 1 })) // Dust
            };
            if (anim_id == Global.animation_group("Corsair-Axe") + 14) // Corsair Crit Attack
                return new List<Tuple<int, List<int>>> {
                    Tuple.Create(21, Global.animation_group("Corsair-Axe").list_add(new List<int> { 22 })) // Crit Dust
            };
            if (anim_id == Global.animation_group("Corsair-Axe") + 17) // Corsair Hold (miss)
                return new List<Tuple<int, List<int>>> {
                    Tuple.Create(1, Global.animation_group("Effects").list_add(new List<int> { 1 })) // Dust
            };

            // Sage
            if (anim_id == Global.animation_group("Sage-Magic") + 11) // Sage Attack1
                return new List<Tuple<int, List<int>>> {
                    Tuple.Create(6, Global.animation_group("Sage-Magic").list_add(new List<int> { 29 })) // Rune
            };
            if (anim_id == Global.animation_group("Sage-Magic") + 16) // Sage Crit Attack1
                return new List<Tuple<int, List<int>>> {
                    Tuple.Create(10, Global.animation_group("Sage-Magic").list_add(new List<int>{ 30 })), // Crit Rune
                    Tuple.Create(111, Global.animation_group("Sage-Magic").list_add(new List<int> { 29 })) // Rune
            };

            // Justice
            if (anim_id == Global.animation_group("Justice-Magic") + 14) // Justice Crit Attack
                return new List<Tuple<int, List<int>>> {
                    Tuple.Create(1, Global.animation_group("Justice-Magic").list_add(new List<int> { 28, 29, 29, 30 })) // Crit Rune
            };

            // Dragon
            if (anim_id == Global.animation_group("Dragon-Dragon") + 13) // Dragon Hit
                return new List<Tuple<int, List<int>>> {
                    Tuple.Create(1, Global.animation_group("Dragon-Dragon").list_add(new List<int> { 17 })) // Fire Breath Heat
            };
            return null;
        }
    }
}
