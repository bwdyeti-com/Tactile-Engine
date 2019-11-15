using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNATexture2DExtension;

namespace FEXNA
{
    public class Face_Sprite : Sprite
    {
        protected Color[] Palette;
        protected int Expression = 0;
        protected int Emotion_Count = Face_Sprite_Data.DEFAULT_EMOTIONS;
        protected int Blink_Mode = 0;
        protected int Blink_Timer, Talk_Phase, Talk_Timer;
        protected int Phase_In = 13;
        protected int Phase_Out = -1;
        protected int Move_To = 0;
        protected int Bob = -1;
        protected bool Idle = false;
        protected string Filename;
        protected bool Override_Tone;
        protected Vector2 Loc, Eyes_Loc, Mouth_Loc;
        protected int EyesFrame, MouthFrame;
        protected Rectangle Eyes_Rect, Mouth_Rect;
        protected bool Convo_Placement_Offset = false;
        private static Random rand = new Random();

        #region Accessors
        new public Vector2 loc
        {
            get { return Loc; }
            set
            {
                Loc = value;
                reset_move();
            }
        }

        public bool phased_out
        {
            get { return Phase_Out <= 0; }
        }

        public override bool mirrored
        {
            get { return Mirrored; }
            set
            {
                Mirrored = value;
                if (texture != null)
                    RefreshSrcRect();
                set_offsets();
            }
        }

        public int expression
        {
            set
            {
                Expression = Math.Max(value >= Emotion_Count ? 0 : value, 0);
                if (texture != null)
                    RefreshSrcRect();
                blink_control();
                talk_control();
            }
        }

        public int move_to
        {
            set
            {
                Move_To = value;
                if (Loc.X == Move_To)
                    Bob = Config.FACE_SPRITE_MOVEMENT_BOB_TIME;
            }
        }

        public bool moving
        {
            get { return (Move_To != Loc.X); }
            //get { return ((Move_To != Loc.X) || Bob >= 0); }
        }

        public bool moving_full
        {
            get { return ((Move_To != Loc.X) || Bob >= 0); }
        }

        public bool idle { set { Idle = value; } }

        public bool convo_placement_offset { set { Convo_Placement_Offset = value; } }
        #endregion

        protected Face_Sprite()
        {
            initialize_palette();
        }
        public Face_Sprite(string filename)
        {
            initialize_palette();
            initialize(filename, false);
        }
        public Face_Sprite(string filename, bool override_tone)
        {
            initialize_palette();
            initialize(filename, override_tone);
        }

        protected void initialize(string filename, bool override_tone)
        {
            this.filename = filename;
            Override_Tone = override_tone;
            Loc.Y = Config.WINDOW_HEIGHT;
            int alpha = 255 - (int)((255 / 12f) * (Phase_In - 1));
            tint = new Color(alpha, alpha, alpha, 255);
            opacity = 0;
        }

        protected void initialize_palette()
        {
            Palette = new Color[Palette_Handler.PALETTE_SIZE];
        }

        protected void refresh_palette()
        {
            if (palette_exists)
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(Global.face_palette_data[Filename].Length <= 256,
                    string.Format("Face sprite \"{0}\" has more than 256 colors in its palette data,\nand should instead not have a palette and be full color", Filename));
#endif
                for (int i = 0; i < Global.face_palette_data[Filename].Length && i < Palette.Length; i++)
                    Palette[i] = Global.face_palette_data[Filename][i];
            }
        }

        protected bool palette_exists { get { return Global.face_palette_data.ContainsKey(Filename) && Global.face_palette_data[Filename].Length > 0; } }

