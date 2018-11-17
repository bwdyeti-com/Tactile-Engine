using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA.Pathfinding
{
    class ClosedListRoute<T> : ClosedList<T> where T : IEquatable<T>
    {
        #region Accessors
        protected override int loc_count { get { return (Loc as List<T>).Count; } }
        #endregion

        protected override void initialize_loc()
        {
            Loc = new List<T>();
        }

        protected override void add_loc(T loc)
        {
            (Loc as List<T>).Add(loc);
        }

        private T loc(int index)
        {
            return (Loc as List<T>)[index];
        }

        public List<T> get_route(int temp_id)
        {
            List<T> list = new List<T>();
            while (temp_id != -1)
            {
                list.Add(loc(temp_id));
                temp_id = Parent[temp_id];
            }
            return list;
        }
        public List<T> get_reverse_route(int temp_id, T starting_loc)
        {
            List<T> list = new List<T>();
            list.Add(starting_loc);
            while (temp_id != -1)
            {
                list.Insert(0, loc(temp_id));
                temp_id = Parent[temp_id];
            }
            return list;
        }

        public override HashSet<T> get_range()
        {
            return new HashSet<T>(Loc);
        }
    }
}
