using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FEXNA_Library;
using EnumExtension;
using InputConfig = FEXNA.Services.Input.InputConfig;
using InputState = FEXNA.Services.Input.InputState;
#if __MOBILE__ || TOUCH_EMULATION
using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace FEXNA
{
    enum Inputs : byte { Down, Left, Right, Up, A, B, Y, X, L, R, Start, Select }
    enum MouseButtons
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Middle = 1 << 2,
    }
    enum TouchGestures
    {
        None =          0,
        Tap =           1 << 0,
        DoubleTap =     1 << 1,
        TapNoDouble =   1 << 2,
        ShortPress =    1 << 3,
        LongPress =     1 << 4,
        FreeDrag =      1 << 5,
        VerticalDrag =  1 << 6,
        SwipeDown =     1 << 7,
        SwipeLeft =     1 << 8,
        SwipeRight =    1 << 9,
        SwipeUp =       1 << 10,
        Pinch =         1 << 11,
        PinchIn =       1 << 12,
        PinchOut =      1 << 13,
        Scrubbing =     1 << 14,
    }
    enum Directions : byte
    {
        None =          0,
        DownLeft =      1,
        Down =          2,
        DownRight =     3,
        Left =          4,
        Right =         6,
        UpLeft =        7,
        Up =            8,
        UpRight =       9,
    }
    [Flags]
    enum DirectionFlags : byte
    {
        None = 0,
        Down =          1 << 0,
        Left =          1 << 1,
        Right =         1 << 2,
        Up =            1 << 3,

        DownLeft =      Down | Left,
        DownRight =     Down | Right,
        UpLeft =        Up | Left,
        UpRight =       Up | Right,
    }
    enum ControlSchemes { Buttons, Mouse, Touch }

    public class Input
    {
        internal const float STICK_DEAD_ZONE = 0.2f;
        const float CLICK_TRAVEL_DIST = 8f;

        internal const bool INVERSE_DIRECTIONS_CANCEL = true;

        internal readonly static HashSet<Inputs> DIRECTIONS = new HashSet<Inputs> { Inputs.Up, Inputs.Down, Inputs.Left, Inputs.Right };
        internal readonly static Dictionary<Inputs, Keys> INPUT_OVERRIDES = new Dictionary<Inputs, Keys> {
            { Inputs.Down, Keys.Down },
            { Inputs.Left, Keys.Left },
            { Inputs.Right, Keys.Right },
            { Inputs.Up, Keys.Up }
        };
        internal readonly static Dictionary<Keys, string> REMAPPABLE_KEYS = new Dictionary<Keys, string> {
            { Keys.A, "A" },
            { Keys.B, "B" },
            { Keys.C, "C" },
            { Keys.D, "D" },
            { Keys.E, "E" },
            { Keys.F, "F" },
            { Keys.G, "G" },
            { Keys.H, "H" },
            { Keys.I, "I" },
            { Keys.J, "J" },
            { Keys.K, "K" },
            { Keys.L, "L" },
            { Keys.M, "M" },
            { Keys.N, "N" },
            { Keys.O, "O" },
            { Keys.P, "P" },
            { Keys.Q, "Q" },
            { Keys.R, "R" },
            { Keys.S, "S" },
            { Keys.T, "T" },
            { Keys.U, "U" },
            { Keys.V, "V" },
            { Keys.W, "W" },
            { Keys.X, "X" },
            { Keys.Y, "Y" },
            { Keys.Z, "Z" },
            { Keys.Enter, "Enter" },
            { Keys.RightShift, "RShift" },
            { Keys.Back, "Back" },
            { Keys.Space, "Space" },
            { Keys.NumPad0, "Num0" },
            { Keys.NumPad1, "Num1" },
            { Keys.NumPad2, "Num2" },
            { Keys.NumPad3, "Num3" },
            { Keys.NumPad4, "Num4" },
            { Keys.NumPad5, "Num5" },
            { Keys.NumPad6, "Num6" },
            { Keys.NumPad7, "Num7" },
            { Keys.NumPad8, "Num8" },
            { Keys.NumPad9, "Num9" },
            { Keys.OemMinus, "-" },
            { Keys.OemPlus, "+" },
            { Keys.OemPipe, "\\" },
            { Keys.OemOpenBrackets, "[" },
            { Keys.OemCloseBrackets, "]" },
            { Keys.OemSemicolon, ";" },
            { Keys.OemQuotes, "'" },
            { Keys.OemComma, "," },
            { Keys.OemPeriod, "." },
            { Keys.OemQuestion, "/" }
        };
        
        private static InputConfig InputConfig;
        private static InputState[] PlayerInputs;
        private static InputState PlayerOneInputs { get { return PlayerInputs[0]; } }
        
        private static MouseState MouseState, LastMouseState;
        private static MouseButtons MouseComboHeld;
        private static Dictionary<MouseButtons, Maybe<Vector2>> MouseClickLocs = new Dictionary<MouseButtons, Maybe<Vector2>>
        {
            { MouseButtons.Left, default(Maybe<Vector2>) },
            { MouseButtons.Right, default(Maybe<Vector2>) },
            { MouseButtons.Middle, default(Maybe<Vector2>) }
        };
        private static Dictionary<MouseButtons, Maybe<Vector2>> MouseDownLocs = new Dictionary<MouseButtons, Maybe<Vector2>>
        {
            { MouseButtons.Left, default(Maybe<Vector2>) },
            { MouseButtons.Right, default(Maybe<Vector2>) },
            { MouseButtons.Middle, default(Maybe<Vector2>) }
        };
        private static MouseButtons MouseButtonClicks = MouseButtons.None;
        private static Vector2 ScreenScaleZoom = Vector2.One;

#if __MOBILE__ || TOUCH_EMULATION
        private static TouchCollection _touchCollection;
        private static HashSet<GestureSample> Gestures = new HashSet<GestureSample>();
#if TOUCH_EMULATION
        private static MouseState LastMouseTouchState;
#endif
#endif

        public static bool Controller_Active { get; protected set; }
        public static bool ControlSchemeSwitched { get; protected set; }
        private static bool ShouldSwitchControlScheme = false;
        internal static ControlSchemes ControlScheme { get; private set; }
        public static bool IsControllingOnscreenMouse
        {
            get
            {
                return ControlScheme == ControlSchemes.Mouse && mouseOnscreen && GameActive;
            }
        }
        public static bool MouseVisible
        {
            get
            {
#if TOUCH_EMULATION
                return !IsControllingOnscreenMouse;
#else
                return !IsControllingOnscreenMouse;
#endif
            }
        }
        public static bool GameActive { get; private set; }

        private static Vector2 mousePosition { get { return (mouse_world_loc(MouseState.X, MouseState.Y)); } }

        private static bool mouseOnscreen
        {
            get
            {
                return new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT)
                    .Contains((int)mousePosition.X, (int)mousePosition.Y);
            }
        }

        private static Enum[] InputEnums;
        private static Enum[] MouseEnums;
        private static Enum[] GestureEnums;

        #region Serialization
        public static void write(BinaryWriter writer)
        {
            InputConfig.write(writer);
        }
        #endregion

        #region Config
        public static void default_controls()
        {
            InputConfig.SetDefaults();
        }

        internal static string key_name(Inputs input)
        {
            return InputConfig.KeyName(input);
        }
        internal static string key_name(Keys key)
        {
            return InputConfig.KeyName(key);
        }

        internal static bool remap_key(Inputs input, Keys key)
        {
            return InputConfig.RemapKey(input, key);
        }
        #endregion

        static Input()
        {
            ControlScheme = ControlSchemes.Buttons;
#if TOUCH_EMULATION || __MOBILE__
            ControlScheme = ControlSchemes.Touch;
#endif

#if TOUCH_EMULATION || __MOBILE__
            TouchPanel.EnableMouseTouchPoint = true;
            TouchPanel.EnableMouseGestures = true;

            TouchPanel.EnabledGestures =
                GestureType.Tap | GestureType.DoubleTap |
                GestureType.Hold | GestureType.ShortHold |
                GestureType.FreeDrag | GestureType.Flick |
                GestureType.Pinch | GestureType.PinchComplete;
            List<Vector2> TouchPressLocations = new List<Vector2>();
            List<Vector2> TouchReleaseLocations = new List<Vector2>();

            /*// Is this every possible gesture lol // yep
            TouchPanel.EnabledGestures =
                GestureType.Tap | GestureType.DoubleTap | GestureType.DragComplete |
                GestureType.HorizontalDrag | GestureType.VerticalDrag |
                GestureType.FreeDrag | GestureType.Flick | GestureType.Hold |
                GestureType.Pinch | GestureType.PinchComplete;*/
#endif

            InputConfig = new InputConfig();
            //PlayerInputs = Enumerable.Range(0, (int)PlayerIndex.Four + 1) //@Debug
            PlayerInputs = Enumerable.Range(0, (int)PlayerIndex.One + 1)
                .Select(x => new InputState())
                .ToArray();

            InputEnums = Enum_Values.GetEnumValues(typeof(Inputs));
            MouseEnums = Enum_Values.GetEnumValues(typeof(MouseButtons));
            GestureEnums = Enum_Values.GetEnumValues(typeof(TouchGestures));
        }
        
        #region Update
        public static void update(bool game_active, GameTime gameTime,
            KeyboardState key_state, GamePadState controller_state)
        {
            PlayerInputs = InputConfig.Update(PlayerInputs);
            
            LastMouseState = MouseState;
            MouseState = Mouse.GetState();
#if TOUCH_EMULATION
            //The TouchPanel needs to know the time for when touches arrive
            TouchPanelState.CurrentTimestamp = gameTime.TotalGameTime;

            Vector2 posThisFrame = new Vector2(MouseState.X, MouseState.Y);
            Vector2 posLastFrame = new Vector2(LastMouseTouchState.X, LastMouseTouchState.Y);

            simulate_touch(MouseState.LeftButton, LastMouseTouchState.LeftButton,
                posThisFrame, posLastFrame, false);
            simulate_touch(MouseState.RightButton, LastMouseTouchState.RightButton,
                posThisFrame, posLastFrame, true);

            LastMouseTouchState = MouseState;
            MouseState = new MouseState();
#endif

            // Current keyboard/controller state
            //KeyboardState key_state = Keyboard.GetState(); //Debug
            //GamePadState controller_state = GamePad.GetState(PlayerIndex.One);
            float left_stick_angle = (float)Math.Atan2(controller_state.ThumbSticks.Left.Y, controller_state.ThumbSticks.Left.X);
            if (left_stick_angle < 0)
                left_stick_angle += MathHelper.TwoPi;
            left_stick_angle *= 360 / MathHelper.TwoPi;
            
            ControlSchemeSwitched = false;

#if __MOBILE__ || TOUCH_EMULATION
            _touchCollection = TouchPanel.GetState();
            Gestures.Clear();
            while (TouchPanel.IsGestureAvailable)
            {
                var gesture = TouchPanel.ReadGesture();
                Gestures.Add(gesture);
            }

#if CONTROL_MOUSE_WITH_TOUCH
                MouseState = new MouseState(
                    (int)(Config.WINDOW_WIDTH * ScreenScaleZoom.X / 2),
                    (int)(Config.WINDOW_HEIGHT * ScreenScaleZoom.Y / 2), 0,
                    ButtonState.Released,
                    ButtonState.Released, ButtonState.Released,
                    ButtonState.Released, ButtonState.Released);
            if (_touchCollection.Count != 0)
            {
                if (GameActive)
                {
                    if (ControlScheme != ControlSchemes.Mouse)
                        ControlSchemeSwitched = true;
                    ControlScheme = ControlSchemes.Mouse;
                }

                TouchLocation touch = _touchCollection.First();
                MouseState = new MouseState(
                    (int)touch.Position.X,
                    (int)touch.Position.Y, 0,
                    touch.State == TouchLocationState.Released ?
                    ButtonState.Released : ButtonState.Pressed,
                    ButtonState.Released, ButtonState.Released,
                    ButtonState.Released, ButtonState.Released);
            }
#endif
#endif

            update_control_scheme(key_state, controller_state);

            GameActive = game_active;

            update_mouse_button_hold();
            update_mouse_click_locs();

            update_controller_active(key_state, controller_state);
        }