        internal string filename
        {
            set
            {
                string name = value;

                if (Face_Sprite_Data.FACE_TO_GENERIC_RENAME.ContainsKey(name))
                {
                    name = Face_Sprite_Data.FACE_TO_GENERIC_RENAME[name].GraphicName;
                }
                else if (Face_Sprite_Data.FACE_RENAME.ContainsKey(name))
                    name = Face_Sprite_Data.FACE_RENAME[name];

                // Exact filename exists
                if (Global.content_exists(@"Graphics/Faces/" + name))
                    Filename = name;
                // Generic version of filename exists
                else if (Global.content_exists(@"Graphics/Faces/" + name.Split(Constants.Actor.ACTOR_NAME_DELIMITER)[0]))
                    Filename = name.Split(Constants.Actor.ACTOR_NAME_DELIMITER)[0];
                // Default face exists
                else if (Global.content_exists(@"Graphics/Faces/" + Face_Sprite_Data.DEFAULT_FACE))
                    Filename = Face_Sprite_Data.DEFAULT_FACE;
                else
                    Filename = "";

                if (!string.IsNullOrEmpty(Filename))
                    texture = Global.Content.Load<Texture2D>(@"Graphics/Faces/" + Filename);
                // Determines emotion count
                get_emotion_count();
                if (texture == null)
                    src_rect = new Rectangle(0, 0, 0, 0);
                else
                {
                    RefreshSrcRect();
                    offset = new Vector2(src_rect.Width / 2, src_rect.Height);
                }
                EyesFrame = 0;
                MouthFrame = 0;
                set_offsets();
                refresh_palette();

                // Auto-recolor named generics
                if (Face_Sprite_Data.FACE_TO_GENERIC_RENAME.ContainsKey(value))
                {
                    recolor_country(value);
                }
            }
        }

        private void get_emotion_count()
        {
            string name = Filename.Split(Constants.Actor.BUILD_NAME_DELIMITER)[0];
            if (Global.face_data != null && Global.face_data.ContainsKey(name)) //FaceData //Debug
            {
                Emotion_Count = Face_Sprite_Data.EmotionCount(Global.face_data[name]);
            }
            else
                Emotion_Count = Face_Sprite_Data.DEFAULT_EMOTIONS;
        }

        protected void set_offsets()
        {
            if (texture == null)
            {
                Eyes_Loc = Vector2.Zero;
                Mouth_Loc = Vector2.Zero;
            }
            else
            {
                Eyes_Loc = new Vector2(mirrored ? this.FaceWidth - (eyes_offset.X + (int)eyes_size.X) : eyes_offset.X, eyes_offset.Y);
                Mouth_Loc = new Vector2(mirrored ? this.FaceWidth - (mouth_offset.X + (int)mouth_size.X) : mouth_offset.X, mouth_offset.Y);
            }
        }

        protected Rectangle eyes_rect(int eyesFrame)
        {
            Vector2 size = eyes_size;
            return new Rectangle(
                this.SrcX + (int)size.X * eyesFrame,
                this.SrcY + this.FaceHeight,
                (int)size.X, (int)size.Y);
        }
        protected Rectangle mouth_rect(int mouthFrame)
        {
            Vector2 size = mouth_size;
            return new Rectangle(
                this.SrcX + (int)size.X * mouthFrame,
                this.SrcY + this.FaceHeight + (int)this.eyes_size.Y,
                (int)size.X, (int)size.Y);
        }

        private IEnumerable<Rectangle> frame_rects()
        {
            yield return new Rectangle(
                (int)eyes_offset.X + this.SrcX,
                (int)eyes_offset.Y + this.SrcY,
                (int)eyes_size.X, (int)eyes_size.Y);
            yield return new Rectangle(
                (int)mouth_offset.X + this.SrcX,
                (int)mouth_offset.Y + this.SrcY,
                (int)mouth_size.X, (int)mouth_size.Y);
        }
        protected FEXNA_Library.RectangleExclusion frame_exclusion()
        {
            return new FEXNA_Library.RectangleExclusion(this.src_rect, frame_rects());
        }

