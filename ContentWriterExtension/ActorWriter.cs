using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ListExtension;
using DictionaryExtension;

using TWrite = TactileLibrary.Data_Actor;

namespace ContentWriterExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class ActorWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {
            // Id
            output.Write(value.Id);
            // Name
            output.Write(value.Name);
            // Description
            output.Write(value.Description);
            // Class Id
            output.Write(value.ClassId);
            // Level
            output.Write(value.Level);
            // Base Stats
            value.BaseStats.write(output);
            // Growths
            value.Growths.write(output);
            // Weapon Level
            value.WLvl.write(output);
            // Gender
            output.Write(value.Gender);
            // Affinity
            output.Write((int)value.Affinity);
            // Items
            value.Items.write(output);
            // Supports
            value.Supports.write(output);
            // Skills
            value.Skills.write(output);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(TactileLibrary.ActorReader).AssemblyQualifiedName;
        }
    }
}
