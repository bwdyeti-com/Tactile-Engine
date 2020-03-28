using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ListExtension;

namespace FEXNA_Library
{
    public enum Frame_Tween_Types { X_Pos, Y_Pos, X_Scale, Y_Scale, Rotation, Opacity, Frame_Index }
    public enum Frame_Tween_Functions { Linear, Sinusoidal, Modulo }
    public enum Frame_Tween_Intervals { Tick, Frame }
    public class Battle_Animation_Data
    {
        public int id = 0;
        public string name = "";
        public string filename = "";
        public List<Battle_Frame_Data> Frames = new List<Battle_Frame_Data>();
        public bool loop = false;
#if true//PANCOMPATIBILITY
        [ContentSerializer(Optional = true)]
#endif
        public List<Battle_Animation_Tween_Data> Tween_Data = new List<Battle_Animation_Tween_Data>();

#if true//PANCOMPATIBILITY
        [ContentSerializer(Optional = true, ElementName = "pan")]
        //[ContentSerializerIgnore]
        public int? Pan = null;

        [ContentSerializerIgnore]
        public int pan {
            get { return (int)Pan; }
            set { Pan = value; }
        }
        [ContentSerializerIgnore]
        public List<Battle_Animtion_Modifier> modifiers;
#else
        [ContentSerializerIgnore]
        public int pan;
#endif
        //[ContentSerializerIgnore]
        //public Dictionary<int, Dictionary<int, List<List<float>>>> Processed_Tween_Data =
        //    new Dictionary<int, Dictionary<int, List<List<float>>>>();

        #region Accessors
        public int frame_count { get { return Frames.Count; } }

        public int duration
        {
            get
            {
                int result = 0;
                foreach (Battle_Frame_Data data in Frames)
                    result += data.time;
                return result;
            }
        }

        public bool has_tween_data { get { return true; } }
        #endregion

        #region Serialization
        public void read(BinaryReader reader)
        {
            // Id
            id = reader.ReadInt32();
            // Name
            name = reader.ReadString();
            // Filename
            filename = reader.ReadString();
            // Frames
            Frames.read(reader);
            // Loop
            loop = reader.ReadBoolean();
            // Tween Data
            Tween_Data.read(reader);

            setup_pan();
            setup_tweening();
        }

        public void write(BinaryWriter writer)
        {
            // Id
            writer.Write(id);
            // Name
            writer.Write(name);
            // Filename
            writer.Write(filename);
            // Frames
            Frames.write(writer);
            // Loop
            writer.Write(loop);
            // Tween Data
            Tween_Data.write(writer);
        }
        #endregion

        public override string ToString()
        {
            return String.Format("Battle Animation Data: {0}; Anim Filename: {1}", name, filename);
        }

        public Battle_Animation_Data() { }
        public Battle_Animation_Data(Battle_Animation_Data other)
        {
            id = other.id;
            name = other.name;
            filename = other.filename;
            Frames = other.Frames
                .Select(x => new Battle_Frame_Data(x))
                .ToList();
            loop = other.loop;
            Tween_Data = other.Tween_Data.ToList();

            setup_pan();
            setup_tweening();
        }

        public Battle_Frame_Data current_frame(int index)
        {
            if (Frames.Count == 0)
                return null;
            return Frames[index];
        }

