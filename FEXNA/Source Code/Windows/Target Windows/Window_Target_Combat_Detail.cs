using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Windows.Target
{
    class Window_Target_Combat_Detail : Window_Target_Combat
    {
        const int ROWS = 6;

        Sprite Terrain_Label;

        public Window_Target_Combat_Detail(
            int unit_id, int item_index, Vector2 loc, string skill,
            Maybe<Vector2> initialTargetLoc = default(Maybe<Vector2>))
                : base(unit_id, item_index, loc, skill, initialTargetLoc) { }

        protected override int window_rows
        {
            get
            {
                if (this.skills_visible)
                    return ROWS;
                return ROWS - 1;
            }
        }
        protected new int stat_rows { get { return this.window_rows - 1; } }

        private bool skills_visible
        {
            get
            {
                var unit = get_unit();
                return this.targets.Any(target_id =>
                {
                    var target = Global.game_map.attackable_map_object(target_id);
                    if (!target.is_unit())
                        return false;
                    var target_unit = target as Game_Unit;

                    return unit.has_any_mastery() != null ||
                        target_unit.has_any_mastery() != null ||
                        unit.shown_skill_rate(target_unit) != null ||
                        target_unit.shown_skill_rate(unit) != null;
                });
            }
        }

        protected override void initialize_images()
        {
            // Terrain Label
            Terrain_Label = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Terrain_Info"));
            Terrain_Label.src_rect = new Rectangle(16, 212, 16, 15);
            Terrain_Label.offset = new Vector2(-29, -(4 + (window_rows * LINE_HEIGHT)));

            base.initialize_images();

            for (int i = base.stat_rows; i < stat_rows; i++)
            {
                Stat_Labels.Add(new FE_Text());
                Stat_Labels[i].offset = new Vector2(-28, -(4 + ((i + 1) * LINE_HEIGHT)));
                Stat_Labels[i].Font = "FE7_Text";
                Stat_Labels[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            }
            if (this.skills_visible)
            {
                Stat_Labels[4].offset.X += 1;
                Stat_Labels[4].text = "Skill";
            }
        }

        protected override void set_images()
        {
            base.set_images();

            Game_Unit unit = get_unit();
            Combat_Map_Object target = Global.game_map.attackable_map_object(this.target);
            Game_Unit target_unit = null;
            bool is_target_unit = false;
            if (target.is_unit())
            {
                is_target_unit = true;
                target_unit = (Game_Unit)target;
            }
            int distance = Global.game_map.combat_distance(unit.id, this.target);
            // Get weapon data
            FEXNA_Library.Data_Weapon weapon1 = unit.actor.weapon, weapon2 = null;
            if (is_target_unit)
                weapon2 = target_unit.actor.weapon;

            List<int?> combat_stats = Combat.combat_stats(unit.id, this.target, distance);

            if (this.skills_visible)
            {
                for (int i = base.stat_rows * 2; i < stat_rows * 2; i++)
                {
                    if (is_target_unit &&
                        combat_stats[(i % 2) * 4 + 3] == null &&
                        (i % 2 == 0 ? unit.shown_skill_rate(target_unit) : target_unit.shown_skill_rate(unit)) == null &&
                        (i % 2 == 0 ? unit : target_unit).has_any_mastery() != null)
                    {
                        Mastery_Gauge gauge = new Mastery_Gauge((i % 2 == 0 ? unit : target_unit).any_mastery_charge_percent());
                        gauge.height = 4;
                        Stats.Add(gauge);
                        gauge.draw_offset = new Vector2(
                            48 + 4 - (48 * (i % 2)),
                            0 + ((i / 2 + 1) * LINE_HEIGHT));
                    }
                    else
                    {
                        FE_Text_Int text = new FE_Text_Int();
                        Stats.Add(text);
                        text.draw_offset = new Vector2(68 - (48 * (i % 2)), 4 + ((i / 2 + 1) * LINE_HEIGHT));
                        text.Font = "FE7_Text";
                        text.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
                        text.text = "0";
                    }
                }

                // Sets first units's stats //
                // Skill
                if (!(is_target_unit && combat_stats[3] == null && unit.shown_skill_rate(target_unit) == null && unit.has_any_mastery() != null))
                    ((FE_Text_Int)Stats[base.stat_rows * 2]).text = combat_stats[3] == null ? "--" : combat_stats[3].ToString();

                // Sets second units's stats //
                if (!is_target_unit || !(combat_stats[7] == null && target_unit.shown_skill_rate(unit) == null && target_unit.has_any_mastery() != null))
                {
                    // Skill
                    if (is_target_unit && can_counter(unit, target_unit, weapon1, distance))
                        ((FE_Text_Int)Stats[base.stat_rows * 2 + 1]).text = combat_stats[7] == null ? "--" : combat_stats[7].ToString();
                    else
                        ((FE_Text_Int)Stats[base.stat_rows * 2 + 1]).text = "--";
                }
            }

            for (int i = 0; i < 4; i++)
            {
                FE_Text_Int text = new FE_Text_Int();
                Stats.Add(text);
                text.offset = new Vector2(-(69 - (48 * (i / 2))),
                    -(5 + ((i % 2 + 1) * (LINE_HEIGHT / 2)) + ((Window.rows - 1) * LINE_HEIGHT)));
                text.Font = "FE7_TextS";
                text.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
                text.text = "--";
            }
            if (is_target_unit)
            {
                // Terrain def
                bool magic2 = weapon2 == null ? false : target_unit.check_magic_attack(weapon2, distance);
                var def = magic2 ? unit.terrain_res_bonus(target_unit) : unit.terrain_def_bonus(target_unit);
                ((FE_Text_Int)Stats[stat_rows * 2]).text = def.IsNothing ? "--" : def.ToString();
                // Terrain avo
                var avo = unit.terrain_avo_bonus(target_unit, magic2);
                ((FE_Text_Int)Stats[stat_rows * 2 + 1]).text = avo.IsNothing ? "--" : avo.ToString();
            }
            if (is_target_unit)
            {
                // Terrain def
                bool magic1 = unit.check_magic_attack(weapon1, distance);
                var def = magic1 ? target_unit.terrain_res_bonus(unit) : target_unit.terrain_def_bonus(unit);
                ((FE_Text_Int)Stats[stat_rows * 2 + 2]).text = def.IsNothing ? "--" : def.ToString();
                // Terrain avo
                var avo = target_unit.terrain_avo_bonus(unit, magic1);
                ((FE_Text_Int)Stats[stat_rows * 2 + 3]).text = avo.IsNothing ? "--" : avo.ToString();
            }
            refresh();
        }

        protected override void refresh()
        {
            base.refresh();
            //Terrain_Label.loc = Loc;
        }

        protected override void draw_data(SpriteBatch sprite_batch)
        {
            Terrain_Label.draw(sprite_batch, -Loc);
            base.draw_data(sprite_batch);
        }
    }
}
