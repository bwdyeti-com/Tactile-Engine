using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class No_Damage_Battle_Spark : Battle_Text_Spark
    {
        public No_Damage_Battle_Spark()
        {
            initialize();
        }

        protected override Texture2D get_texture()
        {
            Texture2D texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/NoDamage");
            Size = new Vector2(texture.Width / 5, texture.Height / 5);
            return texture;
        }

        protected override void update_frame()
        {
            if (Timer < 25)
                Src_Rect = new Rectangle((int)Size.X * (Timer % 5), (int)Size.Y * (Timer / 5), (int)Size.X, (int)Size.Y);
            Timer++;
        }

        public override void remove()
        {
            remove(48);
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
