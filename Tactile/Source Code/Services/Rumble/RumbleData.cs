using System;
using Microsoft.Xna.Framework;

namespace Tactile.Services.Rumble
{
    class RumbleData
    {
        private int ElapsedTicks = -1;
        private TimeSpan Time;
        internal Microsoft.Xna.Framework.PlayerIndex Player { get; private set; }
        private float _LeftMotor;
        private float _RightMotor;
        private float Multiplier;

        internal float LeftMotor { get { return MathHelper.Clamp(_LeftMotor * (1 + (Multiplier - 1) * progress), 0, 1); } }
        internal float RightMotor { get { return MathHelper.Clamp(_RightMotor * (1 + (Multiplier - 1) * progress), 0, 1); } }
        private float progress { get { return (float)((ElapsedTicks / (float)Config.FRAME_RATE) / Time.TotalSeconds); } }

        public RumbleData(TimeSpan time, Microsoft.Xna.Framework.PlayerIndex player, float left_motor, float right_motor, float mult)
        {
            Time = time;
            Player = player;
            _LeftMotor = left_motor;
            _RightMotor = right_motor;
            Multiplier = mult;
        }

        public bool finished { get { return ElapsedTicks / (float)Config.FRAME_RATE >= Time.TotalSeconds; } }

        internal void update()
        {
            ElapsedTicks++;
        }
    }
}
