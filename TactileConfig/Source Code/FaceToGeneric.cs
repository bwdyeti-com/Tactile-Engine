namespace Tactile
{
    public struct FaceToGeneric
    {
        public string GraphicName { get; private set; }
        public string RecolorCountry { get; private set; }
        public string FlagCountry { get; private set; }
        
        internal FaceToGeneric(string graphicName, string recolorCountry) :
            this(graphicName, recolorCountry, recolorCountry) { }
        internal FaceToGeneric(
            string graphicName,
            string recolorCountry,
            string flagCountry) : this()
        {
            GraphicName = graphicName;
            RecolorCountry = recolorCountry;
            FlagCountry = flagCountry;
        }
    }
}
