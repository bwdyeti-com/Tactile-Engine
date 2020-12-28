using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Command;
using TactileLibrary;

namespace Tactile.Windows.Command
{
    class Window_Trade : Stereoscopic_Graphic_Object, ISelectionMenu
    {
        const int SPACING = 144;

        public bool active = true, visible = true;
        private int Mode = 0;
        private int[] Selected = new int[] { -1, -1 };
        private int Actor_Id1, Actor_Id2;
        private int Num = Global.ActorConfig.NumItems, Items1, Items2;
        private List<Inputs> locked_inputs = new List<Inputs>();
        private bool Glow = false;
        private System_Color_Window Window1, Window2;
        private List<CommandUINode> Item_Imgs1 = new List<CommandUINode>(), Item_Imgs2 = new List<CommandUINode>();
        private TextSprite Equipped_Tag1, Equipped_Tag2;
        private Face_Sprite Face1, Face2;
        private Window_Help Help_Window;
        private Unit_Line_Cursor Glowing_Line;
        private Maybe<float> Face_Stereo_Offset = new Maybe<float>(), Help_Stereo_Offset = new Maybe<float>();

        private UINodeSet<CommandUINode> ItemNodes;
        private UICursor<CommandUINode> UICursor;
        private Hand_Cursor Grey_Cursor;

        protected int SelectedIndex = -1, HelpIndex = -1;
        private bool Canceled; 

        #region Accessors
        private int index
        {
            get
            {
                if (ItemNodes.ActiveNodeIndex == -1)
                    return -1;
                return Item_Imgs1.Contains(ItemNodes.ActiveNode) ?
                    Item_Imgs1.IndexOf(ItemNodes.ActiveNode) :
                    Item_Imgs2.IndexOf(ItemNodes.ActiveNode);
            }
            set
            {
                if (this.column == 0)
                    index1 = value;
                else
                    index2 = value;
            }
        }

        private int index1 { set { ItemNodes.set_active_node(Item_Imgs1[value]); } }
        private int index2 { set { ItemNodes.set_active_node(Item_Imgs2[value]); } }

        public int column
        {
            get
            {
                return ItemNodes == null ||
                    Item_Imgs1.Contains(ItemNodes.ActiveNode) ? 0 : 1;
            }
        }

        public int mode { get { return Mode; } }

        public bool glow { set { Glow = value; } }

        private Game_Actor actor1 { get { return Global.game_actors[Actor_Id1]; } }

        private Game_Actor actor2 { get { return Global.game_actors[Actor_Id2]; } }

        public bool ready { get { return true; } }// Cursor_Loc == new Vector2(Loc.X - 8 + SPACING * Column, Loc.Y + 8 + 16 * (Column == 0 ? Index1 : Index2)); } }//Debug

        public float face_stereoscopic { set { Face_Stereo_Offset = value; } }
        public float help_stereoscopic { set { Help_Stereo_Offset = value; } }
        #endregion

        public Window_Trade(int actor_id1, int actor_id2, int initial_index)
        {
            initialize(actor_id1, actor_id2, initial_index);
        }

        #region Initialize
        private void initialize(int actor_id1, int actor_id2, int initial_index)
        {
            Actor_Id1 = actor_id1;
            Actor_Id2 = actor_id2;
            //Hand_Texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Menu_Hand");
            loc = new Vector2(
                (Config.WINDOW_WIDTH - (SPACING * 2)) / 2,
                Config.WINDOW_HEIGHT - Math.Max(96, Num * 16 + 16 + 12));
            initialize_images();
            refresh();

            if (initial_index != -1)
            {
#if DEBUG
                throw new NotImplementedException();
#endif
                this.index = initial_index;
                enter();
                UICursor.move_to_target_loc();
            }
        }

