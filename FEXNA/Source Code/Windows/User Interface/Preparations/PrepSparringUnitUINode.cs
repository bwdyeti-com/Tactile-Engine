//Sparring
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Preparations;

namespace FEXNA.Windows.UserInterface.Preparations
{
    class PrepSparringUnitUINode : PrepItemsUnitUINode
    {
        private Sparring_Gauge Gauge;

        internal PrepSparringUnitUINode(string name, int points, int staffPoints)
            : base(name)
        {
            Gauge = new Sparring_Gauge(false);
            Gauge.points = points;
            Gauge.staffPoints = staffPoints;
            Gauge.draw_offset = new Vector2(16, 12);
        }

        internal void set_active_pips(int value)
        {
            Gauge.active_pips = value;
        }

        internal void set_pip_opacity(float value)
        {
            Gauge.active_pip_opacity = value;
        }

        public override void Draw(
            SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            Gauge.draw(sprite_batch, draw_offset - loc);
        }
    }
}
