using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface.Command;
using FEXNA_Library;

namespace FEXNA.Windows.Command.Items
{
    class Window_Command_Item : Window_Command
    {
        protected int WIDTH = 144;

        protected int Unit_Id = -1;
        protected int Actor_Id = -1;
        int Equipped = 0;
        protected List<int> Index_Redirect = new List<int>();
        protected int[] Stat_Values = new int[4];

        protected StatsPanel ItemInfo;

        #region Accessors
        new public int immediate_index
        {
            set
            {
                base.Items.set_active_node( //Debug
                    base.Items[Math.Max(0, Math.Min(num_items() - 1, value))]); //Debug
                UICursor.update();
                UICursor.move_to_target_loc();

                if (Grey_Cursor != null)
                    Grey_Cursor.force_loc(UICursor.target_loc);
                refresh();
            }
        }

        public int equipped { get { return Equipped; } }

        public Game_Unit unit { get { return (Unit_Id == -1 ? null : Global.game_map.units[Unit_Id]); } }

        protected Item_Data current_item_data
        {
            get
            {
                return items(redirect());
            }
        }
        protected Data_Equipment current_item
        {
            get
            {
                return current_item_data.to_equipment;
            }
        }

        public int width { get { return WIDTH; } }

        public int item_count { get { return get_equipment().Count; } }

        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value; //Debug
            }
        }

        public virtual float data_stereoscopic
        {
            set
            {
                ItemInfo.stereoscopic = value;
            }
        }
        #endregion

        public Window_Command_Item() { }
        public Window_Command_Item(int unit_id, Vector2 loc)
        {
            Unit_Id = unit_id;
            initialize(loc, WIDTH, new List<string>());
        }

        protected virtual Game_Actor actor()
        {
            if (Actor_Id != -1)
                return Global.game_actors[Actor_Id];
            if (unit == null)
                return null;
            return unit.actor;
        }

        protected Item_Data items(int index)
        {
            if (index < 0 || index >= get_equipment().Count)
                return new Item_Data();
            return get_equipment()[index];
        }

        protected override void initialize(Vector2 loc, int width, List<string> strs)
        {
            item_initialize(loc, width, strs);
            base.initialize(loc, width, strs);
            refresh();
        }

        protected virtual void item_initialize(Vector2 loc, int width, List<string> strs)
        {
            ItemInfo = new StatsPanel(actor());
            ItemInfo.loc = loc + new Vector2(160, Config.WINDOW_HEIGHT - 88);
        }

        public void refresh_items()
        {
            set_items(new List<string>());
            refresh_layout();
            refresh();
        }
        
        protected void refresh_item_stats()
        {
            refresh_item_stats(Stat_Values);
        }
        protected void refresh_item_stats(int[] stat_values)
        {
            if (unit != null && current_item_data.is_weapon)
            {
                //var stats = new BattlerStats(unit.id, redirect()); //Debug
                var stats = new Calculations.Stats.BattlerStats(
                    unit.id, current_item_data.Id);

                stat_values[0] = stats.dmg();
                stat_values[1] = stats.crt();
                stat_values[2] = stats.hit();
                stat_values[3] = unit.atk_spd(1, current_item_data);
                //stat_values[3] = unit.avo(); //Debug
            }
        }

        protected override void set_items(List<string> strs)
        {
            Index_Redirect.Clear();

            add_commands(strs);
            Window_Img.set_lines(num_items(), (int)Size_Offset.Y);
            refresh_item_stats();

            //if (actor() != null) //Debug
            //    set_items(actor().items);

            refresh_equipped_tag();
        }

        protected virtual List<Item_Data> get_equipment()
        {
            if (unit == null)
            {
                if (actor() == null)
                    return null;
                return actor().items;
            }
            return unit.items;
        }

        protected override void add_commands(List<string> strs)
        {
            if (get_equipment() == null)
                return;
            int count = get_equipment().Count;

            var nodes = new List<CommandUINode>();
            for (int i = 0; i < count; i++)
            {
                var item_node = item("", i);
                if (item_node != null)
                {
                    item_node.loc = item_loc(nodes.Count);
                    nodes.Add(item_node);
                    Index_Redirect.Add(i);
                }
            }

            set_nodes(nodes);
        }

        protected override CommandUINode item(object value, int i)
        {
            return item(value as string, i);
        }
        protected virtual CommandUINode item(string str, int i)
        {
            var item_data = get_equipment()[i];
            if (!is_valid_item(get_equipment(), i))
                return null;

            var text = new Status_Item();
            text.set_image(actor(), item_data);
            var text_node = new ItemUINode("", text, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }

        protected virtual bool is_valid_item(List<Item_Data> items, int i)
        {
            var item_data = get_equipment()[i];
            return !item_data.non_equipment;
        }

        protected override void update_commands(bool input)
        {
            base.update_commands(input);
            update_graphics();
            if (is_help_active)
                Help_Window.update();
        }

        protected override void on_index_changed(int oldIndex)
        {
            base.on_index_changed(oldIndex);
            refresh();
        }

        public void update_graphics()
        {
            if (ItemInfo != null)
                ItemInfo.Update();
        }

        protected void refresh()
        {
            equip_actor();
            if (should_refresh_info())
            {
                int[] stat_values = new int[4];
                refresh_item_stats(stat_values);
                ItemInfo.refresh_info(unit == null ? actor() : unit.actor,
                    current_item, stat_values, Stat_Values);
            }
            if (is_help_active)
            {
                if (current_item_data.non_equipment)
                    close_help();
                else
                {
                    Help_Window.set_item(current_item_data, actor());
                    update_help_loc();
                }
            }
        }

        protected virtual bool should_refresh_info()
        {
            return ItemInfo != null;
        }

        protected virtual void equip_actor()
        {
            if (redirect() >= 0)
            {
                // Equip weapon the cursor is on to accurately get its stats
                if (current_item_data.is_weapon)
                {
                    if (unit == null)
                        actor().equip(redirect() + 1);
                    else
                        unit.equip(redirect() + 1);
                }
                // On items, equip the weapon the actor is actually using
                // For example, so that arms scroll knows what its boosting
                else
                {
                    restore_equipped();
                }
            }
        }

        public void restore_equipped()
        {
            if (unit == null)
                actor().equip(Equipped);
            else
                unit.equip(Equipped);
        }

        /* //Debug
        protected virtual void refresh_info()
        {
            // No item
            if (redirect() < 0)
            {
                Item_Description.text = "";
            }
            // Weapon
            else if (is_weapon_highlighted())
            {
                int[] stat_values = new int[4];
                refresh_item_stats(stat_values);
                Stats[0].text = stat_values[0].ToString();
                Stats[1].text = ((Data_Weapon)current_item).Crt < 0 ? "--" : stat_values[1].ToString();
                Stats[2].text = stat_values[2].ToString();
                Stats[3].text = stat_values[3].ToString();
                for (int i = 0; i < 4; i++)
                    Arrows[i].value = stat_values[i] == Stat_Values[i] ? WeaponTriangle.Nothing :
                        (stat_values[i] > Stat_Values[i] ? WeaponTriangle.Advantage : WeaponTriangle.Disadvantage);
                Type_Icon.index = (int)((Data_Weapon)current_item).Main_Type;
            }
            // Item
            else
            {
                Item_Description.text = "";
                string[] desc_ary = current_item.Quick_Desc.Split('|');
                for (int i = 0; i < desc_ary.Length; i++)
                    Item_Description.text += desc_ary[i] + "\n";
            }
        }*/

        protected virtual bool is_weapon_highlighted()
        {
            return redirect() >= 0 && current_item_data.is_weapon && !((Data_Weapon)current_item).is_staff();
        }

        #region Help
        public virtual void open_help()
        {
            if (current_item_data.non_equipment)
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
                return;
            }

            Help_Window = new Window_Help();
            Help_Window.set_item(this.get_equipment()[Index_Redirect[HelpIndex]], actor());
            Help_Window.loc = loc + item_loc(this.index);
            update_help_loc();
            Global.game_system.play_se(System_Sounds.Help_Open);
        }

        public virtual void close_help()
        {
            Help_Window = null;
            Global.game_system.play_se(System_Sounds.Help_Close);
        }

        protected virtual void update_help_loc()
        {
            Help_Window.set_loc(loc + new Vector2(0, 8 + base.Items.ActiveNodeIndex * 16)); //Debug
        }
        #endregion

        public int redirect()
        {
            if (index < 0)
                return index;
            return Index_Redirect[index];
        }

        public int GetRedirect(int index)
        {
            return Index_Redirect.IndexOf(index);
        }

        public void refresh_equipped_tag()
        {
            Equipped = actor().equipped;
            if (show_equipped() && base.Items != null)
            {
                for (int i = 0; i < base.Items.Count; i++) //Debug
                {
                    (base.Items[i] as ItemUINode).equip(
                        Index_Redirect[i] == Equipped - 1); //Debug
                }
            }
        }

        #region Draw
        protected virtual bool show_equipped()
        {
            return true;// Equipped > 0;
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            if (visible)
            {
                // Info
                draw_info(sprite_batch);

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                draw_window(sprite_batch);
                // Text
                draw_text(sprite_batch);
                // Cursor
                draw_cursor(sprite_batch);
                sprite_batch.End();

                draw_help(sprite_batch, false);
            }
        }

        public virtual void draw_help(SpriteBatch sprite_batch)
        {
            draw_help(sprite_batch, true);
        }
        protected void draw_help(SpriteBatch sprite_batch, bool called)
        {
            if (called == Manual_Help_Draw)
                if (is_help_active)
                    Help_Window.draw(sprite_batch, -help_draw_vector());
        }

        protected virtual void draw_info(SpriteBatch sprite_batch)
        {
            if (ItemInfo != null)
                ItemInfo.Draw(sprite_batch);
        }
        #endregion
    }
}
