using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Preparations;
using FEXNA_Library;

namespace FEXNA.Windows.Preparations
{
    abstract class WindowPrepActorList : Stereoscopic_Graphic_Object
    {
        protected int Scroll = 0;
        protected int Rows;
        private bool Active = true;
        private int Selected_Unit_Index;
        protected Vector2 Offset = Vector2.Zero;
        protected List<int> ActorList;

        protected System_Color_Window Window_Img;
        protected Scroll_Bar Scrollbar;
        protected Hand_Cursor Selected_Cursor;

        protected PartialRangeVisibleUINodeSet<PrepItemsUnitUINode> UnitNodes;
        protected UICursor<PrepItemsUnitUINode> UnitCursor;

        private RasterizerState Scissor_State = new RasterizerState { ScissorTestEnable = true };

        #region Accessors
        public int index
        {
            get { return UnitNodes.ActiveNodeIndex; }
            protected set
            {
                if (value >= 0 && value < UnitNodes.Count())
                {
                    UnitNodes.set_active_node(UnitNodes[value]);
                    UnitNodes.Update(false, visible_indexes_range(false).Enumerate(),
                        Offset - (this.loc + draw_offset));
                }
            }
        }

        public int actor_id
        {
            get { return ActorList[this.index]; }
            set
            {
                this.index = ActorList.IndexOf(value);
                refresh_scroll(false);
                update_cursor();
                UnitCursor.move_to_target_loc();
            }
        }

        public bool active
        {
            set
            {
                Active = value;
                if (!Active)
                {
                    set_selected_loc();
                }
                darkened = !Active;
            }
        }

        public bool darkened { set { ((Prepartions_Item_Window)Window_Img).darkened = value; } }

        protected int ColorOverride
        {
            get { return Window_Img.color_override; }
            set
            {
                Window_Img.color_override = value;
                foreach (var item in UnitNodes)
                    item.SetColorOverride(value);
            }
        }

        protected virtual int Width { get { return unit_spacing() * this.Columns + 32 + 8; } }
        protected virtual int Height { get { return this.VisibleRows * this.RowSize + 8; } }

        protected virtual Rectangle Unit_Scissor_Rect
        {
            get
            {
                Vector2 loc = this.loc + ScissorRectOffset;
                return new Rectangle((int)loc.X, (int)loc.Y, this.Width, this.Height - 8);
            }
        }
        protected virtual Vector2 ScissorRectOffset { get { return new Vector2(0, 4); } }

        protected virtual Vector2 ScrollbarLoc { get { return new Vector2(this.Width - 16, 12); } }

        protected abstract int Columns { get; }
        protected abstract int VisibleRows { get; }
        protected abstract int RowSize { get; }
        #endregion

        public WindowPrepActorList()
        {
            initialize();
        }

        protected virtual void initialize()
        {
            loc = new Vector2((Config.WINDOW_WIDTH - this.Width) / 2, 0);
            initialize_sprites();
            initialize_index();
        }

        protected abstract List<int> GetActorList();

        protected virtual void initialize_sprites()
        {
            // Window
            Window_Img = new Prepartions_Item_Window(false);
            Window_Img.width = this.Width;
            Window_Img.height = this.Height;

            refresh_nodes();

            Rows = (int)Math.Ceiling(ActorList.Count / (float)this.Columns);
            // Scrollbar
            if (Rows > this.VisibleRows)
            {
                Scrollbar = new Scroll_Bar(this.VisibleRows * this.RowSize - 16, Rows, this.VisibleRows, 0);
                Scrollbar.loc = this.ScrollbarLoc;

                Scrollbar.UpArrowClicked += Scrollbar_UpArrowClicked;
                Scrollbar.DownArrowClicked += Scrollbar_DownArrowClicked;
            }
            // Cursor
            Selected_Cursor = new Hand_Cursor();
            Selected_Cursor.loc = cursor_loc() + new Vector2(8, 4);
            Selected_Cursor.tint = new Color(192, 192, 192, 255);
        }

