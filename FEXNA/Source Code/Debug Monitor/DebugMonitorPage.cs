#if DEBUG && WINDOWS
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Debug_Monitor
{
    abstract class DebugMonitorPage
    {
        protected HashSet<DebugDisplay> DebugDisplays = new HashSet<DebugDisplay>();

        public virtual void update()
        {
            foreach (var debug in DebugDisplays)
                debug.update();
        }

        public virtual void draw(SpriteBatch sprite_batch, ContentManager content)
        {
            foreach (var debug in DebugDisplays)
                debug.draw(sprite_batch, content);
        }
    }
}
#endif