using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using FEXNAVersionExtension;

namespace FEXNA.State
{
    class Game_Item_State : Game_Combat_State_Component
    {
        protected bool Item_Calling = false;
        protected bool In_Item_Use = false;
        protected int Item_User_Id = -1;
        protected int Item_Used = -1;
        protected int Item_Inventory_Target = -1;
        protected int Item_Phase = 0, Item_Action = 0, Item_Timer = 0;

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            base.write(writer);
            writer.Write(Item_Calling);
            writer.Write(In_Item_Use);
            writer.Write(Item_User_Id);
            writer.Write(Item_Used);
            writer.Write(Item_Inventory_Target);
            writer.Write(Item_Phase);
            writer.Write(Item_Action);
            writer.Write(Item_Timer);
        }

        internal override void read(BinaryReader reader)
        {
            base.read(reader);
            Item_Calling = reader.ReadBoolean();
            In_Item_Use = reader.ReadBoolean();
            Item_User_Id = reader.ReadInt32();
            Item_Used = reader.ReadInt32();
            if (!Global.LOADED_VERSION.older_than(0, 4, 6, 2))
                Item_Inventory_Target = reader.ReadInt32();
            Item_Phase = reader.ReadInt32();
            Item_Action = reader.ReadInt32();
            Item_Timer = reader.ReadInt32();
        }
        #endregion

        #region Accessors
        public bool item_calling
        {
            get { return Item_Calling; }
            set { Item_Calling = value; }
        }

        public bool in_item_use { get { return In_Item_Use; } }

        public Game_Unit item_user { get { return Item_User_Id == -1 ? null : Units[Item_User_Id]; } }

        public int item_used { get { return Item_Used; } }
        #endregion

