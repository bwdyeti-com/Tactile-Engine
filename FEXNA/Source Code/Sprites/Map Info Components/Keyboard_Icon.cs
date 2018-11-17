using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA.Graphics.Help
{
    class Keyboard_Icon : Sprite
    {
        const int MINIMUM_WIDTH = 12;
        const int LETTER_OFFSET = 2;
        const int WIDTH_ADDITION = 5;

        protected Inputs Key;
        protected FE_Text Letter;
        protected int _width, _textWidth;
        protected bool Colon_Visible;

        #region Accessors
        internal virtual int minimum_width { get { return MINIMUM_WIDTH; } }
        internal virtual int letter_offset { get { return LETTER_OFFSET; } }
        protected virtual int width_addition { get { return WIDTH_ADDITION; } }

        public string letter
        {
            set
            {
                Letter.text = value;

                _textWidth = Letter.text_width + this.width_addition;
                Letter.loc = new Vector2(
                    this.letter_offset + (_textWidth < this.minimum_width ?
                        (this.minimum_width - _textWidth) / 2 : 0), -1);
                _textWidth = Math.Max(this.minimum_width, _textWidth);
                _width = _textWidth;
            }
        }

        public int ButtonWidth { get { return _width; } }

        public int TextWidth
        {
            get { return _textWidth; }
            set { _textWidth = value + this.width_addition; }
        }

        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Letter.stereoscopic = value;
            }
        }
        #endregion

        public Keyboard_Icon(Inputs input, Texture2D texture, bool colon = true)
        {
            Key = input;

            this.texture = texture;

            Letter = new FE_Text();
            Letter.Font = "FE7_Text";
            Letter.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Keys");

            refresh();

            Colon_Visible = colon;
        }

        internal void refresh()
        {
            this.letter = Input.key_name((int)Key);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Vector2 loc = this.loc + draw_vector() - draw_offset;
                    Vector2 offset = this.offset;
                    // Left
                    sprite_batch.Draw(texture, loc,
                        new Rectangle(0, 0, 4, 16), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Middle
                    sprite_batch.Draw(texture, loc + new Vector2(4, 0),
                        new Rectangle(4, 0, 4, 16), tint, angle, offset, new Vector2((_textWidth - 8) / 4f, 1f) * scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Right
                    sprite_batch.Draw(texture, loc + new Vector2(_textWidth - 4, 0),
                        new Rectangle(8, 0, 4, 16), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);

                    // Colon
                    if (Colon_Visible)
                        sprite_batch.Draw(texture, loc + new Vector2(_textWidth, 0),
                            new Rectangle(12, 0, 4, 16), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);

                    // Label
                    Letter.draw(sprite_batch, offset - loc);
                }
        }
    }
}
