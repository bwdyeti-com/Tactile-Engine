using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FEXNA_Library;

namespace FEXNA.IO.Serialization
{
    class SerializerData
    {
        private Dictionary<SerializerMetaData, object> ObjectData = new Dictionary<SerializerMetaData, object>();

        /// <summary>
        /// Gets a read value from deserialized data and sets it on a parameter.
        /// </summary>
        /// <typeparam name="T">The type to cast the value to.</typeparam>
        /// <param name="value">The reference to set the value on.</param>
        /// <param name="name">The name of the value to search for.</param>
        public void ReadValue<T>(out T value, string name)
        {
            value = (T)ObjectData.First(x => x.Key.Name == name).Value;
        }
        /// <summary>
        /// Gets a read value from deserialized data and sets it on an out parameter.
        /// </summary>
        /// <typeparam name="T">The type to cast the value to.</typeparam>
        /// <param name="value">The output to set the value on.</param>
        /// <param name="name">The name of the value to search for.</param>
        public void ReadAndUpdateValue<T>(ref T value, string name)
        {
            ReadValue(out value, name);
        }

        /// <summary>
        /// Writes <see cref="SerializerData"/> to a stream using a
        /// <see cref="BinaryWriter"/>.
        /// Returns the metadata of the write operation.
        /// </summary>
        public List<SerializerMetaData> Write(BinaryWriter writer)
        {
            writer.Flush();
            var metaData = new List<SerializerMetaData>();

            foreach (var pair in ObjectData)
            {
                long position = writer.BaseStream.Position;

                Write(writer, pair.Value);
                writer.Flush();

                long length = writer.BaseStream.Position - position;

                metaData.Add(pair.Key.SetPosition(position, length));
            }

            return metaData;
        }

        /// <summary>
        /// Writes a data value to a stream.
        /// </summary>
        private void Write(BinaryWriter writer, object value)
        {
            Type type = value.GetType();
            if (type.IsEnum)
            {
                // Write enums as longs to cover any possibility
                long enumValue = Convert.ToInt64(value);
                writer.Write(enumValue);
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.String:
                        writer.Write((string)value);
                        break;
                    case TypeCode.Single:
                        writer.Write((float)value);
                        break;
                    case TypeCode.UInt64:
                        writer.Write((ulong)value);
                        break;
                    case TypeCode.Int64:
                        writer.Write((long)value);
                        break;
                    case TypeCode.UInt32:
                        writer.Write((uint)value);
                        break;
                    case TypeCode.Int32:
                        writer.Write((int)value);
                        break;
                    case TypeCode.UInt16:
                        writer.Write((ushort)value);
                        break;
                    case TypeCode.Int16:
                        writer.Write((short)value);
                        break;
                    case TypeCode.Double:
                        writer.Write((double)value);
                        break;
                    case TypeCode.Char:
                        writer.Write((char)value);
                        break;
                    case TypeCode.SByte:
                        writer.Write((sbyte)value);
                        break;
                    case TypeCode.Byte:
                        writer.Write((byte)value);
                        break;
                    case TypeCode.Boolean:
                        writer.Write((bool)value);
                        break;
                    case TypeCode.Object:
                        WriteObject(writer, value);
                        break;
                    default:
                        throw new ArgumentException("Tried to write SerializerData for a\nprimitive type that is not handled");
                }
            }
        }

        /// <summary>
        /// Writes a non-primitive data value to a stream.
        /// </summary>
        private void WriteObject(BinaryWriter writer, object value)
        {
            Type type = value.GetType();
            // ISerializableGameObject
            if (typeof(ISerializableGameObject).IsAssignableFrom(type))
            {
                ((ISerializableGameObject)value).Write(writer);
            }
            // Maybe<>
            else if (type.IsGenericType &&
                typeof(Maybe<>) == type.GetGenericTypeDefinition())
            {
                Type[] genericTypes = type.GetGenericArguments();
                var resolvedType = typeof(Maybe<>).MakeGenericType(genericTypes);

                string propertyName = nameof(Maybe<object>.IsSomething);
                var isSomethingProperty = resolvedType.GetProperty(
                    propertyName,
                    BindingFlags.Instance | BindingFlags.Public);
                bool isSomething = (bool)isSomethingProperty.GetValue(value, null);

                writer.Write(isSomething);
                if (isSomething)
                {
                    propertyName = nameof(Maybe<object>.ValueOrDefault);
                    var valueOrDefault = resolvedType.GetProperty(
                        propertyName,
                        BindingFlags.Instance | BindingFlags.Public);
                    object maybeValue = valueOrDefault.GetValue(value, null);
                    Write(writer, maybeValue);
                }
            }
            // Array
            else if (typeof(Array).IsAssignableFrom(type))
            {
                // Write dimensions
                int dimensions = type.GetArrayRank();
                writer.Write(dimensions);
                for (int i = 0; i < dimensions; i++)
                    writer.Write(((Array)value).GetLength(i));

                // Write elements
                foreach (object o in ((Array)value))
                {
                    Write(writer, o);
                }
            }
            else
            {
                throw new NotImplementedException("Tried to write SerializerData for a\ntype that is not handled");
            }
        }

        /// <summary>
        /// Reads <see cref="SerializerData"/> from a stream using a
        /// <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="metaData">The metadata of the stored data.</param>
        public static SerializerData Read(BinaryReader reader, List<SerializerMetaData> metaData)
        {
            long startPosition = reader.BaseStream.Position;
            var result = new SerializerData();

            foreach (var meta in metaData)
            {
                reader.BaseStream.Seek(startPosition + meta.Position, SeekOrigin.Begin);
                object value = result.Read(reader, meta.Type);

                result.ObjectData.Add(meta, value);
            }

            return result;
        }

