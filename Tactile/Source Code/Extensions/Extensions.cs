using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.IO;

namespace Tactile
{
    using System.Reflection;
    class Enum_Values
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

    static class Array_Compare
    {
        public static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }
    }
}

namespace TactileArrayExtension
{
    using HashSetExtension;
    using ListExtension;
    using DictionaryExtension;

    public static partial class Extension
    {
        // int[,]
        public static void write(this int[,] ary, BinaryWriter writer)
        {
            writer.Write(ary.GetLength(0));
            writer.Write(ary.GetLength(1));
            for (int j = 0; j < ary.GetLength(1); j++)
            {
                for (int i = 0; i < ary.GetLength(0); i++)
                {
                    writer.Write(ary[i, j]);
                }
            }
        }

        public static int[,] read(this int[,] ary, BinaryReader reader)
        {
            int[,] result = new int[reader.ReadInt32(), reader.ReadInt32()];
            for (int j = 0; j < result.GetLength(1); j++)
            {
                for (int i = 0; i < result.GetLength(0); i++)
                {
                    result[i, j] = reader.ReadInt32();
                }
            }
            return result;
        }

        // HashSet<Vector2>[]
        public static void write(this HashSet<Vector2>[] ary, BinaryWriter writer)
        {
            writer.Write(ary.Length);
            for (int i = 0; i < ary.Length; i++)
            {
                ary[i].write(writer);
            }
        }

        public static HashSet<Vector2>[] read(this HashSet<Vector2>[] ary, BinaryReader reader)
        {
            HashSet<Vector2>[] result = new HashSet<Vector2>[reader.ReadInt32()];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new HashSet<Vector2>();
                result[i].read(reader);
            }
            return result;
        }

        // List<Vector2>[]
        public static void write(this List<Vector2>[] ary, BinaryWriter writer)
        {
            writer.Write(ary.Length);
            for (int i = 0; i < ary.GetLength(0); i++)
            {
                ary[i].write(writer);
            }
        }

        public static List<Vector2>[] read(this List<Vector2>[] ary, BinaryReader reader)
        {
            List<Vector2>[] result = new List<Vector2>[reader.ReadInt32()];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                result[i] = new List<Vector2>();
                result[i].read(reader);
            }
            return result;
        }

        // Item_Data[]
        public static void write(this TactileLibrary.Item_Data[] ary, BinaryWriter writer)
        {
            writer.Write(ary.Length);
            foreach (TactileLibrary.Item_Data value in ary)
                value.write(writer);
        }

        public static TactileLibrary.Item_Data[] read(this TactileLibrary.Item_Data[] ary, BinaryReader reader)
        {
            TactileLibrary.Item_Data[] result = new TactileLibrary.Item_Data[reader.ReadInt32()];
            for (int i = 0; i < result.GetLength(0); i++)
                result[i] = TactileLibrary.Item_Data.read(reader);
            return result;
        }

        // List<Rectangle>[]
        public static void write(this List<Rectangle>[] ary, BinaryWriter writer)
        {
            writer.Write(ary.Length);
            for (int i = 0; i < ary.GetLength(0); i++)
            {
                ary[i].write(writer);
            }
        }

        public static List<Rectangle>[] read(this List<Rectangle>[] ary, BinaryReader reader)
        {
            List<Rectangle>[] result = new List<Rectangle>[reader.ReadInt32()];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                result[i] = new List<Rectangle>();
                result[i].read(reader);
            }
            return result;
        }

        // Dictionary<int, List<Rectangle>[]
        public static void write(this Dictionary<int, List<Rectangle>>[] ary, BinaryWriter writer)
        {
            writer.Write(ary.Length);
            for (int i = 0; i < ary.GetLength(0); i++)
            {
                ary[i].write(writer);
            }
        }

        public static Dictionary<int, List<Rectangle>>[] read(this Dictionary<int, List<Rectangle>>[] ary, BinaryReader reader)
        {
            Dictionary<int, List<Rectangle>>[] result = new Dictionary<int, List<Rectangle>>[reader.ReadInt32()];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                result[i] = new Dictionary<int, List<Rectangle>>();
                result[i].read(reader);
            }
            return result;
        }
    }
}

namespace TactileVector2Extension
{
    static class Extension
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

namespace TactileListExtension
{
    using TactileVector2Extension;
    using RectangleExtension;
    static partial class Extension
    {
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

