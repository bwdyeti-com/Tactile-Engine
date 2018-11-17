using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNA_Library;
using ArrayExtension;
using DictionaryExtension;
using Vector2Extension;
using FEXNAArrayExtension;

namespace FEXNA.Metrics
{
    class Actor_Metrics
    {
        internal int Id { get; private set; }
        private string Name;
        private Vector2 Loc = Config.OFF_MAP;
	    private int ClassId;
	    private string ClassName;
	    private int Level;
	    private int Exp;
        private int Hp;
        private int[] Stats;
        private Item_Data[] Items;
        private string[] ItemNames;
	    private Dictionary<int, int> Supports = new Dictionary<int,int>();
	    private int Bond;
        private int Lives;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Name);
            Loc.write(writer);
            writer.Write(ClassId);
            writer.Write(ClassName);
            writer.Write(Level);
            writer.Write(Exp);
            writer.Write(Hp);
            Stats.write(writer);
            Items.write(writer);
            ItemNames.write(writer);
            Supports.write(writer);
            writer.Write(Bond);
            writer.Write(Lives);
        }

        public static Actor_Metrics read(BinaryReader reader)
        {
            Actor_Metrics result = new Actor_Metrics();
            result.Id = reader.ReadInt32();
            result.Name = reader.ReadString();
            result.Loc = result.Loc.read(reader);
            result.ClassId = reader.ReadInt32();
            result.ClassName = reader.ReadString();
            result.Level = reader.ReadInt32();
            result.Exp = reader.ReadInt32();
            result.Hp = reader.ReadInt32();
            result.Stats = result.Stats.read(reader);
            result.Items = result.Items.read(reader);
            result.ItemNames = result.ItemNames.read(reader);
            result.Supports.read(reader);
            result.Bond = reader.ReadInt32();
            result.Lives = reader.ReadInt32();
            return result;
        }
        #endregion

        public override string ToString()
        {
            return string.Format("{0}:{1}, {2}, Level: {3}.{4}, Hp: {5}/{6}",
                Id, Name, ClassName, Level, Exp.ToString("D2"), Hp, Stats[0]);
        }

        private Actor_Metrics() { }
        internal Actor_Metrics(Game_Unit unit) : this(unit.actor)
        {
            Loc = unit.loc;
        }
        internal Actor_Metrics(Game_Actor actor)
        {
            Id = actor.id;
            Name = actor.is_generic_actor ? actor.name_full : Global.data_actors[actor.id].Name;
	        ClassId = actor.class_id;
	        ClassName = actor.class_name_full;
	        Level = actor.level;
	        Exp = actor.exp;
            Hp = actor.hp;
            Stats = Enumerable.Range(0, Enum_Values.GetEnumCount(typeof(Stat_Labels))).Select(x => actor.stat(x)).ToArray();
            Items = actor.items.Select(x => new Item_Data(x)).ToArray();
            ItemNames = actor.items.Select(x => x.non_equipment ? "-----" : x.name).ToArray();
	        Supports = new Dictionary<int,int>(actor.supports);
	        Bond = actor.bond;
	        Lives = actor.lives;
        }
        internal Actor_Metrics(Actor_Metrics other)
        {
            Id = other.Id;
            Name = other.Name;
            ClassId = other.ClassId;
            ClassName = other.ClassName;
            Level = other.Level;
            Exp = other.Exp;
            Hp = other.Hp;
            Stats = other.Stats.Select(x => x).ToArray();
            Items = other.Items.Select(x => new Item_Data(x)).ToArray();
            ItemNames = other.ItemNames.Select(x => x).ToArray();
            Supports = new Dictionary<int, int>(other.Supports);
            Bond = other.Bond;
            Lives = other.Lives;
        }
    }
}
