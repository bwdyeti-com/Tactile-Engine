using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Graphics.Map
{
    class Map_Effect : Matrix_Position_Sprite
    {
        protected Texture2D Texture;
        protected int Type = 0;
        protected int Id = 1;
        protected int Image_Index;
        protected int Frame = 0, Frame_Time = 0;
        protected int Timer = 0;
        protected bool Finished = false;
        protected List<Map_Effect> Sub_Effects = new List<Map_Effect>();

        #region Accessors
        new public Texture2D texture
        {
            get { return Texture; }
            set
            {
                Texture = value;
                offset = new Vector2(Texture.Width / 10);
                process_frame(Frame);
            }
        }

        public bool finished { get { return Finished && Sub_Effects.Count == 0; } }

        protected Map_Effect_Data data
        {
            get
            {
                return Map_Animations.effect_data(ref Type, ref Id);
            }
        }

        public string filename { get { return data.image.Key; } }

        protected int frame_time_max { get { return data.animation_data[Frame].Value; } }

        public bool hit
        {
            get
            {
#if DEBUG
                if (Frame == data.animation_data.Count - 1 && Frame_Time == data.animation_data[Frame].Value)
                {
                    int test = 0;
                }
#endif
                return Timer >= data.image.Value || (Frame == data.animation_data.Count - 1 && Frame_Time == data.animation_data[Frame].Value);
            }
        }
        #endregion

        public Map_Effect() { }
        public Map_Effect(int type, int id)
        {
            Type = type;
            Id = id;
        }

        protected void process_frame(int frame)
        {
            Map_Effect_Data data = this.data;
            Image_Index = data.animation_data[frame].Key[0];
            byte source = (byte)data.animation_data[frame].Key[1];
            byte dest = (byte)data.animation_data[frame].Key[2];
            tint = new Color(source, source, source, 255 - dest);
        }

        protected void process_timing(int timer)
        {
            process_timing(timer, data.processing_data, Sub_Effects, loc);
        }
        public static void process_timing(int timer, List<KeyValuePair<int, string[]>> processing_data,
            List<Map_Effect> sub_effects, Vector2 loc)
        {
            foreach (KeyValuePair<int, string[]> processing in processing_data)
                if (timer == processing.Key)
                    switch (processing.Value[0])
                    {
                        // Brighten
                        case "b":
                            if (Global.scene.is_map_scene)
                                ((Scene_Map)Global.scene).spell_brighten(true);
                            break;
                        // Darken
                        case "d":
                            if (Global.scene.is_map_scene)
                                ((Scene_Map)Global.scene).spell_brighten(false);
                            break;
                        // Torch Flash
                        case "torch":
                            if (sub_effects != null)
                            {
                                sub_effects.Add(new Torch_Flash());
                                sub_effects[sub_effects.Count - 1].loc = loc;
                            }
                            break;
                        // Sounds
                        case "s":
                            Global.Audio.play_se("Map Sounds", processing.Value[1]);
                            break;
                    }
        }

        public override Rectangle src_rect
        {
            get
            {
                return new Rectangle(texture.Width / 5 * (Image_Index % 5), texture.Width / 5 * (Image_Index / 5), texture.Width / 5, texture.Width / 5);
            }
        }

        public override void update()
        {
            // Update image
            if (Frame_Time >= frame_time_max)
            {
                if (data.animation_data.Count <= Frame + 1)
                {
                    Finished = true;
                    Image_Index = -1;
                }
                else
                {
                    Frame++;
                    process_frame(Frame);
                    Frame_Time = 0;
                }
            }
            Frame_Time++;
            // Update sounds/effects
            Timer++;
            process_timing(Timer);
            // Update sub effects
            int i = 0;
            while (i < Sub_Effects.Count)
            {
                Sub_Effects[i].update();
                if (Sub_Effects[i].finished)
                    Sub_Effects.RemoveAt(i);
                else
                    i++;
            }
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix)
        {
            if (texture != null)
                if (visible)
                {
                    foreach (Map_Effect effect in Sub_Effects)
                        effect.draw(sprite_batch, draw_offset, matrix);
                    if (Image_Index > -1)
                    {
                        Rectangle src_rect = this.src_rect;
                        Vector2 offset = this.offset;
                        if (mirrored) offset.X = src_rect.Width - offset.X;
                        if (Dest_Rect != null)
                            sprite_batch.Draw(texture, (Rectangle)Dest_Rect,
                                src_rect, tint, angle, Vector2.Zero,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        else
                            sprite_batch.Draw(texture, Vector2.Transform((this.loc + draw_vector()) - draw_offset, matrix),
                                src_rect, tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                }
        }
    }
}
