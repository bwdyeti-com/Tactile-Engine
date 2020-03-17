using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Rendering
{
    public interface IFullscreenService
    {
        void SetFullscreen(bool value, GraphicsDeviceManager graphics);

        bool NeedsRefresh(bool value);

        int WindowWidth(GraphicsDevice device);
        int WindowHeight(GraphicsDevice device);

        void MinimizeFullscreen(Game game);
    }
}
