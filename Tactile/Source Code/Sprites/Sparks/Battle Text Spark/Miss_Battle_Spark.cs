using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Miss_Battle_Spark : Battle_Text_Spark
    {
        public Miss_Battle_Spark()
        {
            initialize();
        }

        protected override Texture2D get_texture()
        {
            Texture2D texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Miss");
            Size = new Vector2(texture.Width / 5, texture.Height / 4);
            return texture;
        }

        protected override void update_frame()
        {
            if (Timer < 20)
                Src_Rect = new Rectangle((int)Size.X * (Timer % 5), (int)Size.Y * (Timer / 5), (int)Size.X, (int)Size.Y);
            Timer++;
        }

        public override void remove()
        {
            remove(23);
        }
        public override void remove(int time)
        {
            if (Remove)
                return;
            Remove = true;
            Remove_Timer = time;
        }
    }
}
