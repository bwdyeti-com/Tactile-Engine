using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA_Library;

namespace FEXNA
{
    public enum TextSkips { None, NextScene, SkipEvent }

    public class Window_Message : Stereoscopic_Graphic_Object
    {
        internal const string FONT = "FE7_Convo";
        internal const int NO_SPEAKER = -2;
        internal const int CENTER_TOP_SPEAKER = -1;
        internal const int CG_VOICEOVER_SPEAKER = -3;
        public readonly static Dictionary<Constants.Message_Speeds, int> TEXT_SPEED =
            new Dictionary<Constants.Message_Speeds, int> {
                { Constants.Message_Speeds.Slow,   5 }, // Slow //9
                { Constants.Message_Speeds.Normal, 2 }, // Norm //5
                { Constants.Message_Speeds.Fast,   1 }, // Fast //2
                { Constants.Message_Speeds.Max,    0 }  // Max
        };
        protected const int SCROLL_TIME = 16;
        const int ARROW_WIDTH = 12;

        protected int Message_Lines = Face_Sprite_Data.MESSAGE_LINES;
        protected Constants.Message_Speeds Text_Speed;
        protected List<string> Text = null;
        protected bool Active = false;
        protected bool Waiting_For_Event = false;
        protected bool Closing = false;
        protected bool Reverse;
        protected bool Skipping;
        private TextSkips _convoSkip;
        protected bool Event_Skip;
        protected bool QuickRender;
        protected int Speaker = NO_SPEAKER, Temp_Speaker = NO_SPEAKER;
        protected int Phase, Phase_Timer, Timer;
        protected int Line_Wait, Wait_Timer;
        protected bool Scroll, Full_Scroll, Wait, Was_Waiting, Text_End;
        protected bool Text_Shown;
        protected string Default_Text_Color, Backlog_Default_Color;
        protected float Pitch = 0;
        protected List<int> Widths = new List<int>();
        protected Dictionary<int, string> Names;
        protected Vector2 Text_Loc = Vector2.Zero;
        protected int Width, Height;
        protected Dictionary<int, Face_Sprite> Faces = new Dictionary<int, Face_Sprite>();
        protected List<Tuple<bool, int>> Face_Zs = new List<Tuple<bool, int>>();
        protected List<Face_Sprite> Clearing_Faces = new List<Face_Sprite>();
        protected Dictionary<int, bool> Reversed_Faces = new Dictionary<int, bool>();
        protected Text_Window Window_Img;
        private Convo_Background Background;
        private Convo_Backlog Backlog;
        private Window_Convo_Location Location;
        protected Message_Arrow Arrow;
        protected string Background_Name, Temp_Background_Name;
        protected Maybe<float> Face_Stereo_Offset = new Maybe<float>(), Location_Stereo_Offset = new Maybe<float>();

        #region Accessors
        internal bool active
        {
            get { return Active; }
        }
        internal bool waiting_for_event
        {
            get { return Waiting_For_Event; }
        }

        internal bool reverse { set { Reverse = value; } }

        internal bool event_skip
        {
            get { return Event_Skip; }
            set { Event_Skip = value; }
        }

        protected int pitch
        {
            set
            {
                Pitch = (float)Math.Log(value / 100f, 2);
                //Global.Audio.set_pitch_global_var("Talk_Boop", (float)Math.Log(value / 100f, 2));
                //Audio.set_global_var("Talk_Pitch", (float)Math.Log(value / 100f, 2)); //Yeti
            }
            get { return (int)Math.Round(Math.Pow(2, Pitch) * 100); }
        }

        private Maybe<int> current_speaker_default_pitch
        {
            get
            {
                // Pitch for speaker name
                if (Global.face_data.ContainsKey(this.current_speaker_name))
                    return Global.face_data[this.current_speaker_name].Pitch;
                // Pitch for face sprite
                if (Faces.ContainsKey(Speaker))
                    return Faces[Speaker].face_data.Pitch;

                return default(Maybe<int>);
            }
        }

        private string current_speaker_name { get { return Speaker == NO_SPEAKER ? "" : Names[Speaker]; } }

        internal bool is_clearing_faces
        {
            get { return Clearing_Faces.Count > 0; }
        }

        internal bool background_active { get { return Background_Name != ""; } }

        internal bool wait { get { return Wait; } }

        internal bool text_end { get { return Text_End; } }

        internal float face_stereoscopic { set { Face_Stereo_Offset = value; } }
        internal float location_stereoscopic { set { Location_Stereo_Offset = value; } }
        internal float backlog_stereoscopic { set { Backlog.stereoscopic = value; } }

        internal TextSkips ConvoSkip { set { _convoSkip = value; } }
        internal bool backlog_active { get { return Backlog.Active; } }
        internal bool closing { get { return Closing; } }

        internal static Font_Data FontData { get { return Font_Data.Data[FONT]; } }
        #endregion

        public Window_Message()
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(!valid_face_or_offscreen_speaker(CENTER_TOP_SPEAKER),
                string.Format(
@"Speaker id for 'top center of the
screen' is within the range of normal
speakers ({0}-{1}). Change it to another
number so conversation display can
function normally. Suggested value is -1.",
                    0, Face_Sprite_Data.FACE_COUNT + 1));
            System.Diagnostics.Debug.Assert(!valid_face_or_offscreen_speaker(NO_SPEAKER),
                string.Format(
@"Speaker id for 'no one currently
speaking' is within the range of normal
speakers ({0}-{1}). Change it to another
number so conversation display can
function normally. Suggested value is -2.",
                    0, Face_Sprite_Data.FACE_COUNT + 1));
            System.Diagnostics.Debug.Assert(!valid_face_or_offscreen_speaker(CG_VOICEOVER_SPEAKER),
                string.Format(
@"Speaker id for 'bottom of the screen
CG voiceover' is within the range of normal
speakers ({0}-{1}). Change it to another
number so conversation display can
function normally. Suggested value is -3.",
                    0, Face_Sprite_Data.FACE_COUNT + 1));
#endif
            Backlog = new Convo_Backlog();
            Text_Speed = (Constants.Message_Speeds)Global.game_options.text_speed;
            reset(false, new Vector2(40, 24),
                240, Message_Lines * Window_Message.FontData.CharHeight + 16);
        }

        public void reset(bool active)
        {
            reset(active, this.loc, Width, Height);
        }
        public virtual void reset(bool active, Vector2 loc, int width, int height)
        {
            Text_Speed = (Constants.Message_Speeds)Global.game_options.text_speed;
            Active = active;
            Waiting_For_Event = false;
            this.loc = loc;
            Width = width;
            Height = height;
            Skipping = false;
            _convoSkip = TextSkips.None;
            Phase = 0;
            Phase_Timer = 0;
            Timer = 0;
            Line_Wait = 0;
            Wait_Timer = 0;
            Scroll = false;
            Full_Scroll = false;
            Wait = false;
            Was_Waiting = false;
            Text_End = false;
            Faces.Clear();
            Clearing_Faces.Clear();
            Face_Zs.Clear();
            Widths.Clear();
            Text_Shown = false;
            Temp_Speaker = NO_SPEAKER;
            Background_Name = "";
            Temp_Background_Name = "";
            Reverse = false;
            set_default_color();
            reset_pitch();
            Event_Skip = false;
            QuickRender = false;
            Reversed_Faces.Clear();
            Names = new Dictionary<int, string>();
            for (int i = 0; i <= Face_Sprite_Data.FACE_COUNT + 1; i++)
                Names[i] = "Name" + i.ToString();
            Names[CENTER_TOP_SPEAKER] = "???";
            Names[CG_VOICEOVER_SPEAKER] = "???";
            Backlog.reset();
        }

