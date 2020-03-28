#if !MONOGAME && DEBUG
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.UserInterface.Command;
using FEXNA.Windows.UserInterface.Status;
using FEXNA_Library;

namespace FEXNA.Windows.Command
{
    class WindowCommandReinforcements : Window_Command_Scrollbar
    {
        public const int WIDTH = 160;
        const int ROWS = Config.WINDOW_HEIGHT / 16 - 3;

        List<Data_Unit> ReinforcementData;
        private ReinforcementDetailWindow DetailWindow;

        public WindowCommandReinforcements(
            Window_Command_Scroll window, Vector2 loc, List<Data_Unit> reinforcements)
            : base(window, loc, WIDTH, ROWS, reinforcement_commands(reinforcements))
        {
            set_text_color(0, "Blue");

            ReinforcementData = reinforcements;
            DetailWindow = new ReinforcementDetailWindow();
            DetailWindow.loc = new Vector2(WIDTH + 16, ROWS * 16 - 56);

            refresh_detail();
        }

        private static List<string> reinforcement_commands(List<Data_Unit> reinforcements)
        {
            List<string> commands = new List<string> { "Reinforcements:" };
            //foreach (Data_Unit unit in Unit_Data.Reinforcements)
            for (int i = 0; i < reinforcements.Count; i++)
            {
                Test_Battle_Character_Data test_battler = Test_Battle_Character_Data.from_data(
                    reinforcements[i].type, reinforcements[i].identifier, reinforcements[i].data);
                string str;// = "Team " + test_battler.Team.ToString() + ": ";
                if (test_battler.Generic)
                {
                    str = string.Format("{0}- Team {1}: {2}",
                        i, test_battler.Team, Global.data_classes[test_battler.Class_Id].Name);
                }
                else
                    str = string.Format("{0}- Team {1}: {2}",
                        i, test_battler.Team, Global.data_actors[test_battler.Actor_Id].Name);
                commands.Add(str);
            }

            if (reinforcements.Count == 0)
                commands.Add("New Reinforcement");

            return commands;
        }

        protected override void update_ui(bool input)
        {
            base.update_ui(input);

            if (DetailWindow != null)
                DetailWindow.update();
        }

        protected override void on_index_changed(int oldIndex)
        {
            base.on_index_changed(oldIndex);

            refresh_detail();
        }

        private void refresh_detail()
        {
            if (DetailWindow != null)
            {
                Test_Battle_Character_Data test_battler = null;

                if (ReinforcementData != null)
                {
                    int index = this.index - 1;
                    if (index >= 0 && index < ReinforcementData.Count)
                        test_battler = Test_Battle_Character_Data.from_data(
                            ReinforcementData[index].type,
                            ReinforcementData[index].identifier,
                            ReinforcementData[index].data);
                    else if (index == ReinforcementData.Count + 1)
                        test_battler = Global.test_battler_1;
                }

                DetailWindow.refresh(test_battler);
            }
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            base.draw(sprite_batch);

            DetailWindow.draw(sprite_batch, -this.loc);
        }
    }

    class ReinforcementDetailWindow : Stereoscopic_Graphic_Object
    {
        private WindowPanel Window;
        private MapSpriteUINode MapSprite;
        private FE_Text Level, Identifier, Build, Mission;
        private Unit_Info_Inventory Inventory;

        public ReinforcementDetailWindow()
        {
            Window = new System_Color_Window();
            Window.offset = new Vector2(8, 8);
            Window.width = 136;
            Window.height = 80;
            
            var text = new FE_Text();
            text.Font = "FE7_Text";
            text.texture = Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_White");
            MapSprite = new MapSpriteUINode("", text, 48);
            MapSprite.loc = new Vector2(0, 0);

            Level = new FE_Text("FE7_TextL",
                Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_Blue"),
                new Vector2(88, 0));
            Level.text_colors[0] = "FE7_Text_Yellow";
            Level.text_colors[2] = "FE7_Text_Blue";
            Identifier = new FE_Text("FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_White"),
                new Vector2(0, 16));
            Build = new FE_Text("FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_Blue"),
                new Vector2(88, 16));
            Mission = new FE_Text("FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_White"),
                new Vector2(0, 32));

            Inventory = new Unit_Info_Inventory();
            Inventory.loc = new Vector2(0, 48);
        }

        internal void refresh(Test_Battle_Character_Data testBattler)
        {
            MapSprite.clear_map_sprite();

            if (testBattler == null)
            {
                MapSprite.set_text("");
                Level.text = "";
                Identifier.text = "";
                Build.text = "";
                Mission.text = "";
                Inventory.set_images((List<Item_Data>)null);
            }
            else
            {
                int team = testBattler.Team;

                MapSprite.set_map_sprite(Game_Actors.get_map_sprite_name(testBattler.class_id, testBattler.gender), team);
                MapSprite.set_text(testBattler.name);
                Level.text = string.Format("LV{0}", testBattler.level);
                Identifier.text = string.IsNullOrEmpty(testBattler.Identifier) ?
                    "(no identifier)" : testBattler.Identifier;
                Build.text = testBattler.Generic ? ((Generic_Builds)testBattler.Build).ToString() : "Unique";
                int mission = testBattler.Mission % Game_AI.MISSION_COUNT;
                Mission.text = Game_AI.MISSION_NAMES.ContainsKey(mission) ?
                        Game_AI.MISSION_NAMES[mission] :
                        string.Format("Mission {0}", mission);
                Inventory.set_images(testBattler.items);
            }
        }

        public void update()
        {
            MapSprite.Update();
        }

        public void draw(SpriteBatch spriteBatch, Vector2 draw_offset = default(Vector2))
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Window.draw(spriteBatch, draw_offset - this.loc);
            MapSprite.Draw(spriteBatch, draw_offset - this.loc);
            Level.draw_multicolored(spriteBatch, draw_offset - this.loc);
            Identifier.draw(spriteBatch, draw_offset - this.loc);
            Build.draw(spriteBatch, draw_offset - this.loc);
            Mission.draw(spriteBatch, draw_offset - this.loc);
            spriteBatch.End();

            Inventory.draw(spriteBatch, draw_offset - this.loc);
        }
    }
}
#endif