using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using TactileStringExtension;

namespace Tactile.Windows.Map
{
    enum Unit_Sort { Name,
        Class, Lvl, Exp, Hp, MaxHp, Affin, Cond,
        Pow, Skl, Spd, Lck, Def, Res, Mov, Con, Aid,
        Equip, Skills,
        Atk, Hit, Avo, Crt, Dod, AS, Rng,
        Sword, Lance, Axe, Bow, Fire, Thund, Wind, Light, Elder, Staff,
        Ally,
        Anim }
    internal class Window_Unit : Map_Window_Base
    {
        protected const int BASE_Y = 56;
        const int BASE_PAGES = 6;
        public const int SUPPORTS_PER_PAGE = 4;
        const int Y_PER_ROW = 16;
        const int ROWS_AT_ONCE = (Config.WINDOW_HEIGHT - 72) / Y_PER_ROW;
        const int OFFSET_Y = -(16 - Y_PER_ROW) / 2;
        readonly static int[] COLUMNS = new int[] { 8, 10, 3, 8, 11, 2 };
        const int SORT_ARROW_TIME = 32;
        readonly static Unit_Sort[] PAGE_SORT_START = new Unit_Sort[] {
            Unit_Sort.Class, Unit_Sort.Pow, Unit_Sort.Equip, Unit_Sort.Atk, Unit_Sort.Sword, Unit_Sort.Ally };
        readonly static KeyValuePair<int, string>[][] HEADERS = new KeyValuePair<int, string>[][] {
            new KeyValuePair<int, string>[]{
                new KeyValuePair<int, string>(0, "Class"), new KeyValuePair<int, string>(64+8, "Lvl"), new KeyValuePair<int, string>(88+4, "Exp"),
                new KeyValuePair<int, string>(112, "HP"), new KeyValuePair<int, string>(132, "Max"), new KeyValuePair<int, string>(155, "Affin"),
                new KeyValuePair<int, string>(188, "Cond")},
            new KeyValuePair<int, string>[]{
                new KeyValuePair<int, string>(0, "Pow"), new KeyValuePair<int, string>(28, "Skl"), new KeyValuePair<int, string>(48, "Spd"),
                new KeyValuePair<int, string>(72, "Luck"), new KeyValuePair<int, string>(96, "Def"), new KeyValuePair<int, string>(120, "Res"),
                new KeyValuePair<int, string>(148, "Move"), new KeyValuePair<int, string>(178, "Con"), new KeyValuePair<int, string>(204, "Aid")},
            new KeyValuePair<int, string>[]{
                new KeyValuePair<int, string>(16, "Equip"), new KeyValuePair<int, string>(104, "Skills")},
            new KeyValuePair<int, string>[]{
                new KeyValuePair<int, string>(8, "Atk"), new KeyValuePair<int, string>(40, "Hit"), new KeyValuePair<int, string>(64, "Avoid"),
                new KeyValuePair<int, string>(104, "Crit"), new KeyValuePair<int, string>(128, "Dod"), new KeyValuePair<int, string>(160, "AS"),
                new KeyValuePair<int, string>(196, "Rng")},
            new KeyValuePair<int, string>[]{
                new KeyValuePair<int, string>(0 * 20 + 8, "WType1"), new KeyValuePair<int, string>(1 * 20 + 8, "WType2"),
                new KeyValuePair<int, string>(2 * 20 + 8, "WType3"), new KeyValuePair<int, string>(3 * 20 + 8, "WType4"),
                new KeyValuePair<int, string>(4 * 20 + 8, "WType5"), new KeyValuePair<int, string>(5 * 20 + 8, "WType6"),
                new KeyValuePair<int, string>(6 * 20 + 8, "WType7"), new KeyValuePair<int, string>(7 * 20 + 8, "WType8"),
                new KeyValuePair<int, string>(8 * 20 + 8, "WType9"), new KeyValuePair<int, string>(9 * 20 + 8, "WType10")},
            new KeyValuePair<int, string>[]{
                new KeyValuePair<int, string>(8, "Ally")}
        };
        readonly static string[] SORT_NAMES = new string[] { "Name", "Class", "Lvl", "Exp", "HP", "Max", "Affin", "Cond",
            "Pow", "Skl", "Spd", "Luck", "Def", "Res", "Move", "Con", "Aid", "Equip", "Skills",
            "Atk", "Hit", "Avoid", "Crit", "Dod", "AS", "Rng",
            "WType1", "WType2", "WType3", "WType4", "WType5", "WType6", "WType7", "WType8", "WType9", "WType10",
            "Ally", "Anim" };

