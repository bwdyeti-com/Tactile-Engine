using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    abstract class Unit_Page : Stereoscopic_Graphic_Object
    {
        protected static RasterizerState Scissor_State = new RasterizerState { ScissorTestEnable = true };

        public abstract void update();

        public abstract void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2));
    }
}
