using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;
using TactileStringExtension;
using TactileWeaponExtension;

namespace Tactile
{
    enum Class_Reel_Phases { Class_Name, Animation, Closing, Force_Close }
    enum Class_Reel_Battler_Actions { Idle, Start_Attack, End_Attack, Wait_For_Return, Next_Attack }
    class Scene_Class_Reel : Scene_Base
    {
        protected Class_Reel_Phases Phase = Class_Reel_Phases.Class_Name;
        protected int Action = 0, Timer = 0;
        private Class_Reel_Battler_Actions Battler_Action = Class_Reel_Battler_Actions.Idle;
        private int Battler_Timer = 0;
        private int Black_Bar_Timer = -1;
        private List<Reel_Classes> Data_Order;
        protected int Index = -1;
        protected int Attack_Count = 0;
        protected bool Closing = false;
        protected string Class_Name;
        protected Game_Unit Unit;
        protected int[] Stats;
        protected int Terrain_Tag;

        protected Battler_Sprite Battler;
        protected Sprite Background;
        protected Battle_Platform Platform;
        protected Title_Background Title_Back;
        protected Sprite Black_Screen, White_Screen, White_Flash, Title_Black, Class_Banner, Burst;
        protected Sprite Black_Bar1, Black_Bar2;
        protected Color Class_Banner_Color;
        protected List<Spiral_Letter> Letters = new List<Spiral_Letter>();
        protected Message_Box Message;
        protected Sprite Text_Bg;
        protected TextSprite Name, Name_Bg;
        protected Class_Reel_Stat_Bars[] Stat_Bars = new Class_Reel_Stat_Bars[7];
        protected TextSprite[] Stat_Labels = new TextSprite[7];
        protected TextSprite[] Stat_Values = new TextSprite[7];
        protected List<Icon_Sprite> Weapon_Icons = new List<Icon_Sprite>();

        #region Accessors
        protected Class_Reel_Data class_data { get { return Class_Reel_Data.DATA[Data_Order[Index]]; } }

        protected bool is_last_class { get { return Index >= Data_Order.Count - 1; } }

        public int terrain_tag { get { return Terrain_Tag; } }
        #endregion

        public Scene_Class_Reel()
        {
            initialize_base();
            Scene_Type = "Class_Reel";
            get_data_list();
            initialize_images();
        }

        private void get_data_list()
        {
            Data_Order = new List<Reel_Classes>();
            HashSet<Tuple<string, bool>> already_added = new HashSet<Tuple<string, bool>>();
            for(int i = 0; i < Class_Reel_Data.ORDER.Length; i++)
                if (!already_added.Contains(Class_Reel_Data.ORDER[i]))
                    if (valid_reel_data(already_added, i))
                    {
                        Data_Order.AddRange(Class_Reel_Data.CH_DATA[Class_Reel_Data.ORDER[i]]);
                        already_added.Add(Class_Reel_Data.ORDER[i]);
                    }
        }

        private static bool valid_reel_data(HashSet<Tuple<string, bool>> already_added, int i)
        {
            if (Class_Reel_Data.CH_DATA.ContainsKey(Class_Reel_Data.ORDER[i]))
            {
                if (string.IsNullOrEmpty(Class_Reel_Data.ORDER[i].Item1))
                    return true;
                return Class_Reel_Data.ORDER[i].Item2 ?
                    Global.progress.ChapterCompleted(Class_Reel_Data.ORDER[i].Item1) :
                    Global.progress.ChapterAvailable(Class_Reel_Data.ORDER[i].Item1);
            }
            return false;
        }

