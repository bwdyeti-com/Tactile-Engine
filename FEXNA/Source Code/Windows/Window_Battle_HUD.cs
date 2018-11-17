using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using ListExtension;
using FEXNAWeaponExtension;

namespace FEXNA
{
    class Window_Battle_HUD : Sprite
    {
        readonly static Rectangle STAT_PANES = new Rectangle(0, 0, 43, 29);
        readonly static Rectangle HP_PANES = new Rectangle(0, 29, 160, 22);
        readonly static Rectangle FACE_PANES = new Rectangle(0, 117, 112, 32);
        readonly static Rectangle SKILL_PANES = new Rectangle(120, 117, 48, 12);
        readonly static Rectangle WEAPON_PANE = new Rectangle(0, 245, 117, 18);

        readonly static Rectangle CLASS_PANES = new Rectangle(160, 29, 84, 22);
        readonly static Rectangle NAME_PANES = new Rectangle(180, 29, 64, 22);

        readonly static Rectangle FACE_OVER_PANES = new Rectangle(0, 263, 112, 32); // Is this actually used? //Yeti
        readonly static Rectangle MESSAGE_PANES = new Rectangle(120, 197, 96, 20);

        protected Combat_Data Data;
        protected int Battler_1_Id, Battler_2_Id = -1;
        protected bool Reverse;
        protected int Distance;
        protected int Hp1 = 0, Hp2 = 0, Data_Hp1 = -1, Data_Hp2 = -1;
        protected int No_Hp1_Timer = 0, No_Hp2_Timer = 0;
        protected int No_Health_Timer = 0;
        protected bool No_Health_Flag = false;
        protected int Hp_Gain = 0;
        protected int Weapon_Flash_Timer = 0;
        protected int Hp1_Timer = 0, Hp2_Timer = 0;
        protected int Hit_Wait_Timer = 0;
        protected int Stat_Timer = 0;
        protected int Health_Loss = 0;
        protected List<HUD_Component> Components = new List<HUD_Component>(), Name_Components = new List<HUD_Component>();
        protected List<FE_Text> Names = new List<FE_Text>();
        protected List<Battle_Stats_BG> Stat_BGs = new List<Battle_Stats_BG>();
        protected int Attack_Id = 0, Action_Id = -1;
        protected List<int?> Stats = new List<int?>();
        protected List<FE_Text_Int> Stat_Imgs = new List<FE_Text_Int>();
        protected List<Item_Icon_Sprite> Weapon_Icons = new List<Item_Icon_Sprite>();
        protected List<Multiplier_Img> Mults = new List<Multiplier_Img>();
        protected List<Weapon_Triangle_Arrow> WTAs = new List<Weapon_Triangle_Arrow>();
        protected List<FE_Text> Weapon_Names = new List<FE_Text>();
        protected List<Battle_Face_Sprite> Faces = new List<Battle_Face_Sprite>();
        protected HP_Gauge HP_Gauge_1, HP_Gauge_2;
        protected HP_Counter HP_Count_1, HP_Count_2;
        protected Vector2 Base_Loc, Name_Loc = Vector2.Zero;
        protected List<int> Y_Move = new List<int>();
        protected Texture2D HP_Gauge_Texture, Combat_Text_Texture, Combat_Num_Texture;

        #region Accessors
        public Combat_Data combat_data { set { Data = value; } }

        protected Game_Unit battler_1
        {
            get
            {
                if (Data != null)
                    return Global.game_map.units[Data.Battler_1_Id];
                return Global.game_map.units[Battler_1_Id];
            }
        }
        protected Game_Unit battler_2
        {
            get
            {
                if (Data != null)
                {
                    if (Data.Battler_2_Id == null)
                        return null;
                    return Global.game_map.units[(int)Data.Battler_2_Id];
                }
                else
                {
                    if (Battler_2_Id != -1)
                        return Global.game_map.units[Battler_2_Id];
                    else
                        return null;
                }
            }
        }

        protected int data_hp1
        {
            get
            {
                if (Data != null)
                    return Data.Hp1;
                // The hp value is stored here the first time it's checked so that on promotion it doesn't update to the healed number
                if (Data_Hp1 == -1)
                    Data_Hp1 = battler_1.actor.hp;
                return Data_Hp1;
                //return battler_1.actor.hp; //Debug
            }
        }
        protected int data_hp2
        {
            get
            {
                if (Data != null)
                    return Data.Hp2;
                if (battler_2 != null)
                {
                    // The hp value is stored here the first time it's checked so that on promotion it doesn't update to the healed number
                    if (Data_Hp2 == -1)
                        Data_Hp2 = battler_2.actor.hp;
                    return Data_Hp2;
                    //return battler_2.actor.hp; //Debug
                }
                return 0;
            }
        }

