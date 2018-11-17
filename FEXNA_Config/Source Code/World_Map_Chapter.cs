using Microsoft.Xna.Framework;
namespace FEXNA
{
    public struct World_Map_Chapter
    {
        public string Id;
        public string World_Map_Name;
        public Vector2 World_Map_Loc;
        public int World_Map_Lord_Id;
        public bool Standalone;
        public Preset_Chapter_Data Preset_Data;

        public override string ToString()
        {
            return string.Format("{0}, {1}", Id, World_Map_Name);
        }
    }

    public struct Preset_Chapter_Data
    {
        public int Lord_Lvl;
        public int Units;
        public int Battalion;
        public int Gold;
        public int Playtime;
    }
}
