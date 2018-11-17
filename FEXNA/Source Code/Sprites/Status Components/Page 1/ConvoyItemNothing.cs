namespace FEXNA
{
    class ConvoyItemNothing : Status_Item
    {
        internal ConvoyItemNothing()
        {
            Icon.texture = null;
            Name.text = "Nothing";
            Uses.text = "";
            Slash.text = "";
            Use_Max.text = "";

            set_text_color(false);
        }
    }
}
