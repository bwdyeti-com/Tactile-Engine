using System;
using Microsoft.Xna.Framework;

namespace Tactile
{
    abstract class Graphic_Object
    {
        public Vector2 loc = Vector2.Zero, draw_offset = Vector2.Zero;
        public Vector2 offset = Vector2.Zero;
    }
}
