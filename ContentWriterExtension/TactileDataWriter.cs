﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ArrayExtension;
using ListExtension;
using Vector2Extension;

using TWrite = TactileLibrary.TactileDataContent;

namespace ContentWriterExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class TactileDataWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {
            value.Write(output);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(TactileLibrary.TactileDataReader).AssemblyQualifiedName;
        }
    }
}