        public int image_index(int frame_id, int tick, int data_index)
        {
            Battle_Frame_Image_Data frame = Frames[frame_id].image_data(data_index);
            int index = frame.frame_id + (int)layer_offset(Frame_Tween_Types.Frame_Index, frame_id, tick, data_index);
            return index;
        }
        public Vector2 image_location(int frame_id, int tick, int data_index)
        {
            Battle_Frame_Image_Data frame = Frames[frame_id].image_data(data_index);
            Vector2 loc = Vector2.Zero;
            Vector2 temp_loc = frame.loc + new Vector2(layer_offset(Frame_Tween_Types.X_Pos, frame_id, tick, data_index),
                layer_offset(Frame_Tween_Types.Y_Pos, frame_id, tick, data_index));

            int j = Frames[frame_id].frame_parent_index(data_index, frame.parent_index);
            while (j != -1)
            {
                Battle_Frame_Image_Data parent_frame = Frames[frame_id].image_data(j);
                float rotation = parent_frame.rotation + layer_offset(Frame_Tween_Types.Rotation, frame_id, tick, j);
                if (modifiers != null)
                {
                    foreach (var modifier in modifiers.Where(x => x.layer_id == j && x.type == Frame_Tween_Types.Rotation))
                        rotation += modifier.magnitude;
                }

                Matrix m1 = Matrix.Identity * Matrix.CreateScale(parent_frame.scale.X, parent_frame.scale.Y, 1f) *
                    Matrix.CreateRotationZ(rotation / 180 * MathHelper.Pi);
                Matrix m2 = Matrix.Identity * Matrix.CreateRotationZ(rotation / 180 * MathHelper.Pi);

                loc = Vector2.Transform(loc, m2) + Vector2.Transform(temp_loc, m1);
                temp_loc = parent_frame.loc + new Vector2(layer_offset(Frame_Tween_Types.X_Pos, frame_id, tick, j),
                    layer_offset(Frame_Tween_Types.Y_Pos, frame_id, tick, j));
                j = Frames[frame_id].frame_parent_index(j, parent_frame.parent_index);
            }
            loc += temp_loc;
            return loc;
        }
        public Vector2 image_scale(int frame_id, int tick, int data_index)
        {
            Battle_Frame_Image_Data frame = Frames[frame_id].image_data(data_index);
            Vector2 scale = frame.scale + (new Vector2(layer_offset(Frame_Tween_Types.X_Scale, frame_id, tick, data_index),
                layer_offset(Frame_Tween_Types.Y_Scale, frame_id, tick, data_index)) / 100f);
            return scale;
        }
        public float image_rotation(int frame_id, int tick, int data_index)
        {
            Battle_Frame_Image_Data frame = Frames[frame_id].image_data(data_index);
            float rotation = frame.rotation + layer_offset(Frame_Tween_Types.Rotation, frame_id, tick, data_index);
            if (modifiers != null)
            {
                foreach (var modifier in modifiers.Where(x => x.layer_id == data_index && x.type == Frame_Tween_Types.Rotation))
                    rotation += modifier.magnitude;
            }

            int j = Frames[frame_id].frame_parent_index(data_index, frame.parent_index);
            while (j != -1)
            {
                Battle_Frame_Image_Data parent_frame = Frames[frame_id].image_data(j);
                rotation += parent_frame.rotation + layer_offset(Frame_Tween_Types.Rotation, frame_id, tick, j);
                if (modifiers != null)
                {
                    foreach (var modifier in modifiers.Where(x => x.layer_id == j && x.type == Frame_Tween_Types.Rotation))
                        rotation += modifier.magnitude;
                }
                j = Frames[frame_id].frame_parent_index(j, parent_frame.parent_index);
            }
            return rotation;
        }
        public int image_opacity(int frame_id, int tick, int data_index)
        {
            Battle_Frame_Image_Data frame = Frames[frame_id].image_data(data_index);
            int opacity = frame.opacity + (int)layer_offset(Frame_Tween_Types.Opacity, frame_id, tick, data_index);
            return opacity;
        }

