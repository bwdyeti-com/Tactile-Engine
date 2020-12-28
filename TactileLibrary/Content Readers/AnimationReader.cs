using Microsoft.Xna.Framework.Content;
using ListExtension;

using TRead = TactileLibrary.Battle_Animation_Data;

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
    public class AnimationReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            existingInstance = new TRead();

            existingInstance.read(input);

            return existingInstance;
        }
    }
}