        protected int Y_Per_Row;
        protected int Rows_At_Once;
        protected int Offset_Y;

        protected int Row = 1, Column_Max, Column = 0;
        protected List<int> Team;
        protected int Pages;
        protected int Delay = 0, Direction = 0, Sort_Arrow_Timer = 0;
        protected int Scroll = 0;
        protected Vector2 Offset = Vector2.Zero;
        protected bool Show_Page_Number = true;
        protected bool Preparations;
        protected bool Unit_Selected = false;
        protected MenuScreenBanner Banner;
        protected Sprite Banner_Text;
        protected SystemWindowHeadered Background_Window;
        protected TextSprite Name_Header;
        protected List<Sprite> Headers = new List<Sprite>();
        protected System_Color_Window Sort_Window, Page_Window;
        protected List<Character_Sprite> Map_Sprites = new List<Character_Sprite>();
        protected List<TextSprite> Names = new List<TextSprite>();
        protected Sprite Sort_Label_Text, Sort_Label, Sort_Arrow;
        protected TextSprite Page_Number, Max_Page_Number, Page_Number_Slash;
        protected List<List<Unit_Page>> Page_Imgs = new List<List<Unit_Page>>();
        protected Unit_Line_Cursor Line_Cursor;
        protected Scroll_Bar Scrollbar;
        protected Page_Arrow Left_Page_Arrow, Right_Page_Arrow;
        protected Window_Prep_PickUnits PickUnits_Window;

        protected Rectangle Unit_Scissor_Rect, Header_Scissor_Rect, Data_Scissor_Rect;
        protected RasterizerState Scissor_State = new RasterizerState { ScissorTestEnable = true };

        #region Accessors
        protected bool changing_page { get { return Direction == 4 || Direction == 6; } }

        public bool unit_selected { get { return Unit_Selected; } }

        public int unit_index
        {
            set
            {
                Row = Team.IndexOf(value) + 1;
                if ((Row > (Rows_At_Once - 1) + Scroll && Scroll < row_max - (Rows_At_Once + 1)))
                {
                    while (Row > (Rows_At_Once - 1) + Scroll && Scroll < row_max - (Rows_At_Once + 1))
                        Scroll++;
                }
                else
                {
                    while (Row - 1 < Scroll + 1 && Scroll > 0)
                        Scroll--;
                }
                refresh_scroll();
                refresh_cursor_location();
            }
            get { return Team[Row - 1]; }
        }

        public List<int> team { get { return new List<int>(Team); } }

        protected int page
        {
            get { return _get_page(); }
            set { _set_page(value); }
        }
        protected int sort
        {
            get { return Global.game_system.Unit_Sort; }
            set { Global.game_system.Unit_Sort = value; }
        }
        protected bool sort_up
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
                var actors = Team.Select(x => _unit(x).actor.id).ToList();
                int index = actors.IndexOf(value);
                if (index > -1)
                {
                    while (index + 1 > Row)
                        move_down();
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
            if (Row == 0)
                return null;
            return _unit(Team[Row - 1]);
        }
        protected virtual Game_Unit _unit(int id)
        {
            return Global.game_map.units[id];
        }
        #endregion

        public Window_Unit()
        {
            initialize();
        }

