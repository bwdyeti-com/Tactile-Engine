using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Calculations.Stats;
using FEXNA.Graphics.Help;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Text;
using FEXNA.Menus;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Status;
using FEXNA_Library;

namespace FEXNA
{
    partial class Window_Status : BaseMenu
    {
        const int PAGE_SPACING = 64;//136; // 24 larger than typed //Debug
        const int BLACK_SCREEN_TIME = 4;
        readonly static Vector2 MAP_SPRITE_LOC = new Vector2(160, 44);

        protected bool Actors = false;
        protected List<int> Team;
        protected int Unit_Id;
        protected bool Closing = false;
        protected int Delay = 0, Direction = 0;
        protected bool Goto_Rescued = false;
        protected int Bg_Alpha = 0;
        protected int Black_Screen_Timer = BLACK_SCREEN_TIME;
        protected int Map_Sprite_Timer = 0;
        protected Vector2 Offset = Vector2.Zero;
        protected string Help_Index = "Name";
        protected HashSet<string> Blocked_Help_Indices = new HashSet<string>();
        protected Menu_Background Background;
        protected Sprite Black_Screen;
        protected Window_Help Help_Window;
        // Top Panel
        protected Status_Top_Panel_Bg Top_Panel;
        protected Status_Face Face;
        protected Menu_Background Face_Bg;
        protected Character_Sprite Map_Sprite;
        protected Sprite Platform;
        protected List<FE_Text> Battle_Stat_Labels;
        protected Sprite Rescue_Icon;
        protected List<StatusUINode> TopPanelNodes;
        protected List<UINodeSet<StatusUINode>> StatusNodes;
        protected List<UICursor<StatusUINode>> StatusCursors;
        // Pages
        protected Page_Arrow Left_Page_Arrow, Right_Page_Arrow;
        protected List<Status_Page> Pages = new List<Status_Page>();

        private Button_Description CancelButton;

        #region Accessors
        protected int page
        {
            get { return Global.game_system.Status_Page; }
            set { Global.game_system.Status_Page = value; }
        }

        protected Game_Unit unit { get { return Global.game_map.units[Actors ? Unit_Id : Team[Unit_Id]]; } }

        internal bool closed { get { return Black_Screen_Timer <= 0 && Closing; } }

        internal int current_unit { get { return Actors ? -1 : Team[Unit_Id]; } }

        internal int current_actor { get { return unit.actor.id; } }
        #endregion

        public Window_Status(List<int> team, int id)
        {
            initialize(team, id, false);
        }
        public Window_Status(List<int> team, int id, bool actors)
        {
            initialize(team, id, actors);
        }

        protected void initialize(List<int> team, int id, bool actors)
        {
            Actors = actors;
            Team = team;
            if (Actors)
            {
                Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM,
                    Config.OFF_MAP, id, "");
                Unit_Id = Global.game_map.last_added_unit.id;
            }
            else
                Unit_Id = id;
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
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Status_Background");
            Background.vel = new Vector2(-0.25f, 0);
            Background.tile = new Vector2(3, 2);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;