        // List<Game_Actor>
        public static void write(this List<Tactile.Game_Actor> actors, BinaryWriter writer)
        {
            writer.Write(actors.Count);
            foreach (Tactile.Game_Actor actor in actors)
            {
                actor.write(writer);
            }
        }

        public static void read(this List<Tactile.Game_Actor> actors, BinaryReader reader)
        {
            actors.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                actors.Add(new Tactile.Game_Actor());
                actors[actors.Count - 1].read(reader);
            }
        }

        // List<Battle_Convo>
        public static void write(this List<Tactile.Battle_Convo> convos, BinaryWriter writer)
        {
            writer.Write(convos.Count);
            foreach (Tactile.Battle_Convo convo in convos)
            {
                convo.write(writer);
            }
        }

        public static void read(this List<Tactile.Battle_Convo> convos, BinaryReader reader)
        {
            convos.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                convos.Add(new Tactile.Battle_Convo());
                convos[convos.Count - 1].read(reader);
            }
        }

        // List<Talk_Event>
        public static void write(this List<Tactile.Talk_Event> talks, BinaryWriter writer)
        {
            writer.Write(talks.Count);
            foreach (Tactile.Talk_Event talk in talks)
            {
                talk.write(writer);
            }
        }

        public static void read(this List<Tactile.Talk_Event> talks, BinaryReader reader)
        {
            talks.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                talks.Add(new Tactile.Talk_Event());
                talks[talks.Count - 1].read(reader);
            }
        }

        // List<Torch_Staff_Point>
        public static void write(this List<Tactile.Map.Torch_Staff_Point> torches, BinaryWriter writer)
        {
            writer.Write(torches.Count);
            foreach (Tactile.Map.Torch_Staff_Point torch in torches)
            {
                torch.write(writer);
            }
        }

        public static void read(this List<Tactile.Map.Torch_Staff_Point> torches, BinaryReader reader)
        {
            torches.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                torches.Add(Tactile.Map.Torch_Staff_Point.read(reader));
            }
        }

        // List<Fow_View_Object>
        public static void write(this List<Tactile.Map.Fow_View_Object> torches, BinaryWriter writer)
        {
            writer.Write(torches.Count);
            foreach (Tactile.Map.Fow_View_Object talk in torches)
            {
                talk.write(writer);
            }
        }

        public static void read(this List<Tactile.Map.Fow_View_Object> torches, BinaryReader reader)
        {
            torches.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                torches.Add(Tactile.Map.Fow_View_Object.read(reader));
            }
        }

        // List<Home_Base_Event_Data>
        public static void write(this List<Tactile.Home_Base_Event_Data> base_events, BinaryWriter writer)
        {
            writer.Write(base_events.Count);
            foreach (Tactile.Home_Base_Event_Data event_data in base_events)
            {
                event_data.write(writer);
            }
        }

        public static void read(this List<Tactile.Home_Base_Event_Data> base_events, BinaryReader reader)
        {
            base_events.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                base_events.Add(new Tactile.Home_Base_Event_Data());
                base_events[base_events.Count - 1].read(reader);
            }
        }

        // List<Event_Processor>
        public static void write(this List<Tactile.Event_Processor> events, BinaryWriter writer)
        {
            writer.Write(events.Count);
            foreach (Tactile.Event_Processor processor in events)
            {
                processor.write(writer);
            }
        }

        public static void read(this List<Tactile.Event_Processor> events, BinaryReader reader)
        {
            events.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                events.Add(Tactile.Event_Processor.read(reader));
            }
        }

        // List<Actor_Metrics>
        public static void write(this List<Tactile.Metrics.Actor_Metrics> actors, BinaryWriter writer)
        {
            writer.Write(actors.Count);
            foreach (Tactile.Metrics.Actor_Metrics processor in actors)
            {
                processor.write(writer);
            }
        }

        public static void read(this List<Tactile.Metrics.Actor_Metrics> actors, BinaryReader reader)
        {
            actors.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                actors.Add(Tactile.Metrics.Actor_Metrics.read(reader));
            }
        }

        // List<Combat_Metrics>
        public static void write(this List<Tactile.Metrics.Combat_Metrics> combats, BinaryWriter writer)
        {
            writer.Write(combats.Count);
            foreach (Tactile.Metrics.Combat_Metrics processor in combats)
            {
                processor.write(writer);
            }
        }

        public static void read(this List<Tactile.Metrics.Combat_Metrics> combats, BinaryReader reader)
        {
            combats.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                combats.Add(Tactile.Metrics.Combat_Metrics.read(reader));
            }
        }

        // List<Combatant_Metric>
        public static void write(this List<Tactile.Metrics.Combatant_Metric> combatants, BinaryWriter writer)
        {
            writer.Write(combatants.Count);
            foreach (Tactile.Metrics.Combatant_Metric processor in combatants)
            {
                processor.write(writer);
            }
        }

        public static void read(this List<Tactile.Metrics.Combatant_Metric> combatants, BinaryReader reader)
        {
            combatants.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                combatants.Add(Tactile.Metrics.Combatant_Metric.read(reader));
            }
        }

        // List<Item_Metrics>
        public static void write(this List<Tactile.Metrics.Item_Metrics> items, BinaryWriter writer)
        {
            writer.Write(items.Count);
            foreach (Tactile.Metrics.Item_Metrics processor in items)
            {
                processor.write(writer);
            }
        }

        public static void read(this List<Tactile.Metrics.Item_Metrics> items, BinaryReader reader)
        {
            items.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                items.Add(Tactile.Metrics.Item_Metrics.read(reader));
            }
        }

        // List<Tactile.TileOutlineData>
        public static void write(this List<Tactile.TileOutlineData> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (Tactile.TileOutlineData data in list)
                data.write(writer);
        }

        public static void read(this List<Tactile.TileOutlineData> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(Tactile.TileOutlineData.read(reader));
        }

        // List<Tactile.Move_Arrow_Data>
        public static void write(this List<Tactile.Move_Arrow_Data> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (Tactile.Move_Arrow_Data data in list)
                data.write(writer);
        }

        public static void read(this List<Tactile.Move_Arrow_Data> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(Tactile.Move_Arrow_Data.read(reader));
        }

        // List<Tactile.Map.EscapePoint>
        public static void write(this List<Tactile.Map.EscapePoint> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (Tactile.Map.EscapePoint data in list)
                data.write(writer);
        }

        public static void read(this List<Tactile.Map.EscapePoint> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(Tactile.Map.EscapePoint.read(reader));
        }

        // List<Tuple<int, bool>>
        public static void write(this List<Tuple<int, bool>> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (Tuple<int, bool> pair in list)
            {
                writer.Write(pair.Item1);
                writer.Write(pair.Item2);
            }
        }

        public static void read(this List<Tuple<int, bool>> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int key = reader.ReadInt32();
                bool value = reader.ReadBoolean();
                list.Add(new Tuple<int, bool>(key, value));
            }
        }

        // List<Tuple<Rectangle, string>>
        public static void write(this List<Tuple<Rectangle, string>> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (Tuple<Rectangle, string> pair in list)
            {
                pair.Item1.write(writer);
                writer.Write(pair.Item2);
            }
        }

        public static void read(this List<Tuple<Rectangle, string>> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Rectangle key = new Rectangle();
                key = key.read(reader);
                string value = reader.ReadString();
                list.Add(new Tuple<Rectangle, string>(key, value));
            }
        }
    }
}

