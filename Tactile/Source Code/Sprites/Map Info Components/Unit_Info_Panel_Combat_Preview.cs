using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Calculations.Stats;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Unit_Info_Panel_Combat_Preview : Sprite
    {
        protected readonly Vector2 TEXT_LOC = new Vector2(-3, -4);

        protected Rectangle DmgRect, HitRect, CrtRect, ASpdSrcRect;
        protected RightAdjustedText Hp1, Dmg1, Hit1, Crt1, ASpd1;
        protected RightAdjustedText Hp2, Dmg2, Hit2, Crt2, ASpd2;

        public Unit_Info_Panel_Combat_Preview()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Unit_Info");
            Src_Rect = new Rectangle(0, 40, 16, 8);
            DmgRect = new Rectangle(72, 40 + 8, 16, 8);
            HitRect = new Rectangle(72, 40 + 0, 16, 8);
            CrtRect = new Rectangle(72, 40 + 16, 16, 8);
            ASpdSrcRect = new Rectangle(72 + 16, 40 + 24, 16, 8);

            Hp1 = stat_text();
            Dmg1 = stat_text();
            Hit1 = stat_text();
            Crt1 = stat_text();
            ASpd1 = stat_text();

            Hp2 = stat_text();
            Dmg2 = stat_text();
            Hit2 = stat_text();
            Crt2 = stat_text();
            ASpd2 = stat_text();
        }

        private static RightAdjustedText stat_text()
        {
            var text = new RightAdjustedText();
            text.SetFont(Config.INFO_FONT, Global.Content);
            return text;
        }

        public virtual void set_images(Game_Unit unit)
        {
            Hp1.text = "--";
            Dmg1.text = "--";
            Hit1.text = "--";
            Crt1.text = "--";

            var stats = new BattlerStats(unit.id);
            Hp1.text = unit.hp.ToString();
            ASpd1.text = unit.atk_spd().ToString();

            if (stats.has_non_staff_weapon)
            {
                Dmg1.text = stats.dmg().ToString();
                Hit1.text = stats.hit().ToString();
                Crt1.text = stats.crt().ToString();
            }

            Hp2.text = "--";
            Dmg2.text = "--";
            Hit2.text = "--";
            Crt2.text = "--";
            ASpd2.text = "--";

            var player_unit = Global.game_map.get_selected_unit();
            if (player_unit != null)
            {
                Hp2.text = player_unit.hp.ToString();
                ASpd2.text = player_unit.atk_spd().ToString();

                if (player_unit.actor.weapon != null)
                {
                    if (!player_unit.actor.weapon.is_staff())
                    {
                        var combat = Combat.combat_stats(player_unit.id, unit.id, player_unit.min_range());

                        stats = new BattlerStats(player_unit.id);

                        Dmg2.text = combat[1].ToString();
                        Hit2.text = combat[0].ToString();
                        Crt2.text = combat[2].ToString();

                        Dmg1.text = "--";
                        Hit1.text = "--";
                        Crt1.text = "--";
                        if (combat[4] != null)
                        {
                            Dmg1.text = combat[5].ToString();
                            Hit1.text = combat[4].ToString();
                            Crt1.text = combat[6].ToString();
                        }
                    }
                }
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
                    sprite_batch.Draw(texture,
                        loc + draw_vector() +
                            new Vector2(0, 8) - draw_offset,
                        DmgRect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    sprite_batch.Draw(texture,
                        loc + draw_vector() +
                            new Vector2(0, 16) - draw_offset,
                        HitRect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    sprite_batch.Draw(texture,
                        loc + draw_vector() +
                            new Vector2(0, 24) - draw_offset,
                        CrtRect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    sprite_batch.Draw(texture,
                        loc + draw_vector() +
                            new Vector2(0, 32) - draw_offset,
                        ASpdSrcRect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);

                    Vector2 text_loc = loc + draw_vector() + TEXT_LOC - offset;
                    Hp1.draw(sprite_batch, draw_offset - (text_loc + new Vector2(0, 0)));
                    Dmg1.draw(sprite_batch, draw_offset - (text_loc + new Vector2(0, 8)));
                    Hit1.draw(sprite_batch, draw_offset - (text_loc + new Vector2(0, 16)));
                    Crt1.draw(sprite_batch, draw_offset - (text_loc + new Vector2(0, 24)));
                    ASpd1.draw(sprite_batch, draw_offset - (text_loc + new Vector2(0, 32)));

                    Hp2.draw(sprite_batch, draw_offset - (text_loc + new Vector2(36, 0)));
                    Dmg2.draw(sprite_batch, draw_offset - (text_loc + new Vector2(36, 8)));
                    Hit2.draw(sprite_batch, draw_offset - (text_loc + new Vector2(36, 16)));
                    Crt2.draw(sprite_batch, draw_offset - (text_loc + new Vector2(36, 24)));
                    ASpd2.draw(sprite_batch, draw_offset - (text_loc + new Vector2(36, 32)));
                }
        }
    }
}
