using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FEXNA_Library;

namespace FEXNA.Services.Input
{
    class InputState
    {
        const int INITIAL_WAIT = 12;
        const int REPEAT_WAIT = 4;

        private float StickDeadZone;

        private GamePadState GamePad;
        private KeyboardState KeyState;

        private int[] InputsHeldTime;
        private bool[] InputsReleased;

        private HashSet<int> LockedRepeats = new HashSet<int>();

        internal InputState()
        {
            int inputCount = Enum_Values.GetEnumValues(typeof(Inputs)).Length;
            InputsHeldTime = new int[inputCount];
            InputsReleased = new bool[inputCount];
            GamePad = new GamePadState();
        }

        internal InputState(InputConfig config,
            GamePadState padState, Maybe<KeyboardState> keyState,
            InputState previousState, float stickDeadZone)
        {
            int inputCount = Enum_Values.GetEnumValues(typeof(Inputs)).Length;
            InputsHeldTime = new int[inputCount];
            InputsReleased = new bool[inputCount];

            GamePad = padState;
            if (keyState.IsSomething)
                KeyState = keyState;
            StickDeadZone = stickDeadZone;

            UpdateState(config, previousState);
        }

        private void UpdateState(InputConfig config, InputState previousState)
        {
            float leftStickAngle = (float)Math.Atan2(GamePad.ThumbSticks.Left.Y, GamePad.ThumbSticks.Left.X);
            if (leftStickAngle < 0)
                leftStickAngle += MathHelper.TwoPi;
            leftStickAngle *= 360 / MathHelper.TwoPi;

            // Loop through inputs
            foreach (Inputs input in Enum_Values.GetEnumValues(typeof(Inputs)))
            {
                int key = (int)input;

                bool keyPressed = (KeyState.IsKeyDown(config.KeyRedirect[input]) ||
                    GamePad.IsButtonDown(config.PadRedirect[input]));
                if (FEXNA.Input.INPUT_OVERRIDES.ContainsKey(input))
                    keyPressed |= KeyState.IsKeyDown(FEXNA.Input.INPUT_OVERRIDES[input]);

                // Left stick
                if (GamePad.ThumbSticks.Left.LengthSquared() > StickDeadZone * StickDeadZone)
                {
                    switch (key)
                    {
                        case ((int)Inputs.Right):
                            if (leftStickAngle < 67.5f || leftStickAngle > 292.5f)
                                keyPressed = true;
                            break;
                        case ((int)Inputs.Up):
                            if (leftStickAngle > 22.5f && leftStickAngle < 157.5f)
                                keyPressed = true;
                            break;
                        case ((int)Inputs.Left):
                            if (leftStickAngle > 112.5f && leftStickAngle < 247.5f)
                                keyPressed = true;
                            break;
                        case ((int)Inputs.Down):
                            if (leftStickAngle > 202.5f && leftStickAngle < 337.5f)
                                keyPressed = true;
                            break;
                    }
                }
                if (!FEXNA.Input.INVERSE_DIRECTIONS_CANCEL)
                {
                    // If pressing up and down
                    if (key == (int)Inputs.Down && Pressed(Inputs.Up))
                        keyPressed = false;
                    // If pressing left and right
                    if (key == (int)Inputs.Right && Pressed(Inputs.Left))
                        keyPressed = false;
                }
                // If not pressed, remove lock
                if (!keyPressed)
                    LockedRepeats.Remove(key);

                // Set data to state
                if (keyPressed)
                    // If pressed, set input held time to previous frame's hold time plus 1
                    InputsHeldTime[key] = previousState.InputsHeldTime[key] + 1;
                else if (previousState.Pressed(input))
                    // If not pressed and was pressed last frame, set released value
                    InputsReleased[key] = true;
            }

            if (FEXNA.Input.INVERSE_DIRECTIONS_CANCEL)
            {
                if (Pressed(Inputs.Down) && Pressed(Inputs.Up))
                {
                    InputsHeldTime[(int)Inputs.Down] = 0;
                    InputsHeldTime[(int)Inputs.Up] = 0;
                }
                if (Pressed(Inputs.Left) && Pressed(Inputs.Right))
                {
                    InputsHeldTime[(int)Inputs.Left] = 0;
                    InputsHeldTime[(int)Inputs.Right] = 0;
                }
            }
            foreach (Inputs key in FEXNA.Input.DIRECTIONS)
            {
                // If just triggered a direction and it's not locked, unlock repeats
                if (Triggered(key) && !LockedRepeats.Contains((int)key))
                {
                    ClearLockedRepeats();
                    break;
                }
            }
        }

