using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Graphics.Text;

namespace Tactile.Windows.Map.Info
{
    enum Button_Description_Mode { Normal, Unit_Highlighted, Unit_Selected, Unit_Selected_Blank }
    class Window_Button_Descriptions : Stereoscopic_Graphic_Object
    {
        protected Button_Description_Mode Mode = Button_Description_Mode.Normal;
        protected List<Button_Description> Buttons;

        public Window_Button_Descriptions()
        {
            refresh();
        }

        public void update()
        {
            Game_Unit unit_here = Global.game_map.is_off_map(Global.player.loc, false) ? null : Global.game_map.get_unit(Global.player.loc);
            switch (Mode)
            {
                case Button_Description_Mode.Normal:
                    if (Global.game_system.Selected_Unit_Id != -1)
                    {
                        if (unit_here != null && unit_here.visible_by())
                            Mode = Button_Description_Mode.Unit_Selected;
                        else
                            Mode = Button_Description_Mode.Unit_Selected_Blank;
                        refresh();
                    }
#if DEBUG
                    else if (unit_here != null)
#else
                    else if (Global.game_temp.highlighted_unit_id != -1)
#endif
                    {
                        Mode = Button_Description_Mode.Unit_Highlighted;
                        refresh();
                    }
                    break;
                case Button_Description_Mode.Unit_Highlighted:
                    if (Global.game_system.Selected_Unit_Id != -1)
                    {
                        if (unit_here != null && unit_here.visible_by())
                            Mode = Button_Description_Mode.Unit_Selected;
                        else
                            Mode = Button_Description_Mode.Unit_Selected_Blank;
                        refresh();
                    }
#if DEBUG
                    else if (unit_here == null)
#else
                    else if (Global.game_temp.highlighted_unit_id == -1)
#endif
                    {
                        Mode = Button_Description_Mode.Normal;
                        refresh();
                    }
                    break;
                case Button_Description_Mode.Unit_Selected:
                    if (Global.game_system.Selected_Unit_Id == -1)
                    {
#if DEBUG
                        if (unit_here != null)
#else
                        if (Global.game_temp.highlighted_unit_id != -1)
#endif
                            Mode = Button_Description_Mode.Unit_Highlighted;
                        else
                            Mode = Button_Description_Mode.Normal;
                        refresh();
                    }
                    else if (unit_here == null)
                    {
                        Mode = Button_Description_Mode.Unit_Selected_Blank;
                        refresh();
                    }
                    break;
                case Button_Description_Mode.Unit_Selected_Blank:
                    if (Global.game_system.Selected_Unit_Id == -1)
                    {
#if DEBUG
                        if (unit_here != null)
#else
                        if (Global.game_temp.highlighted_unit_id != -1)
#endif
                            Mode = Button_Description_Mode.Unit_Highlighted;
                        else
                            Mode = Button_Description_Mode.Normal;
                        refresh();
                    }
                    else if (unit_here != null)
                    {
                        Mode = Button_Description_Mode.Unit_Selected;
                        refresh();
                    }
                    break;
            }

            if (Input.ControlSchemeSwitched)
                refresh();
        }

