using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Graphics.Map;
using FEXNA.Menus.Worldmap;
using FEXNA.Windows;
using FEXNA.Windows.Command;
using FEXNA.Windows.Map;
using FEXNA.Windows.Map.Items;
using FEXNA.Windows.UserInterface.Command;
using FEXNA_Library;

namespace FEXNA
{
    enum Worldmap_Phases { Fade_In, Command_Process, Controls_Fade, Worldmap_Event, Fade_Out, Return_To_Title }
    enum ChapterCommands { StartChapter, SelectPrevious, Options, Unit, Manage, Ranking }
    class Scene_Worldmap : Scene_Base, IWorldmapMenuHandler
    {
        protected const int WIDTH = 120;

        protected Worldmap_Phases Phase = Worldmap_Phases.Fade_In;
        protected int Fade_Timer = Constants.WorldMap.WORLDMAP_FADE_TIME;
        private int Timer = 0;
        private bool Zoomed_Map_Visible = false;
        private int Zoomed_Fade_Timer = 0;
        private Vector2 Offset = Vector2.Zero, Target_Loc;
        private Vector2 MenuOffset = Vector2.Zero;
        private float EventScrollSpeed = Constants.WorldMap.WORLDMAP_EVENT_SCROLL_SPEED;
        private float ScrollSpeed = 0f;
        private Character_Sprite Lord_Sprite;
        private Sprite Map, Minimap, Minimap_Backing, Zoomed_Out_Map;
        private World_Minimap_ViewArea ViewArea;

        private WorldmapMenuData MenuData;
        private WorldmapMenuManager MenuManager;

        private Parchment_Info_Window Hard_Mode_Blocked_Window;

        private List<Flashing_Worldmap_Object> Worldmap_Objects = new List<Flashing_Worldmap_Object>();
        private Worldmap_Beacon Beacon;
        private List<Worldmap_Unit> Units = new List<Worldmap_Unit>();
        private List<Worldmap_Unit> Clearing_Units = new List<Worldmap_Unit>();
        private int Tracking_Unit = -1;

        #region Accessors
        public Vector2 target_loc
        {
            set
            {
                SetTargetLoc(value);
                EventScrollSpeed = Constants.WorldMap.WORLDMAP_EVENT_SCROLL_SPEED;
                Tracking_Unit = -1;
                foreach (Worldmap_Unit unit in Units)
                    unit.remove_all_tracking();
            }
        }

        public float scroll_speed { set { EventScrollSpeed = Math.Max(0.001f, value); } }

        public bool scrolling
        {
            get
            {
                if (Zoomed_Fade_Timer > 0)
                    return true; return Offset != Target_Loc;
            }
        }

        public bool units_moving
        {
            get
            {
                for (int i = 0; i < Units.Count; i++)
                    if (Units[i].moving)
                        return true;
                return false;
            }
        }

        protected override bool has_convo_scene_button { get { return false; } }
        #endregion

        public Scene_Worldmap(string completedChapterId)
        {
            initialize_base();
            Scene_Type = "Scene_Worldmap";
            Global.game_map = null;

            if (Constants.WorldMap.SEPARATE_CHAPTERS_INTO_ARCS)
                MenuData = new WorldmapArcsMenuData(completedChapterId);
            else
                MenuData = new WorldmapMenuData(completedChapterId);

            //Global.save_file.Difficulty = Global.game_system.Difficulty_Mode; //Debug
            Global.game_system.Difficulty_Mode = Global.save_file.Difficulty;

            initialize_images();
            SetTargetLoc(MenuData.Chapter.World_Map_Loc -
                Constants.WorldMap.WORLDMAP_MAP_SPRITE_OFFSET);
            Offset = Target_Loc;

            Global.Chapter_Text_Content.Unload();
            Global.chapter_text = Global.Chapter_Text_Content.Load<Dictionary<string, string>>(@"Data/Text/Worldmap");
        }

