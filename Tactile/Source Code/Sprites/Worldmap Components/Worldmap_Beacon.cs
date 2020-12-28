using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Worldmap_Beacon : Sprite
    {
        const int BEACON_TIME = 60;
        protected int Beacon_Timer = 0;

        public Worldmap_Beacon()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Worldmap_Sprites");
            Src_Rect = new Rectangle(0, 64, 48, 48);
            offset = new Vector2(24, 24);
        }

        public override void update()
        {
            Beacon_Timer = (Beacon_Timer + 1) % BEACON_TIME;
            int alpha = ((BEACON_TIME - Beacon_Timer) * 255) / BEACON_TIME;
            tint = new Color(alpha, alpha, alpha, alpha);
            scale = new Vector2(Beacon_Timer / (float)BEACON_TIME);
        }
    }
}
