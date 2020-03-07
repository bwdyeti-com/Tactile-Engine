#if !MONOGAME && DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using FEXNA.Menus.Map;
using FEXNA.Windows.Command;
using FEXNA.Windows.Map;
using FEXNA.Windows.UserInterface.Command;
using FEXNA_Library;

namespace FEXNA
{
    enum Unit_Editor_Options { Unit, Add_Unit, Paste_Unit, Reinforcements, Options, Clear_Units, Mirror_Units, Playtest, Revert, Save, Quit }
    class Scene_Map_Unit_Editor : Scene_Map, IUnitEditorMapMenuHandler, IUnitEditorUnitMenuHandler
    {
        private static string Save_Name, Map_Data_Key;
        internal static string UnitDataKey { get; private set; }
        private static Map_Unit_Data _unitData, LastSavedUnitData;

        int Reinforcement_Index = -1;

        //@Debug: actually should not ever be used
        internal static Map_Unit_Data UnitData { get { return new Map_Unit_Data(_unitData); } }

        public Scene_Map_Unit_Editor()
        {
            Global.Audio.BgmFadeOut();
            Global.load_save_info = true;
            suspend_fade_in();
        }

        protected override void initialize_base()
        {
            Scene_Type = "Scene_Map_Unit_Editor";
            main_window();
            camera = new Camera(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT, Vector2.Zero);
        }

        new public void set_map()
        {
            set_map(Global.game_system.New_Chapter_Id, Map_Data_Key, "", "");
        }
        public override void set_map(string chapter_id, string map_data_key, string unit_data_key, string event_data_key)
        {
            if (string.IsNullOrEmpty(Save_Name))
            {
                if (!string.IsNullOrEmpty(chapter_id))
                    Save_Name = chapter_id;
                Map_Data_Key = map_data_key;
                UnitDataKey = unit_data_key;
            }
            // Set difficulty
            Global.game_system.Difficulty_Mode = Difficulty_Modes.Normal;
            foreach (var pair in Constants.Difficulty.DIFFICULTY_SAVE_APPEND)
                if (Save_Name.EndsWith("" + pair.Value))
                {
                    Global.game_system.Difficulty_Mode = pair.Key;
                    break;
                }

            chapter_id = "";
            /* //Debug
            if (Unit_Data != null)
            {
                HashSet<Vector2> keys = new HashSet<Vector2>();
                keys.UnionWith(Unit_Data.Units.Keys);
                Dictionary<Vector2, Data_Unit> fgdsf = new Dictionary<Vector2, Data_Unit>();
                foreach (Vector2 key in keys)
                //for (int i = 0; i < keys.Count; i++)
                {
                    Data_Unit unit = Unit_Data.Units[key];
                    fgdsf[key + new Vector2(2, 2)] = unit;
                }
                //Unit_Data.Units = fgdsf;
            }
            */

            Global.game_battalions = new Game_Battalions();
            Global.game_battalions.add_battalion(0);
            Global.game_battalions.current_battalion = 0;

            Global.game_actors = new Game_Actors();
            while (Global.game_map.units.Count > 0)
                Global.game_map.remove_unit(Global.game_map.units.First().Key);

            bool new_map = _unitData == null;
            reset_map(new_map);
            // Map Data
            Data_Map map_data;
            if (Map_Data_Key == "")
                map_data = new Data_Map();
            else
            {
                Data_Map loaded_map_data = get_map_data(Map_Data_Key);
                map_data = new Data_Map(loaded_map_data.values, loaded_map_data.GetTileset());
            }
            // Unit Data
            if (!string.IsNullOrEmpty(unit_data_key) && _unitData == null)
            {
                if (Global.content_exists(string.Format(@"Data/Map Data/Unit Data/{0}", UnitDataKey)))
                    _unitData = Map_Content[0].Load<Map_Unit_Data>(string.Format(@"Data/Map Data/Unit Data/{0}", UnitDataKey));
                else if (_unitData == null)
                    _unitData = new Map_Unit_Data();

                LastSavedUnitData = new Map_Unit_Data(_unitData);
            }
            // Event Data

            Global.game_state.setup(chapter_id, map_data, _unitData, event_data_key);
            if (Global.test_battler_1.Generic)
                Global.test_battler_1.Actor_Id = Global.game_actors.next_actor_id();
            set_map_texture();
            if (new_map)
            {
                Global.player.center();
                Global.game_system.Instant_Move = true;
                Global.game_state.update();
                Global.game_system.update();
            }
            else
                Global.game_map.highlight_test();
        }
        
