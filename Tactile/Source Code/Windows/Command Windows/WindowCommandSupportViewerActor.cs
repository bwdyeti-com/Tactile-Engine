using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface.Command;
using Tactile.Windows.UserInterface.Title;
using TactileLibrary;

namespace Tactile.Windows.Command
{
    class WindowCommandSupportViewerActor : Window_Command_Scrollbar
    {
        const int WIDTH = 136;
        const int LINES = 9;

        protected int ActorId;
        private Support_Command_Components Header;
        private List<SupportViewerUINode> Units;
        private Character_Sprite PlayerCursor;

        #region Accessors
        private Data_Actor actorData { get { return Global.data_actors[ActorId]; } }

        public Tuple<string, int> TargetId { get { return this.SupportPartners[this.index]; } }

        protected int SupportsRemaining
        {
            get
            {
                int total = 0, remaining = 0;
                foreach (var tuple in this.SupportPartners)
                {
                    total += Global.data_supports[tuple.Item1].MaxLevel;
                    if (Global.progress.recruitedActors.Contains(tuple.Item2) &&
                        Global.progress.supports.ContainsKey(tuple.Item1))
                    {
                        remaining += Global.progress.supports[tuple.Item1];
                    }
                }

                return total - remaining;
            }
        }

        protected List<Tuple<string, int>> SupportPartners
        {
            get
            {
                return this.actorData.SupportPartners(Global.data_supports, Global.data_actors)
                    .ToList();
            }
        }

        private int ColumnCount { get { return Constants.Support.MAX_SUPPORT_LEVEL; } }
        #endregion

        public WindowCommandSupportViewerActor(int actorId, Vector2 loc)
        {
            Rows = LINES;

            ActorId = actorId;
            Header = new Support_Command_Components(LINES, this.SupportsRemaining, true);
            Header.color_override = 0;

            List<string> strs = GetNames();
            initialize(loc, 8 + 16, strs);
            Window_Img.color_override = 0;
            Window_Img.set_lines(LINES, (int)Size_Offset.Y + 8);

            int width = WIDTH - 16;
            this.text_offset = new Vector2(width - (this.ColumnCount + 1) * 8, 0);
            set_columns(this.ColumnCount);
            this.size_offset = new Vector2(width - this.text_area_width, Size_Offset.Y);
            Window_Img.set_lines(LINES, (int)Size_Offset.Y + 8);

            initialize_scrollbar();
            if (Scrollbar != null)
                Scrollbar.loc += new Vector2(4, 0);

            // Bar
            Glow = true;
            this.glow_width = 16;
            this.bar_offset = new Vector2(-4, 0);

            // Cursor
            UICursor.UpdateTargetLoc(ScrollOffset);
            UICursor.move_to_target_loc();
            PlayerCursor = new Character_Sprite(Global.Content.Load<Texture2D>(@"Graphics/Characters/Cursor"));
            PlayerCursor.offset = new Vector2(4, 0 - 2);

            SetUnits();
        }

        private void SetUnits()
        {
            Units = new List<SupportViewerUINode>();
            for (int i = 0; i < this.SupportPartners.Count; i++)
            {
                string name = "-----";
                if (Global.progress.recruitedActors.Contains(SupportPartners[i].Item2))
                    name = Global.data_actors[SupportPartners[i].Item2].Name.Split(Global.ActorConfig.ActorNameDelimiter)[0];

                var text_node = new SupportViewerUINode(
                    "", ActorId, this.SupportPartners[i].Item2,
                    name, (int)Text_Offset.X);
                text_node.loc = item_loc(i * Columns);
                Units.Add(text_node);
            }
        }

        protected override void set_default_offsets(int width)
        {
            this.text_offset = new Vector2(0, 0);
            this.glow_width = width - (24 + (int)(Text_Offset.X * 2));
            Bar_Offset = new Vector2(0, 0);
            Size_Offset = new Vector2(0, 0);
        }
        
        protected List<string> GetNames()
        {
            List<string> strs = new List<string>();
            foreach (var tuple in this.SupportPartners)
            {
                for (int j = 0; j < this.ColumnCount; j++)
                    strs.Add("");
            }
            return strs;
        }

        protected override void initialize_window()
        {
            Window_Img = new Prepartions_Item_Window(true);
        }
        
