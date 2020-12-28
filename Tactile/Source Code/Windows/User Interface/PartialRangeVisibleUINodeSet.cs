using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EnumExtension;
using TactileLibrary;

namespace Tactile.Windows.UserInterface
{
    class PartialRangeVisibleUINodeSet<T> : UINodeSet<T> where T : UINode
    {
        internal PartialRangeVisibleUINodeSet(IEnumerable<T> set) : base(set) { }

        public void Update(bool input, IEnumerable<int> range,
            Vector2 draw_offset = default(Vector2))
        {
            if (input)
                update_input();
            foreach (int index in range)
                Nodes[index].Update(this, input, draw_offset);
        }
        public void Update(ControlSet input, IEnumerable<int> range,
            Vector2 draw_offset = default(Vector2))
        {
            if (input.HasEnumFlag(ControlSet.Buttons))
                update_input();
            foreach (int index in range)
                Nodes[index].Update(this, input, draw_offset);
        }

        public void Draw(SpriteBatch sprite_batch, IEnumerable<int> range,
            Vector2 draw_offset = default(Vector2))
        {
#if DEBUG
            const bool PREVIEW_NODES = false;
            // Draw node connections
            if (PREVIEW_NODES)
            {
                draw_node_connections(sprite_batch, draw_offset);
            }
#endif
            foreach (int index in range)
                Nodes[index].Draw(sprite_batch, draw_offset);
#if DEBUG
            // Draw hitboxes
            if (PREVIEW_NODES) //Debug
            {
                DrawNodeHitboxes(sprite_batch, draw_offset);
            }
#endif
        }
    }
}
