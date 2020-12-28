using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;
using TactileStringExtension;

namespace Tactile.Windows.Map
{
    class Window_SoloAnim : Window_Unit
    {
        const int Y_PER_ROW = 16;
        const int ROWS_AT_ONCE = 8;
        const int OFFSET_Y = -(16 - Y_PER_ROW) / 2;
        readonly static int[] COLUMNS = new int[] { 4 };
        readonly static Unit_Sort[] PAGE_SORT_START = new Unit_Sort[] { Unit_Sort.Class };
        readonly static KeyValuePair<int, string>[][] HEADERS = new KeyValuePair<int, string>[][] {
            new KeyValuePair<int, string>[]{
                new KeyValuePair<int, string>(0, "Class"), new KeyValuePair<int, string>(88, "Equip"),
                new KeyValuePair<int, string>(88+96, "Anim")}
        };

        #region Accessor Overrides
        protected override int _get_page()
        {
            return 0;
        }
        protected override void _set_page(int value) { }

        public override bool HidesParent { get { return false; } }
        #endregion

        #region Initialization
        protected override void initialize()
        {
            Show_Page_Number = false;
            Y_Per_Row = Y_PER_ROW;
            Rows_At_Once = ROWS_AT_ONCE;
            Offset_Y = OFFSET_Y;
            Unit_Scissor_Rect = new Rectangle(-12, BASE_Y, 80, ROWS_AT_ONCE * Y_PER_ROW);
            Header_Scissor_Rect = new Rectangle(76, BASE_Y - 16, data_width, 16);
            Data_Scissor_Rect = new Rectangle(76, BASE_Y, data_width, ROWS_AT_ONCE * Y_PER_ROW);

            Preparations = Global.game_system.home_base;
            Team = determine_team();
            Pages = 1;
            page = Math.Min(page, Pages - 1);
            Column_Max = COLUMNS[page];
            initialize_sprites();
            update_black_screen();
        }