        protected void initialize_images()
        {
            // Background
            Background = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Pictures/Preparation_Background"));
            Background.mirrored = true;
            Background.loc = new Vector2(0, -80);
            Background.offset = new Vector2(Config.WINDOW_WIDTH, 0);
            Background.stereoscopic = Config.REEL_BG_DEPTH;
            // Platform
            Platform = new Battle_Platform(false);
            Platform.loc_1 = new Vector2(133 + 127 - 87, 88);
            Platform.loc_2 = new Vector2(133 + 127, 88);
            // Black_Screen
            Black_Screen = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 0);
            White_Screen = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            White_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            White_Screen.opacity = 0;
            White_Flash = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            White_Flash.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Title_Black = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            Title_Black.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Title_Black.tint = new Color(0, 0, 0, 0);
            Black_Bar1 = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            Black_Bar1.tint = new Color(0, 0, 0, 255);
            Black_Bar1.visible = false;
            Black_Bar2 = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            Black_Bar2.tint = new Color(0, 0, 0, 255);
            Black_Bar2.visible = false;
            // Class Banner
            Class_Banner = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Pictures/Class Reel Banner"));
            Class_Banner.loc = new Vector2(Config.WINDOW_WIDTH / 2, Config.WINDOW_HEIGHT / 2 + 24);
            Class_Banner.opacity = 0;
            Class_Banner.stereoscopic = Config.REEL_NAME_PLAQUE_DEPTH;
            // Text Background
            Text_Bg = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Class_Reel_Window"));
            Text_Bg.loc = new Vector2(80, 144);
            Text_Bg.stereoscopic = Config.REEL_TEXT_BOX_DEPTH;
            // Name
            Name = new TextSprite();
            Name.loc = new Vector2(248, 8);
            Name.SetFont(Config.REEL_FONT, Global.Content);
            Name_Bg = new TextSprite();
            Name_Bg.loc = new Vector2(248, 8);
            Name_Bg.draw_offset = new Vector2(2, 2);
            Name_Bg.SetFont(Config.REEL_FONT, Global.Content);
            Name_Bg.tint = new Color(0, 0, 0, 255);
            // Stat Bars
            for (int i = 0; i < Stat_Bars.Length; i++)
            {
                Stat_Bars[i] = new Class_Reel_Stat_Bars();
                Stat_Bars[i].loc = new Vector2(0, 16 + 16 * i);
                Stat_Bars[i].stereoscopic = Config.REEL_STATS_DEPTH;
            }
            // Stat Labels
            for (int i = 0; i < Stat_Bars.Length; i++)
            {
                Stat_Labels[i] = new TextSprite();
                Stat_Labels[i].loc = new Vector2(8, 8 + 16 * i);
                Stat_Labels[i].SetFont(Config.UI_FONT, Global.Content, "Yellow");
                Stat_Labels[i].stereoscopic = Config.REEL_STATS_DEPTH;
            }
            Stat_Labels[0].text = "HP";
            Stat_Labels[2].text = "Skl";
            Stat_Labels[3].text = "Spd";
            if (Class_Reel_Data.SKIP_LCK)
            {
                Stat_Labels[4].text = "Def";
                Stat_Labels[5].text = "Res";
                Stat_Labels[6].text = "Con";
            }
            else
            {
                Stat_Labels[4].text = "Lck";
                Stat_Labels[5].text = "Def";
                Stat_Labels[6].text = "Res";
            }
            // Stat Labels
            for (int i = 0; i < Stat_Values.Length; i++)
            {
                Stat_Values[i] = new RightAdjustedText();
                Stat_Values[i].loc = new Vector2(48, 8 + 16 * i);
                Stat_Values[i].SetFont(Config.UI_FONT, Global.Content, "White");
                Stat_Values[i].stereoscopic = Config.REEL_STATS_DEPTH;
            }
        }

