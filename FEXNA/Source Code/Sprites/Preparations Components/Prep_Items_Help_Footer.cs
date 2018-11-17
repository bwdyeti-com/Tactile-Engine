using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Graphics.Preparations
{
    class Prep_Items_Help_Footer : Sprite
    {
        private Weapon_Type_Icon Type_Icon;
        private List<FE_Text> Stat_Labels = new List<FE_Text>();
        private List<FE_Text> Stats = new List<FE_Text>();

        private bool ShowingDescription = true;
        private int DescriptionLength;
        private Vector2 DescriptionOffset = Vector2.Zero;
        private int DescriptionScrollWait;
        private FE_Text QuickDescrip;

        internal Prep_Items_Help_Footer()
        {
            texture = Global.Content.Load<Texture2D>("Graphics/White_Square");
            tint = new Color(0, 0, 0, 128);
            scale = new Vector2(Config.WINDOW_WIDTH / (float)texture.Width,
                16f / texture.Height);

            Type_Icon = new Weapon_Type_Icon();
            Type_Icon.loc = new Vector2(16, 0);

            for (int i = 0; i < 4; i++)
            {
                Stat_Labels.Add(new FE_Text());
                Stat_Labels[i].loc = new Vector2(56 + i * 56, 0);
                Stat_Labels[i].Font = "FE7_Text";
                Stat_Labels[i].texture = Global.Content.Load<Texture2D>(
                    @"Graphics/Fonts/FE7_Text_Yellow");
            }
            Stat_Labels[0].text = "Atk";
            Stat_Labels[1].text = "Crit";
            Stat_Labels[2].text = "Hit";
            Stat_Labels[3].text = "AS";

            Stats.Add(new FE_Text());
            Stats[0].loc = new Vector2(32, 0);
            Stats[0].Font = "FE7_TextL";
            Stats[0].texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_Blue");
            for (int i = 1; i < 5; i++)
            {
                Stats.Add(new FE_Text_Int());
                Stats[i].loc = new Vector2(40 + i * 56, 0);
                Stats[i].Font = "FE7_Text";
                Stats[i].texture = Global.Content.Load<Texture2D>(
                    @"Graphics/Fonts/FE7_Text_Blue");
            }

            QuickDescrip = new FE_Text();
            QuickDescrip.loc = new Vector2(16, 0);
            QuickDescrip.Font = "FE7_Text";
            QuickDescrip.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
        }

        public void refresh(Game_Unit unit, Item_Data itemData)
        {
            DescriptionOffset = Vector2.Zero;
            DescriptionScrollWait = 120;
            ShowingDescription = true;
            if (unit == null || itemData == null || itemData.non_equipment)
            {
                QuickDescrip.text = "";
            }
            else
            {
                var item = itemData.to_equipment;
                if (item.is_weapon && !(item as Data_Weapon).is_staff())
                {
                    var weapon = (item as Data_Weapon);
                    Type_Icon.index = weapon.main_type().IconIndex;
                    Stats[0].Font = weapon.Rank == Weapon_Ranks.None ? "FE7_Text" : "FE7_TextL";
                    Stats[0].text = weapon.rank;

                    var stats = new Calculations.Stats.BattlerStats(unit.id, weapon.Id);

                    Stats[1].text = stats.dmg().ToString();
                    Stats[2].text = stats.crt().ToString();
                    Stats[3].text = stats.hit().ToString();
                    Stats[4].text = unit.atk_spd(1, itemData).ToString();

                    refresh_equippable(unit, weapon);

                    ShowingDescription = false;
                }
                else
                {
                    QuickDescrip.text = string.Join(" ", item.Quick_Desc.Split('\n'));
                }
            }
            DescriptionLength = QuickDescrip.text_width;
        }

        private void refresh_equippable(Game_Unit unit, Data_Weapon weapon)
        {
            string color = unit.actor.is_equippable(weapon) ? "Blue" : "Grey";
            foreach(var stat in Stats)
            {
                stat.texture = Global.Content.Load<Texture2D>(
                    string.Format(@"Graphics/Fonts/FE7_Text_{0}", color));
            }
        }

        private bool description_too_long
        {
            get { return description_repeat_spacing > Config.WINDOW_WIDTH - 64; }
        }

        private int description_repeat_spacing { get { return DescriptionLength + 16; } }

        public override void update()
        {
            base.update();

            if (description_too_long)
            {
                if (DescriptionScrollWait > 0)
                    DescriptionScrollWait--;
                else
                {
                    DescriptionOffset.X += 1;
                    if (DescriptionOffset.X >= description_repeat_spacing + 4)
                        DescriptionOffset.X -= description_repeat_spacing;
                }
            }
        }

        public override void draw(SpriteBatch sprite_batch, Texture2D texture, Vector2 draw_offset = default(Vector2))
        {
            base.draw(sprite_batch, texture, draw_offset);
            if (ShowingDescription)
            {
                if (description_too_long)
                {
                    QuickDescrip.draw(sprite_batch,
                        draw_offset + DescriptionOffset - (this.loc + draw_vector()));
                    QuickDescrip.draw(sprite_batch,
                        draw_offset + DescriptionOffset -
                        (this.loc + draw_vector() +
                        new Vector2(description_repeat_spacing, 0)));
                    QuickDescrip.draw(sprite_batch,
                        draw_offset + DescriptionOffset -
                        (this.loc + draw_vector() +
                        new Vector2(description_repeat_spacing * 2, 0)));
                }
                else
                    QuickDescrip.draw(sprite_batch,
                        draw_offset - (this.loc + draw_vector()));
            }
            else
            {
                Type_Icon.draw(sprite_batch,
                    draw_offset - (this.loc + draw_vector()));
                foreach (var label in Stat_Labels)
                    label.draw(sprite_batch,
                        draw_offset - (this.loc + draw_vector()));
                foreach (var stat in Stats)
                    stat.draw(sprite_batch,
                        draw_offset - (this.loc + draw_vector()));
            }
        }
    }
}
