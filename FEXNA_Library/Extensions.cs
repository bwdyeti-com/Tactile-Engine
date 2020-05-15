using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.IO;

namespace FEXNA_Library
{
    using System.Reflection;
    public class Enum_Values
    {
        public static Enum[] GetEnumValues(Type enumType)
        {
            if (enumType.BaseType == typeof(Enum))
            {
                FieldInfo[] info = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
                Enum[] values = new Enum[info.Length];
                for (int i = 0; i < values.Length; ++i)
                {
                    values[i] = (Enum)info[i].GetValue(null);
                }
                return values;
            }
            else
            {
                throw new Exception("Given type is not an Enum type");
            }
        }

        public static int GetEnumCount(Type enumType)
        {
            if (enumType.BaseType == typeof(Enum))
            {
                FieldInfo[] info = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
                return info.Length;
            }
            else
            {
                throw new Exception("Given type is not an Enum type");
            }
        }
    }
}

namespace EnumExtension
{
    internal static class EnumHelper<T1>
    {
        public static Func<T1, T1, bool> TestOverlapProc = initOverlapProc;
        public static Func<T1, T1, bool> TestHasFlagProc = initHasFlagProc;
        private static Dictionary<Type, Type[]> Signatures =
            new Dictionary<Type, Type[]>();

        public static bool Overlaps(SByte p1, SByte p2) { return (p1 & p2) != 0; }
        public static bool Overlaps(Byte p1, Byte p2) { return (p1 & p2) != 0; }
        public static bool Overlaps(Int16 p1, Int16 p2) { return (p1 & p2) != 0; }
        public static bool Overlaps(UInt16 p1, UInt16 p2) { return (p1 & p2) != 0; }
        public static bool Overlaps(Int32 p1, Int32 p2) { return (p1 & p2) != 0; }
        public static bool Overlaps(UInt32 p1, UInt32 p2) { return (p1 & p2) != 0; }
        public static bool Overlaps(Int64 p1, Int64 p2) { return (p1 & p2) != 0; }
        public static bool Overlaps(UInt64 p1, UInt64 p2) { return (p1 & p2) != 0; }

        public static bool HasFlag(SByte p1, SByte p2) { return (p1 & p2) == p2; }
        public static bool HasFlag(Byte p1, Byte p2) { return (p1 & p2) == p2; }
        public static bool HasFlag(Int16 p1, Int16 p2) { return (p1 & p2) == p2; }
        public static bool HasFlag(UInt16 p1, UInt16 p2) { return (p1 & p2) == p2; }
        public static bool HasFlag(Int32 p1, Int32 p2) { return (p1 & p2) == p2; }
        public static bool HasFlag(UInt32 p1, UInt32 p2) { return (p1 & p2) == p2; }
        public static bool HasFlag(Int64 p1, Int64 p2) { return (p1 & p2) == p2; }
        public static bool HasFlag(UInt64 p1, UInt64 p2) { return (p1 & p2) == p2; }

        public static bool initOverlapProc(T1 p1, T1 p2)
        {
            Type typ1 = typeof(T1);
            Type[] types = get_types(ref typ1);

            var method = typeof(EnumHelper<T1>).GetMethod("Overlaps", types);
            if (method == null)
                method = typeof(T1).GetMethod("Overlaps", types);
            if (method == null)
                throw new MissingMethodException("Unknown type of enum");
            TestOverlapProc = (Func<T1, T1, bool>)Delegate.CreateDelegate(typeof(Func<T1, T1, bool>), method);
            return TestOverlapProc(p1, p2);
        }
        public static bool initHasFlagProc(T1 p1, T1 p2)
        {
            Type typ1 = typeof(T1);
            Type[] types = get_types(ref typ1);

            var method = typeof(EnumHelper<T1>).GetMethod("HasFlag", types);
            if (method == null)
                method = typeof(T1).GetMethod("HasFlag", types);
            if (method == null)
                throw new MissingMethodException("Unknown type of enum");
            TestHasFlagProc = (Func<T1, T1, bool>)Delegate.CreateDelegate(typeof(Func<T1, T1, bool>), method);
            return TestHasFlagProc(p1, p2);
        }

