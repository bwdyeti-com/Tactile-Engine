using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    internal class Scene_Level_Up : Scene_Base
    {
        protected bool Level_Up_Calling = false;
        protected bool Leveling_Up = false;
        protected int Level_Up_Action = 0;
        protected int Level_Up_Timer = 0;
        protected List<int> Level_Up_Battler_Ids = new List<int>();
        protected bool Promoting = false;
        protected int Promotion_Old_Class_Id;
        private Spark LevelUp_Spark;
        protected Window_LevelUp Level_Window;

        #region Accessors
        internal List<int> level_up_battler_ids { get { return Level_Up_Battler_Ids; } }

        internal bool levelup_spark_active { get { return LevelUp_Spark != null; } }
        #endregion

        public void level_up(int id)
        {
            if (!is_leveling_up())
            {
                Level_Up_Battler_Ids = new List<int> { id };
                Level_Up_Calling = true;
            }
        }

        public void promote(int id, int old_class_id)
        {
            if (!is_leveling_up())
            {
                Level_Up_Battler_Ids = new List<int> { id };
                Level_Up_Calling = true;
                Promoting = true;
                Promotion_Old_Class_Id = old_class_id;
            }
        }
        public void promote(List<int> ids, int old_class_id)
        {
            if (!is_leveling_up())
            {
                Level_Up_Battler_Ids = ids;
                Level_Up_Calling = true;
                Promoting = true;
                Promotion_Old_Class_Id = old_class_id;
            }
        }

        public bool is_leveling_up()
        {
            return Level_Up_Calling || Leveling_Up;
        }

        public virtual void create_levelup_spark(Vector2 loc)
        {
            LevelUp_Spark = new LevelUp_Map_Spark();
            LevelUp_Spark.texture = level_up_content().Load<Texture2D>(@"Graphics/Pictures/" + LevelUp_Map_Spark.FILENAME);
            loc.X -= 32;
            // Should figure this out better // Seems pretty good now, even if I"d prefer map.min_scroll_x not be public //Yeti
            loc.X = MathHelper.Clamp(loc.X, Global.game_map.min_scroll_x * 16,
                Config.WINDOW_WIDTH + (Global.game_map.max_scroll_x * 16) - 80);
            //loc.X = MathHelper.Clamp(loc.X, Global.game_map.map_edge_offset_x1 * 16,
            //    (Global.game_map.width() - Global.game_map.map_edge_offset_x2) * 16 - 80);
            if (loc.Y < 32)
                loc.Y += 16;
            else
                loc.Y -= 16;
            LevelUp_Spark.loc = loc;
        }
        protected void create_action_levelup_spark(Vector2 loc)
        {
            LevelUp_Spark = new LevelUp_Battle_Spark(level_up_content());
            LevelUp_Spark.loc = new Vector2(0, 6);
        }

        public virtual void create_promotion_spark(Vector2 loc)
        {
            LevelUp_Spark = new ClassChange_Map_Spark();
            LevelUp_Spark.texture = level_up_content().Load<Texture2D>(@"Graphics/Pictures/" + ClassChange_Map_Spark.FILENAME);
            loc.X -= 56;
            // Should figure this out better // Seems pretty good now, even if I"d prefer map.min_scroll_x not be public //Yeti
            loc.X = MathHelper.Clamp(loc.X, Global.game_map.min_scroll_x * 16,
                Config.WINDOW_WIDTH + (Global.game_map.max_scroll_x * 16) - 120);
            //loc.X = MathHelper.Clamp(loc.X, Global.game_map.map_edge_offset_x1 * 16,
            //    (Global.game_map.width() - Global.game_map.map_edge_offset_x2) * 16 - 80);
            if (loc.Y < 32)
                loc.Y += 16;
            else
                loc.Y -= 16;
            LevelUp_Spark.loc = loc;
        }
        protected void create_action_promotion_spark(Vector2 loc)
        {
            LevelUp_Spark = new ClassChange_Battle_Spark(level_up_content());
            LevelUp_Spark.loc = new Vector2(0, 6);
        }

        protected void create_levelup_window()
        {
            Level_Window = new Window_LevelUp(level_up_content(), Global.game_map.units[Level_Up_Battler_Ids[0]].actor.id);
        }

        protected void reset()
        {
            Level_Up_Calling = false;
            Leveling_Up = false;
            Level_Up_Action = 0;
            Level_Up_Timer = 0;
            Level_Up_Battler_Ids.Clear();;
        }

        protected virtual void clear_graphic_objects()
        {
            LevelUp_Spark = null;
            Level_Window = null;
        }

        public override void update()
        {
            if (Level_Up_Calling)
            {
                Level_Up_Calling = false;
                Leveling_Up = true;
            }
            if (Leveling_Up)
            {
                if (Promoting)
                    update_promotion();
                else
                    update_level_up();
            }
            if (levelup_spark_active)
            {
                LevelUp_Spark.update();
                if (LevelUp_Spark.completed())
                    LevelUp_Spark = null;
            }
            if (Level_Window != null)
            {
                Level_Window.update();
            }
        }

        #region Level Up
        protected void update_level_up()
        {
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Level_Up_Action)
                {
                    case 0:
                        update_level_up_action_0();
                        break;
                    case 1:
                        update_level_up_action_1();
                        break;
                    case 2:
                        update_level_up_action_2();
                        break;
                    case 3:
                        update_level_up_action_3();
                        break;
                    case 4:
                        switch (Level_Up_Timer)
                        {
                            case 31:
                                Level_Up_Action++;
                                Level_Up_Timer = 0;
                                Level_Window.execute = true;
                                break;
                            default:
                                Level_Up_Timer++;
                                break;
                        }
                        break;
                    case 5:
                        if (Level_Window.is_ready())
                        {
                            Level_Window.finish();
                            Level_Up_Action++;
                            Level_Up_Timer = 0;
                        }
                        break;
                    case 6:
                        switch (Level_Up_Timer)
                        {
                            case 58:
                                Level_Up_Action++;
                                Level_Up_Timer = 0;
                                break;
                            default:
                                Level_Up_Timer++;
                                break;
                        }
                        break;
                    case 7:
                        update_level_up_action_7();
                        break;
                    case 8:
                        Leveling_Up = false;
                        Level_Up_Action = 0;
                        Level_Up_Timer = 0;
                        Level_Up_Battler_Ids.RemoveAt(0);
                        if (Level_Window != null)
                            Level_Window = null;
                        break;
                }
            }
        }

        protected virtual void update_level_up_action_0()
        {
            level_up_fanfare();
            create_levelup_spark(Global.game_map.units[Level_Up_Battler_Ids[0]].pixel_loc);
            Level_Up_Action++;
        }

        protected virtual void update_level_up_action_1()
        {
            if (!levelup_spark_active)
            {
                Level_Up_Action++;
                Level_Up_Timer = 0;
            }
        }

        protected virtual void update_level_up_action_2()
        {
            switch (Level_Up_Timer)
            {
                case 18:
                    Level_Up_Action++;
                    Level_Up_Timer = 0;
                    create_levelup_window();
                    break;
                default:
                    Level_Up_Timer++;
                    break;
            }
        }

        protected virtual void update_level_up_action_3()
        {
            switch (Level_Up_Timer)
            {
                case 0:
                    Level_Window.move_on();
                    Level_Up_Timer++;
                    break;
                case 12:
                    Level_Up_Action++;
                    Level_Up_Timer = 0;
                    break;
                default:
                    Level_Up_Timer++;
                    break;
            }
        }

        protected virtual void update_level_up_action_7()
        {
            switch (Level_Up_Timer)
            {
                case 0:
                    Level_Window.move_off();
                    Level_Up_Timer++;
                    break;
                case 11:
                    Level_Up_Action++;
                    Level_Up_Timer = 0;
                    break;
                default:
                    Level_Up_Timer++;
                    break;
            }
        }
        #endregion

        #region Promotion
        protected virtual void update_promotion()
        {
            // Overwritten by Scene_Battle_Level_Up for the actual level up, this handles the part in the battle scene
            bool cont = false;
            while (!cont)
            {
                cont = true;
                switch (Level_Up_Action)
                {
                    case 0:
                        update_promotion_action_0();
                        break;
                    case 1:
                        update_level_up_action_1();
                        break;
                    case 2:
                        update_promotion_action_2();
                        break;
                }
            }
        }

        protected virtual void update_promotion_action_0()
        {
            level_up_fanfare();
            create_promotion_spark(Global.game_map.units[Level_Up_Battler_Ids[0]].pixel_loc);
            Level_Up_Action++;
        }

        protected virtual void update_promotion_action_2()
        {
            switch (Level_Up_Timer)
            {
                case 40:
                    Level_Up_Action++;
                    Level_Up_Timer = 0;
                    Global.scene_change("Scene_Promotion");
                    break;
                default:
                    Level_Up_Timer++;
                    break;
            }
        }
        #endregion

        protected void level_up_fanfare()
        {
            Global.Audio.play_se("System Sounds", "Level_Up_Fanfare", duckBgm: true);
        }

        protected virtual ContentManager level_up_content()
        {
            return null;
        }

        protected virtual void draw_level_up(SpriteBatch sprite_batch, Vector2 offset)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (levelup_spark_active)
                LevelUp_Spark.draw(sprite_batch, offset);
            sprite_batch.End();
            if (Level_Window != null)
                Level_Window.draw(sprite_batch);
        }
    }
}