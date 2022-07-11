using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TactileLibrary;
using TactileBattleFrameDataExtension;

namespace Tactile
{
    [Flags]
    enum AnimationFlag
    {
        None =      0,
        Reverse =   1 << 0,
        Repeat =    1 << 1,
        Dmg =       1 << 2,
        NoHit =     1 << 3,
        Kill =      1 << 4,
    }

    class Battle_Animation : Sprite
    {
        string Filename;
        int Frame_Timer = 0;
        int Frame = 0;
        List<int> Ids;
        int Anim_Index = -1;
        Battle_Animation_Data Data;
        int Shake = 0;
        bool Finished = false;
        int Duration, Max_Duration, Anim_Timer;
        int Opacity = 255;
        Color[] Palette = new Color[Palette_Handler.PALETTE_SIZE];
        bool Palette_Used = false, Use_Animation_Texture = false;

        new public int blend_mode = 0;
        private int terrain_sound_tag = 0;
        private AnimationFlag Flags;

        #region Accessors
        public int id
        {
            get
            {
                return GetAnimationId(Anim_Index);
            }
        }

        public int shake { get { return Shake; } }

        public bool finished { get { return Finished; } }

        public int duration
        {
            get { return Duration; }
        }

        public int anim_timer
        {
            get { return 1 + Anim_Timer; }
        }

        public int max_duration
        {
            get { return Max_Duration; }
        }

        public override int opacity { set { Opacity = (int)MathHelper.Clamp(value, 0, 255); } }

        public int pan_time
        {
            get
            {
                if (Data.pan <= -1)
                    return 0;
                if (Data.current_frame(Frame).pan > -1)
                    return Data.pan - Anim_Timer;
                return -1;
            }
        }

        public int brighten_duration
        {
            get
            {
                int offset = 0;
                if (Ids.Count <= Anim_Index + 1)
                {
                    int? brighten_offest = On_Hit.SPELL_BRIGHTEN_OFFSET(this.id);
                    if (brighten_offest != null)
                        offset = (int)brighten_offest + 1; // the +1 fixed Purge, but whatever calls this might be the one that's wrong? //Debug
                }
                return Duration - offset;
            }
        }

        public Color[] palette { get { return Palette; } }

        public bool palette_used { get { return Palette_Used; } }
        #endregion

        public Battle_Animation(string filename, Texture2D texture, List<int> ids, AnimationFlag flags)
        {
            Flags = flags;

            initialize(filename, texture, ids);
        }
        public Battle_Animation(string filename, Texture2D texture, List<int> ids, AnimationFlag flags, int terrain_sound_tag)
        {
            Flags = flags;
            this.terrain_sound_tag = terrain_sound_tag;

            initialize(filename, texture, ids);
        }
        public Battle_Animation(string filename, Texture2D texture, List<int> ids,
            Battle_Animation flagSource, bool repeat)
        {
            Flags = flagSource.Flags;
            Flags &= ~AnimationFlag.Repeat;
            Flags |= repeat ? AnimationFlag.Repeat : AnimationFlag.None;

            initialize(filename, texture, ids);
        }

        protected void initialize(string filename, Texture2D texture, List<int> ids)
        {
            Ids = remove_missing_anims(ids);
            if (Ids.Count <= 0)
            {
                if (ids.Count > 0)
                    throw new IndexOutOfRangeException("Animation with no ids started; ids were given, but no data for them exists");
                else
                    throw new IndexOutOfRangeException("Animation with no ids started");
            }

            string[] str_ary;
            if (filename != null)
            {
                str_ary = filename.Split('/');
                Filename = str_ary[str_ary.Length - 1];
            }
            // If a texture was provided, set it
            if (texture != null)
                this.texture = texture;
            // Else if no texture given
            else
                Use_Animation_Texture = true;

            Anim_Index = -1;
            Max_Duration = Duration = total_duration(Ids) + 1;
            next_animation();
        }

        static List<int> remove_missing_anims(List<int> ids)
        {
            List<int> result = new List<int>();
            foreach (int id in ids)
            {
                if (Global.data_animations.ContainsKey(id))
                    result.Add(id);
            }
            return result;
        }

        protected void refresh_palette(string filename)
        {
            if (Global.battlerPaletteData.ContainsKey(filename) || Global.battlerPaletteData.ContainsKey(filename.Split('-')[0]))
            {
                Palette_Used = true;
                Battler_Sprite.palette_data(filename, -1, -1, "").CopyTo(Palette, 0);
            }
        }

