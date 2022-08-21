using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Windows;
using Tactile.Windows.Command;
using TactileLibrary;

namespace Tactile
{
    class Window_Supply_Items : Stereoscopic_Graphic_Object, ISelectionMenu
    {
        const int ROWS = 8;
        const int WIDTH = 144;
        const int TYPE_CHANGE_TIME = 8;

        protected int Actor_Id;
        protected List<int> Index = new List<int>();
        protected List<int> Scroll = new List<int>();
        protected List<SupplyItem> SupplyList = new List<SupplyItem>();
        protected int Page_Change_Time = 0;
        protected bool Page_Change_Right;
        protected bool Active = false;
        protected float PageOffset;
        protected Window_Command_Supply CommandWindow;
        protected List<int> SupplyWeaponTypes;
        protected bool TypeIconInactive;
        protected List<Weapon_Type_Icon> Type_Icons = new List<Weapon_Type_Icon>();
        protected Window_Help Help_Window;
        protected Page_Arrow Left_Arrow, Right_Arrow;
        protected bool Wants_Help_Open = false;
        protected bool Manual_Cursor_Draw = false;
        protected Maybe<float> Arrow_Stereo_Offset = new Maybe<float>(), Help_Stereo_Offset = new Maybe<float>();

        #region Accessors
        protected int type
        {
            get { return Global.game_system.Supply_Item_Type; }
            set
            {
                Global.game_system.Supply_Item_Type = value;
                show_type_icon();
            }
        }

        public int supply_type_count { get { return SupplyWeaponTypes.Count; } }

        protected Game_Actor actor { get { return Global.game_actors[Actor_Id]; } }

        protected int index
        {
            get { return Index[this.type]; }
            set { Index[this.type] = value; }
        }
        internal int scroll
        {
            get { return Scroll[this.type]; }
            private set { Scroll[this.type] = value; }
        }

        public Vector2 size { get { return CommandWindow.size; } }

        public SupplyItem active_item { get { return SupplyList[index]; } }

        public bool active
        {
            set
            {
                Active = value;
                if (!Active)
                    Wants_Help_Open = false;
                if (Active)
                    show_type_icon();
            }
        }

        public bool can_take { get { return SupplyList.Count > 0; } }

        public bool ready { get { return Page_Change_Time == 0; } }

        public Vector2 current_cursor_loc
        {
            get { return CommandWindow.current_cursor_loc; }
            set { CommandWindow.current_cursor_loc = value; }
        }

        public bool manual_cursor_draw { set { Manual_Cursor_Draw = value; } }

        public float arrow_stereoscopic { set { Arrow_Stereo_Offset = value; } }
        public float help_stereoscopic { set { Help_Stereo_Offset = value; } }
        #endregion

        public Window_Supply_Items(int actor_id, Vector2 loc)
        {
            Actor_Id = actor_id;
            get_supply_types();
            for (int i = 0; i < this.supply_type_count; i++)
            {
                Index.Add(0);
                Scroll.Add(0);
            }
            this.loc = loc;
            initialize_sprites();
        }

        protected void get_supply_types()
        {
            SupplyWeaponTypes = new List<int>();
            for (int i = 1; i < Global.weapon_types.Count; i++)
            {
                if (Global.weapon_types[i].DisplayedInStatus)
                    SupplyWeaponTypes.Add(i);
            }
            // Add none type for items and catch-all
            SupplyWeaponTypes.Add(0);
        }

        protected void initialize_sprites()
        {
            CommandWindow = new Window_Command_Supply(
                this.loc + new Vector2(0, 8), WIDTH, ROWS);
            CommandWindow.glow = true;
            CommandWindow.glow_width = WIDTH - 24;
            CommandWindow.manual_cursor_draw = true;
            // Weapon Type Icons
            for (int i = 0; i < this.supply_type_count; i++)
            {
                Type_Icons.Add(new Weapon_Type_Icon());
                Type_Icons[i].loc = new Vector2(i * 12 + 4, 0);
                Type_Icons[i].index = Global.weapon_types[SupplyWeaponTypes[i]].IconIndex;
            }
            // Arrows
            Left_Arrow = new Page_Arrow();
            Left_Arrow.loc = new Vector2(0, 0);
            Left_Arrow.ArrowClicked += Left_Arrow_ArrowClicked;
            Right_Arrow = new Page_Arrow();
            Right_Arrow.loc = new Vector2(144, 0);
            Right_Arrow.mirrored = true;
            Right_Arrow.ArrowClicked += Right_Arrow_ArrowClicked;

            refresh(true);
        }