        protected int data_maxhp1
        {
            get
            {
                if (Data != null)
                    return Data.MaxHp1;
                return battler_1.actor.maxhp;
            }
        }
        protected int data_maxhp2
        {
            get
            {
                if (Data != null)
                    return Data.MaxHp2;
                if (battler_2 != null)
                    return battler_2.actor.maxhp;
                return 0;
            }
        }
        #endregion

        public Window_Battle_HUD(Combat_Data data, bool reverse, int distance)
        {
            Data = data;
            initialize(reverse, distance);
        }
        public Window_Battle_HUD(int battler_1_id, bool reverse, int distance)
        {
            Battler_1_Id = battler_1_id;
            initialize(reverse, distance);
        }
        public Window_Battle_HUD(int battler_1_id, int battler_2_id, bool reverse, int distance)
        {
            Battler_1_Id = battler_1_id;
            Battler_2_Id = battler_2_id;
            initialize(reverse, distance);
        }

        protected void initialize(bool reverse, int distance)
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Combat_HUD");
            HP_Gauge_Texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Combat_HP_Gauge");
            Combat_Text_Texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_CombatWhite");
            Combat_Num_Texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Combat3");
            Reverse = reverse;
            Distance = distance;
            Base_Loc = new Vector2(battler_2 == null ? (Reverse ? 1 : -1) * 64 : 0, Config.WINDOW_HEIGHT - 148);
            setup_images();
            move_on();
            move(12);
            stereoscopic = Config.BATTLE_HUD_DEPTH;
        }

        public void move_on()
        {
            complete_move();
            Y_Move = new List<int> { -12, -12, -8, -12, -8, -12, -8, -12, -12 };
        }

        public void complete_move()
        {
            while (Y_Move.Count > 0)
                move(Y_Move.pop());
        }

        public void go_away()
        {
            complete_move();
            move_on();
            for (int i = 0; i < Y_Move.Count; i++)
                Y_Move[i] = -Y_Move[i];
        }

        public bool is_ready { get { return Y_Move.Count == 0; } }

        protected void move(int y)
        {
            Base_Loc.Y += y;
            Name_Loc.Y += (y > 0 ? -8 : 8);
            //for message in @messages
            //    message.y += y
            //end
        }

