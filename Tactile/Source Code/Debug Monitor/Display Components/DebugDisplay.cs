using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Debug_Monitor
{
    internal delegate bool boolFunc();
    internal delegate int intFunc();
    internal delegate string stringFunc();

    abstract class DebugDisplay : Graphic_Object
    {
        protected int Width;
        protected int UpdateTiming = 1;

        #region Accessors
        internal int width { get { return Width; } }

        protected bool is_update_frame
        {
            get { return Global.game_system.total_play_time % UpdateTiming == 0; }
        }
        #endregion

        internal void set_update_timing(int value)
        {
            UpdateTiming = Math.Max(1, value);
        }

        internal abstract void update();

        internal abstract void draw(SpriteBatch sprite_batch, ContentManager content);
    }
}
