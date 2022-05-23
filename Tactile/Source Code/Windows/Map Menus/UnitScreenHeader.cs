namespace Tactile
{
    struct UnitScreenHeader
    {
        public int Index { get; private set; }
        public string Name { get; private set; }
        public int Page { get; private set; }

        public UnitScreenHeader(
            int index,
            string name,
            int page)
        {
            Index = index;
            Name = name;
            Page = page;
        }
    }
}
