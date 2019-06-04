#if !MONOGAME && DEBUG
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FEXNA.Windows.Command;
using FEXNA_Library;

namespace FEXNA.Menus.Map
{
    class ReinforcementsMenu : CommandMenu
    {
        private List<Data_Unit> ReinforcementData;

        public ReinforcementsMenu(List<Data_Unit> reinforcementData)
        {
            ReinforcementData = reinforcementData;
            open_reinforcements_menu();
        }

        private void open_reinforcements_menu()
        {
            Window = new WindowCommandReinforcements(
                Window as WindowCommandReinforcements,
                new Vector2(8, 24),
                ReinforcementData);
        }

        private int new_team(int old_team, bool left)
        {
            if (left)
                old_team--;
            else
                old_team++;

            old_team--;
            old_team = ((old_team + Constants.Team.NUM_TEAMS) % Constants.Team.NUM_TEAMS);
            old_team++;

            return old_team;
        }

        public void Enable()
        {
            Window.active = true;
            Window.greyed_cursor = false;
        }

        public void Refresh(int index)
        {
            int scroll = (Window as Window_Command_Scroll).scroll;

            open_reinforcements_menu();

            Window.immediate_index = Math.Max(1,
                Math.Min(ReinforcementData.Count, index));
            (Window as Window_Command_Scroll).scroll = scroll;
            (Window as Window_Command_Scroll).refresh_scroll();
        }

        protected override void UpdateMenu(bool active)
        {
            Window.update(active);

            if (Window.is_canceled())
            {
                Cancel();
            }
            else if (Window.is_selected())
            {
                if (Window.selected_index() == 0)
                    Global.game_system.play_se(System_Sounds.Buzzer);
                else
                {
                    if (ReinforcementData.Count == 0)
                    {
                        switch (Window.selected_index())
                        {
                            case 0:
                            case 3:
                                Global.game_system.play_se(System_Sounds.Buzzer);
                                return;
                        }
                    }
                    
                    Window.greyed_cursor = true;
                    Window.active = false;
                    Window.index = Window.selected_index();
                    SelectItem(true);
                }
            }
            else if (Global.Input.repeated(Inputs.Left) ||
                Global.Input.repeated(Inputs.Right))
            {
                if (Window.index > 0 && Window.index <= ReinforcementData.Count)
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move2);
                    Data_Unit unit = ReinforcementData[Window.index - 1];
                    Test_Battle_Character_Data test_battler = Test_Battle_Character_Data.from_data(unit.type, unit.identifier, unit.data);
                    int team = test_battler.Team;
                    team = new_team(team, Global.Input.repeated(Inputs.Left));

                    test_battler.Team = team;
                    string[] ary = test_battler.to_string();
                    ReinforcementData[Window.index - 1] = new Data_Unit(ary[0], ary[1], ary[2]);

                    open_reinforcements_menu();
                }
                // also set scroll, and make the cursor not jump in from the top //Yeti
            }
        }
    }
}
#endif
