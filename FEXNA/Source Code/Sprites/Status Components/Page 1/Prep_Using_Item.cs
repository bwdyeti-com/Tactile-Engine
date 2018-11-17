using FEXNA_Library;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Prep_Using_Item : Status_Item
    {
        protected override void set_text_color(Game_Actor actor, Data_Equipment item)
        {
            bool useable;
            if (item.is_weapon)
                useable = false;
            else
                useable = Combat.can_use_item(actor, item.Id, false);

            set_text_color(useable);
        }
    }
}
