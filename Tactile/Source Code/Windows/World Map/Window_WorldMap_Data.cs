using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;

namespace Tactile.Windows.WorldMap
{
    class Window_WorldMap_Data : Graphic_Object
    {
        public const int WIDTH = 136;

        private Sprite Window, Labels;
        private TextSprite Chapter, LordLvl, UnitCount, Gold, Mode;
        private Play_Time_Counter Counter;
        private Character_Sprite Lord_Sprite;
        private Page_Arrow LeftArrow, RightArrow;

        public bool LeftClicked { get; private set; }
        public bool RightClicked { get; private set; }

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
            LordLvl = new RightAdjustedText();
            LordLvl.loc = new Vector2(128, 8);
            LordLvl.SetFont(Config.UI_FONT, Global.Content, "Blue");
            // Mode
            Mode = new TextSprite();
            Mode.loc = new Vector2(12, 24);
            Mode.SetFont(Config.UI_FONT, Global.Content, "Blue");
            // Unit_Count
            UnitCount = new RightAdjustedText();
            UnitCount.loc = new Vector2(40, 40);
            UnitCount.SetFont(Config.UI_FONT, Global.Content, "Blue");
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
            LeftArrow = new Page_Arrow();
            LeftArrow.loc = new Vector2(0, 24);
            LeftArrow.ArrowClicked += LeftArrow_ArrowClicked;
            RightArrow = new Page_Arrow();
            RightArrow.loc = new Vector2(WIDTH, 24);
            RightArrow.mirrored = true;
            RightArrow.ArrowClicked += RightArrow_ArrowClicked;
        }

        private void LeftArrow_ArrowClicked(object sender, EventArgs e)
        {
            LeftClicked = true;
        }

        private void RightArrow_ArrowClicked(object sender, EventArgs e)
        {
            RightClicked = true;
        }

        public void update()
        {
            LeftClicked = false;
            RightClicked = false;

            Counter.update();
            LeftArrow.update();
            RightArrow.update();

            LeftArrow.UpdateInput(-loc);
            RightArrow.UpdateInput(-loc);
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
            LordLvl.text = level.ToString();
            // Unit_Count
            UnitCount.text = data.Units.ToString();
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
            LeftArrow.visible = RightArrow.visible = arrowsVisible;

            LeftClicked = false;
            RightClicked = false;
        }

        public void draw(SpriteBatch sprite_batch)
        {
            Window.draw(sprite_batch, -loc);
            Labels.draw(sprite_batch, -loc);
            Chapter.draw(sprite_batch, -loc);
            LordLvl.draw(sprite_batch, -loc);
            Mode.draw(sprite_batch, -loc);
            UnitCount.draw(sprite_batch, -loc);
            Gold.draw(sprite_batch, -loc);
            Counter.draw(sprite_batch, -loc);
            Lord_Sprite.draw(sprite_batch, -loc);
            LeftArrow.draw(sprite_batch, -loc);
            RightArrow.draw(sprite_batch, -loc);
        }
    }
}