        internal override void update()
        {
            if (Item_Calling)
            {
                In_Item_Use = true;
                Item_Calling = false;
            }
            if (In_Item_Use)
            {
                bool cont = false;
                while (!cont)
                {
                    cont = true;
                    switch (Item_Phase)
                    {
                        // Set up, suspend
                        case 0:
                            if (!Global.game_state.skip_ai_turn_activating && !Global.game_system.preparations)
                            {
                                if (Global.game_state.is_player_turn)
                                    Global.scene.suspend();
                            }
                            else
                                cont = false;
                            Item_User_Id = Global.game_system.Item_User;
                            Item_Used = Global.game_system.Item_Used;
                            Item_Inventory_Target = Global.game_system.Item_Inventory_Target;
                            Global.game_system.Item_User = -1;
                            Global.game_system.Item_Used = -1;
                            Global.game_system.Item_Inventory_Target = -1;
                            Item_Phase = 1;
                            Global.game_state.add_item_metric(item_user, Item_Used);
                            break;
                        case 1:
                            FEXNA_Library.Data_Item item = item_user.actor.items[Item_Used].to_item;
                            // Promotion
                            if (item.Promotes.Count > 0)
                            {
                                cont = update_item_promotion(item);
                            }
                            // Healing/Status Healing/Torch/Pure Water
                            else if (item.can_heal_hp() ||
                                item.Status_Remove.Intersect(item_user.actor.negative_states).Any() ||
                                item.Torch_Radius > 0 || item.is_stat_buffer())
                            {
                                cont = update_item_healing(item);
                            }
                            // Stat/Growth Booster
                            else if (item.is_stat_booster() || item.is_growth_booster())
                            {
                                cont = update_item_stat_boost(item);
                            }
                            // Repair Kit
                            else if (item.can_repair)
                            {
                                cont = update_item_repair(item);
                            }
                            // Status Healing catch //Debug
                            else if (item.Status_Remove.Count > 0)
                            {
                                cont = update_item_healing(item);
                            }
                            else
                            {
#if DEBUG
                                throw new ArgumentException("this item does nothing?");
#endif
                                cont = update_item_healing(item);
                            }
                                //Item_Phase = 2; //Debug
                            break;
                        case 2:
                            Item_Phase = 3;
                            Item_Timer = 0;
                            break;
                        default:
                            switch (Item_Timer)
                            {
                                case 0:
                                    if (item_user.has_canto() && !item_user.full_move())
                                    {
                                        item_user.cantoing = true;
                                        if (item_user.is_active_player_team && !item_user.berserk) //Multi
                                            item_user.open_move_range();
                                    }
                                    else
                                    {
                                        item_user.start_wait(false);
                                        Global.game_system.Menu_Canto = Canto_Records.None;
                                    }
                                    item_user.queue_move_range_update();
                                    refresh_move_ranges();
                                    Item_Timer++;
                                    break;
                                case 1:
                                    if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                                    {
                                        Item_Timer++;
                                    }
                                    break;
                                case 2:
                                    end_item_use();
                                    highlight_test();
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        protected bool update_item_promotion(FEXNA_Library.Data_Item item)
        {
            if (get_scene_map() == null)
                return true;
            switch (Item_Action)
            {
                case 0:
                    switch (Item_Timer)
                    {
                        case 0:
                            item_user.sprite_moving = false;
                            item_user.frame = 0;
                            item_user.facing = 2;
                            Item_Timer++;
                            return false;
                        case 1:
                            if (!Scrolling)
                            {
                                Item_Timer++;
                                item_user.item_effect(item, Item_Inventory_Target);
                                Global.game_system.Class_Changer = Item_User_Id;
                                Global.game_system.Class_Change_To = (int)item_user.actor.promotes_to();
                                Transition_To_Battle = true;
                                if (Global.game_system.preparations)
                                {
                                    Battle_Transition_Timer = 0;
                                    Item_Timer++;
                                    return false;
                                }
                                else
                                    Battle_Transition_Timer = Constants.BattleScene.BATTLE_TRANSITION_TIME;
                            }
                            break;
                        case 2:
                            Battle_Transition_Timer--;
                            if (Battle_Transition_Timer == 0)
                            {
                                Item_Timer++;
                            }
                            break;
                        case 3:
                            if (Global.game_system.preparations)
                                Global.game_system.Battle_Mode = Constants.Animation_Modes.Full;
                            else
                                Global.game_system.Battle_Mode = Global.game_options.animation_mode ==
                                    (byte)Constants.Animation_Modes.Map ?
                                        Constants.Animation_Modes.Map : Constants.Animation_Modes.Full; //Yeti
                            Global.scene_change("Scene_Promotion");
                            Item_Action = 1;
                            Item_Timer = 0;
                            break;
                    }
                    break;
                case 1:
                    Item_Action = 2;
                    break;
                case 2:
                    switch (Item_Timer)
                    {
                        case 1:
                            Transition_To_Battle = false;
                            if (Global.game_system.preparations)
                            {
                                Global.game_system.Class_Changer = -1;
                                // Consume item
                                item_user.actor.use_item(Item_Used, true);
                                item_user.recover_all();

                                end_item_use();
                                get_scene_map().resume_preparations_item_menu();
                            }
                            else
                            {
                                Battle_Transition_Timer = Constants.BattleScene.BATTLE_TRANSITION_TIME;
                                Item_Timer++;
                            }
                            break;
                        case 2:
                            if (Battle_Transition_Timer > 0) Battle_Transition_Timer--;
                            if (Battle_Transition_Timer == 0)
                            {
                                Global.game_system.Class_Changer = -1;
                                Item_Phase = 2;
                                // Consume item
                                item_user.actor.use_item(Item_Used, true);
                            }
                            break;
                        default:
                            Item_Timer++;
                            break;
                    }
                    break;
            }
            return true;
        }

        protected bool update_item_healing(FEXNA_Library.Data_Item item)
        {
            if (get_scene_map() == null)
                return true;
            bool unit_visible = !Global.game_state.skip_ai_turn_activating &&
                (!Global.game_map.fow || item.Torch_Radius > 0 ||
                (item_user.visible_by() ||
                    Global.game_map.fow_visibility[Constants.Team.PLAYER_TEAM].Contains(item_user.loc)));
            switch (Item_Action)
            {
                // Makes HP window, stops map sprite
                case 0:
                    switch (Item_Timer)
                    {
                        case 0:
                            if (!Scrolling)
                            {
                                if (!unit_visible)
                                {
                                    Item_Action++;
                                    break;
                                }
                                item_user.sprite_moving = false;
                                item_user.frame = 0;
                                item_user.facing = 2;
                                if (item.can_heal_hp() && !item_user.actor.is_full_hp())
                                {
                                    get_scene_map().create_hud(Item_User_Id);
                                }
                                Item_Timer++;
                            }
                            else
                                return true;
                            break;
                        case 1:
                            item_user.facing = 6;
                            Item_Timer++;
                            break;
                        case 23:
                            Item_Action = 1;
                            Item_Timer = 0;
                            break;
                        default:
                            Item_Timer++;
                            break;
                    }
                    break;
                // Map sprite animates
                case 1:
                    switch (Item_Timer)
                    {
                        case 0:
                            if (!unit_visible)
                            {
                                Item_Action++;
                                break;
                            }
                            item_user.frame = 1;
                            Item_Timer++;
                            break;
                        case 4:
                            item_user.frame = 2;
                            Item_Timer++;
                            break;
                        case 10:
                            Item_Action = 2;
                            Item_Timer = 0;
                            break;
                        default:
                            Item_Timer++;
                            break;
                    }
                    break;
                // Starts item animation
                case 2:
                    switch (Item_Timer)
                    {
                        case 0:
                            if (!unit_visible)
                            {
                                Item_Action++;
                                break;
                            }
                            get_scene_map().set_map_effect(item_user.loc, 0, Map_Animations.item_effect_id(item.Id));
                            Item_Timer++;
                            break;
                        case 30:
                            Item_Action = 3;
                            Item_Timer = 0;
                            break;
                        default:
                            Item_Timer++;
                            break;
                    }
                    break;
                // HP gain
                case 3:
                    switch (Item_Timer)
                    {
                        case 0:
                            item_user.item_effect(item, Item_Inventory_Target);
                            if (!unit_visible)
                            {
                                Item_Action++;
                                break;
                            }
                            Item_Timer++;
                            break;
                        case 1:
                            if (!get_scene_map().is_map_effect_active())
                            {
                                if (get_scene_map().combat_hud_ready())
                                    Item_Timer++;
                            }
                            break;
                        case 27:
                            item_user.frame = 1;
                            Item_Timer++;
                            break;
                        case 35:
                            item_user.frame = 0;
                            Item_Timer++;
                            break;
                        case 47:
                            Item_Action = 4;
                            Item_Timer = 0;
                            break;
                        default:
                            Item_Timer++;
                            break;
                    }
                    break;
                // Finish
                case 4:
                    Item_Phase = 2;
                    // Consume item
                    item_user.actor.use_item(Item_Used);
                    if (!unit_visible)
                    {
                        break;
                    }
                    get_scene_map().clear_combat();
                    break;
            }
            return unit_visible;
            return true;
        }

        protected bool update_item_stat_boost(FEXNA_Library.Data_Item item)
        {
            if (get_scene_map() == null)
                return true;
            switch (Item_Action)
            {
                // Makes stat up window
                case 0:
                    switch (Item_Timer)
                    {
                        case 3:
                            item_user.item_effect(item, Item_Inventory_Target);
                            Global.game_system.play_se(System_Sounds.Gain);
                            get_scene_map().set_stat_gain_popup(item_user.actor.items[Item_Used], item_user, 242);
                            Item_Action++;
                            Item_Timer = 0;
                            break;
                        default:
                            Item_Timer++;
                            break;
                    }
                    break;
                // Wait for popup
                case 1:
                    if (!get_scene_map().is_map_popup_active())
                        Item_Action++;
                    break;
                // Finish
                case 2:
                    Item_Phase = 2;
                    // Consume item
                    item_user.use_item(Item_Used);
                    break;
            }
            return true;
        }

        protected bool update_item_repair(FEXNA_Library.Data_Item item)
        {
            if (get_scene_map() == null)
                return true;
            switch (Item_Action)
            {
                // Makes stat up window
                case 0:
                    switch (Item_Timer)
                    {
                        case 3:
                            item_user.item_effect(item, Item_Inventory_Target);
                            Global.game_system.play_se(System_Sounds.Gain);
                            get_scene_map().set_repair_popup(item_user.actor.items[Item_Inventory_Target], item_user, 242);
                            Item_Action++;
                            Item_Timer = 0;
                            break;
                        default:
                            Item_Timer++;
                            break;
                    }
                    break;
                // Wait for popup
                case 1:
                    if (!get_scene_map().is_map_popup_active())
                        Item_Action++;
                    break;
                // Finish
                case 2:
                    Item_Phase = 2;
                    // Consume item
                    item_user.use_item(Item_Used);
                    break;
            }
            return true;
        }

        protected void end_item_use()
        {
            In_Item_Use = false;
            Item_Phase = 0;
            Item_Action = 0;
            Item_Timer = 0;
            Item_User_Id = -1;
            Item_Used = -1;
            Item_Inventory_Target = -1;
            Global.game_map.move_range_visible = true;
            Global.game_state.any_trigger_end_events();
        }
    }
}
