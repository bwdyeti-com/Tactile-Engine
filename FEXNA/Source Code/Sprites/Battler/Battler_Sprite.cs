using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA_Library;
using EnumExtension;
using IntExtension;
using ListExtension;
using FEXNAWeaponExtension;

namespace FEXNA
{
    class Battler_Sprite : Sprite
    {
        readonly static int[] NO_DAMAGE_JITTER = new int[] { 1, 2, 3, 2, 1, 0 };

        string Battler_Name, Name;
        int Gender;
        BattlerSpriteData Battler;
        Texture2D Anim_Bitmap;
        Color[] Palette = new Color[Palette_Handler.PALETTE_SIZE];
        Battle_Animation Animation;
        Battle_Animation Spell_Effect, Anima_Effect, Skill_Effect;
        Battle_Animation Spell_Effect_2, Spell_Effect_3, Spell_Effect_4;
        List<Battle_Animation> Temp_Anims = new List<Battle_Animation>();
        bool Idle = false, Avoiding = false;
        int Shake = 0;
        Vector2 Size = new Vector2(Config.BATTLER_SIZE, Config.BATTLER_SIZE);
        int NoDamageIndex = -1;
        List<int> Death_Ary = new List<int>();
        int Promotion_Timer = 0, Promotion_Index = -1, Promotion_Duration = 0;
        Promotion_Effect[] Promotion_Data = null;
        Color Color_Shift = Color.Transparent;
        int Color_Shift_Time = 0, Color_Shift_Timer = 0, Color_Shift_Base_Alpha;
        int Status_Effect_Timer = 0;
        bool Reverse, Scene_Reverse;
        bool Battle_Start = false;
        int Distance;
        bool Greyed_Out = false;
        protected bool Has_Palette = true;

        protected int Whiten_Duration = 0;
        protected int Flash_Timer = 0;

        #region Accessors
        public Game_Unit battler { get { return Battler.Unit; } }

        public Battle_Animation spell_effect { get { return Spell_Effect; } }

        public Battle_Animation anima_effect { get { return Anima_Effect; } }

        public Battle_Animation skill_effect { get { return Skill_Effect; } }

        public bool avoiding { get { return Avoiding; } }

        public int duration
        {
            get
            {
                //if (Animation != null)
                //    return Animation.duration;
                if (Promotion_Data != null)
                    return Promotion_Duration;
                if (!Idle && Animation != null)
                    return Animation.duration;
                return 0;
            }
        }

        public bool anima_ready
        {
            get
            {
                if (Anima_Effect != null)
                    return Anima_Effect.duration <= (Config.BATTLER_ANIMA_DONE_TIME);
                return true;
            }
        }

        public bool spell_ready
        {
            get
            {
                return spell_effect_duration <= (Config.BATTLER_SPELL_DONE_TIME);
            }
        }

        public bool spell_pan
        {
            get
            {
                if (Spell_Effect == null)
                    return true;
                if (Spell_Effect.pan_time <= -1)
                    return false;
                if (Spell_Effect.pan_time <= 1)
                    return true;
                return false;
            }
        }

        public int spell_effect_duration
        {
            get
            {
                if (Spell_Effect == null)
                {
                    if (Global.scene.scene_type != "Scene_Promotion" && Spell_Effect_2 != null)
                        return Spell_Effect_2.duration;
                    return 0;
                }
                return Spell_Effect.duration;
            }
        }

        public bool flash { get { return Flash_Timer > 0; } }
        #endregion
        
        public Battler_Sprite(BattlerSpriteData battler, bool reverse = false, int distance = 1, bool scene_reverse = true)
        {
            initialize(battler, reverse, distance, scene_reverse);
        }

        public override string ToString()
        {
            return string.Format("Battler_Sprite: {0}, {1}", Battler.Name, Battler_Name);
        }

        protected void initialize(BattlerSpriteData battler, bool reverse, int distance, bool scene_reverse)
        {
            Battler = battler;

            // if promoting, using magic = false //Yeti
            Reverse = reverse;
            Distance = distance;
            Scene_Reverse = scene_reverse;
            Src_Rect = new Rectangle(0, 0, Config.BATTLER_SIZE, Config.BATTLER_SIZE);
            visible = false;

            Battler_Name = (Global.scene.scene_type == "Scene_Dance" &&
                    Battler.Unit.id == Global.game_state.dancer_id) ?
                Battler.BattlerName(Global.weapon_types[0].AnimName) :
                Battler.BattlerName();
            if (Reverse)
                mirrored = true;
            reset_pose();
            initialize_palette();
            initialize_animation();
            pre_battle_anim(Distance);
            dance_grey_out();
        }

        protected void dance_grey_out()
        {
            if (Global.scene.scene_type == "Scene_Dance" && !Battler.Unit.ready)
                if (Global.game_state.dance_item == -1 || Constants.Combat.RING_REFRESH)
                    Greyed_Out = true;
        }

        protected void initialize_palette()
        {
            // Get and set animation bitmap
            //Anim_Sprite_Data anim_data = Battler.AnimData(Global.scene.scene_type == "Scene_Dance");
            //string name = anim_data.name;

            if (false)// || Battler.WeaponId == 0) // || promotion //Yeti
            //if (false)//name == "")// || Battler.WeaponId == 0) // || promotion //Yeti
            { } //Palette = null; //Debug
            else
            {
                string name = Battler_Name;
                if (!Global.battlerPaletteData.ContainsKey(Battler_Name))
                {
                    List<int> anim = FE_Battler_Image_Wrapper.attack_animation_value(Battler, false, 1, false);
                    if (anim.Count > 0)
                        name = Global.data_animations[anim[0]].filename.Split('-')[0];
                }

                if (!Global.battlerPaletteData.ContainsKey(name))
                    Has_Palette = false;
                else
                    palette_data(name, Battler.ClassId, Battler.Gender, Battler.NameFull).CopyTo(Palette, 0);
            }
        }

