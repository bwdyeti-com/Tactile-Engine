using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Command;
using TactileLibrary;

namespace Tactile.Windows.Command
{
    class Window_Command : Stereoscopic_Graphic_Object, ISelectionMenu
    {
        public bool active = true, visible = true;
        protected bool Movement_Locked = false;
        protected int Index; //Debug
        protected int Width;
        protected int Columns = 1;
        protected Vector2 Text_Offset { get; private set; }
        protected Vector2 Size_Offset = Vector2.Zero, Bar_Offset = Vector2.Zero;
        protected List<Inputs> locked_inputs = new List<Inputs>();
        protected bool Glow = false, Greyed_Cursor = false;

        protected System_Color_Window Window_Img;
        protected Hand_Cursor Grey_Cursor;
        protected Unit_Line_Cursor Glowing_Line;
        protected Window_Help Help_Window;

        protected UINodeSet<CommandUINode> Items;
        protected UICursor<CommandUINode> UICursor;

        protected ConsumedInput SelectedIndex;
        protected ConsumedInput HelpIndex;
        private bool Canceled; 

        protected bool Manual_Cursor_Draw = false, Manual_Help_Draw = false;
        protected Maybe<float> Help_Stereo_Offset = new Maybe<float>();

        #region Accessors
        public virtual int index
        {
            get
            {
                if (Items == null)
                    return -1;
                return Items.ActiveNodeIndex;
            }
            set
            {
                int oldIndex = Items.ActiveNodeIndex;
                if (index == -1)
                    Items.set_active_node(null);
                else
                    Items.set_active_node(
                        Items[Math.Max(0, Math.Min(num_items() - 1, value))]);
                if (oldIndex != Items.ActiveNodeIndex)
                    on_index_changed(oldIndex);
            }
        }
        public int immediate_index
        {
            set
            {
                int oldIndex = Items.ActiveNodeIndex;
                Items.set_active_node(
                    Items[Math.Max(0, Math.Min(num_items() - 1, value))]);
                if (oldIndex != Items.ActiveNodeIndex)
                    on_index_changed(oldIndex);

                UICursor.update();
                UICursor.move_to_target_loc();
            }
        }
        internal virtual int rows
        {
            get { return (int)Math.Ceiling(num_items() / (float)Columns); }
        }

        public Vector2 current_cursor_loc
        {
            get
            {
                return (this.loc + Text_Offset) + UICursor.target_loc + UICursor.draw_offset;
            }
            set
            {
                UICursor.set_loc((value - UICursor.draw_offset) - (this.loc + Text_Offset));
                UICursor.move_to_target_loc();
            }
        }

        public Vector2 text_offset { set { Text_Offset = value; } }
        public Vector2 size_offset
        {
            set
            {
                Size_Offset = value;
                refresh_size();
            }
        }
        public bool glow { set { Glow = value; } }
        public int glow_width
        {
            set
            {
                Glowing_Line = new Unit_Line_Cursor(value);
                if (Window_Img != null)
                    Glowing_Line.color_override = Window_Img.color_override;
            }
        }
        public Vector2 bar_offset { set { Bar_Offset = value; } }
        public bool greyed_cursor { set { Greyed_Cursor = value; } }

        protected int text_area_width { get { return this.column_spacing * Columns; } }
        protected int column_width
        {
            get { return (int)(Width - (16 + Text_Offset.X * 2)); }
        }
        private int column_spacing
        {
            get { return (int)(this.column_width + Text_Offset.X * 2); }
        }

        public bool small_window
        {
            get { return Window_Img.small; }
            set { Window_Img.small = value; }
        }

        public bool still_cursor { set { UICursor.still_cursor = value; } }

        public Color tint { set { Window_Img.tint = value; } }

        public Texture2D texture
        {
            set
            {
                if (Window_Img != null)
                    Window_Img.texture = value;
            }
        }

        public bool WindowVisible
        {
            set
            {
                if (Window_Img != null)
                    Window_Img.visible = value;
            }
        }

        public int color_override
        {
            set
            {
                if (Window_Img != null)
                    Window_Img.color_override = value;
                if (Glowing_Line != null)
                    Glowing_Line.color_override = value;
            }
        }

        public bool is_cursor_moving { get { return UICursor.is_moving; } }

        public bool manual_cursor_draw { set { Manual_Cursor_Draw = value; } }
        public bool manual_help_draw { set { Manual_Help_Draw = value; } }

