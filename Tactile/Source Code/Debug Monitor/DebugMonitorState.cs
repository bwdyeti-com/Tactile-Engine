#if DEBUG && WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Debug_Monitor
{
    public class DebugMonitorState
    {
        private int Index = 0;
        DebugMonitorPage[] Monitors;

        public DebugMonitorState()
        {
            var monitors = new DebugMonitorPage[]
            {
                new DebugMonitorOverviewPage(),
                new DebugMonitorBattalionPage(),
                new DebugMonitorVariablesPage(),
                new DebugMonitorRankingPage(),
                new DebugMonitorRngPage(),
                new DebugMonitorAudioPage(),
                new DebugMonitorInputPage()
            };

            Monitors = monitors;
        }

        public void change_page(int page)
        {
            Index = page;
        }
        public int Page { get { return Index; } }

        public void ChangeBattalion(int index)
        {
            // Change variable group on variable pages
            for (int i = 0; i < Monitors.Length; i++)
                if (Monitors[i] is DebugMonitorBattalionPage)
                    (Monitors[i] as DebugMonitorBattalionPage).Index = index;
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