        public void refresh(bool correctToScrollRange = false)
        {
            if (Global.battalion.has_convoy)
                Global.game_battalions.sort_convoy(Global.battalion.convoy_id);
            switch (Constants.Gameplay.CONVOY_ITEMS_STACK)
            {
                case Convoy_Stack_Types.Full:
                    refresh_stacked_items();
                    break;
                case Convoy_Stack_Types.Use:
                    refresh_stacked_items(true);
                    break;
                case Convoy_Stack_Types.None:
                    refresh_items();
                    break;
            }

            CommandWindow.immediate_index =  this.index;
            this.index = CommandWindow.index;
            CommandWindow.scroll = this.scroll;
            CommandWindow.refresh_scroll(correctToScrollRange);

            // Weapon Type Icons
            refresh_type_icons();
            if (is_help_active)
            {
                if (SupplyList.Count == 0)
                {
                    close_help();
                    Wants_Help_Open = true;
                }
            }
            refresh_loc();
        }

        private void refresh_type_icons()
        {
            for (int i = 0; i < Type_Icons.Count; i++)
            {
                int alpha = (!TypeIconInactive && this.type == i) ? 255 : 160;
                Type_Icons[i].tint = new Color(alpha, alpha, alpha, 255);
            }
        }
        protected void refresh_items()
        {
            SupplyList.Clear();
            get_items(SupplyList);

            CommandWindow.refresh_items(new List<SupplyItem>(SupplyList), this.actor);
        }
        protected void refresh_stacked_items(bool sameUses = false)
        {
            List<SupplyItem> supplies = new List<SupplyItem>();
            SupplyList.Clear();
            get_items(supplies);

            List<int> item_counts = new List<int>();
            // Goes through all item data and finds any the match
            for (int i = 0; i < supplies.Count; i++)
            {
                Item_Data item_data = supplies[i].get_item();
                SupplyList.Add(supplies[i]);
                item_counts.Add(1);
                // Checks the following items after the current one, and adds one to the count for each with the same id
                while ((i + 1) < supplies.Count &&
                        item_data.Id == supplies[i + 1].get_item().Id &&
                        // Block stacking actor items for now
                        supplies[i].Convoy &&
                        // Same actor
                        supplies[i].ActorId == supplies[i + 1].ActorId &&
                        // If caring about uses, the uses must also match
                        (!sameUses || (item_data.Uses == supplies[i + 1].get_item().Uses)))
                    {
                        item_counts[item_counts.Count - 1]++;
                        i++;
                    }
            }

            CommandWindow.refresh_stacked_items(new List<SupplyItem>(SupplyList),
                this.actor, item_counts, sameUses);
        }

        protected virtual void get_items(List<SupplyItem> items)
        {
            for (int i = 0; i < Global.game_battalions.active_convoy_data.Count; i++)
            {
                if (is_item_add_valid(
                        this.type, Global.game_battalions.active_convoy_data[i]))
                    items.Add(new SupplyItem(0, i));
            }
        }

        /// <summary>
        /// Returns true if the item matches the supply type given
        /// </summary>
        /// <param name="item_data">Index of supply type to check against</param>
        /// <param name="item_data">Item data to test</param>
        protected bool is_item_add_valid(int typeIndex, Item_Data item_data)
        {
            /* //Debug
            var supply_type = Global.weapon_types[SupplyWeaponTypes[typeIndex]];

            bool is_weapon = item_data.is_weapon;
            WeaponType weapon_type =
                !is_weapon ? Global.weapon_types[0] : item_data.to_weapon.main_type();*/

            int item_supply_type = supply_type_of_item(typeIndex, item_data);
            return typeIndex == item_supply_type;

            /* //Debug
            if (supply_type == Global.weapon_types[0])
                return !is_weapon || !weapon_type.DisplayedInStatus;
            else
                return supply_type == weapon_type;*/
        }

        private int supply_type_of_item(int typeIndex, Item_Data itemData)
        {
            int supply_type = 0;

            if (!itemData.is_weapon)
            {
                supply_type = 0;
            }
            else
            {
                WeaponType weapon_type = itemData.to_weapon.main_type();
                if (weapon_type.DisplayedInStatus)
                    supply_type = weapon_type.Key;
                else
                {
                    supply_type = 0;
                    if (Global.ActorConfig.ChildWeaponTypeAllowsParent && supply_type == 0)
                    {
                        if (typeIndex >= 0 && Global.weapon_types[SupplyWeaponTypes[typeIndex]]
                                .type_and_parents(Global.weapon_types)
                                .Skip(1)
                                .Contains(weapon_type))
                            supply_type = SupplyWeaponTypes[typeIndex];
                        else
                            foreach (var child_type in Global.weapon_types)
                                if (SupplyWeaponTypes.Contains(child_type.Key))
                                    if (child_type.type_and_parents(Global.weapon_types)
                                            .Skip(1)
                                            .Contains(weapon_type))
                                    {
                                        supply_type = child_type.Key;
                                        break;
                                    }
                    }
                    if (Global.ActorConfig.ParentWeaponTypeAllowsChild && supply_type == 0)
                    {
                        foreach (var parent_type in weapon_type
                                .type_and_parents(Global.weapon_types)
                                .Skip(1))
                            if (SupplyWeaponTypes.Contains(parent_type.Key))
                            {
                                supply_type = parent_type.Key;
                                break;
                            }
                    }
                }
            }

            return SupplyWeaponTypes.IndexOf(supply_type);
        }

