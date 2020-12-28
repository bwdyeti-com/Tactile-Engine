using System;
using System.IO;
#if DEBUG
using System.Diagnostics;
#endif
using ArrayExtension;

namespace Tactile
{
    enum Event_Variable_Types { Boolean, Integer }
    class Event_Variable_Data<T>
    {
        bool[] Modified;
        T[] Data;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Modified.Length);
            Event_Variable_Types type;
            if (typeof(T) == typeof(bool))
                type = Event_Variable_Types.Boolean;
            else
                type = Event_Variable_Types.Integer;
            writer.Write((int)type);
            Modified.write(writer);

            if (type == Event_Variable_Types.Boolean)
                (Data as bool[]).write(writer);
            else if (type == Event_Variable_Types.Integer)
                (Data as int[]).write(writer);
        }

        public static Event_Variable_Data<T> read(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            Event_Variable_Types type = (Event_Variable_Types)reader.ReadInt32();
            Event_Variable_Data<T> result = new Event_Variable_Data<T>(count);
            result.Modified = result.Modified.read(reader);

            if (type == Event_Variable_Types.Boolean)
            {
#if DEBUG
                Debug.Assert(typeof(T) == typeof(bool));
#endif
                result.Data = ((result.Data as bool[]).read(reader) as T[]);
            }
            else
            {
#if DEBUG
                Debug.Assert(typeof(T) == typeof(int));
#endif
                result.Data = ((result.Data as int[]).read(reader) as T[]);
            }
            return result;
        }
        public static Event_Variable_Data<T> read(BinaryReader reader, int count)
        {
            var loaded_result = read(reader);
            Event_Variable_Data<T> result = new Event_Variable_Data<T>(count);
            copy(loaded_result, result, true);
            return result;
        }
        #endregion

        #region Accessors
        public T this[int index]
        {
            get
            {
                if (Modified[index])
                    return Data[index];
                return default(T);
            }
            set
            {
                Modified[index] = true;
                Data[index] = value;
            }
        }

        public int Length { get { return Modified.Length; } }
        #endregion

        public Event_Variable_Data(int count)
        {
#if DEBUG
            Debug.Assert(typeof(T) == typeof(int) || typeof(T) == typeof(bool));
#endif
            Modified = new bool[count];
            Data = new T[count];
        }

        public static void copy(Event_Variable_Data<T> source, Event_Variable_Data<T> target,
            bool allow_different_length = false, bool overwriteAlreadySet = true)
        {
            if (!allow_different_length && source.Length != target.Length)
                throw new ArgumentException(string.Format("Data length mismatch: {0} vs {1}", source.Length, target.Length));

            int length = Math.Min(source.Length, target.Length);
            for (int i = 0; i < length; i++)
                if (source.Modified[i])
                {
                    if (!overwriteAlreadySet && target.Modified[i])
                        continue;
                    target[i] = source[i];
                }
        }
    }
}