        protected override CommandUINode item(object value, int i)
        {
            int lvl = i % this.ColumnCount;
            var tuple = this.SupportPartners.ElementAt(i / this.ColumnCount);

            int rank = 0;
            SupportViewerState state = SupportViewerState.Disabled;
            bool fieldBaseDifference = false;
            if (Global.progress.recruitedActors.Contains(tuple.Item2))
            {
                var otherActor = Global.data_actors[tuple.Item2];
                if (Global.data_supports[tuple.Item1].Supports[lvl].ValidSupport)
                {
                    rank = lvl + 1;
                    fieldBaseDifference = Global.data_supports[tuple.Item1].Supports[lvl].FieldBaseDifference;

                    if (Global.progress.supports.ContainsKey(tuple.Item1) &&
                        lvl < Global.progress.supports[tuple.Item1])
                    {
                        if (Global.data_supports[tuple.Item1].MaxLevel == Global.progress.supports[tuple.Item1])
                            state = SupportViewerState.Capped;
                        else
                            state = SupportViewerState.Enabled;
                    }
                }
            }
            
            var text_node = new SupportViewerRankUINode("", rank, state, fieldBaseDifference);
            text_node.loc = item_loc(i);
            return text_node;
        }

        protected override void refresh_layout()
        {
            base.refresh_layout();

            for (int i = 0; i < num_items(); i++)
                Items[i].Size = new Vector2(8, 16);
        }

        public int SelectedLevel { get { return this.index % Columns; } }
        public string SelectedKey { get { return this.SupportPartners.ElementAt(this.index / this.ColumnCount).Item1; } }

        public void SetAtBase(bool atBase)
        {
            foreach (var item in Items)
                (item as SupportViewerRankUINode).SetAtBase(atBase);
        }

        public bool FieldBaseDifference
        {
            get
            {
                foreach (var item in Items)
                    if ((item as SupportViewerRankUINode).FieldBaseDifference)
                        return true;

                return false;
            }
        }

        protected override void update_commands(bool input)
        {
            if (Units != null)
                for (int i = 0; i < Units.Count; i++)
                    Units[i].Update();
            if (PlayerCursor != null)
            {
                PlayerCursor.update();
                PlayerCursor.frame = Player.CursorFrame;
            }

            base.update_commands(input);
        }

        #region Draw
        protected override void draw_window(SpriteBatch sprite_batch)
        {
            base.draw_window(sprite_batch);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Header.draw(sprite_batch, -(loc + draw_vector()));

            // Draw the top entry of the list, so it overlaps the header
            DrawFirstVisibleRow(sprite_batch);
            if (Scroll * 16 == (int)(ScrollOffset.Y))
                Units[Scroll].Draw(sprite_batch, -(loc + text_draw_vector() - Text_Offset));
            sprite_batch.End();
        }

        protected override void draw_bar(SpriteBatch sprite_batch, Vector2 loc)
        {
            base.draw_bar(sprite_batch, loc);
        }

        protected override void draw_text(SpriteBatch sprite_batch)
        {
            DrawRangeText(sprite_batch);

            var range = visible_indexes_range().Enumerate()
                .Select(x => x / this.ColumnCount)
                .Distinct();

            // Units[i].loc should be 8, 8
            if (Units != null)
                foreach (int i in range)
                    Units[i].Draw(sprite_batch, -(loc + text_draw_vector() - Text_Offset));

            // Draw player cursor
            if (Scroll * 16 != (int)(ScrollOffset.Y))
                DrawPlayerCursor(sprite_batch);
        }

        public override void draw_cursor(SpriteBatch sprite_batch)
        {
            // Draw player cursor
            if (Scroll * 16 == (int)(ScrollOffset.Y))
                DrawPlayerCursor(sprite_batch);

            base.draw_cursor(sprite_batch);
        }

        private void DrawPlayerCursor(SpriteBatch spriteBatch)
        {
            if (active && Items.ActiveNode != null)
            {
                Vector2 cursorOffset = new Vector2(
                    (this.index % Columns) * 8,
                    (this.index / Columns) * 16);
                PlayerCursor.draw(spriteBatch, -(loc + cursorOffset + text_draw_vector()));
            }
        }
        #endregion
    }
}