            // UI Nodes
            TopPanelNodes = new List<StatusUINode>();
            // Top Panel //
            // Top Panel
            Top_Panel = new Status_Top_Panel_Bg(new List<Texture2D>{
                Global.Content.Load<Texture2D>(@"Graphics/Pictures/Portrait_bg"),
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/FE_Window")});
            Top_Panel.stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;
            // Face Bg
            Face_Bg = new Menu_Background();// Sprite(); //Debug
            Face_Bg.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Portrait_bg");
            Face_Bg.loc = new Vector2(4, 4);
            Face_Bg.src_rect = new Rectangle(12, 4, 8, 8);
            Face_Bg.tile = new Vector2(11, 9);
            Face_Bg.stereoscopic = Config.STATUS_FACE_BG_DEPTH;
            // Map Sprite
            Map_Sprite = new Character_Sprite();
            Map_Sprite.draw_offset = MAP_SPRITE_LOC + new Vector2(16, 16);
            Map_Sprite.facing_count = 3;
            Map_Sprite.frame_count = 3;
            Map_Sprite.stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;
            // Map Sprite Platform
            Platform = new Sprite();
            Platform.texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/StatusPlatform");
            Platform.loc = MAP_SPRITE_LOC;
            Platform.stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;
            // Name
            TopPanelNodes.Add(new StatusTextUINode(
                "Name",
                (Game_Unit unit) => unit.name, true));
            TopPanelNodes.Last().loc = new Vector2(112, 4);
            TopPanelNodes.Last().draw_offset = new Vector2(34, 0);
            TopPanelNodes.Last().Size = new Vector2(68, 16);
            TopPanelNodes.Last().stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;

            // Class Name
            TopPanelNodes.Add(new StatusTextUINode(
                "Class",
                (Game_Unit unit) => unit.actor.class_name));
            TopPanelNodes.Last().loc = new Vector2(104, 24);
            TopPanelNodes.Last().Size = new Vector2(72, 16);
            TopPanelNodes.Last().stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;

            // Lives
            TopPanelNodes.Add(new StatusLivesUINode("Lives"));
            TopPanelNodes.Last().loc = new Vector2(180, 60);
            TopPanelNodes.Last().stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;
            // LV/HP/etc

            TopPanelNodes.Add(new StatusStatLargeLabelUINode(
                "Lvl", "LV", (Game_Unit unit) => unit.actor.level.ToString(), 32));
            TopPanelNodes.Last().loc = new Vector2(104, 40);
            TopPanelNodes.Last().stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;
            Func<Game_Unit, DirectionFlags, bool> lvl_cheat = (unit, dir) =>
            {
                int lvl_gain = 0;
                if (dir.HasFlag(DirectionFlags.Right))
                    lvl_gain = 1;
                else if (dir.HasFlag(DirectionFlags.Left))
                    lvl_gain = -1;
                unit.actor.level += lvl_gain;
                unit.queue_move_range_update();
                return lvl_gain != 0;
            };
#if DEBUG
            TopPanelNodes.Last().set_cheat(lvl_cheat);
#endif

            TopPanelNodes.Add(new StatusStatUINode(
                "Exp", "$", (Game_Unit unit) =>
                    unit.actor.can_level() ? unit.actor.exp.ToString() : "--", 24));
            TopPanelNodes.Last().loc = new Vector2(136, 40);
            TopPanelNodes.Last().stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;
            Func<Game_Unit, DirectionFlags, bool> exp_cheat = (unit, dir) =>
            {
                int exp_gain = 0;
                if (dir.HasFlag(DirectionFlags.Right))
                    exp_gain = 1;
                else if (dir.HasFlag(DirectionFlags.Left))
                    exp_gain = -1;
                unit.actor.exp = (int)MathHelper.Clamp(
                    unit.actor.exp + exp_gain,
                    0, Constants.Actor.EXP_TO_LVL - 1);
                return exp_gain != 0;
            };
#if DEBUG
            TopPanelNodes.Last().set_cheat(exp_cheat);
#endif

            Func<Game_Unit, Color> label_color = null;
            if (show_stat_colors(Stat_Labels.Hp))
            {
                label_color = (Game_Unit unit) =>
                {
                    if (unit.average_stat_hue_shown)
                        return unit.actor.stat_color(Stat_Labels.Hp);

                    return Color.White;
                };
            }
            TopPanelNodes.Add(new StatusHpUINode(
                "Hp",
                "HP",
                (Game_Unit unit) =>
                    {

                        if (unit.actor.hp >=
                            Math.Pow(10, Constants.BattleScene.STATUS_HP_COUNTER_VALUES))
                        {
                            //Lv_Hp_Values[2].text = ""; //Debug
                            //for (int i = 0; i < Constants.BattleScene.STATUS_HP_COUNTER_VALUES; i++)
                            //    Lv_Hp_Values[2].text += "?";
                            return "--";
                        }
                        else
                            return unit.actor.hp.ToString();
                    },
                    (Game_Unit unit) =>
                    {
                        if (unit.actor.maxhp >=
                            Math.Pow(10, Constants.BattleScene.STATUS_HP_COUNTER_VALUES))
                        {
                            //Lv_Hp_Values[3].text = ""; //Debug
                            //for (int i = 0; i < Constants.BattleScene.STATUS_HP_COUNTER_VALUES; i++)
                            //    Lv_Hp_Values[3].text += "?";
                            return "--";
                        }
                        else
                            return unit.actor.maxhp.ToString();
                    },
                    label_color));
            TopPanelNodes.Last().loc = new Vector2(104, 56);
            TopPanelNodes.Last().stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;
#if DEBUG
            TopPanelNodes.Last().set_cheat(
                Status_Page_1.stat_cheat(Stat_Labels.Hp));
#endif

            // Battle Stat Labels
            Battle_Stat_Labels = new List<FE_Text>();
            for (int i = 0; i < 1; i++)
            {
                Battle_Stat_Labels.Add(new FE_Text());
                Battle_Stat_Labels[i].loc = new Vector2(204 + (i / 4) * 56, 8 + (i % 4) * 16);
                Battle_Stat_Labels[i].Font = "FE7_Text";
                Battle_Stat_Labels[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
                Battle_Stat_Labels[i].text = "doop";
                Battle_Stat_Labels[i].stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;
            }
            Battle_Stat_Labels[0].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Battle_Stat_Labels[0].text = "Battle Stats";
            // Battle Stats
            for (int i = 1; i < 8; i++)
            {
                string help_label;
                string label;
                Func<Game_Unit, string> stat_formula;
                switch(i)
                {
                    // Atk
                    case 1:
                    default:
                        help_label = "Atk";
                        label = "Atk";
                        stat_formula = (Game_Unit unit) =>
                        {
                            var stats = new BattlerStats(unit.id);
                            if (!stats.has_non_staff_weapon)
                                return "--";
                            return stats.dmg().ToString();
                        };
                        break;
                    // Hit
                    case 2:
                        help_label = "Hit";
                        label = "Hit";
                        stat_formula = (Game_Unit unit) =>
                        {
                            var stats = new BattlerStats(unit.id);
                            if (!stats.has_non_staff_weapon)
                                return "--";
                            return stats.hit().ToString();
                        };
                        break;
                    // Dodge
                    case 7:
                        help_label = "Dodge";
                        label = "Dodge";
                        stat_formula = (Game_Unit unit) =>
                        {
                            var stats = new BattlerStats(unit.id);
                            return stats.dodge().ToString();
                        };
                        break;
                    // Range
                    case 4:
                        help_label = "Range";
                        label = "Rng";
                        stat_formula = (Game_Unit unit) =>
                        {
                            if (unit.actor.weapon == null ||
                                    unit.actor.weapon.is_staff())
                                return "--";

                            Data_Weapon weapon = unit.actor.weapon;
                            int min_range = unit.min_range();
                            int max_range = unit.max_range();
                            if (min_range == max_range)
                                return min_range.ToString();
                            else
                                return string.Format("{0}-{1}", min_range, max_range);
                        };
                        break;
                    // Crit
                    case 3:
                        help_label = "Crit";
                        label = "Crit";
                        stat_formula = (Game_Unit unit) =>
                        {
                            var stats = new BattlerStats(unit.id);
                            if (!stats.has_non_staff_weapon || !stats.can_crit())
                                return "--";
                            return stats.crt().ToString();
                        };
                        break;
                    // Avoid
                    case 6:
                        help_label = "Avoid";
                        label = "Avoid";
                        stat_formula = (Game_Unit unit) =>
                        {
                            var stats = new BattlerStats(unit.id);
                            return stats.avo().ToString();
                        };
                        break;
                    // AS
                    case 5:
                        help_label = "AS";
                        label = "AS";
                        stat_formula = (Game_Unit unit) => unit.atk_spd().ToString();
                        break;
                }

                Vector2 loc = new Vector2(204 + (i / 4) * 56, 8 + (i % 4) * 16);

                TopPanelNodes.Add(new StatusStatUINode(help_label, label, stat_formula));
                TopPanelNodes.Last().loc = loc;
                TopPanelNodes.Last().stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;
            }
            // Rescue Icon
            Rescue_Icon = new Sprite();
            Rescue_Icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/RescueIcon");
            Rescue_Icon.loc = new Vector2(103, -5);
            Rescue_Icon.stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;
            // Pages //
            Pages.Add(new Status_Page_1());
            Pages.Add(new Status_Page_2());
            Pages.Add(new Status_Page_3());
            // Page Arrows
            Left_Page_Arrow = new Page_Arrow();
            Left_Page_Arrow.loc = new Vector2(4, 84);
            Left_Page_Arrow.stereoscopic = Config.STATUS_ARROW_DEPTH;
            Left_Page_Arrow.ArrowClicked += Left_Page_Arrow_ArrowClicked;
            Right_Page_Arrow = new Page_Arrow();
            Right_Page_Arrow.loc = new Vector2(Config.WINDOW_WIDTH - 4, 84);
            Right_Page_Arrow.mirrored = true;
            Right_Page_Arrow.stereoscopic = Config.STATUS_ARROW_DEPTH;
            Right_Page_Arrow.ArrowClicked += Right_Page_Arrow_ArrowClicked;

            create_cancel_button();

            set_images();
        }

        private void create_cancel_button()
        {
            CancelButton = Button_Description.button(Inputs.B,
                Config.WINDOW_WIDTH / 2 - 16);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = Config.STATUS_TOP_PANEL_DEPTH;
        }

        internal static bool show_stat_colors(Stat_Labels stat_label)
        {
            if (Constants.Actor.STAT_LABEL_COLORING != Constants.StatLabelColoring.None)
                if (stat_label < Stat_Labels.Con &&
                        (!Constants.Actor.STAT_COLORS_ONLY_IN_PREP ||
                        (Global.scene is Scene_Worldmap || Global.game_system.preparations)))
                    return true;
            return false;
        }

        protected void set_images()
        {
            Game_Actor actor = unit.actor;
            // Face
            set_face(unit);
            // Map Sprite
            Map_Sprite.texture = Scene_Map.get_team_map_sprite(unit.team, unit.map_sprite_name);
            if (Map_Sprite.texture != null)
                Map_Sprite.offset = new Vector2(
                    (Map_Sprite.texture.Width / Map_Sprite.frame_count) / 2,
                    (Map_Sprite.texture.Height / Map_Sprite.facing_count) - 8);
            Map_Sprite.mirrored = unit.has_flipped_map_sprite;
            // Lives

            //Debug
            Blocked_Help_Indices.Remove("Lives");
            if (Global.game_system.Style != Mode_Styles.Casual || !unit.lives_visible)
                Blocked_Help_Indices.Add("Lives");

            // Rescue_Icon
            Rescue_Icon.visible = unit.is_rescued;
            if (unit.is_rescued)
                Rescue_Icon.src_rect = new Rectangle(
                    (Global.game_map.units[unit.rescued].team - 1) *
                        (Rescue_Icon.texture.Width / Constants.Team.NUM_TEAMS),
                    0,
                    Rescue_Icon.texture.Width / Constants.Team.NUM_TEAMS,
                    Rescue_Icon.texture.Height);
            // Pages
            foreach (Status_Page status_page in Pages)
                status_page.set_images(unit);

            // Refresh UI nodes
            foreach (StatusUINode node in TopPanelNodes)
            {
                node.refresh(unit);
            }

            // Get page UI nodes
            StatusNodes = new List<UINodeSet<StatusUINode>>();
            StatusCursors = new List<UICursor<StatusUINode>>();
            for (int i = 0; i < Pages.Count; i++)
            {
                var page_nodes = Pages[i].node_union(TopPanelNodes);
                StatusNodes.Add(new UINodeSet<StatusUINode>(page_nodes));

                var cursor = new UICursor<StatusUINode>(StatusNodes.Last());
                cursor.draw_offset = new Vector2(-14, 0);
                cursor.hide_when_using_mouse(false);
                StatusCursors.Add(cursor);
            }
        }

        protected void set_face(Game_Unit unit)
        {
            Face = null;
            Global.dispose_face_textures();
            Face = new Status_Face(unit);
            Face.loc = new Vector2(8 + Face_Sprite_Data.STATUS_FACE_SIZE.X / 2, 4 + Face_Sprite_Data.STATUS_FACE_SIZE.Y);
            Face.mirrored = unit.has_flipped_face_sprite;
            Face.stereoscopic = Config.STATUS_FACE_DEPTH;
        }

        public event EventHandler<EventArgs> Closed;
        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            update_map_sprite();
            update_direction();
            // Black Screen
            update_black_screen();
            // Inputs
            bool input = active && Delay == 0 && !Closing && Black_Screen_Timer <= 0;
#if DEBUG
            if (Help_Window != null && input && Global.Input.pressed(Inputs.X))
            {
                DirectionFlags dir = Global.Input.dir_triggered();
                if (dir != DirectionFlags.None)
                {
                    StatusNodes[page].ActiveNode.cheat(this.unit, dir);
                    move_to(Help_Index);
                    Help_Window.add_remaining_text();

                    foreach (var status_page in Pages)
                        status_page.refresh(this.unit);
                    foreach (StatusUINode node in TopPanelNodes)
                        node.refresh(unit);
                }
                input = false;
            }
#endif

            // Cancel button
            CancelButton.Update(input);

            StatusNodes[page].Update(!input ? ControlSet.None :
                (Help_Window != null ?
                    ControlSet.Movement : (ControlSet.Mouse | ControlSet.Touch)));

            if (input)
            {
                if (Help_Window == null)
                {
                    update_input();

                    var help_index = StatusNodes[page].consume_triggered(
                        MouseButtons.Left, TouchGestures.Tap);
                    if (help_index.IsNothing)
                        help_index = StatusNodes[page].consume_triggered(
                            TouchGestures.LongPress);

                    if (help_index.IsSomething)
                    {
                        Help_Index = StatusNodes[page][help_index].HelpLabel;
                        open_help();
                    }
                }
                else
                {
                    if (StatusNodes[page].ActiveNode.HelpLabel != Help_Index)
                        move_to(StatusNodes[page].ActiveNode.HelpLabel);

                    var help_index = StatusNodes[page].consume_triggered(
                        MouseButtons.Left, TouchGestures.Tap);
                    var help_cancel_index = StatusNodes[page].consume_triggered(
                        TouchGestures.LongPress);

                    if (Global.Input.triggered(Inputs.B) ||
                            Global.Input.triggered(Inputs.R) ||
                            Global.Input.mouse_click(MouseButtons.Right) ||
                            help_cancel_index.IsSomething ||
                            CancelButton.consume_trigger(MouseButtons.Left) ||
                            CancelButton.consume_trigger(TouchGestures.Tap))
                        close_help();
                    /* //Debug
                    if (Global.Input.repeated(Inputs.Down))
                    {
                        if (move(2))
                        {

                        }
                    }
                    if (Global.Input.repeated(Inputs.Up))
                    {
                        if (move(8))
                        {

                        }
                    }
                    if (Global.Input.repeated(Inputs.Right))
                    {
                        if (move(6))
                        {

                        }
                    }
                    if (Global.Input.repeated(Inputs.Left))
                    {
                        if (move(4))
                        {

                        }
                    }
                    else if (Global.Input.triggered(Inputs.R) || Global.Input.triggered(Inputs.B))
                    {
                        close_help();
                    }*/
                }
                StatusCursors[page].update();
            }

            if (Help_Window != null)
                Help_Window.update();
            Background.update();
            // Top Panel
            Top_Panel.update();
            foreach (FE_Text label in Battle_Stat_Labels)
                label.update();
            // Pages
            foreach (Status_Page status_page in Pages)
                status_page.update();
            Left_Page_Arrow.update();
            Right_Page_Arrow.update();
        }

        protected override void UpdateAncillary()
        {
            if (Input.ControlSchemeSwitched)
                create_cancel_button();
        }

        private void update_input()
        {
            Left_Page_Arrow.UpdateInput(new Vector2(0, Offset.Y));
            Right_Page_Arrow.UpdateInput(new Vector2(0, Offset.Y));
            if (Delay != 0)
                return;

            if (Global.Input.pressed(Inputs.Up) || Global.Input.mouseScroll > 0 ||
                Global.Input.gesture_triggered(TouchGestures.SwipeDown))
            {
                Delay = 14;
                Direction = 8;
            }
            else if (Global.Input.pressed(Inputs.Down) || Global.Input.mouseScroll < 0 ||
                Global.Input.gesture_triggered(TouchGestures.SwipeUp))
            {
                Delay = 14;
                Direction = 2;
            }
            else if (Global.Input.pressed(Inputs.Left) ||
                Global.Input.gesture_triggered(TouchGestures.SwipeRight))
            {
                Delay = 22;
                Direction = 4;
            }
            else if (Global.Input.pressed(Inputs.Right) ||
                Global.Input.gesture_triggered(TouchGestures.SwipeLeft))
            {
                Delay = 22;
                Direction = 6;
            }
            else if (Global.Input.triggered(Inputs.B) ||
                Global.Input.mouse_click(MouseButtons.Right) ||
                CancelButton.consume_trigger(MouseButtons.Left) ||
                CancelButton.consume_trigger(TouchGestures.Tap))
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                if (!Actors)
                {
                    Game_Unit status_unit = Global.game_map.units[Global.game_temp.status_unit_id];
                    status_unit.highlighted = false;
                    Global.game_temp.status_unit_id = -1;
                }
                Closing = true;
                Black_Screen_Timer = BLACK_SCREEN_TIME;
                Black_Screen.visible = true;
            }
            else if (Global.Input.triggered(Inputs.R))
            {
                open_help();
            }
            else if (Global.Input.triggered(Inputs.A) && (unit.is_rescued || unit.is_rescuing))
            {
                if (Actors)
                {
                    bool same_team = false;
                    if (unit.is_rescued)
                    {
                        if (Global.game_map.units[unit.rescued].same_team(unit))
                            same_team = true;
                    }
                    else
                        if (Global.game_map.units[unit.rescuing].same_team(unit))
                            same_team = true;
                    if (!same_team)
                        return;
                }
                if (unit.is_rescued && !Team.Contains(unit.rescued) &&
                        Global.game_map.units[unit.rescued].same_team(unit))
                    return;
                if (unit.is_rescuing && !Team.Contains(unit.rescuing) &&
                        Global.game_map.units[unit.rescuing].same_team(unit))
                    return;

                Delay = 14;
                Direction = unit.is_rescuing ? 2 : 8;
                Goto_Rescued = true;
            }
        }

