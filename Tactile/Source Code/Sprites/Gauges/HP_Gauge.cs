using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class HP_Gauge : Sprite
    {
        const int BASE_Y = 186;

        int Timer = 0;
        int MaxHp = 0;
        int Hp = 0;
        int Frame = 0;

        #region Accessors
        public int maxhp
        {
            set
            {
                MaxHp = (int)MathHelper.Clamp(value, 1,
                    Global.BattleSceneConfig.HpTabsPerRow *
                    Global.BattleSceneConfig.MaxHpRows);
                loc.Y = BASE_Y - (int)((7 * ((MaxHp - 1) /
                    Global.BattleSceneConfig.HpTabsPerRow)) / 2.0);
            }
        }

        public int hp
        {
            set
            {
                //Hp = (int)MathHelper.Clamp(value, 0, MaxHp); //Debug
                Hp = Math.Max(value, 0);
            }
        }

        private bool is_health_excess
        {
            get
            {
                return Hp > Global.BattleSceneConfig.HpTabsPerRow * Global.BattleSceneConfig.MaxHpRows;
            }
        }
        #endregion

        public HP_Gauge(Texture2D texture)
        {
            this.texture = texture;
            loc = new Vector2(0, BASE_Y);
        }

        public override void update()
        {
            switch (Timer)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    Frame = 0;
                    break;
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                    Frame = (Timer - 8) / 2;
                    break;
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                    Frame = 5;
                    break;
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                case 33:
                case 34:
                case 35:
                    Frame = (37 - Timer) / 2;
                    break;
            }
            Timer = (Timer + 1) % 36;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    int tabWidth = Global.BattleSceneConfig.HpGaugeTabWidth;
                    int tabHeight = Global.BattleSceneConfig.HpGaugeTabHeight;

                    Rectangle filled_rect = new Rectangle(
                        Frame * tabWidth,
                        0 + (is_health_excess ? tabHeight * 2 : 0),
                        tabWidth,
                        tabHeight);
                    Rectangle empty_rect = new Rectangle(
                        Frame * tabWidth,
                        tabHeight +
                            (is_health_excess ? tabHeight * 2 : 0),
                        tabWidth,
                        tabHeight);
                    Vector2 offset = this.offset;
                    Vector2 tab_location;
                    for (int i = 0; i < MaxHp; i++)
                    {
                        tab_location = new Vector2(
                            (i % Global.BattleSceneConfig.HpTabsPerRow) * (tabWidth - 1),
                            (i / Global.BattleSceneConfig.HpTabsPerRow) * (tabHeight + 1));
                        if (i < Hp)
                            sprite_batch.Draw(texture, this.loc + draw_vector() + tab_location - draw_offset,
                                empty_rect, tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        else
                            sprite_batch.Draw(texture, this.loc + draw_vector() + tab_location - draw_offset,
                                filled_rect, tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                }
        }
    }
}