        #region Initialization
        protected virtual void initialize()
        {
            Y_Per_Row = Y_PER_ROW;
            Rows_At_Once = ROWS_AT_ONCE;
            Offset_Y = OFFSET_Y;
            Unit_Scissor_Rect = new Rectangle(-12, BASE_Y, 80, ROWS_AT_ONCE * Y_PER_ROW);
            Header_Scissor_Rect = new Rectangle(76, BASE_Y - 16, data_width, 16);
            Data_Scissor_Rect = new Rectangle(76, BASE_Y, data_width, ROWS_AT_ONCE * Y_PER_ROW);

            Preparations = Global.game_system.preparations && !Global.game_system.home_base;
            Team = determine_team();
            Pages = determine_page_count();
            page = Math.Min(page, Pages - 1);
            Column_Max = COLUMNS[Math.Min(page, BASE_PAGES - 1)];
            initialize_sprites();
            update_black_screen();
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

        protected int determine_page_count()
        {
            int support_pages = 0;
            foreach (int id in Team)
            {
                Game_Unit unit = _unit(id);
                // if event has more than SUPPORTS_PER_PAGE available support partners
                support_pages = (int)Math.Max(support_pages,
                    Math.Ceiling(unit.actor.ready_supports().Count / (float)SUPPORTS_PER_PAGE) - 1);
            }
            return BASE_PAGES + support_pages;
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
            // Header
            Name_Header = new TextSprite();
            Name_Header.loc = new Vector2(40 + Unit_Scissor_Rect.X, Unit_Scissor_Rect.Y - 16);
            Name_Header.SetFont(Config.UI_FONT, Global.Content, "White");
            Name_Header.text = "Name";
            Name_Header.stereoscopic = Config.UNIT_WINDOW_DEPTH;
            set_headers();
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
            Max_Page_Number.text = Pages.ToString();
            Max_Page_Number.stereoscopic = Config.UNIT_SORT_DEPTH;
            // Cursor
            Cursor = new Hand_Cursor();
            Cursor.visible = false;
            Cursor.loc = cursor_loc();
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
            Right_Page_Arrow = new Page_Arrow();
            Right_Page_Arrow.loc = new Vector2(Config.WINDOW_WIDTH - 4, 32);
            Right_Page_Arrow.mirrored = true;
            Right_Page_Arrow.stereoscopic = Config.UNIT_ARROWS_DEPTH;

            refresh_arrow_visibility();
            refresh_banner_text();
        }

        protected virtual void set_headers()
        {
            for (int i = 0; i < Pages; i++)
            {
                KeyValuePair<int, string>[] header_page = HEADERS[Math.Min(i, BASE_PAGES - 1)];
                foreach (KeyValuePair<int, string> header in header_page)
                {
                    if (header.Value.substring(0, 5) == "WType")
                    {
                        Headers.Add(new Weapon_Type_Icon());
                        ((Weapon_Type_Icon)Headers[Headers.Count - 1]).loc = new Vector2(header.Key + i * data_width + Header_Scissor_Rect.X, Header_Scissor_Rect.Y);
                        ((Weapon_Type_Icon)Headers[Headers.Count - 1]).index = Convert.ToInt32(header.Value.substring(5, header.Value.Length - 5));
                    }
                    else
                    {
                        Headers.Add(new TextSprite());
                        ((TextSprite)Headers[Headers.Count - 1]).loc = new Vector2(header.Key + i * data_width + Header_Scissor_Rect.X, Header_Scissor_Rect.Y);
                        ((TextSprite)Headers[Headers.Count - 1]).SetFont(Config.UI_FONT, Global.Content, "White");
                        ((TextSprite)Headers[Headers.Count - 1]).text = header.Value;
                    }
                    Headers[Headers.Count - 1].stereoscopic = Config.UNIT_WINDOW_DEPTH;
                }
            }
            // do pages beyond 6 //Yeti
        }

        protected void set_images()
        {
            Names.Clear();
            Map_Sprites.Clear();
            Page_Imgs.Clear();
            for(int i = 0; i < Pages; i++)
                Page_Imgs.Add(new List<Unit_Page>());
            set_pages();
            for (int i = 0; i < Page_Imgs.Count; i++)
                for (int j = 0; j < Page_Imgs[i].Count; j++)
                    Page_Imgs[i][j].stereoscopic = Config.UNIT_WINDOW_DEPTH;
            Map_Sprite_Frame = -1;
            update_map_sprite();
            Offset.X = data_width * page;
        }

        protected virtual void set_pages()
        {
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
                // Name
                Names.Add(new TextSprite());
                Names[i].loc = new Vector2(40, y + Offset_Y) + new Vector2(Unit_Scissor_Rect.X, Unit_Scissor_Rect.Y);
                Names[i].SetFont(Config.UI_FONT);
                Names[i].text = unit.actor.name;
                Names[i].stereoscopic = Config.UNIT_WINDOW_DEPTH;

                refresh_unit_name_and_sprite_texture(i);
                // Page 1
                Page_Imgs[0].Add(new Unit_Page_1(unit));
                Page_Imgs[0][i].loc = new Vector2(data_width * 0, y + Offset_Y) + new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);
                // Page 2
                Page_Imgs[1].Add(new Unit_Page_2(unit));
                Page_Imgs[1][i].loc = new Vector2(data_width * 1, y + Offset_Y) + new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);
                // Page 3
                Page_Imgs[2].Add(new Unit_Page_3(unit));
                Page_Imgs[2][i].loc = new Vector2(data_width * 2, y + Offset_Y) + new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);
                // Page 4
                Page_Imgs[3].Add(new Unit_Page_4(unit));
                Page_Imgs[3][i].loc = new Vector2(data_width * 3, y + Offset_Y) + new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);
                // Page 5
                Page_Imgs[4].Add(new Unit_Page_5(unit));
                Page_Imgs[4][i].loc = new Vector2(data_width * 4, y + Offset_Y) + new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);
                // Page 6
                for (int j = BASE_PAGES - 1; j < Pages; j++)
                {
                    Page_Imgs[j].Add(new Unit_Page_6(unit, ((j + 1) - BASE_PAGES) * SUPPORTS_PER_PAGE));
                    Page_Imgs[j][i].loc = new Vector2(data_width * j, y + Offset_Y) + new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);
                }
            }
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
            // Name
            if (Preparations)
                Names[index].SetColor(Global.Content, prep_name_color(actor));
            else
                Names[index].SetColor(Global.Content, "White");
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
            Left_Page_Arrow.visible = page > 0;
            Right_Page_Arrow.visible = page < Pages - 1;
        }

        protected virtual void refresh_banner_text()
        {
            Banner_Text.src_rect = new Rectangle(0, 16 * Math.Min(page + 1, BASE_PAGES), 96, 16);
            Page_Number.text = (page + 1).ToString();
        }
        #endregion

        #region Sorting
        protected virtual int sort_column()
        {
            if (Column == 0)
                return 0;
            return Column - 1 + (int)PAGE_SORT_START[Math.Min(page, BASE_PAGES - 1)];
        }

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
            var stats_a = new Calculations.Stats.BattlerStats(unit_a.id);
            var stats_b = new Calculations.Stats.BattlerStats(unit_b.id);

            int value;
            switch ((Unit_Sort)new_sort)
            {
                case Unit_Sort.Name:
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
                    break;
                case Unit_Sort.Class:
                    value = unit_a.actor.class_id - unit_b.actor.class_id;
                    break;
                case Unit_Sort.Lvl:
                    value = unit_b.actor.full_level - unit_a.actor.full_level;
                    break;
                case Unit_Sort.Exp:
                    value = unit_b.actor.exp - unit_a.actor.exp;
                    break;
                case Unit_Sort.Hp:
                    value = unit_b.actor.hp - unit_a.actor.hp;
                    break;
                case Unit_Sort.MaxHp:
                    value = unit_b.actor.maxhp - unit_a.actor.maxhp;
                    break;
                case Unit_Sort.Affin:
                    value = (int)unit_a.actor.affin - (int)unit_b.actor.affin;
                    break;
                case Unit_Sort.Cond:
                    if (unit_b.actor.states.Count == unit_a.actor.states.Count)
                    {
                        int turns_a = 0, turns_b = 0;
                        for (int i = 0; i < unit_a.actor.states.Count; i++)
                            turns_a += unit_a.actor.state_turns_left(unit_a.actor.states[i]);
                        for (int i = 0; i < unit_b.actor.states.Count; i++)
                            turns_b += unit_b.actor.state_turns_left(unit_b.actor.states[i]);
                        value = turns_b - turns_a;
                    }
                    else
                        value = unit_b.actor.states.Count - unit_a.actor.states.Count;
                    break;
                case Unit_Sort.Pow:
                    value = unit_b.stat(Stat_Labels.Pow) - unit_a.stat(Stat_Labels.Pow);
                    break;
                case Unit_Sort.Skl:
                    value = unit_b.stat(Stat_Labels.Skl) - unit_a.stat(Stat_Labels.Skl);
                    break;
                case Unit_Sort.Spd:
                    value = unit_b.stat(Stat_Labels.Spd) - unit_a.stat(Stat_Labels.Spd);
                    break;
                case Unit_Sort.Lck:
                    value = unit_b.stat(Stat_Labels.Lck) - unit_a.stat(Stat_Labels.Lck);
                    break;
                case Unit_Sort.Def:
                    value = unit_b.stat(Stat_Labels.Def) - unit_a.stat(Stat_Labels.Def);
                    break;
                case Unit_Sort.Res:
                    value = unit_b.stat(Stat_Labels.Res) - unit_a.stat(Stat_Labels.Res);
                    break;
                case Unit_Sort.Mov:
                    value = unit_b.mov - unit_a.mov;
                    break;
                case Unit_Sort.Con:
                    value = unit_b.stat(Stat_Labels.Con) - unit_a.stat(Stat_Labels.Con);
                    break;
                case Unit_Sort.Aid:
                    value = unit_b.aid() - unit_a.aid();
                    break;
                case Unit_Sort.Equip:
                    int weapon_id_a = unit_a.actor.weapon_id;
                    if (weapon_id_a == 0)
                        weapon_id_a = unit_a.actor.items[0].Id;
                    int weapon_id_b = unit_b.actor.weapon_id;
                    if (weapon_id_b == 0)
                        weapon_id_b = unit_b.actor.items[0].Id;

                    if (Global.data_weapons.ContainsKey(weapon_id_a) && Global.data_weapons.ContainsKey(weapon_id_b))
                    {
                        var weapon_a = Global.data_weapons[weapon_id_a];
                        var weapon_b = Global.data_weapons[weapon_id_b];

                        if ((weapon_a.Rank == TactileLibrary.Weapon_Ranks.None && weapon_a.is_prf) ||
                                (weapon_b.Rank == TactileLibrary.Weapon_Ranks.None && weapon_b.is_prf))
                            value = (int)weapon_a.Rank - (int)weapon_b.Rank;
                        else
                            value = (int)weapon_b.Rank - (int)weapon_a.Rank;
                        // If the rank is the same, go by type
                        if (value == 0)
                        {
                            value = (int)weapon_a.Main_Type - (int)weapon_b.Main_Type;
                            // If the type is the same, use the price/use
                            if (value == 0)
                            {
                                value = (int)weapon_a.Cost - (int)weapon_b.Cost;
                                // If the type is the same, use the id
                                if (value == 0)
                                    value = weapon_id_b - weapon_id_a;
                            }
                        }
                    }
                    // If either weapon is missing, use the ids
                    else
                        value = weapon_id_b - weapon_id_a;

                    if (value == 0)
                        value = unit_b.actor.num_items - unit_a.actor.num_items;
                    break;
                case Unit_Sort.Skills:
                    value = unit_b.actor.skills.Count - unit_a.actor.skills.Count;
                    break;
                case Unit_Sort.Atk:
                    value = stats_b.dmg() - stats_a.dmg();
                    break;
                case Unit_Sort.Hit:
                    bool staff_b = unit_b.actor.weapon != null && unit_b.actor.weapon.is_staff();
                    bool staff_a = unit_a.actor.weapon != null && unit_a.actor.weapon.is_staff();
                    if (staff_b && staff_a)
                        return 0;
                    else if (staff_b)
                        return -1;
                    else if (staff_a)
                        return 1;
                    value = stats_b.hit() - stats_a.hit();
                    break;
                case Unit_Sort.Avo:
                    value = stats_b.avo() - stats_a.avo();
                    break;
                case Unit_Sort.Crt:
                    value = stats_b.crt() - stats_a.crt();
                    break;
                case Unit_Sort.Dod:
                    value = stats_b.dodge() - stats_a.dodge();
                    break;
                case Unit_Sort.AS:
                    value = unit_b.atk_spd() - unit_a.atk_spd();
                    break;
                case Unit_Sort.Rng:
                    value = unit_a.actor.id - unit_b.actor.id; //Yeti
                    break;
                case Unit_Sort.Sword:
                case Unit_Sort.Lance:
                case Unit_Sort.Axe:
                case Unit_Sort.Bow:
                case Unit_Sort.Fire:
                case Unit_Sort.Thund:
                case Unit_Sort.Wind:
                case Unit_Sort.Light:
                case Unit_Sort.Elder:
                case Unit_Sort.Staff:
                    TactileLibrary.WeaponType type = Global.weapon_types[new_sort - (int)Unit_Sort.Sword + 1];
                    value = unit_b.actor.get_weapon_level(type) - unit_a.actor.get_weapon_level(type);
                    break;
                case Unit_Sort.Ally:
                    value = unit_b.actor.ready_supports().Count - unit_a.actor.ready_supports().Count;
                    break;
                case Unit_Sort.Anim:
                    value = unit_a.actor.individual_animation - unit_b.actor.individual_animation;
                    break;
                default:
                    value = 0;
                    break;
            }
            return value;
        }

        protected virtual void refresh_sort_images()
        {
            switch ((Unit_Sort)sort)
            {
                case Unit_Sort.Sword:
                case Unit_Sort.Lance:
                case Unit_Sort.Axe:
                case Unit_Sort.Bow:
                case Unit_Sort.Fire:
                case Unit_Sort.Thund:
                case Unit_Sort.Wind:
                case Unit_Sort.Light:
                case Unit_Sort.Elder:
                case Unit_Sort.Staff:
                    Weapon_Type_Icon sort_label_icon = new Weapon_Type_Icon();
                    sort_label_icon.index = sort + 1 - (int)Unit_Sort.Sword;
                    Sort_Label_Text = sort_label_icon;
                    break;
                default:
                    TextSprite sort_label_text = new TextSprite();
                    sort_label_text.SetFont(Config.UI_FONT, Global.Content, "White");
                    //if ((Unit_Sort)sort == Unit_Sort.Name)
                    //    sort_label_text.Value = Name_Header.Value;
                    //else
                        sort_label_text.text = SORT_NAMES[sort];
                    Sort_Label_Text = sort_label_text;
                    break;
            }
            Sort_Label_Text.loc = new Vector2(Config.WINDOW_WIDTH - 84, 8);
            Sort_Label_Text.stereoscopic = Config.UNIT_SORT_DEPTH;
            Sort_Arrow.src_rect = new Rectangle(sort_up ? 0 : 8, 0, 8, 16);
        }

        protected void reverse_sort()
        {
            if (sort == (int)Unit_Sort.Anim)
                refresh_sort(sort);
            else
            {
                Team.Reverse();
                //foreach (List<Unit_Page> page_img in Page_Imgs)
                //    page_img.Reverse();
                //Names.Reverse();
                //Map_Sprites.Reverse();
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
                foreach(Character_Sprite sprite in Map_Sprites)
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

        protected void update_cursor_location()
        {
            int target_y = Y_Per_Row * Scroll;
            if (Math.Abs(Offset.Y - target_y) <= Y_Per_Row / 4)
                Offset.Y = target_y;
            if (Math.Abs(Offset.Y - target_y) <= Y_Per_Row)
                Offset.Y = Additional_Math.int_closer((int)Offset.Y, target_y, Y_Per_Row / 4);
            else
                Offset.Y = ((int)(Offset.Y + target_y)) / 2;
            if (Offset.Y != target_y && Scrollbar != null)
            {
                if (Offset.Y > target_y)
                    Scrollbar.moving_up();
                else
                    Scrollbar.moving_down();
            }

            Cursor.update();
        }

        protected void refresh_cursor_location()
        {
            int target_y = Y_Per_Row * Scroll;
            Offset.Y = target_y;
        }

        protected virtual Vector2 cursor_loc()
        {
            int x;
            if (Column == 0)
                x = 40 + (int)Unit_Scissor_Rect.X;
            else
                x = HEADERS[Math.Min(page, BASE_PAGES - 1)][Column - 1].Key + Header_Scissor_Rect.X;
            return new Vector2(x - 16, BASE_Y - 16);
        }

        protected override void UpdateMenu(bool active)
        {
            update_direction();
            update_map_sprite();
            update_sort_arrow();
            if (Scrollbar != null)
                Scrollbar.update();
            Line_Cursor.update();
            Left_Page_Arrow.update();
            Right_Page_Arrow.update();

            base.UpdateMenu(active);

            if (is_help_active)
                Help_Window.update();
            update_cursor_location();
            foreach (List<Unit_Page> page_img in Page_Imgs)
                foreach (Unit_Page unit_page in page_img)
                    unit_page.update();
        }

        protected override void update_input(bool active)
        {
            if (active && this.ready_for_inputs)
            {
                // Change page
                if (Row > 0 && Global.Input.pressed(Inputs.Left) ||
                        Global.Input.gesture_triggered(TouchGestures.SwipeRight))
                    page_left();
                else if (Row > 0 && Global.Input.pressed(Inputs.Right) ||
                        Global.Input.gesture_triggered(TouchGestures.SwipeLeft))
                    page_right();
                // Change header
                else if (Global.Input.repeated(Inputs.Left))
                {
                    if (Column == 0)
                        page_left();
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        move_left();
                    }
                }
                else if (Global.Input.repeated(Inputs.Right))
                {
                    if (Column == Column_Max - 1)
                        page_right();
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        move_right();
                    }
                }
                else if (!is_help_active && (Global.Input.repeated(Inputs.Down)))
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move1);
                    if (Row == row_max - 1)
                        index = 0;
                    else
                        move_down();
                }
                else if (!is_help_active && (Global.Input.repeated(Inputs.Up)))
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move1);
                    if (Row == 0)
                        index = row_max - 1;
                    else
                        move_up();
                }
                else if (!is_help_active && (Preparations && Row > 0 && Global.Input.triggered(Inputs.A)))
                {
                    if (PickUnits_Window.switch_unit(unit.actor))
                        refresh_unit_name_and_sprite_texture(Row - 1);
                }
                // Close unit window
                else if (!is_help_active && ((Row > 0 &&
                    (!Preparations && Global.Input.triggered(Inputs.A)) ||
                    Global.Input.triggered(Inputs.B)) ||
                    Global.Input.gesture_triggered(TouchGestures.LongPress))) //Debug
                {
                    if (Preparations)
                        PickUnits_Window.actor_id = Row == 0 ? Global.game_map.units[Team[0]].actor.id : unit.actor.id;
                    Unit_Selected = Global.Input.triggered(Inputs.A, false);
                    if (Unit_Selected)
                        Global.game_system.play_se(System_Sounds.Confirm);
                    else
                        Global.game_system.play_se(System_Sounds.Cancel);
                    close();
                }
                else if (Global.Input.triggered(Inputs.R) && Row > 0)
                {
                    if (Status != null)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        unit.status();
                        OnStatus(new EventArgs());
                    }
                }
                else if (is_help_active &&
                    (Global.Input.triggered(Inputs.R) || Global.Input.triggered(Inputs.B)))
                {
                    close_help();
                }
                else if (Row == 0 && !is_help_active && Global.Input.triggered(Inputs.A))
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    if (sort == sort_column())
                    {
                        sort_up = !sort_up;
                        reverse_sort();
                    }
                    else
                        refresh_sort(sort_column(), true);
                }
                else if (Row == 0 && !is_help_active && Global.Input.triggered(Inputs.R))
                {
                    open_help();
                }
            }
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
                        page = page + (Direction == 4 ? -1 : 1);
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
        protected int index
        {
            set
            {
                while (Row != value)
                    if (value > Row)
                        move_down();
                    else
                        move_up();
            }
        }

        protected void move_down()
        {
            Row++;
            if (Row > (Rows_At_Once - 1) + Scroll && Scroll < row_max - (Rows_At_Once + 1))
                Scroll++;
            refresh_scroll();
        }
        protected void move_up()
        {
            Row--;
            if (Row - 1 < Scroll + 1 && Scroll > 0)
                Scroll--;
            refresh_scroll();
        }

        protected void refresh_scroll()
        {
            if (Scrollbar != null)
                Scrollbar.scroll = Scroll;
            if (Row == 0)
                Line_Cursor.visible = !(Cursor.visible = true);
            else
                Cursor.visible = !(Line_Cursor.visible = true);
            Line_Cursor.loc = new Vector2(Line_Cursor.loc.X, 8 + BASE_Y + Offset_Y + (Row - Scroll - 1) * Y_Per_Row);
        }

        protected void move_left()
        {
            Column--;
            Cursor.set_loc(cursor_loc());
            if (is_help_active)
            {
                update_help_info();
                update_help_loc();
            }
        }
        protected void move_right()
        {
            Column++;
            Cursor.set_loc(cursor_loc());
            if (is_help_active)
            {
                update_help_info();
                update_help_loc();
            }
        }

        protected void page_left()
        {
            if (page > 0)
            {
                Delay = 16;
                Direction = 4;
                fix_cursor_page_change();
            }
        }
        protected void page_right()
        {
            if (page + 1 < Pages)
            {
                Delay = 16;
                Direction = 6;
                fix_cursor_page_change();
            }
        }

        protected void fix_cursor_page_change()
        {
            int temp_page = page;
            page += (Direction == 4 ? -1 : 1);
            Column_Max = COLUMNS[Math.Min(page, BASE_PAGES - 1)];
            if (Row > 0)
            {
                Column = 0;
                Cursor.loc = cursor_loc();
            }
            else
            {
                Column = (Direction == 4 ? (Column_Max - 1) : 0);
                Cursor.set_loc(cursor_loc());
            }
            if (is_help_active)
            {
                update_help_info();
                update_help_loc();
            }
            page = temp_page;
        }
        #endregion

        #region Help
        public bool is_help_active { get { return Help_Window != null; } }

        public void open_help()
        {
            Help_Window = new Window_Help();
            Help_Window.stereoscopic = Config.UNIT_HELP_DEPTH;
            update_help_info();
            Help_Window.loc = cursor_loc() + new Vector2(20, -8);
            update_help_loc();
            Global.game_system.play_se(System_Sounds.Help_Open);
        }

        public virtual void close_help()
        {
            Help_Window = null;
            Global.game_system.play_se(System_Sounds.Help_Close);
        }

        protected virtual void update_help_loc()
        {
            Help_Window.set_loc(cursor_loc() + new Vector2(20, 0));
        }

        protected virtual void update_help_info()
        {
            if (Column == 0)
                Help_Window.set_text(Global.system_text["Unit Help Name"]);
            else
                Help_Window.set_text(Global.system_text["Unit Help " + HEADERS[Math.Min(page, BASE_PAGES - 1)][Column - 1].Value]);
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
            Name_Header.draw(sprite_batch);
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
            // Map sprite (first one, if not scrolling)
            if (Scroll < Map_Sprites.Count && Offset.Y == Scroll * Y_Per_Row)
                Map_Sprites[Scroll].draw(sprite_batch, unit_offset);
            sprite_batch.End();

            Rectangle unit_scissor_rect = new Rectangle(Unit_Scissor_Rect.X +
                    (int)Stereoscopic_Graphic_Object.graphic_draw_offset(Config.UNIT_WINDOW_DEPTH).X,
                Unit_Scissor_Rect.Y, Unit_Scissor_Rect.Width, Unit_Scissor_Rect.Height);
            sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(unit_scissor_rect);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            // Map Sprites
            for (int i = 0; i < Map_Sprites.Count; i++)
            {
                if (i == 5)
                {
                    int test = 0;
                    test++;
                }
                // Skips the map sprite at the top of and directly below the screen, unless scrolling (top one is handled elsewhere)
                if ((i != Scroll && i != Scroll + Rows_At_Once) || Offset.Y != Scroll * Y_Per_Row)
                    Map_Sprites[i].draw(sprite_batch, unit_offset);
            }
            foreach (TextSprite name in Names)
                name.draw(sprite_batch, unit_offset);
            sprite_batch.End();
            // Data
            if (Page_Imgs.Count > 0)
            {
                Rectangle data_scissor_rect = new Rectangle(Data_Scissor_Rect.X +
                        (int)Stereoscopic_Graphic_Object.graphic_draw_offset(Config.UNIT_WINDOW_DEPTH).X,
                    Data_Scissor_Rect.Y, Data_Scissor_Rect.Width, Data_Scissor_Rect.Height);
                sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(data_scissor_rect);
                foreach (Unit_Page page_img in Page_Imgs[page])
                    page_img.draw(sprite_batch, Offset);
                if (Delay > 0 && changing_page)
                {
                    if (Direction == 6 ^ Delay < 9)
                        foreach (Unit_Page page_img in Page_Imgs[page + 1])
                            page_img.draw(sprite_batch, Offset);
                    else
                        foreach (Unit_Page page_img in Page_Imgs[page - 1])
                            page_img.draw(sprite_batch, Offset);
                }
            }
            // Header
            Rectangle header_scissor_rect = new Rectangle(Header_Scissor_Rect.X +
                    (int)Stereoscopic_Graphic_Object.graphic_draw_offset(Config.UNIT_WINDOW_DEPTH).X,
                Header_Scissor_Rect.Y, Header_Scissor_Rect.Width, Header_Scissor_Rect.Height);
            sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(header_scissor_rect);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            Vector2 header_offset = new Vector2(Offset.X, 0);
            foreach (Sprite header in Headers)
                header.draw(sprite_batch, header_offset);
            sprite_batch.End();
            // Cursor
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Cursor.draw(sprite_batch);
            sprite_batch.End();
            // Help Window
            if (is_help_active)
                Help_Window.draw(sprite_batch);
        }
        #endregion
    }
}
