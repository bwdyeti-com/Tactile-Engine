using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Preparations;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Preparations;

namespace Tactile
{
    class Window_Prep_PickUnits_Unit : Window_Prep_Items_Unit
    {
        const int COLUMNS = 2;
        const int ROW_SIZE = 16;
        const int ROWS = (Config.WINDOW_HEIGHT - 64) / ROW_SIZE;

        private TextSprite Pick_Label, More_Units_Label, Slash_Label;
        private RightAdjustedText Units_Left, Units_Selected, Units_Total;
        private Pick_Units_Header Unit_Header;
        private int Total_Units, Unit_Count, Other_Units;
        private Icon_Sprite TalkIcon;
        private List<int> TalkIndices = new List<int>();

        #region Accessors
        public int unit_count
        {
            set
            {
                Unit_Count = value;
                refresh_unit_counts();
            }
        }
        #endregion

        public Window_Prep_PickUnits_Unit(int total_units, int unit_count, int other_units)
        {
            this.loc = new Vector2((Config.WINDOW_WIDTH - this.Width), 20);

            TalkIcon = new Icon_Sprite();
            TalkIcon.texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/TalkIcon");
            TalkIcon.loc = new Vector2(0, -Constants.Map.TILE_SIZE / 4);
            TalkIcon.size = new Vector2(32, 32);
            TalkIcon.columns = 5;
            TalkIcon.index = 0;
            TalkIcon.visible = false;

            for (int i = 0; i < UnitNodes.Count(); i++)
            {
                int id = Global.battalion.actors[i];
                if (Global.game_state.CanActorTalk(id))
                    TalkIndices.Add(i);
            }

            Total_Units = total_units;
            Unit_Count = unit_count;
            Other_Units = other_units;
            refresh_unit_counts();
        }

        #region WindowPrepActorList Abstract
        protected override int Columns { get { return COLUMNS; } }
        protected override int VisibleRows { get { return ROWS; } }
        protected override int RowSize { get { return ROW_SIZE; } }
        #endregion

        protected override int Width { get { return unit_spacing() * this.Columns + 8 + 16; } }

        protected override Vector2 ScissorRectOffset { get { return new Vector2(0, 4); } }

        protected override Vector2 ScrollbarLoc { get { return new Vector2(this.Width + 2 - 16, 12); } }

        protected override void initialize_sprites()
        {
            // Window
            Window_Img = new System_Color_Window();
            Window_Img.width = this.Width;
            Window_Img.height = this.Height + 12;
            Window_Img.offset = new Vector2(0, 8);
            // UI Nodes
            refresh_nodes();

            Rows = (int)Math.Ceiling(Global.battalion.actors.Count / (float)this.Columns);
            // Scrollbar
            if (Rows > this.VisibleRows)
            {
                Scrollbar = new Scroll_Bar(this.VisibleRows * this.RowSize - 16, Rows, this.VisibleRows, 0);
                Scrollbar.loc = this.ScrollbarLoc;

                Scrollbar.UpArrowClicked += Scrollbar_UpArrowClicked;
                Scrollbar.DownArrowClicked += Scrollbar_DownArrowClicked;
            }
            // Unit Header
            Unit_Header = new Pick_Units_Header(this.Width + 8);
            Unit_Header.loc = new Vector2(-8, -20);
            // Labels
            Pick_Label = new TextSprite();
            Pick_Label.loc = new Vector2(4, -12);
            Pick_Label.SetFont(Config.UI_FONT, Global.Content, "White");
            Pick_Label.text = "Pick";
            More_Units_Label = new TextSprite();
            More_Units_Label.loc = new Vector2(44, -12);
            More_Units_Label.SetFont(Config.UI_FONT, Global.Content, "White");
            More_Units_Label.text = "more units";
            Slash_Label = new TextSprite();
            Slash_Label.loc = new Vector2(108, -12);
            Slash_Label.SetFont(Config.UI_FONT, Global.Content, "White");
            Slash_Label.text = "/";
            // Data
            Units_Left = new RightAdjustedText();
            Units_Left.loc = new Vector2(40, -12);
            Units_Left.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Units_Selected = new RightAdjustedText();
            Units_Selected.loc = new Vector2(108, -12);
            Units_Selected.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Units_Total = new RightAdjustedText();
            Units_Total.loc = new Vector2(132, -12);
            Units_Total.SetFont(Config.UI_FONT, Global.Content, "Blue");
        }

