using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FEXNA.Graphics.Text;
using FEXNA.Options;
using FEXNA.Windows.UserInterface.Command;
using FEXNA.Windows.UserInterface.Command.Config;

namespace FEXNA.Windows.Options
{
    class SettingsWindow : Command.Window_Command_Scrollbar
    {
        const int VALUE_OFFSET = 120;
        const int ROWS = 9;

        private ISettings Settings;
        private bool SettingSelected;
        private ISettings TempSelectedSettings, TempOriginalSettings;

        private Hand_Cursor SelectedSettingCursor;

        public SettingsWindow(Vector2 loc, int width, ISettings settings)
            : base()
        {
            Settings = settings;
            Rows = ROWS;

            List<string> strs = new List<string>();
            foreach (string label in Settings.SettingLabels)
                strs.Add(label);

            loc.Y = Math.Min(loc.Y, (Config.WINDOW_HEIGHT - 16) - (ROWS + 1) * 16);

            SelectedSettingCursor = new Hand_Cursor();
            SelectedSettingCursor.draw_offset = new Vector2(-16, 0);
            
            initialize(loc, width, strs);
        }

        protected override void set_items(List<string> strs)
        {
            base.set_items(strs);

            RefreshItemValues();
        }

        protected override void initialize_window() { }

        private void RefreshItemValues()
        {
            for (int i = 0; i < Settings.SettingLabels.Count(); i++)
            {
                RefreshItemValue(Settings, i);
            }
        }
        private void RefreshItemValue(ISettings settings, int index)
        {
            var type = settings.SettingType(index);
            switch (type)
            {
                case ConfigTypes.OnOffSwitch:
                    (Items[index] as ConfigTextUINode).set_text(settings.ValueString(index));
                    break;
                case ConfigTypes.Number:
                    (Items[index] as NumberUINode).set_text(settings.ValueString(index));
                    break;
                case ConfigTypes.Slider:
                    (Items[index] as SliderUINode).set_value(settings.Value<int>(index), settings.ValueString(index));
                    break;
                case ConfigTypes.Button:
                    (Items[index] as ButtonUINode).set_description(settings.Value<String>(index));
                    break;
                case ConfigTypes.Input:
                    (Items[index] as KeyRemapUINode).set_key(settings.Value<Keys>(index));
                    break;
            }

            (Items[index] as ConfigUINode).locked = !settings.IsSettingEnabled(index);
        }

        private void RefreshCurrentValue(ISettings settings)
        {
            RefreshItemValue(settings, this.index);
            foreach (int dependent in Settings.DependentSettings(this.index))
                RefreshItemValue(settings, dependent);
        }

        protected override void add_commands(List<string> strs)
        {
            var nodes = new List<CommandUINode>();
            for (int i = 0; i < strs.Count; i++)
            {
                var type = Settings.SettingType(i);
                var text_node = item(Tuple.Create(strs[i], type), i);
                nodes.Add(text_node);
            }

            set_nodes(nodes);
        }

        protected override CommandUINode item(object value, int index)
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
                case ConfigTypes.Slider:
                    var range = Settings.ValueRange(index);
                    int min = range.Minimum;
                    int max = range.Maximum;
                    node = new SliderUINode("", str, this.column_width, min, max, 48);
                    break;
                case ConfigTypes.OnOffSwitch:
                    node = new ConfigTextUINode("", str, this.column_width);
                    break;
                case ConfigTypes.Button:
                    node = new ButtonUINode("", str, "", this.column_width);
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
                    if (str.Split('\n').Length == 1)
                        label = str.Split('\n')[0];
                    else
                        label = str.Split('\n')[1];

