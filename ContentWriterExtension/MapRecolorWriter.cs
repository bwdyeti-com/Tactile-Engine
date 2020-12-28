using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TWrite = TactileLibrary.MapSpriteRecolorData;

namespace ContentWriterExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class MapRecolorWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {
            output.Write(value.data.Count);
            foreach(KeyValuePair<Color, List<Color>> color_data in value.data)
            {
                output.Write(color_data.Key);
                output.Write(color_data.Value.Count);
                foreach (Color color in color_data.Value)
                {
                    output.Write(color);
                }
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(TactileLibrary.MapRecolorReader).AssemblyQualifiedName;
        }
    }
}