        protected void setup_images()
        {
            Game_Unit temp_battler;
            // Windows
            temp_battler = Reverse ? battler_2 : battler_1;
            if (temp_battler != null)
            {
                add_windows(temp_battler, Components, true);
                add_name_windows(temp_battler, Name_Components, true);
            }
            temp_battler = Reverse ? battler_1 : battler_2;
            if (temp_battler != null)
            {
                add_windows(temp_battler, Components, false);
                add_name_windows(temp_battler, Name_Components, false);
            }
            // Skill activation highlight //Yeti
            // Battle stats //Yeti
            Stat_BGs.Add(new Battle_Stats_BG(texture, battler_1.team, Reverse));
            Stat_BGs[Stat_BGs.Count - 1].loc.Y = 149;
            if (battler_2 != null)
            {
                Stat_BGs.Add(new Battle_Stats_BG(texture, battler_2.team, !Reverse));
                Stat_BGs[Stat_BGs.Count - 1].loc.Y = 149;
            }
            for (int i = 0; i < (battler_2 == null ? 4 : 8); i++)
            {
                Stat_Imgs.Add(new FE_Text_Int());
                //int side = (Reverse ^ i >= 4) ? 0 : 2;
                bool side = (Reverse ^ i >= 4);
                if ((i - 3) % 4 == 0)
                    //Stat_Imgs[Stat_Imgs.Count - 1].loc = new Vector2(Config.WINDOW_WIDTH - (155 + ((2 - side) * 51) / 2) + 39, 149 + 50 + 3);
                    Stat_Imgs[Stat_Imgs.Count - 1].loc = new Vector2((side ? 114 : (Config.WINDOW_WIDTH - 155)) + 39, 149 + 50 + 3);
                else
                    //Stat_Imgs[Stat_Imgs.Count - 1].loc = new Vector2(((Config.WINDOW_WIDTH - 40) / 2) * side + 39, 149 + 3 + (i % 4) * 8);
                    Stat_Imgs[Stat_Imgs.Count - 1].loc = new Vector2((side ? 0 : (Config.WINDOW_WIDTH - 40)) + 39, 149 + 3 + (i % 4) * 8);
                Stat_Imgs[Stat_Imgs.Count - 1].Font = "FE7_Text";
                Stat_Imgs[Stat_Imgs.Count - 1].texture = Combat_Num_Texture;
                Stat_Imgs[Stat_Imgs.Count - 1].text = "0";
            }
            // Weapon
            Weapon_Icons.Add(new Item_Icon_Sprite());
            //Weapon_Icons[Weapon_Icons.Count - 1].loc = new Vector2(Config.WINDOW_WIDTH - (Reverse ? 276 : 156), 149 + 13);
            Weapon_Icons[Weapon_Icons.Count - 1].loc = new Vector2((Reverse ? 44 : Config.WINDOW_WIDTH - 156), 149 + 13);
            Weapon_Names.Add(new FE_Text());
            //Weapon_Names[Weapon_Names.Count - 1].loc = new Vector2(Config.WINDOW_WIDTH - (Reverse ? 212 : 92), 149 + 11);
            Weapon_Names[Weapon_Names.Count - 1].loc = new Vector2((Reverse ? 108 : Config.WINDOW_WIDTH - 92), 149 + 11);
            Weapon_Names[Weapon_Names.Count - 1].Font = "FE7_Text";
            Weapon_Names[Weapon_Names.Count - 1].texture = Combat_Text_Texture;
            Mults.Add(new Multiplier_Img());
            //Mults[Mults.Count - 1].loc = new Vector2(Config.WINDOW_WIDTH - (Reverse ? 276 : 156), 149 + 13);
            Mults[Mults.Count - 1].loc = new Vector2((Reverse ? 44 : Config.WINDOW_WIDTH - 156), 149 + 13);
            WTAs.Add(new Weapon_Triangle_Arrow());
            //WTAs[WTAs.Count - 1].loc = new Vector2(Config.WINDOW_WIDTH - (Reverse ? 276 : 156), 149 + 13);
            WTAs[WTAs.Count - 1].loc = new Vector2((Reverse ? 44 : Config.WINDOW_WIDTH - 156), 149 + 13);
            if (battler_2 != null)
            {
                Weapon_Icons.Add(new Item_Icon_Sprite());
                //Weapon_Icons[Weapon_Icons.Count - 1].loc = new Vector2(Config.WINDOW_WIDTH - (!Reverse ? 276 : 156), 149 + 13);
                Weapon_Icons[Weapon_Icons.Count - 1].loc = new Vector2((!Reverse ? 44 : Config.WINDOW_WIDTH - 156), 149 + 13);
                Weapon_Names.Add(new FE_Text());
                //Weapon_Names[Weapon_Names.Count - 1].loc = new Vector2(Config.WINDOW_WIDTH - (!Reverse ? 212 : 92), 149 + 11);
                Weapon_Names[Weapon_Names.Count - 1].loc = new Vector2((!Reverse ? 108 : Config.WINDOW_WIDTH - 92), 149 + 11);
                Weapon_Names[Weapon_Names.Count - 1].Font = "FE7_Text";
                Weapon_Names[Weapon_Names.Count - 1].texture = Combat_Text_Texture;
                Mults.Add(new Multiplier_Img());
                //Mults[Mults.Count - 1].loc = new Vector2(Config.WINDOW_WIDTH - (!Reverse ? 276 : 156), 149 + 13);
                Mults[Mults.Count - 1].loc = new Vector2((!Reverse ? 44 : Config.WINDOW_WIDTH - 156), 149 + 13);
                WTAs.Add(new Weapon_Triangle_Arrow());
                //WTAs[WTAs.Count - 1].loc = new Vector2(Config.WINDOW_WIDTH - (!Reverse ? 276 : 156), 149 + 13);
                WTAs[WTAs.Count - 1].loc = new Vector2((!Reverse ? 44 : Config.WINDOW_WIDTH - 156), 149 + 13);
            }
            // Weapon triangle //Yeti
            if (Data == null)
                Stats = battler_2 == null ? new List<int?> { null, null, null, null } :
                    new List<int?> { null, null, null, null, null, null, null, null };
            else
                foreach (int? stat in Data.Data[0].Key.Stats)
                    Stats.Add(stat);
            refresh_battle_stats(); //Yeti
            // Add names/classes
            temp_battler = battler_1;
            Names.Add(new FE_Text());
                Names[Names.Count - 1].loc = new Vector2((Reverse ? 28 : Config.WINDOW_WIDTH - 28) -
                    Font_Data.text_width(temp_battler.actor.name) / 2, -85 + 48);
            Names[Names.Count - 1].Font = "FE7_Text";
            Names[Names.Count - 1].texture = Combat_Text_Texture;
            Names[Names.Count - 1].text = temp_battler.actor.name;
            Names.Add(new FE_Text());
                Names[Names.Count - 1].loc = new Vector2((Reverse ? 36 : Config.WINDOW_WIDTH - 36) -
                    Font_Data.text_width(temp_battler.actor.class_name_short()) / 2, -85 + 29);
            Names[Names.Count - 1].Font = "FE7_Text";
            Names[Names.Count - 1].texture = Combat_Text_Texture;
            Names[Names.Count - 1].text = temp_battler.actor.class_name_short();
            temp_battler = battler_2;
            if (temp_battler != null)
            {
                Names.Add(new FE_Text());
            Names[Names.Count - 1].loc = new Vector2((Reverse ? Config.WINDOW_WIDTH - 28 : 28) -
                Font_Data.text_width(temp_battler.actor.name) / 2, -85 + 48);
                Names[Names.Count - 1].Font = "FE7_Text";
                Names[Names.Count - 1].texture = Combat_Text_Texture;
                Names[Names.Count - 1].text = temp_battler.actor.name;
                Names.Add(new FE_Text());
            Names[Names.Count - 1].loc = new Vector2((Reverse ? Config.WINDOW_WIDTH - 36 : 36) -
                Font_Data.text_width(temp_battler.actor.class_name_short()) / 2, -85 + 29);
                Names[Names.Count - 1].Font = "FE7_Text";
                Names[Names.Count - 1].texture = Combat_Text_Texture;
                Names[Names.Count - 1].text = temp_battler.actor.class_name_short();
            }
            // Faces
            Faces.Add(new Battle_Face_Sprite(battler_1));
            Faces[Faces.Count - 1].loc = new Vector2(Reverse ? 3 : Config.WINDOW_WIDTH - 3, 149 + 51);
            Faces[Faces.Count - 1].mirrored = Reverse;
            temp_battler = Reverse ? battler_1 : battler_2;
            if (battler_2 != null)
            {
                Faces.Add(new Battle_Face_Sprite(battler_2));
                Faces[Faces.Count - 1].loc = new Vector2(Reverse ? Config.WINDOW_WIDTH - 3 : 3, 149 + 51);
                Faces[Faces.Count - 1].mirrored = !Reverse;
            }
            // HP Gauges
            Hp1 = data_hp1;
            HP_Gauge_1 = new HP_Gauge(HP_Gauge_Texture);
            HP_Gauge_1.loc.X = (Reverse ? 29 : Config.WINDOW_WIDTH - 131);
            HP_Gauge_1.maxhp = data_maxhp1;
            HP_Gauge_1.hp = data_hp1;
            HP_Count_1 = new HP_Counter();
            HP_Count_1.loc = new Vector2((Reverse ? 0 : Config.WINDOW_WIDTH - 160) + 25, 189);
            HP_Count_1.Font = "FE7_Text_Combat3";
            HP_Count_1.texture = Combat_Num_Texture;
            HP_Count_1.set_hp(Hp1);
            if (battler_2 != null)
            {
                Hp2 = data_hp2;
                HP_Gauge_2 = new HP_Gauge(HP_Gauge_Texture);
                HP_Gauge_2.loc.X = (Reverse ? Config.WINDOW_WIDTH - 131 : 29);
                HP_Gauge_2.maxhp = data_maxhp2;
                HP_Gauge_2.hp = data_hp2;
                HP_Count_2 = new HP_Counter();
                HP_Count_2.loc = new Vector2((Reverse ? Config.WINDOW_WIDTH - 160 : 0) + 25, 189);
                HP_Count_2.Font = "FE7_Text_Combat3";
                HP_Count_2.texture = Combat_Num_Texture;
                HP_Count_2.set_hp(Hp2);
            }
        }

