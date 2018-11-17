using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface.Status;

namespace FEXNA
{
    class Status_Support_Bonuses : Stereoscopic_Graphic_Object
    {
        protected FE_Text Bond_Label, Bond_Name;
        protected List<StatusSupportBonusUINode> Bonuses = new List<StatusSupportBonusUINode>();

        #region Accessors
        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Bond_Label.stereoscopic = value;
                Bond_Name.stereoscopic = value;
                foreach (var bonus in Bonuses)
                    bonus.stereoscopic = value;
            }
        }
        #endregion

        public Status_Support_Bonuses(int bond_offset = 0)
        {
            // Bond Label
            Bond_Label = new FE_Text();
            Bond_Label.loc = new Vector2(12, bond_offset);
            Bond_Label.Font = "FE7_Text";
            Bond_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Bond_Label.text = "Bond";
            Bond_Label.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            // Bond Name
            Bond_Name = new FE_Text();
            Bond_Name.loc = new Vector2(64, bond_offset);
            Bond_Name.Font = "FE7_Text";
            Bond_Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Bond_Name.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            // Bonus Values
            for (int i = 0; i < 6; i++)
            {
                var stat = (Combat_Stat_Labels)i;
                string label;
                switch (stat)
                {
                    case Combat_Stat_Labels.Dmg:
                    default:
                        label = "Atk";
                        break;
                    case Combat_Stat_Labels.Def:
                        label = "Def";
                        break;
                    case Combat_Stat_Labels.Hit:
                        label = "Hit";
                        break;
                    case Combat_Stat_Labels.Avo:
                        label = "Avoid";
                        break;
                    case Combat_Stat_Labels.Crt:
                        label = "Crit";
                        break;
                    case Combat_Stat_Labels.Dod:
                        label = "Dodge";
                        break;
                }

                var bonus = new StatusSupportBonusUINode("", label,
                    (Game_Unit unit) => unit.support_bonus(stat, true).ToString(),
                    (Game_Actor actor) => actor.total_support_bonus(stat).ToString(),
                    44);
                //bonus.loc = new Vector2((i % 2) * 56 + 4, (i / 2) * 16 + 16); //Debug
                bonus.loc = new Vector2((i % 2) * 64 + 0, (i / 2) * 16 + 16);
                bonus.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
                Bonuses.Add(bonus);
            }
        }

        public void set_images(Game_Unit unit, Game_Actor actor)
        {
            if (unit != null && unit.actor != actor)
                unit = null;

            // Bond
            if (actor.bond > 0)
                Bond_Name.text = Global.game_actors[actor.bond].name; //Yeti
            else
                Bond_Name.text = "-----";
            Bond_Name.offset.X = Font_Data.text_width(Bond_Name.text) / 2;
            // Bonuses
            for (int i = 0; i < 6; i++)
            {
                if (unit != null && unit.actor == actor)
                    Bonuses[i].refresh(unit);
                else
                    Bonuses[i].refresh(actor);
            }
        }

        internal void set_next_level_bonus(Game_Actor actor, Game_Actor target)
        {
            for (int i = 0; i < 6; i++)
            {
                if (target != null &&
                    Global.battalion.actors.Contains(target.id) &&
                    //!actor.is_support_maxed(false, target.id) &&
                    //!actor.is_support_level_maxed(target.id) &&
                    actor.is_support_ready(target.id))
                {
                    int bonus = actor.support_bonus_from_next_level(target.id, (Combat_Stat_Labels)i);
                    Bonuses[i].stat_bonus(bonus);
                }
                else
                    Bonuses[i].stat_bonus(0);
            }
        }

        public void update()
        {
            foreach (var bonus in Bonuses)
                bonus.Update(false);
        }

        public void draw(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            foreach (var bonus in Bonuses)
                bonus.Draw(sprite_batch, draw_offset - this.loc);
        }
    }
}
