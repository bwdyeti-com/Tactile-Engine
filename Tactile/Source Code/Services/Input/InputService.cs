using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using TactileLibrary;
using EnumExtension;

namespace Tactile.Services.Input
{
    class InputService : BaseInputService
    {
        const float TOUCH_PRESS_TOLERANCE = 8f;

        readonly static Buttons[] ALL_BUTTONS;

        private InputStates[] ButtonStates;

        private Dictionary<MouseButtons, InputStates> MouseStates;
        private Vector2 MousePosition, LastMousePosition;
        private Dictionary<MouseButtons, Maybe<Vector2>> MouseDownLocs =
            new Dictionary<MouseButtons,Maybe<Vector2>>();
        private int MouseScroll;

        private KeyboardState KeyState, LastKeyState;
        private GamePadState GamepadState, LastGamepadState;

        private Dictionary<TouchGestures, GestureSample> Gestures;
        private Dictionary<TouchGestures, InputStates> GestureStates;
        private InputStates TouchState;
        private Vector2 TouchTriggerPosition, TouchPressPosition, TouchReleasePosition;
        private List<float> PinchLengthDeltas;
        private List<GestureSample> DeferredTaps;
        private bool TouchPressOutsideTolerance;

        static InputService()
        {
            ALL_BUTTONS = new Buttons[]
            {
                Buttons.DPadUp,
                Buttons.DPadDown,
                Buttons.DPadLeft,
                Buttons.DPadRight,
                Buttons.Start,
                Buttons.Back,
                Buttons.LeftStick,
                Buttons.RightStick,
                Buttons.LeftShoulder,
                Buttons.RightShoulder,
                Buttons.BigButton,
                Buttons.A,
                Buttons.B,
                Buttons.X,
                Buttons.Y,
                Buttons.LeftThumbstickLeft,
                Buttons.RightTrigger,
                Buttons.LeftTrigger,
                Buttons.RightThumbstickUp,
                Buttons.RightThumbstickDown,
                Buttons.RightThumbstickRight,
                Buttons.RightThumbstickLeft,
                Buttons.LeftThumbstickUp,
                Buttons.LeftThumbstickDown,
                Buttons.LeftThumbstickRight
            };
        }

        public InputService(Game game)
            : base(game)
        {
            ButtonStates = new InputStates[Enum_Values.GetEnumCount(typeof(Inputs))];
            MouseStates = Enum_Values.GetEnumValues(typeof(MouseButtons))
                .ToDictionary(x => (MouseButtons)x, x => InputStates.None);
            Gestures = new Dictionary<TouchGestures, GestureSample>();
            GestureStates = Enum_Values.GetEnumValues(typeof(TouchGestures))
                .ToDictionary(x => (TouchGestures)x, x => InputStates.None);

            PinchLengthDeltas = new List<float>();
            DeferredTaps = new List<GestureSample>();
        }

        public override void update_input(
            Inputs inputName,
            bool pressed,
            bool triggered,
            bool released,
            bool repeated)
        {
            InputStates state = InputStates.None;
            if (pressed)
            {
                state |= Services.Input.InputStates.Pressed;
                if (triggered)
                    state |= Services.Input.InputStates.Triggered;
                if (repeated)
                    state |= Services.Input.InputStates.Repeated;
            }
            else if (released)
                state |= Services.Input.InputStates.Released;

            if (ButtonStates[(int)inputName].HasEnumFlag(InputStates.InputConsumed))
            {
                if (state != InputStates.None)
                    state |= InputStates.InputConsumed;
            }

            ButtonStates[(int)inputName] = state;
        }
        public override void update_input(
            MouseButtons inputName,
            bool pressed,
            bool triggered,
            bool released,
            bool clicked)
        {
            InputStates state = InputStates.None;
            if (pressed)
            {
                state |= Services.Input.InputStates.Pressed;
                if (triggered)
                    state |= Services.Input.InputStates.Triggered;
            }
            else
            {
                if (released)
                    state |= Services.Input.InputStates.Released;
                if (clicked)
                    state |= Services.Input.InputStates.Click;
            }

            if (MouseStates[inputName].HasEnumFlag(InputStates.InputConsumed))
            {
                if (state != InputStates.None)
                    state |= InputStates.InputConsumed;
            }

            MouseStates[inputName] = state;
        }

