using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Windows.UserInterface.Command
{
    class Parchment_Confirm_Window : Window_Confirmation
    {
        private bool World_Map_Talk_Boop;

        #region Accessors
        protected override System_Sounds talk_sound { get { return World_Map_Talk_Boop ? System_Sounds.Worldmap_Talk_Boop : System_Sounds.Help_Talk_Boop; } }
        #endregion

        public Parchment_Confirm_Window(bool world_map_talk_boop = false)
        {
            World_Map_Talk_Boop = world_map_talk_boop;
        }

        protected override void initialize()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Message_Window");
            Src_Rect = new Rectangle(0, 0, 0, 0);
            Background = new Parchment_Box(48, 32);
            loc = new Vector2(48, 48);
        }
    }
}
