﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Graphics.Text;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Command;
using TactileLibrary;

namespace Tactile.Windows.Map
{
    class WindowRankingsOverview : Map_Window_Base
    {
        const bool SHOW_ALL_CHAPTERS = true;
        const int ROWS = 8;
        const int COLUMN_WIDTH = 80;
        const int SCROLL_SPEED = 4;
        readonly static Vector2 DATA_OFFSET = new Vector2(32, 32);

        private string Chapter;
        private Difficulty_Modes Difficulty;
        private List<KeyValuePair<string, Game_Ranking>> Rankings;
        private IndexScrollComponent Scroll;
        private int CharHeight;
        private UIEmptyObject LeftScrollRegion, RightScrollRegion;

        private Button_Description CancelButton;
        private Sprite RankingIcons; //
        private UINodeSet<TextUINode> Nodes;
        new private UICursor<TextUINode> Cursor; //@Yeti
        private Window_Ranking DetailedRanking;
        private Sprite DataBackground;
        private TextSprite StyleText, DifficultyText;

        private bool cancel_button_triggered
        {
            get
            {
                return CancelButton.consume_trigger(MouseButtons.Left) ||
                    CancelButton.consume_trigger(TouchGestures.Tap) ||
                    Global.Input.mouse_click(MouseButtons.Right);
            }
        }

        protected override bool ready_for_inputs { get { return base.ready_for_inputs && DetailedRanking == null; } }

        public WindowRankingsOverview(string chapter, Difficulty_Modes difficulty)
        {
            Chapter = chapter;
            Difficulty = difficulty;
            CharHeight = Font_Data.Data[Config.UI_FONT].CharHeight;

            initialize_sprites();
            update_black_screen();

            // Jump to chapter
            int index = Rankings.FindIndex(x => x.Key == chapter);
            Nodes.set_active_node(Nodes[index]);
            Cursor.UpdateTargetLoc();
            Cursor.move_to_target_loc();
            Scroll.FixScroll(index);
        }

