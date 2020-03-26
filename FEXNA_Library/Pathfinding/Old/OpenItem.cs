using Microsoft.Xna.Framework;

namespace FEXNA_Library.Pathfinding.Old
{
    class OpenItem
    {
        public int Id;
        public int Index;
        public int Parent;
        public int Fcost;
        public int Gcost;
        public bool Accessible;

        public override string ToString()
        {
            return string.Format(
                "OpenItem: Index = {0}, Order = {1}, Moved = {2}, Remaining = {3}",
                Index, Fcost / 10, Gcost / 10, (Fcost - Gcost) / 10);
        }
    }
}
