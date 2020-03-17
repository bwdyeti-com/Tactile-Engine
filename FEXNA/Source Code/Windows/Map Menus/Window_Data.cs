using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;

namespace FEXNA.Windows.Map
{
    class Window_Data : Map_Window_Base
    {
        const int RANKING_BAR_LENGTH = 56;

        readonly static Vector2 TEAM_WINDOW_LOC = new Vector2(8, 32);
        private int Index = 0;
        private bool ChangingPage = false;
        private int ChangingPageTime = 0;
        private Vector2 PageLoc;
        private List<KeyValuePair<int, int>> Groups = new List<KeyValuePair<int, int>>();
        new private Data_Team_Cursor Cursor;
        private FE_Text Banner;
        private Sprite BannerBg, UnitBg, FileBg;
        private Sprite Objective_Window, Game_Data_Window;
        private Data_Ranking_Window RankingWindow;
        private List<Data_Team_Window> Team_Windows = new List<Data_Team_Window>();
        private Sprite Objective_Label, Defeat_Label, Turn_Label, Funds_Label, Gold_G, Lv_Label, Hp_Label, Hp_Slash, RatingLabel;
        private Play_Time_Counter Counter;
        private Item_Display LeaderWeapon;
        private List<FE_Text> Group_Names = new List<FE_Text>(), Group_Counts = new List<FE_Text>();
        private FE_Text Victory_Text, Loss_Text, FileLabel, RankingLabel,
            TurnsRankLabel, CombatRankLabel, ExpRankLabel;
        private FE_Text_Int Turn, Funds, FileNumber;
        private FE_Text Leader_Name, StyleText, DifficultyText, TotalRankLetter,
            TurnsRankLetter, CombatRankLetter, ExpRankLetter;
        private Stat_Bar TurnsBar, CombatBar, ExpBar;
        private FE_Text_Int Lvl, Hp, Hp_Max, Rating;
        private Character_Sprite Map_Sprite;
        private Miniface Face;
        private Page_Arrow Left_Page_Arrow, Right_Page_Arrow;

        #region Accessors
        private Vector2 target_page_loc { get { return new Vector2(page == 0 ? 0 : 128, 0); } }

        private int page
        {
            get { return Global.game_system.DataPage; }
            set { Global.game_system.DataPage = value; }
        }

        protected override bool ready_for_inputs { get { return base.ready_for_inputs && !ChangingPage; } }
        #endregion

        public Window_Data()
        {
            PageLoc = target_page_loc;
            initialize_sprites();
            update_black_screen();
        }

        private void initialize_sprites()
        {
            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 255);
            // Banner
            Banner = new FE_Text();
            string str = Global.data_chapters[Global.game_state.chapter_id].FullName;
            Banner.loc = new Vector2(Config.WINDOW_WIDTH, 32) / 2 - new Vector2(Font_Data.text_width(str, "FE7_Chapter") / 2, 8);
            Banner.Font = "FE7_Chapter";
            Banner.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Chapter_Data");
            Banner.text = str;
            Banner.stereoscopic = Config.DATA_BANNER_DEPTH;