        /// <summary>
        /// Reads a data value from a stream.
        /// </summary>
        private object Read(BinaryReader reader, Type type)
        {
            if (type.IsEnum)
            {
                // Enums write as longs to cover any possibility
                long value = reader.ReadInt64();
                return Enum.ToObject(type, value);
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.String:
                        return reader.ReadString();
                    case TypeCode.Single:
                        return reader.ReadSingle();
                    case TypeCode.UInt64:
                        return reader.ReadUInt64();
                    case TypeCode.Int64:
                        return reader.ReadInt64();
                    case TypeCode.UInt32:
                        return reader.ReadUInt32();
                    case TypeCode.Int32:
                        return reader.ReadInt32();
                    case TypeCode.UInt16:
                        return reader.ReadUInt16();
                    case TypeCode.Int16:
                        return reader.ReadInt16();
                    case TypeCode.Double:
                        return reader.ReadDouble();
                    case TypeCode.Char:
                        return reader.ReadChar();
                    case TypeCode.SByte:
                        return reader.ReadSByte();
                    case TypeCode.Byte:
                        return reader.ReadByte();
                    case TypeCode.Boolean:
                        return reader.ReadBoolean();
                    case TypeCode.Object:
                        string methodName = nameof(SerializerData.ReadObject);
                        var readObjectMethod = typeof(SerializerData).GetMethod(
                            methodName,
                            BindingFlags.Instance | BindingFlags.NonPublic);

                        // Sets ReadObject<T> to use the right generic
                        var genericMethod = readObjectMethod.MakeGenericMethod(type);
                        return genericMethod.Invoke(this, new object[] { reader });
                    default:
                        throw new ArgumentException("Tried to read SerializerData for a\nprimitive type that is not handled");
                }
            }
        }

        /// <summary>
        /// Reads a non-primitive data value from a stream.
        /// </summary>
        private object ReadObject<T>(BinaryReader reader)
        {
            Type type = typeof(T);
            // ISerializableGameObject
            if (typeof(ISerializableGameObject).IsAssignableFrom(type))
            {
                object obj = Activator.CreateInstance(type);
                ISerializableGameObject result = (ISerializableGameObject)obj;
                result.Read(reader);
                return result;
            }
            // Maybe<>
            else if (type.IsGenericType &&
                typeof(Maybe<>) == type.GetGenericTypeDefinition())
            {
                Type[] genericTypes = type.GetGenericArguments();
                var resolvedType = typeof(Maybe<>).MakeGenericType(genericTypes);
                bool isSomething = reader.ReadBoolean();

                if (isSomething)
                {
                    object value = Read(reader, genericTypes[0]);
                    string methodName = "op_Implicit";
                    var opImplicit = resolvedType.GetMethod(
                        methodName,
                        new Type[] { genericTypes[0] });
                    object obj = (T)opImplicit.Invoke(null, new object[] { value });
                    return obj;
                }
                else
                {
                    string propertyName = nameof(Maybe<object>.Nothing);
                    var nothing = resolvedType.GetProperty(
                        propertyName,
                        BindingFlags.Static | BindingFlags.Public);
                    object maybeValue = nothing.GetValue(null, null);
                    return maybeValue;
                }
            }
            // Array
            else if (typeof(Array).IsAssignableFrom(type))
            {
                // Read dimensions
                int dimensions = reader.ReadInt32();
                int[] lengths = new int[dimensions];
                for (int i = 0; i < dimensions; i++)
                {
                    lengths[i] = reader.ReadInt32();
                }

                // Read elements
                object value = ReadArray(reader, lengths, type);
                return value;
            }
            else
            {
                throw new ArgumentException("Tried to read SerializerData for a\ntype that is not handled");
            }
        }

        /// <summary>
        /// Reads an <see cref="Array"/> from a stream.
        /// </summary>
        private object ReadArray(BinaryReader reader, int[] lengths, Type type)
        {
            Type genericType = type.GetElementType();

            // Create array
            Array value = (Array)Activator.CreateInstance(type, lengths.Select(x => (object)x).ToArray());

            long length = 1;
            for (int i = 0; i < lengths.Length; i++)
                length *= lengths[i];

            int[] index = new int[lengths.Length];
            for (int i = 0; i < length; i++)
            {
                GetLinearIndex(value, ref index, i);
                object o = Read(reader, genericType);
                value.SetValue(o, index);
            }

            return value;
        }

        /// <summary>
        /// Gets the multidimensional indices of an array from its linear index.
        /// </summary>
        /// <param name="a">The array to find indices for.</param>
        /// <param name="indices">The indices object to set the result to.</param>
        /// <param name="index">The linear index into the array.</param>
        private static void GetLinearIndex(Array a, ref int[] indices, int index)
        {
            int dimensions = a.Rank;
            for (int i = a.Rank - 1; i >= 0; i--)
            { 
                int length = a.GetLength(i);
                indices[i] = index % length;
                index /= length;
            }
        }

        /// <summary>
        /// A builder for <see cref="SerializerData"/>.
        /// </summary>
        public class Builder
        {
            private Dictionary<SerializerMetaData, object> ObjectData = new Dictionary<SerializerMetaData, object>();

            public Builder Add(string name, object value)
            {
                SerializerMetaData metaData = new SerializerMetaData(
                    name,
                    value.GetType());
                ObjectData.Add(metaData, value);
                return this;
            }

            /// <summary>
            /// Returns the completed <see cref="SerializerData"/>.
            /// </summary>
            /// <returns></returns>
            public SerializerData Build()
            {
                var result = new SerializerData()
                {
                    ObjectData = ObjectData,
                };
                return result;
            }
        }
    }
}