        protected override void set_headers()
        {
            for (int i = 0; i < HEADERS.Length; i++)
            {
                KeyValuePair<int, string>[] header_page = HEADERS[i];
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

        protected override void set_pages()
        {
            for (int i = 0; i < Team.Count; i++)
            {
                Game_Unit unit = Global.game_map.units[Team[i]];
                Game_Actor actor = unit.actor;
                int y = i * Y_Per_Row;
                // Add map sprite
                Map_Sprites.Add(new Character_Sprite());
                Map_Sprites[i].facing_count = 3;
                Map_Sprites[i].frame_count = 3;
                Map_Sprites[i].texture = Scene_Map.get_team_map_sprite(unit.team, unit.map_sprite_name);
                Map_Sprites[i].offset = new Vector2(
                    (Map_Sprites[i].texture.Width / Map_Sprites[i].frame_count) / 2,
                    (Map_Sprites[i].texture.Height / Map_Sprites[i].facing_count) - 8);
                Map_Sprites[i].loc = new Vector2(32, y + 16 + Offset_Y) + new Vector2(Unit_Scissor_Rect.X, Unit_Scissor_Rect.Y);
                Map_Sprites[i].stereoscopic = Config.UNIT_WINDOW_DEPTH;
                Map_Sprites[i].mirrored = unit.has_flipped_map_sprite;
                // Name
                Names.Add(new TextSprite());
                Names[i].loc = new Vector2(40, y + Offset_Y) + new Vector2(Unit_Scissor_Rect.X, Unit_Scissor_Rect.Y);
                Names[i].SetFont(Config.UI_FONT, Global.Content, "White");
                Names[i].text = unit.actor.name;
                Names[i].stereoscopic = Config.UNIT_WINDOW_DEPTH;
                // Page 1
                Page_Imgs[0].Add(new SoloAnim_Page(unit));
                Page_Imgs[0][i].loc = new Vector2(data_width * 0, y + Offset_Y) + new Vector2(Data_Scissor_Rect.X, Data_Scissor_Rect.Y);
            }
        }

        protected override void refresh_banner_text()
        {
            Banner_Text.src_rect = new Rectangle(0, 16 * 7, 96, 16);
            Page_Number.text = (page + 1).ToString();
        }
        #endregion

        protected override Vector2 cursor_loc()
        {
            int x;
            if (Column == 0)
                x = 40 + (int)Unit_Scissor_Rect.X;
            else
                x = HEADERS[page][Column - 1].Key + Header_Scissor_Rect.X;
            return new Vector2(x - 16, BASE_Y - 16);
        }

        #region Sorting
        protected override int sort_column()
        {
            switch (Column)
            {
                case 1:
                    return (int)Unit_Sort.Class;
                case 2:
                    return (int)Unit_Sort.Equip;
                case 3:
                    return (int)Unit_Sort.Anim;
            }
            return 0;
        }

        protected virtual int sort_switch(Game_Actor actor_a, Game_Actor actor_b, int new_sort)
        {
            int value;
            switch ((Unit_Sort)new_sort)
            {
                case Unit_Sort.Name:
                    if (Preparations)
                    {
                        bool deployed_a = PickUnits_Window.actor_deployed(actor_a.id);
                        bool deployed_b = PickUnits_Window.actor_deployed(actor_b.id);
                        if (deployed_a != deployed_b)
                            return deployed_a ? -1 : 1;
                    }
                    if (Global.battalion.actors.Contains(actor_a.id) && Global.battalion.actors.Contains(actor_b.id))
                        value = Global.battalion.actors.IndexOf(actor_a.id) - Global.battalion.actors.IndexOf(actor_b.id);
                    else
                        value = actor_a.id - actor_b.id;
                    break;
                case Unit_Sort.Anim:
                    value = actor_a.individual_animation - actor_b.individual_animation;
                    break;
                default:
                    value = 0;
                    break;
            }
            return value;
        }
        #endregion

        #region Update
        protected override void update_input(bool active)
        {
            if (active && this.ready_for_inputs)
            {
                // Change unit animation mode
                if (Global.Input.triggered(Inputs.Left) && Row > 0)
                {
                    actor.individual_animation--;
                    if (actor.individual_animation == (int)Constants.Animation_Modes.Map)
                        Global.game_system.play_se(System_Sounds.Cancel);
                    else
                        Global.game_system.play_se(System_Sounds.Confirm);
                }
                else if ((Global.Input.triggered(Inputs.Right) || Global.Input.triggered(Inputs.A)) && Row > 0)
                {
                    actor.individual_animation++;
                    if (actor.individual_animation == (int)Constants.Animation_Modes.Map)
                        Global.game_system.play_se(System_Sounds.Cancel);
                    else
                        Global.game_system.play_se(System_Sounds.Confirm);
                }
                else if (Global.Input.repeated(Inputs.Down) && !is_help_active)
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move1);
                    if (Row == row_max - 1)
                        index = 0;
                    else
                        move_down();
                }
                else if (Global.Input.repeated(Inputs.Up) && !is_help_active)
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move1);
                    if (Row == 0)
                        index = row_max - 1;
                    else
                        move_up();
                }
                // Change header
                else if (Global.Input.repeated(Inputs.Left) && Row == 0)
                {
                    if (Column == 0)
                        page_left();
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        move_left();
                    }
                }
                else if (Global.Input.repeated(Inputs.Right) && Row == 0)
                {
                    if (Column == Column_Max - 1)
                        page_right();
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        move_right();
                    }
                }
                // Close unit window
                else if (Global.Input.triggered(Inputs.B) && !is_help_active)
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    close();
                }
                else if (Global.Input.triggered(Inputs.R) && Row > 0)
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    unit.status();
                    OnStatus(new EventArgs());
                }
                else if ((Global.Input.triggered(Inputs.R) || Global.Input.triggered(Inputs.B)) && is_help_active)
                {
                    close_help();
                }
                else if (Global.Input.triggered(Inputs.A) && Row == 0 && !is_help_active)
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
                else if (Global.Input.triggered(Inputs.R) && Row == 0 && !is_help_active)
                {
                    open_help();
                }
            }
        }
        #endregion

        #region Help
        protected override void update_help_info()
        {
            if (Column == 0)
                Help_Window.set_text(Global.system_text["Unit Help Name"]);
            else
                Help_Window.set_text(Global.system_text["Unit Help " + HEADERS[page][Column - 1].Value]);
        }
        #endregion
    }
}
