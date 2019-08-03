using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Menus;
using FEXNA.Menus.Preparations;
using FEXNA.Windows.Command;
using FEXNA_Library;

namespace FEXNA
{
    class Window_Manage_Items : Window_Prep_Items
    {
        public override ItemsCommandMenu GetCommandMenu()
        {
            return new ManageItemsCommandMenu(ActorId);
        }

        public override void CommandSelection(Maybe<int> selectedIndex)
        {
            switch (selectedIndex)
            {
                // Trade
                case 0:
                    if (Global.battalion.actors.Count <= 1)
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        OnTradeSelected(new EventArgs());
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
                    if (Global.battalion.has_convoy)
                        give_all();
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                // Optimize
                case 4:
                    if (Global.battalion.has_convoy)
                        optimize();
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                default:
                    Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
            }
        }
    }
}
