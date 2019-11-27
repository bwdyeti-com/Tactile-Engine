using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    public abstract partial class Scene_Base
    {
        protected string Scene_Type = "Scene_Base";
        protected bool Suspend_Calling = false;
        protected bool Save_Data_Calling = false;
        protected string Suspend_Filename = "";
        protected bool Returning_To_Title = false;
        protected bool SoftResetBlocked = false;

        #region Accessors
        public string scene_type
        {
            get { return Scene_Type; }
        }

        public bool suspend_calling
        {
            get { return Suspend_Calling; }
        }

        public bool save_data_calling
        {
            get { return Save_Data_Calling; }
        }
        public void CallSaveData()
        {
            Save_Data_Calling = true;
        }
        public void EndSaveData()
        {
            Save_Data_Calling = false;
        }

        public string suspend_filename
        {
            get { return Suspend_Filename; }
        }
        public bool is_map_save_filename { get { return Suspend_Filename == Config.MAP_SAVE_FILENAME; } }

        public bool returning_to_title
        {
            get { return Returning_To_Title; }
        }

        public bool is_worldmap_scene
        {
            get
            {
                switch (Scene_Type)
                {
                    case "Scene_Worldmap":
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool is_map_scene
        {
            get
            {
                switch (Scene_Type)
                {
                    case "Scene_Staff":
                    case "Scene_Map":
                    case "Scene_Dance":
                    case "Scene_Arena":
                    case "Scene_Battle":
                    case "Scene_Promotion":
#if DEBUG
                    case "Scene_Map_Unit_Editor":
#endif
                        return true;
                    default:
                        return false;
                }
            }
        }

#if !MONOGAME && DEBUG
        public bool is_unit_editor { get { return this is Scene_Map_Unit_Editor; } }
#endif

        public bool is_strict_map_scene
        {
            get
            {
                switch (Scene_Type)
                {
                    case "Scene_Map":
#if DEBUG
                    case "Scene_Map_Unit_Editor":
#endif
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool is_action_scene
        {
            get
            {
                switch (Scene_Type)
                {
                    case "Scene_Dance":
                    case "Scene_Staff":
                    case "Scene_Arena":
                    case "Scene_Test_Battle":
                    case "Scene_Battle":
                    case "Scene_Promotion":
                    //Sparring
                    case "Scene_Sparring":
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool is_test_battle
        {
            get
            {
                switch (Scene_Type)
                {
                    case "Scene_Test_Battle":
                        return true;
                    case "Scene_Title":
                        return true;
                    default:
                        return false;
                }
            }
        }
        #endregion

        protected virtual void initialize_base()
        {
            main_window();
        }

        public virtual void suspend()
        {
            Suspend_Calling = true;
        }
        public virtual void reset_suspend_calling()
        {
            Suspend_Calling = false;
        }

        public void reset_suspend_filename()
        {
            Suspend_Filename = "";
        }

        public virtual bool suspend_blocked()
        {
            return Global.game_temp.menu_call || Global.game_temp.menuing;
        }

        public virtual bool fullscreen_switch_blocked()
        {
            return false;
        }

        public virtual void update()
        {
            update_data();
            update_sprites();
        }

        public virtual void update_data() { }

        public virtual void update_sprites() { }

        public virtual void remove_map_sprite(int id) { }

        protected virtual bool update_soft_reset()
        {
            if (Global.Input.soft_reset() && !SoftResetBlocked)
            {
                Global.Audio.stop();
                Global.scene_change("Scene_Soft_Reset");
                return true;
            }
            SoftResetBlocked = false;
            return false;
        }

        public virtual bool skip_frame() { return false; }

        #region Draw
        public virtual void draw(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets) { }
        #endregion

        #region Dispose
        public virtual void dispose() { }
        #endregion
    }
}