        protected void initialize_images()
        {
            // Lord
            Lord_Sprite = new Character_Sprite();
            Lord_Sprite.draw_offset = new Vector2(0, 4); // (0, 8); //Debug
            Lord_Sprite.facing_count = 3;
            Lord_Sprite.frame_count = 3;
            Lord_Sprite.stereoscopic = Config.MAP_UNITS_DEPTH;
            Lord_Sprite.mirrored = Constants.Team.flipped_map_sprite(
                Constants.Team.PLAYER_TEAM);
            // Map
            Map = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Panoramas/Worldmap"));
            Map.stereoscopic = Config.MAP_MAP_DEPTH;
            Zoomed_Out_Map = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Panoramas/Worldmap"));
            Zoomed_Out_Map.loc = new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2;
            Zoomed_Out_Map.offset = new Vector2(Zoomed_Out_Map.texture.Width, Zoomed_Out_Map.texture.Height) / 2;
            Zoomed_Out_Map.scale = new Vector2(
                Math.Max((float)(Config.WINDOW_WIDTH + Config.WMAP_ZOOMED_DEPTH * 4) / Zoomed_Out_Map.texture.Width,
                (float)(Config.WINDOW_HEIGHT) / Zoomed_Out_Map.texture.Height));//Debug
            Zoomed_Out_Map.opacity = 0;
            Zoomed_Out_Map.stereoscopic = Config.WMAP_ZOOMED_DEPTH;

            Minimap = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Panoramas/Worldmap"));
            Minimap.scale = Constants.WorldMap.WORLDMAP_MINIMAP_SCALE;
            Minimap.loc = new Vector2(Config.WINDOW_WIDTH - 1, Config.WINDOW_HEIGHT - 1) +
                Constants.WorldMap.WORLDMAP_MINIMAP_OFFSET;
            Minimap.offset = new Vector2(Minimap.texture.Width, Minimap.texture.Height);
            Minimap.stereoscopic = Config.WMAP_MINIMAP_DEPTH;

            Minimap_Backing = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            Minimap_Backing.scale = new Vector2((((int)Math.Round(Minimap.scale.X * Minimap.texture.Width) + 2) / (float)Minimap_Backing.texture.Width),
                (((int)Math.Round(Minimap.scale.Y * Minimap.texture.Height) + 2) / (float)Minimap_Backing.texture.Height));
            Minimap_Backing.loc = new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) +
                Constants.WorldMap.WORLDMAP_MINIMAP_OFFSET;
            Minimap_Backing.offset = new Vector2(Minimap_Backing.texture.Width, Minimap_Backing.texture.Height);
            Minimap_Backing.tint = new Color(0, 0, 0, 255);
            Minimap_Backing.stereoscopic = Config.WMAP_MINIMAP_DEPTH;

            ViewArea = new World_Minimap_ViewArea(new Vector2((int)Math.Round(Config.WINDOW_WIDTH * Minimap.scale.X) + 2,
                (int)Math.Round(Config.WINDOW_HEIGHT * Minimap.scale.Y) + 2));
            ViewArea.loc = Minimap.loc - new Vector2((int)Math.Round(Minimap.scale.X * Minimap.texture.Width),
                (int)Math.Round(Minimap.scale.Y * Minimap.texture.Height)) - new Vector2(1, 1) -
                new Vector2((int)Math.Round(Config.WINDOW_WIDTH * Minimap.scale.X) + 2,
                (int)Math.Round(Config.WINDOW_HEIGHT * Minimap.scale.Y) + 2) / 2;
            ViewArea.stereoscopic = Config.WMAP_MINIMAP_DEPTH;

            MenuManager = new WorldmapMenuManager(this, MenuData);
        }

        protected void start_chapter()
        {
            start_chapter(MenuData.Chapter, MenuData.GetSelectedPreviousChapters());
        }
        protected virtual void start_chapter(Data_Chapter chapter,
            Dictionary<string, string> selectedPreviousChapters)
        {
            Difficulty_Modes difficulty = Global.game_system.Difficulty_Mode;
            var validPreviousChapters =
                Global.save_file.valid_previous_chapters(chapter.Id);
            if (chapter.Standalone || validPreviousChapters.Count == 0)
            {
                Global.game_system.reset();
                Global.game_system.reset_event_variables();
                //Event_Processor.reset_variables(); //Debug
                int battalion_index = chapter.Battalion;
                Global.game_battalions.add_battalion(battalion_index);
                Global.game_battalions.current_battalion = battalion_index;
            }
            else
            {
                WorldmapLoadData(chapter.Id, selectedPreviousChapters);
                Global.game_actors.heal_battalion();
                Global.battalion.refresh_deployed();
            }
            Global.game_system.Difficulty_Mode = difficulty;
            //if (Global.game_system.Style != Mode_Styles.Classic) // Save file is about to be set to null //Debug
            //    Global.save_file.Difficulty = Global.game_system.Difficulty_Mode;

            Global.game_system.New_Chapter_Id = chapter.Id;
            Global.game_system.new_chapter(chapter.Prior_Chapters, chapter.Id, selectedPreviousChapters);
            Global.game_temp = new Game_Temp();
            Global.current_save_info.SetStartedChapter(chapter.Id);
            Global.save_file = null;
            Global.scene_change("Start_Chapter");
        }

