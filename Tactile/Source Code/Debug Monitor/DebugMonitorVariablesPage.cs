#if DEBUG && WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Debug_Monitor
{
    class DebugMonitorVariablesPage : DebugMonitorPage
    {
        private int Index = 0;

        private HashSet<DebugDisplay>[] DebugPages;

        internal int index
        {
            get { return Index; }
            set { Index = value; }
        }

        public DebugMonitorVariablesPage()
        {
            DebugPages = new HashSet<DebugDisplay>[
                (int)Math.Ceiling(Config.EVENT_DATA_LENGTH /
                    (float)Config.EVENT_DATA_MONITOR_PAGE_SIZE)];
            for (int i = 0; i < DebugPages.Length; i++)
                DebugPages[i] = new HashSet<DebugDisplay>();

            // Variables
            DebugStringDisplay variable_label = new DebugStringDisplay(
                () => "Variables", 80, text_color: "Yellow");
            variable_label.loc = new Vector2(8, 0);
            DebugDisplays.Add(variable_label);
            for (int i = 0; i < Config.EVENT_DATA_LENGTH; i++)
            {
                int k = i;
                DebugIntDisplay variable = new DebugIntDisplay(
                    () => Global.game_system.VARIABLES[k],
                        Event_Processor.variable_name(k), 8, 80);
                variable.loc = new Vector2(
                    0, 16 + (i % Config.EVENT_DATA_MONITOR_PAGE_SIZE) * 16);

                DebugPages[i / Config.EVENT_DATA_MONITOR_PAGE_SIZE].Add(variable);
            }
            // Switches
            DebugStringDisplay switch_label = new DebugStringDisplay(
                () => "Switches", 80, text_color: "Yellow");
            switch_label.loc = new Vector2(152 + 8, 0);
            DebugDisplays.Add(switch_label);
            for (int i = 0; i < Config.EVENT_DATA_LENGTH; i++)
            {
                int k = i;
                DebugBooleanDisplay flag = new DebugBooleanDisplay(
                    () => Global.game_system.SWITCHES[k],
                        Event_Processor.switch_name(k), 80);
                flag.loc = new Vector2(
                    168, 16 + (i % Config.EVENT_DATA_MONITOR_PAGE_SIZE) * 16);

                DebugPages[i / Config.EVENT_DATA_MONITOR_PAGE_SIZE].Add(flag);
            }
        }

        public override void update()
        {
            base.update();
            foreach (DebugDisplay debug in DebugPages
                    .SelectMany(x => x))
                debug.update();
        }

        public override void draw(SpriteBatch sprite_batch, ContentManager content)
        {
            base.draw(sprite_batch, content);
            foreach (var debug in DebugPages[Index])
                debug.draw(sprite_batch, content);
        }
    }
}
#endif