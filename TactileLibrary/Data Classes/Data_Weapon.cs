using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using ArrayExtension;

namespace TactileLibrary
{
    //public enum Weapon_Types { None, Sword, Lance, Axe, Bow, Fire, Thunder, Wind, Light, Dark, Staff } //Debug
    public enum Weapon_Ranks { None, E, D, C, B, A, S }
    public enum Weapon_Traits { Thrown, Reaver, Brave, Cursed, Doubles_WTA,
		Hits_All_in_Range, Ballista, Ignores_Pow, Drains_HP, Ignores_Def, Halves_HP }
    public enum Stave_Traits { Heals, Torch, Unlock, Repair, Barrier, Rescue, Warp }
    public enum Attack_Types { Physical, Magical, Magic_At_Range }
    public class Data_Weapon : Data_Equipment
    {
        internal static IWeaponTypeService WeaponTypeData { get; private set; }
        public static IWeaponTypeService weapon_type_data
        {
            get { return WeaponTypeData; }
            set
            {
                if (WeaponTypeData == null)
                    WeaponTypeData = value;
            }
        }

        public readonly static int[] WLVL_THRESHOLDS = new int[] { 0, 1, 31, 71, 121, 181, 251 };
        public readonly static string[] WLVL_LETTERS = new string[] { "-", "E", "D", "C", "B", "A", "S" };
        //public readonly static int[] ANIMA_TYPES = { (int)Weapon_Types.Fire, (int)Weapon_Types.Thunder, (int)Weapon_Types.Wind }; //Debug
        public readonly static int[] ANIMA_TYPES = { 5, 6, 7 };

        public int Mgt = 0;
        public int Hit = 0;
        public int Crt = 0;
        public int Wgt = 0;
        public int Min_Range = 1;
        public int Max_Range = 1;
        public bool Mag_Range = false;
        public bool No_Counter = false;
        public bool Long_Range = false;
        public int Main_Type = 1; //public Weapon_Types Main_Type = Weapon_Types.Sword; //Debug
        public int Scnd_Type = 0;  //public Weapon_Types Scnd_Type = Weapon_Types.None; //Debug
        public Weapon_Ranks Rank = Weapon_Ranks.E;
        public Attack_Types Attack_Type = Attack_Types.Physical;
        public int WExp = 1;
        public int Staff_Exp = 0;
        [ContentSerializerIgnore]
        public bool[] Traits = new bool[] { false, false, false, false, false, false, false, false, false, false };
        [ContentSerializerIgnore]
        public bool[] Staff_Traits = new bool[] { false, false, false, false, false, false, false };
        public int[] Effectiveness;

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static Data_Weapon GetEmptyInstance()
        {
            return new Data_Weapon();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var weapon = (Data_Weapon)other;

            copy_traits(weapon);

            Mgt = weapon.Mgt;
            Hit = weapon.Hit;
            Crt = weapon.Crt;
            Wgt = weapon.Wgt;
            Min_Range = weapon.Min_Range;
            Max_Range = weapon.Max_Range;
            Mag_Range = weapon.Mag_Range;
            No_Counter = weapon.No_Counter;
            Long_Range = weapon.Long_Range;
            Main_Type = weapon.Main_Type;
            Scnd_Type = weapon.Scnd_Type;
            Rank = weapon.Rank;
            Attack_Type = weapon.Attack_Type;
            WExp = weapon.WExp;
            Staff_Exp = weapon.Staff_Exp;
            Traits = new bool[weapon.Traits.Length];
            Array.Copy(weapon.Traits, Traits, Traits.Length);
            Staff_Traits = new bool[weapon.Staff_Traits.Length];
            Array.Copy(weapon.Staff_Traits, Staff_Traits, Staff_Traits.Length);
            Effectiveness = new int[weapon.Effectiveness.Length];
            Array.Copy(weapon.Effectiveness, Effectiveness, Effectiveness.Length);
        }

        public static Data_Weapon ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader reader)
        {
            read_equipment(reader);

            Mgt = reader.ReadInt32();
            Hit = reader.ReadInt32();
            Crt = reader.ReadInt32();
            Wgt = reader.ReadInt32();
            Min_Range = reader.ReadInt32();
            Max_Range = reader.ReadInt32();
            Mag_Range = reader.ReadBoolean();
            No_Counter = reader.ReadBoolean();
            Long_Range = reader.ReadBoolean();
            Main_Type = reader.ReadInt32();
            Scnd_Type = reader.ReadInt32();
            Rank = (Weapon_Ranks)reader.ReadInt32();
            Attack_Type = (Attack_Types)reader.ReadInt32();
            WExp = reader.ReadInt32();
            Staff_Exp = reader.ReadInt32();
            Traits = Traits.read(reader);
            Staff_Traits = Staff_Traits.read(reader);
            Effectiveness = Effectiveness.read(reader);
        }

