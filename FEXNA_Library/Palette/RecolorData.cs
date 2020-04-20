using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using FEXNAContentExtension;

namespace FEXNA_Library.Palette
{
    public class RecolorData : IFEXNADataContent
    {
        public string Name;
        public Dictionary<string, PaletteParameters> Recolors = new Dictionary<string, PaletteParameters>();

        #region Serialization
        public IFEXNADataContent Read_Content(ContentReader input)
        {
            RecolorData result = new RecolorData();

            result.Name = input.ReadString();
            input.ReadFEXNAContent(result.Recolors);

            return result;
        }

        public void Write(BinaryWriter output)
        {
            output.Write(Name);
            output.Write(Recolors);
        }
        #endregion

        private RecolorData() { }
        public RecolorData(string name)
        {
            Name = name;
        }
        public RecolorData(RecolorData source)
        {
            Name = source.Name;
            Recolors = source.Recolors.ToDictionary(
                p => p.Key,
                p => (PaletteParameters)p.Value.Clone());
        }

        #region ICloneable
        public object Clone()
        {
            return new RecolorData(this);
        }
        #endregion
    }
}
