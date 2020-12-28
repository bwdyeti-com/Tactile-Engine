using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Calculations.Stats;
using Tactile.Graphics.Text;
using TactileWeaponExtension;
using TactileLibrary;

namespace Tactile.Windows.Map.Info
{
    class Window_Unit_Info_Combat_Preview : Window_Unit_Info_Burst
    {
        private Item_Icon_Sprite Item, TargetItem;
        private Effective_WT_Arrow WTA1, WTA2;
        private Unit_Info_Hp_Gauge_Damage TargetHpGauge;
        private TextSprite TargetName;
        private Sprite VsLabel;

        public Window_Unit_Info_Combat_Preview() { }

        protected override void initialize_images()
        {
            WTA1 = new Effective_WT_Arrow();
            WTA2 = new Effective_WT_Arrow();

            WTA1.draw_offset = new Vector2(40, 16);
            WTA2.draw_offset = new Vector2(56 + 16, 16);

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

            TargetName = new TextSprite();
            //TargetName.draw_offset = new Vector2(Window_Width - 24, 32); //Debug
            TargetName.draw_offset = new Vector2(56 + 16, 0);
            TargetName.SetFont(Config.INFO_FONT, Global.Content);

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

        public override void update()
        {
            base.update();

            WTA1.update();
            WTA2.update();
            Item.update();
            TargetItem.update();
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

            Item.flash = false;
            TargetItem.flash = false;

            // Name
            set_name(unit);
            Name.offset = new Vector2(Name.text_width / 2, 0); //Debug

            Weapon_Triangle_Arrow.ResetWeaponTriangle(WTA1, WTA2);

            Item.texture = null;
            var unitWeapon = unit.actor.weapon;
            if (!player_selected && unit.is_on_siege())
                unitWeapon = unit.items[Siege_Engine.SiegeInventoryIndex].to_weapon;
            if (unitWeapon != null)
            {
                string filename = string.Format(@"Graphics/Icons/{0}", unitWeapon.Image_Name);
                if (Global.content_exists(filename))
                    Item.texture = Global.Content.Load<Texture2D>(filename);
                Item.index = unitWeapon.Image_Index;
            }

            var playerUnitWeapon = player_unit.actor.weapon;
            if (player_selected && player_unit.is_on_siege())
                playerUnitWeapon = player_unit.items[Siege_Engine.SiegeInventoryIndex].to_weapon;

            var selectedUnitWeapon = player_selected ? playerUnitWeapon : unitWeapon;

            // HP
            if (player_unit == null || player_unit == unit ||
                selectedUnitWeapon == null || selectedUnitWeapon.is_staff())
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
                if (playerUnitWeapon != null)
                {
                    string filename = string.Format(@"Graphics/Icons/{0}", playerUnitWeapon.Image_Name);
                    if (Global.content_exists(filename))
                        TargetItem.texture = Global.Content.Load<Texture2D>(filename);
                    TargetItem.index = playerUnitWeapon.Image_Index;
                }
                
                // Player unit has no weapon
                if (playerUnitWeapon == null || playerUnitWeapon.is_staff())
                {
                    (Hp_Gauge as Unit_Info_Hp_Gauge_Damage).set_val(
                        unit.actor.hp, unit.actor.maxhp);
                }
                else
                {
                    int distance = playerUnitWeapon != null ?
                        playerUnitWeapon.Min_Range : 1;
                    var stats = new CombatStats(player_unit.id, unit.id,
                        itemIndex: player_selected && player_unit.is_on_siege() ?
                            Siege_Engine.SiegeInventoryIndex : -1,
                        distance: distance)
                    {
                        location_bonuses = CombatLocationBonuses.NoAttackerBonus
                    };
                    (Hp_Gauge as Unit_Info_Hp_Gauge_Damage).set_val(
                        unit.actor.hp, unit.actor.maxhp,
                        stats.inverse_rounds_to_kill());

                    Weapon_Triangle_Arrow.SetWeaponTriangle(
                        WTA2,
                        player_unit,
                        unit,
                        playerUnitWeapon,
                        unitWeapon,
                        distance);
                    if (playerUnitWeapon.effective_multiplier(player_unit, unit) > 1)
                        TargetItem.flash = true;
                } 

                // Target has no weapon
                if (unitWeapon == null || unitWeapon.is_staff())
                {
                    TargetHpGauge.set_val(unit.actor.hp, unit.actor.maxhp);
                }
                else
                {
                    int distance = unitWeapon != null ?
                        unitWeapon.Min_Range : 1;
                    var target_stats = new CombatStats(unit.id, player_unit.id,
                        itemIndex: !player_selected && unit.is_on_siege() ?
                            Siege_Engine.SiegeInventoryIndex : -1,
                        distance: distance)
                    {
                        location_bonuses = CombatLocationBonuses.NoDefenderBonus
                    };
                    TargetHpGauge.set_val(
                        player_unit.actor.hp, player_unit.actor.maxhp,
                        target_stats.inverse_rounds_to_kill());

                    Weapon_Triangle_Arrow.SetWeaponTriangle(
                        WTA1,
                        unit,
                        player_unit,
                        unitWeapon,
                        playerUnitWeapon,
                        distance);
                    if (unitWeapon.effective_multiplier(unit, player_unit) > 1)
                        Item.flash = true;
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
                WTA1.draw(sprite_batch, -loc);
                WTA2.draw(sprite_batch, -loc);

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