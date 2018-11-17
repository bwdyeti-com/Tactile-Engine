using Microsoft.Xna.Framework;

namespace FEXNA
{
    class Worldmap_Dot : Flashing_Worldmap_Object
    {
        public Worldmap_Dot(int team)
        {
            Team = team;
            offset = new Vector2(4, 4);
        }

        public override void update()
        {
            Src_Rect = new Rectangle((Team - 1) * 24, Flash_Frame * 16, 8, 8);
        }
    }
}
