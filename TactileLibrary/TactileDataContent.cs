using System;
using System.IO;

namespace TactileLibrary
{
    public abstract class TactileDataContent : ICloneable
    {
        public abstract TactileDataContent EmptyInstance();
        public abstract void CopyFrom(TactileDataContent other);

        /// <summary>
        /// Checks if this object and the given <see cref="TactileDataContent"/> are
        /// the same class, for use before copying data between them.
        /// </summary>
        /// <param name="other">The other <see cref="TactileDataContent"/> object.</param>
        protected bool CheckSameClass(TactileDataContent other)
        {
            if (other.GetType() != this.GetType())
            {
                throw new ArgumentException(
                    "Tried to copy different types of content.");

                return false;
            }

            return true;
        }

        public abstract void Read(BinaryReader input);
        public abstract void Write(BinaryWriter output);

        public abstract object Clone();
    }

    public interface ITactileDataContentStruct : ICloneable
    {
        ITactileDataContentStruct Read(BinaryReader input);
        void Write(BinaryWriter output);
    }
}
