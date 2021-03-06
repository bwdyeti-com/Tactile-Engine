﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TactileLibrary.Pathfinding
{
    // only needs to be pubic while Tactile.Pathfind class exists //@Debug
    public class ClosedList<T> where T : IEquatable<T>
    {
        protected IEnumerable<T> Loc;
        protected List<int> Parent = new List<int>();
        List<int> Fcost = new List<int>();
        List<int> Gcost = new List<int>();
        List<bool> Accessible = new List<bool>();
        Dictionary<T, int> Locations = new Dictionary<T, int>();

        #region Accessors
        protected virtual int loc_count { get { return (Loc as HashSet<T>).Count; } }
        #endregion

        public ClosedList()
        {
            initialize_loc();
            //Locations = new int[Global.game_map.width() * Global.game_map.height()];
        }

        protected virtual void initialize_loc()
        {
            Loc = new HashSet<T>();
        }

        protected virtual void add_loc(T loc)
        {
            (Loc as HashSet<T>).Add(loc);
        }

        public int add_item(T loc, int parent, int fcost, int gcost, bool accessible)
        {
            add_loc(loc);
            Parent.Add(parent);
            Fcost.Add(fcost);
            Gcost.Add(gcost);
            Accessible.Add(accessible);

            Locations[loc] = loc_count;
            //Locations[(int)loc.X + (int)loc.Y * Global.game_map.width()] = Loc.Count;

            return loc_count - 1;
        }
        public int add_item(OpenItem<T> openItem)
        {
            return add_item(openItem.Loc, openItem.Parent,
                openItem.Fcost, openItem.Gcost, openItem.Accessible);
        }

        public bool already_added(T loc)
        {
            return Locations.ContainsKey(loc);
        }
        public int search(T loc)
        {
            if (!Locations.ContainsKey(loc))
                return -1;
            return Locations[loc] - 1;
            //return Locations[(int)loc.X + (int)loc.Y * Global.game_map.width()] - 1;

            //for (int i = 0; i < Loc.Count; i++)
            //{
            //    if (Loc[i] == loc) return i;
            //}
            return -1;
        }

        public virtual HashSet<T> get_range()
        {
            return Loc as HashSet<T>;
        }

        public Dictionary<T, int> GetMoveCosts()
        {
            var result = Locations
                .ToDictionary(p => p.Key, p => get_g(p.Value - 1));
            return result;
        }

        public int get_g(int index)
        {
            return Gcost[index];
        }
        public int GetParent(int index)
        {
            return Parent[index];
        }
    }
}
