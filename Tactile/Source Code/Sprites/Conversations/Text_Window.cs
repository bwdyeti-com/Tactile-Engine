using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;
using TactileListExtension;

namespace Tactile
{
    enum SpeakerArrowSide { Bottom, Left, Right, Top }

    class Text_Window : Text_Box
    {
        const int FADE_TIME = 10;
        const int CG_FADE_TIME = 16;
        internal const int BASE_Y = 24;
        internal const int BACKGROUND_OFFSET = -16;
        internal const int OFFSCREEN_OFFSET = -16;
        internal const int CG_OFFSET = 96;
        internal const int CG_WIDTH = 200;
        const int GUTTERS = 16;
        const int OFFSCREEN_GUTTER = 16;
        const bool SPEAKER_ARROW_POINTS_DOWN = true;

        private Vector2 Loc;
        private int Base_Y;
        private List<int> Move = new List<int>();
        private int Speaker = Window_Message.CENTER_TOP_SPEAKER;
        private int? SpeakerArrowPos;
        private SpeakerArrowSide ArrowSide = SPEAKER_ARROW_POINTS_DOWN ? SpeakerArrowSide.Bottom : SpeakerArrowSide.Top;
        private int FadeTimer;
        private int Scroll_Dist = 0, Temp_Scroll_Dist = 0;
        private TextSprite SpeakerName;
        private Maybe<int> NextSpeakerId, NextSpeakerWidth, NextSpeakerLoc;
        private string NextSpeakerName = null;
        private List<TextSprite> Text_Imgs = new List<TextSprite>();
        private RasterizerState Text_State = new RasterizerState { ScissorTestEnable = true };
        private Rectangle Text_Rect;
        private string Color = "Black";
        public bool window_visible = true;

        #region Accessors
        new public Vector2 loc
        {
            get { return Loc; }
            set
            {
                Loc = value;
                Text_Rect.X = (int)Loc.X + 8;
                Text_Rect.Y = (int)Loc.Y + 8;
            }
        }

        public int base_y { set { Base_Y = value; } }

        new public int width
        {
            set
            {
                Width = value;
                Text_Rect.Width = Width - 16;
                text_clear();
                Scroll_Dist = 0;
            }
        }

        public override int opacity
        {
            get { return base.opacity; }
            set
            {
                base.opacity = value;
                SpeakerName.tint = this.tint;
            }
        }

        public string text_color
        {
            set
            {
                if (Global.content_exists(string.Format(@"Graphics/Fonts/{0}_{1}",
                    Config.CONVO_FONT, value)))
                {
                    Color = value;
                    if (Text_Imgs.Any())
                    {
                        var last_text_img = Text_Imgs.Last();
                        last_text_img.SetTextFontColor(Color);
                    }
                }
            }
        }

        private int CharHeight { get { return Window_Message.FontData.CharHeight; } }
        #endregion

        public Text_Window() { }

        public Text_Window(int width, int height)
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Message_Window");
            Height = height;
            Text_Rect = new Rectangle(0, 0, 0, Height - 16);
            this.loc = loc;
            Base_Y = (int)loc.Y;
            this.width = width;

            SpeakerName = new TextSprite();
            SpeakerName.draw_offset = new Vector2(24, -12);
            SpeakerName.SetFont(Config.UI_FONT, Global.Content, "White");

            opacity = 0;
        }

        public override void update()
        {
            if (!ready)
            {
                if (FadeTimer != 0)
                {
                    int fade_time = Speaker == Window_Message.CG_VOICEOVER_SPEAKER ? CG_FADE_TIME : FADE_TIME;
                    int fade;
                    // Fade in
                    if (FadeTimer > 0)
                    {
                        FadeTimer--;
                        fade = fade_time - FadeTimer;
                    }
                    // Fade out
                    else
                    {
                        FadeTimer++;
                        fade = -FadeTimer;
                    }
                    opacity = (255 * fade) / fade_time;
                    fade = fade_time - fade;
                    if (Speaker == Window_Message.CG_VOICEOVER_SPEAKER)
                        offset.Y = 0;
                    else
                        offset.Y = -(int)(Math.Pow(fade, 2) * 0.1f + fade);
                }

                if (FadeTimer == 0 && NextSpeakerId.IsSomething)
                {
                    set_speaker(NextSpeakerId, NextSpeakerWidth, NextSpeakerLoc, NextSpeakerName);
                    NextSpeakerId = default(Maybe<int>);
                    NextSpeakerWidth = default(Maybe<int>);
                    NextSpeakerLoc = default(Maybe<int>);
                }
            }

            //if (Move.Count > 0)
            //    offset.Y += Move.pop();
        }

