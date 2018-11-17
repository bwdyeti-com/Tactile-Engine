using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Windows.UserInterface.Status
{
    class StatusSiegeItemUINode : StatusItemUINode
    {
        internal StatusSiegeItemUINode(
                string helpLabel,
                Func<Game_Unit, ItemState> itemFormula)
            : base(helpLabel, itemFormula)
        {
            Item = new Attack_Item();
            Item.draw_offset = new Vector2(0, 0);
        }
    }
}
