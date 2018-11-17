namespace FEXNA
{
    class Class_Service : FEXNA_Library.IClassService
    {
        public FEXNA_Library.Data_Class get_class(int id)
        {
            if (Global.data_classes.ContainsKey(id))
                return Global.data_classes[id];
            return null;
        }
    }
}