        private void SetTargetLoc(Vector2 loc)
        {
            Target_Loc = loc;
            ScrollSpeed = 0f;
        }

        #region Update
        public override void update()
        {
            update_message();
            update_data();
            Player.update_anim();

            if (update_soft_reset())
                return;

            if (MenuManager != null)
                MenuManager.Update(Fade_Timer == 0);
            update_worldmap_command();

            ViewArea.update();
            update_frame();
            update_event_objects();
            update_loc();
        }

        private void update_worldmap_command()
        {
            switch (Phase)
            {
                case Worldmap_Phases.Fade_In:
                    switch (Timer)
                    {
                        default:
                            if (Fade_Timer > 0)
                                Fade_Timer--;
                            if (Fade_Timer == Constants.WorldMap.WORLDMAP_FADE_TIME / 4)
                                if (!MenuData.Classic)
                                    Global.Audio.PlayBgm(Constants.WorldMap.WORLDMAP_THEME);
                            if (Fade_Timer == 0)
                                Phase = Worldmap_Phases.Command_Process;
                            break;
                    }
                    break;
                case Worldmap_Phases.Command_Process:
                    if (MenuData.Classic)
                        select_chapter_fade();
                    break;
                case Worldmap_Phases.Controls_Fade:
                    if (Hard_Mode_Blocked_Window != null)
                    {
                        Hard_Mode_Blocked_Window.update();
                        if (Hard_Mode_Blocked_Window.is_ready)
                            if (Hard_Mode_Blocked_Window.selected_index().IsSomething)
                            {
                                Global.game_system.play_se(System_Sounds.Confirm);
                                Hard_Mode_Blocked_Window = null;
                            }
                    }
                    else
                    {
                        if (Fade_Timer > 0)
                        {
                            Fade_Timer--;
                            MenuOffset += new Vector2(-1, 0);
                        }
                        if (Fade_Timer == 0 && !scrolling)
                        {
                            MenuManager = null;
                            start_chapter_worldmap_event();
                        }
                    }
                    break;
                case Worldmap_Phases.Worldmap_Event:
                    if (!Global.game_system.is_interpreter_running)
                    {
                        Phase = Worldmap_Phases.Fade_Out;
                        Fade_Timer = Constants.WorldMap.WORLDMAP_FADE_TIME;
                        Global.Audio.BgmFadeOut(Constants.WorldMap.WORLDMAP_FADE_TIME);
                    }
                    break;
                case Worldmap_Phases.Fade_Out:
                    if (Fade_Timer > 0)
                        Fade_Timer--;
                    if (Fade_Timer == 0)
                        start_chapter();
                    break;
                case Worldmap_Phases.Return_To_Title:
                    if (Fade_Timer > 0)
                        Fade_Timer--;
                    if (Fade_Timer == 0)
                    {
                        Global.scene_change("Scene_Title_Load");
                        MenuManager = null;
                    }
                    break;
            }
        }

        protected void update_event_objects()
        {
            Flashing_Worldmap_Object.update_flash();
            if (Beacon != null)
                Beacon.update();
            foreach (Flashing_Worldmap_Object sprite in Worldmap_Objects)
                sprite.update();
            int i = 0;
            while (i < Clearing_Units.Count)
            {
                Clearing_Units[i].update();
                if (Clearing_Units[i].finished)
                    Clearing_Units.RemoveAt(i);
                else
                    i++;
            }
            i = 0;
            while (i < Units.Count)
            {
                Units[i].update();
                if (Units[i].tracking)
                {
                    Tracking_Unit = i;
                }
                if (Tracking_Unit == i)
                    update_tracking_unit();
                if (Units[i].is_removed)
                {
                    if (i == Tracking_Unit)
                        Tracking_Unit = -1;
                    else
                        Tracking_Unit--;
                    Clearing_Units.Add(Units[i]);
                    Units.RemoveAt(i);
                }
                else
                    i++;
            }
            if (Zoomed_Fade_Timer > 0)
            {
                Zoomed_Fade_Timer--;
                Zoomed_Out_Map.opacity = 256 *
                    (Zoomed_Map_Visible ?
                        (Constants.WorldMap.WORLDMAP_ZOOM_FADE_TIME - Zoomed_Fade_Timer) :
                        Zoomed_Fade_Timer) /
                    Constants.WorldMap.WORLDMAP_ZOOM_FADE_TIME;
            }
        }