        public static Color[] palette_data(string image_name, int class_id, int gender, string name)
        {
            List<Color> palette = new List<Color>();

            // Start with default colors by image
            if (!Global.battlerPaletteData.ContainsKey(image_name))
                image_name = image_name.Split('-')[0];
            // Then get battler recolored palette
            if (Global.battlerPaletteData.ContainsKey(image_name))
            {
                var paletteData = Global.battlerPaletteData[image_name];
                for (int i = palette.Count; i < paletteData.Count; i++)
                    palette.Add(paletteData.GetEntry(i).Value);
                var basePalette = new List<Color>(palette);

                // First, get the colors defined by FEXNA.Recolor
                if (class_id > -1)
                {
                    int color_count = Math.Max(15, palette.Count);

                    bool recolors_found = false;
                    for (int i = 0; i < color_count; i++)
                    {
                        Color? target_color = FEXNA.Recolor.get_color(class_id, gender, name, i);
                        if (target_color != null)
                        {
                            if (palette.Count == i)
                                palette.Add((Color)target_color);
                            else
                                palette[i] = (Color)target_color;
                            recolors_found = true;
                        }
                    }
                    // If no recolors were found, try again with base gender
                    if (!recolors_found)
                        for (int i = 0; i < color_count; i++)
                        {
                            Color? target_color = FEXNA.Recolor.get_color(class_id, gender % 2, name, i);
                            if (target_color != null)
                            {
                                if (palette.Count == i)
                                    palette.Add((Color)target_color);
                                else
                                    palette[i] = (Color)target_color;
                                recolors_found = true;
                            }
                        }
                }

                // Then, get colors from recolor data
                HashSet<string> alreadyRenamed = new HashSet<string>();
                // Check for renames a set depth
                for (int i = 0; i < 16; i++)
                {
                    if (Recolor.SPRITE_RENAME_LIST.ContainsKey(name))
                    {
                        // To avoid getting caught in loops
                        if (alreadyRenamed.Contains(name))
                            break;

                        alreadyRenamed.Add(name);
                        name = Recolor.SPRITE_RENAME_LIST[name];
                    }
                    else
                        break;
                }

                FEXNA_Library.Palette.RecolorData recolorData;
                if (Global.battlerRecolorData.ContainsKey(name))
                    recolorData = Global.battlerRecolorData[name];
                else
                    recolorData = Global.battlerRecolorData.FirstOrDefault(x => x.Key.StartsWith(name)).Value;

                if (recolorData != null)
                {
                    var recolors = paletteData.GetRecolors(recolorData, FEXNA.Recolor.RAMP_DEFAULT_OTHER_NAMES);
                    for (int i = 0; i < palette.Count && i < recolors.Length; i++)
                        palette[i] = recolors[i];
                }
            }
            return palette.ToArray();
        }

        bool Animation_Initialized = false;
        public void initialize_animation()
        {
            if (!Animation_Initialized)
            {
                Animation_Initialized = true;
                if (Battler_Name != "")
                {
                    // Get and set animation bitmap
                    Anim_Sprite_Data anim_data = Battler.AnimData(Global.scene.scene_type == "Scene_Dance");
                    Name = anim_data.name;
                    Gender = anim_data.gender;
                    // I still don't understand what entirely is happening here
                    // For a while it was
                    // if (Name == "") || Battler.WeaponId == 0)
                    // But that defaults to the animation's bitmap when the unit is unarmed, and unarmed units have animations now so
                    if (Name == "") // || Battler.WeaponId == 0) // || promotion //Yeti
                        Anim_Bitmap = null;
                    else
                    {
                        Anim_Bitmap = Global.Battler_Content.Load<Texture2D>(@"Graphics/Animations/" + Name);
                    }
                }
            }
        }

        public void whiten(int time = 8)
        {
            //tint = new Color(0, 0, 0, 0);
            Whiten_Duration = time;
        }

        public void start_battle()
        {
            Battle_Start = true;
        }

        public void update_animation_position()
        {
            if (Animation != null)
            {
                Animation.loc = loc;
                Animation.offset = offset;
            }
        }

        #region Animations
        public void idle_anim()
        {
            Avoiding = false;
            if (Idle)
                return;
            Idle = true;

            List<int> anim = FE_Battler_Image_Wrapper.idle_animation_value(Battler, 1); // Distance? //Yeti
            if (anim.Count == 0)
            {
#if DEBUG
                //throw new ArgumentNullException("No idle animation for " + Battler_Name); //Debug
#endif
                Animation = null;
            }
            else
            {
                set_animation(anim, true);
#if DEBUG
                if (Anim_Bitmap == null && Animation == null)
                    throw new ArgumentNullException("Did not load the bitmap for this animation properly");
#endif
            }
        }
        public void avoid()
        {
            Idle = false;
            Avoiding = true;

            List<int> anim = FE_Battler_Image_Wrapper.avoid_animation_value(Battler, 1); // Distance? //Yeti
            if (anim.Count == 0)
            {
                idle_anim();
            }
            else
            {
                set_animation(anim, true);
#if DEBUG
                if (Anim_Bitmap == null && Animation == null)
                    throw new ArgumentNullException("Did not load the bitmap for this animation properly");
#endif
            }
        }
        public void avoid_return()
        {
            Idle = false;
            Avoiding = true;

            List<int> anim = FE_Battler_Image_Wrapper.avoid_return_animation_value(Battler, 1); // Distance? //Yeti
            if (anim.Count == 0)
            {
                idle_anim();
            }
            else
            {
                set_animation(anim);
#if DEBUG
                if (Anim_Bitmap == null && Animation == null)
                    throw new ArgumentNullException("Did not load the bitmap for this animation properly");
#endif
            }
        }
        public void get_hit(int dmg, bool crit)
        {
            Idle = false;

            List<int> anim = FE_Battler_Image_Wrapper.get_hit_animation_value(Battler, crit, 1); // Distance? //Yeti
            if (anim.Count == 0)
            {
                idle_anim();
            }
            else
            {
                set_animation(anim, true);
#if DEBUG
                if (Anim_Bitmap == null && Animation == null)
                    throw new ArgumentNullException("Did not load the bitmap for this animation properly");
#endif
            }
        }