        protected override void open_map_menu()
        {
            MapMenu = new UnitEditorMapMenuManager(this, _unitData.Reinforcements);
        }

        protected override bool update_menu_map()
        {
            if (MapMenu != null)
            {
                MapMenu.Update();
                if (MapMenu != null && MapMenu.Finished)
                    MapMenu = null;
                return true;
            }

            return false;
        }

        internal bool get_map_data(out string chapterId, out string mapDataKey, out string unitDataKey, out string eventDataKey)
        {
            chapterId = "";
            mapDataKey = "";
            unitDataKey = "";
            eventDataKey = "";

            Data_Chapter chapter = null;

            // Find best fit chapter
            var potential_chapters = Global.data_chapters
                .Where(x => Map_Data_Key.StartsWith(x.Key) && UnitDataKey.StartsWith(x.Key));
            if (!potential_chapters.Any())
                return false;
            chapter = potential_chapters
                .OrderByDescending(x => x.Key.Length)
                .First()
                .Value;

            chapterId = chapter.Id;
            mapDataKey = Map_Data_Key;
            unitDataKey = UnitDataKey;

            string event_key = UnitDataKey;
            // Set a default difficulty value //@Yeti
            if (unitDataKey.Last() == Constants.Difficulty.DIFFICULTY_EVENT_APPEND[Global.game_system.Difficulty_Mode])
                event_key = UnitDataKey.Substring(0, UnitDataKey.Length - 1);
            
            eventDataKey = event_key;
            if (Global.loaded_files.Contains(string.Format("Data/Map Data/Event Data/{0}",
                    event_key + Constants.Difficulty.DIFFICULTY_EVENT_APPEND[Global.game_system.Difficulty_Mode])))
                eventDataKey += ", " + string.Format("{0}{1}", event_key,
                    Constants.Difficulty.DIFFICULTY_EVENT_APPEND[Global.game_system.Difficulty_Mode]);

            return true;
        }

        #region Unit Command Menu
        protected override void open_unit_menu()
        {
            MapMenu = UnitEditorUnitMenuManager.CommandMenu(this);
        }
        
        private int new_team(int old_team, bool left)
        {
            if (left)
                old_team--;
            else
                old_team++;

            old_team--;
            old_team = ((old_team + Constants.Team.NUM_TEAMS) % Constants.Team.NUM_TEAMS);
            old_team++;

            return old_team;
        }
        
        public void move_unit()
        {
            _unitData.Units[Global.player.loc] = _unitData.Units[Global.game_map.get_selected_unit().loc];
            _unitData.Units.Remove(Global.game_map.get_selected_unit().loc);
        }
        #endregion

        #region IUnitEditorUnitMenuHandler
        public void UnitEditorUnitMenuEditUnit()
        {
            Game_Unit unit = Global.game_map.units[Global.game_system.Selected_Unit_Id];
            MapMenu = UnitEditorUnitMenuManager.UnitEditor(this, _unitData.Units[unit.loc]);
        }

        public void UnitEditorUnitMenuConfirmEditUnit()
        {
            Game_Unit unit = Global.game_map.units[Global.game_system.Selected_Unit_Id];
            if (Reinforcement_Index != -1)
            {
                string[] ary = Global.test_battler_1.to_string(unit.team);
                _unitData.Reinforcements[Reinforcement_Index] = new Data_Unit(ary[0], ary[1], ary[2]);
            }
            else
            {
                string[] ary = Global.test_battler_1.to_string(unit.team);
                _unitData.Units[Global.player.loc] = new Data_Unit(ary[0], ary[1], ary[2]);
            }
            UnitEditorUnitMenuClose();
        }

