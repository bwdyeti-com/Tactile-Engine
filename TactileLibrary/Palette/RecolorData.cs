using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using TactileContentExtension;

namespace TactileLibrary.Palette
{
    public class RecolorData : TactileDataContent
    {
        public string Name;
        public Dictionary<string, RecolorEntry> Recolors = new Dictionary<string, RecolorEntry>();

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static RecolorData GetEmptyInstance()
        {
            return new RecolorData();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var source = (RecolorData)other;

            Name = source.Name;
            Recolors = source.Recolors.ToDictionary(
                p => p.Key,
                p => (RecolorEntry)p.Value.Clone());
        }

        public static RecolorData ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            Name = input.ReadString();
            input.ReadTactileContent(Recolors, RecolorEntry.GetEmptyInstance());
        }

        public override void Write(BinaryWriter output)
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
            CopyFrom(source);
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
        public override object Clone()
        {
            return new RecolorData(this);
        }
        #endregion
    }
}
