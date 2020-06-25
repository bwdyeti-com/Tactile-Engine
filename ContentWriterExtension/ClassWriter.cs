using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ListExtension;

using TWrite = FEXNA_Library.Data_Class;

namespace ContentWriterExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class ClassWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {
            // Id
            output.Write(value.Id);
            // Name
            output.Write(value.Name);
            // Class Type
            output.Write(value.Class_Types.Count);
            foreach (FEXNA_Library.ClassTypes type in value.Class_Types)
            {
                output.Write((int)type);
            }
            // Skills
            value.Skills.write(output);
            // Description
            output.Write(value.Description);
            // Caps
            if (value.Caps == null)
            {
                output.Write(false);
            }
            else
            {
                output.Write(true);
                output.Write(value.Caps[0].Count);
                foreach (int cap in value.Caps[0])
                {
                    output.Write(cap);
                }
                foreach (int cap in value.Caps[1])
                {
                    output.Write(cap);
                }
            }
            // Max Wlvl
            output.Write(value.Max_WLvl.Count);
            foreach (int wlvl in value.Max_WLvl)
            {
                output.Write(wlvl);
            }
            value.Max_WLvl = new List<int>();
            // Promotions
            output.Write(value.Promotion.Count);
            foreach (KeyValuePair<int, List<int>[]> promotion in value.Promotion)
            {
                output.Write(promotion.Key);
                output.Write(promotion.Value[0].Count);
                foreach (int stat in promotion.Value[0])
                {
                    output.Write(stat);
                }
                output.Write(promotion.Value[1].Count);
                foreach (int wlvl in promotion.Value[1])
                {
                    output.Write(wlvl);
                }
            }
            // Tier
            output.Write(value.Tier);
            // Mov
            output.Write(value.Mov);
            // Mov Cap
            output.Write(value.Mov_Cap);
            // Movement Type
            output.Write((int)value.Movement_Type);
            // Generics
            output.Write(value.Generic_Stats.Count);
            foreach (List<int>[] generic in value.Generic_Stats)
            {
                output.Write(generic[0].Count);
                foreach (int stat in generic[0])
                {
                    output.Write(stat);
                }
                output.Write(generic[1].Count);
                foreach (int growth in generic[1])
                {
                    output.Write(growth);
                }
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(FEXNA_Library.ClassReader).AssemblyQualifiedName;
        }
    }
}
