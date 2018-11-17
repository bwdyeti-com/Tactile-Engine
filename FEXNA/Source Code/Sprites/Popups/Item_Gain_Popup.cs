using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    class Item_Gain_Popup : Item_Break_Popup
    {
        protected FE_Text Got;

        public Item_Gain_Popup() { }
        public Item_Gain_Popup(int item_id, bool is_item, int time)
        {
            initialize(item_id, is_item, false);
            Timer_Max = time;
            loc = new Vector2((Config.WINDOW_WIDTH - Width) / 2, 80);
        }

        protected override void set_text(FEXNA_Library.Data_Equipment item, bool battle_scene)
        {
            int x = 8;
            Got = new FE_Text();
            Got.loc = new Vector2(x, 8);
            Got.Font = "FE7_Text";
            Got.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/" +
                (battle_scene ? "FE7_Text_CombatBlue" : "FE7_Text_White"));
            Got.text = got_text();
            x += Font_Data.text_width(Got.text, "FE7_Text");
            A_Name = new FE_Text();
            A_Name.loc = new Vector2(x, 8);
            A_Name.Font = "FE7_Text";
            A_Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/" +
                (battle_scene ? "FE7_Text_CombatBlue" : "FE7_Text_Blue"));
            A_Name.text = aname_text(item.Name, item);
            x += Font_Data.text_width(A_Name.text, "FE7_Text") + 1;
            Icon.loc = new Vector2(x, 8);
            x += 17;
            Broke = new FE_Text();
            Broke.loc = new Vector2(x, 8);
            Broke.Font = "FE7_Text";
            Broke.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Broke.text = broke_text();
            x += Font_Data.text_width(Broke.text, "FE7_Text");
            Width = x + 8 + (x % 8 != 0 ? (8 - x % 8) : 0);
        }

        protected virtual string got_text()
        {
            return "Got ";
        }

        protected virtual string aname_text(string item_name, FEXNA_Library.Data_Equipment item)
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
