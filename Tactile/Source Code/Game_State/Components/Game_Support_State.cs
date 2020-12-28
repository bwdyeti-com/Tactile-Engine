using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using DictionaryExtension;
using Tactile.Graphics;
using ListExtension;
using TactileVersionExtension;

namespace Tactile.State
{
    class Game_Support_State : Game_State_Component
    {
        const int SUPPORT_GAIN_TIME = 30;
        const int SUPPORT_GAIN_STARTUP = 1;

        protected bool Support_Calling = false, SupportGainCalling = false;
        protected bool In_Support = false, InSupportGain = false;
        protected int Support_Id1 = -1;
        protected int Support_Id2 = -1;
        protected List<int> SupportTargets = new List<int>();
        protected int Support_Timer = 0;
        internal Dictionary<int, HashSet<int>> Supports_This_Chapter { get; private set; }

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            writer.Write(Support_Calling);
            writer.Write(In_Support);

            writer.Write(SupportGainCalling);
            writer.Write(InSupportGain);

            writer.Write(Support_Timer);
            writer.Write(Support_Id1);
            writer.Write(Support_Id2);
            SupportTargets.write(writer);
            Supports_This_Chapter.write(writer);
        }

        internal override void read(BinaryReader reader)
        {
            Support_Calling = reader.ReadBoolean();
            In_Support = reader.ReadBoolean();

            if (!Global.LOADED_VERSION.older_than(0, 5, 5, 3)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                SupportGainCalling = reader.ReadBoolean();
                InSupportGain = reader.ReadBoolean();
            }

            Support_Timer = reader.ReadInt32();
            Support_Id1 = reader.ReadInt32();
            Support_Id2 = reader.ReadInt32();
            if (!Global.LOADED_VERSION.older_than(0, 5, 5, 3)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                SupportTargets.read(reader);
            }
            if (!Global.LOADED_VERSION.older_than(0, 4, 5, 4)) // This is a suspend load, so this isn't needed for public release //Debug
                Supports_This_Chapter.read(reader);
        }
        #endregion

        #region Accessors
        public bool support_calling
        {
            get { return Support_Calling; }
            set { Support_Calling = value; }
        }

        public bool in_support { get { return In_Support; } }

        public bool support_gain_calling
        {
            get { return SupportGainCalling; }
            set { SupportGainCalling = value; }
        }

        public bool in_support_gain { get { return InSupportGain; } }

        protected Game_Unit supporter { get { return Support_Id1 == -1 ? null : Units[Support_Id1]; } }
        protected Game_Actor supporter_actor
        {
            get
            {
                if (Global.game_system.preparations)
                    return Global.game_actors[Support_Id1];
                else
                    return supporter == null ? null : supporter.actor;
            }
        }

        protected Game_Actor support_target_actor
        {
            get
            {
                if (Global.game_system.preparations)
                    return Global.game_actors[Support_Id2];
                else
                    return Units[Support_Id2].actor;
            }
        }

        public IEnumerable<int> SupportGainIds
        {
            get
            {
                return (GetSupportGainReady(false));
            }
        }
        public IEnumerable<int> SupportGainReadyIds
        {
            get
            {
                return (GetSupportGainReady(true));
            }
        }

        private IEnumerable<int> GetSupportGainReady(bool ready)
        {
            if (!InSupportGain || this.supporter == null)
                yield break;

            // Check active unit
            bool activeReady = false;
            foreach (int id in SupportTargets)
            {
                if (!Units.ContainsKey(id))
                    continue;

                if (this.supporter.actor.is_support_ready(Units[id].actor.id))
                {
                    activeReady = true;
                    break;
                }
            }
            if (activeReady ^ !ready)
                yield return Support_Id1;

            // Check targets
            foreach (int id in SupportTargets)
            {
                if (!Units.ContainsKey(id))
                    continue;

                if (this.supporter.actor.is_support_ready(Units[id].actor.id) ^ !ready)
                    yield return id;
            }
        }

        private int supportGainTimer { get { return Support_Timer - SUPPORT_GAIN_STARTUP; } }

