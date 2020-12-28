namespace Tactile
{
    class Scene_Soft_Reset : Scene_Base
    {
        const int WAIT_TIME = 8;
        protected int Timer = WAIT_TIME;
        protected bool Loading;

        public Scene_Soft_Reset()
        {
            Scene_Type = "Scene_Soft_Reset";
            Global.load_save_info = true;
            Global.Battler_Content.Unload();
        }

        public override void update()
        {
            if (Loading)
            {
                // If trying to load suspend and failed
                if (!Global.suspend_load_successful)
                {
                    Loading = false;
                }
                else
                {
                    Global.scene_change("Load_Suspend");
                    return;
                }
            }
            if (Timer <= 0)
            {
                string next_scene = "Scene_Splash";
                if (Global.Input.pressed(Inputs.Start))
                {
                    if (Global.suspend_file_info != null)
                    {
                        Global.game_map = null;
                        Global.call_load_suspend();
                        Loading = true;
                        return;
                    }
                    else
                    {
                        next_scene = "Scene_Title";
                    }
                }
#if DEBUG
                if (Global.UnitEditorActive)
                    Global.scene_change("Scene_Map_Unit_Editor");
                else
#endif
                {
                    Global.game_map = null;
                    Global.scene_change(next_scene);
                }
            }
            else
            {
                Timer--;
                if (Global.load_save_info || Global.Input.soft_reset())
                    Timer = WAIT_TIME;
            }
        }
    }
}
