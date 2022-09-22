using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Preparations;
using TactileLibrary;

namespace Tactile.Windows.Preparations
{
    //@Yeti: why isn't this just a Window_Command_Scrollbar
    abstract class WindowPrepActorList : Stereoscopic_Graphic_Object
    {
        protected int Rows;
        private bool Active = true;
        protected int Selected_Unit_Index { get; private set; }
        protected List<int> ActorList;
        protected IndexScrollComponent Scroll;

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
                        Scroll.IntOffset - (this.loc + draw_offset));
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
            }
        }

        public bool active
        {
            set
            {
                Active = value;
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
                return new Rectangle((int)loc.X, (int)loc.Y, this.Width, this.VisibleRows * this.RowSize);
            }
        }
        protected virtual Vector2 ScissorRectOffset { get { return new Vector2(0, 4); } }

        protected virtual Vector2 ScrollbarLoc { get { return new Vector2(this.Width - 16, 12); } }

        protected virtual bool TouchMoveBlocked { get { return false; } }

        protected virtual bool CursorSelected { get { return !Active; } }

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
            CreateScrollbar();
            // Cursor
            Selected_Cursor = new Hand_Cursor();
            Selected_Cursor.draw_offset = new Vector2(-12, 0);
            Selected_Cursor.tint = new Color(192, 192, 192, 255);
        }

        protected void CreateScrollbar()
        {
            Scrollbar = null;
            if (Rows > this.VisibleRows)
            {
                Scrollbar = new Scroll_Bar(this.VisibleRows * this.RowSize - 16, Rows, this.VisibleRows, 0);
                Scrollbar.loc = this.ScrollbarLoc;
            }
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
                    (index % this.Columns) * unit_spacing(),
                    (index / this.Columns) * this.RowSize);
                int id = ActorList[index];

                nodes.Add(unit_node(id));
                nodes[index].loc = loc + this.ScissorRectOffset + unit_offset();
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
            Scroll = new IndexScrollComponent(
                new Vector2(this.Columns * unit_spacing(), this.Unit_Scissor_Rect.Height),
                new Vector2(unit_spacing(), this.RowSize),
                ScrollAxes.Vertical);
            Scroll.loc = this.ScissorRectOffset + unit_offset();
            Scroll.Scrollbar = Scrollbar;
            Scroll.SetElementLengths(new Vector2(this.Columns, Rows));
            Scroll.SetBuffers(new Rectangle(1, 2, 2, 4));
            Scroll.SetResolveToIndex(true);

            // Default index to Global.game_system.Preparations_Actor_Id if possible
            if (ActorList.Contains(
                Global.game_system.Preparations_Actor_Id))
            {
                this.index = ActorList.IndexOf(
                    Global.game_system.Preparations_Actor_Id);
                refresh_scroll();
            }
            else
                this.index = 0;

            set_selected_loc();
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
            return new Vector2(
                    (this.index % this.Columns) * unit_spacing(),
                    (this.index / this.Columns) * this.RowSize) +
                unit_offset() + this.ScissorRectOffset - Scroll.IntOffset;
        }

        public void set_selected_loc()
        {
            if (Selected_Cursor != null)
            {
                Selected_Cursor.loc =
                cursor_loc() + Scroll.IntOffset;
            }
            Selected_Unit_Index = this.index;
        }
        protected void ResetToSelectedIndex()
        {
            this.index = Selected_Unit_Index;
        }

        protected virtual Vector2 unit_offset()
        {
            return new Vector2(20, 0);
        }

        protected virtual int unit_spacing()
        {
            return 64;
        }

        protected IntRange visible_indexes_range(bool skipFirstLine = true)
        {
            int first_line_offset = !skipFirstLine ? 0 :
                    ((Scroll.IsScrolling ? -1 : 1));
            int min = Math.Max(0,
                (int)((Scroll.IntOffset.Y / this.RowSize) + first_line_offset) * this.Columns);
            int max = Math.Min(UnitNodes.Count(),
                (((int)(Scroll.IntOffset.Y / this.RowSize) +
                    (Scroll.IsScrolling ? 1 : 0)) + this.VisibleRows) * this.Columns);

            return new IntRange(min, max - 1);
        }

        #region Update
        protected void UpdateScroll(bool active)
        {
            Scroll.Update(active, UnitNodes.ActiveNodeIndex, -this.loc);
            if (Scroll.Index >= 0 && Scroll.Index < UnitNodes.Count)
                UnitNodes.set_active_node(UnitNodes[Scroll.Index]);
            if (Scrollbar != null)
                Scrollbar.scroll = (int)Scroll.IntOffset.Y / this.RowSize;
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
            UpdateScroll(active);
            update_cursor();
        }

        protected void update_cursor()
        {
            UnitCursor.update(this.ScissorRectOffset + unit_offset());
        }
        #endregion

        public ConsumedInput consume_triggered(
            Inputs input, MouseButtons button, TouchGestures gesture)
        {
            return UnitNodes.consume_triggered(input, button, gesture);
        }

        #region Movement
        internal EventHandler IndexChanged;

        private void update_node_location(bool active)
        {
            int old_index = this.index;
            // Disable touch move if needed
            ControlSet control = !active ? ControlSet.None :
                (!this.TouchMoveBlocked ? ControlSet.All :
                ControlSet.All & ~ControlSet.TouchMove);
            UnitNodes.Update(control, visible_indexes_range(false).Enumerate(),
                Scroll.IntOffset - (this.loc + draw_offset));
            if (old_index != this.index)
            {
                if (IndexChanged != null)
                    IndexChanged(this, new EventArgs());
            }
        }

        public void refresh_scroll(bool instant = true)
        {
            if (IndexChanged != null)
                IndexChanged(this, new EventArgs());
            if (instant)
            {
                Scroll.FixScroll(this.index);
                if (Scrollbar != null)
                    Scrollbar.scroll = (int)Scroll.IntOffset.Y / this.RowSize;
            }

            update_cursor();
            UnitCursor.move_to_target_loc();
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
            sprite_batch.End();

            // Map Sprites
            draw_units(sprite_batch);

            sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(Unit_Scissor_Rect);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            draw_selected_cursor(sprite_batch);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (Active)
                UnitCursor.draw(sprite_batch, Scroll, -(this.ScissorRectOffset + unit_offset() + offset));

            sprite_batch.End();
        }

        protected virtual void draw_window(SpriteBatch sprite_batch)
        {
            Vector2 offset = this.loc + draw_vector();

            Window_Img.draw(sprite_batch, -offset);
        }

        protected virtual void draw_units(SpriteBatch spriteBatch)
        {
            Vector2 offset = this.loc + draw_vector();

            // Map sprite (first row, if not scrolling)
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            int scrollOffset = (int)Scroll.IntOffset.Y / this.RowSize;
            if (scrollOffset < UnitNodes.Count() && !Scroll.IsScrolling)
            {
                int scroll = scrollOffset * this.Columns;
                UnitNodes.Draw(spriteBatch,
                    Enumerable.Range(scroll,
                        Math.Min(UnitNodes.Count - scroll, this.Columns)),
                    Scroll.IntOffset - offset);
            }
            spriteBatch.End();

            // Draw other units
            spriteBatch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(Unit_Scissor_Rect);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);

            var visible_range = visible_indexes_range();
            if (visible_range.IsValid())
                UnitNodes.Draw(spriteBatch, visible_range.Enumerate(),
                    Scroll.IntOffset - offset);

            spriteBatch.End();
        }

        protected virtual void draw_selected_cursor(SpriteBatch sprite_batch)
        {
            Vector2 offset = this.loc + draw_vector();

            if (this.CursorSelected)
                Selected_Cursor.draw(sprite_batch, Scroll.IntOffset - offset);
        }
        #endregion
    }
}
