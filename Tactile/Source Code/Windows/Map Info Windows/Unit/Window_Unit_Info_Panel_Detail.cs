using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Windows.Map.Info
{
    class Window_Unit_Info_Panel_Detail : Window_Unit_Info_Panel
    {
        protected Unit_Info_Exp_Gauge Exp_Gauge;
        protected Icon_Sprite Affinity_Icon;
        protected Unit_Info_Inventory Inventory;
        protected Unit_Info_Panel_Stats Stats;

        public Window_Unit_Info_Panel_Detail()
        {
            BOTTOM_Y = Math.Max(Config.WINDOW_HEIGHT / 2,
                Config.WINDOW_HEIGHT - (56 + (Global.game_options.controller == 0 ? 16 : 0)));
            NAME_LOC = new Vector2(32, 16);
            Window_Width = 128;
            Window_Height = 48;
            initialize();
            loc = new Vector2(-(Window_Width + 8), TOP_Y);
            refresh();
        }

        protected override void initialize_images()
        {
            Affinity_Icon = new Icon_Sprite();
            Affinity_Icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Affinity Icons");
            Affinity_Icon.size = new Vector2(16, 16);
            Inventory = new Unit_Info_Inventory();
            Inventory.offset = -new Vector2(32, 0);
            Stats = new Unit_Info_Panel_Stats();
            Stats.offset = -new Vector2(96, 16);
            base.initialize_images();
        }
        
        protected override void init_hp_gauge()
        {
            Hp_Gauge = new Unit_Detail_Info_Hp_Gauge();
            Exp_Gauge = new Unit_Info_Exp_Gauge();
        }

        #region Refresh
        protected override void draw_images(Game_Unit unit)
        {
            base.draw_images(unit);
            if (unit.is_active_player_team) //Multi
                Affinity_Icon.index = (int)unit.actor.affin;
            else
                Affinity_Icon.index = -1;
            if (unit.is_active_player_team)
            {
                Exp_Gauge = new Unit_Info_Exp_Gauge();
                Exp_Gauge.set_val(unit.actor.exp, unit.actor.level, !unit.actor.can_level());
            }
            else
            {
                Exp_Gauge = new Unit_Info_Skill_Gauge();
                ((Unit_Info_Skill_Gauge)Exp_Gauge).set_val(unit.highest_mastery_charge_percent(), unit.actor.level, string.IsNullOrEmpty(unit.has_any_mastery()));
            }
            //Exp_Gauge.set_val(unit.actor.exp, unit.actor.level, !(unit.is_active_player_team && unit.actor.can_level())); //Multi //Debug
        }

        protected override void set_name(Game_Unit unit)
        {
            Name.text = unit.actor.name;
            Name.offset = new Vector2(unit.is_active_player_team ? -16 : 0, 0); //Multi
            Inventory.set_images(unit);
            Stats.set_images(unit);
        }

        protected override void refresh()
        {
            refresh_graphic_object(Affinity_Icon);
            refresh_graphic_object(Exp_Gauge);
            refresh_graphic_object(Inventory);
            refresh_graphic_object(Stats);

            base.refresh();
        }
        #endregion

        public override void update()
        {
            base.update();
            Inventory.update();
        }

        #region Movement
        protected override void move_off(bool also_y_move)
        {
            if (Offscreen) return;
            X_Move_List = new List<int> { -(Window_Width + 8), LEFT_X - 40, LEFT_X - 8, LEFT_X };
            if (!also_y_move)
            {
                for (int i = 0; i < Y_Move_List.Count - 1; i++)
                    X_Move_List.Add(-(Window_Width + 8)); //should this be 5, it seems odd //Yeti
            }
            Offscreen = true;
        }

        protected override void move_on()
        {
            Offscreen = false;
            X_Move_List = new List<int> { LEFT_X, LEFT_X - 8, LEFT_X - 24, LEFT_X - 56 };
            for (int i = 0; i < Y_Move_List.Count - 1; i++)
                X_Move_List.Add(-(Window_Width + 8));
        }
        #endregion

        #region Draw
        public override void draw(SpriteBatch sprite_batch)
        {
            base.draw(sprite_batch);
            if (!(Offscreen && Y_Move_List.Count == 0 && X_Move_List.Count == 0))
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Affinity_Icon.draw(sprite_batch, -new Vector2(32, 16));
                Exp_Gauge.draw(sprite_batch);
                sprite_batch.End();

                Inventory.draw(sprite_batch);

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Stats.draw(sprite_batch);
                sprite_batch.End();
            }
        }
        #endregion
    }
}
