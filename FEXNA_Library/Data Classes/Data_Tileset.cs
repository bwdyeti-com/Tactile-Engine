using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using DictionaryExtension;
using ListExtension;

namespace FEXNA_Library
{
    public class Data_Tileset : IFEXNADataContent
    {
        public int Id = 0;
        public string Name = "New Tileset";
        public string Graphic_Name = "";
        [ContentSerializer(Optional = true)]
        public string BattlebackSuffix = "";
        public List<string> Animated_Tile_Names = new List<string> { };
        public List<Rectangle> Animated_Tile_Data = new List<Rectangle> { };
        public Dictionary<Vector2, Vector2> Pillage_Tile_Changes = new Dictionary<Vector2, Vector2>();
        public List<int> Terrain_Tags = new List<int>();

        public IFEXNADataContent Read_Content(ContentReader input)
        {
            Data_Tileset result = new Data_Tileset();

            result.Id = input.ReadInt32();
            result.Name = input.ReadString();
            result.Graphic_Name = input.ReadString();
            result.BattlebackSuffix = input.ReadString();
            result.Animated_Tile_Names.read(input);
            result.Animated_Tile_Data.read(input);
            result.Pillage_Tile_Changes.read(input);
            result.Terrain_Tags.read(input);

            return result;
        }
        public void Write(BinaryWriter output)
        {
            output.Write(Id);
            output.Write(Name);
            output.Write(Graphic_Name);
            output.Write(BattlebackSuffix);
            Animated_Tile_Names.write(output);
            Animated_Tile_Data.write(output);
            Pillage_Tile_Changes.write(output);
            Terrain_Tags.write(output);
        }

        #region Serialization
        public static Data_Tileset read(BinaryReader reader)
        {
            Data_Tileset result = new Data_Tileset();
            result.Id = reader.ReadInt32();
            result.Name = reader.ReadString();
            result.Graphic_Name = reader.ReadString();
            result.BattlebackSuffix = reader.ReadString();
            result.Animated_Tile_Names.read(reader);
            result.Animated_Tile_Data.read(reader);
            result.Pillage_Tile_Changes.read(reader);
            result.Terrain_Tags.read(reader);
            return result;
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Name);
            writer.Write(Graphic_Name);
            writer.Write(BattlebackSuffix);
            Animated_Tile_Names.write(writer);
            Animated_Tile_Data.write(writer);
            Pillage_Tile_Changes.write(writer);
            Terrain_Tags.write(writer);
        }
        #endregion

        public Data_Tileset() { }
        public Data_Tileset(Data_Tileset source)
        {
            Id = source.Id;
            Name = source.Name;
            Graphic_Name = source.Graphic_Name;
            BattlebackSuffix = source.BattlebackSuffix;
            Animated_Tile_Names = new List<string>(source.Animated_Tile_Names);
            Animated_Tile_Data = new List<Rectangle>(source.Animated_Tile_Data);
            Pillage_Tile_Changes = new Dictionary<Vector2, Vector2>(source.Pillage_Tile_Changes);
            Terrain_Tags = new List<int>(source.Terrain_Tags);
        }

        public override string ToString()
        {
            return string.Format("Tileset Data: {0}, filename = {1}", Name, Graphic_Name);
        }

        public object Clone()
        {
            return new Data_Tileset(this);
        }
    }
}
