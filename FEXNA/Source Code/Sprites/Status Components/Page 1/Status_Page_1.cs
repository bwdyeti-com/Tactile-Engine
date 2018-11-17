using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Status;
using FEXNA_Library;

namespace FEXNA
{
    class Status_Page_1 : Status_Page
    {
        protected System_Color_Window Stats_Window, Items_Window;
        protected StatusStatUINode PowNode;
        private Status_Support_Background SiegeBg;

        public Status_Page_1()
        {
            var nodes = new List<StatusUINode>();

            // Stats Window
            Stats_Window = new System_Color_Window();
            Stats_Window.loc = new Vector2(8, 80);
            Stats_Window.width = 144;
            Stats_Window.height = 112;
            Stats_Window.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            // Stats
            for (int i = 0; i < 6; i++)
            {
                string help_label;
                string label;

                var stat_label = (Stat_Labels)i + 1;

                Vector2 loc = Stats_Window.loc + new Vector2(8, i * 16 + 8);
                PrimaryStatState.label((Stat_Labels)i + 1, out label, out help_label);

                Func<Game_Unit, PrimaryStatState> stat_formula = (Game_Unit unit) =>
                {
                    return new PrimaryStatState(unit, stat_label);
                };

                Func<Game_Unit, Color> label_color = null;
                if (Window_Status.show_stat_averages(stat_label))
                {
                    label_color = (Game_Unit unit) =>
                    {
                        if (unit.average_stat_hue_shown)
                            return unit.actor.stat_color(stat_label);
                        return Color.White;
                    };
                }

                nodes.Add(new StatusPrimaryStatUINode(
                    help_label, label, stat_formula, label_color, 40));
                nodes.Last().loc = loc;
                nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
#if DEBUG
                nodes.Last().set_cheat(stat_cheat(stat_label));
#endif

                if (stat_label == Stat_Labels.Pow)
                    PowNode = nodes.Last() as StatusStatUINode;
            }

            // Move
            nodes.Add(new StatusPrimaryStatUINode(
                "Move",
                "Move",
                (Game_Unit unit) =>
                {
                    if (unit.immobile)
                        return new PrimaryStatState
                        {
                            Stat = 0,
                            Bonus = 0,
                            Cap = unit.stat_cap(Stat_Labels.Mov),
                            NullStat = true,
                        };
                    return new PrimaryStatState
                    {
                        Stat = unit.base_mov,
                        Bonus = unit.mov - unit.base_mov,
                        Cap = unit.stat_cap(Stat_Labels.Mov),
                    };
                }, null, 40));
            nodes.Last().loc = Stats_Window.loc + new Vector2(72, 0 * 16 + 8);
            nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
#if DEBUG
            nodes.Last().set_cheat(stat_cheat(Stat_Labels.Mov));
#endif
            // Con
            nodes.Add(new StatusPrimaryStatUINode(
                "Con",
                "Con",
                (Game_Unit unit) =>
                {
                    return new PrimaryStatState
                    {
                        Stat = unit.actor.stat(Stat_Labels.Con),
                        Bonus = Math.Min(unit.stat_bonus(Stat_Labels.Con),
                            unit.actor.cap_base_difference(Stat_Labels.Con)),
                        Cap = unit.stat_cap(Stat_Labels.Con),
                        IsCapped = unit.actor.get_capped(Stat_Labels.Con)
                    };
                }, null, 40));
            nodes.Last().loc = Stats_Window.loc + new Vector2(72, 1 * 16 + 8);
            nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
#if DEBUG
            nodes.Last().set_cheat(stat_cheat(Stat_Labels.Con));
#endif
            // Aid
            nodes.Add(new StatusAidUINode(
                "Aid",
                "Aid",
                (Game_Unit unit) =>
                {
                    return unit.aid().ToString();
                },
                (Game_Unit unit) =>
                {
                    if (unit.actor.actor_class.Class_Types.Contains(ClassTypes.FDragon))
                        return 3;
                    else if (unit.actor.actor_class.Class_Types.Contains(ClassTypes.Flier))
                        return 2;
                    else if (unit.actor.actor_class.Class_Types.Contains(ClassTypes.Cavalry))
                        return 1;
                    else
                        return 0;
                }, 40));
            nodes.Last().loc = Stats_Window.loc + new Vector2(72, 2 * 16 + 8);
            nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            // Trv
            nodes.Add(new StatusTravelerUINode(
                "Trv",
                "Trv",
                (Game_Unit unit) =>
                {
                    if (unit.is_rescued)
                        return Global.game_map.units[unit.rescued].actor.name;
                    else if (unit.is_rescuing)
                        return Global.game_map.units[unit.rescuing].actor.name;
                    return "---";
                },
                (Game_Unit unit) =>
                {
                    if (!unit.is_rescuing)
                        return 0;
                    return Global.game_map.units[unit.rescuing].team;
                }, 24));
            nodes.Last().loc = Stats_Window.loc + new Vector2(72, 3 * 16 + 8);
            nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            // Type
            nodes.Add(new StatusClassTypesUINode(
                "Type",
                "Type",
                (Game_Unit unit) =>
                {
                    return unit.actor.class_types;
                }, 24));
            nodes.Last().loc = Stats_Window.loc + new Vector2(72, 4 * 16 + 8);
            nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            // Rating
            nodes.Add(new StatusLabeledTextUINode(
                "Rating",
                "Rating",
                (Game_Unit unit) =>
                {
                    return unit.rating().ToString();
                }, 32));
            nodes.Last().loc = Stats_Window.loc + new Vector2(72, 5 * 16 + 8);
            nodes.Last().Size = new Vector2(64, 16);
            nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;

            // Items Window
            Items_Window = new System_Color_Window();
            Items_Window.loc = new Vector2(168, 80);
            Items_Window.width = 144;
            Items_Window.height = Constants.Actor.NUM_ITEMS * 16 + 16;
            Items_Window.stereoscopic = Config.STATUS_RIGHT_WINDOW_DEPTH;

            // Skill Bg
            SiegeBg = new Status_Support_Background();
            SiegeBg.loc = Items_Window.loc + new Vector2(
                8, 8 + (Constants.Actor.NUM_ITEMS - 1) * 16);
            SiegeBg.stereoscopic = Config.STATUS_RIGHT_WINDOW_DEPTH;
            SiegeBg.visible = false;

            // Items
            for (int i = 0; i < Constants.Actor.NUM_ITEMS; i++)
            {
                int j = i;

                Vector2 loc = Items_Window.loc + new Vector2(8, i * 16 + 8);

                nodes.Add(new StatusItemUINode(
                    string.Format("Item{0}", i + 1),
                    (Game_Unit unit) =>
                    {
                        return new ItemState
                        {
                            Item = unit.actor.items[j],
                            Drops = unit.drops_item && j == unit.actor.num_items - 1,
                            Equipped = unit.actor.equipped - 1 == j
                        };
                    }));
                nodes.Last().loc = loc;
                nodes.Last().stereoscopic = Config.STATUS_RIGHT_WINDOW_DEPTH;
                Func<Game_Unit, DirectionFlags, bool> item_cheat = (unit, dir) =>
                {
                    // Uses
                    if (dir.HasFlag(DirectionFlags.Up) || dir.HasFlag(DirectionFlags.Down))
                    {
                        if (unit.actor.items[j].non_equipment ||
                                unit.actor.items[j].infinite_uses)
                            return false;
                        int uses = unit.actor.items[j].Uses;
                        if (dir.HasFlag(DirectionFlags.Up))
                            uses++;
                        else
                            uses--;
                        uses = Math.Max(Math.Min(
                            uses, unit.actor.items[j].max_uses), 1);
                        if (uses == unit.actor.items[j].Uses)
                            return false;
                        unit.actor.items[j].Uses = uses;
                        return true;
                    }
                    else
                    {
                        // Change item
                        if (unit.actor.items[j].is_weapon)
                        {
                            List<int> weapon_keys = Global.data_weapons.Keys.ToList();
                            int index = weapon_keys.IndexOf(unit.actor.items[j].Id);
                            if (dir.HasFlag(DirectionFlags.Right))
                                index++;
                            else if (dir.HasFlag(DirectionFlags.Left))
                                index--;
                            else
                                return false;
                            index = (index + weapon_keys.Count) % weapon_keys.Count;
                            unit.actor.items[j].Id = weapon_keys[index];
                            unit.actor.setup_items(false);
                        }
                        else
                        {
                            List<int> item_keys = Global.data_items.Keys.ToList();
                            int index = item_keys.IndexOf(unit.actor.items[j].Id);
                            if (dir.HasFlag(DirectionFlags.Right))
                                index++;
                            else if (dir.HasFlag(DirectionFlags.Left))
                                index--;
                            else
                                return false;
                            index = (index + item_keys.Count) % item_keys.Count;
                            unit.actor.items[j].Id = item_keys[index];
                        }
                        if (unit.actor.items[j].infinite_uses)
                            unit.actor.items[j].Uses = -1;
                        else
                        {
                            if (unit.actor.items[j].Uses == -1)
                                unit.actor.items[j].Uses = 1;
                        }
                        return true;
                    }
                };
#if DEBUG
                nodes.Last().set_cheat(item_cheat);
#endif
            }

            // Siege engine
            Vector2 siege_loc = Items_Window.loc +
                new Vector2(8, (Constants.Actor.NUM_ITEMS - 1) * 16 + 8 + 2);

            nodes.Add(new StatusSiegeItemUINode(
                string.Format("Item{0}", Constants.Actor.NUM_ITEMS + 1),
                (Game_Unit unit) =>
                {
                    Item_Data siege = new Item_Data();
                    if (!unit.actor.is_full_items && unit.is_on_siege())
                        siege = unit.items[Siege_Engine.SIEGE_INVENTORY_INDEX];

                    return new ItemState
                    {
                        Item = siege,
                        Drops = false,
                        Equipped = false
                    };
                }));
            nodes.Last().loc = siege_loc;
            nodes.Last().stereoscopic = Config.STATUS_RIGHT_WINDOW_DEPTH;


            StatusPageNodes = new UINodeSet<StatusUINode>(nodes);

            init_design();
        }

#if DEBUG
        internal static Func<Game_Unit, DirectionFlags, bool> stat_cheat(Stat_Labels stat_label)
        {
            Func<Game_Unit, DirectionFlags, bool> stat_cheat = (unit, dir) =>
            {
                int stat_gain = 0;
                if (dir.HasFlag(DirectionFlags.Right))
                    stat_gain = 1;
                else if (dir.HasFlag(DirectionFlags.Left))
                    stat_gain = -1;
                unit.actor.gain_stat(stat_label, stat_gain);
                unit.queue_move_range_update();
                return stat_gain != 0;
            };
            return stat_cheat;
        }
#endif