namespace TactileRectangleExtension
{
    static class Extension
    {
        public static Vector4 to_vector4(this Rectangle rect)
        {
            return new Vector4(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
        }
    }
}

namespace TactileDictionaryExtension
{
    using ArrayExtension;
    using HashSetExtension;
    using ListExtension;
    using TactileListExtension;
    using TactileVector2Extension;
    static class Extension
    {
        // Dictionary<int, bool>
        public static void write(this Dictionary<int, bool> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, bool> pair in dictionary)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }

        public static void read(this Dictionary<int, bool> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int key = reader.ReadInt32();
                bool value = reader.ReadBoolean();
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

        // Dictionary<int, string>
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
                int key = reader.ReadInt32();
                string value = reader.ReadString();
                dictionary.Add(key, value);
            }
        }

        // Dictionary<int, Vector2>
        public static void write(this Dictionary<int, Vector2> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, Vector2> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, Vector2> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int key = reader.ReadInt32();
                Vector2 value = Vector2.Zero;
                value = value.read(reader);
                dictionary.Add(key, value);
            }
        }

        // Dictionary<int, Dictionary<int, Vector2>>
        public static void write(this Dictionary<int, Dictionary<int, Vector2>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, Dictionary<int, Vector2>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, Dictionary<int, Vector2>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int key = reader.ReadInt32();
                Dictionary<int, Vector2> value = new Dictionary<int, Vector2>();
                value.read(reader);
                dictionary.Add(key, value);
            }
        }

        // Dictionary<int, Game_Unit>
        public static void write(this Dictionary<int, Tactile.Game_Unit> units, BinaryWriter writer)
        {
            writer.Write(units.Count);
            foreach (KeyValuePair<int, Tactile.Game_Unit> unit in units)
            {
                writer.Write(unit.Key);
                unit.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, Tactile.Game_Unit> units, BinaryReader reader)
        {
            units.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                Tactile.Game_Unit unit = new Tactile.Game_Unit();
                unit.read(reader);
                units.Add(id, unit);
            }
        }

        // Dictionary<int, Destroyable_Object>
        public static void write(this Dictionary<int, Tactile.Destroyable_Object> map_objects, BinaryWriter writer)
        {
            writer.Write(map_objects.Count);
            foreach (KeyValuePair<int, Tactile.Destroyable_Object> map_unit in map_objects)
            {
                writer.Write(map_unit.Key);
                map_unit.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, Tactile.Destroyable_Object> map_objects, BinaryReader reader)
        {
            map_objects.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                Tactile.Destroyable_Object map_object = new Tactile.Destroyable_Object();
                map_object.read(reader);
                map_objects.Add(id, map_object);
            }
        }

        // Dictionary<int, Siege_Engine>
        public static void write(this Dictionary<int, Tactile.Siege_Engine> map_objects, BinaryWriter writer)
        {
            writer.Write(map_objects.Count);
            foreach (KeyValuePair<int, Tactile.Siege_Engine> map_unit in map_objects)
            {
                writer.Write(map_unit.Key);
                map_unit.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, Tactile.Siege_Engine> map_objects, BinaryReader reader)
        {
            map_objects.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                Tactile.Siege_Engine map_object = new Tactile.Siege_Engine();
                map_object.read(reader);
                map_objects.Add(id, map_object);
            }
        }

        // Dictionary<int, LightRune>
        public static void write(this Dictionary<int, Tactile.LightRune> map_objects, BinaryWriter writer)
        {
            writer.Write(map_objects.Count);
            foreach (KeyValuePair<int, Tactile.LightRune> map_unit in map_objects)
            {
                writer.Write(map_unit.Key);
                map_unit.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, Tactile.LightRune> map_objects, BinaryReader reader)
        {
            map_objects.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                Tactile.LightRune map_object = new Tactile.LightRune();
                map_object.read(reader);
                map_objects.Add(id, map_object);
            }
        }

        // Dictionary<int, Game_Actor>
        public static void write(this Dictionary<int, Tactile.Game_Actor> actors, BinaryWriter writer)
        {
            writer.Write(actors.Count);
            foreach (KeyValuePair<int, Tactile.Game_Actor> actor in actors)
            {
                writer.Write(actor.Key);
                actor.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, Tactile.Game_Actor> actors, BinaryReader reader)
        {
            actors.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                Tactile.Game_Actor actor = new Tactile.Game_Actor();
                actor.read(reader);
                actors.Add(id, actor);
            }
        }

        // Dictionary<int, Game_Convoy>
        public static void write(this Dictionary<int, Tactile.Game_Convoy> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, Tactile.Game_Convoy> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, Tactile.Game_Convoy> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int key = reader.ReadInt32();
                Tactile.Game_Convoy value = Tactile.Game_Convoy.read(reader);
                dictionary.Add(key, value);
            }
        }

        // Dictionary<int, HashSet<Vector2>>
        public static void write(this Dictionary<int, HashSet<Vector2>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, HashSet<Vector2>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, HashSet<Vector2>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                HashSet<Vector2> value = new HashSet<Vector2>();
                value.read(reader);
                dictionary.Add(id, value);
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

        // Dictionary<int, List<int[]>>
        public static void write(this Dictionary<int, List<int[]>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, List<int[]>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, List<int[]>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                List<int[]> list = new List<int[]>();
                list.read(reader);
                dictionary.Add(id, list);
            }
        }

        // Dictionary<int, List<Vector2>>
        public static void write(this Dictionary<int, List<Vector2>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, List<Vector2>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, List<Vector2>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                List<Vector2> value = new List<Vector2>();
                value.read(reader);
                dictionary.Add(id, value);
            }
        }

        // Dictionary<int, Tactile.Battalion>
        public static void write(this Dictionary<int, Tactile.Battalion> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, Tactile.Battalion> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, Tactile.Battalion> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                Tactile.Battalion battalion = Tactile.Battalion.read(reader);
                dictionary.Add(id, battalion);
            }
        }

        // Dictionary<int, Dictionary<Vector2, Vector2>>
        public static void write(this Dictionary<int, Dictionary<Vector2, Vector2>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, Dictionary<Vector2, Vector2>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, Dictionary<Vector2, Vector2>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                Dictionary<Vector2, Vector2> value = new Dictionary<Vector2, Vector2>();
                value.read(reader);
                dictionary.Add(id, value);
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

        // Dictionary<string, Game_Ranking>
        public static void write(this Dictionary<string, Tactile.Game_Ranking> rankings, BinaryWriter writer)
        {
            writer.Write(rankings.Count);
            foreach (KeyValuePair<string, Tactile.Game_Ranking> ranking in rankings)
            {
                writer.Write(ranking.Key);
                ranking.Value.write(writer);
            }
        }

        public static void read(this Dictionary<string, Tactile.Game_Ranking> rankings, BinaryReader reader)
        {
            rankings.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                Tactile.Game_Ranking ranking = Tactile.Game_Ranking.read(reader);
                rankings.Add(key, ranking);
            }
        }

        // Dictionary<Vector2, string[]>
        public static void write(this Dictionary<Vector2, string[]> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<Vector2, string[]> pair in dictionary)
            {
                pair.Key.write(writer);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<Vector2, string[]> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Vector2 key = new Vector2();
                key = key.read(reader);
                string[] value = new string[0];
                value = value.read(reader);
                dictionary.Add(key, value);
            }
        }

        // Dictionary<int, List<Item_Data>>
        public static void write(this Dictionary<int, List<TactileLibrary.Item_Data>> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, List<TactileLibrary.Item_Data>> pair in dictionary)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<int, List<TactileLibrary.Item_Data>> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                List<TactileLibrary.Item_Data> list = new List<TactileLibrary.Item_Data>();
                list.read(reader);
                dictionary.Add(id, list);
            }
        }

        // Dictionary<Vector2, Shop_Data>
        public static void write(this Dictionary<Vector2, Tactile.Map.Shop_Data> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<Vector2, Tactile.Map.Shop_Data> pair in dictionary)
            {
                pair.Key.write(writer);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<Vector2, Tactile.Map.Shop_Data> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Vector2 key = new Vector2();
                key = key.read(reader);
                dictionary.Add(key, Tactile.Map.Shop_Data.read(reader));
            }
        }

        // Dictionary<Vector2, Visit_Data>
        public static void write(this Dictionary<Vector2, Tactile.Map.Visit_Data> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (var pair in dictionary)
            {
                pair.Key.write(writer);
                pair.Value.write(writer);
            }
        }

        public static void read(this Dictionary<Vector2, Tactile.Map.Visit_Data> dictionary, BinaryReader reader)
        {
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Vector2 key = new Vector2();
                key = key.read(reader);
                dictionary.Add(key, Tactile.Map.Visit_Data.read(reader));
            }
        }

        // Dictionary<Vector2, Vector2>
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
            dictionary.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Vector2 key = new Vector2();
                key = key.read(reader);
                Vector2 value = new Vector2();
                value = value.read(reader);
                dictionary.Add(key, value);
            }
        }
    }
}