        protected virtual void set_default_color()
        {
            Default_Text_Color = "Black";
            Backlog_Default_Color = "White";
        }
        protected void reset_text_color()
        {
            if (Window_Img != null)
                Window_Img.text_color = Default_Text_Color;
            if (Backlog != null && Backlog.started)
                Backlog.set_color(Backlog_Default_Color);
        }

        private void reset_pitch()
        {
            Pitch = 0;
        }

        public void resume()
        {
            Waiting_For_Event = false;
        }

        public void append_new_text()
        {
            text_setup(true);
            Waiting_For_Event = false;
        }

        protected virtual void setup_images(int width, int height)
        {
            Window_Img = new Text_Window(width, height);
            Window_Img.loc = this.loc;
            Window_Img.base_y = (int)this.loc.Y;
            Window_Img.text_color = Default_Text_Color;
            Background = new Convo_Background();
        }

        #region Close
        protected virtual void begin_terminate_message()
        {
            Closing = true;
            if (Window_Img != null && !Window_Img.ready)
                return;
            if (Background_Name != "" && Background.filename != "")
                Background.filename = "";
            if (Background.clear_faces)
            {
                Background.clear_faces = false;
                Faces.Clear();
                Clearing_Faces.Clear();
                Face_Zs.Clear();
                Window_Img = null;
            }
            if (!Background.ready)
            {
                if (Location != null && Background.full_black)
                    Location = null;
                return;
            }
            terminate_message();
        }

        public void terminate_message()
        {
            terminate_message(false);
        }
        public void terminate_message(bool immediately)
        {
            Background = null;
            Location = null;
            Background_Name = "";
            Arrow = null;
            Active = false;
            Waiting_For_Event = false;
            Window_Img = null;
            Text = null;
            while (Faces.Count > 0)
                foreach (int i in Faces.Keys)
                {
                    remove_face(i);
                    break;
                }
            Faces.Clear();
            Widths.Clear();
            if (Global.scene.is_map_scene)
                Global.game_state.wait_time = 1;
            Closing = false;
            Phase = 0;
            if (immediately)
                clear_faces();
        }
        #endregion

        #region Refresh
        protected bool refresh()
        {
            if (Global.game_temp.message_text != null && Text == null)
                text_setup();
            if (Global.game_temp.message_text == null && Text == null)
                throw new Exception("Window message running with no text set");
            switch (Phase)
            {
                // Setup
                case 0:
                    if (text_process())
                        Phase = 1;
                        //Phase++; //Debug
                    break;
                // Waits for face sprites, text boxes, and such
                case 1:
                    update_phase_1();
                    break;
                // Regular processing
                case 2:
                    return text_process();
                // Changing speaker
                case 3:
                    switch (Phase_Timer)
                    {
                        case 0:
                        /*case 1: //Debug
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            Phase_Timer++;
                            break;
                        case 6:*/
                            Widths.RemoveAt(0);
                            Text_Loc = Vector2.Zero;
                            new_text_box();
                            Phase_Timer++;
                            break;
                        case 1:
                            if (Window_Img.ready)
                            {
                                if (Speaker == NO_SPEAKER)
                                    Phase_Timer = 25;
                                else
                                    Phase_Timer = 1;//8; //Debug
                                Phase = 1;
                            }
                            break;
                    }
                    break;
            }
            return true;
        }

        protected virtual void update_phase_1()
        {
            switch (Phase_Timer)
            {
                case 0:
                    if (Faces.Values.All(x => x.ready))
                        Phase_Timer++;
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    Phase_Timer++;
                    break;
                case 7:
                    if (Speaker != Temp_Speaker)
                        Window_Img.reset_move();
                    Phase_Timer++;
                    break;
                case 8:
                    if (Window_Img.ready)
                        Phase_Timer = 18; //Phase_Timer++; //Debug
                    break;
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                    Phase_Timer++;
                    break;
                case 25:
                    Temp_Speaker = Speaker;
                    Phase_Timer = 0;
                    Phase = 2;
                    break;
                default:
                    Phase_Timer++;
                    if (Phase_Timer > 25)
                        Phase_Timer = 25;
                    break;
            }
        }

        protected void new_text_box()
        {
            // Changing the width clears the text as well
            reset_text_color();
            Maybe<int> speaker_x = Maybe<int>.Nothing;
            if (Faces.ContainsKey(Speaker))
                speaker_x = (int)Faces[Speaker].loc.X;
            else if (valid_onscreen_speaker(Speaker))
                speaker_x = (int)face_location(Speaker).X;

            Window_Img.try_set_speaker(Speaker, Widths[0],
                speaker_x, this.current_speaker_name);
        }

        protected void clear_text_box()
        {
            Speaker = NO_SPEAKER;
            new_text_box();
            Arrow = null;
        }

        protected void text_setup(bool continuing_text = false)
        {
            if (Window_Img == null)
                Text_Loc = Vector2.Zero;
            string text = Global.game_temp.message_text;
            continuing_text &= Text != null;
            if (continuing_text)
                text = string.Format(@"\s[{0}]", NO_SPEAKER) + text;

            // Safe numbers: 1-6, 14-17, 19-27, 31
            // 0x01-0x06, 0x0e-0x11, 0x13-0x1b, 0x1f

            // Change "\\\\" to "\000" for convenience
            text = Regex.Replace(text, @"\\\\", "" + (char)(0x0));


            foreach (var command in TextCommand.COMMANDS.Values)
            {
                command.replace_raw_tags(ref text);
            }

            List<string> text_list = text.Split('\n').ToList();
            var widths = get_text_widths(text_list);
            if (continuing_text)
                widths = widths.Skip(1).ToList();
            Widths.AddRange(widths);

            if (!continuing_text)
                Text = new List<string>();
            foreach (string str in text_list)
                Text.Add(str);
            if (Window_Img == null)
                setup_images(Widths[0], Height);

            Global.game_temp.message_text = null;
        }