        private void initialize_images()
        {
            // Face
            Face1 = new Face_Sprite(actor1.face_name, true);
            if (actor1.generic_face)
                Face1.recolor_country(actor1.name_full);
            Face1.expression = Face1.status_frame;
            Face1.phase_in();
            Face1.loc = new Vector2(loc.X + SPACING / 2,
                loc.Y + 12 +
                (int)Math.Max(0, (Face1.src_rect.Height - Face1.eyes_offset.Y) - 40) / 2);
            Face1.mirrored = true;
            // Face
            Face2 = new Face_Sprite(actor2.face_name, true);
            if (actor2.generic_face)
                Face2.recolor_country(actor2.name_full);
            Face2.expression = Face2.status_frame;
            Face2.phase_in();
            Face2.loc = new Vector2((int)loc.X + SPACING + SPACING / 2,
                loc.Y + 12 +
                (int)Math.Max(0, (Face2.src_rect.Height - Face2.eyes_offset.Y) - 40) / 2);

            Grey_Cursor = new Hand_Cursor();
            Grey_Cursor.visible = false;
            Grey_Cursor.draw_offset = new Vector2(-12, 0);

            Window1 = new System_Color_Window();
            Window1.width = SPACING;
            Window1.height = Num * 16 + 16;
            Window1.loc = loc;
            Window2 = new System_Color_Window();
            Window2.width = SPACING;
            Window2.height = Num * 16 + 16;
            Window2.loc = loc + new Vector2(SPACING, 0);
            Equipped_Tag1 = new TextSprite();
            Equipped_Tag1.SetFont(Config.UI_FONT, Global.Content, "White");
            Equipped_Tag1.text = "$";
            Equipped_Tag2 = new TextSprite();
            Equipped_Tag2.SetFont(Config.UI_FONT, Global.Content, "White");
            Equipped_Tag2.text = "$";

            Glowing_Line = new Unit_Line_Cursor(SPACING - 16);

        }

        private void refresh(bool preserveIndex = false)
        {
            if (ItemNodes == null)
                preserveIndex = false;

            int index = 0;
            if (preserveIndex)
                index = this.index;

            Items1 = actor1.num_items;
            Items2 = actor2.num_items;

            int column = this.column;
            if (Items1 == 0)
                column = 1;

            Item_Imgs1.Clear();
            Item_Imgs2.Clear();

            List<CommandUINode> nodes = new List<CommandUINode>();

            bool add_empty_slot = Mode != 0 && !this.is_help_active;

            for (int i = 0; i < Num; i++)
            {
                if (i < Items1)
                {
                    item_node(0, i, nodes);
                    Item_Imgs1.Add(nodes.Last());
                }
                else if (add_empty_slot && Selected[0] == 1 && i == Items1)
                {
                    empty_slot_node(0, i, nodes);
                    Item_Imgs1.Add(nodes.Last());
                }

                if (i < Items2)
                {
                    item_node(1, i, nodes);
                    Item_Imgs2.Add(nodes.Last());
                }
                else if (add_empty_slot && Selected[0] == 0 && i == Items2)
                {
                    empty_slot_node(1, i, nodes);
                    Item_Imgs2.Add(nodes.Last());
                }
            }

            ItemNodes = new UINodeSet<CommandUINode>(nodes);
            ItemNodes.WrapVerticalSameColumn = true;
            ItemNodes.CursorMoveSound = System_Sounds.Menu_Move1;
            ItemNodes.HorizontalCursorMoveSound = System_Sounds.Menu_Move2;

            ItemNodes.AngleMultiplier = 2f;
            ItemNodes.TangentDirections = new List<CardinalDirections>
            { CardinalDirections.Left, CardinalDirections.Right };
            ItemNodes.refresh_destinations();

            if (column == 0)
                this.index1 = Math.Min(index, Items1 - 1);
            else
                this.index2 = Math.Min(index, Items2 - 1);

            var old_cursor = UICursor;
            UICursor = new UICursor<CommandUINode>(ItemNodes);
            UICursor.draw_offset = new Vector2(-12, 0);
            UICursor.ratio = new int[] { 1, 1 };
            if (preserveIndex)
            {
                UICursor.force_loc(old_cursor.loc);
                UICursor.offset = old_cursor.offset;
            }

            Equipped_Tag1.loc = loc + new Vector2(SPACING - 16, actor1.equipped * 16 - 8);
            Equipped_Tag2.loc = loc + new Vector2(SPACING * 2 - 16, actor2.equipped * 16 - 8);
        }

