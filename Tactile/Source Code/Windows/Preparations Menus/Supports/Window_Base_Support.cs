using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Preparations;
using Tactile.Menus;
using Tactile.Menus.Preparations;
using Tactile.Windows.Command;
using Tactile.Windows.UserInterface.Command;
using TactileLibrary;

namespace Tactile
{
    enum PrepSupportInputResults { None, Status, Closing }
    
    class Window_Base_Support : PreparationsBaseMenu
    {
        const int SUPPORT_START_TIME = 8;
        const int SUPPORT_END_HOLD_TIME = 16;
        
        private bool StartingSupport, ReturningFromSupport;
        private Prep_Support_Stats_Window SupportStatsWindow;
        private Prep_Support_List_Window SupportListWindow;

        #region Accessors
        protected override bool ready_for_inputs
        {
            get
            {
                return base.ready_for_inputs && !StartingSupport && !ReturningFromSupport;
            }
        }
        #endregion

        public Window_Base_Support()
        {
            ChooseUnitWindow.width = 104;
            ChooseUnitWindow.loc -= new Vector2(16, 0);
            SetLabel("");
            LabelVisible(false);
        }

        protected override void SetUnitWindow()
        {
            UnitWindow = new Window_Prep_Support_Unit();
        }

        public override void refresh()
        {
            CreateSupportWindow();

            ItemHeader = new Pick_Units_Items_Header(this.ActorId, 144);
            ItemHeader.loc = (new Vector2(UnitWindow.loc.X + 4, ((Config.WINDOW_HEIGHT / 16) - 5) * 16 + 8)) - new Vector2(4, 36);
            ItemHeader.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
        }

        protected void CreateSupportWindow()
        {
            // This creates a temporary unit, try and get around this if possible //@Debug
            Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, this.ActorId, "");

            SupportStatsWindow = new Prep_Support_Stats_Window(Global.game_map.last_added_unit);
            SupportStatsWindow.loc = new Vector2(12, Config.WINDOW_HEIGHT - 76);
            SupportListWindow = new Prep_Support_List_Window(Global.game_map.last_added_unit.actor);
            SupportListWindow.loc = new Vector2(Config.WINDOW_WIDTH - 164, Config.WINDOW_HEIGHT - 100);

            Global.game_map.completely_remove_unit(Global.game_map.last_added_unit.id);
        }

        public event EventHandler<EventArgs> Support;
        public event EventHandler<EventArgs> SupportEnded;

        protected override void UpdateMenu(bool active)
        {
            base.UpdateMenu(active);

            if (ReturningFromSupport)
            {
                Black_Screen.visible = true;

                if (!Global.game_state.support_active &&
                    !Global.scene.is_message_window_active)
                {
                    Black_Screen.visible = false;
                    Black_Screen.tint = new Color(0f, 0f, 0f, 0f);
                    ReturningFromSupport = false;
                    refresh();
                    (UnitWindow as Window_Prep_Support_Unit).refresh_map_sprites();

                    SupportEnded(this, new EventArgs());
                }
            }
        }

        protected override void UpdateUnitWindow(bool active)
        {
            UnitWindow.update(active && this.ready_for_inputs);
        }

        protected override void Activate()
        {
            UnitWindow.active = true;
            UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DEPTH;
            base.Activate();
        }

        protected override void Deactivate()
        {
            UnitWindow.active = false;
            UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DIMMED_DEPTH;
            base.Deactivate();
        }

        public ScreenFadeMenu SupportFadeIn()
        {
            return new ScreenFadeMenu(
                SUPPORT_START_TIME,
                0,
                SUPPORT_START_TIME,
                true,
                this);
        }
        public ScreenFadeMenu SupportFadeOut()
        {
            return new ScreenFadeMenu(
                0,
                SUPPORT_END_HOLD_TIME,
                SUPPORT_START_TIME,
                false,
                this);
        }

        public override void FadeShow()
        {
            if (StartingSupport)
            {
                Black_Screen.visible = true;
                Black_Screen.tint = new Color(0f, 0f, 0f, 1f);
                StartSupport();
            }
        }

        #region Controls
        public override void CancelUnitSelecting()
        {
            close();
        }
        public override bool SelectUnit(int index)
        {
            if (this.actor.support_candidates().Any())
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                OnUnitSelected(new EventArgs());
                return true;
            }
            else
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
                return false;
            }
        }

        public Window_Command_Support GetCommandWindow()
        {
            var commandWindow = new Window_Command_Support(
                this.actor.id, new Vector2(Config.WINDOW_WIDTH - 152, 16));
            commandWindow.IndexChanged += Command_Window_IndexChanged;
            RefreshActorBonuses(commandWindow);
            return commandWindow;
        }

        void Command_Window_IndexChanged(object sender, System.EventArgs e)
        {
            var commandWindow = (sender as Window_Command_Support);
            RefreshActorBonuses(commandWindow);
        }

        private void RefreshActorBonuses(Window_Command_Support commandWindow)
        {
            int targetId = commandWindow.TargetId;
            RefreshActorBonuses(targetId);
        }
        private void RefreshActorBonuses(int targetId)
        {
            SupportStatsWindow.set_images(
                Global.game_actors[this.ActorId],
                Global.game_actors[targetId]);
        }

        public bool TrySelectPartner(int targetId)
        {
            if (IsSupportPartnerReady(targetId))
            {
                return true;
            }
            return false;
        }

        private bool IsSupportPartnerReady(int targetId)
        {
            if (!Global.battalion.actors.Contains(targetId))
                return false;
#if DEBUG
            if (Global.Input.pressed(Inputs.Start) &&
                !this.actor.is_support_maxed(false, targetId) &&
                !this.actor.is_support_level_maxed(targetId))
            {
                Global.Audio.play_se("System Sounds", "Buzzer");
                return true;
            }
#endif
            return this.actor.is_support_ready(targetId);
        }
        
        public void AcceptSupport()
        {
            StartingSupport = true;
        }

        private void StartSupport()
        {
            Support(this, new EventArgs());

            StartingSupport = false;
            ReturningFromSupport = true;
            
            SupportStatsWindow.set_images(Global.game_actors[this.ActorId]);
        }
        #endregion

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            base.draw_window(sprite_batch);
        }

        protected override void DrawStatsWindow(SpriteBatch spriteBatch)
        {
            if (SupportStatsWindow != null)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                SupportStatsWindow.draw(spriteBatch);
                spriteBatch.End();
            }
            if (SupportListWindow != null)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                SupportListWindow.draw(spriteBatch);
                spriteBatch.End();
            }
        }
    }
}