// For lists of dictionaries
namespace TactileListExtension
{
    using TactileDictionaryExtension;

    static partial class Extension
    {
        // List<Dictionary<int, string>>
        public static void write(this List<Dictionary<int, string>> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (Dictionary<int, string> value in list)
                value.write(writer);
        }

        public static void read(this List<Dictionary<int, string>> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list.Add(new Dictionary<int, string>());
                list[i].read(reader);
            }
        }

        // List<Dictionary<Vector2, Vector2>>
        public static void write(this List<Dictionary<Vector2, Vector2>> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (Dictionary<Vector2, Vector2> value in list)
                value.write(writer);
        }

        public static void read(this List<Dictionary<Vector2, Vector2>> list, BinaryReader reader)
        {
            list.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list.Add(new Dictionary<Vector2, Vector2>());
                list[i].read(reader);
            }
        }

        // List<Team_Escape_Data>
        public static void write(this List<Tactile.Map.Team_Escape_Data> escapes, BinaryWriter writer)
        {
            writer.Write(escapes.Count);
            foreach (var processor in escapes)
            {
                processor.write(writer);
            }
        }

        public static void read(this List<Tactile.Map.Team_Escape_Data> escape_points, BinaryReader reader)
        {
            escape_points.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var escape_point = new Tactile.Map.Team_Escape_Data();
                escape_point.read(reader);
                escape_points.Add(escape_point);
            }
        }
    }
}

