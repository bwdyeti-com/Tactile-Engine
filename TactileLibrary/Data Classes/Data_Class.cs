using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using ArrayExtension;
using DictionaryExtension;
using ListExtension;

namespace TactileLibrary
{
    public enum ClassTypes { None, Infantry, Heavy, Armor, Cavalry, Flier, FDragon, Swordsman, Dragon, Mage, Monster }
    public enum MovementTypes { Light, Heavy, Armor, Mounted, Flying }
    public enum Generic_Builds { Weak, Mid, Normal, Strong }
    public class Data_Class
    {
        internal static IClassService Class_Data { get; private set; }
        public static IClassService class_data
        {
            get { return Class_Data; }
            set
            {
                if (Class_Data == null)
                    Class_Data = value;
            }
        }

        public readonly static List<int> GENERIC_CAPS = new List<int> { 80, 20, 20, 20, 20, 20, 20 };

        public int Id;
        public string Name;
        public List<ClassTypes> Class_Types;
        public List<Data_Class_Skill> Skills;
        public string Description;
        public List<int>[] Caps;
        public List<int> Max_WLvl;
        public Dictionary<int, List<int>[]> Promotion;
        public int Tier = 0;
        public int Mov;
        public int Mov_Cap;
        public MovementTypes Movement_Type;
        public List<List<int>[]> Generic_Stats;

        #region Accessors
        public string name
        {
            get
            {
                string[] ary = Name.Split(new char[] { '_' });
                return ary[0];
            }
        }

        public IEnumerable<int> promotion_keys { get { return Promotion.Keys; } }
        #endregion

        #region Serialization
        public static Data_Class read(BinaryReader reader)
        {
            Data_Class result = new Data_Class();
            result.Id = reader.ReadInt32();
            result.Name = reader.ReadString();
            result.Class_Types.read(reader);
            result.Skills.read(reader);
            result.Description = reader.ReadString();
            bool own_caps = reader.ReadBoolean();
            if (own_caps)
                result.Caps = result.Caps.read(reader);
            else
                result.Caps = null;
            result.Max_WLvl.read(reader);
            result.Promotion.read(reader);
            result.Tier = reader.ReadInt32();
            result.Mov = reader.ReadInt32();
            result.Mov_Cap = reader.ReadInt32();
            result.Movement_Type = (MovementTypes)reader.ReadInt32();
            result.Generic_Stats.read(reader);
            return result;
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Name);
            Class_Types.write(writer);
            Skills.write(writer);
            writer.Write(Description);
            writer.Write(Caps != null);
            if (Caps != null)
                Caps.write(writer);
            Max_WLvl.write(writer);
            Promotion.write(writer);
            writer.Write(Tier);
            writer.Write(Mov);
            writer.Write(Mov_Cap);
            writer.Write((int)Movement_Type);
            Generic_Stats.write(writer);
        }
        #endregion

        public Data_Class()
        {
            Id = 0;
            Name = "";
            Class_Types = new List<ClassTypes>();
            Skills = new List<Data_Class_Skill>();
            Description = "";
            Caps = new List<int>[] { new List<int>(GENERIC_CAPS), new List<int>(GENERIC_CAPS) };
            Max_WLvl = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Promotion = new Dictionary<int, List<int>[]>();
            Mov = 5;
            Mov_Cap = 15;
            Movement_Type = MovementTypes.Light;
            Generic_Stats = new List<List<int>[]>
            {
                new List<int>[2] {new List<int>{0, 0, 0, 0, 0, 0, 0, 0},new List<int>{0,0,0,0,0,0,0}},
                new List<int>[2] {new List<int>{0, 0, 0, 0, 0, 0, 0, 0},new List<int>{0,0,0,0,0,0,0}},
                new List<int>[2] {new List<int>{0, 0, 0, 0, 0, 0, 0, 0},new List<int>{0,0,0,0,0,0,0}},
                new List<int>[2] {new List<int>{0, 0, 0, 0, 0, 0, 0, 0},new List<int>{0,0,0,0,0,0,0}}
            };
        }
        public Data_Class(Data_Class data)
        {
            Id = data.Id;
            Name = data.Name;
            Class_Types = new List<ClassTypes>(data.Class_Types);
            Skills = new List<Data_Class_Skill>(data.Skills);
            Description = data.Description;
            if (data.Caps == null)
                Caps = null;
            else
            {
                Caps = new List<int>[data.Caps.Length];
                for (int i = 0; i < Caps.Length; i++)
                    Caps[i] = new List<int>(data.Caps[i]);
            }
            Max_WLvl = new List<int>(data.Max_WLvl);
            Promotion = new Dictionary<int, List<int>[]>();
            foreach (KeyValuePair<int, List<int>[]> pair in data.Promotion)
            {
                Promotion.Add(pair.Key, new List<int>[pair.Value.Length]);
                for (int i = 0; i < pair.Value.Length; i++)
                    Promotion[pair.Key][i] = new List<int>(pair.Value[i]);
            }
            Tier = data.Tier;
            Mov = data.Mov;
            Mov_Cap = data.Mov_Cap;
            Movement_Type = data.Movement_Type;
            Generic_Stats = new List<List<int>[]>();
            foreach (List<int>[] stats in data.Generic_Stats)
            {
                Generic_Stats.Add(new List<int>[stats.Length]);
                for (int i = 0; i < stats.Length; i++)
                    Generic_Stats[Generic_Stats.Count - 1][i] = new List<int>(stats[i]);
            }
        }

