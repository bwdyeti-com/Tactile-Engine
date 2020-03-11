using System;
using System.Collections.Generic;
using System.IO;

namespace FEXNA.IO.Serialization
{
    interface ISerializableGameObject
    {
        /// <summary>
        /// Writes this object to stream a using a <see cref="BinaryWriter"/>.
        /// </summary>
        void Write(BinaryWriter writer);

        /// <summary>
        /// Reads into this object from a stream using a
        /// <see cref="BinaryReader"/>.
        /// </summary>
        void Read(BinaryReader reader);

        /// <summary>
        /// Updates the read <see cref="SerializerData"/>.
        /// Should result in data that can be passed successfully into
        /// <see cref="SetReadValues(SerializerData)"/>.
        /// </summary>
        /// <param name="v">The <see cref="Version"/> the data was saved in.</param>
        /// <param name="data">The <see cref="SerializerData"/> to update.</param>
        void UpdateReadValues(Version v, SerializerData data);

        /// <summary>
        /// Updates the values of this object using read
        /// <see cref="SerializerData"/>.
        /// </summary>
        void SetReadValues(SerializerData data);

        /// <summary>
        /// Converts the values of this object into a
        /// <see cref="SerializerData"/> that can be serialized.
        /// </summary>
        /// <returns></returns>
        SerializerData GetSaveData();

        /// <summary>
        /// Returns the names and types of values that are expected when
        /// loading from the given <see cref="Version"/>.
        /// </summary>
        Dictionary<string, Type> ExpectedData(Version version);
    }
}
