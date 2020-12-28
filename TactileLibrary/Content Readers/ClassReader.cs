using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using ListExtension;

using TRead = TactileLibrary.Data_Class;

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
    public class ClassReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            existingInstance = new TRead();
            // Id
            existingInstance.Id = input.ReadInt32();
            // Name
            existingInstance.Name = input.ReadString();
            // Class Type
            existingInstance.Class_Types = new List<ClassTypes>();
            int class_types = input.ReadInt32();
            for (int i = 0; i < class_types; i++)
            {
                existingInstance.Class_Types.Add((ClassTypes)input.ReadInt32());
            }
            // Skills
            existingInstance.Skills.read(input);
            // Description
            existingInstance.Description = input.ReadString();
            // Caps
            bool has_caps = input.ReadBoolean();
            if (has_caps)
            {
                int caps = input.ReadInt32();
                existingInstance.Caps = new List<int>[2];
                existingInstance.Caps[0] = new List<int>();
                for (int i = 0; i < caps; i++)
                {
                    existingInstance.Caps[0].Add(input.ReadInt32());
                }
                existingInstance.Caps[1] = new List<int>();
                for (int i = 0; i < caps; i++)
                {
                    existingInstance.Caps[1].Add(input.ReadInt32());
                }
            }
            else
                existingInstance.Caps = null;
            // Max Wlvl
            existingInstance.Max_WLvl = new List<int>();
            int wlvls = input.ReadInt32();
            for (int i = 0; i < wlvls; i++)
            {
                existingInstance.Max_WLvl.Add(input.ReadInt32());
            }
            // Promotions
            existingInstance.Promotion = new Dictionary<int, List<int>[]>();
            int pairs = input.ReadInt32();
            for (int i = 0; i < pairs; i++)
            {
                int key = input.ReadInt32();
                List<int>[] value = new List<int>[2] { new List<int>(), new List<int>() };
                int stat_length = input.ReadInt32();
                for (int j = 0; j < stat_length; j++)
                {
                    value[0].Add(input.ReadInt32());
                }
                int wlvl_length = input.ReadInt32();
                for (int j = 0; j < wlvl_length; j++)
                {
                    value[1].Add(input.ReadInt32());
                }
                existingInstance.Promotion.Add(key, value);
            }
            // Tier
            existingInstance.Tier = input.ReadInt32();
            // Mov
            existingInstance.Mov = input.ReadInt32();
            // Mov Cap
            existingInstance.Mov_Cap = input.ReadInt32();
            // Movement Type
            existingInstance.Movement_Type = (MovementTypes)input.ReadInt32();
            // Generics
            existingInstance.Generic_Stats = new List<List<int>[]>();
            int generics = input.ReadInt32();
            for (int i = 0; i < generics; i++)
            {
                List<int>[] value = new List<int>[2] { new List<int>(), new List<int>() };
                int stat_length = input.ReadInt32();
                for (int j = 0; j < stat_length; j++)
                {
                    value[0].Add(input.ReadInt32());
                }
                int growths_length = input.ReadInt32();
                for (int j = 0; j < growths_length; j++)
                {
                    value[1].Add(input.ReadInt32());
                }
                existingInstance.Generic_Stats.Add(value);
            }

            return existingInstance;
        }
    }
}
