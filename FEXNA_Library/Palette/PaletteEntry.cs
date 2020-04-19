using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ColorExtension;

namespace FEXNA_Library.Palette
{
    public class PaletteEntry : IFEXNADataContent
    {
        public Color Value;
        public int Weight;

        #region Serialization
        public IFEXNADataContent Read_Content(ContentReader input)
        {
            PaletteEntry result = new PaletteEntry();

            result.Value = result.Value.read(input);
            result.Weight = input.ReadInt32();

            return result;
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