        protected List<int> get_text_widths(List<string> text_list)
        {
            List<int> widths = new List<int>();
            // Width of the current line
            int width = 0;
            // Width of the current line after the most recent input wait
            int stop_width = 0;
            int? speaker = null;
            List<string> text_array = new List<string>();
            // Split Text on "\scroll"s
            foreach (string str in text_list)
                foreach (string substr in str.Split(new string[] { TextCommand.COMMANDS["Scroll"].marker_identifier }, StringSplitOptions.RemoveEmptyEntries))
                    text_array.Add(substr);
            var names = new Dictionary<int, string>();
            for (int i = 0; i <= Face_Sprite_Data.FACE_COUNT + 1; i++)
                names[i] = null;
            names[CENTER_TOP_SPEAKER] = null;
            names[CG_VOICEOVER_SPEAKER] = null;
            if (Text != null)
                foreach (int i in Faces.Keys)
                {
                    names[i] = Names[i];
                }
            foreach (string str in text_array)
            {
                string copytext1 = "" + str;
                string copytext2 = "";
                char? c = null;
                while (copytext1.Any())
                {
                    c = copytext1[0];
                    copytext1 = copytext1.Remove(0, 1);
                    Regex r;
                    if (c == TextCommand.MARKER)
                    {
                        c = copytext1[0];
                        copytext1 = copytext1.Remove(0, 1);
                        string key = TextCommand.key_from_identifier((char)c);
                        switch (key)
                        {
                            // Face change
                            case "F":
                                r = TextCommand.COMMANDS[key].regex();
                                string face_test = r.Match(copytext1).Groups[1].Value;
                                string[] face_array = face_test.Split('|');
                                int face_id = Convert.ToInt32(face_array[0]);
                                if (valid_face_speaker(face_id))
                                {
                                    string name, recolor_country;
                                    get_face_name(face_array[1], out name, out recolor_country);
                                    names[face_id] = name.Split(Constants.Actor.ACTOR_NAME_DELIMITER)[0];
                                }
                                copytext1 = r.Replace(copytext1, "", 1);
                                break;
                            // Move
                            case "M":
                                r = TextCommand.COMMANDS[key].regex();
                                string move_test = r.Match(copytext1).Groups[1].Value;
                                string[] move_array = move_test.Split('|');
                                int id1 = Convert.ToInt32(move_array[0]);
                                int id2 = Convert.ToInt32(move_array[1]);
                                string temp_name = names[id1];
                                names[id1] = names[id2];
                                names[id2] = temp_name;
                                copytext1 = r.Replace(copytext1, "", 1);
                                break;
                            // Speaker change
                            case "S":
                                r = TextCommand.COMMANDS[key].regex();
                                int speaker_id = get_speaker(r.Match(copytext1).Groups[1].Value);
                                copytext1 = r.Replace(copytext1, "", 1);
                                if (speaker != speaker_id || speaker == NO_SPEAKER)
                                {
                                    test_width(ref width, copytext2);
                                    copytext2 = "";
                                    widths.Add(new_box(speaker, ref width, ref stop_width));

                                    speaker = speaker_id;
                                }
                                break;
                            // Set Name
                            case "Name":
                                r = TextCommand.COMMANDS[key].regex();
                                string name_test = r.Match(copytext1).Groups[1].Value;
                                string[] name_array = name_test.Split('|');
                                int name_id = Convert.ToInt32(name_array[0]);
                                if (valid_speaker(name_id))
                                {
                                    names[name_id] = name_array[1];
                                }
                                copytext1 = r.Replace(copytext1, "", 1);
                                break;
                            // Insert face name
                            case "I":
                                r = TextCommand.COMMANDS[key].regex();
                                int name_insert_id = Convert.ToInt32(r.Match(copytext1).Groups[1].Value);
                                copytext1 = r.Replace(copytext1, "", 1);
                                copytext2 += names[name_insert_id] ?? "";
                                break;
                            case "C": // Change text color
                            case "W": // Wait
                            case "Wait":
                            case "E": // Expression set
                            case "B": // Blink set
                            case "G": // Background
                            case "X": // Pitch set
                            case "R": // Reverse face
                            case "FC": // Generic recolor
                            case "Music": // Music
                            case "Loc": // Location display
                            case "Sound": // Sound
                            case "O": // Face order
                            case "Quick": // Skip through text
                                r = TextCommand.COMMANDS[key].regex();
                                copytext1 = r.Replace(copytext1, "", 1);
                                break;
                            // Pause and return to event processing
                            case "Event":
                                break;
                            default:
                                throw new KeyNotFoundException();
                        }
                    }
                    else
                        switch (c)
                        {
                            case '|':
                                test_width(ref stop_width, copytext2, ARROW_WIDTH);
                                break;
                            default:
                                copytext2 += c;
                                break;
                        }
                }
                // If at the last character
                bool end_of_text = false;
                if (str == text_array[text_array.Count - 1])
                    if (str.Length == 0 || c == str[str.Length - 1])
                        end_of_text = true;
                // Should this still be here, since |s at the end of text are handled manually now??? //Yeti
                test_width(ref width, copytext2, end_of_text ? ARROW_WIDTH : 0);
            }
            widths.Add(new_box(speaker, ref width, ref stop_width));
            return widths;
        }

        private int get_speaker(string speaker)
        {
            if (speaker == "nil")
                return NO_SPEAKER;
            return Convert.ToInt32(speaker);
        }

        protected virtual void test_width(ref int width, string copy_text2, int offset = 0)
        {
            int new_width = Font_Data.text_width(copy_text2, FONT) + offset;
            width = new_width > width ? new_width : width;
        }

        protected int new_box(int? speaker, ref int width, ref int stop_width)
        {
            if (speaker == CG_VOICEOVER_SPEAKER)
            {
                width = stop_width = Text_Window.CG_WIDTH;
            }

            width = Math.Max(Math.Max(stop_width, width), 8);
            if (width % 8 != 0)
                width += 8 - (width % 8);
            width += 16;
            int result = width;

            width = 0;
            stop_width = 0;

            return result;
        }

        protected bool text_process()
        {
            if (Line_Wait <= 0)
            {
                if (Scroll)
                {
                    Timer--;
                    Window_Img.scroll_up(1);
                    if ((Timer % 16) == 0)
                    {
                        Text_Loc.Y--;
                        if (Timer == 0)
                        {
                            Scroll = false;
                            Full_Scroll = false;
                            Line_Wait = TEXT_SPEED[Text_Speed];
                        }
                        else if (!Full_Scroll)
                            Window_Img.add_line();
                    }
                    Window_Img.scroll();
                }
                if (!Scroll && Line_Wait <= 0)
                {
                    if (Text == null || Closing)
                        return false;
                    if (Text.Count == 0)
                        return false;
                    if (Timer >= TEXT_SPEED[Text_Speed] || QuickRender)
                        Timer = 0;
                    if (Timer == 0 && is_arrow_cleared())
                    {
                        if (Phase == 0)
                        {
                            // If current line is done
                            if (!Text[0].Any())
                            {
                                Text.RemoveAt(0);
                                if (Text.Count == 0)
                                    Text_End = true;
                                Text_Loc.X = 0;
                                Text_Loc.Y++;
                                Window_Img.add_line();
                                Backlog.new_line();
                                if (Text_Loc.Y >= (Height - 16) / Window_Message.FontData.CharHeight)
                                {
                                    Line_Wait = TEXT_SPEED[Text_Speed];
                                    if (Text.Count > 0)
                                        Scroll = true;
                                    Timer = SCROLL_TIME - 1;
                                }
                                else
                                {
                                    Timer = 0;
                                    Line_Wait = TEXT_SPEED[Text_Speed];
                                }
                            }
                        }
                        if (Text[0].Any())
                        {
                            var result = process_next_character();
                            if (result.IsSomething)
                                return result;
                        }
                        if (!Text[0].Any())
                        {
                            Text.RemoveAt(0);
                            // If all text is done
                            if (Text.Count == 0)
                            {
                                Text_End = true;
                                if (false)//Speaker != NO_SPEAKER) //Yeti
                                {
                                    Wait = true;
                                    create_arrow();
                                }
                            }
                            else
                            {
                                Text_Loc.X = 0;
                                Text_Loc.Y += 1;
                                Window_Img.add_line();
                                Backlog.new_line();
                                if (Text_Loc.Y >= (Height - 16) / Window_Message.FontData.CharHeight)
                                {
                                    if (Text.Count > 0)
                                        Scroll = true;
                                    Timer = SCROLL_TIME - 1;
                                }
                                Line_Wait -= TEXT_SPEED[Text_Speed];
                            }
                        }
                    }
                    Timer++;
                    return !(Scroll || !is_arrow_cleared());
                }
            }
            Line_Wait--;
            return false;
        }

