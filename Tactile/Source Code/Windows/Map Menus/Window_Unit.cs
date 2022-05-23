using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.ConfigData;
using Tactile.Graphics.Help;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;
using Tactile.Graphics.UnitScreen;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Options;

namespace Tactile.Windows.Map
{
    internal class Window_Unit : Map_Window_Base
    {
        protected const int BASE_Y = 56;
        public const int SUPPORTS_PER_PAGE = 4;
        const int Y_PER_ROW = 16;
        const int ROWS_AT_ONCE = (Config.WINDOW_HEIGHT - 72) / Y_PER_ROW;
        const int OFFSET_Y = -(16 - Y_PER_ROW) / 2;
        const int SORT_ARROW_TIME = 32;

        protected int Y_Per_Row;
        protected int Rows_At_Once;
        protected int Offset_Y;

        protected List<int> Team;
        
        protected int Delay = 0, Direction = 0, Sort_Arrow_Timer = 0;
        protected int _Scroll = 0;
        protected Vector2 Offset = Vector2.Zero;
        protected bool Show_Page_Number = true;
        protected bool Preparations;
        protected bool Unit_Selected = false;
        private TactileLibrary.Maybe<int> StoredActorId;
        protected MenuScreenBanner Banner;
        protected Sprite Banner_Text;
        protected SystemWindowHeadered Background_Window;
        protected System_Color_Window Sort_Window, Page_Window;
        protected List<Character_Sprite> Map_Sprites = new List<Character_Sprite>();
        protected Sprite Sort_Label_Text, Sort_Label, Sort_Arrow;
        protected TextSprite Page_Number, Max_Page_Number, Page_Number_Slash;
        protected Unit_Line_Cursor Line_Cursor;
        protected Scroll_Bar Scrollbar;
        protected Page_Arrow Left_Page_Arrow, Right_Page_Arrow;
        protected Window_Prep_PickUnits PickUnits_Window;

        protected bool HeaderActive = false;
        protected List<UnitScreenHeader[]> PageKeys;
        private List<PartialRangeVisibleUINodeSet<UnitHeaderUINode>> HeaderNodes;
        protected List<UICursor<UnitHeaderUINode>> HeaderCursors;
        protected PartialRangeVisibleUINodeSet<UnitScreenUINode> UnitNodes;
        private UnitScreenRow[][] UnitData;
        protected IndexScrollComponent Scroll;

        private Button_Description CancelButton, AButton, RButton;

        protected Rectangle Unit_Scissor_Rect, Header_Scissor_Rect, Data_Scissor_Rect;
        protected RasterizerState Scissor_State = new RasterizerState { ScissorTestEnable = true };

        #region Accessors
        protected bool changing_page { get { return Direction == 4 || Direction == 6; } }

        public bool unit_selected { get { return Unit_Selected; } }

        public int unit_index
        {
            set
            {
                int index = Team.IndexOf(value);
                UnitNodes.set_active_node(UnitNodes[index]);
                Scroll.FixScroll(index);
                refresh_scroll();
                refresh_cursor_location();
            }
            get { return Team[UnitNodes.ActiveNodeIndex]; }
        }

        public List<int> team { get { return new List<int>(Team); } }

        protected int page
        {
            get { return _get_page(); }
            set { _set_page(value); }
        }
        protected int PageCount { get { return PageKeys.Count; } }
        protected virtual int sort
        {
            get { return Global.game_system.Unit_Sort; }
            set { Global.game_system.Unit_Sort = value; }
        }
        protected virtual bool sort_up
        {
            get { return Global.game_system.Unit_Sort_Up; }
            set { Global.game_system.Unit_Sort_Up = value; }
        }

        protected static int data_width { get { return Config.WINDOW_WIDTH - 96; } }

        protected int row_max { get { return Team.Count + 1; } }

        protected Game_Unit unit
        {
            get
            {
                return _unit();
            }
        }
        protected Game_Actor actor
        {
            get
            {
                Game_Unit result = this.unit;
                if (result == null)
                    return null;
                return result.actor;
            }
        }
        internal int ActorId
        {
            get
            {
                Game_Actor result = this.actor;
                if (result == null)
                    return -1;
                return result.id;
            }
            set
            {
                if (UnitNodes == null)
                    StoredActorId = value;
                else
                {
                    StoredActorId = TactileLibrary.Maybe<int>.Nothing;

                    var actors = Team.Select(x => _unit(x).actor.id).ToList();
                    int index = actors.IndexOf(value);
                    if (index > -1)
                    {
                        this.unit_index = Team[index];
                    }
                }
            }
        }

        public Window_Prep_PickUnits pickunits_window
        {
            set
            {
                if (PickUnits_Window == null)
                    PickUnits_Window = value;
            }
        }

        protected override bool ready_for_inputs { get { return Delay == 0 && base.ready_for_inputs; } }
        #endregion

        #region Accessor Overrides
        protected virtual int _get_page()
        {
            return Global.game_system.Unit_Page;
        }
        protected virtual void _set_page(int value)
        {
            Global.game_system.Unit_Page = value;
        }

        protected Game_Unit _unit()
        {
            return _unit(Team[UnitNodes.ActiveNodeIndex]);
        }
        protected virtual Game_Unit _unit(int id)
        {
            return Global.game_map.units[id];
        }

        protected virtual UnitScreenData[] DataSet { get { return UnitScreenConfig.UNIT_DATA; } }
        #endregion

        public Window_Unit()
        {
            initialize();
        }