        protected void refresh()
        {
            Buttons = new List<Button_Description>();
            int button_x = this.buttons_base_x;
            switch (Mode)
            {
                case Button_Description_Mode.Normal:
                    Buttons.Add(Button_Description.button(Inputs.X, button_x));
                    Buttons[Buttons.Count - 1].description = "Enemy Range";
                    //base_x += 80; //Debug
                    button_x = next_button_x(button_x, Buttons[Buttons.Count - 1].width);
                    Buttons.Add(Button_Description.button(Inputs.L, button_x));
                    Buttons[Buttons.Count - 1].description = "Next Unit";
                    //base_x += 68; //Debug
                    button_x = next_button_x(button_x, Buttons[Buttons.Count - 1].width);
                    Buttons.Add(Button_Description.button(Inputs.Select, button_x));
                    Buttons[Buttons.Count - 1].description = "Menu";
                    break;
                case Button_Description_Mode.Unit_Highlighted:
                    Buttons.Add(Button_Description.button(Inputs.X, button_x));
                    Buttons[Buttons.Count - 1].description = "Enemy Range";
                    //base_x += 80; //Debug
                    button_x = next_button_x(button_x, Buttons[Buttons.Count - 1].width);
                    Buttons.Add(Button_Description.button(Inputs.R, button_x));
                    Buttons[Buttons.Count - 1].description = "Info";
                    //base_x += 44; //Debug
                    button_x = next_button_x(button_x, Buttons[Buttons.Count - 1].width);
                    Buttons.Add(Button_Description.button(Inputs.L, button_x));
                    Buttons[Buttons.Count - 1].description = "Next Unit";
                    //base_x += 68; //Debug
                    button_x = next_button_x(button_x, Buttons[Buttons.Count - 1].width);
                    Buttons.Add(Button_Description.button(Inputs.Select, button_x));
                    Buttons[Buttons.Count - 1].description = "Menu";
                    break;
                case Button_Description_Mode.Unit_Selected:
                    Buttons.Add(Button_Description.button(Inputs.X, button_x));
                    Buttons[Buttons.Count - 1].description = "Enemy Range";
                    //base_x += 80; //Debug
                    button_x = next_button_x(button_x, Buttons[Buttons.Count - 1].width);
                    Buttons.Add(Button_Description.button(Inputs.R, button_x));
                    Buttons[Buttons.Count - 1].description = "Info";
                    //base_x += 44; //Debug
                    button_x = next_button_x(button_x, Buttons[Buttons.Count - 1].width);
                    Buttons.Add(Button_Description.button(Inputs.L, button_x));
                    Buttons[Buttons.Count - 1].description = "Reset Arrow";
                    break;
                case Button_Description_Mode.Unit_Selected_Blank:
                    Buttons.Add(Button_Description.button(Inputs.X, button_x));
                    Buttons[Buttons.Count - 1].description = "Enemy Range";
                    //base_x += 80; //Debug
                    button_x = next_button_x(button_x, Buttons[Buttons.Count - 1].width);
                    Buttons.Add(Button_Description.button(Inputs.L, button_x));
                    Buttons[Buttons.Count - 1].description = "Reset Arrow";
                    break;
            }
        }

        private int buttons_base_x
        {
            get
            {
                int base_x = 0;
                switch (Mode)
                {
                    case Button_Description_Mode.Normal:
                        base_x = 4 + 48;
                        break;
                    case Button_Description_Mode.Unit_Highlighted:
                        base_x = 4 + 28;
                        break;
                    case Button_Description_Mode.Unit_Selected:
                        base_x = 56 + 4;
                        break;
                    case Button_Description_Mode.Unit_Selected_Blank:
                        base_x = 80 + 4;
                        break;
                }
                if (Input.ControlScheme == ControlSchemes.Touch)
                {
                    switch (Mode)
                    {
                        case Button_Description_Mode.Unit_Selected:
                            base_x += 12;
                            break;
                        case Button_Description_Mode.Unit_Selected_Blank:
                            base_x += 8;
                            break;
                        case Button_Description_Mode.Normal:
                        case Button_Description_Mode.Unit_Highlighted:
                        default:
                            base_x += 20;
                            break;
                    }
                }
                return base_x;
            }
        }

        private int next_button_x(int x, int previousButtonWidth)
        {
            previousButtonWidth += 4 + 7;
            x += previousButtonWidth / 8 * 8;
            if (Input.ControlScheme == ControlSchemes.Touch)
                x += 16;
            return x;
        }

        public void draw(SpriteBatch sprite_batch)
        {
            if (Global.game_state.is_button_description_ready)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                foreach (Button_Description button in Buttons)
                    button.Draw(sprite_batch, -draw_vector());
                sprite_batch.End();
            }
        }
    }
}
