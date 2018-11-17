using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA
{
    class Help_Weapon_Data : Graphic_Object
    {
        protected List<FE_Text> Labels = new List<FE_Text>();
        protected FE_Text Rank;
        protected List<FE_Text> Stats = new List<FE_Text>(), Stat_Bonuses = new List<FE_Text>();
        protected List<Icon_Sprite> Effectiveness_Icons = new List<Icon_Sprite>();
        protected List<Effective_WT_Arrow> Effectiveness_Multipliers = new List<Effective_WT_Arrow>();

        public Help_Weapon_Data(Item_Data item_data, Game_Actor actor)
        {
            initialize(item_data, actor);
        }

        protected void initialize(Item_Data item_data, Game_Actor actor)
        {
            Data_Weapon weapon = item_data.to_weapon;
            int stats = !weapon.is_staff() ? 6 : 3;
            bool effective = false;
            foreach(int bonus in weapon.Effectiveness)
                if (bonus != 1)
                {
                    effective = true;
                    stats++;
                    break;
                }
            for (int i = 0; i < stats; i++)
            {
                Labels.Add(new FE_Text());
                Labels[Labels.Count - 1].loc = new Vector2((i % 3) * 60, (i / 3) * 16);
                //Labels[Labels.Count - 1].loc = new Vector2((i % 3) * 48, (i / 3) * 16);
                if (i % 3 == 2)
                    Labels[Labels.Count - 1].loc += new Vector2(4, 0);
                    //Labels[Labels.Count - 1].loc += new Vector2(12, 0);
                Labels[Labels.Count - 1].Font = "FE7_Text";
                Labels[Labels.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            }
            //Yeti
            bool knife = (actor != null && actor.has_skill("KNIFE") && weapon.main_type().Name == "Sword" && !weapon.is_magic());
            bool crossbow = (actor != null && actor.has_skill("CROSSBOW") && weapon.main_type().Name == "Bow" && !weapon.Ballista());

            Labels[0].text = weapon.type;
            if (knife)
                Labels[0].text = "Knife";
            if (crossbow)
                Labels[0].text = "Crossbow";
            Labels[1].text = "Rng";
            Labels[2].text = "Wgt";
            // If not a staff
            if (!weapon.is_staff())
            {
                Labels[3].text = "Mgt";
                Labels[4].text = "Hit";
                Labels[5].text = "Crit";
            }
            if (effective)
                Labels[Labels.Count - 1].text = "Effective";

            // Rank
            Rank = new FE_Text();
            Rank.loc = new Vector2(32, 0);
            if (crossbow)
                Rank.loc.X += 16;
            Rank.Font = weapon.Rank == Weapon_Ranks.None ? "FE7_Text" : "FE7_TextL";
            Rank.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Rank.text = weapon.rank;
            // Range
            Stats.Add(new FE_Text());
            Stats[Stats.Count - 1].loc = new Vector2(92, 0);
            Stats[Stats.Count - 1].Font = "FE7_Text";
            Stats[Stats.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            int min_range = weapon.Min_Range;
            int max_range = weapon.Max_Range;
            if (knife && max_range == 1)
                max_range = 2;
            if (weapon.Mag_Range)
            {
                Stats[0].text = min_range.ToString() + "-Mg/2";
                Stats[0].offset = new Vector2(15, 0);
            }
            else
            {
                if (min_range == max_range)
                    Stats[0].text = min_range.ToString();
                else
                    Stats[0].text = min_range.ToString() + "-" + max_range.ToString();
                Stats[0].offset = new Vector2(Stats[0].text.Length > 1 ? 12 : 0, 0);
                //Stats[0].offset = new Vector2(Stats[0].text.Length > 1 ? 8 : 0, 0);
            }
            for (int i = 2; i < stats; i++)
            {
                Stats.Add(new FE_Text_Int());
                Stats[Stats.Count - 1].loc = new Vector2((i % 3) * 60 + 40, (i / 3) * 16);
                //Stats[Stats.Count - 1].loc = new Vector2((i % 3) * 48 + 40, (i / 3) * 16);
                if (i % 3 == 2)
                    Stats[Stats.Count - 1].loc += new Vector2(4, 0);
                    //Stats[Stats.Count - 1].loc += new Vector2(12, 0);
                Stats[Stats.Count - 1].Font = "FE7_Text";
                Stats[Stats.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            }
            // Wgt
            Stats[1].text = weapon.Wgt.ToString();
            if (actor != null)
            {
                int actor_wgt = actor.weapon_wgt(weapon);
                if (actor_wgt != weapon.Wgt)
                {
                    int difference = actor_wgt - weapon.Wgt;

                    Stat_Bonuses.Add(new FE_Text());
                    Stat_Bonuses[Stat_Bonuses.Count - 1].loc = Stats[1].loc + new Vector2(0, 0);
                    Stat_Bonuses[Stat_Bonuses.Count - 1].Font = "FE7_TextBonus";
                    Stat_Bonuses[Stat_Bonuses.Count - 1].texture =
                        Global.Content.Load<Texture2D>(string.Format(
                            @"Graphics/Fonts/FE7_Text_{0}",
                            difference <= 0 ? "Green" : "Red"));
                    //Stat_Bonuses[Stat_Bonuses.Count - 1].text = ((weapon.Mgt + 1) / 2 - weapon.Mgt).ToString(); //Debug
                    Stat_Bonuses[Stat_Bonuses.Count - 1].text = difference.ToString();
                }
            }
            // Stats
            if (!weapon.is_staff())
            {
                Stats[2].text = weapon.Mgt.ToString();
                if (knife)
                {
                    //Stats[2].text = ((weapon.Mgt + 1) / 2).ToString();
                    //Stats[2].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Red");
                    Stat_Bonuses.Add(new FE_Text());
                    Stat_Bonuses[Stat_Bonuses.Count - 1].loc = Stats[2].loc + new Vector2(0, 0);
                    Stat_Bonuses[Stat_Bonuses.Count - 1].Font = "FE7_TextBonus";
                    Stat_Bonuses[Stat_Bonuses.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Red");
                    //Stat_Bonuses[Stat_Bonuses.Count - 1].text = ((weapon.Mgt + 1) / 2 - weapon.Mgt).ToString(); //Debug
                    Stat_Bonuses[Stat_Bonuses.Count - 1].text = "-3";
                }
                Stats[3].text = weapon.Hit.ToString();
                if (knife)
                {
                    //Stats[3].text = (weapon.Hit + 10).ToString();
                    //Stats[3].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Green");
                }
                if (!knife)
                    Stats[3].offset.X = -((Stats[3].text.Length - 1) / 2) * 8;
                if (knife)
                {
                    Stat_Bonuses.Add(new FE_Text());
                    Stat_Bonuses[Stat_Bonuses.Count - 1].loc = Stats[3].loc - Stats[3].offset + new Vector2(0, 0);
                    Stat_Bonuses[Stat_Bonuses.Count - 1].Font = "FE7_TextBonus";
                    Stat_Bonuses[Stat_Bonuses.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Green");
                    Stat_Bonuses[Stat_Bonuses.Count - 1].text = "+10";
                }
                Stats[4].text = weapon.Crt == -1 ? "--" : weapon.Crt.ToString();
            }
            if (effective)
                for (int i = 0; i < weapon.Effectiveness.Length; i++)
                    if (weapon.Effectiveness[i] != 1)
                    {
                        Effectiveness_Icons.Add(new Icon_Sprite());
                        Effectiveness_Icons[Effectiveness_Icons.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Class_Types");
                        Effectiveness_Icons[Effectiveness_Icons.Count - 1].size = new Vector2(16, 16);
                        Effectiveness_Icons[Effectiveness_Icons.Count - 1].columns = 1;
                        Effectiveness_Icons[Effectiveness_Icons.Count - 1].loc = new Vector2(
                            48 + ((Effectiveness_Icons.Count - 1) * 16), 32);
                        Effectiveness_Icons[Effectiveness_Icons.Count - 1].index = i;

                        Effectiveness_Multipliers.Add(new Effective_WT_Arrow());
                        Effectiveness_Multipliers[Effectiveness_Icons.Count - 1].loc = new Vector2(
                            48 + ((Effectiveness_Icons.Count - 1) * 16), 32);
                        Effectiveness_Multipliers[Effectiveness_Icons.Count - 1].draw_offset = new Vector2(8, 8);
                        Effectiveness_Multipliers[Effectiveness_Icons.Count - 1].set_effectiveness(weapon.Effectiveness[i]);
                    }
        }

        public void update()
        {
            foreach (FE_Text label in Labels)
                label.update();
            Rank.update();
            foreach (FE_Text stat in Stats)
                stat.update();
            foreach (FE_Text bonus in Stat_Bonuses)
                bonus.update();
            foreach (Icon_Sprite icon in Effectiveness_Icons)
                icon.update();
            foreach (Effective_WT_Arrow arrow in Effectiveness_Multipliers)
                arrow.update();
        }

        public void draw(SpriteBatch sprite_batch)
        {
            draw(sprite_batch, Vector2.Zero);
        }
        public void draw(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            foreach (FE_Text label in Labels)
                label.draw(sprite_batch, draw_offset - (loc - offset));
            Rank.draw(sprite_batch, draw_offset - (loc - offset));
            foreach (FE_Text stat in Stats)
                stat.draw(sprite_batch, draw_offset - (loc - offset));
            foreach (FE_Text bonus in Stat_Bonuses)
                bonus.draw(sprite_batch, draw_offset - (loc - offset));
            foreach (Icon_Sprite icon in Effectiveness_Icons)
                icon.draw(sprite_batch, draw_offset - (loc - offset));
            foreach (Effective_WT_Arrow arrow in Effectiveness_Multipliers)
                arrow.draw(sprite_batch, draw_offset - (loc - offset));
        }
    }
}