        #region Initialization
        protected void initialize()
        {
            InitializePositioning();

            Preparations = DeterminePreparations();
            Team = determine_team();

            InitializePageKeys(this.DataSet);

            this.page = Math.Min(this.page, this.PageCount - 1);
            initialize_sprites();
            update_black_screen();
        }
        protected virtual void InitializePositioning()
        {
            Y_Per_Row = Y_PER_ROW;
            Rows_At_Once = ROWS_AT_ONCE;
            Offset_Y = OFFSET_Y;
            Unit_Scissor_Rect = new Rectangle(-12, BASE_Y, 80 + Window_Unit.data_width + 16, ROWS_AT_ONCE * Y_PER_ROW);
            Header_Scissor_Rect = new Rectangle(76, BASE_Y - 16, Window_Unit.data_width, 16);
            Data_Scissor_Rect = new Rectangle(76, BASE_Y, Window_Unit.data_width, ROWS_AT_ONCE * Y_PER_ROW);
        }

        protected virtual bool DeterminePreparations()
        {
            return Global.game_system.preparations && !Global.game_system.home_base;
        }
        protected virtual List<int> determine_team()
        {
            List<int> team = new List<int>();
            if (Preparations || Global.scene.is_worldmap_scene || Global.game_system.home_base)
            {
                Global.game_map.init_preparations_unit_team();
                team.AddRange(Global.game_map.preparations_unit_team);
            }
            else
            {
#if DEBUG
                if (Global.scene.scene_type == "Scene_Map_Unit_Editor")
                    team.AddRange(Global.game_map.teams[Global.game_state.team_turn]);
                else
#endif
                    // Only list units that are on the map or rescued (rescued units can be off map)
                    team.AddRange(Global.game_map.teams[Global.game_state.team_turn]
                        .Where(x => !Global.game_map.is_off_map(Global.game_map.units[x].loc) || Global.game_map.units[x].is_rescued));
            }
            return team;
        }

        protected void InitializePageKeys(UnitScreenData[] data)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(
                data.Select(x => x.Name).Distinct().Count() == data.Length,
                "Unit Screen keys must all have distinct names");
#endif
            var pages = data
                .Select(x => x.Page)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
            PageKeys = new List<UnitScreenHeader[]>();
            var teamMembers = Team
                .Select(x => (object)_unit(x))
                .ToList();
            for (int i = 0; i < pages.Length; i++)
            {
                var page = pages[i];
                var pageData = data
                    .Where(x => x.Page == pages[i])
                    .ToArray();
                // Get the number of pages each header wants to repeat for
                var pageCounts = pageData
                    .Select(x => x.GetPageCount(teamMembers))
                    .ToArray();
                int count = pageCounts.Max();

                for (int j = 0; j < count; j++)
                {
                    var newPage = Enumerable.Range(0, pageData.Length)
                        .Where(x => j < pageCounts[x])
                        .Select(x =>
                        {
                            var header = pageData[x];
                            int index = Array.FindIndex(data, val => val.Equals(header));
                            return new UnitScreenHeader(index, header.Name, j);
                        })
                        .ToArray();
                    if (newPage.Any())
                        PageKeys.Add(newPage);
                }
            }
        }

        protected void initialize_sprites()
        {
            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 255);
            // Banner
            Banner = new MenuScreenBanner();
            Banner.width = 136;
            Banner.loc = new Vector2(4, 0);
            Banner.stereoscopic = Config.UNIT_BANNER_DEPTH;
            Banner_Text = new Sprite();
            Banner_Text.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Banner_Text");
            Banner_Text.loc = new Vector2(4 + 24 + 2, 0 + 8);
            Banner_Text.stereoscopic = Config.UNIT_BANNER_DEPTH;
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Status_Background");
            (Background as Menu_Background).vel = new Vector2(-0.25f, 0);
            (Background as Menu_Background).tile = new Vector2(3, 2);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
            // Window
            Background_Window = new SystemWindowHeadered();
            Background_Window.loc = new Vector2(4, BASE_Y - 24);
            Background_Window.width = Config.WINDOW_WIDTH - 8;
            Background_Window.height = Rows_At_Once * Y_Per_Row + 34; // not 32 o_O? //Yeti
            Background_Window.stereoscopic = Config.UNIT_WINDOW_DEPTH;
            // Sort/Page window
            Sort_Window = new System_Color_Window();
            Sort_Window.loc = new Vector2(Config.WINDOW_WIDTH - 92, 0);
            Sort_Window.width = 48;
            Sort_Window.height = 32;
            Sort_Window.stereoscopic = Config.UNIT_SORT_DEPTH;
            Page_Window = new System_Color_Window();
            Page_Window.loc = new Vector2(Config.WINDOW_WIDTH - 44, 0);
            Page_Window.width = 40;
            Page_Window.height = 32;
            Page_Window.stereoscopic = Config.UNIT_SORT_DEPTH;
            // Sort Label
            Sort_Label = new Sprite();
            Sort_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Unit_Window_Components");
            Sort_Label.loc = new Vector2(Config.WINDOW_WIDTH - 84, 0);
            Sort_Label.src_rect = new Rectangle(16, 0, 24, 8);
            Sort_Label.stereoscopic = Config.UNIT_SORT_DEPTH;
            Sort_Arrow = new Sprite();
            Sort_Arrow.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Unit_Window_Components");
            Sort_Arrow.loc = new Vector2(Config.WINDOW_WIDTH - 60, 8);
            Sort_Arrow.stereoscopic = Config.UNIT_SORT_DEPTH;
            // Page Numbers
            Page_Number = new RightAdjustedText();
            Page_Number.loc = new Vector2(Config.WINDOW_WIDTH - 28, 8);
            Page_Number.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Page_Number.stereoscopic = Config.UNIT_SORT_DEPTH;
            Page_Number_Slash = new TextSprite();
            Page_Number_Slash.loc = new Vector2(Config.WINDOW_WIDTH - 28, 8);
            Page_Number_Slash.SetFont(Config.UI_FONT, Global.Content, "White");
            Page_Number_Slash.text = "/";
            Page_Number_Slash.stereoscopic = Config.UNIT_SORT_DEPTH;
            Max_Page_Number = new RightAdjustedText();
            Max_Page_Number.loc = new Vector2(Config.WINDOW_WIDTH - 12, 8);
            Max_Page_Number.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Max_Page_Number.text = this.PageCount.ToString();
            Max_Page_Number.stereoscopic = Config.UNIT_SORT_DEPTH;
            // Cursor
            Cursor = new Hand_Cursor();
            Cursor.visible = false;
            Cursor.stereoscopic = Config.UNIT_HELP_DEPTH;
            // Line Cursor
            Line_Cursor = new Unit_Line_Cursor(Config.WINDOW_WIDTH - 24);
            Line_Cursor.loc = new Vector2(8, 8 + BASE_Y + Offset_Y);
            Line_Cursor.stereoscopic = Config.UNIT_WINDOW_DEPTH;
            // Scroll Bar
            if (Team.Count > Rows_At_Once)
            {
                Scrollbar = new Scroll_Bar(Rows_At_Once * Y_Per_Row - 16, Team.Count, Rows_At_Once, 0);
                Scrollbar.loc = Background_Window.loc + new Vector2(Background_Window.width - 16, 32);
                Scrollbar.stereoscopic = Config.UNIT_WINDOW_DEPTH;
            }
            // Page Arrows
            Left_Page_Arrow = new Page_Arrow();
            Left_Page_Arrow.loc = new Vector2(4, 32);
            Left_Page_Arrow.stereoscopic = Config.UNIT_ARROWS_DEPTH;
            Left_Page_Arrow.ArrowClicked += Left_Page_Arrow_ArrowClicked;
            Right_Page_Arrow = new Page_Arrow();
            Right_Page_Arrow.loc = new Vector2(Config.WINDOW_WIDTH - 4, 32);
            Right_Page_Arrow.mirrored = true;
            Right_Page_Arrow.stereoscopic = Config.UNIT_ARROWS_DEPTH;
            Right_Page_Arrow.ArrowClicked += Right_Page_Arrow_ArrowClicked;

