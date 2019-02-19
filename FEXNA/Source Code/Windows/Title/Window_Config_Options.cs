using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FEXNA.Graphics.Help;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface.Command;
using FEXNA.Windows.UserInterface.Command.Config;

namespace FEXNA.Windows.Command
{
    enum Config_Options { Zoom, Fullscreen, Stereoscopic, Anaglyph, Metrics, Check_For_Updates, Rumble, ResetControls, Controls }
    enum ConfigTypes { None, Number, OnOffSwitch, Button, Input }
    class Window_Config_Options : Window_Command_Scrollbar
    {
        const int VALUE_OFFSET = 120;
        const int CONTROLS = 12;
        const int ROWS_AT_ONCE = 9;

        private bool OptionSelected;
        internal int NonControlOptions { get; private set; }
        private Hand_Cursor SelectedOptionCursor;
        private Dictionary<int, Config_Options> OptionIndices;

        internal int Zoom { get; private set; }
        internal int StereoscopicLevel { get; private set; }
        internal bool Fullscreen { get; private set; }
        internal bool Anaglyph { get; private set; }
        internal bool Metrics { get; private set; }
        internal bool Updates { get; private set; }
        internal bool Rumble { get; private set; }

        #region Accessors
        private Config_Options ActiveOption { get { return OptionIndices[this.index]; } }

        public bool is_option_enabled
        {
            get
            {
                if (this.ActiveOption == Config_Options.Anaglyph)
                    return Fullscreen && (StereoscopicLevel > 0);
                return true;
            }
        }
        #endregion

        public Window_Config_Options()
        {
            Rows = ROWS_AT_ONCE;

            List<string> strs = new List<string>();
            foreach (Config_Options option in Enum_Values.GetEnumValues(typeof(Config_Options)))
            {
                switch (option)
                {
                    case Config_Options.Zoom:
                        strs.Add("Zoom");
                        break;
                    case Config_Options.Fullscreen:
                        strs.Add("Fullscreen");
                        break;
                    case Config_Options.Stereoscopic:
                        strs.Add("Stereoscopic 3D");
                        break;
                    case Config_Options.Anaglyph:
                        strs.Add("  Red-Cyan (3D)");
                        break;
                    case Config_Options.Metrics:
                        strs.Add("Metrics");
                        break;
                    case Config_Options.Check_For_Updates:
                        strs.Add("Check for Updates");
                        break;
                    case Config_Options.Rumble:
                        strs.Add("Rumble");
                        break;
                    case Config_Options.ResetControls:
                        strs.Add("Controls:");
                        break;
                    case Config_Options.Controls:
                        break;
                    default:
#if DEBUG
                        throw new IndexOutOfRangeException(string.Format("There is no description text for the option\n\"{0}\" in Window_Config_Options.cs", option));
#endif
                        strs.Add("");
                        break;
                }
            }
            strs.AddRange(new List<string> { "Down", "Left", "Right", "Up",
                "A\nSelect/Confirm", "B\nCancel", "Y\nCursor Speed", "X\nEnemy Range",
                "L\nNext Unit", "R\nStatus", "Start\nSkip/Map", "Select\nMenu" });
            initialize(new Vector2(64, 16), 224, strs);
        }

        protected override void initialize(Vector2 loc, int width, List<string> strs)
        {
            this.loc = loc;
            Width = width;

            Grey_Cursor = new Hand_Cursor();
            Grey_Cursor.tint = new Color(192, 192, 192, 255);
            Grey_Cursor.draw_offset = new Vector2(-16, 0);

            SelectedOptionCursor = new Hand_Cursor();
            SelectedOptionCursor.draw_offset = new Vector2(-16, 0);

            set_items(strs);
            update();
        }

        protected override void set_items(List<string> strs)
        {
            if (strs == null)
                return;

            base.set_items(strs);
            reset_all();
        }