#if TOUCH_EMULATION
        private static void simulate_touch(
            ButtonState buttonThisFrame, ButtonState buttonLastFrame,
            Vector2 posThisFrame, Vector2 posLastFrame, bool mirrorPosition)
        {
            if (buttonThisFrame == ButtonState.Pressed ||
                buttonLastFrame == ButtonState.Pressed)
            {
                TouchLocationState state;
                Vector2 position;
                // Pressed
                if (buttonLastFrame == ButtonState.Released)
                {
                    state = TouchLocationState.Pressed;
                    position = posThisFrame;
                }
                // Released
                else if (buttonThisFrame == ButtonState.Released)
                {
                    state = TouchLocationState.Released;
                    position = posLastFrame;
                }
                // Moved
                else
                {
                    state = TouchLocationState.Moved;
                    position = posThisFrame;
                }

                if (mirrorPosition)
                    position = new Vector2(
                        Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) *
                        ScreenScaleZoom - position;
                TouchPanel.AddMouseEvent(mirrorPosition ? 1 : 0,
                    state, position);
            }
        }
#endif
        
        private static void update_control_scheme(
            KeyboardState keyState, GamePadState controllerState)
        {
            // Force updating icons/etc
            if (GameActive && ShouldSwitchControlScheme)
            {
                ControlSchemeSwitched = true;
                ShouldSwitchControlScheme = false;
            }

            if (!GameActive)
                LastMouseState = new Microsoft.Xna.Framework.Input.MouseState(
                    LastMouseState.X, LastMouseState.Y, LastMouseState.ScrollWheelValue,
                    ButtonState.Pressed, ButtonState.Pressed, ButtonState.Pressed, ButtonState.Pressed, ButtonState.Pressed);
            // If the player pressed buttons, we want to disable mouse control
            // Otherwise, if the player moves the mouse, enable mouse control
            if (ControlScheme != ControlSchemes.Buttons)
            {
                bool buttons_pressed = controller_pressed(controllerState);
                if (buttons_pressed)
                {
                    ControlScheme = ControlSchemes.Buttons;
                    return;
                }
            }

            if (ControlScheme == ControlSchemes.Mouse)
            {
                if (other_pressed(new HashSet<Inputs> { Inputs.A, Inputs.B, Inputs.Y, Inputs.X,
                    Inputs.L, Inputs.R, Inputs.Start, Inputs.Select }))
                {
                    ControlScheme = ControlSchemes.Buttons;
                    return;
                }
            }
            else
            {
                //else if (MouseState.X != LastMouseState.X || MouseState.Y != LastMouseState.Y) //Debug
                if (any_mouse_triggered && mouseOnscreen)
                {
                    ControlScheme = ControlSchemes.Mouse;
                    ControlSchemeSwitched = true;
                    // Cancel clicks for this mouse state, since we just took control of the game
                    LastMouseState = new Microsoft.Xna.Framework.Input.MouseState(
                        LastMouseState.X, LastMouseState.Y, LastMouseState.ScrollWheelValue,
                        ButtonState.Pressed, ButtonState.Pressed, ButtonState.Pressed, ButtonState.Pressed, ButtonState.Pressed);
                }
            }
#if __MOBILE__ || TOUCH_EMULATION
            if (ControlScheme == ControlSchemes.Touch)
            {
                if (key_pressed(keyState))
                {
                    ControlScheme = ControlSchemes.Buttons;
                    ControlSchemeSwitched = true;
                }
            }
#if !CONTROL_MOUSE_WITH_TOUCH
            else
            {
                if ((_touchCollection.Count != 0 || Gestures.Count != 0) &&
                    !key_pressed(keyState))
                {
                    ControlScheme = ControlSchemes.Touch;
                    ControlSchemeSwitched = true;
                }
            }
#endif
#endif
        }

        internal static void RefreshControlScheme()
        {
            ShouldSwitchControlScheme = true;
        }

        private static void update_mouse_click_locs()
        {
            MouseButtonClicks = MouseButtons.None;

            Vector2 mouse_loc = new Vector2(MouseState.X, MouseState.Y);
            // Left Mouse Button
            mouse_loc = update_mouse_click(mouse_loc, MouseButtons.Left);
            // Right Mouse Button
            mouse_loc = update_mouse_click(mouse_loc, MouseButtons.Right);
            // Middle Mouse Button
            mouse_loc = update_mouse_click(mouse_loc, MouseButtons.Middle);
        }

        private static Vector2 update_mouse_click(Vector2 mouse_loc, MouseButtons mouse_button)
        {
            // If mouse was just pressed
            if (mouse_triggered(mouse_button))
                MouseDownLocs[mouse_button] = mouse_loc;
            // Else if last frame the mouse was released
            else if (mouse_button_state(LastMouseState, mouse_button) == ButtonState.Released)
                MouseDownLocs[mouse_button] = default(Maybe<Vector2>);

            if (MouseClickLocs[mouse_button].IsSomething)
            {
                // If the cursor has moved substantially, cancel out the click location
                if ((MouseClickLocs[mouse_button] - mouse_loc).LengthSquared() > CLICK_TRAVEL_DIST * CLICK_TRAVEL_DIST)
                    MouseClickLocs[mouse_button] = default(Maybe<Vector2>);
                // Else the cursor hasn't moved; thus if it's been released, the mouse was clicked
                else if (mouse_released(mouse_button))
                    MouseButtonClicks |= mouse_button;

                // If the mouse is released, clear the click loc
                if (mouse_button_state(MouseState, mouse_button) == ButtonState.Released)
                    MouseClickLocs[mouse_button] = default(Maybe<Vector2>);
            }
            // If the mouse was just pressed, set the click loc
            else if (mouse_triggered(mouse_button))
            {
                MouseClickLocs[mouse_button] = mouse_loc;
            }
            return mouse_loc;
        }

        private static void update_mouse_button_hold()
        {
            if ((!MouseComboHeld.HasEnumFlag(MouseButtons.Left) || LastMouseState.LeftButton != ButtonState.Pressed) &&
                    (!MouseComboHeld.HasEnumFlag(MouseButtons.Right) || LastMouseState.RightButton != ButtonState.Pressed) &&
                    (!MouseComboHeld.HasEnumFlag(MouseButtons.Middle) || LastMouseState.MiddleButton != ButtonState.Pressed))
                MouseComboHeld = MouseButtons.None;

            var mouse_held = MouseButtons.None;
            if (MouseState.LeftButton == ButtonState.Pressed)
                mouse_held |= MouseButtons.Left;
            if (MouseState.RightButton == ButtonState.Pressed)
                mouse_held |= MouseButtons.Right;
            if (MouseState.MiddleButton == ButtonState.Pressed)
                mouse_held |= MouseButtons.Middle;

            if (mouse_held != MouseButtons.Left && mouse_held != MouseButtons.Right && mouse_held != MouseButtons.Middle)
                MouseComboHeld |= mouse_held;
        }

        private static void update_controller_active(KeyboardState key_state, GamePadState controller_state)
        {
            bool active = Controller_Active;
            bool keys_pressed = key_state.GetPressedKeys().Length > 0;
            bool buttons_pressed = controller_pressed(controller_state);

            if (keys_pressed != buttons_pressed)
            {
                if (Controller_Active)
                {
                    if (keys_pressed)
                        Controller_Active = false;
                }
                else
                {
                    if (buttons_pressed)
                        Controller_Active = true;
                }
            }
            else if (IsControllingOnscreenMouse && Controller_Active)
                Controller_Active = false;

            ControlSchemeSwitched |= Controller_Active != active;
        }

        readonly static HashSet<Buttons> ALL_BUTTONS = new HashSet<Buttons>{
            Buttons.DPadDown, Buttons.DPadLeft, Buttons.DPadRight, Buttons.DPadUp,
            Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.Start, Buttons.Back,
            Buttons.LeftShoulder, Buttons.RightShoulder, Buttons.LeftStick, Buttons.RightStick};

        private static bool key_pressed(KeyboardState keyState)
        {
            return keyState.GetPressedKeys().Length > 0;
        }
        private static bool controller_pressed(GamePadState controllerState)
        {
            if (controllerState.IsConnected)
            {
                // Sticks
                if (controllerState.ThumbSticks.Left.Length() > STICK_DEAD_ZONE || controllerState.ThumbSticks.Right.Length() > STICK_DEAD_ZONE)
                    return true;
                if (ALL_BUTTONS.Any(x => controllerState.IsButtonDown(x)))
                    return true;
                /*foreach(Buttons button in ALL_BUTTONS) //Debug
                    if (controller_state.IsButtonDown(button))
                        return true;*/
            }

            return false;
        }

        public static void update_screen_scale(Vector2 zoom, Vector2 screenSize)
        {
            ScreenScaleZoom = zoom;
#if TOUCH_EMULATION
            TouchPanel.DisplayWidth = (int)screenSize.X;
            TouchPanel.DisplayHeight = (int)screenSize.Y;
            TouchPanel.WindowSize = screenSize;
                //new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) * zoom;
#endif
        }

        public static void clear_locked_repeats()
        {
            Input.PlayerOneInputs.ClearLockedRepeats();
        }
        #endregion

        internal static void update_input_state(
            Services.Input.IInputService inputService)
        {
            if (inputService != null)
            {
                // Buttons
                foreach (Inputs input in InputEnums)
                {
                    inputService.update_input(
                        input,
                        pressed(input),
                        triggered(input),
                        released(input),
                        repeated(input));
                }

                // Mouse
                inputService.update_mouse(
                    new Vector2(MouseState.X, MouseState.Y),
                    MouseDownLocs,
                    ControlScheme != ControlSchemes.Mouse ? 0 :
                        MouseState.ScrollWheelValue - LastMouseState.ScrollWheelValue);
                foreach (MouseButtons input in MouseEnums)
                {
                    if (input != MouseButtons.None)
                        inputService.update_input(
                            input,
                            mouse_pressed(input),
                            mouse_triggered(input),
                            mouse_released(input),
                            MouseButtonClicks.HasEnumFlag(input) &&
                                IsControllingOnscreenMouse);
                }

#if __MOBILE__ || TOUCH_EMULATION
                // Touch
                if (!GameActive)
                {
                    inputService.update_touch(
                        false, false, false, Vector2.Zero, Vector2.Zero);
                }
                else
                {
                    var touch_presses = _touchCollection
                        .Where(x => x.State == TouchLocationState.Pressed ||
                            x.State == TouchLocationState.Moved);
                    var touch_triggers = _touchCollection
                        .Where(x => x.State == TouchLocationState.Pressed);
                    var touch_releases = _touchCollection
                        .Where(x => x.State == TouchLocationState.Released);

                    Vector2 touch_press_loc = touch_presses.Any() ? (touch_presses
                        .Aggregate(Vector2.Zero, (a, b) => a + b.Position) /
                            touch_presses.Count()) :
                        Vector2.Zero;
                    Vector2 touch_release_loc = touch_releases.Any() ? (touch_releases
                        .Aggregate(Vector2.Zero, (a, b) => a + b.Position) /
                            touch_releases.Count()) :
                        Vector2.Zero;

                    inputService.update_touch(
                        touch_presses.Any(),
                        touch_triggers.Any(),
                        touch_releases.Any(),
                        touch_press_loc,
                        touch_release_loc);
                    foreach (TouchGestures input in GestureEnums)
                    {
                        Maybe<GestureSample> sample;
                        sample = convert_gesture(input);
                        if (sample.IsSomething)
                        {
                            inputService.update_input(input, sample);
                        }
                    }
                }
#else
                inputService.update_touch(
                    false, false, false, Vector2.Zero, Vector2.Zero);
#endif
            }
        }

