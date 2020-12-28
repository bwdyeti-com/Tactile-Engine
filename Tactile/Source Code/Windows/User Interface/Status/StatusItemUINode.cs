using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile.Windows.UserInterface.Status
{
    class StatusItemUINode : StatusUINode
    {
        protected Func<Game_Unit, ItemState> ItemFormula;
        protected Status_Item Item;
        protected TextSprite EquippedTag;

        internal StatusItemUINode(
                string helpLabel,
                Func<Game_Unit, ItemState> itemFormula)
            : base(helpLabel)
        {
            ItemFormula = itemFormula;

            Item =  new Status_Item();
            Item.draw_offset = new Vector2(0, 0);

            EquippedTag = new TextSprite();
            EquippedTag.draw_offset = new Vector2(120, 0);
            EquippedTag.SetFont(Config.UI_FONT, Global.Content, "White");
            EquippedTag.text = "$";
            EquippedTag.visible = false;

            Size = new Vector2(128, 16);
        }

        internal override void refresh(Game_Unit unit)
        {
            _enabled = false;
            if (ItemFormula != null)
            {
                var state = ItemFormula(unit);

                Item.set_image(unit.actor, state.Item);
                if (state.Drops)
                    Item.change_text_color("Green");
                EquippedTag.visible = state.Equipped;

                _enabled = !state.Item.non_equipment;
            }
        }

        protected override void update_graphics(bool activeNode)
        {
            Item.update();
        }

        protected override void mouse_off_graphic()
        {
            Item.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            Item.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            Item.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Item.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            EquippedTag.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
    
    struct ItemState
    {
        internal Item_Data Item;
        internal bool Drops;
        internal bool Equipped;
    }
}
