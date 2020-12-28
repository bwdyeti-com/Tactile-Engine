using Microsoft.Xna.Framework.Content;

using TRead = TactileLibrary.TactileDataContent;

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
    public class TactileDataReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            TRead newInstance = existingInstance.EmptyInstance();
            newInstance.Read(input);

            existingInstance.CopyFrom(newInstance);

            return existingInstance;
        }
    }
}
