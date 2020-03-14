using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Options;
using OptionsConfig = FEXNA.Constants.OptionsConfig;

namespace FEXNA.Windows.Map
{
    class Window_Options : Map_Window_Base
    {
        #region Options
        #endregion

        const int ROWS_AT_ONCE = (Config.WINDOW_HEIGHT - 80) / 16;
        const int OPTIONS_OFFSET = 40;
        const int CHOICES_OFFSET = OPTIONS_OFFSET + 128;

        protected int Scroll = 0;
        protected Vector2 Offset = Vector2.Zero;
        protected bool SoloAnim_Allowed;
        protected bool Map_Info_Changed = false;
        protected FE_Banner Banner;
        protected Sprite Banner_Text;
        protected System_Color_Window Description_Window;
        protected FE_Text Description;
        protected SoloAnim_Button Solo_Icon;
        private Button_Description CancelButton;
        protected Page_Arrow Up_Page_Arrow, Down_Page_Arrow;
        protected Scroll_Bar Scrollbar;
        protected bool ManualScroll = false;
        protected Vector2 ManualScrollVel = Vector2.Zero;

        protected PartialRangeVisibleUINodeSet<OptionsUINode> OptionsNodes;
        protected UICursor<OptionsUINode> OptionsCursor;

        protected PartialRangeVisibleUINodeSet<SettingUINode> SettingsNodes;
        protected UICursor<SettingUINode> SettingsCursor;
        protected List<List<SettingUINode>> SettingsGroups;

        Rectangle Data_Scissor_Rect = new Rectangle(0, 40, Config.WINDOW_WIDTH, ROWS_AT_ONCE * 16);
        RasterizerState Scissor_State = new RasterizerState { ScissorTestEnable = true };

        #region Accessors
        private int active_option { get { return (int)OptionsNodes.ActiveNode.Option; } }
        private ConfigData.OptionsData active_option_data { get { return OptionsConfig.OPTIONS_DATA[this.active_option]; } }
        // This actually changes/gets the individual options
        protected byte current_setting
        {
            get
            {
                return Global.game_options.Data[this.active_option];
            }
            set { Global.game_options.Data[this.active_option] = value; }
        }

        protected int max_setting
        {
            get
            {
                var option = OptionsNodes.ActiveNode.Option;
                return this.active_option_data.Options.Length;
            }
        }

        protected bool on_soloanim
        {
            get
            {
                return OptionsNodes.ActiveNode.Option == Constants.Options.Animation_Mode &&
                    this.current_setting == (int)Constants.Animation_Modes.Solo;
            }
        }
        #endregion

        public Window_Options(bool soloAnimAllowed = true)
        {
            SoloAnim_Allowed = soloAnimAllowed;
            initialize_sprites();
            update_black_screen();
        }

