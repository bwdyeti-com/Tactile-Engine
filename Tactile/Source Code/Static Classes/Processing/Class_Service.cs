namespace Tactile
{
    class Class_Service : TactileLibrary.IClassService
    {
        public TactileLibrary.Data_Class get_class(int id)
        {
            if (Global.data_classes.ContainsKey(id))
                return Global.data_classes[id];
            return null;
        }
    }
}