        private float layer_offset(Frame_Tween_Types tween_type, int frame_id, int tick, int data_index)
        {
            /*if (Processed_Tween_Data.Count == 0)
                setup_tweening();
            if (!Processed_Tween_Data.ContainsKey((int)tween_type) || !Processed_Tween_Data[(int)tween_type].ContainsKey(data_index) ||
                    Processed_Tween_Data[(int)tween_type][data_index].Count == 0)
                return 0;
            //if (Processed_Tween_Data[(int)tween_type][data_index].Keys.Min() < frame_id ||
            //        Processed_Tween_Data[(int)tween_type][data_index][frame_id].Keys.Min() < tick)
            //    return 0;
            return Processed_Tween_Data[(int)tween_type][data_index][frame_id][tick];
            return 0;*/

            float result = 0;
            foreach (Battle_Animation_Tween_Data data in Tween_Data)
            {
                if (data.layer == data_index && tween_type == data.data && frame_id >= data.start_frame &&
                    data.start_frame < data.end_frame && data.interval > 0)
                {
                    int frame = data.start_frame;
                    int timer = 0;

                    int steps = 0;
                    if (frame < 0)
                    {
                        frame = 0;
                        steps = 1;
                    }
                    while (true)
                    {
                        if (data.interval_type == Frame_Tween_Intervals.Tick)
                            timer += data.interval;
                        else
                            frame += data.interval;
                        while (frame < Frames.Count && timer >= Frames[frame].time)
                        {
                            timer -= Frames[frame].time;
                            frame++;
                        }
                        // If frame has gone past the end of the animation, or frame has gone past the end of the data, or frame has gone past the checked frame
                        if (frame >= Frames.Count || (frame == data.end_frame && timer > 0) || frame > data.end_frame || (frame == frame_id && timer > tick) || frame > frame_id)
                            break;
                        steps++;
                    }
                    switch (data.function)
                    {
                        case Frame_Tween_Functions.Linear:
                            result += steps * data.magnitude;
                            break;
                        case Frame_Tween_Functions.Sinusoidal:
                            result += (float)(data.magnitude / 2 * Math.Sin((steps + data.offset) * Math.PI * 2 / data.period) -
                                data.magnitude / 2 * Math.Sin((data.offset) * Math.PI * 2 / data.period));
                            break;
                        case Frame_Tween_Functions.Modulo:
                            result += data.magnitude * ((steps + data.offset) % data.period);
                            break;
                    }
                }
            }

            return result;
        }

        public int next_frame(int index)
        {
            // If the frame list is empty just return now
            if (Frames.Count <= 0)
                return index;
            int start_frame = index;
            index = (index + 1) % frame_count;
            while (Frames[index].time == 0)
            {
                // If looped back to the original frame, to prevent infinite loops
                if (start_frame == index)
                    return start_frame;
                index = (index + 1) % frame_count;
                if (start_frame == -1 && index == 0)
                    return 0;
            }
            return index;
        }

        private void setup_pan()
        {
            pan = 0;
            bool pan_test = false;
            for (int i = 0; i < Frames.Count; i++)
            {
                if (Frames[i].pan == -1)
                    pan += Frames[i].time;
                else
                {
                    pan += Frames[i].pan;
                    pan_test = true;
                    break;
                }
            }
            if (!pan_test)
                pan = 1;
        }
        public void setup_tweening()
        {
            /*Processed_Tween_Data.Clear();
            for(int i = 0; i < Enum_Values.GetEnumCount(typeof(Frame_Tween_Types)); i++)
            {
                Processed_Tween_Data.Add(i, new Dictionary<int,List<List<float>>>());
                setup_tweening(i);
            }*/
        }
        /*public void setup_tweening(int i)
        {
            Processed_Tween_Data[i].Clear();
            // Adds a dictionary for each layer that uses this type
            foreach (Battle_Animation_Tween_Data data in Tween_Data)
                if (data.data == (Frame_Tween_Types)i)
                    Processed_Tween_Data[i][data.layer] = new List<List<float>>();
            List<int> layers = new List<int>(Processed_Tween_Data[i].Keys);
            foreach (int layer in layers)
            setup_tweening(i, layer);
        }
        public void setup_tweening(int i, int layer)
        {
            Processed_Tween_Data[i][layer].Clear();
            for (int frame_id = 0; frame_id < Frames.Count; frame_id++)
            {
                Processed_Tween_Data[i][layer].Add(new List<float>());
                for (int tick = 0; tick < Frames[frame_id].time; tick++)
                {
                    Processed_Tween_Data[i][layer][frame_id].Add(tween_setup_layer_offset((Frame_Tween_Types)i, frame_id, tick, layer));
                }
            }
        }

        private float tween_setup_layer_offset(Frame_Tween_Types tween_type, int frame_id, int tick, int data_index)
        {
            float result = 0;
            foreach (Battle_Animation_Tween_Data data in Tween_Data)
            {
                if (data.layer == data_index && tween_type == data.data && frame_id >= data.start_frame &&
                    data.start_frame < data.end_frame && data.interval > 0)
                {
                    int frame = data.start_frame;
                    int timer = 0;

                    int steps = 0;
                    if (frame < 0)
                    {
                        frame = 0;
                        steps = 1;
                    }
                    while (true)
                    {
                        if (data.interval_type == Frame_Tween_Intervals.Tick)
                            timer += data.interval;
                        else
                            frame += data.interval;
                        while (frame < Frames.Count && timer >= Frames[frame].time)
                        {
                            timer -= Frames[frame].time;
                            frame++;
                        }
                        // If frame has gone past the end of the animation, or frame has gone past the end of the data, or frame has gone past the checked frame
                        if (frame >= Frames.Count || (frame == data.end_frame && timer > 0) || frame > data.end_frame || (frame == frame_id && timer > tick) || frame > frame_id)
                            break;
                        steps++;
                    }
                    switch (data.function)
                    {
                        case Frame_Tween_Functions.Linear:
                            result += steps * data.magnitude;
                            break;
                        case Frame_Tween_Functions.Sinusoidal:
                            result += (float)(data.magnitude / 2 * Math.Sin((steps + data.offset) * Math.PI * 2 / data.period) -
                                data.magnitude / 2 * Math.Sin((data.offset) * Math.PI * 2 / data.period));
                            break;
                        case Frame_Tween_Functions.Modulo:
                            result += data.magnitude * ((steps + data.offset) % data.period);
                            break;
                    }
                }
            }

            return result;
        }*/

