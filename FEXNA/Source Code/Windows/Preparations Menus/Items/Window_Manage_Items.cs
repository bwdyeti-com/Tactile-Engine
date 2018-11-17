using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.Command;

namespace FEXNA
{
    class Window_Manage_Items : Window_Prep_Items
    {
        protected override void set_command_window()
        {
            List<string> strs = new List<string> { "Trade", "List", "Convoy", "Give All", "Optimize" };
            Command_Window = new Window_Command(new Vector2(Config.WINDOW_WIDTH - 128, Config.WINDOW_HEIGHT - 100), 56, strs);
            if (Global.battalion.actors.Count <= 1)
                Command_Window.set_text_color(0, "Grey");
            if (Global.battalion.actors.Count <= 1 &&
                    Global.battalion.convoy_id == -1)
                Command_Window.set_text_color(1, "Grey");
            if (Global.battalion.convoy_id == -1)
            {
                Command_Window.set_text_color(2, "Grey");
                Command_Window.set_text_color(3, "Grey");
                Command_Window.set_text_color(4, "Grey");
            }
            Command_Window.size_offset = new Vector2(0, -8);
            Command_Window.text_offset = new Vector2(0, -4);
            Command_Window.glow_width = 56 - 8;
            Command_Window.glow = true;
            Command_Window.bar_offset = new Vector2(-8, 0);
            Command_Window.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Item_Options_Window");
            Command_Window.color_override = 0;
            Command_Window.set_columns(2);
            Command_Window.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;
        }

        protected override void refresh_convoy_use_color() { }

        protected override void update_selected_input()
        {
            if (Command_Window.is_canceled())
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                cancel_unit_selection();
            }
            else if (Command_Window.is_selected())
            {
                switch (Command_Window.selected_index())
                {
                    // Trade
                    case 0:
                        if (Global.battalion.actors.Count <= 1)
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            trade();
                        }
                        break;
                    // List
                    case 1:
                        OnList(new EventArgs());
                        break;
                    // Supply
                    case 2:
                        OnConvoy(new EventArgs());
                        break;
                    // Give All
                    case 3:
                        if (Global.battalion.convoy_id > -1)
                            give_all();
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                    // Optimize
                    case 4:
                        if (Global.battalion.convoy_id > -1)
                            optimize();
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                    default:
                        Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                }
            }
            else if (Command_Window.getting_help())
            {
            }
        }
    }
}
