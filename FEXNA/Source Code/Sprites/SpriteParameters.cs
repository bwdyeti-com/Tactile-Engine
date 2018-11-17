using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics
{
    struct SpriteParameters
    {
        internal Vector2 Offset { get; private set; }
        internal Vector2 Scale { get; private set; }
        internal Color Tint { get; private set; }
        internal Rectangle SrcRect { get; private set; }
        internal Color ColorShift { get; private set; }

        internal SpriteParameters(
            Vector2 offset = default(Vector2),
            Vector2 scale = default(Vector2),
            Color tint = default(Color),
            Rectangle srcRect = default(Rectangle),
            Color colorShift = default(Color))
            : this()
        {
            this.Offset = offset;
            this.Scale = scale;
            this.Tint = tint;
            this.SrcRect = SrcRect;
            this.ColorShift = colorShift;
        }
    }
}
