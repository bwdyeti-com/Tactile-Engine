using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile.Windows.UserInterface.Status
{
    class StatusTravelerUINode : StatusLabeledTextUINode
    {
        const int ICON_SPACING = 12;
        protected Func<Game_Unit, int> RescuingFormula;
        protected Sprite RescueIcon;

        internal StatusTravelerUINode(
                string helpLabel,
                string label,
                Func<Game_Unit, string> textFormula,
                Func<Game_Unit, int> rescuingFormula,
                int textOffset = 32)
            : base(helpLabel, label, textFormula, textOffset)
        {
            RescuingFormula = rescuingFormula;

            RescueIcon = new Sprite();
            RescueIcon.texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/RescueIcon");
            RescueIcon.draw_offset = new Vector2(textOffset - 25, -9);

            Size = new Vector2(textOffset + 40, 16);
        }

        internal override void refresh(Game_Unit unit)
        {
            base.refresh(unit);

            if (RescuingFormula != null)
            {
                int team = RescuingFormula(unit);

                RescueIcon.visible = team > 0;
                if (team > 0)
                    RescueIcon.src_rect = new Rectangle(
                        (team - 1) *
                            (RescueIcon.texture.Width / Constants.Team.NUM_TEAMS),
                        0,
                        RescueIcon.texture.Width / Constants.Team.NUM_TEAMS,
                        RescueIcon.texture.Height);
            }
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);

            RescueIcon.update();
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);

            if (Global.game_map.icons_visible)
                RescueIcon.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
