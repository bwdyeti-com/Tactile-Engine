using System.IO;
using Microsoft.Xna.Framework;
using Vector2Extension;

namespace Tactile.Map
{
    class Fow_View_Object
    {
        public Vector2 loc;
        protected int Vision;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            loc.write(writer);
            writer.Write(Vision);
        }

        public static Fow_View_Object read(BinaryReader reader)
        {
            Fow_View_Object result = new Fow_View_Object();
            result.loc = result.loc.read(reader);
            result.Vision = reader.ReadInt32();
            return result;
        }
        #endregion

        protected Fow_View_Object() { }
        public Fow_View_Object(Vector2 loc, int vision)
        {
            this.loc = loc;
            Vision = vision;
        }
        internal Fow_View_Object(Game_Unit unit) : this(unit.loc, unit.vision()) { }

        public virtual int vision()
        {
            return Vision;
        }
    }
}