        public float help_stereoscopic { set { Help_Stereo_Offset = value; } }
        #endregion

        protected Window_Command() { }
        public Window_Command(Vector2 loc, int width, List<string> strs)
        {
            initialize(loc, width, strs);
        }

        protected virtual void initialize(Vector2 loc, int width, List<string> strs)
        {
            initialize_window();
            Grey_Cursor = new Hand_Cursor();
            Grey_Cursor.tint = new Color(192, 192, 192, 255);
            Grey_Cursor.draw_offset = new Vector2(-16, 0);
            //Hand_Texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Menu_Hand");
            this.loc = loc;

            Width = width;
            if (Window_Img != null)
                Window_Img.width = Width;

            if (Glowing_Line == null)
            {
                Glowing_Line = new Unit_Line_Cursor(column_width);
                Glowing_Line.color_override = Window_Img.color_override;
            }
            set_items(strs);

            update(false);
        }

        protected virtual void initialize_window()
        {
            Window_Img = new System_Color_Window();
        }

        protected virtual void set_items(List<string> strs)
        {
            add_commands(strs);

            if (Window_Img != null)
                Window_Img.set_lines(this.rows, (int)Size_Offset.Y);
        }

        protected virtual void add_commands(List<string> strs)
        {
            var nodes = new List<CommandUINode>();
            for (int i = 0; i < strs.Count; i++)
            {
                var text_node = item(strs[i], i);
                nodes.Add(text_node);
            }

            set_nodes(nodes);
        }

        protected virtual void set_nodes(List<CommandUINode> nodes)
        {
            Items = new UINodeSet<CommandUINode>(nodes);
            Items.CursorMoveSound = System_Sounds.Menu_Move1;
            Items.WrapVerticalMove = true;

            UICursor = new UICursor<CommandUINode>(Items);
            UICursor.draw_offset = new Vector2(-16, 0);
            UICursor.ratio = new int[] { 1, 1 };
        }

        protected virtual CommandUINode item(object value, int i)
        {
            return text_item((string)value, i);
        }