        public List<int> caps()
        {
            return caps(0);
        }
        public List<int> caps(int gender)
        {
            if (Caps == null)
                return GENERIC_CAPS;
            return Caps[gender % 2];
        }

        public bool can_promote()
        {
            return Promotion.Any();
        }
        public bool can_promote(int class_id)
        {
            return Promotion.ContainsKey(class_id);
        }

        public List<int> promotion_bonuses(int class_id)
        {
            if (Class_Data != null && Class_Data.get_class(class_id) != null)
            {
                var promotion_class = Class_Data.get_class(class_id);
                return Enumerable.Range(0, Generic_Stats[(int)Generic_Builds.Normal][0].Count)
                    .Select(x => promotion_class.Generic_Stats[(int)Generic_Builds.Normal][0][x] - Generic_Stats[(int)Generic_Builds.Normal][0][x])
                    .ToList();
            }
            return null;
        }
        public List<int> promotion_wlvl_bonuses(int class_id)
        {
            if (!Promotion.ContainsKey(class_id))
                return null;
            return Promotion[class_id][1];
        }

        /// <summary>
        /// Checks the Prf data for an item and only returns true if the class check passes
        /// </summary>
        /// <param name="item">Item to check</param>
        public bool prf_check(Data_Equipment item)
        {
            // Block use if the class/type are on any of the disabled lists
            if (item.Prf_Class.Any())
                if (item.Prf_Class.Contains(-Id)) return false;
            if (item.Prf_Type.Any())
                foreach (ClassTypes type in Class_Types)
                    if (item.Prf_Type.Contains(-(int)type))
                        return false;

            // If there are any positive class/type prf requirements and not just negative ones
            bool prf_required = false;
            if (item.Prf_Class.Any())
            {
                if (item.Prf_Class.Contains(Id)) return true;
                // If there are any prf classes, this class must succeed one of the other tests or this is unusable
                for (int i = 0; i < item.Prf_Class.Count; i++)
                    if (item.Prf_Class[i] > 0)
                    {
                        prf_required = true;
                        break;
                    }
            }
            if (item.Prf_Type.Any())
            {
                foreach (ClassTypes type in Class_Types)
                    if (item.Prf_Type.Contains((int)type))
                        return true;
                // If there are any prf types, this class must succeed one of the other tests or this is unusable (but there are no more tests)
                for (int i = 0; i < item.Prf_Type.Count; i++)
                    if (item.Prf_Type[i] > 0)
                    {
                        prf_required = true;
                        break;
                    }

            }
            return !prf_required;
        }

        public override string ToString()
        {
            return string.Format("Data_Class: {0}", Name);
        }
    }

    public struct Data_Class_Skill
    {
        [ContentSerializer]
        public int Level { get; private set; }
        [ContentSerializer]
        public int SkillId { get; private set; }

        #region Serialization
        public static Data_Class_Skill read(BinaryReader reader)
        {
            int level = reader.ReadInt32();
            int skill_id = reader.ReadInt32();

            Data_Class_Skill result = new Data_Class_Skill(level, skill_id);
            return result;
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(Level);
            writer.Write(SkillId);
        }
        #endregion

        public Data_Class_Skill(int level, int skill_id) : this()
        {
            Level = level;
            SkillId = skill_id;
        }

        /// <summary>
        /// Returns a copy of this object with the level value changed
        /// </summary>
        /// <param name="new_level">New level the skill is gained</param>
        public Data_Class_Skill change_level(int new_level)
        {
            return new Data_Class_Skill(new_level, SkillId);
        }

        /// <summary>
        /// Returns a copy of this object with the skill value changed
        /// </summary>
        /// <param name="new_skill_id">New skill value</param>
        public Data_Class_Skill change_id(int new_skill_id)
        {
            return new Data_Class_Skill(Level, new_skill_id);
        }
    }

    public interface IClassService
    {
        Data_Class get_class(int id);
    }
}