        private static Type[] get_types(ref Type typ1)
        {
            if (typ1.IsEnum)
                typ1 = Enum.GetUnderlyingType(typ1);

            if (!Signatures.ContainsKey(typ1))
                Signatures[typ1] = new Type[] { typ1, typ1 };
            Type[] types = Signatures[typ1];
            return types;
        }
    }

    public static class EnumHelper
    {
        /*
        public static bool Overlaps(this Enum p1, Enum p2)
        {
            if (p1 == null)
                return false;

            if (p2 == null)
                throw new ArgumentNullException("value");

            ulong num = Convert.ToUInt64(p1);
            return ((Convert.ToUInt64(p2) & num) == num);

            //return EnumHelper<Enum>.TestOverlapProc(p1, p2);
        }*/
        public static bool Overlaps<T>(this T p1, T p2) where T : struct
        {
            return EnumHelper<T>.TestOverlapProc(p1, p2);
        }

        public static bool HasEnumFlag<T>(this T p1, T p2) where T : struct
        {
            return EnumHelper<T>.TestHasFlagProc(p1, p2);
        }

        public static T Parse<T>(string name) where T : struct
        {
            T result;
            TryParse(name, out result);
            return result;
        }
        public static bool TryParse<T>(string name, out T result) where T : struct
        {
            Type type = typeof(T);
            if (!type.IsEnum)
                throw new InvalidCastException("Trying to parse an enum, but a non-enum type was provided.");

            if (!Enum.TryParse<T>(name, out result))
                return false;
            return true;
        }
    }
}

namespace ColorExtension
{
    public static class Extension
    {
        public static void write(this Color color, BinaryWriter writer)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
            writer.Write(color.A);
        }

        public static Color read(this Color color, BinaryReader reader)
        {
            color.R = reader.ReadByte();
            color.G = reader.ReadByte();
            color.B = reader.ReadByte();
            color.A = reader.ReadByte();
            return color;
        }
    }
}

namespace Vector2Extension
{
    public static class Extension
    {
        public static void write(this Vector2 vector, BinaryWriter writer)
        {
            writer.Write((double)vector.X);
            writer.Write((double)vector.Y);
        }

        public static Vector2 read(this Vector2 vector, BinaryReader reader)
        {
            vector.X = (float)reader.ReadDouble();
            vector.Y = (float)reader.ReadDouble();
            return vector;
        }
    }
}

namespace RectangleExtension
{
    public static class Extension
    {
        public static void write(this Rectangle rect, BinaryWriter writer)
        {
            writer.Write(rect.X);
            writer.Write(rect.Y);
            writer.Write(rect.Width);
            writer.Write(rect.Height);
        }

        public static Rectangle read(this Rectangle rect, BinaryReader reader)
        {
            rect.X = reader.ReadInt32();
            rect.Y = reader.ReadInt32();
            rect.Width = reader.ReadInt32();
            rect.Height = reader.ReadInt32();
            return rect;
        }
    }
}

namespace ArrayExtension
{
    public static partial class Extension
    {
        // byte[]
        public static void write(this byte[] array, BinaryWriter writer)
        {
            writer.Write(array.Length);
            foreach (byte value in array)
                writer.Write(value);
        }

        public static byte[] read(this byte[] array, BinaryReader reader)
        {
            array = new byte[reader.ReadInt32()];
            for (int i = 0; i < array.Length; i++)
                array[i] = reader.ReadByte();
            return array;
        }

        // int[]
        public static void write(this int[] array, BinaryWriter writer)
        {
            writer.Write(array.Length);
            foreach (int value in array)
                writer.Write(value);
        }

        public static int[] read(this int[] array, BinaryReader reader)
        {
            array = new int[reader.ReadInt32()];
            for (int i = 0; i < array.Length; i++)
                array[i] = reader.ReadInt32();
            return array;
        }

