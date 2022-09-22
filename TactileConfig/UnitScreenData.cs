using System;
using System.Collections.Generic;
using System.Linq;

namespace Tactile.ConfigData
{
    public enum UnitScreenOutput { Text, TextDivisor, Affinity, Status, Inventory, Skills, Supports }
    public enum ParagraphAlign { Left, Center, Right }

    public struct UnitScreenData
    {
        public int Offset { get; private set; }
        public string Name { get; private set; }
        public int Page { get; private set; }
        private Func<object, int, object> Function;
        private Func<object, object, int> SortFunc;
        public UnitScreenOutput Output { get; private set; }
        public ParagraphAlign Align { get; private set; }
        public int DataOffset { get; private set; }
        private Func<object, string> TextColor;
        public bool LargeText { get; private set; }
        public int WeaponIcon { get; private set; }
        public bool NoSort { get; private set; }
        private Func<object, int> MultiplePageFunc;

        public UnitScreenData(
            int offset,
            string name,
            int page,
            Func<object, int, object> function = null,
            Func<object, object, int> sortFunc = null,
            UnitScreenOutput output = UnitScreenOutput.Text,
            ParagraphAlign align = ParagraphAlign.Right,
            int dataOffset = 0,
            Func<object, string> textColor = null,
            bool largeText = false,
            int weaponIcon = -1,
            bool noSort = false,
            Func<object, int> multiplePageFunc = null) : this()
        {
            Offset = offset;
            Name = name;
            Page = page;
            Function = function;
            SortFunc = sortFunc;
            Output = output;
            Align = align;
            DataOffset = dataOffset;
            TextColor = textColor;
            LargeText = largeText;
            WeaponIcon = weaponIcon;
            NoSort = noSort;
            MultiplePageFunc = multiplePageFunc;
        }

        public object GetValue(object input, int page)
        {
            if (Function == null)
                return null;

            return Function(input, page);
        }

        public int GetSort(object a, object b)
        {
            if (SortFunc == null)
            {
                var resultA = GetValue(a, 0);
                var resultB = GetValue(b, 0);

                if (resultA is string)
                    return string.Compare((string)resultB, (string)resultA);
                else if (resultA is int)
                    return (int)resultB - (int)resultA;

                return 0;
            }

            return SortFunc(a, b);
        }

        public string GetTextColor(object input)
        {
            if (TextColor == null)
                return "Blue";

            return TextColor(input);
        }

        public int GetPageCount(List<object> input)
        {
            if (MultiplePageFunc == null)
                return 1;

            var func = MultiplePageFunc;
            return input
                .Select(x => func(x))
                .Max();
        }
    }
}
