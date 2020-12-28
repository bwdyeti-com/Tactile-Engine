using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using ListExtension;

namespace TactileLibrary.Palette
{
    public class RecolorEntry : TactileDataContent
    {
        public List<string> OtherNames = new List<string>();
        public PaletteParameters Parameters;

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static RecolorEntry GetEmptyInstance()
        {
            return new RecolorEntry();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var source = (RecolorEntry)other;

            OtherNames = new List<string>(source.OtherNames);
            Parameters = (PaletteParameters)source.Parameters.Clone();
        }

        public static RecolorEntry ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            OtherNames.read(input);
            Parameters = PaletteParameters.ReadContent(input);
        }

        public override void Write(BinaryWriter output)
        {
            OtherNames.write(output);
            Parameters.Write(output);
        }
        #endregion

        private RecolorEntry() { }
        public RecolorEntry(PaletteParameters parameters)
        {
            OtherNames = new List<string>();
            Parameters = parameters;
        }
        public RecolorEntry(RecolorEntry source)
        {
            CopyFrom(source);
        }

        #region ICloneable
        public override object Clone()
        {
            return new RecolorEntry(this);
        }
        #endregion
    }
}
