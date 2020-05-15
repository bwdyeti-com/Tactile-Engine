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
        
        #region IFEXNADataContent
        public IFEXNADataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static Data_Tileset GetEmptyInstance()
        {
            return new Data_Tileset();
        }

        public static Data_Tileset ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public void Read(BinaryReader input)
        {
            Id = input.ReadInt32();
            Name = input.ReadString();
            Graphic_Name = input.ReadString();
            BattlebackSuffix = input.ReadString();
            Animated_Tile_Names.read(input);
            Animated_Tile_Data.read(input);
            Pillage_Tile_Changes.read(input);
            Terrain_Tags.read(input);
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
