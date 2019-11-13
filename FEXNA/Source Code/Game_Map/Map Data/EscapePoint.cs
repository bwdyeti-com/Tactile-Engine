using System.IO;
using Microsoft.Xna.Framework;
using FEXNAVector2Extension;
using FEXNAVersionExtension;

namespace FEXNA.Map
{
    struct EscapePoint
    {
        internal Vector2 Loc { get; private set; }
        internal Vector2 EscapeToLoc { get; private set; }
        internal int Team { get; private set; }
        internal int Group { get; private set; }
        internal string EventName { get; private set; }
        internal bool LordOnly { get; private set; }

        #region Serialization
        public void write(BinaryWriter writer)
        {
            Loc.write(writer);
            EscapeToLoc.write(writer);
            writer.Write(Team);
            writer.Write(Group);
            writer.Write(EventName);
            writer.Write(LordOnly);
        }

        public static EscapePoint read(BinaryReader reader)
        {
            EscapePoint result = new EscapePoint();

            result.Loc = result.Loc.read(reader);
            result.EscapeToLoc = result.EscapeToLoc.read(reader);
            result.Team = reader.ReadInt32();
            result.Group = reader.ReadInt32();
            result.EventName = reader.ReadString();
            if (!Global.LOADED_VERSION.older_than(0, 6, 7, 2)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                result.LordOnly = reader.ReadBoolean();
            }

            return result;
        }
        #endregion

        internal EscapePoint(Vector2 loc, Vector2 escapeToLoc, int team, int group, string eventName = "", bool lordOnly = false) : this()
        {
            Loc = loc;
            EscapeToLoc = escapeToLoc;
            Team = team;
            Group = group;
            EventName = eventName;
            LordOnly = lordOnly;
        }
    }
}
