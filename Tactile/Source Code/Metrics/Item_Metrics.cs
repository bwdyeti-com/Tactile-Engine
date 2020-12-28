using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using TactileLibrary;
using TactileListExtension;
using TactileVector2Extension;
using TactileVersionExtension;

namespace Tactile.Metrics
{
    class Item_Metrics
    {
        internal int Turn { get; private set; }
        internal int Team { get; private set; }
        internal int ActorId { get; private set; }
        internal int ClassId { get; private set; }
        internal Item_Data ItemData { get; private set; }
        internal string ItemName { get; private set; }
        internal Vector2 Loc { get; private set; }

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Turn);
            writer.Write(Team);
            writer.Write(ActorId);
            writer.Write(ClassId);
            ItemData.write(writer);
            writer.Write(ItemName);
            Loc.write(writer);
        }

        public static Item_Metrics read(BinaryReader reader)
        {
            Item_Metrics result = new Item_Metrics();
            if (!Global.LOADED_VERSION.older_than(0, 4, 6, 9))
                result.Turn = reader.ReadInt32();
            result.Team = reader.ReadInt32();
            result.ActorId = reader.ReadInt32();
            result.ClassId = reader.ReadInt32();
            result.ItemData = Item_Data.read(reader);
            result.ItemName = reader.ReadString();
            result.Loc = result.Loc.read(reader);
            return result;
        }
        #endregion

        public override string ToString()
        {
            return string.Format("Combat Metrics: Turn {0}, Actor {1}, {2}", Turn, ActorId, ItemData.name);
        }

        private Item_Metrics() { }
        public Item_Metrics(int turn, Game_Unit unit, Item_Data item_data)
        {
            Turn = turn;
            Team = unit.team;
            ActorId = unit.actor.id;
            ClassId = unit.actor.class_id;
            ItemData = item_data;
            ItemName = item_data.to_equipment.full_name();
            Loc = unit.loc;
        }
        internal Item_Metrics(Item_Metrics other)
        {
            Turn = other.Turn;
            Team = other.Team;
            ActorId = other.ActorId;
            ClassId = other.ClassId;
            ItemData = new Item_Data(other.ItemData);
            ItemName = other.ItemName;
            Loc = other.Loc;
        }
    }
}