// For arrays of other collections
namespace TactileArrayExtension
{
    using TactileDictionaryExtension;

    public static partial class Extension
    {
        // Dictionary<int, HashSet<Vector2>>
        public static void write(this Dictionary<int, HashSet<Vector2>>[] array, BinaryWriter writer)
        {
            writer.Write(array.Length);
            for (int i = 0; i < array.GetLength(0); i++)
            {
                array[i].write(writer);
            }
        }

        public static Dictionary<int, HashSet<Vector2>>[] read(
            this Dictionary<int, HashSet<Vector2>>[] array,
            BinaryReader reader)
        {
            var result = new Dictionary<int, HashSet<Vector2>>[reader.ReadInt32()];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                result[i] = new Dictionary<int, HashSet<Vector2>>();
                result[i].read(reader);
            }
            return result;
        }
    }
}

namespace TactileStringExtension
{
    static class Extension
    {
        public static string substring(this string str, int index, int length)
        {
            if (index >= str.Length)
                return "";
            if (index + length > str.Length)
                length = str.Length - index;
            return str.Substring(index, length);
        }
    }
}

namespace TactileVersionExtension
{
    static class Extension
    {
        public static void Write(this BinaryWriter writer, Version version)
        {
            writer.Write(version.Major);
            writer.Write(version.Minor);
            writer.Write(version.Build);
            writer.Write(version.Revision);
        }
        public static Version ReadVersion(this BinaryReader reader)
        {
            int major = reader.ReadInt32();
            int minor = reader.ReadInt32();
            int build = reader.ReadInt32();
            int revision = reader.ReadInt32();

            return new Version(major, minor, build, revision);
        }