        readonly static Vector2 TRACKING_OFFSET = new Vector2(48, 24);
        protected void update_tracking_unit()
        {
            if (!Units[Tracking_Unit].moving)
            {
                Tracking_Unit = -1;
                return;
            }
            // X
            if (Units[Tracking_Unit].loc.X > Offset.X + TRACKING_OFFSET.X)
            {
                if (Units[Tracking_Unit].loc.X > Worldmap_Unit.tracking_unit_max.X + TRACKING_OFFSET.X)
                    Offset.X = Worldmap_Unit.tracking_unit_max.X;
                else
                    Offset.X = Units[Tracking_Unit].loc.X - TRACKING_OFFSET.X;
            }
            else if (Units[Tracking_Unit].loc.X < Offset.X - TRACKING_OFFSET.X)
            {
                if (Units[Tracking_Unit].loc.X < Worldmap_Unit.tracking_unit_min.X - TRACKING_OFFSET.X)
                    Offset.X = Worldmap_Unit.tracking_unit_min.X;
                else
                    Offset.X = Units[Tracking_Unit].loc.X + TRACKING_OFFSET.X;
            }
            // Y
            if (Units[Tracking_Unit].loc.Y > Offset.Y + TRACKING_OFFSET.Y)
            {
                if (Units[Tracking_Unit].loc.Y > Worldmap_Unit.tracking_unit_max.Y + TRACKING_OFFSET.Y)
                    Offset.Y = Worldmap_Unit.tracking_unit_max.Y;
                else
                    Offset.Y = Units[Tracking_Unit].loc.Y - TRACKING_OFFSET.Y;
            }
            else if (Units[Tracking_Unit].loc.Y < Offset.Y - TRACKING_OFFSET.Y)
            {
                if (Units[Tracking_Unit].loc.Y < Worldmap_Unit.tracking_unit_min.Y - TRACKING_OFFSET.Y)
                    Offset.Y = Worldmap_Unit.tracking_unit_min.Y;
                else
                    Offset.Y = Units[Tracking_Unit].loc.Y + TRACKING_OFFSET.Y;
            }

            SetTargetLoc(Offset);
        }

        public override void update_data()
        {
            Global.game_system.update();
        }

        protected void update_loc()
        {
            if (Offset != Target_Loc)
            {
                // If close to the target
                if ((Offset - Target_Loc).Length() <= 0.5f)
                {
                    Offset = Target_Loc;
                    ScrollSpeed = 0f;
                }
                else
                {
                    // Get an offset vector 1/4 the way to the target
                    Vector2 offset = (Target_Loc + Offset * 3) / 4;
                    offset -= Offset;

                    float scroll_speed;
                    if (Phase == Worldmap_Phases.Worldmap_Event)
                        scroll_speed = EventScrollSpeed;
                    else
                    {
                        // Use a smoother scroll for mouse controls
                        if (Input.ControlScheme == ControlSchemes.Mouse)
                        {
                            if (ScrollSpeed < 1)
                                ScrollSpeed = 1f;
                            else
                                ScrollSpeed *= 2;
                            ScrollSpeed = Math.Min(ScrollSpeed,
                                Constants.WorldMap.WORLDMAP_SCROLL_SPEED);
                            scroll_speed = ScrollSpeed;
                        }
                        else
                            scroll_speed = Constants.WorldMap.WORLDMAP_SCROLL_SPEED;
                    }

                    // If the offset is too big of a jump, use the scroll speed instead
                    if (offset.Length() > scroll_speed)
                    {
                        offset.Normalize();
                        offset *= scroll_speed;
                    }
                    Offset += offset;
                }
            }
        }

