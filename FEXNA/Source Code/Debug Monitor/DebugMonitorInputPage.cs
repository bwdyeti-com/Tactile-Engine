#if DEBUG && WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Debug_Monitor
{
    class DebugMonitorInputPage : DebugMonitorPage
    {
        public DebugMonitorInputPage()
        {
            var diagnostics = Input.InputDiagnostics();

            List<Inputs> keys = diagnostics.Inputs.Keys.ToList();

            // Labels
            DebugStringDisplay pressedLabel = new DebugStringDisplay(
                () => "Pressed", 80, text_color: "Yellow");
            pressedLabel.loc = new Vector2(48, 0);
            DebugDisplays.Add(pressedLabel);
            DebugStringDisplay timeHeldLabel = new DebugStringDisplay(
                () => "Held", 80, text_color: "Yellow");
            timeHeldLabel.loc = new Vector2(104 + 4, 0);
            DebugDisplays.Add(timeHeldLabel);
            DebugStringDisplay repeatLabel = new DebugStringDisplay(
                () => "Repeat", 80, text_color: "Yellow");
            repeatLabel.loc = new Vector2(144, 0);
            DebugDisplays.Add(repeatLabel);

            for (int i = 0; i < keys.Count; i++)
            {
                // Pressed Buttons
                Inputs key = keys[i];
                Inputs input = (Inputs)key;
                DebugBooleanDisplay pressedButton = new DebugBooleanDisplay(() =>
                    diagnostics.Inputs[key](),
                    input.ToString(),
                    48);
                pressedButton.loc = new Vector2(0, 16 + i * 16);
                DebugDisplays.Add(pressedButton);
                // Button Held Timers
                DebugIntDisplay heldTimer = new DebugIntDisplay(
                    () => diagnostics.HeldInputsTime[key](),
                    "", 3, 0);
                heldTimer.loc = new Vector2(104, 16 + i * 16);
                DebugDisplays.Add(heldTimer);
                // Repeated Buttons
                DebugBooleanDisplay repeated = new DebugBooleanDisplay(
                    () => diagnostics.Repeated[key](),
                    "",
                    0);
                repeated.loc = new Vector2(144, 16 + i * 16);
                DebugDisplays.Add(repeated);
            }
        }
    }
}
#endif