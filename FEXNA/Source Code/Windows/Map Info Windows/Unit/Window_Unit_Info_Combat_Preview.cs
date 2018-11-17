using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Calculations.Stats;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Windows.Map.Info
{
    class Window_Unit_Info_Combat_Preview : Window_Unit_Info_Burst
    {
        private Item_Icon_Sprite Item, TargetItem;
        private Unit_Info_Hp_Gauge_Damage TargetHpGauge;
        private FE_Text TargetName;
        private Sprite VsLabel;

        public Window_Unit_Info_Combat_Preview() { }

        protected override void initialize_images()
        {
            Window_Width = 56 + 72;
            Window_Height = 32;
            //Window_Width = 56 + 32; //Debug
            //Window_Height = 56;
            //Stats = new Unit_Info_Panel_Combat_Preview(); //Debug
            //Stats.draw_offset = new Vector2(24, 16);

            Item = new Item_Icon_Sprite();
            Item.draw_offset = new Vector2(40, 16);
            TargetItem = new Item_Icon_Sprite();
            TargetItem.draw_offset = new Vector2(56 + 16, 16);

            NAME_LOC = new Vector2(28, 0);
            base.initialize_images();

            TargetName = new FE_Text();
            //TargetName.draw_offset = new Vector2(Window_Width - 24, 32); //Debug
            TargetName.draw_offset = new Vector2(56 + 16, 0);
            TargetName.Font = "FE7_Text_Info";
            TargetName.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_Info");

            VsLabel = new Sprite(Global.Content.Load<Texture2D>(
                @"Graphics/Windowskins/Unit_Info"));
            VsLabel.draw_offset = new Vector2(56, 12);
            VsLabel.src_rect = new Rectangle(0, 64, 16, 8);
        }

        protected override void init_hp_gauge()
        {
            Hp_Gauge = new Unit_Info_Hp_Gauge_Damage();
            Hp_Gauge.draw_offset = new Vector2(0, -16);
            TargetHpGauge = new Unit_Info_Hp_Gauge_Damage();
            TargetHpGauge.draw_offset = new Vector2(56 + 32, -16);
            //Hp_Gauge.gauge_visible = false; //Debug
        }

        #region Refresh
        protected override void draw_images(Game_Unit unit)
        {
            Window_Img.team = unit.team;
            var selected_unit = Global.game_map.get_selected_unit();
            var player_unit = selected_unit;
            bool player_selected = true;
            // Swap units if the selected unit is on an enemy team
            if (player_unit != null &&
                !player_unit.is_player_allied)
            {
                Game_Unit temp = unit;
                unit = player_unit;
                player_unit = temp;
                player_selected = false;
            }

            // Name
            set_name(unit);
            Name.offset = new Vector2(Name.text_width / 2, 0); //Debug

            Item.texture = null;
            var weapon = unit.actor.weapon;
            if (!player_selected && unit.is_on_siege())
                weapon = unit.items[Siege_Engine.SIEGE_INVENTORY_INDEX].to_weapon;
            if (weapon != null)
            {
                string filename = string.Format(@"Graphics/Icons/{0}", weapon.Image_Name);
                if (Global.content_exists(filename))
                    Item.texture = Global.Content.Load<Texture2D>(filename);
                Item.index = weapon.Image_Index;
            }

            // HP
            if (player_unit == null || player_unit == unit ||
                selected_unit.actor.weapon == null || selected_unit.actor.weapon.is_staff())
                //player_unit.actor.weapon == null || player_unit.actor.weapon.is_staff()) //Debug
            {
                Window_Width = 56;
                Window_Img.set_width(Window_Width);
                target_info_visible(false);

                Hp_Gauge.set_val(unit.actor.hp, unit.actor.maxhp);
            }
            else
            {
                Window_Width = 56 + 72;
                Window_Img.set_width(Window_Width);
                target_info_visible(true);

                TargetName.text = player_unit.actor.name;
                TargetName.offset = new Vector2(TargetName.text_width / 2, 0); //Debug
                //TargetName.offset = new Vector2(24, 0);

                TargetItem.texture = null;
                weapon = player_unit.actor.weapon;
                if (player_selected && player_unit.is_on_siege())
                    weapon = player_unit.items[Siege_Engine.SIEGE_INVENTORY_INDEX].to_weapon;
                if (weapon != null)
                {
                    string filename = string.Format(@"Graphics/Icons/{0}", weapon.Image_Name);
                    if (Global.content_exists(filename))
                        TargetItem.texture = Global.Content.Load<Texture2D>(filename);
                    TargetItem.index = weapon.Image_Index;
                }

                if (player_unit.actor.weapon == null || player_unit.actor.weapon.is_staff())
                {
                    (Hp_Gauge as Unit_Info_Hp_Gauge_Damage).set_val(
                        unit.actor.hp, unit.actor.maxhp);
                }
                else
                {
                    var stats = new CombatStats(player_unit.id, unit.id,
                        itemIndex: player_selected && player_unit.is_on_siege() ?
                            Siege_Engine.SIEGE_INVENTORY_INDEX : -1,
                        distance: player_unit.actor.weapon != null ?
                        player_unit.actor.weapon.Min_Range : 1)
                    {
                        location_bonuses = CombatLocationBonuses.NoAttackerBonus
                    };
                    //(Hp_Gauge as Unit_Info_Hp_Gauge_Damage).set_val( //Debug
                    //    unit.actor.hp, unit.actor.maxhp,
                    //    stats.avg_dmg_per_round() / (float)unit.actor.maxhp);
                    (Hp_Gauge as Unit_Info_Hp_Gauge_Damage).set_val(
                        unit.actor.hp, unit.actor.maxhp,
                        stats.inverse_rounds_to_kill());
                }

                if (unit.actor.weapon == null || unit.actor.weapon.is_staff())
                {
                    TargetHpGauge.set_val(unit.actor.hp, unit.actor.maxhp);
                }
                else
                {
                    var target_stats = new CombatStats(unit.id, player_unit.id,
                        itemIndex: !player_selected && unit.is_on_siege() ?
                            Siege_Engine.SIEGE_INVENTORY_INDEX : -1,
                        distance: unit.actor.weapon != null ?
                        unit.actor.weapon.Min_Range : 1)
                    {
                        location_bonuses = CombatLocationBonuses.NoDefenderBonus
                    };
                    //TargetHpGauge.set_val(
                    //    player_unit.actor.hp, player_unit.actor.maxhp,
                    //    target_stats.avg_dmg_per_round() / (float)player_unit.actor.maxhp);
                    TargetHpGauge.set_val(
                        player_unit.actor.hp, player_unit.actor.maxhp,
                        target_stats.inverse_rounds_to_kill());
                }
            }
        }

        private void target_info_visible(bool value)
        {
            VsLabel.visible = value;

            TargetName.visible = value;
            TargetHpGauge.visible = value;
            TargetItem.visible = value;
        }

        protected override void set_name(Game_Unit unit)
        {
            Name.text = unit.actor.name;
            Name.offset = new Vector2(24, 0);
        }

        protected override void refresh()
        {
            base.refresh();
        }
        #endregion

        #region Draw
        public override void draw(SpriteBatch sprite_batch)
        {
            base.draw(sprite_batch);

            if (is_onscreen_for_drawing)
            {
                Vector2 loc = this.loc + draw_vector();
                Item.draw(sprite_batch, -loc);
                TargetItem.draw(sprite_batch, -loc);

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                VsLabel.draw(sprite_batch, -loc);
                sprite_batch.End();
            }
        }

        protected override void draw_hp(SpriteBatch sprite_batch)
        {
            Vector2 loc = this.loc + draw_vector();

            TargetName.draw(sprite_batch, -(NAME_LOC + loc));
            base.draw_hp(sprite_batch);
            TargetHpGauge.draw(sprite_batch, -loc);
        }
        #endregion
    }
}