        protected override void add_commands(List<string> strs)
        {
            NonControlOptions = 0;
            OptionIndices = new Dictionary<int, Config_Options>();

            var nodes = new List<CommandUINode>();
            int options = Enum_Values.GetEnumCount(typeof(Config_Options));
            for (int i = 0; i < strs.Count; i++)
            {
                ConfigTypes value = ConfigTypes.None;
                Config_Options option;
                if (i < options)
                {
                    option = (Config_Options)i;
                    switch (option)
                    {
                        case Config_Options.Zoom:
                            value = ConfigTypes.Number;
                            break;
                        case Config_Options.Metrics:
                            if (Global.metrics_allowed)
                                value = ConfigTypes.OnOffSwitch;
                            break;
                        case Config_Options.ResetControls:
                            value = ConfigTypes.Button;
                            break;
                        case Config_Options.Controls:
                            value = ConfigTypes.Input;
                            break;
                        case Config_Options.Fullscreen:
                        case Config_Options.Stereoscopic:
                        case Config_Options.Anaglyph:
                        case Config_Options.Check_For_Updates:
                        case Config_Options.Rumble:
                        default:
                            value = ConfigTypes.OnOffSwitch;
                            break;
                    }

                    if (value == ConfigTypes.None)
                        continue;

                    if (option != Config_Options.ResetControls &&
                            option != Config_Options.Controls)
                        NonControlOptions++;
                }
                else
                {
                    // Controls
                    value = ConfigTypes.Input;
                    option = Config_Options.Controls;
                }

                OptionIndices.Add(nodes.Count, option);
                var node = item(new Tuple<string, ConfigTypes>(strs[i], value), nodes.Count);
                nodes.Add(node);
            }

            set_nodes(nodes);
        }

        protected override CommandUINode item(object value, int i)
        {
            var str = (value as Tuple<string, ConfigTypes>).Item1;
            var type = (value as Tuple<string, ConfigTypes>).Item2;

            var text = new FE_Text();
            text.Font = "FE7_Text";
            text.texture = Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_White");
            text.text = str;

            CommandUINode node;
            switch (type)
            {
                case ConfigTypes.Number:
                    node = new NumberUINode("", str, this.column_width);
                    break;
                case ConfigTypes.OnOffSwitch:
                    node = new SwitchUINode("", str, this.column_width);
                    break;
                case ConfigTypes.Button:
                    string description = "";
                    if (i == NonControlOptions)
                        description = "Reset to Default";
                    node = new ButtonUINode("", str, description, this.column_width);
                    break;
                case ConfigTypes.Input:
                default:
                    Inputs input;
                    switch (str.Split('\n')[0])
                    {
                        case "A":
                            input = Inputs.A;
                            break;
                        case "B":
                            input = Inputs.B;
                            break;
                        case "Y":
                            input = Inputs.Y;
                            break;
                        case "X":
                            input = Inputs.X;
                            break;
                        case "L":
                            input = Inputs.L;
                            break;
                        case "R":
                            input = Inputs.R;
                            break;
                        case "Start":
                            input = Inputs.Start;
                            break;
                        case "Select":
                            input = Inputs.Select;
                            break;

                        case "Down":
                            input = Inputs.Down;
                            break;
                        case "Left":
                            input = Inputs.Left;
                            break;
                        case "Right":
                            input = Inputs.Right;
                            break;
                        case "Up":
                            input = Inputs.Up;
                            break;

                        default:
                            input = Inputs.A;
                            break;
                    }
                    string label;
                    if (i < NonControlOptions + 5)
                        label = str.Split('\n')[0];
                    else
                        label = str.Split('\n')[1];

                    node = new InputUINode("", input, label, this.column_width);
                    break;
            }
            node.loc = item_loc(i);
            return node;
        }