        protected void pre_battle_anim(int distance)
        {
            Idle = false;

            List<int> anim = FE_Battler_Image_Wrapper.pre_battle_animation_value(Battler, distance);
            if (anim.Count == 0)
                idle_anim();
            else
            {
                set_animation(anim);
#if DEBUG
                if (Anim_Bitmap == null && Animation == null)
                    throw new ArgumentNullException("Did not load the bitmap for this animation properly");
#endif
                //animation(anim,true,@reverse, @anim_bitmap, @name,
                //    false, no_damage, kill)
                //temp_anim_update();
            }
        }

        #region Attack Animations
        public void attack(bool crit, int distance)
        {
            attack(crit, distance, false, false, true);
        }
        public void attack(bool crit, int distance, bool no_damage, bool kill, bool hit)
        {
            Idle = false;

            List<int> anim = FE_Battler_Image_Wrapper.attack_animation_value(Battler, crit, distance, hit);
            if (anim.Count == 0)
                idle_anim();
            else
            {
                set_animation(anim, true, no_damage, hit, kill);
#if DEBUG
                if (Anim_Bitmap == null && Animation == null)
                    throw new ArgumentNullException("Did not load the bitmap for this animation properly");
#endif
                //animation(anim,true,@reverse, @anim_bitmap, @name,
                //    false, no_damage, kill)
                //temp_anim_update();
            }
        }

        public void hit_freeze(bool crit, int distance)
        {
            hit_freeze(crit, distance, false, false, true);
        }
        public void hit_freeze(bool crit, int distance, bool no_damage, bool kill, bool hit)
        {
            Idle = false;

            List<int> anim = FE_Battler_Image_Wrapper.still_animation_value(Battler, crit, distance, hit);
            if (anim.Count == 0)
                idle_anim();
            else
            {
                set_animation(anim, true, no_damage, hit, kill);
                //animation(anim, true, @reverse, @anim_bitmap, @name, true)
                //temp_anim_update();
            }
        }

        public void return_anim(bool crit, int distance)
        {
            return_anim(crit, distance, false, false, true);
        }
        public void return_anim(bool crit, int distance, bool no_damage, bool kill, bool hit)
        {
            Idle = false;

            List<int> anim = FE_Battler_Image_Wrapper.return_animation_value(Battler, crit, distance, hit, no_damage);
            if (anim.Count == 0)
                idle_anim();
            else
            {
                set_animation(anim, false, no_damage, hit, kill);
                //animation(anim,true,@reverse, @anim_bitmap, @name,
                //temp_anim_update();
            }
        }
        #endregion

        #region Dance Animations
        public void dance_attack()
        {
            Idle = false;

            List<int> anim = FE_Battler_Image_Wrapper.dance_attack_animation_value(Battler);
            if (anim.Count == 0)
                idle_anim();
            else
            {
                set_animation(anim);
#if DEBUG
                if (Anim_Bitmap == null && Animation == null)
                    throw new ArgumentNullException("Did not load the bitmap for this animation properly");
#endif
            }
        }

        public void dance_hit_freeze()
        {
            Idle = false;

            List<int> anim = FE_Battler_Image_Wrapper.dance_still_animation_value(Battler);
            if (anim.Count == 0)
                idle_anim();
            else
            {
                set_animation(anim, true);
            }
        }

        public void dance_return_anim()
        {
            Idle = false;

            List<int> anim = FE_Battler_Image_Wrapper.dance_return_animation_value(Battler);
            if (anim.Count == 0)
                idle_anim();
            else
            {
                set_animation(anim, true);
            }
        }
        #endregion

        void set_animation(List<int> anim, bool repeat = false)
        {
            set_animation(anim, repeat, true, true, false);
        }
        void set_animation(List<int> anim, bool repeat, bool no_damage, bool hit, bool kill)
        {
            Animation = new Battle_Animation(Name, Anim_Bitmap, anim, Reverse, repeat,
                !no_damage, hit, kill, terrain_based_sound());

            Animation.loc = loc;
            Animation.offset = offset;
            Animation.blend_mode = Blend_Mode;
            Animation.opacity = tint.R;
            Animation.mirrored = Reverse;
            update_temp_anim();
        }

        public bool is_skill_time()
        {
            return is_skill_time(false);
        }
        public bool is_skill_time(bool crt)
        {
            if (Animation == null) return false;
            int time_offset = 1;
            if (FE_Battler_Image.SKILL_ACTIVATION_FRAME.ContainsKey(Battler.ClassId))
                time_offset = Math.Max(1, FE_Battler_Image.SKILL_ACTIVATION_FRAME[Battler.ClassId][crt ? 1 : 0]);
                
            return Animation.max_duration - Animation.duration == time_offset;
        }

        public void skill_animation(int distance)
        {
            int? id = Battler.Unit.skill_animation_val();
            if (id != null && id != -1)
            {
                id += Global.animation_group("Skills");
                List<int> anim = FE_Battler_Image_Wrapper.correct_animation_value((int)id, Battler);
                if (anim.Count > 0)
                    if (anim[0] > -1)
                    {
                        Skill_Effect = new Battle_Animation(Name, null, anim, Reverse, false);
                        Skill_Effect.loc = loc + new Vector2(Reverse ? -48 : 48, 0);
                        if (distance > 2 && Scene_Reverse ^ Reverse)
                            Skill_Effect.loc += new Vector2(Reverse ? -270 : 270, 0);
                        Skill_Effect.offset = offset;
                        Skill_Effect.mirrored = Reverse;
                        Skill_Effect.stereoscopic = Config.BATTLE_PLATFORM_BASE_DEPTH;
                    }
            }
        }

