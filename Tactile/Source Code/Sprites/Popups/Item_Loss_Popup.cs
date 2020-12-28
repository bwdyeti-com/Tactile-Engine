using System.Linq;
using Microsoft.Xna.Framework;

namespace Tactile
{
    class Item_Loss_Popup : Item_Gain_Popup
    {
        public Item_Loss_Popup(int item_id, bool is_item, int time)
        {
            initialize(item_id, is_item, false);
            Timer_Max = time;
            loc = new Vector2((Config.WINDOW_WIDTH - Width) / 2, 80);
        }

        public override void update()
        {
            if (Timer > 0 && this.skip_input)
                Timer = Timer_Max;
            base.update();
        }

        protected override string got_text()
        {
            return "";
        }
        
        protected override string aname_text(string item_name, TactileLibrary.Data_Equipment item)
        {
            return string.Format("{0} {1}", item.capitalizedArticle, item_name);
        }

        protected override string broke_text()
        {
            return "was pilfered.";
        }
    }
}
