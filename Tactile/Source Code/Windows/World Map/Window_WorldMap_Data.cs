using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Window_WorldMap_Data : Graphic_Object
    {
        public const int WIDTH = 136;

        Sprite Window, Labels;
        TextSprite Chapter, Lord_Lvl, Unit_Count, Gold, Mode;
        Play_Time_Counter Counter;
        Character_Sprite Lord_Sprite;
        Page_Arrow Left_Arrow, Right_Arrow;

        public Window_WorldMap_Data()
        {
            Window = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Worldmap_Window"));
            Window.src_rect = new Rectangle(0, 0, 136, 64);
            Window.tint = new Color(224, 224, 224, 224);
            Labels = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Worldmap_Window"));
            Labels.src_rect = new Rectangle(0, 64, 136, 64);
            // Chapter
            Chapter = new TextSprite();
            Chapter.loc = new Vector2(8, 8);
            Chapter.SetFont(Config.UI_FONT, Global.Content, "White");
            // Lord_Lvl
            Lord_Lvl = new RightAdjustedText();
            Lord_Lvl.loc = new Vector2(128, 8);
            Lord_Lvl.SetFont(Config.UI_FONT, Global.Content, "Blue");
            // Mode
            Mode = new TextSprite();
            Mode.loc = new Vector2(12, 24);
            Mode.SetFont(Config.UI_FONT, Global.Content, "Blue");
            // Unit_Count
            Unit_Count = new RightAdjustedText();
            Unit_Count.loc = new Vector2(40, 40);
            Unit_Count.SetFont(Config.UI_FONT, Global.Content, "Blue");
            // Gold
            Gold = new RightAdjustedText();
            Gold.loc = new Vector2(120, 40);
            Gold.SetFont(Config.UI_FONT, Global.Content, "Blue");
            // Lord
            Lord_Sprite = new Character_Sprite();
            Lord_Sprite.loc = new Vector2(88, 24);
            Lord_Sprite.facing_count = 3;
            Lord_Sprite.frame_count = 3;
            Lord_Sprite.mirrored = Constants.Team.flipped_map_sprite(
                Constants.Team.PLAYER_TEAM);
            // Arrows
            Left_Arrow = new Page_Arrow();
            Left_Arrow.loc = new Vector2(0, 24);
            Right_Arrow = new Page_Arrow();
            Right_Arrow.loc = new Vector2(WIDTH, 24);
            Right_Arrow.mirrored = true;
        }

        public void update()
        {
            Counter.update();
            Left_Arrow.update();
            Right_Arrow.update();
        }

        public void set(string name, int lord_id, TactileLibrary.Preset_Chapter_Data data)
        {
            // Chapter
            Chapter.text = name;
            // Lord_Lvl
            int level = data.Lord_Lvl;
            if (level == 0)
                if (Global.data_actors.ContainsKey(lord_id))
                    level = Global.data_actors[lord_id].Level;
            Lord_Lvl.text = level.ToString();
            // Unit_Count
            Unit_Count.text = data.Units.ToString();
            // Gold
            Gold.text = data.Gold.ToString();
            // Play Time Counter
            Counter = new Play_Time_Counter(data.Playtime);
            Counter.loc = new Vector2(40, 24);
            // Lord
            if (Global.content_exists(string.Format(@"Graphics/Characters/{0}", Global.game_actors[lord_id].map_sprite_name)))
                Lord_Sprite.texture = Global.Content.Load<Texture2D>(
                    string.Format(@"Graphics/Characters/{0}", Global.game_actors[lord_id].map_sprite_name));
            else
                Lord_Sprite.texture = Global.Content.Load<Texture2D>(
                    string.Format(@"Graphics/Characters/{0}", Scene_Map.DEFAULT_MAP_SPRITE));
            if (Lord_Sprite.texture != null)
                Lord_Sprite.offset = new Vector2(
                    (Lord_Sprite.texture.Width / Lord_Sprite.frame_count) / 2,
                    (Lord_Sprite.texture.Height / Lord_Sprite.facing_count) - 8);
        }

        public void set_mode(Difficulty_Modes difficulty, bool arrowsVisible)
        {
            // Mode
            Mode.text = difficulty.ToString();// == Difficulty_Modes.Hard ? "Hard" : "Normal"; //Yeti
            // Arrows
            Left_Arrow.visible = Right_Arrow.visible = arrowsVisible;
        }

        public void draw(SpriteBatch sprite_batch)
        {
            Window.draw(sprite_batch, -loc);
            Labels.draw(sprite_batch, -loc);
            Chapter.draw(sprite_batch, -loc);
            Lord_Lvl.draw(sprite_batch, -loc);
            Mode.draw(sprite_batch, -loc);
            Unit_Count.draw(sprite_batch, -loc);
            Gold.draw(sprite_batch, -loc);
            Counter.draw(sprite_batch, -loc);
            Lord_Sprite.draw(sprite_batch, -loc);
            Left_Arrow.draw(sprite_batch, -loc);
            Right_Arrow.draw(sprite_batch, -loc);
        }
    }
}
