using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Status;
using FEXNA_Library;

namespace FEXNA
{
    class Status_Page_2 : Status_Page
    {
        const int ACTOR_SKILLS = 4;
        const int ITEM_SKILLS = 8;
        const int WLVL_COLUMNS = 2;

        protected System_Color_Window Skills_Window, WLvls_Window;
        private Status_Support_Background Skill_Bg;

        private List<StatusUINode> TemporaryWLvls = new List<StatusUINode>();

        public Status_Page_2()
        {
            var nodes = new List<StatusUINode>();

            // Skills Window
            Skills_Window = new System_Color_Window();
            Skills_Window.loc = new Vector2(8, 96);
            Skills_Window.width = 144;
            Skills_Window.height = 96;
            Skills_Window.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;

            // WLvls Window
            int max_wlvl_index = 0;
            if (Global.weapon_types.Any(x => x.DisplayedInStatus))
            {
                max_wlvl_index = Global.weapon_types
                    .Where(x => x.DisplayedInStatus)
                    .Max(x => x.StatusIndex);
            }
            // @Debug: this doesn't really do what it's supposed to,
            // and the window height will be set in set_images() anyway
            int wlvl_rows = (max_wlvl_index / WLVL_COLUMNS) + 1;

            WLvls_Window = new System_Color_Window();
            WLvls_Window.loc = new Vector2(168, 96);
            WLvls_Window.width = 144;
            WLvls_Window.height = (wlvl_rows + 1) * 16; // 96; //Debug
            WLvls_Window.stereoscopic = Config.STATUS_RIGHT_WINDOW_DEPTH;

            // Skill Bg
            Skill_Bg = new Status_Support_Background();
            Skill_Bg.loc = Skills_Window.loc + new Vector2(8, 8 + ACTOR_SKILLS * 16);
            Skill_Bg.stereoscopic = Config.STATUS_RIGHT_WINDOW_DEPTH;
            
            // Skills
            for(int i = 0; i < ACTOR_SKILLS; i++)
            {
                int j = i;

                Vector2 loc = Skills_Window.loc +
                    new Vector2(8, 8 + (Config.SKILL_ICON_SIZE - 16) / 2 +
                        i * Config.SKILL_ICON_SIZE);

                nodes.Add(new StatusSkillUINode(
                    string.Format("Skill{0}", i + 1),
                    (Game_Unit unit) =>
                    {
                        if (unit.actor.skills.Count <= j)
                            return new SkillState();
                        var skill = Global.data_skills[unit.actor.skills[j]];

                        float charge = -1f;
                        if (Game_Unit.MASTERIES.Contains(skill.Abstract))
                            charge = unit.mastery_charge_percent(skill.Abstract);
                        return new SkillState
                        {
                            Skill = skill,
                            Charge = charge
                        };
                    }));
                nodes.Last().loc = loc;
                nodes.Last().draw_offset = new Vector2(
                    0, -(Config.SKILL_ICON_SIZE - 16) / 2);
                nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
#if DEBUG
                // Charges skill gauges
                Func<Game_Unit, DirectionFlags, bool> skill_cheat = (unit, dir) =>
                {
                    if (unit.actor.skills.Count > j)
                    {
                        var skill = Global.data_skills[unit.actor.skills[j]];
                        if (Game_Unit.MASTERIES.Contains(skill.Abstract))
                        {
                            int charge = 0;
                            if (dir.HasFlag(DirectionFlags.Right))
                                charge = 1;
                            else if (dir.HasFlag(DirectionFlags.Left))
                                charge = -1;

                            unit.charge_masteries(skill.Abstract,
                                charge * Game_Unit.MASTERY_RATE_NEW_TURN);
                            return charge != 0;
                        }
                    }
                    return false;
                };
                nodes.Last().set_cheat(skill_cheat);
#endif
            }
            for (int i = 0; i < ITEM_SKILLS; i++)
            {
                int j = i;

                Vector2 loc = Skills_Window.loc +
                    new Vector2(8 + (Config.SKILL_ICON_SIZE - 16) / 2 +
                        i * Config.SKILL_ICON_SIZE, 72 + 2);

                nodes.Add(new StatusSkillIconUINode(
                    string.Format("Item Skill{0}", i + 1),
                    (Game_Unit unit) =>
                    {
                        if (unit.actor.item_skills.Count <= j)
                            return new SkillState();
                        var skill = Global.data_skills[unit.actor.item_skills[j]];

                        float charge = -1f;
                        if (Game_Unit.MASTERIES.Contains(skill.Abstract))
                            charge = unit.mastery_charge_percent(skill.Abstract);
                        return new SkillState
                        {
                            Skill = skill,
                            Charge = charge
                        };
                    }));
                nodes.Last().loc = loc;
                nodes.Last().draw_offset = new Vector2(
                    0, -(Config.SKILL_ICON_SIZE - 16) / 2);
                nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            }
            // WLvls
            foreach (var weapon_type in Global.weapon_types)
            {
                if (!weapon_type.DisplayedInStatus)
                    continue;

                nodes.Add(weapon_type_icon(weapon_type, weapon_type.StatusIndex));
            }

            StatusPageNodes = new UINodeSet<StatusUINode>(nodes);

            init_design();
        }