            // Scroll
            Scroll = new IndexScrollComponent(
                new Vector2(Unit_Scissor_Rect.Width, Unit_Scissor_Rect.Height),
                new Vector2(Unit_Scissor_Rect.Width, Y_Per_Row),
                ScrollAxes.Vertical);
            Scroll.loc = new Vector2(Unit_Scissor_Rect.X, Unit_Scissor_Rect.Y);
            Scroll.Scrollbar = Scrollbar;
            Scroll.SetElementLengths(new Vector2(1, Team.Count));
            Scroll.SetBuffers(new Rectangle(1, 2, 2, 4));
            Scroll.SetResolveToIndex(true);

            HeaderNodes = new List<PartialRangeVisibleUINodeSet<UnitHeaderUINode>>();
            HeaderCursors = new List<UICursor<UnitHeaderUINode>>();
            foreach (var page in PageKeys)
            {
                List<UnitHeaderUINode> headerNodes = new List<UnitHeaderUINode>();
                // Name
                var nameNode = GetHeader(UnitScreenConfig.NAME_NODE,
                    48 + Data_Scissor_Rect.Width - Unit_Scissor_Rect.Width);
                headerNodes.Add(nameNode);

                // Page Headers
                foreach (var header in page)
                {
                    var config = this.DataSet[header.Index];
                    var node = GetHeader(config);
                    headerNodes.Add(node);
                }
                var nodeSet = new PartialRangeVisibleUINodeSet<UnitHeaderUINode>(headerNodes);
                var cursor = new UICursor<UnitHeaderUINode>(nodeSet);
                cursor.draw_offset = new Vector2(-15, 0);
                cursor.stereoscopic = Config.UNIT_HELP_DEPTH;
                cursor.move_to_target_loc();

                HeaderNodes.Add(nodeSet);
                HeaderCursors.Add(cursor);
            }

            CreateCancelButton();
            RefreshInputHelp();

