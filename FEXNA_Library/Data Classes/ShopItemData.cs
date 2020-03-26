using System.IO;

namespace FEXNA_Library
{
    public class ShopItemData : Item_Data
    {
        public static ShopItemData read(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            ShopItemData result;
            if (count == 3)
                result = new ShopItemData(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
            else //Debug
                result = new ShopItemData(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

            return result;
        }

        public ShopItemData(Item_Data_Type type, int id, int uses)
            : base(type, id, uses) { }
        public ShopItemData(int type, int id, int uses)
            : base(type, id, uses) { }

        public override void consume_use()
        {
            Uses--;
        }
        public void add_stock()
        {
            Uses++;
        }
    }
}
