using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Item_Gain_Popup : Item_Break_Popup
    {
        protected TextSprite Got;

        public Item_Gain_Popup() { }
        public Item_Gain_Popup(int item_id, bool is_item, int time)
        {
            initialize(item_id, is_item, false);
            Timer_Max = time;
            loc = new Vector2((Config.WINDOW_WIDTH - Width) / 2, 80);
        }

        protected override void set_text(TactileLibrary.Data_Equipment item, bool battle_scene)
        {
            int x = 8;
            Got = new TextSprite();
            Got.loc = new Vector2(x, 8);
            Got.SetFont(Config.UI_FONT, Global.Content,
                battle_scene ? "CombatBlue" : "White");
            Got.text = got_text();
            x += Got.text_width;
            A_Name = new TextSprite();
            A_Name.loc = new Vector2(x, 8);
            A_Name.SetFont(Config.UI_FONT, Global.Content,
                battle_scene ? "CombatBlue" : "Blue");
            A_Name.text = aname_text(item.Name, item);
            x += A_Name.text_width + 1;
            Icon.loc = new Vector2(x, 8);
            x += 17;
            Broke = new TextSprite();
            Broke.loc = new Vector2(x, 8);
            Broke.SetFont(Config.UI_FONT, Global.Content, "White");
            Broke.text = broke_text();
            x += Broke.text_width;
            Width = x + 8 + (x % 8 != 0 ? (8 - x % 8) : 0);
        }

        protected virtual string got_text()
        {
            return "Got ";
        }

        protected virtual string aname_text(string item_name, TactileLibrary.Data_Equipment item)
        {
            return string.Format("{0} {1}", item.article, item_name);
        }

        protected virtual string broke_text()
        {
            return ".";
        }

        protected override void draw_image(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Got.draw(sprite_batch, -(loc + draw_vector()));
            sprite_batch.End();

            base.draw_image(sprite_batch);
        }
    }
}