        public void anima_start(int distance)
        {
            int spell_anim;
            int anim_offset = Global.animation_group("Spells");
            if (Global.weapon_types[Battler.WeaponAnimaType].Name == "Fire") //@Yeti
                spell_anim = anim_offset + 1;
            else if (Global.weapon_types[Battler.WeaponAnimaType].Name == "Thunder")
                spell_anim = anim_offset + 61;
            else if (Global.weapon_types[Battler.WeaponAnimaType].Name == "Wind")
                spell_anim = anim_offset + 121;
            else
                return;

            Anima_Effect = new Battle_Animation(Name, null, new List<int> { spell_anim }, Reverse, false);
            Anima_Effect.loc = loc + new Vector2(Reverse ? -64 : 64, 0);
            if (distance > 2 && Scene_Reverse ^ Reverse)
                Anima_Effect.loc += new Vector2(Reverse ? -270 : 270, 0);
            Anima_Effect.offset = offset;
            Anima_Effect.mirrored = Reverse;
            Anima_Effect.stereoscopic = Config.BATTLE_BATTLERS_DEPTH;
        }

        public void attack_spell(bool hit, bool crit, int distance)
        {
            Vector2 spell_loc = loc + new Vector2(Reverse ? -48 : 48, 0);
            if (distance > 1)
                spell_loc += new Vector2(Reverse ? -48 : 80, 0) + new Vector2(Scene_Reverse ? 0 : -32, 0);
            if (distance > 2 && Scene_Reverse ^ Reverse)
                spell_loc += new Vector2(Reverse ? -270 : 270, 0);

            List<int> anim;
            anim = FE_Battler_Image_Wrapper.spell_attack_animation_value(
                Battler.WeaponId, Battler.MagicAttack, distance, hit);
            if (anim.Count > 0)
            {
                Spell_Effect = new Battle_Animation(Name, null, anim, Reverse, false,
                    Animation.dmg, Animation.hit, Animation.kill);
                Spell_Effect.loc = spell_loc;
                Spell_Effect.offset = offset;
                Spell_Effect.mirrored = Reverse;
                Spell_Effect.stereoscopic = Config.BATTLE_PLATFORM_BASE_DEPTH;
            }
            anim = FE_Battler_Image_Wrapper.spell_attack_animation_value_bg1(
                Battler.WeaponId, Battler.MagicAttack, distance, hit);
            if (anim.Count > 0)
            {
                Spell_Effect_2 = new Battle_Animation(Name, null, anim, Reverse, false);
                Spell_Effect_2.loc = spell_loc;
                Spell_Effect_2.offset = offset;
                Spell_Effect_2.mirrored = Reverse;
                Spell_Effect_2.stereoscopic = Config.BATTLE_BATTLERS_DEPTH;
            }
            anim = FE_Battler_Image_Wrapper.spell_attack_animation_value_bg2(
                Battler.WeaponId, Battler.MagicAttack, distance, hit);
            if (anim.Count > 0)
            {
                Spell_Effect_3 = new Battle_Animation(Name, null, anim, Reverse, false);
                Spell_Effect_3.loc = spell_loc;
                Spell_Effect_3.offset = offset;
                Spell_Effect_3.mirrored = Reverse;
                Spell_Effect_3.stereoscopic = Config.BATTLE_PLATFORM_BASE_DEPTH + Config.BATTLE_PLATFORM_TOP_DEPTH_OFFSET;
            }
            anim = FE_Battler_Image_Wrapper.spell_attack_animation_value_fg(
                Battler.WeaponId, Battler.MagicAttack, distance, hit);
            if (anim.Count > 0)
            {
                Spell_Effect_4 = new Battle_Animation(Name, null, anim, Reverse, false);
                Spell_Effect_4.loc = spell_loc;
                Spell_Effect_4.offset = offset;
                Spell_Effect_4.mirrored = Reverse;
                Spell_Effect_4.stereoscopic = Config.BATTLE_HUD_DEPTH;
            }
        }

        public void end_spell(bool hit, bool crit, int distance)
        {
            Vector2 spell_loc = loc + new Vector2(Reverse ? -48 : 48, 0);
            if (distance > 1)
                spell_loc += new Vector2(Reverse ? -48 : 80, 0) + new Vector2(Scene_Reverse ? 0 : -32, 0);
            if (distance > 2 && Scene_Reverse ^ Reverse)
                spell_loc += new Vector2(Reverse ? -270 : 270, 0);

            List<int> anim = FE_Battler_Image_Wrapper.spell_end_animation_value(
                Battler.WeaponId, Battler.MagicAttack, hit);
            if (anim.Count > 0)
            {
                Spell_Effect = new Battle_Animation(Name, null, anim, Reverse, false);
                Spell_Effect.loc = spell_loc;
                Spell_Effect.offset = offset;
                Spell_Effect.mirrored = Reverse;
                Spell_Effect.stereoscopic = Config.BATTLE_PLATFORM_BASE_DEPTH;
            }
        }

        public void life_drain_spell_1(bool hit, bool crit, int distance)
        {
            Vector2 spell_loc = loc + new Vector2(Reverse ? -48 : 48, 0);
            if (distance > 1)
                spell_loc += new Vector2(Reverse ? -48 : 80, 0) + new Vector2(Scene_Reverse ? 0 : -32, 0);
            if (distance > 2 && Scene_Reverse ^ Reverse)
                spell_loc += new Vector2(Reverse ? -270 : 270, 0);

            List<int> anim = FE_Battler_Image_Wrapper.spell_lifedrain_animation_value(
                Battler.WeaponId, Battler.MagicAttack, distance);
            if (anim.Count > 0)
            {
                Spell_Effect = new Battle_Animation(Name, null, anim, Reverse, false);
                Spell_Effect.loc = spell_loc;
                Spell_Effect.offset = offset;
                Spell_Effect.mirrored = Reverse;
                Spell_Effect.stereoscopic = Config.BATTLE_PLATFORM_BASE_DEPTH;
            }
        }

