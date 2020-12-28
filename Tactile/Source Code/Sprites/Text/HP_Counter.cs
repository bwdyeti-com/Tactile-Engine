using System;
using Microsoft.Xna.Framework;

namespace Tactile.Graphics.Text
{
    class HP_Counter : RightAdjustedText
    {
        int Zoom_Timer = 0;

        #region Accessors
        public int zoom_timer
        {
            set
            {
                Zoom_Timer = value;
                scale.Y = 2;
            }
        }
        #endregion

        public HP_Counter()
        {
            offset = new Vector2(0, 4);
        }

        public void set_hp(int hp)
        {
            if (hp >= Math.Pow(10, Global.BattleSceneConfig.HpCounterValues) ||
                hp > Global.BattleSceneConfig.MaxHpRows * Global.BattleSceneConfig.HpTabsPerRow)
            {
                text = "";
                for (int i = 0; i < Global.BattleSceneConfig.HpCounterValues; i++)
                    text += "?";
            }
            else
                text = hp.ToString();
        }

        public override void update()
        {
            if (Zoom_Timer == 1)
                scale.Y = 1;
            if (Zoom_Timer > 0)
                Zoom_Timer--;
        }
    }
}
