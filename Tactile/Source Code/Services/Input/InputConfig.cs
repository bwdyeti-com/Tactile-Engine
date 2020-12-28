//@Yeti: Is this class still needed anymore?
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TactileVersionExtension;
using TactileLibrary;

namespace Tactile.Services.Input
{
    class InputConfig
    {
        public InputConfig() { }

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

        public Dictionary<Inputs, Buttons> PadRedirect
        {
            get
            {
                Dictionary<Inputs, Buttons> keys = new Dictionary<Inputs, Buttons>();
                for (int i = 0; i < Global.gameSettings.Controls.GamepadConfig.Length; i++)
                {
                    keys.Add((Inputs)i, Global.gameSettings.Controls.GamepadConfig[i]);
                }
                return keys;
            }
        }

        public string KeyName(Inputs inputName)
        {
            return KeyName(KeyRedirect[inputName]);
        }
        public string KeyName(Keys key)
        {
            if (Tactile.Input.REMAPPABLE_KEYS.ContainsKey(key))
                return Tactile.Input.REMAPPABLE_KEYS[key];

            return "";
        }
        #endregion

        public InputState[] Update(
            InputState[] inputs,
            KeyboardState keyState,
            GamePadState gamePadState)
        {
            InputState[] result = new InputState[inputs.Length];

            // Current keyboard/controller state
            for (PlayerIndex i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                if ((int)i >= result.Length)
                    break;

                GamePadState controllerState;
                if (i == PlayerIndex.One)
                    controllerState = gamePadState;
                else
                    controllerState = GamePad.GetState(i, GamePadDeadZone.None);

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
