using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using ListExtension;

namespace FEXNA_Library.Palette
{
    public class RecolorEntry : IFEXNADataContent
    {
        public List<string> OtherNames = new List<string>();
        public PaletteParameters Parameters;
        
        #region IFEXNADataContent
        public IFEXNADataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static RecolorEntry GetEmptyInstance()
        {
            return new RecolorEntry();
        }

        public static RecolorEntry ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public void Read(BinaryReader input)
        {
            OtherNames.read(input);
            Parameters = PaletteParameters.ReadContent(input);
        }

        public void Write(BinaryWriter output)
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
            OtherNames = new List<string>(source.OtherNames);
            Parameters = (PaletteParameters)source.Parameters.Clone();
        }

        #region ICloneable
        public object Clone()
        {
            return new RecolorEntry(this);
        }
        #endregion
    }
}
