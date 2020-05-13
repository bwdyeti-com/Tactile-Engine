using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ColorExtension;
using FEXNAContentExtension;

namespace FEXNA_Library.Palette
{
    public class SpritePalette : IFEXNADataContent
    {
        [ContentSerializer]
        private string Name;
        [ContentSerializer]
        public bool IsIndexedPalette;
        [ContentSerializer]
        private List<PaletteEntry> Palette = new List<PaletteEntry>();
        [ContentSerializer]
        private List<PaletteRamp> Ramps = new List<PaletteRamp>();
        [ContentSerializer(ElementName = "DarkestColor")]
        private Color _DarkestColor;
        
        [ContentSerializerIgnore]
        public Color DarkestColor { get { return _DarkestColor; } }

        #region IFEXNADataContent
        public IFEXNADataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static SpritePalette GetEmptyInstance()
        {
            return new SpritePalette();
        }

        public static SpritePalette ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public void Read(BinaryReader input)
        {
            Name = input.ReadString();
            IsIndexedPalette = input.ReadBoolean();
            input.ReadFEXNAContent(Palette, PaletteEntry.GetEmptyInstance());
            input.ReadFEXNAContent(Ramps, PaletteRamp.GetEmptyInstance());
            _DarkestColor = _DarkestColor.read(input);
        }

        public void Write(BinaryWriter output)
        {
            output.Write(Name);
            output.Write(IsIndexedPalette);
            output.Write(Palette);
            output.Write(Ramps);
            _DarkestColor.write(output);
        }
        #endregion

        public SpritePalette() { }
        public SpritePalette(string name, Color darkestColor)
        {
            Name = name;
            IsIndexedPalette = true;
            _DarkestColor = darkestColor;
        }
        public SpritePalette(SpritePalette source)
        {
            Name = source.Name;
            IsIndexedPalette = source.IsIndexedPalette;
            Palette = source.Palette.Select(x => (PaletteEntry)x.Clone()).ToList();
            Ramps = source.Ramps.Select(x => (PaletteRamp)x.Clone()).ToList();
            _DarkestColor = source._DarkestColor;
        }

        public override string ToString()
        {
            return string.Format("SpritePalette: {0}", Name);
        }

        [ContentSerializerIgnore]
        public int Count { get { return Palette.Count; } }
        public List<Color> GetPalette()
        {
            return Palette
                .Select(x => x.Value)
                .ToList();
        }
        public List<Color> GetRemappedPalette()
        {
            return Palette
                .Select(x => x.Color)
                .ToList();
        }
        public PaletteEntry GetEntry(int index)
        {
            return (PaletteEntry)Palette[index].Clone();
        }
        public PaletteEntry GetEntry(int rampIndex, int colorIndex)
        {
            int index = ColorIndex(rampIndex, colorIndex);
            return GetEntry(index);
        }
        public PaletteEntry GetEntry(Color color)
        {
            int index = ColorIndex(color);
            if (index < 0)
                return new PaletteEntry(color, 1);
            return GetEntry(index);
        }
        public List<PaletteEntry> GetEntries()
        {
            return Palette.Select(x => (PaletteEntry)x.Clone()).ToList();
        }

        public int AddColor(Color color, int weight)
        {
            int index = ColorIndex(color);
            if (index >= 0)
                return index;
            else
            {
                var entry = new PaletteEntry(color, weight);
                Palette.Add(entry);
                return Palette.Count - 1;
            }
        }

        public void AddColorToRamp(int colorIndex, int rampIndex)
        {
            for (int i = 0; i < Ramps.Count; i++)
            {
                if (rampIndex != i)
                    Ramps[i].RemoveColor(Palette[colorIndex].Value);
            }

            var spriteRamp = GetRamp(rampIndex);
            spriteRamp.AddRampColor(Palette[colorIndex].Value);
        }

        public void RemoveColor(Color color)
        {
            int index = ColorIndex(color);
            if (index >= 0)
                RemoveAt(index);
        }
        public void RemoveAt(int index)
        {
            var entry = Palette[index];
            Palette.RemoveAt(index);
            for (int i = 0; i < Ramps.Count; i++)
            {
                Ramps[i].RemoveColor(entry.Value);
            }
        }

