using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile.Windows.UserInterface.Title
{
    class Suspend_Info_Panel : Title_Info_Panel
    {
        Miniface Lord_Mini;
        TextSprite Turn_Label, Units_Label, Save_Label;
        TextSprite Chapter, Turn, Units, Gold, Save, Mode, Style;
        Play_Time_Counter Counter;
        Sprite Gold_G;
        Sprite Screenshot;

        #region Accessors
        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Lord_Mini.stereoscopic = value;
                Turn_Label.stereoscopic = value;
                Units_Label.stereoscopic = value;
                Save_Label.stereoscopic = value;
                Chapter.stereoscopic = value;
                Turn.stereoscopic = value;
                Units.stereoscopic = value;
                Gold.stereoscopic = value;
                Save.stereoscopic = value;
                Mode.stereoscopic = value;
                Style.stereoscopic = value;
                Counter.stereoscopic = value;
                Gold_G.stereoscopic = value;
            }
        }
        #endregion

        public Suspend_Info_Panel(bool mainMenu = false)
            : this(mainMenu, Global.suspend_file_info) { }
        public Suspend_Info_Panel(bool mainMenu, IO.Suspend_Info suspend_file_info)
        {
            this.offset = new Vector2(-12, -4);

            create_window(mainMenu, suspend_file_info);

            Lord_Mini = new Miniface();
            Lord_Mini.set_actor(suspend_file_info.lord_actor_face);
            Lord_Mini.loc = new Vector2(24, 0);

            Turn_Label = new TextSprite();
            Turn_Label.loc = new Vector2(48, 16);
            Turn_Label.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Units_Label = new TextSprite();
            Units_Label.loc = new Vector2(112, 16);
            Units_Label.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Units_Label.text = "Units";
            Save_Label = new TextSprite();
            Save_Label.loc = new Vector2(0, 48);
            Save_Label.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Save_Label.text = "File";

            Chapter = new TextSprite();
            Chapter.loc = new Vector2(48, 0);
            Chapter.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Chapter.text = !Global.data_chapters.ContainsKey(suspend_file_info.chapter_id) ?
                suspend_file_info.chapter_id :
                Global.data_chapters[suspend_file_info.chapter_id].FileSelectName;
            Turn = new RightAdjustedText();
            Turn.loc = new Vector2(88, 16);
            Turn.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Units = new RightAdjustedText();
            Units.loc = new Vector2(152, 16);
            Units.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Units.text = suspend_file_info.units.ToString();
            Gold = new RightAdjustedText();
            Gold.loc = new Vector2(140, 32);
            Gold.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Gold.text = suspend_file_info.gold.ToString();
            Save = new RightAdjustedText();
            Save.loc = new Vector2(40, 48);
            Save.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Save.text = suspend_file_info.save_id.ToString();
            Mode = new TextSprite();
            Mode.loc = new Vector2(80, 48);
            Mode.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Mode.text = suspend_file_info.difficulty.ToString();
            Style = new TextSprite();
            Style.loc = new Vector2(120, 48);
            Style.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Style.text = suspend_file_info.style.ToString();//.Substring(0, Math.Min(3, suspend_file_info.style.ToString().Length)); //Debug

            if (suspend_file_info.home_base)
            {
                Turn_Label.text = "Home Base";
                Units_Label.visible = false;
                Units.visible = false;
            }
            else if (suspend_file_info.preparations)
                Turn_Label.text = "Preparations";
            else
            {
                Turn_Label.text = "Turn";
                Turn.text = suspend_file_info.turn.ToString();
            }

            Counter = new Play_Time_Counter(suspend_file_info.playtime);
            Counter.loc = new Vector2(-4, 32);

            Gold_G = new Sprite();
            Gold_G.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Data_Screen");
            Gold_G.loc = new Vector2(140, 32);
            Gold_G.src_rect = new Rectangle(0, 160, 16, 16);

            Screenshot = new Sprite();
            Screenshot.texture = suspend_file_info.Screenshot;
            Screenshot.loc = new Vector2(0, 0);
            Screenshot.scale = new Vector2(0.25f, 0.25f);
        }

        private void create_window(bool mainMenu, IO.Suspend_Info suspend_file_info)
        {
            Window = new System_Color_Window();
            Window.height = 80;
            Window.offset = new Vector2(16, 8);

            Window.width = 208;
            if (Window is System_Color_Window)
                (Window as System_Color_Window).color_override =
                    Constants.Difficulty.DIFFICULTY_COLOR_REDIRECT[suspend_file_info.difficulty];
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = (draw_offset + this.offset) -
                (this.loc + this.draw_offset + stereo_offset());

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Window.draw(sprite_batch, loc);
            Turn_Label.draw(sprite_batch, loc);
            Units_Label.draw(sprite_batch, loc);
            Save_Label.draw(sprite_batch, loc);

            Chapter.draw(sprite_batch, loc);
            Turn.draw(sprite_batch, loc);
            Units.draw(sprite_batch, loc);
            Gold.draw(sprite_batch, loc);
            Save.draw(sprite_batch, loc);
            Mode.draw(sprite_batch, loc);
            Style.draw(sprite_batch, loc);
            Counter.draw(sprite_batch, loc);
            Gold_G.draw(sprite_batch, loc);
            //Screenshot.draw(sprite_batch, loc); //Yeti
            sprite_batch.End();

            Lord_Mini.draw(sprite_batch, loc);
        }
    }
}
