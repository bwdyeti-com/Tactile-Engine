namespace Tactile
{
    public struct FaceCountryRecolor
    {
        public string RecolorCountry { get; private set; }
        public bool UseOwnFlag { get; private set; }

        internal FaceCountryRecolor(
            string recolorCountry,
            bool useOwnFlag = false) : this()
        {
            RecolorCountry = recolorCountry;
            UseOwnFlag = useOwnFlag;
        }
    }
}