        // int[][]
        public static void write(this int[][] array, BinaryWriter writer)
        {
            writer.Write(array.Length);
            foreach (int[] value in array)
                value.write(writer);
        }

        public static int[][] read(this int[][] array, BinaryReader reader)
        {
            array = new int[reader.ReadInt32()][];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array[i].read(reader);
            }
            return array;
        }

        // bool[]
        public static void write(this bool[] array, BinaryWriter writer)
        {
            writer.Write(array.Length);
            foreach (bool value in array)
                writer.Write(value);
        }

        public static bool[] read(this bool[] array, BinaryReader reader)
        {
            array = new bool[reader.ReadInt32()];
            for (int i = 0; i < array.Length; i++)
                array[i] = reader.ReadBoolean();
            return array;
        }

        // string[]
        public static void write(this string[] array, BinaryWriter writer)
        {
            writer.Write(array.Length);
            foreach (string value in array)
                writer.Write(value);
        }

        public static string[] read(this string[] array, BinaryReader reader)
        {
            array = new string[reader.ReadInt32()];
            for (int i = 0; i < array.Length; i++)
                array[i] = reader.ReadString();
            return array;
        }
    }
}

namespace HashSetExtension
{
    using Vector2Extension;
    public static partial class Extension
    {
        // HashSet<int>
        public static void write(this HashSet<int> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (int value in list)
                writer.Write(value);
        }

        public static void read(this HashSet<int> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(reader.ReadInt32());
        }

        // HashSet<string>
        public static void write(this HashSet<string> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (string value in list)
                writer.Write(value);
        }

        public static void read(this HashSet<string> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(reader.ReadString());
        }

        // HashSet<Vector2>
        public static void write(this HashSet<Vector2> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (Vector2 vector in list)
                vector.write(writer);
        }

        public static void read(this HashSet<Vector2> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            Vector2 v = Vector2.Zero;
            for (int i = 0; i < count; i++)
            {
                list.Add(v.read(reader));
            }
        }
    }
}

namespace ListExtension
{
    using ColorExtension;
    using Vector2Extension;
    using RectangleExtension;
    using ArrayExtension;
    public static partial class Extension
    {
        // List<T>
        public static bool compare<T>(this List<T> list, List<T> other)
        {
            if (list.Count != other.Count)
                return false;
            for (int i = 0; i < list.Count; i++)
                if (!list[i].Equals(other[i]))
                    return false;
            return true;
        }

        public static T pop<T>(this List<T> list)
        {
            if (list.Count == 0)
                throw new IndexOutOfRangeException("Empty list popped");
            T value = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return value;
        }

        public static T shift<T>(this List<T> list)
        {
            if (list.Count == 0)
                throw new IndexOutOfRangeException("Empty list shifted");
            T value = list[0];
            list.RemoveAt(0);
            return value;
        }

        // List<byte>
        public static void write(this List<byte> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (byte value in list)
                writer.Write(value);
        }

        public static void read(this List<byte> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(reader.ReadByte());
        }

        // List<short>
        public static void write(this List<short> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (short value in list)
                writer.Write(value);
        }

        public static void read(this List<short> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(reader.ReadInt16());
        }

        // List<int>
        public static void write(this List<int> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (int value in list)
                writer.Write(value);
        }

        public static void read(this List<int> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(reader.ReadInt32());
        }

        // List<int[]>
        public static void write(this List<int[]> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (int[] array in list)
            {
                array.write(writer);
            }
        }

        public static void read(this List<int[]> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            int[] arr = new int[0];
            for (int i = 0; i < count; i++)
            {
                list.Add(arr.read(reader));
            }
        }

        // List<List<int>>
        public static void write(this List<List<int>> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (List<int> sub_list in list)
            {
                sub_list.write(writer);
            }
        }

        public static void read(this List<List<int>> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var sub_list = new List<int>();
                sub_list.read(reader);
                list.Add(sub_list);
            }
        }

        // List<string>
        public static void write(this List<string> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (string value in list)
                writer.Write(value);
        }

