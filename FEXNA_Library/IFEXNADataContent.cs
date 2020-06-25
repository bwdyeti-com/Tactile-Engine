using System;
using System.IO;

namespace FEXNA_Library
{
    public interface IFEXNADataContent : ICloneable
    {
        IFEXNADataContent EmptyInstance();
        void Read(BinaryReader input);
        void Write(BinaryWriter output);
    }

    public interface IFEXNADataContentStruct : ICloneable
    {
        IFEXNADataContentStruct Read(BinaryReader input);
        void Write(BinaryWriter output);
    }
}
