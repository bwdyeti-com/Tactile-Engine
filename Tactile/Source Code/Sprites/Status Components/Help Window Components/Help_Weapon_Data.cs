using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile
{
    class Help_Weapon_Data : Graphic_Object
    {
        protected List<TextSprite> Labels = new List<TextSprite>();
        protected TextSprite Rank;
        protected List<TextSprite> Stats = new List<TextSprite>(), Stat_Bonuses = new List<TextSprite>();
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
                Labels.Add(new TextSprite());
                Labels[Labels.Count - 1].loc = new Vector2((i % 3) * 60, (i / 3) * 16);
                if (i % 3 == 2)
                    Labels[Labels.Count - 1].loc += new Vector2(4, 0);
                Labels[Labels.Count - 1].SetFont(Config.UI_FONT, Global.Content, "Yellow");
            }
            //@Yeti: handle weapon type replacement skills better than hardcoding
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
            Rank = new TextSprite();
            Rank.loc = new Vector2(32, 0);
            if (crossbow)
                Rank.loc.X += 16;
            Rank.SetFont(
                weapon.Rank == Weapon_Ranks.None ? Config.UI_FONT : Config.UI_FONT + "L",
                Global.Content, "Blue", Config.UI_FONT);
            Rank.text = weapon.rank;
            // Range
            Stats.Add(new TextSprite());
            Stats[Stats.Count - 1].loc = new Vector2(92, 0);
            Stats[Stats.Count - 1].SetFont(Config.UI_FONT, Global.Content, "Blue");
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
            }
            for (int i = 2; i < stats; i++)
            {
                Stats.Add(new RightAdjustedText());
                Stats[Stats.Count - 1].loc = new Vector2((i % 3) * 60 + 40, (i / 3) * 16);
                if (i % 3 == 2)
                    Stats[Stats.Count - 1].loc += new Vector2(4, 0);
                Stats[Stats.Count - 1].SetFont(Config.UI_FONT, Global.Content, "Blue");
            }
            // Wgt
            Stats[1].text = weapon.Wgt.ToString();
            if (actor != null)
            {
                int actor_wgt = actor.weapon_wgt(weapon);
                if (actor_wgt != weapon.Wgt)
                {
                    int difference = actor_wgt - weapon.Wgt;

                    Stat_Bonuses.Add(new TextSprite());
                    Stat_Bonuses[Stat_Bonuses.Count - 1].loc = Stats[1].loc + new Vector2(0, 0);
                    Stat_Bonuses[Stat_Bonuses.Count - 1].SetFont(
                        Config.UI_FONT + "Bonus", Global.Content,
                        difference <= 0 ? "Green" : "Red", Config.UI_FONT);
                    Stat_Bonuses[Stat_Bonuses.Count - 1].text = difference.ToString();
                }
            }
            // Stats
            if (!weapon.is_staff())
            {
                Stats[2].text = weapon.Mgt.ToString();
                if (knife)
                {
                    Stat_Bonuses.Add(new TextSprite());
                    Stat_Bonuses[Stat_Bonuses.Count - 1].loc = Stats[2].loc + new Vector2(0, 0);
                    Stat_Bonuses[Stat_Bonuses.Count - 1].SetFont(
                        Config.UI_FONT + "Bonus", Global.Content, "Red", Config.UI_FONT);
                    Stat_Bonuses[Stat_Bonuses.Count - 1].text = "-3";
                }
                Stats[3].text = weapon.Hit.ToString();
                if (!knife)
                    Stats[3].offset.X = -((Stats[3].text.Length - 1) / 2) * 8;
                if (knife)
                {
                    Stat_Bonuses.Add(new TextSprite());
                    Stat_Bonuses[Stat_Bonuses.Count - 1].loc = Stats[3].loc - Stats[3].offset + new Vector2(0, 0);
                    Stat_Bonuses[Stat_Bonuses.Count - 1].SetFont(
                        Config.UI_FONT + "Bonus", Global.Content, "Green", Config.UI_FONT);
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
            foreach (TextSprite label in Labels)
                label.update();
            Rank.update();
            foreach (TextSprite stat in Stats)
                stat.update();
            foreach (TextSprite bonus in Stat_Bonuses)
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
            foreach (TextSprite label in Labels)
                label.draw(sprite_batch, draw_offset - (loc - offset));
            Rank.draw(sprite_batch, draw_offset - (loc - offset));
            foreach (TextSprite stat in Stats)
                stat.draw(sprite_batch, draw_offset - (loc - offset));
            foreach (TextSprite bonus in Stat_Bonuses)
                bonus.draw(sprite_batch, draw_offset - (loc - offset));
            foreach (Icon_Sprite icon in Effectiveness_Icons)
                icon.draw(sprite_batch, draw_offset - (loc - offset));
            foreach (Effective_WT_Arrow arrow in Effectiveness_Multipliers)
                arrow.draw(sprite_batch, draw_offset - (loc - offset));
        }
    }
}
