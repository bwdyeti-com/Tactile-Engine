using Microsoft.Xna.Framework.Content;

using TRead = FEXNA_Library.Data_Map;

namespace FEXNA_Library
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class MapReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            int tileset = input.ReadInt32();
            int columns = input.ReadInt32();
            int rows = input.ReadInt32();

            int[,] map_data = new int[columns, rows];

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    map_data[column, row] = input.ReadInt32();
                }
            }

            return new Data_Map(map_data, tileset);
        }
    }
}
