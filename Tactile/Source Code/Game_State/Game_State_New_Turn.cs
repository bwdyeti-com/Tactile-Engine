using System;
using System.Collections.Generic;
using System.IO;
using ListExtension;

namespace Tactile
{
    partial class Game_State
    {
        const float ATTRITION_DAMAGE = 0.35f;

        bool New_Turn_Calling = false;
        bool In_New_Turn = false;
        int New_Turn_Phase = 0;
        int New_Turn_Action = 0;
        int New_Turn_Timer = 0;
        int New_Turn_Unit_Id = -1;
        List<int> New_Turn_Team = new List<int>();

        int New_Turn_Healing_Value, New_Turn_Terrain_Healing_Value;

        #region Serialization
        public void new_turn_write(BinaryWriter writer)
        {
            writer.Write(New_Turn_Calling);
            writer.Write(In_New_Turn);
            writer.Write(New_Turn_Phase);
            writer.Write(New_Turn_Action);
            writer.Write(New_Turn_Timer);
            writer.Write(New_Turn_Unit_Id);
            New_Turn_Team.write(writer);
        }

        public void new_turn_read(BinaryReader reader)
        {
            New_Turn_Calling = reader.ReadBoolean();
            In_New_Turn = reader.ReadBoolean();
            New_Turn_Phase = reader.ReadInt32();
            New_Turn_Action = reader.ReadInt32();
            New_Turn_Timer = reader.ReadInt32();
            New_Turn_Unit_Id = reader.ReadInt32();
            New_Turn_Team.read(reader);
        }
        #endregion

        #region Accessors
        public bool new_turn_calling
        {
            get { return New_Turn_Calling; }
            set { New_Turn_Calling = value; }
        }
        public bool in_new_turn { get { return In_New_Turn; } }
        public bool new_turn_active { get { return New_Turn_Calling || In_New_Turn; } }

        public int new_turn_unit_id { get { return New_Turn_Unit_Id; } }

        protected Game_Unit new_turn_unit { get { return !Global.game_map.units.ContainsKey(New_Turn_Unit_Id) ? null : Global.game_map.units[New_Turn_Unit_Id]; } }

        private bool new_turn_skip_input
        {
            get
            {
                return Global.Input.triggered(Inputs.A, false) ||
                    Global.Input.mouse_triggered(MouseButtons.Left, false) ||
                    Global.Input.gesture_triggered(TouchGestures.Tap, false);
            }
        }
        #endregion

        private void reset_new_turn()
        {
            New_Turn_Calling = false;
            In_New_Turn = false;
            New_Turn_Phase = 0;
            New_Turn_Action = 0;
            New_Turn_Timer = 0;
            New_Turn_Unit_Id = -1;
            New_Turn_Team = new List<int>();
        }

