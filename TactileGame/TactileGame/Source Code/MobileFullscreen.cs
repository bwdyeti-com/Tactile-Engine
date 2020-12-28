#if __MOBILE__
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TactileGame
{
    class MobileFullscreenService : Tactile.Rendering.IFullscreenService
    {
        public MobileFullscreenService(Game game) { }

        public void SetFullscreen(bool value, GraphicsDeviceManager graphics) { }

        public bool NeedsRefresh(bool value)
        {
            return false;
        }

        public int WindowWidth(GraphicsDevice device)
        {
            // Doesn't really matter, mobile won't use this value
            return Tactile.Config.WINDOW_WIDTH;
        }
        public int WindowHeight(GraphicsDevice device)
        {
            // Doesn't really matter, mobile won't use this value
            return Tactile.Config.WINDOW_HEIGHT;
        }

        public void MinimizeFullscreen(Game game) { }
    }
}
#endif