        protected void update_frame()
        {
            Lord_Sprite.frame = Global.game_system.unit_anim_idle_frame;
        }

        protected void select_chapter_fade()
        {
            Phase = Worldmap_Phases.Controls_Fade;
            Fade_Timer = MenuData.Classic ?
                1 : Constants.WorldMap.WORLDMAP_CONTROLS_FADE_TIME;
            if (Constants.WorldMap.HARD_MODE_BLOCKED.Contains(MenuData.ChapterId) &&
                Global.game_system.Difficulty_Mode > Difficulty_Modes.Normal)
            {
                Hard_Mode_Blocked_Window = new Parchment_Info_Window();
                Hard_Mode_Blocked_Window.set_text(@"This chapter does not yet have
hard mode data, and will be
loaded in normal mode. Sorry!");
                Hard_Mode_Blocked_Window.size = new Vector2(160, 64);
                Hard_Mode_Blocked_Window.loc = new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2 - Hard_Mode_Blocked_Window.size / 2;

                Global.game_system.Difficulty_Mode = Difficulty_Modes.Normal;
            }
        }

        protected virtual void start_chapter_worldmap_event()
        {
            Phase = Worldmap_Phases.Worldmap_Event;
            Map_Event_Data events =
                Global.Content.Load<Map_Event_Data>(@"Data/Map Data/Event Data/Worldmap");
            int event_index = 0;
            for (; event_index < events.Events.Count; event_index++)
                if (events.Events[event_index].name == MenuData.ChapterId + "Worldmap")
                    break;
            if (event_index >= events.Events.Count)
            {
                Phase = Worldmap_Phases.Fade_Out;
                Fade_Timer = Constants.WorldMap.WORLDMAP_FADE_TIME;
                Global.Audio.BgmFadeOut(Constants.WorldMap.WORLDMAP_FADE_TIME);
            }
            else
                Global.game_system.add_event(events.Events[event_index]);
        }
        #endregion

        #region ISetupMenuHandler
        public void SetupSave()
        {
            // Save file
            Save_Data_Calling = true;
        }

        #region IWorldmapMenuHandler
        public void WorldmapStartChapter()
        {
            // Start chapter
            select_chapter_fade();
        }

        public void WorldmapChapterChanged(Data_Chapter chapter)
        {
            // Update lord map sprite location
            Lord_Sprite.loc = chapter.World_Map_Loc;
            Lord_Sprite.texture = Scene_Map.get_team_map_sprite(
                Constants.Team.PLAYER_TEAM,
                Global.game_actors[chapter.World_Map_Lord_Id].map_sprite_name);

            if (Lord_Sprite.texture != null)
                Lord_Sprite.offset = new Vector2(
                    (Lord_Sprite.texture.Width / Lord_Sprite.frame_count) / 2,
                    (Lord_Sprite.texture.Height / Lord_Sprite.facing_count) - 8);

            SetTargetLoc(chapter.World_Map_Loc -
                Constants.WorldMap.WORLDMAP_MAP_SPRITE_OFFSET);
        }

        public void WorldmapLoadData(
            string chapterId,
            Dictionary<string, string> previousChapters)
        {
            Global.save_file.load_data(chapterId, previousChapters, "");
        }

        public void WorldmapExit()
        {
            Phase = Worldmap_Phases.Return_To_Title;
            Fade_Timer = Constants.WorldMap.WORLDMAP_FADE_TIME;
            Global.Audio.BgmFadeOut(Constants.WorldMap.WORLDMAP_FADE_TIME);
        }
        #endregion
        #endregion

        #region Message
        protected override void main_window()
        {
            Message_Window = new Window_Worldmap_Message();
            Message_Window.stereoscopic = Config.CONVO_TEXT_DEPTH;
            Message_Window.face_stereoscopic = Config.CONVO_FACE_DEPTH;
        }

        public override void event_skip()
        {
            Global.Audio.BgmFadeOut(Constants.WorldMap.WORLDMAP_FADE_TIME);
            start_chapter();
        }
        #endregion

        #region Events
        public void add_dot(int team, Vector2 loc)
        {
            Flashing_Worldmap_Object worldmap_object = new Worldmap_Dot(team);
            worldmap_object.loc = loc;
            worldmap_object.stereoscopic = Config.MAP_UNITS_DEPTH;
            Worldmap_Objects.Add(worldmap_object);
        }

        public void add_arrow(int team, int speed, Vector2[] waypoints)
        {
            Flashing_Worldmap_Object worldmap_object = new Worldmap_Arrow(team, speed, waypoints);
            worldmap_object.stereoscopic = Config.MAP_UNITS_DEPTH;
            Worldmap_Objects.Add(worldmap_object);
        }

        public void remove_dots()
        {
            // Remove all dots
            Worldmap_Objects.Clear();
        }
        public void remove_dot(int index)
        {
            // Negative indices count from the end of the list
            if (index < 0)
                index = Worldmap_Objects.Count + index;

#if DEBUG
            System.Diagnostics.Debug.Assert(index < Worldmap_Objects.Count,
                string.Format("Tried to remove a world map object at index {0},\nbut only {1} object{2} exist{3}.",
                    index, Worldmap_Objects.Count,
                    Worldmap_Objects.Count == 1 ? "" : "s",
                    Worldmap_Objects.Count == 1 ? "s" : ""));
#endif
            Worldmap_Objects.RemoveAt(index);
        }

        public void add_beacon(Vector2 loc)
        {
            Beacon = new Worldmap_Beacon();
            Beacon.loc = loc;
            Beacon.stereoscopic = Config.MAP_UNITS_DEPTH;
        }

        public void remove_beacon()
        {
            Beacon = null;
        }

        public void add_unit(int team, string filename, Vector2 loc)
        {
            Worldmap_Unit unit = new Worldmap_Unit(team, filename);
            unit.loc = loc;
            unit.stereoscopic = Config.MAP_UNITS_DEPTH;
            Units.Add(unit);
        }

        public void queue_unit_move(int index, int speed, Vector2[] waypoints)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(index < Units.Count, string.Format(
                "Trying to move a world map sprite, but\nthe index is past the end of the unit list\nIndex: {0}, Unit count: {1}",
                index, Units.Count));
#endif   
            Units[index].queue_move(speed, waypoints);
        }
        public void queue_unit_idle(int index)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(index < Units.Count, string.Format(
                "Trying to idle a world map sprite, but\nthe index is past the end of the unit list\nIndex: {0}, Unit count: {1}",
                index, Units.Count));
#endif   
            Units[index].queue_idle();
        }
        public void queue_unit_pose(int index)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(index < Units.Count, string.Format(
                "Trying to pose a world map sprite, but\nthe index is past the end of the unit list\nIndex: {0}, Unit count: {1}",
                index, Units.Count));
