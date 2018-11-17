using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Calculations.Stats;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    class Unit_Info_Panel_Stats : Sprite
    {
        protected FE_Text_Int Atk, Hit, Crt, Avo;
        protected readonly Vector2 TEXT_LOC = new Vector2(33, -4);

        public Unit_Info_Panel_Stats()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Unit_Info");
            Src_Rect = new Rectangle(72, 40, 16, 32);
            Atk = new FE_Text_Int();
            Atk.Font = "FE7_Text_Info";
            Atk.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Info");
            Hit = new FE_Text_Int();
            Hit.Font = "FE7_Text_Info";
            Hit.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Info");
            Crt = new FE_Text_Int();
            Crt.Font = "FE7_Text_Info";
            Crt.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Info");
            Avo = new FE_Text_Int();
            Avo.Font = "FE7_Text_Info";
            Avo.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Info");
        }

        public virtual void set_images(Game_Unit unit)
        {
            Atk.text = "--";
            Hit.text = "--";
            Crt.text = "--";

            BattlerStats stats = new BattlerStats(unit.id);
            if (unit.is_on_siege())
            {
                var siege = unit.items[Siege_Engine.SIEGE_INVENTORY_INDEX];
                if (siege.is_weapon)
                    stats = new BattlerStats(unit.id, siege.to_weapon);
            }

            Avo.text = stats.avo().ToString();

            if (stats.has_non_staff_weapon)
            {
                Atk.text = stats.dmg().ToString();
                Hit.text = stats.hit().ToString();
                Crt.text = stats.crt().ToString();
            }
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(texture, loc + draw_vector() - draw_offset,
                        src_rect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    Atk.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + new Vector2(0, 0) - offset));
                    Hit.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + new Vector2(0, 8) - offset));
                    Crt.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + new Vector2(0, 16) - offset));
                    Avo.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + new Vector2(0, 24) - offset));
                }
        }
    }
}
