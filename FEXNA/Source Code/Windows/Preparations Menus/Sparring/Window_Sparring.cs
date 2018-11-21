//Sparring
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Preparations;
using FEXNA.Windows.Target;
using FEXNA.Windows.UserInterface.Command;
using FEXNA_Library;

namespace FEXNA
{
    enum PrepSparringInputResults { None, Status, Closing }

    class Window_Sparring : Window_Prep_Items
    {
        const int SPAR_START_TIME = 8;
        const int READY_FADE_TIME = 8;

        internal static int Healer_Id = -1, Battler_1_Id = -1, Battler_2_Id = -1;
        static int temp_battler_1_id = -1, temp_battler_2_id = -1;
        private bool Starting_Battle;
        private int Sparring_Timer = 0;
        private int ReadyFadeTimer;
        protected Sprite BlackFadeOut;
        protected Prep_Stats_Window Stats_Window_2;
        protected Pick_Units_Items_Header Item_Header_2;
        protected Window_Target_Sparring Combat_Window;

        #region Accessors
        public bool healer_selected { get { return Healer_Id > -1; } }
        public bool battlers_selected { get { return Battler_1_Id > -1 && Battler_2_Id > -1; } }

        new public bool ready
        {
            get
            {
                return this.ready_for_inputs && !Starting_Battle;
            }
        }
        #endregion

        public Window_Sparring()
        {
            Choose_Unit_Window.width = 104;
            Choose_Unit_Window.loc -= new Vector2(16, 0);
            set_label("Select healer to\noversee training");
        }
        protected override void initialize_sprites()
        {
            base.initialize_sprites();

            // Black Fade Out
            BlackFadeOut = new Sprite();
            BlackFadeOut.texture = Global.Content.Load<Texture2D>(
                @"Graphics\White_Square");
            BlackFadeOut.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            BlackFadeOut.tint = new Color(0, 0, 0, 255);
        }

        protected override void returning_to_menu(bool returning_to_sparring)
        {
            if (returning_to_sparring)
            {
            }
        }

        protected override void set_unit_window()
        {
            UnitWindow = new Window_Prep_Sparring_Unit();
        }

        protected override void set_command_window() { }

        public override void refresh()
        {
            // Item Windows
            if (Battler_1_Id == -1)
            {
                Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, UnitWindow.actor_id, "");
                create_stats_window();
                Global.game_map.completely_remove_unit(Global.game_map.last_added_unit.id);

                Item_Header = new Sparring_Items_Header(UnitWindow.actor_id, 144);
                Item_Header.loc = (new Vector2(UnitWindow.loc.X + 4, ((Config.WINDOW_HEIGHT / 16) - 5) * 16 + 8)) - new Vector2(4, 36);
                Item_Header.stereoscopic = Config.PREPITEM_UNIT_DEPTH;

                Stats_Window_2 = null;
                Item_Header_2 = null;
            }
            else
            {
                Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, UnitWindow.actor_id, "");
                create_stats_window();
                Global.game_map.completely_remove_unit(Global.game_map.last_added_unit.id);

