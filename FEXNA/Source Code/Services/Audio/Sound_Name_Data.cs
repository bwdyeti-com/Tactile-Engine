using FEXNA_Library;

namespace FEXNA.Services.Audio
{
    class Sound_Name_Data
    {
        public string Bank { get; private set; }
        public string Name { get; private set; }
        public bool Priority { get; private set; }
        public Maybe<float> Pitch { get; private set; }

        public Sound_Name_Data(string bank, string name, bool priority, Maybe<float> pitch)
        {
            this.Bank = bank;
            this.Name = name;
            this.Priority = priority;
            this.Pitch = pitch;
        }
    }
}