        internal bool ready { get { return FadeTimer == 0 && NextSpeakerId.IsNothing; } }

        #region Text Controls
        public void try_set_speaker(int id, int width, Maybe<int> speaker_loc_x, string name)
        {
            if (Speaker == Window_Message.CG_VOICEOVER_SPEAKER)
            {
                reset_move(false);

                NextSpeakerId = id;
                NextSpeakerWidth = width;
                NextSpeakerLoc = speaker_loc_x;
                NextSpeakerName = name;
            }
            else
                set_speaker(id, width, speaker_loc_x, name);
        }

        private void set_speaker(int id, int width, Maybe<int> speaker_loc_x, string name)
        {
            this.width = width;

            Speaker = id;
            int x = 0;
            // If an onscreen normal speaker
            if (speaker_loc_x.IsSomething)
            {
                x = (int)speaker_loc_x + 20;
                draw_offset.Y = 0;
            }
            // Else a special case speaker
            else
            {
                // Offscreen right
                if (Speaker == Face_Sprite_Data.FACE_COUNT + 1)
                {
                    x = Config.WINDOW_WIDTH - (Width + OFFSCREEN_GUTTER);
                    draw_offset.Y = OFFSCREEN_OFFSET;
                }
                else switch (Speaker)
                {
                    // Background
                    case Window_Message.CENTER_TOP_SPEAKER:
                        x = Config.WINDOW_WIDTH / 2;
                        draw_offset.Y = BACKGROUND_OFFSET;
                        break;
                    // CG Voiceover
                    case Window_Message.CG_VOICEOVER_SPEAKER:
                        x = Config.WINDOW_WIDTH / 2;
                        draw_offset.Y = CG_OFFSET;
                        break;
                    // No speaker
                    case Window_Message.NO_SPEAKER:
                        x = Config.WINDOW_WIDTH * 2;
                        break;
                    // Offscreen left
                    case 0:
                        x = OFFSCREEN_GUTTER;
                        draw_offset.Y = OFFSCREEN_OFFSET;
                        break;
                }
            }
            int window_x, speaker_x;
            // If offscreen speaker
            if (Speaker == 0 || Speaker == Face_Sprite_Data.FACE_COUNT + 1)
            {
                window_x = x;
                if (Speaker == 0)
                    speaker_x = 8;
                else
                    speaker_x = Width - 24;
            }
            else
            {
                if (Speaker > Face_Sprite_Data.FACE_COUNT / 2)
                    x -= 56;
                window_x = (x + 8) - Width / 2;
                if (Speaker > 0 && Speaker <= Face_Sprite_Data.FACE_COUNT)
                {
                    if (Speaker <= Face_Sprite_Data.FACE_COUNT / 2)
                        window_x = Math.Max(window_x, GUTTERS);
                    else
                        window_x = Math.Min(window_x, Config.WINDOW_WIDTH - (Width + GUTTERS));
                }
                speaker_x = x - window_x;
            }
            Loc.X = window_x;
            Text_Rect.X = window_x + 8;
            //x -= window_x; //Debug
            if (Speaker >= 0 && Speaker <= Face_Sprite_Data.FACE_COUNT + 1)
                SpeakerArrowPos = speaker_x / 8 * 8;
            else
                SpeakerArrowPos = null;
            ArrowSide = SPEAKER_ARROW_POINTS_DOWN ? SpeakerArrowSide.Bottom : SpeakerArrowSide.Top;

            opacity = 0;
            set_speaker_name(name);
        }

        private void set_speaker_name(string name)
        {
            SpeakerName.text = name;
            SpeakerName.offset.X = SpeakerName.text_width / 2;
            NextSpeakerName = null;
        }