        public override void update_mouse(
            Vector2 mousePosition,
            Dictionary<MouseButtons, Maybe<Vector2>> mouseDownLocs,
            int mouseScroll)
        {
            LastMousePosition = MousePosition;
            MousePosition = mousePosition;

            MouseDownLocs.Clear();
            foreach (var pair in mouseDownLocs)
                MouseDownLocs.Add(pair.Key, pair.Value);

            MouseScroll = mouseScroll;
        }

        public override void update_input(TouchGestures gesture, GestureSample sample)
        {
#if DEBUG
            bool gesture_added = false;
            bool movement_gesture = false;
#endif

            switch (gesture)
            {
                case TouchGestures.FreeDrag:
                    Gestures[gesture] = sample;
#if DEBUG
                    gesture_added = true;
                    movement_gesture = true;
#endif
                    break;
                case TouchGestures.HorizontalDrag:
                    sample = new GestureSample(
                        sample.GestureType,
                        sample.Timestamp,
                        sample.Position,
                        sample.Position2,
                        new Vector2(sample.Delta.X, 0),
                        new Vector2(sample.Delta2.X, 0));
                    Gestures[gesture] = sample;
#if DEBUG
                    gesture_added = true;
                    movement_gesture = true;
#endif
                    break;
                case TouchGestures.VerticalDrag:
                    sample = new GestureSample(
                        sample.GestureType,
                        sample.Timestamp,
                        sample.Position,
                        sample.Position2,
                        new Vector2(0, sample.Delta.Y),
                        new Vector2(0, sample.Delta2.Y));
                    Gestures[gesture] = sample;
#if DEBUG
                    gesture_added = true;
                    movement_gesture = true;
#endif
                    break;
                #region Swipes
                case TouchGestures.SwipeDown:
                    if (sample.Delta.Y > 0 && Math.Abs(sample.Delta.Y) >
                        Math.Abs(sample.Delta.X))
                    {
                        Gestures[gesture] = sample;
#if DEBUG
                        gesture_added = true;
                        movement_gesture = true;
#endif
                    }
                    break;
                case TouchGestures.SwipeLeft:
                    if (sample.Delta.X < 0 && Math.Abs(sample.Delta.X) >
                        Math.Abs(sample.Delta.Y))
                    {
                        Gestures[gesture] = sample;
#if DEBUG
                        gesture_added = true;
                        movement_gesture = true;
#endif
                    }
                    break;
                case TouchGestures.SwipeRight:
                    if (sample.Delta.X > 0 && Math.Abs(sample.Delta.X) >
                        Math.Abs(sample.Delta.Y))
                    {
                        Gestures[gesture] = sample;
#if DEBUG
                        gesture_added = true;
                        movement_gesture = true;
#endif
                    }
                    break;
                case TouchGestures.SwipeUp:
                    if (sample.Delta.Y < 0 && Math.Abs(sample.Delta.Y) >
                        Math.Abs(sample.Delta.X))
                    {
                        Gestures[gesture] = sample;
#if DEBUG
                        gesture_added = true;
                        movement_gesture = true;
#endif
                    }
                    break;
                #endregion // Swipes
                case TouchGestures.Pinch:
                    Vector2 sample_position =
                        sample.Position - sample.Position2;
                    sample_position = Tactile.Input.mouse_world_loc(sample_position);

                    Vector2 sample_delta_position =
                        (sample.Position + sample.Delta) -
                        (sample.Position2 + sample.Delta2);
                    sample_delta_position =
                        Tactile.Input.mouse_world_loc(sample_delta_position);

                    float length =
                        sample_delta_position.LengthSquared() -
                        sample_position.LengthSquared();

                    PinchLengthDeltas.Add(length);
                    while (PinchLengthDeltas.Count > 8)
                        PinchLengthDeltas.RemoveAt(0);
                    break;
                case TouchGestures.PinchIn:
                    if (PinchLengthDeltas.Any())
                    {
                        float pinch_motion = PinchLengthDeltas.Sum();
                        int screen_distance = Config.WINDOW_WIDTH / 4;
                        if (Math.Abs(pinch_motion) >
                            screen_distance * screen_distance)
                        {
                            if (pinch_motion < 0)
                            {
                                Gestures[gesture] = sample;
#if DEBUG
                                gesture_added = true;
                                movement_gesture = true;
#endif
                            }
                            else
                            {
                                gesture = TouchGestures.PinchOut;
                                Gestures[gesture] = sample;
#if DEBUG
                                gesture_added = true;
                                movement_gesture = true;
#endif
                            }
                        }
                        PinchLengthDeltas.Clear();
                    }
                    break;
                // PinchIn fires both, depending on movement
                case TouchGestures.PinchOut:
                    break;
                default:
                    Gestures[gesture] = sample;
#if DEBUG
                    gesture_added = true;
#endif
                    break;
            }

#if DEBUG
            if (gesture_added)
            {
                if (movement_gesture)
                {
                    if (sample.Delta.X != 0 || sample.Delta.Y != 0 ||
                            sample.Delta2.X != 0 || sample.Delta2.Y != 0)
                        Console.WriteLine(string.Format(
                            "{0}: Movement [{1}, {2}] - [{3}, {4}]; [{5}, {6}]", gesture,
                            sample.Delta.X, sample.Delta.Y,
                            sample.Delta2.X, sample.Delta2.Y,
                            sample.Position2.X, sample.Position2.Y));
                }
                else
                    Console.WriteLine(string.Format(
                        "{0}: Position [{1}, {2}] - [{3}, {4}]", gesture,
                        sample.Position.X, sample.Position.Y,
                        sample.Position2.X, sample.Position2.Y));
            }
#endif
        }
        public override void update_touch(
            bool pressed,
            bool triggered,
            bool released,
            Vector2 pressLoc,
            Vector2 releasedLoc)
        {
            InputStates state = InputStates.None;
            if (pressed)
            {
                state |= Services.Input.InputStates.Pressed;
                if (triggered)
                    state |= Services.Input.InputStates.Triggered;
            }
            else if (released)
                state |= Services.Input.InputStates.Released;

            if (TouchState.HasEnumFlag(InputStates.InputConsumed))
            {
                // @Debug: Because this waits until the input is none, an input
                // consumed on a trigger will stay consumed if released on the next
                // frame, and on the frame after that if pressed again
                // Basically pressing and releasing a button every 2 ticks
                // is impossible
                if (state != InputStates.None)
                    state |= InputStates.InputConsumed;
            }

            TouchState = state;
            if (triggered)
            {
                TouchTriggerPosition = pressLoc;
                TouchPressOutsideTolerance = false;
            }
            TouchPressPosition = pressLoc;
            if ((TouchTriggerPosition - TouchPressPosition).LengthSquared() >
                    TOUCH_PRESS_TOLERANCE * TOUCH_PRESS_TOLERANCE)
                TouchPressOutsideTolerance = true;
            TouchReleasePosition = releasedLoc;
            
            // If any double taps received, clear deferred taps
            if (Gestures.ContainsKey(TouchGestures.DoubleTap))
                DeferredTaps.Clear();
            // Else add new taps
            else if (Gestures.ContainsKey(TouchGestures.Tap))
            {
                // Clear old deferred taps
                DeferredTaps.Clear();
                // If the tap was consumed, don't defer it
                if (!GestureStates[TouchGestures.Tap].HasEnumFlag(
                        InputStates.InputConsumed))
                    DeferredTaps.Add(Gestures[TouchGestures.Tap]);
            }

            // Clear gesture list each frame
            Gestures.Clear();
            foreach (var key in GestureStates.Keys.ToList())
                GestureStates[key] = InputStates.None;

            // If any taps have existed for long enough they can't be double taps
            var time = DateTime.Now;
            for (int i = DeferredTaps.Count - 1; i >= 0; i--)
            {
                var sample = DeferredTaps[i];
                if (TouchPanelState.DoubleTapThresholdElapsed(sample))
                {
                    Gestures[TouchGestures.TapNoDouble] = sample;
                    DeferredTaps.RemoveAt(i);
#if DEBUG
                    Console.WriteLine(string.Format(
                        "{0}: Position [{1}, {2}] - [{3}, {4}]",
                        TouchGestures.TapNoDouble,
                        sample.Position.X, sample.Position.Y,
                        sample.Position2.X, sample.Position2.Y));
#endif
                }
            }
        }

