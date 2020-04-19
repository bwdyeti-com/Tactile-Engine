using Microsoft.Xna.Framework;

namespace FEXNA_Library.Palette
{
    public class PaletteEntry
    {
        public Color Value;
        public int Weight;

        public override string ToString()
        {
            return string.Format("PaletteEntry: {0} Weight {1}",
                Value, Weight);
        }
    }
}