        /// <summary>
        /// Processes one character of the active text.
        /// </summary>
        private Maybe<bool> process_next_character()
        {
            char current_character = Text[0][0];
            if (current_character == TextCommand.MARKER)
                return process_text_command(Text[0][1]);

            #region default: Draw text
            if (Phase == 0)
                return true;
            
            Text[0] = Text[0].Remove(0, 1);
            if (current_character == '|')
            {
                QuickRender = false;
                Wait = true;
                create_arrow();
                return false;
            }

            Text_Shown = true;
            Window_Img.text_set("" + current_character);
            // Send text to
            add_backlog(current_character);
            // Don't play text sound or make speaker talk on new line or when quick rendering
            if (!QuickRender && !new List<char> { '\n' }.Contains(current_character))
            {
                if (!new List<char> { ' ', '.', '!', '?' }.Contains(current_character))
                {
                    speaker_talk();
                }
                if (!Skipping)
                    play_talk_sound();
            }
            Text_Loc.X += Font_Data.text_width("" + current_character, FONT);
            return default(Maybe<bool>);
            #endregion
        }

        private bool process_text_command(char current_character)
        {
            string key = TextCommand.key_from_identifier(current_character);
            var text_command = TextCommand.COMMANDS[key];

            if (Phase > 0)
                Text[0] = Text[0].Remove(0, 2);
            // If not a startup command but we're in the startup phase
            else if (!text_command.StartupCommand)
                return true;


            if (text_command.null_replacement)
            {
                switch (key)
                {
                    #region Scroll: Force scroll
                    case "Scroll":
                        Text_Loc.X = 0;
                        Text_Loc.Y += 1;
                        Window_Img.add_line();
                        Backlog.new_line();
                        Backlog.new_line();
                        Scroll = Full_Scroll = true;
                        Timer = ((int)Text_Loc.Y * SCROLL_TIME);
                        return true;
                    #endregion
                    #region Event: Continue Event Processing
                    case "Event":
                        Waiting_For_Event = true;
                        return false;
                    #endregion
                }
            }
            else
            {
                string filename;
                int id;

                Regex r = TextCommand.COMMANDS[key].regex();
                string test_text = r.Match(Text[0]).Groups[1].Value;
                // If the command can delay its execution for whatever reason
                if (text_command.ExecutionNotCertain)
                {
                    switch (key)
                    {
                        #region S: Change the active speaker
                        case "S":
                            id = get_speaker(test_text);
                            // If next speaker is moving, return and wait until they're done
                            if (Phase != 0)
                                if (Faces.ContainsKey(id) && Faces[id].moving_full)
                                {
                                    Text[0] = string.Concat(TextCommand.MARKER, current_character) + Text[0];
                                    return false;
                                }
                            break;
                        #endregion
                        #region M: Move a face sprite
                        case "M":
                            string[] move_array = test_text.Split('|');
                            int id1 = id_fix(Convert.ToInt32(move_array[0]));
                            int id2 = id_fix(Convert.ToInt32(move_array[1]));
                            // If someone is moving, return and wait until they're done
                            if (Phase != 0)
                                if (Faces.ContainsKey(id1) && Faces[id1].moving_full ||
                                    Faces.ContainsKey(id2) && Faces[id2].moving_full)
                                {
                                    Text[0] = string.Concat(TextCommand.MARKER, current_character) + Text[0];
                                    return false;
                                }
                            break;
                        #endregion
                        #region R: Reverse face
                        case "R":
                            id = id_fix(Convert.ToInt32(test_text));
                            // If reversing face is moving, return and wait until they're done
                            if (Phase != 0)
                                if (Faces.ContainsKey(id) && Faces[id].moving_full)
                                {
                                    Text[0] = string.Concat(TextCommand.MARKER, current_character) + Text[0];
                                    return false;
                                }
                            break;
                        #endregion
                        default:
                            throw new KeyNotFoundException();
                    }
                }
                Text[0] = r.Replace(Text[0], "", 1);
                if (text_command.StartupCommand)
                {
                    remove_consumed_text_command();
                    switch (key)
                    {
                        #region F: Change a face sprite graphic
                        case "F":
                            string[] face_array = test_text.Split('|');
                            id = id_fix(Convert.ToInt32(face_array[0]));
                            if (!valid_face_speaker(id))
                                return false;

                            string recolor_country;
                            get_face_name(face_array[1], out filename, out recolor_country);

                            bool changed_existing_face = false;
                            if (filename == "nil")
                            {
                                if (Faces.ContainsKey(id))
                                {
                                    remove_face(id);
                                    if (id == Speaker)
                                        Speaker = NO_SPEAKER;
                                }
                            }
                            else
                            {
                                Names[id] = filename.Split(Constants.Actor.ACTOR_NAME_DELIMITER)[0];

                                // Fix generic class filenames, if they don't exist
                                filename = FixGenericName(filename);

                                if (Faces.ContainsKey(id))
                                {
                                    Faces[id].filename = filename;
                                    changed_existing_face = true;
                                }
                                else
                                {
                                    Faces[id] = new Face_Sprite(filename);
                                    Faces[id].convo_placement_offset = true;
                                    Face_Zs.Add(new Tuple<bool, int>(true, id));
                                    Faces[id].loc = face_location(id);
                                    Faces[id].reset_move();
                                    // Determines if reversed
                                    bool reverse = false;
                                    if (Reversed_Faces.ContainsKey(id))
                                    {
                                        reverse = Reversed_Faces[id];
                                        Reversed_Faces.Remove(id);
                                    }
                                    Faces[id].mirrored = (id <= Face_Sprite_Data.FACE_COUNT / 2) ^ reverse;
                                    Faces[id].opacity = 0;
                                    Faces[id].recolor_country(recolor_country);
                                    if (Face_Stereo_Offset.IsSomething)
                                        Faces[id].stereoscopic = Face_Stereo_Offset; //Debug
                                }
                            }
                            if (Phase == 0)
                                refresh();
                            else
                            {
                                if (Text[0].Any())
                                {
                                    current_character = Text[0][0];
                                    // After changing faces, perform all following face appearance related commands immediately
                                    // Expression, blink, recolor, name, face order
                                    while (current_character == TextCommand.MARKER &&
                                        new List<string> { "E", "B", "FC", "Name", "O" }
                                            .Select(x => TextCommand.COMMANDS[x].Identifier)
                                            .Contains(Text[0][1]))
                                    {
                                        refresh();
                                        current_character = Text[0][0];
                                    }
                                    // Load another face
                                    if (current_character == TextCommand.MARKER &&
                                            TextCommand.COMMANDS[key].Identifier == Text[0][1])
                                        refresh();
                                    else if (!changed_existing_face)
                                        Wait_Timer += 20;
                                }
                                else if (!changed_existing_face)
                                    Wait_Timer += 20;
                            }
                            return false;
                        #endregion
                        #region S: Change the active speaker
                        case "S":
                            return change_speaker(r, test_text);
                        #endregion
                        #region E: Change a face sprite's expression
                        case "E":
                            face_array = test_text.Split('|');
                            id = id_fix(Convert.ToInt32(face_array[0]));
                            if (!Faces.ContainsKey(id))
                                return false;
                            Faces[id].expression = Convert.ToInt32(face_array[1]);
                            if (Text_Speed != Constants.Message_Speeds.Max || Phase == 0)
                                refresh();
                            return false;
                        #endregion
                        #region B: Change a face sprite's blink mode
                        case "B":
                            face_array = test_text.Split('|');
                            id = id_fix(Convert.ToInt32(face_array[0]));
                            if (!Faces.ContainsKey(id))
                                return false;
                            Faces[id].blink(Convert.ToInt32(face_array[1]));
                            if (Text_Speed != Constants.Message_Speeds.Max || Phase == 0)
                                refresh();
                            return false;
                        #endregion
                        #region G: Change background
                        case "G":
                            if (test_text == "nil")
                            {
                                Background_Name = "";
                                return false;
                            }
                            if (Global.content_exists(@"Graphics/Panoramas/" + test_text))
                                Background_Name = test_text;
                            return false;
                        #endregion
                        #region R: Reverse face
                        case "R":
                            id = id_fix(Convert.ToInt32(test_text));
                            if (!valid_face_speaker(id))
                                return (Phase == 0 ? false : true);
                            if (Faces.ContainsKey(id))
                                Faces[id].reverse();
                            else
                            {
                                if (Reversed_Faces.ContainsKey(id))
                                    Reversed_Faces[id] = !Reversed_Faces[id];
                                else
                                    Reversed_Faces.Add(id, true);
                            }
                            return (Text_Speed == Constants.Message_Speeds.Max && Phase != 0);
                        #endregion
                        #region FC: Recolor generic face ////////////////////////////////////////////////////////////////
                        case "FC":
                            face_array = test_text.Split('|');
                            id = id_fix(Convert.ToInt32(face_array[0]));
                            if (!valid_face_speaker(id))
                                return (Phase == 0 ? false : true);
                            if (Faces.ContainsKey(id))
                                Faces[id].recolor_country(face_array[1]);
                            if (Phase == 0) refresh();
                            return false;
                        #endregion
                        #region Music: Change music
                        case "Music":
                            Global.Audio.BgmFadeOut();
                            if (test_text != "nil")
                                Global.Audio.PlayBgm(test_text, forceRestart: true);
                            return false;
                        #endregion
                        #region Name: Change name
                        case "Name":
                            string[] name_array = test_text.Split('|');
                            id = id_fix(Convert.ToInt32(name_array[0]));
                            if (!valid_speaker(id))
                                return false;
                            Names[id] = name_array[1];
                            return false;
                        #endregion
                        #region O: Face order
                        case "O":
                            id = id_fix(Convert.ToInt32(test_text));
                            if (!valid_face_speaker(id))
                                return (Phase == 0 ? false : true);
                            if (Faces.ContainsKey(id))
                            {
                                Face_Zs.Remove(new Tuple<bool, int>(true, id));
                                Face_Zs.Add(new Tuple<bool, int>(true, id));
                            }
                            if (Phase == 0) refresh();
                            return false;
                        #endregion
                    }
                }
                else
                {
                    switch (key)
                    {
                        #region C: Change text color
                        case "C":
                            string text_color = Default_Text_Color;
                            string backlog_color = Backlog_Default_Color;
                            if (!string.IsNullOrEmpty(test_text) && test_text != "nil")
                            {
                                if (test_text[0] == ':')
                                {
                                    text_color += test_text.Substring(1);
                                    backlog_color += test_text.Substring(1);
                                }
                                else
                                {
                                    text_color = test_text;
                                    backlog_color = test_text;
                                }
                            }
                            Window_Img.text_color = text_color;
                            Backlog.set_color(backlog_color);
                            //Text[0] = (Names[id] ?? "") + Text[0];
                            return true;
                        #endregion
                        #region M: Move a face sprite
                        case "M":
                            string[] move_array = test_text.Split('|');
                            int id1 = id_fix(Convert.ToInt32(move_array[0]));
                            int id2 = id_fix(Convert.ToInt32(move_array[1]));
                            // Move faces
                            if (Faces.ContainsKey(id1))
                            {
                                if (Phase == 0)
                                {
                                    Faces[id1].loc = face_location(id2);
                                    Faces[id1].reset_move();
                                }
                                else
                                    Faces[id1].move_to = (int)face_location(id2).X;
                            }
                            if (Faces.ContainsKey(id2))
                            {
                                if (Phase == 0)
                                {
                                    Faces[id2].loc = face_location(id1);
                                    Faces[id2].reset_move();
                                }
                                else
                                    Faces[id2].move_to = (int)face_location(id1).X;
                            }
                            // Switch ids
                            Face_Sprite face1 = Faces.ContainsKey(id1) ? Faces[id1] : null;
                            Face_Sprite face2 = Faces.ContainsKey(id2) ? Faces[id2] : null;
                            Faces.Remove(id1);
                            Faces.Remove(id2);
                            if (face1 != null)
                                Faces[id2] = face1;
                            if (face2 != null)
                                Faces[id1] = face2;
                            // Reorder names
                            string temp_name = Names[id1];
                            Names[id1] = Names[id2];
                            Names[id2] = temp_name;
                            // Reorder layering
                            for (int i = 0; i < Face_Zs.Count; i++)
                                if (Face_Zs[i].Item1 && Face_Zs[i].Item2 == id1)
                                    Face_Zs[i] = new Tuple<bool, int>(true, id2);
                                else if (Face_Zs[i].Item1 && Face_Zs[i].Item2 == id2)
                                    Face_Zs[i] = new Tuple<bool, int>(true, id1);

                            if (Text_Speed != Constants.Message_Speeds.Max)
                                refresh();
                            return (Text_Speed == Constants.Message_Speeds.Max && Phase != 0);
                        #endregion
                        #region W, Wait: Wait
                        case "W":
                        case "Wait":
                            bool skip = key == "W" && Skipping;
                            if (!skip)
                            {
                                if (Text_Speed == Constants.Message_Speeds.Max && Text_Shown)
                                    Wait_Timer = (int)MathHelper.Clamp(15, Convert.ToInt32(test_text) * 3, 60);
                                else
                                    Wait_Timer = Convert.ToInt32(test_text);
                            }
                            return skip;
                        #endregion
                        #region X: Pitch change
                        case "X":
                            int pitch;
                            Regex pitch_adjust_regex = new Regex(@"[\+\-]([0-9]+)");
                            Match pitch_adjust_match = pitch_adjust_regex.Match(test_text);
                            // Pitch is a modifier
                            if (pitch_adjust_match.Success)
                            {
                                pitch = this.pitch + int.Parse(pitch_adjust_match.Groups[1].Value);
                                this.pitch = (int)MathHelper.Clamp(pitch, 1, 400);
                            }
                            // Pitch is a number
                            else if (int.TryParse(test_text, out pitch))
                            {
                                this.pitch = (int)MathHelper.Clamp(Convert.ToInt32(test_text), 1, 400);
                            }
                            // Get pitch from a named face sprite's default value
                            else
                            {
                                if (Global.face_data.ContainsKey(test_text))
                                    this.pitch = Global.face_data[test_text].Pitch;
                            }
                            return true;
                        #endregion
                        #region Loc: Location display
                        case "Loc":
#if DEBUG
                            if (Phase == 0) //Debug
                                throw new Exception();
                            //remove_consumed_text_command();
#endif

                            Location = new Window_Convo_Location(test_text);
                            if (Location_Stereo_Offset.IsSomething)
                                Location.stereoscopic = Location_Stereo_Offset;
                            Wait_Timer = Window_Convo_Location.WAIT_TIME;
                            return false;
                        #endregion
                        #region Sound: Play sound
                        case "Sound":
#if DEBUG
                            if (Phase == 0) //Debug
                                throw new Exception();
                            //remove_consumed_text_command();
#endif
                            if (test_text != "nil")
                                Global.Audio.play_se("Map Sounds", test_text);
                            return false;
                        #endregion
                        #region I: Insert face name
                        case "I":
                            id = id_fix(Convert.ToInt32(test_text));
                            if (!valid_speaker(id))
                            {
#if DEBUG
                                Print.message("Unusable speaker id " + id.ToString() + " called");
#endif
                                return false;
                            }
                            Text[0] = (Names[id] ?? "") + Text[0];
                            return true;
                        #endregion
                        #region Quick: Skip through text without speech sound
                        case "Quick":
                            QuickRender = test_text == "true";
                            return true;
                        #endregion
                    }
                }
            }
            return default(Maybe<bool>);
        }

