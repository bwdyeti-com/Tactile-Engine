using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Calculations.Stats;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Unit_Info_Panel_Stats : Sprite
    {
        protected RightAdjustedText Atk, Hit, Crt, AtkSpd;
        private Sprite AtkSpdLabel;
        protected readonly Vector2 TEXT_LOC = new Vector2(33, -4);

        public Unit_Info_Panel_Stats()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Unit_Info");
            Src_Rect = new Rectangle(72, 40, 16, 24);
            AtkSpdLabel = new Sprite(this.texture);
            AtkSpdLabel.src_rect = new Rectangle(88, 64, 16, 8);
            AtkSpdLabel.loc = new Vector2(0, 24);

            Atk = new RightAdjustedText();
            Atk.SetFont(Config.INFO_FONT, Global.Content);
            Hit = new RightAdjustedText();
            Hit.SetFont(Config.INFO_FONT, Global.Content);
            Crt = new RightAdjustedText();
            Crt.SetFont(Config.INFO_FONT, Global.Content);
            AtkSpd = new RightAdjustedText();
            AtkSpd.SetFont(Config.INFO_FONT, Global.Content);
        }

        public virtual void set_images(Game_Unit unit)
        {
            Atk.text = "--";
            Hit.text = "--";
            Crt.text = "--";

            BattlerStats stats = new BattlerStats(unit.id);
            int atkSpd = unit.atk_spd();
            if (unit.is_on_siege())
            {
                var siege = unit.items[Siege_Engine.SiegeInventoryIndex];
                if (siege.is_weapon)
                {
                    stats = new BattlerStats(unit.id, siege.to_weapon);
                    atkSpd = unit.atk_spd(siege.to_weapon.Max_Range, siege.to_weapon);
                }
            }
            
            AtkSpd.text = atkSpd.ToString();

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
                    if (mirrored)
                        offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(texture, loc + draw_vector() - draw_offset,
                        src_rect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);


                    AtkSpdLabel.draw(sprite_batch, -draw_offset - (loc + draw_vector() + new Vector2(0, 0) - offset));
                    Atk.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + new Vector2(0, 0) - offset));
                    Hit.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + new Vector2(0, 8) - offset));
                    Crt.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + new Vector2(0, 16) - offset));
                    AtkSpd.draw(sprite_batch, -draw_offset - (loc + draw_vector() + TEXT_LOC + new Vector2(0, 24) - offset));
                }
        }
    }
}
