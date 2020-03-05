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

        public Dictionary<Inputs, Keys> KeyRedirect;
        public Dictionary<Inputs, Buttons> PadRedirect;

        public InputConfig()
        {
            KeyRedirect = new Dictionary<Inputs, Keys>();
            PadRedirect = new Dictionary<Inputs, Buttons>();

            SetDefaults();
        }

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(KeyRedirect.Count);

            foreach (var pair in KeyRedirect)
            {
                writer.Write((byte)pair.Key);
                writer.Write((int)pair.Value);
            }
        }

        public void read(BinaryReader reader)
        {
            SetDefaults();

            if (Global.LOADED_VERSION.older_than(0, 6, 5, 0))
            {
                var keyRedirect = new Dictionary<byte, Keys>();
                for (byte i = 0; i < (byte)Inputs.Select; i++)
                    keyRedirect[i] = (Keys)reader.ReadInt32();
                foreach (var pair in keyRedirect)
                {
                    if (FEXNA.Input.REMAPPABLE_KEYS.ContainsKey(pair.Value))
                        KeyRedirect[(Inputs)pair.Key] = pair.Value;
                }
            }
            else
            {
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    Inputs key = (Inputs)reader.ReadByte();
                    Keys value = (Keys)reader.ReadInt32();
                    if (FEXNA.Input.REMAPPABLE_KEYS.ContainsKey(value))
                        KeyRedirect[key] = value;
                }
            }
        }
        #endregion

        #region Config
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

            DefaultKeys();
        }

        public void DefaultKeys()
        {
            KeyRedirect.Clear();

            KeyRedirect.Add(Inputs.Down, Keys.NumPad2);
            KeyRedirect.Add(Inputs.Left, Keys.NumPad4);
            KeyRedirect.Add(Inputs.Right, Keys.NumPad6);
            KeyRedirect.Add(Inputs.Up, Keys.NumPad8);
            KeyRedirect.Add(Inputs.A, Keys.X);
            KeyRedirect.Add(Inputs.B, Keys.Z);
            KeyRedirect.Add(Inputs.Y, Keys.D);
            KeyRedirect.Add(Inputs.X, Keys.C);
            KeyRedirect.Add(Inputs.L, Keys.A);
            KeyRedirect.Add(Inputs.R, Keys.S);
            /*KeyRedirect.Add(Inputs.L2, Keys.Q); //@Debug
            KeyRedirect.Add(Inputs.R2, Keys.W);
            KeyRedirect.Add(Inputs.L3, Keys.LeftShift);
            KeyRedirect.Add(Inputs.R3, Keys.LeftControl);*/
            KeyRedirect.Add(Inputs.Start, Keys.Enter);
            KeyRedirect.Add(Inputs.Select, Keys.RightShift);
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

        public bool RemapKey(Inputs inputName, Keys key)
        {
            if (!FEXNA.Input.REMAPPABLE_KEYS.ContainsKey(key))
                return false;

            if (KeyRedirect.ContainsValue(key))
                foreach (var pair in KeyRedirect)
                    if (pair.Value == key)
                    {
                        KeyRedirect[pair.Key] = KeyRedirect[inputName];
                        break;
                    }
            KeyRedirect[inputName] = key;
            return true;
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
                        inputs[(int)i], FEXNA.Input.STICK_DEAD_ZONE);
                else
                    result[(int)i] = new InputState(
                        this,
                        controllerState, default(Maybe<KeyboardState>),
                        inputs[(int)i], FEXNA.Input.STICK_DEAD_ZONE);
            }

            MouseState mouseState = Mouse.GetState();

            return result;
        }
    }
}