        public void UnitEditorUnitMenuMoveUnit()
        {
            Global.game_state.moving_editor_unit = true;
            Global.game_temp.menuing = false;
            close_map_menu();
            Global.game_map.move_range_visible = true;
            Global.game_map.highlight_test();
        }

        public void UnitEditorUnitMenuChangeTeam(bool increment)
        {
            Game_Unit unit = Global.game_map.units[Global.game_system.Selected_Unit_Id];
            int team = unit.team;
            team = new_team(team, !increment);

            Global.game_map.change_unit_team(unit.id, team);
            Global.game_map.refresh_move_ranges();
            Global.game_map.wait_for_move_update();

            Global.test_battler_1 = Test_Battle_Character_Data.from_data(
                _unitData.Units[unit.loc].type,
                _unitData.Units[unit.loc].identifier,
                _unitData.Units[unit.loc].data);
            string[] ary = Global.test_battler_1.to_string(unit.team);
            _unitData.Units[Global.player.loc] = new Data_Unit(ary[0], ary[1], ary[2]);
        }

        public void UnitEditorUnitMenuCopyUnit()
        {
            Game_Unit unit = Global.game_map.units[Global.game_system.Selected_Unit_Id];
            Global.test_battler_1 = Test_Battle_Character_Data.from_data(
                _unitData.Units[unit.loc].type, _unitData.Units[unit.loc].identifier, _unitData.Units[unit.loc].data);
            if (Global.test_battler_1.Generic)
                Global.test_battler_1.Actor_Id = Global.game_actors.next_actor_id();
            Global.game_temp.menuing = false;
            close_map_menu();
            Global.game_system.Selected_Unit_Id = -1;
            Global.game_map.move_range_visible = true;
            Global.game_map.highlight_test();
        }

        public void UnitEditorUnitMenuRemoveUnit()
        {
            Game_Unit unit = Global.game_map.units[Global.game_system.Selected_Unit_Id];
            // Remove unit and refresh the map
            _unitData.Units.Remove(unit.loc);
            set_map(Global.game_system.New_Chapter_Id, Map_Data_Key, "", "");
            // Close menu
            Global.game_temp.menuing = false;
            close_map_menu();
            Global.game_system.Selected_Unit_Id = -1;
            Global.game_map.move_range_visible = true;
            Global.game_map.highlight_test();
        }

        public void UnitEditorUnitMenuClose()
        {
            Global.game_temp.menuing = false;
            MapMenu = null;
            Global.game_system.Selected_Unit_Id = -1;
            Reinforcement_Index = -1;
            set_map(Global.game_system.New_Chapter_Id, Map_Data_Key, "", "");
            Global.game_map.move_range_visible = true;
            Global.game_map.highlight_test();
        }
        #endregion

        #region IUnitEditorMapMenuHandler
        public void UnitEditorMapMenuAddUnit()
        {
            // Add a unit and refresh the map
            _unitData.Units.Add(Global.player.loc, new Data_Unit("character", "", "1|Actor ID\n1|Team\n0|AI Priority\n3|AI Mission"));
            set_map(Global.game_system.New_Chapter_Id, Map_Data_Key, "", "");

            // Close menu
            Global.game_temp.menuing = false;
            MapMenu = null;
            Global.game_map.highlight_test();
        }

        public void UnitEditorMapMenuPasteUnit()
        {
            // Add a unit and refresh the map
            string[] ary = Global.test_battler_1.to_string();
            _unitData.Units.Add(Global.player.loc, new Data_Unit(ary[0], ary[1], ary[2]));
            set_map(Global.game_system.New_Chapter_Id, Map_Data_Key, "", "");

            // Close menu
            Global.game_temp.menuing = false;
            MapMenu = null; 
            Global.game_map.highlight_test();
        }

