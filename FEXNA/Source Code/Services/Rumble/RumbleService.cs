using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FEXNA.Services.Rumble
{
    class RumbleService : BaseRumbleService
    {
        private static Enum[] PlayerEnums;

        private HashSet<RumbleData> Rumbles = new HashSet<RumbleData>();

        static RumbleService()
        {
            PlayerEnums = Enum_Values.GetEnumValues(typeof(PlayerIndex));
        }

        public RumbleService(Game game) : base(game) { }

        public override void add_rumble(TimeSpan time, float left_motor, float right_motor, PlayerIndex player, float mult)
        {
            if (Global.gameSettings.Controls.Rumble)
                Rumbles.Add(new RumbleData(time, player, left_motor, right_motor, mult));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // If the rumble setting is turned off
            if (!Global.gameSettings.Controls.Rumble)
            {
                Rumbles.Clear();
                return;
            }

            foreach (var rumble in Rumbles)
                rumble.update();
            Rumbles.RemoveWhere(x => x.finished);
            foreach (PlayerIndex p in PlayerEnums)
            {
                if (FEXNA.Input.Controller_Active && Rumbles.Any(x => x.Player == p))
                {
                    var rumbles = Rumbles.Where(x => x.Player == p);
                    float left = 1 - rumbles.Select(x => x.LeftMotor)
                        .Aggregate(1f, (a, b) => a * (1 - b));
                    float right = 1 - rumbles.Select(x => x.RightMotor)
                        .Aggregate(1f, (a, b) => a * (1 - b));
                    GamePad.SetVibration(p, left, right);
                    // I don't remember what this was here for //Yeti
                    // I assume as a reminder to add an option to disable rumble
                    // but I already did that so...
//#if !DEBUG
//                    throw new Exception();
//#endif
                }
                else
                    GamePad.SetVibration(p, 0, 0);
            }
        }
    }
}
