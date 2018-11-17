using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEXNA.Menus.Worldmap
{
    class WorldmapArcsMenuData : WorldmapMenuData
    {
        private List<string> Arcs = new List<string>();
        private bool NonArcChapters = false;

        public WorldmapArcsMenuData(string completedChapterId)
            : base(completedChapterId) { }

        protected override void GetArcs(List<int> availableChapters)
        {
            // Get the arcs that are available
            var available_chapters = new HashSet<string>(availableChapters
                .Select(x => Global.Chapter_List[x]));

            // Get any arcs used by chapters that don't have a spot in the order
            List<string> arcs = new List<string>(Constants.WorldMap.GAME_ARCS);
            foreach(string arc in Global.Chapter_List
                .Select(x => Global.data_chapters[x].Arc))
            {
                if (!string.IsNullOrEmpty(arc) && !arcs.Contains(arc))
                    arcs.Add(arc);
            }

            foreach (string arc in arcs)
            {
                var arc_chapters = ArcChapters(arc);
                if (available_chapters.Intersect(arc_chapters).Any())
                {
                    Arcs.Add(arc);
                    available_chapters.ExceptWith(arc_chapters);
                }
            }
            NonArcChapters = available_chapters.Count > 0;

            // Sort the chapters into arcs
            IndexRedirects = new List<int>[Arcs.Count + (NonArcChapters ? 1 : 0)];
            for (int i = 0; i < IndexRedirects.Length; i++)
            {
                string arc = i >= Arcs.Count ? "" : Arcs[i];
                var arc_chapters = ArcChapters(arc).ToList();

                List<int> chapter_list = availableChapters
                    .Where(x =>
                    {
                        string ch = Global.Chapter_List[x];
                        return arc_chapters.Contains(ch);
                    })
                    .ToList();
                IndexRedirects[i] = chapter_list;
            }
        }

        private IEnumerable<string> ArcChapters(string arc)
        {
            foreach (string chapter in Global.Chapter_List)
            {
                var data = Global.data_chapters[chapter];
                if (string.IsNullOrEmpty(arc) && string.IsNullOrEmpty(data.Arc))
                    yield return chapter;
                else if (data.Arc == arc)
                    yield return chapter;
            }
        }
    }
}