        public bool MoveColor(int oldIndex, int newIndex)
        {
            if (oldIndex != newIndex &&
                oldIndex >= 0 && oldIndex < Palette.Count &&
                newIndex >= 0 && newIndex < Palette.Count)
            {
                var entry = Palette[oldIndex];
                Palette.RemoveAt(oldIndex);
                Palette.Insert(newIndex, entry);

                return true;
            }

            return false;
        }

        public int ColorIndex(Color color)
        {
            return Palette.FindIndex(x => x.Value == color);
        }
        public int ColorIndex(int rampIndex, int colorIndex)
        {
            Color color = Ramps[rampIndex].Colors[colorIndex];
            return Palette.FindIndex(x => x.Value == color);
        }

        public void SetWeight(int index, int value)
        {
            Palette[index].Weight = value;
        }

        public void RemapColor(int index, Maybe<Color> remap)
        {
            Palette[index].Remap = remap.IsSomething ? (Color?)remap : null;

            // The darkest color might have changed
            RefreshDarkestColor();

            // All the ramps need resorted
            ResortRamps();

            // Each ramp with the color needs to re-minimize its error and
            // calculate its adjustments
            for (int i = 0; i < Ramps.Count; i++)
            {
                if (Ramps[i].Colors.Contains(Palette[index].Value))
                {
                    var spriteRamp = GetRamp(i);
                    spriteRamp.RecalibrateError();
                }
            }
        }

        public void PopularitySort()
        {
            var ordered_colors = Palette
                // Put transparent colors last
                .OrderBy(x => x.Value.A == 0 ? 1 : -1)
                // Sort by weight
                .ThenByDescending(x => x.Weight)
                .ToList();

            // Reverse if it was already sorted
            if (Enumerable.Range(0, Palette.Count)
                    .All(x => Palette.ElementAt(x) == ordered_colors.ElementAt(x)))
            {
                ordered_colors = ordered_colors
                    .Reverse<PaletteEntry>()
                    .OrderBy(x => x.Value.A == 0 ? 1 : -1)
                    .ToList();
            }
            Palette = ordered_colors.ToList();
        }
        public void LuminanceSort()
        {
            var ordered_colors = Palette
                .Select(x => new Tuple<PaletteEntry, float>(
                    x, Color_Util.GetLuma(x.Color)))
                // Put transparent colors last
                .OrderBy(x => x.Item1.Value.A == 0 ? 1 : -1)
                // Sort by luma
                .ThenByDescending(x => x.Item2)
                .Select(x => x.Item1)
                .ToList();

            // Reverse if it was already sorted
            if (Enumerable.Range(0, Palette.Count)
                    .All(x => Palette.ElementAt(x) == ordered_colors.ElementAt(x)))
            {
                ordered_colors = ordered_colors
                    .Reverse<PaletteEntry>()
                    .OrderBy(x => x.Value.A == 0 ? 1 : -1)
                    .ToList();
            }
            Palette = ordered_colors.ToList();
        }

        internal List<PaletteEntry> RampEntries(PaletteRamp ramp)
        {
            var result = ramp.Colors
                .Select(color =>
                {
                    int index = ColorIndex(color);
                    if (index >= 0)
                        return GetEntry(index);
                    else
                        return new PaletteEntry(color, 0);
                })
                .ToList();
            return result;
        }

        [ContentSerializerIgnore]
        public int RampCount { get { return Ramps.Count; } }
        public int RampSize(int index)
        {
            return Ramps[index].Count;
        }

        public void AddRamp()
        {
            string name = "New Ramp";
            for (int index = 1;  Ramps.Any(x => x.Name == name); index++)
            {
                name = string.Format("New Ramp{0}", index);
            }

            var ramp = new PaletteRamp(name, _DarkestColor);

            Ramps.Add(ramp);
        }
        public void AddRamp(List<Color> sourceColors, List<int> sourceWeights)
        {
            // Order the colors by lightness
            var order = PaletteRamp.ColorLumaOrder(sourceColors);
            var orderedColors = order
                .Select(x => sourceColors[x])
                .ToList();
            var orderedWeights = order
                .Select(x => sourceWeights[x])
                .ToList();

            AddRamp();
            int rampIndex = Ramps.Count - 1;

            foreach (int index in order)
            {
                int colorIndex = AddColor(orderedColors[index], orderedWeights[index]);
                AddColorToRamp(colorIndex, rampIndex);
            }
        }