        internal void SetSpeakerArrow(int? x = null, SpeakerArrowSide side = SpeakerArrowSide.Bottom)
        {
            if (x != null)
                SpeakerArrowPos = (x / 8) * 8;
            else
                SpeakerArrowPos = x;
            ArrowSide = side;
        }

        public void text_set(string text)
        {
            Text_Imgs[Text_Imgs.Count - 1].text += text;
        }

        public void text_clear()
        {
            Text_Imgs.Clear();
            Text_Imgs.Add(new TextSprite());
            Text_Imgs[0].SetFont(Config.CONVO_FONT);
            Text_Imgs[0].SetTextFontColor(0, Color);
        }

        public void add_line()
        {
            int i = 0;
            while (i < Text_Imgs.Count)
            {
                if (Text_Imgs[i].loc.Y <= -this.CharHeight)
                    Text_Imgs.RemoveAt(i);
                else
                    i++;
            }
            Text_Imgs.Add(new TextSprite());
            Text_Imgs[Text_Imgs.Count - 1].SetFont(Config.CONVO_FONT);
            Text_Imgs[Text_Imgs.Count - 1].SetTextFontColor(0, Color);
            Text_Imgs[Text_Imgs.Count - 1].loc = Text_Imgs[Text_Imgs.Count - 2].loc + new Vector2(0, this.CharHeight);
        }

        public void reset_move(bool fade_in = true)
        {
            if (Speaker == Window_Message.NO_SPEAKER)
                FadeTimer = 0;
            else
            {
                int fade_time = Speaker == Window_Message.CG_VOICEOVER_SPEAKER ? CG_FADE_TIME : FADE_TIME;
                FadeTimer = fade_time * (fade_in ? 1 : -1);
            }
        }
        #endregion

        #region Scrolling
        public void scroll_up(int y)
        {
            Temp_Scroll_Dist += y;
            Scroll_Dist += y;
        }