        public static void read(this List<string> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(reader.ReadString());
        }

        // List<string[]>
        public static void write(this List<string[]> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (string[] array in list)
            {
                writer.Write(array.Length);
                foreach (string value in array)
                    writer.Write(value);
            }
        }

        public static void read(this List<string[]> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int array_size = reader.ReadInt32();
                list.Add(new string[array_size]);
                for (int j = 0; j < array_size; j++)
                    list[list.Count - 1][j] = reader.ReadString();
            }
        }

        // List<Vector2>
        public static void write(this List<Vector2> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (Vector2 vector in list)
                vector.write(writer);
        }

        public static void read(this List<Vector2> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            Vector2 v = Vector2.Zero;
            for (int i = 0; i < count; i++)
            {
                list.Add(v.read(reader));
            }
        }

        // List<Color>
        public static void write(this List<Color> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (Color color in list)
                color.write(writer);
        }

        public static void read(this List<Color> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            Color color = Color.Black;
            for (int i = 0; i < count; i++)
            {
                list.Add(color.read(reader));
            }
        }

        // List<Rectangle>
        public static void write(this List<Rectangle> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (Rectangle rect in list)
                rect.write(writer);
        }

        public static void read(this List<Rectangle> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list.Add(new Rectangle());
                list[list.Count - 1] = list[list.Count - 1].read(reader);
            }
        }

        // List<ClassTypes>
        public static void write(this List<FEXNA_Library.ClassTypes> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (int value in list)
                writer.Write((int)value);
        }

        public static void read(this List<FEXNA_Library.ClassTypes> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add((FEXNA_Library.ClassTypes)reader.ReadInt32());
        }

        // List<Data_Class_Skill>
        public static void write(this List<FEXNA_Library.Data_Class_Skill> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Data_Class_Skill value in list)
                value.write(writer);
        }

        public static void read(this List<FEXNA_Library.Data_Class_Skill> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(FEXNA_Library.Data_Class_Skill.read(reader));
        }

        // List<Item_Data>
        public static void write(this List<FEXNA_Library.Item_Data> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Item_Data value in list)
                value.write(writer);
        }

        public static void read(this List<FEXNA_Library.Item_Data> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(FEXNA_Library.Item_Data.read(reader));
        }

        // List<ShopItemData>
        public static void write(this List<FEXNA_Library.ShopItemData> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.ShopItemData value in list)
                value.write(writer);
        }

        public static void read(this List<FEXNA_Library.ShopItemData> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(FEXNA_Library.ShopItemData.read(reader));
        }

        // List<Battle_Frame_Data>
        public static void write(this List<FEXNA_Library.Battle_Frame_Data> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Battle_Frame_Data frame_data in list)
                frame_data.write(writer);
        }

        public static void read(this List<FEXNA_Library.Battle_Frame_Data> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list.Add(new FEXNA_Library.Battle_Frame_Data());
                list[list.Count - 1].read(reader);
            }
        }

        // List<Battle_Frame_Image_Data>
        public static void write(this List<FEXNA_Library.Battle_Frame_Image_Data> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Battle_Frame_Image_Data image_data in list)
                image_data.write(writer);
        }

        public static void read(this List<FEXNA_Library.Battle_Frame_Image_Data> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list.Add(FEXNA_Library.Battle_Frame_Image_Data.read(reader));
            }
        }

        // List<Sound_Data>
        public static void write(this List<FEXNA_Library.Sound_Data> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Sound_Data value in list)
                value.write(writer);
        }

        public static void read(this List<FEXNA_Library.Sound_Data> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(FEXNA_Library.Sound_Data.read(reader));
        }

        // List<Battle_Animation_Tween_Data>
        public static void write(this List<FEXNA_Library.Battle_Animation_Tween_Data> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Battle_Animation_Tween_Data value in list)
                value.write(writer);
        }

        public static void read(this List<FEXNA_Library.Battle_Animation_Tween_Data> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(FEXNA_Library.Battle_Animation_Tween_Data.read(reader));
        }

        // List<Event_Data>
        public static void write(this List<FEXNA_Library.Event_Data> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Event_Data value in list)
                value.write(writer);
        }

        public static void read(this List<FEXNA_Library.Event_Data> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(FEXNA_Library.Event_Data.read(reader));
        }

        // List<Event_Control>
        public static void write(this List<FEXNA_Library.Event_Control> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Event_Control value in list)
                value.write(writer);
        }

        public static void read(this List<FEXNA_Library.Event_Control> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(FEXNA_Library.Event_Control.read(reader));
        }

        // List<FEXNA_Library.Support_Entry>
        public static void write(this List<FEXNA_Library.Support_Entry> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Support_Entry value in list)
                value.write(writer);
        }

        public static void read(this List<FEXNA_Library.Support_Entry> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(FEXNA_Library.Support_Entry.read(reader));
        }
        
        // List<FEXNA_Library.Battler.Battle_Animation_Attack_Set>
        public static void write(this List<FEXNA_Library.Battler.Battle_Animation_Attack_Set> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Battler.Battle_Animation_Attack_Set value in list)
                value.Write(writer);
        }

        public static void read(this List<FEXNA_Library.Battler.Battle_Animation_Attack_Set> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(FEXNA_Library.Battler.Battle_Animation_Attack_Set.Read(reader));
        }

        // List<FEXNA_Library.Battler.Battle_Animation_Variable_Set>
        public static void write(this List<FEXNA_Library.Battler.Battle_Animation_Variable_Set> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Battler.Battle_Animation_Variable_Set value in list)
                value.Write(writer);
        }

        public static void read(this List<FEXNA_Library.Battler.Battle_Animation_Variable_Set> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(FEXNA_Library.Battler.Battle_Animation_Variable_Set.Read(reader));
        }

        // List<Data_Unit>
        public static void write(this List<FEXNA_Library.Data_Unit> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (FEXNA_Library.Data_Unit value in list)
                value.write(writer);
        }

        public static void read(this List<FEXNA_Library.Data_Unit> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(FEXNA_Library.Data_Unit.read(reader));
        }
    }
}