        protected void add_name_windows(Game_Unit temp_battler, List<HUD_Component> components)
        {
            add_name_windows(temp_battler, components, false);
        }
        protected void add_name_windows(Game_Unit temp_battler, List<HUD_Component> components, bool mirrored)
        {
            components.Add(new HUD_Component
            {
                loc = new Vector2(mirrored ? Config.WINDOW_WIDTH - (NAME_PANES.Width - 8) : -8, -85 + 45),
                mirrored = mirrored,
                src_rect = new Rectangle(
                    NAME_PANES.X, NAME_PANES.Y + (temp_battler.team - 1) * NAME_PANES.Height, NAME_PANES.Width, NAME_PANES.Height)
            });
            components.Add(new HUD_Component
            {
                loc = new Vector2(mirrored ? Config.WINDOW_WIDTH - (CLASS_PANES.Width -8) : -8, -85 + 26),
                mirrored = mirrored,
                src_rect = new Rectangle(
                    CLASS_PANES.X, CLASS_PANES.Y + (temp_battler.team - 1) * CLASS_PANES.Height, CLASS_PANES.Width, CLASS_PANES.Height)
            });
        }

        protected void add_windows(Game_Unit temp_battler, List<HUD_Component> components)
        {
            add_windows(temp_battler, components, false);
        }
        protected void add_windows(Game_Unit temp_battler, List<HUD_Component> components, bool mirrored)
        {
            components.Add(new HUD_Component
            {
                loc = new Vector2(mirrored ? Config.WINDOW_WIDTH - (STAT_PANES.Width + 0) : 0, 149 + 0),
                mirrored = mirrored,
                src_rect = new Rectangle(
                    STAT_PANES.X + (temp_battler.team - 1) * STAT_PANES.Width, STAT_PANES.Y, STAT_PANES.Width, STAT_PANES.Height)
            });
            components.Add(new HUD_Component
            {
                loc = new Vector2(mirrored ? Config.WINDOW_WIDTH - (HP_PANES.Width + 0) : 0, 149 + 29),
                mirrored = mirrored,
                src_rect = new Rectangle(
                    HP_PANES.X, HP_PANES.Y + (temp_battler.team - 1) * HP_PANES.Height, HP_PANES.Width, HP_PANES.Height)
            });
            components.Add(new HUD_Component
            {
                loc = new Vector2(mirrored ? Config.WINDOW_WIDTH - (FACE_PANES.Width + 0) : 0, 149 + 51),
                mirrored = mirrored,
                src_rect = new Rectangle(
                    FACE_PANES.X, FACE_PANES.Y + (temp_battler.team - 1) * FACE_PANES.Height, FACE_PANES.Width, FACE_PANES.Height)
            });
            components.Add(new HUD_Component
            {
                loc = new Vector2(mirrored ? Config.WINDOW_WIDTH - (SKILL_PANES.Width + 112) : 112, 149 + 51),
                mirrored = mirrored,
                src_rect = new Rectangle(
                    SKILL_PANES.X, SKILL_PANES.Y + (temp_battler.team - 1) * SKILL_PANES.Height, SKILL_PANES.Width, SKILL_PANES.Height)
            });
            components.Add(new HUD_Component
            {
                loc = new Vector2(mirrored ? Config.WINDOW_WIDTH - (WEAPON_PANE.Width + 43) : 43, 149 + 11),
                mirrored = mirrored,
                src_rect = new Rectangle(WEAPON_PANE.X, WEAPON_PANE.Y, WEAPON_PANE.Width, WEAPON_PANE.Height)
            });
        }

