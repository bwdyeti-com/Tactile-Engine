using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile
{
    class Item_Break_Popup : Popup
    {
        protected Item_Icon_Sprite Icon;
        protected TextSprite A_Name, Broke;

        #region Accessors
        public int width { get { return Width; } }
        #endregion

        public Item_Break_Popup() { }
        public Item_Break_Popup(int item_id, bool is_item)
        {
            initialize(item_id, is_item, true);
        }
        public Item_Break_Popup(int item_id, bool is_item, bool battle_scene)
        {
            initialize(item_id, is_item, battle_scene);
        }

        protected virtual void initialize(int item_id, bool is_item, bool battle_scene)
        {
            Timer_Max = 97;
            TactileLibrary.Data_Equipment item;
            if (is_item)
                item = Global.data_items[item_id];
            else
                item = Global.data_weapons[item_id];
            // Item icon
            set_icon(item);
            // Text
            set_text(item, battle_scene);
            set_window(battle_scene);
        }

        protected void set_window(bool battle_scene)
        {
            if (battle_scene)
                texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Combat_Popup");
            else
            {
                if (Global.game_system.preparations && !Global.game_system.Preparation_Events_Ready)
                {
                    Window = new WindowPanel(Global.Content.Load<Texture2D>(
                        @"Graphics/Windowskins/Preparations_Item_Options_Window"));
                    Window.width = Width - 8;
                    Window.height = 24;
                    Window.offset = new Vector2(-4, -4);
                }
                else
                {
                    Window = new System_Color_Window();
                    Window.width = Width;
                    Window.height = 32;
                }
            }
        }

        protected void set_icon(TactileLibrary.Data_Equipment item)
        {
            Icon = new Item_Icon_Sprite();
            Icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + item.Image_Name);
            Icon.index = item.Image_Index;
        }

        protected virtual void set_text(TactileLibrary.Data_Equipment item, bool battle_scene)
        {
            int x = battle_scene ? 23 : 8;
            A_Name = new TextSprite();
            A_Name.loc = new Vector2(x, 8);
            A_Name.SetFont(Config.UI_FONT, Global.Content,
                battle_scene ? "CombatBlue" : "Blue");
            A_Name.text = string.Format("{0} {1}", item.capitalizedArticle, item.Name);
            x += A_Name.text_width;
            Icon.loc = new Vector2(battle_scene ? 7 : 1 + x, 8);
            if (!battle_scene)
                x += 15;
            Broke = new TextSprite();
            Broke.loc = new Vector2(x, 8);
            Broke.SetFont(Config.UI_FONT, Global.Content, "White");
            Broke.text = " broke!";
            x += Broke.text_width;
            Width = x + 8 + (x % 8 != 0 ? (8 - x % 8) : 0);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                if (Window != null)
                    Window.draw(sprite_batch, -(loc + draw_vector()));
                else
                    draw_panel(sprite_batch, Width);
                sprite_batch.End();

                draw_image(sprite_batch);
            }
        }

        protected virtual void draw_image(SpriteBatch sprite_batch)
        {
            Icon.draw(sprite_batch, -(loc + draw_vector()));

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            A_Name.draw(sprite_batch, -(loc + draw_vector()));
            Broke.draw(sprite_batch, -(loc + draw_vector()));
            sprite_batch.End();
        }
    }
}