        public void refresh_nodes()
        {
            int active_index = 0;
            if (UnitNodes != null)
                active_index = UnitNodes.ActiveNodeIndex;
            var old_cursor = UnitCursor;

            ActorList = GetActorList();
            List<PrepItemsUnitUINode> nodes = new List<PrepItemsUnitUINode>();

            // Units
            for (int index = 0; index < ActorList.Count; index++)
            {
                Vector2 loc = new Vector2(
                    (index % this.Columns) * unit_spacing() + 28,
                    (index / this.Columns) * this.RowSize + this.ScissorRectOffset.Y) + unit_offset();
                int id = ActorList[index];

                nodes.Add(unit_node(id));
                nodes[index].loc = loc + new Vector2(this.ScissorRectOffset.X - 8, 0);
            }

            UnitNodes = new PartialRangeVisibleUINodeSet<PrepItemsUnitUINode>(nodes);
            UnitNodes.CursorMoveSound = System_Sounds.Menu_Move1;
            for (int index = 0; index < ActorList.Count; index++)
            {
                refresh_map_sprite(index);
                refresh_font(index);
            }
            UnitCursor = new UICursor<PrepItemsUnitUINode>(UnitNodes);
            UnitCursor.draw_offset = new Vector2(-12, 0);

            UnitNodes.set_active_node(UnitNodes[active_index]);
            UnitCursor.move_to_target_loc();
            if (old_cursor != null)
                UnitCursor.loc = old_cursor.loc;

            UnitNodes.Update(false);
        }

        protected virtual PrepItemsUnitUINode unit_node(int actorId)
        {
            var node = new PrepItemsUnitUINode(ActorName(actorId));
            node.Size = new Vector2(unit_spacing(), this.RowSize);
            return node;
        }
        protected abstract string ActorName(int actorId);

        protected void initialize_index()
        {
            // Default index to Global.game_system.Preparations_Actor_Id if possible
            if (ActorList.Contains(
                Global.game_system.Preparations_Actor_Id))
            {
                this.index = ActorList.IndexOf(
                    Global.game_system.Preparations_Actor_Id);
                Scroll = (int)MathHelper.Clamp(
                    this.index / this.Columns - (this.VisibleRows / 2), 0, Rows - this.VisibleRows);
                if (Scrollbar != null)
                    Scrollbar.scroll = Scroll;
                update_cursor();
                UnitCursor.move_to_target_loc();
            }
            else
                this.index = 0;
        }

        protected virtual bool refresh_map_sprite(int index)
        {
            bool deployed = map_sprite_ready(index);
            map_sprite_texture(index, deployed);
            return deployed;
        }
        protected void map_sprite_texture(
            int index, bool deployed)
        {
            int id = ActorList[index];
            PrepItemsUnitUINode unitNode = UnitNodes[index];
            unitNode.set_map_sprite_texture(
                deployed ? Constants.Team.PLAYER_TEAM : 0,
                ActorMapSpriteName(id));
        }
        protected abstract string ActorMapSpriteName(int actorId);

        protected virtual bool map_sprite_ready(int index)
        {
            if (Global.game_system.home_base || !Global.scene.is_map_scene)
                return true;
            return false;
        }

        protected abstract void refresh_font(int i);
        protected void refresh_font(int i, bool forced, bool available = true)
        {
            UnitNodes[i].set_name_texture(forced ? "Green" : (available ? "White" : "Grey"));
        }

        protected Vector2 cursor_loc()
        {
            Vector2 old_loc = new Vector2(
                    (this.index % this.Columns) * unit_spacing(),
                    (this.index / this.Columns) * this.RowSize - Scroll * this.RowSize) +
                unit_offset();

            return new Vector2(
                    (this.index % this.Columns) * unit_spacing(),
                    (this.index / this.Columns) * this.RowSize - Scroll * this.RowSize) +
                unit_offset();
        }

        public void set_selected_loc()
        {
            Selected_Cursor.loc =
                cursor_loc() + new Vector2(8, 4 + Scroll * this.RowSize);
            Selected_Unit_Index = this.index;
        }
        protected void ResetToSelectedIndex()
        {
            this.index = Selected_Unit_Index;
        }

        protected virtual Vector2 unit_offset()
        {
            return new Vector2(0, 0);
        }

        protected virtual int unit_spacing()
        {
            return 64;
        }

        protected IntRange visible_indexes_range(bool skipFirstLine = true)
        {
            int first_line_offset = !skipFirstLine ? 0 :
                    ((Offset.Y != Scroll * this.RowSize ? -1 : 1));
            int min = Math.Max(0,
                (int)((Offset.Y / this.RowSize) + first_line_offset) * this.Columns);
            int max = Math.Min(UnitNodes.Count(),
                (((int)(Offset.Y / this.RowSize) +
                    (Offset.Y != Scroll * this.RowSize ? 1 : 0)) + this.VisibleRows) * this.Columns);
            return new IntRange(min, max - 1);
        }

        #region Update
        protected void update_scroll_offset()
        {
            // come up with a better name for this method //Debug
            int target_y = this.RowSize * Scroll;
            if (Math.Abs(Offset.Y - target_y) <= this.RowSize / 4)
                Offset.Y = target_y;
            if (Math.Abs(Offset.Y - target_y) <= this.RowSize)
                Offset.Y = Additional_Math.int_closer((int)Offset.Y, target_y, this.RowSize / 4);
            else
                Offset.Y = ((int)(Offset.Y + target_y)) / 2;

            if (Offset.Y != target_y && Scrollbar != null)
            {
                if (Offset.Y > target_y)
                    Scrollbar.moving_up();
                else
                    Scrollbar.moving_down();
            }
        }

