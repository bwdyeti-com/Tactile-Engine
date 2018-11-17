using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Convo_Background : Sprite
    {
        protected string Filename = "";
        protected int Timer = -1;
        protected int Black_Screen_Opacity = 0;
        private Texture2D OldTexture;
        public bool clear_faces = false;

        #region Accessors
        public string filename
        {
            get { return Filename; }
            set
            {
                if (value != "" && texture != null)
                {
                    OldTexture = texture;
                    Timer = 19;
                    Black_Screen_Opacity = 255;
                }
                else
                    Timer = 36;
                Filename = value;
                clear_faces = false;
            }
        }

        public bool ready { get { return Timer == -1; } }

        public bool full_black { get { return Black_Screen_Opacity == 255; } }
        #endregion

        public Convo_Background()
        {
            opacity = 0;
            stereoscopic = Config.CONVO_BG_DEPTH;
        }

        public override void update()
        {
            //fix_tone
            if (Timer >= 0)
            {
                switch (Timer)
                {
                    case 0:
                        Black_Screen_Opacity = (Timer * 16);
                        OldTexture = null;
                        if (Filename == "")
                            texture = null;
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        Black_Screen_Opacity = (Timer * 16);
                        break;
                    case 16:
                    case 17:
                    case 18:
                        Black_Screen_Opacity = 255;
                        break;
                    case 19:
                        Black_Screen_Opacity = 255;
                        if (Filename == "")
                            opacity = 0;
                        else
                        {
                            texture = Global.Content.Load<Texture2D>(@"Graphics/Panoramas/" + Filename);
                            opacity = 255;
                        }
                        break;
                    case 36:
                        if (texture == null && Filename != "")
                            texture = Global.Content.Load<Texture2D>(@"Graphics/Panoramas/" + Filename);
                        break;
                    default:
                        Black_Screen_Opacity = ((37 - Timer) * 16);
                        break;
                }
                Timer = Math.Max(-1, Timer - 1);
                if (Timer == 17 && Filename == "")
                    clear_faces = true;
            }
        }

        public override void draw(SpriteBatch sprite_batch, Texture2D texture, Vector2 draw_offset = default(Vector2))
        {
            base.draw(sprite_batch, texture, draw_offset);
            if (visible)
                if (OldTexture != null)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored)
                        offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(OldTexture, loc + draw_vector(),
                        src_rect, new Color(Black_Screen_Opacity, Black_Screen_Opacity, Black_Screen_Opacity, Black_Screen_Opacity),
                        angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }

        public void draw_black(SpriteBatch sprite_batch)
        {
            if (visible)
            {
                if (OldTexture == null && texture != null)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored)
                        offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(texture, loc + draw_vector(),
                        src_rect, new Color(0, 0, 0, Black_Screen_Opacity), angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
            }
        }
    }
}