        public void life_drain_spell_2(bool hit, bool crit, int distance)
        {
            Vector2 spell_loc = loc + new Vector2(Reverse ? -48 : 48, 0);
            if (distance > 1)
                spell_loc += new Vector2(Reverse ? 0 : 32, 0) + new Vector2(Scene_Reverse ? 0 : -32, 0);
            if (distance > 2 && Scene_Reverse ^ Reverse)
                spell_loc += new Vector2(Reverse ? -270 : 270, 0);

            List<int> anim = FE_Battler_Image_Wrapper.spell_lifedrain_end_animation_value(
                Battler.WeaponId, Battler.MagicAttack);
            if (anim.Count > 0)
            {
                Spell_Effect = new Battle_Animation(Name, null, anim, Reverse, false);
                Spell_Effect.loc = spell_loc;
                Spell_Effect.offset = offset;
                Spell_Effect.mirrored = Reverse;
                Spell_Effect.stereoscopic = Config.BATTLE_PLATFORM_BASE_DEPTH;
            }
        }

        public void refersh_spell(int distance, int ring_index)
        {
            Vector2 spell_loc = loc + new Vector2(Reverse ? -48 : 48, 0);
            if (distance > 1)
                spell_loc += new Vector2(Reverse ? -48 : 80, 0) + new Vector2(Scene_Reverse ? 0 : -32, 0);
            if (distance > 2 && Scene_Reverse ^ Reverse)
                spell_loc += new Vector2(Reverse ? -270 : 270, 0);

            int ring_id = ring_index > -1 ? Battler.Unit.items[ring_index].Id : -1;
            List<int> anim = FE_Battler_Image_Wrapper.refresh_animation_value(Battler, ring_id);
            if (anim.Count > 0)
            {
                Spell_Effect = new Battle_Animation(Name, null, anim, Reverse, false);
                Spell_Effect.loc = spell_loc;
                Spell_Effect.offset = offset;
                Spell_Effect.mirrored = Reverse;
                Spell_Effect.stereoscopic = Config.BATTLE_PLATFORM_BASE_DEPTH;
            }
        }

        public void class_change_start()
        {
            Vector2 spell_loc = loc + new Vector2(Reverse ? -48 : 48, 0);

            Promotion_Data = FE_Battler_Image.PROMOTION_START;
            update_promotion();
            List<int> anim;
            int anim_offset = Global.animation_group("Promotion");
            // Lightning effects
            anim = anim_offset.list_add(new List<int> { 1 });
            if (anim.Count > 0)
            {
                Spell_Effect = new Battle_Animation(Name, null, anim, false, false);
                Spell_Effect.loc = spell_loc;
                Spell_Effect.offset = offset;
                Spell_Effect.stereoscopic = Config.BATTLE_PLATFORM_BASE_DEPTH;
            }
            // Pillar effects
            anim = anim_offset.list_add(new List<int> { 6 });
            if (anim.Count > 0)
            {
                Spell_Effect_2 = new Battle_Animation(Name, null, anim, false, false);
                Spell_Effect_2.loc = spell_loc;
                Spell_Effect_2.offset = offset;
                Spell_Effect_2.stereoscopic = Config.BATTLE_BATTLERS_DEPTH;
            }
        }

        public void class_change_end()
        {
            Vector2 spell_loc = loc + new Vector2(Reverse ? -48 : 48, 0);

            Promotion_Data = FE_Battler_Image.PROMOTION_END;
            update_promotion();
            List<int> anim;
            int anim_offset = Global.animation_group("Promotion");
            // Lightning effects
            anim = anim_offset.list_add(new List<int> { 2, 3 });
            if (anim.Count > 0)
            {
                Spell_Effect = new Battle_Animation(Name, null, anim, false, false);
                Spell_Effect.loc = spell_loc;
                Spell_Effect.offset = offset;
                Spell_Effect.stereoscopic = Config.BATTLE_PLATFORM_BASE_DEPTH;
            }
            // Pillar effects
            anim = anim_offset.list_add(new List<int> { 7 });
            if (anim.Count > 0)
            {
                Spell_Effect_2 = new Battle_Animation(Name, null, anim, false, false);
                Spell_Effect_2.loc = spell_loc;
                Spell_Effect_2.offset = offset;
                Spell_Effect_2.stereoscopic = Config.BATTLE_BATTLERS_DEPTH;
            }
            // End screen flash
            anim = anim_offset.list_add(new List<int> { 4, 5 });
            if (anim.Count > 0)
            {
                Spell_Effect_4 = new Battle_Animation(Name, null, anim, false, false);
                Spell_Effect_4.loc = spell_loc;
                Spell_Effect_4.offset = offset;
            }
        }
        #endregion

        #region Sprite Effects
        public void kill()
        {
            Death_Ary = new List<int>
            {
                0,20,20,20,20,44,44,44,44,64,64,64,64,84,84,84,108,108,108,
                108,128,128,128,128,148,148,148,148,172,172,172,192,192,192,192,212,
                212,212,212,236,236,236,236,255,255,255,0,0,0,0,0,0,256,0,0,0,0,0,0,
                255,0,0,0,0,0,0,255,0,0,0,0,0,0,255,0,0,0,0,0,0,255,0,0,0,0,0,0
            };
        }

        public bool is_dead()
        {
            return Death_Ary.Count <= 39;
        }

        public void no_damage()
        {
            NoDamageIndex = 0;
        }
        #endregion

        public void reset_pose()
        {
            offset = new Vector2(Size.X / 2, Size.Y);
            if (Animation != null)
            {
                Animation.loc = loc;
                Animation.offset = offset;
            }
        }

        public int shake()
        {
            int shake = Shake;
            Shake = 0;
            return shake;
        }

        public bool ignore_hud()
        {
            return Battler.Unit.ignore_hud();
        }