        protected void add_letter()
        {
            Letters.Add(new Spiral_Letter(Class_Name[0],
                Unit.actor.class_name.Length - Class_Name.Length, Unit.actor.class_name.Length));
            Class_Name = Class_Name.substring(1, Class_Name.Length - 1);
            if (Letters.Count == 1)
            {
                int width = Font_Data.text_width(Unit.actor.class_name, Config.REEL_SPLASH_FONT);
                Letters[0].loc.X = Config.WINDOW_WIDTH / 2 - width / 2;
            }
            else
            {
                int x = Font_Data.text_width(Letters[Letters.Count - 2].text, Config.REEL_SPLASH_FONT);
                Letters[Letters.Count - 1].loc.X = Letters[Letters.Count - 2].loc.X + x;
            }
            Letters[Letters.Count - 1].loc.Y = Config.WINDOW_HEIGHT / 2 + 24;
            Letters[Letters.Count - 1].stereoscopic = Config.REEL_NAME_PLAQUE_DEPTH;
        }

        private void increment_index()
        {
            Index++;
            if (Index >= Data_Order.Count)
                Closing = true;
            else
            {
                Global.dispose_battle_textures();
                load_class(class_data);
                Black_Screen.TintA = 255;
                Title_Back = new Title_Background();
                Title_Back.stereoscopic = Config.TITLE_BG_DEPTH;
                Title_Black.TintA = 255;
                Class_Banner_Color = new Color(0, 0, 0, 80);
                Class_Banner.scale = Vector2.One;
                Class_Banner.visible = true;
                Class_Banner.offset = new Vector2(Class_Banner.texture.Width / 2, 92 - 8);
            }
        }

        protected void load_class(Class_Reel_Data data)
        {
            // Sets up actor
            Unit = Game_Unit.class_reel_unit();// new Game_Unit(); //Debug
            Unit.actor.name = data.Name;
            Unit.actor.gender = data.Gender;
            Unit.actor.class_id = data.Class_Id;
            Unit.actor.weapon_id = data.Weapon_Id;
            Unit.magic_attack = Unit.check_magic_attack(Global.GetWeapon(data.Weapon_Id), data.Distance);
            Stats = new int[7];
            for (int i = 0; i < 7; i++)
            {
                int j = i + (Class_Reel_Data.SKIP_LCK && i >= (int)Tactile.Stat_Labels.Lck ? 1 : 0);
                switch (data.Stat_Type)
                {
                    case Reel_Generic_Stats.Actor:
                        Game_Actor actor = Global.game_actors[data.Stats[0]];
                        if (j == 0)
                            Stats[0] = actor.hp;
                        else
                            Stats[i] = actor.stat(j);
                        break;
                    case Reel_Generic_Stats.Class:
                        Data_Class data_class = Global.data_classes[data.Class_Id];
                        Stats[i] = data_class.Generic_Stats[(int)Generic_Builds.Normal][0][j];
                        break;
                    case Reel_Generic_Stats.Listed:
                        Stats[i] = data.Stats[j];
                        break;
                }
            }
            Attack_Count = data.Num_Attacks;
            // Platform
            Terrain_Tag = data.Platform;
            string terrain_name = Global.data_terrains[Terrain_Tag].PlatformName;

            if (Global.content_exists(@"Graphics/Battlebacks/" + terrain_name + "-Melee"))
                Platform.platform_1 = Global.Content.Load<Texture2D>(@"Graphics/Battlebacks/" + terrain_name + "-Melee");
            else
                Platform.platform_1 = Global.Content.Load<Texture2D>(@"Graphics/Battlebacks/" + "Plains" + "-Melee");
            Platform.platform_2 = Platform.platform_1;
        }

        protected void create_battler()
        {
            var battlerData = new BattlerSpriteData(Unit); //@Debug: could probably roll this without a unit
            Battler = new Battler_Sprite(battlerData, true, class_data.Distance);
            Battler.loc = new Vector2(256, 176);
            Battler.offset.Y = 120;
            Battler.visible = true;
            Battler.initialize_animation();
            Battler.reset_pose();
            Battler.start_battle();
            Battler.stereoscopic = Config.BATTLE_BATTLERS_DEPTH;
        }

