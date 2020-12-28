using System.Collections.Generic;
using System.IO;
using ListExtension;

namespace TactileLibrary
{
    public class Data_Support
    {
        public string Key;
        public int Id1, Id2;
        public List<Support_Entry> Supports = new List<Support_Entry>();

        #region Serialization
        public static Data_Support read(BinaryReader reader)
        {
            Data_Support data = new Data_Support();
            data.Key = reader.ReadString();
            data.Id1 = reader.ReadInt32();
            data.Id2 = reader.ReadInt32();
            data.Supports.read(reader);
            return data;
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(Key);
            writer.Write(Id1);
            writer.Write(Id2);
            Supports.write(writer);
        }
        #endregion

        public Data_Support() { }
        public Data_Support(Data_Support data)
        {
            Key = data.Key;
            Id1 = data.Id1;
            Id2 = data.Id2;
            Supports = new List<Support_Entry>(data.Supports);
        }

        public override string ToString()
        {
            return string.Format("Data_Support: {0} ({1}, {2})", Key, Id1, Id2);
        }

        public int MaxLevel
        {
            get
            {
                for (int i = Supports.Count - 1; i >= 0; i--)
                    if (Supports[i].ValidSupport)
                        return i + 1;
                return 0;
            }
        }
    }

    public struct Support_Entry
    {
        public int Turns;
        public string Field_Convo;
        public string Base_Convo;

        #region Serialization
        public static Support_Entry read(BinaryReader reader)
        {
            Support_Entry data = new Support_Entry();
            data.Turns = reader.ReadInt32();
            data.Field_Convo = reader.ReadString();
            data.Base_Convo = reader.ReadString();
            return data;
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(Turns);
            writer.Write(Field_Convo);
            writer.Write(Base_Convo);
        }
        #endregion

        public Support_Entry(int turns, string field_convo, string base_convo)
        {
            Turns = turns;
            Field_Convo = field_convo;
            Base_Convo = base_convo;
        }
        public Support_Entry(Support_Entry data)
        {
            Turns = data.Turns;
            Field_Convo = data.Field_Convo;
            Base_Convo = data.Base_Convo;
        }

        public override string ToString()
        {
            return string.Format("Support Entry: {0} Points, ({1}, {2})", Turns, Field_Convo, Base_Convo);
        }

        public bool ValidSupport
        {
            get { return Turns >= 0 || !string.IsNullOrEmpty(Field_Convo) || !string.IsNullOrEmpty(Base_Convo); }
        }

        public bool FieldBaseDifference
        {
            get
            {
                return !string.IsNullOrEmpty(Field_Convo) &&
                  !string.IsNullOrEmpty(Base_Convo) &&
                  Field_Convo != Base_Convo;
            }
        }

        public string ConvoName(bool atBase)
        {
            if (atBase && !string.IsNullOrEmpty(Base_Convo))
                return Base_Convo;
            return Field_Convo;
        }
    }
}
