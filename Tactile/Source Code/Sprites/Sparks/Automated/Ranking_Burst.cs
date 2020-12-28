using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Ranking_Burst : Spark
    {
        public readonly static string FILENAME = "Ranking_Burst";

        public Ranking_Burst()
        {
            Loop = true;
            Timer_Maxes = new int[] { 7, 7, 7, 7, 7, 7, 7, 7 };
            Frames = new Vector2(5, 1);
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + FILENAME);
            offset = new Vector2(200, 208);
        }

        public override void update()
        {
            base.update();
            //angle += MathHelper.Pi / 3600; //Debug
        }

        public override Rectangle src_rect
        {
            get
            {
                int frame = Frame;
                if (Frame >= Frames.X)
                    frame = Frame - (Frame - ((int)Frames.X - 1)) * 2;
                return new Rectangle((frame % (int)Frames.X) * (int)(texture.Width / Frames.X),
                    (frame / (int)Frames.X) * (int)(texture.Height / Frames.Y),
                    (int)(texture.Width / Frames.X), (int)(texture.Height / Frames.Y));
            }
        }
    }
}