        protected void update_new_turn()
        {
            if (New_Turn_Calling)
            {
                In_New_Turn = true;
                New_Turn_Calling = false;
            }
            if (In_New_Turn)
            {
                bool cont = false;
                while (!cont)
                {
                    cont = true;
                    switch (New_Turn_Phase)
                    {
                        case 0:
                            New_Turn_Team.AddRange(Global.game_map.teams[Team_Turn]);
                            foreach (int id in New_Turn_Team)
                                Global.game_map.units[id].new_turn_fow();
                            Global.game_map.update_fow();
                            Global.game_map.update_enemy_range();
                            New_Turn_Phase++;
                            cont = false;
                            break;
                        case 1:
                            // Loops through units
                            if (New_Turn_Team.Count > 0)
                            {
                                New_Turn_Unit_Id = New_Turn_Team.shift();
                                new_turn_unit.new_turn();
                                if (!new_turn_unit.is_rescued)
                                {
                                    New_Turn_Action = 0;
                                    New_Turn_Phase++;
                                }
                            }
                            else
                            {
                                New_Turn_Unit_Id = -1;
                                New_Turn_Action = 0;
                                New_Turn_Phase++;
                            }
                            cont = false;
                            break;
                        // Heal Status
                        case 2:
                            cont = update_new_turn_heal_status();
                            break;
                        // Terrain Effect
                        case 3:
                            cont = update_new_turn_terrain();
                            break;
                        // Status Effects
                        case 4:
                            cont = update_new_turn_status();
                            break;
                        // Cleanup
                        default:
                            switch (New_Turn_Action)
                            {
                                // Next unit/end
                                case 0:
                                    if (New_Turn_Team.Count == 0)
                                    {
                                        New_Turn_Action++;
                                        New_Turn_Unit_Id = -1;
                                    }
                                    else
                                    {
                                        New_Turn_Phase = 1;
                                        cont = false;
                                    }
                                    break;
                                case 1:
                                    Global.player.instant_move = true;
                                    if (is_player_turn)
                                        Global.player.autocursor(Temp_Player_Loc);
                                    else
                                        Global.player.ai_autocursor(Team_Turn);
                                    New_Turn_Action++;
                                    break;
                                case 2:
                                    if (!Global.game_map.scrolling)
                                    {
                                        Global.game_map.update_new_turn_torch_staves();
                                        // End stuff
                                        In_New_Turn = false;
                                        prev_player_loc = null;
                                        New_Turn_Phase = 0;
                                        New_Turn_Action = 0;
                                        New_Turn_Timer = 0;
                                        //New_Turn_Unit_Id = -1; // above now //Yeti
                                        Global.game_map.update_fow(); //Yeti
                                        Global.game_map.update_enemy_range();
                                        wait_time = 1;
                                        turn_start_events();
                                    }
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        protected bool update_new_turn_heal_status()
        {
            if (get_scene_map() == null)
                return true;
            switch (New_Turn_Action)
            {
                case 0:
                    if (new_turn_unit == null)
                    {
                        New_Turn_Phase++;
                        return false;
                    }
                    else
                    {
                        var statusEffectUpdate = new_turn_unit.actor.clear_updated_states();

                        get_scene_map().update_map_sprite_status(New_Turn_Unit_Id);

                        // Refresh move range if any status effect changed
                        if (statusEffectUpdate != StatusEffectCleared.None)
                            Global.game_map.remove_updated_move_range(New_Turn_Unit_Id);

                        // Focus on unit for status heal effect if a negative effect was removed
                        if (statusEffectUpdate == StatusEffectCleared.NegativeRemoved)
                        {
                            Global.player.force_loc(new_turn_unit.loc);
                            New_Turn_Action++;
                            return true;
                        }
                        else
                        {
                            New_Turn_Phase++;
                            return false;
                        }
                    }
                case 1:
                    if (!Global.game_map.scrolling)
                    {
                        if (new_turn_unit.visible_by())
                            get_scene_map().set_status_heal(new_turn_unit);
                        new_turn_unit.status_recover();
                        New_Turn_Action++;
                    }
                    break;
                case 2:
                    switch (New_Turn_Timer)
                    {
                        case 0:
                            new_turn_unit.update_attack_graphics();
                            if (!new_turn_unit.is_hit_flashing())
                            {
                                New_Turn_Timer++;
                            }
                            break;
                        case 12:
                            New_Turn_Timer = 0;
                            New_Turn_Action = 0;
                            New_Turn_Phase++;
                            return false;
                        default:
                            New_Turn_Timer++;
                            break;
                    }
                    break;
            }
            return true;
        }

        protected bool update_new_turn_terrain()
        {
            if (get_scene_map() == null)
                return true;
            int heal_amount;
            switch (New_Turn_Action)
            {
                case 0:
                    if (new_turn_unit == null)
                    {
                        New_Turn_Phase++;
                        return false;
                    }

                    heal_amount = Global.game_map.terrain_healing_amount(new_turn_unit.loc);
                    New_Turn_Terrain_Healing_Value = 0;
                    if (heal_amount != 0)
                    {
                        New_Turn_Terrain_Healing_Value = ((heal_amount * new_turn_unit.actor.maxhp) / 100);
                        if (heal_amount > 0)
                            New_Turn_Terrain_Healing_Value = Math.Max(1, New_Turn_Terrain_Healing_Value);
                        else
                            New_Turn_Terrain_Healing_Value = Math.Min(-1, New_Turn_Terrain_Healing_Value);
                    }

                    New_Turn_Healing_Value = New_Turn_Terrain_Healing_Value + new_turn_unit.skill_new_turn_heal_amount();
#if !MONOGAME && DEBUG
                    // Don't cause invalid terrain damage in the unit editor
                    if (!Global.scene.is_unit_editor)
                    {
#endif
                        // Eventually kill units standing on invalid terrain
                        if (new_turn_unit.move_cost(new_turn_unit.loc) == -1)
                        {
                            var terrain = Global.game_map.terrain_data(new_turn_unit.loc);
                            // Ignore villages
                            // Add data to Data_Terrain for whether this should happen //Yeti
                            if (terrain.Name != "--" && terrain.Name != "Village")
                            {
                                New_Turn_Healing_Value += -(int)Math.Ceiling(
                                    new_turn_unit.actor.maxhp * ATTRITION_DAMAGE);
                                New_Turn_Healing_Value = Math.Min(New_Turn_Healing_Value, -1);
                            }
                        }
#if !MONOGAME && DEBUG
                    }
#endif
                    // If the healing value is nonzero, and either it will damage the unit or can heal the unit
                    // Or the terrain has some healing value and there is an injured rescuee (and rescued units can be healed by terrain)
                    if ((New_Turn_Healing_Value != 0 && (New_Turn_Healing_Value < 0 || !new_turn_unit.actor.is_full_hp())) ||
                        (Constants.Map.RESCUED_TERRAIN_HEAL &&
                        New_Turn_Terrain_Healing_Value > 0 &&
                        new_turn_unit.is_rescuing &&
                        !new_turn_unit.rescuing_unit.actor.is_full_hp()))
                    {
                        if (Constants.Map.RESCUED_TERRAIN_HEAL &&
                            new_turn_unit.is_rescuing)
                        {
                            New_Turn_Terrain_Healing_Value = ((heal_amount * new_turn_unit.rescuing_unit.actor.maxhp) / 100);
                            if (heal_amount > 0)
                                New_Turn_Terrain_Healing_Value = Math.Max(1, New_Turn_Terrain_Healing_Value);
                            else
                                New_Turn_Terrain_Healing_Value = Math.Min(-1, New_Turn_Terrain_Healing_Value);
                        }
                        else
                            New_Turn_Terrain_Healing_Value = 0;
                        New_Turn_Action++;
                        Global.player.force_loc(new_turn_unit.loc);
                    }
                    else
                    {
                        New_Turn_Phase++;
                        return false;
                    }
                    break;
                case 1:
                    switch (New_Turn_Timer)
                    {
                        case 4:
                            if (!Global.game_map.scrolling)
                            {
                                New_Turn_Action++;
                                New_Turn_Timer = 0;
                                return false;
                            }
                            break;
                        default:
                            New_Turn_Timer++;
                            break;
                    }
                    break;
                #region Makes HP window, stops map sprite
                case 2:
                    switch (New_Turn_Timer)
                    {
                        case 0:
                            new_turn_unit.sprite_moving = false;
                            // I think the map sprite is supposed to lock into no animation here if damaged, but??? //Yeti
                            new_turn_unit.frame = 0;
                            new_turn_unit.facing = 2;
                            if (Constants.Map.RESCUED_TERRAIN_HEAL &&
                                New_Turn_Terrain_Healing_Value > 0 && new_turn_unit.is_rescuing && !new_turn_unit.rescuing_unit.actor.is_full_hp())
                            {
                                // Just rescued unit
                                if (!Constants.Map.RESCUER_TERRAIN_HEAL_FULL_HP_DISPLAY &&
                                        New_Turn_Healing_Value >= 0 &&
                                        new_turn_unit.actor.is_full_hp())
                                    get_scene_map().create_hud(new_turn_unit.rescuing);
                                // Both units
                                else
                                    get_scene_map().create_hud(New_Turn_Unit_Id, new_turn_unit.rescuing);
                            }
                            // Just active unit
                            else
                                get_scene_map().create_hud(New_Turn_Unit_Id);
                            New_Turn_Timer++;
                            break;
                        case 1:
                            if (New_Turn_Healing_Value >= 0)
                            {
                                new_turn_unit.frame = 0;
                                new_turn_unit.facing = 6;
                            }
                            New_Turn_Timer++;
                            break;
                        case 23:
                            New_Turn_Action++;
                            New_Turn_Timer = 0;
                            break;
                        default:
                            New_Turn_Timer++;
                            break;
                    }
                    break;
                #endregion
                #region Map sprite animates
                case 3:
                    switch (New_Turn_Timer)
                    {
                        case 0:
                            if (New_Turn_Healing_Value >= 0)
                                new_turn_unit.frame = 1;
                            New_Turn_Timer++;
                            break;
                        case 4:
                            if (New_Turn_Healing_Value >= 0)
                                new_turn_unit.frame = 2;
                            New_Turn_Timer++;
                            break;
                        case 10:
                            New_Turn_Action++;
                            New_Turn_Timer = 0;
                            break;
                        default:
                            New_Turn_Timer++;
                            break;
                    }
                    break;
                #endregion
                #region Starts item animation
                case 4:
                    switch (New_Turn_Timer)
                    {
                        case 0:
                            heal_amount = Global.game_map.terrain_healing_amount(new_turn_unit.loc);
                            
                            // Invalid terrain
                            if (new_turn_unit.move_cost(new_turn_unit.loc) == -1 &&
                                Global.game_map.terrain_data(new_turn_unit.loc).Name != "--")
                            {
                                get_scene_map().set_map_effect(new_turn_unit.loc, 3, 31);
                            }
                            // If the terrain is causing damage but overall the unit is healing, default to vulnerary anim, otherwise use terrain anim
                            else if (heal_amount != 0)
                                get_scene_map().set_map_effect(new_turn_unit.loc, 0,
                                    New_Turn_Healing_Value >= 0 && heal_amount < 0 ? 1 :
                                    Global.game_map.terrain_data(new_turn_unit.loc).Heal[1]);
                            else
                                get_scene_map().set_map_effect(new_turn_unit.loc, 0, 1);
                            New_Turn_Timer++;
                            break;
                        default:
                            if (New_Turn_Timer == 31 || new_turn_skip_input)
                            {
                                New_Turn_Action++;
                                New_Turn_Timer = 0;
                                return false;
                            }
                            else
                                New_Turn_Timer++;
                            break;
                    }
                    break;
                #endregion
                #region HP gain
                case 5:
                    switch (New_Turn_Timer)
                    {
                        case 0:
                            new_turn_unit.hp += New_Turn_Healing_Value;
                            if (Constants.Map.RESCUED_TERRAIN_HEAL && New_Turn_Terrain_Healing_Value > 0)
                                new_turn_unit.rescuing_unit.hp += New_Turn_Terrain_Healing_Value;
                            if (New_Turn_Healing_Value < 0)
                            {
                                Global.Audio.play_se("Map Sounds", new_turn_unit.is_dead ? "Hit_Kill" : "Hit");
                                new_turn_unit.hit_color();
                            }
                            new_turn_unit.update_attack_graphics();
                            New_Turn_Timer++;
                            break;
                        case 1:
                            if ((!get_scene_map().is_map_effect_active() &&
                                !new_turn_unit.is_hit_flashing()) ||
                                new_turn_skip_input)
                            {
                                if (get_scene_map().combat_hud_ready())
                                    New_Turn_Timer++;
                            }
                            break;
                        case 27:
                            if (New_Turn_Healing_Value >= 0)
                                new_turn_unit.frame = 1;
                            New_Turn_Timer++;
                            break;
                        case 35:
                            if (New_Turn_Healing_Value >= 0)
                                new_turn_unit.frame = 0;
                            else
                            {
                                new_turn_unit.sprite_moving = false;
                                new_turn_unit.frame = 0;
                                new_turn_unit.facing = 2;
                            }
                            New_Turn_Timer++;
                            break;
                        case 36:
                            if (!get_scene_map().is_map_effect_active() &&
                                    !new_turn_unit.is_hit_flashing())
                                New_Turn_Timer++;
                            break;
                        case 47:
                            New_Turn_Action++;
                            New_Turn_Timer = 0;
                            new_turn_unit.update_attack_graphics();
                            return true;
                        default:
                            New_Turn_Timer++;
                            break;
                    }
                    new_turn_unit.update_attack_graphics();
                    if (New_Turn_Timer < 17 && new_turn_skip_input)
                    {
                        new_turn_unit.reset_attack_graphics();
                        New_Turn_Timer = 17;
                    }
                    break;
                #endregion
                #region Remove unit if dead
                case 6:
                    if (new_turn_unit == null || new_turn_unit.is_dead)
                    {
                        switch (New_Turn_Timer)
                        {
                            case 0:
                                Global.game_temp.dying_unit_id = New_Turn_Unit_Id;
                                if (!string.IsNullOrEmpty(get_death_quote(Global.game_temp.dying_unit_id)))
                                {
                                    get_scene_map().clear_combat();
                                    Global.game_map.play_death_quote(Global.game_temp.dying_unit_id);
                                    get_scene_map().clear_combat();
                                }
                                New_Turn_Timer++;
                                break;
                            case 1:
                                if (!Global.scene.is_message_window_active)
                                {
                                    Global.game_map.add_dying_unit_animation(new_turn_unit);
                                    New_Turn_Timer++;
                                    return false;
                                }
                                break;
                            case 2:
                                new_turn_unit.update_attack_graphics();
                                if (!Global.game_map.units_dying) // !new_turn_unit.changing_opacity()) //Debug
                                {
                                    if (new_turn_unit.is_player_team && !Global.game_system.is_loss()) //Multi
                                    {
                                        Global.Audio.BgmFadeOut(15);
                                        resume_turn_theme(true);
                                    }
                                    new_turn_unit.kill();
                                    New_Turn_Unit_Id = -1;
                                    New_Turn_Timer++;
                                    return false;
                                }
                                break;
                            case 3:
                                Global.game_map.refresh_move_ranges();

                                Global.game_map.wait_for_move_update();
                                New_Turn_Timer++;
                                break;
                            case 4:
                                get_scene_map().clear_combat();

                                any_trigger_events();
                                if (Global.game_system.is_interpreter_running)
                                {
                                    New_Turn_Timer++;
                                    return false;
                                }
                                else
                                    New_Turn_Action++;
                                break;
                            case 5:
                                if (!Global.game_system.is_interpreter_running)
                                    New_Turn_Action++;
                                break;
                        }
                    }
                    else
                    {
                        New_Turn_Action++;
                        get_scene_map().clear_combat();
                    }
                    break;
                #endregion
                #region Next phase
                case 7:
                    New_Turn_Phase++;
                    New_Turn_Action = 0;
                    New_Turn_Timer = 0;
                    return false;
                #endregion
            }
            return true;
        }

        protected bool update_new_turn_status()
        {
            if (get_scene_map() == null)
                return true;
            switch (New_Turn_Action)
            {
                case 0:
                    if (new_turn_unit == null)
                    {
                        New_Turn_Phase++;
                        return false;
                    }
                    else
                    {
                        if (new_turn_unit.actor.has_damaging_status())
                        {
                            New_Turn_Action++;
                            Global.player.force_loc(new_turn_unit.loc);
                        }
                        else
                        {
                            New_Turn_Phase++;
                            return false;
                        }
                    }
                    break;
                case 1:
                    switch (New_Turn_Timer)
                    {
                        case 4:
                            if (!Global.game_map.scrolling)
                            {
                                New_Turn_Action++;
                                New_Turn_Timer = 0;
                                return false;
                            }
                            break;
                        default:
                            New_Turn_Timer++;
                            break;
                    }
                    break;
                // Makes HP window, stops map sprite
                case 2:
                    switch (New_Turn_Timer)
                    {
                        case 0:
                            new_turn_unit.sprite_moving = false;
                            new_turn_unit.frame = 0;
                            new_turn_unit.facing = 2;
                            get_scene_map().create_hud(New_Turn_Unit_Id);
                            New_Turn_Timer++;
                            break;
                        case 1:
                            if (false) // if regen //Yeti
                            {
                                new_turn_unit.frame = 0;
                                new_turn_unit.facing = 6;
                            }
                            New_Turn_Timer++;
                            break;
                        case 23:
                            New_Turn_Action++;
                            New_Turn_Timer = 0;
                            break;
                        default:
                            New_Turn_Timer++;
                            break;
                    }
                    break;
                // Map sprite animates
                case 3:
                    switch (New_Turn_Timer)
                    {
                        case 0:
                            if (false) // if regen //Yeti
                                new_turn_unit.frame = 1;
                            New_Turn_Timer++;
                            break;
                        case 4:
                            if (false) // if regen //Yeti
                                new_turn_unit.frame = 2;
                            New_Turn_Timer++;
                            break;
                        case 10:
                            New_Turn_Action++;
                            New_Turn_Timer = 0;
                            break;
                        default:
                            New_Turn_Timer++;
                            break;
                    }
                    break;
                // Starts item animation
                case 4:
                    switch (New_Turn_Timer)
                    {
                        case 0:
                            get_scene_map().set_map_effect(new_turn_unit.loc, 3,
                                Global.data_statuses[new_turn_unit.actor.damaging_status_effect_id()].Map_Anim_Id);
                            New_Turn_Timer++;
                            break;
                        default:
                            if (get_scene_map().is_map_effect_hit() || new_turn_skip_input)
                            {
                                New_Turn_Action++;
                                New_Turn_Timer = 0;
                                return false;
                            }
                            break;
                    }
                    break;
                // HP gain
                case 5:
                    switch (New_Turn_Timer)
                    {
                        case 0:
                            int value = new_turn_unit.actor.status_damage();
                            new_turn_unit.hp += value;
                            if (value < 0)
                            {
                                Global.Audio.play_se("Map Sounds", new_turn_unit.is_dead ? "Hit_Kill" : "Hit");
                                new_turn_unit.hit_color(Global.data_statuses[new_turn_unit.actor.damaging_status_effect_id()].Battle_Color);
                            }
                            new_turn_unit.update_attack_graphics();
                            New_Turn_Timer++;
                            break;
                        case 1:
                            if ((!get_scene_map().is_map_effect_active() &&
                                !new_turn_unit.is_hit_flashing()) ||
                                new_turn_skip_input)
                            {
                                if (get_scene_map().combat_hud_ready())
                                    New_Turn_Timer++;
                            }
                            break;
                        case 27:
                            New_Turn_Timer++;
                            if (false) // if regen //Yeti
                                new_turn_unit.frame = 1;
                            else
                            {
                                New_Turn_Action++;
                                New_Turn_Timer = 0;
                            }
                            break;
                        case 35:
                            if (false) // if regen //Yeti
                                new_turn_unit.frame = 0;
                            else
                            {
                                new_turn_unit.sprite_moving = false;
                                new_turn_unit.frame = 0;
                                new_turn_unit.facing = 2;
                            }
                            New_Turn_Timer++;
                            break;
                        case 36:
                            if (!get_scene_map().is_map_effect_active() &&
                                    !new_turn_unit.is_hit_flashing())
                                New_Turn_Timer++;
                            break;
                        case 47:
                            New_Turn_Action++;
                            New_Turn_Timer = 0;
                            new_turn_unit.update_attack_graphics();
                            return true;
                        default:
                            New_Turn_Timer++;
                            break;
                    }
                    new_turn_unit.update_attack_graphics();
                    if (New_Turn_Timer < 17 && new_turn_skip_input)
                    {
                        new_turn_unit.reset_attack_graphics();
                        New_Turn_Timer = 17;
                    }
                    break;
                // Remove unit if dead
                case 6:
                    if (new_turn_unit == null || new_turn_unit.is_dead)
                    {
                        switch (New_Turn_Timer)
                        {
                            case 0:
                                Global.game_temp.dying_unit_id = New_Turn_Unit_Id;
                                if (!string.IsNullOrEmpty(get_death_quote(Global.game_temp.dying_unit_id)))
                                {
                                    get_scene_map().clear_combat();
                                    Global.game_map.play_death_quote(Global.game_temp.dying_unit_id);
                                    get_scene_map().clear_combat();
                                }
                                New_Turn_Timer++;
                                break;
                            case 1:
                                if (!Global.scene.is_message_window_active)
                                {
                                    Global.game_map.add_dying_unit_animation(new_turn_unit);
                                    New_Turn_Timer++;
                                    return false;
                                }
                                break;
                            case 2:
                                new_turn_unit.update_attack_graphics();
                                if (!Global.game_map.units_dying) // !new_turn_unit.changing_opacity()) //Debug
                                {
                                    if (new_turn_unit.is_player_team && !Global.game_system.is_loss()) //Multi
                                    {
                                        Global.Audio.BgmFadeOut(15);
                                        resume_turn_theme(true);
                                    }
                                    new_turn_unit.kill();
                                    New_Turn_Unit_Id = -1;
                                    New_Turn_Timer++;
                                    return false;
                                }
                                break;
                            case 3:
                                Global.game_map.refresh_move_ranges();

                                Global.game_map.wait_for_move_update();
                                New_Turn_Timer++;
                                break;
                            case 4:
                                get_scene_map().clear_combat();

                                any_trigger_events();
                                if (Global.game_system.is_interpreter_running)
                                {
                                    New_Turn_Timer++;
                                    return false;
                                }
                                else
                                    New_Turn_Action++;
                                break;
                            case 5:
                                if (!Global.game_system.is_interpreter_running)
                                    New_Turn_Action++;
                                break;
                        }
                    }
                    else
                    {
                        New_Turn_Action++;
                        get_scene_map().clear_combat();
                    }
                    break;
                // Next phase
                case 7:
                    New_Turn_Phase++;
                    New_Turn_Action = 0;
                    New_Turn_Timer = 0;
                    return false;
            }
            return true;
        }
    }
}