        protected void initialize_sprites()
        {
            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 255);
            // Banner
            Banner = new FE_Banner();
            Banner.width = 120;
            Banner.loc = new Vector2(OPTIONS_OFFSET, 8);
            Banner.stereoscopic = Config.OPTIONS_BANNER_DEPTH;
            Banner_Text = new Sprite();
            Banner_Text.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Banner_Text");
            Banner_Text.src_rect = new Rectangle(0, 0, 96, 16);
            Banner_Text.loc = new Vector2(OPTIONS_OFFSET + 24 + 2, 8 + 8);
            Banner_Text.stereoscopic = Config.OPTIONS_BANNER_DEPTH;
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Option_Background");
            (Background as Menu_Background).vel = new Vector2(-0.25f, 0);
            (Background as Menu_Background).tile = new Vector2(3, 1);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
            // Description Window
            Description_Window = new System_Color_Window();
            Description_Window.loc = new Vector2(60, 156);
            Description_Window.width = 200;
            Description_Window.height = 24;
            Description_Window.small = true;
            Description_Window.stereoscopic = Config.OPTIONS_DESC_DEPTH;
            // Description
            Description = new FE_Text();
            Description.loc = new Vector2(72, 160);
            Description.Font = "FE7_Text";
            Description.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Description.stereoscopic = Config.OPTIONS_DESC_DEPTH;
            // Solo Anim Button Icon
            Solo_Icon = new SoloAnim_Button();
            Solo_Icon.loc = new Vector2(
                CHOICES_OFFSET + 20 +
                    OptionsConfig.OPTIONS_DATA[(int)Constants.Options.Animation_Mode]
                        .Options[(int)Constants.Animation_Modes.Solo].Offset,
                16 * (int)Constants.Options.Animation_Mode) +
                    new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);
            Solo_Icon.stereoscopic = Config.OPTIONS_OPTIONS_DEPTH;
            // Page Arrows
            Up_Page_Arrow = new Page_Arrow();
            Up_Page_Arrow.loc = new Vector2(CHOICES_OFFSET - 4, Data_Scissor_Rect.Y - 4);
            Up_Page_Arrow.angle = MathHelper.PiOver2;
            Up_Page_Arrow.stereoscopic = Config.OPTIONS_ARROWS_DEPTH;
            Down_Page_Arrow = new Page_Arrow();
            Down_Page_Arrow.loc = new Vector2(CHOICES_OFFSET - 4, Data_Scissor_Rect.Y + Data_Scissor_Rect.Height + 4);
            Down_Page_Arrow.mirrored = true;
            Down_Page_Arrow.angle = MathHelper.PiOver2;
            Down_Page_Arrow.stereoscopic = Config.OPTIONS_ARROWS_DEPTH;

            Up_Page_Arrow.ArrowClicked += Up_Page_Arrow_ArrowClicked;
            Down_Page_Arrow.ArrowClicked += Down_Page_Arrow_ArrowClicked;

            // UI Nodes
            List<OptionsUINode> nodes = new List<OptionsUINode>();
            // Options
            for (int index = 0; index < OptionsConfig.OPTIONS_DATA.Length; index++)
            {
                Vector2 loc = new Vector2(OPTIONS_OFFSET + 16, index * 16) +
                    new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);

