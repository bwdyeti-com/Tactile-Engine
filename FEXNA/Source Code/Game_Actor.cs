using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNA.Calculations.LevelUp;
using FEXNA_Library;
using ListExtension;
using FEXNADictionaryExtension;
using FEXNAWeaponExtension;
using FEXNAVersionExtension;

namespace FEXNA
{
    public enum Power_Types { Strength, Magic, Power }
    enum Equippability { CanEquip, CannotEquip, CannotEquipPrf, CanEquipSiege, Silenced }
    internal partial class Game_Actor
    {
        internal const int LEVEL_UP_VIABLE_STATS = (int)Stat_Labels.Con; // Stats preceding this can go up on level
        readonly static int STATS = Enum_Values.GetEnumCount(typeof(Stat_Labels));

        private Data_Actor Data;
        private int Id;
        private string Name = "";
        private List<byte> Promotion_Choices = new List<byte>();
        private int Level;
        private int Exp;
        private List<int> Stats = new List<int>();
        private int Hp;
        private List<int> WLvl = new List<int>();
        private List<int> WLvl_Ups = new List<int>();
        private int Build = (int)Generic_Builds.Strong;
        private int Lives = -1;
        private List<Item_Data> Items = new List<Item_Data>();
        private int Weapon_Id = 0, Equipped = 0;
        private bool Needs_Promotion = false;
        private List<int> Growth_Bonuses = new List<int>();
        private List<int[]> States = new List<int[]>();
        private Dictionary<int, int> Support_Progress = new Dictionary<int, int>();
        private Dictionary<int, int> Supports = new Dictionary<int, int>();
        private int Bond = -1;

        private List<int[]> Temp_States;
        private int Needed_Levels = 0;
        private bool Instant_Level = false;
        private HashSet<string> Skills = new HashSet<string>();
        private bool Skills_Need_Updated = false;

