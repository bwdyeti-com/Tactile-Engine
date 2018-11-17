using Microsoft.Xna.Framework;

namespace FEXNA.Pathfinding
{
    // struct? //Yeti
    struct OpenItem<T>
    //class OpenItem //Yeti
    {
        public int Id;
        public T Loc;
        public int Parent;
        public int Fcost;
        public int Gcost;
        public bool Accessible;

        public override string ToString()
        {
            if (Loc is Vector2)
                return string.Format(
                    "loc:({0}, {1}), order: {2}, moved: {3}, remaining: {4}",
                    (int)(Loc as Vector2?).Value.X,
                    (int)(Loc as Vector2?).Value.Y,
                    Fcost / 10, Gcost / 10, (Fcost - Gcost) / 10);
            else
                return string.Format(
                    "loc:{0}, order: {1}, moved: {2}, remaining: {3}",
                    Loc, Fcost / 10, Gcost / 10, (Fcost - Gcost) / 10);
        }

        public OpenItem<T> repoint(int parent, int f, int g)
        {
            return new OpenItem<T>
            {
                Id = this.Id,
                Loc = this.Loc,
                Parent = parent,
                Fcost = f,
                Gcost = g,
                Accessible = this.Accessible
            };
        }
    }
}