        protected virtual bool change_speaker(Regex r, string test_text)
        {
            int id = get_speaker(test_text);
            id = id_fix(id);

            // nah //Debug
            // If this id is for an onscreen face and there is no face in the slot
            //if ((!Faces.ContainsKey(id) && valid_face_speaker(id)) && !valid_offscreen_speaker(id))
            //    return false;

            // If this isn't a slot that can speak, and isn't the no speaker slot
            if (id != NO_SPEAKER && !valid_speaker(id))
            {
#if DEBUG
                Print.message("Unusable speaker id " + id.ToString() + " called");
#endif
                return false;
            }
            if (Phase > 0)
            {
                if (Speaker == id)
                {
                    if (Speaker == NO_SPEAKER)
                        Widths.RemoveAt(0);
                    return false;
                }
            }
            else
            {
                foreach (Face_Sprite face in Faces.Values)
                    face.stop();
            }
            // Moves speaker to the front // if we're doing that //Debug
            if (Config.MOVE_SPEAKER_TO_FRONT && Faces.ContainsKey(id))
            {
                Face_Zs.Remove(new Tuple<bool, int>(true, id));
                Face_Zs.Add(new Tuple<bool, int>(true, id));
            }
            Speaker = id;
            reset_pitch();
            if (valid_speaker(Speaker))
            {
                var pitch = this.current_speaker_default_pitch;
                if (pitch.IsSomething)
                {
                    this.pitch = pitch;
                }
#if DEBUG
                else if (false)//current_speaker_name != "???") //Debug
                    throw new ArgumentException(string.Format("Hey a speaker \"{0}\"doesn't have a preset pitch idiot.", current_speaker_name));
#endif
            }
            if (!Text[0].Any())
                Text.RemoveAt(0);
            if (Phase == 0)
            {
                Widths.RemoveAt(0);
                new_text_box();
            }
            else
            {
                if (Phase == 2)
                    Phase = 3;
            }
            if (Text_Speed != Constants.Message_Speeds.Max)
                refresh();
            set_backlog_speaker(id);
            return false;
        }

