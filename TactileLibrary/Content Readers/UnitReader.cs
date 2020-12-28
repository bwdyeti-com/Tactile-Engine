using Microsoft.Xna.Framework.Content;
using DictionaryExtension;
using ListExtension;

using TRead = TactileLibrary.Map_Unit_Data;

namespace TactileLibrary.Content_Readers
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class UnitReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            existingInstance = new TRead();

            // Units
            existingInstance.Units.read(input);
            // Reinforcements
            existingInstance.Reinforcements.read(input);

            return existingInstance;
        }
    }
}
