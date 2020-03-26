using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using ArrayExtension;
using ListExtension;

namespace FEXNA_Library
{
    public class Data_Terrain : IFEXNADataContent
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

        #region IFEXNADataContent
        public IFEXNADataContent Read_Content(ContentReader input)
        {
            Data_Terrain result = new Data_Terrain();
            
            result.Id = input.ReadInt32();
            result.Name = input.ReadString();

            result.Avoid = input.ReadInt32();
            result.Def = input.ReadInt32();
            result.Res = input.ReadInt32();
            result.Stats_Visible = input.ReadBoolean();

            result.Step_Sound_Group = input.ReadInt32();
            result.Platform_Rename = input.ReadString();
            result.Background_Rename = input.ReadString();
            result.Dust_Type = input.ReadInt32();
            result.Fire_Through = input.ReadBoolean();

            result.Move_Costs = result.Move_Costs.read(input);
            bool healHasValue = input.ReadBoolean();
            result.Heal = healHasValue ? new int[0].read(input) : null;
            result.Minimap = input.ReadInt32();
            result.Minimap_Group.read(input);

            return result;
        }

        public void Write(BinaryWriter output)
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
        public object Clone()
        {
            return new Data_Terrain(this);
        }
        #endregion
    }
}
