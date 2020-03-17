using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Preparations;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Preparations;

namespace FEXNA
{
    class Window_Prep_PickUnits_Unit : Window_Prep_Items_Unit
    {
        const int COLUMNS = 2;
        const int ROW_SIZE = 16;
        const int ROWS = (Config.WINDOW_HEIGHT - 64) / ROW_SIZE;

        private FE_Text Pick_Label, More_Units_Label, Slash_Label;
        private FE_Text_Int Units_Left, Units_Selected, Units_Total;
        private Pick_Units_Header Unit_Header;
        private int Total_Units, Unit_Count, Other_Units;

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

        protected override Rectangle Unit_Scissor_Rect
        {
            get
            {
                Vector2 loc = this.loc + ScissorRectOffset;
                return new Rectangle((int)loc.X, (int)loc.Y, this.Width, this.Height - 4);
            }
        }
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
            Pick_Label = new FE_Text();
            Pick_Label.loc = new Vector2(4, -12);
            Pick_Label.Font = "FE7_Text";
            Pick_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Pick_Label.text = "Pick";
            More_Units_Label = new FE_Text();
            More_Units_Label.loc = new Vector2(44, -12);
            More_Units_Label.Font = "FE7_Text";
            More_Units_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            More_Units_Label.text = "more units";
            Slash_Label = new FE_Text();
            Slash_Label.loc = new Vector2(108, -12);
            Slash_Label.Font = "FE7_Text";
            Slash_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Slash_Label.text = "/";
            // Data
            Units_Left = new FE_Text_Int();
            Units_Left.loc = new Vector2(40, -12);
            Units_Left.Font = "FE7_Text";
            Units_Left.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Units_Selected = new FE_Text_Int();
            Units_Selected.loc = new Vector2(108, -12);
            Units_Selected.Font = "FE7_Text";
            Units_Selected.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Units_Total = new FE_Text_Int();
            Units_Total.loc = new Vector2(132, -12);
            Units_Total.Font = "FE7_Text";
            Units_Total.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
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
            Units_Left.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" +
                (maxed ? "Grey" : "Blue"));
            Units_Selected.texture = Units_Total.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" +
                (maxed ? "Green" : "Blue"));
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
        #endregion
    }
}
