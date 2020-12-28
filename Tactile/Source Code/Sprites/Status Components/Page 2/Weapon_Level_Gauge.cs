using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    internal class Weapon_Level_Gauge : Stereoscopic_Graphic_Object
    {
        internal const int WLVL_BAR_WIDTH = 34;

        protected Weapon_Type_Icon WLvl_Icon;
        protected Stat_Bar WLvl_Bar;
        protected TextSprite WLvl_Letter;
        private int Type;

        #region Accessors
        public int color_override { set { WLvl_Bar.color_override = value; } }

        public int type
        {
            set
            {
                Type = value;
                WLvl_Icon.index = Global.weapon_types[value].IconIndex;
            }
        }

        public Color tint
        {
            get { return WLvl_Letter.tint; }
            set
            {
                WLvl_Letter.tint = value;
                WLvl_Icon.tint = value;
            }
        }
        #endregion

        public override string ToString()
        {
            return string.Format("Weapon Level Gauge: {0}", Global.weapon_types[Type].Name);
        }

        public Weapon_Level_Gauge(int type)
        {
            WLvl_Icon = new Weapon_Type_Icon();
            this.type = type;
            WLvl_Icon.loc = new Vector2(8, 8);

            WLvl_Bar = new Stat_Bar();
            WLvl_Bar.loc = new Vector2(24, 16);
            WLvl_Bar.offset = new Vector2(-2, 0);
            WLvl_Bar.bar_width = WLVL_BAR_WIDTH;
            initialize_letter();
        }

        protected virtual void initialize_letter()
        {
            WLvl_Letter = new TextSprite();
            WLvl_Letter.loc = new Vector2(40, 8);
            WLvl_Letter.SetFont(Config.UI_FONT + "L", Config.UI_FONT);
        }

        public void set_data(float percent, string color, string letter)
        {
            WLvl_Bar.SetFillWidth(WLVL_BAR_WIDTH,
                (int)Math.Round(percent * 20000),
                0, 20000);
            WLvl_Letter.SetColor(Global.Content, color);
            WLvl_Letter.text = letter;
        }

        public void update()
        {
            WLvl_Icon.update();
            WLvl_Letter.update();
        }

        public virtual void draw(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            WLvl_Icon.draw(sprite_batch, draw_offset - (this.loc + draw_vector()));
            WLvl_Bar.draw_fill(sprite_batch, draw_offset - (this.loc + draw_vector()));
            WLvl_Letter.draw(sprite_batch, draw_offset - (this.loc + draw_vector()));
        }
        public virtual void draw_bg(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            WLvl_Bar.draw_bg(sprite_batch, draw_offset - (this.loc + draw_vector()));
        }
    }
}
