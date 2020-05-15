using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ColorExtension;
using ListExtension;

namespace FEXNA_Library
{
    public class Data_Status : IFEXNADataContent
    {
        public int Id = 0;
        public string Name = "";
        public string Description = "";
        public int Turns = 1;
        public bool Negative = false;
        public float Damage_Per_Turn = 0;
        public bool Unselectable = false;
        public bool Ai_Controlled = false;
        public bool Attacks_Allies = false;
        public bool No_Magic = false;
        public List<int> Skills = new List<int> { };
        public int Image_Index = 0;
        public int Map_Anim_Id = -1;
        public Color Battle_Color = new Color(0, 0, 0, 0);

        public Data_Status() { }
        public Data_Status(Data_Status source)
        {
            Id = source.Id;
            Name = source.Name;
            Description = source.Description;

            Turns = source.Turns;
            Negative = source.Negative;
            Damage_Per_Turn = source.Damage_Per_Turn;
            Unselectable = source.Unselectable;
            Ai_Controlled = source.Ai_Controlled;
            Attacks_Allies = source.Attacks_Allies;
            No_Magic = source.No_Magic;
            Skills = new List<int>(source.Skills);

            Image_Index = source.Image_Index;
            Map_Anim_Id = source.Map_Anim_Id;
            Battle_Color = source.Battle_Color;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Id, Name);
        }
        
        #region IFEXNADataContent
        public IFEXNADataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static Data_Status GetEmptyInstance()
        {
            return new Data_Status();
        }

        public static Data_Status ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public void Read(BinaryReader input)
        {
            Id = input.ReadInt32();
            Name = input.ReadString();
            Description = input.ReadString();

            Turns = input.ReadInt32();
            Negative = input.ReadBoolean();
            Damage_Per_Turn = (float)input.ReadDouble();
            Unselectable = input.ReadBoolean();
            Ai_Controlled = input.ReadBoolean();
            Attacks_Allies = input.ReadBoolean();
            No_Magic = input.ReadBoolean();
            Skills.read(input);

            Image_Index = input.ReadInt32();
            Map_Anim_Id = input.ReadInt32();
            Battle_Color = Battle_Color.read(input);
        }

        public void Write(BinaryWriter output)
        {
            output.Write(Id);
            output.Write(Name);
            output.Write(Description);

            output.Write(Turns);
            output.Write(Negative);
            output.Write((double)Damage_Per_Turn);
            output.Write(Unselectable);
            output.Write(Ai_Controlled);
            output.Write(Attacks_Allies);
            output.Write(No_Magic);
            Skills.write(output);

            output.Write(Image_Index);
            output.Write(Map_Anim_Id);
            Battle_Color.write(output);
        }
        #endregion

        #region ICloneable
        public object Clone()
        {
            return new Data_Status(this);
        }
        #endregion
    }
}