        private bool consume_input(Inputs inputName)
        {
            if (ButtonStates[(int)inputName].HasEnumFlag(InputStates.InputConsumed))
                return true;

            ButtonStates[(int)inputName] |= InputStates.InputConsumed;
            return false;
        }
        public override bool consume_input(MouseButtons inputName)
        {
            if (MouseStates[inputName].HasEnumFlag(InputStates.InputConsumed))
                return true;
            MouseStates[inputName] |= InputStates.InputConsumed;
            return false;
        }
        private bool consume_input(TouchGestures inputName)
        {
            if (GestureStates[inputName].HasEnumFlag(InputStates.InputConsumed))
                return true;
            GestureStates[inputName] |= InputStates.InputConsumed;
            return false;
        }
        private bool consume_touch_input()
        {
            if (TouchState.HasEnumFlag(InputStates.InputConsumed))
                return true;
            TouchState |= InputStates.InputConsumed;
            return false;
        }

        public override void UpdateKeyboardStart(KeyboardState keyState)
        {
            LastKeyState = KeyState;
            KeyState = keyState;
        }

        public override bool KeyPressed(Keys key)
        {
            return KeyState.IsKeyDown(key) && !LastKeyState.IsKeyDown(key);
        }

        public override Keys[] PressedKeys()
        {
            return KeyState.GetPressedKeys()
                .Where(x => KeyPressed(x))
                .ToArray();
        }

