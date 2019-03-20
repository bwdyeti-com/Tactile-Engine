using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Window_Promotion : Window_LevelUp
    {
        protected int Old_Class_Id;
        protected string Old_Class_Name;
        internal static List<int> Promotion_Stats = new List<int>();

        public Window_Promotion(ContentManager content, int id, int old_class_id)
        {
            Content = content;
            Actor_Id = id;
            Old_Class_Id = old_class_id;
            Old_Class_Name = Global.data_classes[Old_Class_Id].Name;
            initialize();
        }


        protected override void refresh_stat_label_color(int i) { }

        protected override int stat_value(int i)
        {
            return Promotion_Stats[i];
        }

        protected override void gain_stat(Stat_Labels stat)
        {
            Promotion_Stats[(int)stat] = actor.stat(stat);
        }

        protected override void get_stats()
        {
            LevelUp = actor.promotion(Old_Class_Id);
            Old_Class_Name = actor.class_name;
            actor.promotion_reset_level();
        }

        protected override void set_class_name()
        {
            set_class_name(Old_Class_Name);
        }

        protected override void update_skip()
        {
            if (OnLevelGain)
                return;
            base.update_skip();
        }

        protected override bool update_stat()
        {
            if (OnLevelGain)
            {
                switch (Timer)
                {
                    case 0:
                        Global.Audio.play_se("System Sounds", "Level_Up_Level");
                        Header_Scale.Y = (9 - Timer) / 10f;
                        Timer++;
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        Header_Scale.Y = (9 - Timer) / 10f;
                        Timer++;
                        break;
                    case 10:
                        get_stats();
                        refresh();
                        Header_Scale.Y = (Timer - 9) / 10f;
                        Timer++;
                        break;
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                        Header_Scale.Y = (Timer - 9) / 10f;
                        Timer++;
                        break;
                    case 20:
                        Header_Scale.Y = 1;
                        Timer++;
                        break;
                    case 38:
                        get_next_stat();
                        Timer = 0;
                        break;
                    default:
                        Timer++;
                        break;
                }
                return false;
            }
            else
                return base.update_stat();
        }
    }
}
