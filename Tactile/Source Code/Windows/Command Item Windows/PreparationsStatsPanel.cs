using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Windows;
using TactileLibrary;

namespace Tactile.Windows.Command.Items
{
    class PreparationsStatsPanel : StatsPanel
    {
        public PreparationsStatsPanel(Game_Actor actor) : base(actor)
        {
            //Item_Description.loc = new Vector2(128, 48);
            //Item_Description.loc =
            //    new Vector2(Config.WINDOW_WIDTH - 152, Config.WINDOW_HEIGHT - 56);
        }

        internal void face_values(bool facingRight, WindowPanel window)
        {
            Face.mirrored = facingRight;
            Face.loc = new Vector2(-window.width / 2, window.height - (32 + 2));
        }

        protected override void set_face(Game_Actor actor)
        {
            Face = new Face_Sprite(actor.face_name, true);
            if (actor.generic_face)
                Face.recolor_country(actor.name_full);
            Face.expression = Face.status_frame;
            Face.phase_in();
            Face.tint = new Color(128, 128, 128, 128);
            Face.idle = true;
        }

        public override void refresh_info(Game_Actor actor, Data_Equipment item,
            int[] statValues, int[] baseStatValues)
        {
            // No item or weapon
            if (item == null || is_weapon_highlighted(item))
            {
                Item_Description.text = "";
            }
            // Item
            else
            {
                Item_Description.text = "";
                if (item.is_weapon)
                    Item_Description.SetColor(Global.Content, "Grey");
                else
                    Item_Description.SetColor(Global.Content,
                        Combat.can_use_item(actor, item.Id, false) ?
                        "White" : "Grey");
                string[] desc_ary = item.Quick_Desc.Split('|');
                for (int i = 0; i < desc_ary.Length; i++)
                    Item_Description.text += desc_ary[i] + "\n";
            }
        }

        protected override void draw_window(
            SpriteBatch sprite_batch, Vector2 data_draw_offset) { }
    }
}
