using Microsoft.Xna.Framework;
#if DEBUG
using Microsoft.Xna.Framework.Graphics;
#endif

namespace Tactile.Windows.UserInterface
{
    class UIEmptyObject : Graphic_Object, IUIObject
    {
        private Vector2 Size;
        private float Angle;

        public UIEmptyObject(Vector2 size, float angle = 0f)
        {
            Size = size;
            Angle = angle;
        }

        public void UpdateInput(Vector2 drawOffset = default(Vector2)) { }

        public Rectangle OnScreenBounds(Vector2 drawOffset)
        {
            Vector2 loc = (this.loc + this.draw_offset) - drawOffset;
            return new Rectangle(
                (int)loc.X, (int)loc.Y, (int)Size.X, (int)Size.Y);
        }

        public bool MouseOver(Vector2 drawOffset = default(Vector2))
        {
            Rectangle objectRect = OnScreenBounds(drawOffset);
            Vector2 scale = Size / new Vector2(16f);
            bool result = Global.Input.mouse_in_rectangle(
                objectRect, loc - drawOffset, this.offset * scale, Angle, false);
            return result;
        }

#if DEBUG
        public void Draw(SpriteBatch spriteBatch, Vector2 drawOffset = default(Vector2))
        {
            var texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Color color = new Color(128, 120, 120, 128);
            spriteBatch.Draw(texture, this.loc, new Rectangle(0, 0, 16, 16),
                color, Angle, this.offset, Size / new Vector2(16f), SpriteEffects.None, 0f);
        }
#endif
    }
}
