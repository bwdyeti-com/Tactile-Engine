using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArrayExtension;
using ListExtension;

namespace FEXNA_Library
{
    public class Battle_Frame_Data
    {
        public List<Battle_Frame_Image_Data> Lower_Frames = new List<Battle_Frame_Image_Data>();
        public List<Battle_Frame_Image_Data> Upper_Frames = new List<Battle_Frame_Image_Data>();
        public int time = 5;
        public int pan = -1;
        public int[] flash = new int[] { -1, -1 };
        public List<Sound_Data> sounds = new List<Sound_Data>();

        #region Serialization
        public void read(BinaryReader reader)
        {
            Lower_Frames.read(reader);
            Upper_Frames.read(reader);
            time = reader.ReadInt32();
            pan = reader.ReadInt32();
            flash = flash.read(reader);
            sounds.read(reader);
        }

        public void write(BinaryWriter writer)
        {
            Lower_Frames.write(writer);
            Upper_Frames.write(writer);
            writer.Write(time);
            writer.Write(pan);
            flash.write(writer);
            sounds.write(writer);
        }
        #endregion

        public Battle_Frame_Data() { }
        public Battle_Frame_Data(Battle_Frame_Data other)
        {
            Lower_Frames = new List<Battle_Frame_Image_Data>(other.Lower_Frames);
            Upper_Frames = new List<Battle_Frame_Image_Data>(other.Upper_Frames);
            time = other.time;
            pan = other.pan;
            flash = other.flash.ToArray();
            sounds = other.sounds.ToList();
        }

        public Battle_Frame_Image_Data image_data(int data_index)
        {
            if (data_index < Lower_Frames.Count)
                return Lower_Frames[data_index];
            else if (data_index - Lower_Frames.Count < Upper_Frames.Count)
                return Upper_Frames[data_index - Lower_Frames.Count];
            else throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Returns the parent index of a lower layer.
        /// If the parent chain working up from the layer is cyclical, returns -1 instead.
        /// </summary>
        /// <param name="data_index">Index of layer to test</param>
        public int lower_frame_parent_index(int data_index)
        {
            int parent_index = Lower_Frames[data_index].parent_index;
            return frame_parent_index(data_index, parent_index);
        }
        /// <summary>
        /// Returns the parent index of an upper layer.
        /// If the parent chain working up from the layer is cyclical, returns -1 instead.
        /// </summary>
        /// <param name="data_index">Index of layer to test</param>
        public int upper_frame_parent_index(int data_index)
        {
            int parent_index = Upper_Frames[data_index].parent_index;
            return frame_parent_index(data_index + Lower_Frames.Count, parent_index);
        }
        /// <summary>
        /// Returns the parent index of a layer.
        /// If the parent chain working up from the layer is cyclical, returns -1 instead.
        /// </summary>
        /// <param name="data_index">Index of layer to test</param>
        /// <param name="parent_index">Theoretical parent index as stored in the layer's data</param>
        public int frame_parent_index(int data_index, int parent_index)
        {
            if (parent_index == -1 || parent_index >= Lower_Frames.Count + Upper_Frames.Count)
                return -1;

            HashSet<int> tested_data = new HashSet<int>();
            tested_data.Add(data_index);
            data_index = parent_index;
            while (true)
            {
                if (tested_data.Contains(data_index))
                    return -1;
                tested_data.Add(data_index);
                Battle_Frame_Image_Data image_data = this.image_data(data_index);
                if (image_data.parent_index == -1)
                    return parent_index;
                data_index = image_data.parent_index;
            }
        }

        /// <summary>
        /// Fixes the parent indices of layers to account for a layer being deleted
        /// </summary>
        /// <param name="data_index">Index of the layer being deleted</param>
        public void fix_delete_parent(int data_index)
        {
            for (int i = 0; i < Lower_Frames.Count; i++)
            {
                Battle_Frame_Image_Data image_data = Lower_Frames[i];
                fix_delete_parent(data_index, ref image_data);
                Lower_Frames[i] = image_data;
            }
            for (int i = 0; i < Upper_Frames.Count; i++)
            {
                Battle_Frame_Image_Data image_data = Upper_Frames[i];
                fix_delete_parent(data_index, ref image_data);
                Upper_Frames[i] = image_data;
            }
        }
        private void fix_delete_parent(int data_index, ref Battle_Frame_Image_Data image_data)
        {
            if (image_data.parent_index == data_index)
                image_data.parent_index = this.image_data(data_index).parent_index; //-1; // Set the parent value to the parent of the removed layer
            if (image_data.parent_index >= data_index)
                image_data.parent_index--;
        }
        /// <summary>
        /// Fixes the parent indices of layers to account for two layers switching places
        /// </summary>
        /// <param name="data_index1">Index of the first layer being switched</param>
        /// <param name="data_index2">Index of the second layer being switched </param>
        public void fix_switch_position_parent(int data_index1, int data_index2)
        {
            for (int i = 0; i < Lower_Frames.Count; i++)
            {
                Battle_Frame_Image_Data image_data = Lower_Frames[i];
                fix_switch_position_parent(data_index1, data_index2, ref image_data);
                Lower_Frames[i] = image_data;
            }
            for (int i = 0; i < Upper_Frames.Count; i++)
            {
                Battle_Frame_Image_Data image_data = Upper_Frames[i];
                fix_switch_position_parent(data_index1, data_index2, ref image_data);
                Upper_Frames[i] = image_data;
            }
        }
        private void fix_switch_position_parent(int data_index1, int data_index2, ref Battle_Frame_Image_Data image_data)
        {
            if (image_data.parent_index == data_index1)
                image_data.parent_index = data_index2;
            else if (image_data.parent_index == data_index2)
                image_data.parent_index = data_index1;
        }
        /// <summary>
        /// Fixes the parent indices of layers to account for a layer being switched from upper to lower
        /// </summary>
        /// <param name="data_index">Index of the layer being switched to lower</param>
        public void fix_switch_to_lower_parent(int data_index)
        {
            for (int i = 0; i < Lower_Frames.Count; i++)
            {
                Battle_Frame_Image_Data image_data = Lower_Frames[i];
                fix_switch_to_lower_parent(data_index, ref image_data);
                Lower_Frames[i] = image_data;
            }
            for (int i = 0; i < Upper_Frames.Count; i++)
            {
                Battle_Frame_Image_Data image_data = Upper_Frames[i];
                fix_switch_to_lower_parent(data_index, ref image_data);
                Upper_Frames[i] = image_data;
            }
        }
        private void fix_switch_to_lower_parent(int data_index, ref Battle_Frame_Image_Data image_data)
        {
            if (image_data.parent_index == data_index)
                image_data.parent_index = Lower_Frames.Count;
            else if (image_data.parent_index < data_index && image_data.parent_index >= Lower_Frames.Count)
                image_data.parent_index++;
        }
        /// <summary>
        /// Fixes the parent indices of layers to account for a layer being switched from lower to upper
        /// </summary>
        /// <param name="data_index">Index of the layer being switched to upper</param>
        public void fix_switch_to_upper_parent(int data_index)
        {
            for (int i = 0; i < Lower_Frames.Count; i++)
            {
                Battle_Frame_Image_Data image_data = Lower_Frames[i];
                fix_switch_to_upper_parent(data_index, ref image_data);
                Lower_Frames[i] = image_data;
            }
            for (int i = 0; i < Upper_Frames.Count; i++)
            {
                Battle_Frame_Image_Data image_data = Upper_Frames[i];
                fix_switch_to_upper_parent(data_index, ref image_data);
                Upper_Frames[i] = image_data;
            }
        }
        private void fix_switch_to_upper_parent(int data_index, ref Battle_Frame_Image_Data image_data)
        {
            if (image_data.parent_index == data_index)
                image_data.parent_index = Upper_Frames.Count + Lower_Frames.Count - 1;
            else if (image_data.parent_index > data_index)
                image_data.parent_index--;
        }

        #region Tweening
        #endregion
    }

