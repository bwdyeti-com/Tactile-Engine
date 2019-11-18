using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Preparations;
using FEXNA_Library;

namespace FEXNA
{
    internal class Window_Prep_Items_Unit : Stereoscopic_Graphic_Object
    {
        const int COLUMNS = 4;
        const int ROW_SIZE = 16;
        readonly static int ROWS = (Config.WINDOW_HEIGHT - (Constants.Actor.NUM_ITEMS + 1) * 16) / ROW_SIZE;
        protected int WIDTH;
        protected int HEIGHT;

        protected int Scroll = 0;
        protected int Rows;
        private bool Active = true;
        private bool Trading = false;
        private int Selected_Unit_Index;
        protected Vector2 Offset = Vector2.Zero;
        protected System_Color_Window Window_Img;
        protected Scroll_Bar Scrollbar;
        protected Hand_Cursor Selected_Cursor;

        protected PartialRangeVisibleUINodeSet<PrepItemsUnitUINode> UnitNodes;
        protected UICursor<PrepItemsUnitUINode> UnitCursor;

        protected Rectangle Unit_Scissor_Rect;
        private RasterizerState Scissor_State = new RasterizerState { ScissorTestEnable = true };

        #region Accessors
        public int width { get { return WIDTH; } }

        public int index
        {
            get { return UnitNodes.ActiveNodeIndex; }
            protected set
            {
                if (value >= 0 && value < UnitNodes.Count())
                {
                    UnitNodes.set_active_node(UnitNodes[value]);
                    UnitNodes.Update(false, visible_indexes_range(false).Enumerate(),
                        Offset - draw_offset);
                }
            }
        }

