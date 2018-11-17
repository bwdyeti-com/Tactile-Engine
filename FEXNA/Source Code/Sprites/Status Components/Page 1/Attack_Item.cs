using FEXNA_Library;

namespace FEXNA
{
    class Attack_Item : Status_Item
    {
        protected override void set_text_color(Game_Actor actor, Data_Equipment item)
        {
            bool useable;
            if (item.is_weapon)
                useable = actor.is_equippable_as_siege(item as Data_Weapon);
            else
                useable = actor.prf_check(item) && ((item as Data_Item).Promotes.Count == 0) || Combat.can_use_item(actor, item.Id, false);

            set_text_color(useable);
        }
    }
}
