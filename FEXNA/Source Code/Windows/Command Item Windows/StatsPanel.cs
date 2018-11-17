using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA_Library;

namespace FEXNA.Windows.Command.Items
{
    class StatsPanel : Stereoscopic_Graphic_Object
    {
        private bool ShowingDescription = true;
        private System_Color_Window Info_Window;
        protected Face_Sprite Face;
        private Weapon_Type_Icon Type_Icon;
        private List<FE_Text> Stat_Labels = new List<FE_Text>();
        private List<FE_Text_Int> Stats = new List<FE_Text_Int>();
        private List<Weapon_Triangle_Arrow> Arrows = new List<Weapon_Triangle_Arrow>();
        protected FE_Text Item_Description;
        private WeaponTriangleHelpSet WTHelp;

        public StatsPanel(Game_Actor actor)
        {
            // Face
            set_face(actor);
            Info_Window = new System_Color_Window();
            Info_Window.width = 120;
            Info_Window.height = 64;
            Info_Window.loc = loc +
                new Vector2(160, Config.WINDOW_HEIGHT - 88);
            Info_Window.loc = new Vector2(0, 0);
            // Stats
            for (int i = 0; i < 4; i++)
            {
                Stat_Labels.Add(new FE_Text());
                Stat_Labels[i].loc = new Vector2(10 + (i % 2) * 58, 24 + (i / 2) * 16);
                Stat_Labels[i].Font = "FE7_Text";
                Stat_Labels[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            }
            Stat_Labels[0].text = "Atk";
            Stat_Labels[1].text = "Crit";
            Stat_Labels[2].text = "Hit";
            Stat_Labels[3].text = "AS";
            //Stat_Labels[3].text = "Avoid"; //Debug
            Stat_Labels.Add(new FE_Text());
            Stat_Labels[4].loc = new Vector2(40, 8);
            Stat_Labels[4].Font = "FE7_Text";
            Stat_Labels[4].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Stat_Labels[4].text = "Affi";
            for (int i = 0; i < 4; i++)
            {
                Stats.Add(new FE_Text_Int());
                Stats[i].loc = new Vector2(46 + (i % 2) * 62, 24 + (i / 2) * 16);
                Stats[i].Font = "FE7_Text";
                Stats[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            }
            for (int i = 0; i < 4; i++)
            {
                Arrows.Add(new Weapon_Triangle_Arrow());
                Arrows[i].loc = new Vector2(34 + (i % 2) * 62, 28 + (i / 2) * 16 - 4);
            }
            // Weapon Type Icon
            Type_Icon = new Weapon_Type_Icon();
            Type_Icon.loc = new Vector2(56, 8);
            // Description
            Item_Description = new FE_Text();
            Item_Description.loc = new Vector2(8, 8);
            Item_Description.Font = "FE7_Text";
            Item_Description.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            // Weapon Triangle
            WTHelp = new WeaponTriangleHelpSet();
            WTHelp.loc = new Vector2(-24, 16 + 8);
        }

        protected virtual void set_face(Game_Actor actor)
        {
            Face = new Face_Sprite(actor.face_name, true);
            if (actor.generic_face)
                Face.recolor_country(actor.name_full);
            Face.convo_placement_offset = true;
            Face.expression = Face.status_frame;
            Face.phase_in();
            Face.loc = new Vector2(244, Config.WINDOW_HEIGHT - 76);
            Face.loc = new Vector2(60, 4);
        }

        public virtual void refresh_info(Game_Actor actor, Data_Equipment item,
            int[] statValues, int[] baseStatValues)
        {
            // No item
            if (item == null)
            {
                Item_Description.text = "";
            }
            // Weapon
            else if (is_weapon_highlighted(item))
            {
                ShowingDescription = false;
                var type = (item as Data_Weapon).main_type();
                Stats[0].text = statValues[0].ToString();
                Stats[1].text = ((Data_Weapon)item).Crt < 0 ? "--" : statValues[1].ToString();
                Stats[2].text = statValues[2].ToString();
                Stats[3].text = statValues[3].ToString();
                for (int i = 0; i < 4; i++)
                    Arrows[i].value = statValues[i] == baseStatValues[i] ?
                        WeaponTriangle.Nothing : (statValues[i] > baseStatValues[i] ?
                            WeaponTriangle.Advantage : WeaponTriangle.Disadvantage);
                Type_Icon.index = type.IconIndex;
            }
            // Item
            else
            {
                ShowingDescription = true;
                Item_Description.text = "";
                string[] desc_ary = item.Quick_Desc.Split('|');
                for (int i = 0; i < desc_ary.Length; i++)
                    Item_Description.text += desc_ary[i] + "\n";
            }
            
            WTHelp.set_item(item);
        }

        protected virtual bool is_weapon_highlighted(Data_Equipment item)
        {
            return item != null && item.is_weapon && !(item as Data_Weapon).is_staff();
        }

        public void Update()
        {
            Info_Window.update();
            Face.update();
            Type_Icon.update();
            foreach (var label in Stat_Labels)
                label.update();
            foreach (var stat in Stats)
                stat.update();
            foreach (var arrow in Arrows)
                arrow.update();
            Item_Description.update();
            WTHelp.update();
        }

        public virtual void Draw(SpriteBatch sprite_batch)
        {
            Vector2 data_draw_offset = loc + draw_vector();

            Face.draw(sprite_batch, -data_draw_offset);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            draw_window(sprite_batch, data_draw_offset);
            if (ShowingDescription)
            {
                Item_Description.draw(sprite_batch, -data_draw_offset);
            }
            else
            {
                Type_Icon.draw(sprite_batch, -data_draw_offset);
                foreach (FE_Text label in Stat_Labels)
                    label.draw(sprite_batch, -data_draw_offset);
                foreach (FE_Text_Int stat in Stats)
                    stat.draw(sprite_batch, -data_draw_offset);
                foreach (Weapon_Triangle_Arrow arrow in Arrows)
                    arrow.draw(sprite_batch, -data_draw_offset);
            }
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            WTHelp.draw(sprite_batch, -data_draw_offset);
            sprite_batch.End();
        }

        protected virtual void draw_window(SpriteBatch sprite_batch, Vector2 data_draw_offset)
        {
            Info_Window.draw(sprite_batch, -data_draw_offset);
        }
    }
}
