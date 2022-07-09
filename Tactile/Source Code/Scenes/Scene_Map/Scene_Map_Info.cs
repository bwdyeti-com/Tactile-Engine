using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows.Map.Info;

namespace Tactile
{
    partial class Scene_Map
    {
        Window_Unit_Info_Panel Unit_Info;
        Window_Unit_Info_Burst Enemy_Info;
        Window_Terrain_Info Terrain_Info;
        Window_Objective_Info Objective_Info;
        Window_Button_Descriptions Button_Info;

        public void create_info_windows()
        {
            switch (Global.game_options.unit_window)
            {
                case 0:
                    Unit_Info = new Window_Unit_Info_Panel_Detail();
                    Unit_Info.stereoscopic = Config.MAP_INFO_DEPTH;
                    break;
                case 1:
                    Unit_Info = new Window_Unit_Info_Panel();
                    Unit_Info.stereoscopic = Config.MAP_INFO_DEPTH;
                    break;
                case 2:
                    Unit_Info = new Window_Unit_Info_Burst();
                    Unit_Info.stereoscopic = Config.MAP_INFO_BURST_DEPTH;
                    break;
                default:
                    Unit_Info = null;
                    break;
            }
            switch (Global.game_options.enemy_window)
            {
                case 0:
                    Enemy_Info = new Window_Unit_Info_Burst();
                    break;
                case 1:
                    Enemy_Info = new Window_Unit_Info_Combat_Preview();
                    break;
                default:
                    Enemy_Info = null;
                    break;
            }
            if (Enemy_Info != null)
            {
                Enemy_Info.enemy_info = true;
                Enemy_Info.stereoscopic = Config.MAP_INFO_BURST_DEPTH;
            }
            switch (Global.game_options.terrain_window)
            {
                case 0:
                    Terrain_Info = new Window_Terrain_Info();
                    Terrain_Info.stereoscopic = Config.MAP_INFO_DEPTH;
                    break;
                default:
                    Terrain_Info = null;
                    break;
            }
            switch (Global.game_options.objective_window)
            {
                case 0:
#if !MONOGAME && DEBUG
                    if (Scene_Type == "Scene_Map_Unit_Editor")
                        Objective_Info = new Window_Location_Info();
                    else
                        Objective_Info = new Window_Objective_Info();
                    Objective_Info.stereoscopic = Config.MAP_INFO_DEPTH;
#else
                    Objective_Info = new Window_Objective_Info();
                    Objective_Info.stereoscopic = Config.MAP_INFO_DEPTH;
#endif
                    break;
                default:
                    Objective_Info = null;
                    break;
            }
            switch (Global.game_options.controller)
            {
                case 0:
                    Button_Info = new Window_Button_Descriptions();
                    Button_Info.stereoscopic = Config.MAP_INFO_DEPTH;
                    break;
                default:
                    Button_Info = null;
                    break;
            }
        }

        private void UpdateInfoWindowInputs()
        {
            if (Button_Info != null)
                Button_Info.UpdateInputs();
        }

        private void update_info_windows()
        {
            if (Unit_Info != null)
                Unit_Info.update();
            if (Enemy_Info != null)
                Enemy_Info.update();
            if (Terrain_Info != null)
                Terrain_Info.update();
            if (Objective_Info != null)
                Objective_Info.update();

            if (Button_Info != null)
                Button_Info.update();
        }

        internal void update_objective()
        {
            if (Objective_Info != null)
                Objective_Info.refresh_objective();
        }

        public void update_info_image()
        {
            update_info_image(false);
        }
        public void update_info_image(bool player_called)
        {
            if (Unit_Info != null)
                Unit_Info.set_images(player_called);
            if (Enemy_Info != null)
                Enemy_Info.set_images(player_called);
            if (Terrain_Info != null)
                Terrain_Info.set_images(player_called);
            if (Objective_Info != null)
                Objective_Info.set_images(player_called);
        }

        public void info_windows_offscreen()
        {
            foreach (Window_Map_Info window in new List<Window_Map_Info> { Unit_Info, Enemy_Info, Terrain_Info, Objective_Info })
                if (window != null)
                    window.go_offscreen();
        }

        private void draw_info_windows(SpriteBatch sprite_batch)
        {
            if (Button_Info != null)
                Button_Info.draw(sprite_batch);
            if (Unit_Info != null)
                Unit_Info.draw(sprite_batch);
            if (Enemy_Info != null)
                Enemy_Info.draw(sprite_batch);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (Terrain_Info != null)
                Terrain_Info.draw(sprite_batch);
            if (Objective_Info != null && !Global.game_system.preparations)
                Objective_Info.draw(sprite_batch);
            sprite_batch.End();
        }
    }
}
