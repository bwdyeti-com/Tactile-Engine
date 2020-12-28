using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using TactileContentExtension;

namespace TactileLibrary.Battler
{
    public class Battle_Animation_Association_Set : TactileDataContent
    {
        public string Key;
        public Dictionary<int, Battle_Animation_Association_Data> DataSet;

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static Battle_Animation_Association_Set GetEmptyInstance()
        {
            return new Battle_Animation_Association_Set();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var source = (Battle_Animation_Association_Set)other;

            Key = source.Key;
            DataSet = source.DataSet
                .ToDictionary(p => p.Key, p => new Battle_Animation_Association_Data(p.Value));
        }

        public static Battle_Animation_Association_Set ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            Key = input.ReadString();
            input.ReadTactileContent(DataSet, Battle_Animation_Association_Data.GetEmptyInstance());
        }

        public override void Write(BinaryWriter output)
        {
            output.Write(Key);
            output.Write(DataSet);
        }
        #endregion

        public Battle_Animation_Association_Set()
        {
            DataSet = new Dictionary<int, Battle_Animation_Association_Data>();
            DataSet.Add(-1, new Battle_Animation_Association_Data());
        }
        public Battle_Animation_Association_Set(Battle_Animation_Association_Set source)
        {
            CopyFrom(source);
        }

        public override string ToString()
        {
            return string.Format("Animation Ids: {0}", Key);
        }

        public void add_data_set(int key)
        {
            DataSet.Add(key, new Battle_Animation_Association_Data());
        }

        public bool remove_data_set_index(int index)
        {
            var keys = DataSet.Keys.OrderBy(x => x).ToList();
            int key = keys[index];
            if (key > -1)
            {
                DataSet.Remove(key);
                return true;
            }
            return false;
        }

        public int key_from_index(int index)
        {
            var keys = DataSet.Keys.OrderBy(x => x).ToList();
            return keys[index];
        }

        public Battle_Animation_Association_Data data_set_from_index(int index)
        {
            var keys = DataSet.Keys.OrderBy(x => x).ToList();
            int key = keys[index];
            return DataSet[key];
        }

        public Battle_Animation_Association_Data anim_set(int id)
        {
            if (DataSet.ContainsKey(id))
                return DataSet[id];
            if (DataSet.ContainsKey(id % 2))
                return DataSet[id % 2];
            else if (DataSet.ContainsKey(-1))
                return DataSet[-1];
            return null;
        }

        public override object Clone()
        {
            return new Battle_Animation_Association_Set(this);
        }
    }
}
