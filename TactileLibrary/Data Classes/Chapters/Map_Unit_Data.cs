using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using DictionaryExtension;
using ListExtension;

namespace TactileLibrary
{
    public class Map_Unit_Data
    {
        public Dictionary<Vector2, Data_Unit> Units = new Dictionary<Vector2, Data_Unit> ();
        public List<Data_Unit> Reinforcements = new List<Data_Unit>();

        #region Serialization
        public void write(BinaryWriter writer)
        {
            Units.write(writer);
            Reinforcements.write(writer);
        }

        public static Map_Unit_Data read(BinaryReader reader)
        {
            Map_Unit_Data unit_data = new Map_Unit_Data();
            unit_data.Units.read(reader);
            unit_data.Reinforcements.read(reader);
            return unit_data;
        }
        #endregion

        public Map_Unit_Data() { }
        public Map_Unit_Data(Map_Unit_Data other)
        {
            Units = other.Units.ToDictionary(p => p.Key, p => new Data_Unit(
                p.Value.type, p.Value.identifier, p.Value.data));
            Reinforcements = other.Reinforcements
                .Select(p => new Data_Unit(
                    p.type, p.identifier, p.data))
                .ToList();
        }

        public override string ToString()
        {
            return string.Format("Map Unit Data: {0} units, {1} reinforcements", Units.Count, Reinforcements.Count);
        }
    }

    public struct Data_Unit
    {
        public string type;
        public string identifier;
        public string data;

        public override string ToString()
        {
            return string.Format("Unit Data: {0}, identifier \"{1}\"", type, identifier);
        }

        public Data_Unit(string type, string identifier, string data)
        {
            this.type = type;
            this.identifier = identifier;
            this.data = data;
        }

        #region Serialization
        public static Data_Unit read(BinaryReader reader)
        {
            return new Data_Unit(reader.ReadString(), reader.ReadString(), reader.ReadString());
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(type);
            writer.Write(identifier);
            writer.Write(data);
        }
        #endregion

        public void reset(Data_Unit unit)
        {
            type = unit.type;
            identifier = unit.identifier;
            data = unit.data;
        }

        public Character_Data character(int numItems, int numWLvls)
        {
            return Character_Data.from_data(
                this.type, this.identifier, this.data,
                numItems, numWLvls);
        }
    }
}