#if __MOBILE__ || TOUCH_EMULATION
        private static Maybe<GestureSample> convert_gesture(TouchGestures input)
        {
            GestureType type = convert_gesture_type(input);

            foreach (var gesture in Gestures)
                if (gesture.GestureType == type)
                    return gesture;

            return default(Maybe<GestureSample>);
        }

        private static GestureType convert_gesture_type(TouchGestures input)
        {
            switch (input)
            {
                case TouchGestures.Tap:
                    return GestureType.Tap;
                case TouchGestures.DoubleTap:
                    return GestureType.DoubleTap;
                case TouchGestures.LongPress:
                    return GestureType.Hold;
                case TouchGestures.ShortPress:
                    return GestureType.ShortHold;
                case TouchGestures.FreeDrag:
                    return GestureType.FreeDrag;
                case TouchGestures.VerticalDrag:
                    return GestureType.FreeDrag;
                case TouchGestures.SwipeDown:
                    return GestureType.Flick;
                case TouchGestures.SwipeLeft:
                    return GestureType.Flick;
                case TouchGestures.SwipeRight:
                    return GestureType.Flick;
                case TouchGestures.SwipeUp:
                    return GestureType.Flick;
                case TouchGestures.Pinch:
                    return GestureType.Pinch;
                case TouchGestures.PinchIn:
                    return GestureType.PinchComplete;
                case TouchGestures.PinchOut:
                    return GestureType.PinchComplete;
            }
            return GestureType.None;
        }
