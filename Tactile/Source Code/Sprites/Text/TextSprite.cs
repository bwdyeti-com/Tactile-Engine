using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ContentManager = Microsoft.Xna.Framework.Content.ContentManager;
using TactileLibrary;

namespace Tactile.Graphics.Text
{
    class TextSprite : Sprite
    {
        protected readonly static TactileLibrary.Noise NoiseGen = new TactileLibrary.Noise(12345);

        protected string Font = "";
        private string FontFilename = "";
        protected string Text = "";
        protected int Character_Count = 0, Character_Timer = 0;
        protected bool One_At_A_Time = false;
        private int Text_Speed = -1;
        private Dictionary<int, string> TextColors = new Dictionary<int, string>();

        #region Accessors
        public string text
        {
            get { return Text; }
            set
            {
                Text = value;
                Character_Count = (One_At_A_Time ? 0 : Text.Length);
                Character_Timer = 0;
            }
        }

        public int character_count
        {
            get { return Character_Count; }
            set { Character_Count = value; }
        }

        public bool one_at_a_time
        {
            set
            {
                One_At_A_Time = value;
                Character_Count = (One_At_A_Time ? 0 : Text.Length);
                Character_Timer = 0;
            }
        }

        public int text_speed { set { Text_Speed = value; } }

        internal static int TEXT_COUNTER { get { return Window_Message.TEXT_SPEED[(Constants.Message_Speeds)Global.game_options.text_speed]; } }
        private int text_counter
        {
            get
            {
                if (Text_Speed == -1)
                    return TEXT_COUNTER;
                return Window_Message.TEXT_SPEED[(Constants.Message_Speeds)Text_Speed];
            }
        }

        protected Maybe<Font_Data> font_data
        {
            get
            {
                if (Font_Data.Data.ContainsKey(Font))
                    return Font_Data.Data[Font];
                return default(Maybe<Font_Data>);
            }
        }

        public int text_width { get { return Font_Data.text_width(Text, Font); } }
        public int CharHeight { get { return font_data.IsNothing ? 0 : ((Font_Data)font_data).CharHeight; } }
        #endregion

        public TextSprite() : this(-1) { }
        public TextSprite(int text_speed)
        {
            Text_Speed = text_speed;
        }
        public TextSprite(string font, Texture2D texture, Vector2 loc, string text = "") : this()
        {
            this.Font = font;
            this.FontFilename = font;
            this.texture = texture;
            this.loc = loc;
            this.text = text;
        }
        public TextSprite(string font, ContentManager contentManager, string color, Vector2 loc, string text = "") : this()
        {
            SetFont(font, contentManager, color);
            this.loc = loc;
            this.text = text;
        }

        public override string ToString()
        {
            return "FE Text: \"" + Text + "\"";
        }

        /// <summary>
        /// Sets the font for this text sprite.
        /// If a <see cref="ContentManager"/> is given, also replaces the texture.
        /// Also uses a specific color texture if one is given.
        /// </summary>
        public void SetFont(
            string font,
            ContentManager contentManager = null,
            string color = null,
            string actualFilename = null)
        {
            this.Font = font;
            // If the filename and font name are the same (because actualFilename is null)
            this.FontFilename = string.IsNullOrEmpty(actualFilename) ?
                font : actualFilename;
            if (contentManager != null)
            {
                SetColor(contentManager, color);
            }
        }
        /// <summary>
        /// Sets the font for this text sprite.
        /// </summary>
        public void SetFont(
            string font,
            string actualFilename)
        {
            SetFont(font, null, null, actualFilename);
        }

        public void SetColor(
            ContentManager contentManager,
            string color = null)
        {
#if DEBUG
            if (string.IsNullOrEmpty(FontFilename) && !string.IsNullOrEmpty(Font))
            {
                FontFilename = Font;
                Print.message(string.Format(
                    "TextSprite doesn't have FontFilename set"));
            }
#endif

            // Append the color to the filename, if one is given
            string filename = string.Format(string.IsNullOrEmpty(color) ?
                @"Graphics/Fonts/{0}" : @"Graphics/Fonts/{0}_{1}",
                this.FontFilename, color);
            if (Global.content_exists(filename))
            {
                this.texture = contentManager.Load<Texture2D>(filename);
            }
            else
            {
                this.texture = null;
#if DEBUG
                Print.message(string.Format(
                    "Tried to load missing font texture\n\"{0}\"", filename));
#endif
            }
        }

