using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FEXNAVersionExtension;
using FEXNA_Library;

namespace FEXNA.Services.Input
{
    class InputConfig
    {
        public Dictionary<Inputs, Buttons> PadRedirect;

        public InputConfig()
        {
            PadRedirect = new Dictionary<Inputs, Buttons>();

            SetDefaults();
        }

        #region Config
        public Dictionary<Inputs, Keys> KeyRedirect
        {
            get
            {
                Dictionary<Inputs, Keys> keys = new Dictionary<Inputs, Keys>();
                for (int i = 0; i < Global.gameSettings.Controls.KeyboardConfig.Length; i++)
                {
                    keys.Add((Inputs)i, Global.gameSettings.Controls.KeyboardConfig[i]);
                }
                return keys;
            }
        }

        public void SetDefaults()
        {
            PadRedirect.Clear();

            // Down
            PadRedirect.Add(Inputs.Down, Buttons.DPadDown);
            // Left
            PadRedirect.Add(Inputs.Left, Buttons.DPadLeft);
            // Right
            PadRedirect.Add(Inputs.Right, Buttons.DPadRight);
            // Up
            PadRedirect.Add(Inputs.Up, Buttons.DPadUp);
            // A
            PadRedirect.Add(Inputs.A, Buttons.B);
            // B
#if __ANDROID__
            PadRedirect.Add(Inputs.B, Buttons.Back);
#else
            PadRedirect.Add(Inputs.B, Buttons.A);
#endif
            // Y
            PadRedirect.Add(Inputs.Y, Buttons.X);
            // X
            PadRedirect.Add(Inputs.X, Buttons.Y);
            // L
            PadRedirect.Add(Inputs.L, Buttons.LeftShoulder);
            // R
            PadRedirect.Add(Inputs.R, Buttons.RightShoulder);
            /*// L2 //@Debug
            PadRedirect.Add(Inputs.L2, Buttons.LeftTrigger);
            // R2
            PadRedirect.Add(Inputs.R2, Buttons.RightTrigger);
            // L3
            PadRedirect.Add(Inputs.L3, Buttons.LeftStick);
            // R3
            PadRedirect.Add(Inputs.R3, Buttons.RightStick);*/
            // Start
            PadRedirect.Add(Inputs.Start, Buttons.Start);
            // Select
#if __ANDROID__
            PadRedirect.Add(Inputs.Select, Buttons.A);
#else
            PadRedirect.Add(Inputs.Select, Buttons.Back);
#endif
        }

        public string KeyName(Inputs inputName)
        {
            return KeyName(KeyRedirect[inputName]);
        }
        public string KeyName(Keys key)
        {
            if (FEXNA.Input.REMAPPABLE_KEYS.ContainsKey(key))
                return FEXNA.Input.REMAPPABLE_KEYS[key];

            return "";
        }
        #endregion

        public InputState[] Update(InputState[] inputs)
        {
            InputState[] result = new InputState[inputs.Length];

            // Current keyboard/controller state
            KeyboardState keyState = Keyboard.GetState();

            for (PlayerIndex i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                if ((int)i >= result.Length)
                    break;

                GamePadState controllerState = GamePad.GetState(i, GamePadDeadZone.None);
                if (i == PlayerIndex.One)
                    result[(int)i] = new InputState(
                        this,
                        controllerState, keyState,
                        inputs[(int)i]);
                else
                    result[(int)i] = new InputState(
                        this,
                        controllerState, default(Maybe<KeyboardState>),
                        inputs[(int)i]);
            }

            MouseState mouseState = Mouse.GetState();

            return result;
        }
    }
}