        public SpriteParameters SupportGainGfx
        {
            get
            {
                int timer = this.supportGainTimer;

                // Location
                Vector2 loc = Vector2.Zero;
                if (timer < 7)
                    loc = new Vector2(0, MathHelper.Lerp(
                        Constants.Map.TILE_SIZE * 1.1f, 0f, timer / 7f));

                // Offset
                Vector2 offset = new Vector2(-0.5f, 0);

                // Scale
                const float LARGE_SCALE = 1.75f;
                float scale = 1f;
                if (timer < 10)
                    scale = MathHelper.Lerp(0.5f, LARGE_SCALE, timer / 10f);
                else if (timer < 12)
                    scale = LARGE_SCALE;
                else if (timer < 18)
                    scale = MathHelper.Lerp(LARGE_SCALE, 1f, (timer - 12) / 6f);

                // Tint
                int alpha = 255;
                if (timer < 0)
                    alpha = 0;
                else if (timer < 4)
                    alpha = Math.Min(timer * 256 / 4, 255);
                else if (timer > SUPPORT_GAIN_TIME - 4)
                    alpha = Math.Min((SUPPORT_GAIN_TIME - timer) * 256 / 4, 255);
                Color tint = new Color(alpha, alpha, alpha, alpha);

                // Source Rect
                Rectangle srcRect = new Rectangle(0, 32, 8, 8);

                // Color Shift
                Color color_shift = Color.Transparent;

                return new SpriteParameters(
                    location: loc,
                    scale: new Vector2(scale), offset: offset,
                    tint: tint, srcRect: srcRect, colorShift: color_shift);
            }
        }
        public SpriteParameters SupportGainReadyGfx
        {
            get
            {
                int timer = this.supportGainTimer;

                var baseParameters = this.SupportGainGfx;

                // Scale
                const float LARGE_SCALE = 2.5f;
                float scale = 1f;
                if (timer < 10)
                    scale = MathHelper.Lerp(0.5f, LARGE_SCALE, timer / 10f);
                else if (timer < 12)
                    scale = LARGE_SCALE;
                else if (timer < 18)
                    scale = MathHelper.Lerp(LARGE_SCALE, 1f, (timer - 12) / 6f);

                // Source Rect
                Rectangle srcRect = new Rectangle(8, 32, 8, 8);

                // Color Shift
                float shift_alpha = 0f;
                if (timer < 7)
                    shift_alpha = 0;
                else if (timer < 10)
                    shift_alpha = MathHelper.Lerp(0f, 0.9f, (timer - 7) / 3f);
                else if (timer < 14)
                    shift_alpha = MathHelper.Lerp(0.9f, 0f, (timer - 10) / 4f);
                Color colorShift = new Color(1f, 1f, 1f, shift_alpha);

                return new SpriteParameters(
                    location: baseParameters.Location,
                    scale: new Vector2(scale), offset: baseParameters.Offset,
                    tint: baseParameters.Tint, srcRect: srcRect, colorShift: colorShift);
            }
        }
        #endregion

        internal Game_Support_State()
        {
            Supports_This_Chapter = new Dictionary<int, HashSet<int>>();
        }

