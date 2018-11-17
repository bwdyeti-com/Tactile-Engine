using System;
using Microsoft.Xna.Framework;

namespace FEXNA.Graphics.Text
{
    class HP_Counter : FE_Text_Int
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
            if (hp >= Math.Pow(10, Constants.BattleScene.HP_COUNTER_VALUES) ||
                hp > Constants.BattleScene.MAX_HP_ROWS * Constants.BattleScene.HP_TABS_PER_ROW)
            {
                text = "";
                for (int i = 0; i < Constants.BattleScene.HP_COUNTER_VALUES; i++)
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
