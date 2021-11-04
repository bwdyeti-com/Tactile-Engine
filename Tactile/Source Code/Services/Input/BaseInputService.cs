using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using TactileLibrary;

namespace Tactile.Services.Input
{
    [Flags]
    enum InputStates
    {
        None            = 0,
        Pressed         = 1 << 0,
        Triggered       = 1 << 1,
        Released        = 1 << 2,
        Repeated        = 1 << 3,
        Click           = 1 << 4,
        InputConsumed   = 1 << 5
    }
    abstract class BaseInputService : GameComponent, IInputService
    {
        internal BaseInputService(Game game) : base(game) { }

        public abstract void update_input(
            Inputs inputName,
            bool pressed,
            bool triggered,
            bool released,
            bool repeated);
        public abstract void update_input(
            MouseButtons inputName,
            bool pressed,
            bool triggered,
            bool released,
            bool clicked);

        public abstract void update_mouse(
            Vector2 mousePosition,
            Dictionary<MouseButtons, Maybe<Vector2>> mouseDownLocs,
            int mouseScroll);

        public abstract void update_input(TouchGestures gesture, GestureSample sample);
        public abstract void update_touch(
            bool pressed,
            bool triggered,
            bool released,
            Vector2 pressLoc,
            Vector2 releasedLoc);

        public abstract bool consume_input(MouseButtons inputName);

        public abstract void UpdateKeyboardStart(KeyboardState keyState);

        public abstract bool KeyPressed(Keys key);

        public abstract Keys[] PressedKeys();

        public abstract void UpdateGamepadState(GamePadState padState);

        public abstract Buttons[] PressedButtons();

        #region Controls
        #region Buttons
        public bool triggered(Inputs inputName, bool consumeInput)
        {
            return get_input(inputName, InputStates.Triggered, consumeInput);
        }

        public bool pressed(Inputs inputName)
        {
            return get_input(inputName, InputStates.Pressed, false);
        }

        public bool released(Inputs inputName, bool consumeInput)
        {
            return get_input(inputName, InputStates.Released, consumeInput);
        }

        public bool repeated(Inputs inputName)
        {
            return get_input(inputName, InputStates.Repeated, false);
        }

        protected abstract bool get_input(
            Inputs inputName,
            InputStates inputState,
            bool consumeInput);


        public abstract Directions dir8();
        public abstract DirectionFlags dir_triggered();

        public abstract bool soft_reset();

        public bool speed_up_input()
        {
            foreach (Inputs input in Tactile.Input.speed_up_inputs())
                if (pressed(input))
                    return true;
            return false;
        }
        #endregion // Buttons

        #region Mouse
        public abstract Vector2 mousePosition { get; }
        public abstract bool mouseMoved { get; }
        public abstract int mouseScroll { get; }

        public bool click
        {
            get
            {
                return mouse_click(MouseButtons.Left) ||
                    mouse_click(MouseButtons.Right) ||
                    mouse_click(MouseButtons.Middle);
            }
        }
        public bool any_mouse_triggered
        {
            get
            {
                return mouse_triggered(MouseButtons.Left) ||
                    mouse_triggered(MouseButtons.Right) ||
                    mouse_triggered(MouseButtons.Middle);
            }
        }

        public bool mouse_click(MouseButtons inputName, bool consumeInput = true)
        {
            return get_input(inputName, InputStates.Click, consumeInput);
        }

        public bool mouse_triggered(MouseButtons inputName, bool consumeInput = true)
        {
            return get_input(inputName, InputStates.Triggered, consumeInput);
        }

        public bool mouse_pressed(MouseButtons inputName, bool consumeInput = true)
        {
            return get_input(inputName, InputStates.Pressed, consumeInput);
        }

        public bool mouse_released(MouseButtons inputName, bool consumeInput = true)
        {
            return get_input(inputName, InputStates.Released, consumeInput);
        }

        protected abstract bool get_input(
            MouseButtons inputName,
            InputStates inputState,
            bool consumeInput);

        public bool mouse_in_rectangle(Rectangle rect)
        {
            return mouse_down_rectangle(MouseButtons.None, rect, false);
        }
        public bool mouse_in_rectangle(
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored)
        {
            return mouse_down_rectangle(MouseButtons.None,
                rect, loc, rectOriginOffset, angle, mirrored, false);
        }

        public abstract bool mouse_down_rectangle(
            MouseButtons button, Rectangle rect, bool consumeInput);

        public abstract bool mouse_down_rectangle(
            MouseButtons button,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput);

        public abstract bool mouse_clicked_rectangle(
            MouseButtons button, Rectangle rect);
        public abstract bool mouse_clicked_rectangle(
            MouseButtons button,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput);
        #endregion // Mouse

        #region Touch
        public abstract Vector2 touchPressPosition { get; }
        public abstract Vector2 touchReleasePosition { get; }
        public abstract Vector2 freeDragVector { get; }
        public abstract Vector2 horizontalDragVector { get; }
        public abstract Vector2 verticalDragVector { get; }

        public abstract bool touch_triggered(bool consumeInput);
        public abstract bool touch_pressed(bool consumeInput);
        public abstract bool touch_released(bool consumeInput);

        public abstract bool any_positional_gesture { get; }
        public abstract Vector2 first_gesture_loc { get; }

        public abstract bool gesture_triggered(TouchGestures gesture, bool consumeInput);

        public abstract Vector2 gesture_loc(TouchGestures gesture);

        public abstract Vector2 GestureStartLoc(TouchGestures gesture);

        public abstract bool touch_rectangle(
            InputStates state, Rectangle rect, bool consumeInput);

        public abstract bool touch_rectangle(
            InputStates state,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput);

        public abstract bool gesture_rectangle(
            TouchGestures gesture, Rectangle rect, bool consumeInput);

        public abstract bool gesture_rectangle(
            TouchGestures gesture,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput);
        #endregion // Touch
        #endregion // Controls
    }
}