        static Random rand = new Random();

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Name);
            //writer.Write(Description);
            if (is_generic_actor)
                writer.Write(class_id);
            Promotion_Choices.write(writer);
            writer.Write(Level);
            writer.Write(Exp);
            if (is_generic_actor)
            {
                writer.Write((int)Data.Affinity);
                writer.Write(Data.Gender);
                Data.BaseStats.write(writer);
            }
            Stats.write(writer);
            //writer.Write(MaxHp);
            writer.Write(Hp);
            WLvl.write(writer);
            WLvl_Ups.write(writer);
            //writer.Write(Gender);
            writer.Write(Build);
            writer.Write(Lives);
            Items.write(writer);
            //writer.Write(Con_Plus);
            //writer.Write(Mov_Plus);
            writer.Write(Weapon_Id);
            writer.Write(Equipped);
            writer.Write(Needs_Promotion);
            //writer.Write(Backfire);
            Growth_Bonuses.write(writer);
            States.write(writer);
            Support_Progress.write(writer);
            Supports.write(writer);
            writer.Write(Bond);
        }

        public void read(BinaryReader reader) // Make this static, maybe? //Yeti
        {
            Id = reader.ReadInt32();
            //if (Id >= Global.data_actors.Count)
            if (is_generic_actor)
            {
                Data = new Data_Actor();
                Data.Id = Id;
            }
            else
                Data = Global.data_actors[Id];
            Name = reader.ReadString();
            if (Global.LOADED_VERSION.older_than(0, 4, 3, 1))
            {
                string description = reader.ReadString();
            }
            if (!Global.LOADED_VERSION.older_than(0, 4, 3, 1))
            {
                if (is_generic_actor)
                    Data.ClassId = reader.ReadInt32();
                Promotion_Choices.read(reader);
            }
            else
            {
                int loaded_class = reader.ReadInt32();
                if (is_generic_actor)
                    Data.ClassId = loaded_class;
                if (loaded_class != this.class_id)
                {
                    if (!Global.data_classes.ContainsKey(loaded_class))
                        throw new IndexOutOfRangeException("Invalid class id: " + loaded_class);

                    //@Debug: no reason to care if promotions are higher tier than base classes, I guess
                    //int tier = Global.data_classes[loaded_class].Tier;
                    //if (tier > actor_class.Tier)
                    bool failed;
                    while (loaded_class != this.class_id)
                    {
                        failed = true;
                        List<int> promotions = Global.data_classes[this.class_id].promotion_keys.ToList();
                        for (byte i = 0; i < promotions.Count; i++)
                        {
                            if (promotes_to(promotions, loaded_class, 0))
                            {
                                Promotion_Choices.Add(i);
                                failed = false;
                                break;
                            }
                        }
                        if (failed)
                            break;
                    }
                }
            }
            Level = reader.ReadInt32();
            Exp = reader.ReadInt32();
            if (!Global.LOADED_VERSION.older_than(0, 4, 3, 1))
            {
                if (is_generic_actor)
                {
                    Data.Affinity = (Affinities)reader.ReadInt32();
                    Data.Gender = reader.ReadInt32();
                    Data.BaseStats.read(reader);
                }
                Stats.read(reader);
                Hp = reader.ReadInt32();
            }
            else
            {
                List<int> stats = new List<int>();
                stats.read(reader);
                if (is_generic_actor)
                {
                    for (int i = 0; i < stats.Count; i++)
                        Data.BaseStats[i + 1] = stats[i];
                    Data.BaseStats[(int)Stat_Labels.Hp] = reader.ReadInt32() - base_stat(Stat_Labels.Hp);
                    for (int i = 0; i < STATS; i++)
                        Stats.Add(0);
                }
                else
                {
                    for (int i = 0; i < stats.Count; i++)
                        stats[i] -= base_stat((Stat_Labels)i + 1);
                    int max_hp = reader.ReadInt32() - base_stat(Stat_Labels.Hp);
                    Stats.Add(max_hp);
                    Stats.AddRange(stats);
                    Stats.Add(0); // Mov
                }
                Hp = reader.ReadInt32();
            }
            // WLvl length needs corrected to the number of weapon types that exist on load
            WLvl.read(reader);
            WLvl = Enumerable.Range(0, Global.weapon_types.Count - 1)
                .Select(x => x < WLvl.Count ? WLvl[x] : 0)
                .ToList();
            WLvl_Ups.read(reader);
            if (Global.LOADED_VERSION.older_than(0, 4, 3, 1))
            {
                int gender = reader.ReadInt32();
                if (is_generic_actor)
                    Data.Gender = gender;
            }
            Build = reader.ReadInt32();
            Lives = reader.ReadInt32();
            Items.read(reader);
            if (Global.LOADED_VERSION.older_than(0, 4, 3, 1))
            {
                int con_plus = reader.ReadInt32();
                int mov_plus = reader.ReadInt32();
                Stats[(int)Stat_Labels.Con] = con_plus;
                Stats[(int)Stat_Labels.Mov] = mov_plus;
            }
            Weapon_Id = reader.ReadInt32();
            Equipped = reader.ReadInt32();
            Needs_Promotion = reader.ReadBoolean();
            if (Global.LOADED_VERSION.older_than(0, 4, 3, 1))
            {
                bool backfire = reader.ReadBoolean();
            }
            Growth_Bonuses.read(reader);
            States.read(reader);
            initialize_support_progress();
            Dictionary<int, int> support_progress = new Dictionary<int, int>();
            support_progress.read(reader);
            foreach (KeyValuePair<int, int> pair in support_progress)
                // If a saved support value isn't valid anymore, this will prevent copying it over
                if (Support_Progress.ContainsKey(pair.Key))
                    Support_Progress[pair.Key] = pair.Value;
            Supports.read(reader);
            Bond = reader.ReadInt32();

            skill_list_update();
        }
        #endregion

        #region Accessors
        public int id
        {
            get { return Id; }
        }

        public string name
        {
            get
            {
                return this.name_full.Split(Constants.Actor.ACTOR_NAME_DELIMITER)[0];
            }
            set
            {
                Name = value;
                Skills_Need_Updated = true;
            }
        }

        public string name_full
        {
            get { return string.IsNullOrEmpty(Name) ? Data.Name : Name; }
        }

        public string face_name
        {
            get
            {
                // If renamed
                if (Face_Sprite_Data.FACE_RENAME.ContainsKey(this.name_full))
                    return Face_Sprite_Data.FACE_RENAME[this.name_full];
                // Simply return the name if not generic
                if (!generic_face)
                    return this.name_full;

                // Generic face for this non-generic actor
                if (Face_Sprite_Data.FACE_TO_GENERIC_RENAME.ContainsKey(this.name_full))
                    return Face_Sprite_Data.FACE_TO_GENERIC_RENAME[this.name_full].GraphicName;

                // Generate generic name
                string name = class_name_full;
                if (Face_Sprite_Data.CLASS_RENAME.ContainsKey(name))
                    name = Face_Sprite_Data.CLASS_RENAME[name];
                return name + (gender % 2 == 0 ? "M" : "F") +
                    Constants.Actor.BUILD_NAME_DELIMITER + Build.ToString();
            }
        }

        public string flag_name
        {
            get
            {
                string country = this.name_full;
                if (Face_Sprite_Data.FACE_TO_GENERIC_RENAME.ContainsKey(this.name_full))
                    country = Face_Sprite_Data.FACE_TO_GENERIC_RENAME[this.name_full].FlagCountry;

                //else if (Face_Sprite_Data.FACE_COUNTRY_RENAME.ContainsKey(country) && //Debug
                if (Face_Sprite_Data.FACE_COUNTRY_RENAME.ContainsKey(country) &&
                        !Face_Sprite_Data.FACE_COUNTRY_RENAME[country].UseOwnFlag)
                    country = Face_Sprite_Data.FACE_COUNTRY_RENAME[country].RecolorCountry;
                return string.Format("{0} Flags", country);
            }
        }

        public bool generic_face
        {
            get
            {
                if (!Global.game_actors.is_temp_actor(this) &&
                        !Face_Sprite_Data.FACE_TO_GENERIC_RENAME.ContainsKey(this.name_full))
                    return false;
                if (Global.content_exists(@"Graphics/Faces/" + this.name_full))
                    return false;
                if (Face_Sprite_Data.FACE_RENAME.ContainsKey(this.name_full))
                    return false;

                return true;
            }
        }

        private Data_Generic_Actor generic_data
        {
            get
            {
                if (is_generic_actor && Global.generic_actors != null && Global.generic_actors.ContainsKey(Name))
                    return Global.generic_actors[Name];
                return null;
            }
        }

        public string description
        {
            get
            {
                var generic_data = this.generic_data;
                if (generic_data != null)
                    return generic_data.Description;
                if (Global.actor_descriptions.ContainsKey(this.name_full))
                    return Global.actor_descriptions[this.name_full];
                return "";
            }
        }

        public int class_id
        {
            get
            {
                int class_id = Data.ClassId;

                if (!Global.data_classes.ContainsKey(class_id))
                    throw new ArgumentException(string.Format(
                        "Actor {0} has invalid class id {1}",
                        this.name_full, class_id));

                foreach (byte promotion_choice in Promotion_Choices)
                {
                    if (promotion_choice < Global.data_classes[class_id].Promotion.Count)
                    {
                        int[] promotions = Global.data_classes[class_id].promotion_keys.ToArray();
                        class_id = promotions[promotion_choice];

                        if (!Global.data_classes.ContainsKey(class_id))
                            throw new ArgumentException(string.Format(
                                "Actor {0} has invalid class id {1}",
                                this.name_full, class_id));
                    }
                }
                return class_id;
            }
            set
            {
                Needs_Promotion = false;
                if (is_generic_actor)
                {
                    int old_class = class_id;
                    Skills_Need_Updated = true;
                    Data.ClassId = value;
                    Promotion_Choices.Clear();
                    // Remove weapon if new class cannot wield it
                    if (is_equipped)
                        if (!is_equippable(this.weapon))
                            unequip();
                    // Skill stuff here
                }
                /*if (Global.data_classes.ContainsKey(value))
                {
                    int old_class = ClassId;
                    ClassId = value;
                    // Remove weapon if new class cannot wield it
                    if (is_equipped)
                        if (!is_equippable(this.weapon))
                            unequip();
                    // Skill stuff here
                }*/
            }
        }

        public int promotion_class_id
        {
            set
            {
                //if (Needs_Promotion)
                //{
                    Needs_Promotion = false;
                    if (Global.data_classes.ContainsKey(value))
                    {
                        Window_Promotion.Promotion_Stats.Clear();
                        for (int i = 0; i < 8; i++)
                            Window_Promotion.Promotion_Stats.Add(stat(i));

                        int old_class = class_id;
                        int old_hp = maxhp;
                        List<int> promotions = actor_class.promotion_keys.ToList();
                        Promotion_Choices.Add((byte)promotions.IndexOf(value));
                        if (Constants.Actor.ACTOR_GAINED_HP_HEAL)
                            Hp = Math.Min(Hp + (maxhp - old_hp), maxhp);
                        else
                            Hp = Math.Min(Hp, maxhp);
                        // Remove weapon if new class cannot wield it
                        if (is_equipped)
                            if (!is_equippable(this.weapon))
                                unequip();
                        // Skill stuff here
                    }
                //}
            }
        }

        public int level
        {
            get { return Level; }
            set
            {
                Level = (int)MathHelper.Clamp(value, 1, level_cap());
                Skills_Need_Updated = true;
            }
        }
        public int full_level
        {
            get
            {
                return Constants.Actor.ActualLevel(this.tier, Level);
            }
        }

        public int exp
        {
            get { return Exp; }
            set
            {
                Exp = Math.Max(0, value);
                if (Exp >= Constants.Actor.EXP_TO_LVL)
                {
                    // Level up promotion needed
                    if (!can_level(false) && can_level())
                    {
                        Needs_Promotion = true;
                        Exp = Exp % Constants.Actor.EXP_TO_LVL;
                    }
                    else if (can_level(false))
                    {
                        while (Exp >= Constants.Actor.EXP_TO_LVL)
                        {
                            Needed_Levels++;
                            Exp -= Constants.Actor.EXP_TO_LVL;
                        }
                        if (Instant_Level)
                        {
                            // For average level ups
                            //int old_level = Level;
                            //level = Level + Needed_Levels;
                            //Needed_Levels = 0;
                            //fixed_lvl_up(Level - old_level);
                            level_up();
                        }
                    }
                    else
                        Exp = 0;
                }
            }
        }

        public int maxhp
        {
            get { return Math.Min(stat(Stat_Labels.Hp), get_cap(Stat_Labels.Hp)); }
        }

        public int hp
        {
            get { return Hp; }
            set
            {
                Hp = (int)MathHelper.Clamp(value, 0, maxhp);
            }
        }

        public int gender
        {
            get { return Data.Gender; }
            set
            {
                if (is_generic_actor)
                    Data.Gender = value;
            }
        }
        public int battle_gender
        {
            get
            {
                int gender = this.gender;
                if (FE_Battler_Image.Single_Gender_Battle_Sprite.Contains(class_id))
                    gender = (gender / 2) * 2;
                return gender;
            }
        }

        public int build { get { return Build; } }

        public int lives { get { return Lives; } }

        public Affinities affin
        {
            get { return Data.Affinity; }
        }

        public List<Item_Data> items { get { return Items.GetRange(0, Constants.Actor.NUM_ITEMS); } }
        public List<Item_Data> whole_inventory { get { return Items; } }

        public List<ClassTypes> class_types { get { return actor_class.Class_Types; } }

        public int con_plus { get { return Stats[(int)Stat_Labels.Con]; } }

        public int mov { get { return actor_class.Mov; } }

        public int mov_plus { get { return Stats[(int)Stat_Labels.Mov]; } }

        public int move_type { get { return (int)actor_class.Movement_Type; } }

        public int mov_cap { get { return (int)actor_class.Mov_Cap; } }

        /// <summary>
        /// The Id of the weapon currently being used by this actor.
        /// This can differ from the equipped weapon, for example during the arena or when using siege engines.
        /// If not using a weapon this returns 0.
        /// </summary>
        public int weapon_id
        {
            get { return Weapon_Id; }
            set
            {
                Weapon_Id = value;
                skill_list_update();
            }
        }
        /// <summary>
        /// The weapon currently being used by this actor.
        /// This can differ from the equipped weapon, for example during the arena or when using siege engines.
        /// </summary>
        public Data_Weapon weapon
        {
            get
            {
                if (!Global.data_weapons.ContainsKey(Weapon_Id))
                    return null;
                return Weapon_Id == 0 ? null : Global.data_weapons[Weapon_Id];
            }
        }

        /// <summary>
        /// The Id of the weapon currently being used by this actor, or the first equippable weapon if nothing can be equipped.
        /// For use when sorting actors by their weapons.
        /// </summary>
        public int sort_weapon_id
        {
            get
            {
                // If something is equipped, use that
                if (weapon_id != 0)
                    return weapon_id;
                // Otherwise check each item in the inventory, and if one can be equipped, return that
                for (int i = 0; i < Items.Count; i++)
                    if (is_equippable(i))
                        return Items[i].Id;
                // Otherwise check for any weapons in teh inventory at all
                for (int i = 0; i < Items.Count; i++)
                    if (Items[i].is_weapon)
                        return Items[i].Id;
                return 0;
            }
        }

        public int equipped { get { return Equipped; } }
        public bool is_equipped { get { return Equipped > 0; } }

        public List<int> states
        {
            get
            {
                List<int> result = new List<int>();
                foreach (int[] state in States)
                    result.Add(state[0]);
                return result;
            }
        }

        public List<int> negative_states
        {
            get
            {
                List<int> result = new List<int>();
                foreach (int[] state in States)
                    if (Global.data_statuses[state[0]].Negative)
                        result.Add(state[0]);
                return result;
            }
        }

        public Dictionary<int, int> supports { get { return Supports; } }

        public int bond { get { return Bond; } }

        public int needed_levels
        {
            get { return Needed_Levels; }
            set { Needed_Levels = value; }
        }

        public bool needs_promotion { get { return Needs_Promotion; } }

        public bool instant_level { set { Instant_Level = value; } }

        public bool is_generic_actor { get { return !Global.data_actors.ContainsKey(Id); } }
        #endregion

        public override string ToString()
        {
            return string.Format("{0}:{1}, {2}{3}, Level: {4}, Hp: {5}/{6}",
                Id, name_full, class_name, gender, Level, Hp, maxhp);
        }

        public Game_Actor() { }

        public Game_Actor(int id)
        {
            Data = new Data_Actor();
            Id = id;
            Data.Id = id;
            initialize();
        }
        public Game_Actor(Data_Actor data)
        {
            this.Data = data;
            Id = data.Id;
            initialize();
        }

        private void initialize()
        {
            //Name = Data.Name; //Debug
            //Description = Data.Description;
            //ClassId = Data.ClassId;
            this.level = Data.Level;
            for (int i = 0; i < STATS; i++)
                Stats.Add(0);
            Hp = maxhp;
            WLvl = Enumerable.Range(0, Global.weapon_types.Count - 1)
                .Select(x => x < Data.WLvl.Count ? Data.WLvl[x] : 0)
                .ToList();
            Items = new List<Item_Data>();
            for (int i = 0; i < Constants.Actor.NUM_ITEMS; i++)
            {
                if (Data.Items.Count > i)
                {
                    // make this implicit/its own constructor //Debug
                    var item = new Item_Data(
                        (Item_Data_Type)Data.Items[i][0],
                        Data.Items[i][1], Data.Items[i][2]);
                    // If default uses value
                    if (!item.non_equipment && item.Uses == 0)
                        item.repair_fully();
                    Items.Add(item);
                }
                else
                    Items.Add(new Item_Data());
            }
            setup_items(false);

            for (int i = 0; i < LEVEL_UP_VIABLE_STATS; i++)
                Growth_Bonuses.Add(0);
            initialize_support_progress();
            skill_list_update();
            reset_lives();
        }

        public void lose_a_life()
        {
             if (Lives > 0)
                Lives--;
        }

        public void reset_lives()
        {
            Lives = Global.game_system.Style == Mode_Styles.Casual ?
                Constants.Actor.CASUAL_MODE_LIVES : 1;
        }

        public void setup_generic(
            int class_id, int level, int exp, int prepromote_levels,
            Generic_Builds build, int con,
            bool fixed_level = false, int[] growths = null, int[] wexp = null)
        {
            Skills_Need_Updated = true;
            Data.ClassId = class_id;
            Build = (int)build;
            // Set base stats
            set_generic_bases(con);
            // Set growths
            set_generic_growths(level, exp, prepromote_levels, growths);
            // Gain levels
            int exp_gain = generic_exp(level, prepromote_levels) + exp;
            if (fixed_level)
            {
                fixed_lvl_up(exp_gain / Constants.Actor.EXP_TO_LVL);
                this.exp = exp_gain % Constants.Actor.EXP_TO_LVL;
            }
            else
            {
                int total_levels = (exp_gain / Constants.Actor.EXP_TO_LVL);
                exp_gain -= total_levels * Constants.Actor.EXP_TO_LVL;

                int random_levels = get_generic_random_level_count(total_levels);
                // The remaining levels are fixed
                int fixed_levels = total_levels - random_levels;

                fixed_lvl_up(fixed_levels);
                for (int i = 0; i < random_levels; i++)
                {
                    Instant_Level = true;
                    this.exp = Constants.Actor.EXP_TO_LVL;
                    level_down();
                }
                this.exp = exp_gain;
            }
            Instant_Level = false;
            this.level = level;
            // Reset HP
            Hp = maxhp;

            // Set WLvls
            if (wexp != null)
            {
                for (int i = 0; i < Global.weapon_types.Count - 1; i++)
                    if (i < wexp.Length)
                    {
                        if (max_weapon_level(Global.weapon_types[i + 1]) > 0)
                            wexp_set(Global.weapon_types[i + 1], wexp[i], false);
                    }
            }

            // Set WLvls to the minimum needed to use gear
            if (Constants.Actor.GENERIC_AUTO_WEXP)
            {
                foreach (var item_data in Items)
                {
                    var weapon = item_data.to_weapon;
                    if (weapon == null)
                        continue;
                    if (!is_equippable(weapon))
                    {
                        WeaponType type = valid_possible_weapon_type_of(weapon);
                        if (prf_check(weapon) && (int)weapon.Rank <= max_weapon_level(type))
                            //if (actor_class.is_equippable(weapon) && !is_equippable(weapon)) //Debug
                            wexp_set(type, weapon.Rank, true);
                    }
                }
            }
            clear_wlvl_up();
        }

        private void set_generic_bases(int con)
        {
            List<int>[] generic_stats = actor_class.Generic_Stats[Build];

            for (int i = 0; i <= (int)Stat_Labels.Con; i++)
            {
                if (i == (int)Stat_Labels.Con)
                    Data.BaseStats[i] = con == -1 ? generic_stats[0][i] : con;
                // Setting this to -1 automatically uses the class bases, and updates them if they change
                else
                    Data.BaseStats[i] = -1;
            }
            // Minimum hp of 1 // This should be handled elsewhere, in a way that makes everyone have at least 1 //Debug
            /*if ((Data.BaseStats[(int)Stat_Labels.Hp] == -1 && generic_stats[0][(int)Stat_Labels.Hp] <= 0) ||
                    Data.BaseStats[(int)Stat_Labels.Hp] == 0)
                Data.BaseStats[(int)Stat_Labels.Hp] = 1;*/
        }
        private void set_generic_growths(int level, int exp, int prepromote_levels, int[] growths)
        {
            List<int>[] generic_stats = actor_class.Generic_Stats[Build];

            // Preset growths
            if (growths != null && growths.Length >= Data.Growths.Count)
            {
                Data.Growths[0] = growths[0];
                Data.Growths[1] = growths[1];
                Data.Growths[2] = growths[2];
                Data.Growths[3] = growths[3];
                Data.Growths[4] = growths[4];
                Data.Growths[5] = growths[5];
                Data.Growths[6] = growths[6];
            }
            else
            {
                Data.Growths[0] = generic_stats[1][0];
                Data.Growths[1] = generic_stats[1][1];
                Data.Growths[2] = generic_stats[1][2];
                Data.Growths[3] = generic_stats[1][3];
                Data.Growths[4] = generic_stats[1][4];
                Data.Growths[5] = generic_stats[1][5];
                Data.Growths[6] = generic_stats[1][6];

                // Modifies growths based on generic data
                var generic_data = this.generic_data;
                if (generic_data != null)
                {
                    for (int i = 0; i < Data.Growths.Count; i++)
                        Data.Growths[i] += generic_data.Growths[i];
                }

                // If hard mode, growth rates get bonuses
                difficulty_bonuses(level, prepromote_levels, exp);

                // Set affinity randomly
                if (Constants.Actor.GENERIC_ACTOR_RANDOM_AFFINITIES)
                    Data.Affinity = (Affinities)(Global.game_system.get_rng() % (Enum_Values.GetEnumCount(typeof(Affinities)) - 1));
                // Raises/lowers growths based on affinity
                if (Constants.Support.AFFINITY_GROWTHS.ContainsKey(Data.Affinity))
                {
                    foreach (Stat_Labels stat in Constants.Support.AFFINITY_GROWTHS[affin][0])
                    {
                        Data.Growths[(int)stat] +=
                            Constants.Support.AFFINITY_GROWTH_MOD *
                            GetStatRatio((int)stat);
                    }
                    foreach (Stat_Labels stat in Constants.Support.AFFINITY_GROWTHS[affin][1])
                    {
                        Data.Growths[(int)stat] -=
                            Constants.Support.AFFINITY_GROWTH_MOD *
                            GetStatRatio((int)stat);
                    }
                }
            }
        }

        private void difficulty_bonuses(int level, int prepromote_levels, int exp)
        {
            float difficulty_bonus;
            switch (Global.game_system.Difficulty_Mode)
            {
                case Difficulty_Modes.Normal:
                    difficulty_bonus = 0;
                    break;
                case Difficulty_Modes.Hard:
                //case Difficulty_Modes.Lunatic: //Debug
                //    difficulty_bonus = 0.2f;
                default:
                    // 1.15x multiplier to growths, instead of +5% to each
                    difficulty_bonus = 0.1f;

                    // +15 for lunatic and +25 for L+??? //Yeti
                    // honestly both need to use a lower growth bonus and then give exp bonuses instead
                    // this makes the stats more variable, since flat growth bonuses boost every stat equally
                    // and also it keeps promoted units from getting insane bonuses compared to unpromoted
                    break;
            }

#if DEBUG
            //if (Global.scene.scene_type == "Scene_Map_Unit_Editor") //Debug
            //    difficulty_bonus = 0;
#endif

            var adjusted_growths = Enumerable.Range(0, (int)Stat_Labels.Con)
                .Select(x => Data.Growths[x] * GetStatValue(x));
#if DEBUG
            int total_growth_bonus = (int)(adjusted_growths.Sum() * difficulty_bonus);
            int total_difficulty_stat_bonus =
                ((generic_exp(level, prepromote_levels) + exp) / Constants.Actor.EXP_TO_LVL) *
                total_growth_bonus / 100;
#endif
            int total_bonus = (int)(adjusted_growths.Sum() * difficulty_bonus);
            int actual_bonus = adjusted_growths
                .Select(x => (int)(x * difficulty_bonus))
                .Sum();
            int remaining_bonus = total_bonus - actual_bonus;

            if (difficulty_bonus != 0)
                for (int i = 0; i < (int)Stat_Labels.Con; i++)
                    Data.Growths[i] += (int)(Data.Growths[i] * difficulty_bonus);
            while (remaining_bonus != 0)
            {
                foreach (int i in Enumerable.Range(0, (int)Stat_Labels.Con))
                    if (remaining_bonus != 0)
                    {
                        Data.Growths[i] += GetStatRatio(i);

                        if (remaining_bonus > 0)
                            remaining_bonus--;
                        else
                            remaining_bonus++;
                    }
            }
        }
        /* //Debug
        private void difficulty_bonuses(int level, int prepromote_levels, int exp)
        {
            int difficulty_bonus;
            switch (Global.game_system.Difficulty_Mode)
            {
                case Difficulty_Modes.Normal:
                    difficulty_bonus = 0;
                    break;
                case Difficulty_Modes.Hard:
                default:
                    difficulty_bonus = 5;
                    // +15 for lunatic and +25 for L+??? //Yeti
                    // honestly both need to use a lower growth bonus and then give exp bonuses instead
                    // this makes the stats more variable, since flat growth bonuses boost every stat equally
                    // and also it keeps promoted units from getting insane bonuses compared to unpromoted
                    break;
            }
#if DEBUG
            //if (Global.scene.scene_type == "Scene_Map_Unit_Editor") //Debug
            //    difficulty_bonus = 0;
#endif
#if DEBUG
            int total_growth_bonus = (int)(Stat_Labels.Con) * difficulty_bonus;
            int total_difficulty_stat_bonus =
                ((generic_exp(level, prepromote_levels) + exp) / Constants.Actor.EXP_TO_LVL) *
                total_growth_bonus / 100;
#endif
            if (difficulty_bonus != 0)
                for (int i = 0; i < (int)Stat_Labels.Con; i++)
                    if (i == (int)Stat_Labels.Hp)
                        Data.Growths[i] +=
                            (int)(difficulty_bonus / Constants.Actor.HP_VALUE);
                    else
                        Data.Growths[i] += difficulty_bonus;
        }*/

        private int generic_exp(int level, int prepromote_levels)
        {
            int levels = prepromote_levels + (level - 1);
            if (Constants.Actor.RESET_LEVEL_ON_PROMOTION)
                levels -= (this.tier - Constants.Actor.LOWEST_TIER);
            int result = levels * Constants.Actor.EXP_TO_LVL;
            return result;
        }

        private int get_generic_random_level_count(int levels)
        {
            float random_percent = Constants.Actor.GENERIC_RANDOM_LEVEL_PERCENT;
            int random_max = Constants.Actor.GENERIC_RANDOM_LEVEL_MAX;
#if DEBUG
            if (Global.scene.scene_type == "Scene_Map_Unit_Editor")
            {
                random_percent = 1f;
                random_max = 0;
            }
#endif

            int random_levels = (int)(levels * random_percent);
            // Clamp random range
            random_max = Math.Min(random_max, levels);
            random_levels = Math.Max(0, Math.Min(random_levels, random_max));

            return random_levels;
        }

        public int stat(int index)
        {
            return stat((Stat_Labels)index);
        }
        public int stat(Stat_Labels index)
        {
            return (int)MathHelper.Clamp(get_stat(index), 0, get_cap(index));
        }

        private int get_stat(Stat_Labels i)
        {
            // Con
            if (i == Stat_Labels.Con)
                return base_stat(i);
            // Mov
            else if (i == Stat_Labels.Mov) //Debug
                return mov + Stats[(int)i];

            return base_stat(i) + Stats[(int)i];
        }

        public int stat_total()
        {
            int total = 0;
            for (int i = (int)Stat_Labels.Hp; i < LEVEL_UP_VIABLE_STATS; i++)
                total += (int)(stat(i) * GetStatValue(i));
            return total;
        }

        public int rating()
        {
            int rating = 0;
            for (int i = (int)Stat_Labels.Hp + 1; i <= (int)Stat_Labels.Con; i++)
                rating += stat((Stat_Labels)i);
            rating += (tier * 7) / 2;
            return rating;
        }

        internal int gained_levels()
        {
            int baseLevel = this.full_base_level;
            int currentLevel = this.full_level;

            int gainedLevels = currentLevel - baseLevel;

            // If resetting to level 1 on promotion
            if (Constants.Actor.RESET_LEVEL_ON_PROMOTION)
            {
                int gainedTiers;
                if (this.is_generic_actor)
                    gainedTiers = this.tier - Constants.Actor.LOWEST_TIER;
                else
                {
                    //gainedTiers = Promotion_Choices.Count; //@Debug: ???
                    var baseClass = Global.data_classes[Data.ClassId];
                    gainedTiers = this.tier - baseClass.Tier;
                }

                gainedLevels -= gainedTiers;
            }

            return gainedLevels;
        }

        internal int full_base_level
        {
            get
            {
                if (is_generic_actor)
                {
                    return 1;
                }
                else
                {
                    var baseClass = Global.data_classes[Data.ClassId];
                    return Constants.Actor.LevelsBeforeTier(baseClass.Tier) + Data.Level;
                }
            }
        }

        internal float stat_avg_comparison(int index, int level_offset = 0)
        {
            return stat_avg_comparison((Stat_Labels)index, level_offset);
        }
        internal float stat_avg_comparison(Stat_Labels stat, int level_offset = 0)
        {
            int growth = 0;
            if ((int)stat < Data.Growths.Count)
            {
                growth = Data.Growths[(int)stat];
                // Removes the effects of affinity growth bonuses/maluses, so the player can see how the stats compare to a baseline unit
                if (is_generic_actor && Constants.Support.AFFINITY_GROWTHS.ContainsKey(this.affin))
                {
                    if (Constants.Support.AFFINITY_GROWTHS[this.affin][0].Contains(stat))
                        growth -= Constants.Support.AFFINITY_GROWTH_MOD;
                    if (Constants.Support.AFFINITY_GROWTHS[this.affin][1].Contains(stat))
                        growth += Constants.Support.AFFINITY_GROWTH_MOD;
                }
            }
            float average_stat = (growth * (gained_levels() + level_offset)) / 100f;
            return Stats[(int)stat] - average_stat;
        }
        
        internal float stat_quality(int index, int level_offset = 0)
        {
            return stat_quality((Stat_Labels)index, level_offset);
        }
        internal float stat_quality(Stat_Labels index, int level_offset = 0)
        {
#if DEBUG
            int avg = (int)Math.Round(stat_avg_comparison(index, level_offset));
            return avg / 10f;

            float quality = (float)(avg / Math.Sqrt(gained_levels() + level_offset));
            return quality;
#else
            return (int)Math.Round(stat_avg_comparison(index, level_offset)) / 10f;

            return (float)(stat_avg_comparison(index, level_offset) / Math.Sqrt(gained_levels() + level_offset));
#endif
        }

        internal Color stat_color(Stat_Labels stat_label)
        {
            return stat_color(stat_label, Constants.Actor.STAT_LABEL_COLORING);
        }
        internal Color stat_color(
            Stat_Labels stat_label,
            Constants.StatLabelColoring labelColoring,
            int levels = 0,
            float multiplier = 1f)
        {
            switch (labelColoring)
            {
                case Constants.StatLabelColoring.Averages:
                    // Stat labels are colored based on how far from the average the stat value is
                    float stat_quality = this.stat_quality(stat_label, levels) * multiplier;
                    // If capped, don't go below yellow
                    if (this.get_capped(stat_label))
                        stat_quality = Math.Max(0, stat_quality);
                    int r = 255 - (int)MathHelper.Clamp((stat_quality * 1.25f * 255), 0, 255);
                    int g = (int)MathHelper.Clamp(255 + (stat_quality * 255), 0, 255);
                    return new Color(r, g, 255);
                case Constants.StatLabelColoring.Growths:
                    // Stat labels are colored based on the unit's growth in that stat
                    if ((int)stat_label < this.Data.Growths.Count)
                    {
                        //@Debug: get_growths() includes bonuses from skills, permanent growth boosts, etc
                        //int growth = this.Data.Growths[(int)stat_label]; 
                        int growth = get_growths((int)stat_label);
                        growth = (int)(growth * GetStatValue((int)stat_label));
                        growth = (int)MathHelper.Clamp(
                            growth,
                            Constants.Actor.GROWTH_AVERAGE_COLOR_MIN,
                            Constants.Actor.GROWTH_AVERAGE_COLOR_MAX);
                        //Sets the function to be within the range provided.
                        if (growth == Constants.Actor.GROWTH_AVERAGE_COLOR_MED)
                        {
                            return Color.White;
                        } else if(growth < Constants.Actor.GROWTH_AVERAGE_COLOR_MED)
                        {
                            growth -= Constants.Actor.GROWTH_AVERAGE_COLOR_MIN;
                            int mid = Constants.Actor.GROWTH_AVERAGE_COLOR_MED - Constants.Actor.GROWTH_AVERAGE_COLOR_MIN;
                            float value = (growth * multiplier) / mid;
                            int green = (int)MathHelper.Clamp(
                                63 + 192 * value, 0, 255);
                            return new Color(255, green, 255);
                        } else
                        {
                            growth -= Constants.Actor.GROWTH_AVERAGE_COLOR_MED;
                            int max = Constants.Actor.GROWTH_AVERAGE_COLOR_MAX - Constants.Actor.GROWTH_AVERAGE_COLOR_MED;
                            float value = (growth * multiplier) / max;
                            int red = (int)MathHelper.Clamp(
                                255 - 192 * value, 0, 255);
                            return new Color(red, 255, 255);
                        }
                    }
                    return Color.White;
                case Constants.StatLabelColoring.None:
                default:
                    // Default color if no method is active
                    return Color.White;
            }
        }

        #region Class/Tier/Level
        public Data_Class actor_class { get { return Global.data_classes[class_id]; } }

        public string class_name { get { return actor_class.name; } }
        public string class_name_full { get { return actor_class.Name; } }
        public string class_name_short() // Should get short versions (Peg Knight, Nmd Trooper, etc)
        {
            return class_name;
        }

        public string map_sprite_name
        {
            get
            {
                return Game_Actors.get_map_sprite_name(class_name_full, class_id, gender);
            }
        }

        public int tier
        {
            get
            {
                return actor_class.Tier;
            }
        }

        public bool can_promote()
        {
            return actor_class.can_promote();
        }

        public bool promotion_level()
        {
            int level;

            if (Constants.Actor.RESET_LEVEL_ON_PROMOTION)
                level = Level;
            else
                level = this.full_level -
                    Constants.Actor.LevelsBeforeTier(this.tier);

            return level >= Constants.Actor.PromotionLevel(this.tier);
        }

        public int? promotes_to(bool confirm_possible = true)
        {
            if (confirm_possible && (!can_promote() || !promotion_level()))
                return null;
            int i = rand.Next(actor_class.Promotion.Count);
            int j = 0;
            foreach (int id in actor_class.promotion_keys)
            {
                if (j == i) return id;
                j++;
            }
            return null;
        }
        private static bool promotes_to(List<int> promotions, int promotion)
        {
            return promotes_to(promotions, promotion);
        }
        private static bool promotes_to(List<int> promotions, int promotion, int depth)
        {
            if (depth > 100)
            {
#if DEBUG
                throw new OverflowException("Protracted promotion chain detected");
#endif
                return false;
            }
            if (promotions.Contains(promotion))
                return true;
            else
                foreach (int class_id in promotions)
                    if (Global.data_classes[class_id].can_promote())
                        if (promotes_to(Global.data_classes[class_id].promotion_keys.ToList(), promotion, depth + 1))
                            return true;
            return false;
        }

        public bool can_level()
        {
            return can_level(true);
        }
        public bool can_level(bool promotion_check)
        {
            // This should be modified to work if exp gain is greater than Config.EXP_TO_LVL //Debug
            if (Config.LEVEL_UP_PROMOTION.Contains(tier) &&
                    (Needs_Promotion ? Exp < Constants.Actor.EXP_TO_LVL : true) &&
                    promotion_check && can_promote())
                return true;
            else
                return (Level + Needed_Levels < level_cap());
        }

        public bool would_promote(int exp_gain)
        {
            // This should be modified to work if exp gain is greater than Config.EXP_TO_LVL //Debug
            if (Exp + exp_gain >= Constants.Actor.EXP_TO_LVL)
                if (!can_level(false) && can_level())
                {
                    return true;
                }
            return false;
        }

        internal int level_cap()
        {
            return Constants.Actor.LevelCap(this.tier);
        }
        
        public int exp_loss_possible()
        {
            return (Level - 1) * Constants.Actor.EXP_TO_LVL + exp;
        }

        public int exp_gain_possible()
        {
            int levels = level_cap() - Level;

            // Check for level up promotion
            if (Config.LEVEL_UP_PROMOTION.Contains(tier))
                levels += 1;

            return levels * Constants.Actor.EXP_TO_LVL - exp;
        }

        private void confirm_needed_levels()
        {
            this.level = Level + Needed_Levels;
            Needed_Levels = 0;
        }

        public void level_up()
        {
            level_up(should_use_semifixed_level);
        }
        private void level_up(bool semiFixed)
        {
            int old_level = Level;
            confirm_needed_levels();
            LevelUpProcessor level_up = level_up_stats(Level - old_level, semiFixed);
            level_up.Apply();
            Instant_Level = false;

            if (!can_level())
                Exp = 0;
        }

        public void level_down()
        {
            this.level = 1;
            Needed_Levels = 0;
            Exp = 0;
        }

        public void promotion_reset_level()
        {
            int level = this.level;
            level_down();

            if (!Constants.Actor.RESET_LEVEL_ON_PROMOTION)
                this.level = level;
        }

        public LevelUpProcessor full_level_up()
        {
            return full_level_up(this.should_use_semifixed_level);
        }
        private LevelUpProcessor full_level_up(bool semiFixed)
        {
            int old_level = Level;
            confirm_needed_levels();
            return level_up_stats(Level - old_level, semiFixed);
        }

        private bool should_use_semifixed_level
        {
            get
            {
                if (Constants.Actor.SEMIFIXED_LEVELS_AT_PREPARATIONS)
                    if (Global.game_system.preparations)
                        return true;
#if DEBUG
                if (Global.scene is Scene_Title)
                    return true;
#endif
                return false;
            }
        }

        private LevelUpProcessor level_up_stats(int level = 1, bool semiFixed = false)
        {
            LevelUpProcessor levelUp;
            if (semiFixed)
                levelUp = new SemifixedLevelUp(this, level);
            else
                levelUp = new StandardLevelUp(this, level);

            return levelUp;
        }

        private void fixed_lvl_up(int level)
        {
            LevelUpProcessor levelUp = new FixedLevelUp(this, level);
            levelUp.Apply();
            Instant_Level = false;
        }

        public int get_growths(int i)
        {
            return Data.Growths[i] + Growth_Bonuses[i] + growth_bonus_skill((Stat_Labels)i);
        }

        private int base_stat(Stat_Labels i)
        {
            int stat = Data.BaseStats[(int)i];
            if (is_generic_actor && stat == -1 && i <= Stat_Labels.Con)
                stat = actor_class.Generic_Stats[Build][0][(int)i];
            if (i != Stat_Labels.Lck)
            {
                int class_id = Data.ClassId, promotion_id;
                foreach (int promotion_choice in Promotion_Choices)
                {
                    if (!Global.data_classes[class_id].can_promote())
                        break;
                    promotion_id = Global.data_classes[class_id].promotion_keys.ToArray()[promotion_choice];
                    stat += Global.data_classes[class_id].promotion_bonuses(promotion_id)[(int)i];
                    //stat += Global.data_classes[class_id].Promotion[promotion_id][0][i >= Stat_Labels.Lck ? (int)i - 1 : (int)i]; //Debug
                    class_id = promotion_id;
                }
            }
            // Modifies bases based on generic data
            if (i <= Stat_Labels.Con)
            {
                var generic_data = this.generic_data;
                if (generic_data != null)
                {
                    stat += generic_data.BaseStats[(int)i];
                }
            }
            return stat;
        }

        public int get_cap(int i)
        {
            return get_cap((Stat_Labels)i);
        }
        public int get_cap(Stat_Labels i)
        {
            switch (i)
            {
                case Stat_Labels.Lck:
                    return Constants.Actor.LUCK_CAP;
                case Stat_Labels.Mov:
                    return this.mov_cap;
                default:
                    if (i < Stat_Labels.Lck)
                        return actor_class.caps(gender % 2)[(int)i];
                    else
                        return actor_class.caps(gender % 2)[(int)i - 1];
            }
        }

        public int cap_base_difference(Stat_Labels i)
        {
            return get_cap(i) - base_stat(i);
        }

        public bool get_capped(int i)
        {
            return get_capped((Stat_Labels)i, 0);
        }
        public bool get_capped(Stat_Labels i)
        {
            return get_capped(i, 0);
        }
        public bool get_capped(int i, int added)
        {
            return get_capped((Stat_Labels)i, added);
        }
        public bool get_capped(Stat_Labels i, int added)
        {
            return (get_stat(i) + added) >= get_cap(i);
        }

        internal static int GetStatRatio(int i)
        {
            float result = (1 / GetStatValue(i));
            return (int)result;
        }
        internal static float GetStatValue(int i)
        {
            switch (i)
            {
                case (int)Stat_Labels.Hp:
                    return Constants.Actor.HP_VALUE;
                default:
                    return 1.0f;
            }
        }

        public void gain_stat(int i, int stat_gain)
        {
            gain_stat((Stat_Labels)i, stat_gain);
        }
        public void gain_stat(Stat_Labels i, int stat_gain)
        {
            if (i == Stat_Labels.Mov)
            {
                int base_value = this.mov;
                Stats[(int)i] = (int)MathHelper.Clamp(Stats[(int)i] + stat_gain,
                    -base_value, get_cap(i) - base_value);

                //Stats[(int)i] += stat_gain;
            }
            else
            {
                int base_value = base_stat(i);
                // Don't go below 1 hp
                if (i == Stat_Labels.Hp)
                    base_value -= 1;
                int old_hp = this.maxhp;
                Stats[(int)i] = (int)MathHelper.Clamp(Stats[(int)i] + stat_gain,
                    -base_value, get_cap(i) - base_value);
                if (i == Stat_Labels.Hp)
                {
                    if (Constants.Actor.ACTOR_GAINED_HP_HEAL)
                        Hp = Math.Min(Hp + (this.maxhp - old_hp), this.maxhp);
                    else
                        Hp = Math.Min(Hp, this.maxhp);
                    Hp = Math.Max(1, Hp);
                }
            }
        }

        internal void transfer_blessing(Game_Actor sourceActor)
        {
            for(Stat_Labels i = 0; i <= Stat_Labels.Con; i++)
            {
                int blessing = (int)Math.Round(sourceActor.stat_avg_comparison(i));
                if (blessing > 0)
                    gain_stat(i, blessing);
            }
        }
        #endregion

        #region Promotion
        public PromotionProcessor promotion(int old_class)
        {
            PromotionProcessor promotion_gains = new PromotionProcessor(this, old_class);
            int[] old_weapon_levels = new int[WLvl.Count];
            for (int i = 1; i < Global.weapon_types.Count; i++)
                old_weapon_levels[i - 1] = weapon_levels(Global.weapon_types[i]);
            promotion_weapon_types(old_class, old_weapon_levels);
            return promotion_gains;
        }

        public void quick_promotion(int new_class)
        {
            if (can_promote())
            {
                if (actor_class.can_promote(new_class))
                {
                    List<int> promotions = actor_class.promotion_keys.ToList();

                    promotion_reset_level();
                    int old_class = class_id;
                    int old_hp = this.maxhp;
                    //class_id = new_class;
                    //for(int i = 0; i < 8; i++)
                    //    gain_stat(i, stat_gains[i]);
                    int[] old_weapon_levels = new int[WLvl.Count];
                    for (int i = 1; i < Global.weapon_types.Count; i++)
                        old_weapon_levels[i - 1] = weapon_levels(Global.weapon_types[i]);

                    Promotion_Choices.Add((byte)promotions.IndexOf(new_class));

                    if (Constants.Actor.ACTOR_GAINED_HP_HEAL)
                        Hp = Math.Min(Hp + (maxhp - old_hp), maxhp);
                    else
                        Hp = Math.Min(Hp, maxhp);
                    // Get new weapon types
                    promotion_weapon_types(old_class, old_weapon_levels);
                    clear_wlvl_up();
                }
                else
                    Print.message(this.name_full + ", " + Id.ToString() + " invalid promotion choice");
            }
            else
                Print.message(this.name_full + ", " + Id.ToString() + " shouldn't be promoting");
        }

        public static List<int> promotion_bonuses(int old_class, int new_class)
        {
            if (!Global.data_classes.ContainsKey(old_class) || !Global.data_classes.ContainsKey(new_class) ||
                    !Global.data_classes[old_class].can_promote(new_class))
                throw new IndexOutOfRangeException();

            return new List<int>(Global.data_classes[old_class].promotion_bonuses(new_class));

            /*List<int> result = new List<int>();
            if (Global.data_classes.ContainsKey(old_class)) //Debug
                if (Global.data_classes[old_class].can_promote(new_class))
                    foreach (int i in Global.data_classes[old_class].Promotion[new_class][0])
                        result.Add(i);
            result.Insert((int)Stat_Labels.Lck, 0); // This manually adds a Lck gain of 0 to promotion bonuses, maybe it shouldn't be hardcoded? //Debug
            return result;*/
        }

        private void promotion_weapon_types(int old_class, int[] old_weapon_levels)
        {
            List<int> result = new List<int>();
            if (Global.data_classes.ContainsKey(old_class))
                if (Global.data_classes[old_class].can_promote(class_id))
                    foreach (int i in Global.data_classes[old_class].promotion_wlvl_bonuses(class_id))
                        result.Add(i);
            if (result.Count > 0)
            {
                for (int i = 1; i < Global.weapon_types.Count; i++)
                {
                    if (result.Count < i)
                    {
                        continue;
                    }
                    if (result[i - 1] != 0)
                    {
                        var weapon_type = Global.weapon_types[i];
                        int old_val = old_weapon_levels[i - 1];
                        // Gain wexp
                        if (!Config.SINGLE_ANIMA_PROMOTION || !FEXNA_Library.Data_Weapon.ANIMA_TYPES.Contains(i))
                        {
                            wexp_gain(weapon_type, result[i - 1], true);
                        }
                        // Unless it's anima, then only if you would get all kinds or don't have anima yet at all
                        else
                        {
                            bool has_anima = false;
                            bool all_anima = true;
                            foreach (int anima in FEXNA_Library.Data_Weapon.ANIMA_TYPES)
                            {
                                if (old_weapon_levels[i - 1] > 0)
                                    has_anima = true;
                                else if (!has_rank(weapon_type))
                                    all_anima = false;
                            }
                            if (all_anima || !has_anima)
                                wexp_gain(weapon_type, result[i - 1], true);
                        }

                        // If the type is newly gained, have the weapon level up popup appear
                        if (old_val == 0 && has_rank(weapon_type))
                            WLvl_Ups.Add(i);
                    }
                }
            }
        }
        /*private void promotion_weapon_types(int old_class)
        {
            int new_class = class_id;
            List<int> result = new List<int>();
            if (Global.data_classes.ContainsKey(old_class))
                if (Global.data_classes[old_class].can_promote(class_id))
                    foreach (int i in Global.data_classes[old_class].promotion_wlvl_bonuses(class_id))
                        result.Add(i);
            if (result.Count > 0)
            {
                for (int i = 1; i <= Data_Weapon.WEAPON_TYPES; i++)
                {
                    // Gain wexp
                    if (Config.SINGLE_ANIMA_PROMOTION && !FEXNA_Library.Data_Weapon.ANIMA_TYPES.Contains(i))
                    {
                        class_id = old_class;
                        int old_val = weapon_levels(i);
                        class_id = new_class;

                        wexp_gain(i, result[i - 1], true);
                        // If the type is newly gained, have the weapon level up popup appear
                        if (old_val == 0 && weapon_levels(i) > 0) WLvl_Ups.Add(i);
                    }
                    // Unless it's anima, then only if you would get all kinds or don't have anima yet at all
                    else
                    {
                        bool has_anima = false;
                        bool all_anima = true;
                        foreach (int anima in FEXNA_Library.Data_Weapon.ANIMA_TYPES)
                        {
                            if (weapon_levels(i) > 0)
                                has_anima = true;
                            else
                                all_anima = false;
                        }
                        if (all_anima || !has_anima)
                            wexp_gain(i, result[i - 1], true);
                    }
                }
            }
        }*/
        #endregion

        public int move_speed
        {
            get
            {
                // Armor
                if (actor_class.Movement_Type == FEXNA_Library.MovementTypes.Armor &&
                        !this.class_types.Contains(ClassTypes.Cavalry) && !this.class_types.Contains(ClassTypes.FDragon))
                    return 1;
                // Magus
                else if (this.class_types.Contains(ClassTypes.Mage) &&
                        this.class_types.Contains(ClassTypes.Heavy))
                    return 1;
                // Everyone else
                return 2;
            }
        }

        public Power_Types power_type()
        {
            bool physical = false, magical = false;
            var useable_types = Global.weapon_types
                .Select(x => can_use_weapon_type(x))
                .ToList();
            // If all weapon ranks are 0
            if (!useable_types.Any(x => x))
                // Check if the class can use the type at all
                useable_types = Global.weapon_types
                    .Select(x => max_weapon_level(x, true) > 0)
                    .ToList();

            for (int i = 1; i < Global.weapon_types.Count; i++)
            {
                WeaponType type = Global.weapon_types[i];

                if (!physical)
                    if (!type.IsMagic && !type.IsStaff)
                        if (useable_types[i])
                            physical = true;

                if (!magical)
                    if (type.IsMagic || type.IsStaff)
                        if (useable_types[i])
                            magical = true;
            }

            if (physical && magical)
                return Power_Types.Power;
            else if (!physical && magical)
                return Power_Types.Magic;
            else if (physical && !magical)
                return Power_Types.Strength;
            return Power_Types.Strength;
        }

        #region Skills
        public bool has_skill(string name)
        {
            if (Skills_Need_Updated)
                skill_list_update();
            Skills_Need_Updated = false;

            if (Skills.Contains(name))
                return true;
            /*foreach (int skill_id in actor_skills()) //Debug
                if (Global.data_skills[skill_id].Abstract == name)
                    return true;
            foreach (int skill_id in actor_class.Skills)
                if (Global.data_skills[skill_id].Abstract == name)
                    return true;
            if (weapon_id > 0)
                foreach (int skill_id in this.weapon.Skills)
                    if (Global.data_skills[skill_id].Abstract == name)
                        return true;*/
            foreach (Item_Data item_data in Items)
                if (item_data.Id > 0 && item_data.is_item)
                {
                    Data_Item item = item_data.to_item;
                    if (is_useable(item))
                        foreach (int skill_id in item.Skills)
                            if (Global.data_skills[skill_id].Abstract == name)
                                return true;
                }
            /*foreach(int status_id in states) //Debug
                    foreach (int skill_id in Global.data_statuses[status_id].Skills)
                        if (Global.data_skills[skill_id].Abstract == name)
                            return true;*/
            return false;
        }

        public void skill_list_update()
        {
            Skills.Clear();

            // all_skills, without items
            List<int> skills = new List<int>();

            // Actor skills
            foreach (int skill_id in actor_skills())
                skills.Add(skill_id);
            // Class skills
            foreach (int skill_id in class_skills())
                skills.Add(skill_id);
            // Weapon skills
            if (this.weapon != null)
                foreach (int skill_id in this.weapon.Skills)
                    skills.Add(skill_id);
            // Status skills
            foreach (int status_id in states)
                foreach (int skill_id in Global.data_statuses[status_id].Skills)
                    skills.Add(skill_id);

            foreach (int id in skills)
                Skills.Add(Global.data_skills[id].Abstract);
        }

        private IEnumerable<int> actor_skills()
        {
            // Actor skills
            foreach (int skill_id in Data.Skills)
                yield return skill_id;
            // Generic Actor skills
            var generic_data = this.generic_data;
            if (generic_data != null)
                foreach (int skill_id in generic_data.Skills)
                    yield return skill_id;
        }

        private IEnumerable<int> class_skills()
        {
            return actor_class.Skills.Where(x => x.Level <= Level).Select(x => x.SkillId);
        }

        internal IEnumerable<int> skills_gained_on_level()
        {
            return actor_class.Skills.Where(x => x.Level == Level + Needed_Levels).Select(x => x.SkillId);
        }
        internal IEnumerable<int> skills_gained_on_promotion(int old_class_id, int old_level)
        {
            var new_skills = actor_class.Skills.Where(x => x.Level <= Level).Select(x => x.SkillId);
            var old_skills = Global.data_classes[old_class_id].Skills.Where(x => x.Level <= old_level).Select(x => x.SkillId);
            return new_skills.Except(old_skills);
        }

        public List<int> skills
        {
            get
            {
                List<int> skills = new List<int>();
                // Actor skills
                foreach (int skill_id in actor_skills())
                    if (!skills.Contains(skill_id))
                        skills.Add(skill_id);
                // Class skills
                foreach (int skill_id in class_skills())
                    if (!skills.Contains(skill_id))
                        skills.Add(skill_id);
                return skills;
            }
        }
        public List<int> item_skills
        {
            get
            {
                List<int> skills = new List<int>();
                // Weapon skills
                if (this.weapon != null)
                    foreach (int skill_id in this.weapon.Skills)
                        if (!skills.Contains(skill_id))
                            skills.Add(skill_id);
                // Item skills
                foreach (Item_Data item_data in Items)
                    if (item_data.Id > 0 && item_data.is_item)
                    {
                        Data_Item item = item_data.to_item;
                        if (is_useable(item))
                            foreach (int skill_id in item.Skills)
                                if (!skills.Contains(skill_id))
                                    skills.Add(skill_id);
                    }
                return skills;
            }
        }

        public List<int> all_skills
        {
            get
            {
                List<int> skills = new List<int>();
                // Actor skills
                foreach (int skill_id in actor_skills())
                    skills.Add(skill_id);
                // Class skills
                foreach (int skill_id in class_skills())
                    skills.Add(skill_id);

                // Weapon skills
                if (this.weapon != null)
                    foreach (int skill_id in this.weapon.Skills)
                        skills.Add(skill_id);
                // Item skills
                /* //Debug
                foreach (Item_Data item_data in Items)
                    if (item_data.Id > 0 && item_data.is_item && is_useable(item_data.to_item))
                        foreach (int skill_id in item_data.to_equipment.Skills)
                            skills.Add(skill_id);*/
                foreach (Item_Data item_data in Items)
                    if (item_data.Id > 0 && item_data.is_item)
                    {
                        Data_Item item = item_data.to_item;
                        if (is_useable(item))
                            foreach (int skill_id in item.Skills)
                                skills.Add(skill_id);
                    }
                // Status skills
                foreach (int status_id in states)
                    foreach (int skill_id in Global.data_statuses[status_id].Skills)
                        skills.Add(skill_id);
                return skills;
            }
        }

        public int buy_price_mod()
        {
            // Skills: Bargain
            if (has_skill("BARGAIN"))
                return 2;
            return 1;
        }
        #endregion

        #region Weapon Levels
        public int weapon_levels(WeaponType type)
        {
            if (type.Key <= 0 || type.Key > WLvl.Count)
                return 0;
            int wlvl = min_weapon_exp_skill(type, WLvl[type.Key - 1]);
            return Math.Min(wlvl, weapon_level_cap(type));
        }

        public int weapon_level_cap(WeaponType type)
        {
            return Data_Weapon.WLVL_THRESHOLDS[max_weapon_level(type)];
        }

        public float weapon_level_percent(WeaponType type)
        {
            if (!has_rank(type))
                return 0;
            int wlvl = weapon_levels(type);
            int i = Data_Weapon.WLVL_THRESHOLDS.Length - 1;
            while (i >= 0)
            {
                if (Data_Weapon.WLVL_THRESHOLDS[i] <= wlvl)
                    break;
                i--;
            }
            wlvl -= Data_Weapon.WLVL_THRESHOLDS[i];
            if (wlvl == 0)
                return 0;
            return wlvl / (float)(Data_Weapon.WLVL_THRESHOLDS[i + 1] - Data_Weapon.WLVL_THRESHOLDS[i]);
        }

        public string weapon_level_letter(WeaponType type)
        {
            if (type == Global.weapon_types[0])
                return Data_Weapon.WLVL_LETTERS[0];
            return Data_Weapon.WLVL_LETTERS[get_weapon_level(type)];
        }

        public int max_weapon_level(WeaponType type, bool absolute = false)
        {
            if (type.Key <= 0)
                return 0;
            int rank = actor_class.Max_WLvl[type.Key - 1];
            rank = max_weapon_level_skill(type, rank);
            if (!absolute && Constants.Actor.ONE_S_RANK &&
                rank >= Data_Weapon.WLVL_THRESHOLDS.Length - 1)
            {
                int max_wlvl = Data_Weapon.WLVL_THRESHOLDS[Data_Weapon.WLVL_THRESHOLDS.Length - 1];
                // Stop if this type's rank is already at the max level
                if (Data_Weapon.WLVL_THRESHOLDS[rank] < max_wlvl)
                {
                    for (int i = 1; i < Global.weapon_types.Count; i++)
                    {
                        // If a different type is already at max rank
                        if (i != type.Key && WLvl[i - 1] >= max_wlvl)
                        {
                            // Min with one less than max rank
                            rank = Math.Min(rank, Data_Weapon.WLVL_THRESHOLDS.Length - 2);
                            break;
                        }
                    }
                }
            }
            return max_weapon_level_override(type, rank);
        }

        public List<int> weapon_types()
        {
            List<int> types = new List<int>();
            for (int i = 1; i < Global.weapon_types.Count; i++)
                if (max_weapon_level(Global.weapon_types[i], true) > 0)
                    types.Add(i);
            return types;
        }

        public string used_weapon_type()
        {
            // This does need to handle unique anims somehow, though //Yeti
            string weapon_type = Global.weapon_types[0].AnimName;
            if (this.weapon == null)
            {
                // If no weapon equipped, and an equippable staff in inventory
                if (can_staff())
                    foreach (Item_Data item_data in Items)
                        if (item_data.is_weapon && item_data.to_weapon.is_staff() &&
                            is_equippable(item_data.to_weapon))
                        {
                            weapon_type = item_data.to_weapon.main_type().AnimName;
                            break;
                        }
            }
            else if (this.weapon.Ballista())
                weapon_type = "";
            //else if (this.weapon.Unique()) // Need to add this tag to weapons, or come up with a more useful system //Yeti
            //    weapon_type = (int)Anim_Types.Unique;
            else
            {
                weapon_type = this.weapon.main_type().AnimName;
                if (this.weapon.Thrown() && !string.IsNullOrEmpty(this.weapon.main_type().ThrownAnimName))
                    weapon_type = this.weapon.main_type().ThrownAnimName;
            }
            return weapon_type;
        }

        public int get_weapon_level(WeaponType type)
        {
            int wlvl = weapon_levels(type);
            for (int i = Data_Weapon.WLVL_THRESHOLDS.Length - 1; i >= 0; i--)
            {
                if (wlvl >= Data_Weapon.WLVL_THRESHOLDS[i])
                    return i;
            }
            return 0;
        }

        public int wexp_from_weapon(Data_Weapon weapon)
        {
            int wexp = weapon.WExp;
            wexp_from_weapon_skill(weapon, ref wexp);
            return wexp;
        }

        public void wexp_gain(WeaponType type, int wexp)
        {
            wexp_gain(type, wexp, false);
        }
        public void wexp_gain(WeaponType type, int wexp_gain, bool promoting)
        {
            int old_val = weapon_levels(type);
            wexp_set(type, old_val + wexp_gain, promoting);
        }

        public void wexp_set(WeaponType type, Weapon_Ranks rank, bool promoting)
        {
            wexp_set(type, Data_Weapon.WLVL_THRESHOLDS[(int)rank], promoting);
        }
        public void wexp_set(WeaponType type, int wexp, bool promoting)
        {
            int old_val = get_weapon_level(type);
            //int val = Math.Max(1, Math.Min(wexp, weapon_level_cap((Weapon_Types)type))); //Debug
            int val = Math.Max(0, wexp);
            WLvl[type.Key - 1] = val;
            val = get_weapon_level(type);
            bool level_up = val > old_val;
            if (!promoting)
            {
                if (level_up)
                    WLvl_Ups.Add(type.Key);
            }
#if DEBUG
            if (old_val == 0 && weapon_levels(type) == 0 && wexp > 0)
            {
                if (!Global.scene.is_test_battle)
                    Print.message(string.Format(
                        "{0} gained wexp they can't use, type {1}",
                        this.name_full, type));
            }
#endif
        }

        public int wlvl_up_type()
        {
            if (WLvl_Ups.Count > 0)
                return WLvl_Ups[0];
            return 0;
        }

        public void clear_wlvl_up()
        {
            WLvl_Ups.Clear();
        }

        public bool wlvl_up()
        {
            return WLvl_Ups.Count > 0;
        }

        public bool is_weapon_level_capped(WeaponType type)
        {
            return get_weapon_level(type) >= max_weapon_level(type);
        }

        public int s_rank_bonus(Data_Weapon weapon)
        {
            return s_rank_bonus(valid_weapon_type_of(weapon));
        }
        private int s_rank_bonus(WeaponType type)
        {
            return (get_weapon_level(type) >= Data_Weapon.WLVL_THRESHOLDS.Length - 1) ? s_bonus_amount() : 0;
        }

        private int s_bonus_amount()
        {
            return Constants.Actor.S_RANK_BONUS;
        }
        #endregion

        #region Item Stuff
        /// <summary>
        /// Equips the first viable weapon found in the inventory
        /// </summary>
        /// <param name="organize">Move equipped item to the front of the item list?</param>
        public void setup_items(bool organize = true)
        {
            // Checks items in reverse order and equips the last one that can be
            // why in reverse order, did i not know about break; at the time? //Yeti
            // Yeah but now it does something meaningful so don't worry about it //Debug
            int equipped = 0;
            for (int i = Constants.Actor.NUM_ITEMS - 1; i >= 0; i--)
            {
                // If -1 uses, assume the item is at default uses and set it to the max
                if (Items[i].Uses == -1)
                    Items[i].Uses = Items[i].Id == 0 ? 0 : Items[i].max_uses;
                // Look for a weapon to equip
                if (!Items[i].blank_item && Items[i].is_weapon)
                    if (is_equippable(Items[i].to_weapon))
                        equipped = i + 1;
            }
            // Equips the indicated item
            if (equipped != 0)
            {
                equip(equipped);
                if (organize)
                    organize_items();
            }
            else
                unequip();
            skill_list_update();
        }

        /// <summary>
        /// Moves equipped weapon to the front of the inventory
        /// </summary>
        public void organize_items()
        {
            if (Equipped != 0)
            {
                List<Item_Data> not_equipped = new List<Item_Data>();
                for (int i = 0; i < Items.Count; i++)
                {
                    if (i != Equipped - 1)
                        if (Items[i].Uses != 0)
                            not_equipped.Add(Items[i]);
                }
                List<Item_Data> temp_items = new List<Item_Data>();
                temp_items.Add(Items[Equipped - 1]);
                foreach (Item_Data item_data in not_equipped)
                    temp_items.Add(item_data);
                Items = temp_items;
                while (Items.Count < Constants.Actor.NUM_ITEMS)
                    Items.Add(new Item_Data());
                Equipped = 1;
            }
            else
                remove_broken_items();
        }

        /// <summary>
        /// Removes items with 0 uses left from inventory and compresses empty slots
        /// </summary>
        public void remove_broken_items()
        {
            int equipped = 0, broken_before_equipped = 0;
            List<Item_Data> items = new List<Item_Data>();
            for (int i = 0; i < Items.Count; i++)
            {
                Item_Data item = Items[i];
                // If item isn't broken
                if (!item.out_of_uses)//.Uses != 0) //Debug
                {
                    items.Add(item);
                    if (Equipped == i + 1)
                        equipped = Equipped - broken_before_equipped;
                }
                // Subtract from equipped to account for broken items
                else if (equipped == 0)
                    broken_before_equipped++;
            }
            while (items.Count < Constants.Actor.NUM_ITEMS)
                items.Add(new Item_Data());

            Items = items;
            if (is_equipped)
            {
                Equipped = equipped;
                if (Equipped == 0)
                    setup_items();
            }
        }

        /// <summary>
        /// Moves all blank items to the end of the inventory to remove gaps, then equips the first weapon found
        /// </summary>
        public void sort_items()
        {
            List<Item_Data> items = new List<Item_Data>();
            foreach (Item_Data item_data in Items)
                if (!item_data.blank_item)
                    items.Add(item_data);
            while (items.Count < Constants.Actor.NUM_ITEMS)
                items.Add(new Item_Data());
            Items = items;
            setup_items(false);
        }

        /// <summary>
        /// Equips the first non-staff weapon found, if possible.
        /// Returns true if the actor can only use/only has staves, otherwise returns false
        /// </summary>
        public bool staff_fix()
        {
            // If no staff rank
            if (!can_staff())
                return false;
            // If only staff rank
            if (is_staff_only())
                return true;

            // If non-staff already equipped
            if (this.is_equipped && !Items[Equipped - 1].to_weapon.is_staff())
                return false;

            // Checks items to see if actor has non-staves to equip
            bool has_non_staff = false;
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].is_weapon)
                    if (is_equippable(Global.data_weapons[Items[i].Id]) && !Global.data_weapons[Items[i].Id].is_staff())
                    {
                        has_non_staff = true;
                        break;
                    }
            if (has_non_staff)
            {
                int equipped = 0;
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    Item_Data item_data = Items[i];
                    if (item_data.is_weapon)
                        if (is_equippable(Global.data_weapons[item_data.Id]))
                            if (!Global.data_weapons[item_data.Id].is_staff())
                                equipped = i + 1;
                }
                // Equips a nonstaff if possible
                if (equipped != 0)
                {
                    equip(equipped);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the index of the first weapon found that matches the given weapon id. Returns null if none found
        /// </summary>
        /// <param name="weapon_id">Weapon id to search for</param>
        public int? weapon_index(int weapon_id)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Item_Data item_data = Items[i];
                if (item_data.is_weapon && item_data.Id == weapon_id)
                    return i;
            }
            return null;
        }

        /// <summary>
        /// Adds an item to the inventory
        /// </summary>
        /// <param name="item_data">Item data to add</param>
        public void gain_item(Item_Data item_data)
        {
            if (is_full_items)
                Items.Add(item_data);
            else
                Items[num_items] = item_data;
            sort_items();
        }
        public void set_items(IEnumerable<Item_Data> items)
        {
            Items.Clear();
            Items.AddRange(items);
            while (Items.Count < Constants.Actor.NUM_ITEMS)
                Items.Add(new Item_Data());
        }

        public void trade(Game_Actor target, int index1, int index2)
        {
            Item_Data item_data1 = Items[index1];
            Item_Data item_data2 = target.Items[index2];
            Items[index1] = item_data2;
            target.Items[index2] = item_data1;

            sort_items();
            staff_fix();
            target.sort_items();
            target.staff_fix();
        }

        /// <summary>
        /// Removes an item from the inventory
        /// </summary>
        /// <param name="index">Index of the item to remove</param>
        public void discard_item(int index)
        {
            if (index < 0)
                return;
            if (too_many_items)
                Items.RemoveAt(index);
            else
                Items[index] = new Item_Data();
            sort_items();
        }

        /// <summary>
        /// Removes all items from the inventory
        /// </summary>
        public void clear_items()
        {
            while (num_items > 0)
                discard_item(0);
        }

        /// <summary>
        /// Removes the last item from the inventory, and then returns the data of that item
        /// </summary>
        public Item_Data drop_item()
        {
            return drop_item(num_items - 1);
        }
        /// <summary>
        /// Removes an item from the inventory, and then returns the data of that item
        /// </summary>
        /// <param name="index">Index of the item to drop</param>
        public Item_Data drop_item(int index)
        {
            if (index < 0)
                return null;
            Item_Data item_data = Items[index];
            discard_item(index);
            return item_data;
        }

        /// <summary>
        /// If equipped, removes the weapon that is equipped from the inventory
        /// </summary>
        public void discard_weapon()
        {
            if (is_equipped)
                discard_item(Equipped - 1);
        }

        /// <summary>
        /// Repairs all items in the inventory with uses from items in the convoy
        /// </summary>
        public bool restock()
        {
            bool result = false;
            for (int i = 0; i < Constants.Actor.NUM_ITEMS; i++)
                result |= restock(i);
            return result;
        }
        /// <summary>
        /// Repairs an item with uses from items in the convoy
        /// </summary>
        /// <param name="index">Index of item to restock</param>
        public bool restock(int index)
        {
            // If there is no item at this index, return
            if (Items[index].Id <= 0)
                return false;
            int max_uses = Items[index].max_uses;
            // If the item is already fully repaired, return
            if (Items[index].Uses == max_uses)
                return false;
            else
            {
                int convoy_index = -1;
                for (int i = 0; i < Global.game_battalions.active_convoy_data.Count; i++)
                    if (Items[index].same_item(Global.game_battalions.active_convoy_data[i]))
                    {
                        convoy_index = i;
                    }
                // No matching items in convoy
                if (convoy_index == -1)
                    return false;
                // Restock item
                else
                {
                    while (Items[index].Uses < max_uses && convoy_index >= 0 && Global.game_battalions.active_convoy_data.Count > 0 &&
                        Items[index].same_item(Global.game_battalions.active_convoy_data[convoy_index]))
                    {
                        // If combining items breaks convoy item
                        if (Global.game_battalions.active_convoy_data[convoy_index].Uses + Items[index].Uses <= max_uses)
                        {
                            Items[index].add_uses(Global.game_battalions.active_convoy_data[convoy_index].Uses);
                            Global.game_battalions.remove_item_from_convoy(Global.battalion.convoy_id, convoy_index);
                        }
                        else
                        {
                            Global.game_battalions.adjust_convoy_item_uses(Global.battalion.convoy_id, convoy_index, Items[index].Uses - max_uses);
                            Items[index].repair_fully();
                        }
                        convoy_index--;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the number of items in the inventory
        /// </summary>
        public int num_items
        {
            get
            {
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    if (!Items[i].blank_item)
                        return i + 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// Returns true if the inventory has the max number of items (or more)
        /// </summary>
        public bool is_full_items { get { return num_items >= Constants.Actor.NUM_ITEMS; } }
        /// <summary>
        /// Returns true if the inventory has more than the max number of items
        /// </summary>
        public bool too_many_items { get { return num_items > Constants.Actor.NUM_ITEMS; } }
        /// <summary>
        /// Returns true if there are any items in the inventory
        /// </summary>
        public bool has_items { get { return num_items > 0; } }
        /// <summary>
        /// Returns true if there are no items in the inventory
        /// </summary>
        public bool has_no_items { get { return num_items == 0; } }

        /// <summary>
        /// Equips the weapon at the given index
        /// </summary>
        /// <param name="index">Index to equip</param>
        public void equip(int index)
        {
            if (index == 0 || index > num_items)
            {
                Weapon_Id = 0;
                Equipped = 0;
            }
            else
            {
                Weapon_Id = Items[index - 1].Id;
                Equipped = index;
            }
            skill_list_update();
        }

        /// <summary>
        /// 
        /// </summary>
        public void unequip()
        {
            equip(0);
        }

        public bool in_equip_range(int index)
        {
            return index <= Constants.Actor.NUM_ITEMS;
        }

        /// <summary>
        /// Returns a dictionary of the gains to each stat resulting from using the given item
        /// </summary>
        /// <param name="item">Item to get stat boosts from</param>
        internal Dictionary<Boosts, int> item_boosts(Data_Item item)
        {
            Dictionary<Boosts, int> result = new Dictionary<Boosts, int>();
            for (int i = 0; i < item.Stat_Boost.Length; i++)
            {
                if (item.Stat_Boost[i] != 0)
                    switch ((Boosts)i)
                    {
                        case Boosts.MaxHp:
                        case Boosts.Pow:
                        case Boosts.Skl:
                        case Boosts.Spd:
                        case Boosts.Lck:
                        case Boosts.Def:
                        case Boosts.Res:
                            if (item.Stat_Boost[i] > 0 ? !get_capped(i) : stat(i) > 0)
                            {
                                if (item.Stat_Boost[i] > 0)
                                    result.Add((Boosts)i, Math.Min(item.Stat_Boost[i], get_cap(i) - stat(i)));
                                else
                                    result.Add((Boosts)i, Math.Max(item.Stat_Boost[i], -stat(i)));
                            }
                            break;
                        case Boosts.Con:
                            if (item.Stat_Boost[i] > 0 ? !get_capped(Stat_Labels.Con) : stat(Stat_Labels.Con) > 0)
                            {
                                if (item.Stat_Boost[i] > 0)
                                    result.Add((Boosts)i, Math.Min(item.Stat_Boost[i], get_cap(Stat_Labels.Con) - stat(Stat_Labels.Con)));
                                else
                                    result.Add((Boosts)i, Math.Max(item.Stat_Boost[i], -stat(Stat_Labels.Con)));
                            }
                            break;
                    }
            }
            return result;
        }
        /// <summary>
        /// Apply effect of item data
        /// </summary>
        /// <param name="item">Item to apply</param>
        internal void item_effect(Data_Item item, int target_index)
        {
            // Healing
            int recover = item_heal_amount(item);
            hp = Math.Min(maxhp, Hp + recover);
            // Status
            foreach (int i in item.Status_Remove)
            {
                remove_state(i);
            }
            foreach (int i in item.Status_Inflict)
            {
                add_state(i);
            }
            // Stat Boost
            WeaponType weapon_type;
            for (int i = 0; i < item.Stat_Boost.Length; i++)
            {
                if (item.Stat_Boost[i] != 0)
                    switch ((Boosts)i)
                    {
                        case Boosts.MaxHp:
                            gain_stat(i, item.Stat_Boost[i]);
                            break;
                        case Boosts.Pow:
                        case Boosts.Skl:
                        case Boosts.Spd:
                        case Boosts.Lck:
                        case Boosts.Def:
                        case Boosts.Res:
                            gain_stat(i, item.Stat_Boost[i]);
                            break;
                        case Boosts.Mov:
                            gain_stat(Stat_Labels.Mov, item.Stat_Boost[i]);
                            break;
                        case Boosts.Con:
                            gain_stat((Stat_Labels.Con), item.Stat_Boost[i]);
                            break;
                        case Boosts.WLvl:
                            if (is_equipped)
                                weapon_type = valid_weapon_type_of(Items[Equipped - 1].to_weapon);
                            else
                            {
                                // Find first valid type or something //Yeti
                                weapon_type = Global.weapon_types[0];
                            }
                            if (weapon_type != Global.weapon_types[0])
                            {
                                int wexp = Data_Weapon.WLVL_THRESHOLDS[Math.Min(get_weapon_level(weapon_type) + item.Stat_Boost[i],
                                    Data_Weapon.WLVL_THRESHOLDS.Length - 1)] - weapon_levels(weapon_type);
                                wexp_gain(weapon_type, wexp);
                                clear_wlvl_up();
                            }
                            break;
                        case Boosts.WExp:
                            if (is_equipped)
                                weapon_type = valid_weapon_type_of(Items[Equipped - 1].to_weapon);
                            else
                            {
                                // Find first valid type or something //Yeti
                                weapon_type = Global.weapon_types[0];
                            }
                            if (weapon_type != Global.weapon_types[0])
                            {
                                wexp_gain(weapon_type, item.Stat_Boost[i]);
                                clear_wlvl_up();
                            }
                            break;
                    }
            }
            // Growth Boost
            for (int i = 0; i < item.Growth_Boost.Length; i++)
                Growth_Bonuses[i] += item.Growth_Boost[i];
            // Repair
            if (item.can_repair && target_index >= 0)
                if (!Items[target_index].infinite_uses)
                    Items[target_index].set_uses(
                        Items[target_index].Uses + Math.Max(1, (int)(Items[target_index].max_uses * item.Repair_Percent) +
                        (Items[target_index].cost == 0 ? 0 : item.Repair_Val / Items[target_index].cost)));
        }

        /// <summary>
        /// Returns the amount of hp a given item would heal
        /// </summary>
        private int item_heal_amount(Data_Item item)
        {
            return (int)(maxhp * item.Heal_Percent / 1.0f) + item.Heal_Val;
        }

        /// <summary>
        /// Reduces the use count of an item by one, then removes it if it is at 0 uses
        /// </summary>
        /// <param name="index">Index of the item to consume</param>
        /// <param name="organize_if_broken">If true and the used item is consumed, move the equipped weapon to the first slot before removing empty items</param>
        public void use_item(int index, bool organize_if_broken = false)
        {
            if (!Items[index].infinite_uses)
                Items[index].consume_use();
            if (Items[index].out_of_uses)
            {
                if (organize_if_broken)
                    organize_items();
                else
                    remove_broken_items();
            }
        }

        /// <summary>
        /// Gets the weapon type this weapon is treated as by this actor.
        /// For example, if the actor has an anima rank and no fire rank,
        /// and the anima rank is higher than the tome rank,
        /// it would treat a fire tome as anima.
        /// </summary>
        /// <param name="weapon">Weapon to check</param>
        public WeaponType valid_weapon_type_of(Data_Weapon weapon)
        {
            var weapon_type = weapon.main_type();

            // Check if the weapon's type is a parent of a type this actor has
            if (Constants.Actor.CHILD_WEAPON_TYPE_ALLOWS_PARENT)
            {
                // If this type can't already be used as its own type
                if ((int)weapon.Rank > get_weapon_level(weapon_type))
                    foreach (var child_type in Global.weapon_types
                            .OrderByDescending(x => weapon_levels(x)))
                        if (child_type.type_and_parents(Global.weapon_types)
                                .Skip(1)
                                .Contains(weapon_type))
                            if ((int)weapon.Rank <= get_weapon_level(child_type))
                                return child_type;
            }
            // Check if the weapon's type is a child of a type this actor has
            if (Constants.Actor.PARENT_WEAPON_TYPE_ALLOWS_CHILD)
            {
                // If this type can't already be used as its own type
                if ((int)weapon.Rank > get_weapon_level(weapon_type))
                    foreach (var parent_type in weapon_type.type_and_parents(Global.weapon_types).Skip(1))
                        if ((int)weapon.Rank <= get_weapon_level(parent_type))
                            return parent_type;
            }

            return weapon_type;
        }
        /// <summary>
        /// Gets the weapon type this weapon is treated as by this actor.
        /// For example, if the actor has an anima rank and no fire rank,
        /// it would treat a fire tome as anima.
        /// </summary>
        /// <param name="weapon">Weapon to check</param>
        public WeaponType valid_possible_weapon_type_of(Data_Weapon weapon)
        {
            var weapon_type = weapon.main_type();

            // Check if the weapon's type is a parent of a type this actor has
            if (Constants.Actor.CHILD_WEAPON_TYPE_ALLOWS_PARENT)
            {
                // If this type can't already be used as its own type
                if ((int)weapon.Rank > max_weapon_level(weapon_type))
                    foreach (var child_type in Global.weapon_types
                            .OrderByDescending(x => weapon_levels(x)))
                        if (child_type.type_and_parents(Global.weapon_types)
                                .Skip(1)
                                .Contains(weapon_type))
                            if ((int)weapon.Rank <= max_weapon_level(child_type))
                                return child_type;
            }
            // Check if the weapon's type is a child of a type this actor has
            if (Constants.Actor.PARENT_WEAPON_TYPE_ALLOWS_CHILD)
            {
                // If this type can't already be used as its own type
                if ((int)weapon.Rank > max_weapon_level(weapon_type))
                    foreach (var parent_type in weapon_type.type_and_parents(Global.weapon_types).Skip(1))
                        if ((int)weapon.Rank <= max_weapon_level(parent_type))
                            return parent_type;
            }

            return weapon_type;
        }

        /// <summary>
        /// Returns true if the checked item is a weapon and it can be used
        /// </summary>
        /// <param name="item_index">Index of item to check</param>
        public bool is_equippable(int item_index)
        {
            if (!Items[item_index].is_weapon || Items[item_index].non_equipment)
                return false;
            return is_equippable(Items[item_index].to_weapon);
        }
        /// <summary>
        /// Returns true if the actor qualifies for the checked weapon's rank and all Prf checks succeed
        /// </summary>
        /// <param name="weapon">Weapon to check</param>
        public bool is_equippable(Data_Weapon weapon)
        {
            return equippable(weapon) == Equippability.CanEquip;
        }
        /// <summary>
        /// Returns true if the actor can equip the weapon, allowing siege weapons
        /// </summary>
        /// <param name="weapon">Weapon to check</param>
        public bool is_equippable_as_siege(Data_Weapon weapon)
        {
            var equip = equippable(weapon);
            return equip == Equippability.CanEquip ||
                equip == Equippability.CanEquipSiege;
        }

        public bool is_equippable(List<Item_Data> items, int index)
        {
            var item_data = items[index];
            if (item_data.non_equipment || !item_data.is_weapon)
                return false;

            var weapon = item_data.to_weapon;
            if (index == Siege_Engine.SIEGE_INVENTORY_INDEX)
                return is_equippable_as_siege(weapon);
            else
                return is_equippable(weapon);
        }

        /// <summary>
        /// Determines the equippability of a weapon, and the reason why
        /// </summary>
        private Equippability equippable(Data_Weapon weapon)
        {
            var weapon_type = valid_weapon_type_of(weapon);
            bool valid_rank = (int)weapon.Rank <= get_weapon_level(weapon_type);

            if (valid_rank)
            {
                if (!prf_check(weapon))
                    return Equippability.CannotEquipPrf;

                if (this.silenced && weapon.blocked_by_silence)
                    return Equippability.Silenced;

                return weapon.Ballista() ?
                    Equippability.CanEquipSiege : Equippability.CanEquip;
            }
            return Equippability.CannotEquip;
        }

        /// <summary>
        /// Returns true if the class qualifies for the checked weapon's rank and class Prf check succeeds
        /// </summary>
        /// <param name="weapon">Weapon to check</param>
        public bool class_equippable(Data_Weapon weapon)
        {
            var weapon_type = weapon.main_type();
            bool valid_rank = (int)weapon.Rank <= this.actor_class.Max_WLvl[weapon_type.Key - 1];

            // Check if the weapon's type is a parent of a type this actor has
            if (Constants.Actor.CHILD_WEAPON_TYPE_ALLOWS_PARENT && !valid_rank)
            {
                foreach (var child_type in Global.weapon_types)
                    if (child_type.type_and_parents(Global.weapon_types)
                            .Skip(1)
                            .Contains(weapon_type))
                        if ((int)weapon.Rank <= this.actor_class.Max_WLvl[child_type.Key - 1])
                        {
                            valid_rank = true;
                            break;
                        }
            }
            // Check if the weapon's type is a child of a type this actor has
            if (Constants.Actor.PARENT_WEAPON_TYPE_ALLOWS_CHILD && !valid_rank)
            {
                foreach (var parent_type in weapon_type.type_and_parents(Global.weapon_types).Skip(1))
                    if ((int)weapon.Rank <= this.actor_class.Max_WLvl[parent_type.Key - 1])
                        {
                            valid_rank = true;
                            break;
                        }
            }

            if ((int)weapon.Rank <= this.actor_class.Max_WLvl[(int)weapon.Main_Type - 1])
            {
                if (!this.actor_class.prf_check(weapon))
                    return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if all Prf checks succeed and, if the item can promote classes, the actor's class is on the item's promotion list
        /// </summary>
        /// <param name="weapon">Item to check</param>
        public bool is_useable(Data_Item item)
        {
            if (!prf_check(item))
                return false;
            if (item.Promotes.Count > 0)
                if (!item.Promotes.Contains(class_id))
                    return false;
            return true;
        }

        /// <summary>
        /// Checks the Prf data for an item and only returns true if the actor passes all checks
        /// </summary>
        /// <param name="item">Item to check</param>
        public bool prf_check(Data_Equipment item)
        {
            // Block use if the actor/class/type are on any of the disabled lists
            if (item.Prf_Character.Count > 0)
                if (item.Prf_Character.Contains(-Id)) return false;
            if (item.Prf_Class.Count > 0)
                if (item.Prf_Class.Contains(-class_id)) return false;
            if (item.Prf_Type.Count > 0)
                foreach (ClassTypes type in this.class_types)
                    if (item.Prf_Type.Contains(-(int)type))
                        return false;

            // If there are any positive prf requirements and not just negative ones, this actor must succeed at least one of those requirements
            bool prf_required = false;
            if (item.Prf_Character.Count > 0)
            {
                if (item.Prf_Character.Contains(Id)) return true;
                // If there are any prf actors, this actor must succeed one of the other tests or this is unusable
                for (int i = 0; i < item.Prf_Character.Count; i++)
                    if (item.Prf_Character[i] > 0)
                    {
                        prf_required = true;
                        break;
                    }
            }
            if (item.Prf_Class.Count > 0)
            {
                if (item.Prf_Class.Contains(class_id)) return true;
                // If there are any prf classes, this actor must succeed one of the other tests or this is unusable
                for (int i = 0; i < item.Prf_Class.Count; i++)
                    if (item.Prf_Class[i] > 0)
                    {
                        prf_required = true;
                        break;
                    }
            }
            if (item.Prf_Type.Count > 0)
            {
                foreach (ClassTypes type in this.class_types)
                    if (item.Prf_Type.Contains((int)type))
                        return true;
                // If there are any prf types, this actor must succeed one of the other tests or this is unusable (but there are no more tests)
                for (int i = 0; i < item.Prf_Type.Count; i++)
                    if (item.Prf_Type[i] > 0)
                    {
                        prf_required = true;
                        break;
                    }

            }
            return !prf_required;
        }
        /*public bool prf_check(Data_Equipment item) //Debug
        {
            if (item.Prf_Character.Count > 0)
                if (!item.Prf_Character.Contains(Id) || item.Prf_Character.Contains(-Id)) return false;
            if (item.Prf_Class.Count > 0)
                if (!item.Prf_Class.Contains(class_id) || item.Prf_Class.Contains(-class_id)) return false;
            if (item.Prf_Type.Count > 0)
            {
                bool use = false;
                foreach (ClassTypes type in this.class_types)
                {
                    if (item.Prf_Type.Contains(-(int)type))
                        return false;
                    else if (!use && item.Prf_Type.Contains((int)type))
                        use = true;
                }
                if (!use) return false;
            }
            return true;
        }*/

        /// <summary>
        /// Returns a list of all useable non-staff weapons
        /// </summary>
        public List<int> useable_weapons()
        {
            return useable_weapons(Items);
        }
        /// <summary>
        /// Returns all the useable non-staff weapons from the given list as a new list
        /// </summary>
        /// <param name="items">List of items to check</param>
        public List<int> useable_weapons(List<Item_Data> items)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < items.Count; i++)
            {
                Item_Data item_data = items[i];
                if (item_data.is_weapon)
                {
                    Data_Weapon weapon = item_data.to_weapon;
                    if (!weapon.is_staff())
                    {
                        bool can_equip = is_equippable(weapon);
                        if (!can_equip)
                            can_equip = i == Siege_Engine.SIEGE_INVENTORY_INDEX &&
                                is_equippable_as_siege(weapon);

                        if (can_equip)
                            result.Add(i);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a list of all useable phsyical melee weapons
        /// </summary>
        public List<int> useable_melee_weapons()
        {
            return useable_melee_weapons(Items);
        }
        /// <summary>
        /// Returns all the useable physical melee weapons from the given list as a new list
        /// </summary>
        /// <param name="items">List of items to check</param>
        public List<int> useable_melee_weapons(List<Item_Data> items)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < items.Count; i++)
            {
                if (useable_melee_weapon(items[i]))
                    result.Add(i);
            }
            return result;
        }

        /// <summary>
        /// Checks if an item is a useable physical melee weapon
        /// </summary>
        /// <param name="items">List of items to check</param>
        public bool useable_melee_weapon(Item_Data itemData)
        {
            if (itemData.is_weapon)
            {
                Data_Weapon weapon = Global.data_weapons[itemData.Id];
                if (is_equippable(weapon) && weapon.Min_Range == 1 &&
                        !weapon.is_staff() && !weapon.is_always_magic())
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a list of all useable staves
        /// </summary>
        public List<int> useable_staves()
        {
            return useable_staves(Items);
        }
        /// <summary>
        /// Returns all the useable staves from the given list as a new list
        /// </summary>
        /// <param name="items">List of items to check</param>
        public List<int> useable_staves(List<Item_Data> items)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < items.Count; i++)
            {
                Item_Data item_data = items[i];
                if (item_data.is_weapon)
                {
                    Data_Weapon weapon = item_data.to_weapon;
                    if (weapon.is_staff())
                    {
                        bool can_equip = is_equippable(weapon);
                        if (!can_equip)
                            can_equip = i == Siege_Engine.SIEGE_INVENTORY_INDEX &&
                                is_equippable_as_siege(weapon);

                        if (can_equip)
                            result.Add(i);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a list of all useable attack staves
        /// </summary>
        public List<int> useable_attack_staves()
        {
            return useable_attack_staves(Items);
        }
        /// <summary>
        /// Returns all the useable attack staves from the given list as a new list
        /// </summary>
        /// <param name="items">List of items to check</param>
        public List<int> useable_attack_staves(List<Item_Data> items)
        {
            List<int> result = useable_staves(items);
            int i = 0;
            while (i < result.Count)
            {
                Item_Data item_data = items[result[i]];
                Data_Weapon weapon = Global.data_weapons[item_data.Id];
                if (weapon.is_attack_staff())
                    i++;
                else
                    result.RemoveAt(i);
            }
            return result;
        }

        /// <summary>
        /// Returns a list of all useable healing staves
        /// </summary>
        public List<int> useable_healing_staves()
        {
            return useable_healing_staves(Items);
        }
        /// <summary>
        /// Returns all the useable healing staves from the given list as a new list
        /// </summary>
        /// <param name="items">List of items to check</param>
        public List<int> useable_healing_staves(List<Item_Data> items)
        {
            return useable_staves(items)
                .Where(x =>
                {
                    Item_Data item_data = items[x];
                    Data_Weapon weapon = Global.data_weapons[item_data.Id];
                    // If the weapon is an attack staff return false
                    if (weapon.is_attack_staff())
                        return false;
                    // Staff must heal, or heal statuses, or barrier, or apply statuses
                    return weapon.Heals() || weapon.Status_Remove.Count > 0 ||
                        weapon.Barrier() || weapon.Status_Inflict.Count > 0;
                })
                .ToList();
            /*List<int> result = useable_staves(items); //Debug
            int i = 0;
            while (i < result.Count)
            {
                Item_Data item_data = items[result[i]];
                Data_Weapon weapon = Global.data_weapons[item_data.Id];
                if (!weapon.is_always_magic() && (weapon.Heals() || weapon.Status_Remove.Count > 0))
                    i++;
                else
                    result.RemoveAt(i);
            }
            return result;*/
        }

        /// <summary>
        /// Returns a list of all useable untargeted staves
        /// </summary>
        public List<int> useable_untargeted_staves()
        {
            return useable_untargeted_staves(Items);
        }
        /// <summary>
        /// Returns all the useable untargeted staves from the given list as a new list
        /// </summary>
        /// <param name="items">List of items to check</param>
        public List<int> useable_untargeted_staves(List<Item_Data> items)
        {
            List<int> result = useable_staves(items);
            int i = 0;
            while (i < result.Count)
            {
                Item_Data item_data = items[result[i]];
                Data_Weapon weapon = Global.data_weapons[item_data.Id];
                if (!weapon.is_attack_staff() &&
                        (weapon.Torch() || (weapon.Hits_All_in_Range() &&
                        !(weapon.Heals() || weapon.Status_Remove.Count > 0))))
                    i++;
                else
                    result.RemoveAt(i);
            }
            return result;
        }

        public bool can_give(Item_Data itemData)
        {
            // Use to stop trading away body parts or prized possessions or whatever //Debug
            return true;
        }
        public bool can_take(Item_Data itemData)
        {
            if (itemData.is_weapon)
            {
                var weapon = itemData.to_weapon;
                // Siege engines cannot be held in an actor's inventory
                if (weapon.Ballista())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the number of uses to consume for a weapon use
        /// </summary>
        /// <param name="is_hit">Did the attack hit?</param>
        /// <param name="player_team">Is this actor on a player team?</param>
        public int weapon_use_count(bool is_hit, bool player_team)
        {
            // Various properties here should be part of the weapon type data, instead of inferred //Yeti

            if (Global.game_system.In_Arena)
                return 0;
            if (this.weapon.infinite_uses)
                return 0;

            int result = 1;
            // If the attack missed and missing doesn't count
            // or is an enemy, enemies don't use weapon uses, and the override is on
            if (!(is_hit || Constants.Combat.WEAPON_USE_MISS) ||
                (!player_team && !Constants.Combat.AI_WEAPON_USE &&
                Constants.Combat.AI_WEAPON_USE_MISS_OVERRIDE))
            {
                // and missing with ranged doesn't count
                if (this.weapon.Thrown() || this.weapon.main_type().Name == "Bow")
                {
                    if (!Constants.Combat.RANGED_USE_MISS)
                        result = 0;
                }
                // and missing with magic doesn't count
                else if (this.weapon.main_type().IsMagic || this.weapon.main_type().IsStaff ||
                    this.weapon.scnd_type().IsMagic || this.weapon.scnd_type().IsStaff)
                {
                    if (!Constants.Combat.MAGIC_USE_MISS)
                        result = 0;
                }
                // and the weapon isn't ranged or magic
                else
                    result = 0;
            }
            // If an AI unit and AI units don't consume uses
            else if (!player_team && !Constants.Combat.AI_WEAPON_USE)
                result = 0;

            int? skill_use_count = weapon_use_count_skill(is_hit, result); // Overwritten by Blessing //Yeti
            if (skill_use_count != null)
                return (int)skill_use_count;
            return result;
        }

        /// <summary>
        /// Consumes one use of the equipped weapon
        /// </summary>
        public void weapon_use()
        {
            if (Items[Equipped - 1].Uses <= 0)
                return;
            Items[Equipped - 1].consume_use();
        }

        /// <summary>
        /// Returns true if the actor has a rank in this weapon type
        /// </summary>
        public bool has_rank(WeaponType weapon_type)
        {
            return weapon_levels(weapon_type) > 0;
        }

        /// <summary>
        /// Returns true if the actor has a rank in this weapon type,
        /// or a rank in any other type that allows using this type
        /// </summary>
        private bool can_use_weapon_type(WeaponType weapon_type)
        {
            if (has_rank(weapon_type))
                return true;

            // Check if the type is a parent of a type this actor has
            if (Constants.Actor.CHILD_WEAPON_TYPE_ALLOWS_PARENT)
            {
                // If this type can't already be used as its own type
                foreach (var child_type in Global.weapon_types
                        .OrderByDescending(x => weapon_levels(x)))
                    if (child_type.type_and_parents(Global.weapon_types)
                            .Skip(1)
                            .Contains(weapon_type))
                        if (has_rank(child_type))
                            return true;
            }
            // Check if the type is a child of a type this actor has
            if (Constants.Actor.PARENT_WEAPON_TYPE_ALLOWS_CHILD)
            {
                // If this type can't already be used as its own type
                foreach (var parent_type in weapon_type
                        .type_and_parents(Global.weapon_types)
                        .Skip(1))
                    if (has_rank(parent_type))
                        return true;
            }

            return false;
        }

        public bool can_staff()
        {
            return Global.weapon_types.Skip(1)
                .Any(x => x.IsStaff && can_use_weapon_type(x));
        }
        public WeaponType staff_type()
        {
            var type = Global.weapon_types
                .Skip(1)
                .FirstOrDefault(x => x.IsStaff && can_use_weapon_type(x));
            if (type == null)
                return Global.weapon_types[0];
            return type;
        }

        /// <summary>
        /// Returns true if the actor has a staff rank and no other weapon ranks
        /// </summary>
        public bool is_staff_only()
        {
            if (!can_staff())
                return false;
            for (int i = 1; i < Global.weapon_types.Count; i++)
                if (!Global.weapon_types[i].IsStaff)
                    if (can_use_weapon_type(Global.weapon_types[i]))
                        return false;
            return true;
        }

        /// <summary>
        /// Returns true if the actor has a staff rank and no equippable non-staff weapons in the inventory
        /// </summary>
        public bool has_staves_only()
        {
            if (!can_staff())
                return false;
            for (int i = 0; i < Items.Count; i++)
            {
                Item_Data item_data = Items[i];
                if (item_data.is_weapon)
                {
                    Data_Weapon weapon = Global.data_weapons[item_data.Id];
                    if (is_equippable(weapon) && !weapon.is_staff())
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the actor can enter the arena
        /// </summary>
        public bool can_arena()
        {
            // Does not take manaketes into account, but since this is only used to test arena viability... //Debug
            for (int i = 1; i < Global.weapon_types.Count; i++)
            {
                var weapon_type = Global.weapon_types[i];
                if (!weapon_type.IsStaff &&
                        weapon_type.DisplayedInStatus &&
                        can_use_weapon_type(weapon_type))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the actor has no useable weapon types
        /// </summary>
        public bool is_non_combat()
        {
            for (int i = 1; i < Global.weapon_types.Count; i++)
                if (has_rank(Global.weapon_types[i]))
                    return false;
            return true;
        }

        //Sparring
        /// <summary>
        /// Returns true if the actor can oversee sparring
        /// </summary>
        public bool can_oversee_sparring()
        {
            if (can_oversee_sparring_normally())
                return true;
            if (has_skill("SACRIFICE"))
                return true;
            return false;
        }

        //Sparring
        /// <summary>
        /// Returns true if the actor can always oversee sparring, without special skills
        /// </summary>
        internal bool can_oversee_sparring_normally()
        {
            if (can_staff())
                return true;
            if (has_skill("PROVISION"))
                return true;
            return false;
        }

        //Sparring
        /// <summary>
        /// Returns the number of times this unit can oversee sparring
        /// </summary>
        public int sparring_overseer_points()
        {
            if (!can_oversee_sparring())
                return 0;
            if (can_oversee_sparring_normally())
            {
                if (can_staff())
                    return (int)Math.Ceiling(get_weapon_level(staff_type()) / 2f);
                return Battalion.SPAR_COUNT;
            }
            return 1;
        }

        //Sparring
        public int sparring_points_cost(bool overseeing)
        {
            return 1;

            if (overseeing)
            {
                if (can_oversee_sparring_normally())
                    return 1;
                else
                    return Battalion.SPAR_COUNT;
            }
            else
                return 1;
        }

        /// <summary>
        /// Returns true if the actor has any equippable non-staff weapons
        /// </summary>
        public bool can_attack()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Item_Data item_data = Items[i];
                if (item_data.is_weapon)
                {
                    Data_Weapon weapon = Global.data_weapons[item_data.Id];
                    if (is_equippable(weapon) && !weapon.is_staff())
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the actor has any equippable staves that can heal the target
        /// </summary>
        /// <param name="target">Unit to test against</param>
        internal bool can_heal(Game_Unit target)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Item_Data item_data = Items[i];
                if (item_data.is_weapon)
                {
                    Data_Weapon weapon = Global.data_weapons[item_data.Id];
                    if (is_equippable(weapon) && weapon.can_heal(target))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the weapon type that should be used in the arena.
        /// If any weapon is equipped, its type is used; otherwise the first weapon type with a rank in order is used
        /// </summary>
        public WeaponType determine_arena_weapon_type()
        {
            // Already has something equipped, so use that type
            if (this.weapon != null)
                if (!this.weapon.is_staff() && Config.ARENA_WEAPON_TYPES.ContainsKey(this.weapon.Main_Type))
                    return this.weapon.main_type();
            // Otherwise use first possible type
            for (int i = 1; (int)i < Global.weapon_types.Count; i++)
                if (!Global.weapon_types[i].IsStaff &&
                        can_use_weapon_type(Global.weapon_types[i]) &&
                        Config.ARENA_WEAPON_TYPES.ContainsKey(i))
                    return Global.weapon_types[i];
            // Could I just call determine_sparring_weapon_type() and then if it's staff or none throw the exception? //Yeti
            throw new KeyNotFoundException("No valid weapon type for the arena");
            return Global.weapon_types[1]; // Should never get here
        }
        //Sparring
        /// <summary>
        /// Returns the weapon type that should be used when sparring.
        /// If any weapon is equipped, its type is used; otherwise the first weapon type with a rank in order is used.
        /// If no combat type is valid, returns staff if there is a staff rank, and otherwise returns none
        /// </summary>
        public WeaponType determine_sparring_weapon_type()
        {
            // Already has something equipped, so use that type
            if (this.weapon != null)
                if (!this.weapon.is_staff() && Config.ARENA_WEAPON_TYPES.ContainsKey(this.weapon.Main_Type))
                    return this.weapon.main_type();
            // Otherwise use first possible type
            for (int i = 1; (int)i < Global.weapon_types.Count; i++)
                if (!Global.weapon_types[i].IsStaff &&
                        can_use_weapon_type(Global.weapon_types[i]) &&
                        Config.ARENA_WEAPON_TYPES.ContainsKey(i))
                    return Global.weapon_types[i];
            if (can_staff())
                return staff_type();
            return Global.weapon_types[0];
        }

        /// <summary>
        /// Returns true if any items in the inventory can be used from the convoy item screen
        /// </summary>
        public bool can_use_convoy_item()
        {
            foreach (Item_Data item in Items)
                if (!item.non_equipment && !item.is_weapon &&
                        prf_check(item.to_equipment) && Combat.can_use_item(this, item.Id, false))
                    return true;
            return false;
        }

        /// <summary>
        /// Returns true if health is below critical level
        /// </summary>
        public bool has_critical_health()
        {
            return has_critical_health(Hp);
        }
        private bool has_critical_health(int hp)
        {
            return hp <= critical_health_level();
        }

        public bool would_heal_above_critcal(Data_Item item)
        {
            if (has_critical_health())
            {
                return !has_critical_health(Hp + item_heal_amount(item));
            }
            return false;
        }

        private int critical_health_level()
        {
            return (int)(Constants.Actor.LOW_HEALTH_RATE * this.maxhp);
        }

        internal int weapon_wgt(Data_Weapon weapon)
        {
            int wgt = weapon.Wgt;
            return wgt;
        }

        public int wgt_penalty(Data_Weapon weapon)
        {
            int wgt = weapon_wgt(weapon);
            return (int)(stat(Stat_Labels.Con) < wgt ? wgt - stat(Stat_Labels.Con) : 0);
        }
        #endregion

        #region Supports
        internal void initialize_support_progress()
        {
            foreach (string support_name in Data.Supports)
            {
                int partner_id = Global.data_supports[support_name].Id1 != Id ?
                    Global.data_supports[support_name].Id1 : Global.data_supports[support_name].Id2;
                // If this partner doesn't have any support data already, add them
                if (!Support_Progress.ContainsKey(partner_id))
                    Support_Progress[partner_id] = 0;
            }
        }

        /// <summary>
        /// Returns whether supports can happen at all with another actor.
        /// </summary>
        /// <param name="actorId">Id of the other actor</param>
        private bool can_support(int actorId)
        {
            return Support_Progress.ContainsKey(actorId);
        }
        /// <summary>
        /// Returns a list of Ids of all actors this actor can possibly support.
        /// </summary>
        public List<int> support_candidates()
        {
            List<int> result = Support_Progress.Keys.ToList();
            //result.Sort(); // why was this being sorted though, maybe I want to choose the order in the editor //Yeti
            return result;
        }

        /// <summary>
        /// Returns whether this actor could possibly gain a support rank with another actor;
        /// they can support, a support isn't blocked, they're not at the max support rank, etc.
        /// </summary>
        /// <param name="actorId">Id of the other actor</param>
        internal bool support_possible(int actorId)
        {
            return can_support(actorId) &&
                !Global.game_state.is_support_blocked(Id, actorId) &&
                !is_support_level_maxed(actorId) &&
                !is_support_maxed() &&
                needed_support_points(actorId) != -1;
        }

        public bool has_points_for_support(int actorId)
        {
            if (needed_support_points(actorId) != -1)
            {
                if (Scene_Map.debug_chapter_options_blocked())
                    if (Global.game_system.preparations)
                        return true;

                return Support_Progress[actorId] >= needed_support_points(actorId);
            }
            return false;
        }

        /// <summary>
        /// Returns true if this unit can currently support with another actor.
        /// </summary>
        /// <param name="actorId">Id of the other actor</param>
        /// <param name="include_blocked">If true, ignores whether these actors have their support blocked</param>
        public bool is_support_ready(int actorId, bool include_blocked = false)
        {
            if (Global.scene.is_map_scene && Global.game_state.is_support_blocked(
                    Id, actorId, activation_only: !include_blocked))
                return false;
            if (can_support(actorId))
                if (!is_support_level_maxed(actorId) &&
                        Global.game_actors.actor_loaded(actorId) &&
                        Global.battalion.actors.Contains(actorId) &&
                        !is_support_maxed(false, actorId) &&
                        !Global.game_actors[actorId].is_support_maxed(false, Id))
                    return has_points_for_support(actorId);
            return false;
        }
        /// <summary>
        /// Returns actor Ids this actor is ready to support with.
        /// Includes Ids for pairs that cannot be activated because the support level was already increased.
        /// </summary>
        public List<int> ready_supports()
        {
            List<int> result = Support_Progress.Keys.ToList();
            int i = 0;
            while (i < result.Count)
            {
                if (!is_support_ready(result[i], include_blocked: true))
                    result.RemoveAt(i);
                else
                    i++;
            }
            result.Sort();
            return result;
        }
        /// <summary>
        ///  Returns true if this actor can currently support with any other actor.
        /// </summary>
        public bool any_supports_ready()
        {
            if (is_support_maxed())
                return false;
            foreach (int actor_id in Support_Progress.Keys)
                if (is_support_ready(actor_id))
                    return true;
            return false;
        }

        /// <summary>
        /// Returns the amount of support points this actor and another actor need to rank up. Returns -1 if they cannot support.
        /// </summary>
        /// <param name="actorId">Id of the other actor</param>
        private int needed_support_points(int actorId)
        {
            if (!can_support(actorId))
                return -1;
            int result = support_data(actorId)[get_support_level(actorId)].Turns;
            // If the support cannot be gained organically, the value will be -1
            if (result == -1)
                return -1;
            // A support rank can at most take Config.MAX_SUPPORT_POINTS points
            return Math.Min(Constants.Support.MAX_SUPPORT_POINTS, result);
        }

        /// <summary>
        /// Returns the support string data for this and another actor. Returns null if they cannot support or have no data.
        /// </summary>
        /// <param name="actorId">Id of the other actor</param>
        /// <returns></returns>
        private List<Support_Entry> support_data(int actorId)
        {
            foreach (string support_name in Data.Supports)
                if (Global.data_supports[support_name].Id1 == actorId || Global.data_supports[support_name].Id2 == actorId)
                    return Global.data_supports[support_name].Supports;
            return null;
        }

        /// <summary>
        /// Gets the support convo for this actor and another
        /// </summary>
        /// <param name="actorId">Id of the other actor</param>
        /// <param name="at_base">Is this convo taking place at the home base, as opposed to on the battlefield</param>
        /// <returns></returns>
        public string support_convo(int actorId, bool at_base)
        {
            if (can_support(actorId))
                if (!is_support_level_maxed(actorId))
                    return at_base ? support_data(actorId)[get_support_level(actorId)].Base_Convo :
                        support_data(actorId)[get_support_level(actorId)].Field_Convo;
            return "";
        }

        #region Support Progress
        public void new_turn_support_gain(int actorId, int dist)
        {
            new_turn_support_gain(actorId, dist, false);
        }
        public void new_turn_support_gain(int actorId, int dist, bool rescuing)
        {
            int points = rescuing ? Constants.Support.RESCUE_SUPPORT_POINTS :
                support_distance_bonus(dist);
            if (points != 0)
            {
                try_support_gain(actorId, points);
            }
        }

        public void same_target_support_gain(int actorId)
        {
            try_support_gain(actorId, Constants.Support.SAME_TARGET_SUPPORT_POINTS);
        }

        public void heal_support_gain(int actorId)
        {
            try_support_gain(actorId, Constants.Support.HEAL_SUPPORT_POINTS);
        }

        public void chapter_support_gain(int actorId)
        {
            try_support_gain(actorId, Constants.Support.CHAPTER_SUPPORT_POINTS);
        }

        public void talk_support_gain(int actorId)
        {
            try_support_gain(actorId, Constants.Support.TALK_SUPPORT_POINTS);
        }

        /// <summary>
        /// Actually adds support points for this actor and another actor, if possible.
        /// </summary>
        /// <param name="actorId">Id of the other actor</param>
        /// <param name="points">Support points to gain</param>
        internal void try_support_gain(int actorId, int points)
        {
            if (Global.game_state.is_support_blocked(Id, actorId) || points == 0)
                return;
            if (can_support(actorId))
                // If the support is below the max level
                if (!is_support_level_maxed(actorId))
                {
                    // Edited to allow the progress to go over the required value, in case the required value is changed for any reason //Debug
                    //Support_Progress[actor_id] = Math.Min(needed_support_points(actor_id), Support_Progress[actor_id] + points);

                    if (needed_support_points(actorId) == -1)
                        Support_Progress[actorId] = -1;
                    else
                        // Adds points
                        Support_Progress[actorId] = Math.Min(Constants.Support.MAX_SUPPORT_POINTS, Support_Progress[actorId] + points);
                }
        }

        private int support_distance_bonus(int dist)
        {
            return Math.Max(0, Constants.Support.ADJACENT_SUPPORT_POINTS - (dist - 1));
        }
        #endregion

        public int get_support_level(int actorId)
        {
            if (Supports.ContainsKey(actorId))
                return Supports[actorId];
            return 0;
        }

        public bool is_support_level_maxed(int actorId)
        {
            return get_support_level(actorId) >= support_data(actorId).Count;
        }

        public float get_support_bonus(int actorId, Combat_Stat_Labels stat)
        {
            return get_support_bonus(actorId, (int)stat);
        }
        private float get_support_bonus(int actorId, int type)
        {
            float n = 0;
            // Support
            if (Global.game_actors.ContainsKey(actorId) && Supports.ContainsKey(actorId))
                n += (support_bonuses(type) + Global.game_actors[actorId].support_bonuses(type)) * Supports[actorId];
            // Bond
            if (Global.game_actors.ContainsKey(actorId) && Bond == actorId)
                n += Constants.Support.BOND_BOOSTS[type];
            return n;
        }

        public int support_bonus_from_next_level(int actorId, Combat_Stat_Labels stat)
        {
            int support_level = Supports.ContainsKey(actorId) ? Supports[actorId] : 0;
            float bonus = (support_bonuses((int)stat) +
                Global.game_actors[actorId].support_bonuses((int)stat)) *
                support_level;
            float next_bonus = (support_bonuses((int)stat) +
                Global.game_actors[actorId].support_bonuses((int)stat)) *
                (support_level + 1);
            return (int)(next_bonus - bonus);
        }

        public int total_support_bonus(Combat_Stat_Labels stat)
        {
            IEnumerable<int> support_partners;
            if (Global.game_system.home_base)
                support_partners = new HashSet<int>(Global.battalion.actors.Where(actor_id => actor_id != Id));
            else
                support_partners = Supports.Keys.ToList();

            float n = 0;
            foreach (int actor_id in support_partners)
                n += get_support_bonus(actor_id, stat);
            return (int)Math.Floor(n);
        }

        public float support_bonuses(int type)
        {
            return Constants.Support.AFFINITY_BOOSTS[affin][type];
        }

        public void increase_support_level(int actorId)
        {
            // Already over limit?
            if (is_support_maxed())
                return;
            // Add an entry for this support if there isn't one already
            if (!Supports.ContainsKey(actorId))
                Supports.Add(actorId, 0);
            // Already maxed with this person?
            if (Supports[actorId] >= Constants.Support.MAX_SUPPORT_LEVEL)
                return;
            if (Support_Progress.ContainsKey(actorId))
                Support_Progress[actorId] = 0;
            Supports[actorId]++;
        }

        public void set_bond_partner(int actorId)
        {
            // Cancel the old bond
            if (Bond != -1)
            {
                var old_partner = Global.game_actors[Bond];
                if (old_partner.bond == Id)
                    old_partner.Bond = -1;
            }

            Bond = actorId;
        }

        /// <summary>
        /// Returns the number of support points this actor currently has used.
        /// </summary>
        /// <param name="ignore_reserved">If false, any reserved supports for this actor count as a max rank support worth of points.</param>
        /// <param name="reserved_id">If ignore_reserved, a reserved support with an actor with this id will be treated as non-reserved.</param>
        /// <returns></returns>
        public int support_count(bool ignore_reserved = false, int reserved_id = -1)
        {
            if (ignore_reserved)
                return Supports.Values.Sum();
            else
            {
                int supports = Support_Progress.Select(x =>
                        {
                            if (x.Key != reserved_id &&
                                    Constants.Support.RESERVED_SUPPORTS.ContainsKey(x.Key) &&
                                    Constants.Support.RESERVED_SUPPORTS[x.Key].Contains(Id))
                                return Constants.Support.MAX_SUPPORT_LEVEL;
                            else
                                return Supports.ContainsKey(x.Key) ? Supports[x.Key] : 0;
                        })
                    .Sum();
                supports += Supports.Where(x => !Support_Progress.ContainsKey(x.Key)).Sum(x => x.Value);
                return supports;
            }
        }
        public int supports_remaining { get { return Constants.Support.SUPPORT_TOTAL - support_count(); } }
        public bool is_support_maxed(bool ignore_reserved = true, int reserved_id = -1)
        {
            return support_count(ignore_reserved, reserved_id) >=
                Constants.Support.SUPPORT_TOTAL;
        }
        #endregion

        public bool is_dead()
        {
            return Hp <= 0;
        }

        public bool is_out_of_lives()
        {
            return Lives == 0;
        }

        public bool is_full_hp()
        {
            return Hp >= maxhp;
        }

        public void recover_hp()
        {
            Hp = maxhp;
        }

        public void recover_all(bool negative_only = false)
        {
            recover_hp();

            if (negative_only)
                States = States.Where(x => !Global.data_statuses[x[0]].Negative).ToList();
            else
                States.Clear();
            Skills_Need_Updated = true;
        }

        public int individual_animation
        {
            get { return Global.game_battalions.individual_animations[Id]; }
            set
            {
                Global.game_battalions.individual_animations[Id] =
                    (value + ((int)Constants.Animation_Modes.Map + 1)) % ((int)Constants.Animation_Modes.Map + 1);
            }
        }

        #region Status Effects
        /// <summary>
        /// Decrements turn counter on status effects.
        /// </summary>
        public void update_states()
        {
            for (int i = 0; i < States.Count; i++)
                if (States[i][1] > 0)
                    States[i][1]--;
        }

        /// <summary>
        /// Removes states that had their timers run out.
        /// Returns true if a negative state was removed.
        /// </summary>
        /// <returns></returns>
        public bool clear_updated_states()
        {
            int i = 0;
            bool result = false;
            while (i < States.Count)
            {
                // If there are no turns left on a state and it doesn't last indefinitely
                if (States[i][1] <= 0 && Global.data_statuses[States[i][0]].Turns > -1)
                {
                    if (Global.data_statuses[States[i][0]].Negative)
                        result = true;
                    States.RemoveAt(i);
                }
                else
                    i++;
            }
            Skills_Need_Updated = true;
            if (result && weapon == null)
            {
                setup_items(false);
            }
            return result;
        }

        public void add_state(int id)
        {
            // If the state already exists, remove it before re-adding it
            // Do this with linq though? //Debug
            int j = 0;
            while (j < States.Count)
            {
                if (id == States[j][0])
                    States.RemoveAt(j);
                else
                    j++;
            }
            States.Add(new int[] { id, Global.data_statuses[id].Turns });
            Skills_Need_Updated = true;
        }

        public void remove_state(int id)
        {
            int j = 0;
            while (j < States.Count)
            {
                if (id == States[j][0])
                    States.RemoveAt(j);
                else
                    j++;
            }
            Skills_Need_Updated = true;
        }

        public bool has_damaging_status()
        {
            foreach (int id in states)
                if (Global.data_statuses[id].Damage_Per_Turn > 0)
                    return true;
            return false;
        }

        public int status_damage()
        {
            float damage = 0;
            foreach (int id in states)
                if (Global.data_statuses[id].Damage_Per_Turn > 0)
                    damage += Global.data_statuses[id].Damage_Per_Turn * maxhp;
            return -(int)Math.Max(damage, 1);
        }

        public int damaging_status_effect_id()
        {
            foreach (int id in states)
                if (Global.data_statuses[id].Damage_Per_Turn > 0)
                    return id;
            return -1;
        }

        public bool silenced
        {
            get { return states.Any(x => Global.data_statuses[x].No_Magic); }
        }

        public void store_states()
        {
            Temp_States = new List<int[]>();
            Temp_States.AddRange(States);
        }

        public void restore_states()
        {
            if (Temp_States != null)
                States = Temp_States;
            Temp_States = null;
            Skills_Need_Updated = true;
        }

        public int state_turns_left(int id)
        {
            int j = 0;
            while (j < States.Count)
            {
                if (id == States[j][0])
                    return States[j][1];
                else
                    j++;
            }
            return 0;
        }
        #endregion
    }
}