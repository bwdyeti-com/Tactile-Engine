using System;
using System.Collections.Generic;
using System.Linq;

namespace FEXNA.Windows.Title
{
    class WindowSupportViewerActorList : Preparations.WindowPrepActorList
    {
        const int COLUMNS = 4;
        const int ROW_SIZE = 16;
        readonly static int ROWS = (Config.WINDOW_HEIGHT - (Constants.Actor.NUM_ITEMS + 1) * 16) / ROW_SIZE;

        private Dictionary<int, Dictionary<int, string>> SupportLevels;

        public WindowSupportViewerActorList()
        {
            loc.Y = Config.WINDOW_HEIGHT - (this.Height + 16);
            this.ColorOverride = 0;
        }

        #region WindowPrepActorList Abstract
        protected override int Columns { get { return COLUMNS; } }
        protected override int VisibleRows { get { return ROWS; } }
        protected override int RowSize { get { return ROW_SIZE; } }

        protected override List<int> GetActorList()
        {
            HashSet<int> actorIds = new HashSet<int>();
            foreach (var pair in Global.data_supports)
            {
                var supportData = pair.Value;
                actorIds.Add(supportData.Id1);
                actorIds.Add(supportData.Id2);
            }
            
            // Use data actors set as the base collection
            return Global.data_actors
                .Select(x => x.Key)
                // Actor has been recruited
                .Where(x => Global.progress.recruitedActors.Contains(x))
                // Actor has a support
                .Where(x => actorIds.Contains(x))
                .ToList();
        }

        protected override string ActorName(int actorId)
        {
            return Global.data_actors[actorId].Name.Split(Constants.Actor.ACTOR_NAME_DELIMITER)[0];
        }
        protected override string ActorMapSpriteName(int actorId)
        {
            var actorData = Global.data_actors[actorId];
            var classData = Global.data_classes[actorData.ClassId];
            return Game_Actors.get_map_sprite_name(classData.Name, actorData.ClassId, actorData.Gender);
        }

        protected override void refresh_font(int i)
        {
            int actorId = ActorList[i];
            bool forced = false, available;
            available = SupportLevels.ContainsKey(actorId) &&
                SupportLevels[actorId].Any(x => Global.progress.supports[x.Value] > 0);
            if (available)
            {
                forced = true;
                foreach (var tuple in Global.data_actors[actorId].SupportPartners(Global.data_supports, Global.data_actors))
                {
                    if (Global.progress.recruitedActors.Contains(tuple.Item2) &&
                        Global.progress.supports.ContainsKey(tuple.Item1))
                    {
                        if (Global.progress.supports[tuple.Item1] == Global.data_supports[tuple.Item1].MaxLevel)
                            continue;
                    }
                    
                    forced = false;
                    break;
                }

            }
            refresh_font(i, forced, available);
        }
        #endregion

        protected override void initialize()
        {
            SupportLevels = new Dictionary<int, Dictionary<int, string>>();
            foreach (var pair in Global.progress.supports)
            {
                var supportData = Global.data_supports[pair.Key];

                if (!SupportLevels.ContainsKey(supportData.Id1))
                    SupportLevels.Add(supportData.Id1, new Dictionary<int, string>());
                if (!SupportLevels.ContainsKey(supportData.Id2))
                    SupportLevels.Add(supportData.Id2, new Dictionary<int, string>());

                SupportLevels[supportData.Id1][supportData.Id2] = pair.Key;
                SupportLevels[supportData.Id2][supportData.Id1] = pair.Key;
            }

            base.initialize();
        }
    }
}
