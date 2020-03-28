using System;
using Microsoft.Xna.Framework.Content.Pipeline;

using TInput = System.String;
using TOutput = FEXNA_Library.Data_Map;

namespace ContentWriterExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentProcessor(DisplayName = "Map Processor")]
    public class MapProcessor : ContentProcessor<TInput, TOutput>
    {
        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            string[] lines = input.Split(new char[] { '\n' });
            int tileset = Convert.ToInt32(lines[0]);
            string[] dim_line = lines[1].Split(new char[] { ' ' });
            int rows = Convert.ToInt32(dim_line[0]);
            int columns = Convert.ToInt32(dim_line[1]);

            int[,] map_data = new int[columns, rows];
            for (int row = 0; row < rows; row++)
            {
                string[] values = lines[row + 2].Split(new char[] { ' ' });
                for (int column = 0; column < columns; column++)
                {
                    map_data[column, row] = Convert.ToInt32(values[column]);
                }
            }

            return new FEXNA_Library.Data_Map(map_data, tileset);
        }
    }
}