        public void recolor_country(string country)
        {
            if (!Face_Recolor.COUNTRY_COLORS.ContainsKey(country))
            {
                // This and the Game_Actor.flag_name version should be in one method call //Yeti
                if (Face_Sprite_Data.FACE_TO_GENERIC_RENAME.ContainsKey(country))
                    country = Face_Sprite_Data.FACE_TO_GENERIC_RENAME[country].RecolorCountry;

                //else if (Face_Sprite_Data.FACE_COUNTRY_RENAME.ContainsKey(country)) //Debug
                if (Face_Sprite_Data.FACE_COUNTRY_RENAME.ContainsKey(country))
                    country = Face_Sprite_Data.FACE_COUNTRY_RENAME[country].RecolorCountry;
            }

            if (Face_Recolor.COUNTRY_COLORS.ContainsKey(country) && texture != null)
            {
                Dictionary<Color, Color> recolors = new Dictionary<Color, Color>();
                foreach (KeyValuePair<FEXNA.Face_Color_Keys, Color> pair in FEXNA.Face_Recolor.COUNTRY_COLORS[country])
                {
                    recolors.Add(FEXNA.Face_Recolor.COUNTRY_COLORS["Default"][pair.Key], pair.Value);
                }

                for (int i = 0; i < Palette.Length; i++)
                    if (recolors.ContainsKey(Palette[i]))
                        Palette[i] = recolors[Palette[i]];

                //texture = texture.recolor_face(country);
                //Global.Face_Textures.Add(texture);
            }
        }

        protected virtual void RefreshSrcRect()
        {
            this.src_rect = new Rectangle(
                this.SrcX, this.SrcY,
                this.FaceWidth, this.FaceHeight);
        }

        #region Controls
        public void blink(int val)
        {
            Blink_Mode = (int)MathHelper.Clamp(val, 0, 3);
            if (!new List<int> { 5, 6, 7 }.Contains(Blink_Timer) && Blink_Mode == 2)
                Blink_Timer = 10;
            if (!new List<int> { 10 }.Contains(Blink_Timer) && Blink_Mode == 3)
                Blink_Timer = 0;
            else if (Blink_Mode == 1)
                Blink_Timer = 10;
        }

        public void talk()
        {
            if (new List<int> { 0, 3 }.Contains(Talk_Phase) && Talk_Timer == 0)
            {
                Talk_Phase = 4;
                Talk_Timer = talk_speed();
            }
        }

        public void stop()
        {
            Talk_Phase = 0;
            MouthFrame = 0;
            blink_control();
        }

        protected int talk_speed()
        {
            int speed = 5;
            switch (Global.game_options.text_speed)
            {
                case 0:
                case 1:
                case 2:
                    speed = rand.Next(6) + 2;
                    break;
                case 3:
                    speed = 1;
                    break;
            }
            return Math.Max(1, speed);
        }

        public void reset_move()
        {
            Move_To = (int)Loc.X;
        }

        public void reverse()
        {
            if (Bob < 0)
                Bob = Config.FACE_SPRITE_MOVEMENT_BOB_TIME;
            mirrored = !mirrored;
        }
        #endregion

        public bool ready { get { return Phase_In == -1; } }

        protected int FaceWidth
        {
            get
            {
                if (texture == null)
                    return 0;

                return Face_Sprite_Data.FaceWidth(this.face_data, texture.Width);
            }
        }
        protected int FaceHeight
        {
            get
            {
                if (texture == null)
                    return 0;

                return Face_Sprite_Data.FaceHeight(this.face_data, texture.Height);
            }
        }

        protected int SrcX
        {
            get
            {
                int x = 0;
                if (this.face_data.Asymmetrical)
                    x = Mirrored ? this.FaceWidth : 0;
                return x;
            }
        }
        protected int SrcY { get { return Expression * Face_Sprite_Data.EmotionHeight(this.face_data, texture.Height); } }

