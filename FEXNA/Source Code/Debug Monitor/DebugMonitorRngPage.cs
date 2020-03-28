#if DEBUG && WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Debug_Monitor
{
    class DebugMonitorRngPage : DebugMonitorPage
    {
        const int RNG_RANGE = 100;
        const int RNGS_PER_ROW = 5;

        public DebugMonitorRngPage()
        {
            // Variables
            DebugStringDisplay rngLabel = new DebugStringDisplay(
                () => "RNG", 80, text_color: "Yellow");
            rngLabel.loc = new Vector2(8, 0);
            DebugDisplays.Add(rngLabel);

            string format = "";
            for (int i = 0; i < RNGS_PER_ROW; i++)
            {
                format += string.Format("{{{0}:00}}", i);
                if (i + 1 < RNGS_PER_ROW)
                    format += "   ";
            }
            for (int i = 0; i < RNG_RANGE / RNGS_PER_ROW; i++)
            {
                int k = i * RNGS_PER_ROW;
                DebugStringDisplay variable = new DebugStringDisplay(
                    () =>
                    {
                        int[] rns = Global.game_system.preview_rng(RNG_RANGE);
                        int[] row = new int[RNGS_PER_ROW];
                        Array.Copy(rns, k, row, 0, row.Length);
                        return string.Format(format, row
                            .Select(x => (object)x)
                            .ToArray<object>());
                    },
                        128, string.Format("Rng({0:00}-{1:00})", k, k + (RNGS_PER_ROW - 1)));
                variable.loc = new Vector2(
                    0, 16 + (i % Config.EVENT_DATA_MONITOR_PAGE_SIZE) * 16);

                DebugDisplays.Add(variable);
            }
        }

        public override void update()
        {
            if (Global.game_system.RngActivity)
                base.update();
        }
    }
}
#endif