        public override void update(bool active)
        {
            base.update(active);

            // Blink talk icon
            if (Global.game_map != null)
                TalkIcon.visible = Global.game_map.icons_visible;
        }

        public void refresh_unit(bool deployed)
        {
            int id = Global.battalion.actors[this.index];
            map_sprite_texture(UnitNodes.ActiveNodeIndex, deployed);
            refresh_font(this.index, id, deployed);
        }

        protected override void refresh_font(int index)
        {
            int id = Global.battalion.actors[index];
            bool deployed = map_sprite_ready(index);
            refresh_font(index, id, deployed);
        }
        private void refresh_font(int i, int actor_id, bool deployed)
        {
            // If forced by events
            bool forced = Global.game_map.forced_deployment.Contains(actor_id);
            if (!forced)
            {
                int unit_id = Global.game_map.get_unit_id_from_actor(actor_id);
                if (unit_id != -1)
                    // If predeployed
                    forced = !Global.game_map.deployment_points.Contains(Global.game_map.units[unit_id].loc);
            }

            refresh_font(i, forced, deployed);
        }

        private void refresh_unit_counts()
        {
            bool maxed = Unit_Count == Total_Units;
            Units_Left.text = (Total_Units - Unit_Count).ToString();
            Units_Selected.text = (Unit_Count + Other_Units).ToString();
            Units_Total.text = (Total_Units + Other_Units).ToString();
            Units_Left.SetColor(Global.Content, maxed ? "Grey" : "Blue");
            Units_Selected.SetColor(Global.Content, maxed ? "Green" : "Blue");
            Units_Total.SetColor(Global.Content, maxed ? "Green" : "Blue");
        }
        
        protected override Vector2 unit_offset()
        {
            return new Vector2(-12, 0);
        }

        protected override int unit_spacing()
        {
            return 64;
        }

        #region Draw
        protected override void draw_window(SpriteBatch sprite_batch)
        {
            Vector2 offset = this.loc + draw_vector();

            base.draw_window(sprite_batch);
            Unit_Header.draw(sprite_batch, -offset);
            // Labels
            Pick_Label.draw(sprite_batch, -offset);
            More_Units_Label.draw(sprite_batch, -offset);
            Slash_Label.draw(sprite_batch, -offset);
            // Data
            Units_Left.draw(sprite_batch, -offset);
            Units_Selected.draw(sprite_batch, -offset);
            Units_Total.draw(sprite_batch, -offset);
        }

        protected override void draw_units(SpriteBatch spriteBatch)
        {
            Vector2 offset = this.loc + draw_vector() - Offset;

            base.draw_units(spriteBatch);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Get the range of indices that are completely visible
            int count = (ROWS - (Offset.Y == Scroll * this.RowSize ? 0 : 1)) * COLUMNS;
            int start = (Scroll + (Offset.Y <= Scroll * this.RowSize ? 0 : 1)) * COLUMNS;
            List<int> indices = Enumerable.Range(start, count).ToList();

            foreach (int index in indices.Intersect(TalkIndices))
            {
                int ox = (index % COLUMNS) * unit_spacing();
                int oy = (index / COLUMNS) * ROW_SIZE;
                TalkIcon.draw(spriteBatch, -(offset + new Vector2(ox, oy)));
            }
            spriteBatch.End();
        }
        #endregion
    }
}