        public override void Write(BinaryWriter output)
        {
            base.write_equipment(output);

            output.Write(Mgt);
            output.Write(Hit);
            output.Write(Crt);
            output.Write(Wgt);
            output.Write(Min_Range);
            output.Write(Max_Range);
            output.Write(Mag_Range);
            output.Write(No_Counter);
            output.Write(Long_Range);
            output.Write(Main_Type);
            output.Write(Scnd_Type);
            output.Write((int)Rank);
            output.Write((int)Attack_Type);
            output.Write(WExp);
            output.Write(Staff_Exp);
            Traits.write(output);
            Staff_Traits.write(output);
            Effectiveness.write(output);
        }

        [ContentSerializer(ElementName = "Traits", CollectionItemName = "Trait")]
        private string[] TraitsAsStrings
        {
            get
            {
                return EnumSerializer
                    .BoolArrayToEnumStrings<Weapon_Traits>(Traits);
            }
            set
            {
                Traits = EnumSerializer
                    .BoolArrayFromEnumStrings<Weapon_Traits>(value);
            }
        }

        [ContentSerializer(ElementName = "Staff_Traits", CollectionItemName = "Trait")]
        private string[] StaffTraitsAsStrings
        {
            get
            {
                return EnumSerializer
                    .BoolArrayToEnumStrings<Stave_Traits>(Staff_Traits);
            }
            set
            {
                Staff_Traits = EnumSerializer
                    .BoolArrayFromEnumStrings<Stave_Traits>(value);
            }
        }
        #endregion

        public override string ToString()
        {
            return ToString(0);
        }
        public override string ToString(int uses_left)
        {
            return String.Format("Weapon: {0}, Mgt {1}, Uses {2}",
                full_name(), Mgt, uses_left == 0 ? Uses.ToString() : string.Format("{0}/{1}", uses_left, Uses));
        }

        public Data_Weapon() : this(11) { }
        public Data_Weapon(int effectiveness_count)
        {
            Effectiveness = new int[effectiveness_count];
            for (int i = 0; i < effectiveness_count; i++)
                Effectiveness[i] = 1;
        }
        public Data_Weapon(Data_Weapon weapon)
        {
            CopyFrom(weapon);
        }

        public override bool is_weapon { get { return true; } }

        public string type { get { return main_type().StatusHelpName; } }

        public string rank
        {
            get
            {
                if ((int)Rank == 0) return "Prf";
                return WLVL_LETTERS[(int)Rank];
            }
        }

        public bool is_staff()
        {
            return main_type().IsStaff;
        }

        public bool is_attack_staff()
        {
            return is_staff() && Attack_Type == Attack_Types.Magical;
        }

        public bool is_magic()
        {
            return main_type().IsMagic || scnd_type().IsMagic;
        }

        public bool is_always_magic()
        {
            return is_magic() && Attack_Type == Attack_Types.Magical;
        }

        public bool is_ranged_magic()
        {
            return is_magic() && Attack_Type == Attack_Types.Magic_At_Range;
        }

        public bool is_imbued()
        {
            return !main_type().IsMagic || scnd_type().IsMagic;
            //int num = PHYSICAL_TYPES; //Debug
            //return ((int)Main_Type <= num && (int)Scnd_Type > num);
        }

        public bool blocked_by_silence
        {
            get
            {
                return main_type().IsMagic || main_type().IsStaff || is_always_magic();
            }
        }

        public bool imbue_range_reduced_by_silence
        {
            get
            {
                if (blocked_by_silence)
                    return false;
                return is_ranged_magic();
            }
        }

        public WeaponType main_type()
        {
            if (WeaponTypeData != null)
                return WeaponTypeData.type(Main_Type);
            return null;
        }
        public WeaponType scnd_type()
        {
            if (WeaponTypeData != null)
                return WeaponTypeData.type(Scnd_Type);
            return null;
        }

        public int HitsPerAttack
        {
            get
            {
                if (Brave())
                    return 2;
                return 1;
            }
        }

        /// <summary>
        /// Gets the Mgt of a weapon, but penalizes for each non-standard effect,
        /// for attempting to find iron/steel/silver/etc
        /// </summary>
        public int vanilla_value()
        {
            int result_mgt = Mgt;
            result_mgt += special_effect_value(false);

            return result_mgt;
        }
        /// <summary>
        /// Gets the Mgt of a weapon, but penalizes for each non-standard effect,
        /// for attempting to find thrown and siege
        /// </summary>
        public int ranged_value()
        {
            int result_mgt = Mgt;
            // 15 points for max range is different from min range
            if (Max_Range > Min_Range)
                result_mgt += 15;
            // 3 points times max range
            result_mgt += Max_Range * 3;

            return result_mgt;
        }
        /// <summary>
        /// Gets the Mgt of a weapon, but penalizes for each non-standard effect,
        /// for attempting to find slayers, brave, killer, etc
        /// </summary>
        public int effect_value()
        {
            int result_mgt = Mgt;
            result_mgt += special_effect_value(true);

            return result_mgt;
        }