        public override void UpdateGamepadState(GamePadState padState)
        {
            LastGamepadState = GamepadState;
            GamepadState = padState;
        }

        public override Buttons[] PressedButtons()
        {
            return ALL_BUTTONS
                .Where(x => GamepadState.IsButtonDown(x))
                .Where(x => !LastGamepadState.IsButtonDown(x))
                .ToArray();
        }

        #region Controls
        #region Buttons
        protected override bool get_input(
            Inputs inputName,
            InputStates inputState,
            bool consumeInput)
        {
            bool result = ButtonStates[(int)inputName].HasEnumFlag(inputState);
            if (result) //@Debug
            { }
            if (result && consumeInput && consume_input(inputName))
                return false;
            return result;
        }


        public override Directions dir8()
        {
            if (pressed(Inputs.Down))
            {
                if (pressed(Inputs.Left))
                    return Directions.DownLeft;
                else if (pressed(Inputs.Right))
                    return Directions.DownRight;
                else
                    return Directions.Down;
            }
            else if (pressed(Inputs.Up))
            {
                if (pressed(Inputs.Left))
                    return Directions.UpLeft;
                else if (pressed(Inputs.Right))
                    return Directions.UpRight;
                else
                    return Directions.Up;
            }
            else
            {
                if (pressed(Inputs.Left))
                    return Directions.Left;
                else if (pressed(Inputs.Right))
                    return Directions.Right;
                else
                    return 0;
            }
        }
        public override DirectionFlags dir_triggered()
        {
            DirectionFlags dir = DirectionFlags.None;

            if (repeated(Inputs.Down))
                dir |= DirectionFlags.Down;
            else if (repeated(Inputs.Up))
                dir |= DirectionFlags.Up;

            if (repeated(Inputs.Left))
                dir |= DirectionFlags.Left;
            else if (repeated(Inputs.Right))
                dir |= DirectionFlags.Right;

            return dir;
        }

        public override bool soft_reset()
        {
            if (pressed(Inputs.Start) && pressed(Inputs.Select))
            {
                if (pressed(Inputs.A) && pressed(Inputs.B))
                    return true;
                if (pressed(Inputs.L) && pressed(Inputs.R))
                    return true;
            }
            return false;
        }
        #endregion // Buttons

