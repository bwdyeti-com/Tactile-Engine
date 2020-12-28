using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TactileLibrary;

namespace Tactile.Map
{
    internal class Team_Escape_Data : Dictionary<int, Dictionary<Vector2, Vector2>>
    {
        internal void add_point(int group, Vector2 loc, Vector2 escape_to_loc)
        {
            if (!this.ContainsKey(group))
                Add(group, new Dictionary<Vector2, Vector2>());
            if (!this[group].ContainsKey(loc))
                this[group].Add(loc, escape_to_loc);
        }

        internal Maybe<Vector2> get_escape_point(int group, Vector2 loc)
        {
            if (ContainsKey(group))
                if (this[group].ContainsKey(loc))
                    return this[group][loc];
            return default(Maybe<Vector2>);
        }
    }
}