        public override void update()
        {
            while (One_At_A_Time && Character_Count < Text.Length && Character_Timer <= 0)
            {
                Character_Count++;
                Character_Timer = text_counter;
            }
            if (Character_Timer > 0)
                Character_Timer--;
            base.update();
        }

        internal void SetTextFontColor(string color)
        {
            if (this.text != null)
                TextColors[this.text.Length] = string.Format("{0}_{1}", FontFilename, color);
        }
        internal void SetTextFontColor(int index, string color)
        {
            TextColors[index] = string.Format("{0}_{1}", FontFilename, color);
        }

        internal void clear_text_colors()
        {
            TextColors.Clear();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            draw(sprite_batch, this.texture, draw_offset);
        }
        public override void draw(SpriteBatch sprite_batch, Texture2D texture, Vector2 draw_offset = default(Vector2))
        {
            if (!(texture == null))
                if (visible)
                {
                    Vector2 temp_loc = Vector2.Zero;
                    // If this font has no data, return
                    if (font_data.IsNothing)
                        return;
                    Font_Data data = font_data;

                    for (int i = 0; i < Character_Count; i++)
                    {
                        char letter = Text[i];
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
                            // If the character isn't a space, render the character and also do some other things
                            if (letter != ' ')
                            {
                                if (data.CharacterOffsets != null)
                                    if (data.CharacterOffsets.ContainsKey(letter))
                                        temp_loc.X += data.CharacterOffsets[letter];
                                draw_letter(sprite_batch, texture, i, draw_offset, temp_loc, data);
                            }
                            advance_character(letter, ref temp_loc, data);
                        }
                    }
                }
        }

        public void draw_multicolored(SpriteBatch sprite_batch, Maybe<Vector2> draw_offset = default(Maybe<Vector2>))
        {
            if (draw_offset.IsNothing)
                draw_offset = Vector2.Zero;

            if (visible && TextColors.Any())
            {
                int i = TextColors.Keys.Min();
                Texture2D texture = Global.Content.Load<Texture2D>(string.Format(@"Graphics/Fonts/{0}", TextColors[i]));

                Vector2 temp_loc = Vector2.Zero;
                // If this font has no data, return
                if (font_data.IsNothing)
                    return;
                Font_Data data = font_data;

                for(; i < Character_Count; i++)
                {
                    if (TextColors.ContainsKey(i))
                        texture = Global.Content.Load<Texture2D>(string.Format(@"Graphics/Fonts/{0}", TextColors[i]));

                    char letter = Text[i];
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
                        // If the character isn't a space, render the character and also do some other things
                        if (letter != ' ')
                        {
                            if (data.CharacterOffsets != null)
                                if (data.CharacterOffsets.ContainsKey(letter))
                                    temp_loc.X += data.CharacterOffsets[letter];
                            draw_letter(sprite_batch, texture, i, draw_offset, temp_loc, data);
                        }
                        advance_character(letter, ref temp_loc, data);
                    }
                }
            }
        }

        protected void draw_letter(SpriteBatch sprite_batch, Texture2D texture, int index, Vector2 draw_offset, Vector2 temp_loc, Font_Data data)
        {
            Vector2 loc = (this.loc + draw_vector()) - draw_offset;
            // Sine wave
            if (false)
            {
                const float magnitude = 3 / 16f;
                float sineOffset = Global.game_system.play_time_sine_wave(
                    1f,
                    -(index + ((int)(loc.Y / 16) * Config.FRAME_RATE * 37 / 64)) * 2,
                    false);
                loc += new Vector2(0, (int)((magnitude * data.CharHeight) * sineOffset));
            }
            // Random shake
            else if (false)
            {
                const float magnitude = 1.025f / 16;
                double random = NoiseGen.noise(
                    Global.game_system.total_play_time / 2 +
                    index * 40 + ((int)(loc.Y / 16) * Config.WINDOW_WIDTH));
                Vector2 shake_offset = Additional_Math.from_polar(
                    (float)((random + 1) * MathHelper.TwoPi / 2),
                    magnitude * data.CharHeight);
                loc += new Vector2((int)shake_offset.X, (int)shake_offset.Y);
            }
            
            Rectangle src_rect = data.CharacterSrcRect(Text[index]);

            Vector2 offset = this.offset;
            if (mirrored)
                offset.X = src_rect.Width - offset.X;
            
            sprite_batch.Draw(texture, loc, src_rect, tint, angle, offset - temp_loc, scale,
                    mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
        }

        protected virtual void advance_character(char letter, ref Vector2 temp_loc, Font_Data data)
        {
            temp_loc.X += data.CharacterWidth(letter);
        }
    }
}