        private StatusUINode weapon_type_icon(WeaponType weapon_type, int statusIndex)
        {
            Vector2 loc = WLvls_Window.loc + new Vector2(
                (statusIndex % WLVL_COLUMNS) * 64 + 8,
                (statusIndex / WLVL_COLUMNS) * 16 + 8);

            var node = new StatusWLvlUINode(
                weapon_type.StatusHelpName,
                weapon_type,
                (Game_Unit unit) =>
                {
                    return new WLvlState
                    {
                        Rank = unit.actor.weapon_level_letter(weapon_type),
                        Progress = unit.actor.weapon_level_percent(weapon_type),
                        IsCapped = unit.actor.weapon_level_letter(weapon_type) ==
                            Data_Weapon.WLVL_LETTERS[Data_Weapon.WLVL_LETTERS.Length - 1]
                    };
                });
            node.loc = loc;
            node.stereoscopic = Config.STATUS_RIGHT_WINDOW_DEPTH;
#if DEBUG
            Func<Game_Unit, DirectionFlags, bool> wlvl_cheat = (unit, dir) =>
            {
                if (unit.actor.max_weapon_level(weapon_type, true) > 0)
                {
                    int wexp_gain = 0;
                    if (dir.HasFlag(DirectionFlags.Right))
                        wexp_gain = 5;
                    else if (dir.HasFlag(DirectionFlags.Left))
                        wexp_gain = -5;
                    unit.actor.wexp_gain(weapon_type, wexp_gain);
                    return wexp_gain != 0;
                }
                return false;
            };
            node.set_cheat(wlvl_cheat);
#endif
            return node;
        }

        public override void set_images(Game_Unit unit)
        {
            if (Constants.Actor.SHOW_ALL_ACTOR_WEAPON_TYPES)
            {
                // Add potential WLvls
                var nodes = StatusPageNodes.ToList();
                foreach (var weapon_type in TemporaryWLvls)
                    nodes.Remove(weapon_type);

                HashSet<int> weapon_type_indices = new HashSet<int>();
                HashSet<int> added_weapon_types = new HashSet<int>();
                foreach (var weapon_type in Global.weapon_types.Where(x => x.DisplayedInStatus))
                {
                    added_weapon_types.Add(weapon_type.Key);
                    weapon_type_indices.Add(weapon_type.StatusIndex);
                }
                int index = 0;
                foreach (var weapon_type in Global.weapon_types
                    .Where(x => !x.DisplayedInStatus)
                    .OrderBy(x => x.StatusIndex))
                {
                    if (unit.actor.has_rank(weapon_type))
                    {
                        while (weapon_type_indices.Contains(index))
                            index++;
                        var node = weapon_type_icon(weapon_type, index);
                        nodes.Add(node);
                        TemporaryWLvls.Add(node);

                        added_weapon_types.Add(weapon_type.Key);
                        weapon_type_indices.Add(index);
                    }
                }

                StatusPageNodes = new UINodeSet<StatusUINode>(nodes);

                int wlvl_rows = (int)Math.Ceiling(added_weapon_types.Count / (float)WLVL_COLUMNS);
                WLvls_Window.height = (wlvl_rows + 1) * 16;
            }
            // Refresh UI nodes
            refresh(unit);
        }

        public override void update()
        {
            base.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            // Skills Window
            Skills_Window.draw(sprite_batch, draw_offset);
            // WLvls Window
            WLvls_Window.draw(sprite_batch, draw_offset);
            Skill_Bg.draw(sprite_batch, draw_offset);

            foreach (var node in StatusPageNodes
                    .Where(x => x is StatusWLvlUINode))
                (node as StatusWLvlUINode).DrawGaugeBg(
                    sprite_batch, draw_offset);

            // Window Design //
            Window_Design.draw(sprite_batch, draw_offset);

            // Draw Window Contents //
            foreach (var node in StatusPageNodes)
                node.Draw(sprite_batch, draw_offset);
        }
    }
}