        public void ClearLockedRepeats()
        {
            LockedRepeats.Clear();
        }

        #region Controls
        /// <summary>
        /// Gets whether an input was pressed on this tick.
        /// </summary>
        public bool Triggered(Inputs inputName)
        {
            return InputsHeldTime[(int)inputName] == 1;
        }

        /// <summary>
        /// Gets whether an input is being held down.
        /// </summary>
        public bool Pressed(Inputs inputName)
        {
            return InputsHeldTime[(int)inputName] >= 1;
        }

        /// <summary>
        /// Gets whether an input was released on this tick.
        /// </summary>
        public bool Released(Inputs inputName)
        {
            return InputsReleased[(int)inputName] && InputsHeldTime[(int)inputName] == 0;
        }

        /// <summary>
        /// Gets whether an input was triggered, or if it's been held for a moment and is repeating
        /// </summary>
        public bool Repeated(Inputs inputName)
        {
            // If not pressed, obviously return false
            if (InputsHeldTime[(int)inputName] == 0)
                return false;
            // If the input was pressed this tick
            if (Triggered(inputName))
                return true;
            // If repeating is locked
            if (LockedRepeats.Contains((int)inputName))
                return false;
            // If it was pressed INITIAL_WAIT ticks ago, or every REPEAT_WAIT ticks after that
            return InputsHeldTime[(int)inputName] - (INITIAL_WAIT + 1) >= 0 &&
                ((InputsHeldTime[(int)inputName] - (INITIAL_WAIT + 1)) % REPEAT_WAIT) == 0;
        }

        /// <summary>
        /// Gets the time an input has been held down.
        /// </summary>
        public int HeldTime(Inputs inputName)
        {
            return InputsHeldTime[(int)inputName];
        }

        /// <summary>
        /// Gets whether any buttons other than the given list are pressed.
        /// </summary>
        public bool OtherPressed(HashSet<Inputs> inputNames)
        {
            foreach (Inputs input in Enum_Values.GetEnumValues(typeof(Inputs)))
                if (!inputNames.Contains(input) && Pressed(input))
                    return true;
            return false;
        }

        /// <summary>
        /// Gets the pressed input direction as a numpad representation
        /// </summary>
        public Directions Dir8()
        {
            if (Pressed(Inputs.Down))
            {
                if (Pressed(Inputs.Left))
                    return Directions.DownLeft;
                else if (Pressed(Inputs.Right))
                    return Directions.DownRight;
                else
                    return Directions.Down;
            }
            else if (Pressed(Inputs.Up))
            {
                if (Pressed(Inputs.Left))
                    return Directions.UpLeft;
                else if (Pressed(Inputs.Right))
                    return Directions.UpRight;
                else
                    return Directions.Up;
            }
            else
            {
                if (Pressed(Inputs.Left))
                    return Directions.Left;
                else if (Pressed(Inputs.Right))
                    return Directions.Right;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets whether soft reset combination was pressed.
        /// </summary>
        public bool SoftReset()
        {
            return (Pressed(Inputs.L) && Pressed(Inputs.R) && Pressed(Inputs.Start) && Pressed(Inputs.Select));
        }

        /// <summary>
        /// Locks repeating inputs of a key.
        /// </summary>
        public void LockRepeat(Inputs key)
        {
            if (!LockedRepeats.Contains((int)key))
                LockedRepeats.Add((int)key);
        }
        #endregion
    }
}