    public struct Sound_Data
    {
        public int Key;
        public string Value;

        public Sound_Data(int key, string value)
        {
            Key = key;
            Value = value;
        }

        #region Serialization
        public static Sound_Data read(BinaryReader reader)
        {
            return new Sound_Data(reader.ReadInt32(), reader.ReadString());
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(Key);
            writer.Write(Value);
        }
        #endregion
    }

    public struct BattleFrameRenderData
    {
        private Texture2D texture;
        private Vector2 loc;
        private Rectangle src_rect;
        private Color tint;
        private float rotation;
        private Vector2 offset;
        private Vector2 scale;
        public SpriteEffects flip { get; private set; }
        private float depth;
        public int blend_mode { get; private set; }

        public BattleFrameRenderData(
            Texture2D texture,
             Vector2 loc,
             Rectangle src_rect,
             Color tint,
             float rotation,
             Vector2 offset,
             Vector2 scale,
             SpriteEffects flip,
             float depth,
             int blend_mode) : this()
        {
            this.texture = texture;
            this.loc = loc;
            this.src_rect = src_rect;
            this.tint = tint;
            this.rotation = rotation;
            this.offset = offset;
            this.scale = scale;
            this.flip = flip;
            this.depth = depth;
            this.blend_mode = blend_mode;
        }

        public void draw(SpriteBatch sprite_batch)
        {
            draw(sprite_batch, this.texture);
        }
        public void draw(SpriteBatch sprite_batch, Texture2D texture)
        {
            //sprite_batch.Draw(texture, Vector2.Zero, new Rectangle(0, 0, 192, 192), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            sprite_batch.Draw(texture, loc, src_rect, tint, rotation, offset, scale, flip, depth);
        }
    }
}