using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ColorExtension;

namespace TactileLibrary.Palette
{
    public class PaletteEntry : TactileDataContent
    {
        [ContentSerializer]
        public Color Value { get; private set; }
        [ContentSerializer]
        public int Weight;
        [ContentSerializer(Optional = true)]
        internal Color? Remap;

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static PaletteEntry GetEmptyInstance()
        {
            return new PaletteEntry();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var source = (PaletteEntry)other;

            Value = source.Value;
            Weight = source.Weight;
            Remap = source.Remap;
        }

        public static PaletteEntry ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            Value = Value.read(input);
            Weight = input.ReadInt32();
            bool remap = input.ReadBoolean();
            if (remap)
                Remap = new Color().read(input);
        }

        public override void Write(BinaryWriter output)
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
            Remap = null;
        }
        public PaletteEntry(PaletteEntry source)
        {
            CopyFrom(source);
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
        public override object Clone()
        {
            return new PaletteEntry(this);
        }
        #endregion
    }
}
