

namespace FEXNA
{
    class SceneInitialLoad : SceneContentLoad
    {
        public SceneInitialLoad()
        {
            ContentLoader = Global.start_initial_load_content();
        }

        public override void update_data()
        {
            base.update_data();

            if (ContentLoader == null)
                if (!Global.start_initial_load && !Global.running_content_load)
                    load_complete();

            if (ready_to_change_scene)
            {
#if DEBUG
                /*
                Global.scene_change("Debug_Start");
                Global.load_save_info = true;
                Global.check_for_updates();
                */

                Global.scene = new Scene_Splash();
#else
                Global.scene = new Scene_Splash();
#endif
            }
        }
    }
}
