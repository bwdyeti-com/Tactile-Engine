using TactileLibrary;

namespace Tactile
{
    class Equipment_Service : IEquipmentService
    {
        private Data_Weapon DebugWeapon;
        private Data_Item DebugItem;

        public Equipment_Service()
        {
            DebugWeapon = new Data_Weapon() { Name = "[DEBUG WEAPON]", Main_Type = 0, Uses = -1 };
            DebugItem = new Data_Item() { Name = "[DEBUG ITEM]", Uses = -1 };
        }

        public Data_Equipment Equipment(Item_Data data)
        {
            if (data.is_weapon)
            {
                if (Global.HasWeapon(data.Id))
                    return Global.GetWeapon(data.Id);
#if DEBUG
                else
                    return DebugWeapon;
#endif
            }
            else if (data.is_item)
            {
                if (Global.data_items.ContainsKey(data.Id))
                    return Global.data_items[data.Id];
#if DEBUG
                else
                    return DebugItem;
#endif
            }
            return null;
        }

        public bool Exists(Item_Data data)
        {
            if (data.is_weapon)
            {
                return Global.HasWeapon(data.Id);
            }
            else if (data.is_item)
            {
                return Global.data_items.ContainsKey(data.Id);
            }
            return false;
        }
    }
}
