using System;
using System.IO;
using System.Linq;
using Tactile.Metrics;
using TactileLibrary;
using TactileVersionExtension;

namespace Tactile.State
{
    class Game_Chapter_End_State : Game_State_Component
    {
        private bool ChapterEndCalling = false;
        private bool InChapterEnd = false;
        private int ChapterEndTimer = 0;
        private bool ShowRankings = false;
        private bool SendMetrics = false;
        private bool SupportPoints = false;

        internal bool Active { get { return ChapterEndCalling || InChapterEnd; } }

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            //@Yeti: it's real weird that this saves at all, and doesn't save everything
            writer.Write(InChapterEnd);
            writer.Write(ChapterEndTimer);

            writer.Write(ShowRankings);
            writer.Write(SendMetrics);
            writer.Write(SupportPoints);
        }

        internal override void read(BinaryReader reader)
        {
            InChapterEnd = reader.ReadBoolean();
            ChapterEndTimer = reader.ReadInt32();

            if (!Global.LOADED_VERSION.older_than(0, 6, 7, 1)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                ShowRankings = reader.ReadBoolean();
                SendMetrics = reader.ReadBoolean();
                SupportPoints = reader.ReadBoolean();
            }
        }
        #endregion

        internal void end_chapter(bool showRankings, bool sendMetrics, bool supportPoints)
        {
            ChapterEndCalling = true;

            ShowRankings = showRankings;
            SendMetrics = sendMetrics;
            SupportPoints = supportPoints;
        }

        internal override void update()
        {
            if (ChapterEndCalling)
            {
                InChapterEnd = true;
                ChapterEndCalling = false;
            }
            if (InChapterEnd && get_scene_map() != null)
            {
                bool cont = false;
                while (!cont)
                {
                    cont = true;
                    switch (ChapterEndTimer)
                    {
                        case 0:
                            // End of chapter support gains
                            if (SupportPoints)
                            {
                                Global.game_map.apply_chapter_support();
                            }

                            // Send metrics
                            if (SendMetrics)
                            {
#if DEBUG
                                if (!Global.game_temp.chapter_skipped)
                                    if (!Global.UnitEditorActive)
#endif
                                        send_metrics();
                            }

                            // Show rankings
                            if (ShowRankings)
                            {
                                Global.game_temp.menu_call = true;
                                Global.game_temp.rankings_call = true;
                            }
                            cont = false;

                            ChapterEndTimer++;
                            break;
                        case 1:
                            // Wait for rankings screen to finish
                            if (!Global.game_temp.menu_call && !Global.game_temp.menuing)
                            {
                                // Reclaim siege engines and show popups for them
                                var constructors = Global.game_map.allies
                                    .Select(x => Global.game_map.units[x])
                                    .Where(x => x.can_reclaim(true))
                                    .ToList();

                                var siegeEngineLocs = constructors.SelectMany(x => x.reclaim_targets())
                                    .Distinct()
                                    .ToList();

                                if (siegeEngineLocs.Any())
                                {
                                    // Show popup
                                    Global.game_system.play_se(System_Sounds.Gain);
                                    Global.game_map.get_scene_map().set_popup("Recovered siege engines.", 113);

                                    for (int i = 0; i < siegeEngineLocs.Count; i++)
                                    {
                                        Siege_Engine siegeEngine = Global.game_map.get_siege(siegeEngineLocs[i]);
                                        // Add a copy of the siege engine's item to the convoy
                                        // (leave the original item on the map so it's visible during the outro)
                                        Item_Data item = new Item_Data(siegeEngine.item);
                                        Global.game_battalions.add_item_to_convoy(item, true);
                                    }
                                }

                                ChapterEndTimer++;
                                cont = false;
                            }
                            break;
                        case 2:
                            // Wait for popup to clear
                            if (!Global.game_map.get_scene_map().is_map_popup_active())
                            {
                                InChapterEnd = false;
                                ChapterEndTimer = 0;
                                ShowRankings = false;
                                SendMetrics = false;
                                SupportPoints = false;
                            }
                            break;
                    }
                }
            }
        }

        private void send_metrics()
        {
#if !__MOBILE__
#if DEBUG || PRERELEASE
            // Save a copy of metrics locally
            Gameplay_Metrics gameplay = new Gameplay_Metrics(Global.game_state.metrics);
            gameplay.set_pc_ending_stats();
            Metrics_Data metrics = new Metrics_Data(gameplay);

            // Get the path to the folder to save metrics into
            string game_path = System.IO.Path.GetDirectoryName(Global.GAME_ASSEMBLY.Location);
#if DEBUG
            string metrics_path = Directory.GetParent(game_path).FullName;
#elif PRERELEASE
            string metrics_path = game_path;
#endif
            metrics_path = Path.Combine(metrics_path, "Metrics");
            try
            {
                if (!Directory.Exists(metrics_path))
                    Directory.CreateDirectory(metrics_path);
                // Determine filename
                string filename = string.Format("{0} Metrics - {1:yyyy-MM-dd_HH-mm-ss-ffff}",
                    Global.game_system.chapter_id,
                    Global.game_system.chapter_start_time);
                filename = Path.Combine(metrics_path, filename + ".dat");
                // Save metrics to a file
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                    using (BinaryWriter writer = new BinaryWriter(stream))
                        metrics.write(writer);
            }
            // Could not save metrics because no folder permissions
            catch (UnauthorizedAccessException ex) { }
#endif
#endif

            // Send metrics to remote server
            Global.send_metrics();
        }
    }
}