        public bool MoveRamp(int oldIndex, int newIndex)
        {
            if (oldIndex != newIndex &&
                oldIndex >= 0 && oldIndex < Ramps.Count &&
                newIndex >= 0 && newIndex < Ramps.Count)
            {
                var ramp = Ramps[oldIndex];
                Ramps.RemoveAt(oldIndex);
                Ramps.Insert(newIndex, ramp);

                return true;
            }

            return false;
        }

        public void RemoveRamp(int index)
        {
            Ramps.RemoveAt(index);
        }

        public SpriteRamp GetRamp(int index)
        {
            var ramp = Ramps[index];
            return new SpriteRamp(this, ramp);
        }
        internal void ReplaceRamp(int index, PaletteRamp ramp)
        {
            Ramps[index] = ramp;
        }

        public int FindRampIndexFromName(string name, List<string> otherNames)
        {
            var ramp = Ramps
                .FirstOrDefault(x => x.Name == name);
            if (ramp == null && otherNames != null)
            {
                // Use other names if the actual name doesn't show up
                foreach (string otherName in otherNames)
                {
                    ramp = Ramps
                        .FirstOrDefault(x => x.Name == otherName);
                    if (ramp != null)
                        break;
                }
            }

            return Ramps.IndexOf(ramp);
        }

        public string GetRampName(int index)
        {
            return Ramps[index].Name;
        }
        public void SetRampName(int index, string name)
        {
            Ramps[index].Name = name;
        }

        public IEnumerable<string> AllRampNames()
        {
            return Ramps.Select(x => x.Name);
        }

        public bool AnyRampName(string name)
        {
            return Ramps.Any(x => name == x.Name);
        }
        public bool AnyRampName(IEnumerable<string> names)
        {
            return Ramps.Any(x => names.Contains(x.Name));
        }

        public int RampColorIndex(int rampIndex, Color color)
        {
            return Ramps[rampIndex].Colors.IndexOf(color);
        }

        public IEnumerable<Color> AllRampColors()
        {
            return Ramps.SelectMany(x => x.Colors);
        }

        public void SetDarkestColor(int index)
        {
            _DarkestColor = Palette[index].Color;

            for (int i = 0; i < Ramps.Count; i++)
                Ramps[i].SetBlackLevel(_DarkestColor);
        }

        public void RefreshDarkestColor()
        {
            if (Palette.Any())
            {
                var order = PaletteRamp.ColorLumaOrder(GetRemappedPalette());
                order = order
                    .Where(x => Palette[x].Color.A > 0)
                    .ToList();
                if (order.Any())
                {
                    int index = order.ElementAt(0);
                    SetDarkestColor(index);
                }
            }
        }

        private void ResortRamps()
        {
            for (int i = 0; i < Ramps.Count; i++)
            {
                var spriteRamp = GetRamp(i);
                spriteRamp.OrderColors();
            }
        }

        public Color[] GetRecolors(RecolorData recolorData, Dictionary<string, string> defaultOtherNames)
        {
            // Go through each ramp to make a dictionary of the color remaps
            Dictionary<Color, Color> recolors = new Dictionary<Color, Color>();
            if (recolorData != null)
            {
                for (int i = 0; i < Ramps.Count; i++)
                {
                    var ramp = GetRamp(i);
                    string name = ramp.Name;
                    if (!recolorData.Recolors.ContainsKey(ramp.Name))
                    {
                        // If no recolor has this name, check their OtherNames
                        var pair = recolorData.Recolors
                            .FirstOrDefault(x => recolorData.OtherNames(x.Key, defaultOtherNames).Contains(ramp.Name));
                        if (!string.IsNullOrEmpty(pair.Key))
                            name = pair.Key;
                        else
                            continue;
                    }

                    var parameters = recolorData.Recolors[name].Parameters;
                    var recolorPalette = ramp.GetIndexedPalette(parameters);
                    for (int j = 0; j < ramp.Count; j++)
                    {
                        recolors[ramp.GetEntry(j).Value] = recolorPalette.GetColor(j);
                    }
                }
            }

            // Take the original palette and apply all the recolors
            Color[] result = new Color[Palette.Count];
            for (int i = 0; i < Palette.Count; i++)
            {
                if (recolors.ContainsKey(Palette[i].Value))
                    result[i] = recolors[Palette[i].Value];
                else
                    result[i] = Palette[i].Color;
            }
            return result;
        }

        #region ICloneable
        public object Clone()
        {
            return new SpritePalette(this);
        }
        #endregion
    }
}