        private void item_node(int unit_index, int index, List<CommandUINode> nodes)
        {
            Status_Item item = new Status_Item();
            TactileLibrary.Item_Data item_data = unit_index == 0 ?
                actor1.whole_inventory[index] :
                actor2.whole_inventory[index];
            item.set_image(unit_index == 0 ? actor1 : actor2, item_data);

            var node = new ItemUINode("", item, SPACING - 16); //Debug
            node.loc = new Vector2(8, 8) + new Vector2(SPACING * unit_index, index * 16);
            nodes.Add(node);
        }

        private void empty_slot_node(int unit_index, int index, List<CommandUINode> nodes)
        {
            var text = new TextSprite(
                Config.UI_FONT, Global.Content, "White",
                Vector2.Zero);

            var node = new TextUINode("", text, SPACING - 16); //Debug
            node.loc = new Vector2(8, 8) + new Vector2(SPACING * unit_index, index * 16);
            nodes.Add(node);
        }
        #endregion

        private TactileLibrary.Item_Data item_data()
        {
            if (this.index == -1 || this.index >= (this.column == 0 ? actor1 : actor2).num_items)
                return new TactileLibrary.Item_Data();
            return (this.column == 0 ?
                actor1.whole_inventory[this.index] :
                actor2.whole_inventory[this.index]);
        }

        private bool can_trade(Game_Actor actor, Game_Actor target, int itemIndex)
        {
            var item = actor.whole_inventory[itemIndex];
            if (item.non_equipment)
                return true;

            return actor.can_give(item) && target.can_take(item);
        }

        public bool enter()
        {
            // Selecting the first item
            if (Mode == 0)
            {
                if (SelectItem(this.column, this.index, false))
                    Global.game_system.play_se(System_Sounds.Confirm);
                else
                    Global.game_system.play_se(System_Sounds.Buzzer);
            }
            // Player selected the same item twice
            else if (this.column == Selected[0] && Selected[1] == this.index)
            {
                cancel();
                Global.game_system.play_se(System_Sounds.Cancel);
            }
            else
            {
                Game_Actor actor1;
                if (Mode == 1)
                    actor1 = this.actor1;
                else
                    actor1 = this.actor2;

                Game_Actor actor2;
                if (this.column == 0)
                    actor2 = this.actor1;
                else
                    actor2 = this.actor2;


                int index1 = Selected[1];
                int index2 = this.index;

                if (actor1 != actor2)
                {
                    if (!can_trade(actor2, actor1, index2))
                    {
                        Global.game_system.play_se(System_Sounds.Buzzer);
                        return false;
                    }
                }

                actor1.trade(actor2, index1, index2);

                Global.game_system.play_se(System_Sounds.Confirm);
                cancel();
                return true;
            }
            return false;
        }

        public bool SelectItem(int index)
        {
            return SelectItem(0, index, true);
        }
        private bool SelectItem(int column, int index, bool immediateMove)
        {
            if (immediateMove)
            {
                if (column == 0)
                    this.index1 = index;
                else
                    this.index2 = index;
                UICursor.UpdateTargetLoc();
            }

            if (column == 0)
            {
                if (!can_trade(this.actor1, this.actor2, index))
                {
                    return false;
                }

                Mode = column + 1;
                Grey_Cursor.force_loc(UICursor.target_loc);
                Grey_Cursor.visible = true;
                Selected = new int[] { 0, index };
                refresh(true);

                if (!Input.IsControllingOnscreenMouse)
                {
                    this.index2 = Items2 < Num ? Items2 : Math.Min(Items2, Selected[1]);
                }
                Items2 = Math.Min(Num, Items2 + 1);
            }
            else
            {
                if (!can_trade(this.actor2, this.actor1, index))
                {
                    return false;
                }

                Mode = column + 1;
                Grey_Cursor.force_loc(UICursor.target_loc);
                Grey_Cursor.visible = true;
                Selected = new int[] { 1, index };
                refresh(true);

                if (!Input.IsControllingOnscreenMouse)
                {
                    this.index1 = Items1 < Num ? Items1 : Math.Min(Items1, Selected[1]);
                }
                Items1 = Math.Min(Num, Items1 + 1);
            }
            if (immediateMove)
            {
                UICursor.UpdateTargetLoc();
                UICursor.move_to_target_loc();
            }
            return true;
        }