        public void UnitEditorMapMenuEditReinforcement(int index)
        {
            Reinforcement_Index = index;
            
            Global.game_map.add_reinforcement_unit(-1, Config.OFF_MAP, Reinforcement_Index, "");
            Global.game_system.Selected_Unit_Id = Global.game_map.last_added_unit.id;
            MapMenu = UnitEditorUnitMenuManager.UnitEditor(
                this,
                _unitData.Reinforcements[Reinforcement_Index],
                true);
        }
        public void UnitEditorMapMenuAddReinforcement(int index)
        {
            // Add a unit
            _unitData.Reinforcements.Insert(index,
                new Data_Unit("character", "", "1|Actor ID\n1|Team\n0|AI Priority\n3|AI Mission"));
        }
        public void UnitEditorMapMenuPasteReinforcement(int index)
        {
            // Add a unit
            string[] ary = Global.test_battler_1.to_string();
            _unitData.Reinforcements.Insert(index,
                new Data_Unit(ary[0], ary[1], ary[2]));
        }
        public void UnitEditorMapMenuDeleteReinforcement(int index)
        {
            _unitData.Reinforcements.RemoveAt(index);
        }

        public void UnitEditorMapMenuClearUnits()
        {
            _unitData.Units.Clear();
            _unitData.Reinforcements.Clear();
            set_map(Global.game_system.New_Chapter_Id, Map_Data_Key, "", "");

            // Close menu
            Global.game_temp.menuing = false;
            MapMenu = null; 
            Global.game_map.highlight_test();
        }

        public void UnitEditorMapMenuMirrorUnits()
        {
            Dictionary<Vector2, Data_Unit> unit_data = _unitData.Units
                .Select(p => new KeyValuePair<Vector2, Data_Unit>(new Vector2(
                    (Global.game_map.width - 1) - p.Key.X, p.Key.Y), p.Value))
                .ToDictionary(p => p.Key, p => p.Value);
            _unitData.Units = unit_data;
            set_map(Global.game_system.New_Chapter_Id, Map_Data_Key, "", "");

            // Close menu
            Global.game_temp.menuing = false;
            MapMenu = null;
            Global.game_map.highlight_test();
        }

        public void UnitEditorMapMenuPlaytest()
        {
            string chapter_id, map_data_key, unit_data_key, event_data_key;
            bool valid_map_data = get_map_data(
                out chapter_id, out map_data_key, out unit_data_key, out event_data_key);
            if (valid_map_data)
            {
                Global.game_system.play_se(System_Sounds.Confirm);

                Difficulty_Modes difficulty = Global.game_system.Difficulty_Mode;
                Global.game_system = new Game_System();
                Global.game_battalions = new Game_Battalions();
                Global.game_actors = new Game_Actors();
                Global.game_system.Difficulty_Mode = difficulty;

                // Switch to the best fit save file
                Global.game_system.New_Chapter_Id = chapter_id;
                Global.current_save_id = -1;
                if (Global.save_files_info != null)
                {
                    var matching_files = Global.save_files_info
                        .Where(x => x.Value.chapter_available(
                            Global.game_system.New_Chapter_Id));
                    matching_files = matching_files.ToList();

                    if (matching_files.Any())
                        Global.current_save_id = matching_files
                            .OrderBy(x => x.Key)
                            .First()
                            .Key;
                }

                // Failed to find a save
                if (Global.current_save_id < 0)
                    Global.current_save_id = 1;

                Global.scene_change("Scene_Map_Playtest");
                Global.game_temp.menuing = false;
                MapMenu = null; 
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        public void UnitEditorMapMenuRevert()
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            _unitData = new Map_Unit_Data(LastSavedUnitData);
            set_map(Global.game_system.New_Chapter_Id, Map_Data_Key, "", "");

            // Close menu
            Global.game_temp.menuing = false;
            MapMenu = null; 
            Global.game_map.highlight_test();
        }

        public void UnitEditorMapMenuSave()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            path += @"\SavedUnitData\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (XmlWriter writer = XmlWriter.Create(
                path + Save_Name + ".xml", settings))
            {
                IntermediateSerializer.Serialize(writer, _unitData, null);
            }
            LastSavedUnitData = new Map_Unit_Data(_unitData);
        }

        public void UnitEditorMapMenuQuit()
        {
            Global.quit();
        }
        #endregion

#if DEBUG
        internal override void open_debug_menu() { }
#endif

        protected override bool update_soft_reset() { return false; }
    }
}
#endif