        protected void initialize_sprites()
        {
            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(
                @"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 255);
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Pictures/Status_Background");
            (Background as Menu_Background).vel = new Vector2(-0.25f, 0);
            (Background as Menu_Background).tile = new Vector2(3, 2);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
            // Cancel Button
            CancelButton = Button_Description.button(Inputs.B, 32);
            CancelButton.loc = new Vector2(32, Config.WINDOW_HEIGHT - 24);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;

            RankingIcons = new Sprite();
            RankingIcons.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Pictures/RankingIcons");
            RankingIcons.loc = DATA_OFFSET + new Vector2(48, -16);

            DataBackground = new Sprite();
            DataBackground.texture = Global.Content.Load<Texture2D>(
                @"Graphics/White_Square");
            DataBackground.dest_rect = new Rectangle(
                0, (int)DATA_OFFSET.Y,
                Config.WINDOW_WIDTH, ROWS * CharHeight);
            DataBackground.tint = new Color(0, 0, 0, 128);

            // Style
            StyleText = new TextSprite();
            StyleText.loc = new Vector2(
                Config.WINDOW_WIDTH - 112, Config.WINDOW_HEIGHT - 24);
            StyleText.SetFont(Config.UI_FONT, Global.Content, "Blue");
            StyleText.text = Global.game_system.Style.ToString();
            StyleText.stereoscopic = Config.DATA_LEADER_DEPTH;
            // Difficulty
            DifficultyText = new TextSprite();
            DifficultyText.loc = new Vector2(
                Config.WINDOW_WIDTH - 56, Config.WINDOW_HEIGHT - 24);
            DifficultyText.SetFont(Config.UI_FONT, Global.Content, "Blue");
            DifficultyText.text = Difficulty.ToString();
            DifficultyText.stereoscopic = Config.DATA_LEADER_DEPTH;

            refresh_rankings(Chapter);
        }

        private void refresh_rankings(string chapter)
        {
            // Get the rankings based on the selected chapter
            var current_rankings = Global.save_file.all_rankings(chapter);
            var rankings = current_rankings
                .ToDictionary(p => p.Key, p => p.Value);
            // Get the rankings of all other chapters, and show them greyed out
            if (SHOW_ALL_CHAPTERS)
            {
                // Sort chapters
                var rankedChapters = Global.Chapter_List
                    .Where(x => !Global.data_chapters[x].Unranked)
                    .GroupBy(x => Global.data_chapters[x].Arc)
                    .OrderBy(x =>
                    {
                        int index = Constants.WorldMap.GAME_ARCS.Count;
                        if (Constants.WorldMap.GAME_ARCS.Contains(x.Key))
                            index = Constants.WorldMap.GAME_ARCS.IndexOf(x.Key);
                        return index;
                    })
                    .SelectMany(x => x
                        .OrderBy(y => Global.Chapter_List.IndexOf(y))
                        .ToList())
                    .ToList();
                // Get all cleared chapters, and
                // their ranking data accoring to themselves
                rankings = rankedChapters
                    .Where(x => Global.save_file.ContainsKey(x))
                    .ToDictionary(x => x, x => Global.save_file.ranking(x));
                // Overwrite each chapter based on the current chapter's history
                foreach (var key in current_rankings.Keys)
                    rankings[key] = current_rankings[key];
                rankings = rankings
                    .OrderBy(x => rankedChapters.IndexOf(x.Key))
                    .ToDictionary(p => p.Key, p => p.Value);
            }

            Rankings = rankings.ToList();

            int i = 0;
            List<TextUINode> ranks = new List<TextUINode>();
            foreach (string ch in Global.Chapter_List)
            {
                var chapter_data = Global.data_chapters[ch];

                if (rankings.ContainsKey(ch))
                {
                    var ranking = rankings[ch];

                    // Color the label yellow for history of the current
                    // chapter, grey if just other data from this file
                    string color = current_rankings.ContainsKey(ch) ?
                        "Yellow" : "Grey";
                    var text = new TextSprite();
                    text.SetFont(Config.UI_FONT, Global.Content, color);
                    text.text = chapter_data.ListName;
                    text.stereoscopic = Config.OPTIONS_OPTIONS_DEPTH; //@Yeti

                    var node = new RankingUINode("", text, COLUMN_WIDTH, ranking);
                    node.loc = DATA_OFFSET +
                        new Vector2((i / ROWS * COLUMN_WIDTH), (i % ROWS) * CharHeight);

                    ranks.Add(node);
                    i++;
                }
            }

            Nodes = new UINodeSet<TextUINode>(ranks);
            Nodes.AngleMultiplier = 4f;
            Nodes.TangentDirections = new List<CardinalDirections>
                { CardinalDirections.Left, CardinalDirections.Right };
            Nodes.refresh_destinations();

            Cursor = new UICursor<TextUINode>(Nodes);
            Cursor.draw_offset = new Vector2(-16, 0);

            LeftScrollRegion = new UIEmptyObject(new Vector2(DATA_OFFSET.X, ROWS * CharHeight));
            LeftScrollRegion.loc = new Vector2(0, DATA_OFFSET.Y);
            RightScrollRegion = new UIEmptyObject(new Vector2(DATA_OFFSET.X, ROWS * CharHeight));
            RightScrollRegion.loc = new Vector2(Config.WINDOW_WIDTH - DATA_OFFSET.X, DATA_OFFSET.Y);

            Scroll = new IndexScrollComponent(
                new Vector2(Config.WINDOW_WIDTH - DATA_OFFSET.X * 2, ROWS * CharHeight),
                new Vector2(COLUMN_WIDTH, CharHeight),
                ScrollAxes.Horizontal);
            Scroll.SetScrollSpeeds(
                SCROLL_SPEED,
                Config.CONVO_BACKLOG_TOUCH_SCROLL_FRICTION);
            Scroll.loc = DATA_OFFSET;
            Scroll.LeftMouseOver = LeftScrollRegion;
            Scroll.RightMouseOver = RightScrollRegion;
            Scroll.SetElementLengths(new Vector2(
                (ranks.Count + ROWS - 1) / ROWS, Math.Min(ranks.Count, ROWS)));
        }

        protected override void UpdateMenu(bool active)
        {
            bool input = active && this.ready_for_inputs;

            if (DetailedRanking != null)
            {
                DetailedRanking.update();
                if (DetailedRanking.is_ready)
                {
                    DetailedRanking = null;
                }
            }

            base.UpdateMenu(input);

            StyleText.update();
            DifficultyText.update();
        }

        protected override void update_input(bool active)
        {
            active &= DetailedRanking == null;

            CancelButton.Update(active);
            Nodes.Update(active, Scroll.IntOffset);
            Cursor.update();

            Scroll.Update(active, Nodes.ActiveNodeIndex);
            // Set index back onto nodes if scrolling updated it
            if (Scroll.Index >= 0 && Scroll.Index < Nodes.Count)
                Nodes.set_active_node(Nodes[Scroll.Index]);

            if (active)
            {
                var selected = Nodes.consume_triggered(
                    Inputs.A, MouseButtons.Left, TouchGestures.Tap);

                if (selected.IsSomething)
                {
                    DetailedRanking = new Window_Ranking(Rankings[selected].Value);
                }
                else if (Global.Input.triggered(Inputs.B) || cancel_button_triggered)
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    close();
                }
            }
        }


        protected override void draw_window(SpriteBatch sprite_batch)
        {
            Vector2 scroll = Scroll.IntOffset;

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            DataBackground.draw(sprite_batch);
            //for (int i = 0; i < Math.Ceiling(Nodes.Count / (float)ROWS); i++) //Debug
            //    RankingIcons.draw(sprite_batch, -new Vector2(i * COLUMN_WIDTH, 0));
            Nodes.Draw(sprite_batch, scroll);
            Cursor.draw(sprite_batch, scroll);

            CancelButton.Draw(sprite_batch);
            StyleText.draw(sprite_batch);
            //DifficultyText.draw(sprite_batch); //Debug

            sprite_batch.End();

            if (DetailedRanking != null)
                DetailedRanking.draw(sprite_batch);
        }
    }
}