        public void cancel()
        {
            Grey_Cursor.visible = false;
            int column = Selected[0];

            if (Mode != 0)
            {
                if ((Mode == 1 && actor1.num_items == 0) ||
                    (Mode == 2 && actor2.num_items == 0))
                {
                    column = 2 - Mode;
                    Selected[1] = 0;
                }
            }
            if (column == 0)
                this.index1 = 0;
            else
                this.index2 = 0;

            Mode = 0;
            int index = Selected[1];
            refresh(true);
            index = Math.Min(index, (this.column == 0 ? Items1 - 1 : Items2 - 1));
            this.index = index;
            UICursor.UpdateTargetLoc();
        }

        public void staff_fix()
        {
            actor1.staff_fix();
            actor2.staff_fix();
        }

        public Maybe<int> selected_index()
        {
            if (SelectedIndex < 0)
                return Maybe<int>.Nothing;
            return SelectedIndex;
        }

        public bool is_selected()
        {
            return SelectedIndex >= 0;
        }

        public Maybe<int> help_index()
        {
            if (HelpIndex < 0)
                return Maybe<int>.Nothing;
            return HelpIndex;
        }

        public bool getting_help()
        {
            return HelpIndex >= 0;
        }

        public bool is_canceled()
        {
            return Canceled;
        }

        public void reset_selected()
        {
            SelectedIndex = -1;
            HelpIndex = -1;
            Canceled = false;
        }

        #region Update
        public void update()
        {
            update(true);
        }
        public void update(bool input)
        {
            Face1.update();
            Face2.update();

            update_ui(input);

            if (Glowing_Line != null)
                Glowing_Line.update();
            if (is_help_active)
                Help_Window.update();
        }

