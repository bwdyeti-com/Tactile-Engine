using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TWrite = FEXNA_Library.Data_Map;

namespace ContentWriterExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class MapWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {
            output.Write(value.GetTileset());
            output.Write(value.Columns);
            output.Write(value.Rows);
            for (int row = 0; row < value.Rows; row++)
            {
                for (int column = 0; column < value.Columns; column++)
                {
                    output.Write(value.GetValue(column, row));
                }
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(FEXNA_Library.MapReader).AssemblyQualifiedName;
        }
    }
}