        #region Mouse
        public override Vector2 mousePosition
        {
            get
            {
                return Tactile.Input.mouse_world_loc(
                    (int)MousePosition.X, (int)MousePosition.Y);
            }
        }
        public override bool mouseMoved
        {
            get
            {
                return MousePosition != LastMousePosition;
            }
        }
        public override int mouseScroll { get { return MouseScroll; } }

        protected override bool get_input(
            MouseButtons inputName,
            InputStates inputState,
            bool consumeInput)
        {
            bool result = MouseStates[inputName].HasEnumFlag(inputState);
            if (result && consumeInput && consume_input(inputName))
                return false;
            return result;
        }

        public override bool mouse_down_rectangle(
            MouseButtons button, Rectangle rect, bool consumeInput)
        {
            Maybe<Vector2> mouse_down_loc;
            if (button == MouseButtons.None)
            {
                mouse_down_loc = MousePosition;
                consumeInput = false;
            }
            else
                mouse_down_loc = MouseDownLocs[button];

            if (mouse_down_loc.IsSomething)
            {
                Vector2 down_loc = mouse_down_loc;
                down_loc = Tactile.Input.mouse_world_loc((int)down_loc.X, (int)down_loc.Y);
                bool result = rect.Contains((int)down_loc.X, (int)down_loc.Y);

                if (result && consumeInput && consume_input(button))
                    return false;

                return result;
            }
            return false;
        }

        public override bool mouse_down_rectangle(
            MouseButtons button,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput)
        {
            Maybe<Vector2> mouse_down_loc;
            if (button == MouseButtons.None)
            {
                mouse_down_loc = MousePosition;
                consumeInput = false;
            }
            else
                mouse_down_loc = MouseDownLocs[button];

            if (mouse_down_loc.IsSomething)
            {
                Vector2 down_loc = mouse_down_loc;
                down_loc = Tactile.Input.mouse_world_loc((int)down_loc.X, (int)down_loc.Y);

                bool result = transform_rect_contains(
                    rect, loc, rectOriginOffset, angle, mirrored, down_loc);

                if (result && consumeInput && consume_input(button))
                    return false;

                return result;
            }
            return false;
        }

        private static bool transform_rect_contains(
            Rectangle rect, Vector2 loc, Vector2 rectOriginOffset,
            float angle, bool mirrored, Vector2 down_loc)
        {
            if (mirrored)
            {
                angle += MathHelper.Pi;
                //rectOriginOffset.X = rect.Width - rectOriginOffset.X;
                rectOriginOffset.Y = rect.Height - rectOriginOffset.Y;
            }

            // Rotate the mouse point in reverse around the rectangle origin
            // and you can just check if it is inside the unrotated rectangle
            down_loc -= loc;
            //float mouse_angle = (float)Math.Atan(down_loc.Y / down_loc.X); //Debug
            float mouse_angle = (float)Math.Atan2(down_loc.Y, down_loc.X);
            float length = down_loc.Length();
            down_loc = new Vector2(
                (float)(Math.Cos(-angle + mouse_angle) * length),
                (float)(Math.Sin(-angle + mouse_angle) * length));
            down_loc += rectOriginOffset;
            down_loc += loc;

            bool result = rect.Contains((int)down_loc.X, (int)down_loc.Y);
            return result;
        }

        public override bool mouse_clicked_rectangle(MouseButtons button, Rectangle rect)
        {
            return mouse_released(button) &&
                mouse_down_rectangle(button, rect, false) &&
                rect.Contains((int)mousePosition.X, (int)mousePosition.Y);
        }

        public override bool mouse_clicked_rectangle(
            MouseButtons button,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput)
        {
            if (mouse_released(button, false) && mouse_down_rectangle(
                button, rect, loc, rectOriginOffset, angle, mirrored, false))
            {
                Vector2 up_loc = mousePosition;

                bool result = transform_rect_contains(
                    rect, loc, rectOriginOffset, angle, mirrored, up_loc);

                if (result && consumeInput && consume_input(button))
                    return false;

                return result;
            }
            return false;
        }
        #endregion // Mouse

