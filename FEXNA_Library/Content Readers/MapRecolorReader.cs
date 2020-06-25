using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

// TODO: replace this with the type you want to read.
using TRead = FEXNA_Library.MapSpriteRecolorData;

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
    public class MapRecolorReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            existingInstance = new TRead();
            existingInstance.data = new Dictionary<Color,List<Color>>();
            int pairs = input.ReadInt32();
            for (int i = 0; i < pairs; i++)
            {
                Color key = input.ReadColor();
                int value_length = input.ReadInt32();
                List<Color> value = new List<Color>();
                for (int j = 0; j < value_length; j++)
                {
                    value.Add(input.ReadColor());
                }
                existingInstance.data.Add(key, value);
            }
            return existingInstance;
        }
    }
}