#endif

        #region Controls
        private static bool triggered(Inputs input_name)
        {
            return Input.PlayerOneInputs.Triggered(input_name);
        }

        private static bool pressed(Inputs input_name)
        {
            return Input.PlayerOneInputs.Pressed(input_name);
        }

        private static bool released(Inputs input_name)
        {
            return Input.PlayerOneInputs.Released(input_name);
        }

        private static bool repeated(Inputs input_name)
        {
            return Input.PlayerOneInputs.Repeated(input_name);
        }

        private static int held_time(Inputs input_name)
        {
            return Input.PlayerOneInputs.HeldTime(input_name);
        }

        private static bool other_pressed(HashSet<Inputs> input_names)
        {
            return Input.PlayerOneInputs.OtherPressed(input_names);
        }
        
        internal static IEnumerable<Inputs> speed_up_inputs()
        {
            yield return Inputs.Y;
        }

        internal static void lock_repeat(Inputs input_name)
        {
            Input.PlayerOneInputs.LockRepeat(input_name);
        }

        #region Mouse
        private static bool any_mouse_triggered
        {
            get
            {
                return mouse_triggered(MouseButtons.Left) ||
                    mouse_triggered(MouseButtons.Right) ||
                    mouse_triggered(MouseButtons.Middle);
            }
        }

        private static ButtonState mouse_button_state(MouseState state, MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left:
                    return state.LeftButton;
                case MouseButtons.Right:
                    return state.RightButton;
                case MouseButtons.Middle:
                    return state.MiddleButton;
                default:
#if DEBUG
                    throw new ArgumentException();
#endif
                    return ButtonState.Released;
            }
        }
        internal static Vector2 mouse_world_loc(int x, int y)
        {
            return new Vector2(
                (int)(x / ScreenScaleZoom.X),
                (int)(y / ScreenScaleZoom.Y));
        }
        /// <summary>
        /// Maps a Vector2 from screen space to world space.
        /// X and Y will be converted to int before scaling.
        /// </summary>
        internal static Vector2 mouse_world_loc(Vector2 loc)
        {
            return mouse_world_loc((int)loc.X, (int)loc.Y);
        }

        private static bool mouse_triggered(MouseButtons button)
        {
            return mouse_button_state(MouseState, button) == ButtonState.Pressed &&
                mouse_button_state(LastMouseState, button) == ButtonState.Released &&
                MouseComboHeld == MouseButtons.None && mouseOnscreen;
        }
        private static bool mouse_released(MouseButtons button)
        {
            return mouse_button_state(MouseState, button) == ButtonState.Released &&
                mouse_button_state(LastMouseState, button) == ButtonState.Pressed &&
                MouseComboHeld == MouseButtons.None && mouseOnscreen;
        }
        private static bool mouse_pressed(MouseButtons button)
        {
            return mouse_button_state(MouseState, button) == ButtonState.Pressed;
        }
        #endregion
        #endregion

        internal static DirectionFlags direction_to_flag(Directions dir)
        {
            switch (dir)
            {
                case Directions.DownLeft:
                    return DirectionFlags.DownLeft;
                case Directions.Down:
                    return DirectionFlags.Down;
                case Directions.DownRight:
                    return DirectionFlags.DownRight;
                case Directions.Left:
                    return DirectionFlags.Left;
                case Directions.Right:
                    return DirectionFlags.Right;
                case Directions.UpLeft:
                    return DirectionFlags.UpLeft;
                case Directions.Up:
                    return DirectionFlags.Up;
                case Directions.UpRight:
                    return DirectionFlags.UpRight;

                default:
                    return DirectionFlags.None;
            }
        }

