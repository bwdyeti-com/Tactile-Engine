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
    class Status_Page_3 : Status_Page
    {
        const int STATUS_ICONS_AT_ONCE = 3;
        const int ACTOR_STATUSES = 5;

        private System_Color_Window Bonuses_Window, Supports_Window;
        private Status_Bonus_Background Bonus_Bg;
        private Status_Support_Background Support_Bg;
        // Supports
        private Status_Support_List Supports;

        public Status_Page_3()
        {
            var nodes = new List<StatusUINode>();

            // Bonuses Window
            Bonuses_Window = new System_Color_Window();
            Bonuses_Window.loc = new Vector2(8, 80);
            Bonuses_Window.width = 144;
            Bonuses_Window.height = 112;
            Bonuses_Window.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            // Status Label
            nodes.Add(new StatusTextUINode(
                "Cond",
                (Game_Unit unit) => "Status"));
            nodes.Last().loc = Bonuses_Window.loc + new Vector2(16, 8);
            (nodes.Last() as StatusTextUINode).set_color("Yellow");
            nodes.Last().Size = new Vector2(32, 16);
            nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;

            // Statuses
            for (int i = 0; i < ACTOR_STATUSES; i++)
            {
                int j = i;

                Vector2 loc = Bonuses_Window.loc + new Vector2(48 + i * 16, 8);

                nodes.Add(new StatusStateUINode(
                    string.Format("Status{0}", i + 1),
                    (Game_Unit unit) =>
                    {
                        if (unit.actor.states.Count <= j)
                            return new Tuple<int, int>(-1, 0);

                        int id = unit.actor.states[j];
                        int turns = unit.actor.state_turns_left(id);

                        return new Tuple<int, int>(id, turns);
                    }));
                nodes.Last().loc = loc;
                nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            }

            // Bond
            nodes.Add(new StatusLabeledTextUINode(
                "Bond",
                "Bond",
                (Game_Unit unit) =>
                {
                    if (unit.actor.bond > 0)
                        return Global.game_actors[unit.actor.bond].name;
                    else
                        return "-----";
                }, 52, true));
            nodes.Last().loc = Bonuses_Window.loc + new Vector2(32 + 4, 2 * 16 + 4);
            (nodes.Last() as StatusTextUINode).set_color("White");
            nodes.Last().Size = new Vector2(80, 16);
            nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            // Bonuses
            for (int i = 0; i < 6; i++)
            {
                string help_label;
                string label;
                Func<Game_Unit, string> stat_formula;
                switch (i)
                {
                    // Atk
                    case 0:
                    default:
                        help_label = "BAtk";
                        label = "Atk";
                        stat_formula = (Game_Unit unit) =>
                            unit.support_bonus(Combat_Stat_Labels.Dmg, true).ToString();
                        break;
                    // Hit
                    case 1:
                        help_label = "BHit";
                        label = "Hit";
                        stat_formula = (Game_Unit unit) =>
                            unit.support_bonus(Combat_Stat_Labels.Hit, true).ToString();
                        break;
                    // Crit
                    case 2:
                        help_label = "BCrt";
                        label = "Crit";
                        stat_formula = (Game_Unit unit) =>
                            unit.support_bonus(Combat_Stat_Labels.Crt, true).ToString();
                        break;
                    // Def
                    case 3:
                        help_label = "BDef";
                        label = "Def";
                        stat_formula = (Game_Unit unit) =>
                            unit.support_bonus(Combat_Stat_Labels.Def, true).ToString();
                        break;
                    // Avoid
                    case 4:
                        help_label = "BAvo";
                        label = "Avoid";
                        stat_formula = (Game_Unit unit) =>
                            unit.support_bonus(Combat_Stat_Labels.Avo, true).ToString();
                        break;
                    // Dodge
                    case 5:
                        help_label = "BDod";
                        label = "Dodge";
                        stat_formula = (Game_Unit unit) =>
                            unit.support_bonus(Combat_Stat_Labels.Dod, true).ToString();
                        break;
                }

                Vector2 loc = Bonuses_Window.loc +
                    new Vector2(20 + (i / 3) * 56, 56 + (i % 3) * 16);

                nodes.Add(new StatusStatUINode(help_label, label, stat_formula));
                nodes.Last().loc = loc;
                nodes.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            }
            // Bonus Bg
            Bonus_Bg = new Status_Bonus_Background();
            Bonus_Bg.loc = Bonuses_Window.loc + new Vector2(8, 24);
            Bonus_Bg.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;

            // Supports Window
            Supports_Window = new System_Color_Window();
            Supports_Window.loc = new Vector2(168, 80);
            Supports_Window.width = 144;
            Supports_Window.height = 112;
            Supports_Window.stereoscopic = Config.STATUS_RIGHT_WINDOW_DEPTH;
            // Affinity
            nodes.Add(new StatusAffinityUINode(
                "Affin",
                (Game_Unit unit) => unit.actor.affin));
            nodes.Last().loc = Supports_Window.loc + new Vector2(40, 8);
            nodes.Last().stereoscopic = Config.STATUS_RIGHT_WINDOW_DEPTH;
            // Supports
            Supports = new Status_Support_List();
            Supports.loc = Supports_Window.loc + new Vector2(32, 24);
            Supports.stereoscopic = Config.STATUS_RIGHT_WINDOW_DEPTH;
            // Support Bg
            Support_Bg = new Status_Support_Background();
            Support_Bg.loc = Supports_Window.loc + new Vector2(8, 24);
            Support_Bg.stereoscopic = Config.STATUS_RIGHT_WINDOW_DEPTH;

            StatusPageNodes = new UINodeSet<StatusUINode>(nodes);

            init_design();
        }

        public override void set_images(Game_Unit unit)
        {
            Game_Actor actor = unit.actor;
            // Supports
            Supports.set_images(unit.actor);

            // Refresh UI nodes
            refresh(unit);
        }

        public override void update()
        {
            base.update();
            // Supports
            Supports.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            // Bonuses Window
            Bonuses_Window.draw(sprite_batch, draw_offset);
            Bonus_Bg.draw(sprite_batch, draw_offset);
            // Supports Window
            Supports_Window.draw(sprite_batch, draw_offset);
            Support_Bg.draw(sprite_batch, draw_offset);
            // Window Design //
            Window_Design.draw(sprite_batch, draw_offset);
            // Draw Window Contents //
            // Bonuses Window

            // Supports Window
            Supports.draw(sprite_batch, draw_offset);

            // Draw Window Contents //
            foreach (var node in StatusPageNodes)
                node.Draw(sprite_batch, draw_offset);
        }
    }
}