        protected Vector2 eyes_size { get { return Face_Sprite_Data.EYES_FACE_SIZE; } }
        protected Vector2 mouth_size { get { return Face_Sprite_Data.MOUTH_FACE_SIZE; } }

        public Vector2 eyes_offset { get { return face_data.EyesOffset; } }
        public Vector2 mouth_offset { get { return face_data.MouthOffset; } }
        public Vector2 status_offset { get { return face_data.StatusOffset; } }
        public int status_frame { get { return face_data.StatusFrame; } }
        public int placement_offset { get { return face_data.PlacementOffset; } }

        public FEXNA_Library.Face_Data face_data
        {
            get
            {
                return get_face_data(Filename);
            }
        }
        public static FEXNA_Library.Face_Data get_face_data(string filename)
        {
            string name = filename.Split(Constants.Actor.BUILD_NAME_DELIMITER)[0];
            if (Global.face_data != null && Global.face_data.ContainsKey(name)) //FaceData //Debug
                return Global.face_data[name];
            return new FEXNA_Library.Face_Data();
        }

        #region Update
        public override void update()
        {
#if DEBUG
            if (false)//Global.Input.pressed(Inputs.A)) //Debug
            {
                int j = -1;
                if (false) // if skip outline
                {
                    j = 0;
                    for (; j < 15; j++)
                    {
                        if (Palette[j].R == 88 && Palette[j].G == 64 && Palette[j].B == 96)
                            break;
                    }
                }
                int k = (j == 0 ? 1 : 0);
                Color temp = Palette[k];
                for (int i = k + 1; i < 15; i++)
                {
                    if (i != j)
                    {
                        Palette[k] = Palette[i];
                        k = i;
                    }
                }
                Palette[k] = temp;
            }
#endif

            // Update position
            update_move();
            if (Phase_In >= 0)
            {
                if (Phase_In < 13)
                {
                    if (Phase_In == 12)
                        opacity = 255;
                    int alpha = 255 - Math.Max(0, (255 * Phase_In / 12));
                    tint = new Color(alpha, alpha, alpha, 255);
                }
                Phase_In--;
            }
            if (Phase_Out >= 0)
            {
                int alpha = 255 - (int)((255 / 12f) * (12 - Phase_Out));
                tint = new Color(alpha, alpha, alpha, 255);
                Phase_Out--;
            }
            // Random blink start check
            if (Blink_Timer == 0 && Blink_Mode == 0 && !Global.scene.is_worldmap_scene)
            {
                if (rand.Next(150) == 0)
                    Blink_Timer = 10;
            }
            if (!((Blink_Mode == 2 && Blink_Timer == 5) || (Blink_Mode == 3 && Blink_Timer == 0)))
                blink_control();
            // Handles talking
            if (Talk_Timer == 1 && Talk_Phase != 0)
            {
                Talk_Phase = (Talk_Phase + 1) % 4;
                talk_control();
                Talk_Timer = talk_speed();
            }
            // Updates timers
            switch (Blink_Mode)
            {
                case 1:
                    //Blink_Timer = Math.Max(0, Blink_Timer - 1); //Debug
                    break;
                case 2:
                    if (Blink_Timer != 5)
                        Blink_Timer = Math.Max(0, Blink_Timer - 1);
                    break;
                case 3:
                    if (Blink_Timer != 0)
                        Blink_Timer = Math.Max(0, Blink_Timer - 1);
                    break;
                default:
                    Blink_Timer = Math.Max(0, Blink_Timer - 1);
                    break;
            }
            Talk_Timer = Math.Max(0, Talk_Timer - 1);
        }

        protected void blink_control()
        {
            switch (Blink_Timer)
            {
                case 0:
                case 1:
                    EyesFrame = 0;
                    break;
                case 2:
                case 3:
                case 4:
                case 8:
                case 9:
                case 10:
                    EyesFrame = 1;
                    break;
                case 5:
                case 6:
                case 7:
                    EyesFrame = 2;
                    break;
            }
        }

