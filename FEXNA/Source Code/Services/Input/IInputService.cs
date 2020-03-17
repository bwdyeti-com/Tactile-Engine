using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using FEXNA_Library;

namespace FEXNA.Services.Input
{
    interface IInputService
    {
        void update_input(
            Inputs inputName,
            bool pressed,
            bool triggered,
            bool released,
            bool repeated);
        void update_input(
            MouseButtons inputName,
            bool pressed,
            bool triggered,
            bool released,
            bool clicked);

        void update_mouse(
            Vector2 mousePosition,
            Dictionary<MouseButtons, Maybe<Vector2>> mouseDownLocs,
            int mouseScroll);

        void update_input(TouchGestures gesture, GestureSample sample);
        void update_touch(
            bool pressed,
            bool triggered,
            bool released,
            Vector2 pressLoc,
            Vector2 releasedLoc);

        bool consume_input(MouseButtons inputName);

        void UpdateKeyboardStart(KeyboardState keyState);

        bool KeyPressed(Keys key);

        Keys[] PressedKeys();

        void UpdateGamepadState(GamePadState padState);

        Buttons[] PressedButtons();

        #region Buttons
        bool triggered(Inputs inputName, bool consumeInput = true);
        bool pressed(Inputs inputName);
        bool released(Inputs inputName, bool consumeInput = true);
        bool repeated(Inputs inputName);


        Directions dir8();
        DirectionFlags dir_triggered();
        bool soft_reset();
        bool speed_up_input();
        #endregion // Buttons


        #region Mouse
        Vector2 mousePosition { get; }
        bool mouseMoved { get; }
        int mouseScroll { get; }

        bool click { get; }
        bool any_mouse_triggered { get; }

        bool mouse_click(MouseButtons inputName, bool consumeInput = true);
        bool mouse_triggered(MouseButtons inputName, bool consumeInput = true);
        bool mouse_pressed(MouseButtons inputName, bool consumeInput = true);
        bool mouse_released(MouseButtons inputName, bool consumeInput = true);

        bool mouse_in_rectangle(Rectangle rect);
        bool mouse_in_rectangle(
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored);

        bool mouse_down_rectangle(
            MouseButtons button, Rectangle rect, bool consumeInput = true);

        bool mouse_down_rectangle(
            MouseButtons button,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput = true);

        bool mouse_clicked_rectangle(
            MouseButtons button,
            Rectangle rect);
        bool mouse_clicked_rectangle(
            MouseButtons button,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput = true);
        #endregion // Mouse


        #region Touch
        Vector2 touchPressPosition { get; }
        Vector2 touchReleasePosition { get; }
        Vector2 freeDragVector { get; }
        Vector2 verticalDragVector { get; }

        bool any_positional_gesture { get; }
        Vector2 first_gesture_loc { get; }

        bool touch_triggered(bool consumeInput = true);
        bool touch_pressed(bool consumeInput = true);
        bool touch_released(bool consumeInput = true);

        bool gesture_triggered(TouchGestures gesture, bool consumeInput = true);

        Vector2 gesture_loc(TouchGestures gesture);

        bool touch_rectangle(
            InputStates state, Rectangle rect, bool consumeInput = true);

        bool touch_rectangle(
            InputStates state,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput = true);

        bool gesture_rectangle(
            TouchGestures gesture, Rectangle rect, bool consumeInput = true);

        bool gesture_rectangle(
            TouchGestures gesture,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput = true);
        #endregion // Touch
    }
}
