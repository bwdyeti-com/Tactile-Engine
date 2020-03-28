using System;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace FEXNA_Library
{
    public interface IFEXNADataContent : ICloneable
    {
        IFEXNADataContent Read_Content(ContentReader input);
        void Write(BinaryWriter output);
    }
}
