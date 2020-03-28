using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Vector2Extension;

namespace FEXNA_Library
{
    public struct Battle_Frame_Image_Data
    {
#if true//PANCOMPATIBILITY
        [ContentSerializer(Optional = true, ElementName = "parent_index")]
        public int? Parent_Index;

        [ContentSerializerIgnore]
        public int parent_index
        {
            get
            {
                if (Parent_Index == null)
                    Parent_Index = -1;
                return (int)Parent_Index;
            }
            set { Parent_Index = value; }
        }
#else
        public int parent_index;
#endif
        public int frame_id;
        public Vector2 loc;
        public Vector2 scale;
#if true//PANCOMPATIBILITY
        [ContentSerializer(Optional = true, ElementName = "rotation")]
        public float? Rotation;

        [ContentSerializerIgnore]
        public float rotation
        {
            get
            {
                if (Rotation == null)
                    Rotation = 0;
                return (float)Rotation;
            }
            set { Rotation = value; }
        }
#else
        public float rotation;
#endif
        public bool flipped;
        public int opacity;
        public int blend_mode;

        #region Serialization
        public static Battle_Frame_Image_Data read(BinaryReader reader)
        {
            Battle_Frame_Image_Data image_data = new Battle_Frame_Image_Data();

            image_data.parent_index = reader.ReadInt32();
            image_data.frame_id = reader.ReadInt32();
            image_data.loc = new Vector2().read(reader);
            image_data.scale = new Vector2().read(reader);
            image_data.rotation = (float)reader.ReadDouble();
            image_data.flipped = reader.ReadBoolean();
            image_data.opacity = reader.ReadInt32();
            image_data.blend_mode = reader.ReadInt32();

            return image_data;
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(parent_index);
            writer.Write(frame_id);
            loc.write(writer);
            scale.write(writer);
            writer.Write((double)rotation);
            writer.Write(flipped);
            writer.Write(opacity);
            writer.Write(blend_mode);
        }
        #endregion
    }
}