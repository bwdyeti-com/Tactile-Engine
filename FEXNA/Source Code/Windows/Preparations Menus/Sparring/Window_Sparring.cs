//Sparring
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Preparations;
using FEXNA.Menus;
using FEXNA.Menus.Preparations;
using FEXNA.Windows.Target;
using FEXNA.Windows.UserInterface.Command;
using FEXNA_Library;

namespace FEXNA
{
    enum PrepSparringInputResults { None, Status, Closing }
    
    class Window_Sparring : PreparationsBaseMenu
    {
        const int SPAR_START_TIME = 8;
        const int SPAR_HOLD_TIME = 16;
        const int READY_FADE_TIME = 8;

        internal static int Healer_Id = -1, Battler_1_Id = -1, Battler_2_Id = -1;
        private bool StartingBattle;
        private int ReadyFadeTimer;
        protected Sprite BlackFadeOut;
        protected Prep_Stats_Window StatsWindow, StatsWindow2;
        protected Pick_Units_Items_Header ItemHeader2;
        protected Window_Target_Sparring CombatWindow;

        #region Accessors
        private bool HealerSelected { get { return Healer_Id > -1; } }
        private bool BothBattlersSelected { get { return Battler_1_Id > -1 && Battler_2_Id > -1; } }

        protected override bool ready_for_inputs
        {
            get
            {
                return base.ready_for_inputs && !StartingBattle;
            }
        }
        #endregion

        public Window_Sparring()
        {
            ChooseUnitWindow.width = 104;
            ChooseUnitWindow.loc -= new Vector2(16, 0);
            SetLabel("Select healer to\noversee training");
        }
        protected override void InitializeSprites()
        {
            base.InitializeSprites();

            // Black Fade Out
            BlackFadeOut = new Sprite();
            BlackFadeOut.texture = Global.Content.Load<Texture2D>(
                @"Graphics\White_Square");
            BlackFadeOut.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            BlackFadeOut.tint = new Color(0, 0, 0, 255);
        }
        
        protected override void SetUnitWindow()
        {
            UnitWindow = new Window_Prep_Sparring_Unit();
        }

        public override void refresh()
        {
            // Item Windows
            if (Battler_1_Id == -1)
            {
                Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, UnitWindow.actor_id, "");
                CreateStatsWindow();
                Global.game_map.completely_remove_unit(Global.game_map.last_added_unit.id);

                ItemHeader = new Sparring_Items_Header(UnitWindow.actor_id, 144);
                ItemHeader.loc = (new Vector2(UnitWindow.loc.X + 4, ((Config.WINDOW_HEIGHT / 16) - 5) * 16 + 8)) - new Vector2(4, 36);
                ItemHeader.stereoscopic = Config.PREPITEM_UNIT_DEPTH;

                StatsWindow2 = null;
                ItemHeader2 = null;
            }
            else
            {
                Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, UnitWindow.actor_id, "");
                CreateStatsWindow();
                Global.game_map.completely_remove_unit(Global.game_map.last_added_unit.id);