        public static bool older_than(this Version v, Version v2)
        {
            return v.older_than(v2.Major, v2.Minor, v2.Build, v2.Revision);
        }

        public static bool older_than(this Version v,
            int major, int minor, int build, int revision)
        {
            if (v.Major != major)
                return v.Major < major;
            if (v.Minor != minor)
                return v.Minor < minor;
            if (v.Build != build)
                return v.Build < build;
            if (v.Revision != revision)
                return v.Revision < revision;
            return false;
        }
    }
}

namespace TactileContentExtension
{
    using System;
    using System.Threading;
    using Microsoft.Xna.Framework.Content;
    using ContentManagers;
    /// <summary>
    /// Extensions to the Microsoft.Xna.Framework.Content.ContentManager class.
    /// </summary>
    static class ContentManagerExtensions
    {
        /// <summary>
        /// Loads the asset asynchronously on another thread.
        /// </summary>
        /// <typeparam name="T">The type of the asset to load</typeparam>
        /// <param name="contentManager">The content manager that will load the asset</param>
        /// <param name="assetName">The path and name of the asset (without the extension) relative to the root directory of the content manager</param>
        /// <param name="action">Callback that is called when the asset is loaded</param>
        public static void Load<T>(this ThreadSafeContentManager contentManager, string assetName, Action<T> action)
        {
#if MONOMAC
            // Loading content on a background thread seems to be completely undoable on Mac //Yeti
            return;
#endif

            return;
            ThreadPool.QueueUserWorkItem((s) =>
            {
                //T asset = contentManager.DefaultLoad<T>(assetName); // non-blocking version, maybe? //Debug
                T asset = contentManager.Load<T>(assetName);
                if (action != null)
                {
                    // Silverlight/Windows Phone version, or something idk
                    //Deployment.Current.Dispatcher.BeginInvoke(action, asset);
                    action.BeginInvoke(asset, null, null);
                }
            });
        }
    }
}

namespace TactileRenderTarget2DExtension
{
    using Microsoft.Xna.Framework.Graphics;
    /// <summary>
    /// Extensions to the Microsoft.Xna.Framework.Graphics.RenderTarget2D class.
    /// </summary>
    static class RenderTarget2DExtensions
    {
        public static void raw_copy_render_target(this RenderTarget2D source_render,
            SpriteBatch sprite_batch,
            GraphicsDevice device,
            RenderTarget2D target_render)
        {
            device.SetRenderTarget(target_render);
            device.Clear(Color.Transparent);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            sprite_batch.Draw(source_render, Vector2.Zero, Color.White);
            sprite_batch.End();
        }
    }
}