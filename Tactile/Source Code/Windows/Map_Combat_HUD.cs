using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using ListExtension;

namespace Tactile
{
    class Map_Combat_HUD : Sprite
    {
        const int HP_GAIN_TIME = 4;
        const int TIMER_MAX = 8;
        protected readonly static List<Vector2> WINDOW_SHAKE = new List<Vector2> { new Vector2(0, -4), new Vector2(2, -4), new Vector2(-2, 2),
            new Vector2(-2, -3), new Vector2(2, -3), new Vector2(-2, -1), new Vector2(-1, 0), new Vector2(-1, -3), new Vector2(3, 2),
            new Vector2(4, 2), new Vector2(-2, -3), new Vector2(-2, 4), new Vector2(-4, -4), new Vector2(1, 0), new Vector2(-2, 0),
            new Vector2(1, 3), new Vector2(0, 0) };
        protected readonly static List<Vector2> DATA_SHAKE = new List<Vector2> { new Vector2(-2, 0), new Vector2(4, 2), new Vector2(-1, 4),
            new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, 0), new Vector2(2, 1), new Vector2(-2, 4),
            new Vector2(1, -3), new Vector2(-3, -1), new Vector2(1, -1), new Vector2(-3, 1), new Vector2(-1, 3), new Vector2(-1, 3),
            new Vector2(-2, -2), new Vector2(0, 0) };

        Combat_Data Data = null;
        int Unit_Id1 = -1, Unit_Id2 = -1;
        int Hp1, Hp2;
        RightAdjustedText HP_Counter1, HP_Counter2;
        protected int Hp1_Timer = 0, Hp2_Timer = 0;
        TextSprite Name1, Name2;
        protected int Attack_Id = 0, Action_Id = -1;
        protected List<int?> Stats = new List<int?>();
        List<RightAdjustedText> Stat_Imgs;
        RasterizerState Raster_State = new RasterizerState { ScissorTestEnable = true };
        int Timer = 1;
        protected int Stat_Timer = 0;
        protected Vector2 Window_Offset = Vector2.Zero, Data_Offset = Vector2.Zero;
        protected List<Vector2> Window_Shake = new List<Vector2>();
        protected List<Vector2> Data_Shake = new List<Vector2>();

        #region Accessors
        public RasterizerState raster_state { get { return Raster_State; } }

        protected Game_Unit unit_1
        {
            get
            {
                if (Unit_Id1 >= 0)
                    return Global.game_map.units.ContainsKey(Unit_Id1) ? Global.game_map.units[Unit_Id1] : null;
                if (Data != null)
                    return Global.game_map.units[Data.Battler_1_Id];
                return null;
            }
        }
        protected Combat_Map_Object unit_2
        {
            get
            {
                if (Unit_Id2 >= 0)
                    return Global.game_map.units.ContainsKey(Unit_Id2) ? Global.game_map.units[Unit_Id2] : null;

                if (Data != null && Data.Battler_2_Id != null)
                    return Global.game_map.attackable_map_object((int)Data.Battler_2_Id);
                else if (Data != null && Data is Aoe_Data)
                    return Global.game_map.attackable_map_object(
                        ((Aoe_Data)Data).Battler_2_Ids[Math.Max(0, Attack_Id)]);
                else if (Data != null && Data is Aoe_Staff_Data)
                    return Global.game_map.attackable_map_object(
                        ((Aoe_Staff_Data)Data).Battler_2_Ids[Math.Max(0, Attack_Id)]);

                return null;
            }
        }

        protected int battler_2_id
        {
            get
            {
                if (Unit_Id2 >= 0)
                    return Unit_Id2;

                if (Data != null && Data.Battler_2_Id != null)
                    return (int)Data.Battler_2_Id;
                else if (Data != null && Data is Aoe_Data)
                    return ((Aoe_Data)Data).Battler_2_Ids[Math.Max(0, Attack_Id)];
                else if (Data != null && Data is Aoe_Staff_Data)
                    return ((Aoe_Staff_Data)Data).Battler_2_Ids[Math.Max(0, Attack_Id)];
                return -1;
            }
        }

        protected int hp1
        {
            get
            {
                if (Data != null)
                    return Data.Hp1;
                else
                    return unit_1 == null ? 0 : unit_1.actor.hp;
            }
        }
        protected int hp2
        {
            get
            {
                if (Data != null)
                    return Data.Hp2;
                else
                    //return unit_2.is_unit() ? ((Game_Unit)unit_2).actor.hp : ((Destroyable_Object)unit_2).hp; //Debug
                    return unit_2.hp;
            }
        }