        protected void background_clear(int timer)
        {
            int height = (Config.WINDOW_HEIGHT * (20 - timer) / 20) / 2;
            Black_Bar1.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, height);
            Black_Bar2.dest_rect = new Rectangle(0, Config.WINDOW_HEIGHT - height, Config.WINDOW_WIDTH, height);
        }

        protected void set_stats()
        {
            // Description
            Message = new Message_Box(88, 144, 224, 2, false, "White");
            Message.text_speed = Constants.Message_Speeds.Slow;
            Message.silence();
            Message.block_skip();
            Message.stereoscopic = Config.REEL_TEXT_BOX_DEPTH;
            // Name
            Name.text = Unit.actor.class_name;
            Name.offset.X = Name.text_width;
            Name.stereoscopic = Config.REEL_CLASS_NAME_DEPTH;
            Name_Bg.text = Name.text;
            Name_Bg.offset = Name.offset;
            Name_Bg.stereoscopic = Config.REEL_CLASS_NAME_SHADOW_DEPTH;
            // Stat Bars
            for (int i = 0; i < Stat_Bars.Length; i++)
            {
                int j = i + (Class_Reel_Data.SKIP_LCK && i >= (int)Tactile.Stat_Labels.Lck ? 1 : 0);
                Stat_Bars[i].cap = Unit.actor.get_cap(j);
                Stat_Bars[i].stat = Stats[i];
                Stat_Values[i].text = Stats[i].ToString();
            }
            switch (Unit.actor.power_type())
            {
                case Power_Types.Strength:
                    Stat_Labels[1].text = "Str";
                    break;
                case Power_Types.Magic:
                    Stat_Labels[1].text = "Mag";
                    break;
                case Power_Types.Power:
                    Stat_Labels[1].text = "Pow";
                    break;
            }
            // Weapon types
            Weapon_Icons.Clear();
            List<int> weapon_types = Unit.actor.weapon_types();
            // Compacts Anima types into one icon
            for (int i = 0; i < weapon_types.Count; i++)
                if (Data_Weapon.ANIMA_TYPES.Contains(weapon_types[i]))
                    weapon_types[i] = Data_Weapon.ANIMA_TYPES.Min();
                else if (weapon_types[i] > Data_Weapon.ANIMA_TYPES.Min())
                    weapon_types[i] -= (Data_Weapon.ANIMA_TYPES.Length - 1);

            weapon_types = weapon_types.Distinct().ToList(); //ListOrEquals
            for (int i = 0; i < weapon_types.Count; i++)
            {
                Weapon_Icons.Add(new Icon_Sprite());
                Weapon_Icons[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Class Reel Weapons");
                Weapon_Icons[i].size = new Vector2(32, 32);
                Weapon_Icons[i].columns = 8;
                Weapon_Icons[i].loc = new Vector2(8, 124) + new Vector2(32 * (i % 2), 32 * (i / 2));
                Weapon_Icons[i].index = weapon_types[i] - 1;
                Weapon_Icons[i].stereoscopic = Config.REEL_WEAPON_ICONS_DEPTH;
            }
        }

        #region Update
        public override void update()
        {
            if (Title_Back != null)
            {
                Title_Back.update();
                foreach (Spiral_Letter letter in Letters)
                    letter.update();
            }
            if (Battler != null)
                Battler.update();
            if (Message != null)
                Message.update();
            if ((Global.Input.triggered(Inputs.A) ||
                    Global.Input.mouse_triggered(MouseButtons.Left) ||
                    Global.Input.gesture_triggered(TouchGestures.SwipeLeft)) &&
                    (Action > 0 || Timer > 0))
                switch (Phase)
                {
                    case Class_Reel_Phases.Class_Name:
                        if (Index < Data_Order.Count)
                        {
                            change_phase(Class_Reel_Phases.Animation);
                            Timer = -16;
                        }
                        break;
                    case Class_Reel_Phases.Animation:
                        change_phase(Class_Reel_Phases.Force_Close);
                        Action = 1;
                        break;
                }
            else
                switch (Phase)
                {
                    case Class_Reel_Phases.Class_Name:
                        update_class_name();
                        break;
                    case Class_Reel_Phases.Animation:
                        update_animation();
                        break;
                    case Class_Reel_Phases.Closing:
                    case Class_Reel_Phases.Force_Close:
                        update_closing();
                        break;
                }
            if (Black_Bar_Timer >= 0)
            {
                Black_Bar_Timer++;
                background_clear(Black_Bar_Timer);
                if (Black_Bar_Timer == 20)
                    Black_Bar_Timer = -1;
            }

            White_Flash.visible = false;
            if (Battler != null)
            {
                update_battler_animation();
                Battler.update_flash();
                if (Battler.flash)
                    White_Flash.visible = true;
            }
            if (Global.Input.triggered(Inputs.B) ||
                Global.Input.triggered(Inputs.Start) ||
                Global.Input.mouse_triggered(MouseButtons.Right) ||
                Global.Input.gesture_triggered(TouchGestures.Tap))
            {
                Global.scene_change("Scene_Title");
                Global.Audio.StopBgm();
            }
        }

        protected void update_class_name()
        {
            switch (Action)
            {
                // Load next class
                case 0:
                    switch (Timer)
                    {
                        case 0:
                            increment_index();
                            Timer++;
                            break;
                        case 60:
                            if (Closing)
                                Global.scene_change("Scene_Title_Load");
                            else
                            {
                                Timer = 0;
                                Action++;
                                Class_Name = "" + Unit.actor.class_name;
                            }
                            break;
                        default:
                            if (Timer >= 10 && Timer < 42)
                            {
                                Title_Black.TintA = (byte)(256 - (Timer - 9) * 5);
                                Class_Banner.opacity += 8;
                                Class_Banner.offset.Y += Timer % 4 == 0 ? 1 : 0;
                            }
                            Timer++;
                            break;
                    }
                    break;
                // Create letters
                case 1:
                    if (Timer == 0)
                    {
                        if (Class_Name.Length == 0)
                            Action++;
                        else
                        {
                            add_letter();
                            Timer++;
                            if (Timer >= Spiral_Letter.DELAY)
                                Timer = 0;
                        }
                    }
                    else
                    {
                        Timer++;
                        if (Timer >= Spiral_Letter.DELAY)
                            Timer = 0;
                    }
                    break;
                // Pause on name
                case 2:
                    if (Timer >= Spiral_Letter.FLASH_TIME && Timer < Spiral_Letter.FLASH_TIME + 8)
                    {
                        Class_Banner_Color.R = (byte)MathHelper.Clamp(Class_Banner_Color.R + 32, 0, 255);
                        Class_Banner_Color.G = (byte)MathHelper.Clamp(Class_Banner_Color.G + 32, 0, 255);
                        Class_Banner_Color.B = (byte)MathHelper.Clamp(Class_Banner_Color.B + 32, 0, 255);
                        Class_Banner_Color.A = (byte)MathHelper.Clamp(Class_Banner_Color.A + 22, 0, 255);
                    }
                    else if (Timer >= Spiral_Letter.FLASH_TIME + 16 && Timer < Spiral_Letter.FLASH_TIME + 24)
                    {
                        Class_Banner_Color.A = (byte)MathHelper.Clamp(Class_Banner_Color.A - 32, 0, 255);
                    }
                    else if (Timer >= Spiral_Letter.TOTAL_WAIT && Timer < Spiral_Letter.TOTAL_WAIT + 32)
                    {
                        int timer = Timer - Spiral_Letter.TOTAL_WAIT;
                        Class_Banner.scale = new Vector2(1 + 2 * ((1 + timer) / 32f), 1 - (float)Math.Sqrt(timer / 32f));
                        Class_Banner.opacity -= 8;
                    }
                    else if (Timer == Spiral_Letter.TOTAL_WAIT + 32)
                    {
                        Class_Banner.visible = false;
                        Timer = 0;
                        Action++;
                        return;
                    }
                    Timer++;
                    break;
                // White flash
                case 3:
                    switch (Timer)
                    {
                        case 47:
                            White_Screen.opacity = 255;
                            create_battler();
                            Burst = null;
                            Title_Back = null;
                            Letters.Clear();
                            Timer = 0;
                            Action++;
                            break;
                        default:
                            if (Timer == 0)
                            {

                                Burst = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Pictures/burst"));
                                Burst.loc = new Vector2(Config.WINDOW_WIDTH / 2, Config.WINDOW_HEIGHT / 2);
                                Burst.offset = new Vector2(Burst.texture.Width / 2, Burst.texture.Height / 2);
                                Burst.blend_mode = 1;
                                Burst.opacity = 0;
                                Burst.stereoscopic = Config.REEL_BURST_DEPTH;
                            }
                            Burst.opacity += 8;
                            // Resize/rotate
                            if (Timer % 3 == 0)
                            {
                                Burst.scale = Vector2.One * (Timer + 1) / 100f * ((Timer + 1) / 48f + 1);
                                Burst.angle = -(Timer + 1) * 4 / 360f * MathHelper.Pi;
                            }
                            // White screen
                            if (Timer >= 16)
                            {
                                White_Screen.opacity = (Timer - 15) * 8;
                            }
                            Timer++;
                            break;
                    }
                    break;
                // Fade out for next phase
                case 4:
                    switch (Timer)
                    {
                        case 47:
                            change_phase(Class_Reel_Phases.Animation);
                            break;
                        default:
                            if (Timer >= 16 && Timer <= 31)
                                White_Screen.opacity = (31 - Timer) * 16;
                            Timer++;
                            break;
                    }
                    break;
            }
        }
        protected void update_animation()
        {
            switch (Action)
            {
                // Load battler, fade in screen
                case 0:
                    switch (Timer)
                    {
                        case 0:
                            set_stats();
                            Message.set_text("" + class_data.Description);
                            Timer++;
                            Black_Bar_Timer = 1;
                            Black_Bar1.visible = true;
                            Black_Bar2.visible = true;
                            Black_Screen.visible = false;
                            break;
                        case 51:
                            Timer = 0;
                            Action++;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                // Starts attacks
                case 1:
                    if (Battler_Action == Class_Reel_Battler_Actions.Idle)
                    {
                        if (Attack_Count > 0)
                        {
                            Attack_Count--;
                            Battler.attack(false, class_data.Distance);
                            Battler_Action = Class_Reel_Battler_Actions.Start_Attack;
                        }
                        else
                        {
                            change_phase(Class_Reel_Phases.Closing);
                        }
                    }
                    break;
            }
        }
        protected void update_closing()
        {
            // Wait for text, close class
            switch (Action)
            {
                case 0:
                    switch (Timer)
                    {
                        case 0:
                            if (Message.text_end && !Message.wait)
                                Timer++;
                            break;
                        case 59:
                            Timer = 0;
                            Action = 1;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                case 1:
                    int wait_time = is_last_class ? 60 : 16;
                    if (Timer == 0)
                    {
                        if (is_last_class)
                            Global.Audio.BgmFadeOut(120);
                        Black_Screen.visible = true;
                        Timer++;
                    }
                    else if (Timer == wait_time)
                    {
                        change_phase(Class_Reel_Phases.Class_Name);
                        Black_Bar1.visible = false;
                        Black_Bar2.visible = false;
                        return;
                    }
                    else
                        Timer++;
                    Black_Screen.TintA = (byte)MathHelper.Clamp((256 * Timer) / wait_time, 0, 255);
                    break;
            }
        }

        private void change_phase(Class_Reel_Phases phase)
        {
            Timer = 0;
            Action = 0;
            switch (phase)
            {
                case Class_Reel_Phases.Animation:
                    White_Screen.opacity = 0;
                    if (Battler == null)
                        create_battler();
                    Burst = null;
                    Title_Back = null;
                    Letters.Clear();
                    Class_Banner.opacity = 0;
                    Class_Banner.visible = false;
                    break;
                case Class_Reel_Phases.Class_Name:
                    Battler = null;
                    Battler_Action = Class_Reel_Battler_Actions.Idle;
                    Battler_Timer = 0;
                    Message = null;
                    Black_Screen.visible = true;
                    Black_Bar1.visible = false;
                    Black_Bar2.visible = false;
                    Black_Bar_Timer = -1;
                    if (Phase == Class_Reel_Phases.Force_Close)
                    {
                        increment_index();
                        Timer = 10;
                    }
                    break;
                case Class_Reel_Phases.Closing:
                case Class_Reel_Phases.Force_Close:
                    Action = 0;
                    break;
            }
            Phase = phase;
        }

        private void update_battler_animation()
        {
            switch (Battler_Action)
            {
                // Starts attack
                case Class_Reel_Battler_Actions.Start_Attack:
                    switch (Battler_Timer)
                    {
                        // Wait for attack
                        case 0:
                            if (Battler.duration <= 1)
                            {
                                Battler.hit_freeze(false, class_data.Distance);
                                if (Unit.spell_animation())
                                {
                                    Data_Weapon weapon = Unit.actor.weapon;
                                    if (weapon.has_anima_start())
                                    {
                                        Battler.anima_start(1);
                                        Battler_Timer = 1;
                                    }
                                    else
                                    {
                                        Battler.attack_spell(true, false, class_data.Distance);
                                        Battler_Timer = 2;
                                    }
                                }
                                else
                                    Battler_Timer = 2;
                            }
                            break;
                        // Wait for anima startup
                        case 1:
                            if (Battler.anima_ready)
                            {
                                Battler.attack_spell(true, false, class_data.Distance);
                                Battler_Timer = 2;
                            }
                            break;
                        // Wait for hit
                        case 2:
                            if (Unit.spell_animation() ? Battler.spell_ready : Battler.duration <= 1)
                            {
                                if (Unit.spell_animation())
                                    Battler.end_spell(true, false, 1);
                                Battler_Timer = 0;
                                Battler_Action = Class_Reel_Battler_Actions.End_Attack;
                            }
                            break;
                    }
                    break;
                // Ends attack
                case Class_Reel_Battler_Actions.End_Attack:
                    if (Battler_Timer >= class_data.Wait_Time[0] + 30)
                    {
                        if (Battler.spell_ready)
                        {
                            Battler.return_anim(false, class_data.Distance);
                            Battler_Timer = 0;
                            Battler_Action = Class_Reel_Battler_Actions.Wait_For_Return;
                        }
                    }
                    else
                        Battler_Timer++;
                    break;
                // Waits for return
                case Class_Reel_Battler_Actions.Wait_For_Return:
                    if (Battler.duration <= 4)
                        Battler_Action = Class_Reel_Battler_Actions.Next_Attack;
                    break;
                // Next attack
                case Class_Reel_Battler_Actions.Next_Attack:
                    if (Battler_Timer >= class_data.Wait_Time[1])
                    {
                        Battler_Timer = 0;
                        Battler_Action = Class_Reel_Battler_Actions.Idle;
                    }
                    else
                        Battler_Timer++;
                    break;
            }
        }
        #endregion

        public override void draw(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            device.SetRenderTarget(render_targets[0]);
            device.Clear(Color.Transparent);

            if (Title_Back == null)
            {
                draw_animation_scene(sprite_batch, device, render_targets[0], render_targets[2], render_targets[3]);
            }
            else
            {
                draw_banner_scene(sprite_batch);
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            White_Screen.draw(sprite_batch);
            sprite_batch.End();

            base.draw(sprite_batch, device, render_targets);
        }

        protected void draw_animation_scene(SpriteBatch sprite_batch, GraphicsDevice device,
            RenderTarget2D final_render, RenderTarget2D temp_render, RenderTarget2D effect_render)
        {
            Effect reel_shader = Global.effect_shader();
            // Background
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Background.draw(sprite_batch);
            sprite_batch.End();
            // Spell Bg
            if (Battler != null)
            {
                Battler.draw_bg_effects(sprite_batch, reel_shader);
            }
            // Platform
            Platform.draw(sprite_batch);
            // Battler

            if (Battler != null)
            {
                BattleSpriteRenderer battler_renderer = new BattleSpriteRenderer(
                    false, Vector2.Zero, Vector2.Zero, Vector2.Zero);
                battler_renderer.draw(sprite_batch, device,
                    new Tuple<Battler_Sprite, bool>(Battler, false), null,
                    final_render, temp_render, effect_render);
            }

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            /*if (Battler != null) //Debug
            {
                sprite_batch.End();
                Battler.draw_lower_effects(sprite_batch, reel_shader);
                Battler.draw_lower(sprite_batch, Vector2.Zero, reel_shader);
                Battler.draw(sprite_batch, Vector2.Zero, reel_shader);
                Battler.draw_upper(sprite_batch, Vector2.Zero, reel_shader);
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            }*/
            // Stats
            if (Message != null)
            {
                foreach (Class_Reel_Stat_Bars bar in Stat_Bars)
                    bar.draw(sprite_batch);
                foreach (TextSprite label in Stat_Labels)
                    label.draw(sprite_batch);
                foreach (TextSprite value in Stat_Values)
                    value.draw(sprite_batch);
                foreach (Icon_Sprite icon in Weapon_Icons)
                    icon.draw(sprite_batch);
                Name_Bg.draw(sprite_batch);
            }
            // Battler
            if (Battler != null)
            {
                sprite_batch.End();
                Battler.draw_upper_effects(sprite_batch, reel_shader);
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            }
            // Name/Message
            if (Message != null)
            {
                Text_Bg.draw(sprite_batch);
                Name.draw(sprite_batch);
                sprite_batch.End();
                // Message
                Message.draw_background(sprite_batch);
                Message.draw_faces(sprite_batch);
                Message.draw_foreground(sprite_batch);

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            }
            // Spell Fg
            if (Battler != null)
            {
                sprite_batch.End();
                Battler.draw_fg_effects(sprite_batch, reel_shader);
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                White_Flash.draw(sprite_batch);
            }
            // Black Screen
            Black_Screen.draw(sprite_batch);
            Black_Bar1.draw(sprite_batch);
            Black_Bar2.draw(sprite_batch);
            sprite_batch.End();
        }

        protected void draw_banner_scene(SpriteBatch sprite_batch)
        {
            // Title Screen Background
            Title_Back.draw(sprite_batch);
            // Title Background Darken
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Title_Black.draw(sprite_batch);
            sprite_batch.End();
            // Class Banner
            Effect banner_shader = Global.effect_shader();
            if (banner_shader != null)
            {
                banner_shader.CurrentTechnique = banner_shader.Techniques["Technique2"];
                banner_shader.Parameters["color_shift"].SetValue(Class_Banner_Color.ToVector4());
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, banner_shader);
            Class_Banner.draw(sprite_batch);
            sprite_batch.End();
            // Letters
            if (Letters.Count > 0)
            {
                if (banner_shader != null)
                {
                    banner_shader.CurrentTechnique = banner_shader.Techniques["Technique2"];
                    banner_shader.Parameters["color_shift"].SetValue(Letters[0].flash.ToVector4());
                }
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.PointClamp, null, null, banner_shader);
                foreach (Spiral_Letter letter in Letters)
                    letter.draw(sprite_batch);
                sprite_batch.End();
            }
            if (Burst != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null);
                Burst.draw(sprite_batch);
                sprite_batch.End();
            }
        }
    }
}
