using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using ColorExtension;
using HashSetExtension;
using TactileListExtension;

namespace Tactile
{
    class TileOutlineData
    {
        internal byte Type { get; private set; }
        internal Color Tint { get; private set; }
        private HashSet<Vector2> Tiles = new HashSet<Vector2>();

        private HashSet<Rectangle> Edges;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Type);
            Tint.write(writer);
            Tiles.write(writer);
        }

        public static TileOutlineData read(BinaryReader reader)
        {
            TileOutlineData result = new TileOutlineData();
            result.Type = reader.ReadByte();
            result.Tint = result.Tint.read(reader);
            result.Tiles.read(reader);
            result.refresh_edges();
            return result;
        }
        #endregion

        private TileOutlineData() { }
        internal TileOutlineData(byte type, Color tint)
        {
            Type = type;
            Tint = tint;
        }

        internal void add_area(Rectangle area)
        {
            for (int y = area.Top; y < area.Bottom; y++)
                for (int x = area.Left; x < area.Right; x++)
                    Tiles.Add(new Vector2(x, y));
            refresh_edges();
        }

        internal void remove_area(Rectangle area)
        {
            for (int y = area.Top; y < area.Bottom; y++)
                for (int x = area.Left; x < area.Right; x++)
                    Tiles.Remove(new Vector2(x, y));
            refresh_edges();
        }

        private void refresh_edges()
        {
            Edges = new HashSet<Rectangle>(Additional_Math.get_edges(Tiles));
        }

        internal IEnumerable<Rectangle> get_edges()
        {
            foreach (var rect in Edges)
                yield return rect;
        }
    }
}