#if DEBUG && WINDOWS
        internal static InputDiagnostics InputDiagnostics()
        {
            var inputs = Enum_Values
                .GetEnumValues(typeof(Inputs))
                .Select(x => (Inputs)x)
                .ToArray();
            
            return new InputDiagnostics(
                inputs
                    .ToDictionary(x => x, x => new Func<bool>(() => pressed(x))),
                inputs
                    .ToDictionary(x => x, x => new Func<int>(() => held_time(x))),
                inputs
                    .ToDictionary(x => x, x => new Func<bool>(() => repeated(x) && !triggered(x))));
        }
#endif
    }

#if DEBUG && WINDOWS
    struct InputDiagnostics
    {
        public Dictionary<Inputs, Func<bool>> Inputs { get; private set; }
        public Dictionary<Inputs, Func<int>> HeldInputsTime { get; private set; }
        public Dictionary<Inputs, Func<bool>> Repeated { get; private set; }

        public InputDiagnostics(
                Dictionary<Inputs, Func<bool>> inputs,
                Dictionary<Inputs, Func<int>> heldInputsTime,
                Dictionary<Inputs, Func<bool>> repeated)
            : this()
        {
            Inputs = inputs; //@Debug: .ToDictionary(p => p.Key, p => p.Value);
            HeldInputsTime = heldInputsTime;
            Repeated = repeated;
        }
    }
#endif
}
