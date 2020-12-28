using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface;

namespace Tactile.Windows.UserInterface.Title
{
    abstract class Title_Info_Panel : UINode
    {
        protected WindowPanel Window;

        #region Accessors
        public int height { get { return Window.height; } }

        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Window.stereoscopic = value;
            }
        }

        protected override IEnumerable<Inputs> ValidInputs
        {
            get
            {
                yield return Inputs.A;
            }
        }
        protected override bool RightClickActive { get { return false; } }
        #endregion

        protected override void update_graphics(bool activeNode) //Debug
        {
        }

        protected override void mouse_off_graphic() { } //Debug
        protected override void mouse_highlight_graphic() { }
        protected override void mouse_click_graphic() { }
    }
}
