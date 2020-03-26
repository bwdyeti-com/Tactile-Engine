using Microsoft.Xna.Framework.Content;
using ArrayExtension;
using ListExtension;

using TRead = FEXNA_Library.Data_Item;

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
    public class ItemReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            existingInstance = Data_Item.read(input);

            return existingInstance;
        }
    }
}
