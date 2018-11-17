#if DEBUG && WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Debug_Monitor
{
    class DebugMonitorAudioPage : DebugMonitorPage
    {
        public DebugMonitorAudioPage()
        {
            var diagnostics = Global.Audio.AudioDiagnostics();

            // Music tracks
            DebugStringDisplay music = new DebugStringDisplay(
                () => !diagnostics.Bgm.Any() ?
                    "(no music)" : string.Join("\n", diagnostics.Bgm),
                80, "Music", false, "Blue");
            music.loc = new Vector2(0, 0);
            DebugDisplays.Add(music);

            DebugStringDisplay pendingMusic = new DebugStringDisplay(
                () => !diagnostics.PendingBgm.Any() ?
                    "-----" : string.Join("\n", diagnostics.PendingBgm),
                80, "Queued", false, "Blue");
            pendingMusic.loc = new Vector2(0, 64);
            DebugDisplays.Add(pendingMusic);
        }
    }
}
#endif