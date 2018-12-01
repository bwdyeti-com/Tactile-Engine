using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Calculations.LevelUp;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    internal class Window_LevelUp : Sprite
    {
        const int SKIP_WAIT_TIME = 60;
        static Vector2 LOC = new Vector2(8, 80);
        const int COLUMN_HEIGHT = 4;

        protected int Actor_Id;
        protected bool Execute = false;
        protected bool Skipping = false;
        protected int Skip_Wait_Time = 0;
        protected int Glow_Timer = 0;
        protected List<int> Window_Move_Array = new List<int>(), Header_Move_Array = new List<int>(), Face_Move_Array = new List<int>();
        protected int Timer = 0;
        protected bool OnLevelGain = true;
        protected int ActiveStat = -1;
        protected int Stat_Gain;
        protected LevelUpProcessor LevelUp;
        protected Vector2 Header_Loc, Header_Offset, Header_Scale;
        protected Face_Sprite Face_Img;
        private FE_Text Class_Name, Lv_Label;
        private FE_Text_Int Level;
        private List<FE_Text> Stat_Labels = new List<FE_Text>();
        private List<FE_Text_Int> Stats = new List<FE_Text_Int>();
        private List<Spark> Swirls = new List<Spark>(), Arrows = new List<Spark>(), Bars = new List<Spark>();
        private List<Stat_Up_Num> Stat_Gains = new List<Stat_Up_Num>();
        protected ContentManager Content;
        protected Texture2D Blue_Text_Texture, Green_Text_Texture;
        protected Texture2D Arrow_Texture, Bar_Texture;
        protected List<Texture2D> Stat_Gain_Textures = new List<Texture2D>();

        #region Accessors
        public bool execute
        {
            get { return Execute; }
            set { Execute = value; }
        }

        public bool skipping
        {
            get { return Skipping; }
        }

        protected Game_Unit unit { get { return Global.game_map.get_unit_from_actor(Actor_Id); } }

        protected Game_Actor actor { get { return Global.game_actors[Actor_Id]; } }
        #endregion

        public Window_LevelUp()
        {
            stereoscopic = Config.BATTLE_LEVEL_UP_DEPTH;
        }
        public Window_LevelUp(ContentManager content, int id)
        {
            Content = content;
            Actor_Id = id;
            initialize();
            stereoscopic = Config.BATTLE_LEVEL_UP_DEPTH;
        }

        protected void initialize()
        {
            List<Texture2D> textures = new List<Texture2D> {
                Content.Load<Texture2D>(@"Graphics/Windowskins/Level_Up_Window"),
                Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White"),
                Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow"),
                Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue"),
                Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Green")};
            texture = textures[0];
            Blue_Text_Texture = textures[3];
            Green_Text_Texture = textures[4];
            Arrow_Texture = Content.Load<Texture2D>(@"Graphics/Pictures/" + Stat_Change_Arrow.FILENAME);
            Bar_Texture = Content.Load<Texture2D>(@"Graphics/Pictures/" + Stat_Change_Bar.FILENAME);
            Stat_Gain_Textures.Add(Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Stat2"));
            Stat_Gain_Textures.Add(Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Stat1"));
            Header_Loc = LOC + new Vector2(-8 * 18, 11 - 3);
            Header_Offset = new Vector2(0, 11);
            Header_Scale = new Vector2(1, 1);
            loc = LOC + new Vector2(-8 * 18, 32);
            offset = new Vector2(3, 3);
            // Class Name
            Class_Name = new FE_Text();
            Class_Name.Font = "FE7_Text";
            Class_Name.texture = textures[1];
            Class_Name.offset = new Vector2(0, 8);
            // Lv Label
            Lv_Label = new FE_Text();
            Lv_Label.Font = "FE7_Text";
            Lv_Label.texture = textures[2];
            Lv_Label.text = "Lv";
            Lv_Label.offset = new Vector2(-(64 + 6), 8);
            // Level
            Level = new FE_Text_Int();
            Level.Font = "FE7_Text";
            Level.texture = textures[3];
            Level.offset = new Vector2(-(104), 8);
            // Stat Labels
            for (int i = 0; i < 8; i++)
            {
                Stat_Labels.Add(new FE_Text());
                Stat_Labels[i].Font = "FE7_Text";
                Stat_Labels[i].texture = textures[2];
                Stat_Labels[i].offset += new Vector2(
                    -(0 + (i / COLUMN_HEIGHT) * 64),
                    -((i % COLUMN_HEIGHT) * 16));
            }
            Stat_Labels[0].text = "HP";
            Stat_Labels[0].offset += new Vector2(-2, 0);
            if (actor.power_type() == Power_Types.Strength)
                Stat_Labels[1].text = "Str";
            if (actor.power_type() == Power_Types.Magic)
                Stat_Labels[1].text = "Mag";
            if (actor.power_type() == Power_Types.Power)
                Stat_Labels[1].text = "Pow";
            Stat_Labels[1].offset += new Vector2(-1, 0);
            Stat_Labels[2].text = "Skill";
            Stat_Labels[2].offset += new Vector2(1, 0);
            Stat_Labels[3].text = "Spd";
            Stat_Labels[4].text = "Luck";
            Stat_Labels[4].offset += new Vector2(2, 0);
            Stat_Labels[5].text = "Def";
            Stat_Labels[6].text = "Res";
            Stat_Labels[7].text = "Con";
            // Stats
            for (int i = 0; i < 8; i++)
            {
                Stats.Add(new FE_Text_Int());
                Stats[i].Font = "FE7_Text";
                Stats[i].texture = textures[3];
                Stats[i].offset += new Vector2(
                    -(48 + (i / COLUMN_HEIGHT) * 64),
                    -((i % COLUMN_HEIGHT) * 16));
            }
            // Face
            Face_Img = new Face_Sprite(actor.face_name, true);
            if (actor.generic_face)
                Face_Img.recolor_country(actor.name_full);
            Face_Img.expression = 1;
            Face_Img.phase_in();
            Face_Img.loc = new Vector2(Config.WINDOW_WIDTH - 80, Config.WINDOW_HEIGHT + (8 * 12));

            refresh();
        }

        public void move_on()
        {
            Window_Move_Array = new List<int> { 0, 0, 0, 0, 18, 18, 18, 18, 18, 18, 18, 18 };
            Header_Move_Array = new List<int> { 0, 0, 18, 18, 18, 18, 18, 18, 18, 18 };
            Face_Move_Array = new List<int> { -12, -12, -12, -12, -12, -12, -12, -12 };
        }

        public void move_off()
        {
            Window_Move_Array = new List<int> { 0, 0, 0, -18, -18, -18, -18, -18, -18, -18, -18 };
            Header_Move_Array = new List<int> { 0, 0, 0, -18, -18, -18, -18, -18, -18, -18, -18 };
            Face_Move_Array = new List<int> { 0, 0, 0, 12, 12, 12, 12, 12, 12, 12, 12 };
            Arrows.Clear();
            Bars.Clear();
            Stat_Gains.Clear();
        }

        public bool is_ready()
        {
            bool ready = !OnLevelGain && ActiveStat == -1 && Swirls.Count == 0;
            if (ready)
            {
                if (Skip_Wait_Time <= 0)
                    return true;
                Skip_Wait_Time--;
            }
            return false;
        }

        public void finish()
        {
            //actor.needed_levels = 0; //Debug
        }

        #region Update
        protected void refresh()
        {
            set_class_name();
            Level.text = actor.level.ToString();
            for (int i = 0; i < 8; i++)
            {
                Stats[i].text = stat_value(i).ToString();
                Stats[i].texture = actor.get_capped(i) ? Green_Text_Texture : Blue_Text_Texture;

                if (i < Game_Actor.LEVEL_UP_VIABLE_STATS)
                    refresh_stat_label_color(i);
            }
        }

        protected virtual void refresh_stat_label_color(int i)
        {
            if (Constants.Actor.STAT_LABEL_COLORING != Constants.StatLabelColoring.None &&
                (unit.average_stat_hue_shown))
            {
                float stat_quality = actor.stat_quality(i, actor.needed_levels);
                if (actor.get_capped(i))
                    stat_quality = Math.Max(0, stat_quality);
                Stat_Labels[i].tint = new Color(
                    255 - (int)MathHelper.Clamp((stat_quality * 1.25f * 255), 0, 255),
                    (int)MathHelper.Clamp(255 + (stat_quality * 255), 0, 255), 255);
            }
        }

        protected virtual int stat_value(int i)
        {
            return actor.stat(i);
        }

        protected virtual void gain_stat(Stat_Labels stat)
        {
            LevelUp.Apply(stat);
        }

        protected virtual void set_class_name()
        {
            set_class_name(actor.class_name);
        }
        protected virtual void set_class_name(string name)
        {
            Class_Name.text = name;
        }

        public override void update()
        {
            Glow_Timer++;
            Face_Img.update();
            update_skip();
            bool stat_sound = false;
            bool cont = false;
            while (!cont)
            {
                cont = true;
                cont = !(Skipping && Execute);
                if (Execute)
                {
                    stat_sound |= update_stat();
                }
            }
            // Update sparks
            int id = 0;
            while (id < Swirls.Count)
            {
                Swirls[id].update();
                if (Swirls[id].completed())
                    Swirls.RemoveAt(id);
                else
                    id++;
            }
            foreach (Spark effect in Arrows)
            {
                ((Stat_Change_Arrow)effect).update(Glow_Timer);
            }
            foreach (Spark effect in Bars)
            {
                ((Stat_Change_Bar)effect).update(Glow_Timer);
            }
            foreach (Stat_Up_Num effect in Stat_Gains)
            {
                effect.update(Glow_Timer);
            }
            // Update window offsets
            if (Window_Move_Array.Count > 0)
            {
                loc.X += Window_Move_Array[0];
                Window_Move_Array.RemoveAt(0);
            }
            if (Header_Move_Array.Count > 0)
            {
                Header_Loc.X += Header_Move_Array[0];
                Header_Move_Array.RemoveAt(0);
            }
            if (Face_Move_Array.Count > 0)
            {
                Face_Img.loc += new Vector2(0, Face_Move_Array[0]);
                Face_Move_Array.RemoveAt(0);
            }
            if (stat_sound)
                Global.game_system.play_se(System_Sounds.Level_Up_Stat);
        }

        protected virtual void update_skip()
        {
            if (!Skipping && (Global.Input.triggered(Inputs.A) ||
                Global.Input.any_mouse_triggered ||
                Global.Input.gesture_triggered(TouchGestures.Tap)))
            {
                Skipping = true;
                Skip_Wait_Time = SKIP_WAIT_TIME;
            }
        }

        protected virtual bool update_stat()
        {
            bool stat_sound = false;
            switch (Timer)
            {
                case 0:
                    if (OnLevelGain)
                    {
                        get_stats();
                        Global.Audio.play_se("System Sounds", "Level_Up_Level");
                    }
                    else
                    {
                        Stat_Gain = LevelUp.StatGain((Stat_Labels)ActiveStat);
                        gain_stat((Stat_Labels)ActiveStat);
                        if (Skipping)
                            stat_sound = true;
                        else
                            Global.game_system.play_se(System_Sounds.Level_Up_Stat);
                    }
                    refresh();
                    break;
                case 1:
                    Swirls.Add(new Stat_Up_Spark());
                    if (OnLevelGain)
                    {
                        Swirls[Swirls.Count - 1].loc = Header_Loc + new Vector2(80, -16) - Header_Offset + new Vector2(1, 1);
                    }
                    else
                    {
                        Vector2 stat_loc = new Vector2(
                            (ActiveStat / COLUMN_HEIGHT) * 64,
                            (ActiveStat % COLUMN_HEIGHT) * 16);

                        Swirls[Swirls.Count - 1].loc = loc + stat_loc +
                            new Vector2(16, -16) +
                            new Vector2(0, -2);
                        Arrows.Add(new Stat_Change_Arrow());
                        Arrows[Arrows.Count - 1].texture = Arrow_Texture;
                        Arrows[Arrows.Count - 1].loc = loc + stat_loc +
                            new Vector2(56, -8) +
                            new Vector2(-3, -2);
                        if (Stat_Gain > 0)
                        {
                            Bars.Add(new Stat_Change_Bar());
                            Bars[Bars.Count - 1].texture = Bar_Texture;
                            Bars[Bars.Count - 1].loc = loc + stat_loc +
                                new Vector2(12, 12) +
                                new Vector2(-2, -3);
                        }
                    }
                    // Effects
                    break;
                case 15:
                    if (!OnLevelGain)
                    {
                        Stat_Gains.Add(new Stat_Up_Num(Stat_Gain_Textures));
                        Stat_Gains[Stat_Gains.Count - 1].value = Stat_Gain;
                        Stat_Gains[Stat_Gains.Count - 1].loc = loc +
                            new Vector2(
                                64 + ((ActiveStat / COLUMN_HEIGHT) * 64),
                                14 + ((ActiveStat % COLUMN_HEIGHT) * 16)) +
                            new Vector2(-3, -3);
                    }
                    // Numbers
                    break;
                case 17:
                    // Large numbers appear here on promotion? //Yeti
                    break;
                case 20:
                    get_next_stat();
                    Timer = 0;
                    return false;
            }
            Timer++;
            return stat_sound;
        }

        protected virtual void get_stats()
        {
            LevelUp = actor.full_level_up();
        }

        protected void get_next_stat()
        {
            OnLevelGain = false;
            bool cancel = true;
            for (int i = ActiveStat + 1; i < LevelUp.StatCount; i++)
            {
                if (LevelUp.StatChanged((Stat_Labels)i))
                {
                    cancel = false;
                    ActiveStat = i;
                    break;
                }
            }
            if (cancel)
                ActiveStat = -1;
            Execute = !cancel;
        }
        #endregion

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Vector2 offset = this.offset;
                    // Face Sprite
                    Face_Img.draw(sprite_batch, -draw_vector());

                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    // Window
                    sprite_batch.Draw(texture, this.loc + draw_vector(),
                        new Rectangle(0, 32, 135, 71), tint, 0f, offset, 1f,
                        SpriteEffects.None, Z);
                    // Header
                    sprite_batch.Draw(texture, Header_Loc + draw_vector(),
                        new Rectangle(3, 0, 129, 23), tint, 0f, Header_Offset, Header_Scale,
                        SpriteEffects.None, Z);
                    // Bar effects
                    foreach (Spark effect in Bars)
                    {
                        effect.draw(sprite_batch, -draw_vector());
                    }
                    // Class Name
                    Class_Name.loc = Header_Loc + new Vector2(8, 0);
                    Class_Name.scale = Header_Scale;
                    Class_Name.draw(sprite_batch, -draw_vector());
                    // Lv Label
                    Lv_Label.loc = Header_Loc + new Vector2(8, 0);
                    Lv_Label.scale = Header_Scale;
                    Lv_Label.draw(sprite_batch, -draw_vector());
                    // Level
                    Level.loc = Header_Loc + new Vector2(8, 0);
                    Level.scale = Header_Scale;
                    Level.draw(sprite_batch, -draw_vector());
                    // Stat Labels
                    foreach (FE_Text label in Stat_Labels)
                    {
                        label.loc = loc + new Vector2(12, 0);
                        label.draw(sprite_batch, -draw_vector());
                    }
                    // Stat
                    foreach (FE_Text stat in Stats)
                    {
                        stat.loc = loc + new Vector2(0, 0);
                        stat.draw(sprite_batch, -draw_vector());
                    }
                    // Arrow effects
                    foreach (Spark effect in Arrows)
                        effect.draw(sprite_batch, -draw_vector());
                    // Stat Gain effects
                    foreach (Stat_Up_Num effect in Stat_Gains)
                        effect.draw(sprite_batch, -draw_vector());
                    // Swirl effects
                    foreach (Spark effect in Swirls)
                        effect.draw(sprite_batch, -draw_vector());
                    sprite_batch.End();
                }
        }
    }
}
