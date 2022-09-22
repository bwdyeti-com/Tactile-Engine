﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Windows.UserInterface.Command
{
    class Parchment_Info_Window : Parchment_Confirm_Window
    {
        Message_Arrow Arrow;

        public Parchment_Info_Window(bool world_map_talk_boop = false) : base(world_map_talk_boop)
        {
            Arrow = new Message_Arrow();
            Active = true;
        }

        public override void set_text(string text, Vector2 offset = new Vector2())
        {
            base.set_text(text);

            string[] text_ary = Help_String.Split('\n');
            Arrow.loc = new Vector2(Font_Data.text_width(text_ary[text_ary.Length - 1], Tactile.Config.CONVO_FONT) + 12, (text_ary.Length - 1) * 16 + 8);
                
        }

        public override void update()
        {
            if (is_ready)
                Arrow.update();
            if (Active)
                update_input();
            base.update();
        }

        protected override void update_ui(bool input)
        {
            reset_selected();

            if (input)
            {
                if (Global.Input.triggered(Inputs.A))
                    SelectedIndex = new ConsumedInput(ControlSchemes.Buttons, 0);
                else if (Global.Input.mouse_click(MouseButtons.Left))
                    SelectedIndex = new ConsumedInput(ControlSchemes.Mouse, 0);
                else if (Global.Input.gesture_triggered(TouchGestures.Tap))
                    SelectedIndex = new ConsumedInput(ControlSchemes.Touch, 0);

                if (Global.Input.triggered(Inputs.B))
                    Canceled = true;
            }
        }

        protected override void draw_cursor(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            if (is_ready)
                Arrow.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