        private static void get_face_name(string raw_name, out string filename, out string recolor_country)
        {
            recolor_country = "";
            switch (raw_name)
            {
                // Visitor: get the unit that called the active event
                case "Visitor":
#if DEBUG
                    if (Global.game_state.event_caller_unit == null)
                        filename = "";
                    else
                    {
#endif
                        actor_face_name(Global.game_state.event_caller_unit.actor,
                            out filename, out recolor_country);
#if DEBUG
                    }
#endif
                    break;
                // Selected: get the unit the player has currently selected
                case "Selected":
#if DEBUG
                    if (Global.game_system.Selected_Unit_Id == -1)
                        filename = "";
                    else
                    {
#endif
                        actor_face_name(Global.game_map.units[Global.game_system.Selected_Unit_Id].actor,
                            out filename, out recolor_country);
#if DEBUG
                    }
#endif
                    break;
                // Enemy: get the current enemy unit, for example the opponent that killed the PC doing a death quote
                case "Enemy":
                    Game_Unit enemy = Global.game_state.enemy_of_dying_unit();
                    if (enemy == null)
                    {
                        if (true) // Need a default enemy to show during death quotes or whatever (especially death quotes outside combat eg. poison ) //Yeti
                        {
                            filename = Face_Sprite_Data.DEFAULT_BATTLE_DEATH_ENEMY;
                            recolor_country = Face_Sprite_Data.DEFAULT_BATTLE_DEATH_ENEMY_COLOR;
                        }
                        else
                            filename = "";
                    }
                    else
                    {
                        actor_face_name(enemy.actor, out filename, out recolor_country);
                    }
                    break;
                default:
                    string actor_pattern = "Actor([0-9]+)";
                    Regex actor_regex = new Regex(actor_pattern);
                    Match actor_match = actor_regex.Match(raw_name);
                    if (actor_match.Success)
                    {
                        int actor_id = int.Parse(actor_match.Groups[1].Value);
                        actor_face_name(Global.game_actors[actor_id], out filename, out recolor_country);
                    }
                    else
                    {
                        filename = raw_name;
                    }
                    break;
            }
        }

        private static void actor_face_name(Game_Actor actor, out string filename, out string recolorCountry)
        {
            recolorCountry = "";

            filename = actor.face_name;
            if (actor.generic_face)
                recolorCountry = actor.name_full;
        }

