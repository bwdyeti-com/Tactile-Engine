using System;
using System.Collections.Generic;

namespace Tactile
{
    public enum Reel_Classes
    {
        Drifter, Squire, Recruit, Skywatcher, Ruffian, Journeyman, Scout, Page, Ascetic, Pupil, Medic,
        Myrmidon, Thief, ThiefF, Archer, Crossbowman, Soldier, Dracoknight, Fighter, Brigand, Pirate, Mercenary,
        Nomad, Cohort, Vanguard, Gendarme, Phalanx, Lieutenant, Zweihander, PegasusKnight,
        Cleric, Monk, Mage, Troubadour, Witch, Sorcerer, Shaman, Diviner, Scholar, Corsair,
        Swordmaster, Longbowman, Halberdier, Warrior, Hero,
        NomadTrooper, Paladin, General, Falcoknight, Bishop, Sage, MageKnight, Valkyrie, Warlock, Justice,
        Uther, Tristan, Eliza
    }
    public enum Reel_Generic_Stats { Actor, Class, Listed }
    public class Class_Reel_Data
    {
        public const bool SKIP_LCK = true;

        // If true the chapter must be completed; if false it merely needs to be unlocked
        public readonly static Tuple<string, bool>[] ORDER = new Tuple<string, bool>[] {
            new Tuple<string, bool>("Tr3", false),
            new Tuple<string, bool>("", false),
        };
        public readonly static Dictionary<Tuple<string, bool>, Reel_Classes[]> CH_DATA = new Dictionary<Tuple<string, bool>, Reel_Classes[]> {
            { // Uther guys
                new Tuple<string, bool>("", false),
                new Reel_Classes[] { Reel_Classes.Uther, Reel_Classes.Recruit, Reel_Classes.Drifter, Reel_Classes.Squire,
                Reel_Classes.Medic, Reel_Classes.Paladin, Reel_Classes.Nomad, Reel_Classes.Scout }
            },
            { // Other lord guys
                new Tuple<string, bool>("Tr3", false),
                new Reel_Classes[] { Reel_Classes.Tristan, Reel_Classes.Zweihander, Reel_Classes.Journeyman, Reel_Classes.Ruffian,
                Reel_Classes.Eliza, Reel_Classes.Warrior, Reel_Classes.PegasusKnight, Reel_Classes.ThiefF }
            },
        };
        public readonly static Dictionary<Reel_Classes, Class_Reel_Data> DATA = new Dictionary<Reel_Classes, Class_Reel_Data> {
            #region Tier 0s
            { Reel_Classes.Drifter,
                new Class_Reel_Data("Harken", 1, 1, 1, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 1,
                    "A young swordsman with potential to\nreach any goal, but with none in mind.") },
            { Reel_Classes.Squire,
                new Class_Reel_Data("Isadora", 2, 1, 1, 1, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 1,
                    "A valorous adolescent chasing mythical\ndreams of knights and dragons.") },
            { Reel_Classes.Recruit,
                new Class_Reel_Data("Marcus", 3, 31, 1, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 1,
                    "A budding tactician and proven militiaman,\neager to be exalted into greatness.") },
            { Reel_Classes.Skywatcher,
                new Class_Reel_Data("Cybil", 4, 31, 1, 1, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 1,
                    "A daydreaming spearmaiden who aches\nto soar on the four winds.") },
            { Reel_Classes.Ruffian,
                new Class_Reel_Data("Gado", 5, 56, 1, 0, 2, new int[] { 10, 10 }, Reel_Generic_Stats.Class, null, 17,
                    "An inexperienced Axe raider with\npotential for awesome strength.") },
            { Reel_Classes.Journeyman,
                new Class_Reel_Data("Shen", 6, 56, 1, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 17,
                    "A simple youth with a simple skill -\nhitting things with Axes.") },
            { Reel_Classes.Scout,
                new Class_Reel_Data("Toni", 7, 81, 2, 1, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 1,
                    "A novice tracker and trapper who has\nsome notable ability with a bow.") },
            { Reel_Classes.Page,
                new Class_Reel_Data("Magnus", 8, 111, 1, 0, 1, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 1,
                    "Having finished his tuition, this aspiring\nmage travels in search of knowledge.") },
            { Reel_Classes.Ascetic,
                new Class_Reel_Data("Reno", 9, 131, 1, 0, 2, new int[] { 10, 20 }, Reel_Generic_Stats.Class, null, 5,
                    "A wandering missionary whose blind faith\nallows them to strike with magic.") },
            { Reel_Classes.Pupil,
                new Class_Reel_Data("Eiry", 10, 141, 1, 1, 1, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 1,
                    "This student of the arcane revives\nlong-forgotten arts from old tomes.") },
            { Reel_Classes.Medic,
                new Class_Reel_Data("Madelyn", 11, 167, 1, 1, 1, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 1,
                    "An aspiring healer with elementary\nknowledge of the restorative arts.") },
            #endregion
            #region Tier 1s
            { Reel_Classes.Myrmidon,
                new Class_Reel_Data("Sacae", 16, 1, 1, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 1,
                    "The very essence of swordsmanship, they are\ndedicated solely to honing their skills.") },
            { Reel_Classes.Thief,
                new Class_Reel_Data("Leonard", 17, 1, 2, 0, 2, new int[] { 10, 30 }, Reel_Generic_Stats.Class, null, 23,
                    "Specialists-for-hire, skilled at stealing\nsecrets, gold, and lives alike.") },
            { Reel_Classes.ThiefF,
                new Class_Reel_Data("Melanie", 17, 1, 1, 1, 2, new int[] { 0, 30 }, Reel_Generic_Stats.Class, null, 23,
                    "Specialists-for-hire, skilled at stealing\nsecrets, gold, and lives alike.") },
            { Reel_Classes.Archer,
                new Class_Reel_Data("Bern", 19, 81, 2, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 12,
                    "Unarmored soldiers who prefer to keep\ntheir foes an arrow's flight away.") },
            { Reel_Classes.Crossbowman,
                new Class_Reel_Data("Ilia", 20, 81, 2, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 10,
                    "Specially trained soldiers wielding\nexperimental mechanical bows.") },
            { Reel_Classes.Soldier,
                new Class_Reel_Data("Bennet", 22, 31, 1, 0, 2, new int[] { 15, 30 }, Reel_Generic_Stats.Class, null, 23,
                    "The fundamental heart of any armed force,\nvolunteer infantry armed with spears.") },
            { Reel_Classes.Dracoknight,
                new Class_Reel_Data("Vaida", 23, 31, 1, 0, 2, new int[] { 0, 30 }, Reel_Generic_Stats.Class, null, 18,
                    "Ancient predators of the sky bearing\nelite Lance-wielding troopers.") },
            { Reel_Classes.Fighter,
                new Class_Reel_Data("Laus", 24, 56, 1, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 17,
                    "Men of staggering might whose lives\nare dedicated to their axes.") },
            { Reel_Classes.Brigand,
                new Class_Reel_Data("Bandit", 25, 56, 1, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 37,
                    "Criminal gangs who roam the mountains,\nliving solely through robbery and murder.") },
            { Reel_Classes.Pirate,
                new Class_Reel_Data("Rebel", 26, 56, 1, 0, 2, new int[] { 10, 10 }, Reel_Generic_Stats.Class, null, 21,
                    "Violent thieves and murderers who control\nthe waves with their skillful Axe strokes.") },
            { Reel_Classes.Mercenary,
                new Class_Reel_Data("Saka", 27, 1, 1, 1, 2, new int[] { 30, 0 }, Reel_Generic_Stats.Class, null, 6,
                    "Aimless swordsmen, willing to attach their\nlives to whatever ideal pays the highest.") },
            { Reel_Classes.Nomad,
                new Class_Reel_Data("Hassar", 29, 81, 2, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 1,
                    "Plainsmen whose steed is like a brother,\nand whose arrows are like lightning.") },
            { Reel_Classes.Cohort,
                new Class_Reel_Data("Rebel", 30, 56, 1, 0, 2, new int[] { 10, 30 }, Reel_Generic_Stats.Class, null, 17,
                    "A mounted axeman who uses quick movement\nand powerful strikes to crush his enemies.") },
            { Reel_Classes.Vanguard,
                new Class_Reel_Data("Abelia", 31, 31, 1, 1, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 2,
                    "A valorous horseman of the code, trained\nin the arts of mobility and spearplay.") },
            { Reel_Classes.Gendarme,
                new Class_Reel_Data("Chester", 32, 1, 1, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 2,
                    "A brave swordfighter on horseback whose\ndeft equestrianism is unparalleled.") },
            { Reel_Classes.Phalanx,
                new Class_Reel_Data("Dom", 33, 56, 1, 1, 1, new int[] { 45, 0 }, Reel_Generic_Stats.Class, null, 10,
                    "With the strength of a bull, this stern\nknight carries his axe and armor with pride.") },
            { Reel_Classes.Lieutenant,
                new Class_Reel_Data("Wallace", 34, 31, 1, 0, 1, new int[] { 45, 0 }, Reel_Generic_Stats.Class, null, 23,
                    "Knights of extensive training, beacons of\nmorale for those they shelter.") },
            { Reel_Classes.Zweihander,
                new Class_Reel_Data("Celeste", 35, 1, 1, 1, 1, new int[] { 45, 0 }, Reel_Generic_Stats.Class, null, 6,
                    "A deadly opponent, as skilled as any\nfencer and as sturdy as any knight.") },
            { Reel_Classes.PegasusKnight,
                new Class_Reel_Data("Zephyr", 37, 31, 1, 1, 2, new int[] { 0, 45 }, Reel_Generic_Stats.Class, null, 38,
                    "Cold-blooded predators of the skies, spear-\nwielding amazons with no concept of mercy.") },
            { Reel_Classes.Cleric,
                new Class_Reel_Data("William", 38, 152, 1, 0, 1, new int[] { 0, 0 }, Reel_Generic_Stats.Class, null, 3,
                    "An ordained minister, granted the\nblessing of healing for his services.") },
            { Reel_Classes.Monk,
                new Class_Reel_Data("Etruria", 39, 131, 1, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 5,
                    "Solemn men whose worship is like mourning.\nThey strike with power from on high.") },
            { Reel_Classes.Mage,
                new Class_Reel_Data("Etruria", 40, 101, 1, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 5,
                    "Free-spirited spellcasters who traverse\nthe land in search of ever greater knowledge.") },
            { Reel_Classes.Troubadour,
                new Class_Reel_Data("Ostia", 41, 151, 1, 1, 1, new int[] { 0, 0 }, Reel_Generic_Stats.Class, null, 5,
                    "Brides of the wounded, the helpless and\nthe meek - artisans of healing and egress.") },
            { Reel_Classes.Witch,
                new Class_Reel_Data("Elaice", 43, 169, 1, 1, 1, new int[] { 0, 0 }, Reel_Generic_Stats.Class, null, 5,
                    "Folk of superstition and ritual, their\nreputation as \"healers\" is thoroughly debated.") },
            { Reel_Classes.Sorcerer,
                new Class_Reel_Data("Bandit", 44, 142, 1, 1, 1, new int[] { 45, 0 }, Reel_Generic_Stats.Class, null, 14,
                    "Cultists, heretics, professors, and generals,\nunited in their love of elder-crafts.") },
            { Reel_Classes.Shaman,
                new Class_Reel_Data("Sacae", 45, 112, 1, 0, 1, new int[] { 45, 0 }, Reel_Generic_Stats.Class, null, 12,
                    "Men of wisdom and spirituality, whose\nweapon is the very terrain they fight on.") },
            { Reel_Classes.Diviner,
                new Class_Reel_Data("Hyde", 47, 131, 1, 0, 1, new int[] { 60, 0 }, Reel_Generic_Stats.Class, null, 19,
                    "Men of the cloth who spend as much time\nhoning their bodies as they do their magics.") },
            { Reel_Classes.Scholar,
                new Class_Reel_Data("Jericho", 48, 141, 1, 1, 1, new int[] { 45, 0 }, Reel_Generic_Stats.Class, null, 14,
                    "A dangerous genius with withering morals,\nseduced by the potential of taboo magics.") },
            { Reel_Classes.Corsair,
                new Class_Reel_Data("Fargus", 49, 56, 1, 0, 2, new int[] { 10, 45 }, Reel_Generic_Stats.Class, null, 21,
                    "Eccentric thugs, possessed by their love of\nthe ocean, of violence, and of plunder.") },
            #endregion
            #region Tier 2s
            { Reel_Classes.Swordmaster,
                new Class_Reel_Data("Mundus", 51, 1, 1, 0, 2, new int[] { 15, 10 }, Reel_Generic_Stats.Class, null, 1,
                    "Ever hunting stronger opponents, the fabled\nSwordmasters crave the thrill of battle.") },
            { Reel_Classes.Longbowman,
                new Class_Reel_Data("Etruria", 55, 81, 1, 1, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 12,
                    "A Longbowman's best defense is that when\nher arrow reaches you, she is long gone.") },
            { Reel_Classes.Halberdier,
                new Class_Reel_Data("Uriel", 59, 31, 1, 0, 2, new int[] { 0, 15 }, Reel_Generic_Stats.Class, null, 23,
                    "With Lance in hand and loyal men at his\nback, he can lead any force to victory.") },
            { Reel_Classes.Warrior,
                new Class_Reel_Data("Brendan", 63, 81, 1, 0, 2, new int[] { 30, 50 }, Reel_Generic_Stats.Class, null, 10,
                    "Criminals and soldiers, vigilantes and farmers,\nall equally fearsome behind Axe and Bow.") },
            { Reel_Classes.Hero,
                new Class_Reel_Data("Laus", 65, 1, 1, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 8,
                    "This freelancer has seen all known forms\nof combat - and a few that are unknown.") },
            { Reel_Classes.NomadTrooper,
                new Class_Reel_Data("Sacae", 67, 81, 2, 2, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 12,
                    "The silent braves of Sacae strike from\nforests, hills, and plains with equal lethality.") },
            { Reel_Classes.Paladin,
                new Class_Reel_Data("Eagler", 70, 31, 1, 0, 1, new int[] { 30, 0 }, Reel_Generic_Stats.Class, null, 1,
                    "A cavalryman of the order, unremitting in\nhis vigilance and in his love for the people.") },
            { Reel_Classes.General,
                new Class_Reel_Data("Ostia", 73, 31, 1, 0, 1, new int[] { 45, 0 }, Reel_Generic_Stats.Class, null, 31,
                    "Proven in countless battles, this peerless\nleader is eager to join his troops in the fray.") },
            { Reel_Classes.Falcoknight,
                new Class_Reel_Data("Kirin", 76, 31, 1, 0, 2, new int[] { 0, 0 }, Reel_Generic_Stats.Class, null, 23,
                    "Ilian commanders of highest honor, blessed\nwith a thoroughbred and vast authority.") },
            { Reel_Classes.Bishop,
                new Class_Reel_Data("Elle", 78, 131, 1, 3, 2, new int[] { 45, 30 }, Reel_Generic_Stats.Class, null, 31,
                    "Ranking church officials and paragons of\nthe community, expert in healing and magic.") },
            { Reel_Classes.Sage,
                new Class_Reel_Data("Meredith", 79, 122, 1, 3, 2, new int[] { 0, 0 }, Reel_Generic_Stats.Class, null, 5,
                    "In the search for their passion, the Sage has\nbecome enraptured by magic of all forms.") },
            { Reel_Classes.MageKnight,
                new Class_Reel_Data("Bern", 80, 103, 1, 1, 1, new int[] { 0, 0 }, Reel_Generic_Stats.Class, null, 1,
                    "Vigilant riders whose mastery of\nthe arcane is to be feared.") },
            { Reel_Classes.Valkyrie,
                new Class_Reel_Data("Ostia", 81, 133, 1, 1, 1, new int[] { 45, 0 }, Reel_Generic_Stats.Class, null, 2,
                    "A Valkyrie is tasked with undoing the first\nstrike, and turning it instead unto the enemy.") },
            { Reel_Classes.Warlock,
                new Class_Reel_Data("Roeis", 84, 144, 1, 0, 1, new int[] { 45, 0 }, Reel_Generic_Stats.Class, null, 11,
                    "Gifted, delusional, ruthless, and always one\ntome away from understanding the universe.") },
            { Reel_Classes.Justice,
                new Class_Reel_Data("Richard", 88, 133, 1, 0, 1, new int[] { 45, 0 }, Reel_Generic_Stats.Class, null, 31,
                    "The fiercest of the faithful, granted blessed\nplates and jurisdiction just short of a king's.") },
            #endregion
            #region Lords
            { Reel_Classes.Uther,
                new Class_Reel_Data("Uther", 96, 1, 1, 0, 2, new int[] { 0, 10 }, Reel_Generic_Stats.Class, null, 10,
                    "Uther, heir to the Ostian throne.\nA swordfighter with an important mission.") },
            { Reel_Classes.Tristan,
                new Class_Reel_Data("Tristan", 98, 56, 1, 0, 2, new int[] { 10, 30 }, Reel_Generic_Stats.Class, null, 10,
                    "The Promised Son of the Clan Tristan.\nAn axefighter with an important mission.") },
            { Reel_Classes.Eliza,
                new Class_Reel_Data("Eliza", 100, 31, 1, 1, 2, new int[] { 30, 50 }, Reel_Generic_Stats.Class, null, 10,
                    "Eliza of Carrhae, a mercenary commander.\nA lancefighter with an important mission.") },
            #endregion
        };

        public string Name;
        public int Class_Id;
        public int Weapon_Id;
        public int Distance;
        public int Gender;
        public int Num_Attacks;
        public int[] Wait_Time;
        public Reel_Generic_Stats Stat_Type;
        public int[] Stats;
        public int Platform;
        public string Description;

        private Class_Reel_Data(string name, int class_id, int weapon_id, int distance, int gender,
            int num_attacks, int[] wait_time, Reel_Generic_Stats stat_type, int[] stats, int platform, string description)
        {
            Name = name;
            Class_Id = class_id;
            Weapon_Id = weapon_id;
            Distance = distance;
            Gender = gender;
            Num_Attacks = num_attacks;
            Wait_Time = wait_time;
            Stat_Type = stat_type;
            Stats = stats;
            Platform = platform;
            Description = description;
        }
    }
}