        public void change_page(int index)
        {
            if (index != this.type)
            {
                this.type = index;
                refresh(true);
            }
        }

        public void jump_to(Item_Data item_data)
        {
            // Jump to the correct page
            int new_type = supply_type_of_item(-1, item_data);
            change_page(new_type);

            // Jump to the actual item
            for(int i = 0; i < SupplyList.Count; i++)
            {
                if (item_data.same_item(SupplyList[i].get_item()))
                    if (Constants.Gameplay.CONVOY_ITEMS_STACK == Convoy_Stack_Types.Full ||
                        item_data.Uses == SupplyList[i].get_item().Uses)
                    {
                        this.index = i;
                        CommandWindow.immediate_index = this.index;
                        CommandWindow.refresh_scroll();

                        refresh_loc();
                        break;
                    }
            }
        }

        public List<SupplyItem> active_stacked_items()
        {
            var item = this.active_item;
            if (item.Convoy)
                return new List<SupplyItem> { item };
            var item_data = item.get_item();

            List<SupplyItem> items = new List<SupplyItem>();
            int i = 0;
            while ((index + i) < SupplyList.Count &&
                SupplyList[index + i].ActorId == item.ActorId &&
                SupplyList[index + i].get_item().Type == item_data.Type &&
                SupplyList[index + i].get_item().Id == item_data.Id)
            {
                items.Add(SupplyList[index + i]);
                i++;
            }
            return items;
        }

        public void hide_type_icon()
        {
            TypeIconInactive = true;
            refresh_type_icons();
        }
        public void show_type_icon()
        {
            TypeIconInactive = false;
            refresh_type_icons();
        }

        public bool can_take_active_item(Game_Actor actor)
        {
            if (!this.can_take)
                return false;

            var item_data = this.active_item.get_item();
            return actor.can_take(item_data);
        }

        public ConsumedInput selected_index()
        {
            return CommandWindow.selected_index();
        }

        public bool getting_help()
        {
            return CommandWindow.getting_help();
        }

        public bool is_selected()
        {
            return CommandWindow.is_selected();
        }

        public bool is_canceled()
        {
            return CommandWindow.is_canceled() ||
                (is_help_active && CommandWindow.getting_help());
        }

        public void reset_selected() { }