        #region Update
        protected override void update_input(bool input)
        {
            if (OptionSelected)
            {
                switch (this.ActiveOption)
                {
                    // Zoom
                    case Config_Options.Zoom:
                        if (Global.Input.triggered(Inputs.Right))
                        {
                            int zoom = (int)MathHelper.Clamp(Zoom + 1, Global.zoom_min, Global.zoom_max);
                            if (zoom != Zoom)
                            {
                                reset_zoom(zoom);
                                Global.game_system.play_se(System_Sounds.Menu_Move2);
                            }
                        }
                        else if (Global.Input.triggered(Inputs.Left))
                        {
                            int zoom = (int)MathHelper.Clamp(Zoom - 1, Global.zoom_min, Global.zoom_max);
                            if (zoom != Zoom)
                            {
                                reset_zoom(zoom);
                                Global.game_system.play_se(System_Sounds.Menu_Move2);
                            }
                        }
                        break;
                    // Fullscreen
                    case Config_Options.Fullscreen:
                        if (Global.Input.triggered(Inputs.Right) || Global.Input.triggered(Inputs.Left))
                        {
                            reset_fullscreen(!Fullscreen);
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                        }
                        break;
                    // Stereoscopic
                    case Config_Options.Stereoscopic:
                        if (Global.Input.repeated(Inputs.Right))
                        {
                            int stereoscopic = (int)MathHelper.Clamp(StereoscopicLevel + 1, 0, Global.MAX_STEREOSCOPIC_LEVEL);
                            if (stereoscopic != StereoscopicLevel)
                            {
                                reset_stereoscopic(stereoscopic);
                                Global.game_system.play_se(System_Sounds.Menu_Move2);
                            }
                        }
                        else if (Global.Input.repeated(Inputs.Left))
                        {
                            int stereoscopic = (int)MathHelper.Clamp(StereoscopicLevel - 1, 0, Global.MAX_STEREOSCOPIC_LEVEL);
                            if (stereoscopic != StereoscopicLevel)
                            {
                                reset_stereoscopic(stereoscopic);
                                Global.game_system.play_se(System_Sounds.Menu_Move2);
                            }
                        }
                        break;
                    // Anaglyph
                    case Config_Options.Anaglyph:
                        if (Global.Input.triggered(Inputs.Right) || Global.Input.triggered(Inputs.Left))
                        {
                            reset_anaglyph(!Anaglyph);
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                        }
                        break;
                    // Metrics
                    case Config_Options.Metrics:
                        if (Global.Input.triggered(Inputs.Right) || Global.Input.triggered(Inputs.Left))
                        {
                            reset_metrics(!Metrics);
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                        }
                        break;
                    // Check for Updates
                    case Config_Options.Check_For_Updates:
                        if (Global.Input.triggered(Inputs.Right) || Global.Input.triggered(Inputs.Left))
                        {
                            reset_updates(!Updates);
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                        }
                        break;
                    // Rumble
                    case Config_Options.Rumble:
                        if (Global.Input.triggered(Inputs.Right) || Global.Input.triggered(Inputs.Left))
                        {
                            reset_rumble(!Rumble);
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                        }
                        break;
                }
            }
        }

        protected override void update_ui(bool input)
        {
            base.update_ui(input);

            if (input)
            {
                // Select with Start or the Enter key
                if (!is_help_active && !is_selected())
                {
                    if (Global.Input.triggered(Inputs.Start) ||
                            Global.Input.KeyPressed(Keys.Enter))
                        SelectedIndex = this.index;
                }
            }
        }

        protected override void update_movement(bool input)
        {
            base.update_movement(input);
            SelectedOptionCursor.update();
        }

        public void select_option(bool selected)
        {
            OptionSelected = selected;
            Greyed_Cursor = OptionSelected;
            if (this.index < NonControlOptions + 1)
                Items[this.index].set_text_color(OptionSelected ? "Green" : "White");

            if (OptionSelected)
            {
                SelectedOptionCursor.force_loc(UICursor.loc);
                SelectedOptionCursor.set_loc(UICursor.target_loc + new Vector2(VALUE_OFFSET, 0));
                SelectedOptionCursor.update();
            }
            else
            {
                UICursor.force_loc(SelectedOptionCursor.loc);
                UICursor.update();
            }
            active = !OptionSelected;
        }
        #endregion

