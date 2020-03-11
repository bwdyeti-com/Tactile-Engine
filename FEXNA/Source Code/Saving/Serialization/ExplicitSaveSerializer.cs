using System;
using System.Collections.Generic;
using System.IO;

namespace FEXNA.IO.Serialization
{
	class ExplicitSaveSerializer : SaveSerializer
    {
		protected override void WriteMetaData(BinaryWriter writer, SerializerMetaData meta)
		{
			writer.Write(meta.Name);
			writer.Write(meta.Position);
			writer.Write(meta.Length);
		}

		protected override SerializerMetaData ReadMetaData(
			BinaryReader reader,
			Dictionary<string, Type> expected)
		{
			string name = reader.ReadString();
			if (!expected.ContainsKey(name))
				throw new ArgumentException("Save data loaded a value with an unexpected name");
			Type type = expected[name];
			long position = reader.ReadInt64();
			long length = reader.ReadInt64();

			SerializerMetaData meta = new SerializerMetaData(name, type);
			meta = meta.SetPosition(position, length);
			return meta;
		}
	}
}
