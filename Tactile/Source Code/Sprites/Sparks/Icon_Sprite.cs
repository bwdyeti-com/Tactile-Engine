using Microsoft.Xna.Framework;

namespace Tactile
{
    class Icon_Sprite : Sprite
    {
        public int Index = 0;
        public int columns = 1;
        public Vector2 size = Vector2.Zero;

        #region Accessors
        public virtual int index
        {
            get { return Index; }
            set { Index = value; }
        }
        #endregion

        public override Rectangle src_rect
        {
            get
            {
                return new Rectangle((int)(size.Y * (Index % columns)), (int)(size.Y * (Index / columns)), (int)size.X, (int)size.Y);
            }
        }
    }
}