                ItemHeader2 = new Sparring_Items_Header(UnitWindow.actor_id, 144);
                ItemHeader2.loc = (new Vector2(UnitWindow.loc.X + 148, ((Config.WINDOW_HEIGHT / 16) - 5) * 16 + 8)) - new Vector2(4, 36);
                ItemHeader2.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
            }
        }
        
        protected void CreateStatsWindow()
        {
            if (Battler_1_Id == -1)
            {
                StatsWindow = new Sparring_Stats_Window(Global.game_map.last_added_unit);
                StatsWindow.loc = new Vector2(12, Config.WINDOW_HEIGHT - 76);
            }
            else
            {
                StatsWindow2 = new Sparring_Stats_Window(Global.game_map.last_added_unit);
                StatsWindow2.loc = new Vector2(Config.WINDOW_WIDTH - 164, Config.WINDOW_HEIGHT - 76);
            }
        }

        protected override void UpdateMenu(bool active)
        {
            base.UpdateMenu(active);

            if (CombatWindow != null)
                CombatWindow.update(active);

            if (StartingBattle)
                Black_Screen.visible = true;

            if (this.BothBattlersSelected)
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

        protected override void UpdateUnitWindow(bool active)
        {
            UnitWindow.update(active && this.ready_for_inputs && !this.BothBattlersSelected);
        }

        public static void reset()
        {
            Healer_Id = -1;
            Battler_1_Id = -1;
            Battler_2_Id = -1;
        }
        
        public event EventHandler<EventArgs> Spar;
        public event EventHandler<EventArgs> BattlersSelected;

        public ScreenFadeMenu SparringFadeIn()
        {
            return new ScreenFadeMenu(
                SPAR_START_TIME,
                SPAR_HOLD_TIME,
                SPAR_START_TIME,
                true,
                this);
        }

        public override void FadeShow()
        {
            if (StartingBattle)
            {
                Black_Screen.visible = true;
                Black_Screen.tint = new Color(0f, 0f, 0f, 1f);
                Spar(this, new EventArgs());
            }
        }

        #region Controls
        public override void CancelUnitSelecting()
        {
            if (!this.HealerSelected)
            {
                close();
            }
            else
            {
                CancelBattlerSelection();
            }
        }
        public override bool SelectUnit(int index)
        {
            // Select healer
            if (!this.HealerSelected)
            {
                return SelectHealer(UnitWindow.actor_id);
            }
            // Select battler
            else
            {
                return SelectBattler(UnitWindow.actor_id);
            }
        }

        private bool SelectHealer(int actorId)
        {
            // If the actor can heal
            if (Global.game_actors[actorId].can_oversee_sparring() &&
                Global.battalion.can_spar(actorId, true))
            {
                Healer_Id = actorId;
                ((Window_Prep_Sparring_Unit)UnitWindow).healer_set = true;
                UnitWindow.refresh_scroll(false);
                (UnitWindow as Window_Prep_Sparring_Unit).refresh_map_sprites();
                refresh();

                SetLabel("Select the units\nwho will participate");
                Global.game_system.play_se(System_Sounds.Confirm);
                return true;
            }
            else
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
                return false;
            }
        }

        private void CancelHealerSelection()
        {
            ((Window_Prep_Sparring_Unit)UnitWindow).healer_set = false;
            UnitWindow.actor_id = Healer_Id;
            Healer_Id = -1;
            (UnitWindow as Window_Prep_Sparring_Unit).refresh_map_sprites();
            SetLabel("Select healer to\noversee training");
        }

        private bool SelectBattler(int actorId)
        {
            // If no battler is selected yet, set them as the first one
            if (Battler_1_Id == -1)
            {
                return SelectFirstBattler(actorId);
            }
            // If the first battler has been selected, make sure the second battler is different
            else
            {
                return SelectSecondBattler(actorId);
            }
        }
        private bool SelectFirstBattler(int actorId)
        {
            // This is the healer, deselect them
            if (actorId == Healer_Id)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                CancelHealerSelection();
            }
            // This is not a valid fighter
            else if (!ValidSparringActor(actorId))
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
                ((Sparring_Stats_Window)StatsWindow).darkened = true;
                LabelVisible(false);
                
                UnitWindow.refresh_scroll(false);
                (UnitWindow as Window_Prep_Sparring_Unit).refresh_map_sprites();
                refresh();
                return true;
            }
            return false;
        }
        private bool SelectSecondBattler(int actorId)
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
                CancelBattlerSelection();
            }
            // This is not a valid fighter
            else if (!ValidSparringActor(actorId))
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
            }
            // Select this actor, if they are in a compatible range with the first
            else if (sparring_range(Battler_1_Id, actorId) != -1)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Battler_2_Id = actorId;
                BattlersSelected(this, new EventArgs());
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

                CombatWindow = new Window_Target_Sparring(
                    Global.game_system.Battler_2_Id, Global.game_system.Battler_1_Id,
                    new Vector2(Config.WINDOW_WIDTH / 2 - 36, 4));
                ((Sparring_Stats_Window)StatsWindow).darkened = false;
                return true;
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
            return false;
        }

        private bool ValidSparringActor(int actorId)
        {
            // If the actor can fight and has enough sparring points
            return Global.game_actors[actorId].can_arena() &&
                Global.battalion.can_spar(actorId, false) &&
                Config.ARENA_WEAPON_TYPES.ContainsKey(
                    Global.game_actors[actorId].determine_sparring_weapon_type().Key);
        }

        public Preparations_Confirm_Window ConfirmWindow()
        {
            var confirmWindow = new Preparations_Confirm_Window();
            confirmWindow.set_text("Is this okay?");
            confirmWindow.add_choice("Yes", new Vector2(8, 12));
            confirmWindow.add_choice("No", new Vector2(48, 12));
            confirmWindow.size = new Vector2(96, 40);
            confirmWindow.loc = new Vector2(Config.WINDOW_WIDTH / 2 - 48, 128);
            confirmWindow.index = 1;
            return confirmWindow;
        }

        internal static int sparring_range(int actorId1, int actorId2)
        {
            WeaponType type1 = Global.game_actors[actorId1].determine_sparring_weapon_type();
            if (!Config.ARENA_WEAPON_TYPES.ContainsKey(type1.Key))
                return -1;
            WeaponType type2 = Global.game_actors[actorId2].determine_sparring_weapon_type();
            if (!Config.ARENA_WEAPON_TYPES.ContainsKey(type2.Key))
                return -1;
            List<int> ranges = Config.ARENA_WEAPON_TYPES[type1.Key].Key.Intersect(Config.ARENA_WEAPON_TYPES[type2.Key].Key).ToList();
            if (ranges.Count > 0)
                return ranges[0];
            return -1;
        }

        private void CancelBattlerSelection()
        {
            if (Battler_1_Id == -1)
                CancelHealerSelection();
            else if (Battler_2_Id > -1)
            {
                CombatWindow = null;

                Global.game_actors[Battler_1_Id].equip(Global.game_actors[Battler_1_Id].equipped);
                Global.game_actors[Battler_2_Id].equip(Global.game_actors[Battler_2_Id].equipped);

                Global.game_map.completely_remove_unit(Global.game_system.Battler_2_Id);
                Global.game_map.completely_remove_unit(Global.game_system.Battler_1_Id);

                Global.game_system.Battler_2_Id = -1;
                Global.game_system.Battler_1_Id = -1;

                UnitWindow.actor_id = Battler_2_Id;
                Battler_2_Id = -1;
                UnitWindow.active = true;
                UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DEPTH;
                ((Sparring_Stats_Window)StatsWindow).darkened = true;
            }
            else
            {
                ((Window_Prep_Sparring_Unit)UnitWindow).battler_1_set = false;
                UnitWindow.actor_id = Battler_1_Id;
                Battler_1_Id = -1;
                LabelVisible(true);
                ((Sparring_Stats_Window)StatsWindow).darkened = false;
            }
            UnitWindow.refresh_scroll(false);
            (UnitWindow as Window_Prep_Sparring_Unit).refresh_map_sprites();
            refresh();
        }

        public bool AcceptBattle()
        {
            bool result = Spar != null;
            if (result)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                StartingBattle = true;
            }
            else
                Global.game_system.play_se(System_Sounds.Cancel);
            return result;
        }
        #endregion

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            base.draw_window(sprite_batch);

            DrawConfirmWindow(sprite_batch);
        }

        protected void DrawConfirmWindow(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            BlackFadeOut.draw(spriteBatch);
            spriteBatch.End();
            
            if (CombatWindow != null)
                CombatWindow.draw(spriteBatch);
        }

        protected override void DrawStatsWindow(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (StatsWindow != null)
                StatsWindow.draw(spriteBatch);
            if (StatsWindow2 != null)
                StatsWindow2.draw(spriteBatch);
            spriteBatch.End();
        }

        protected override void DrawHeader(SpriteBatch spriteBatch)
        {
            base.DrawHeader(spriteBatch);
            if (ItemHeader2 != null)
                ItemHeader2.draw(spriteBatch);
        }
    }
}