                nodes.Add(new OptionsUINode(index));
                nodes[index].stereoscopic = Config.OPTIONS_OPTIONS_DEPTH;
                nodes[index].loc = loc;
            }

            OptionsNodes = new PartialRangeVisibleUINodeSet<OptionsUINode>(nodes);
            OptionsNodes.CursorMoveSound = System_Sounds.Menu_Move1;

            OptionsCursor = new UICursor<OptionsUINode>(OptionsNodes);
            OptionsCursor.draw_offset = new Vector2(-(16 + 12), 0);
            OptionsCursor.stereoscopic = Config.OPTIONS_CURSOR_DEPTH;
            // Settings
            List<SettingUINode> settings_nodes = new List<SettingUINode>();
            SettingsGroups = new List<List<SettingUINode>>();
            foreach (var option_node in OptionsNodes)
            {
                int i = (int)option_node.Option;
                var option = OptionsConfig.OPTIONS_DATA[i];
                var settings_group = new List<SettingUINode>();

                if (option.Gauge)
                {
                    Vector2 loc = new Vector2(CHOICES_OFFSET, i * 16) +
                        new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);

                    var node = new SettingGaugeUINode(option.Options[0].Name,
                        option.GaugeWidth, option.GaugeMin, option.GaugeMax, option.gauge_offset);
                    node.stereoscopic = Config.OPTIONS_OPTIONS_DEPTH;
                    node.loc = loc;
                    settings_nodes.Add(node);
                    settings_group.Add(node);
                }
                else
                {
                    for (int j = 0; j < option.Options.Length; j++)
                    {
                        Vector2 loc = new Vector2(CHOICES_OFFSET + option.Options[j].Offset, i * 16) +
                            new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);

                        var node = new SettingUINode(option.Options[j].Name);
                        node.stereoscopic = Config.OPTIONS_OPTIONS_DEPTH;
                        node.loc = loc;
                        settings_nodes.Add(node);
                        settings_group.Add(node);
                    }
                }

                SettingsGroups.Add(settings_group);
            }

            SettingsNodes = new PartialRangeVisibleUINodeSet<SettingUINode>(settings_nodes);
            SettingsNodes.CursorMoveSound = System_Sounds.Menu_Move2;
            SettingsNodes.set_active_node(SettingsNodes[this.current_setting]);

            SettingsCursor = new UICursor<SettingUINode>(SettingsNodes);
            SettingsCursor.draw_offset = new Vector2(-16, 0);
            SettingsCursor.min_distance_y = 4;
            SettingsCursor.override_distance_y = 16;
            SettingsCursor.stereoscopic = Config.OPTIONS_CURSOR_DEPTH;
            SettingsCursor.move_to_target_loc();

            // Scrollbar
            /* //Debug
            Scrollbar = new Scroll_Bar(
                ROWS_AT_ONCE * 16 - 16, OptionsConfig.OPTIONS_DATA.Length, ROWS_AT_ONCE, 0);
            Scrollbar.loc = new Vector2(Data_Scissor_Rect.Width - 12, Data_Scissor_Rect.Y + 8);

            Scrollbar.UpArrowClicked += Scrollbar_UpArrowClicked;
            Scrollbar.DownArrowClicked += Scrollbar_DownArrowClicked;*/

            create_cancel_button();

            refresh_arrow_visibility();
            update_loc();
        }

        private void create_cancel_button()
        {
            CancelButton = Button_Description.button(Inputs.B,
                Config.WINDOW_WIDTH - 48);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = Config.OPTIONS_DESC_DEPTH;
        }

        protected void refresh_arrow_visibility()
        {
            Up_Page_Arrow.visible = Scroll > 0;
            Down_Page_Arrow.visible = Scroll < OptionsConfig.OPTIONS_DATA.Length - (ROWS_AT_ONCE);
        }

        private void refresh_settings()
        {
            for(int i = 0; i < OptionsNodes.Count; i++)
            {
                var option = OptionsNodes[i].Option;
                if (Constants.OptionsConfig.OPTIONS_DATA[(int)option].Gauge)
                {
                    SettingsGroups[i][0].refresh_active(true);
                    (SettingsGroups[i][0] as SettingGaugeUINode).refresh_value(Global.game_options.Data[i]);
                }
                else
                {
                    byte setting = Global.game_options.Data[(int)option];
                    for (int j = 0; j < SettingsGroups[i].Count; j++)
                        SettingsGroups[i][j].refresh_active(setting == j);
                }
            }
        }

        protected void update_loc()
        {
            var option = OptionsNodes.ActiveNode.Option;
            int index = this.active_option_data.Gauge ? 0 : this.current_setting;
            Description.text = this.active_option_data.Options[index].Description;
        }

        private FEXNA_Library.IntRange visible_indexes_range()
        {
            int min = Math.Min(Scroll, OptionsNodes.Count - 1);
            int max = Math.Min(min + ROWS_AT_ONCE, OptionsNodes.Count);
            return new FEXNA_Library.IntRange(min, max - 1);
        }
        private IEnumerable<int> visible_settings_range()
        {
            return Enumerable.Range(0, SettingsNodes.Count)
                .Where(i =>
                    {
                        float y = SettingsNodes[i].loc.Y;
                        y = ((y - Data_Scissor_Rect.Y) / 16) - Scroll;
                        return y >= 0 && y < ROWS_AT_ONCE;
                    });
        }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            Up_Page_Arrow.update();
            Down_Page_Arrow.update();

            base.UpdateMenu(active);

            refresh_settings();
            if (Scrollbar != null)
            {
                Scrollbar.update();
                if (this.ready_for_inputs)
                    Scrollbar.update_input();
            }
            update_scroll_offset();
            update_cursor();

            Solo_Icon.update();
        }

        protected override void UpdateAncillary()
        {
            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    create_cancel_button();
            }
        }

        public event EventHandler<EventArgs> SoloAnim;

        protected void update_scroll_offset()
        {
            int target_y = 16 * Scroll;
            if (!ManualScroll)
            {
                if (Math.Abs(Offset.Y - target_y) <= 16 / 4)
                    Offset.Y = target_y;
                if (Math.Abs(Offset.Y - target_y) <= 16)
                    Offset.Y = Additional_Math.int_closer((int)Offset.Y, target_y, 16 / 4);
                else
                    Offset.Y = ((int)(Offset.Y + target_y)) / 2;
            }

            if (Offset.Y != target_y && Scrollbar != null)
            {
                if (Offset.Y > target_y)
                    Scrollbar.moving_up();
                else
                    Scrollbar.moving_down();
            }
        }

        private void update_cursor()
        {
            OptionsCursor.update(new Vector2(0, 16 * Scroll));
            SettingsCursor.update(new Vector2(0, 16 * Scroll));
        }

        protected override void update_input(bool active)
        {

            bool input = active && this.ready_for_inputs;

            update_node_location(input);
            if (CancelButton != null)
                CancelButton.Update(input);

            if (input)
            {
                if (Global.Input.mouseScroll > 0)
                {
                    Up_Page_Arrow_ArrowClicked(this, null);
                }
                else if (Global.Input.mouseScroll < 0)
                {
                    Down_Page_Arrow_ArrowClicked(this, null);
                }
                else
                {
                    Up_Page_Arrow.UpdateInput();
                    Down_Page_Arrow.UpdateInput();
                }

                var settings_index = SettingsNodes.consume_triggered(
                    MouseButtons.Left, TouchGestures.Tap);
                bool soloanim_node_clicked = false;
                if (settings_index.IsSomething)
                {
                    var node = SettingsNodes[settings_index];
                    jump_to_option(SettingsNodes.ActiveNode);

                    byte setting = setting_from_node(node);
                    if (!this.active_option_data.Gauge && setting != this.current_setting)
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        change_setting(OptionsNodes.ActiveNode.Option, setting);
                        return;
                    }

                    soloanim_node_clicked = this.active_option == (int)Constants.Options.Animation_Mode &&
                        SettingsGroups[OptionsNodes.ActiveNodeIndex].IndexOf(node) ==
                            (int)Constants.Animation_Modes.Solo;
                }
                var slider_index = SettingsNodes.consume_triggered(TouchGestures.Scrubbing);
                if (slider_index.IsSomething)
                {
                    var node = SettingsNodes[slider_index];
                    jump_to_option(SettingsNodes.ActiveNode);

                    byte setting = setting_from_node(node);
                    if (setting != this.current_setting)
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        ManualScroll = false;
                    }
                    change_setting(OptionsNodes.ActiveNode.Option, setting);
                    return;
                }

                if (Global.Input.repeated(Inputs.Left))
                {
                    if (can_move_left)
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        move_left();
                    }
                }
                else if (Global.Input.repeated(Inputs.Right))
                {
                    if (can_move_right)
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        move_right();
                    }
                }
                else if (Global.Input.triggered(Inputs.B) ||
                    CancelButton.consume_trigger(MouseButtons.Left) ||
                    CancelButton.consume_trigger(TouchGestures.Tap))
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    if (Map_Info_Changed && Global.scene.is_map_scene)
                        ((Scene_Map)Global.scene).create_info_windows();
                    close();
                }
                else if (this.on_soloanim && SoloAnim_Allowed &&
                    (Global.Input.triggered(Inputs.A) || soloanim_node_clicked))
                {
                    if (SoloAnim != null)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        SoloAnim(this, new EventArgs());
                    }
                }
            }
        }

        protected override void close()
        {
            base.close();
        }

        protected override void black_screen_switch()
        {
            Visible = !Visible;
            if (!_Closing)
            {
            }
        }
        #endregion

        #region Movement
        private bool can_move_left
        {
            get
            {
                if (this.active_option_data.Gauge)
                    return this.current_setting > this.active_option_data.GaugeMin;
                else
                    return this.current_setting > 0;
            }
        }
        private bool can_move_right
        {
            get
            {
                if (this.active_option_data.Gauge)
                    return this.current_setting < this.active_option_data.GaugeMax;
                else
                    return this.current_setting < this.max_setting - 1;
            }
        }

        protected void move_left()
        {
            byte setting;
            if (this.active_option_data.Gauge)
                setting = (byte)Math.Max(
                    this.current_setting - this.active_option_data.GaugeInterval,
                    this.active_option_data.GaugeMin);
            else
                setting = (byte)(this.current_setting - 1);
            change_setting((Constants.Options)this.active_option, setting);
        }
        protected void move_right()
        {
            byte setting;
            if (this.active_option_data.Gauge)
                setting = (byte)Math.Min(
                    this.current_setting + this.active_option_data.GaugeInterval,
                    this.active_option_data.GaugeMax);
            else
                setting = (byte)(this.current_setting + 1);
            change_setting((Constants.Options)this.active_option, setting);
        }

        private byte setting_from_node(SettingUINode node)
        {
            if (node is SettingGaugeUINode)
            {
                var option_data = this.active_option_data;
                int range = option_data.GaugeMax - option_data.GaugeMin;
                float value = (SettingsNodes.ActiveNode as SettingGaugeUINode).SliderValue;
                value = value * range;
                int setting = (int)Math.Round(value / option_data.GaugeInterval) * option_data.GaugeInterval;
                setting += option_data.GaugeMin;
                return (byte)MathHelper.Clamp(setting, option_data.GaugeMin, option_data.GaugeMax);
            }
            else
            {
                return (byte)SettingsGroups[OptionsNodes.ActiveNodeIndex].IndexOf(node);
            }
        }

        private void change_setting(Constants.Options option, byte setting)
        {
            Global.game_options.Data[(int)option] = setting;
            if ((int)option == this.active_option && !this.active_option_data.Gauge)
                SettingsNodes.set_active_node(SettingsGroups[OptionsNodes.ActiveNodeIndex][setting]);
            refresh_options(option);
            update_loc();
        }

        private void update_node_location(bool active)
        {
            // Update options nodes
            int old_index = OptionsNodes.ActiveNodeIndex;
            OptionsNodes.Update(active, visible_indexes_range().Enumerate(), Offset);
            if (old_index != OptionsNodes.ActiveNodeIndex)
            {
                if (Input.ControlScheme == ControlSchemes.Buttons)
                {
                    // Moved down
                    if (old_index < OptionsNodes.ActiveNodeIndex)
                        while (OptionsNodes.ActiveNodeIndex >= (ROWS_AT_ONCE - 1) + Scroll && Scroll <
                                OptionsNodes.Count - (ROWS_AT_ONCE))
                            Scroll++;
                    // Moved up
                    else
                        while (OptionsNodes.ActiveNodeIndex < Scroll + 1 && Scroll > 0)
                            Scroll--;
                }
                if (Scrollbar != null)
                    Scrollbar.scroll = Scroll;
                if (this.active_option_data.Gauge)
                    SettingsNodes.set_active_node(SettingsGroups[OptionsNodes.ActiveNodeIndex][0]);
                else
                {
                    SettingsNodes.set_active_node(SettingsGroups[OptionsNodes.ActiveNodeIndex][this.current_setting]);
                }
                refresh_arrow_visibility();
                update_loc();
            }

            // Update settings nodes
            old_index = SettingsNodes.ActiveNodeIndex;
            SettingsNodes.Update(!active ? ControlSet.None :
                (ControlSet.Mouse | ControlSet.Touch), visible_settings_range(), Offset);
            if (old_index != SettingsNodes.ActiveNodeIndex || Input.ControlSchemeSwitched)
            {
                jump_to_option(SettingsNodes.ActiveNode);
                update_loc();
                int setting = SettingsGroups[OptionsNodes.ActiveNodeIndex].IndexOf(SettingsNodes.ActiveNode);
                Description.text = this.active_option_data.Options[setting].Description;
            }

            // Update touch scroll
            update_manual_scroll(active);
        }

        private void update_manual_scroll(bool active)
        {
            if (active)
            {
                bool touch_pressed = Global.Input.touch_pressed(false);
                if (touch_pressed)
                {
                    Vector2 loc = new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);
                    if (Global.Input.touch_rectangle(
                        Services.Input.InputStates.Triggered,
                        new Rectangle((int)loc.X, (int)loc.Y,
                            Data_Scissor_Rect.Width, Data_Scissor_Rect.Height)))
                    {
                        ManualScroll = true;
                        Console.WriteLine("starting scroll");
                    }
                }

                if (ManualScroll)
                {
                    if (Global.Input.gesture_triggered(TouchGestures.VerticalDrag))
                    {
                        ManualScrollVel = -Global.Input.verticalDragVector;
                    }
                    else
                    {
                        ManualScrollVel *= 0.9f;
                        if (!touch_pressed && ManualScrollVel.LengthSquared() < 1f)
                        {
                            ManualScrollVel = Vector2.Zero;
                            ManualScroll = false;
                        }
                    }

                    Offset.Y += ManualScrollVel.Y;
                    Offset.Y = MathHelper.Clamp(Offset.Y,
                        0, (OptionsNodes.Count - ROWS_AT_ONCE) * 16);
                    Scroll = (int)Math.Round(Offset.Y / 16);
                }
            }
        }

        private void jump_to_option(UINode settingNode)
        {
            int option_index = SettingsGroups.FindIndex(x => x.Contains(settingNode));
            OptionsNodes.set_active_node(OptionsNodes[option_index]);
        }

        private void Up_Page_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            if (Scroll > 0)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move1);
                Scroll--;
                if (Scrollbar != null)
                    Scrollbar.scroll = Scroll;
                refresh_arrow_visibility();
            }
        }
        private void Down_Page_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            if (Scroll < OptionsNodes.Count - ROWS_AT_ONCE)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move1);
                Scroll++;
                if (Scrollbar != null)
                    Scrollbar.scroll = Scroll;
                refresh_arrow_visibility();
            }
        }

        protected void Scrollbar_UpArrowClicked(object sender, EventArgs e)
        {
            if (Scroll > 0)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move1);
                Scroll--;
                Scrollbar.scroll = Scroll;
                refresh_arrow_visibility();
            }
        }
        protected void Scrollbar_DownArrowClicked(object sender, EventArgs e)
        {
            if (Scroll < OptionsConfig.OPTIONS_DATA.Length - ROWS_AT_ONCE)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move1);
                Scroll++;
                Scrollbar.scroll = Scroll;
                refresh_arrow_visibility();
            }
        }
        #endregion

        protected void refresh_options(Constants.Options option)
        {
            switch (option)
            {
                case Constants.Options.Unit_Window:
                case Constants.Options.Enemy_Window:
                case Constants.Options.Terrain_Window:
                case Constants.Options.Objective_Window:
                    Map_Info_Changed = true;
                    break;
                case Constants.Options.Controller:
                    Map_Info_Changed = true;
                    if (Global.scene.is_map_scene)
                    {
                        Global.game_system.Instant_Move = true;
                        Global.game_map.center(Global.player.loc, true, forced: true);
                    }
                    break;
                case Constants.Options.Auto_Turn_End:
                    Global.game_state.block_auto_turn_end();
                    //Global.game_state.update_autoend_turn(); //Debug
                    break;
            }
        }

        #region Draw
        protected override void draw_window(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Scroll Bar
            if (Scrollbar != null)
                Scrollbar.draw(sprite_batch);
            Description_Window.draw(sprite_batch);
            Description.draw(sprite_batch);
            CancelButton.Draw(sprite_batch);
            Banner.draw(sprite_batch);
            Banner_Text.draw(sprite_batch);
            sprite_batch.End();
            // Labels
            sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(Data_Scissor_Rect);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            OptionsNodes.Draw(sprite_batch, Offset);
            SettingsNodes.Draw(sprite_batch, Offset);

            if (on_soloanim && SoloAnim_Allowed)
                Solo_Icon.draw(sprite_batch);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Cursor
            OptionsCursor.draw(sprite_batch);
            SettingsCursor.draw(sprite_batch);
            // Page Arrows
            Up_Page_Arrow.draw(sprite_batch);
            Down_Page_Arrow.draw(sprite_batch);
            sprite_batch.End();
        }
        #endregion
    }
}
