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
        public string Name;
        public bool IsIndexedPalette;
        public List<PaletteEntry> Palette = new List<PaletteEntry>();
        public List<PaletteRamp> Ramps = new List<PaletteRamp>();
        public Color DarkestColor;
        
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
            DarkestColor = DarkestColor.read(input);
        }

        public void Write(BinaryWriter output)
        {
            output.Write(Name);
            output.Write(IsIndexedPalette);
            output.Write(Palette);
            output.Write(Ramps);
            DarkestColor.write(output);
        }
        #endregion

        public SpritePalette() { }
        public SpritePalette(string name, Color darkestColor)
        {
            Name = name;
            IsIndexedPalette = true;
            DarkestColor = darkestColor;
        }
        public SpritePalette(SpritePalette source)
        {
            Name = source.Name;
            IsIndexedPalette = source.IsIndexedPalette;
            Palette = source.Palette.Select(x => (PaletteEntry)x.Clone()).ToList();
            Ramps = source.Ramps.Select(x => (PaletteRamp)x.Clone()).ToList();
            DarkestColor = source.DarkestColor;
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

            Ramps[rampIndex].AddColor(Palette[colorIndex].Value);
        }

        public void AddRamp()
        {
            var ramp = new PaletteRamp();
            ramp.Generator.BlackLevel = GeneratedColorPalette.ValueFormula(
                DarkestColor);

            string name = "New Ramp";
            for (int index = 1;  Ramps.Any(x => x.Name == name); index++)
            {
                name = string.Format("New Ramp{0}", index);
            }
            ramp.Name = name;

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

        private int ColorIndex(Color color)
        {
            return Palette.FindIndex(x => x.Value == color);
        }

        public List<PaletteEntry> RampEntries(int rampIndex)
        {
            return RampEntries(Ramps[rampIndex]);
        }
        public List<PaletteEntry> RampEntries(PaletteRamp ramp)
        {
            var result = ramp.Colors
                .Select(color =>
                {
                    int index = ColorIndex(color);
                    if (index >= 0)
                        return Palette[index];
                    else
                        return new PaletteEntry(color, 0);
                })
                .ToList();
            return result;
        }

        public void SetDarkestColor(int index)
        {
            DarkestColor = Palette[index].Value;

            for (int i = 0; i < Ramps.Count; i++)
                Ramps[i].Generator.BlackLevel = GeneratedColorPalette.ValueFormula(DarkestColor);
        }

        public void RefreshDarkestColor()
        {
            if (Palette.Any())
            {
                var order = PaletteRamp.ColorLumaOrder(Palette.Select(x => x.Value));
                int index = order.ElementAt(0);
                SetDarkestColor(index);
            }
        }

        public Color[] GetRecolors(RecolorData recolorData)
        {
            // Go through each ramp to make a dictionary of the color remaps
            Dictionary<Color, Color> recolors = new Dictionary<Color, Color>();
            if (recolorData != null)
            {
                foreach (var ramp in Ramps)
                {
                    string name = ramp.Name;
                    if (!recolorData.Recolors.ContainsKey(ramp.Name))
                    {
                        // If no recolor has this name, check their OtherNames
                        var pair = recolorData.Recolors.FirstOrDefault(x => x.Value.OtherNames.Contains(ramp.Name));
                        if (!string.IsNullOrEmpty(pair.Key))
                            name = pair.Key;
                        else
                            continue;
                    }

                    var parameters = recolorData.Recolors[name].Parameters;
                    var recolorPalette = ramp.GetIndexedPalette(parameters);
                    for (int i = 0; i < ramp.Colors.Count; i++)
                    {
                        recolors[ramp.Colors[i]] = recolorPalette.GetColor(i);
                    }
                }
            }

            // Take the original palette and apply all the recolors
            Color[] result = Palette
                .Select(x => x.Value)
                .ToArray();
            for (int i = 0; i < Palette.Count; i++)
            {
                if (recolors.ContainsKey(result[i]))
                    result[i] = recolors[result[i]];
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
