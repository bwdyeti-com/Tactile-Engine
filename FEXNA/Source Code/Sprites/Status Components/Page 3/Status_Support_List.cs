using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    class Status_Support_List : Stereoscopic_Graphic_Object
    {
        private List<Icon_Sprite> Support_Icons = new List<Icon_Sprite>();
        private List<FE_Text> Support_Names = new List<FE_Text>(), Support_Letters = new List<FE_Text>();

        #region Accessors
        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                foreach (Icon_Sprite icon in Support_Icons)
                    icon.stereoscopic = value;
                foreach (FE_Text name in Support_Names)
                    name.stereoscopic = value;
                foreach (FE_Text letter in Support_Letters)
                    letter.stereoscopic = value;
            }
        }
        #endregion

        public void set_images(Game_Actor actor)
        {
            // Supports
            Support_Icons.Clear();
            Support_Names.Clear();
            Support_Letters.Clear();
            foreach (int i in actor.supports.Keys)
            {
                if (!Global.game_actors.ContainsKey(i))
                    continue;
                Support_Icons.Add(new Icon_Sprite());
                Support_Icons[Support_Icons.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Affinity Icons");
                Support_Icons[Support_Icons.Count - 1].size = new Vector2(16, 16);
                Support_Icons[Support_Icons.Count - 1].loc = new Vector2(0, (Support_Icons.Count - 1) * 16);
                Support_Icons[Support_Icons.Count - 1].index = (int)Global.game_actors[i].affin;
                copy_stereo(Support_Icons[Support_Icons.Count - 1]);
                string name_color = "White";
                if (actor.is_support_maxed())
                    name_color = "Green";
                Support_Names.Add(new FE_Text());
                Support_Names[Support_Names.Count - 1].loc = new Vector2(24, (Support_Names.Count - 1) * 16);
                Support_Names[Support_Names.Count - 1].Font = "FE7_Text";
                Support_Names[Support_Names.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + name_color);
                Support_Names[Support_Names.Count - 1].text = Global.game_actors[i].name;
                copy_stereo(Support_Names[Support_Names.Count - 1]);
                string letter_color = "Blue";
                if (actor.supports[i] >= Constants.Support.MAX_SUPPORT_LEVEL || actor.is_support_maxed(false, i))
                    letter_color = "Green";
                Support_Letters.Add(new FE_Text());
                Support_Letters[Support_Letters.Count - 1].loc = new Vector2(72, (Support_Letters.Count - 1) * 16);
                Support_Letters[Support_Letters.Count - 1].Font = "FE7_TextL";
                Support_Letters[Support_Letters.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + letter_color);
                Support_Letters[Support_Letters.Count - 1].text = Constants.Support.SUPPORT_LETTERS[actor.supports[i]];
                copy_stereo(Support_Letters[Support_Letters.Count - 1]);

            }
        }

        public void update()
        {
            foreach (Icon_Sprite icon in Support_Icons)
                icon.update();
            foreach (FE_Text name in Support_Names)
                name.update();
        }

        public void draw(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            foreach (Icon_Sprite icon in Support_Icons)
                icon.draw(sprite_batch, draw_offset - this.loc);
            foreach (FE_Text name in Support_Names)
                name.draw(sprite_batch, draw_offset - this.loc);
            foreach (FE_Text letter in Support_Letters)
                letter.draw(sprite_batch, draw_offset - this.loc);
        }
    }
}