        static int total_duration(List<int> ids)
        {
            int result = 0;
            foreach (int id in ids)
            {
                Battle_Animation_Data data = Global.data_animations[id];
                result += data.duration;
            }
            return result;
        }

        private int GetAnimationId(int index)
        {
            if (index >= Ids.Count || index < 0)
                return -1;
            return Ids[index];
        }

        public override void update()
        {
            Frame_Timer++;
            Duration--;
            Anim_Timer++;
            bool cont = Data == null;
            while (!cont)
            {
                cont = true;
                if (Data.current_frame(Frame) == null)
                {
                    if (Ids.Count > Anim_Index + 1)
                    {
                        next_animation();
                        cont = false;
                    }
                    else
                    {
                        //Sprite probably needs to dispose //Yeti
                        Finished = true;
                        return;
                    }
                }
                // New Frame
                if (Frame_Timer >= Data.current_frame(Frame).time && Data.current_frame(Frame).time >= 0)
                {
                    // Next frame
                    int current_frame = Frame;
                    Frame = Data.next_frame(Frame);

                    // If frame looped over
                    if (Frame <= current_frame)
                    {
                        // If this is the last animation in this set
                        if (Anim_Index >= Ids.Count - 1)
                        {
                            // If this animation set continues until external input
                            if (Flags.HasFlag(AnimationFlag.Repeat))
                            {
                                bool looped = TryLoop();

                                // If not looped, hold on the last frame
                                if (!looped)
                                {
                                    Frame = current_frame;
                                    return;
                                }
                            }
                            else
                            {
                                Finished = true; //Sprite needs to dispose? //Yeti
                                TryLoop();
                            }
                        }
                        // Else go to the next animation
                        else
                        {
                            Frame_Timer = 0;
                            next_animation();
                            return;
                        }
                    }
                    else
                    {
                        Frame_Timer = 0;
                    }
                }
            }
            update_sounds();
        }

        public void update_flash(ref int flash_timer)
        {
            if (Data.current_frame(Frame).flash[0] == Frame_Timer)
                flash_timer = Data.current_frame(Frame).flash[1];
        }

        protected void update_sounds()
        {
            if (Data == null)
                return;
            Shake = 0;
            if (Global.scene.scene_type == "Class_Reel")
                return;

            List<string> sound_names = Data.current_frame(Frame).get_sounds(Frame_Timer);
            foreach (string name in sound_names)
            {
                double battle_step_test;
                if (name.Length >= 5 && name.Substring(0, 5) == "shake" && double.TryParse(name.Substring(5, name.Length - 5), out battle_step_test))
                {
                    Shake = (int)battle_step_test;
                }
                else if (name == "stopmusic")
                {
                    Global.Audio.StopBgm();
                }
                else if (new string[] { "Hit1", "Hit2", "Hit3" }.Contains(name)) // This should check for any hit sounds //Debug
                {
                    if (Flags.HasFlag(AnimationFlag.Kill))
                        Global.Audio.play_se("Battle Sounds", "Hit_Kill");
                    else if (Flags.HasFlag(AnimationFlag.NoHit))
                        Global.Audio.play_se("Battle Sounds", "Miss");
                    else if (!Flags.HasFlag(AnimationFlag.Dmg))
                        Global.Audio.play_se("Battle Sounds", "Hit_NoDamage");
                    else
                        Global.Audio.play_se("Battle Sounds", name);
                }
                else if (name.Length >= 11 && name.Substring(0, 11) == "Battle_Step" && !double.TryParse(name.Substring(name.Length - 1, 1), out battle_step_test))
                {
                    Global.Audio.play_se("Step Sounds", name + terrain_sound_tag.ToString());
                }
                else if (name == "Critical")
                {
                    if (Flags.HasFlag(AnimationFlag.Dmg))
                        Global.Audio.play_se("Battle Sounds", name);
                }
                else
                    Global.Audio.play_se("Battle Sounds", name);
            }
        }

