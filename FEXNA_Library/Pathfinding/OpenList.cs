using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA_Library.Pathfinding
{
    // only needs to be pubic while FEXNA.Pathfind class exists //@Debug
    public class OpenList<T> where T : IEquatable<T>
    {
        int Current_Id = 0;
        List<OpenItem<T>> Items = new List<OpenItem<T>>();
        Dictionary<T, int> LocationIndices = new Dictionary<T, int>();
        int Size = 0;
        int Lowest_F_Id = -1;

        public int size { get { return Size; } }

        public void add_initial_item(T loc, int hcost)
        {
            if (Current_Id != 0)
                throw new ArgumentException();
            add_item(loc, -1, hcost, 0, true);
        }
        public void add_initial_item(T loc, int gcost, int hcost)
        {
            if (Current_Id != 0)
                throw new ArgumentException();
            add_item(loc, -1, gcost + hcost, gcost, true);
        }
        public void add_item(T loc, int parent, int fcost, int gcost, bool accessible)
        {
            Current_Id += 1;
            Size++;
            // Add new node to the bottom of the heap
            if ((this.size) > Items.Count)
                Items.Add(new OpenItem<T> { Id = Current_Id, Loc = loc, Parent = parent, Fcost = fcost, Gcost = gcost, Accessible = accessible });
            else
                Items[this.size - 1] = new OpenItem<T> { Id = Current_Id, Loc = loc, Parent = parent, Fcost = fcost, Gcost = gcost, Accessible = accessible };
            update_location_index(this.size - 1);

            // Bubble new node up through the tree to its correct position
            bubble_up(this.size);
        }

        private void swap(int index1, int index2)
        {
            OpenItem<T> temp = Items[index2];
            Items[index2] = Items[index1];
            Items[index1] = temp;

            update_location_index(index1);
            update_location_index(index2);
        }

        private void update_location_index(int index)
        {
            LocationIndices[Items[index].Loc] = index;
        }

        public void repoint(int index, int parent, int f, int g)
        {
            if (g < Items[index].Gcost)
            {
                Items[index] = Items[index].repoint(parent, f, g);
                bubble_up(index + 1);
            }
        }

        public OpenItem<T> remove_open_item()
        {
            // Get Item with the lowest F cost
            int remove_id = lowest_f_id();
            OpenItem<T> lowest_f_item = Items[remove_id];
            Lowest_F_Id = -1;

            // Reduce size of list
            LocationIndices.Remove(lowest_f_item.Loc);
            Size--;

            // If the removed item index is within the valid size range
            if (remove_id < this.size)
            {
                // Move last item into the removed item's position
                Items[remove_id] = Items[this.size];
                update_location_index(remove_id);
                // Percolate down the item that was moved up
                heapify(remove_id + 1);
            }

            return lowest_f_item;
        }

        private int bubble_up(int node)
        {
            int fcost = Items[node - 1].Fcost;
            while (node > 1)
            {
                // Compare to the parent node, swap with it if less or equal
                // (Less or equal prioritizes new nodes over old ones
                int parent_index = node / 2 - 1;
                if (fcost <= Items[parent_index].Fcost)
                {
                    swap(parent_index, node - 1);
                    node /= 2;
                }
                else
                    break;
            }

            return node;
        }

        private void heapify(int node)
        {
            // If sorting a node other than the root node, we need to check upwards as well
            if (node > 1)
            {
                int old_node = node;
                node = bubble_up(node);
            }

            for (; ; )
            {
                int child_node = node * 2;
                // If no children
                if (child_node > this.size)
                {
                    break;
                }
                else
                {
                    // If right child exists
                    if (child_node < this.size)
                    {
                        // Find the child with the lower value
                        if (Items[child_node - 1].Fcost > Items[child_node].Fcost)
                            child_node = child_node + 1;
                    }

                    // If child node is less than node, switch them to move it down
                    if (Items[child_node - 1].Fcost < Items[node - 1].Fcost)
                    {
                        swap(child_node - 1, node - 1);
                        node = child_node;
                    }
                    else
                        break;
                }
            }
        }

        public OpenItem<T> get_lowest_f_item()
        {
            int id = lowest_f_id();
            return Items[id];
        }

        public void rng_lowest_f_id(Func<bool> wiggleChance)
        {
            Lowest_F_Id = -1;
            if (this.size >= 2)
            {
                int index = 1;
                // If right child exists and is better than left child
                if (this.size >= 3)
                {
                    if (Items[2].Fcost < Items[1].Fcost)
                        index = 2;
                }
                // If a child of the root node is just as valid as the root node
                if (Items[index].Fcost == Items[0].Fcost &&
                    Items[index].Gcost == Items[0].Gcost)
                {
                    // Some chance to keep the base node instead of swapping
                    if (wiggleChance())
                        index = 0;
                    Lowest_F_Id = index;
                }
            }
        }

        protected int lowest_f_id()
        {
            if (Lowest_F_Id != -1)
                return Lowest_F_Id;
            return 0; // Hopefully the binary heap works correctly...! //Debug

            /* //Debug
            int index = 0;
            for (int i = 1; i < this.size; i++)
            {
                if (Items[index].Fcost > Items[i].Fcost)
                {
                    index = i;
                }
            }
            if (index != 0)
            {
                throw new Exception();
            }
            return index;*/
        }

        public int search(T loc)
        {
            if (LocationIndices.ContainsKey(loc))
            {
                /* //Debug
                if (!Items[LocationIndices[loc]].Loc.Equals(loc))
                {
                    throw new System.ArgumentException();
                }*/
                return LocationIndices[loc];
            }

            /* //Debug
            for (int i = 0; i < this.size; i++)
            {
                if (Items[i].Loc.Equals(loc))
                {
                    throw new System.ArgumentException();
                    return i;
                }
            }*/
            return -1;
        }
    }
}
