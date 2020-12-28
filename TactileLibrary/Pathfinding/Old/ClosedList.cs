using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TactileLibrary.Pathfinding.Old
{
    class ClosedList
    {
        protected IEnumerable<int> Elements;
        protected List<int> Parent = new List<int>();
        List<int> Fcost = new List<int>();
        List<int> Gcost = new List<int>();
        List<bool> Accessible = new List<bool>();
        Dictionary<int, int> ElementDictionary = new Dictionary<int, int>();

        #region Accessors
        protected virtual int size { get { return Elements.Count(); } }
        #endregion

        public ClosedList()
        {
            initialize_elements();
            //Locations = new int[Global.game_map.width() * Global.game_map.height()];
        }

        protected virtual void initialize_elements()
        {
            Elements = new HashSet<int>();
        }

        protected virtual void add_element(int index)
        {
            (Elements as HashSet<int>).Add(index);
        }

        public int add_item(int index, int parent, int fcost, int gcost, bool accessible)
        {
            add_element(index);
            Parent.Add(parent);
            Fcost.Add(fcost);
            Gcost.Add(gcost);
            Accessible.Add(accessible);
            ElementDictionary[index] = size;
            //Locations[(int)loc.X + (int)loc.Y * Global.game_map.width()] = Loc.Count;

            return size - 1;
        }

        public int search(int index)
        {
            if (!ElementDictionary.ContainsKey(index))
                return -1;
            return ElementDictionary[index] - 1;
            //return Locations[(int)loc.X + (int)loc.Y * Global.game_map.width()] - 1;

            //for (int i = 0; i < Loc.Count; i++)
            //{
            //    if (Loc[i] == loc) return i;
            //}
            return -1;
        }

        public virtual HashSet<int> get_range()
        {
            return Elements as HashSet<int>;
        }

        public int get_g(int index)
        {
            return Gcost[index];
        }
    }
}