        #region Audio
        public void hit_sound()
        {
            hit_sound(true, true, false);
        }
        public void hit_sound(bool damage_caused, bool hit, bool magic)
        {
            hit_sound(damage_caused, hit, magic, false, false);
        }
        public void hit_sound(bool damage_caused, bool hit, bool magic, bool crit, bool kill)
        {
            if (magic) // This should be handled somewhere else or some other way //Debug
            {
                // Critical explode
                if (crit && damage_caused)
                    Global.Audio.play_se("Battle Sounds", "Critical");

                // Miss
                if (!hit)
                    Global.Audio.play_se("Battle Sounds", "Miss");
                // Kill
                else if (kill)
                    Global.Audio.play_se("Battle Sounds", "Hit_Kill");
                // No Damage
                else if (!damage_caused)
                    Global.Audio.play_se("Battle Sounds", "Hit_NoDamage");
                else
                {
                    if (!On_Hit.SPELL_HIT_SOUNDS.ContainsKey(Battler.WeaponId))
                        Global.Audio.play_se("Battle Sounds", "Hit3");
                    else
                        Global.Audio.play_se("Battle Sounds", "Hit" + On_Hit.SPELL_HIT_SOUNDS[Battler.WeaponId].ToString());
                }

            }
        }

        protected int terrain_based_sound()
        {
            if (!Global.map_exists || Global.game_system.In_Arena)
                return 0;
            else
                return Global.game_map.terrain_step_sound_group(Battler.Unit.loc);
        }
        #endregion

        public bool all_shake
        {
            get
            {
                if (Animation == null)
                    return false;
                if (On_Hit.BOTH_PLATFORM_SHAKE(Animation.id))
                    return true;
                return false;
            }
        }

        protected void update_status()
        {
            if (Status_Effect_Timer > 0)
            {
                Status_Effect_Timer = Math.Max((Status_Effect_Timer + 1) % 361, 1);
            }
            if (Global.scene.scene_type != "Scene_Promotion")
            {
                bool status_active = false;
                foreach (int state_id in Battler.Unit.actor.states)
                    if (Global.data_statuses[state_id].Battle_Color.A > 0)
                    {
                        status_active = true;
                        Color_Shift = Global.data_statuses[state_id].Battle_Color;
                        if (Status_Effect_Timer == 0)
                            Status_Effect_Timer = 91;
                        Color_Shift.A = (byte)((Math.Sin((Status_Effect_Timer - 1) / 360f * 8 * Math.PI) / 2 + 0.5f) * Color_Shift.A);
                        break;
                    }
                if (!status_active)
                {
                    Color_Shift = Color.Transparent;
                    Status_Effect_Timer = 0;
                }
            }
        }

        protected void update_temp_anim()
        {
            int i = 0;
            while (i < Temp_Anims.Count)
            {
                Temp_Anims[i].update();
                if (Temp_Anims[i].finished)
                    Temp_Anims.RemoveAt(i);
                else
                    i++;
            }

            if (Animation != null)
            {
                var added_effects = On_Hit.ADDED_EFFECTS(Animation.id);
                if (added_effects != null)
                    foreach(var added_effect in added_effects)
                        if (added_effect.Item1 == Animation.anim_timer)
                        {
                            List<int> anims = new List<int>(added_effect.Item2);
                            //anims.AddRange(added_effects[Animation.anim_timer]); //Debug

                            // Correct animations for terrain, etc
                            for (i = anims.Count - 1; i >= 0; i--)
                            {
                                List<int> corrected = FE_Battler_Image_Wrapper.correct_animation_value(anims[i], Battler);
                                anims.RemoveAt(i);
                                anims.InsertRange(i, corrected);
                            }
                            if (anims.Count > 0)
                            {
                                // Add a new sprite and have it play the animation
                                Battle_Animation temp_animation = new Battle_Animation(Name, null, anims, Reverse, false);
                                temp_animation.loc = loc;
                                if (Distance > 2 && Scene_Reverse ^ Reverse)
                                    temp_animation.loc += new Vector2(Reverse ? -270 : 270, 0);
                                temp_animation.offset = offset;
                                temp_animation.mirrored = Reverse;
                                temp_animation.stereoscopic = Config.BATTLE_BATTLERS_DEPTH;
                                Temp_Anims.Add(temp_animation);
                            }
                        }
            }
        }

        protected void update_color_shift()
        {
            if (Whiten_Duration > 0)
                Whiten_Duration--;
            if (Color_Shift_Time > 0)
            {
                Color_Shift_Timer--;
                if (Color_Shift_Timer <= 0)
                {
                    Color_Shift = Color.Transparent;
                    Color_Shift_Timer = 0;
                    Color_Shift_Time = 0;
                    Color_Shift_Base_Alpha = 0;
                }
                else
                    Color_Shift.A = (byte)(Color_Shift_Base_Alpha * Color_Shift_Timer / Color_Shift_Time);
            }
        }

        protected void update_promotion()
        {
            if (Promotion_Data != null)
            {
                Promotion_Timer++;
                Promotion_Duration++;
                if (Promotion_Index < 0 || Promotion_Timer >= Promotion_Data[Promotion_Index].time)
                {
                    Promotion_Timer = 0;
                    Promotion_Index++;
                    if (Promotion_Index >= Promotion_Data.Length)
                    {
                        Promotion_Data = null;
                        Promotion_Index = -1;
                        Promotion_Duration = 0;
                    }
                    else
                    {
                        tint = Promotion_Data[Promotion_Index].tint;
                        Animation.tint = tint;
                        if (!string.IsNullOrEmpty(Promotion_Data[Promotion_Index].sound))
                            Global.Audio.play_se("Battle Sounds", Promotion_Data[Promotion_Index].sound);
                        if (Promotion_Data[Promotion_Index].sprite_flash_time > 0)
                        {
                            Color_Shift = Promotion_Data[Promotion_Index].sprite_tint;
                            Color_Shift_Timer = Color_Shift_Time = Promotion_Data[Promotion_Index].sprite_flash_time;
                            Color_Shift_Base_Alpha = Color_Shift.A;
                        }
                        if (Promotion_Data[Promotion_Index].scene_flash_time > 0)
                            ((Scene_Action)Global.scene).screen_flash(Promotion_Data[Promotion_Index].scene_flash_time);
                    }
                }
            }
        }
        
