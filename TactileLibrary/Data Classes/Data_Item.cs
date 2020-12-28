using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using ArrayExtension;
using ListExtension;

namespace TactileLibrary
{
    public enum Placeables { None, Mine, Light_Rune }
    public enum Buffs { Pow, Skl, Spd, Lck, Def, Res, Mov, Con }
    public enum Boosts { MaxHp, Pow, Skl, Spd, Lck, Def, Res, Mov, Con, WLvl, WExp }
    public partial class Data_Item : Data_Equipment
    {
        public int Heal_Val = 10;
        public float Heal_Percent = 0;
        public bool Door_Key = false;
        public bool Chest_Key = false;
        public bool Dancer_Ring = false;
        public int Torch_Radius = 0;
        public Placeables Placeable = Placeables.None;
        [ContentSerializer(Optional = true)]
        public int Repair_Val = 0;
        [ContentSerializer(ElementName = "Repair_Rate")]
        public float Repair_Percent = 0f;
        public string Boost_Text = "";
        public int[] Stat_Boost = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public int[] Growth_Boost = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        public int[] Stat_Buff = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<int> Promotes = new List<int>();

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static Data_Item GetEmptyInstance()
        {
            return new Data_Item();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var item = (Data_Item)other;

            copy_traits(item);

            Heal_Val = item.Heal_Val;
            Heal_Percent = item.Heal_Percent;
            Door_Key = item.Door_Key;
            Chest_Key = item.Chest_Key;
            Dancer_Ring = item.Dancer_Ring;
            Torch_Radius = item.Torch_Radius;
            Placeable = item.Placeable;
            Repair_Val = item.Repair_Val;
            Repair_Percent = item.Repair_Percent;
            Boost_Text = item.Boost_Text;
            Stat_Boost = new int[item.Stat_Boost.Length];
            Array.Copy(item.Stat_Boost, Stat_Boost, Stat_Boost.Length);
            Growth_Boost = new int[item.Growth_Boost.Length];
            Array.Copy(item.Growth_Boost, Growth_Boost, Growth_Boost.Length);
            Stat_Buff = new int[item.Stat_Buff.Length];
            Array.Copy(item.Stat_Buff, Stat_Buff, Stat_Buff.Length);
            Promotes = new List<int>(item.Promotes);
        }

        public static Data_Item ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader reader)
        {
            read_equipment(reader);

            Heal_Val = reader.ReadInt32();
            Heal_Percent = (float)reader.ReadDouble();
            Door_Key = reader.ReadBoolean();
            Chest_Key = reader.ReadBoolean();
            Dancer_Ring = reader.ReadBoolean();
            Torch_Radius = reader.ReadInt32();
            Placeable = (Placeables)reader.ReadInt32();
            Repair_Val = reader.ReadInt32();
            Repair_Percent = (float)reader.ReadDouble();
            Boost_Text = reader.ReadString();
            Stat_Boost = Stat_Boost.read(reader);
            Growth_Boost = Growth_Boost.read(reader);
            Stat_Buff = Stat_Buff.read(reader);
            Promotes.read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            base.write_equipment(writer);

            writer.Write(Heal_Val);
            writer.Write((double)Heal_Percent);
            writer.Write(Door_Key);
            writer.Write(Chest_Key);
            writer.Write(Dancer_Ring);
            writer.Write(Torch_Radius);
            writer.Write((int)Placeable);
            writer.Write(Repair_Val);
            writer.Write((double)Repair_Percent);
            writer.Write(Boost_Text);
            Stat_Boost.write(writer);
            Growth_Boost.write(writer);
            Stat_Buff.write(writer);
            Promotes.write(writer);
        }
        #endregion

        public override string ToString()
        {
            return ToString(0);
        }
        public override string ToString(int uses_left)
        {
            return String.Format("Item: {0}, Uses {1}",
                full_name(), uses_left == 0 ? Uses.ToString() : string.Format("{0}/{1}", uses_left, Uses));
        }

        public Data_Item() { }
        public Data_Item(Data_Item item)
        {
            CopyFrom(item);
        }

        public bool can_heal_hp()
        {
            return (Heal_Val > 0 || Heal_Percent > 0);
        }
        public bool is_for_healing()
        {
            return (Heal_Val > 0 || Heal_Percent > 0 || Status_Remove.Count > 0);
        }
        public bool can_heal(bool hp_full, List<int> statuses)
        {
            return (!hp_full && (Heal_Val > 0 || Heal_Percent > 0)) || Status_Remove.Intersect(statuses).Any();
        }

        public bool can_repair { get { return Repair_Val > 0 || Repair_Percent > 0; } }

        public bool is_placeable()
        {
            return Placeable != Placeables.None;
        }

        public bool is_stat_booster()
        {
            foreach (int stat in Stat_Boost)
                if (stat > 0) return true;
            return false;
        }

        public bool is_growth_booster()
        {
            foreach (int stat in Growth_Boost)
                if (stat > 0) return true;
            return false;
        }

        public bool is_stat_buffer()
        {
            foreach (int stat in Stat_Buff)
                if (stat > 0) return true;
            return false;
        }

        public bool targets_inventory()
        {
            return can_repair;
        }

        public bool can_target_item(Item_Data item_data)
        {
            if (can_repair && item_data.is_weapon)
            {
                Data_Weapon weapon = item_data.to_weapon;
                if (!weapon.is_staff() && weapon.Uses > 0 && item_data.Uses < weapon.Uses)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the value of a healing item
        /// </summary>
        public int healing_item_value()
        {
            if (!can_heal_hp())
                return 0;

            int value = ((int)Heal_Percent * 50 + Heal_Val) / 5;

            return value;
        }

        /// <summary>
        /// Gets the value of an accessory
        /// </summary>
        public int accessory_value()
        {
            if (Skills.Count == 0)
                return 0;

            int value = (Cost == 0 ? 8000 : full_price()) / 200;

            return value;
        }

        /// <summary>
        /// Gets the value of an dancer ring
        /// </summary>
        public int dance_value()
        {
            if (!Dancer_Ring)
                return 0;

            int value = (Cost == 0 ? 7500 : full_price()) / 200;

            return value;
        }

        public override object Clone()
        {
            return new Data_Item(this);
        }
    }
}
