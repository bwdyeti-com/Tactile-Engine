using System;
using System.Collections.Generic;
using System.IO;
using FEXNADictionaryExtension;
using FEXNA_Library;
using FEXNAVersionExtension;

namespace FEXNA.Map
{
    class MapObjects
    {
        private Dictionary<int, Game_Unit> Units = new Dictionary<int, Game_Unit>();
        private Dictionary<string, int> UnitIdentifiers = new Dictionary<string, int>();
        private Dictionary<int, Destroyable_Object> DestroyableObjects = new Dictionary<int, Destroyable_Object>();
        private Dictionary<int, Siege_Engine> SiegeEngines = new Dictionary<int, Siege_Engine>();
        private Dictionary<int, LightRune> LightRunes = new Dictionary<int, LightRune>();
        
        #region Serialization
        public void write(BinaryWriter writer)
        {
            Units.write(writer);
            UnitIdentifiers.write(writer);
            DestroyableObjects.write(writer);
            SiegeEngines.write(writer);
            LightRunes.write(writer);
        }

        public void read(BinaryReader reader)
        {
            Units.read(reader);
            UnitIdentifiers.read(reader);
            DestroyableObjects.read(reader);
            SiegeEngines.read(reader);
            if (!Global.LOADED_VERSION.older_than(0, 5, 6, 4)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                LightRunes.read(reader);
            }
        }
        #endregion

        #region Accessors
        internal Dictionary<int, Game_Unit> units { get { return Units; } }
        internal Dictionary<int, Destroyable_Object> destroyable_objects { get { return DestroyableObjects; } }
        internal Dictionary<int, Siege_Engine> siege_engines { get { return SiegeEngines; } }

        public Dictionary<string, int> unit_identifiers { get { return UnitIdentifiers; } }
        #endregion

        internal void reset()
        {
            Units.Clear();
            UnitIdentifiers.Clear();
            DestroyableObjects.Clear();
            SiegeEngines.Clear();
            LightRunes.Clear();
        }

        internal void init_sprites()
        {
            foreach (Game_Unit unit in Units.Values)
                unit.init_sprites();
            foreach (Siege_Engine siege in SiegeEngines.Values)
                siege.init_sprites();
            foreach (LightRune rune in LightRunes.Values)
                rune.init_sprites();
        }

        internal Map_Object this[int id]
        {
            get
            {
                if (Units.ContainsKey(id))
                    return Units[id];
                if (DestroyableObjects.ContainsKey(id))
                    return DestroyableObjects[id];
                if (SiegeEngines.ContainsKey(id))
                    return SiegeEngines[id];
                if (LightRunes.ContainsKey(id))
                    return LightRunes[id];
                return null;
            }
        }

        public Combat_Map_Object attackable_map_object(int id)
        {
            if (Units.ContainsKey(id))
                return Units[id];
            if (DestroyableObjects.ContainsKey(id))
                return DestroyableObjects[id];
            if (LightRunes.ContainsKey(id))
                return LightRunes[id];
            return null;
        }

        public IEnumerable<Combat_Map_Object> enumerate_combat_objects()
        {
            foreach (Game_Unit unit in Units.Values)
                yield return unit;
            foreach (Destroyable_Object destroyable in DestroyableObjects.Values)
                yield return destroyable;
            foreach (LightRune rune in LightRunes.Values)
                yield return rune;
        }

        public Map_Object displayed_map_object(int id)
        {
            if (Units.ContainsKey(id))
                return Units[id];
            if (SiegeEngines.ContainsKey(id))
                return SiegeEngines[id];
            if (LightRunes.ContainsKey(id))
                return LightRunes[id];
            return null;
        }

        internal void update()
        {
            foreach (var unit in Units)
                unit.Value.update();
            foreach (var siege in SiegeEngines)
                siege.Value.update();
            foreach (var rune in LightRunes)
                rune.Value.update();
        }

        #region Units
        internal Game_Unit add_unit(Game_Unit unit, string identifier,
            Maybe<int> mission = default(Maybe<int>))
        {
            Units.Add(unit.id, unit);
            if (identifier != "")
                UnitIdentifiers[identifier] = unit.id;
            if (mission.IsSomething)
                unit.full_ai_mission = mission;
            return unit;
        }

        internal void remove_unit(int id)
        {
            if (!Units[id].gladiator)
                Global.game_state.Update_Victory_Theme = true;
            Units[id].remove_old_unit_location();
            Units.Remove(id);
        }

        internal bool unit_exists(int id)
        {
            return Units.ContainsKey(id);
        }

        internal int unit_distance(int id1, int id2)
        {
            Game_Unit unit1 = Units[id1];
            if (unit1.is_rescued)
                unit1 = Units[unit1.rescued];
            Combat_Map_Object unit2 = attackable_map_object(id2);
            if (unit2.is_unit() && ((Game_Unit)unit2).is_rescued)
                unit2 = Units[((Game_Unit)unit2).rescued];
            return (int)(Math.Abs(unit1.loc.X - unit2.loc.X) + Math.Abs(unit1.loc.Y - unit2.loc.Y));
        }
        #endregion

        #region Destroyable Objects
        internal void add_destroyable_object(Destroyable_Object destroyable)
        {
            DestroyableObjects.Add(destroyable.id, destroyable);
        }

        internal void remove_destroyable(int id)
        {
            Global.game_state.activate_event_by_name(DestroyableObjects[id].event_name);
            DestroyableObjects.Remove(id);
        }

        internal Destroyable_Object destroyable(int id)
        {
            return DestroyableObjects[id];
        }

        internal bool destroyable_exists(int id)
        {
            return DestroyableObjects.ContainsKey(id);
        }

        public IEnumerable<Destroyable_Object> enumerate_destroyables()
        {
            return DestroyableObjects.Values;
        }
        #endregion

        #region Siege Engines
        internal void add_siege_engine(Siege_Engine siege)
        {
            SiegeEngines.Add(siege.id, siege);
        }

        internal void remove_siege_engine(int id)
        {
            SiegeEngines.Remove(id);
        }
        #endregion

        #region Light Runes
        internal void add_light_rune(LightRune rune)
        {
            LightRunes.Add(rune.id, rune);
        }

        internal void remove_light_rune(int id)
        {
            LightRunes.Remove(id);
        }

        internal LightRune light_rune(int id)
        {
            return LightRunes[id];
        }

        internal bool light_rune_exists(int id)
        {
            return LightRunes.ContainsKey(id);
        }

        public IEnumerable<LightRune> enumerate_light_runes()
        {
            return LightRunes.Values;
        }
        #endregion

        internal bool id_in_use(int id)
        {
            return Units.ContainsKey(id) || DestroyableObjects.ContainsKey(id) ||
                SiegeEngines.ContainsKey(id) || LightRunes.ContainsKey(id);
        }
    }
}
