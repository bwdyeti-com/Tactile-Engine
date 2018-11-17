using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    class Unit_Info_Hp_Gauge : Sprite
    {
        protected int Width;
        protected bool Gauge_Visible = true;
        protected FE_Text_Int Hp;
        protected FE_Text MaxHp;
        protected int GAUGE_MIN = 1;
        protected int GAUGE_WIDTH = 42;
        protected Vector2 TAG_LOC = new Vector2(32, 16);
        protected Vector2 TEXT_LOC = new Vector2(68, 12);
        protected Vector2 GAUGE_LOC = new Vector2(33, 26);
        protected Vector2 MAX_OFFSET = new Vector2(13, 0);

        #region Accessors
        public bool gauge_visible { set { Gauge_Visible = value; } }
        #endregion

        public Unit_Info_Hp_Gauge()
        {
            init_images();
        }

        protected void init_images()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Unit_Info");
            Hp = new FE_Text_Int();
            Hp.Font = "FE7_Text_Info";
            Hp.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Info");
            MaxHp = new FE_Text_Int();
            MaxHp.Font = "FE7_Text_Info";
            MaxHp.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Info");
        }

        public virtual void set_val(int hp, int maxhp)
        {
            float ratio = hp / (float)maxhp;
            Width = (int)MathHelper.Clamp(ratio * GAUGE_WIDTH, GAUGE_MIN, GAUGE_WIDTH);
            if (hp >= Math.Pow(10, Constants.BattleScene.STATUS_HP_COUNTER_VALUES))
            {
                //Hp.text = ""; //Debug
                //for (int i = 0; i < Constants.BattleScene.STATUS_HP_COUNTER_VALUES; i++)
                //    Hp.text += "?";
                //Hp.text += "/";
                Hp.text = "--/";
            }
            else
                Hp.text = hp.ToString() + "/";
            if (maxhp >= Math.Pow(10, Constants.BattleScene.STATUS_HP_COUNTER_VALUES))
            {
                //MaxHp.text = ""; //Debug
                //for (int i = 0; i < Constants.BattleScene.STATUS_HP_COUNTER_VALUES; i++)
                //    MaxHp.text += "?";
                MaxHp.text = "--";
            }
            else
                MaxHp.text = maxhp.ToString();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    if (mirrored)
                        offset.X = src_rect.Width - offset.X;
                    // Hp Tag
                    draw_tag(sprite_batch, draw_offset);
                    // Hp Gauge
                    if (Gauge_Visible)
                        draw_gauge(sprite_batch, draw_offset);
                    // Text
                    draw_text(sprite_batch, draw_offset);
                }
        }

        protected virtual void draw_tag(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector() - draw_offset;
            sprite_batch.Draw(texture, loc + TAG_LOC,
                new Rectangle(0, 40, 16, 8), tint, angle, offset, scale,
                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
        }

        protected virtual void draw_gauge(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector() - draw_offset;
            // Hp Gauge
            draw_gauge_bg(sprite_batch, loc);
            // Hp Gauge Fill
            draw_gauge_fill(sprite_batch, loc, Width, tint);
        }

        protected virtual void draw_gauge_bg(SpriteBatch sprite_batch, Vector2 loc)
        {
            sprite_batch.Draw(texture, loc + GAUGE_LOC,
                new Rectangle(16, 40, 2 + GAUGE_WIDTH, 5), tint, angle, offset, scale,
                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
            sprite_batch.Draw(texture, loc + GAUGE_LOC + new Vector2(2 + GAUGE_WIDTH, 0),
                new Rectangle(60, 40, 2, 5), tint, angle, offset, scale,
                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
        }
        protected void draw_gauge_fill(SpriteBatch sprite_batch, Vector2 loc, int width, Color tint)
        {
            for (int x = 0; x < width; x++)
                sprite_batch.Draw(texture, loc + GAUGE_LOC + new Vector2(2 + x, 1),
                    new Rectangle(62, 40, 1, 2), tint, angle, offset, scale,
                    mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
        }

        protected virtual void draw_text(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector() - draw_offset;
            Hp.draw(sprite_batch, -(loc + TEXT_LOC - offset));
            MaxHp.draw(sprite_batch, -(loc + TEXT_LOC + MAX_OFFSET - offset));
        }
    }
}
