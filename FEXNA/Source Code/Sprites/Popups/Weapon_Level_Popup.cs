using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;

namespace FEXNA
{
    class Weapon_Level_Popup : Popup
    {
        const int WIDTH = 160;
        Weapon_Type_Icon Icon;
        FE_Text Rank, Text;

        public Weapon_Level_Popup(int weapon, int newRank) : this(weapon, newRank, true) { }
        public Weapon_Level_Popup(int weapon, int newRank, bool battle_scene)
        {
            Width = WIDTH;
            initialize(weapon, newRank, battle_scene);
        }

        protected void initialize(int weapon, int newRank, bool battle_scene)
        {
            Timer_Max = 97;
            if (battle_scene)
                texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Combat_Popup");
            else
            {
                Window = new System_Color_Window();
                Window.width = WIDTH;
                Window.height = 32;
            }
            // Text
            Text = new FE_Text("FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White"),
                //new Vector2(32 + 4, 8), //Debug
                new Vector2(8, 8),
                "Weapon Level increased to ");
            // Weapon rank
            Rank = new FE_Text("FE7_TextL",
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue"),
                //new Vector2(24, 8), //Debug
                Text.loc + new Vector2(Text.text_width, 0),
                FEXNA_Library.Data_Weapon.WLVL_LETTERS[newRank]);
            // Weapon type icon
            Icon = new Weapon_Type_Icon();
            Icon.index = weapon;
            //Icon.loc = new Vector2(8, 8); //Debug
            Icon.loc = Rank.loc + new Vector2(Rank.text_width, 0);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                if (Window != null)
                    Window.draw(sprite_batch, -(loc + draw_vector()));
                else
                    draw_panel(sprite_batch, WIDTH);
                Icon.draw(sprite_batch, -(loc + draw_vector()));
                Rank.draw(sprite_batch, -(loc + draw_vector()));
                Text.draw(sprite_batch, -(loc + draw_vector()));
                sprite_batch.End();
            }
        }
    }
}
