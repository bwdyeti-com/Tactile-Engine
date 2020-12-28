using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile.Graphics.Preparations
{
    class Prep_Items_Help_Footer : Sprite
    {
        private Weapon_Type_Icon Type_Icon;
        private List<TextSprite> Stat_Labels = new List<TextSprite>();
        private List<TextSprite> Stats = new List<TextSprite>();

        private bool ShowingDescription = true;
        private int DescriptionLength;
        private Vector2 DescriptionOffset = Vector2.Zero;
        private int DescriptionScrollWait;
        private TextSprite QuickDescrip;

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
                Stat_Labels.Add(new TextSprite());
                Stat_Labels[i].loc = new Vector2(56 + i * 56, 0);
                Stat_Labels[i].SetFont(Config.UI_FONT, Global.Content, "Yellow");
            }
            Stat_Labels[0].text = "Atk";
            Stat_Labels[1].text = "Crit";
            Stat_Labels[2].text = "Hit";
            Stat_Labels[3].text = "AS";

            Stats.Add(new TextSprite());
            Stats[0].loc = new Vector2(32, 0);
            Stats[0].SetFont(Config.UI_FONT + "L", Global.Content, "Blue", Config.UI_FONT);
            for (int i = 1; i < 5; i++)
            {
                Stats.Add(new RightAdjustedText());
                Stats[i].loc = new Vector2(40 + i * 56, 0);
                Stats[i].SetFont(Config.UI_FONT, Global.Content, "Blue");
            }

            QuickDescrip = new TextSprite();
            QuickDescrip.loc = new Vector2(16, 0);
            QuickDescrip.SetFont(Config.UI_FONT, Global.Content, "White");
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
                    Stats[0].SetFont(
                        weapon.Rank == Weapon_Ranks.None ? Config.UI_FONT : Config.UI_FONT + "L",
                        Config.UI_FONT);
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
                stat.SetColor(Global.Content, color);
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
