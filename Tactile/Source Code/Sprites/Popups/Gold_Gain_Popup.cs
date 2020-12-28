using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile
{
    class Gold_Gain_Popup : Popup
    {
        TextSprite Got, Amount, Gold;
        int Width = 0;

        public Gold_Gain_Popup(int value)
        {
            Timer_Max = 113;

            Width += 8;
            Got = new TextSprite();
            Got.loc = new Vector2(Width, 8);
            Got.SetFont(Config.UI_FONT, Global.Content, "White");
            Got.text = value >= 0 ? "Got " : "";
            Width += Font_Data.text_width(Got.text);

            Amount = new TextSprite();
            Amount.loc = new Vector2(Width, 8);
            Amount.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Amount.text = Math.Abs(value).ToString();
            Width += Font_Data.text_width(Amount.text);

            Gold = new TextSprite();
            Gold.loc = new Vector2(Width, 8);
            Gold.SetFont(Config.UI_FONT, Global.Content, "White");
            Gold.text = value >= 0 ? " gold." : " gold was stolen.";
            Width += Font_Data.text_width(Gold.text);
            Width += 8 + (Width % 8 != 0 ? (8 - Width % 8) : 0);

            Window = new System_Color_Window();
            Window.width = Width;
            Window.height = 32;
            loc = new Vector2((Config.WINDOW_WIDTH - Width) / 2, 80);
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
                Got.draw(sprite_batch, -(loc + draw_vector()));
                Amount.draw(sprite_batch, -(loc + draw_vector()));
                Gold.draw(sprite_batch, -(loc + draw_vector()));
                sprite_batch.End();
            }
        }
    }
}