        public int actor_id
        {
            get { return Global.battalion.actors[this.index]; }
            set
            {
                this.index = Global.battalion.actors.IndexOf(value);
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

        public bool trading
        {
            set
            {
                Trading = value;
                if (Trading)
                {
                    int index = this.index + 1;
                    if (index >= Global.battalion.actors.Count)
                        index = Math.Max(0, index - 2);
                    this.index = index;
                }
                else
                {
                    this.index = Selected_Unit_Index;
                }

                refresh_scroll(false);
                update_cursor();
                UnitCursor.move_to_target_loc();
            }
        }

        public bool darkened { set { ((Prepartions_Item_Window)Window_Img).darkened = value; } }
        #endregion

        public Window_Prep_Items_Unit()
        {
            initialize();
        }

        protected virtual void initialize()
        {
            WIDTH = unit_spacing() * COLUMNS + 32 + 8;
            HEIGHT = ROWS * ROW_SIZE + 8;
            loc = new Vector2((Config.WINDOW_WIDTH - WIDTH) / 2, 0);
            Unit_Scissor_Rect = new Rectangle((int)loc.X, (int)loc.Y + 4, WIDTH, HEIGHT - 8); // - 4); //Debug
            initialize_sprites();
            initialize_index();
        }

        protected virtual void initialize_sprites()
        {
            // Window
            Window_Img = new Prepartions_Item_Window(false);
            Window_Img.width = WIDTH;
            Window_Img.height = HEIGHT;

            refresh_nodes();

            Rows = (int)Math.Ceiling(Global.battalion.actors.Count / (float)columns());
            // Scrollbar
            if (Rows > rows())
            {
                Scrollbar = new Scroll_Bar(rows() * row_size() - 16, Rows, rows(), 0);
                Scrollbar.loc = new Vector2(WIDTH - 4, 12);
                int scrollbar_x = (Config.WINDOW_WIDTH + WIDTH) / 2 - 16;
                Scrollbar.loc = new Vector2(scrollbar_x, 12);

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

            List<PrepItemsUnitUINode> nodes = new List<PrepItemsUnitUINode>();

            // Units
            for (int index = 0; index < Global.battalion.actors.Count; index++)
            {
                Vector2 loc = new Vector2(
                    (index % columns()) * unit_spacing() + 28,
                    (index / columns()) * row_size() + Unit_Scissor_Rect.Y) + unit_offset();
                int id = Global.battalion.actors[index];

                nodes.Add(unit_node(id));
                nodes[index].loc = loc + new Vector2(Unit_Scissor_Rect.X - 8, 0);
            }

            UnitNodes = new PartialRangeVisibleUINodeSet<PrepItemsUnitUINode>(nodes);
            UnitNodes.CursorMoveSound = System_Sounds.Menu_Move1;
            for (int index = 0; index < Global.battalion.actors.Count; index++)
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
            var node = new PrepItemsUnitUINode(Global.game_actors[actorId].name);
            node.Size = new Vector2(unit_spacing(), ROW_SIZE);
            return node;
        }

        protected void initialize_index()
        {
            if (Global.battalion.actors.Contains(
                Global.game_system.Preparations_Actor_Id))
            {
                this.index = Global.battalion.actors.IndexOf(
                    Global.game_system.Preparations_Actor_Id);
                Scroll = (int)MathHelper.Clamp(
                    this.index / columns() - (rows() / 2), 0, Rows - rows());
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
            int id = Global.battalion.actors[index];
            PrepItemsUnitUINode unitNode = UnitNodes[index];
            unitNode.set_map_sprite_texture(
                deployed ? Constants.Team.PLAYER_TEAM : 0,
                Global.game_actors[id].map_sprite_name);
        }

        protected virtual bool map_sprite_ready(int index)
        {
            if (Global.game_system.home_base || !Global.scene.is_map_scene)
                return true;
            if (Global.battalion.is_actor_deployed(index))
                if (Global.game_map.units[Global.game_map.get_unit_id_from_actor(Global.battalion.actors[index])].loc != Config.OFF_MAP)
                    return true;
            return false;
        }

        protected virtual void refresh_font(int i)
        {
            int actor_id = Global.battalion.actors[i];
            bool forced = Global.game_map.forced_deployment.Contains(actor_id);
            if (!forced)
            {
                if (Global.game_system.home_base)
                {
                    //Yeti
                }
                else
                {
                    int unit_id = Global.game_map.get_unit_id_from_actor(actor_id);
                    if (unit_id != -1)
                        forced = !Global.game_map.deployment_points.Contains(Global.game_map.units[unit_id].loc);
                }
            }
            UnitNodes[i].set_name_texture(forced ? "Green" : "White");
        }

        protected Vector2 cursor_loc()
        {
            Vector2 old_loc = new Vector2(
                    (this.index % columns()) * unit_spacing(),
                    (this.index / columns()) * row_size() - Scroll * row_size()) +
                new Vector2(
                    Unit_Scissor_Rect.X,
                    Unit_Scissor_Rect.Y - 4) + unit_offset();

            return new Vector2(
                    (this.index % columns()) * unit_spacing(),
                    (this.index / columns()) * row_size() - Scroll * row_size()) +
                new Vector2(
                    Unit_Scissor_Rect.X,
                    Unit_Scissor_Rect.Y - 4) + unit_offset();
        }

        public void set_selected_loc()
        {
            Selected_Cursor.loc =
                cursor_loc() + new Vector2(8, 4 + Scroll * row_size());
            Selected_Unit_Index = this.index;
        }

        protected virtual int columns()
        {
            return COLUMNS;
        }

        protected virtual int rows()
        {
            return ROWS;
        }
        protected virtual int row_size()
        {
            return ROW_SIZE;
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
                    ((Offset.Y != Scroll * row_size() ? -1 : 1));
            int min = Math.Max(0,
                (int)((Offset.Y / row_size()) + first_line_offset) * columns());
            int max = Math.Min(UnitNodes.Count(),
                (((int)(Offset.Y / row_size()) +
                    (Offset.Y != Scroll * row_size() ? 1 : 0)) + rows()) * columns());
            return new IntRange(min, max - 1);
        }

        #region Update
        protected void update_scroll_offset()
        {
            // come up with a better name for this method //Debug
            int target_y = row_size() * Scroll;
            if (Math.Abs(Offset.Y - target_y) <= row_size() / 4)
                Offset.Y = target_y;
            if (Math.Abs(Offset.Y - target_y) <= row_size())
                Offset.Y = Additional_Math.int_closer((int)Offset.Y, target_y, row_size() / 4);
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
                    Scrollbar.update_input(-draw_offset);
            }
            update_node_location(active);
            update_scroll_offset();
            update_cursor();
        }

        protected void update_cursor()
        {
            UnitCursor.update(new Vector2(0, row_size() * Scroll));
        }

        #endregion

        public Maybe<int> consume_triggered(
            Inputs input, MouseButtons button, TouchGestures gesture)
        {
            return UnitNodes.consume_triggered(input, button, gesture);
        }
        /* //Debug
        public Maybe<int> consume_triggered(Inputs input)
        {
            return UnitNodes.consume_triggered(input);
        }*/

        #region Movement
        internal EventHandler IndexChanged;

        private void update_node_location(bool active)
        {
            int old_index = this.index;
            UnitNodes.Update(active, visible_indexes_range(false).Enumerate(),
                Offset - draw_offset);
            if (old_index != this.index)
            {
                bool vertical_move = Math.Abs(this.index - old_index)
                    >= columns();

                if (vertical_move && Input.ControlScheme == ControlSchemes.Buttons)
                {
                    // Moved down
                    if (old_index < this.index)
                        while (this.index / columns() >= (rows() - 1) + Scroll && Scroll < Rows - (rows()))
                            Scroll++;
                    // Moved up
                    else
                        while (this.index / columns() < Scroll + 1 && Scroll > 0)
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
            if (this.index / columns() >= (rows() - 1) + Scroll && Scroll < Rows - (rows()))
            {
                while (this.index / columns() >= (rows() - 1) + Scroll && Scroll < Rows - (rows()))
                    Scroll++;
            }
            else
            {
                while (this.index / columns() < Scroll + 1 && Scroll > 0)
                    Scroll--;
            }

            if (IndexChanged != null)
                IndexChanged(this, new EventArgs());
            if (instant)
            {
                int target_y = row_size() * Scroll;
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
            if (Scroll < Rows - (rows()))
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
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Window
            draw_window(sprite_batch);
            // Scroll Bar
            if (Scrollbar != null)
                Scrollbar.draw(sprite_batch, -draw_vector());
            // Map sprite (first one, if not scrolling)
            if (Scroll < UnitNodes.Count() && Offset.Y == Scroll * row_size())
            {
                int scroll = Scroll * columns();
                UnitNodes.Draw(sprite_batch,
                    Enumerable.Range(scroll,
                        Math.Min(UnitNodes.Count - scroll, columns())),
                    Offset - draw_vector());
            }
            sprite_batch.End();

            sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(Unit_Scissor_Rect);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            // Map Sprites
            var visible_range = visible_indexes_range();
            if (visible_range.IsValid())
                UnitNodes.Draw(sprite_batch, visible_range.Enumerate(),
                    Offset - draw_vector());

            draw_selected_cursor(sprite_batch);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (Active)
                UnitCursor.draw(sprite_batch, -draw_vector());
            if (Active) //Debug
            { }// Cursor.draw(sprite_batch, -draw_vector());
            sprite_batch.End();
        }

        protected virtual void draw_window(SpriteBatch sprite_batch)
        {
            Vector2 loc = this.loc + draw_vector();
            Window_Img.draw(sprite_batch, -loc);
        }

        protected virtual void draw_selected_cursor(SpriteBatch sprite_batch)
        {
            if (!Active || Trading)
                Selected_Cursor.draw(sprite_batch, Offset - draw_vector());
        }
        #endregion
    }
}