        private int special_effect_value(bool positive)
        {
            int result = 0;
            // 1 point for each 5 crit
            result += Crt / 5;
            if (!positive)
            {
                // 4 points for 1-2 range
                if (Max_Range > Min_Range)
                    result += 4;
            }
            // 5 points for no counter
            if (No_Counter)
                result += 5;
            // 8 points for having a second type
            if (Scnd_Type != 0)
                result += 8;
            // 3 points for more than 1 WExp
            result += Math.Abs(WExp - 1) * 3;
            // Points for each trait
            for (int i = 0; i < Traits.Length; i++)
                if (Traits[i])
                    switch ((Weapon_Traits)i)
                    {
                        case Weapon_Traits.Reaver:
                        case Weapon_Traits.Ignores_Pow:
                        case Weapon_Traits.Halves_HP:
                            result += 10;
                            break;
                        case Weapon_Traits.Brave:
                        case Weapon_Traits.Drains_HP:
                        case Weapon_Traits.Hits_All_in_Range:
                            result += 15;
                            break;
                        case Weapon_Traits.Ignores_Def:
                            result += 20;
                            break;
                        default:
                            result += 3;
                            break;
                    }
            if (positive || !is_prf)
            {
                // 8 points for each slayer bonus
                for (int i = 0; i < Effectiveness.Length; i++)
                    result += Math.Abs(Effectiveness[i] - 1) * 8;
                // 10 points for each skill
                result += Skills.Count * 10;
            }
            // 5 points for each status
            result += Status_Inflict.Count * 5;

            return positive ? result : -result;
        }
        
        /// <summary>
        /// Gets the healing value of a staff, with a benefit to increased range
        /// </summary>
        public int healing_value()
        {
            if (!Heals())
                return 0;
            
            int result_mgt = Math.Min(40, Mgt);
            // 5 points for max range is different from min range
            if (Max_Range > Min_Range)
                result_mgt += 5;
            // 2 points times max range
            result_mgt += Max_Range * 2;
            // 20 points for Mag/2 range
            if (Mag_Range)
                result_mgt += 20;
            // 20 points for hitting all targets
            if (Hits_All_in_Range())
                result_mgt += 20;
            // 5 points for each status removed
            result_mgt += Status_Remove.Count * 5;

            return result_mgt;
        }
        /// <summary>
        /// Gets the effectiveness as a status staff, largely as a function of rank
        /// </summary>
        public int status_value()
        {
            if (!is_attack_staff())
                return 0;

            int result_mgt = (int)Rank * 5;
            // 5 points for max range is different from min range
            if (Max_Range > Min_Range)
                result_mgt += 5;
            // 2 points times max range
            result_mgt += Max_Range * 2;
            // 20 points for Mag/2 range
            if (Mag_Range)
                result_mgt += 20;
            // 20 points for hitting all targets
            if (Hits_All_in_Range())
                result_mgt += 20;

            return result_mgt;
        }
        /// <summary>
        /// Gets the value of a utility staff, basically its rank
        /// </summary>
        public int utility_value()
        {
            if (Heals() || is_attack_staff())
                return 0;

            int result_mgt = (int)Rank * 5;

            return result_mgt;
        }

        #region Traits
        public bool Thrown() { return Traits[(int)Weapon_Traits.Thrown]; }
        public bool Reaver() { return Traits[(int)Weapon_Traits.Reaver]; }
        public bool Brave() { return Traits[(int)Weapon_Traits.Brave]; }
        public bool Cursed() { return Traits[(int)Weapon_Traits.Cursed]; }
        public bool DoubledWeaponTri() { return Traits[(int)Weapon_Traits.Doubles_WTA]; }
        public bool Hits_All_in_Range() { return Traits[(int)Weapon_Traits.Hits_All_in_Range]; }
        public bool Ballista() { return Traits[(int)Weapon_Traits.Ballista]; }
        public bool Ignores_Pow() { return Traits[(int)Weapon_Traits.Ignores_Pow]; }
        public bool Drains_HP() { return Traits[(int)Weapon_Traits.Drains_HP]; }
        public bool Ignores_Def() { return Traits[(int)Weapon_Traits.Ignores_Def]; }
        public bool Halves_HP() { return Traits[(int)Weapon_Traits.Halves_HP]; }
        #endregion

        #region Staff_Traits
        public bool Heals() { return Staff_Traits[(int)Stave_Traits.Heals]; }
        public bool Torch() { return Staff_Traits[(int)Stave_Traits.Torch]; }
        public bool Unlock() { return Staff_Traits[(int)Stave_Traits.Unlock]; }
        public bool Repair() { return Staff_Traits[(int)Stave_Traits.Repair]; }
        public bool Barrier() { return Staff_Traits[(int)Stave_Traits.Barrier]; }
        public bool Rescue() { return Staff_Traits[(int)Stave_Traits.Rescue]; }
        public bool Warp() { return Staff_Traits[(int)Stave_Traits.Warp]; }
        #endregion

        public override object Clone()
        {
            return new Data_Weapon(this);
        }
    }

    public interface IWeaponTypeService
    {
        WeaponType type(int key);
    }
}
