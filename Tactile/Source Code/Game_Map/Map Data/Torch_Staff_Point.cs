using System.IO;
using Microsoft.Xna.Framework;
using TactileVector2Extension;

namespace Tactile.Map
{
    class Torch_Staff_Point : Fow_View_Object
    {
        #region Serialization
        public void write(BinaryWriter writer)
        {
            loc.write(writer);
            writer.Write(Vision);
        }

        public static Torch_Staff_Point read(BinaryReader reader)
        {
            Torch_Staff_Point result = new Torch_Staff_Point();
            result.loc = result.loc.read(reader);
            result.Vision = reader.ReadInt32();
            return result;
        }
        #endregion

        protected Torch_Staff_Point() { }
        public Torch_Staff_Point(Vector2 loc)
        {
            this.loc = loc;
            Vision = starting_bonus();
        }

        public override int vision()
        {
            return min_vision() + Vision;
        }

        internal bool decrease_vision()
        {
            Vision--;
            return vision() <= 0;
            return Vision < 0;
        }

        public float remaining()
        {
            return vision() / (float)initial_vision();
        }

        private static int min_vision()
        {
            return 0;
            return Global.game_map.vision_range;
        }

        public static int starting_bonus()
        {
            return Config.FLARE_VISION;
        }

        public static int initial_vision()
        {
            return min_vision() + starting_bonus();
        }
    }
}
