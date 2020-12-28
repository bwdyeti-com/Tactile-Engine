using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface.Status;

namespace Tactile.Graphics.Preparations
{
    class Prep_Stats_Window : Graphic_Object
    {
        const float MAX_STAT = 30;
        const int STAT_BAR_WIDTH = 41;
        const int STAT_GAIN_TIME = 120;

        private int Timer = 0, Glow_Timer;
        protected System_Color_Window Stats_Window;
        protected List<StatusPrimaryStatUINode> Stats;
        protected StatusStatUINode PowNode;

        private List<Spark> Swirls = new List<Spark>(), Arrows = new List<Spark>();
        private List<Stat_Up_Num> Stat_Gains = new List<Stat_Up_Num>();

        #region Accessors
        public bool is_ready { get { return Timer == 0; } }
        #endregion

        protected virtual int WIDTH()
        {
            return 144;
        }
        protected virtual int SPACING()
        {
            return 64;
        }
        protected virtual int HEIGHT()
        {
            return 80;
        }

        public Prep_Stats_Window(Game_Unit unit)
        {
            // Stats Window
            initialize_window();

            // Stat Labels
            Stats = new List<StatusPrimaryStatUINode>();
            for (int i = 0; i < 8; i++)
            {
                string label, help_label;

                var stat_label = (Stat_Labels)i;

                Vector2 loc = new Vector2(8 + (i / 4) * SPACING(), 8 + (i % 4) * 16);
                PrimaryStatState.label(stat_label, out label, out help_label);

                Func<Game_Unit, PrimaryStatState> stat_formula = (Game_Unit stat_unit) =>
                {
                    return new PrimaryStatState(stat_unit, stat_label);
                };

                Func<Game_Unit, Color> label_color = null;
                if (Window_Status.show_stat_colors(stat_label))
                {
                    label_color = (Game_Unit color_unit) =>
                    {
                        return color_unit.actor.stat_color(stat_label);
                    };
                }

                Stats.Add(new StatusPrimaryStatUINode(
                    help_label, label, stat_formula, label_color, 40));
                Stats.Last().loc = loc;
                Stats.Last().stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;

                if (stat_label == Stat_Labels.Pow)
                    PowNode = Stats.Last() as StatusStatUINode;
            }

            set_images(unit);
        }

        protected virtual void initialize_window()
        {
            Stats_Window = new System_Color_Window();
            Stats_Window.loc = new Vector2(0, 0);
            Stats_Window.width = WIDTH();
            Stats_Window.height = HEIGHT();
            Stats_Window.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
        }

        public void set_images(Game_Unit unit)
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

            // Refresh UI nodes
            foreach (StatusUINode node in Stats)
            {
                node.refresh(unit);
            }
        }

        public void update()
        {
            if (Timer > 0)
            {
                Glow_Timer++;
                Timer--;
                if (Timer == 0)
                    cancel_stats_gain();
                else
                {
                    foreach (Spark arrow in Arrows)
                        ((Stat_Change_Arrow)arrow).update(Glow_Timer);
                    foreach (Stat_Up_Num stat_up in Stat_Gains)
                        stat_up.update(Glow_Timer);
                    foreach (Spark swirl in Swirls)
                        swirl.update();
                }
            }

            // Stats Window
            Stats_Window.update();
            foreach (var stat in Stats)
                stat.Update();
        }

        public void gain_stats(Dictionary<TactileLibrary.Boosts, int> boosts)
        {
            Glow_Timer = 0;
            Timer = STAT_GAIN_TIME;
            Arrows.Clear();
            Stat_Gains.Clear();
            Swirls.Clear();

            foreach(KeyValuePair<TactileLibrary.Boosts, int> pair in boosts)
            {
                Vector2 loc;
                if (pair.Key == TactileLibrary.Boosts.Con)
                    loc = new Vector2(16 + ((((int)Stat_Labels.Con) / 4) * SPACING()), (((int)Stat_Labels.Con) % 4) * 16);
                else
                    loc = new Vector2(16 + ((((int)pair.Key) / 4) * SPACING()), ((((int)pair.Key) % 4) * 16));

                Stat_Gains.Add(new Quick_Stat_Up_Num(new List<Texture2D> {
                    Global.Content.Load<Texture2D>(@"Graphics/Fonts/" + Config.PROMOTION_STAT_FONT),
                    Global.Content.Load<Texture2D>(@"Graphics/Fonts/" + Config.LEVEL_STAT_FONT) }));
                Stat_Gains[Stat_Gains.Count - 1].value = pair.Value;
                Stat_Gains[Stat_Gains.Count - 1].loc = loc + new Vector2(40, 23);
                Arrows.Add(new Stat_Change_Arrow());
                Arrows[Arrows.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + Stat_Change_Arrow.FILENAME);
                Arrows[Arrows.Count - 1].loc = loc + new Vector2(32, 1);
                ((Stat_Change_Arrow)Arrows[Arrows.Count - 1]).update(0);
                Swirls.Add(new Stat_Up_Spark());
                Swirls[Swirls.Count - 1].loc = loc + new Vector2(-5, -7);
                Swirls[Swirls.Count - 1].update();
            }
        }

        public void cancel_stats_gain()
        {
            Timer = 0;
            Arrows.Clear();
            Stat_Gains.Clear();
            Swirls.Clear();
        }

        public void draw(SpriteBatch sprite_batch)
        {
            // Stats Window
            Stats_Window.draw(sprite_batch, -loc);
            // Draw Window Contents //
            // Stats Window
            foreach (var stat in Stats)
            {
                stat.DrawGaugeBg(sprite_batch, -loc);
            }
            foreach (var stat in Stats)
            {
                stat.Draw(sprite_batch, -loc);
            }

            if (!is_ready)
            {
                foreach (Spark arrow in Arrows)
                    arrow.draw(sprite_batch, -loc);
                foreach (Stat_Up_Num stat_up in Stat_Gains)
                    stat_up.draw(sprite_batch, -loc);
                foreach (Spark swirl in Swirls)
                    swirl.draw(sprite_batch, -loc);
            }
        }
    }
}
