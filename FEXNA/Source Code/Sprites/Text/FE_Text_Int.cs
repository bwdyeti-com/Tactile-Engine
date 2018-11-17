using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Text
{
    class FE_Text_Int : FE_Text
    {
        public override string ToString()
        {
            return "FE Text Int: \"" + Text + "\"";
        }

        public FE_Text_Int() : base() { }
        public FE_Text_Int(string font, Texture2D texture, Vector2 loc, string text = "")
            : base(font, texture, loc, text) { }

        public override void draw(SpriteBatch sprite_batch, Texture2D texture, Vector2 draw_offset = default(Vector2))
        {
            if (!(texture == null))
                if (visible)
                {
                    Vector2 temp_loc = Vector2.Zero;
                    Vector2 offset = this.offset;
                    // If this font has no data, return
                    if (!Font_Data.Data.ContainsKey(Font))
                        return;
                    Font_Data data = Font_Data.Data[Font];
                    string value = Text.ToString();
                    for (int i = Character_Count - 1; i >= 0; i--)
                    {
                        char letter = value[i];
                        if (letter == '\n')
                        {
                            temp_loc.X = 0;
                            temp_loc.Y += data.CharHeight;
                        }
                        else
                        {
                            // If there's not data for this letter
                            if (!data.CharacterData.ContainsKey(letter))
                                continue;
                            int[] char_data = data.CharacterData[letter];
                            advance_character(letter, ref temp_loc, data);
                            // If the character isn't a space, render the character and also do some other things
                            if (letter != ' ')
                            {
                                //if (i == Character_Count - 1 || i + 1 < Character_Count && Text[i + 1] == '\n') //Debug
                                //    if (Font_Data.letter_offsets.ContainsKey(Font))
                                //        if (Font_Data.letter_offsets[Font].ContainsKey(letter))
                                //            temp_loc.X -= Font_Data.letter_offsets[Font][letter];
                                draw_letter(sprite_batch, texture, i, draw_offset, temp_loc, data);

                                if (data.CharacterOffsets != null)
                                    if (data.CharacterOffsets.ContainsKey(letter))
                                        temp_loc.X -= data.CharacterOffsets[letter];
                            }
                        }
                    }
                }
        }

        protected override void advance_character(char letter, ref Vector2 temp_loc, Font_Data data)
        {
            int[] char_data = data.CharacterData[letter];
            temp_loc.X -= (char_data.Length == 3 ? char_data[2] : char_data[4]);
        }
    }
}