        private static string FixGenericName(string baseName)
        {
            // Check name as is
            if (Global.content_exists(@"Graphics/Faces/" + baseName))
                return baseName;

            // If a generic, try other builds
            string[] nameAry = baseName.Split(FEXNA.Constants.Actor.BUILD_NAME_DELIMITER);
            int baseBuild;
            if (nameAry.Length == 2 && int.TryParse(nameAry[1], out baseBuild))
            {
                string className = nameAry[0];
                // Check down first
                for (int build = baseBuild; build >= 0; build--)
                {
                    string name = string.Format("{0}{1}{2}",
                        className,
                        FEXNA.Constants.Actor.BUILD_NAME_DELIMITER,
                        build);
                    if (Global.content_exists(@"Graphics/Faces/" + name))
                        return name;
                }
                // Then check up
                int builds = Enum_Values.GetEnumCount(typeof(FEXNA_Library.Generic_Builds));
                for (int build = baseBuild + 1; build < builds; build++)
                {
                    string name = string.Format("{0}{1}{2}",
                        className,
                        FEXNA.Constants.Actor.BUILD_NAME_DELIMITER,
                        build);
                    if (Global.content_exists(@"Graphics/Faces/" + name))
                        return name;
                }
            }

            return baseName;
        }

        protected void remove_consumed_text_command()
        {
            if (Phase == 0)
                Text[0] = Text[0].Remove(0, 2);
        }

        protected void set_backlog_speaker(int id)
        {
            if (valid_speaker(id))
            {
                Backlog.new_speaker_line();
                Backlog.set_color("Yellow");
                Backlog.add_text(Names[id]);
            }
            Backlog.new_line();
            Backlog.set_color(Backlog_Default_Color);
        }

        protected virtual void add_backlog(char c)
        {
            switch (c)
            {
                case '\n':
                    Backlog.new_line();
                    break;
                default:
                    Backlog.add_text(c);
                    break;
            }
        }

        private static bool valid_speaker(int id)
        {
            return id != NO_SPEAKER && (id == CENTER_TOP_SPEAKER || id == CG_VOICEOVER_SPEAKER ||
                (id >= 0 && id <= Face_Sprite_Data.FACE_COUNT + 1));
        }
        private bool valid_face_speaker()
        {
            return valid_face_speaker(Speaker);
        }
        private static bool valid_face_speaker(int id)
        {
            //return id > 0 && id <= Face_Sprite_Data.FACE_COUNT; //Debug
            return id >= 0 && id <= Face_Sprite_Data.FACE_COUNT + 1;
        }
        private static bool valid_onscreen_speaker(int id)
        {
            return id > 0 && id <= Face_Sprite_Data.FACE_COUNT;
        }
        private static bool valid_face_or_offscreen_speaker(int id)
        {
            return id >= 0 && id <= Face_Sprite_Data.FACE_COUNT + 1;
        }
        private static bool valid_offscreen_speaker(int id)
        {
            return id == 0 || id == Face_Sprite_Data.FACE_COUNT + 1;
        }

        protected virtual void play_talk_sound()
        {
            Global.game_system.play_se(System_Sounds.Talk_Boop, pitch: Pitch);
        }

        protected virtual void speaker_talk()
        {
            if (valid_face_speaker() && Faces.ContainsKey(Speaker) && Text_Speed != Constants.Message_Speeds.Max)
                Faces[Speaker].talk();
        }

        protected virtual void create_arrow()
        {
            Arrow = new Message_Arrow();
            int x = (int)(Window_Img.loc.X + Text_Loc.X + ARROW_WIDTH);
            float y = this.loc.Y + Text_Loc.Y * Window_Message.FontData.CharHeight + 8;
            y += text_bubble_y_offset();
            Arrow.loc = new Vector2(x, (int)y);
        }

        protected virtual float text_bubble_y_offset()
        {
            if (Speaker == CG_VOICEOVER_SPEAKER)
                return Text_Window.CG_OFFSET;
            else if (Speaker == CENTER_TOP_SPEAKER)
                return Text_Window.BACKGROUND_OFFSET;
            else if (valid_offscreen_speaker(Speaker))
                return Text_Window.OFFSCREEN_OFFSET;
            else
                return 0;
        }

        protected virtual bool is_arrow_cleared()
        {
            if (Arrow == null)
                return true;
            return Arrow.cleared;
        }

        protected int id_fix(int id)
        {
            if (!Reverse || id < 0 || id > (Face_Sprite_Data.FACE_COUNT + 1))
                return id;
            return Face_Sprite_Data.FACE_COUNT + 1 - id;
        }
        #endregion

        #region Face Update/Cleaing
        protected virtual Vector2 face_location(int id)
        {
            int x;
            // Offscreen left
            if (id <= 0)
                x = -(int)(Face_Sprite_Data.BATTLE_FACE_SIZE.X) / 2;
            // Offscreen right
            else if (id >= Face_Sprite_Data.FACE_COUNT + 1)
                x = Config.WINDOW_WIDTH + (int)(Face_Sprite_Data.BATTLE_FACE_SIZE.X) / 2;
            // Left half
            else if (id <= Face_Sprite_Data.FACE_COUNT / 2)
                x = Face_Sprite_Data.BASE_X + ((id - 1) * Face_Sprite_Data.SPACING);
            // Right half
            else
                x = (Config.WINDOW_WIDTH - Face_Sprite_Data.BASE_X) - ((Face_Sprite_Data.FACE_COUNT - id) * Face_Sprite_Data.SPACING);

            return new Vector2(x, Config.WINDOW_HEIGHT);
        }

        protected void update_faces()
        {
            foreach (KeyValuePair<int, Face_Sprite> pair in Faces)
                pair.Value.update();
            update_clearing_faces();
        }

        protected void update_clearing_faces()
        {
            int i = 0;
            while (i < Clearing_Faces.Count)
            {
                if (Clearing_Faces[i].phased_out)
                {
                    Clearing_Faces.RemoveAt(i);
                    int j = 0;
                    while (j < Face_Zs.Count)
                    {
                        if (!Face_Zs[j].Item1)
                        {
                            if (Face_Zs[j].Item2 == i)
                                Face_Zs.RemoveAt(j);
                            else if (Face_Zs[j].Item2 > i)
                                Face_Zs[j] = new Tuple<bool, int>(false, Face_Zs[j].Item2 - 1);
                            else
                                j++;
                        }
                        else
                            j++;
                    }
                }
                else
                {
                    Clearing_Faces[i].update();
                    i++;
                }
                if (Clearing_Faces.Count == 0 && Text == null)
                    reset_pitch();
            }
        }

        protected void remove_face(int i)
        {
            Faces[i].remove();
            Clearing_Faces.Add(Faces[i]);
            Faces.Remove(i);
            int face_id = Face_Zs.IndexOf(new Tuple<bool, int>(true, i));
            Face_Zs.Remove(new Tuple<bool, int>(true, i));
            Face_Zs.Insert(face_id, new Tuple<bool, int>(false, Clearing_Faces.Count - 1));
        }

        protected void clear_faces()
        {
            Clearing_Faces.Clear();
            int i = 0;
            while (i < Face_Zs.Count)
            {
                if (!Face_Zs[i].Item1)
                    Face_Zs.RemoveAt(i);
                else
                    i++;
            }
            reset_pitch();
        }
        #endregion

