﻿using Microsoft.Xna.Framework.Content;

using TRead = TactileLibrary.Chapters.TextData;

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
    public class TextDataReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new TRead();

            TRead newInstance = (TRead)existingInstance.EmptyInstance();
            newInstance.Read(input);

            existingInstance.CopyFrom(newInstance);

            return existingInstance;
        }
    }
}