        public virtual void update(bool active)
        {
            if (Scrollbar != null)
            {
                Scrollbar.update();
                if (active)
                    Scrollbar.update_input(-(this.loc + draw_offset));
            }
            update_node_location(active);
            update_scroll_offset();
            update_cursor();
        }

        protected void update_cursor()
        {
            UnitCursor.update(new Vector2(0, this.RowSize * Scroll) - this.loc);
        }
        #endregion

        public Maybe<int> consume_triggered(
            Inputs input, MouseButtons button, TouchGestures gesture)
        {
            return UnitNodes.consume_triggered(input, button, gesture);
        }

        #region Movement
        internal EventHandler IndexChanged;

        private void update_node_location(bool active)
        {
            int old_index = this.index;
            UnitNodes.Update(active, visible_indexes_range(false).Enumerate(),
                Offset - (this.loc + draw_offset));
            if (old_index != this.index)
            {
                bool vertical_move = Math.Abs(this.index - old_index)
                    >= this.Columns;

                if (vertical_move && Input.ControlScheme == ControlSchemes.Buttons)
                {
                    // Moved down
                    if (old_index < this.index)
                        while (this.index / this.Columns >= (this.VisibleRows - 1) + Scroll && Scroll < Rows - (this.VisibleRows))
                            Scroll++;
                    // Moved up
                    else
                        while (this.index / this.Columns < Scroll + 1 && Scroll > 0)
                            Scroll--;
                }
                if (IndexChanged != null)
                    IndexChanged(this, new EventArgs());
                if (vertical_move)
                {
                    if (Scrollbar != null)
                        Scrollbar.scroll = Scroll;
                }
            }
        }

        public void refresh_scroll(bool instant = true)
        {
            if (this.index / this.Columns >= (this.VisibleRows - 1) + Scroll && Scroll < Rows - (this.VisibleRows))
            {
                while (this.index / this.Columns >= (this.VisibleRows - 1) + Scroll && Scroll < Rows - (this.VisibleRows))
                    Scroll++;
            }
            else
            {
                while (this.index / this.Columns < Scroll + 1 && Scroll > 0)
                    Scroll--;
            }

            if (IndexChanged != null)
                IndexChanged(this, new EventArgs());
            if (instant)
            {
                int target_y = this.RowSize * Scroll;
                Offset.Y = target_y;
            }
        }

        protected void Scrollbar_UpArrowClicked(object sender, EventArgs e)
        {
            if (Scroll > 0)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move1);
                Scroll--;
                Scrollbar.scroll = Scroll;
            }
        }
        protected void Scrollbar_DownArrowClicked(object sender, EventArgs e)
        {
            if (Scroll < Rows - (this.VisibleRows))
            {
                Global.game_system.play_se(System_Sounds.Menu_Move1);
                Scroll++;
                Scrollbar.scroll = Scroll;
            }
        }
        #endregion

        #region Draw
        public void draw(SpriteBatch sprite_batch)
        {
            Vector2 offset = this.loc + draw_vector();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Window
            draw_window(sprite_batch);
            // Scroll Bar
            if (Scrollbar != null)
                Scrollbar.draw(sprite_batch, -offset);
            // Map sprite (first one, if not scrolling)
            if (Scroll < UnitNodes.Count() && Offset.Y == Scroll * this.RowSize)
            {
                int scroll = Scroll * this.Columns;
                UnitNodes.Draw(sprite_batch,
                    Enumerable.Range(scroll,
                        Math.Min(UnitNodes.Count - scroll, this.Columns)),
                    Offset - offset);
            }
            sprite_batch.End();

            sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(Unit_Scissor_Rect);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            // Map Sprites
            var visible_range = visible_indexes_range();
            if (visible_range.IsValid())
                UnitNodes.Draw(sprite_batch, visible_range.Enumerate(),
                    Offset - offset);

            draw_selected_cursor(sprite_batch);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (Active)
                UnitCursor.draw(sprite_batch, -draw_vector());
            sprite_batch.End();
        }

        protected virtual void draw_window(SpriteBatch sprite_batch)
        {
            Vector2 offset = this.loc + draw_vector();

            Window_Img.draw(sprite_batch, -offset);
        }

        protected virtual void draw_selected_cursor(SpriteBatch sprite_batch)
        {
            Vector2 offset = this.loc + draw_vector();

            if (!Active)
                Selected_Cursor.draw(sprite_batch, Offset - offset);
        }
        #endregion
    }
}
