using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA
{
    class Window_Help : Sprite
    {
        readonly static int[] RESIZE_RATIO = { 3, 1 };
        const string FONT = "FE7_Convo";

        protected Text_Box Background;
        protected List<Vector2> Locs = new List<Vector2>();
        protected List<Vector2> Resize = new List<Vector2>();
        protected FE_Text Help_Text;
        protected Help_Weapon_Data Weapon_Data;
        protected string Help_String;
        protected int Text_Timer = 0;
        protected Vector2 Size;
        protected int ScreenBottomAdjustment;

        #region Accessors
        public Vector2 size
        {
            get { return Size; }
            set
            {
                Size = value;
                refresh_size();
            }
        }

        private int screen_bottom_cutoff
        {
            get { return Config.WINDOW_HEIGHT + ScreenBottomAdjustment; }
        }

        internal bool finished_moving
        {
            get { return Locs.Count <= 4 && Resize.Count <= 4; }
        }
        #endregion

        public Window_Help()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Message_Window");
            Src_Rect = new Rectangle(40, 0, 26, 11);
            offset = new Vector2(-8, 3);
            Background = new Text_Box(48, 32);
            loc = new Vector2(48, 48);
        }

        public override void update()
        {
            Help_Text.update();
            if (Weapon_Data != null)
                Weapon_Data.update();
            if (Locs.Count > 0)
            {
                loc = new Vector2((int)Locs[0].X, (int)Locs[0].Y);
                Locs.RemoveAt(0);
            }
            update_size();
            if (this.finished_moving)
            {
                if (Help_Text != null)
                    if (Text_Timer <= 0)
                    {
                        Text_Timer = text_speed();
                        if (skip())
                        {
                            add_remaining_text();
                        }
                        else
                            next_help_char();
                    }
                Text_Timer--;
            }
        }

        protected void update_size()
        {
            if (Resize.Count > 0)
            {
                Background.width = (int)Resize[0].X;
                Background.height = (int)Resize[0].Y;
                Resize.RemoveAt(0);
            }
        }

        internal void add_remaining_text()
        {
            while (Help_String.Length > 0)
            {
                add_char(Help_String[0]);
                Help_String = Help_String.Substring(1, Help_String.Length - 1);
            }
        }

        private void next_help_char()
        {
            for (int i = 0; i < characters_at_once(); i++)
                if (Help_String.Length > 0)
                {
                    add_char(Help_String[0]);
                    Help_String = Help_String.Substring(1, Help_String.Length - 1);
                }
        }

        protected virtual bool skip()
        {
            return Global.game_options.text_speed == (int)Constants.Message_Speeds.Max;
        }

        protected virtual int text_speed()
        {
            return Global.game_options.text_speed == (int)Constants.Message_Speeds.Slow ? 2 : 1;
        }

        protected virtual int characters_at_once()
        {
            return (Global.game_options.text_speed == (int)Constants.Message_Speeds.Fast ? 2 : 1);
        }

        protected virtual void add_char(char text)
        {
            Help_Text.text += text;
        }

        public virtual void set_text(string text, Vector2 offset = new Vector2())
        {
            Weapon_Data = null;
            Help_Text = new FE_Text();
            Help_Text.loc = new Vector2(8, 8);
            Help_Text.Font = FONT;
            Help_Text.texture = Global.Content.Load<Texture2D>(
                string.Format(@"Graphics/Fonts/{0}_Black", FONT));
            Help_Text.text = "";
            Help_Text.draw_offset = offset;
            Help_String = Regex.Replace(text, @"LVL_CAP", Constants.Actor.LVL_CAP.ToString());
            int width = 32;
            string[] text_ary = Help_String.Split('\n'); //text.Split('\n'); //Debug
            foreach (string str in text_ary)
                width = Math.Max(Font_Data.text_width(str, FONT), width);
            width = width + (width % 8 == 0 ? 0 : (8 - width % 8)) + 16;
            //size = new Vector2(width, (Math.Max(1, text.Split('\n').Length) + 1) * 16); //Debug
            size = new Vector2(width, (Math.Max(1, Help_String.Split('\n').Length) + 1) * 16);
        }

        public void set_screen_bottom_adjustment(int value)
        {
            ScreenBottomAdjustment = value;
        }
        
        public void set_item(Item_Data item_data)
        {
            set_item(item_data, null);
        }
        public void set_item(Item_Data item_data, Game_Actor actor)
        {
            Weapon_Data = null;
            Help_Text = new FE_Text();
            Help_Text.loc = new Vector2(8, 8);
            Help_Text.Font = FONT;
            Help_Text.texture = Global.Content.Load<Texture2D>(
                string.Format(@"Graphics/Fonts/{0}_Black", FONT));
            Help_Text.text = "";

            Data_Equipment item = item_data.to_equipment;
            Help_String = item.Description.Replace("|", "\n");
            int width, rows;
            // If weapon
            if (item.is_weapon)
            {
                // If staff
                if (!(item as Data_Weapon).is_staff())
                {
                    //Help_Text.loc += new Vector2(0, 32); //Debug
                    width = 176; //160;
                    rows = 2;
                }
                // Else
                else
                {
                    //Help_Text.loc += new Vector2(0, 16); //Debug
                    width = 160;
                    rows = 1;
                }
                foreach (int bonus in (item as Data_Weapon).Effectiveness)
                    if (bonus != 1)
                    {
                        rows++;
                        break;
                    }
                Help_Text.loc += new Vector2(0, rows * 16);

                Weapon_Data = new Help_Weapon_Data(item_data, actor);
                Weapon_Data.loc = new Vector2(8, 8);
                string[] text_ary = Help_String.Split('\n');
                foreach (string str in text_ary)
                    width = Math.Max(Font_Data.text_width(str, FONT), width);
                width = (width % 8 == 0 ? 0 : (8 - width % 8)) + width + 16;
            }
            else
            {
                width = 16;
                rows = 0;
                string[] text_ary = Help_String.Split('\n');
                foreach (string str in text_ary)
                    width = Math.Max(Font_Data.text_width(str, FONT), width);
                width = (width % 8 == 0 ? 0 : (8 - width % 8)) + width + 16;
            }
            size = new Vector2(width, (Math.Max(0, Help_String.Length == 0 ? 0 : Help_String.Split('\n').Length) + rows + 1) * 16);
        }

        #region Move/Resize
        public void set_loc(Vector2 new_loc)
        {
            new_loc += new Vector2(-12 - 32, 16);
            Vector2 end_size = new Vector2(Background.width, Background.height);
            if (Resize.Count > 0)
                end_size = Resize[Resize.Count - 1];
            new_loc.X = (int)MathHelper.Clamp(new_loc.X, 0, Config.WINDOW_WIDTH - end_size.X);
            if (new_loc.Y > this.screen_bottom_cutoff - end_size.Y)
                new_loc.Y = new_loc.Y - (16 + end_size.Y);

            Locs.Clear();
            Locs.Add((loc * RESIZE_RATIO[0] + new_loc * RESIZE_RATIO[1]) / (RESIZE_RATIO[0] + RESIZE_RATIO[1]));
            for (; ; )
            {
                Locs.Add((new Vector2(Locs[Locs.Count - 1].X, Locs[Locs.Count - 1].Y) * RESIZE_RATIO[0] +
                    new_loc * RESIZE_RATIO[1]) / (RESIZE_RATIO[0] + RESIZE_RATIO[1]));

                if (Math.Abs(Locs[Locs.Count - 1].X - new_loc.X) <= 0.5f)
                    Locs[Locs.Count - 1] = new Vector2(new_loc.X, Locs[Locs.Count - 1].Y);
                if (Math.Abs(Locs[Locs.Count - 1].Y - new_loc.Y) <= 0.5f)
                    Locs[Locs.Count - 1] = new Vector2(Locs[Locs.Count - 1].X, new_loc.Y);
                if (Locs[Locs.Count - 1].X == new_loc.X && Locs[Locs.Count - 1].Y == new_loc.Y)
                    break;
            }
        }

        private void refresh_size()
        {
            Resize.Clear();
            Resize.Add((new Vector2(Background.width, Background.height) * RESIZE_RATIO[0] +
                size * RESIZE_RATIO[1]) / (RESIZE_RATIO[0] + RESIZE_RATIO[1]));
            for (; ; )
            {
                Resize.Add((new Vector2(Resize[Resize.Count - 1].X, Resize[Resize.Count - 1].Y) * RESIZE_RATIO[0] +
                    size * RESIZE_RATIO[1]) / (RESIZE_RATIO[0] + RESIZE_RATIO[1]));

                if (Math.Abs(Resize[Resize.Count - 1].X - size.X) <= 0.5f)
                    Resize[Resize.Count - 1] = new Vector2(size.X, Resize[Resize.Count - 1].Y);
                if (Math.Abs(Resize[Resize.Count - 1].Y - size.Y) <= 0.5f)
                    Resize[Resize.Count - 1] = new Vector2(Resize[Resize.Count - 1].X, size.Y);
                if (Resize[Resize.Count - 1].X == size.X && Resize[Resize.Count - 1].Y == size.Y)
                    break;
            }
        }
        #endregion

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (visible)
            {
                Background.draw(sprite_batch, draw_offset - (loc + draw_vector()));
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                if (this.finished_moving)
                {
                    if (Help_Text != null)
                        Help_Text.draw(sprite_batch, draw_offset - (loc + draw_vector()));
                    if (Weapon_Data != null && Resize.Count <= 5 && Locs.Count <= 5)
                        Weapon_Data.draw(sprite_batch, draw_offset - (loc + draw_vector()));
                }
                // Draw help label
                sprite_batch.Draw(texture, this.loc + draw_vector() - draw_offset,
                    src_rect, tint, angle, offset, scale,
                    mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                sprite_batch.End();
            }
        }
    }
}