        public void face_set(int id, int frame)
        {
            if (id >= Faces.Count)
                return;
            Faces[id].set_frame(frame);
        }

        public void face_kill(int id)
        {
            if (id >= Faces.Count)
                return;
            Faces[id].kill();
        }

        public void add_message(string message, int team)
        {
            add_message(message, team, 120, "White");
        }
        public void add_message(string message, int team, int end_timer)
        {
            add_message(message, team, end_timer, "White");
        }
        public void add_message(string message, int team, int end_timer, string color_name)
        {

        }

        public bool is_hp_ready()
        {
            if (Hit_Wait_Timer > 1)
                return false;
            if (battler_2 != null)
                return data_hp1 == Hp1 && data_hp2 == Hp2;
            else
                return data_hp1 == Hp1;
        }
  
        public void hit(int target)
        {
            Hit_Wait_Timer = 31; // 30 frames
            Health_Loss = target;
            if (target == 2)
                HP_Count_2.zoom_timer = Math.Max(31, (Hp2 - data_hp2) * 2);
            else
                HP_Count_1.zoom_timer = Math.Max(31, (Hp1 - data_hp1) * 2);
        }

        public void drain_hit(int target)
        {
            Hit_Wait_Timer = 85; // 84 frames
            Health_Loss = target;
            if (target == 2)
                HP_Count_2.zoom_timer = Math.Max(85, (Hp2 - data_hp2) * 2);
            else
                HP_Count_1.zoom_timer = Math.Max(85, (Hp1 - data_hp1) * 2);
        }