            int alpha = 7 * 16;
            BannerBg = new Sprite();
            BannerBg.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            BannerBg.loc = new Vector2(64, 0);
            BannerBg.src_rect = new Rectangle(0, 176, 192, 32);
            BannerBg.tint = new Color(alpha, alpha, alpha, 256 - alpha);
            BannerBg.stereoscopic = Config.DATA_BANNER_DEPTH;
            // Unit Bg
            UnitBg = new Sprite();
            UnitBg.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            UnitBg.loc = new Vector2(132, 120);
            UnitBg.src_rect = new Rectangle(0, 272, 128, 72);
            UnitBg.tint = new Color(alpha, alpha, alpha, 256 - alpha);
            UnitBg.stereoscopic = Config.DATA_LEADER_DEPTH;
            // File Bg
            FileBg = new Sprite();
            FileBg.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            FileBg.loc = new Vector2(260, 128);
            FileBg.src_rect = new Rectangle(0, 208, 56, 64);
            FileBg.tint = new Color(alpha, alpha, alpha, 256 - alpha);
            FileBg.stereoscopic = Config.DATA_LEADER_DEPTH;
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Status_Background");
            (Background as Menu_Background).vel = new Vector2(-0.25f, 0);
            (Background as Menu_Background).tile = new Vector2(3, 2);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
            // Cursor
            Cursor = new Data_Team_Cursor();
            Cursor.stereoscopic = Config.DATA_TEAMS_DEPTH;
            // Objective Window
            Objective_Window = new Sprite();
            Objective_Window.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Objective_Window.loc = new Vector2(132, 32);
            Objective_Window.src_rect = new Rectangle(32, 0, 184, 96);
            Objective_Window.stereoscopic = Config.DATA_WINDOW_DEPTH;
            // Game Data Window
            Game_Data_Window = new Sprite();
            Game_Data_Window.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Game_Data_Window.loc = new Vector2(324, 120);
            Game_Data_Window.src_rect = new Rectangle(104, 96, 112, 72);
            Game_Data_Window.stereoscopic = Config.DATA_DATA_DEPTH;
            // Team Windows/Text
            int y = 0;
            for (int i = 1; i <= Constants.Team.NUM_TEAMS; i++)
                if (Global.game_map.teams[i].Count > 0)
                {
                    List<int> groups = new List<int>();
                    Dictionary<int, int> group_sizes = new Dictionary<int, int>();
                    foreach (int id in Global.game_map.teams[i])
                    {
                        if (!groups.Contains(Global.game_map.units[id].group))
                        {
                            groups.Add(Global.game_map.units[id].group);
                            group_sizes.Add(Global.game_map.units[id].group, 0);
                        }
                        group_sizes[Global.game_map.units[id].group]++;
                    }
                    Groups.Add(new KeyValuePair<int, int>(i, groups.Count));
                    Team_Windows.Add(new Data_Team_Window(i, group_sizes.Count));
                    Team_Windows[Team_Windows.Count - 1].loc = TEAM_WINDOW_LOC + new Vector2(0, 0 + y);
                    Team_Windows[Team_Windows.Count - 1].stereoscopic = Config.DATA_TEAMS_DEPTH;
                    groups.Sort();
                    for (int j = 0; j < groups.Count; j++)
                    {
                        Group_Names.Add(new FE_Text());
                        Group_Names[Group_Names.Count - 1].loc = TEAM_WINDOW_LOC + new Vector2(4, y + 12 + j * 16);
                        Group_Names[Group_Names.Count - 1].Font = "FE7_Text";
                        Group_Names[Group_Names.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
                        Group_Names[Group_Names.Count - 1].text = Global.game_map.group_name(i, groups[j]);
                        Group_Names[Group_Names.Count - 1].stereoscopic = Config.DATA_TEAMS_DEPTH;
                        Group_Counts.Add(new FE_Text_Int());
                        Group_Counts[Group_Names.Count - 1].loc = TEAM_WINDOW_LOC + new Vector2(112, y + 12 + j * 16);
                        Group_Counts[Group_Names.Count - 1].Font = "FE7_Text";
                        Group_Counts[Group_Names.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
                        Group_Counts[Group_Names.Count - 1].text = group_sizes[groups[j]].ToString();
                        Group_Counts[Group_Names.Count - 1].stereoscopic = Config.DATA_TEAMS_DEPTH;
                    }
                    y += (group_sizes.Count + 1) * 16;
                }
            // Game Data Window
            RankingWindow = new Data_Ranking_Window();
            RankingWindow.loc = new Vector2(320, 32);
            RankingWindow.width = 120;
            RankingWindow.height = 80;
            RankingWindow.stereoscopic = Config.DATA_TEAMS_DEPTH;
            // Counter
            Counter = new Play_Time_Counter();
            Counter.loc = Game_Data_Window.loc + new Vector2(12, 48);
            Counter.stereoscopic = Config.DATA_DATA_DEPTH;
            // Labels
            Objective_Label = new Sprite();
            Objective_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Objective_Label.loc = Objective_Window.loc + new Vector2(16 - 2, 12 - 2);
            Objective_Label.src_rect = new Rectangle(0, 96, 56, 16);
            Objective_Label.stereoscopic = Config.DATA_WINDOW_DEPTH;
            Defeat_Label = new Sprite();
            Defeat_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Defeat_Label.loc = Objective_Window.loc + new Vector2(16 - 2, 44 - 2);
            Defeat_Label.src_rect = new Rectangle(56, 96, 40, 16);
            Defeat_Label.stereoscopic = Config.DATA_WINDOW_DEPTH;

            Turn_Label = new Sprite();
            Turn_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Turn_Label.loc = Game_Data_Window.loc + new Vector2(8, 8);
            Turn_Label.src_rect = new Rectangle(0, 112, 32, 16);
            Turn_Label.stereoscopic = Config.DATA_DATA_DEPTH;
            Funds_Label = new Sprite();
            Funds_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Funds_Label.loc = Game_Data_Window.loc + new Vector2(8, 24);
            Funds_Label.src_rect = new Rectangle(0, 128, 32, 16);
            Funds_Label.stereoscopic = Config.DATA_DATA_DEPTH;
            Gold_G = new Sprite();
            Gold_G.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Gold_G.loc = Game_Data_Window.loc + new Vector2(88 + 5, 24);
            Gold_G.src_rect = new Rectangle(0, 160, 16, 16);
            Gold_G.stereoscopic = Config.DATA_DATA_DEPTH;

            Lv_Label = new Sprite();
            Lv_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Lv_Label.loc = UnitBg.loc + new Vector2(8, 40 + 0 - 2);
            Lv_Label.src_rect = new Rectangle(32, 112, 16, 16);
            Lv_Label.stereoscopic = Config.DATA_LEADER_DEPTH;
            Hp_Label = new Sprite();
            Hp_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Hp_Label.loc = UnitBg.loc + new Vector2(8, 40 + 12 - 2);
            Hp_Label.src_rect = new Rectangle(48, 112, 24, 16);
            Hp_Label.stereoscopic = Config.DATA_LEADER_DEPTH;
            FE_Text hp_slash = new FE_Text(); // why is this done so //Yeti
            hp_slash.loc = UnitBg.loc + new Vector2(44, 40 + 12);
            hp_slash.Font = "FE7_Text";
            hp_slash.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            hp_slash.text = "/";
            Hp_Slash = hp_slash;
            Hp_Slash.stereoscopic = Config.DATA_LEADER_DEPTH;
            RatingLabel = new Sprite();
            RatingLabel.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            RatingLabel.loc = UnitBg.loc + new Vector2(72, 40 + 12 - 2);
            RatingLabel.src_rect = new Rectangle(72, 112, 24, 16);
            RatingLabel.stereoscopic = Config.DATA_LEADER_DEPTH;

            RankingLabel = new FE_Text();
            RankingLabel.loc = RankingWindow.loc + new Vector2(24, 8);
            RankingLabel.Font = "FE7_Text";
            RankingLabel.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            RankingLabel.text = "Progress";
            RankingLabel.stereoscopic = Config.DATA_TEAMS_DEPTH;
            TurnsRankLabel = new FE_Text();
            TurnsRankLabel.loc = RankingWindow.loc + new Vector2(8, 24);
            TurnsRankLabel.Font = "FE7_Text";
            TurnsRankLabel.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            TurnsRankLabel.text = "Turns";
            TurnsRankLabel.stereoscopic = Config.DATA_TEAMS_DEPTH;
            CombatRankLabel = new FE_Text();
            CombatRankLabel.loc = RankingWindow.loc + new Vector2(8, 40);
            CombatRankLabel.Font = "FE7_Text";
            CombatRankLabel.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            CombatRankLabel.text = "Combat";
            CombatRankLabel.stereoscopic = Config.DATA_TEAMS_DEPTH;
            ExpRankLabel = new FE_Text();
            ExpRankLabel.loc = RankingWindow.loc + new Vector2(8, 56);
            ExpRankLabel.Font = "FE7_Text";
            ExpRankLabel.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            ExpRankLabel.text = "Exp";
            ExpRankLabel.stereoscopic = Config.DATA_TEAMS_DEPTH;

            FileLabel = new FE_Text();
            FileLabel.loc = FileBg.loc + new Vector2(8, 40);
            FileLabel.Font = "FE7_Text";
            FileLabel.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            FileLabel.text = "File";
            FileLabel.stereoscopic = Config.DATA_LEADER_DEPTH;

            // Victory Text
            Victory_Text = new FE_Text();
            Victory_Text.loc = Objective_Window.loc + new Vector2(68, 12);
            Victory_Text.Font = "FE7_Text";
            Victory_Text.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Victory_Text.text = Global.game_system.Victory_Text;
            Victory_Text.stereoscopic = Config.DATA_WINDOW_DEPTH;
            // Loss Text
            Loss_Text = new FE_Text();
            Loss_Text.loc = Objective_Window.loc + new Vector2(68, 44);
            Loss_Text.Font = "FE7_Text";
            Loss_Text.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Loss_Text.text = Global.game_system.Loss_Text;
            Loss_Text.stereoscopic = Config.DATA_WINDOW_DEPTH;

            // Turn Text
            Turn = new FE_Text_Int();
            Turn.loc = Game_Data_Window.loc + new Vector2(100, 8);
            Turn.Font = "FE7_Text";
            Turn.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Turn.text = Global.game_system.chapter_turn.ToString();
            Turn.stereoscopic = Config.DATA_DATA_DEPTH;
            // Funds Text
            Funds = new FE_Text_Int();
            Funds.loc = Game_Data_Window.loc + new Vector2(92, 24);
            Funds.Font = "FE7_Text";
            Funds.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Funds.text = Global.battalion.gold.ToString();
            Funds.stereoscopic = Config.DATA_DATA_DEPTH;

            // Leader Name
            Leader_Name = new FE_Text();
            Leader_Name.loc = UnitBg.loc + new Vector2(32 + 24, 12);
            Leader_Name.Font = "FE7_Text";
            Leader_Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Leader_Name.stereoscopic = Config.DATA_LEADER_DEPTH;
            // Level
            Lvl = new FE_Text_Int();
            Lvl.loc = UnitBg.loc + new Vector2(68, 40 + 0);
            Lvl.Font = "FE7_Text";
            Lvl.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Lvl.stereoscopic = Config.DATA_LEADER_DEPTH;
            // Hp
            Hp = new FE_Text_Int();
            Hp.loc = UnitBg.loc + new Vector2(44, 40 + 12);
            Hp.Font = "FE7_Text";
            Hp.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Hp.stereoscopic = Config.DATA_LEADER_DEPTH;
            // Hp Max
            Hp_Max = new FE_Text_Int();
            Hp_Max.loc = UnitBg.loc + new Vector2(68, 40 + 12);
            Hp_Max.Font = "FE7_Text";
            Hp_Max.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Hp_Max.stereoscopic = Config.DATA_LEADER_DEPTH;
            // Rating
            Rating = new FE_Text_Int();
            Rating.loc = UnitBg.loc + new Vector2(120, 40 + 12);
            Rating.Font = "FE7_Text";
            Rating.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Rating.stereoscopic = Config.DATA_LEADER_DEPTH;
            // Map Sprite
            Map_Sprite = new Character_Sprite();
            Map_Sprite.facing_count = 3;
            Map_Sprite.frame_count = 3;
            Map_Sprite.loc = UnitBg.loc + new Vector2(20, 24);
            Map_Sprite.stereoscopic = Config.DATA_LEADER_DEPTH;
            // Miniface
            Face = new Miniface();
            Face.loc = UnitBg.loc + new Vector2(104, 8);
            Face.stereoscopic = Config.DATA_LEADER_DEPTH;
            // Weapon
            LeaderWeapon = new Item_Display();
            LeaderWeapon.loc = UnitBg.loc + new Vector2(8, 28);
            LeaderWeapon.stereoscopic = Config.DATA_LEADER_DEPTH;

            // File number
            FileNumber = new FE_Text_Int();
            FileNumber.loc = FileBg.loc + new Vector2(40, 40);
            FileNumber.Font = "FE7_Text";
            FileNumber.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            FileNumber.text = Global.current_save_id.ToString();
            FileNumber.stereoscopic = Config.DATA_LEADER_DEPTH;
            // Style
            StyleText = new FE_Text();
            StyleText.loc = FileBg.loc + new Vector2(8, 8);
            StyleText.Font = "FE7_Text";
            StyleText.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            StyleText.text = Global.game_system.Style.ToString();
            StyleText.stereoscopic = Config.DATA_LEADER_DEPTH;
            // Difficulty
            DifficultyText = new FE_Text();
            DifficultyText.loc = FileBg.loc + new Vector2(8, 24);
            DifficultyText.Font = "FE7_Text";
            DifficultyText.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            DifficultyText.text = Global.game_system.Difficulty_Mode.ToString();
            DifficultyText.stereoscopic = Config.DATA_LEADER_DEPTH;

            // Rank Letters
            TotalRankLetter = new FE_Text();
            TotalRankLetter.loc = RankingWindow.loc + new Vector2(88, 8);
            TotalRankLetter.Font = "FE7_TextL";
            TotalRankLetter.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            TotalRankLetter.stereoscopic = Config.DATA_TEAMS_DEPTH;
            TurnsRankLetter = new FE_Text();
            TurnsRankLetter.loc = RankingWindow.loc + new Vector2(104, 24);
            TurnsRankLetter.Font = "FE7_TextL";
            TurnsRankLetter.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            TurnsRankLetter.stereoscopic = Config.DATA_TEAMS_DEPTH;
            CombatRankLetter = new FE_Text();
            CombatRankLetter.loc = RankingWindow.loc + new Vector2(104, 40);
            CombatRankLetter.Font = "FE7_TextL";
            CombatRankLetter.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            CombatRankLetter.stereoscopic = Config.DATA_TEAMS_DEPTH;
            ExpRankLetter = new FE_Text();
            ExpRankLetter.loc = RankingWindow.loc + new Vector2(104, 56);
            ExpRankLetter.Font = "FE7_TextL";
            ExpRankLetter.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            ExpRankLetter.stereoscopic = Config.DATA_TEAMS_DEPTH;
            // Rank Gauges
            TurnsBar = new Stat_Bar();
            TurnsBar.loc = RankingWindow.loc + new Vector2(40, 32);
            TurnsBar.offset = new Vector2(-2, 0);
            TurnsBar.bar_width = RANKING_BAR_LENGTH;
            TurnsBar.stereoscopic = Config.DATA_TEAMS_DEPTH;
            CombatBar = new Stat_Bar();
            CombatBar.loc = RankingWindow.loc + new Vector2(40, 48);
            CombatBar.offset = new Vector2(-2, 0);
            CombatBar.bar_width = RANKING_BAR_LENGTH;
            CombatBar.stereoscopic = Config.DATA_TEAMS_DEPTH;
            ExpBar = new Stat_Bar();
            ExpBar.loc = RankingWindow.loc + new Vector2(40, 64);
            ExpBar.offset = new Vector2(-2, 0);
            ExpBar.bar_width = RANKING_BAR_LENGTH;
            ExpBar.stereoscopic = Config.DATA_TEAMS_DEPTH;

            // Page Arrows
            Left_Page_Arrow = new Page_Arrow();
            Left_Page_Arrow.loc = new Vector2(4, 72);
            Left_Page_Arrow.stereoscopic = Config.DATA_BANNER_DEPTH;
            Right_Page_Arrow = new Page_Arrow();
            Right_Page_Arrow.loc = new Vector2(Config.WINDOW_WIDTH - 4, 72);
            Right_Page_Arrow.mirrored = true;
            Right_Page_Arrow.stereoscopic = Config.DATA_BANNER_DEPTH;

            refresh_rank();
            set_images();
        }

        private void refresh_rank()
        {
            var ranking = Global.game_state.calculate_ranking_progress();
            if (ranking == null)
            {
                refresh_ranking_letter(TotalRankLetter, "");
                refresh_ranking_letter(TurnsRankLetter, "-");
                refresh_ranking_letter(CombatRankLetter, "-");
                refresh_ranking_letter(ExpRankLetter, "-");
            }
            else
            {
                refresh_ranking_letter(TotalRankLetter, ranking.rank);
                refresh_ranking_letter(TurnsRankLetter, ranking.turns_letter);
                refresh_ranking_letter(CombatRankLetter, ranking.combat_letter);
                refresh_ranking_letter(ExpRankLetter, ranking.exp_letter);

                refresh_ranking_bar(TurnsBar, ranking.turns);
                refresh_ranking_bar(CombatBar, ranking.combat);
                refresh_ranking_bar(ExpBar, ranking.exp);
            }
        }

        private void refresh_ranking_letter(FE_Text text, string letter)
        {
            text.text = letter;
            if (letter == FEXNA_Library.Chapters.DataRanking.best_rank)
                text.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Green");
            else
                text.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
        }
        private void refresh_ranking_bar(Stat_Bar bar, int score)
        {
            int bar_length = score * RANKING_BAR_LENGTH / 150;
            if (bar_length > RANKING_BAR_LENGTH * 2 / 3)
            {
                bar.fill_width = RANKING_BAR_LENGTH * 2 / 3;
                bar.bonus_width = bar_length - RANKING_BAR_LENGTH * 2 / 3;
            }
            else
            {
                bar.fill_width = bar_length;
                bar.bonus_width = bar_length - RANKING_BAR_LENGTH * 2 / 3;
            }
        }

        private void set_images()
        {
            if (Team_Windows.Count == 0)
            {
                Cursor.loc = TEAM_WINDOW_LOC;
                Cursor.height = 8;
            }
            else
            {
                Cursor.loc = Team_Windows[Index].loc;
                Cursor.height = (Groups[Index].Value + 1) * 16 + 8;
            }
            int leader_id = -1;
            if (Groups.Count > 0)
                leader_id = Global.game_map.team_leaders[Groups[Index].Key];
            if (leader_id == -1 || Global.game_map.unit_defeated(leader_id) ||
                Global.game_map.unit_escaped(leader_id))
            {
                Leader_Name.text = "";
                Lvl.text = "--";
                Hp.text = "--";
                Hp_Max.text = "--";
                Rating.text = "--";
                Map_Sprite.texture = null;
                Face.set_actor("");
                LeaderWeapon.set_image(null, null);
            }
            else
            {
                Game_Unit unit = Global.game_map.units[leader_id];
                Leader_Name.text = unit.actor.name;
                Leader_Name.offset.X = Leader_Name.text_width / 2;
                Lvl.text = unit.actor.level.ToString();
                Hp.text = unit.actor.hp.ToString();
                Hp_Max.text = unit.actor.maxhp.ToString();
                Rating.text = unit.rating().ToString();
                Map_Sprite.texture = Scene_Map.get_team_map_sprite(unit.team, unit.map_sprite_name);
                if (Map_Sprite.texture != null)
                    Map_Sprite.offset = new Vector2(
                        (Map_Sprite.texture.Width / Map_Sprite.frame_count) / 2,
                        (Map_Sprite.texture.Height / Map_Sprite.facing_count) - 8);
                Map_Sprite.mirrored = unit.has_flipped_map_sprite;
                Face.set_actor(unit.actor);
                Face.mirrored = false;
                LeaderWeapon.set_image(unit.actor, unit.actor.weapon);
            }
        }

        #region Update
        private void update_map_sprite()
        {
            int old_frame = Map_Sprite_Frame;
            Map_Sprite_Frame = Global.game_system.unit_anim_idle_frame;
            if (Map_Sprite_Frame != old_frame)
                Map_Sprite.frame = Map_Sprite_Frame;
        }

        protected override void black_screen_switch()
        {
            Visible = !Visible;
            if (!_Closing)
            {
            }
        }

        protected override void UpdateMenu(bool active)
        {
            update_map_sprite();
            Cursor.update();
            Counter.update();
            Left_Page_Arrow.update();
            Right_Page_Arrow.update();

            base.UpdateMenu(active);

            if (ChangingPage)
            {
                (PageLoc.X < target_page_loc.X ? Right_Page_Arrow : Left_Page_Arrow).twirling_update();
                float distance = Math.Abs(PageLoc.X - target_page_loc.X) / 2;
                distance = MathHelper.Clamp(MathHelper.Lerp(0, distance, ChangingPageTime / 12f), 0, distance);
                distance = Math.Min(distance, 32);
                distance = Math.Max(distance, 1);
                distance = (float)Math.Pow(2, Math.Round(Math.Log(distance, 2)));
                PageLoc.X = (float)Additional_Math.double_closer(PageLoc.X, target_page_loc.X, (int)distance);

                ChangingPageTime++;
                if (PageLoc.X == target_page_loc.X)
                    ChangingPage = false;
            }
        }

        protected override void update_input(bool active)
        {
            if (active && this.ready_for_inputs)
            {
                if (Global.Input.repeated(Inputs.Down))
                {
                    if (Index < Groups.Count - 1 && page == 0)
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move1);
                        move_down();
                    }
                }
                else if (Global.Input.repeated(Inputs.Up))
                {
                    if (Index > 0 && page == 0)
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move1);
                        move_up();
                    }
                }
                else if (Global.Input.triggered(Inputs.Left) ||
                    Global.Input.gesture_triggered(TouchGestures.SwipeRight)) //Debug
                {
                    if (page > 0)
                    {
                        Global.game_system.play_se(System_Sounds.Status_Page_Change);
                        page_left();
                    }
                }
                else if (Global.Input.triggered(Inputs.Right) ||
                    Global.Input.gesture_triggered(TouchGestures.SwipeLeft)) //Debug
                {
                    if (page < 1)
                    {
                        Global.game_system.play_se(System_Sounds.Status_Page_Change);
                        page_right();
                    }
                }
                else if (Global.Input.triggered(Inputs.B) ||
                    Global.Input.gesture_triggered(TouchGestures.LongPress)) //Debug
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    close();
                }
            }
        }

        protected override void close()
        {
            base.close();
        }
        #endregion

        #region Movement
        private void move_down()
        {
            Index++;
            set_images();
        }

        private void move_up()
        {
            Index--;
            set_images();
        }

        private void page_left()
        {
            page--;
            ChangingPage = true;
            ChangingPageTime = 0;
        }

        private void page_right()
        {
            page++;
            ChangingPage = true;
            ChangingPageTime = 0;
        }
        #endregion

        #region Draw
        protected override void draw_background(SpriteBatch sprite_batch)
        {
            if (Background != null)
            {
                Effect background_shader = Global.effect_shader();
                if (background_shader != null)
                {
                    background_shader.CurrentTechnique = background_shader.Techniques["Technique2"];
                    background_shader.Parameters["color_shift"].SetValue(new Color(128, 0, 0, 64).ToVector4());
                }
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, background_shader);
                Background.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            Vector2 offset = PageLoc;
            draw_leader_data(sprite_batch, offset);
            draw_file_data(sprite_batch, offset);
            if (page == 0 || ChangingPage)
                draw_team_data(sprite_batch, offset);
            if (page == 1 || ChangingPage)
            {
                draw_gameplay_data(sprite_batch, offset);
                draw_ranking_data(sprite_batch, offset);
            }
            draw_objective_data(sprite_batch, offset);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Banner
            BannerBg.draw(sprite_batch);
            Banner.draw(sprite_batch);

            // Page Arrows
            if (page == 0 || ChangingPage)
                Right_Page_Arrow.draw(sprite_batch);
            if (page == 1 || ChangingPage)
                Left_Page_Arrow.draw(sprite_batch);
            sprite_batch.End();
        }

        private void draw_leader_data(SpriteBatch sprite_batch, Vector2 offset)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Team leader
            // Background
            UnitBg.draw(sprite_batch, offset);
            // Labels
            Lv_Label.draw(sprite_batch, offset);
            Hp_Label.draw(sprite_batch, offset);
            Hp_Slash.draw(sprite_batch, offset);
            RatingLabel.draw(sprite_batch, offset);
            sprite_batch.End();
            // Data
            Face.draw(sprite_batch, offset);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Leader_Name.draw(sprite_batch, offset);
            Lvl.draw(sprite_batch, offset);
            Hp.draw(sprite_batch, offset);
            Hp_Max.draw(sprite_batch, offset);
            Rating.draw(sprite_batch, offset);
            LeaderWeapon.draw(sprite_batch, offset);
            Map_Sprite.draw(sprite_batch, offset);
            sprite_batch.End();
        }

        private void draw_file_data(SpriteBatch sprite_batch, Vector2 offset)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Team leader
            // Background
            FileBg.draw(sprite_batch, offset);
            // Labels
            FileLabel.draw(sprite_batch, offset);
            // Data
            FileNumber.draw(sprite_batch, offset);
            DifficultyText.draw(sprite_batch, offset);
            StyleText.draw(sprite_batch, offset);
            sprite_batch.End();
        }

        private void draw_gameplay_data(SpriteBatch sprite_batch, Vector2 offset)
        {
            // Turns/Funds panel
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Game_Data_Window.draw(sprite_batch, offset);
            Turn_Label.draw(sprite_batch, offset);
            Funds_Label.draw(sprite_batch, offset);
            Gold_G.draw(sprite_batch, offset);
            Turn.draw(sprite_batch, offset);
            Funds.draw(sprite_batch, offset);
            Counter.draw(sprite_batch, offset);
            sprite_batch.End();
        }

        private void draw_team_data(SpriteBatch sprite_batch, Vector2 offset)
        {
            // Team panels
            Effect background_shader = Global.effect_shader();
            if (background_shader != null)
            {
                background_shader.CurrentTechnique = background_shader.Techniques["Technique2"];
                background_shader.Parameters["color_shift"].SetValue(new Color(255, 255, 255, 32).ToVector4());
            }
            for (int i = 0; i < Team_Windows.Count; i++)
            {
                if (i == Index)
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, background_shader);
                else
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Team_Windows[i].draw(sprite_batch, offset);
                sprite_batch.End();
            }

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Data
            foreach (FE_Text name in Group_Names)
                name.draw(sprite_batch, offset);
            foreach (FE_Text count in Group_Counts)
                count.draw(sprite_batch, offset);

            Cursor.draw(sprite_batch, offset);
            sprite_batch.End();
        }

        private void draw_ranking_data(SpriteBatch sprite_batch, Vector2 offset)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Windows
            RankingWindow.draw(sprite_batch, offset);
            // Labels
            RankingLabel.draw(sprite_batch, offset);
            TurnsRankLabel.draw(sprite_batch, offset);
            CombatRankLabel.draw(sprite_batch, offset);
            ExpRankLabel.draw(sprite_batch, offset);
            // Data
            TurnsBar.draw(sprite_batch, offset);
            CombatBar.draw(sprite_batch, offset);
            ExpBar.draw(sprite_batch, offset);
            TotalRankLetter.draw(sprite_batch, offset);
            TurnsRankLetter.draw(sprite_batch, offset);
            CombatRankLetter.draw(sprite_batch, offset);
            ExpRankLetter.draw(sprite_batch, offset);
            sprite_batch.End();
        }

        private void draw_objective_data(SpriteBatch sprite_batch, Vector2 offset)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Windows
            Objective_Window.draw(sprite_batch, offset);
            // Labels
            Objective_Label.draw(sprite_batch, offset);
            Defeat_Label.draw(sprite_batch, offset);
            // Data
            Victory_Text.draw(sprite_batch, offset);
            Loss_Text.draw(sprite_batch, offset);
            sprite_batch.End();
        }
        #endregion
    }
}
