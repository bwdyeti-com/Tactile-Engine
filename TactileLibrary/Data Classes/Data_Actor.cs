using System;
using System.Collections.Generic;
using System.IO;
using ListExtension;

namespace TactileLibrary
{
    public enum Affinities { Thunder, Dark, Anima, Earth, Fire, Water, Ice, Light, Wind, None }
    public class Data_Actor
    {
        public int Id = 0;
        public string Name = "New Actor";
        public string Description = "";
        public int ClassId = 1;
        public int Level = 1;
        public List<int> BaseStats = new List<int> { 20, 5, 5, 5, 5, 5, 5, 5 };
        public List<int> Growths = new List<int> { 50, 25, 25, 25, 25, 25, 25 };
        public List<int> WLvl = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public int Gender = 0;
        public Affinities Affinity = Affinities.None;
        public List<int[]> Items = new List<int[]>{
            new int[3] { 0, 0, 0 },
            new int[3] { 0, 0, 0 },
            new int[3] { 0, 0, 0 },
            new int[3] { 0, 0, 0 },
            new int[3] { 0, 0, 0 }};
        public List<string> Supports = new List<string>();
        public List<int> Skills = new List<int>();

        #region Serialization
        public static Data_Actor read(BinaryReader reader)
        {
            Data_Actor result = new Data_Actor();
            result.Id = reader.ReadInt32();
            result.Name = reader.ReadString();
            result.Description = reader.ReadString();
            result.ClassId = reader.ReadInt32();
            result.Level = reader.ReadInt32();
            result.BaseStats.read(reader);
            result.Growths.read(reader);
            result.WLvl.read(reader);
            result.Gender = reader.ReadInt32();
            result.Affinity = (Affinities)reader.ReadInt32();
            result.Items.read(reader);
            result.Supports.read(reader);
            result.Skills.read(reader);
            return result;
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Name);
            writer.Write(Description);
            writer.Write(ClassId);
            writer.Write(Level);
            BaseStats.write(writer);
            Growths.write(writer);
            WLvl.write(writer);
            writer.Write(Gender);
            writer.Write((int)Affinity);
            Items.write(writer);
            Supports.write(writer);
            Skills.write(writer);
        }
        #endregion

        public Data_Actor() { }
        public Data_Actor(Data_Actor actor)
        {
            Id = actor.Id;
            Name = actor.Name;
            Description = actor.Description;
            ClassId = actor.ClassId;
            Level = actor.Level;
            BaseStats = new List<int>(actor.BaseStats);
            Growths = new List<int>(actor.Growths);
            WLvl = new List<int>(actor.WLvl);
            Gender = actor.Gender;
            Affinity = actor.Affinity;
            Items = new List<int[]>();
            foreach(int[] item_data in actor.Items)
            {
                int[] ary = new int[item_data.Length];
                Array.Copy(item_data, ary, item_data.Length);
                Items.Add(ary);
            }
            Supports = new List<string>(actor.Supports);
            Skills = new List<int>(actor.Skills);
        }

        public override string ToString()
        {
            return string.Format("Data_Actor: {0}", Name);
        }
        
        public IEnumerable<Tuple<string, int>> SupportPartners(
            Dictionary<string, Data_Support> SupportData,
            Dictionary<int, Data_Actor> Actors)
        {
            foreach (string key in Supports)
            {
                if (!SupportData.ContainsKey(key))
                    continue;
                var supportData = SupportData[key];
                // If not actually valid for this actor
                if (supportData.Id1 != Id && supportData.Id2 != Id)
                    continue;
                int otherActorId = Id == supportData.Id2 ?
                    supportData.Id1 : supportData.Id2;
                // If this isn't another actor, somehow
                if (!Actors.ContainsKey(otherActorId))
                    continue;

                yield return Tuple.Create(key, otherActorId);
            }
        }
    }
}
