using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Windows.UserInterface
{
    class UICursor<T> : Hand_Cursor where T : UINode
    {
        private UINodeSet<T> NodeSet;
        private bool _hiddenWhenMouse = true;

        internal UICursor(UINodeSet<T> nodes)
        {
            NodeSet = nodes;
            if (NodeSet.Any() && NodeSet.ActiveNode != null)
                force_loc(NodeSet.ActiveNode.loc);
        }

        internal void hide_when_using_mouse(bool hide)
        {
            _hiddenWhenMouse = hide;
        }
        
        public override void update()
        {
            this.update(default(Vector2));
        }
        public void update(Vector2 scrollOffset)
        {
            UpdateTargetLoc(scrollOffset);

            base.update();
        }

        public void UpdateTargetLoc(Vector2 scrollOffset = default(Vector2))
        {
            if (NodeSet.Any() && NodeSet.ActiveNode != null)
            {
                Vector2 loc = NodeSet.ActiveNode.loc - scrollOffset;
                if (this.target_loc != loc)
                    set_loc(loc);
            }

            if (Input.ControlScheme != ControlSchemes.Buttons)
            {
                move_to_target_loc();
            }
        }

        /// <summary>
        /// Immediately move this cursor to its target location.
        /// </summary>
        public void move_to_target_loc()
        {
            if (this.loc != this.target_loc)
                force_loc(target_loc);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix)
        {
            //if (!_hiddenWhenMouse || Input.ControlScheme != ControlSchemes.Mouse) //Debug
            if (!_hiddenWhenMouse || Input.ControlScheme == ControlSchemes.Buttons)
                base.draw(sprite_batch, draw_offset, matrix);
        }
    }
}