        private void Left_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            Global.Audio.play_se("System Sounds", "Menu_Move2");
            move_left();
        }
        private void Right_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            Global.Audio.play_se("System Sounds", "Menu_Move2");
            move_right();
        }

        #region Update
        public virtual void update(bool active)
        {
            CommandWindow.update(active && Active && ready);
            bool moved = this.index != CommandWindow.index;
            this.index = CommandWindow.index;
            this.scroll = CommandWindow.scroll;

            if (moved)
                refresh_loc();

            if (active && Active && ready)
                update_input();
            if (!ready)
                update_page_change();
            if (is_help_active)
                Help_Window.update();
            Left_Arrow.update();
            Right_Arrow.update();
        }

        protected void update_input()
        {
            Left_Arrow.UpdateInput(-(this.loc + arrow_draw_vector()));
            Right_Arrow.UpdateInput(-(this.loc + arrow_draw_vector()));

            // Change page
            if (Global.Input.repeated(Inputs.Left) ||
                Global.Input.gesture_triggered(TouchGestures.SwipeRight))
            {
                Global.Audio.play_se("System Sounds", "Menu_Move2");
                move_left();
            }
            else if (Global.Input.repeated(Inputs.Right) ||
                Global.Input.gesture_triggered(TouchGestures.SwipeLeft))
            {
                Global.Audio.play_se("System Sounds", "Menu_Move2");
                move_right();
            }
        }

        protected void update_page_change()
        {
            if (Page_Change_Time != TYPE_CHANGE_TIME)
            {
                if (Page_Change_Time == TYPE_CHANGE_TIME / 2)
                {
                    PageOffset = -PageOffset;
                    int num = this.supply_type_count;
                    this.type = (this.type + (Page_Change_Right ? 1 : -1) + num) % num;
                    refresh(true);
                    if (!is_help_active && Wants_Help_Open && SupplyList.Count > 0)
                    {
                        open_help();
                        Wants_Help_Open = false;
                    }
                }
                else
                    PageOffset += (Page_Change_Right ? 1 : -1) * (WIDTH / (int)Math.Pow(2,
                        Page_Change_Time > TYPE_CHANGE_TIME / 2 ?
                        Page_Change_Time - TYPE_CHANGE_TIME / 2 :
                        TYPE_CHANGE_TIME / 2 - Page_Change_Time));
            }
            Page_Change_Time--;
            if (Page_Change_Time == 0)
                PageOffset = 0;
            CommandWindow.PageOffset = -PageOffset;
        }
        #endregion

        #region Movement
        protected void move_left()
        {
            Page_Change_Time = TYPE_CHANGE_TIME;
            Page_Change_Right = false;
        }
        protected void move_right()
        {
            Page_Change_Time = TYPE_CHANGE_TIME;
            Page_Change_Right = true;
        }

        protected virtual void refresh_loc()
        {
            if (is_help_active)
            {
                Help_Window.set_item(active_item.get_item(), actor);
                update_help_loc();
            }
        }
        #endregion

        #region Help
        public bool is_help_active { get { return Help_Window != null; } }

        public void open_help()
        {
            if (SupplyList.Count == 0)
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
            }
            else
            {
                Help_Window = new Window_Help();
                Help_Window.set_screen_bottom_adjustment(-16);
                Help_Window.set_item(active_item.get_item(), actor);
                Help_Window.loc = loc + new Vector2(0, 8 + (index - scroll) * 16);
                if (Help_Stereo_Offset.IsSomething)
                Help_Window.stereoscopic = Help_Stereo_Offset; //Debug
                update_help_loc();
                Global.game_system.play_se(System_Sounds.Help_Open);
            }
        }

        public virtual void close_help()
        {
            Help_Window = null;
            Global.game_system.play_se(System_Sounds.Help_Close);
        }

        protected virtual void update_help_loc()
        {
            Help_Window.set_loc(loc + new Vector2(0, 16 + (index - scroll) * 16));
        }
        #endregion

        protected Vector2 arrow_draw_vector()
        {
            return draw_offset + graphic_draw_offset(Arrow_Stereo_Offset);
        }

        #region Draw
        public void draw(SpriteBatch sprite_batch)
        {
            Vector2 loc = this.loc + draw_vector();
            // Window
            draw_window(sprite_batch, loc);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            for (int i = Type_Icons.Count - 1; i >= 0; i--)
                if (i != this.type || TypeIconInactive)
                    Type_Icons[i].draw(sprite_batch, -loc);
            if (!TypeIconInactive)
                Type_Icons[this.type].draw(sprite_batch, -loc);

            Left_Arrow.draw(sprite_batch, -(this.loc + arrow_draw_vector()));
            Right_Arrow.draw(sprite_batch, -(this.loc + arrow_draw_vector()));
            if (!Manual_Cursor_Draw)
                draw_cursor(sprite_batch);
            sprite_batch.End();
        }

        protected virtual void draw_window(SpriteBatch sprite_batch, Vector2 drawOffset)
        {
            CommandWindow.draw(sprite_batch);
        }

        public void draw_help(SpriteBatch spriteBatch)
        {
            if (is_help_active)
                Help_Window.draw(spriteBatch);
        }

        public void draw_cursor(SpriteBatch sprite_batch)
        {
            if (Active)
                CommandWindow.draw_cursor(sprite_batch);
        }
        #endregion
    }

    struct SupplyItem
    {
        public int ActorId { get; private set; }
        public int ItemIndex { get; private set; }

        public bool Convoy { get { return ActorId == 0; } }

        public SupplyItem(int actorId, int itemIndex) : this()
        {
            ActorId = actorId;
            ItemIndex = itemIndex;
        }

        public Item_Data get_item()
        {
            // If convoy
            if (this.Convoy)
                return Global.game_battalions.active_convoy_data[ItemIndex];
            else
                return Global.game_actors[ActorId].items[ItemIndex];
        }

        /// <summary>
        /// Removes the referenced item from its target inventory and returns it.
        /// </summary>
        public Item_Data acquire_item()
        {
            // If convoy
            if (this.Convoy)
                return Global.game_battalions.remove_item_from_convoy(
                    Global.battalion.convoy_id, ItemIndex);
            else
            {
                var item_data = Global.game_actors[ActorId].items[ItemIndex];
                Global.game_actors[ActorId].discard_item(ItemIndex);
                return item_data;
            }
        }
    }
}
