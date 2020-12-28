using System;
using System.Collections.Generic;
using System.IO;
using TactileVersionExtension;

namespace Tactile.IO.Serialization
{
    enum SaveSerialization { ExplicitTypes, ImplicitTypes }

    abstract class SaveSerializer
    {
        /// <summary>
        /// Writes an <see cref="ISerializableGameObject"/> to stream using a
        /// BinaryWriter.
        /// </summary>
        /// <param name="type">The <see cref="SaveSerialization"/> method to use.</param>
        public static void Write(
            BinaryWriter writer,
            ISerializableGameObject gameObject,
            SaveSerialization type)
        {
            writer.Write((byte)type);

            SaveSerializer serializer;
            switch (type)
            {
                case SaveSerialization.ExplicitTypes:
                default:
                    serializer = new ExplicitSaveSerializer();
                    break;
                case SaveSerialization.ImplicitTypes:
                    throw new NotImplementedException();
            }

            serializer.WriteGameObject(writer, gameObject.GetSaveData());
        }

        /// <summary>
        /// Reads serialized data into an <see cref="ISerializableGameObject"/>
        /// from stream using a BinaryReader.
        /// </summary>
        public static void Read(
            BinaryReader reader,
            ISerializableGameObject gameObject)
        {
            SaveSerialization type = (SaveSerialization)reader.ReadByte();

            SaveSerializer serializer;
            switch (type)
            {
                case SaveSerialization.ExplicitTypes:
                default:
                    serializer = new ExplicitSaveSerializer();
                    break;
                case SaveSerialization.ImplicitTypes:
                    throw new NotImplementedException();
            }

            serializer.ReadGameObject(reader, gameObject);
        }

        /// <summary>
        /// Writes an <see cref="ISerializableGameObject"/> to stream using a
        /// BinaryWriter.
        /// </summary>
        /// <param name="type">The <see cref="SaveSerialization"/> method to use.</param>
        private void WriteGameObject(BinaryWriter writer, SerializerData data)
        {
            writer.Write(Global.RUNNING_VERSION);
            writer.Flush();

            using (MemoryStream dataStream = new MemoryStream())
            using (BinaryWriter dataWriter = new BinaryWriter(dataStream))
            {
                // Write data to a temporary stream and get meta data
                var metaData = data.Write(dataWriter);
                dataWriter.Flush();

                // Write meta data
                WriteAllMetaData(writer, metaData);

                // Write data length
                writer.Write(dataStream.Length);
                writer.Flush();

                // Write data
                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.CopyTo(writer.BaseStream);
                writer.Flush();
            }
        }

        /// <summary>
        /// Reads serialized data into an <see cref="ISerializableGameObject"/>
        /// from stream using a BinaryReader.
        /// </summary>
        private void ReadGameObject(BinaryReader reader, ISerializableGameObject gameObject)
        {
            Version readVersion = reader.ReadVersion();

            // Read meta data
            var metaData = ReadAllMetaData(reader, gameObject.ExpectedData(readVersion));

            // Read data length
            long length = reader.ReadInt64();

            // Read data
            long startPosition = reader.BaseStream.Position;
            var data = SerializerData.Read(reader, metaData);
            reader.BaseStream.Seek(startPosition + length, SeekOrigin.Begin);

            gameObject.UpdateReadValues(readVersion, data);
            gameObject.SetReadValues(data);
        }

        /// <summary>
        /// Writes the metadata generated from an
        /// <see cref="ISerializableGameObject"/>.
        /// </summary>
        private void WriteAllMetaData(BinaryWriter writer, List<SerializerMetaData> metaData)
        {
            // Write count
            writer.Write(metaData.Count);

            // Iterate through data
            foreach (var meta in metaData)
            {
                WriteMetaData(writer, meta);
            }
        }
        /// <summary>
        /// Writes one value's metadata.
        /// </summary>
        protected abstract void WriteMetaData(BinaryWriter writer, SerializerMetaData meta);

        /// <summary>
        /// Reads the metadata generated from an
        /// <see cref="ISerializableGameObject"/>.
        /// </summary>
        private List<SerializerMetaData> ReadAllMetaData(
            BinaryReader reader,
            Dictionary<string, Type> expected)
        {
            var metaData = new List<SerializerMetaData>();

            // Read count
            int count = reader.ReadInt32();

            // Read data
            for (int i = 0; i < count; i++)
            {
                SerializerMetaData meta = ReadMetaData(reader, expected);
                metaData.Add(meta);
            }

            return metaData;
        }
        /// <summary>
        /// Reads one value's metadata.
        /// </summary>
        protected abstract SerializerMetaData ReadMetaData(
            BinaryReader reader,
            Dictionary<string, Type> expected);
    }
}
