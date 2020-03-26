using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA_Library.Pathfinding.Old
{
    class OpenList
    {
        int Current_Id = 0;
        List<OpenItem> Items = new List<OpenItem>();
        int Size = 0;
        int Lowest_F_Id = -1;

        public int size { get { return Size; } }

        public void add_item(int index, int parent, int fcost, int gcost, bool accessible)
        {
            Current_Id += 1;
            Size++;
            if ((size) > Items.Count)
                Items.Add(new OpenItem { Id = Current_Id, Index = index, Parent = parent, Fcost = fcost, Gcost = gcost, Accessible = accessible });
            else
                Items[size - 1] = new OpenItem { Id = Current_Id, Index = index, Parent = parent, Fcost = fcost, Gcost = gcost, Accessible = accessible };
            int i = size;
            while (i != 1)
            {
                if (fcost <= Items[i / 2 - 1].Fcost)
                {
                    OpenItem temp = Items[i / 2 - 1];
                    Items[i / 2 - 1] = Items[i - 1];
                    Items[i - 1] = temp;
                    i /= 2;
                }
                else
                    break;
            }
        }

        public void repoint(int index, int parent, int f, int g)
        {
            if (g < Items[index].Gcost)
            {
                Items[index].Parent = parent;
                Items[index].Fcost = f;
                Items[index].Gcost = g;

                int i = index + 1;
                while (i != 1)
                {
                    if (Items[i - 1].Fcost <= Items[i / 2 - 1].Fcost)
                    {
                        OpenItem temp = Items[i / 2 - 1];
                        Items[i / 2 - 1] = Items[i - 1];
                        Items[i - 1] = temp;
                        i /= 2;
                    }
                    else
                        break;
                }
            }
        }

        public void remove_open_item()
        {
            int remove_id = lowest_f_id();
            Items[remove_id] = Items[size - 1];
            Size--;
            Lowest_F_Id = -1;
            //Items.RemoveAt(Items.Count - 1);
            resort();
        }

        private void resort()
        {
            int i1 = 1;
            for (; ; )
            {
                int i2 = i1;
                if (i2 * 2 + 1 <= size)
                {
                    if (Items[i2 - 1].Fcost >= Items[i2 * 2 - 1].Fcost)
                        i1 = i2 * 2;
                    if (Items[i1 - 1].Fcost >= Items[i2 * 2].Fcost)
                        i1 = i2 * 2 + 1;
                }
                else if (i2 * 2 <= size)
                {
                    if (Items[i2 - 1].Fcost >= Items[i2 * 2 - 1].Fcost)
                        i1 = i2 * 2;
                }

                if (i1 != i2)
                {
                    OpenItem temp = Items[i2 - 1];
                    Items[i2 - 1] = Items[i1 - 1];
                    Items[i1 - 1] = temp;
                }
                else
                    break;
            }
        }

        public OpenItem get_lowest_f_item()
        {
            int id = lowest_f_id();
            return Items[id];
        }

        public void rng_lowest_f_id()
        {
            //return 0; // Hopefully the binary heap works correctly...! //Debug
            int index1 = 0, index2 = -1;
            //int min_f1 = Items[index1].Fcost, min_f2 = -1;
            for (int i = 1; i < size; i++)
            {
                //if (min_f1 > Items[i].Fcost)
                if (Items[index1].Fcost > Items[i].Fcost)
                {
                    index2 = index1;
                    //min_f2 = min_f1;
                    index1 = i;
                    //min_f1 = Items[index1].Fcost;
                }
                else if (Items[index1].Fcost == Items[i].Fcost && Items[index1].Gcost == Items[i].Gcost)
                {
                    index2 = i;
                    //min_f2 = Items[index1].Fcost;
                }
            }
            if (index1 != 0)
            {
                int test = 0;
                test++;
            }
            if (index2 != -1 && Items[index1].Gcost == Items[index2].Gcost)
            {
                if (use_second_index())
                    index1 = index2;
                Lowest_F_Id = index1;
            }
            else
                Lowest_F_Id = index1;
        }

        private bool use_second_index()
        {
            return false;
            //return !Global.game_system.roll_rng(50); //Debug
        }

        protected int lowest_f_id()
        {
            if (Lowest_F_Id != -1)
                return Lowest_F_Id;
            //return 0; // Hopefully the binary heap works correctly...! //Debug
            int index = 0;
            //int min_f = Items[index].Fcost;
            for (int i = 1; i < size; i++)
            {
                if (Items[index].Fcost > Items[i].Fcost)
                {
                    index = i;
                    //min_f = Items[index].Fcost;
                }
            }
            if (index != 0)
            {
                int test = 0;
                test++;
            }
            return index;
        }

        public int search(int index)
        {
            for (int i = 0; i < size; i++)
            {
                if (Items[i].Index == index)
                    return i;
            }
            return -1;
        }
    }
}