            refresh_arrow_visibility();
            refresh_banner_text();
        }

        private UnitHeaderUINode GetHeader(UnitScreenData config, int offset = 0)
        {
            var text = new TextSprite(
                Config.UI_FONT, Global.Content, "Red",
                Vector2.Zero,
                config.Name);
            var node = new UnitHeaderUINode(
                string.Format("Unit Help {0}", config.Name),
                config);
            node.loc = new Vector2(
                Header_Scissor_Rect.X + config.Offset + offset,
                Header_Scissor_Rect.Y);
            return node;
        }

        private void CreateCancelButton()
        {
            CancelButton = Button_Description.button(Inputs.B,
                Config.WINDOW_WIDTH - 64);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = Config.UNIT_ARROWS_DEPTH;
        }

        protected void RefreshInputHelp()
        {
            AButton = Button_Description.button(Inputs.A,
                Config.WINDOW_WIDTH - 160);
            AButton.description = HeaderActive ? "Sort" : "Select";
            AButton.stereoscopic = Config.UNIT_ARROWS_DEPTH;

            RButton = Button_Description.button(Inputs.R,
                Config.WINDOW_WIDTH - 112);
            RButton.description = HeaderActive ? "Help" : "Info";
            RButton.stereoscopic = Config.UNIT_ARROWS_DEPTH;

            // Hide buttons that don't really do anything for touch
            AButton.Visible = Input.ControlScheme != ControlSchemes.Touch;
            RButton.Visible = Input.ControlScheme != ControlSchemes.Touch;
        }

        protected void set_images()
        {
            // Names
            List<UnitScreenUINode> nodes = new List<UnitScreenUINode>();
            for (int i = 0; i < Team.Count; i++)
            {
                Game_Unit unit = _unit(Team[i]);
                Game_Actor actor = unit.actor;
                int y = i * Y_Per_Row;

                Vector2 loc = new Vector2(24, y + Offset_Y) + new Vector2(Unit_Scissor_Rect.X, Unit_Scissor_Rect.Y);

                var node = new UnitScreenUINode(actor.name);
                node.Size = new Vector2(Unit_Scissor_Rect.Width - 32, Y_Per_Row);
                node.loc = loc;
                nodes.Add(node);
            }

            UnitNodes = new PartialRangeVisibleUINodeSet<UnitScreenUINode>(nodes);
            UnitNodes.CursorMoveSound = System_Sounds.Menu_Move1;
            // Unit Stats
            UnitData = new UnitScreenRow[PageKeys.Count][];
            for (int pageIndex = 0; pageIndex < PageKeys.Count; pageIndex++)
            {
                UnitData[pageIndex] = new UnitScreenRow[Team.Count];
            }
            // Map Sprites
            Map_Sprites.Clear();
            for (int i = 0; i < Team.Count; i++)
            {
                Game_Unit unit = _unit(Team[i]);
                Game_Actor actor = unit.actor;
                int y = i * Y_Per_Row;
                // Add map sprite
                Map_Sprites.Add(new Character_Sprite());
                Map_Sprites[i].facing_count = 3;
                Map_Sprites[i].frame_count = 3;
                Map_Sprites[i].loc = new Vector2(32, y + 16 + Offset_Y) + new Vector2(Unit_Scissor_Rect.X, Unit_Scissor_Rect.Y);
                Map_Sprites[i].stereoscopic = Config.UNIT_WINDOW_DEPTH;
            }

            for (int i = 0; i < Team.Count; i++)
            {
                RefreshRow(i);
            }

            Map_Sprite_Frame = -1;
            update_map_sprite();
            Offset.X = data_width * this.page;

            if (StoredActorId.IsSomething)
                this.ActorId = StoredActorId;
        }

        protected void RefreshRow(int index)
        {
            for (int pageIndex = 0; pageIndex < PageKeys.Count; pageIndex++)
            {
                var page = PageKeys[pageIndex]
                    .Select(x => this.DataSet[x.Index])
                    .ToArray();
                int[] configPages = PageKeys[pageIndex]
                    .Select(x => x.Page)
                    .ToArray();

                Game_Unit unit = _unit(Team[index]);
                int y = index * Y_Per_Row;

                UnitData[pageIndex][index] = new UnitScreenRow(page, configPages, unit);
                UnitData[pageIndex][index].loc =
                    new Vector2(data_width * pageIndex, y + Offset_Y) +
                    new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);
            }

            refresh_unit_name_and_sprite_texture(index);
        }
        protected void refresh_unit_name_and_sprite_texture(int index)
        {
            Game_Unit unit = _unit(Team[index]);
            Game_Actor actor = unit.actor;
            // Map Sprite
            if (Preparations)
                Map_Sprites[index].texture = Scene_Map.get_team_map_sprite(
                    PickUnits_Window.actor_deployed(actor.id) ? unit.team : 0, unit.map_sprite_name);
            else
                Map_Sprites[index].texture = Scene_Map.get_team_map_sprite(unit.ready ? unit.team : 0, unit.map_sprite_name);
            if (Map_Sprites[index].texture != null)
                Map_Sprites[index].offset = new Vector2(
                    (Map_Sprites[index].texture.Width / Map_Sprites[index].frame_count) / 2,
                    (Map_Sprites[index].texture.Height / Map_Sprites[index].facing_count) - 8);
            Map_Sprites[index].mirrored = unit.has_flipped_map_sprite;

            // Map Sprite
            bool readySprite;
            if (Preparations)
                readySprite = PickUnits_Window.actor_deployed(actor.id);
            else
                readySprite = unit.ready;
            UnitScreenUINode unitNode = UnitNodes[index];
            unitNode.set_map_sprite_texture(
                readySprite ? unit.team : 0,
                unit.map_sprite_name);
            // Name
            if (Preparations)
                unitNode.set_name_texture(prep_name_color(actor));
            else
                unitNode.set_name_texture("White");
        }

        protected string prep_name_color(Game_Actor actor)
        {
            int unit_id = Global.game_map.get_unit_id_from_actor(actor.id);
            // Green
            if (Global.game_map.forced_deployment.Contains(actor.id) ||
                    (unit_id > -1 && !Global.game_map.deployment_points.Contains(Global.game_map.units[unit_id].loc)))
                return "Green";
            // White
            if (PickUnits_Window.actor_deployed(actor.id))
                return "White";
            // Grey
            return "Grey";
        }

        protected void refresh_arrow_visibility()
        {
            Left_Page_Arrow.visible = this.page > 0;
            Right_Page_Arrow.visible = this.page < this.PageCount - 1;
        }

        protected virtual void refresh_banner_text()
        {
            int pageCount = this.DataSet
                .Select(x => x.Page)
                .Distinct()
                .Count();

            Banner_Text.src_rect = new Rectangle(0, 16 * Math.Min(this.page + 1, pageCount), 96, 16);
            Page_Number.text = (this.page + 1).ToString();
        }
        #endregion

        #region Sorting
        protected void refresh_sort()
        {
            refresh_sort(sort);
        }
        protected void refresh_sort(int new_sort)
        {
            refresh_sort(new_sort, sort_up);
        }
        protected void refresh_sort(int new_sort, bool sort_dir_up)
        {
            int old_sort = sort;
            sort = new_sort;
            sort_up = sort_dir_up;
            List<int> old_team = new List<int>();
            old_team.AddRange(Team);
            if (old_sort == new_sort && sort_dir_up)
                old_team.Reverse();
            Team.Sort(delegate(int a, int b)
            {
                Game_Unit unit_a = _unit(a);
                Game_Unit unit_b = _unit(b);
                int value = sort_switch(unit_a, unit_b, new_sort);
                if (value == 0)
                    return old_team.IndexOf(a) - old_team.IndexOf(b);
                else
                    return value;
            });
            if (!sort_up)
            {
                Team.Reverse();
            }
            set_images();

            refresh_sort_images();
        }

        protected int sort_switch(Game_Unit unit_a, Game_Unit unit_b, int new_sort)
        {
            int value;
            // Name
            if (new_sort == 0)
            {
                if (Preparations)
                {
                    if (PickUnits_Window != null)
                    {
                        bool deployed_a = PickUnits_Window.actor_deployed(unit_a.actor.id);
                        bool deployed_b = PickUnits_Window.actor_deployed(unit_b.actor.id);
                        if (deployed_a != deployed_b)
                            return deployed_a ? -1 : 1;
                    }
                }
                else
                {
                    if (unit_a.ready && !unit_b.ready)
                        return -1;
                    if (unit_b.ready && !unit_a.ready)
                        return 1;
                }
                if (Global.battalion != null &&
                        Global.battalion.actors.Contains(unit_a.actor.id) &&
                        Global.battalion.actors.Contains(unit_b.actor.id))
                    value = Global.battalion.actors.IndexOf(unit_a.actor.id) -
                        Global.battalion.actors.IndexOf(unit_b.actor.id);
                else
                    value = unit_a.actor.id - unit_b.actor.id;
            }
            else
            {
                UnitScreenData config = this.DataSet[new_sort - 1];
                return config.GetSort(unit_a, unit_b);
            }

            return value;
        }

        protected virtual void refresh_sort_images()
        {
            var data = UnitScreenConfig.NAME_NODE;
            if (this.sort > 0)
                data = this.DataSet[this.sort - 1];

            if (data.WeaponIcon >= 1)
            {
                Weapon_Type_Icon sort_label_icon = new Weapon_Type_Icon();
                sort_label_icon.index = data.WeaponIcon;
                Sort_Label_Text = sort_label_icon;
            }
            else
            {
                TextSprite sort_label_text = new TextSprite();
                sort_label_text.SetFont(Config.UI_FONT, Global.Content, "White");
                sort_label_text.text = data.Name;
                Sort_Label_Text = sort_label_text;
            }
            Sort_Label_Text.loc = new Vector2(Config.WINDOW_WIDTH - 84, 8);
            Sort_Label_Text.stereoscopic = Config.UNIT_SORT_DEPTH;
            Sort_Arrow.src_rect = new Rectangle(sort_up ? 0 : 8, 0, 8, 16);
        }

        protected void reverse_sort()
        {
            var data = UnitScreenConfig.NAME_NODE;
            if (this.sort > 0)
                data = this.DataSet[this.sort - 1];

            if (data.NoSort)
                refresh_sort(sort);
            else
            {
                Team.Reverse();
                set_images();
                refresh_sort_images();
            }
        }
        #endregion

        #region Update
        protected void update_map_sprite()
        {
            int old_frame = Map_Sprite_Frame;
            Map_Sprite_Frame = Global.game_system.unit_anim_idle_frame;
            if (Map_Sprite_Frame != old_frame)
                foreach (Character_Sprite sprite in Map_Sprites)
                    sprite.frame = Map_Sprite_Frame;
        }

        protected void update_sort_arrow()
        {
            switch (Sort_Arrow_Timer / 4)
            {
                case 0:
                    Sort_Arrow.offset.Y = 0;
                    break;
                case 1:
                    Sort_Arrow.offset.Y = 1;
                    break;
                case 2:
                    Sort_Arrow.offset.Y = 0;
                    break;
                case 3:
                    Sort_Arrow.offset.Y = -1;
                    break;
            }
            Sort_Arrow_Timer = (Sort_Arrow_Timer + 1) % SORT_ARROW_TIME;
        }

        protected void refresh_cursor_location()
        {
            int target_y = Y_Per_Row * _Scroll;
            Offset.Y = target_y;
        }

        private void UpdateScroll(bool active)
        {
            if (UnitNodes == null)
                return;

            Scroll.Update(active, UnitNodes.ActiveNodeIndex, Vector2.Zero);
            if (!_Closing)
            {
                if (Scroll.Index >= 0 && Scroll.Index < UnitNodes.Count)
                    UnitNodes.set_active_node(UnitNodes[Scroll.Index]);
            }
            if (Scrollbar != null)
                Scrollbar.scroll = (int)Scroll.IntOffset.Y / Y_Per_Row;
        }

        protected override void UpdateMenu(bool active)
        {
            update_direction();
            update_map_sprite();
            update_sort_arrow();
            Line_Cursor.update();

            if (Scrollbar != null)
            {
                Scrollbar.update();
                if (active)
                    Scrollbar.update_input();
            }

            // Cancel button
            CancelButton.Update(active && this.ready_for_inputs);
            AButton.Update();
            RButton.Update();

            base.UpdateMenu(active);
            UpdateScroll(active && this.ready_for_inputs);
            RefreshCursor();

            Left_Page_Arrow.update();
            Right_Page_Arrow.update();

            if (this.IsHelpActive)
                Help_Window.update();
        }

        protected override void UpdateAncillary()
        {
            if (Input.ControlScheme == ControlSchemes.Touch)
                SetHeaderActive(true);

            if (Input.ControlSchemeSwitched)
            {
                if (Input.ControlScheme == ControlSchemes.Mouse)
                    SetHeaderActive(false);

                if (CancelButton != null)
                    CreateCancelButton();

                RefreshInputHelp();
            }
        }

        protected override void update_input(bool active)
        {
            bool input = active && this.ready_for_inputs;
            if (input)
            {
                Left_Page_Arrow.UpdateInput();
                Right_Page_Arrow.UpdateInput();
            }

            input = active && this.ready_for_inputs;
            if (input)
            {
                // Change page
                if ((!HeaderActive && Global.Input.pressed(Inputs.Left)) ||
                    Global.Input.gesture_triggered(TouchGestures.SwipeRight))
                {
                    page_left();
                    input = false;
                }
                else if ((!HeaderActive && Global.Input.pressed(Inputs.Right)) ||
                    Global.Input.gesture_triggered(TouchGestures.SwipeLeft))
                {
                    page_right();
                    input = false;
                }
                else if (HeaderActive &&
                    HeaderNodes[this.page].ActiveNodeIndex == 0 &&
                    Global.Input.repeated(Inputs.Left))
                {
                    page_left();
                    input = false;
                }
                else if (HeaderActive &&
                    HeaderNodes[this.page].ActiveNodeIndex == HeaderNodes[this.page].Count - 1 &&
                    Global.Input.repeated(Inputs.Right))
                {
                    page_right();
                    input = false;
                }
                // Switch between header and rows
                else if (!HeaderActive && UnitNodes.ActiveNodeIndex == 0 &&
                    Global.Input.triggered(Inputs.Up))
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move1);
                    HeaderCursors[this.page].move_to_target_loc();
                    SetHeaderActive(true);
                    input = false;
                }
                else if (HeaderActive && !this.IsHelpActive &&
                    Global.Input.triggered(Inputs.Down))
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move1);
                    SetHeaderActive(false);
                    input = false;
                }
            }

            // Update nodes
            for (int i = 0; i < HeaderNodes.Count; i++)
            {
                ControlSet control = ControlSet.None;
                if (input && i == this.page)
                {
                    if (this.IsHelpActive)
                        control = ControlSet.Movement;
                    else
                        control = HeaderActive ? ControlSet.All : ControlSet.Pointing;
                }

                int index = HeaderNodes[i].ActiveNodeIndex;
                HeaderNodes[i].Update(control);
                if (index != HeaderNodes[i].ActiveNodeIndex && this.IsHelpActive)
                    RefreshHelp();
            }
            if (UnitNodes != null)
            {
                ControlSet control = ControlSet.None;
                if (input && !this.IsHelpActive)
                    control = !HeaderActive ? ControlSet.All : ControlSet.Pointing;

                UnitNodes.Update(control, VisibleIndexesRange(false).Enumerate(), Scroll.offset);
            }

            if (input)
            {
                if (this.IsHelpActive)
                {
                    if (Global.Input.triggered(Inputs.B) ||
                            Global.Input.triggered(Inputs.R) ||
                            Global.Input.mouse_click(MouseButtons.Right) ||
                            Global.Input.gesture_triggered(TouchGestures.LongPress) ||
                            CancelButton.consume_trigger(MouseButtons.Left) ||
                            CancelButton.consume_trigger(TouchGestures.Tap))
                    {
                        close_help();
                    }
                }
                else
                {
                    // Sort Columns
                    var sortIndex = HeaderNodes[this.page].consume_triggered(
                        Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                    if (sortIndex.IsSomething)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        int key = 0;
                        if (sortIndex > 0)
                            key = PageKeys[this.page][sortIndex - 1].Index + 1;
                        if (this.sort == key)
                        {
                            sort_up = !sort_up;
                            reverse_sort();
                        }
                        else
                        {
                            refresh_sort(key, true);
                        }
                    }

                    // Open Header Help
                    var helpIndex = HeaderNodes[this.page].consume_triggered(
                        Inputs.R, MouseButtons.Right, TouchGestures.LongPress);
                    if (helpIndex.IsSomething)
                    {
                        open_help();
                    }

                    // Open Status Screen
                    var statusIndex = UnitNodes.consume_triggered(
                        Inputs.R, MouseButtons.Right, TouchGestures.LongPress);
                    if (statusIndex.IsSomething)
                    {
                        if (Status != null)
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            this.unit.status();
                            OnStatus(new EventArgs());
                        }
                    }

                    // Jump to Unit
                    var selectionIndex = UnitNodes.consume_triggered(
                        Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                    if (selectionIndex.IsSomething)
                    {
                        UnitSelect();
                    }

                    if (sortIndex.IsNothing && helpIndex.IsNothing &&
                        statusIndex.IsNothing && selectionIndex.IsNothing)
                    {
                        if (Global.Input.triggered(Inputs.B) ||
                            CancelButton.consume_trigger(MouseButtons.Left) ||
                            CancelButton.consume_trigger(TouchGestures.Tap))
                        {
                            if (Preparations && PickUnits_Window != null)
                                PickUnits_Window.actor_id = HeaderActive ? Global.game_map.units[Team[0]].actor.id : unit.actor.id;
                            Unit_Selected = false;
                            Global.game_system.play_se(System_Sounds.Cancel);
                            close();
                        }
                    }
                }
            }

            HeaderCursors[this.page].update();
        }

        protected virtual void UnitSelect()
        {
            if (Preparations)
            {
                if (PickUnits_Window.switch_unit(unit.actor))
                    refresh_unit_name_and_sprite_texture(UnitNodes.ActiveNodeIndex);
            }
            else
            {
                Unit_Selected = true;
                Global.game_system.play_se(System_Sounds.Confirm);
                close();
            }
        }

        private void RefreshCursor()
        {
            if (UnitNodes == null)
                return;

            foreach (var cursor in HeaderCursors)
                cursor.visible = HeaderActive;
            Line_Cursor.visible = !HeaderActive;

            Line_Cursor.loc = new Vector2(
                Line_Cursor.loc.X,
                8 + BASE_Y + Offset_Y + UnitNodes.ActiveNodeIndex * Y_Per_Row);
            Line_Cursor.loc.Y -= Scroll.Target.Y * Y_Per_Row;
        }

        private void SetHeaderActive(bool value)
        {
            HeaderActive = value;
            RefreshInputHelp();
        }

        private TactileLibrary.IntRange VisibleIndexesRange(bool skipFirstLine = true)
        {
            int scrollOffset = (int)Scroll.IntOffset.Y / Y_Per_Row;
            int min = scrollOffset;
            if (skipFirstLine)
                min += (Scroll.IsScrolling ? 0 : 1);
            int max = scrollOffset + Rows_At_Once + (Scroll.IsScrolling ? 0 : -1);
            return new TactileLibrary.IntRange(min, max);
        }

        private void Left_Page_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            page_left();
        }
        private void Right_Page_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            page_right();
        }

        public event EventHandler<EventArgs> Status;
        protected void OnStatus(EventArgs e)
        {
            if (Status != null)
                Status(this, e);
        }

        protected override void black_screen_switch()
        {
            Visible = !Visible;
            if (!_Closing)
            {
                refresh_sort();
                if (Preparations && PickUnits_Window != null)
                    for (int i = 0; i < Team.Count; i++)
                        if (Global.game_map.units[Team[i]].actor.id == PickUnits_Window.actor_id)
                        {
                            unit_index = Team[i];
                            break;
                        }
                update_map_sprite();
            }
        }

        protected void update_direction()
        {
            if (changing_page)
            {
                // Spin the page arrow in the direction moved
                (Direction == 4 ? Left_Page_Arrow : Right_Page_Arrow).twirling_update();

                switch (Delay)
                {
                    case 16:
                        Global.game_system.play_se(System_Sounds.Status_Page_Change);
                        break;
                    case 13:
                        Offset.X += (Direction == 4 ? -1 : 1) * 16 * 2;
                        break;
                    case 12:
                        Offset.X += (Direction == 4 ? -1 : 1) * 16 * 3;
                        break;
                    case 11:
                        Offset.X += (Direction == 4 ? -1 : 1) * 16 * 1;
                        break;
                    case 10:
                        Offset.X += (Direction == 4 ? -1 : 1) * 16 * 1;
                        break;
                    case 9:
                        this.page = this.page + (Direction == 4 ? -1 : 1);
                        refresh_arrow_visibility();
                        refresh_banner_text();
                        Offset.X += (Direction == 4 ? -1 : 1) * 16 * 1;
                        break;
                    case 8:
                        Offset.X += (Direction == 4 ? -1 : 1) * 16 * 1;
                        break;
                    case 7:
                        Offset.X += (Direction == 4 ? -1 : 1) * 16 * 1;
                        break;
                    case 6:
                        Offset.X += (Direction == 4 ? -1 : 1) * 16 * 1;
                        break;
                    case 5:
                        Offset.X += (Direction == 4 ? -1 : 1) * 16 * 1;
                        break;
                    case 4:
                        Offset.X += (Direction == 4 ? -1 : 1) * 16 * 1;
                        break;
                    case 3:
                        Offset.X += (Direction == 4 ? -1 : 1) * 8;
                        break;
                    case 2:
                        Offset.X += (Direction == 4 ? -1 : 1) * 8;
                        break;
                    case 1:
                        Offset.X += 0;
                        Direction = 0;
                        break;
                }
            }
            if (Delay > 0) Delay--;
        }
        #endregion

        #region Movement
        protected void refresh_scroll()
        {
            Scroll.FixScroll(UnitNodes.ActiveNodeIndex);
            if (Scrollbar != null)
                Scrollbar.scroll = (int)Scroll.IntOffset.Y / Y_Per_Row;

            RefreshCursor();
        }

        protected void page_left()
        {
            if (this.page > 0)
            {
                Delay = 16;
                Direction = 4;
                fix_cursor_page_change();
            }
        }
        protected void page_right()
        {
            if (this.page + 1 < this.PageCount)
            {
                Delay = 16;
                Direction = 6;
                fix_cursor_page_change();
            }
        }

        protected void fix_cursor_page_change()
        {
            // Jump the cursor to a node on the next page
            if (HeaderActive)
            {
                int nextPageIndex = this.page + (Direction == 4 ? -1 : 1);
                var nextPage = HeaderNodes[nextPageIndex];
                HeaderCursors[nextPageIndex].force_loc(HeaderCursors[this.page].loc);
                if (Direction == 4 ^ Input.ControlScheme == ControlSchemes.Mouse)
                    nextPage.set_active_node(nextPage.Last());
                else
                    nextPage.set_active_node(nextPage[0]);
            }


            int temp_page = this.page;
            this.page += (Direction == 4 ? -1 : 1);
            if (this.IsHelpActive)
            {
                RefreshHelp();
            }
            this.page = temp_page;
        }
        #endregion

        #region Help
        public bool IsHelpActive { get { return Help_Window != null; } }

        public void open_help()
        {
            SetHeaderActive(true);

            Help_Window = new Window_Help();
            Help_Window.stereoscopic = Config.UNIT_HELP_DEPTH;
            update_help_info();
            Help_Window.loc = HeaderNodes[this.page].ActiveNode.loc;
            update_help_loc();
            Global.game_system.play_se(System_Sounds.Help_Open);
        }

        public virtual void close_help()
        {
            if (Input.ControlScheme == ControlSchemes.Mouse)
                SetHeaderActive(false);

            Help_Window = null;
            Global.game_system.play_se(System_Sounds.Help_Close);
        }

        protected void RefreshHelp()
        {
            update_help_info();
            update_help_loc();
        }

        protected virtual void update_help_loc()
        {
            Help_Window.set_loc(HeaderNodes[this.page].ActiveNode.loc);
        }

        protected virtual void update_help_info()
        {
            string helpText = "";
            if (Global.system_text.ContainsKey(HeaderNodes[this.page].ActiveNode.HelpLabel))
                helpText = Global.system_text[HeaderNodes[this.page].ActiveNode.HelpLabel];
            Help_Window.set_text(helpText);
        }
        #endregion

        #region Draw
        protected override void draw_window(SpriteBatch sprite_batch)
        {
            Vector2 unit_offset = new Vector2(0, Offset.Y);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Banner
            Banner.draw(sprite_batch);
            Banner_Text.draw(sprite_batch);
            // Windows
            Background_Window.draw(sprite_batch);
            Sort_Window.draw(sprite_batch);
            if (Show_Page_Number)
                Page_Window.draw(sprite_batch);
            // Page Arrows
            Left_Page_Arrow.draw(sprite_batch);
            Right_Page_Arrow.draw(sprite_batch);
            // Sort/Page Number
            Sort_Label_Text.draw(sprite_batch);
            if (Show_Page_Number)
            {
                Page_Number_Slash.draw(sprite_batch);
                Page_Number.draw(sprite_batch);
                Max_Page_Number.draw(sprite_batch);
            }
            Sort_Arrow.draw(sprite_batch);
            Sort_Label.draw(sprite_batch);
            // Cursor
            Line_Cursor.draw(sprite_batch);
            // Scroll Bar
            if (Scrollbar != null)
                Scrollbar.draw(sprite_batch);
            sprite_batch.End();


            // Map Sprites
            DrawUnits(sprite_batch);

            // Data
            DrawData(sprite_batch);

            // Header
            DrawHeaders(sprite_batch);

            Rectangle header_scissor_rect = new Rectangle(Header_Scissor_Rect.X +
                    (int)Stereoscopic_Graphic_Object.graphic_draw_offset(Config.UNIT_WINDOW_DEPTH).X,
                Header_Scissor_Rect.Y, Header_Scissor_Rect.Width, Header_Scissor_Rect.Height);
            sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(header_scissor_rect);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            Vector2 header_offset = new Vector2(Offset.X, 0);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            CancelButton.Draw(sprite_batch);
            AButton.Draw(sprite_batch);
            RButton.Draw(sprite_batch);
            // Cursor
            if (HeaderActive)
            {
                HeaderCursors[this.page].draw(sprite_batch);
            }
            sprite_batch.End();
            // Help Window
            if (this.IsHelpActive)
                Help_Window.draw(sprite_batch);
        }

        private void DrawUnits(SpriteBatch sprite_batch)
        {
            // Map sprite (first row, if not scrolling)
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            int scrollOffset = (int)Scroll.IntOffset.Y / Y_Per_Row;
            if (scrollOffset < UnitNodes.Count() && !Scroll.IsScrolling)
            {
                UnitNodes.Draw(sprite_batch,
                    Enumerable.Range(scrollOffset, 1),
                    Scroll.IntOffset);
            }
            sprite_batch.End();

            // Draw other units
            Rectangle unit_scissor_rect = new Rectangle(Unit_Scissor_Rect.X +
                    (int)Stereoscopic_Graphic_Object.graphic_draw_offset(Config.UNIT_WINDOW_DEPTH).X,
                Unit_Scissor_Rect.Y, Unit_Scissor_Rect.Width, Unit_Scissor_Rect.Height);
            sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(unit_scissor_rect);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);

            var range = VisibleIndexesRange();
            UnitNodes.Draw(sprite_batch,
                range.Enumerate(),
                Scroll.IntOffset);

            sprite_batch.End();
        }

        private void DrawData(SpriteBatch sprite_batch)
        {
            Rectangle data_scissor_rect = new Rectangle(Data_Scissor_Rect.X +
                    (int)Stereoscopic_Graphic_Object.graphic_draw_offset(Config.UNIT_WINDOW_DEPTH).X,
                Data_Scissor_Rect.Y, Data_Scissor_Rect.Width, Data_Scissor_Rect.Height);
            sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(data_scissor_rect);

            var range = Enumerable.Range(this.page, 1);
            if (Delay > 0)
                range = Enumerable.Range(this.page - 1, 3);
            foreach (int i in range)
            {
                if (i < 0 || i >= UnitData.Length)
                    continue;

                foreach (var data in UnitData[i])
                    data.Draw(sprite_batch, Scissor_State, Offset + Scroll.IntOffset);
            }
        }

        private void DrawHeaders(SpriteBatch sprite_batch)
        {
            // Name
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            HeaderNodes[Math.Min(this.page, HeaderNodes.Count - 1)].Draw(sprite_batch,
                Enumerable.Range(0, 1));
            sprite_batch.End();

            // Page Headers
            Rectangle header_scissor_rect = new Rectangle(Header_Scissor_Rect.X +
                    (int)Stereoscopic_Graphic_Object.graphic_draw_offset(Config.UNIT_WINDOW_DEPTH).X,
                Header_Scissor_Rect.Y, Header_Scissor_Rect.Width, Header_Scissor_Rect.Height);
            sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(header_scissor_rect);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);

            var range = Enumerable.Range(this.page, 1);
            if (Delay > 0)
                range = Enumerable.Range(this.page - 1, 3);
            foreach (int i in range)
            {
                if (i < 0 || i >= HeaderNodes.Count)
                    continue;
                var headerSet = HeaderNodes[i];
                Vector2 header_offset = new Vector2(Offset.X - Header_Scissor_Rect.Width * i, 0);

                headerSet.Draw(sprite_batch,
                    Enumerable.Range(1, headerSet.Count - 1),
                    header_offset);
            }

            sprite_batch.End();
        }
        #endregion
    }
}