namespace ArrayExtension
{
    using HashSetExtension;
    using ListExtension;

    public static partial class Extension
    {
        // List<int>[]
        public static void write(this List<int>[] array, BinaryWriter writer)
        {
            writer.Write(array.Length);
            foreach (List<int> value in array)
                value.write(writer);
        }

        public static List<int>[] read(this List<int>[] array, BinaryReader reader)
        {
            array = new List<int>[reader.ReadInt32()];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new List<int>();
                array[i].read(reader);
            }
            return array;
        }

        // HashSet<int>[]
        public static void write(this HashSet<int>[] array, BinaryWriter writer)
        {
            writer.Write(array.Length);
            foreach (HashSet<int> value in array)
                value.write(writer);
        }

        public static HashSet<int>[] read(this HashSet<int>[] array, BinaryReader reader)
        {
            array = new HashSet<int>[reader.ReadInt32()];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new HashSet<int>();
                array[i].read(reader);
            }
            return array;
        }
    }
}

namespace ListExtension
{
    using ArrayExtension;

    public static partial class Extension
    {
        // List<List<int>[]>
        public static void write(this List<List<int>[]> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (List<int>[] array in list)
            {
                array.write(writer);
            }
        }

        public static void read(this List<List<int>[]> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list.Add(new List<int>[0].read(reader));
            }
        }
    }
}

namespace DictionaryExtension
{
    using Microsoft.Xna.Framework.Content;
    using ArrayExtension;
    using Vector2Extension;
    using ListExtension;
    using HashSetExtension;

    public static class Extensions
    {
        // Dictionary<byte, List<short>>
        public static void write(this Dictionary<byte, List<short>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<byte, List<short>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<byte, List<short>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                byte id = reader.ReadByte();
                List<short> list = new List<short>();
                list.read(reader);
                dictionary.Add(id, list);
            }
        }