        protected void update_animation(ref Battle_Animation anim)
        {
            if (anim != null)
            {
                anim.update();
                if (anim.shake > 0)
                    Shake = anim.shake;
                if (anim.finished)
                {
                    if (anim == Animation)
                        idle_anim();
                    else
                        anim = null;
                }
            }
        }

        public void update_flash()
        {
            if (flash)
                Flash_Timer--;
            if (Spell_Effect != null)
                Spell_Effect.update_flash(ref Flash_Timer);
        }

        public override void update()
        {
#if DEBUG
            bool color_cycle = false;
            //if (Battle_Start && (Global.Input.pressed(Inputs.A) || Global.Input.triggered(Inputs.X)))
            //    color_cycle = true;
            if (Death_Ary.Count > 0 && Blend_Mode == 1 && Global.Input.pressed(Inputs.A))
                color_cycle = true;
            if (color_cycle) //Debug
            {
                // If skipping outline, j is the index of the outline color
                int j = -1;
                if (false) // if skip outline
                {
                    j = 0;
                    for (; j < 16; j++)
                    {
                        if (Palette[j].R == 40 && Palette[j].G == 40 && Palette[j].B == 40)
                            break;
                    }
                }
                int k = (j == 0 ? 1 : 0);
                Color temp = Palette[k];
                for (int i = k + 1; i < 16; i++)
                {
                    if (i != j)
                    {
                        Palette[k] = Palette[i];
                        k = i;
                    }
                }
                Palette[k] = temp;
            }
#endif
            // Update various animations
            if (Battle_Start || Idle)
            {
                update_animation(ref Animation);
                update_animation(ref Skill_Effect);
                update_animation(ref Anima_Effect);
                update_animation(ref Spell_Effect);
                update_animation(ref Spell_Effect_2);
                update_animation(ref Spell_Effect_3);
                update_animation(ref Spell_Effect_4);
            }
            // Fade on death
            if (Death_Ary.Count > 0)
            {
                int opacity = Death_Ary.pop();
                if (opacity == 256)
                {
                    Blend_Mode = 1;
                    set_opacity(255);
                }
                else
                    set_opacity(opacity);
                if (Death_Ary.Count >= 6 && Death_Ary[Death_Ary.Count - 6] == 256)
                    Global.Audio.play_se("Battle Sounds", "Death");
            }
            // Update no damage shake
            if (NoDamageIndex > -1)
            {
                if (NoDamageIndex >= NO_DAMAGE_JITTER.Length)
                {
                    draw_offset.X = 0;
                    NoDamageIndex = -1;
                }
                else
                {
                    draw_offset.X = NO_DAMAGE_JITTER[NoDamageIndex] * (Reverse ? -1 : 1);
                    NoDamageIndex++;
                }
            }

            update_animation_position();
            // Update temporary animations
            update_status();
            update_temp_anim();
            update_color_shift();
            update_promotion();
        }

        protected void set_opacity(int opacity)
        {
            if (Blend_Mode == 0)
                tint = new Color(opacity, opacity, opacity, opacity);
            else if (Blend_Mode == 1)
                tint = new Color(opacity, opacity, opacity, 0);
            if (Animation != null)
            {
                Animation.blend_mode = Blend_Mode;
                Animation.opacity = opacity;
            }
        }

        public void end_grey_out()
        {
            Greyed_Out = false;
        }

        protected override Vector2 stereo_offset()
        {
            return Stereoscopic_Graphic_Object.graphic_draw_offset(battler_stereo_offset());
        }

        public float battler_stereo_offset()
        {
            // Uses battler depth at 1x scale and map zoom when transitioning to map
            float scale_percent =
                (scale.X - Constants.BattleScene.BATTLER_MIN_SCALE) /
                (1 - Constants.BattleScene.BATTLER_MIN_SCALE);
            float stereo = Stereo_Offset.ValueOrDefault;
            return stereo * scale_percent + (1 - scale_percent) * Config.MAP_UNITS_DEPTH;
        }

        #region Draw
        public void draw(SpriteBatch sprite_batch, Vector2 draw_offset, Effect effect)
        {
            if (texture != null && (Global.scene.scene_type == "Scene_Promotion" || Animation == null))
                if (visible)
                {
                    set_effect_data(effect, Greyed_Out, Whiten_Duration > 0 ? Color.White : Color_Shift);
                    set_effect_palette(sprite_batch, effect);
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect);

                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(texture, loc + draw_vector() - draw_offset,
                        src_rect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);

                    sprite_batch.End();
#if __ANDROID__
                    // There has to be a way to do this for both
                    if (effect != null)
                        effect.Parameters["Palette"].SetValue((Texture2D)null);
#else
                    sprite_batch.GraphicsDevice.Textures[2] = null;
#endif
                }
        }
        