        #region Stats
        protected bool stat_update_done()
        {
            if (Data != null)
                for (int i = 0; i < Stats.Count; i++)
                    if (Stats[i] != stat(i))
                        return false;
            return true;
        }

        public void set_attack_id(int id)
        {
            Attack_Id = Math.Min(Data.Data.Count - 1, id);
            Action_Id = -1;
            refresh_battle_stats();
        }

        public void set_action_id(int id)
        {
            Action_Id = id;
        }

        public void update_battle_stats()
        {
            for (int i = 0; i < Stats.Count; i++)
                Stats[i] = stat(i);
            refresh_battle_stats();
        }

        protected void update_stats()
        {
            for (int i = 0; i < Stats.Count; i++)
                if (Stats[i] != stat(i))
                {
                    if (Stats[i] == null || stat(i) == null)
                        Stats[i] = null;
                    else
                        Stats[i] = Additional_Math.int_closer((int)Stats[i], (int)stat(i), 1);
                }
        }

        protected int? stat(int i)
        {
            switch (i)
            {
                case 0:
                    return Action_Id > -1 ? Data.Data[Attack_Id].Value[Action_Id].Hit1 : Data.Data[Attack_Id].Key.Stats[0];
                case 1:
                    return Action_Id > -1 ? Data.Data[Attack_Id].Value[Action_Id].Dmg1 : Data.Data[Attack_Id].Key.Stats[1];
                case 2:
                    return Action_Id > -1 ? Data.Data[Attack_Id].Value[Action_Id].Crt1 : Data.Data[Attack_Id].Key.Stats[2];
                case 3:
                    return Action_Id > -1 ? Data.Data[Attack_Id].Value[Action_Id].Skl1 : Data.Data[Attack_Id].Key.Stats[3];
                case 4:
                    return Action_Id > -1 ? Data.Data[Attack_Id].Value[Action_Id].Hit2 : Data.Data[Attack_Id].Key.Stats[4];
                case 5:
                    return Action_Id > -1 ? Data.Data[Attack_Id].Value[Action_Id].Dmg2 : Data.Data[Attack_Id].Key.Stats[5];
                case 6:
                    return Action_Id > -1 ? Data.Data[Attack_Id].Value[Action_Id].Crt2 : Data.Data[Attack_Id].Key.Stats[6];
                case 7:
                    return Action_Id > -1 ? Data.Data[Attack_Id].Value[Action_Id].Skl2 : Data.Data[Attack_Id].Key.Stats[7];
            }
            return null;
        }
        #endregion

        protected void refresh_battle_stats()
        {
            refresh_battle_stats(false);
        }
        protected void refresh_battle_stats(bool just_stats)
        {
            if (!just_stats)
            {
                foreach (Item_Icon_Sprite icon in Weapon_Icons)
                    icon.texture = null;
                foreach (FE_Text name in Weapon_Names)
                    name.text = "";

                FEXNA_Library.Data_Weapon weapon1 = null;
                FEXNA_Library.Data_Weapon weapon2 = null;
                WTAs[0].value = 0;
                if (Global.scene.scene_type == "Scene_Dance" || Global.scene.scene_type == "Scene_Promotion")
                {
                    int item_index = -1;
                    if (Global.scene.scene_type == "Scene_Dance")
                        item_index = Global.game_state.dance_item;
                    else if (Global.scene.scene_type == "Scene_Promotion")
                        item_index = Global.game_state.item_used;
                    if (item_index > -1)
                    {
                        FEXNA_Library.Item_Data item_data = battler_1.actor.items[item_index];
                        FEXNA_Library.Data_Equipment item1 = item_data.to_equipment;
                        Weapon_Icons[0].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + item1.Image_Name);
                        Weapon_Icons[0].index = item1.Image_Index;
                        Weapon_Names[0].offset.X = Font_Data.text_width(item1.full_name()) / 2;
                        Weapon_Names[0].text = item1.full_name();
                    }

                    if (battler_2 != null)
                    {
                        WTAs[1].value = 0;
                        weapon2 = battler_2.actor.weapon;
                        if (weapon2 != null)
                        {
                            Weapon_Icons[1].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + weapon2.Image_Name);
                            Weapon_Icons[1].index = weapon2.Image_Index;
                            Weapon_Names[1].offset.X = Font_Data.text_width(weapon2.full_name()) / 2;
                            Weapon_Names[1].text = weapon2.full_name();
                        }
                    }
                }
                else
                {
                    weapon1 = battler_1.actor.weapon;
                    Weapon_Icons[0].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + weapon1.Image_Name);
                    Weapon_Icons[0].index = weapon1.Image_Index;
                    if (!(Data is Staff_Data) || ((Staff_Data)Data).attack_staff)
                        Weapon_Icons[0].flash = weapon1.effective_multiplier(battler_1, battler_2) > 1;
                    Weapon_Names[0].offset.X = Font_Data.text_width(weapon1.full_name()) / 2;
                    Weapon_Names[0].text = weapon1.full_name();


