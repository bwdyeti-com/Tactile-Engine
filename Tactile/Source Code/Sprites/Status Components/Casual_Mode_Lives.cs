using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Casual_Mode_Lives : Sprite
    {
        public Casual_Mode_Lives()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Casual");
            visible = false;
        }

        internal static bool lives_visible(Game_Unit unit)
        {
            return !(Global.game_system.Style != Mode_Styles.Casual || !unit.lives_visible);
        }

        internal void refresh(Game_Unit unit)
        {
            if (!lives_visible(unit))
            {
                visible = false;
                return;
            }
            visible = true;
            int width = texture.Width;
            if (unit.loss_on_death)
                Src_Rect = new Rectangle(0, 0, width, width);
            else
                Src_Rect = new Rectangle(0, (unit.actor.lives + 1) * width, width, width);
        }
    }
}
