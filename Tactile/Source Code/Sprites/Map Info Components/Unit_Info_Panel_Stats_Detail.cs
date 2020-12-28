using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Unit_Info_Panel_Stats_Detail : Unit_Info_Panel_Stats
    {
        protected RightAdjustedText Def, Res, Dod, ASpd;
        protected Rectangle Src_Rect2;
        protected readonly Vector2 COLUMN_TWO_OFFSET = new Vector2(32, 0);

        public Unit_Info_Panel_Stats_Detail()
        {
            Src_Rect2 = new Rectangle(88, 40, 16, 32);
            Def = new RightAdjustedText();
            Def.SetFont(Config.INFO_FONT, Global.Content);
            Res = new RightAdjustedText();
            Res.SetFont(Config.INFO_FONT, Global.Content);
            Dod = new RightAdjustedText();
            Dod.SetFont(Config.INFO_FONT, Global.Content);
            ASpd = new RightAdjustedText();
            ASpd.SetFont(Config.INFO_FONT, Global.Content);
        }

        public override void set_images(Game_Unit unit)
        {
            base.set_images(unit);

            var stats = new Calculations.Stats.BattlerStats(unit.id);
            Def.text = stats.def().ToString();
            Res.text = stats.res().ToString();
            Dod.text = stats.dodge().ToString();
            ASpd.text = unit.atk_spd().ToString();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.draw(sprite_batch, draw_offset);
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = Src_Rect2;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(texture, this.loc + draw_vector() + COLUMN_TWO_OFFSET + new Vector2(4, 0) - draw_offset,
                        src_rect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    Def.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + COLUMN_TWO_OFFSET + new Vector2(0, 0) - offset));
                    Res.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + COLUMN_TWO_OFFSET + new Vector2(0, 8) - offset));
                    Dod.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + COLUMN_TWO_OFFSET + new Vector2(0, 16) - offset));
                    ASpd.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + COLUMN_TWO_OFFSET + new Vector2(0, 24) - offset));
                }
        }
    }
}