                    node = new KeyRemapUINode("", input, label, this.column_width);
                    break;
            }
            node.loc = item_loc(index);
            return node;
        }

        public bool IsSettingEnabled()
        {
            return Settings.IsSettingEnabled(this.index);
        }

        public bool SelectSetting(bool selected)
        {
            switch (Settings.SettingType(this.index))
            {
                case ConfigTypes.Button:
                    // Simply execute the button's operation
                    if (selected)
                    {
                        Settings.ConfirmSetting(this.index, null);
                        RefreshItemValues();
                        SelectedSettingCursor.force_loc(UICursor.loc);
                        selected = false;
                    }
                    break;
                case ConfigTypes.Input:
                    break;
                default:
                    Items[this.index].set_text_color(selected ? "Green" : "White");
                    break;
            }
            SettingSelected = selected;
            Greyed_Cursor = SettingSelected;

            if (SettingSelected)
            {
                TempSelectedSettings = (ISettings)Settings.Clone();
                TempOriginalSettings = (ISettings)Settings.Clone();
                SelectedSettingCursor.force_loc(UICursor.loc);
                SelectedSettingCursor.set_loc(UICursor.target_loc + new Vector2(VALUE_OFFSET, 0));
                SelectedSettingCursor.update();
            }
            else
            {
                TempSelectedSettings = null;
                TempOriginalSettings = null;
                RefreshCurrentValue(Settings);
                UICursor.force_loc(SelectedSettingCursor.loc);
                UICursor.update();
            }
            active = !SettingSelected;

            return SettingSelected;
        }

        public void CancelSetting()
        {
            if (TempOriginalSettings != null)
            {
                Settings.CopySettingsFrom(TempOriginalSettings);
            }

            SelectSetting(false);
        }

        public void ConfirmSetting()
        {
            if (TempSelectedSettings != null)
            {
                ConfirmTempSetting(this.index);

                SelectSetting(false);

                RefreshItemValues();
            }
        }

        private void ConfirmTempSetting(int index)
        {
            Settings.ConfirmSetting(index, TempSelectedSettings.ValueObject(index));
        }

        public bool RemapInput(Keys key)
        {
            switch (TempSelectedSettings.SettingType(this.index))
            {
                case ConfigTypes.Input:
                    TempSelectedSettings.ConfirmSetting(this.index, key);
                    RefreshCurrentValue(TempSelectedSettings);
                    return true;
            }
            
            return false;
        }

        public bool InputRemapSetting
        {
            get
            {
                switch (Settings.SettingType(this.index))
                {
                    case ConfigTypes.Input:
                        return true;
                }
                return false;
            }
        }

        protected override void update_movement(bool input)
        {
            base.update_movement(input);
            SelectedSettingCursor.update();

            bool right = Global.Input.repeated(Inputs.Right);
            bool left = Global.Input.repeated(Inputs.Left);

            bool valueChanged = false;
            if (SettingSelected && (right || left))
            {
                var settings = TempSelectedSettings;
                switch (settings.SettingType(this.index))
                {
                    case ConfigTypes.Number:
                    case ConfigTypes.Slider:
                        int value = settings.Value<int>(index);
                        settings.ConfirmSetting(
                            this.index,
                            value + settings.ValueInterval(this.index) * (right ? 1 : -1));
                        if (value != settings.Value<int>(index))
                        {
                            valueChanged = true;
                            // Menu move sound
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                        }
                        break;
                    case ConfigTypes.OnOffSwitch:
                        bool flag = settings.Value<bool>(index);
                        if (!flag == right)
                        {
                            settings.ConfirmSetting(this.index, !flag);
                            valueChanged = true;
                            // Menu move sound
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                        }
                        break;
                    default:
                        break;
                }
                RefreshCurrentValue(settings);

                // Copy to settings object if updating before confirm
                if (settings.SettingUpdatesBeforeConfirm(this.index))
                    ConfirmTempSetting(this.index);

                /* //@Yet: Cause a short rumble if rumble is turned on
                if (valueChanged &&
                    settings.SettingType(this.index) == ConfigTypes.OnOffSwitch &&
                    settings.Value<bool>(index))
                {
                    Global.Rumble.add_rumble(TimeSpan.FromSeconds(0.2f), 0.8f, 0.8f);
                }*/
            }
        }
        
        public override void draw_cursor(SpriteBatch sprite_batch)
        {
            if (Input.ControlScheme != ControlSchemes.Mouse)
            {
                if (Greyed_Cursor)
                    Grey_Cursor.draw(sprite_batch, -(loc + text_draw_vector()));
            }

            if (!SettingSelected && active && Items.ActiveNode != null)
                UICursor.draw(sprite_batch, -(loc + text_draw_vector()));
            
            if (Input.ControlScheme != ControlSchemes.Mouse)
            {
                if (SettingSelected)
                    SelectedSettingCursor.draw(sprite_batch, -(loc + text_draw_vector()));
            }
        }
    }
}
