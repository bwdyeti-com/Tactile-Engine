using System;
using System.IO;

namespace TactileLibrary
{
    public enum Item_Data_Type { Weapon, Item }
    public class Item_Data
    {
        internal static IEquipmentService Equipment_Data { get; private set; }
        public static IEquipmentService equipment_data
        {
            get { return Equipment_Data; }
            set
            {
                if (Equipment_Data == null)
                    Equipment_Data = value;
            }
        }

        public Item_Data_Type Type;
        public int Id;
        public int Uses;

        #region Accessors
        public Data_Equipment to_equipment
        {
            get
            {
                if (Equipment_Data != null)
                    return Equipment_Data.equipment(this);
                return null;
            }
        }
        public Data_Weapon to_weapon
        {
            get
            {
                //if (is_weapon && Equipment_Data != null) //Debug
                //    return (Data_Weapon)Equipment_Data.equipment(this);
                if (is_weapon)
                    return (Data_Weapon)to_equipment;
                return null;
            }
        }
        public Data_Item to_item
        {
            get
            {
                //if (is_item && Equipment_Data != null)
                //    return (Data_Item)Equipment_Data.equipment(this);
                if (is_item)
                    return (Data_Item)to_equipment;
                return null;
            }
        }

        public string name { get { return to_equipment.Name; } }
        public int max_uses { get { return to_equipment.Uses; } }
        public int cost { get { return to_equipment.Cost; } }

        public bool infinite_uses
        {
            get { return !non_equipment && to_equipment.infinite_uses; }
        }
        public bool out_of_uses { get { return Uses == 0 && !infinite_uses; } }
        #endregion

        #region Serialization
        public static Item_Data read(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            Item_Data result;
            if (count == 3)
                result = new Item_Data(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
            else //Debug
                result = new Item_Data(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

            return result;
        }

        public void write(BinaryWriter writer)
        {
            if (true) //Debug
            {
                writer.Write(3);
                writer.Write((int)Type);
                writer.Write(Id);
                writer.Write(Uses);
            }
        }
        #endregion

        public override string ToString()
        {
            if (blank_item)
                return "(No Item)";
            else if (to_equipment != null)
                return to_equipment.ToString(Uses);
            return base.ToString();
        }

        public Item_Data() : this(0, 0, 0) { }
        public Item_Data(Item_Data_Type type, int id)
        {
            Type = type;
            Id = id;
            if (!this.non_equipment)
                Uses = this.max_uses;
        }
        public Item_Data(int type, int id) : this((Item_Data_Type)type, id) { }
        public Item_Data(Item_Data_Type type, int id, int uses)
        {
            Type = type;
            Id = id;
            Uses = uses;
        }
        public Item_Data(int type, int id, int uses) : this((Item_Data_Type)type, id, uses) { }
        public Item_Data(Item_Data data) : this(data.Type, data.Id, data.Uses) { }

        public bool is_weapon
        {
            get { return Type == Item_Data_Type.Weapon && Id > 0; }
        }

        public bool is_item
        {
            get { return Type == Item_Data_Type.Item; } // Id > 0 && //Debug
        }

        /// <summary>
        /// Returns true if the item is in a completely default state
        /// </summary>
        public bool blank_item
        {
            get { return Type == Item_Data_Type.Weapon && Id == 0 && Uses == 0; }
        }

        /// <summary>
        /// Returns true if the item's id is invalid
        /// </summary>
        public bool non_equipment
        {
            get { return Id <= 0 || to_equipment == null; }
        }

        public bool same_item(Item_Data item)
        {
            return Type == item.Type && Id == item.Id;
        }

        public void repair(bool repair_fully = true, int uses = 0)
        {
            if (this.non_equipment)
                return;

            Uses = UsesAfterRepair(repair_fully, uses);
        }

        public int RepairAmount(Data_Item repairItem)
        {
            // Repair a percent of the total uses
            int repairPercent = (int)(this.max_uses * repairItem.Repair_Percent);
            // Repair a gold value worth of uses
            int repairValue = 0;
            if (this.cost != 0)
                repairValue = repairItem.Repair_Val / this.cost;

            int repairUses = repairPercent + repairValue;

            return Math.Max(1, repairUses);
        }

        public int UsesAfterRepair(bool repair_fully = true, int uses = 0)
        {
            if (!this.non_equipment)
            {
                if (repair_fully)
                    return this.max_uses;
                else if (!this.infinite_uses)
                {
                    return Math.Max(0, Math.Min(Uses + uses, this.max_uses));
                }
            }

            return Uses;
        }

        public virtual void consume_use()
        {
            if (!this.infinite_uses)
                Uses = Math.Min(Uses, this.max_uses);
            Uses--;
        }
        public void repair_fully()
        {
            Uses = this.max_uses;
        }

        public void add_uses(int n)
        {
            Uses += n;
        }

        public void set_uses(int uses)
        {
            Uses =  Math.Min(max_uses, uses);
        }

        public int item_cost(bool buying, int buyPriceMod = 1)
        {
            if (Id == 0)
                return 0;

            int price, uses;
            if (buying)
            {
                uses = Math.Max(1, this.max_uses);
                price = (this.cost / buyPriceMod) * uses;
            }
            else
            {
                uses = Math.Max(1, this.Uses);
                price = (this.cost / 2) * uses;
            }
            return price;
        }
    }

    public interface IEquipmentService
    {
        Data_Equipment equipment(Item_Data data);
    }
}
