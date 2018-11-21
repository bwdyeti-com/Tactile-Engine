using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    partial class Scene_Action
    {
        protected bool Level_Up_Spark_Played = false;

        protected override void update_level_up_action_0()
        {
            if (Level_Up_Spark_Played)
            {
                Level_Up_Action = 2;
                Level_Up_Timer = 4;
                return;
            }
            switch (Level_Up_Timer)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                    Level_Up_Timer++;
                    break;
                case 17:
                    level_up_fanfare();
                    Level_Up_Timer++;
                    break;
                case 18:
                    create_levelup_spark(Global.game_map.units[Level_Up_Battler_Ids[0]].pixel_loc);
                    Level_Up_Spark_Played = true;
                    Level_Up_Action++;
                    Level_Up_Timer = 0;
                    break;
            }
        }

        protected override void update_level_up_action_1()
        {
            if (true)//Scene_Type == "Scene_Battle") //Yeti
            {
                switch (Level_Up_Timer)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        Black_Fill.tint = new Color(0, 0, 0, (Level_Up_Timer + 1) * 32);
                        break;
                }
            }
            Level_Up_Timer++;
            base.update_level_up_action_1();
        }

        protected override void update_level_up_action_2()
        {
            switch (Level_Up_Timer)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    Black_Fill.tint = new Color(0, 0, 0, (4 - Level_Up_Timer) * 32);
                    break;
            }
            base.update_level_up_action_2();
        }

        protected override void update_level_up_action_3()
        {
            //Sparring
            if (!Promoting && (Scene_Type == "Scene_Arena" ||
                Scene_Type == "Scene_Battle" || Scene_Type == "Scene_Staff" ||
                Scene_Type == "Scene_Sparring"))
            {
                switch (Level_Up_Timer)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        Black_Fill.tint = new Color(0, 0, 0, (Level_Up_Timer + 1) * 16);
                        break;
                }
            }
            base.update_level_up_action_3();
        }

        protected override void update_level_up_action_7()
        {
            //Sparring
            if (Scene_Type == "Scene_Arena" ||
                Scene_Type == "Scene_Battle" || Scene_Type == "Scene_Staff" ||
                Scene_Type == "Scene_Sparring")
            {
                switch (Level_Up_Timer)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        Black_Fill.tint = new Color(0, 0, 0, (7 - Level_Up_Timer) * 16);
                        break;
                }
            }
            base.update_level_up_action_7();
        }

        protected override void update_promotion_action_0()
        {
            if (Level_Up_Spark_Played)
            {
                Level_Up_Action = 2;
                Level_Up_Timer = 4;
                return;
            }
            switch (Level_Up_Timer)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                    Level_Up_Timer++;
                    break;
                case 17:
                    level_up_fanfare();
                    Level_Up_Timer++;
                    break;
                case 18:
                    create_promotion_spark(Global.game_map.units[Level_Up_Battler_Ids[0]].pixel_loc);
                    Level_Up_Spark_Played = true;
                    Level_Up_Action++;
                    Level_Up_Timer = 0;
                    break;
            }
        }

        protected override void update_promotion_action_2()
        {
            int alpha;
            switch (Level_Up_Timer)
            {
                case 0:
                    Global.game_temp.boss_theme = false; // Not sure if needed but //Yeti
                    Global.Audio.BgmFadeOut(60);
                    Global.Audio.sfx_fade(60);
                    Skip_Fill = new Sprite(battle_content.Load<Texture2D>(@"Graphics/White_Square"));
                    Skip_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                    alpha = (Level_Up_Timer + 1) * 8;
                    Skip_Fill.tint = new Color(alpha, alpha, alpha, alpha);
                    Level_Up_Timer++;
                    break;
                case 31:
                    Skip_Fill.tint = new Color(255, 255, 255, 255);
                    Level_Up_Timer++;
                    break;
                case 39:
                    Level_Up_Action++;
                    Level_Up_Timer = 0;
                    change_to_promotion();
                    Global.scene_change("Scene_Promotion");
                    break;
                default:
                    if (Level_Up_Timer <= 30)
                    {
                        alpha = (Level_Up_Timer + 1) * 8;
                        Skip_Fill.tint = new Color(alpha, alpha, alpha, alpha);
                    }
                    Level_Up_Timer++;
                    break;
            }
        }

        protected virtual void change_to_promotion() { }
    }
}