        private void open_help()
        {
            while (unit_change_help_index_fix()) { }
            Help_Window = new Window_Help();
            Help_Window.stereoscopic = Config.STATUS_HELP_DEPTH;
            set_help_initial_loc();
            move_to(Help_Index);
            Global.game_system.play_se(System_Sounds.Help_Open);
        }

        private void close_help()
        {
            Help_Window = null;
            Global.game_system.play_se(System_Sounds.Help_Close);
        }

        private void Left_Page_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            Delay = 22;
            Direction = 4;
        }
        private void Right_Page_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            Delay = 22;
            Direction = 6;
        }

        protected void update_map_sprite()
        {
            Map_Sprite_Timer = (Map_Sprite_Timer + 1) % 48;
            int frame = 0;
            int count = Map_Sprite_Timer;
            if (count >= 0 && count < 20)
                frame = 0;
            else if (count >= 20 && count < 24)
                frame = 1;
            else if (count >= 24 && count < 44)
                frame = 2;
            else if (count >= 44 && count < 48)
                frame = 1;
            Map_Sprite.frame = 2 * Map_Sprite.frame_count + frame;
        }

        protected void update_black_screen()
        {
            Black_Screen.visible = Black_Screen_Timer > 0;
            if (Black_Screen_Timer > 0)
            {
                Black_Screen_Timer--;
                if (Closing && Black_Screen_Timer == 0)
                    OnClosed(new EventArgs());
            }
        }

        protected void update_direction()
        {
            if (changing_unit)
            {
                switch (Delay)
                {
                    case 13:
                        Bg_Alpha = 50;
                        break;
                    case 12:
                        Global.game_system.play_se(System_Sounds.Status_Character_Change);
                        Bg_Alpha = 100;
                        break;
                    case 11:
                        Offset.Y = Direction == 2 ? 5 : -5;
                        Bg_Alpha = 150;
                        break;
                    case 10:
                        Offset.Y = Direction == 2 ? 14 : -14;
                        Bg_Alpha = 200;
                        break;
                    case 9:
                    case 8:
                    case 7:
                        Offset.Y = Direction == 2 ? 14 : -14;
                        Bg_Alpha = 255;
                        break;
                    case 6:
                        if (Goto_Rescued)
                        {
                            int id = Direction == 2 ? unit.rescuing : unit.rescued;
                            if (Team.Contains(id))
                                Unit_Id = Team.IndexOf(id);
                            else if (!Actors)
                            {
                                Global.game_temp.status_team = Global.game_map.units[id].team;
                                Team.Clear();
                                Team.AddRange(Global.game_map.teams[Global.game_temp.status_team]);
                                Unit_Id = Team.IndexOf(id);
                            }
                            else
                                throw new KeyNotFoundException("Rescued/Rescuing unit not on this team: " + id.ToString());
                            Goto_Rescued = false;
                        }
                        else
                        {
                            if (Actors)
                                update_direction_actor();
                            else
                                update_direction_unit();
                        }
                        set_images();
                        Offset.Y = Direction == 2 ? 14 : -14;
                        Bg_Alpha = 255;
                        break;
                    case 5:
                        Offset.Y = Direction == 2 ? -49 : 49;
                        Bg_Alpha = 200;
                        break;
                    case 4:
                        Offset.Y = Direction == 2 ? -25 : 25;
                        Bg_Alpha = 150;
                        break;
                    case 3:
                        Offset.Y = Direction == 2 ? -10 : 10;
                        Bg_Alpha = 100;
                        break;
                    case 2:
                        Offset.Y = Direction == 2 ? -3 : 3;
                        Bg_Alpha = 50;
                        break;
                    case 1:
                        Offset = Vector2.Zero;
                        Bg_Alpha = 0;
                        break;
                }
            }
            else if (changing_page)
            {
                // Spin the page arrow in the direction moved
                (Direction == 4 ? Left_Page_Arrow : Right_Page_Arrow).twirling_update();
                Maybe<int> offset = page_change_offset(Delay);
                if (offset.IsSomething)
                    Offset.X = (Direction == 4 ? -1 : 1) * offset;
                switch (Delay)
                {
                    case 20:
                        Global.game_system.play_se(System_Sounds.Status_Page_Change);
                        break;
                    case 12:
                        page = (page + (Direction == 4 ? -1 : 1) + Pages.Count) % Pages.Count;
                        reset_help_index();
                        break;
                    case 2:
                        Offset = Vector2.Zero;
                        Direction = 0;
                        break;
                }
            }
            if (Delay > 0) Delay--;
        }

        private Maybe<int> page_change_offset(int delay)
        {
            switch (Delay)
            {
                case 20:
                    return 8;//* 16;//* 32;
                case 19:
                    return 24;//* 32;//* 56;
                case 18:
                    return 40;//* 56;//* 80;
                case 17:
                    return 64;//* 80;//* 96;
                case 16:
                    return 88;//* 104;//* 112;
                case 15:
                    return 112;//* 136;//* 144;
                case 14:
                    return 144;//* 176;//* 192;
                case 13:
                    return 176;//* 208;//* 240;

                case 12:
                    return -176;//* 200;//* 232;
                case 11:
                    return -144;//* 168;//* 184;
                case 10:
                    return -120;//* 128;//* 136;
                case 9:
                    return -96;//* 104;//* 104;
                case 8:
                    return -72;//* 80;//* 72;
                case 7:
                    return -56;//* 56;//* 56;
                case 6:
                    return -40;//* 40;//* 40;
                case 5:
                    return -24;//* 24;//* 24;
                case 4:
                    return -16;//* 16;//* 16;
                case 3:
                    return -8;//* 8;//* 8;
            }
            return default(Maybe<int>);
        }

        private void update_direction_actor()
        {
            int old_unit_id = Unit_Id;
            Unit_Id = unit.actor.id;
            Unit_Id = (Team.IndexOf(Unit_Id) + (Direction == 2 ? 1 : -1) + Team.Count) % Team.Count;
            Global.game_map.replace_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, Team[Unit_Id], old_unit_id);
            Unit_Id = Global.game_map.last_added_unit.id;
        }

        private void update_direction_unit()
        {
            // If entire team is not visible, don't get stuck in an infinite loop
            if (Team.Any(x => Global.game_map.units[x].visible_by()))
            {
                int dir = (Direction == 2 ? 1 : -1);
                do
                {
                    Unit_Id = (Unit_Id + dir + Team.Count) % Team.Count;
                } while (!unit.visible_by());
            }
        }
        #endregion

        #region Help
        protected void set_help_initial_loc()
        {
            Help_Window.loc = StatusNodes[page].ActiveNode.loc; // Help_Data[page][Help_Index].Loc;
        }

        protected void reset_help_index()
        {
            Help_Index = "Name";
        }

        protected bool unit_change_help_index_fix()
        {
            /* //Debug
            if (Blocked_Help_Indices.Contains(Help_Index))
                reset_help_index();
            if (Help_Index.Length >= 4 && Help_Index.Substring(0, 4) == "Item")
            {
                if (Convert.ToInt32(Help_Index.Substring(4, 1)) > unit.actor.num_items)
                {
                    if (Help_Data[page][Help_Index].Movements.ContainsKey(8))
                        Help_Index = Help_Data[page][Help_Index].Movements[8];
                    else
                        reset_help_index();
                    return true;
                }
            }
            if (Help_Index.Length >= 5 && Help_Index.Substring(0, 5) == "Skill")
            {
                if (Convert.ToInt32(Help_Index.Substring(5, 1)) > unit.actor.skills.Count)
                {
                    if (Help_Data[page][Help_Index].Movements.ContainsKey(8))
                        Help_Index = Help_Data[page][Help_Index].Movements[8];
                    else
                        reset_help_index();
                    return true;
                }
            }
            if (Help_Index.Length >= 6 && Help_Index.Substring(0, 6) == "Status")
            {
                if (Convert.ToInt32(Help_Index.Substring(6, 1)) > unit.actor.states.Count)
                {
                    if (Help_Data[page][Help_Index].Movements.ContainsKey(8))
                        Help_Index = Help_Data[page][Help_Index].Movements[8];
                    else
                        reset_help_index();
                    return true;
                }
            }
            */
            return false;
        }

        protected void move_to(string index)
        {
            if (false) //!Help_Data[page].ContainsKey(index)) //Debug
                reset_help_index();
            else
                Help_Index = index;

            string help_label = StatusNodes[page].ActiveNode.HelpLabel;
            var regex = new System.Text.RegularExpressions.Regex(@"\d+$");
            string label = regex.Replace(help_label, "");
            switch (label)
            {
                case "Name":
                    string description = unit.actor.description;
#if !MONOGAME && DEBUG
                    if (Global.scene.is_unit_editor)
                    {
                        string identifier = "";
                        foreach (KeyValuePair<string, int> ident in Global.game_map.unit_identifiers)
                            if (ident.Value == unit.id)
                            {
                                identifier = ident.Key;
                                break;
                            }
                        description = string.Format("{0}\n{1}, {2}", description, unit.id, identifier);
                    }
#endif
                    Help_Window.set_text(description);
                    break;
                case "Class":
                    Help_Window.set_text(unit.actor.actor_class.Description);
                    break;
                case "Pow":
                    switch (unit.actor.power_type())
                    {
                        case Power_Types.Strength:
                            Help_Window.set_text(Global.system_text["Status Help Str"]);
                            break;
                        case Power_Types.Magic:
                            Help_Window.set_text(Global.system_text["Status Help Mag"]);
                            break;
                        case Power_Types.Power:
                            Help_Window.set_text(Global.system_text["Status Help Pow"]);
                            break;
                    }
                    break;
                case "Item":
                    Help_Window.set_item(unit.items[Convert.ToInt32(help_label.Substring(4, 1)) - 1], unit.actor);
                    break;
                case "Skill":
                    Help_Window.set_text(Global.data_skills[unit.actor.skills[
                        Convert.ToInt32(help_label.Substring(5, 1)) - 1]].Description.Replace("|", "\n"));
                    break;
                case "Item Skill":
                    Help_Window.set_text(Global.data_skills[unit.actor.item_skills[
                        Convert.ToInt32(help_label.Substring(10, 1)) - 1]].Description.Replace("|", "\n"));
                    break;
                case "Status":
                    Help_Window.set_text(Global.data_statuses[unit.actor.states[
                        Convert.ToInt32(help_label.Substring(6, 1)) - 1]].Description.Replace("|", "\n"));
                    break;
                default:
                    string text_key = string.Format("Status Help {0}", help_label);
                    if (Global.system_text.ContainsKey(text_key))
                        Help_Window.set_text(Global.system_text[text_key]);
                    else
                        Help_Window.set_text(help_label);
                    break;
            }
            Help_Window.set_loc(StatusNodes[page].ActiveNode.loc);

            //Help_Window.set_loc(Help_Data[page][Help_Index].Loc + Help_Data[page][Help_Index].Offset);
        }
        #endregion

        protected bool changing_page { get { return Direction == 4 || Direction == 6; } }
        protected bool changing_unit { get { return Direction == 2 || Direction == 8; } }

        public override void Draw(SpriteBatch sprite_batch)
        {
            Vector2 offset = new Vector2(0, Offset.Y);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            Background.draw(sprite_batch);
            // Draw Windows //
            // Page //
            Pages[page].draw(sprite_batch, Offset);
            if (Delay > 0 && changing_page)
            {
                if (Direction == 6 ^ Delay < 12)
                    Pages[(page + 1 + Pages.Count) % Pages.Count].draw(sprite_batch, Offset - new Vector2(PAGE_SPACING + 320, 0));
                else
                    Pages[(page - 1 + Pages.Count) % Pages.Count].draw(sprite_batch, Offset + new Vector2(PAGE_SPACING + 320, 0));
            }
            //if (changing_page)
            //    Background.draw(sprite_batch, new Color(Bg_Alpha, Bg_Alpha, Bg_Alpha, Bg_Alpha));
            // Top Panel //
            // Page Arrows
            Left_Page_Arrow.draw(sprite_batch, offset);
            Right_Page_Arrow.draw(sprite_batch, offset);
            Face_Bg.draw(sprite_batch, offset);

            if (Input.ControlScheme == ControlSchemes.Touch)
                CancelButton.Draw(sprite_batch);
            sprite_batch.End();

            Face.draw(sprite_batch, offset);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Top_Panel.draw(sprite_batch, offset);
            Platform.draw(sprite_batch, offset);
            //LvLabel.draw(sprite_batch, offset);
            //HpLabel.draw(sprite_batch, offset);
            //Lv_Hp_Labels_Other.draw(sprite_batch, offset);
            //foreach (FE_Text_Int stat in Lv_Hp_Values)
            //    stat.draw(sprite_batch, offset);
            foreach (FE_Text label in Battle_Stat_Labels) 
                label.draw(sprite_batch, offset);

            Map_Sprite.draw(sprite_batch, offset);

            foreach (var node in TopPanelNodes)
                node.Draw(sprite_batch, offset);

            if (Global.game_map.icons_visible)
                Rescue_Icon.draw(sprite_batch, offset);
            if (changing_unit)
            {
                Color bg_tint = Background.tint;
                Background.tint = new Color(Bg_Alpha, Bg_Alpha, Bg_Alpha, Bg_Alpha);
                Background.draw(sprite_batch); 
                Background.tint = bg_tint;
            }
            Black_Screen.draw(sprite_batch);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (Help_Window != null) //Debug
                StatusCursors[page].draw(sprite_batch, offset);
            sprite_batch.End();
            if (Help_Window != null)
                Help_Window.draw(sprite_batch);
        }

        public void jump_to_unit()
        {
            if (!Actors)
            {
                if (Input.ControlScheme == ControlSchemes.Buttons)
                {
                    int id;
                    if (Global.game_system.Selected_Unit_Id != -1)
                    {
                        Game_Unit selected_unit = Global.game_map.get_selected_unit();
                        if (selected_unit.is_on_square && selected_unit.ready)
                            id = selected_unit.id;
                        else
                            id = Team[Unit_Id];
                    }
                    else
                        id = Team[Unit_Id];

                    Game_Unit unit = Global.game_map.units[id];
                    if (unit.is_rescued)
                        unit = Global.game_map.units[unit.rescued];
                    Global.player.instant_move = true;
                    Global.player.loc = unit.loc;
                }
            }

            Global.game_map.highlight_test();
            Global.player.update();

            Global.game_map.update_move_arrow();
        }

        public void close()
        {
            if (Actors)
                Global.game_map.completely_remove_unit(Unit_Id);
        }
    }

    struct Help_Node_Data
    {
        public Vector2 Loc, Offset;
        public string Data;
        public Dictionary<int, string> Movements;

        public Help_Node_Data(Vector2 loc, Vector2 window_offset, string data, Dictionary<int, string> movements)
        {
            Loc = loc;
            Offset = window_offset;
            Data = data;
            Movements = movements;
        }
    }
}
