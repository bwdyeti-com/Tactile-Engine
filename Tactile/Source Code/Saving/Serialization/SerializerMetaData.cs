using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tactile.IO.Serialization
{
    struct SerializerMetaData
    {
        /// <summary>
        /// The name of the stored value.
        /// Generally the same as the field that has the value.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The <see cref="System.Type"/> of the stored value.
        /// Used to reconstruct the original object from a stream of bytes.
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// The position in the stream of the value as bytes.
        /// Will be -1 if the value has not yet been written.
        /// </summary>
        public long Position { get; private set; }
        /// <summary>
        /// The length of the value as bytes.
        /// Will be -1 if the value has not yet been written.
        /// </summary>
        public long Length { get; private set; }

        public SerializerMetaData(string name, Type type) : this()
        {
            Name = name;
            Type = type;
            Position = -1;
            Length = -1;
        }
        private SerializerMetaData(string name, Type type, long position, long length) : this()
        {
            Name = name;
            Type = type;
            Position = position;
            Length = length;
        }

        /// <summary>
        /// Gets new metadata copied from this one with the position and length set.
        /// </summary>
        public SerializerMetaData SetPosition(long position, long length)
        {
            return new SerializerMetaData(Name, Type, position, length);
        }

        public override string ToString()
        {
            string format = "SerializerMetaData: {0}";
            if (Position >= 0)
                format = "SerializerMetaData: {0} - {2}, {3}";

            return string.Format(format,
                Name, this.TypeName, Position, Length);
        }

        /// <summary>
        /// A string representation of the value's type.
        /// Used to reconstruct the value's <see cref="System.Type"/>.
        /// </summary>
        public string TypeName
        {
            get
            {
                string asmName = Type.Assembly.GetName().Name;
                return string.Format("{0}, {1}", Type, asmName);
            }
        }
    }
}
