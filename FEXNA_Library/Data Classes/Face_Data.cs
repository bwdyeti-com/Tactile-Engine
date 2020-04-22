using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Vector2Extension;

namespace FEXNA_Library
{
    public class Face_Data : IFEXNADataContent
    {
        public string Name;
        public int Emotions = 0;
        public int Pitch = 100;
        public Vector2 EyesOffset = new Vector2(24, 24);
        public Vector2 MouthOffset = new Vector2(24, 40);
        public Vector2 StatusOffset = new Vector2(8, 0);
        public int StatusFrame = 0;
        public int PlacementOffset = 0;
        public bool ForceEyesClosed = false;
        public bool Asymmetrical = false;
        public bool ClassCard = false;
        
        #region IFEXNADataContent
        public IFEXNADataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static Face_Data GetEmptyInstance()
        {
            return new Face_Data();
        }

        public static Face_Data ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public void Read(BinaryReader input)
        {
            Name = input.ReadString();
            Emotions = input.ReadInt32();
            Pitch = input.ReadInt32();
            EyesOffset = EyesOffset.read(input);
            MouthOffset = MouthOffset.read(input);
            StatusOffset = StatusOffset.read(input);
            StatusFrame = input.ReadInt32();
            PlacementOffset = input.ReadInt32();
            ForceEyesClosed = input.ReadBoolean();
            Asymmetrical = input.ReadBoolean();
            ClassCard = input.ReadBoolean();
        }

        public void Write(BinaryWriter output)
        {
            output.Write(Name);
            output.Write(Emotions);
            output.Write(Pitch);
            EyesOffset.write(output);
            MouthOffset.write(output);
            StatusOffset.write(output);
            output.Write(StatusFrame);
            output.Write(PlacementOffset);
            output.Write(ForceEyesClosed);
            output.Write(Asymmetrical);
            output.Write(ClassCard);
        }
        #endregion

        public Face_Data() { }
        public Face_Data(Face_Data source)
        {
            Name = source.Name;
            Emotions = source.Emotions;
            Pitch = source.Pitch;
            EyesOffset = source.EyesOffset;
            MouthOffset = source.MouthOffset;
            StatusOffset = source.StatusOffset;
            StatusFrame = source.StatusFrame;
            PlacementOffset = source.PlacementOffset;
            ForceEyesClosed = source.ForceEyesClosed;
            Asymmetrical = source.Asymmetrical;
            ClassCard = source.ClassCard;
        }

        public override string ToString()
        {
            return string.Format("Face_Data: {0}", Name);
        }

        public object Clone()
        {
            return new Face_Data(this);
        }
    }
}
