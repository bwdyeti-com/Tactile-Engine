using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Windows.Map.Info
{
    class Window_Unit_Info_Detail_Burst : Window_Unit_Info_Burst
    {
        protected Unit_Info_Exp_Gauge Exp_Gauge;
        protected Unit_Info_Panel_Stats Stats;

        public Window_Unit_Info_Detail_Burst() { }

        protected override void initialize_images()
        {
            Window_Width = 120;
            Window_Height = 32;
            Stats = new Unit_Info_Panel_Stats_Detail();
            Stats.offset = -new Vector2(56, 0);
            base.initialize_images();
        }

        protected override void init_hp_gauge()
        {
            Hp_Gauge = new Unit_Info_Hp_Gauge();
            Hp_Gauge.offset = new Vector2(32, 0);
            Hp_Gauge.gauge_visible = false;
            Exp_Gauge = new Unit_Info_Exp_Gauge();
            Exp_Gauge.offset = new Vector2(0, 16);
            Exp_Gauge.gauge_visible = false;
        }

        #region Refresh
        protected override void draw_images(Game_Unit unit)
        {
            Window_Img.team = unit.team;
            // Name
            set_name(unit);
            // HP
            Hp_Gauge.set_val(unit.actor.hp, unit.actor.maxhp);
            Exp_Gauge.set_val(unit.actor.exp, unit.actor.level, !(unit.is_active_player_team && unit.actor.can_level())); //Multi
        }

        protected override void set_name(Game_Unit unit)
        {
            Name.text = unit.actor.name;
            Name.offset = new Vector2(24, 0);
            //Inventory.set_images(unit);
            Stats.set_images(unit);
        }

        protected override void refresh()
        {
            base.refresh();

            refresh_graphic_object(Exp_Gauge);
            refresh_graphic_object(Stats);
        }
        #endregion

        #region Draw
        public override void draw(SpriteBatch sprite_batch)
        {
            base.draw(sprite_batch);

            if (!Offscreen && X_Move_List.Count == 0)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Exp_Gauge.draw(sprite_batch);
                sprite_batch.End();

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Stats.draw(sprite_batch);
                sprite_batch.End();
            }
        }
        #endregion
    }
}