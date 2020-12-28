using Microsoft.Xna.Framework.Content;
using ListExtension;

using TRead = TactileLibrary.Data_Support;

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
    public class SupportReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            existingInstance = new TRead();

            // Key
            existingInstance.Key = input.ReadString();
            // Id1
            existingInstance.Id1 = input.ReadInt32();
            // Id2
            existingInstance.Id2 = input.ReadInt32();
            // Supports
            existingInstance.Supports.read(input);

            return existingInstance;
        }
    }
}
