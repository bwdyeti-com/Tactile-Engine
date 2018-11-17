using System;
using System.Collections.Generic;

namespace FEXNA
{
    public struct Font_Data
    {
        public readonly static Dictionary<string, Font_Data> Data;

        public static int text_width(string text)
        {
            return text_width(text, "FE7_Text");
        }

        public static int text_width(string text, string font)
        {
            if (!Data.ContainsKey(font))
                return 0;
            Dictionary<char, int[]> font_data = Data[font].CharacterData;
            int text_width = 0, previous_line_width = 0;
            foreach (char letter in text)
            {
                if (letter == '\n')
                {
                    previous_line_width = Math.Max(previous_line_width, text_width);
                    text_width = 0;
                    continue;
                }

                if (!font_data.ContainsKey(letter))
                    continue;
                int[] char_data = font_data[letter];
                text_width += char_data[2];
                if (Data[font].CharacterOffsets != null)
                    if (Data[font].CharacterOffsets.ContainsKey(letter))
                        text_width += Data[font].CharacterOffsets[letter];
            }
            return Math.Max(previous_line_width, text_width);
        }

        static Font_Data()
        {
            Data = new Dictionary<string, Font_Data> {
                #region FE7_Text
                { "FE7_Text",         new Font_Data("FE7_Text",          8, 16,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { 'a', new int[] {  0, 0, 6 }},
                        { 'b', new int[] {  1, 0, 5 }},
                        { 'c', new int[] {  2, 0, 5 }},
                        { 'd', new int[] {  3, 0, 5 }},
                        { 'e', new int[] {  4, 0, 5 }},
                        { 'f', new int[] {  5, 0, 4 }},
                        { 'g', new int[] {  6, 0, 5 }},
                        { 'h', new int[] {  7, 0, 5 }},
                        { 'i', new int[] {  8, 0, 2 }},
                        { 'j', new int[] {  9, 0, 4 }},
                        { 'k', new int[] { 10, 0, 5 }},
                        { 'l', new int[] { 11, 0, 2 }},
                        { 'm', new int[] { 12, 0, 6 }},
                        { 'n', new int[] { 13, 0, 5 }},
                        { 'o', new int[] { 14, 0, 5 }},
                        { 'p', new int[] { 15, 0, 5 }},
                        { 'q', new int[] { 16, 0, 5 }},
                        { 'r', new int[] { 17, 0, 4 }},
                        { 's', new int[] { 18, 0, 5 }},
                        { 't', new int[] { 19, 0, 4 }},
                        { 'u', new int[] { 20, 0, 5 }},
                        { 'v', new int[] { 21, 0, 6 }},
                        { 'w', new int[] { 22, 0, 6 }},
                        { 'x', new int[] { 23, 0, 6 }},
                        { 'y', new int[] { 24, 0, 5 }},
                        { 'z', new int[] { 25, 0, 5 }},
                        { 'A', new int[] {  0, 1, 5 }},
                        { 'B', new int[] {  1, 1, 5 }},
                        { 'C', new int[] {  2, 1, 5 }},
                        { 'D', new int[] {  3, 1, 5 }},
                        { 'E', new int[] {  4, 1, 5 }},
                        { 'F', new int[] {  5, 1, 5 }},
                        { 'G', new int[] {  6, 1, 5 }},
                        { 'H', new int[] {  7, 1, 5 }},
                        { 'I', new int[] {  8, 1, 4 }},
                        { 'J', new int[] {  9, 1, 5 }},
                        { 'K', new int[] { 10, 1, 5 }},
                        { 'L', new int[] { 11, 1, 4 }},
                        { 'M', new int[] { 12, 1, 6 }},
                        { 'N', new int[] { 13, 1, 5 }},
                        { 'O', new int[] { 14, 1, 5 }},
                        { 'P', new int[] { 15, 1, 5 }},
                        { 'Q', new int[] { 16, 1, 6 }},
                        { 'R', new int[] { 17, 1, 5 }},
                        { 'S', new int[] { 18, 1, 5 }},
                        { 'T', new int[] { 19, 1, 6 }},
                        { 'U', new int[] { 20, 1, 5 }},
                        { 'V', new int[] { 21, 1, 6 }},
                        { 'W', new int[] { 22, 1, 6 }},
                        { 'X', new int[] { 23, 1, 6 }},
                        { 'Y', new int[] { 24, 1, 6 }},
                        { 'Z', new int[] { 25, 1, 5 }},
                        { '0', new int[] {  0, 2, 8 }},
                        { '1', new int[] {  1, 2, 8 }},
                        { '2', new int[] {  2, 2, 8 }},
                        { '3', new int[] {  3, 2, 8 }},
                        { '4', new int[] {  4, 2, 8 }},
                        { '5', new int[] {  5, 2, 8 }},
                        { '6', new int[] {  6, 2, 8 }},
                        { '7', new int[] {  7, 2, 8 }},
                        { '8', new int[] {  8, 2, 8 }},
                        { '9', new int[] {  9, 2, 8 }},
                        { '.', new int[] { 10, 2, 2 }},
                        { ',', new int[] { 11, 2, 2 }},
                        { '$', new int[] { 12, 2, 7 }},
                        { '!', new int[] { 13, 2, 4 }},
                        { '(', new int[] { 14, 2, 4 }},
                        { ')', new int[] { 15, 2, 4 }},
                        { '-', new int[] { 16, 2, 8 }},
                        { '/', new int[] { 17, 2, 6 }},
                        { '\\',new int[] { 18, 2, 6 }},
                        { '?', new int[] { 19, 2, 6 }},
                        { ';', new int[] { 20, 2, 2 }},
                        { ':', new int[] { 21, 2, 2 }},
                        { '#', new int[] { 22, 2, 5 }},
                        { '&', new int[] { 23, 2, 6 }},
                        { '"', new int[] { 24, 2, 4 }},
                        { '\'',new int[] { 25, 2, 2 }},
                        { '+', new int[] { 26, 2, 7 }},
                        { '[', new int[] { 26, 0, 4 }},
                        { ']', new int[] { 26, 1, 4 }},
                        { ' ', new int[] {  0, 0, 3 }}
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region FE7 Text (Large)
                { "FE7_TextL",        new Font_Data("FE7_TextL",         8, 16,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { 'a', new int[] {  0, 0, 6 }},
                        { 'b', new int[] {  1, 0, 5 }},
                        { 'c', new int[] {  2, 0, 5 }},
                        { 'd', new int[] {  3, 0, 5 }},
                        { 'e', new int[] {  4, 0, 5 }},
                        { 'f', new int[] {  5, 0, 4 }},
                        { 'g', new int[] {  6, 0, 5 }},
                        { 'h', new int[] {  7, 0, 5 }},
                        { 'i', new int[] {  8, 0, 2 }},
                        { 'j', new int[] {  9, 0, 4 }},
                        { 'k', new int[] { 10, 0, 5 }},
                        { 'l', new int[] { 11, 0, 2 }},
                        { 'm', new int[] { 12, 0, 6 }},
                        { 'n', new int[] { 13, 0, 5 }},
                        { 'o', new int[] { 14, 0, 5 }},
                        { 'p', new int[] { 15, 0, 5 }},
                        { 'q', new int[] { 16, 0, 5 }},
                        { 'r', new int[] { 17, 0, 4 }},
                        { 's', new int[] { 18, 0, 5 }},
                        { 't', new int[] { 19, 0, 4 }},
                        { 'u', new int[] { 20, 0, 5 }},
                        { 'v', new int[] { 21, 0, 6 }},
                        { 'w', new int[] { 22, 0, 6 }},
                        { 'x', new int[] { 23, 0, 6 }},
                        { 'y', new int[] { 24, 0, 5 }},
                        { 'z', new int[] { 25, 0, 5 }},
                        { 'A', new int[] {  0, 3, 8 }},
                        { 'B', new int[] {  1, 3, 8 }},
                        { 'C', new int[] {  2, 3, 8 }},
                        { 'D', new int[] {  3, 3, 8 }},
                        { 'E', new int[] {  4, 3, 8 }},
                        { 'F', new int[] {  5, 3, 8 }},
                        { 'G', new int[] {  6, 3, 8 }},
                        { 'H', new int[] {  7, 3, 8 }},
                        { 'I', new int[] {  8, 3, 8 }},
                        { 'J', new int[] {  9, 3, 8 }},
                        { 'K', new int[] { 10, 3, 8 }},
                        { 'L', new int[] { 11, 3, 8 }},
                        { 'M', new int[] { 12, 3, 8 }},
                        { 'N', new int[] { 13, 3, 8 }},
                        { 'O', new int[] { 14, 3, 8 }},
                        { 'P', new int[] { 15, 3, 8 }},
                        { 'Q', new int[] { 16, 3, 8 }},
                        { 'R', new int[] { 17, 3, 8 }},
                        { 'S', new int[] { 18, 3, 8 }},
                        { 'T', new int[] { 19, 3, 8 }},
                        { 'U', new int[] { 20, 3, 8 }},
                        { 'V', new int[] { 21, 3, 8 }},
                        { 'W', new int[] { 22, 3, 8 }},
                        { 'X', new int[] { 23, 3, 8 }},
                        { 'Y', new int[] { 24, 3, 8 }},
                        { 'Z', new int[] { 25, 3, 8 }},
                        { '0', new int[] {  0, 2, 8 }},
                        { '1', new int[] {  1, 2, 8 }},
                        { '2', new int[] {  2, 2, 8 }},
                        { '3', new int[] {  3, 2, 8 }},
                        { '4', new int[] {  4, 2, 8 }},
                        { '5', new int[] {  5, 2, 8 }},
                        { '6', new int[] {  6, 2, 8 }},
                        { '7', new int[] {  7, 2, 8 }},
                        { '8', new int[] {  8, 2, 8 }},
                        { '9', new int[] {  9, 2, 8 }},
                        { '.', new int[] { 10, 2, 2 }},
                        { ',', new int[] { 11, 2, 2 }},
                        { '$', new int[] { 12, 2, 7 }},
                        { '!', new int[] { 13, 2, 4 }},
                        { '(', new int[] { 14, 2, 4 }},
                        { ')', new int[] { 15, 2, 4 }},
                        { '-', new int[] { 26, 4, 8 }},
                        { '/', new int[] { 17, 2, 6 }},
                        { '\\',new int[] { 18, 2, 6 }},
                        { '?', new int[] { 19, 2, 6 }},
                        { ';', new int[] { 20, 2, 2 }},
                        { ':', new int[] { 21, 2, 2 }},
                        { '#', new int[] { 22, 2, 5 }},
                        { '&', new int[] { 23, 2, 6 }},
                        { '"', new int[] { 24, 2, 4 }},
                        { '\'',new int[] { 25, 2, 2 }},
                        { '+', new int[] { 26, 2, 7 }},
                        { ' ', new int[] { 0, 0, 3 }},
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region FE7_Text (Small)
                { "FE7_TextS",        new Font_Data("FE7_TextS",         8, 16,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { '0', new int[] {  0, 4, 8 }},
                        { '1', new int[] {  1, 4, 8 }},
                        { '2', new int[] {  2, 4, 8 }},
                        { '3', new int[] {  3, 4, 8 }},
                        { '4', new int[] {  4, 4, 8 }},
                        { '5', new int[] {  5, 4, 8 }},
                        { '6', new int[] {  6, 4, 8 }},
                        { '7', new int[] {  7, 4, 8 }},
                        { '8', new int[] {  8, 4, 8 }},
                        { '9', new int[] {  9, 4, 8 }},
                        { '.', new int[] { 10, 2, 2 }},
                        { '-', new int[] { 16, 2, 8 }},
                        { ':', new int[] { 21, 2, 2 }},
                        { '+', new int[] { 26, 2, 8 }},
                        { ' ', new int[] {  0, 0, 3 }}
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region FE7_Text (Bonus +x)
                { "FE7_TextBonus",        new Font_Data("FE7_TextBonus",         8, 16,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { '0', new int[] {  0, 4, 8 }},
                        { '1', new int[] {  1, 4, 8 }},
                        { '2', new int[] {  2, 4, 8 }},
                        { '3', new int[] {  3, 4, 8 }},
                        { '4', new int[] {  4, 4, 8 }},
                        { '5', new int[] {  5, 4, 8 }},
                        { '6', new int[] {  6, 4, 8 }},
                        { '7', new int[] {  7, 4, 8 }},
                        { '8', new int[] {  8, 4, 8 }},
                        { '9', new int[] {  9, 4, 8 }},
                        { '.', new int[] { 10, 2, 2 }},
                        { '-', new int[] { 26, 3, 8 }},
                        { ':', new int[] { 21, 2, 2 }},
                        { '+', new int[] { 26, 2, 8 }},
                        { ' ', new int[] {  0, 0, 3 }}
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region FE7 Convo
                { "FE7_Convo",        new Font_Data("FE7_Convo",        16, 16,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { ' ', new int[] {  0,  2,  4 }},
                        { '!', new int[] {  1,  2,  3 }},
                        { '"', new int[] {  2,  2,  4 }},
                        { '#', new int[] {  3,  2,  8 }},
                        { '$', new int[] {  4,  2,  6 }},
                        { '%', new int[] {  5,  2,  8 }},
                        { '&', new int[] {  6,  2,  7 }},
                        { '\'',new int[] {  7,  2,  2 }},
                        { '(', new int[] {  8,  2,  4 }},
                        { ')', new int[] {  9,  2,  4 }},
                        { '*', new int[] { 10,  2,  8 }},
                        { '+', new int[] { 11,  2,  8 }},
                        { ',', new int[] { 12,  2,  2 }},
                        { '-', new int[] { 13,  2,  4 }},
                        { '.', new int[] { 14,  2,  2 }},
                        { '/', new int[] { 15,  2,  7 }},

                        { '0', new int[] {  0,  3,  6 }},
                        { '1', new int[] {  1,  3,  4 }},
                        { '2', new int[] {  2,  3,  6 }},
                        { '3', new int[] {  3,  3,  6 }},
                        { '4', new int[] {  4,  3,  7 }},
                        { '5', new int[] {  5,  3,  6 }},
                        { '6', new int[] {  6,  3,  6 }},
                        { '7', new int[] {  7,  3,  6 }},
                        { '8', new int[] {  8,  3,  6 }},
                        { '9', new int[] {  9,  3,  6 }},
                        { ':', new int[] { 10,  3,  2 }},
                        { ';', new int[] { 11,  3,  2 }},
                        { '<', new int[] { 12,  3,  6 }},
                        { '=', new int[] { 13,  3,  6 }},
                        { '>', new int[] { 14,  3,  6 }},
                        { '?', new int[] { 15,  3,  7 }},
                        
                        { '@', new int[] {  0,  4,  8 }},
                        { 'A', new int[] {  1,  4,  6 }},
                        { 'B', new int[] {  2,  4,  6 }},
                        { 'C', new int[] {  3,  4,  6 }},
                        { 'D', new int[] {  4,  4,  6 }},
                        { 'E', new int[] {  5,  4,  6 }},
                        { 'F', new int[] {  6,  4,  6 }},
                        { 'G', new int[] {  7,  4,  6 }},
                        { 'H', new int[] {  8,  4,  6 }},
                        { 'I', new int[] {  9,  4,  4 }},
                        { 'J', new int[] { 10,  4,  6 }},
                        { 'K', new int[] { 11,  4,  6 }},
                        { 'L', new int[] { 12,  4,  6 }},
                        { 'M', new int[] { 13,  4,  8 }},
                        { 'N', new int[] { 14,  4,  6 }},
                        { 'O', new int[] { 15,  4,  6 }},

                        { 'P', new int[] {  0,  5,  6 }},
                        { 'Q', new int[] {  1,  5,  7 }},
                        { 'R', new int[] {  2,  5,  6 }},
                        { 'S', new int[] {  3,  5,  7 }},
                        { 'T', new int[] {  4,  5,  6 }},
                        { 'U', new int[] {  5,  5,  6 }},
                        { 'V', new int[] {  6,  5,  6 }},
                        { 'W', new int[] {  7,  5,  8 }},
                        { 'X', new int[] {  8,  5,  8 }},
                        { 'Y', new int[] {  9,  5,  6 }},
                        { 'Z', new int[] { 10,  5,  6 }},
                        { '[', new int[] { 11,  5,  3 }},
                        { '\\',new int[] { 12,  5,  7 }},
                        { ']', new int[] { 13,  5,  3 }},
                        { '^', new int[] { 14,  5,  5 }},
                        { '_', new int[] { 15,  5,  4 }},

                        { '`', new int[] {  0,  6,  2 }},
                        { 'a', new int[] {  1,  6,  6 }},
                        { 'b', new int[] {  2,  6,  5 }},
                        { 'c', new int[] {  3,  6,  5 }},
                        { 'd', new int[] {  4,  6,  5 }},
                        { 'e', new int[] {  5,  6,  5 }},
                        { 'f', new int[] {  6,  6,  5 }},
                        { 'g', new int[] {  7,  6,  7 }},
                        { 'h', new int[] {  8,  6,  5 }},
                        { 'i', new int[] {  9,  6,  2 }},
                        { 'j', new int[] { 10,  6,  3 }},
                        { 'k', new int[] { 11,  6,  5 }},
                        { 'l', new int[] { 12,  6,  2 }},
                        { 'm', new int[] { 13,  6,  6 }},
                        { 'n', new int[] { 14,  6,  5 }},
                        { 'o', new int[] { 15,  6,  5 }},

                        { 'p', new int[] {  0,  7,  5 }},
                        { 'q', new int[] {  1,  7,  5 }},
                        { 'r', new int[] {  2,  7,  4 }},
                        { 's', new int[] {  3,  7,  5 }},
                        { 't', new int[] {  4,  7,  4 }},
                        { 'u', new int[] {  5,  7,  5 }},
                        { 'v', new int[] {  6,  7,  6 }},
                        { 'w', new int[] {  7,  7,  6 }},
                        { 'x', new int[] {  8,  7,  6 }},
                        { 'y', new int[] {  9,  7,  5 }},
                        { 'z', new int[] { 10,  7,  5 }},
                        { '{', new int[] { 11,  7,  4 }},
                        { '|', new int[] { 12,  7,  2 }},
                        { '}', new int[] { 13,  7,  4 }},
                        { '~', new int[] { 14,  7,  7 }},
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region FE7 Text Info
                { "FE7_Text_Info",    new Font_Data("FE7_Text_Info",     8, 16,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { 'a', new int[] {  0, 0, 6 }},
                        { 'b', new int[] {  1, 0, 5 }},
                        { 'c', new int[] {  2, 0, 5 }},
                        { 'd', new int[] {  3, 0, 5 }},
                        { 'e', new int[] {  4, 0, 5 }},
                        { 'f', new int[] {  5, 0, 4 }},
                        { 'g', new int[] {  6, 0, 5 }},
                        { 'h', new int[] {  7, 0, 5 }},
                        { 'i', new int[] {  8, 0, 2 }},
                        { 'j', new int[] {  9, 0, 4 }},
                        { 'k', new int[] { 10, 0, 5 }},
                        { 'l', new int[] { 11, 0, 2 }},
                        { 'm', new int[] { 12, 0, 6 }},
                        { 'n', new int[] { 13, 0, 5 }},
                        { 'o', new int[] { 14, 0, 5 }},
                        { 'p', new int[] { 15, 0, 5 }},
                        { 'q', new int[] { 16, 0, 5 }},
                        { 'r', new int[] { 17, 0, 4 }},
                        { 's', new int[] { 18, 0, 5 }},
                        { 't', new int[] { 19, 0, 4 }},
                        { 'u', new int[] { 20, 0, 5 }},
                        { 'v', new int[] { 21, 0, 6 }},
                        { 'w', new int[] { 22, 0, 6 }},
                        { 'x', new int[] { 23, 0, 6 }},
                        { 'y', new int[] { 24, 0, 5 }},
                        { 'z', new int[] { 25, 0, 5 }},
                        { 'A', new int[] {  0, 1, 5 }},
                        { 'B', new int[] {  1, 1, 5 }},
                        { 'C', new int[] {  2, 1, 5 }},
                        { 'D', new int[] {  3, 1, 5 }},
                        { 'E', new int[] {  4, 1, 5 }},
                        { 'F', new int[] {  5, 1, 5 }},
                        { 'G', new int[] {  6, 1, 5 }},
                        { 'H', new int[] {  7, 1, 5 }},
                        { 'I', new int[] {  8, 1, 4 }},
                        { 'J', new int[] {  9, 1, 5 }},
                        { 'K', new int[] { 10, 1, 5 }},
                        { 'L', new int[] { 11, 1, 4 }},
                        { 'M', new int[] { 12, 1, 6 }},
                        { 'N', new int[] { 13, 1, 5 }},
                        { 'O', new int[] { 14, 1, 5 }},
                        { 'P', new int[] { 15, 1, 5 }},
                        { 'Q', new int[] { 16, 1, 6 }},
                        { 'R', new int[] { 17, 1, 5 }},
                        { 'S', new int[] { 18, 1, 5 }},
                        { 'T', new int[] { 19, 1, 6 }},
                        { 'U', new int[] { 20, 1, 5 }},
                        { 'V', new int[] { 21, 1, 6 }},
                        { 'W', new int[] { 22, 1, 6 }},
                        { 'X', new int[] { 23, 1, 6 }},
                        { 'Y', new int[] { 24, 1, 6 }},
                        { 'Z', new int[] { 25, 1, 5 }},
                        { '0', new int[] {  0, 2, 7 }},
                        { '1', new int[] {  1, 2, 7 }},
                        { '2', new int[] {  2, 2, 7 }},
                        { '3', new int[] {  3, 2, 7 }},
                        { '4', new int[] {  4, 2, 7 }},
                        { '5', new int[] {  5, 2, 7 }},
                        { '6', new int[] {  6, 2, 7 }},
                        { '7', new int[] {  7, 2, 7 }},
                        { '8', new int[] {  8, 2, 7 }},
                        { '9', new int[] {  9, 2, 7 }},
                        { '.', new int[] { 10, 2, 2 }},
                        { ',', new int[] { 11, 2, 2 }},
                        { '$', new int[] { 12, 2, 7 }},
                        { '!', new int[] { 13, 2, 4 }},
                        { '(', new int[] { 14, 2, 4 }},
                        { ')', new int[] { 15, 2, 4 }},
                        { '-', new int[] { 16, 2, 8 }},
                        { '/', new int[] { 17, 2, 5 }},
                        { '\\',new int[] { 18, 2, 6 }},
                        { '?', new int[] { 19, 2, 6 }},
                        { ';', new int[] { 20, 2, 2 }},
                        { ':', new int[] { 21, 2, 2 }},
                        { '#', new int[] { 22, 2, 5 }},
                        { '&', new int[] { 23, 2, 6 }},
                        { '"', new int[] { 24, 2, 4 }},
                        { '\'',new int[] { 25, 2, 2 }},
                        { '+', new int[] { 26, 2, 7 }},
                        { '%', new int[] { 27, 2, 7 }},
                        { ' ', new int[] {  0, 0, 3 }},
                    },
                    #endregion
                    #region Character Offsets
                    new Dictionary<char, int> {
                        { '-', -1 },
                    }) },
                    #endregion
                #endregion
                #region FE7 Text Info2
                { "FE7_Text_Info2",   new Font_Data("FE7_Text_Info2",    8, 16,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { 'a', new int[] {  0, 0, 6 }},
                        { 'b', new int[] {  1, 0, 5 }},
                        { 'c', new int[] {  2, 0, 5 }},
                        { 'd', new int[] {  3, 0, 5 }},
                        { 'e', new int[] {  4, 0, 5 }},
                        { 'f', new int[] {  5, 0, 4 }},
                        { 'g', new int[] {  6, 0, 5 }},
                        { 'h', new int[] {  7, 0, 5 }},
                        { 'i', new int[] {  8, 0, 2 }},
                        { 'j', new int[] {  9, 0, 4 }},
                        { 'k', new int[] { 10, 0, 5 }},
                        { 'l', new int[] { 11, 0, 2 }},
                        { 'm', new int[] { 12, 0, 6 }},
                        { 'n', new int[] { 13, 0, 5 }},
                        { 'o', new int[] { 14, 0, 5 }},
                        { 'p', new int[] { 15, 0, 5 }},
                        { 'q', new int[] { 16, 0, 5 }},
                        { 'r', new int[] { 17, 0, 4 }},
                        { 's', new int[] { 18, 0, 5 }},
                        { 't', new int[] { 19, 0, 4 }},
                        { 'u', new int[] { 20, 0, 5 }},
                        { 'v', new int[] { 21, 0, 6 }},
                        { 'w', new int[] { 22, 0, 6 }},
                        { 'x', new int[] { 23, 0, 6 }},
                        { 'y', new int[] { 24, 0, 5 }},
                        { 'z', new int[] { 25, 0, 5 }},
                        { 'A', new int[] {  0, 1, 5 }},
                        { 'B', new int[] {  1, 1, 5 }},
                        { 'C', new int[] {  2, 1, 5 }},
                        { 'D', new int[] {  3, 1, 5 }},
                        { 'E', new int[] {  4, 1, 5 }},
                        { 'F', new int[] {  5, 1, 5 }},
                        { 'G', new int[] {  6, 1, 5 }},
                        { 'H', new int[] {  7, 1, 5 }},
                        { 'I', new int[] {  8, 1, 4 }},
                        { 'J', new int[] {  9, 1, 5 }},
                        { 'K', new int[] { 10, 1, 5 }},
                        { 'L', new int[] { 11, 1, 4 }},
                        { 'M', new int[] { 12, 1, 6 }},
                        { 'N', new int[] { 13, 1, 5 }},
                        { 'O', new int[] { 14, 1, 5 }},
                        { 'P', new int[] { 15, 1, 5 }},
                        { 'Q', new int[] { 16, 1, 6 }},
                        { 'R', new int[] { 17, 1, 5 }},
                        { 'S', new int[] { 18, 1, 5 }},
                        { 'T', new int[] { 19, 1, 6 }},
                        { 'U', new int[] { 20, 1, 5 }},
                        { 'V', new int[] { 21, 1, 6 }},
                        { 'W', new int[] { 22, 1, 6 }},
                        { 'X', new int[] { 23, 1, 6 }},
                        { 'Y', new int[] { 24, 1, 6 }},
                        { 'Z', new int[] { 25, 1, 5 }},
                        { '0', new int[] {  0, 2, 8 }},
                        { '1', new int[] {  1, 2, 8 }},
                        { '2', new int[] {  2, 2, 8 }},
                        { '3', new int[] {  3, 2, 8 }},
                        { '4', new int[] {  4, 2, 8 }},
                        { '5', new int[] {  5, 2, 8 }},
                        { '6', new int[] {  6, 2, 8 }},
                        { '7', new int[] {  7, 2, 8 }},
                        { '8', new int[] {  8, 2, 8 }},
                        { '9', new int[] {  9, 2, 8 }},
                        { '.', new int[] { 10, 2, 2 }},
                        { ',', new int[] { 11, 2, 2 }},
                        { '$', new int[] { 12, 2, 7 }},
                        { '!', new int[] { 13, 2, 4 }},
                        { '(', new int[] { 14, 2, 4 }},
                        { ')', new int[] { 15, 2, 4 }},
                        { '-', new int[] { 16, 2, 8 }},
                        { '/', new int[] { 17, 2, 5 }},
                        { '\\',new int[] { 18, 2, 6 }},
                        { '?', new int[] { 19, 2, 6 }},
                        { ';', new int[] { 20, 2, 2 }},
                        { ':', new int[] { 21, 2, 2 }},
                        { '#', new int[] { 22, 2, 5 }},
                        { '&', new int[] { 23, 2, 6 }},
                        { '"', new int[] { 24, 2, 4 }},
                        { '\'',new int[] { 25, 2, 2 }},
                        { '+', new int[] { 26, 2, 7 }},
                        { ' ', new int[] {  0, 0, 3 }},
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region FE7 Chapter Title
                { "FE7_Chapter",      new Font_Data("FE7_Chapter",      16, 16,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { 'a', new int[] {  0, 0, 6 }},
                        { 'b', new int[] {  1, 0, 7 }},
                        { 'c', new int[] {  2, 0, 6 }},
                        { 'd', new int[] {  3, 0, 7 }},
                        { 'e', new int[] {  4, 0, 6 }},
                        { 'f', new int[] {  5, 0, 5 }},
                        { 'g', new int[] {  6, 0, 7 }},
                        { 'h', new int[] {  7, 0, 7 }},
                        { 'i', new int[] {  8, 0, 4 }},
                        { 'j', new int[] {  9, 0, 6 }},//
                        { 'k', new int[] { 10, 0, 7 }},
                        { 'l', new int[] { 11, 0, 4 }},
                        { 'm', new int[] { 12, 0, 9 }},
                        { 'n', new int[] { 13, 0, 7 }},
                        { 'o', new int[] { 14, 0, 6 }},
                        { 'p', new int[] { 15, 0, 6 }},
                        { 'q', new int[] { 16, 0, 6 }},//
                        { 'r', new int[] { 17, 0, 5 }},
                        { 's', new int[] { 18, 0, 6 }},
                        { 't', new int[] { 19, 0, 4 }},
                        { 'u', new int[] { 20, 0, 6 }},
                        { 'v', new int[] { 21, 0, 6 }},
                        { 'w', new int[] { 22, 0, 8 }},
                        { 'x', new int[] { 23, 0, 6 }},
                        { 'y', new int[] { 24, 0, 6 }},//
                        { 'z', new int[] { 25, 0, 6 }},//
                        { 'A', new int[] {  0, 1, 8 }},
                        { 'B', new int[] {  1, 1, 8 }},
                        { 'C', new int[] {  2, 1, 8 }},
                        { 'D', new int[] {  3, 1, 8 }},
                        { 'E', new int[] {  4, 1, 8 }},//
                        { 'F', new int[] {  5, 1, 8 }},
                        { 'G', new int[] {  6, 1, 8 }},
                        { 'H', new int[] {  7, 1,10 }},
                        { 'I', new int[] {  8, 1, 5 }},
                        { 'J', new int[] {  9, 1, 8 }},//
                        { 'K', new int[] { 10, 1,10 }},
                        { 'L', new int[] { 11, 1, 9 }},
                        { 'M', new int[] { 12, 1,10 }},
                        { 'N', new int[] { 13, 1, 9 }},
                        { 'O', new int[] { 14, 1, 9 }},
                        { 'P', new int[] { 15, 1, 7 }},
                        { 'Q', new int[] { 16, 1, 8 }},//
                        { 'R', new int[] { 17, 1, 8 }},
                        { 'S', new int[] { 18, 1, 7 }},
                        { 'T', new int[] { 19, 1, 8 }},
                        { 'U', new int[] { 20, 1, 9 }},
                        { 'V', new int[] { 21, 1, 8 }},//
                        { 'W', new int[] { 22, 1,13 }},
                        { 'X', new int[] { 23, 1, 8 }},//
                        { 'Y', new int[] { 24, 1, 8 }},//
                        { 'Z', new int[] { 25, 1, 8 }},//
                        { '0', new int[] {  0, 2, 6 }},
                        { '1', new int[] {  1, 2, 6 }},
                        { '2', new int[] {  2, 2, 6 }},
                        { '3', new int[] {  3, 2, 6 }},
                        { '4', new int[] {  4, 2, 6 }},
                        { '5', new int[] {  5, 2, 6 }},
                        { '6', new int[] {  6, 2, 6 }},
                        { '7', new int[] {  7, 2, 7 }},
                        { '8', new int[] {  8, 2, 6 }},
                        { '9', new int[] {  9, 2, 6 }},
                        { '.', new int[] { 10, 2, 6 }},
                        { ',', new int[] { 11, 2, 6 }},
                        { '$', new int[] { 12, 2, 6 }},
                        { '!', new int[] { 13, 2, 6 }},
                        { '(', new int[] { 14, 2, 6 }},
                        { ')', new int[] { 15, 2, 6 }},
                        { '-', new int[] { 16, 2, 6 }},
                        { '/', new int[] { 17, 2, 6 }},
                        { '\\',new int[] { 18, 2, 3 }},
                        { '?', new int[] { 19, 2, 6 }},
                        { ';', new int[] { 20, 2, 6 }},
                        { ':', new int[] { 21, 2, 6 }},
                        { '#', new int[] { 22, 2, 6 }},
                        { '&', new int[] { 23, 2, 6 }},
                        { '"', new int[] { 24, 2, 6 }},
                        { '\'',new int[] { 25, 2, 6 }},
                        { '+', new int[] { 26, 2, 6 }},
                        { '%', new int[] { 27, 2, 6 }},
                        { ' ', new int[] {  0, 0, 3 }},
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region Blue 100 (for target screen hit/crit of 100)
                { "Blue_100",         new Font_Data("Blue_100",         16, 16,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { '1', new int[] { 0, 0, 16 }},
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region FE7 Text (Stat1)
                { "FE7_Text_Stat1",   new Font_Data("FE7_Text_Stat1",    8, 16,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { '0', new int[] { 0, 0, 8 }},
                        { '1', new int[] { 1, 0, 8 }},
                        { '2', new int[] { 2, 0, 8 }},
                        { '3', new int[] { 3, 0, 8 }},
                        { '4', new int[] { 4, 0, 8 }},
                        { '5', new int[] { 5, 0, 8 }},
                        { '6', new int[] { 6, 0, 8 }},
                        { '7', new int[] { 7, 0, 8 }},
                        { '8', new int[] { 8, 0, 8 }},
                        { '9', new int[] { 9, 0, 8 }},
                        { '-', new int[] {10, 0, 8 }},
                        { '+', new int[] {11, 0, 8 }},
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region FE7 Text (Stat2)
                { "FE7_Text_Stat2",   new Font_Data("FE7_Text_Stat2",   14, 12,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { '0', new int[] { 0, 0, 14 }},
                        { '1', new int[] { 1, 0, 14 }},
                        { '2', new int[] { 2, 0, 14 }},
                        { '3', new int[] { 3, 0, 14 }},
                        { '4', new int[] { 4, 0, 14 }},
                        { '5', new int[] { 5, 0, 14 }},
                        { '6', new int[] { 6, 0, 14 }},
                        { '7', new int[] { 7, 0, 14 }},
                        { '8', new int[] { 8, 0, 14 }},
                        { '9', new int[] { 9, 0, 14 }},
                        { '-', new int[] { 0, 12, 13, 3, 13 }},
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region FE7 Text (Combat)
                { "FE7_Text_Combat3", new Font_Data("FE7_Text_Combat3",  8, 16,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { '0', new int[] { 0, 2, 8 }},
                        { '1', new int[] { 1, 2, 8 }},
                        { '2', new int[] { 2, 2, 8 }},
                        { '3', new int[] { 3, 2, 8 }},
                        { '4', new int[] { 4, 2, 8 }},
                        { '5', new int[] { 5, 2, 8 }},
                        { '6', new int[] { 6, 2, 8 }},
                        { '7', new int[] { 7, 2, 8 }},
                        { '8', new int[] { 8, 2, 8 }},
                        { '9', new int[] { 9, 2, 8 }},
                        { 'H', new int[] { 7, 1, 8 }},
                        { 'P', new int[] {15, 1, 8 }},
                        { '-', new int[] {16, 2, 8 }},
                        { '?', new int[] {19, 2, 8 }},
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region FE7 Reel
                { "FE7_Reel",         new Font_Data("FE7_Reel",         16, 32,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { 'a', new int[] {  0, 0, 12 }},
                        { 'b', new int[] {  1, 0, 12 }},
                        { 'c', new int[] {  2, 0, 12 }},
                        { 'd', new int[] {  3, 0, 12 }},
                        { 'e', new int[] {  4, 0, 12 }},
                        { 'f', new int[] {  5, 0, 12 }},
                        { 'g', new int[] {  6, 0, 12 }},
                        { 'h', new int[] {  7, 0, 12 }},
                        { 'i', new int[] {  8, 0, 12 }},
                        { 'j', new int[] {  9, 0, 12 }},
                        { 'k', new int[] { 10, 0, 12 }},
                        { 'l', new int[] { 11, 0, 12 }},
                        { 'm', new int[] { 12, 0, 12 }},
                        { 'n', new int[] { 13, 0, 12 }},
                        { 'o', new int[] { 14, 0, 12 }},
                        { 'p', new int[] { 15, 0, 12 }},
                        { 'q', new int[] { 16, 0, 12 }},
                        { 'r', new int[] { 17, 0, 12 }},
                        { 's', new int[] { 18, 0, 12 }},
                        { 't', new int[] { 19, 0, 12 }},
                        { 'u', new int[] { 20, 0, 12 }},
                        { 'v', new int[] { 21, 0, 12 }},
                        { 'w', new int[] { 22, 0, 12 }},
                        { 'x', new int[] { 23, 0, 12 }},
                        { 'y', new int[] { 24, 0, 12 }},
                        { 'z', new int[] { 25, 0, 12 }},
                        { 'A', new int[] {  0, 1, 12 }},
                        { 'B', new int[] {  1, 1, 12 }},
                        { 'C', new int[] {  2, 1, 12 }},
                        { 'D', new int[] {  3, 1, 12 }},
                        { 'E', new int[] {  4, 1, 12 }},
                        { 'F', new int[] {  5, 1, 12 }},
                        { 'G', new int[] {  6, 1, 12 }},
                        { 'H', new int[] {  7, 1, 12 }},
                        { 'I', new int[] {  8, 1, 12 }},
                        { 'J', new int[] {  9, 1, 12 }},
                        { 'K', new int[] { 10, 1, 12 }},
                        { 'L', new int[] { 11, 1, 12 }},
                        { 'M', new int[] { 12, 1, 12 }},
                        { 'N', new int[] { 13, 1, 12 }},
                        { 'O', new int[] { 14, 1, 12 }},
                        { 'P', new int[] { 15, 1, 12 }},
                        { 'Q', new int[] { 16, 1, 12 }},
                        { 'R', new int[] { 17, 1, 12 }},
                        { 'S', new int[] { 18, 1, 12 }},
                        { 'T', new int[] { 19, 1, 12 }},
                        { 'U', new int[] { 20, 1, 12 }},
                        { 'V', new int[] { 21, 1, 12 }},
                        { 'W', new int[] { 22, 1, 12 }},
                        { 'X', new int[] { 23, 1, 12 }},
                        { 'Y', new int[] { 24, 1, 12 }},
                        { 'Z', new int[] { 25, 1, 12 }},
                        { ' ', new int[] {  0, 0,  6 }},
                    },
                    #endregion
                    #region Character Offsets
                    null) },
                    #endregion
                #endregion
                #region FE7 Reel2
                { "FE7_Reel2",        new Font_Data("FE7_Reel2",        16, 24,
                    #region Character Data
                    new Dictionary<char, int[]> {
                        { 'a', new int[] {  0, 0, 8 }},
                        { 'b', new int[] {  1, 0, 8 }},
                        { 'c', new int[] {  2, 0, 7 }},
                        { 'd', new int[] {  3, 0, 8 }},
                        { 'e', new int[] {  4, 0, 7 }},
                        { 'f', new int[] {  5, 0, 8 }},
                        { 'g', new int[] {  6, 0,10 }},
                        { 'h', new int[] {  7, 0, 8 }},
                        { 'i', new int[] {  8, 0, 7 }},
                        { 'j', new int[] {  9, 0, 8 }},
                        { 'k', new int[] { 10, 0, 8 }},
                        { 'l', new int[] { 11, 0, 6 }},
                        { 'm', new int[] { 12, 0,14 }},
                        { 'n', new int[] { 13, 0, 8 }},
                        { 'o', new int[] { 14, 0, 8 }},
                        { 'p', new int[] { 15, 0,11 }},
                        { 'q', new int[] { 16, 0,10 }},
                        { 'r', new int[] { 17, 0, 7 }},
                        { 's', new int[] { 18, 0, 6 }},
                        { 't', new int[] { 19, 0, 7 }},
                        { 'u', new int[] { 20, 0, 9 }},
                        { 'v', new int[] { 21, 0,10 }},
                        { 'w', new int[] { 22, 0,12 }},
                        { 'x', new int[] { 23, 0,11 }},
                        { 'y', new int[] { 24, 0, 8 }},
                        { 'z', new int[] { 25, 0,10 }},
                        { 'A', new int[] {  0, 1,15 }},
                        { 'B', new int[] {  1, 1,13 }},
                        { 'C', new int[] {  2, 1,14 }},
                        { 'D', new int[] {  3, 1,15 }},
                        { 'E', new int[] {  4, 1,15 }},
                        { 'F', new int[] {  5, 1,15 }},
                        { 'G', new int[] {  6, 1,13 }},
                        { 'H', new int[] {  7, 1,15 }},
                        { 'I', new int[] {  8, 1,11 }},
                        { 'J', new int[] {  9, 1,15 }},
                        { 'K', new int[] { 10, 1,15 }},
                        { 'L', new int[] { 11, 1,13 }},
                        { 'M', new int[] { 12, 1,15 }},
                        { 'N', new int[] { 13, 1,14 }},
                        { 'O', new int[] { 14, 1,14 }},
                        { 'P', new int[] { 15, 1,12 }},
                        { 'Q', new int[] { 16, 1,16 }},
                        { 'R', new int[] { 17, 1,14 }},
                        { 'S', new int[] { 18, 1,12 }},
                        { 'T', new int[] { 19, 1,13 }},
                        { 'U', new int[] { 20, 1,15 }},
                        { 'V', new int[] { 21, 1,15 }},
                        { 'W', new int[] { 22, 1,16 }},
                        { 'X', new int[] { 23, 1,16 }},
                        { 'Y', new int[] { 24, 1,32 }}, //Yeti
                        { 'Z', new int[] { 25, 1,13 }},
                        { ' ', new int[] { 0, 0, 4 }},
                    },
                    #endregion
                    #region Character Offsets
                    new Dictionary<char, int> {
                        { 'K', -1 },
                        { 'R', -1 },
                        { 'T', -1 },
                        { 'f', -1 },
                        { 'g', -1 },
                        { 'j', -1 },
                        { 'p', -3 },
                    }) },
                    #endregion
                #endregion
            };
        }

        public string Name { get; private set; }
        public int CharWidth { get; private set; }
        public int CharHeight { get; private set; }
        public Dictionary<char, int[]> CharacterData { get; private set; }
        public Dictionary<char, int> CharacterOffsets { get; private set; }

        private Font_Data(string name, int char_width, int char_height, Dictionary<char, int[]> character_data, Dictionary<char, int> character_offsets)
            : this()
        {
            Name = name;
            CharWidth = char_width;
            CharHeight = char_height;
            CharacterData = character_data;
            CharacterOffsets = character_offsets;
        }
    }
}