        #region Touch
        public override Vector2 touchPressPosition
        {
            get
            {
                return Tactile.Input.mouse_world_loc(
                    (int)TouchPressPosition.X, (int)TouchPressPosition.Y);
            }
        }
        public override Vector2 touchReleasePosition
        {
            get
            {
                return Tactile.Input.mouse_world_loc(
                    (int)TouchReleasePosition.X, (int)TouchReleasePosition.Y);
            }
        }
        public override Vector2 freeDragVector
        {
            get
            {
                if (!Gestures.ContainsKey(TouchGestures.FreeDrag))
                    return Vector2.Zero;
                return Tactile.Input.mouse_world_loc(
                    (int)Gestures[TouchGestures.FreeDrag].Delta.X,
                    (int)Gestures[TouchGestures.FreeDrag].Delta.Y);
            }
        }
        public override Vector2 horizontalDragVector
        {
            get
            {
                if (!Gestures.ContainsKey(TouchGestures.HorizontalDrag))
                    return Vector2.Zero;
                return Tactile.Input.mouse_world_loc(
                    (int)Gestures[TouchGestures.HorizontalDrag].Delta.X,
                    (int)Gestures[TouchGestures.HorizontalDrag].Delta.Y);
            }
        }
        public override Vector2 verticalDragVector
        {
            get
            {
                if (!Gestures.ContainsKey(TouchGestures.VerticalDrag))
                    return Vector2.Zero;
                return Tactile.Input.mouse_world_loc(
                    (int)Gestures[TouchGestures.VerticalDrag].Delta.X,
                    (int)Gestures[TouchGestures.VerticalDrag].Delta.Y);
            }
        }

        public override bool any_positional_gesture
        {
            get
            {
                return Gestures.Any(x => gesture_has_loc(x.Key));
            }
        }
        public override Vector2 first_gesture_loc
        {
            get
            {
                if (!any_positional_gesture)
                    return Vector2.Zero;
                return gesture_loc(Gestures.First(x => gesture_has_loc(x.Key)).Key);
            }
        }

        public override bool touch_triggered(bool consumeInput)
        {
            bool result = TouchState.HasEnumFlag(InputStates.Triggered);
            if (result && consumeInput && consume_touch_input())
                return false;
            return result;
        }
        public override bool touch_pressed(bool consumeInput)
        {
            bool result = TouchState.HasEnumFlag(InputStates.Pressed);
            if (result && consumeInput && consume_touch_input())
                return false;
            return result;
        }
        public override bool touch_released(bool consumeInput)
        {
            bool result = TouchState.HasEnumFlag(InputStates.Released);
            if (result && consumeInput && consume_touch_input())
                return false;
            return result;
        }

        public override bool gesture_triggered(TouchGestures gesture, bool consumeInput)
        {
            bool result = Gestures.ContainsKey(gesture);
            if (result) //Debug
            {

            }
            if (result && consumeInput && consume_input(gesture))
                return false;
            return result;
        }

        private bool gesture_has_loc(TouchGestures gesture)
        {
            switch (gesture)
            {
                case TouchGestures.Tap:
                case TouchGestures.DoubleTap:
                case TouchGestures.TapNoDouble:
                case TouchGestures.ShortPress:
                case TouchGestures.LongPress:
                    return true;
                default:
                    return false;
            }
        }

        public override Vector2 gesture_loc(TouchGestures gesture)
        {
            if (gesture_has_loc(gesture))
            {
                Vector2 gesture_loc = Gestures[gesture].Position;
                return Tactile.Input.mouse_world_loc(
                    (int)gesture_loc.X, (int)gesture_loc.Y);
            }
            else
            {
                throw new ArgumentException(string.Format(
                    "Gesture \"{0}\" does not provide position information",
                    gesture));
            }
        }

        private bool GestureHasStartLoc(TouchGestures gesture)
        {
            switch (gesture)
            {
                case TouchGestures.FreeDrag:
                case TouchGestures.HorizontalDrag:
                case TouchGestures.VerticalDrag:
                    return true;
                default:
                    return false;
            }
        }

