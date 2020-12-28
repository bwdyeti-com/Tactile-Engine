#if DEBUG && WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Debug_Monitor
{
    class DebugMonitorAudioPage : DebugMonitorPage
    {
        public DebugMonitorAudioPage()
        {
            // I'm somewhat confused as to how exactly this works,
            // and gets the current state of the audio despite getting
            // a struct once //@Debug
            // I guess the properties hook references into the underlying
            // variables and don't lose them
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