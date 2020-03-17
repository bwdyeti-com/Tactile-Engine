#if DEBUG && WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
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
            Monitors = new DebugMonitorPage[6];

            Monitors[0] = new DebugMonitorOverviewPage();
            Monitors[1] = new DebugMonitorVariablesPage();
            Monitors[2] = new DebugMonitorRankingPage();
            Monitors[3] = new DebugMonitorRngPage();
            Monitors[4] = new DebugMonitorAudioPage();
            Monitors[5] = new DebugMonitorInputPage();
        }

        public void change_page(int page)
        {
            Index = page;
        }

        public void change_variable_group(int group)
        {
            // Change variable group on variable pages
            for (int i = 0; i < Monitors.Length; i++)
                if (Monitors[i] is DebugMonitorVariablesPage)
                    (Monitors[i] as DebugMonitorVariablesPage).index = group;
        }

        public void update()
        {
            if (Index >= 0 && Index < Monitors.Length)
                Monitors[Index].update();

            // Update Rng pages on Rng activity
            if (Global.game_system.RngActivity)
                for (int i = 0; i < Monitors.Length; i++)
                    if (Index != i)
                        if (Monitors[i] is DebugMonitorRngPage)
                            Monitors[i].update();
        }

        public void draw(SpriteBatch sprite_batch, ContentManager content)
        {
            if (Index >= 0 && Index < Monitors.Length)
                Monitors[Index].draw(sprite_batch, content);
        }
    }
}
#endif