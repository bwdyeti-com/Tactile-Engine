using Microsoft.Xna.Framework.Content;
using ListExtension;

using TRead = TactileLibrary.Frame_Data;

namespace TactileLibrary
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class AnimDataReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            existingInstance = new TRead();

            // name
            existingInstance.name = input.ReadString();
            // offsets
            existingInstance.offsets.read(input);
            // src rects
            existingInstance.src_rects.read(input);

            return existingInstance;
        }
    }
}