        internal override void update()
        {
            if (Support_Calling)
            {
                In_Support = true;
                Support_Calling = false;
            }
            if (SupportGainCalling)
            {
                InSupportGain = true;
                SupportGainCalling = false;
            }

            if (In_Support && get_scene_map() != null)
            {
                bool cont = false;
                while (!cont)
                {
                    cont = true;
                    switch (Support_Timer)
                    {
                        case 0:
                            Support_Id1 = Global.game_system.Rescuer_Id;
                            Support_Id2 = Global.game_system.Rescuee_Id;
                            Global.game_system.Rescuer_Id = -1;
                            Global.game_system.Rescuee_Id = -1;
                            // Add the new support level to the game progression
                            Global.progress.AddSupport(
                                supporter_actor.GetSupportKey(support_target_actor.id),
                                supporter_actor.get_support_level(support_target_actor.id) + 1);
                            if (!Global.game_system.preparations)
                                Global.scene.suspend();
                            Support_Timer++;
                            break;
                        case 2:
                            string convo = supporter_actor.support_convo(support_target_actor.id, Global.game_system.preparations);
                            if (Global.supports.ContainsKey(convo))
                            {
                                Global.game_temp.message_text += Global.supports[convo];
                                if (Global.game_temp.message_text.Length == 0 || 
                                        Scene_Map.debug_chapter_options_blocked())
                                    Global.game_temp.message_text = null;
                                else
                                {
                                    if (Global.game_system.preparations)
                                    {
                                        string clear_faces = "";
                                        for (int i = 1; i <= Face_Sprite_Data.FACE_COUNT; i++)
                                            clear_faces += string.Format("\\f[{0}|nil]", i);
                                        Global.game_temp.message_text = string.Format("\\g[{0}]{1}\\s[-2]{2}\\event",
                                            Global.game_system.home_base_background, Global.game_temp.message_text, clear_faces);
                                    }
                                    Global.scene.new_message_window();
                                }
                            }
                            Support_Timer++;
                            break;
                        case 3:
                            if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                            {
                                supporter_actor.increase_support_level(support_target_actor.id);
                                support_target_actor.increase_support_level(supporter_actor.id);
                                add_support_this_chapter(supporter_actor.id, support_target_actor.id);
                                Support_Timer++;
                            }
                            break;
                        case 19:
                            Global.game_system.play_se(System_Sounds.Gain);
                            get_scene_map().set_popup("Support level increased!", 113); //Yeti
                            Support_Timer++;
                            break;
                        case 20:
                            if (!get_scene_map().is_map_popup_active())
                            {
                                Global.scene.resume_message();
                                if (!Global.scene.is_message_window_active)
                                    Support_Timer++;
                            }
                            break;
                        case 21:
                            if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                            {
                                if (!Global.game_system.preparations)
                                {
                                    if (supporter.cantoing && supporter.is_active_player_team) //Multi
                                    {
                                        Global.player.force_loc(supporter.loc);
                                        supporter.open_move_range();
                                    }
                                    else
                                        supporter.start_wait(false);
                                    supporter.queue_move_range_update();
                                    refresh_move_ranges();
                                }
                                Support_Timer++;
                            }
                            break;
                        case 22:
                            if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                            {
                                wait_for_move_update();

                                Support_Timer++;
                            }
                            break;
                        case 23:
                            In_Support = false;
                            Support_Timer = 0;
                            Support_Id1 = -1;
                            Support_Id2 = -1;
                            highlight_test();
                            break;
                        default:
                            Support_Timer++;
                            break;
                    }
                }
            }
            else if (InSupportGain && get_scene_map() != null)
            {
                bool cont = false;
                if (Global.game_state.talk_blocking_support_gain)
                    cont = true;
                while (!cont)
                {
                    // If skipping AI turn
                    if (Global.game_state.skip_ai_turn_active)
                        // Don't show support hearts
                        Support_Timer = SUPPORT_GAIN_TIME + SUPPORT_GAIN_STARTUP;

                    cont = true;
                    switch (Support_Timer)
                    {
                        case 0:
                            if (Global.game_system.SupportGainId != -1)
                            {
                                Support_Id1 = Global.game_system.SupportGainId;
                                SupportTargets = Global.game_system.SupportGainTargets;
                                Global.game_system.SupportGainId = -1;
                                Global.game_system.SupportGainTargets = new List<int>();
                            }
                            Support_Timer++;
                            break;
                        case SUPPORT_GAIN_STARTUP + 3:
                            // Check if the lead unit has a support ready
                            bool supportReady = false;
                            foreach (int id in SupportTargets)
                            {
                                Game_Unit target = Units[id];
                                if (this.supporter.actor.is_support_ready(target.actor.id))
                                {
                                    supportReady = true;
                                    break;
                                }
                            }
                            // Shift the pitch up more if a support is ready
                            if (supportReady)
                                Global.Audio.play_se("System Sounds", "Support_Gain", 0.6f);
                            else
                                Global.Audio.play_se("System Sounds", "Support_Gain", 0.2f);
                            Support_Timer++;
                            break;
                        case SUPPORT_GAIN_TIME + SUPPORT_GAIN_STARTUP:
                            InSupportGain = false;
                            Support_Timer = 0;
                            Support_Id1 = -1;
                            Support_Id2 = -1;
                            SupportTargets.Clear();
                            break;
                        default:
                            Support_Timer++;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Adds an actor pair to the list of units who have supported already this chapter.
        /// They will be prevented from supporting again on this map (changing maps will allow them to, in a two part chapter)
        /// </summary>
        /// <param name="actor_id1">Id of the first actor</param>
        /// <param name="actor_id2">Id of the second actor</param>
        protected void add_support_this_chapter(int actor_id1, int actor_id2)
        {
            if (!Supports_This_Chapter.ContainsKey(actor_id1))
                Supports_This_Chapter.Add(actor_id1, new HashSet<int>());
            Supports_This_Chapter[actor_id1].Add(actor_id2);

            if (!Supports_This_Chapter.ContainsKey(actor_id2))
                Supports_This_Chapter.Add(actor_id2, new HashSet<int>());
            Supports_This_Chapter[actor_id2].Add(actor_id1);
        }

#if DEBUG
        internal void remove_support_this_chapter(int actorId1, int actorId2)
        {
            if (Supports_This_Chapter.ContainsKey(actorId1))
                Supports_This_Chapter[actorId1].Remove(actorId2);

            if (Supports_This_Chapter.ContainsKey(actorId2))
                Supports_This_Chapter[actorId2].Remove(actorId1);
        }
#endif

        internal void reset_support_data() //private //Yeti
        {
            Supports_This_Chapter.Clear();
        }
    }
}