        // Dictionary<short, short>
        public static void write(this Dictionary<short, short> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<short, short> pair in dictionary)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }

        public static void read(this Dictionary<short, short> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                short key = reader.ReadInt16();
                short value = reader.ReadInt16();
                dictionary.Add(key, value);
            }
        }

        // Dictionary<int, int>
        public static void write(this Dictionary<int, int> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, int> pair in dictionary)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }

        public static void read(this Dictionary<int, int> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int key = reader.ReadInt32();
                int value = reader.ReadInt32();
                dictionary.Add(key, value);
            }
        }

        // Dictionary<int,string>
        public static void write(this Dictionary<int, string> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, string> pair in dictionary)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }

        public static void read(this Dictionary<int, string> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                string str = reader.ReadString();
                dictionary.Add(id, str);
            }
        }

        // Dictionary<int, List<int>>
        public static void write(this Dictionary<int, List<int>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, List<int>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, List<int>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                List<int> list = new List<int>();
                list.read(reader);
                dictionary.Add(id, list);
            }
        }

        // Dictionary<int, List<int>[]>
        public static void write(this Dictionary<int, List<int>[]> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, List<int>[]> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, List<int>[]> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                List<int>[] array = new List<int>[0];
                dictionary.Add(id, array.read(reader));
            }
        }

        // Dictionary<int, List<Rectangle>>
        public static void write(this Dictionary<int, List<Rectangle>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, List<Rectangle>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, List<Rectangle>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                List<Rectangle> list = new List<Rectangle>();
                list.read(reader);
                dictionary.Add(id, list);
            }
        }

        // Dictionary<int, HashSet<int>>
        public static void write(this Dictionary<int, HashSet<int>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, HashSet<int>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, HashSet<int>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                HashSet<int> hashset = new HashSet<int>();
                hashset.read(reader);
                dictionary.Add(id, hashset);
            }
        }

        // Dictionary<int, HashSet<string>>
        public static void write(this Dictionary<int, HashSet<string>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, HashSet<string>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, HashSet<string>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                HashSet<string> hashset = new HashSet<string>();
                hashset.read(reader);
                dictionary.Add(id, hashset);
            }
        }

        // Dictionary<int, List<FEXNA_Library.Data_Support>>
        public static void write(this Dictionary<int, List<FEXNA_Library.Support_Entry>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, List<FEXNA_Library.Support_Entry>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, List<FEXNA_Library.Support_Entry>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                List<FEXNA_Library.Support_Entry> list = new List<FEXNA_Library.Support_Entry>();
                list.read(reader);
                dictionary.Add(id, list);
            }
        }

        // Dictionary<string, bool>
        public static void write(this Dictionary<string, bool> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<string, bool> pair in dictionary)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }

        public static void read(this Dictionary<string, bool> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                bool value = reader.ReadBoolean();
                dictionary.Add(key, value);
            }
        }

        // Dictionary<string, int>
        public static void write(this Dictionary<string, int> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<string, int> pair in dictionary)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }

        public static void read(this Dictionary<string, int> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                int value = reader.ReadInt32();
                dictionary.Add(key, value);
            }
        }

        // Dictionary<string, string>
        public static void write(this Dictionary<string, string> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<string, string> pair in dictionary)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }

        public static void read(this Dictionary<string, string> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                string value = reader.ReadString();
                dictionary.Add(key, value);
            }
        }

        // Dictionary<Vector2,Vector2>
        public static void write(this Dictionary<Vector2, Vector2> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<Vector2, Vector2> pair in dictionary)
            {
                pair.Key.write(writer);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<Vector2, Vector2> dictionary, BinaryReader reader)
        {
            dictionary.Clear(); // I don't think this is coded right, but it's not used anywhere... //Yeti
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                Vector2 key = Vector2.Zero;
                Vector2 value = Vector2.Zero;
                key.read(reader);
                value.read(reader);
                dictionary.Add(key, value);
            }
        }

        // Dictionary<Vector2, Data_Unit>
        public static void write(this Dictionary<Vector2, FEXNA_Library.Data_Unit> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<Vector2, FEXNA_Library.Data_Unit> pair in dictionary)
            {
                pair.Key.write(writer);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<Vector2, FEXNA_Library.Data_Unit> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                //int id = reader.ReadInt32();
                Vector2 key = Vector2.Zero;
                key = key.read(reader);
                FEXNA_Library.Data_Unit value = FEXNA_Library.Data_Unit.read(reader);
                dictionary.Add(key, value);
            }
        }

        // Dictionary<FEXNA_Library.Battler.AttackAnims, List<int>>
        public static void write(this Dictionary<FEXNA_Library.Battler.AttackAnims, List<int>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<FEXNA_Library.Battler.AttackAnims, List<int>> pair in dictionary)
            {
                writer.Write((short)pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<FEXNA_Library.Battler.AttackAnims, List<int>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var key = (FEXNA_Library.Battler.AttackAnims)reader.ReadInt16();
                List<int> list = new List<int>();
                list.read(reader);
                dictionary.Add(key, list);
            }
        }
    }
}

