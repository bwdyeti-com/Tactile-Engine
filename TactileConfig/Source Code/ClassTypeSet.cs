using System.Collections.Generic;
using System.Linq;
using TactileLibrary;

namespace Tactile
{
    public struct ClassTypeSet
    {
        public HashSet<ClassTypes> Types { get; private set; }

        public ClassTypeSet(IEnumerable<ClassTypes> types) : this()
        {
            Types = new HashSet<ClassTypes>(types);
        }
        public ClassTypeSet(params ClassTypes[] types) : this()
        {
            Types = new HashSet<ClassTypes>(types);
        }

        public override int GetHashCode()
        {
            int hash = 18;
            SortedSet<ClassTypes> types = new SortedSet<ClassTypes>(Types);
            foreach (var type in types)
                hash = hash * 31 + (int)type;
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is ClassTypeSet)
                return Equals((ClassTypeSet)obj);
            if (obj is List<ClassTypes>)
                return Equals(obj as List<ClassTypes>);
            return false;
        }
        public bool Equals(ClassTypeSet other)
        {
            return Types.Count == other.Types.Count && Types.Intersect(other.Types).Count() == Types.Count;
        }
        public bool Equals(List<ClassTypes> other)
        {
            return Types.Intersect(other).Count() == Types.Count;
        }
    }
}
