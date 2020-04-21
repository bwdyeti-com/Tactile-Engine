using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using ListExtension;

namespace FEXNA_Library.Palette
{
    public class RecolorEntry : IFEXNADataContent
    {
        public List<string> OtherNames;
        public PaletteParameters Parameters;

        #region Serialization
        public IFEXNADataContent Read_Content(ContentReader input)
        {
            RecolorEntry result = new RecolorEntry();

            result.OtherNames.read(input);
            result.Parameters = (PaletteParameters)result.Parameters.Read_Content(input);

            return result;
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