        private void update_ui(bool input)
        {
            reset_selected();

            int index = ItemNodes.ActiveNodeIndex;
            ItemNodes.Update(input, -(loc));
            bool moved = index != ItemNodes.ActiveNodeIndex;
            
            UICursor.update();
            if (moved && is_help_active)
            {
                Help_Window.set_item(item_data(), column == 0 ? actor1 : actor2);
                update_help_loc();
            }

            if (input)
            {
                if (!is_help_active)
                {
                    var selected = ItemNodes.consume_triggered(
                        Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                    if (selected.IsSomething)
                        SelectedIndex = selected;
                    var help = ItemNodes.consume_triggered(
                        Inputs.R, MouseButtons.Right, TouchGestures.LongPress);
                    if (help.IsSomething)
                        HelpIndex = help;
                }

                if (Global.Input.triggered(Inputs.B))
                    Canceled = true;
                if (is_help_active)
                {
                    var help = ItemNodes.consume_triggered(
                        Inputs.R, MouseButtons.Right, TouchGestures.LongPress);
                    if (help.IsSomething)
                        Canceled = true;
                }

                Rectangle target_rect = new Rectangle(
                    (int)loc.X + 8, (int)loc.Y + 8,
                    SPACING * 2 - 16, Global.ActorConfig.NumItems * 16);
                // Right clicked on nothing
                if (!Global.Input.mouse_in_rectangle(target_rect))
                    if (Global.Input.mouse_click(MouseButtons.Right))
                        Canceled = true;
            }
        }
        #endregion

        #region Help
        public bool is_help_active { get { return Help_Window != null; } }

        public void open_help()
        {
            if (index >= (this.column == 0 ? actor1 : actor2).num_items)
                return;
            Help_Window = new Window_Help();
            refresh(true);
            Help_Window.set_item(item_data(), column == 0 ? actor1 : actor2);
            Help_Window.loc = loc + ItemNodes.ActiveNode.loc + new Vector2(20, 0);
            if (Help_Stereo_Offset.IsSomething)
                Help_Window.stereoscopic = Help_Stereo_Offset; //Debug

            update_help_loc();
            Global.game_system.play_se(System_Sounds.Help_Open);
        }

        public void close_help()
        {
            Help_Window = null;
            refresh(true);
            Global.game_system.play_se(System_Sounds.Help_Close);
        }

        private void update_help_loc()
        {
            Help_Window.set_loc(loc + ItemNodes.ActiveNode.loc + new Vector2(20, 0));
            //Help_Window.set_loc(cursor_loc() + new Vector2(20, 0)); //Debug
        }
        #endregion

        private Vector2 face_draw_vector()
        {
            return draw_offset + graphic_draw_offset(Face_Stereo_Offset);
        }

        public void draw(SpriteBatch sprite_batch)
        {
            if (visible)
            {
                Face1.draw(sprite_batch, -face_draw_vector());
                Face2.draw(sprite_batch, -face_draw_vector());

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                // Window background
                Window1.draw(sprite_batch, -draw_vector());
                Window2.draw(sprite_batch, -draw_vector());
                // Text underline
                draw_bar(sprite_batch);

                draw_items(sprite_batch);
                // Cursor
                if (Grey_Cursor.visible)
                    Grey_Cursor.draw(sprite_batch, -(loc + draw_vector()));
                if (active)
                    UICursor.draw(sprite_batch, -(loc + draw_vector()));

                sprite_batch.End();
                if (is_help_active)
                    Help_Window.draw(sprite_batch);
            }
        }

        private void draw_bar(SpriteBatch sprite_batch)
        {
            /* //Debug
            Vector2 loc = (this.column == 0 ? Window1.loc : Window2.loc) +
                new Vector2(0, 16 + 16 * this.index);*/
            Vector2 loc = this.loc + UICursor.target_loc + new Vector2(-8, 8);
            if (Glow)
            {
                Glowing_Line.draw(sprite_batch, -(loc + draw_vector() + new Vector2(8, 0)));
            }
            else
            {
                int x = 8;
                sprite_batch.Draw(UICursor.texture, loc + draw_vector() + new Vector2(x, 0),
                    new Rectangle(16, Global.game_options.window_color * 8, 8, 8), Color.White);
                x += 8;
                while (x < (Window1.width - 16))
                {
                    sprite_batch.Draw(UICursor.texture, loc + draw_vector() + new Vector2(x, 0),
                        new Rectangle(24, Global.game_options.window_color * 8, 8, 8), Color.White);
                    x += 8;
                }
                sprite_batch.Draw(UICursor.texture, loc + draw_vector() + new Vector2(x, 0),
                    new Rectangle(32, Global.game_options.window_color * 8, 8, 8), Color.White);
            }
        }

        private void draw_items(SpriteBatch sprite_batch)
        {
            // Items
            /*
            foreach (Status_Item item in Item_Imgs1)
            {
                item.draw(sprite_batch, -draw_vector());
            }
            foreach (Status_Item item in Item_Imgs2)
            {
                item.draw(sprite_batch, -draw_vector());
            }*/

            ItemNodes.Draw(sprite_batch, - (loc + draw_vector()));

            if (actor1.equipped > 0)
                Equipped_Tag1.draw(sprite_batch, -draw_vector());
            if (actor2.equipped > 0)
                Equipped_Tag2.draw(sprite_batch, -draw_vector());
        }
    }
}