                    if (battler_2 != null)
                    {
                        WTAs[1].value = 0;
                        weapon2 = battler_2.actor.weapon;
                        if (weapon2 != null)
                        {
                            Weapon_Icons[1].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + weapon2.Image_Name);
                            Weapon_Icons[1].index = weapon2.Image_Index;
                            if (!(Data is Staff_Data) || ((Staff_Data)Data).attack_staff)
                                Weapon_Icons[1].flash = weapon2.effective_multiplier(battler_2, battler_1) > 1;
                            Weapon_Names[1].offset.X = Font_Data.text_width(weapon2.full_name()) / 2;
                            Weapon_Names[1].text = weapon2.full_name();
                            // Attack Multiplier
                            if (!weapon1.is_staff())
                            { }// Mults[1].value = Mults[1].get_multi(battler_2, battler_1, weapon2); //Debug
                        }
                        if (!weapon1.is_staff())
                        {
                            // Weapon triangle arrows
                            WeaponTriangle tri = Combat.weapon_triangle(battler_1, battler_2, weapon1, weapon2, Data.Distance);
                            if (tri != WeaponTriangle.Nothing)
                            {
                                WTAs[0].value = tri;
                                if (weapon2 != null)
                                    WTAs[1].value = Combat.reverse_wta(tri);
                            }
                            // Attack Multiplier
                            //Mults[0].value = Mults[0].get_multi(battler_1, battler_2, weapon1); //Debug
                        }
                    }
                }
            }
            for (int i = 0; i < Stat_Imgs.Count; i++)
            {
                if (Stats[i] == null)
                    Stat_Imgs[i].text = "--";
                else if (Stats[i] < 0)
                    Stat_Imgs[i].text = "--";
                else
                    Stat_Imgs[i].text = Stats[i].ToString();
            }
        }

        public override void update()
        {
            if (!is_ready) //Yeti
                move(Y_Move.pop());
            // skill opacity //Yeti
            // Weapon icon flash //Yeti
            foreach (Item_Icon_Sprite icon in Weapon_Icons)
                icon.update();
            foreach (Multiplier_Img mult in Mults)
                mult.update();
            foreach (Weapon_Triangle_Arrow wta in WTAs)
                wta.update();
            HP_Gauge_1.update();
            HP_Gauge_1.hp = Hp1;
            HP_Count_1.update();
            HP_Count_1.set_hp(Hp1);
            if (HP_Gauge_2 != null)
            {
                HP_Gauge_2.update();
                HP_Gauge_2.hp = Hp2;
                HP_Count_2.update();
                HP_Count_2.set_hp(Hp2);
            }
            // Update HP gauges
            if (No_Hp1_Timer > 0)
                No_Hp1_Timer--;
            if (No_Hp2_Timer > 0)
                No_Hp2_Timer--;
            if (No_Health_Timer > 0)
                No_Health_Timer--;
            if (Hp1_Timer == 0)
            {
                // HP gain: battler 1
                if (Hp1 < data_hp1)
                {
                    Hp_Gain = 1;
                    Hp1++;
                    Hp1_Timer = 4;
                }
                // HP loss: battler 1
                else if (Hp1 > data_hp1)
                {
                    if (data_hp1 <= 0 && !No_Health_Flag)
                    {
                        No_Health_Flag = true;
                        No_Health_Timer = 29; // Effectively 30
                    }
                    Hp1--;
                    if (Hp1 <= 0)
                        No_Hp1_Timer = No_Health_Timer;
                    Hp1_Timer = 2;
                }
            }
            else if (Hp_Gain == 1 && Hp1_Timer == 2)
            {
                Global.game_system.play_se(System_Sounds.HP_Recovery);
                Hp_Gain = 0;
            }
            if (battler_2 != null)
            {
                if (Hp2_Timer == 0)
                {
                    // HP gain: battler 2
                    if (Hp2 < data_hp2)
                    {
                        Hp_Gain = 2;
                        Hp2++;
                        Hp2_Timer = 4;
                    }
                    // HP loss: battler 2
                    else if (Hp2 > data_hp2)
                    {
                        if (data_hp2 <= 0 && !No_Health_Flag)
                        {
                            No_Health_Flag = true;
                            No_Health_Timer = 29; // Effectively 30
                        }
                        Hp2--;
                        if (Hp2 <= 0)
                            No_Hp2_Timer = No_Health_Timer;
                        Hp2_Timer = 2;
                    }
                }
                else if (Hp_Gain == 2 && Hp2_Timer == 2)
                {
                    Global.game_system.play_se(System_Sounds.HP_Recovery);
                    Hp_Gain = 0;
                }
            }
            // Update stats
            if (!stat_update_done())
                if (Stat_Timer == 0)
                {
                    update_stats();
                    refresh_battle_stats(true);
                    Stat_Timer = 2;
                }
            // update messages //Yeti
            if (Hp1_Timer > 0)
                Hp1_Timer--;
            if (Hp2_Timer > 0)
                Hp2_Timer--;
            if (Stat_Timer > 0)
                Stat_Timer--;
            if (Hit_Wait_Timer > 0)
            {
                Hit_Wait_Timer--;
                if (Hit_Wait_Timer == 0)
                    Health_Loss = 0;
            }
            foreach (Battle_Face_Sprite face in Faces)
                face.update();
        }

        #region Draw
        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    draw_offset -= draw_vector();
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    // Name section
                    foreach (HUD_Component component in Name_Components)
                    {
                        component.draw(sprite_batch, texture, -1 * (Name_Loc - draw_offset));
                    }
                    foreach (FE_Text name in Names)
                    {
                        name.draw(sprite_batch, -1 * (Name_Loc - draw_offset));
                    }
                    // Main section
                    foreach (HUD_Component component in Components)
                    {
                        component.draw(sprite_batch, texture, -1 * (Base_Loc - draw_offset));
                    }
                    sprite_batch.End();
                    // Faces
                    foreach(Battle_Face_Sprite face in Faces)
                        face.draw(sprite_batch, -1 * (Base_Loc - draw_offset));

                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    // Stat BG
                    foreach (Battle_Stats_BG bg in Stat_BGs)
                    {
                        bg.draw(sprite_batch, -1 * (Base_Loc - draw_offset));
                    }
                    // Stats
                    foreach (FE_Text_Int stat in Stat_Imgs)
                    {
                        stat.draw(sprite_batch, -1 * (Base_Loc - draw_offset));
                    }
                    sprite_batch.End();
                    // Weapon Icons
                    foreach (Item_Icon_Sprite icon in Weapon_Icons)
                    {
                        icon.draw(sprite_batch, -1 * (Base_Loc - draw_offset));
                    }
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    // Weapon Names
                    foreach (FE_Text name in Weapon_Names)
                    {
                        name.draw(sprite_batch, -1 * (Base_Loc - draw_offset));
                    }
                    // Weapon Triangle Arrows
                    foreach (Weapon_Triangle_Arrow wta in WTAs)
                        wta.draw(sprite_batch, -1 * (Base_Loc - draw_offset));
                    // Attack Multipliers
                    foreach (Multiplier_Img mult in Mults)
                        mult.draw(sprite_batch, -1 * (Base_Loc - draw_offset));

                    HP_Gauge_1.draw(sprite_batch, -1 * (Base_Loc - draw_offset));
                    HP_Count_1.draw(sprite_batch, -1 * (Base_Loc - draw_offset));
                    if (HP_Gauge_2 != null)
                    {
                        HP_Gauge_2.draw(sprite_batch, (-1 * Base_Loc) + draw_offset);
                        HP_Count_2.draw(sprite_batch, (-1 * Base_Loc) + draw_offset);
                    }
                    sprite_batch.End();
                }
        }
        #endregion
    }

    struct HUD_Component
    {
        public Vector2 loc;
        public Rectangle src_rect;
        public bool mirrored;

        public void draw(SpriteBatch sprite_batch, Texture2D texture, Vector2 base_loc)
        {
            sprite_batch.Draw(texture, loc - base_loc,
                src_rect, Color.White, 0f, Vector2.Zero, Vector2.One,
                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
        }
    }
}
