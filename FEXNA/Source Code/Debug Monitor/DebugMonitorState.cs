#if DEBUG && WINDOWS
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA.Debug_Monitor
{
    public class DebugMonitorState
    {
        private int Index;
        DebugMonitorPage[] Monitors;

        public DebugMonitorState()
        {
            Monitors = new DebugMonitorPage[5];

            Monitors[0] = new DebugMonitorOverviewPage();
            Monitors[1] = new DebugMonitorVariablesPage();
            Monitors[2] = new DebugMonitorRankingPage();
            Monitors[3] = new DebugMonitorRngPage();
            Monitors[4] = new DebugMonitorAudioPage();
        }

        public void change_page(int page)
        {
            Index = page;
        }

        public void change_variable_group(int group)
        {
            (Monitors[1] as DebugMonitorVariablesPage).index = group;
        }

        public void update()
        {
            if (Index >= 0 && Index < Monitors.Length)
                Monitors[Index].update();
            if (Global.game_system.RngActivity && Index != 3)
                Monitors[3].update();
        }

        public void draw(SpriteBatch sprite_batch, ContentManager content)
        {
            if (Index >= 0 && Index < Monitors.Length)
                Monitors[Index].draw(sprite_batch, content);
        }
    }
}
#endif