        public override void set_images(Game_Unit unit)
        {
            Game_Actor actor = unit.actor;
            // Stats
            switch (actor.power_type())
            {
                case Power_Types.Strength:
                    PowNode.set_label("Str");
                    break;
                case Power_Types.Magic:
                    PowNode.set_label("Mag");
                    break;
                default:
                    PowNode.set_label("Pow");
                    break;
            }
            // Aid
            //Stat_Values[7].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue"); // Green if capped //Yeti
            // Mov
            //Stat_Values[8].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue"); // Green if capped //Yeti

            SiegeBg.visible = !unit.actor.is_full_items && unit.is_on_siege();

            // Refresh UI nodes
            refresh(unit);
        }

        public override void update()
        {
            base.update();
            // Stats Window
            Stats_Window.update();
            // Item Window
            Items_Window.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            // Stats Window
            Stats_Window.draw(sprite_batch, draw_offset);

            foreach (var node in StatusPageNodes
                    .Where(x => x is StatusPrimaryStatUINode))
                (node as StatusPrimaryStatUINode).DrawGaugeBg(
                    sprite_batch, draw_offset);

            // Item Window
            Items_Window.draw(sprite_batch, draw_offset);
            SiegeBg.draw(sprite_batch, draw_offset);

            // Window Design //
            Window_Design.draw(sprite_batch, draw_offset);

            // Draw Window Contents //
            foreach (var node in StatusPageNodes)
                node.Draw(sprite_batch, draw_offset);
        }
    }
}