namespace IntExtension
{
    public static class Extension
    {
        public static List<int> list_add(this int value, List<int> list)
        {
            if (list == null)
                return new List<int>();
            List<int> result = new List<int>(list);
            for (int i = 0; i < result.Count; i++)
                result[i] += value;
            return result;
        }
    }
}

namespace FEXNAContentExtension
{
    using IFEXNADataContent = FEXNA_Library.IFEXNADataContent;
    using IFEXNADataContentStruct = FEXNA_Library.IFEXNADataContentStruct;

    public static class Extension
    {
        // List<IFEXNADataContent>
        public static void Write<T>(this BinaryWriter writer, List<T> list) where T : IFEXNADataContent
        {
            writer.Write(list.Count);
            foreach (T value in list)
                value.Write(writer);
        }

        public static void ReadFEXNAContent<T>(this BinaryReader reader, List<T> list, T existingInstance) where T : IFEXNADataContent
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                T item = (T)existingInstance.EmptyInstance();
                item.Read(reader);
                list.Add(item);
            }
        }

        // Dictionary<int, IFEXNADataContent>
        public static void Write<T>(this BinaryWriter writer, Dictionary<int, T> dictionary) where T : IFEXNADataContent
        {
            writer.Write(dictionary.Count);
            foreach (var pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.Write(writer);
            }
        }

        public static void ReadFEXNAContent<T>(this BinaryReader reader, Dictionary<int, T> dictionary, T existingInstance) where T : IFEXNADataContent
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int key = reader.ReadInt32();
                T item = (T)existingInstance.EmptyInstance();
                item.Read(reader);
                dictionary.Add(key, item);
            }
        }

        // Dictionary<string, IFEXNADataContent>
        public static void Write<T>(this BinaryWriter writer, Dictionary<string, T> dictionary) where T : IFEXNADataContent
        {
            writer.Write(dictionary.Count);
            foreach (var pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.Write(writer);
            }
        }

        public static void ReadFEXNAContent<T>(this BinaryReader reader, Dictionary<string, T> dictionary, T existingInstance) where T : IFEXNADataContent
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                T item = (T)existingInstance.EmptyInstance();
                item.Read(reader);
                dictionary.Add(key, item);
            }
        }

        // List<IFEXNADataContentStruct>
        public static void WriteStruct<T>(this BinaryWriter writer, List<T> list) where T : IFEXNADataContentStruct
        {
            writer.Write(list.Count);
            foreach (T value in list)
                value.Write(writer);
        }

        public static void ReadFEXNAContentStruct<T>(this BinaryReader reader, List<T> list) where T : struct, IFEXNADataContentStruct
        {
            list.Clear();
            int count = reader.ReadInt32();
            T existingInstance = default(T);
            for (int i = 0; i < count; i++)
            {
                var item = (T)existingInstance.Read(reader);
                list.Add(item);
            }
        }

    }
}