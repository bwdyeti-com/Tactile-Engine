#if DEBUG && WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Tactile.Debug_Monitor
{
    class DebugMonitorBattalionPage : DebugMonitorPage
    {
        private int _Index = -1;

        internal int Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        public DebugMonitorBattalionPage()
        {
            // Battalion Id
            DebugStringDisplay battalionId = new DebugStringDisplay(
                () =>
                {
                    if (Global.game_battalions == null)
                        return "-----";
                    // Get index
                    int index = _Index;
                    if (index == -1)
                        index = Global.game_battalions.current_battalion;

                    if (Global.game_battalions.current_battalion == -1)
                    {
                        // Use chapter battalion on the world map
                        if (Global.scene.is_worldmap_scene)
                        {
                            string chapterId = Global.game_system.WorldmapChapterId;
                            if (chapterId != null && Global.data_chapters.ContainsKey(chapterId))
                                index = Global.data_chapters[chapterId].Battalion;
                        }

                        if (index == -1)
                            return "-----";
                    }

                    if (Global.game_battalions.ContainsKey(index))
                        return index.ToString();
                    else
                        return string.Format("{0} (missing)", index);
                },
                80, "Player Battalion ID", false, "Blue");
            battalionId.loc = new Vector2(0, 0);
            DebugDisplays.Add(battalionId);

            DebugStringDisplay battalionMembers = new DebugStringDisplay(
                () =>
                {
                    if (Global.game_battalions == null || Global.battalion == null)
                        return "-----";
                    // Get index
                    int index = _Index;
                    if (index == -1)
                        index = Global.game_battalions.current_battalion;
                    // Get battalion
                    if (!Global.game_battalions.ContainsKey(index))
                        return "-----";
                    var battalion = Global.game_battalions[index];

                    if (!battalion.actors.Any())
                        return "(empty)";

                    var actorNames = string.Join("\n", battalion.actors
                        .Select(x => string.Format("{0} - {1}", x, Global.game_actors[x].name)));
                    return actorNames;
                },
                80, "Members", false, "Blue");
            battalionMembers.loc = new Vector2(0, 16);
            DebugDisplays.Add(battalionMembers);
        }
    }
}
#endif