        public bool flash_visible(int frame_id, int tick)
        {
            for (int frame = frame_id; frame >= 0; frame--)
                if (Frames[frame].flash[0] >= 0 && Frames[frame].flash[0] < Frames[frame].time &&
                    !(frame_id == frame && tick < Frames[frame].flash[0]))
                {
                    int frame2 = frame;
                    int timer = Frames[frame].flash[0];
                    int flash_time = 0;
                    while(true)
                    {
                        flash_time++;
                        if (flash_time > Frames[frame].flash[1])
                            break;
                        while (frame2 < Frames.Count && timer >= Frames[frame2].time)
                        {
                            timer -= Frames[frame2].time;
                            frame2++;
                        }
                        if (frame2 >= frame_id && timer >= tick)
                            return true;
                        if (frame2 >= Frames.Count)
                            break;
                        timer++;
                    }
                }
            return false;
        }
    }

    public struct Battle_Animation_Tween_Data
    {
        public int layer;
        public Frame_Tween_Types data;
        public Frame_Tween_Functions function;
        public int start_frame;
        public int end_frame;
        public Frame_Tween_Intervals interval_type;
        public int interval;
        public float magnitude;
        public int period;
        public int offset;

        #region Serialization
        public static Battle_Animation_Tween_Data read(BinaryReader reader)
        {
            Battle_Animation_Tween_Data result = new Battle_Animation_Tween_Data();
            result.layer = reader.ReadInt32();
            result.data = (Frame_Tween_Types)reader.ReadInt32();
            result.function = (Frame_Tween_Functions)reader.ReadInt32();
            result.start_frame = reader.ReadInt32();
            result.end_frame = reader.ReadInt32();
            result.interval_type = (Frame_Tween_Intervals)reader.ReadInt32();
            result.interval = reader.ReadInt32();
            result.magnitude = (float)reader.ReadDouble();
            result.period = reader.ReadInt32();
            result.offset = reader.ReadInt32();
            return result;
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(layer);
            writer.Write((int)data);
            writer.Write((int)function);
            writer.Write(start_frame);
            writer.Write(end_frame);
            writer.Write((int)interval_type);
            writer.Write(interval);
            writer.Write((double)magnitude);
            writer.Write(period);
            writer.Write(offset);
        }
        #endregion
    }

    public struct Battle_Animtion_Modifier
    {
        public Frame_Tween_Types type;
        public int layer_id;
        public float magnitude;
    }
}