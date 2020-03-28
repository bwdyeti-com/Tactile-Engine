using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ListExtension;

namespace FEXNA_Library
{
    public class Data_Map
    {
        protected int[,] Values = new int[0, 0];
        protected int Tileset;
        public List<Vector2> Seize_Points = new List<Vector2>();
        public List<Vector2> Defend_Points = new List<Vector2>();
        public List<Vector2> Escape_Points = new List<Vector2>();
        public List<Vector2> Thief_Escape_Points = new List<Vector2>();

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Tileset);
            writer.Write(Columns);
            writer.Write(Rows);
            for (int y = 0; y < Rows; y++)
                for (int x = 0; x < Columns; x++)
                    writer.Write(Values[x, y]);
            Seize_Points.write(writer);
            Defend_Points.write(writer);
            Escape_Points.write(writer);
            Thief_Escape_Points.write(writer);
        }

        public void read(BinaryReader reader)
        {
            Tileset = reader.ReadInt32();
            int columns = reader.ReadInt32();
            int rows = reader.ReadInt32();
            Values = new int[columns, rows];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < columns; x++)
                    Values[x, y] = reader.ReadInt32();
            Seize_Points.read(reader);
            Defend_Points.read(reader);
            Escape_Points.read(reader);
            Thief_Escape_Points.read(reader);
        }
        #endregion

        public Data_Map() { }

        public Data_Map(int[,] values, int tileset)
        {
            Values = values;
            Tileset = tileset;
        }
        public Data_Map(Data_Map src_map)
        {
            Values = new int[src_map.values.GetLength(0), src_map.values.GetLength(1)];
            System.Array.Copy(src_map.values, Values, Values.Length);
            Tileset = src_map.GetTileset();
            Seize_Points = new List<Vector2>();
            Seize_Points.AddRange(src_map.Seize_Points);
            Defend_Points = new List<Vector2>();
            Defend_Points.AddRange(src_map.Defend_Points);
            Escape_Points = new List<Vector2>();
            Escape_Points.AddRange(src_map.Escape_Points);
            Thief_Escape_Points = new List<Vector2>();
            Thief_Escape_Points.AddRange(src_map.Thief_Escape_Points);
        }

        public int GetTileset()
        {
            return Tileset;
        }

        public int[,] values { get { return Values; } }

        public int GetValue(int column, int row)
        {
            return Values[column, row];
        }

        public void set_value(int column, int row, int new_value)
        {
            if (column < 0 || column >= Values.GetLength(0))
                return;
            if (row < 0 || row >= Values.GetLength(1))
                return;
            Values[column, row] = new_value;
        }
 
        public int Columns
        {
            get
            {
                return Values.GetLength(0);
            }
        }
 
        public int Rows
        {
            get
            {
                return Values.GetLength(1);
            }
        }
    }
}