#endif   
            Units[index].queue_pose();
        }

        public void queue_unit_remove(int index, bool immediately, bool kill)
        {
            if (immediately)
            {
                if (index == Tracking_Unit)
                    Tracking_Unit = -1;
                else
                    Tracking_Unit--;
                Units[index].remove(kill);
                Clearing_Units.Add(Units[index]);
                Units.RemoveAt(index);
            }
            else
                Units[index].queue_remove(kill);
        }

        public void queue_unit_tracking(int index, Vector2 min, Vector2 max)
        {
            Units[index].queue_tracking(min, max);
        }

        public void clear_removing_units()
        {
            int i = 0;
            while (i < Units.Count)
            {
                Units[i].remove_if_queued();
                if (Units[i].is_removed)
                {
                    if (i == Tracking_Unit)
                        Tracking_Unit = -1;
                    else
                        Tracking_Unit--;
                    Clearing_Units.Add(Units[i]);
                    Units.RemoveAt(i);
                }
                else
                    i++;
            }
        }

        public bool zoomed_map_visible
        {
            set
            {
                Zoomed_Map_Visible = value;
                Zoomed_Fade_Timer = Constants.WorldMap.WORLDMAP_ZOOM_FADE_TIME;
                Zoomed_Out_Map.opacity = Zoomed_Map_Visible ? 0 : 255;
            }
        }
        #endregion

        #region Draw
        public override void draw(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            Vector2 offset = new Vector2((int)Offset.X, (int)Offset.Y) -
                (new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2);

            // Draws the map
            device.SetRenderTarget(render_targets[1]);
            device.Clear(Color.Transparent);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Map.draw(sprite_batch, offset + Constants.WorldMap.WORLDMAP_MAP_OFFSET);
            sprite_batch.End();

            // Draw world map event objects
            if (Phase >= Worldmap_Phases.Worldmap_Event && Phase != Worldmap_Phases.Return_To_Title)
            {
                draw_events(sprite_batch, Zoomed_Map_Visible ? Vector2.Zero : offset);
                draw_message(sprite_batch, device, render_targets);
            }

            if (!MenuData.Classic)
            {
                // Draw controls and menus
                if (Phase < Worldmap_Phases.Worldmap_Event || Phase == Worldmap_Phases.Return_To_Title)
                {
                    // Copies the menus onto the map
                    int alpha = 255;
                    if (Phase == Worldmap_Phases.Controls_Fade)
                        alpha = Fade_Timer * 255 / Constants.WorldMap.WORLDMAP_CONTROLS_FADE_TIME;

                    // Draw minimap and lord
                    device.SetRenderTarget(render_targets[0]);
                    device.Clear(Color.Transparent);
                    draw_map_objects(sprite_batch, offset);
                    draw_render_target(
                        sprite_batch, device,
                        render_targets[0], render_targets[1],
                        Vector2.Zero, alpha);
                    // Draw menus
                    if (MenuManager != null)
                    {
                        device.SetRenderTarget(render_targets[0]);
                        device.Clear(Color.Transparent);
                        MenuManager.Draw(sprite_batch, device, render_targets);
                        draw_render_target(
                            sprite_batch, device,
                            render_targets[0], render_targets[1],
                            MenuOffset, alpha);
                    }
                }
            }

            device.SetRenderTarget(render_targets[1]);
            if (Hard_Mode_Blocked_Window != null)
                Hard_Mode_Blocked_Window.draw(sprite_batch);

            // Draws the composite image, to allow fading the whole thing
            int fade_alpha = 255;
            if (Phase != Worldmap_Phases.Controls_Fade)
                fade_alpha = ((Phase == Worldmap_Phases.Fade_Out ||
                    Phase == Worldmap_Phases.Return_To_Title) ?
                        Fade_Timer :
                        (Constants.WorldMap.WORLDMAP_FADE_TIME - Fade_Timer)) *
                    255 / Constants.WorldMap.WORLDMAP_FADE_TIME;
            draw_render_target(
                sprite_batch, device,
                render_targets[1], render_targets[0],
                Vector2.Zero, fade_alpha,
                true);

            device.SetRenderTarget(render_targets[0]);

            base.draw(sprite_batch, device, render_targets);
        }

        private void draw_render_target(
            SpriteBatch spriteBatch,
            GraphicsDevice device,
            RenderTarget2D source,
            RenderTarget2D target,
            Vector2 loc,
            int alpha,
            bool clearTarget = false)
        {
            device.SetRenderTarget(target);
            if (clearTarget)
                device.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(source, loc, new Color(alpha, alpha, alpha, alpha));
            spriteBatch.End();
        }

        protected void draw_map_objects(SpriteBatch sprite_batch, Vector2 offset)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Lord_Sprite.draw(sprite_batch, offset);
            // Minimap
            Minimap_Backing.draw(sprite_batch);
            Minimap.draw(sprite_batch);
            ViewArea.draw(
                sprite_batch,
                -(Offset + Constants.WorldMap.WORLDMAP_MAP_OFFSET -
                    Constants.WorldMap.WORLDMAP_MAP_SPRITE_OFFSET) * Minimap.scale);
            sprite_batch.End();
        }

        protected void draw_events(SpriteBatch sprite_batch, Vector2 offset)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Zoomed_Out_Map.draw(sprite_batch);
            if (Beacon != null)
                Beacon.draw(sprite_batch, offset);
            foreach (Flashing_Worldmap_Object sprite in Worldmap_Objects)
                sprite.draw(sprite_batch, offset);
            foreach (Worldmap_Unit unit in Units.OrderBy(x => x.loc.Y))
                unit.draw(sprite_batch, offset);
            sprite_batch.End();

            Effect effect = Global.effect_shader();
            if (effect != null)
                effect.CurrentTechnique = effect.Techniques["Technique1"];
            foreach (Worldmap_Unit unit in Clearing_Units)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect);
                if (effect != null)
                    unit.set_sprite_batch_effects(effect);
                unit.draw(sprite_batch, offset);
                sprite_batch.End();
            }
        }
        #endregion
    }
}