        public override Vector2 GestureStartLoc(TouchGestures gesture)
        {
            if (GestureHasStartLoc(gesture))
            {
                Vector2 gesture_loc = Gestures[gesture].Position2;
                return Tactile.Input.mouse_world_loc(
                    (int)gesture_loc.X, (int)gesture_loc.Y);
            }
            else
            {
                throw new ArgumentException(string.Format(
                    "Gesture \"{0}\" does not provide start position information",
                    gesture));
            }
        }

        public override bool touch_rectangle(
            InputStates state, Rectangle rect, bool consumeInput)
        {
            Vector2 touch_loc = TouchTriggerPosition;
            if (state == InputStates.Released)
                touch_loc = TouchReleasePosition;
            else if (state == InputStates.Pressed)
                if (TouchPressOutsideTolerance)
                    return false;

            touch_loc = Tactile.Input.mouse_world_loc(touch_loc);

            bool result = rect.Contains(
                (int)touch_loc.X, (int)touch_loc.Y);

            if (TouchState.HasEnumFlag(state))
            {
                if (result && consumeInput && consume_touch_input())
                    return false;

                return result;
            }
            return false;
        }

        public override bool touch_rectangle(
            InputStates state,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput)
        {
            Vector2 touch_loc = TouchTriggerPosition;
            if (state == InputStates.Released)
                touch_loc = TouchReleasePosition;
            else if (state == InputStates.Pressed)
                if (TouchPressOutsideTolerance)
                    return false;

            if (TouchState.HasEnumFlag(state))
            {
                Vector2 down_loc = touch_loc;
                down_loc = Tactile.Input.mouse_world_loc((int)down_loc.X, (int)down_loc.Y);

                bool result = transform_rect_contains(
                    rect, loc, rectOriginOffset, angle, mirrored, down_loc);

                if (result && consumeInput && consume_touch_input())
                    return false;

                return result;
            }
            return false;
        }

        public override bool gesture_rectangle(
            TouchGestures gesture, Rectangle rect, bool consumeInput)
        {
            switch (gesture)
            {
                case TouchGestures.Tap:
                case TouchGestures.DoubleTap:
                case TouchGestures.TapNoDouble:
                case TouchGestures.ShortPress:
                case TouchGestures.LongPress:
                    if (Gestures.ContainsKey(gesture))
                    {
                        Vector2 gesture_loc = this.gesture_loc(gesture);
                        bool result = rect.Contains(
                            (int)gesture_loc.X, (int)gesture_loc.Y);

                        if (result && consumeInput && consume_input(gesture))
                            return false;

                        return result;
                    }
                    return false;
                case TouchGestures.FreeDrag:
                case TouchGestures.HorizontalDrag:
                case TouchGestures.VerticalDrag:
                    if (Gestures.ContainsKey(gesture))
                    {
                        Vector2 gesture_loc = this.GestureStartLoc(gesture);
                        bool result = rect.Contains(
                            (int)gesture_loc.X, (int)gesture_loc.Y);

                        if (result && consumeInput && consume_input(gesture))
                            return false;

                        return result;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public override bool gesture_rectangle(
            TouchGestures gesture,
            Rectangle rect,
            Vector2 loc,
            Vector2 rectOriginOffset,
            float angle,
            bool mirrored,
            bool consumeInput)
        {
            switch (gesture)
            {
                case TouchGestures.Tap:
                case TouchGestures.DoubleTap:
                case TouchGestures.TapNoDouble:
                case TouchGestures.ShortPress:
                case TouchGestures.LongPress:
                    if (Gestures.ContainsKey(gesture))
                    {
                        Vector2 gesture_loc = this.gesture_loc(gesture);

                        bool result = transform_rect_contains(
                            rect, loc, rectOriginOffset, angle, mirrored, gesture_loc);

                        if (result && consumeInput && consume_input(gesture))
                            return false;

                        return result;
                    }
                    return false;
                default:
                    return false;
            }
        }
        #endregion // Touch
        #endregion
    }
}