                Item_Header_2 = new Sparring_Items_Header(UnitWindow.actor_id, 144);
                Item_Header_2.loc = (new Vector2(UnitWindow.loc.X + 148, ((Config.WINDOW_HEIGHT / 16) - 5) * 16 + 8)) - new Vector2(4, 36);
                Item_Header_2.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
            }
        }

        public override void refresh_trade()
        {
            /*Item_Window_1 = new Window_Command_Item_Preparations(
                Trading_Actor_Id, new Vector2(Unit_Window.loc.X + 4, ((Config.WINDOW_HEIGHT / 16) - (Config.NUM_ITEMS + 1)) * 16 + 8), true);
            Item_Window_1.darkened = true;
            Item_Window_1.stereoscopic = Config.PREPITEM_UNIT_DIMMED_DEPTH;
            Item_Window_2 = new Window_Command_Item_Preparations(
                Unit_Window.actor_id, new Vector2(Unit_Window.loc.X + 148, ((Config.WINDOW_HEIGHT / 16) - (Config.NUM_ITEMS + 1)) * 16 + 8), false);
            Item_Window_2.stereoscopic = Config.PREPITEM_UNIT_DEPTH;*/
        }

        protected override void create_stats_window()
        {
            if (Battler_1_Id == -1)
            {
                Stats_Window = new Sparring_Stats_Window(Global.game_map.last_added_unit);
                Stats_Window.loc = new Vector2(12, Config.WINDOW_HEIGHT - 76);
            }
            else
            {
                Stats_Window_2 = new Sparring_Stats_Window(Global.game_map.last_added_unit);
                Stats_Window_2.loc = new Vector2(Config.WINDOW_WIDTH - 164, Config.WINDOW_HEIGHT - 76);
            }
        }

        protected override void UpdateMenu(bool active)
        {
            base.UpdateMenu(active);

            if (Combat_Window != null)
                Combat_Window.update();
            if (Starting_Battle)
            {
                Black_Screen.visible = true;
                if (Sparring_Timer > 0)
                {
                    Sparring_Timer--;
                    Black_Screen.tint = new Color(
                        0f, 0f, 0f, (SPAR_START_TIME - Sparring_Timer) * (1f / SPAR_START_TIME));
                    if (Sparring_Timer == 0)
                    {
                        Spar(this, new EventArgs());
                    }
                }
            }

            if (battlers_selected)
            {
                if (ReadyFadeTimer < READY_FADE_TIME)
                    ReadyFadeTimer++;
            }
            else
            {
                if (ReadyFadeTimer > 0)
                    ReadyFadeTimer--;
            }

            BlackFadeOut.tint = new Color(0f, 0f, 0f,
                0.6f * (ReadyFadeTimer / (float)READY_FADE_TIME));
        }

        protected override void update_unit_window(bool active)
        {
            UnitWindow.update(active && ready && !battlers_selected);
        }

        public static void reset()
        {
            Healer_Id = -1;
            Battler_1_Id = -1;
            Battler_2_Id = -1;
            temp_battler_1_id = -1;
            temp_battler_2_id = -1;
        }

        protected override void update_input(bool active)
        {
            if (active)
            {
                if (battlers_selected)
                {
                    update_battlers_selected_inputs();
                }
                if (healer_selected)
                {
                    update_healer_selected_input();
                }
                else
                {
                    update_base_input();
                }
            }
        }

        private void update_battlers_selected_inputs()
        {
            if (Use_Confirm_Window.is_canceled())
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                cancel_battler_selection();
            }
            else if (Use_Confirm_Window.is_selected())
            {
                if (accept_battle())
                    Global.game_system.play_se(System_Sounds.Confirm);
                else
                    Global.game_system.play_se(System_Sounds.Cancel);
            }
        }

        private void update_healer_selected_input()
        {
            if (Global.Input.triggered(Inputs.B))
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                cancel_battler_selection();
            }

            // Select unit
            var selected_index = UnitWindow.consume_triggered(
                Inputs.A, MouseButtons.Left, TouchGestures.Tap);
            if (selected_index.IsSomething)
            {
                select_battler(UnitWindow.actor_id);
            }

            // Status screen
            var status_index = UnitWindow.consume_triggered(
                Inputs.R, MouseButtons.Right, TouchGestures.LongPress);
            if (status_index.IsSomething)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                OnStatus(new EventArgs());
            }
        }

        private void update_base_input()
        {
            // Close this window
            if (Global.Input.triggered(Inputs.B))
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                close();
            }

            // Select unit
            var selected_index = UnitWindow.consume_triggered(
                Inputs.A, MouseButtons.Left, TouchGestures.Tap);
            if (selected_index.IsSomething)
            {
                if (select_healer(UnitWindow.actor_id))
                    Global.game_system.play_se(System_Sounds.Confirm);
                else
                    Global.game_system.play_se(System_Sounds.Buzzer);
            }

            // Status screen
            var status_index = UnitWindow.consume_triggered(
                Inputs.R, MouseButtons.Right, TouchGestures.LongPress);
            if (status_index.IsSomething)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                OnStatus(new EventArgs());
            }
        }

        public event EventHandler<EventArgs> Spar;

        #region Controls
        public bool select_healer(int actorId)
        {
            // If the actor can heal
            if (Global.game_actors[actorId].can_oversee_sparring() &&
                Global.battalion.can_spar(actorId, true))
            {
                Healer_Id = actorId;
                ((Window_Prep_Sparring_Unit)UnitWindow).healer_set = true;
                if (temp_battler_1_id != -1 || Healer_Id == temp_battler_1_id)
                    UnitWindow.actor_id = temp_battler_1_id;
                //else //Debug
                //    ((Window_Prep_Sparring_Unit)Unit_Window).index =
                //        Unit_Window.index + 1 >= Global.battalion.actors.Count ? Unit_Window.index - 1 : Unit_Window.index + 1;
                temp_battler_1_id = -1;
                UnitWindow.refresh_scroll(false);
                (UnitWindow as Window_Prep_Sparring_Unit).refresh_map_sprites();
                refresh();

                set_label("Select the units\nwho will participate");
                return true;
            }
            return false;
        }

        public void cancel_healer_selection()
        {
            ((Window_Prep_Sparring_Unit)UnitWindow).healer_set = false;
            //temp_battler_1_id = Unit_Window.actor_id; //Debug
            UnitWindow.actor_id = Healer_Id;
            Healer_Id = -1;
            (UnitWindow as Window_Prep_Sparring_Unit).refresh_map_sprites();
            set_label("Select healer to\noversee training");
        }

        public void select_battler(int actorId)
        {
            // If no battler is selected yet, set them as the first one
            if (Battler_1_Id == -1)
            {
                select_first_battler(actorId);
            }
            // If the first battler has been selected, make sure the second battler is different
            else
            {
                select_second_battler(actorId);
            }
        }
        private void select_first_battler(int actorId)
        {
            // This is the healer, deselect them
            if (actorId == Healer_Id)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                cancel_healer_selection();
            }
            // This is not a valid fighter
            else if (!valid_sparring_actor(actorId))
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
            }
            // Select this actor
            else
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                ((Window_Prep_Sparring_Unit)UnitWindow).battler_1_set = true;
                Battler_1_Id = actorId;
                refresh();
                ((Sparring_Stats_Window)Stats_Window).darkened = true;
                label_visible(false);

                if (temp_battler_2_id != -1 || Battler_1_Id == temp_battler_2_id)
                    UnitWindow.actor_id = temp_battler_2_id;
                //else //Debug
                //    ((Window_Prep_Sparring_Unit)Unit_Window).index =
                //        Unit_Window.index + 1 >= Global.battalion.actors.Count ? Unit_Window.index - 1 : Unit_Window.index + 1;
                temp_battler_2_id = -1;
                UnitWindow.refresh_scroll(false);
                (UnitWindow as Window_Prep_Sparring_Unit).refresh_map_sprites();
                refresh();
            }
        }
        private void select_second_battler(int actorId)
        {
            // This is the healer
            if (actorId == Healer_Id)
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
            }
            // This is the first battler, deselect them
            else if (Battler_1_Id == actorId)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                cancel_battler_selection();
            }
            // This is not a valid fighter
            else if (!valid_sparring_actor(actorId))
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
            }
            // Select this actor, if they are in a compatible range with the first
            else if (sparring_range(Battler_1_Id, actorId) != -1)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Battler_2_Id = actorId;
                initialize_confirm_window();
                (UnitWindow as Window_Prep_Sparring_Unit).refresh_map_sprites();
                UnitWindow.active = false;
                UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DIMMED_DEPTH;

                // Add units for each actor, set their weapon ids, set them as the battling_units
                Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, Battler_1_Id, "");
                Global.game_map.last_added_unit.actor.weapon_id =
                    Config.ARENA_WEAPON_TYPES[Global.game_map.last_added_unit.actor.determine_sparring_weapon_type().Key].Value[0];
                Global.game_system.Battler_1_Id = Global.game_map.last_added_unit.id;

                Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, Battler_2_Id, "");
                Global.game_map.last_added_unit.actor.weapon_id =
                    Config.ARENA_WEAPON_TYPES[Global.game_map.last_added_unit.actor.determine_sparring_weapon_type().Key].Value[0];
                Global.game_system.Battler_2_Id = Global.game_map.last_added_unit.id;

                // Shouldn't I set 'Global.game_system.In_Arena = true;' here, and then set it back to false after the target window //Yeti
                // It should make for more accurate combat stats, right? //Yeti
                Global.game_system.Arena_Distance = sparring_range(Battler_1_Id, Battler_2_Id);

                Combat_Window = new Window_Target_Sparring(
                    Global.game_system.Battler_2_Id, Global.game_system.Battler_1_Id,
                    new Vector2(Config.WINDOW_WIDTH / 2 - 36, 4));
                ((Sparring_Stats_Window)Stats_Window).darkened = false;
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        private bool valid_sparring_actor(int actorId)
        {
            // If the actor can fight and has enough sparring points
            return Global.game_actors[actorId].can_arena() &&
                Global.battalion.can_spar(actorId, false) &&
                Config.ARENA_WEAPON_TYPES.ContainsKey(
                    Global.game_actors[actorId].determine_sparring_weapon_type().Key);
        }

        private void initialize_confirm_window()
        {
            Use_Confirm_Window = new Preparations_Confirm_Window();
            Use_Confirm_Window.set_text("Is this okay?");
            Use_Confirm_Window.add_choice("Yes", new Vector2(8, 12));
            Use_Confirm_Window.add_choice("No", new Vector2(48, 12));
            Use_Confirm_Window.size = new Vector2(96, 40);
            Use_Confirm_Window.loc = new Vector2(Config.WINDOW_WIDTH / 2 - 48, 128);
            Use_Confirm_Window.index = 1;
        }

        internal static int sparring_range(int actor_id1, int actor_id2)
        {
            WeaponType type1 = Global.game_actors[actor_id1].determine_sparring_weapon_type();
            if (!Config.ARENA_WEAPON_TYPES.ContainsKey(type1.Key))
                return -1;
            WeaponType type2 = Global.game_actors[actor_id2].determine_sparring_weapon_type();
            if (!Config.ARENA_WEAPON_TYPES.ContainsKey(type2.Key))
                return -1;
            List<int> ranges = Config.ARENA_WEAPON_TYPES[type1.Key].Key.Intersect(Config.ARENA_WEAPON_TYPES[type2.Key].Key).ToList();
            if (ranges.Count > 0)
                return ranges[0];
            return -1;
        }

        public void cancel_battler_selection()
        {
            if (Battler_1_Id == -1)
                cancel_healer_selection();
            else if (Battler_2_Id > -1)
            {
                Combat_Window = null;

                Global.game_actors[Battler_1_Id].equip(Global.game_actors[Battler_1_Id].equipped);
                Global.game_actors[Battler_2_Id].equip(Global.game_actors[Battler_2_Id].equipped);

                Global.game_map.completely_remove_unit(Global.game_system.Battler_2_Id);
                Global.game_map.completely_remove_unit(Global.game_system.Battler_1_Id);

                Global.game_system.Battler_2_Id = -1;
                Global.game_system.Battler_1_Id = -1;

                UnitWindow.actor_id = Battler_2_Id;
                Battler_2_Id = -1;
                Use_Confirm_Window = null;
                UnitWindow.active = true;
                UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DEPTH;
                ((Sparring_Stats_Window)Stats_Window).darkened = true;
            }
            else
            {
                ((Window_Prep_Sparring_Unit)UnitWindow).battler_1_set = false;
                //temp_battler_2_id = Unit_Window.actor_id; //Debug
                UnitWindow.actor_id = Battler_1_Id;
                Battler_1_Id = -1;
                label_visible(true);
                ((Sparring_Stats_Window)Stats_Window).darkened = false;
            }
            UnitWindow.refresh_scroll(false);
            (UnitWindow as Window_Prep_Sparring_Unit).refresh_map_sprites();
            refresh();
        }

        public bool accept_battle()
        {
            if (Use_Confirm_Window.index == 1)
            {
                cancel_battler_selection();
                return false;
            }
            else
            {
                bool result = Spar != null;
                if (result)
                {
                    Starting_Battle = true;
                    Sparring_Timer = SPAR_START_TIME;
                }
                return result;
            }
        }
        #endregion

        protected override void draw_confirm_window(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            BlackFadeOut.draw(sprite_batch);
            sprite_batch.End();

            base.draw_confirm_window(sprite_batch);

            if (Combat_Window != null)
                Combat_Window.draw(sprite_batch);
        }

        protected override void draw_stats_window(SpriteBatch sprite_batch)
        {
            base.draw_stats_window(sprite_batch);
            if (Stats_Window_2 != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Stats_Window_2.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        protected override void draw_header(SpriteBatch sprite_batch)
        {
            base.draw_header(sprite_batch);
            if (Item_Header_2 != null)
                Item_Header_2.draw(sprite_batch);
        }
    }
}
