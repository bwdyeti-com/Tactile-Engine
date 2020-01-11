using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Palette_Handler
    {
        const int PALETTE_COUNT = 64;
        public const int PALETTE_SIZE = 256;
        protected bool Even_Frame;
        protected Texture2D[][] Palettes;
        protected int Index;

        #region Accessors
        protected Texture2D[] palettes { get { return Palettes[Even_Frame ? 0 : 1]; } }
        #endregion

        public Palette_Handler()
        {
            Palettes = new Texture2D[2][];
            Palettes[0] = new Texture2D[PALETTE_COUNT];
            Palettes[1] = new Texture2D[PALETTE_COUNT];

            for (int i = 0; i < Palettes[0].Length; i++)
                Palettes[0][i] = (Global.Content as ContentManagers.ThreadSafeContentManager).texture_from_size(PALETTE_SIZE, 1);
            for (int i = 0; i < Palettes[1].Length; i++)
                Palettes[1][i] = (Global.Content as ContentManagers.ThreadSafeContentManager).texture_from_size(PALETTE_SIZE, 1);
        }

        public void update()
        {
            Even_Frame = !Even_Frame;
            Index = -1;
        }

        public Texture2D get_palette()
        {
            Index++;
            if (Index < palettes.Length)
                return palettes[Index];
            return null;
        }

        public void Dispose()
        {
            foreach (var array in Palettes)
                for (int i = 0; i < array.Length; i++)
                    if (array[i] != null)
                    {
                        array[i].Dispose();
                        array[i] = null;
                    }
        }
    }
}