        protected void talk_control()
        {
            switch (Talk_Phase)
            {
                case 0:
                    MouthFrame = 0;
                    break;
                case 1:
                case 3:
                    MouthFrame = 1;
                    break;
                case 2:
                    MouthFrame = 2;
                    break;
            }
        }

        protected void update_move()
        {
            if (moving_full)
            {
                if ((Move_To != Loc.X) || Bob < 0)
                {
                    int move_to = Move_To; // Why //Yeti
                    Loc.X = Additional_Math.int_closer((int)Loc.X, move_to, (int)MathHelper.Clamp((int)Math.Round(Math.Abs(Loc.X - move_to)) / Config.FACE_SPRITE_MOVEMENT_BOB_TIME, 1, 8));
                    Move_To = move_to; // why
                    if ((Bob < 0) && (Math.Abs((int)Loc.X - Move_To) < Config.FACE_SPRITE_MOVEMENT_BOB_TIME))
                        Bob = Config.FACE_SPRITE_MOVEMENT_BOB_TIME;
                }
                if (Bob == Config.FACE_SPRITE_MOVEMENT_BOB_TIME) Loc.Y += 1;
                if (Bob == 0) Loc.Y -= 1;
                Bob--;
            }
        }
        #endregion

        public void phase_in()
        {
            if (Phase_Out == -1)
            {
                Phase_In = -1;
                tint = new Color(255, 255, 255, 255);
            }
        }

        public void remove()
        {
            Phase_Out = 11;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    // Adjust position
                    if (Convo_Placement_Offset)
                        draw_offset -= new Vector2(placement_offset * (mirrored ? 1 : -1), 0);
                    // Setup shader for palettes
                    if (palette_exists)
                    {
                        Effect effect = Global.effect_shader();
                        if (effect != null)
                        {
                            Texture2D palette_texture = Global.palette_pool.get_palette();
                            palette_texture.SetData<Color>(Palette);
#if __ANDROID__
                            // There has to be a way to do this for both
                            effect.Parameters["Palette"].SetValue(palette_texture);
#else
                            sprite_batch.GraphicsDevice.Textures[2] = palette_texture;
#endif
                            sprite_batch.GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
                            effect.CurrentTechnique = effect.Techniques["Palette1"];
                            effect.Parameters["color_shift"].SetValue(new Vector4(0, 0, 0, 0));
                            effect.Parameters["opacity"].SetValue(1f);
                        }
                        sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect);
                    }
                    else
                        sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored)
                        offset.X = src_rect.Width - offset.X;

                    // Crop eyes and mouth out of body, then draw it
                    foreach (var rect in frame_exclusion())
                    {
                        Vector2 frame_offset = new Vector2(
                            rect.X - src_rect.X, rect.Y - src_rect.Y);
                        frame_offset = new Vector2(mirrored ?
                            this.FaceWidth - (frame_offset.X + (int)rect.Width) :
                            frame_offset.X, frame_offset.Y);

                        sprite_batch.Draw(texture,
                            this.loc + frame_offset + draw_vector() - draw_offset,
                            rect, tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                    
                    int eyes_frame = Idle ? 0 : EyesFrame;
                    int mouth_frame = Idle ? 0 : MouthFrame;
                    // Eyes
                    sprite_batch.Draw(texture, Loc + Eyes_Loc + draw_vector() - draw_offset,
                        eyes_rect(eyes_frame), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Mouth
                    sprite_batch.Draw(texture, Loc + Mouth_Loc + draw_vector() - draw_offset,
                        mouth_rect(mouth_frame), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);

                    sprite_batch.End();
#if __ANDROID__
                    // There has to be a way to do this for both
                    if (Global.effect_shader() != null)
                        Global.effect_shader().Parameters["Palette"].SetValue((Texture2D)null);
#else
                    sprite_batch.GraphicsDevice.Textures[2] = null;
#endif
                }
        }
    }
}