        #region Controls
        public void reset()
        {
            switch (this.ActiveOption)
            {
                // Zoom
                case Config_Options.Zoom:
                    reset_zoom();
                    break;
                // Fullscreen
                case Config_Options.Fullscreen:
                    reset_fullscreen();
                    break;
                // Stereoscopic
                case Config_Options.Stereoscopic:
                    reset_stereoscopic();
                    break;
                // Anaglyph
                case Config_Options.Anaglyph:
                    reset_anaglyph();
                    break;
                // Metrics
                case Config_Options.Metrics:
                    reset_metrics();
                    break;
                // Check for Updates
                case Config_Options.Check_For_Updates:
                    reset_updates();
                    break;
                // Rumble
                case Config_Options.Rumble:
                    reset_rumble();
                    break;
            }
        }
        private void reset_all()
        {
            reset_zoom();
            reset_fullscreen();
            reset_stereoscopic();
            reset_anaglyph();
            if (Global.metrics_allowed)
                reset_metrics();
            reset_updates();
            reset_rumble();
        }

        private ConfigUINode OptionUINode(Config_Options option)
        {
            return Items[OptionIndices.First(x => x.Value == option).Key] as ConfigUINode;
        }

        // Zoom
        private void reset_zoom()
        {
            reset_zoom(Global.zoom);
        }
        private void reset_zoom(int value)
        {
            Zoom = value;
            (OptionUINode(Config_Options.Zoom) as NumberUINode).set_value(Zoom);
        }

        // Fullscreen
        private void reset_fullscreen()
        {
            reset_fullscreen(Global.fullscreen);
        }
        private void reset_fullscreen(bool value)
        {
            Fullscreen = value;
            (OptionUINode(Config_Options.Fullscreen) as SwitchUINode).set_switch(Fullscreen);

            reset_anaglyph();
        }

        // Stereoscopic 3D
        private void reset_stereoscopic()
        {
            reset_stereoscopic(Global.stereoscopic_level);
        }
        private void reset_stereoscopic(int value)
        {
            StereoscopicLevel = value;
            bool stereoscopy = StereoscopicLevel > 0;
            (OptionUINode(Config_Options.Stereoscopic) as SwitchUINode).set_switch(
                stereoscopy, stereoscopy ? StereoscopicLevel.ToString() : "");

            reset_anaglyph();
        }

        // Anaglyph
        private void reset_anaglyph()
        {
            reset_anaglyph(Global.anaglyph);
        }
        private void reset_anaglyph(bool value)
        {
            if (!Fullscreen || StereoscopicLevel == 0)
            {
                (OptionUINode(Config_Options.Anaglyph) as SwitchUINode).locked = true;
            }
            else
            {
                (OptionUINode(Config_Options.Anaglyph) as SwitchUINode).locked = false;
            }

            Anaglyph = value;
            (OptionUINode(Config_Options.Anaglyph) as SwitchUINode).set_switch(
                StereoscopicLevel > 0 && (!Fullscreen || Anaglyph));
        }

        // Metrics
        private void reset_metrics()
        {
            reset_metrics(Global.metrics == Metrics_Settings.On);
        }
        private void reset_metrics(bool value)
        {
            Metrics = value;
            (OptionUINode(Config_Options.Metrics) as SwitchUINode).set_switch(Metrics);
        }

        // Check for Updates
        private void reset_updates()
        {
            reset_updates(Global.updates_active);
        }
        private void reset_updates(bool value)
        {
            Updates = value;
            (OptionUINode(Config_Options.Check_For_Updates) as SwitchUINode).set_switch(Updates);
        }

        // Rumble
        private void reset_rumble()
        {
            reset_rumble(Global.rumble);
        }
        private void reset_rumble(bool value)
        {
            Rumble = value;
            (OptionUINode(Config_Options.Rumble) as SwitchUINode).set_switch(Rumble);
        }
        #endregion

        public override void draw_cursor(SpriteBatch sprite_batch)
        {
            base.draw_cursor(sprite_batch);
            if (Input.ControlScheme != ControlSchemes.Mouse)
            {
                if (OptionSelected)
                    SelectedOptionCursor.draw(sprite_batch, -(loc + text_draw_vector()));
            }
        }
    }
}
