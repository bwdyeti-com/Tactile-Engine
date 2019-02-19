using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
                writer.Write((int)pair.Key);
                writer.Write((int)pair.Value);
            }
        }

        public void read(BinaryReader reader)
        {
            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                Inputs key = (Inputs)reader.ReadInt32();
                Keys value = (Keys)reader.ReadInt32();
                KeyRedirect[key] = value;
            }
        }
        #endregion

        #region Config
        public void SetDefaults()
        {
            // Down
            PadRedirect.Add(Inputs.Down, Buttons.DPadDown);
            // Left
            PadRedirect.Add(Inputs.Left, Buttons.DPadLeft);
            // Right
            PadRedirect.Add(Inputs.Right, Buttons.DPadRight);
            // Up
            PadRedirect.Add(Inputs.Up, Buttons.DPadUp);
            // A
            PadRedirect.Add(Inputs.A, Buttons.A);
            // B
            PadRedirect.Add(Inputs.B, Buttons.B);
            // Y
            PadRedirect.Add(Inputs.Y, Buttons.Y);
            // X
            PadRedirect.Add(Inputs.X, Buttons.X);
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
            PadRedirect.Add(Inputs.Select, Buttons.Back);

            DefaultKeys();
        }

        public void DefaultKeys()
        {
            KeyRedirect.Clear();

            KeyRedirect.Add(Inputs.Down, Keys.S);
            KeyRedirect.Add(Inputs.Left, Keys.A);
            KeyRedirect.Add(Inputs.Right, Keys.D);
            KeyRedirect.Add(Inputs.Up, Keys.W);
            KeyRedirect.Add(Inputs.A, Keys.Space);
            KeyRedirect.Add(Inputs.B, Keys.Q);
            KeyRedirect.Add(Inputs.Y, Keys.F);
            KeyRedirect.Add(Inputs.X, Keys.E);
            KeyRedirect.Add(Inputs.L, Keys.Z);
            KeyRedirect.Add(Inputs.R, Keys.X);
            /*KeyRedirect.Add(Inputs.L2, Keys.C); //@Debug
            KeyRedirect.Add(Inputs.R2, Keys.V);
            KeyRedirect.Add(Inputs.L3, Keys.LeftShift);
            KeyRedirect.Add(Inputs.R3, Keys.LeftControl);*/
            KeyRedirect.Add(Inputs.Start, Keys.Enter);
            KeyRedirect.Add(Inputs.Select, Keys.RightShift);
        }

        public string KeyName(Inputs inputName)
        {
            return FEXNA.Input.REMAPPABLE_KEYS[KeyRedirect[inputName]];
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
