using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Tactile
{
    class Additional_Math
    {
        public static int int_closer(int source, int target, int val)
        {
            int difference = target - source;
            val = (difference < 0 ? -1 : 1) * Math.Min(Math.Abs(difference), Math.Abs(val));
            return source + val;
        }
        public static double double_closer(double source, double target, double val)
        {
            double difference = target - source;
            val = (difference < 0 ? -1 : 1) * Math.Min(Math.Abs(difference), Math.Abs(val));
            return source + val;
        }

        public static double Logistic(double x, double steepness = 1f, double midpoint = 0f)
        {
            return 1 / (1 + Math.Exp(-steepness * (x - midpoint)));
        }

        /// <summary>
        /// If formula(value) is 0, returns the sign of value. Otherwise returns the result.
        /// </summary>
        public static int preserve_sign(int value, Func<int, int> formula)
        {
            int result = formula(value);
            if (result == 0)
            {
                if (value == 0)
                    return value;
                else
                    return value > 0 ? 1 : -1;
            }
            else
                return result;
        }

        public static Vector2 from_polar(float angle, float magnitude)
        {
            return magnitude * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public static void swap(ref int i, ref int j)
        {
            int temp = i;
            i = j;
            j = temp;
        }

        /// <summary>
        /// Count the number of trues in a set of booleans
        /// </summary>
        public static int bool_count(params bool[] booleans)
        {
            int count = 0;
            foreach (bool flag in booleans)
                if (flag)
                    count++;
            return count;
        }

        public static int bool_index(int key, params bool[] booleans)
        {
            int count = bool_count(booleans);
            if (count == 0)
                return -1;
            key = key % count;
            for (int i = 0; i < booleans.Length; i++ )
            {
                if (booleans[i])
                {
                    if (key == 0)
                        return i;
                    key--;
                }
            }
#if DEBUG
            throw new InvalidOperationException();
#endif
            return -1;
        }

        public static IEnumerable<Rectangle> get_edges(IEnumerable<Vector2> tiles)
        {
            Dictionary<Rectangle, int> lines = new Dictionary<Rectangle, int>();
            foreach (Vector2 tile in tiles)
            {
                // horizontal
                for (int h = 0; h < 2; h++)
                    // offset
                    for (int o = 0; o < 2; o++)
                    {
                        Rectangle rect = new Rectangle(
                            (int)tile.X + (1 - h) * o, (int)tile.Y + h * o, h, 1 - h);
                        if (!lines.ContainsKey(rect))
                            lines.Add(rect, 0);
                        lines[rect]++;
                    }
            }

            foreach (var rect in lines
                    .Where(pair => pair.Value == 1)
                    .Select(x => x.Key))
                yield return rect;
        }
    }
}