#if WINDOWS && DEBUG
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Windows.Map
{
    class Window_Unit_Editor : Map_Window_Base
    {
        const int ROWS_AT_ONCE = 9;
        //readonly static string[] WEAPON_TYPE_LABELS = new string[] { "Sw", "La", "Ax", "Bo", "Fi", "Th", "Wi", "Li", "Da", "St" }; //Debug
        protected const int Data_X = 72;
        protected int Icon_Y;
        const int INPUT_CURSOR_TIME_MAX = 64;

        protected int Index = 0;
        protected int Scroll = 0;
        protected bool Active = true, Option_Selected;
        protected int Input_Cursor_Time = 0;
        bool Reinforcement = false;
        protected List<Inputs> locked_inputs = new List<Inputs>();
        protected Vector2 Offset = Vector2.Zero;
        protected int WLvl_Index = 0;
        protected int Map_Sprite_Anim_Count;
        protected List<int> Actors = new List<int>();
        protected List<int> Classes = new List<int>();
        protected List<int> Items = new List<int>();
        protected List<int> Weapons = new List<int>();
        protected List<int> Weapon_Group_Indices = new List<int>(), Item_Group_Indices = new List<int>();

        protected List<FE_Text> Data = new List<FE_Text>(), Data_Labels = new List<FE_Text>();
        protected FE_Text_Int[] Battle_Stats = new FE_Text_Int[8];
        protected FE_Text[] Stat_Labels = new FE_Text[9];
        protected FE_Text_Int[] Stats = new FE_Text_Int[9];
        protected FE_Text[] Combat_Stat_Labels = new FE_Text[7];
        protected FE_Text_Int[] Combat_Stats = new FE_Text_Int[7];
        protected List<Item_Icon_Sprite> Item_Icons = new List<Item_Icon_Sprite>();
        protected Character_Sprite Map_Sprite;
        protected Sprite Black_Background;
        protected FE_Text Input_Cursor;

        #region Accessors
        public bool active { set { Active = value; } }

        protected Game_Actor actor { get { return Global.game_actors[this.test_battler.Actor_Id]; } }

        protected int max_index { get { return this.test_battler.Generic ? generic_rows : character_rows; } }

        public bool is_ready { get { return !Option_Selected; } }

        public Game_Unit active_unit { get { return Reinforcement ? Global.game_map.last_added_unit : Global.game_map.get_unit(Global.player.loc); } }

        protected virtual int generic_rows
        {
            get
            {
                return 12 + Constants.Actor.NUM_ITEMS;
            }
        }
        protected virtual int character_rows { get { return 8; } }

        private List<string> generic_names
        {
            get
            {
                if (Global.generic_actors != null)
                    return Global.generic_actors.Keys.ToList();
                return new List<string> { "Gladiator" };
            }
        }

        protected override bool ready_for_inputs { get { return base.ready_for_inputs && Active; } }
        #endregion

        public Window_Unit_Editor()
        {
            initialize();
        }
        public Window_Unit_Editor(bool reinforcement)
        {
            Reinforcement = reinforcement;
            initialize();
        }

        protected virtual void initialize()
        {
            Icon_Y = generic_rows - Constants.Actor.NUM_ITEMS;
            Index = 1;

            Actors.AddRange(Global.data_actors.Keys);
            Actors.Sort();
            Classes.AddRange(Global.data_classes.Keys);
            Classes.Sort();
            Items.Add(0);
            Items.AddRange(Global.data_items.Keys);
            Items.Sort();
            Weapons.Add(0);
            Weapons.AddRange(Global.data_weapons.Keys);
            Weapons.Sort();
            initialize_item_groups();

            int weapon_id = Global.test_battler_1.Weapon_Id;
            setup_actor(Global.test_battler_1);
            Global.test_battler_1.Weapon_Id = weapon_id;

            initialize_sprites();
            refresh();
            update_black_screen();
        }

        private void initialize_item_groups()
        {
            string image_name = "";
            Weapon_Group_Indices.Add(0);
            for(int i = 0; i < Weapons.Count; i++)
                if (Global.data_weapons.ContainsKey(Weapons[i]))
                    if (image_name != Global.data_weapons[Weapons[i]].Image_Name)
                    {
                        Weapon_Group_Indices.Add(Weapons[i]);
                        image_name = Global.data_weapons[Weapons[i]].Image_Name;
                    }
            image_name = "";
            Item_Group_Indices.Add(0);
            for (int i = 0; i < Items.Count; i++)
                if (Global.data_items.ContainsKey(Items[i]))
                    if (image_name != Global.data_items[Items[i]].Image_Name)
                    {
                        Item_Group_Indices.Add(Items[i]);
                        image_name = Global.data_items[Items[i]].Image_Name;
                    }
        }

        protected virtual Test_Battle_Character_Data test_battler { get { return Global.test_battler_1; } }

        protected virtual bool generic { get { return Global.test_battler_1.Generic; } }

        protected virtual int team()
        {
            return Constants.Team.PLAYER_TEAM;
        }

        protected void initialize_sprites()
        {
            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 255);
            // Black Screen
            Black_Background = new Sprite();
            Black_Background.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Background.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Background.tint = new Color(0, 0, 0, 128);
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Status_Background");
            (Background as Menu_Background).vel = new Vector2(-0.25f, 0);
            (Background as Menu_Background).tile = new Vector2(3, 2);
            Background.stereoscopic = Config.TITLE_BG_DEPTH;
            // Cursor
            Cursor = new Hand_Cursor();
            Cursor.min_distance_y = 4;
            Cursor.override_distance_y = 16;
            Cursor.loc = cursor_loc();
            Cursor.stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
            // Input Cursor
            Input_Cursor = new FE_Text();
            Input_Cursor.loc = new Vector2(Data_X, 24 + 16 * 1);
            Input_Cursor.Font = "FE7_Text";
            Input_Cursor.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Input_Cursor.text = ".";
            Input_Cursor.visible = false;
            Input_Cursor.stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
            // Map Sprite
            Map_Sprite = new Character_Sprite();
            Map_Sprite.facing_count = 3;
            Map_Sprite.frame_count = 3;
            Map_Sprite.loc = new Vector2(32, 24);
            Map_Sprite.stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
            // Battle Stats
            for (int i = 0; i < Battle_Stats.Length; i++)
            {
                Battle_Stats[i] = new FE_Text_Int();
                Battle_Stats[i].loc = new Vector2(64 + 40 * (i / 4), 8 + 16 * (i % 4));
                Battle_Stats[i].Font = "FE7_Text";
                Battle_Stats[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
                Battle_Stats[i].stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
            }
            // Stats
            for (int i = 0; i < Stat_Labels.Length; i++)
            {
                Stat_Labels[i] = new FE_Text();
                Stat_Labels[i].loc = new Vector2(Config.WINDOW_WIDTH - 64, 8 + 16 * i);
                Stat_Labels[i].Font = "FE7_Text";
                Stat_Labels[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
                Stat_Labels[i].stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
            }

            Stat_Labels[0].text = "Hp";
            Stat_Labels[1].text = "Pow";
            Stat_Labels[2].text = "Skl";
            Stat_Labels[3].text = "Spd";
            Stat_Labels[4].text = "Lck";
            Stat_Labels[5].text = "Def";
            Stat_Labels[6].text = "Res";
            Stat_Labels[7].text = "Con";
            Stat_Labels[8].text = "Rank";
            for (int i = 0; i < Stats.Length; i++)
            {
                Stats[i] = new FE_Text_Int();
                Stats[i].loc = new Vector2(Config.WINDOW_WIDTH - 16, 8 + 16 * i);
                Stats[i].Font = "FE7_Text";
                Stats[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
                Stats[i].stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
            }
            // Combat Stats
            for (int i = 0; i < Combat_Stat_Labels.Length; i++)
            {
                Combat_Stat_Labels[i] = new FE_Text();
                Combat_Stat_Labels[i].loc = new Vector2(Config.WINDOW_WIDTH - 120, 8 + 16 * i);
                Combat_Stat_Labels[i].Font = "FE7_Text";
                Combat_Stat_Labels[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
                Combat_Stat_Labels[i].stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
            }
            Combat_Stat_Labels[0].text = "Hit";
            Combat_Stat_Labels[1].text = "Dmg";
            Combat_Stat_Labels[2].text = "Crt";
            Combat_Stat_Labels[3].text = "Avo";
            Combat_Stat_Labels[4].text = "Dod";
            Combat_Stat_Labels[5].text = "AS";
            Combat_Stat_Labels[6].text = "Rng";
            for (int i = 0; i < Combat_Stats.Length; i++)
            {
                Combat_Stats[i] = new FE_Text_Int();
                Combat_Stats[i].loc = new Vector2(Config.WINDOW_WIDTH - 72, 8 + 16 * i);
                Combat_Stats[i].Font = "FE7_Text";
                Combat_Stats[i].stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
            }
            // Item Icons
            initialize_icons();
        }

        protected virtual void initialize_icons()
        {
            for (int i = 0; i < Constants.Actor.NUM_ITEMS; i++)
            {
                Item_Icons.Add(new Item_Icon_Sprite());
                Item_Icons[i].loc = new Vector2(Data_X, 24 + (Icon_Y + i) * 16);
                Item_Icons[i].stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
            }
        }

        protected virtual void refresh()
        {
            Game_Unit unit = active_unit;
            int team = unit.team;
            int ai_priority = unit.priority;
            int ai_mission = unit.full_ai_mission;
            Map_Sprite.texture = Scene_Map.get_team_map_sprite(
                team, actor.map_sprite_name);
            if (Map_Sprite.texture != null)
            {
                Map_Sprite.offset = new Vector2(
                    (Map_Sprite.texture.Width / Map_Sprite.frame_count) / 2,
                    (Map_Sprite.texture.Height / Map_Sprite.facing_count) - 8);
            }
            Map_Sprite.mirrored = unit.has_flipped_map_sprite;

            Test_Battle_Character_Data test_battler = this.test_battler;

            Global.game_map.remove_unit(unit.id);
            Global.game_map.add_actor_unit(team, Global.player.loc, test_battler.Actor_Id, "");
            Global.game_map.fix_unit_locations();
            unit = active_unit;
            unit.priority = ai_priority;
            unit.full_ai_mission = ai_mission;
            Global.game_temp.highlighted_unit_id = Global.game_system.Selected_Unit_Id = unit.id;
            // Labels
            Data_Labels.Clear();
            for (int i = 0; i < max_index; i++)
            {
                Data_Labels.Add(new FE_Text());
                Data_Labels[i].loc = new Vector2(Data_X - 48, 24 + 16 * i);
                Data_Labels[i].Font = "FE7_Text";
                Data_Labels[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            }
            // Actor data
            Data.Clear();
            for (int i = 0; i < max_index; i++)
            {
                Data.Add(new FE_Text());
                Data[i].loc = new Vector2(Data_X, 24 + 16 * i);
                Data[i].Font = "FE7_Text";
                Data[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
                Data[i].stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
            }
            Data_Labels[0].text = "Type";
            Data_Labels[1].text = "Identifier";
            if (test_battler.Generic)
            {
                Data_Labels[2].text = "Class";
                Data_Labels[7].text = "Build";
                Data_Labels[8].text = "Gender";
                Data_Labels[9].text = "Name";
                Data_Labels[10].text = "Con";
                //Data_Labels[11].text = "WLvls (" + WEAPON_TYPE_LABELS[WLvl_Index] + ")"; //Debug
                Data_Labels[11].text = string.Format("WLvls ({0})",
                    Global.weapon_types[WLvl_Index + 1].Name.Substring(0, 2));
                Data_Labels[12].text = "Items";
                Data[0].text = "Generic";
                Data[1].text = test_battler.Identifier;
                Data[2].text = actor.class_name;
                Data[7].text = test_battler.Build.ToString();
                Data[8].text = test_battler.Gender.ToString();
                Data[9].text = test_battler.Name;
                Data[10].text = test_battler.Con.ToString();
                Data[11].text = test_battler.WLvls[WLvl_Index].ToString();

                int prelevel_expected_min = 0, prelevel_expected_max = 0;
                if (actor.tier > 0)
                {
                    prelevel_expected_min += Constants.Actor.TIER0_LVL_CAP;
                    prelevel_expected_min += (actor.tier - 1) * Config.PROMOTION_LVL;
                }
                if (actor.tier > 0)
                {
                    prelevel_expected_max += Constants.Actor.TIER0_LVL_CAP;
                    prelevel_expected_max += (actor.tier - 1) * Constants.Actor.LVL_CAP;
                }
                bool in_prelevel_range =
                    test_battler.Prepromote_Levels >= prelevel_expected_min &&
                    test_battler.Prepromote_Levels <= prelevel_expected_max;
                Data[6].texture = Global.Content.Load<Texture2D>(!in_prelevel_range ?
                    @"Graphics/Fonts/FE7_Text_Red" : @"Graphics/Fonts/FE7_Text_Blue");

                for (int i = 0; i < Constants.Actor.NUM_ITEMS; i++)
                {
                    int item_index = 12 + i;
                    Data[item_index].loc.X += 16;
                    Data_Equipment item = null;
                    if (test_battler.Items[i].Id > 0)
                    {
                        if (test_battler.Items[i].is_weapon)
                        {
                            //if (Global.data_weapons.ContainsKey(test_battler.Items[i].Id)) //Debug
                            item = test_battler.Items[i].to_equipment;
                        }
                        else
                        {
                            //if (Global.data_items.ContainsKey(test_battler.Items[i].Id)) //Debug
                            item = test_battler.Items[i].to_equipment;
                        }
                    }

                    if (item == null)
                    {
                        Data[item_index].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
                        Item_Icons[i].texture = null;
                    }
                    else
                    {
                        if (test_battler.Items[i].is_weapon && !actor.is_equippable(test_battler.Items[i].to_weapon))
                            Data[item_index].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
                        try
                        {
                            Item_Icons[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + item.Image_Name);
                        }
                        catch (Exception e) { }
                        Item_Icons[i].index = item.Image_Index;
                    }
                    Data[item_index].text = item == null ? (test_battler.Items[i].Type == Item_Data_Type.Weapon ? "None" : "None (Item)") : item.Name;
                }
            }
            else
            {
                Data_Labels[2].text = "Name";
                Data_Labels[7].text = "Tier";
                Data[0].text = "Actor";
                Data[1].text = test_battler.Identifier;
                Data[2].text = actor.name;
                Data[7].text = actor.tier.ToString() + ", " + actor.class_name;
            }
            Data_Labels[3].text = "Priority";
            Data_Labels[4].text = "Mission";
            Data_Labels[5].text = "Level";
            Data_Labels[6].text = "PreLevels";
            Data[3].text = test_battler.Priority.ToString();
            Data[4].text = test_battler.Mission.ToString();
            if (Game_AI.MISSION_NAMES.ContainsKey(test_battler.Mission % Game_AI.MISSION_COUNT))
                Data[4].text += ": " + Game_AI.MISSION_NAMES[test_battler.Mission % Game_AI.MISSION_COUNT];
            //Data[5].loc.X += 16;
            //Data[5].Value = Global.data_weapons[test_battler.Weapon_Id].Name;
            //if (actor.weapon_id <= 0)
            //    Data[5].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
            //Item_Icons[0].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + Global.data_weapons[test_battler.Weapon_Id].Image_Name);
            //Item_Icons[0].index = Global.data_weapons[test_battler.Weapon_Id].Image_Index;
            Data[5].text = test_battler.Level.ToString();
            Data[6].text = test_battler.Prepromote_Levels.ToString();
            // Stats
            for (int i = 0; i < Stats.Length; i++)
            {
                if (i == 8)
                {
                    Stats[i].text = actor.rating().ToString();
                    Stats[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
                }
                else
                {
                    Stats[i].text = actor.stat(i).ToString();
                    Stats[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + (actor.get_capped(i) ? "Green" : "Blue"));
                }
            }
            // Combat Stats
            for (int i = 0; i < Combat_Stats.Length; i++)
                Combat_Stats[i].texture = Global.Content.Load<Texture2D>(
                    @"Graphics/Fonts/FE7_Text_" + (actor.weapon == null ? "Grey" : "Blue"));

            int actor_weapon_id = unit.actor.weapon_id;
            int weapon_id = 0;
            if (actor_weapon_id != 0)
                weapon_id = actor_weapon_id;
            else
            {
                for (int i = 0; i < Constants.Actor.NUM_ITEMS; i++)
                {
                    if (test_battler.Items[i].is_weapon)
                    {
                        weapon_id = test_battler.Items[i].Id;
                        break;
                    }
                }
            }
            unit.actor.weapon_id = weapon_id;

            var stats = new Calculations.Stats.BattlerStats(unit.id);
            Combat_Stats[0].text = stats.hit().ToString();
            Combat_Stats[1].text = stats.dmg().ToString();
            Combat_Stats[2].text = stats.crt().ToString();
            Combat_Stats[3].text = stats.avo().ToString();
            Combat_Stats[4].text = stats.dodge().ToString();
            Combat_Stats[5].text = unit.atk_spd().ToString();
            int min_range = unit.min_range();
            int max_range = unit.max_range();
            Combat_Stats[6].text = min_range != max_range ? min_range.ToString() + "-" + max_range.ToString() : min_range.ToString(); //Yeti
            unit.actor.weapon_id = actor_weapon_id;
        }

        protected Vector2 cursor_loc()
        {
            return new Vector2(8, 24 + (Index - Scroll) * 16);
        }

        #region Update
        protected void update_map_sprite()
        {
            int old_frame = Map_Sprite_Frame;
            Map_Sprite_Anim_Count = (Map_Sprite_Anim_Count + 1) % Config.CHARACTER_TIME;
            int count = Map_Sprite_Anim_Count;
            if (count >= 0 && count < 32)
                Map_Sprite_Frame = 0;
            else if (count >= 32 && count < 36)
                Map_Sprite_Frame = 1;
            else if (count >= 36 && count < 68)
                Map_Sprite_Frame = 2;
            else if (count >= 68 && count < 72)
                Map_Sprite_Frame = 1;
            if (Map_Sprite_Frame != old_frame)
                Map_Sprite.frame = Map_Sprite_Frame;
        }

        protected void update_cursor_location()
        {
            int target_y = 16 * Scroll;
            if (Math.Abs(Offset.Y - target_y) <= 4)
                Offset.Y = target_y;
            if (Math.Abs(Offset.Y - target_y) <= 16)
                Offset.Y = Additional_Math.int_closer((int)Offset.Y, target_y, 4);
            else
                Offset.Y = ((int)(Offset.Y + target_y)) / 2;

            if (Option_Selected)
                Cursor.tint = new Color(192, 192, 192, 255);
            else
                Cursor.tint = Color.White;
            Input_Cursor_Time = (Input_Cursor_Time + 1) % INPUT_CURSOR_TIME_MAX;
            if (Option_Selected && Input_Cursor_Time < INPUT_CURSOR_TIME_MAX / 2)
            {
                Input_Cursor.loc.Y = 24 + 16 * Index;
                switch (Index)
                {
                    case 1:
                        Input_Cursor.visible = true;
                        Input_Cursor.offset.X = -Font_Data.text_width(this.test_battler.Identifier);
                        break;
                    case 9:
                        if (this.test_battler.Generic)
                        {
                            Input_Cursor.visible = true;
                            Input_Cursor.offset.X = -Font_Data.text_width(this.test_battler.Name);
                        }
                        else
                            Input_Cursor.visible = false;
                        break;
                    default:
                        Input_Cursor.visible = false;
                        break;
                }
            }
            else
                Input_Cursor.visible = false;
        }

        protected override void UpdateMenu(bool active)
        {
            update_cursor_location();
            update_map_sprite();
            base.UpdateMenu(active);
            Cursor.update();
        }

        protected override void update_input(bool active)
        {
            if (active && this.ready_for_inputs)
            {
                if (Option_Selected)
                {
                    if (Global.Input.triggered(Inputs.Start))
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Option_Selected = false;
                        Input_Cursor.visible = false;
                    }
                    else
                        update_option_selected();
                }
                else
                {
                    if (Global.Input.repeated(Inputs.Left))
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        update_option_change(false);
                    }
                    else if (Global.Input.repeated(Inputs.Right))
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        update_option_change(true);
                    }
                    else if (Global.Input.repeated(Inputs.L))
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        update_option_change(false, true);
                    }
                    else if (Global.Input.repeated(Inputs.R))
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move2);
                        update_option_change(true, true);
                    }
                    else if (Global.Input.triggered(Inputs.X))
                    {
                        if (Index >= 12)
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                            switch_item_mode();
                        }
                    }
                    else if (Global.Input.triggered(Inputs.A))
                    {
                        update_selecting_option();
                    }
                    else if (Global.Input.repeated(Inputs.Down) && !locked_inputs.Contains(Inputs.Down))
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move1);
                        move_down();
                    }
                    else if (Global.Input.repeated(Inputs.Up) && !locked_inputs.Contains(Inputs.Up))
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move1);
                        move_up();
                    }
                }
            }
            if (!Global.Input.pressed(Inputs.Up))
                locked_inputs.Remove(Inputs.Up);
            if (!Global.Input.pressed(Inputs.Down))
                locked_inputs.Remove(Inputs.Down);
        }

        protected void setup_actor(Test_Battle_Character_Data test_battler)
        {
            Game_Actor actor;
            int weapon_id = test_battler.Weapon_Id;
            // Generic
            if (test_battler.Generic)
            {
                if (Reinforcement && Global.game_actors.ContainsKey(test_battler.Actor_Id) &&
                        Global.game_actors.is_temp_actor(test_battler.Actor_Id))
                {
                    Global.game_actors.temp_clear(Global.game_actors[test_battler.Actor_Id].id);
                    actor = Global.game_actors[test_battler.Actor_Id];
                }
                else
                {
                    if (Global.game_actors.ContainsKey(test_battler.Actor_Id) &&
                            Global.game_actors.is_temp_actor(test_battler.Actor_Id))
                        Global.game_actors.temp_clear(Global.game_actors[test_battler.Actor_Id].id);
                    actor = Global.game_actors.new_actor();
                }
                actor.name = "Gladiator";
                actor.class_id = test_battler.Class_Id;
                actor.gender = test_battler.Gender;

                int[] wexp = Enumerable.Range(0, Global.weapon_types.Count - 1)
                    .Select(x =>
                    {
                        if (Global.scene.scene_type == "Scene_Title")
                        {
                            return Data_Weapon.WLVL_THRESHOLDS[Data_Weapon.WLVL_THRESHOLDS.Length - 1];
                        }
                        else
                        {
                            if (x < test_battler.WLvls.Length)
                                return test_battler.WLvls[x];
                        }
                        return 0;
                    })
                    .ToArray();

                actor.setup_generic(test_battler.Class_Id, test_battler.Level, 0, test_battler.Prepromote_Levels,
                    test_battler.Build, test_battler.Con, wexp: wexp);
                test_battler.Actor_Id = actor.id;
                /* //Debug
                for (int i = 1; i < Global.weapon_types.Count; i++)
                {
                    if (Global.scene.scene_type == "Scene_Title")
                    {
                        if (actor.weapon_level_cap(Global.weapon_types[i]) > 0)
                            actor.wexp_gain(Global.weapon_types[i],
                                Data_Weapon.WLVL_THRESHOLDS[Data_Weapon.WLVL_THRESHOLDS.Length - 1]);
                    }
                    else
                    {
                        if (actor.weapon_level_cap(Global.weapon_types[i]) > 0)
                            actor.wexp_gain(Global.weapon_types[i], test_battler.WLvls[i - 1]);
                    }
                }*/
                actor.setup_items();
            }
            // Actor
            else
            {
                Global.game_actors.remove_actor(test_battler.Actor_Id);
                actor = Global.game_actors[test_battler.Actor_Id];
                int level = actor.level;
                Data_Actor actor_data = Global.data_actors[test_battler.Actor_Id];
                // Loops level up many times (subtracts the one free level per promotion)
                int prepromote_offset = Math.Max(test_battler.Tier - (Global.data_classes[actor_data.ClassId].Tier + 1), 0);
                int exp_gain = 0;
                for (int i = 0; i < (test_battler.Prepromote_Levels - prepromote_offset); i++)
                {
                    // If the actor is at their level cap and trying to gain another level, they want to promote
                    if (actor.level + (exp_gain / Constants.Actor.EXP_TO_LVL) == actor.level_cap())
                    {
                        actor.instant_level = true;
                        actor.exp += exp_gain;
                        exp_gain = 0;
                        if (actor.can_promote() && test_battler.Tier > actor.tier)
                            actor.quick_promotion((int)actor.promotes_to());
                        // If they're trying to gain a level at level cap, they want to promote, but don't have a promotion class?
                        else
                            actor.level_down(); //Yeti
                    }
                    exp_gain += Constants.Actor.EXP_TO_LVL;
                }
                if (exp_gain > 0)
                {
                    // why was a break point here I don't know //Debug
                    // It has something to do with promoted player units in test battles, idk lol!
                    // This whole block doesn't quite work right when the PC doesn't use full promotion levels
                    int x = exp_gain;
                }
                actor.instant_level = true;
                actor.exp += exp_gain;
                exp_gain = 0;
                actor.level_down(); //Yeti
                while (actor.can_promote() && test_battler.Tier > actor.tier)
                {
                    actor.quick_promotion((int)actor.promotes_to(false));
                }
                //while (actor.level < test_battler.Level)
                //{
                //    actor.instant_level = true;
                //    actor.exp += Config.EXP_TO_LVL;
                //}

                actor.instant_level = true;
                // If promoted any, level up from 1 instead of wherever the actor started
                int base_level = test_battler.Tier != Global.data_classes[actor_data.ClassId].Tier ?
                    1 : Global.data_actors[actor.id].Level;
                actor.exp += Constants.Actor.EXP_TO_LVL * (test_battler.Level - base_level);
                actor.level = test_battler.Level;
                for (int i = 1; i < Global.weapon_types.Count; i++)
                    if (actor.weapon_level_cap(Global.weapon_types[i]) > 0)
                        actor.wexp_gain(Global.weapon_types[i],
                            Data_Weapon.WLVL_THRESHOLDS[Data_Weapon.WLVL_THRESHOLDS.Length - 1]);
            }
            actor.clear_wlvl_up();
            actor.clear_items();
            actor.gain_item(new Item_Data(Item_Data_Type.Weapon, weapon_id, 1));
            if (actor.is_equippable(1))
                actor.equip(1);
            else
                actor.unequip();
            update_inventory();
            actor.recover_all();
        }
        #endregion

        #region Movement
        protected void update_option_change(bool right)
        {
            update_option_change(right, false);
        }
        protected virtual void update_option_change(bool right, bool trigger)
        {
            Test_Battle_Character_Data test_battler = this.test_battler;
            Data_Class class_data;
            switch (Index)
            {
                // Generic
                case 0:
                    test_battler.Generic = !test_battler.Generic;
                    if (test_battler.Generic)
                    {
                        test_battler.Class_Id = Test_Battle_Character_Data.default_class_id;
                        test_battler.Build = Generic_Builds.Strong;
                        test_battler.Level = 1;
                        change_generic_actor();
                    }
                    else
                    {
                        test_battler.Actor_Id = 1;
                        change_actor();
                    }
                    setup_actor(test_battler);
                    break;
                // Identifier
                case 1:
                    break;
                case 2:
                    if (test_battler.Generic)
                    {
                        // Class Id
                        test_battler.Class_Id = Classes[
                            (Classes.IndexOf(test_battler.Class_Id) + Classes.Count + (right ? 1 : -1)) % Classes.Count];
                        change_generic_actor();
                    }
                    else
                    {
                        // Actor Id
                        test_battler.Actor_Id = Actors[
                            (Actors.IndexOf(test_battler.Actor_Id) + Actors.Count + (right ? 1 : -1)) % Actors.Count];
                        change_actor();
                    }
                    setup_actor(test_battler);
                    break;
                // Ai Priority
                case 3:
                    if (right || test_battler.Priority - (trigger ? Game_AI.MISSION_COUNT : 1) >= 0)
                        test_battler.Priority += (right ? 1 : -1) * (trigger ? Game_AI.MISSION_COUNT : 1);
                    break;
                // Ai Mission
                case 4:
                    if (right || test_battler.Mission - (trigger ? Game_AI.MISSION_COUNT : 1) >= 0)
                    {
                        test_battler.Mission += (right ? 1 : -1) * (trigger ? Game_AI.MISSION_COUNT : 1);
                        // If the selected mission is not a main mission, jump to the next one
                        if (test_battler.Mission % Game_AI.MISSION_COUNT >= Game_AI.MAIN_MISSION_COUNT)
                            test_battler.Mission = right ? ((test_battler.Mission / Game_AI.MISSION_COUNT) + 1) * Game_AI.MISSION_COUNT :
                                ((test_battler.Mission - (Game_AI.MISSION_COUNT - Game_AI.MAIN_MISSION_COUNT)) /
                                Game_AI.MISSION_COUNT) * Game_AI.MISSION_COUNT + (Game_AI.MAIN_MISSION_COUNT - 1);
                    }
                    break;
                // Level
                case 5:
                    if (right)
                    {
                        if (test_battler.Level < actor.level_cap())
                            test_battler.Level++;
                    }
                    else
                    {
                        if (test_battler.Generic)
                        {
                            if (test_battler.Level > 1)
                                test_battler.Level--;
                        }
                        else
                        {
                            Data_Actor actor_data = Global.data_actors[test_battler.Actor_Id];
                            if (test_battler.Level > (actor.tier == Global.data_classes[actor_data.ClassId].Tier ?
                                    Global.data_actors[test_battler.Actor_Id].Level  : 1))
                                test_battler.Level--;
                        }
                    }
                    setup_actor(test_battler);
                    break;
                // Prepromote levels
                case 6:
                    if (test_battler.Generic)
                    {
                        if (right)
                        {
                            int max_level = 0;
                            if (actor.tier > 0)
                            {
                                max_level += Constants.Actor.TIER0_LVL_CAP;
                                max_level += (actor.tier - 1) * Constants.Actor.LVL_CAP;
                            }
                            if (Global.Input.pressed(Inputs.Y))
                                max_level = 100;
                            if (test_battler.Prepromote_Levels < max_level)
                                test_battler.Prepromote_Levels++;
                        }
                        else
                        {
                            int min_level = actor.tier;
                            if (test_battler.Prepromote_Levels > min_level)
                                test_battler.Prepromote_Levels--;
                        }
                    }
                    else
                    {
                        Data_Actor actor_data = Global.data_actors[test_battler.Actor_Id];
                        class_data = Global.data_classes[actor_data.ClassId];
                        if (right)
                        {
                            int max_level = -actor_data.Level;
                            if (test_battler.Tier > 0 && class_data.Tier <= 0)
                                max_level += Constants.Actor.TIER0_LVL_CAP;
                            if (test_battler.Tier > 1 && class_data.Tier <= 1)
                            {
                                max_level += (test_battler.Tier - Math.Max(class_data.Tier, 1)) * Constants.Actor.LVL_CAP;
                            }
                            if (test_battler.Prepromote_Levels < max_level)
                                test_battler.Prepromote_Levels++;
                        }
                        else
                        {
                            int min_level = -actor_data.Level;
                            if (test_battler.Tier > 0 && class_data.Tier <= 0)
                                min_level += Constants.Actor.TIER0_LVL_CAP;
                            if (test_battler.Tier > 1 && class_data.Tier <= 1)
                            {
                                min_level += (test_battler.Tier - Math.Max(class_data.Tier, 1)) * Config.PROMOTION_LVL;
                            }
                            min_level = Math.Max(min_level, 0);
                            if (test_battler.Prepromote_Levels > min_level)
                                test_battler.Prepromote_Levels--;
                        }
                    }
                    setup_actor(test_battler);
                    break;
                case 7:
                    // Build
                    if (test_battler.Generic)
                    {
                        test_battler.Build = (Generic_Builds)(MathHelper.Clamp((int)test_battler.Build + (right ? 1 : -1),
                            (int)Generic_Builds.Weak, (int)Generic_Builds.Strong));
                    }
                    // Tier
                    else
                    {
                        Data_Actor actor_data = Global.data_actors[test_battler.Actor_Id];
                        class_data = Global.data_classes[actor_data.ClassId];
                        if (right)
                        {
                            int new_class = actor_data.ClassId;
                            int max_tier = class_data.Tier;
                            while (Global.data_classes[new_class].can_promote())
                            {
                                new_class = Global.data_classes[new_class].promotion_keys.Min();
                                max_tier = Global.data_classes[new_class].Tier;
                            }
                            if (test_battler.Tier < max_tier)
                                test_battler.Tier++;
                        }
                        else
                        {
                            if (test_battler.Tier > class_data.Tier)
                            {
                                test_battler.Tier--;
                                test_battler.Level = Math.Max(test_battler.Level, actor_data.Level);
                            }
                        }
                        int max_level = -actor_data.Level;
                        int min_level = -actor_data.Level;
                        if (test_battler.Tier > 0 && class_data.Tier <= 0)
                        {
                            max_level += Constants.Actor.TIER0_LVL_CAP;
                            min_level += Constants.Actor.TIER0_LVL_CAP;
                        }
                        if (test_battler.Tier > 1 && class_data.Tier <= 1)
                        {
                            max_level += (test_battler.Tier -
                                Math.Max(class_data.Tier, 1)) * Constants.Actor.LVL_CAP;
                            min_level += (test_battler.Tier -
                                Math.Max(class_data.Tier, 1)) * Config.PROMOTION_LVL;
                        }
                        min_level = Math.Max(min_level, 0);
                        max_level = Math.Max(max_level, min_level);
                        test_battler.Prepromote_Levels = (int)MathHelper.Clamp(test_battler.Prepromote_Levels, min_level, max_level);
                    }
                    setup_actor(test_battler);
                    break;
                // Gender
                case 8:
                    test_battler.Gender = (int)(MathHelper.Clamp(test_battler.Gender + (right ? 1 : -1), 0, 5));
                    actor.gender = test_battler.Gender;
                    break;
                // Name
                case 9:
                    if (this.generic_names.Count > 0)
                    {
                        int name_index = this.generic_names.IndexOf(test_battler.Name);
                        test_battler.Name = name_index == -1 ? this.generic_names[0] :
                            this.generic_names[(name_index + (right ? 1 : -1) + this.generic_names.Count) % this.generic_names.Count];
                    }
                    break;
                // Con
                case 10:
                    test_battler.Con = (int)MathHelper.Clamp(test_battler.Con + (right ? 1 : -1), -1, actor.get_cap(FEXNA.Stat_Labels.Con));
                    setup_actor(test_battler);
                    break;
                // WLvl
                case 11:
                    int weapon_type_count = Global.weapon_types.Count - 1;
                    if (trigger)
                        WLvl_Index = (WLvl_Index + (right ? 1 : -1) + weapon_type_count) % weapon_type_count;
                    else
                    {
                        if (Global.Input.pressed(Inputs.Y))
                            test_battler.WLvls[WLvl_Index] = Math.Min(Math.Max(test_battler.WLvls[WLvl_Index] + (right ? 1 : -1),
                                1), Data_Weapon.WLVL_THRESHOLDS[actor.max_weapon_level(Global.weapon_types[WLvl_Index + 1])]);
                        else
                        {
                            int wlvl_index = 0;
                            // Gets the weapon level threshold index of the current wexp
                            for (; ; )
                            {
                                if (wlvl_index < Data_Weapon.WLVL_THRESHOLDS.Length - 1 &&
                                        test_battler.WLvls[WLvl_Index] >= Data_Weapon.WLVL_THRESHOLDS[wlvl_index + 1])
                                    wlvl_index++;
                                else
                                    break;
                            }
                            //test_battler.WLvls[WLvl_Index] = Math.Min(Math.Max(Data_Weapon.WLVL_THRESHOLDS[ //Debug
                            //    (int)MathHelper.Clamp(wlvl_index + (right ? 1 : -1), 0, Data_Weapon.WLVL_THRESHOLDS.Length - 1)],
                            //    1), Data_Weapon.WLVL_THRESHOLDS[actor.max_weapon_level(WLvl_Index + 1)]);
                            test_battler.WLvls[WLvl_Index] = Math.Min(Data_Weapon.WLVL_THRESHOLDS[
                                    actor.max_weapon_level(Global.weapon_types[WLvl_Index + 1])],
                                Data_Weapon.WLVL_THRESHOLDS[(int)MathHelper.Clamp(wlvl_index + (right ? 1 : -1), 0, Data_Weapon.WLVL_THRESHOLDS.Length - 1)]);
                        }
                        setup_actor(test_battler);
                    }
                    break;
                // Items
                default:
                    int inventory_index = Index - 12;
                    // If L/R pressed, jump to the start of the next group
                    if (trigger)
                    {
                        int item_id = next_equipment_group(right, test_battler.Items[inventory_index]);
                        test_battler.Items[inventory_index].Id = item_id;
                    }
                    else
                    {
                        bool is_weapon = test_battler.Items[inventory_index].is_weapon;
                        if (test_battler.Items[inventory_index].non_equipment)
                            is_weapon = test_battler.Items[inventory_index].Type == Item_Data_Type.Weapon;
                        if (is_weapon)
                            test_battler.Items[inventory_index].Id = Weapons[
                                (Weapons.IndexOf(test_battler.Items[inventory_index].Id) + Weapons.Count +
                                (Global.Input.pressed(Inputs.Y) ? 5 : 1) * (right ? 1 : -1)) % Weapons.Count];
                        else
                            test_battler.Items[inventory_index].Id = Items[
                                (Items.IndexOf(test_battler.Items[inventory_index].Id) + Items.Count +
                                (Global.Input.pressed(Inputs.Y) ? 5 : 1) * (right ? 1 : -1)) % Items.Count];
                    }
                    update_inventory();
                    break;
            }
            refresh();
        }

        private int next_equipment_group(bool right, Item_Data item_data)
        {
            if (item_data.is_weapon ||
                    (item_data.non_equipment && item_data.Type == Item_Data_Type.Weapon))
                return next_weapon_group(right, item_data.Id);
            else
                return next_item_group(right, item_data.Id);
        }
        protected int next_weapon_group(bool right, int item_id)
        {
            if (right)
            {
                if (item_id >= Weapon_Group_Indices[Weapon_Group_Indices.Count - 1])
                    item_id = Weapon_Group_Indices[0];
                else
                    for (int i = 1; i < Weapon_Group_Indices.Count; i++)
                        if (item_id < Weapon_Group_Indices[i])
                        {
                            return Weapon_Group_Indices[i];
                        }
            }
            else
            {
                if (item_id <= Weapon_Group_Indices[0])
                    item_id = Weapon_Group_Indices[Weapon_Group_Indices.Count - 1];
                else
                    for (int i = Weapon_Group_Indices.Count - 1; i >= 0; i--)
                        if (item_id > Weapon_Group_Indices[i])
                        {
                            return Weapon_Group_Indices[i];
                        }
            }
            return item_id;
        }
        private int next_item_group(bool right, int item_id)
        {
            if (right)
            {
                if (item_id >= Item_Group_Indices[Item_Group_Indices.Count - 1])
                    item_id = Item_Group_Indices[0];
                else
                    for (int i = 1; i < Item_Group_Indices.Count; i++)
                        if (item_id < Item_Group_Indices[i])
                        {
                            return Item_Group_Indices[i];
                        }
            }
            else
            {
                if (item_id <= Item_Group_Indices[0])
                    item_id = Item_Group_Indices[Item_Group_Indices.Count - 1];
                else
                    for (int i = Item_Group_Indices.Count - 1; i >= 0; i--)
                        if (item_id > Item_Group_Indices[i])
                        {
                            return Item_Group_Indices[i];
                        }
            }
            return item_id;
        }

        protected void switch_item_mode()
        {
            Test_Battle_Character_Data test_battler = this.test_battler;
            int inventory_index = Index - 12;
            test_battler.Items[inventory_index].Type = test_battler.Items[inventory_index].Type == Item_Data_Type.Weapon ? Item_Data_Type.Item : Item_Data_Type.Weapon;
            test_battler.Items[inventory_index].Id = 0;
            update_inventory();
            refresh();
        }

        protected virtual void update_selecting_option()
        {
            Test_Battle_Character_Data test_battler = this.test_battler;
            switch(Index)
            {
                case 1:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Option_Selected = true;
                    break;
                case 9:
                    if (test_battler.Generic)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Option_Selected = true;
                    }
                    break;
                default:
                    Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
            }
        }

        protected void update_option_selected()
        {
            Test_Battle_Character_Data test_battler = this.test_battler;
            int text_length;
            switch (Index)
            {
                case 1:
                    text_length = test_battler.Identifier.Length;
                    test_battler.Identifier = Global.append_text_input(test_battler.Identifier);
                    if (text_length != test_battler.Identifier.Length)
                        Global.game_system.play_se(System_Sounds.Talk_Boop);
                    refresh();
                    break;
                case 9:
                    if (test_battler.Generic)
                    {
                        text_length = test_battler.Name.Length;
                        test_battler.Name = Global.append_text_input(test_battler.Name);
                        if (text_length != test_battler.Name.Length)
                            Global.game_system.play_se(System_Sounds.Talk_Boop);
                        refresh();
                    }
                    break;
                default:
                    break;
            }
        }

        protected void change_generic_actor()
        {
            Test_Battle_Character_Data test_battler = this.test_battler;
            Game_Actor temp_actor;
            temp_actor = Global.game_actors.new_actor();
            temp_actor.class_id = test_battler.Class_Id;
            test_battler.Level = Math.Min(test_battler.Level, temp_actor.level_cap());
            test_battler.Prepromote_Levels = 0;
            if (temp_actor.tier > 0)
            {
                test_battler.Prepromote_Levels += Constants.Actor.TIER0_LVL_CAP;
                test_battler.Prepromote_Levels += (temp_actor.tier - 1) * Constants.Actor.LVL_CAP;
            }
            for (int i = 1; i < Global.weapon_types.Count; i++)
            {
                if (Global.scene.scene_type == "Scene_Title")
                {
                    if (temp_actor.weapon_level_cap(Global.weapon_types[i]) > 0)
                        temp_actor.wexp_gain(Global.weapon_types[i],
                            Data_Weapon.WLVL_THRESHOLDS[Data_Weapon.WLVL_THRESHOLDS.Length - 1]);
                }
                else
                {
                    if (temp_actor.weapon_level_cap(Global.weapon_types[i]) > 0)
                        temp_actor.wexp_gain(Global.weapon_types[i], test_battler.WLvls[i - 1]);
                }
            }

            for (int i = 0; i < Weapons.Count; i++)
                if (Global.data_weapons.ContainsKey(Weapons[i]) && temp_actor.is_equippable(Global.data_weapons[Weapons[i]]))
                {
                    test_battler.Weapon_Id = Weapons[i];
                    break;
                }

            temp_actor.gender = 0;
            if (Global.content_exists(@"Graphics/Characters/" + temp_actor.map_sprite_name))
                test_battler.Gender = 0;
            else
            {
                temp_actor.gender = 1;
                if (Global.content_exists(@"Graphics/Characters/" + temp_actor.map_sprite_name))
                    test_battler.Gender = 1;
            }

            WLvl_Index = 0;
            for (int i = 0; i < Global.weapon_types.Count - 1; i++)
                if (temp_actor.weapon_level_cap(Global.weapon_types[i + 1]) > 0)
                {
                    WLvl_Index = i;
                    break;
                }

            Global.game_actors.temp_clear(temp_actor.id);
        }

        protected void change_actor()
        {
            Test_Battle_Character_Data test_battler = this.test_battler;
            Data_Actor actor_data = Global.data_actors[test_battler.Actor_Id];
            test_battler.Weapon_Id = (actor_data.Items[0][0] == 0 && actor_data.Items[0][1] > 0) ? actor_data.Items[0][1] : 1;
            test_battler.Level = actor_data.Level;
            test_battler.Prepromote_Levels = 0;
            test_battler.Tier = Global.data_classes[actor_data.ClassId].Tier;
        }

        protected void update_inventory()
        {
            Item_Data[] items = this.test_battler.Items;
            actor.set_items(items);
            /* @Debug
            for (int i = 0; i < this.test_battler.Items.Length; i++)
            {
                actor.items[i] = items[i]; // @Debug: this.test_battler.Items[i];
            }*/
            actor.setup_items(false);
            this.test_battler.Weapon_Id = actor.weapon_id;
        }

        protected int index
        {
            get { return Index; }
            set
            {
                while (Index != value)
                    if (value > Index)
                        move_down();
                    else
                        move_up();
                Index = value;
            }
        }

        protected void move_up()
        {
            int distance = Global.Input.speed_up_input() ? ROWS_AT_ONCE : 1;
            Module.scroll_window_move_up(ref Index, ref Scroll, distance, ROWS_AT_ONCE, max_index, true);
            //Index = (Index + max_index - 1) % max_index; //Debug
            //if (Index - 1 < Scroll + 1 && Scroll > 0)
            //    Scroll--;
            if (Scroll == 0 && Index - distance < 0)
                locked_inputs.Add(Inputs.Up);
            Cursor.set_loc(cursor_loc());
        }
        protected void move_down()
        {
            int distance = Global.Input.speed_up_input() ? ROWS_AT_ONCE : 1;
            Module.scroll_window_move_down(ref Index, ref Scroll, distance, ROWS_AT_ONCE, max_index, true);
            //Index = (Index + max_index + 1) % max_index; //Debug
            //if (Index > (ROWS_AT_ONCE - 2) + Scroll && Scroll < max_index - (ROWS_AT_ONCE))
            //    Scroll++;
            if (Scroll == max_index - ROWS_AT_ONCE && Index + distance >= max_index)
                locked_inputs.Add(Inputs.Down);
            Cursor.set_loc(cursor_loc());
        }
        #endregion

        #region Draw
        protected override void draw_background(SpriteBatch sprite_batch)
        {
            if (Visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Black_Background.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            foreach (Item_Icon_Sprite icon in Item_Icons)
                icon.draw(sprite_batch, Offset);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            foreach (FE_Text label in Data_Labels)
                label.draw(sprite_batch, Offset);
            foreach (FE_Text data in Data)
                data.draw(sprite_batch, Offset);

            foreach (FE_Text label in Stat_Labels)
                label.draw(sprite_batch);
            foreach (FE_Text_Int stat in Stats)
                stat.draw(sprite_batch);
            foreach (FE_Text label in Combat_Stat_Labels)
                label.draw(sprite_batch);
            foreach (FE_Text_Int stat in Combat_Stats)
                stat.draw(sprite_batch);
            Map_Sprite.draw(sprite_batch);
            // Cursor
            Cursor.draw(sprite_batch);
            Input_Cursor.draw(sprite_batch, Offset);
            sprite_batch.End();
        }
        #endregion
    }
}
#endif
