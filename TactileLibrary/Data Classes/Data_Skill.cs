using System.IO;
using Microsoft.Xna.Framework.Content;

namespace TactileLibrary
{
    public class Data_Skill : TactileDataContent
    {
        public int Id = 0;
        public string Name = "";
        public string Description = "";
        public string Abstract = "";
        public string Image_Name = "";
        public int Image_Index = 0;
        public int Animation_Id = -1;
        public int Map_Anim_Id = -1;

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static Data_Skill GetEmptyInstance()
        {
            return new Data_Skill();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var source = (Data_Skill)other;

            Id = source.Id;
            Name = source.Name;
            Description = source.Description;
            Abstract = source.Abstract;
            Image_Name = source.Image_Name;
            Image_Index = source.Image_Index;
            Animation_Id = source.Animation_Id;
            Map_Anim_Id = source.Map_Anim_Id;
        }

        public static Data_Skill ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            Id = input.ReadInt32();
            Name = input.ReadString();
            Description = input.ReadString();
            Abstract = input.ReadString();
            Image_Name = input.ReadString();
            Image_Index = input.ReadInt32();
            Animation_Id = input.ReadInt32();
            Map_Anim_Id = input.ReadInt32();
        }

        public override void Write(BinaryWriter output)
        {
            output.Write(Id);
            output.Write(Name);
            output.Write(Description);
            output.Write(Abstract);
            output.Write(Image_Name);
            output.Write(Image_Index);
            output.Write(Animation_Id);
            output.Write(Map_Anim_Id);
        }
        #endregion

        public Data_Skill() { }
        public Data_Skill(Data_Skill source)
        {
            CopyFrom(source);
        }

        public override object Clone()
        {
            return new Data_Skill(this);
        }
    }
}
