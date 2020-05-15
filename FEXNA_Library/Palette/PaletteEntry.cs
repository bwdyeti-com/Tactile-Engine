using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ColorExtension;

namespace FEXNA_Library.Palette
{
    public class PaletteEntry : IFEXNADataContent
    {
        [ContentSerializer]
        public Color Value { get; private set; }
        [ContentSerializer]
        public int Weight;
        [ContentSerializer(Optional = true)]
        internal Color? Remap;

        #region IFEXNADataContent
        public IFEXNADataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static PaletteEntry GetEmptyInstance()
        {
            return new PaletteEntry();
        }

        public static PaletteEntry ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public void Read(BinaryReader input)
        {
            Value = Value.read(input);
            Weight = input.ReadInt32();
            bool remap = input.ReadBoolean();
            if (remap)
                Remap = new Color().read(input);
        }

        public void Write(BinaryWriter output)
        {
            Value.write(output);
            output.Write(Weight);
            output.Write(Remap.HasValue);
            if (Remap.HasValue)
                Remap.Value.write(output);
        }
        #endregion

        private PaletteEntry() { }
        public PaletteEntry(Color value, int weight)
        {
            Value = value;
            Weight = weight;
            Remap = Maybe<Color>.Nothing;
        }
        public PaletteEntry(PaletteEntry source)
        {
            Value = source.Value;
            Weight = source.Weight;
            Remap = source.Remap;
        }

        public override string ToString()
        {
            return string.Format("PaletteEntry: {0} Weight {1}{2}",
                Value, Weight, Remap.HasValue ? " (Remapped)" : "");
        }

        [ContentSerializerIgnore]
        public Color Color
        {
            get
            {
                if (Remap.HasValue)
                    return Remap.Value;
                return Value;
            }
        }

        #region ICloneable
        public object Clone()
        {
            return new PaletteEntry(this);
        }
        #endregion
    }
}
