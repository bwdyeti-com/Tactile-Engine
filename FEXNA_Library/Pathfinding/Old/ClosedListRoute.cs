using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA_Library.Pathfinding.Old
{
    class ClosedListRoute : ClosedList
    {
        protected override void initialize_elements()
        {
            Elements = new List<int>();
        }

        protected override void add_element(int index)
        {
            (Elements as List<int>).Add(index);
        }

        private int element(int index)
        {
            return (Elements as List<int>)[index];
        }

        public List<int> get_route(int temp_id)
        {
            List<int> list = new List<int>();
            int temp_parent;
            do
            {
                temp_parent = Parent[temp_id];
                list.Add(element(temp_id));
                temp_id = temp_parent;
            } while (temp_parent != -1);
            return list;
        }

        public override HashSet<int> get_range()
        {
            return new HashSet<int>(Elements);
        }
    }
}