        protected void next_animation()
        {
            while (Anim_Index < Ids.Count)
            {
                Anim_Index++;
                if (Global.data_animations.ContainsKey(this.id))
                {
                    Data = Global.data_animations[this.id];
                    if (Use_Animation_Texture)
                    {
                        // Uses the texture from the first animation in the animation list
                        Filename = Data.filename;
                        if (Global.content_exists(@"Graphics/Animations/" + Filename))
                            this.texture = Global.Battler_Content.Load<Texture2D>(@"Graphics/Animations/" + Filename);

                        if (Filename.Contains('\\'))
                        {
#if DEBUG
                            System.Console.WriteLine(string.Format(
                                "Animation filename with backslash, standardize to forward slashes: \"{0}\"",
                                Filename));
#endif
                            string[] str_ary = Filename.Split('\\');
                            Filename = str_ary.Last();
                        }
                        else
                        {
                            string[] str_ary = Filename.Split('/');
                            Filename = str_ary.Last();
                        }

                        if (this.texture != null)
                            refresh_palette(Filename);
                    }

                    if (Data.frame_count > 0)
                        break;
                    Data = null;
                }
                else
                    Data = null;
            }
            if (Data == null)
                Finished = true;
            Frame = 0;
            Frame_Timer = 0;
            Anim_Timer = 0;
            update_sounds();
        }

        /// <summary>
        /// Return to the start of this animation set's loop, if it has one
        /// </summary>
        protected bool TryLoop()
        {
            int lastId = GetAnimationId(Ids.Count - 1);
            // If the last animation is missing
            if (!Global.data_animations.ContainsKey(lastId))
                return false;
            // If the last animation doesn't loop, there's no loop
            if (!Global.data_animations[lastId].loop)
                return false;

            // Find the last animation that loops
            int loopIndex = Ids.Count - 1;
            for (int i = Ids.Count - 2; i >= 0; i++)
            {
                int id = GetAnimationId(i);
                if (Global.data_animations.ContainsKey(id) &&
                        Global.data_animations[id].loop)
                    loopIndex = i;
                else
                    break;
            }

            // Move to the first loop animation
            Anim_Index = loopIndex - 1;
            next_animation();

            return true;
        }

        #region Draw
        public IEnumerable<BattleFrameRenderData> draw_lower(
            SpriteBatch sprite_batch,
            Maybe<Vector2> draw_offset = default(Maybe<Vector2>),
            Maybe<Vector2> scale_mod = default(Maybe<Vector2>),
            Maybe<Matrix> matrix = default(Maybe<Matrix>))
        {
            if (draw_offset.IsNothing)
                draw_offset = Vector2.Zero;
            if (scale_mod.IsNothing)
                scale_mod = Vector2.One;
            if (matrix.IsNothing)
                matrix = Matrix.Identity;

            if (texture != null)
                if (visible)
                {
                    Vector2 offset = this.offset;
                    if (Data != null)
                    {
                        Battle_Frame_Data frame = Data.current_frame(Frame);
                        if (frame != null)
                            foreach (var render in frame.draw_lower(
                                    texture, Global.frame_data.ContainsKey(Filename) ? Global.frame_data[Filename] : null, sprite_batch,
                                    Data, Frame, Frame_Timer,
                                    loc + draw_vector() - draw_offset, matrix, offset, scale * scale_mod,
                                    mirrored, Flags.HasFlag(AnimationFlag.Reverse), Opacity, blend_mode, tint))
                                yield return render;
                    }
                }
        }

        public IEnumerable<BattleFrameRenderData> draw_upper(
            SpriteBatch sprite_batch,
            Maybe<Vector2> draw_offset = default(Maybe<Vector2>),
            Maybe<Vector2> scale_mod = default(Maybe<Vector2>),
            Maybe<Matrix> matrix = default(Maybe<Matrix>))
        {
            if (draw_offset.IsNothing)
                draw_offset = Vector2.Zero;
            if (scale_mod.IsNothing)
                scale_mod = Vector2.One;
            if (matrix.IsNothing)
                matrix = Matrix.Identity;

            if (texture != null)
                if (visible)
                {
                    Vector2 offset = this.offset;
                    if (Data != null)
                    {
                        Battle_Frame_Data frame = Data.current_frame(Frame);
                        if (frame != null)
                            foreach (var render in frame.draw_upper(
                                    texture, Global.frame_data.ContainsKey(Filename) ? Global.frame_data[Filename] : null, sprite_batch,
                                    Data, Frame, Frame_Timer,
                                    loc + draw_vector() - draw_offset, matrix, offset, scale * scale_mod,
                                    mirrored, Flags.HasFlag(AnimationFlag.Reverse), Opacity, blend_mode, tint))
                                yield return render;
                    }
                }
        }
        #endregion
    }
}
