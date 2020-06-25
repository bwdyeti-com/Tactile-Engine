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
        public Dictionary<string, RecolorEntry> Recolors = new Dictionary<string, RecolorEntry>();

        #region IFEXNADataContent
        public IFEXNADataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static RecolorData GetEmptyInstance()
        {
            return new RecolorData();
        }

        public static RecolorData ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public void Read(BinaryReader input)
        {
            Name = input.ReadString();
            input.ReadFEXNAContent(Recolors, RecolorEntry.GetEmptyInstance());
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
                p => (RecolorEntry)p.Value.Clone());
        }

        public List<string> OtherNames(string key, Dictionary<string, string> defaultOtherNames)
        {
            var result = new List<string>(Recolors[key].OtherNames);
            foreach (var otherName in defaultOtherNames
                .Where(x => x.Value == key)
                .Select(x => x.Key))
            {
                if (!HasRamp(otherName))
                    result.Add(otherName);
            }
            return result;
        }

        public bool HasRamp(string name)
        {
            return Recolors.ContainsKey(name) || Recolors.Any(x => x.Value.OtherNames.Contains(name));
        }

        #region ICloneable
        public object Clone()
        {
            return new RecolorData(this);
        }
        #endregion
    }
}
