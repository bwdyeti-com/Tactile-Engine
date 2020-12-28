using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Windows.UserInterface.Command
{
    class Preparations_Confirm_Window : Window_Confirmation
    {
        #region Accessors
        new public Vector2 size
        {
            get { return Size; }
            set
            {
                Size = value;
                Background.width = (int)Size.X;
                Background.height = (int)Size.Y;
            }
        }
        #endregion

        protected override void initialize()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Message_Window");
            Src_Rect = new Rectangle(0, 0, 0, 0);
            Background = new Text_Box(48, 32);
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Item_Options_Window");
            loc = new Vector2(48, 48);
        }

        public override void add_choice(string str, Vector2 loc)
        {
            List<TextUINode> choices = Choices == null ?
                new List<TextUINode>() : Choices.ToList();

            var text = new TextSprite();
            text.SetFont(Tactile.Config.UI_FONT, Global.Content, "White");
            text.text = str;

            var node = new TextUINode("", text, text.text_width);
            node.loc = loc;
            choices.Add(node);

            Choices = new UINodeSet<TextUINode>(choices);
            Cursor = new UICursor<TextUINode>(Choices);
            Cursor.hide_when_using_mouse(false);
            // Resize if needed
            int width = Font_Data.text_width(str, Tactile.Config.UI_FONT);
            width = width + (width % 8 == 0 ? 0 : (8 - width % 8)) + 16 + (int)loc.X;
            if (width > Size.X)
                size = new Vector2(width, Size.Y);
        }

        protected override void talk_boop() { }

        protected override void update_input()
        {
            if (Help_String.Length != 0)
                Skip = true;
            base.update_input();
        }

        public override void set_text(string text, Vector2 offset = new Vector2())
        {
            Weapon_Data = null;
            Help_Text = new TextSprite();
            Help_Text.loc = new Vector2(16, 4);
            Help_Text.draw_offset = offset;
            Help_Text.SetFont(Tactile.Config.UI_FONT, Global.Content, "White");
            Help_Text.text = "";
            Help_String = Regex.Replace(text, @"LVL_CAP", Global.ActorConfig.LvlCap.ToString());
            int width = 32;
            string[] text_ary = Help_String.Split('\n');
            foreach (string str in text_ary)
                width = Math.Max(Font_Data.text_width(str, Tactile.Config.UI_FONT), width);
            width = width + (width % 8 == 0 ? 0 : (8 - width % 8)) + 16;
            size = new Vector2(width, (Math.Max(1, Help_String.Split('\n').Length) + 1) * 16);
        }
    }
}
