using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile
{
    class Inventory_Target_Item : Status_Item
    {
        private TextSprite RepairUses;
        private Weapon_Triangle_Arrow Arrow;

        public void set_image(Game_Actor actor, Item_Data item_data, int active_item)
        {
            base.set_image(actor, item_data);
            if (!item_data.non_equipment)
            {
                Data_Equipment item = item_data.to_equipment;

                bool useable = false;
                if (active_item >= 0)
                {
                    var usedItem = actor.items[active_item].to_item;
                    bool canTarget = usedItem.can_target_item(item_data);
                    useable = canTarget;

                    if (usedItem.can_repair)
                    {
                        if (canTarget)
                        {
                            Arrow = new Weapon_Triangle_Arrow();
                            Arrow.value = WeaponTriangle.Advantage;

                            int repairUses = item_data.RepairAmount(usedItem);
                            int uses = item_data.UsesAfterRepair(false, repairUses);
                            string color = uses == item_data.max_uses ? "Green" : "Blue";

                            Uses.SetColor(Global.Content, color);

                            RepairUses = new TextSprite(
                                Config.UI_FONT, Global.Content, color,
                                Vector2.Zero,
                                uses < 0 ? "--" : uses.ToString());

                            Slash.loc += new Vector2(24, 0);
                            Use_Max.loc += new Vector2(24, 0);
                        }
                    }
                }

                set_text_color(useable);
            }
        }

        public void HideMaxUses(bool hide)
        {
            Slash.visible = !hide;
            Use_Max.visible = !hide;
        }

        protected override void UpdateSprites()
        {
            if (Arrow != null)
                Arrow.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            if (RepairUses != null)
            {
                Arrow.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(80 + 6, -1) - offset));
                RepairUses.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(104, 0) - offset));
            }
            base.draw(sprite_batch, draw_offset);
        }
    }
}
