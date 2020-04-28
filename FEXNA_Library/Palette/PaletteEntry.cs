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
        }

        public void Write(BinaryWriter output)
        {
            Value.write(output);
            output.Write(Weight);
        }
        #endregion

        public PaletteEntry() { }
        public PaletteEntry(Color value, int weight)
        {
            Value = value;
            Weight = weight;
        }
        public PaletteEntry(PaletteEntry source)
        {
            Value = source.Value;
            Weight = source.Weight;
        }

        public override string ToString()
        {
            return string.Format("PaletteEntry: {0} Weight {1}",
                Value, Weight);
        }

        #region ICloneable
        public object Clone()
        {
            return new PaletteEntry(this);
        }
        #endregion
    }
}
