using System;
using Microsoft.Xna.Framework;

namespace FEXNA.Services.Rumble
{
    public abstract class BaseRumbleService : GameComponent
    {
        public BaseRumbleService(Game game) : base(game) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">Time the rumble will last.</param>
        /// <param name="left_motor">Initial left motor force. The left motor is low frequency, with stronger apparent force.</param>
        /// <param name="right_motor">Initial right motor force. The right motor is high frequency, with weaker apparent force.</param>
        /// <param name="player">PlayerIndex of the controller to vibrate.</param>
        /// <param name="mult">
        /// Rumble force is multiplied by a factor lerped between 1 and this value over the course of the rumble.
        /// Less than 1 causes the force to weaken over time, greater than 1 causes it to increase.
        /// </param>
        public abstract void add_rumble(TimeSpan time, float left_motor, float right_motor, PlayerIndex player = PlayerIndex.One, float mult = 1);
    }
}
