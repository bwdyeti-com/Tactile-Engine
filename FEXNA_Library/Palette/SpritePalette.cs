using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FEXNA_Library.Palette
{
    public class SpritePalette
    {
        public string Name;
        public bool IsIndexedPalette;
        public List<PaletteEntry> Palette = new List<PaletteEntry>();
        public List<PaletteRamp> Ramps = new List<PaletteRamp>();
        public Color DarkestColor;

        public SpritePalette(string name, Color darkestColor)
        {
            Name = name;
            IsIndexedPalette = true;
            DarkestColor = darkestColor;
        }

        public int AddColor(Color color, int weight)
        {
            int index = ColorIndex(color);
            if (index >= 0)
                return index;
            else
            {
                var entry = new PaletteEntry { Value = color, Weight = weight };
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
            var order = PaletteRamp.ColorLightnessOrder(sourceColors);
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
                        return new PaletteEntry() { Value = color, Weight = 0 };
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
                var order = PaletteRamp.ColorLightnessOrder(Palette.Select(x => x.Value));
                int index = order.ElementAt(0);
                SetDarkestColor(index);
            }
        }
    }
}