        //protected int maxhp1 { get { return unit_1.actor.maxhp; } } //Data.MaxHp1 //Yeti //Debug
        //protected int maxhp2 { get { return unit_2.is_unit() ? ((Game_Unit)unit_2).actor.maxhp : ((Destroyable_Object)unit_2).maxhp; } }
        protected int maxhp1 { get { return unit_1.maxhp; } } //Data.MaxHp1 //Yeti
        protected int maxhp2 { get { return unit_2.maxhp; } }

        protected int team1 { get { return unit_1.team; } } //Data.Team1 //Yeti
        protected int team2 { get { return unit_2.team; } }
        #endregion

        public Map_Combat_HUD(List<Texture2D> textures, Combat_Data data)
        {
            this.textures = textures;
            initialize(data);
        }
        public Map_Combat_HUD(List<Texture2D> textures, int id)
        {
            this.textures = textures;
            initialize(id);
        }
        public Map_Combat_HUD(List<Texture2D> textures, int id1, int id2)
        {
            this.textures = textures;
            initialize(id1, id2);
        }

        protected void initialize(Combat_Data data)
        {
            Data = data;
            initialize_images();
            Name1.text = Data.Name1;
            Name1.offset = new Vector2(Font_Data.text_width(Name1.text) / 2, 0);
            if (Data.Battler_2_Id != null)
            {
                Name2.text = Data.Name2;
                Name2.offset = new Vector2(Font_Data.text_width(Name2.text) / 2, 0);
            }
        }

        protected void initialize(int id)
        {
            Unit_Id1 = id;
            initialize_images();
            Name1.text = unit_1.actor.name;
            Name1.offset = new Vector2(Font_Data.text_width(Name1.text) / 2, 0);
        }
        protected void initialize(int id1, int id2)
        {
            Unit_Id1 = id1;
            Unit_Id2 = id2;
            initialize_images();
            Name1.text = unit_1.actor.name;
            Name1.offset = new Vector2(Font_Data.text_width(Name1.text) / 2, 0);
            Name2.text = unit_2.is_unit() ? ((Game_Unit)unit_2).actor.name : ""; // Shouldn't come up, probably
            Name2.offset = new Vector2(Font_Data.text_width(Name2.text) / 2, 0);
        }

