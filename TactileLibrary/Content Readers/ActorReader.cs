using Microsoft.Xna.Framework.Content;
using ListExtension;
using DictionaryExtension;

using TRead = TactileLibrary.Data_Actor;

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
    public class ActorReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            existingInstance = new TRead();
            // Id
            existingInstance.Id = input.ReadInt32();
            // Name
            existingInstance.Name = input.ReadString();
            // Description
            existingInstance.Description = input.ReadString();
            // Class Id
            existingInstance.ClassId = input.ReadInt32();
            // Level
            existingInstance.Level = input.ReadInt32();
            // Base Stats
            existingInstance.BaseStats.read(input);
            // Growths
            existingInstance.Growths.read(input);
            // Weapon Level
            existingInstance.WLvl.read(input);
            // Gender
            existingInstance.Gender = input.ReadInt32();
            // Affinity
            existingInstance.Affinity = (Affinities)input.ReadInt32();
            // Items
            existingInstance.Items.read(input);
            // Supports
            existingInstance.Supports.read(input);
            // Skills
            existingInstance.Skills.read(input);

            return existingInstance;
        }
    }
}