        public void scroll()
        {
            foreach (TextSprite text in Text_Imgs)
                text.loc.Y -= Temp_Scroll_Dist;
            Temp_Scroll_Dist = 0;
        }
        #endregion

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    // Window
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    Vector2 offset = this.offset;
                    if (window_visible)
                    {
                        draw_window(sprite_batch, draw_offset);
                        if (Speaker == Window_Message.CG_VOICEOVER_SPEAKER)
                            SpeakerName.draw(sprite_batch, draw_offset - (this.loc + draw_vector() - offset));
                    }
                    sprite_batch.End();
                    // Text
                    if (ready)
                    {
                        sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(new Rectangle(
                            (Text_Rect.X + (int)draw_vector().X) - (int)draw_offset.X,
                            (Text_Rect.Y + (int)draw_vector().Y) - (int)draw_offset.Y,
                            Text_Rect.Width, Text_Rect.Height));
                        if (sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0 && sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0)
                        {
                            sprite_batch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, null, null, Text_State);
                            foreach (TextSprite text in Text_Imgs)
                                text.draw_multicolored(sprite_batch, -(Loc + draw_vector() + new Vector2(8, 8)) + draw_offset);
                            sprite_batch.End();
                        }
                    }
                }
        }

        protected override void draw_window(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            if (Speaker == Window_Message.CG_VOICEOVER_SPEAKER)
            {
                // Draw nameplate for speaker
                sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset,
                    new Rectangle(0, 24, 64, 24), tint, angle,
                    offset + new Vector2(8, 16), scale, SpriteEffects.None, Z);
            }
            Rectangle src_rect;
            int y = 0;
            int temp_height;
            while (y < Height)
            {
                temp_height = 8;
                if (Height - (y + 8) < 8 && Height - (y + 8) != 0)
                    temp_height = Height - (y + 8);
                int x = 0;
                int temp_width;
                while (x < Width)
                {
                    temp_width = 8;
                    if (Width - (x + 8) < 8 && Width - (x + 8) != 0)
                        temp_width = Width - (x + 8);
                    // Top left corner is removed for cg voiceover
                    if (!(y == 0 && x < 56 && Speaker == Window_Message.CG_VOICEOVER_SPEAKER))
                    {
                        // Left side
                        if (x == 0)
                        {
                            src_rect = this.src_rect((y == 0 ? 7 : (Height - y <= 8 ? 1 : 4)),
                                temp_width, temp_height);
                            if (mirrored)
                                offset.X = src_rect.Width - offset.X;
                            // Little arrow to the speaker
                            if (SpeakerArrowPos != null && ArrowSide == SpeakerArrowSide.Left &&
                                (y == SpeakerArrowPos || y - 8 == SpeakerArrowPos))
                            {
                                if (y == SpeakerArrowPos)
                                {
                                    sprite_batch.Draw(texture, (loc + new Vector2(x, y) + draw_vector()) - draw_offset,
                                        new Rectangle(24, 0, 12, 16), tint, MathHelper.PiOver2,
                                        offset + new Vector2(0, 8), scale,
                                        SpriteEffects.FlipHorizontally, Z);
                                    sprite_batch.Draw(texture, (loc + new Vector2(x, y + 12) + draw_vector()) - draw_offset,
                                        new Rectangle(24 + 12, 0, 4, 16), tint, MathHelper.PiOver2,
                                        offset + new Vector2(0, 8), scale,
                                        SpriteEffects.FlipHorizontally, Z);
                                }
                            }
                            else
                                sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset,
                                    src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                    mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        }
                        // Right side
                        else if (Width - x <= 8)
                        {
                            src_rect = this.src_rect((y == 0 ? 9 : (Height - y <= 8 ? 3 : 6)),
                                temp_width, temp_height);
                            if (mirrored)
                                offset.X = src_rect.Width - offset.X;
                            // Little arrow to the speaker
                            if (SpeakerArrowPos != null && ArrowSide == SpeakerArrowSide.Right &&
                                (y == SpeakerArrowPos || y - 8 == SpeakerArrowPos))
                            {
                                if (y == SpeakerArrowPos)
                                {
                                    sprite_batch.Draw(texture, (loc + new Vector2(x, y) + draw_vector()) - draw_offset,
                                        new Rectangle(24, 0, 12, 16), tint, -MathHelper.PiOver2,
                                        offset + new Vector2(12, 0), scale,
                                        SpriteEffects.None, Z);
                                    sprite_batch.Draw(texture, (loc + new Vector2(x, y + 12) + draw_vector()) - draw_offset,
                                        new Rectangle(24 + 12, 0, 4, 16), tint, -MathHelper.PiOver2,
                                        offset + new Vector2(4, 0), scale,
                                        SpriteEffects.None, Z);
                                }
                            }
                            else
                                sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset,
                                src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        }
                        // Middle
                        else
                        {
                            src_rect = this.src_rect((y == 0 ? 8 : (Height - y <= 8 ? 2 : 5)),
                                temp_width, temp_height);
                            if (mirrored)
                                offset.X = src_rect.Width - offset.X;
                            // Little arrow to the speaker
                            bool normalRender = true;
                            if (SpeakerArrowPos != null &&
                                (x == SpeakerArrowPos || x - 8 == SpeakerArrowPos))
                            {
                                // Check if top or bottom
                                if ((ArrowSide == SpeakerArrowSide.Bottom && Height - y <= 8) ||
                                    (ArrowSide == SpeakerArrowSide.Top && y == 0))
                                {
                                    if (x == SpeakerArrowPos)
                                    {
                                        if (ArrowSide == SpeakerArrowSide.Top)
                                            sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset,
                                                new Rectangle(24, 0, 16, 16), tint, angle,
                                                offset - new Vector2(x, y - 8), scale,
                                                SpriteEffects.FlipVertically | ((Speaker <= Face_Sprite_Data.FACE_COUNT / 2) ? SpriteEffects.FlipHorizontally : SpriteEffects.None), Z);
                                        else
                                            sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset,
                                                new Rectangle(24, 0, 16, 16), tint, angle,
                                                offset - new Vector2(x, y), scale,
                                                (Speaker <= Face_Sprite_Data.FACE_COUNT / 2) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                                    }
                                    normalRender = false;
                                }
                            }
                            if (normalRender)
                                sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset,
                                    src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                    mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        }
                    }
                    x += temp_width;
                }
                y += temp_height;
            }
        }
    }
}