        protected void initialize_images()
        {
            Name1 = new TextSprite();
            Name1.SetFont(Config.UI_FONT, Global.Content, "White");
            Hp1 = hp1;
            HP_Counter1 = new RightAdjustedText();
            HP_Counter1.SetFont(Config.COMBAT_DIGITS_FONT, Global.Content);
            if (unit_2 != null)
            {
                Name2 = new TextSprite();
                Name2.SetFont(Config.UI_FONT, Global.Content, "White");
                Hp2 = hp2;
                HP_Counter2 = new RightAdjustedText();
                HP_Counter2.SetFont(Config.COMBAT_DIGITS_FONT, Global.Content);
                Stat_Imgs = new List<RightAdjustedText>();
                if (Data != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Stat_Imgs.Add(new RightAdjustedText());
                        Stat_Imgs[i].SetFont(Config.COMBAT_DIGITS_FONT, Global.Content);
                        Stat_Imgs[i].offset = new Vector2(-40 * ((i % 4) / 2), -8 * ((i % 4) % 2));
                    }
                }
            }
            else
            {
                Stat_Imgs = new List<RightAdjustedText>();
                if (Data != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Stat_Imgs.Add(new RightAdjustedText());
                        Stat_Imgs[i].SetFont(Config.COMBAT_DIGITS_FONT, Global.Content);
                        Stat_Imgs[i].offset = new Vector2(-40 * ((i % 4) / 2), -8 * ((i % 4) % 2));
                    }
                }
            }
            if (Data != null)
                foreach (int? stat in Data.Data[0].Key.Stats)
                    Stats.Add(stat);
            refresh_battle_stats();
            set_location();
        }

        protected void set_location()
        {
            // If only one battler
            if (unit_2 == null)
            {
                int y = (int)(unit_1.pixel_loc.Y - Global.game_map.display_y);
                if (y <= 72)
                    y += 24;
                else
                    y -= 64;
                loc = new Vector2((Config.WINDOW_WIDTH - 90) / 2, y);
            }
            else
            {
                // Places window above all battlers
                int y = (int)(Math.Min(unit_1.pixel_loc.Y, unit_2.pixel_loc.Y) - Global.game_map.display_y);
                // If off the top of the screen
                if (y <= 72)
                {
                    y = (int)(Math.Max(unit_1.pixel_loc.Y, unit_2.pixel_loc.Y) - Global.game_map.display_y);
                    // If off the bottom of the screen
                    if (y > Config.WINDOW_HEIGHT - 72)
                        y -= 64;
                    else
                        y += 24;
                }
                else
                    y -= 64;
                loc = new Vector2((Config.WINDOW_WIDTH - 176) / 2, y);
            }
        }

        #region Stats
        protected bool stat_update_done()
        {
            for (int i = 0; i < Stats.Count; i++)
                if (Stats[i] != stat(i))
                    return false;
            return true;
        }

        public void set_attack_id(int id)
        {
            Attack_Id = Math.Min(Data.Data.Count - 1, id);
            if (Data != null && Data.MultipleTargets)
            {
                Name2.text = Data.Name2;
                Name2.offset = new Vector2(Font_Data.text_width(Name2.text) / 2, 0);
                Hp2 = Data.Hp2;
            }
            Action_Id = -1;
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
            if (Data != null)
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

        public void shake()
        {
            Window_Shake.Clear();
            Window_Shake.AddRange(WINDOW_SHAKE);
            Data_Shake.Clear();
            Data_Shake.AddRange(DATA_SHAKE);
        }

        public Rectangle scissor_rect()
        {
            return Scene_Map.fix_rect_to_screen( new Rectangle(0, (int)loc.Y + 24 - (Timer * (Data == null ? 2 : 4)), 320, Timer * (Data == null ? 4 : 8)));
        }

        public bool is_ready()
        {
            if (Hp1 != hp1)
                return false;
            if (unit_2 != null && Hp2 != hp2)
                    return false;
            return true;
        }

        public override void update()
        {
            if (Window_Shake.Count > 0)
                Window_Offset = Window_Shake.shift();
            if (Data_Shake.Count > 0)
                Data_Offset = Data_Shake.shift();

            if (Timer < TIMER_MAX)
                Timer++;

            if (Hp1 != this.hp1 || (unit_2 != null && Hp2 != this.hp2))
                if (Global.Input.triggered(Inputs.A) ||
                    Global.Input.mouse_triggered(MouseButtons.Left) ||
                    Global.Input.gesture_triggered(TouchGestures.Tap))
                {
                    if (Hp1 < this.hp1 || (unit_2 != null && Hp2 < this.hp2))
                    {
                        Global.game_system.play_se(System_Sounds.HP_Recovery);
                    }
                    Hp1 = this.hp1;
                    if (unit_2 != null)
                        Hp2 = this.hp2;
                }

            if (Hp1_Timer == 0)
            {
                if (Hp1 < hp1)
                {
                    Global.game_system.play_se(System_Sounds.HP_Recovery);
                    Hp1_Timer = HP_GAIN_TIME;
                }
                Hp1 = Additional_Math.int_closer(Hp1, hp1, 1);
            }
            if (unit_2 != null && Hp2_Timer == 0)
            {
                if (Hp2 < hp2)
                {
                    Global.game_system.play_se(System_Sounds.HP_Recovery);
                    Hp2_Timer = HP_GAIN_TIME;
                }
                Hp2 = Additional_Math.int_closer(Hp2, hp2, 1);
            }
            if (Hp1_Timer > 0)
                Hp1_Timer--;
            if (Hp2_Timer > 0)
                Hp2_Timer--;
            Name1.update();
            HP_Counter1.text = (Hp1 > 99 ? "??" : Hp1.ToString());
            HP_Counter1.update();
            if (unit_2 != null)
            {
                Name2.update();
                HP_Counter2.text = (Hp2 > 99 ? "??" : Hp2.ToString());
                HP_Counter2.update();
            }
            // Update stats
            if (!stat_update_done())
                if (Stat_Timer == 0)
                {
                    update_stats();
                    refresh_battle_stats();
                    Stat_Timer = 2;
                }
            if (Stat_Timer > 0)
                Stat_Timer--;
            foreach (RightAdjustedText text in Stat_Imgs)
                text.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            //if ((textures.Count == 3) && Global.game_map.units.ContainsKey(Data.Battler_1_Id))
            if ((textures.Count == 3) && unit_1 != null)
                if (visible)
                {
                    Vector2 loc = this.loc + new Vector2(0, Data == null ? 8 : 0);
                    Vector2 offset = this.offset;
                    int width;
                    bool two_units = false;
                    if (unit_2 != null)
                        if (Global.game_map.attackable_map_object(battler_2_id) != null)
                            two_units = true;
                    // Window 1 - top
                    sprite_batch.Draw(textures[0], loc + new Vector2(1, 0),
                        new Rectangle(0, 64 * (team1 - 1), 88, 32), tint,
                        0f, offset + Window_Offset, 1f, SpriteEffects.None, 0f);
                    // HP gauge 1
                    sprite_batch.Draw(textures[0], loc + new Vector2(24, 16),
                        new Rectangle(88, 0, 56, 8), tint,
                        0f, offset + Data_Offset, 1f, SpriteEffects.None, 0f);
                    width = (Hp1 * 49) / maxhp1;
                    sprite_batch.Draw(textures[0], loc + new Vector2(24 + 4, 16),
                        new Rectangle(88 + 4, 8, width, 8), tint,
                        0f, offset + Data_Offset, 1f, SpriteEffects.None, 0f);
                    if (Data != null)
                    {
                        // Window 1 - bottom
                        sprite_batch.Draw(textures[0], loc + new Vector2(1, 24),
                            new Rectangle(0, 64 * (team1 - 1) + 32, 88, 32), tint,
                            0f, offset + Window_Offset, 1f, SpriteEffects.None, 0f);
                        // Labels 1
                        sprite_batch.Draw(textures[0], loc + new Vector2(4, 28),
                            new Rectangle(88, 16, 56, 16), tint,
                            0f, offset + Data_Offset, 1f, SpriteEffects.None, 0f);
                        // Stats 1
                        for (int i = 0; i < 4; i++)
                        {
                            Stat_Imgs[i].loc = loc + new Vector2(42, 28);
                            Stat_Imgs[i].draw(sprite_batch, Data_Offset);
                        }
                    }
                    // Name 1
                    Name1.loc = loc + new Vector2(44, 0);
                    Name1.draw(sprite_batch, Data_Offset);
                    // HP 1
                    HP_Counter1.loc = loc + new Vector2(24, 16);
                    HP_Counter1.draw(sprite_batch, Data_Offset);
                    if (two_units)
                    {
                        loc += new Vector2(88, 0);
                        // Window 2 - top
                        sprite_batch.Draw(textures[0], loc + new Vector2(0, 0),
                            new Rectangle(0, 64 * (team2 - 1), 88, 32), tint,
                            0f, offset + Window_Offset, 1f, SpriteEffects.None, 0f);
                        // HP Gauge 2
                        sprite_batch.Draw(textures[0], loc + new Vector2(24, 16),
                            new Rectangle(88, 0, 56, 8), tint,
                            0f, offset + Data_Offset, 1f, SpriteEffects.None, 0f);
                        width = (Hp2 * 49) / maxhp2;
                        sprite_batch.Draw(textures[0], loc + new Vector2(24 + 4, 16),
                            new Rectangle(88 + 4, 8, width, 8), tint,
                            0f, offset + Data_Offset, 1f, SpriteEffects.None, 0f);
                        if (Data != null)
                        {
                            // Window 2 - bottom
                            sprite_batch.Draw(textures[0], loc + new Vector2(0, 24),
                                new Rectangle(0, 64 * (team2 - 1) + 32, 88, 32), tint,
                                0f, offset + Window_Offset, 1f, SpriteEffects.None, 0f);
                            // Labels 2
                            sprite_batch.Draw(textures[0], loc + new Vector2(4, 28),
                                new Rectangle(88, 16, 56, 16), tint,
                                0f, offset + Data_Offset, 1f, SpriteEffects.None, 0f);
                            // Stats 2
                            for (int i = 4; i < 8; i++)
                            {
                                Stat_Imgs[i].loc = loc + new Vector2(42, 28);
                                Stat_Imgs[i].draw(sprite_batch, Data_Offset);
                            }
                        }
                        // Name 2
                        Name2.loc = loc + new Vector2(44, 0);
                        Name2.draw(sprite_batch, Data_Offset);
                        // HP 2
                        HP_Counter2.loc = loc + new Vector2(24, 16);
                        HP_Counter2.draw(sprite_batch, Data_Offset);
                    }
                }
        }
    }
}