        public IEnumerable<BattlerRenderCommands> draw(SpriteBatch sprite_batch, Effect effect, IEnumerable<BattleFrameRenderData> renders)
        {
            if (Animation != null && visible)
            {
                foreach (BattleFrameRenderData render in renders)
                {
                    switch (render.blend_mode)
                    {
                        // Distortion
                        case 2:
                            yield return BattlerRenderCommands.DistortionDraw;
                            Vector2 distortion_flip = new Vector2(
                                render.flip.HasEnumFlag(SpriteEffects.FlipHorizontally) ? -1 : 1,
                                render.flip.HasEnumFlag(SpriteEffects.FlipVertically) ? -1 : 1);
                            effect.CurrentTechnique = effect.Techniques["DistortionFlip"];
                            effect.Parameters["distortion_flip"].SetValue(distortion_flip);

                            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect);
                            render.draw(sprite_batch);
                            sprite_batch.End();
                            break;
                        default:
                            yield return BattlerRenderCommands.NormalDraw;
                            set_effect_data(effect, Greyed_Out, Whiten_Duration > 0 ? Color.White : Color_Shift);
                            if (Has_Palette)
                                set_effect_palette(sprite_batch, effect);

                            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect);
                            render.draw(sprite_batch);
                            sprite_batch.End();
                            break;
                    }
                }

#if __ANDROID__
                    // There has to be a way to do this for both
                    if (effect != null)
                        effect.Parameters["Palette"].SetValue((Texture2D)null);
#else
                sprite_batch.GraphicsDevice.Textures[2] = null;
#endif
            }
        }
        public IEnumerable<BattlerRenderCommands> draw_lower(SpriteBatch sprite_batch, Vector2 draw_offset, Effect effect)
        {
            var lower_renders = Animation != null ?
                Animation.draw_lower(sprite_batch, draw_offset - draw_vector(), scale) : null;
            return draw(sprite_batch, effect, lower_renders);
        }
        public IEnumerable<BattlerRenderCommands> draw_upper(SpriteBatch sprite_batch, Vector2 draw_offset, Effect effect)
        {
            var upper_renders = Animation != null ?
                Animation.draw_upper(sprite_batch, draw_offset - draw_vector(), scale) : null;
            return draw(sprite_batch, effect, upper_renders);
        }

        protected void set_effect_data(Effect effect, bool greyed_out, Color color_shift)
        {
            if (effect != null)
            {
                if (greyed_out)
                {
                    effect.CurrentTechnique = effect.Techniques[!Has_Palette ? "Tone" : "Palette2"];
                    effect.Parameters["tone"].SetValue(new Tone(0, 0, 0, 255).to_vector_4());
                    effect.Parameters["opacity"].SetValue(1f);
                }
                else
                {
                    effect.CurrentTechnique = effect.Techniques[!Has_Palette ? "Technique1" : "Palette1"];
                    effect.Parameters["color_shift"].SetValue(color_shift.ToVector4());
                    effect.Parameters["opacity"].SetValue(1f);
                }
            }
        }

        protected void set_effect_palette(SpriteBatch sprite_batch, Effect effect)
        {
            set_effect_palette(sprite_batch, effect, Palette);
        }
        protected void set_effect_palette(SpriteBatch sprite_batch, Effect effect, Battle_Animation animation)
        {
            set_effect_palette(sprite_batch, effect, animation.palette);
        }
        protected void set_effect_palette(SpriteBatch sprite_batch, Effect effect, Color[] palette)
        {
            if (effect != null)
            {
                Texture2D palette_texture = Global.palette_pool.get_palette();
                palette_texture.SetData<Color>(palette);
#if __ANDROID__
                // There has to be a way to do this for both
                effect.Parameters["Palette"].SetValue(palette_texture);
#else
                sprite_batch.GraphicsDevice.Textures[2] = palette_texture;
#endif
                sprite_batch.GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
            }
        }

        protected void begin_subanim_sprite_batch(SpriteBatch sprite_batch, Effect effect, Battle_Animation animation)
        {
            if (animation.palette_used)
            {
                set_effect_data(effect, Greyed_Out, Color_Shift);
                set_effect_palette(sprite_batch, effect, animation);
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect);
            }
            else
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        }

        private void draw_subanim(Battle_Animation anim, SpriteBatch sprite_batch, Effect effect, Vector2 draw_offset)
        {
            if (anim != null)
            {
                begin_subanim_sprite_batch(sprite_batch, effect, anim);
                foreach (BattleFrameRenderData render in anim.draw_lower(sprite_batch, draw_offset))
                {
                    render.draw(sprite_batch);
                }
                foreach (BattleFrameRenderData render in anim.draw_upper(sprite_batch, draw_offset))
                {
                    render.draw(sprite_batch);
                }
                sprite_batch.End();
            }
        }

        // Spell layer effects
        public void draw_upper_effects(SpriteBatch sprite_batch, Effect effect)
        {
            draw_upper_effects(sprite_batch, effect, Vector2.Zero, Vector2.Zero);
        }
        public void draw_upper_effects(SpriteBatch sprite_batch, Effect effect, Vector2 draw_offset, Vector2 arena_offset)
        {
            foreach (Battle_Animation anim in Temp_Anims)
            {
                begin_subanim_sprite_batch(sprite_batch, effect, anim);
                foreach (BattleFrameRenderData render in anim.draw_upper(sprite_batch, draw_offset))
                {
                    render.draw(sprite_batch);
                }
                sprite_batch.End();
            }

            draw_subanim(Skill_Effect, sprite_batch, effect, draw_offset);
            draw_subanim(Anima_Effect, sprite_batch, effect, draw_offset);
            draw_subanim(Spell_Effect, sprite_batch, effect, arena_offset);
        }

        // Spell effects behind battlers?
        public void draw_lower_effects(SpriteBatch sprite_batch, Effect effect)
        {
            draw_lower_effects(sprite_batch, effect, Vector2.Zero, Vector2.Zero);
        }
        public void draw_lower_effects(SpriteBatch sprite_batch, Effect effect, Vector2 draw_offset, Vector2 arena_offset)
        {
            foreach (Battle_Animation anim in Temp_Anims)
            {
                begin_subanim_sprite_batch(sprite_batch, effect, anim);
                foreach (BattleFrameRenderData render in anim.draw_lower(sprite_batch, draw_offset))
                {
                    render.draw(sprite_batch);
                }
                sprite_batch.End();
            }
            draw_subanim(Spell_Effect_2, sprite_batch, effect, arena_offset);
        }

        // Spell effects behind platform?
        public void draw_bg_effects(SpriteBatch sprite_batch, Effect effect)
        {
            draw_bg_effects(sprite_batch, effect, Vector2.Zero);
        }
        public void draw_bg_effects(SpriteBatch sprite_batch, Effect effect, Vector2 draw_offset)
        {
            draw_subanim(Skill_Effect, sprite_batch, effect, Vector2.Zero);
        }

        // Spell effects above HUD?
        public void draw_fg_effects(SpriteBatch sprite_batch, Effect effect)
        {
            draw_fg_effects(sprite_batch, effect, Vector2.Zero);
        }
        public void draw_fg_effects(SpriteBatch sprite_batch, Effect effect, Vector2 draw_offset)
        {
            draw_subanim(Spell_Effect_4, sprite_batch, effect, Vector2.Zero);
        }
        #endregion
    }
}
