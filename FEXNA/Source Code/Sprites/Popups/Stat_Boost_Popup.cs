using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    class Stat_Boost_Popup : Item_Break_Popup
    {
        Weapon_Type_Icon WLvl_Icon;

        private bool IgnoresInput;

        public Stat_Boost_Popup(int item_id, bool is_item, Game_Unit unit, int time,
            bool ignoresInput = false)
        {
            initialize(item_id, is_item, unit, false);
            Timer_Max = time;
            loc = new Vector2((Config.WINDOW_WIDTH - Width) / 2, 80);
            IgnoresInput = ignoresInput;
        }

        protected void initialize(int item_id, bool is_item, Game_Unit unit, bool battle_scene)
        {
            Timer_Max = 97;
            FEXNA_Library.Data_Equipment item;
            if (is_item)
                item = Global.data_items[item_id];
            else
                item = Global.data_weapons[item_id];
            // Item icon
            set_icon(item);
            // Text
            set_text(item, unit, battle_scene);
            set_window(battle_scene);
        }

        protected void set_text(FEXNA_Library.Data_Equipment item, Game_Unit unit, bool battle_scene)
        {
            int x = 8;
            Icon.loc = new Vector2(x, 8);
            if (((FEXNA_Library.Data_Item)item).Stat_Boost[(int)FEXNA_Library.Boosts.WLvl] > 0 ||
                ((FEXNA_Library.Data_Item)item).Stat_Boost[(int)FEXNA_Library.Boosts.WExp] > 0)
            {
                var weapon = unit.actor.items[unit.actor.equipped - 1].to_weapon;
                x += 16;
                WLvl_Icon = new Weapon_Type_Icon();
                WLvl_Icon.index = unit.actor.valid_weapon_type_of(weapon).IconIndex;
                WLvl_Icon.loc = new Vector2(x, 8);
            }
            x += 17;
            A_Name = new FE_Text();
            A_Name.loc = new Vector2(x, 8);
            A_Name.Font = "FE7_Text";
            A_Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/" +
                (battle_scene ? "FE7_Text_CombatBlue" : "FE7_Text_White"));
            A_Name.text = "" + ((FEXNA_Library.Data_Item)item).Boost_Text;
            string pow = "";
            // Changes POW to relevant
            switch (unit.actor.power_type())
            {
                case Power_Types.Strength:
                    pow = "Strength";
                    break;
                case Power_Types.Magic:
                    pow = "Magic";
                    break;
                case Power_Types.Power:
                    pow = "Power";
                    break;
            }
            A_Name.text = Regex.Replace(A_Name.text, @"POW", pow);
            x += Font_Data.text_width(A_Name.text, "FE7_Text") + 1;
            Broke = new FE_Text();
            Width = x + 8 + (x % 8 != 0 ? (8 - x % 8) : 0);
        }

        public override void update()
        {
            if (!IgnoresInput)
                if (Timer > 0 && this.skip_input)
                    Timer = Timer_Max;
            base.update();
        }

        protected override void draw_image(SpriteBatch sprite_batch)
        {
            base.draw_image(sprite_batch);
            if (WLvl_Icon != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                WLvl_Icon.draw(sprite_batch, -(loc + draw_vector()));
                sprite_batch.End();
            }
        }
    }
}
