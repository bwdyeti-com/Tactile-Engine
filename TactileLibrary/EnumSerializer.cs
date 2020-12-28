using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TactileLibrary
{
    public class EnumSerializer
    {
        public static string[] BoolArrayToEnumStrings<T>(
            bool[] flags) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must enum");
            }

            var enums = Enumerable.Range(0, flags.Length)
                .Where(x => flags[x])
                .Select(x => Enum.GetValues(typeof(T)).GetValue(x))
                .Select(x => x.ToString());
            return enums.ToArray();
        }
        public static bool[] BoolArrayFromEnumStrings<T>(
            string[] enums) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must enum");
            }

            int count = Enum_Values.GetEnumCount(typeof(T));
            return Enumerable.Range(0, count)
                .Select(x => Enum.GetValues(typeof(T)).GetValue(x))
                .Select(x => enums.Contains(x.ToString()))
                .ToArray();
        }

        public static string[] BoolArrayToStrings(bool[] flags, string[] strings)
        {
            var enums = Enumerable.Range(0, flags.Length)
                .Where(x => flags[x])
                .Select(x => strings[x]);
            return enums.ToArray();
        }
    }
}