        protected CommandUINode text_item(string str, int i)
        {
            var text = new TextSprite();
            text.SetFont(Config.UI_FONT, Global.Content, "White");
            text.text = str;
            var text_node = new TextUINode("", text, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }

        protected void refresh_size()
        {
            Window_Img.width = column_spacing * Math.Min(num_items(), Columns) +
                16 + (int)Size_Offset.X;
            Window_Img.set_lines(
                this.rows, (int)Size_Offset.Y);
        }

        public void set_text_color(int index, string color)
        {
            if (index >= num_items())
                return;
            Items[index].set_text_color(color);
        }

        public virtual int num_items()
        {
            if (Items == null)
                return 0;
            return Items.Count;
        }

        protected virtual Vector2 text_draw_vector()
        {
            return Text_Offset + draw_vector();
        }

        public void set_columns(int value)
        {
            Columns = value;
            if (Window_Img != null)
            {
                refresh_size();
                refresh_layout();
            }
        }

        public ConsumedInput selected_index()
        {
            return SelectedIndex;
        }

        public bool is_selected()
        {
            return SelectedIndex.IsSomething;
        }

        public ConsumedInput help_index()
        {
            return HelpIndex;
        }

        public bool getting_help()
        {
            return HelpIndex.IsSomething;
        }

        public bool is_canceled()
        {
            return Canceled;
        }

        public void reset_selected()
        {
            SelectedIndex = new ConsumedInput();
            HelpIndex = new ConsumedInput();
            Canceled = false;
        }

        #region Update
        public void update()
        {
            update(true && this.active);
        }
        public void update(bool input)
        {
            update_input(input);

            update_ui(input);

            if (Glowing_Line != null)
                Glowing_Line.update();

            update_commands(input);
        }

        protected virtual void update_commands(bool input)
        {
        }

        protected virtual void update_ui(bool input)
        {
            reset_selected();

            update_movement(input);

            if (input)
            {
                ConsumedInput selected = Items.consume_triggered(
                    Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                ConsumedInput help = Items.consume_triggered(
                    Inputs.R, MouseButtons.Right, TouchGestures.LongPress);

                if (!is_help_active)
                {
                    SelectedIndex = selected;
                    HelpIndex = help;
                }

                if (Global.Input.triggered(Inputs.B))
                    Canceled = true;
                if (is_help_active)
                {
                    if (help.IsSomething)
                        Canceled = true;
                    if (Global.Input.mouse_click(MouseButtons.Right))
                        Canceled = true;
                }
            }
        }

        protected virtual void update_movement(bool input)
        {
            if (Items == null)
                return; //Debug

            int index = this.index;

            Items.Update(input && !Movement_Locked, -(loc + text_draw_vector()));
            UICursor.update();
            if (Grey_Cursor != null)
                Grey_Cursor.force_loc(UICursor.target_loc);

            if (index != this.index)
                on_index_changed(index);
        }

        internal event EventHandler IndexChanged;
        protected virtual void on_index_changed(int oldIndex)
        {
            if (IndexChanged != null)
                IndexChanged(this, new EventArgs());
        }

        protected virtual void refresh_layout()
        {
            for (int i = 0; i < num_items(); i++)
            {
                Items[i].loc = item_loc(i);
                Items[i].Size = new Vector2(this.column_width, 16);
            }
            Items.refresh_destinations();
            Items.WrapVerticalMove = Columns == 1;
        }

        protected Vector2 item_loc(int index)
        {
            const int LINE_HEIGHT = 16;

            return new Vector2(
                8 + (index % Columns) * column_spacing,
                8 + (index / Columns) * LINE_HEIGHT);
        }

        protected virtual Vector2 cursor_loc(int index = -1)
        {
            if (index == -1 && Items != null)
                index = Items.ActiveNodeIndex;
            int x = (int)this.loc.X + (index % Columns) * column_spacing;
            int y = (int)this.loc.Y + 8 + (index / Columns) * 16;
            return new Vector2(x - 8, y);
        }

        protected virtual void update_input(bool input) { }
        #endregion

        #region Help
        public bool is_help_active { get { return Help_Window != null; } }
        #endregion

        #region Draw
        protected virtual Vector2 help_draw_vector()
        {
            return draw_offset + graphic_draw_offset(Help_Stereo_Offset);
        }

        public virtual void draw(SpriteBatch sprite_batch)
        {
            if (visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                draw_window(sprite_batch);
                // Text
                draw_text(sprite_batch);
                // Cursor
                if (!Manual_Cursor_Draw)
                    draw_cursor(sprite_batch);
                sprite_batch.End();
            }
        }

        protected void draw_window(SpriteBatch sprite_batch)
        {
            if (Window_Img != null && Window_Img.visible)
            {
                // Window background
                Window_Img.draw(sprite_batch, -(loc + draw_vector()));
                // Text underline
                draw_bar(sprite_batch);
            }
        }

        protected virtual void draw_bar(SpriteBatch sprite_batch)
        {
            Vector2 loc = cursor_loc() + text_draw_vector() + Bar_Offset + new Vector2(8, 8);
            draw_bar(sprite_batch, loc);
        }
        protected virtual void draw_bar(SpriteBatch sprite_batch, Vector2 loc)
        {
            if (Window_Img == null)
                return;
            if (Items.ActiveNode == null)
                return;

            if (Glow)
            {
                Glowing_Line.draw(sprite_batch, -(loc + new Vector2(8, 0)));
            }
            else
            {
                int x = 8;
                sprite_batch.Draw(UICursor.texture, loc + new Vector2(x, 0),
                    new Rectangle(16, Window_Img.window_color * 8, 8, 8), Window_Img.tint);
                x += 8;
                while (x < (this.column_width - ((int)draw_offset.X + (int)Bar_Offset.X * 2)))
                {
                    sprite_batch.Draw(UICursor.texture, loc + new Vector2(x, 0),
                        new Rectangle(24, Window_Img.window_color * 8, 8, 8), Window_Img.tint);
                    x += 8;
                }
                sprite_batch.Draw(UICursor.texture, loc + new Vector2(x, 0),
                    new Rectangle(32, Window_Img.window_color * 8, 8, 8), Window_Img.tint);
            }
        }

        protected virtual void draw_text(SpriteBatch sprite_batch)
        {
            Items.Draw(sprite_batch, -(loc + text_draw_vector()));
        }

        public virtual void draw_cursor(SpriteBatch sprite_batch)
        {
            if (Input.ControlScheme != ControlSchemes.Mouse)
            {
                if (Greyed_Cursor)
                    Grey_Cursor.draw(sprite_batch, -(loc + text_draw_vector()));
            }

            if (active && Items.ActiveNode != null)
                UICursor.draw(sprite_batch, -(loc + text_draw_vector()));
        }
        #endregion
    }
}
