using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Menus.Preparations;
using Tactile.Windows;
using Tactile.Windows.Map;
using Tactile.Windows.Map.Items;
using Tactile.Windows.UserInterface.Command;

namespace Tactile
{
    partial class Scene_Map : IPreparationsMenuHandler, IHomeBaseMenuHandler
    {
        public const string DEFAULT_HOME_BASE_BACKGROUND = "Camp";

        protected bool Changing_Formation = false;

        #region Accessors
        public bool changing_formation { get { return Changing_Formation; } }
        #endregion

        public void activate_preparations()
        {
            if (!Global.game_system.preparations)
            {
                Global.game_actors.heal_battalion();

                Global.game_system.preparations = true;
                Global.game_temp.menuing = true;

                MapMenu = new PreparationsMenuManager(this, deployUnits: true);
                start_preparations();
            }
        }

        public void resume_preparations()
        {
            if (Global.game_system.preparations)
            {
                Global.game_temp.menuing = true;

                MapMenu = new PreparationsMenuManager(this, true);
                start_preparations();
            }
        }

        protected void end_preparations()
        {
            Global.game_system.preparations = false;
            if (Global.game_system.home_base)
            {
                Global.battalion.leave_home_base();

                if (Constants.Support.BASE_COUNTS_AS_SEPARATE_CHAPTER)
                    Global.game_state.reset_support_data();
            }
            else
                Global.game_state.end_preparations();
            Global.game_system.home_base = false;
            Global.battalion.refresh_deployed();
        }

        public void activate_home_base()
        {
            activate_home_base(Global.game_system.home_base_background);
        }
        public void activate_home_base(string background)
        {
            if (!Global.game_system.preparations)
            {
                Global.game_actors.heal_battalion();

                Global.game_system.preparations = true;
                Global.game_system.home_base = true;
                Global.game_temp.menuing = true;
                if (!Global.content_exists(@"Graphics/Panoramas/" + background))
                    background = DEFAULT_HOME_BASE_BACKGROUND;
                Global.game_system.home_base_background = background;
                Global.battalion.enter_home_base();

                if (Constants.Support.BASE_COUNTS_AS_SEPARATE_CHAPTER)
                    Global.game_state.reset_support_data();

                MapMenu = new HomeBaseMenuManager(this);
                start_preparations();
            }
        }

        public void resume_home_base()
        {
            if (Global.game_system.home_base)
            {
                Global.game_temp.menuing = true;

                MapMenu = new HomeBaseMenuManager(this, true);
                start_preparations();
            }
        }

        public void resume_preparations_item_menu()
        {
            if (Global.game_system.preparations)
            {
                Global.game_temp.menuing = true;

                if (Global.game_system.home_base)
                {
                    var homeBaseMenuManager = new HomeBaseMenuManager(this);
                    homeBaseMenuManager.ResumeItemUse();
                    MapMenu = homeBaseMenuManager;
                }
                else
                {
                    var preparationsMenuManager = new PreparationsMenuManager(this);
                    preparationsMenuManager.ResumeItemUse();
                    MapMenu = preparationsMenuManager;
                }
                Global.Audio.BgmFadeOut();
                Global.game_state.play_preparations_theme();
            }
        }

        private void start_preparations()
        {
            Global.game_map.move_range_visible = false;
            Global.Audio.BgmFadeOut();
            Global.game_state.play_preparations_theme();
            Global.game_system.Preparations_Actor_Id = -1;
            if (Global.battalion.actors.Count > 0)
                Global.game_system.Preparations_Actor_Id = Global.battalion.actors[0];
            Global.game_system.Preparation_Events_Ready = false;
        }

        #region Update
        protected void update_preparations_menu_calls()
        {
            if (Global.game_system.preparations)
                if (Global.game_temp.map_menu_call)
                {
                    Global.game_system.play_se(System_Sounds.Unit_Select);
                    Global.game_map.clear_move_range();
                    Global.game_temp.menuing = true;
                    Global.game_temp.menu_call = false;
                    Global.game_temp.map_menu_call = false;

                    var preparationsMenuManager = new PreparationsMenuManager(this);
                    preparationsMenuManager.CheckMap(Changing_Formation);
                    MapMenu = preparationsMenuManager;

                    Changing_Formation = false;
                    Global.game_map.clear_move_range();
                    Global.game_map.move_range_visible = false;
                }
        }

        /// <summary>
        /// Updates preparations menus. Returns true if an active menu was processed, so no other menus should be updated.
        /// </summary>
        protected bool update_preparations()
        {
            //@Debug: I think this is redundant with update_menu_map(), other than
            // the outermost if statement
            if (Global.game_system.preparations)
            {
                if (MapMenu != null)
                {
                    MapMenu.Update();
                    if (MapMenu != null && MapMenu.Finished)
                        MapMenu = null;
                    return true;
                }
            }
            return false;
        }

        #region ISetupMenuHandler
        public void SetupSaveConfig()
        {
            Global.save_config = true;
        }

        public void SetupSave()
        {
            Suspend_Filename = Config.MAP_SAVE_FILENAME;
            suspend();
        }

        #region IPreparationsMenuHandler
        public void PreparationsViewMap()
        {
            Global.game_map.move_range_visible = true;
            Global.game_temp.menuing = false;
            MapMenu = null;
        }

        public void PreparationsChangeFormation()
        {
            Changing_Formation = true;
            Global.game_map.view_deployments();
            Global.game_map.highlight_test();

            Global.game_map.move_range_visible = true;
            Global.game_temp.menuing = false;
            MapMenu = null;
        }

        public void PreparationsLeave()
        {
            end_preparations();
            Global.game_temp.menuing = false;
            MapMenu = null;
        }
        #endregion

        #region IHomeBaseMenuHandler
        public void HomeBaseSupport(int actorId1, int actorId2)
        {
            Global.game_state.call_support(actorId1, actorId2);
        }

        public void HomeBaseLeave()
        {
            end_preparations();
            Global.game_temp.menuing = false;
            MapMenu = null;
        }
        #endregion
        #endregion
        #endregion
    }
}