        #region Update
        public virtual void update()
        {
            if (Waiting_For_Event)
            {
                if (update_images()) return;
                update_faces();
            }
            else if (Active)
            {
                if (update_images()) return;
                // If there is no text or not waiting on an arrow
                if (Text == null || !Wait)
                {
                    foreach (Face_Sprite face in Faces.Values)
                    {
                        if (face.moving)
                            Wait_Timer = Math.Max(1, Wait_Timer);
                    }
                    if (Wait_Timer > 0)
                    {
                        Text_Shown = false;
                        Wait_Timer--;
                        if (Phase == 0 || (Wait_Timer != 0 &&
                            !(Global.Input.triggered(Inputs.B, false) ||
                            Global.Input.triggered(Inputs.Start, false))))
                        {
                            update_faces();
                            return;
                        }
                    }
                    else if (Phase != 2 || Text_Speed != Constants.Message_Speeds.Max)
                        refresh();
                }
                else if (valid_face_speaker() && Faces.ContainsKey(Speaker))
                    Faces[Speaker].stop();

                Backlog.update();
                if (Backlog.ready)
                {
                    if (Backlog.Active)
                    {
                        // Close backlog
                        if (Global.Input.triggered(Inputs.A) ||
                            Global.Input.triggered(Inputs.B) ||
                            Global.Input.triggered(Inputs.R) ||
                            Global.Input.mouse_click(MouseButtons.Right) ||
                            Global.Input.gesture_triggered(TouchGestures.LongPress))
                        {
                            Backlog.fade_out();
                        }
                        else if(Global.Input.gesture_triggered(TouchGestures.SwipeLeft))
                        {
                            Backlog.fade_out(true);
                        }
                        update_faces();
                    }
                    else
                    {
                        if (!input_update())
                            update_faces();
                    }
                }
            }
            else
                update_clearing_faces();
        }

        protected virtual bool input_update()
        {
            // Skip
            if (Phase > 0)
            {
                if (_convoSkip == TextSkips.None)
                    _convoSkip = text_skip_input(); 

                if (_convoSkip != TextSkips.None)
                {
                    skip_text(_convoSkip);
                    _convoSkip = TextSkips.None;
                    return true;
                }
            }
            _convoSkip = TextSkips.None;

            // Bring up backlog
            if (Wait)
            {
                if (Global.Input.triggered(Inputs.R) ||
                    Global.Input.mouse_click(MouseButtons.Right) ||
                    Global.Input.gesture_triggered(TouchGestures.LongPress))
                {
                    activate_backlog();
                    return false;
                }
                else if (Global.Input.gesture_triggered(TouchGestures.SwipeRight))
                {
                    activate_backlog(true);
                    return false;
                }
            }
            if (Phase == 2)
            {
                // Max text speed
                if (Text_Speed == Constants.Message_Speeds.Max && !Wait && Text.Any())
                {
                    Skipping = true;
                    for (; ; )
                    {
                        bool result = refresh();
                        if (Was_Waiting && result && !Wait)
                            Was_Waiting = false;
                        if (!(result && !Wait && (Text == null || Text_Loc.Y != Message_Lines)))
                            break;
                    }
                    // Plays boops at the end of a text spam
                    if (Text_Shown && Speaker != NO_SPEAKER && Phase != 3 && !Full_Scroll &&
                            (Wait || (Scroll ? !Was_Waiting && ((Timer + 1) % SCROLL_TIME == 0) : is_arrow_cleared())))
                        play_talk_sound();
                    Skipping = false;
                    return false;
                }
                // Text has ended
                if (Text_End && !Wait)
                {
                    end_text();
                    return true;
                }

                // Text appearing onscreen
                if ((Text == null || !Wait) && !Scroll && Speaker != NO_SPEAKER)
                    // Jump to the end of the current box
                    if (end_wait_input())
                    {
                        Wait_Timer = 0;
                        Skipping = true;
                        while (refresh() && (Text == null || Text_Loc.Y != Message_Lines)) { }
                        Skipping = false;
                        return false;
                    }

                if (end_wait_input() || Global.Input.pressed(Inputs.Down))
                {
                    if (Wait && !Closing)
                    {
                        // Done waiting at an arrow
                        Wait = false;
                        Arrow.clear();
                        Was_Waiting = true;
                        Text_Shown = false;
                        if (Global.Input.pressed(Inputs.Down))
                            Wait_Timer = 20;
                    }
                    // This made waits one frame shorter on button presses?????
                    //else if (Wait_Timer > 0) //Debug
                    //    Wait_Timer = Math.Max(Wait_Timer - 1, 0);
                    return false;
                }
            }
            return false;
        }

        protected virtual TextSkips text_skip_input()
        {
            if (Global.Input.triggered(Inputs.B))
                return TextSkips.NextScene;
            if (Global.Input.triggered(Inputs.Start))
                return TextSkips.SkipEvent;
            return TextSkips.None;
        }

        protected virtual void skip_text(TextSkips skip)
        {
            Event_Skip = skip == TextSkips.SkipEvent;
            clear_text_box();
            if (Event_Skip)
                while (!Window_Img.ready)
                    Window_Img.update();
            begin_terminate_message();
        }

        protected virtual bool end_wait_input()
        {
            return Global.Input.triggered(Inputs.A) ||
                Global.Input.mouse_click(MouseButtons.Left) ||
                Global.Input.gesture_triggered(TouchGestures.Tap);
        }

        protected virtual void end_text()
        {
            if (!Closing)
                clear_text_box();
            begin_terminate_message();
        }

        protected void activate_backlog(bool swipeIn = false)
        {
            Backlog.fade_in(swipeIn);
            update_faces();
        }

        protected virtual bool update_images()
        {
            if (Window_Img != null)
                Window_Img.update();
            if (Location != null)
            {
                Location.update();
                if (Location.is_finished)
                    Location = null;
            }
            if (Background != null)
            {
                if (Background_Name != Temp_Background_Name)
                {
                    Background.filename = Background_Name;
                    Temp_Background_Name = Background_Name;
                }
                Background.update();
                if (Closing)
                {
                    begin_terminate_message();
                    update_clearing_faces();
                }
                if (Background == null)
                    return true;
                if (!Background.ready)
                    return true;
            }
            if (Arrow != null) Arrow.update();
            return false;
        }
        #endregion

        #region Draw
        public void draw_background(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (Background != null)
                Background.draw(sprite_batch);
            sprite_batch.End();
        }

        public void draw_faces(SpriteBatch sprite_batch)
        {
            foreach (Tuple<bool, int> pair in Face_Zs)
                if (pair.Item1)
                    Faces[pair.Item2].draw(sprite_batch);
                else
                    Clearing_Faces[pair.Item2].draw(sprite_batch);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (Background != null && !background_transition_over_text())
                Background.draw_black(sprite_batch);
            sprite_batch.End();
        }

        protected virtual bool background_transition_over_text()
        {
            return true;
        }

        public virtual void draw_foreground(SpriteBatch sprite_batch)
        {
            if (Window_Img != null)
                Window_Img.draw(sprite_batch, -draw_vector());
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (Arrow != null)
                Arrow.draw(sprite_batch, -draw_vector());
            if (Location != null)
                Location.draw(sprite_batch);
            sprite_batch.End();

            Backlog.draw(sprite_batch);

            if (Background != null && background_transition_over_text())
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Background.draw_black(sprite_batch);
                sprite_batch.End();
            }
        }
        #endregion
    }
}
