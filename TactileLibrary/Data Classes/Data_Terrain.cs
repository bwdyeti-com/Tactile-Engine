using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using ArrayExtension;
using ListExtension;

namespace TactileLibrary
{
    public class Data_Terrain : TactileDataContent
    {
        public int Id = 1;
        public string Name = "Plains";
        public int Avoid = 0;
        public int Def = 0;
        public int Res = 0;
        public bool Stats_Visible = true;
        public int Step_Sound_Group = 0;
        public string Platform_Rename = "";
        public string Background_Rename = "";
        public int Dust_Type = 0;
        public bool Fire_Through = false;
        public int[][] Move_Costs = { new int[] { 1, 1, 1, 1, 1 }, new int[] { 1, 1, 1, 1, 1 }, new int[] { 1, 1, 1, 1, 1 } };
        public int[] Heal;
        public int Minimap = 1;
        public List<int> Minimap_Group = new List<int> {};

        public Data_Terrain() { }
        public Data_Terrain(Data_Terrain source)
        {
            CopyFrom(source);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}: stats {2}, {3}, {4}", Id, Name, Avoid, Def, Res);
        }

        public string PlatformName
        {
            get { return string.IsNullOrEmpty(Platform_Rename) ? Name : Platform_Rename; }
        }
        public string BackgroundName
        {
            get { return string.IsNullOrEmpty(Background_Rename) ? Name : Background_Rename; }
        }

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static Data_Terrain GetEmptyInstance()
        {
            return new Data_Terrain();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var source = (Data_Terrain)other;

            Id = source.Id;
            Name = source.Name;

            Avoid = source.Avoid;
            Def = source.Def;
            Res = source.Res;
            Stats_Visible = source.Stats_Visible;

            Step_Sound_Group = source.Step_Sound_Group;
            Platform_Rename = source.Platform_Rename;
            Background_Rename = source.Background_Rename;
            Dust_Type = source.Dust_Type;
            Fire_Through = source.Fire_Through;

            Move_Costs = new int[source.Move_Costs.Length][];
            for (int i = 0; i < Move_Costs.Length; i++)
                Move_Costs[i] = source.Move_Costs[i].ToArray();
            Heal = source.Heal != null ? source.Heal.ToArray() : null;
            Minimap = source.Minimap;
            Minimap_Group = new List<int>(source.Minimap_Group);
        }

        public static Data_Terrain ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            Id = input.ReadInt32();
            Name = input.ReadString();

            Avoid = input.ReadInt32();
            Def = input.ReadInt32();
            Res = input.ReadInt32();
            Stats_Visible = input.ReadBoolean();

            Step_Sound_Group = input.ReadInt32();
            Platform_Rename = input.ReadString();
            Background_Rename = input.ReadString();
            Dust_Type = input.ReadInt32();
            Fire_Through = input.ReadBoolean();

            Move_Costs = Move_Costs.read(input);
            bool healHasValue = input.ReadBoolean();
            Heal = healHasValue ? new int[0].read(input) : null;
            Minimap = input.ReadInt32();
            Minimap_Group.read(input);
        }

        public override void Write(BinaryWriter output)
        {
            output.Write(Id);
            output.Write(Name);

            output.Write(Avoid);
            output.Write(Def);
            output.Write(Res);
            output.Write(Stats_Visible);

            output.Write(Step_Sound_Group);
            output.Write(Platform_Rename);
            output.Write(Background_Rename);
            output.Write(Dust_Type);
            output.Write(Fire_Through);

            Move_Costs.write(output);
            output.Write(Heal != null);
            if (Heal != null)
                Heal.write(output);
            output.Write(Minimap);
            Minimap_Group.write(output);
        }
        #endregion

        #region ICloneable
        public override object Clone()
        {
            return new Data_Terrain(this);
        }
        #endregion
    }
}
