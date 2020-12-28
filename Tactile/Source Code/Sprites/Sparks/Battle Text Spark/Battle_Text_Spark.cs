using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    abstract class Battle_Text_Spark : Sprite
    {
        readonly static Color[] DEFAULT_COLORS = new Color[] {
            new Color(32, 8, 0), new Color(224, 96, 80), new Color(248, 112, 96), new Color(248, 160, 176),
                new Color(248, 200, 216), new Color(248, 248, 248), new Color(64, 64, 0), new Color(112, 112, 8)
        };
        readonly static List<Color>[] TEAM_COLORS = new List<Color>[]{
            new List<Color>{ new Color(0, 8, 40), new Color(88, 176, 224), new Color(96, 192, 248),
                new Color(152, 240, 248), new Color(200, 248, 248) },
            new List<Color>{ new Color(32, 8, 0), new Color(224, 96, 80), new Color(248, 112, 96),
                new Color(248, 160, 176), new Color(248, 200, 216) },
            new List<Color>{ new Color(0, 24, 0), new Color(72, 216, 160), new Color(88, 240, 176),
                new Color(152, 248, 184), new Color(200, 248, 216) },
            new List<Color>{ new Color(24, 24, 24), new Color(144, 144, 144), new Color(168, 168, 168),
                new Color(208, 208, 208), new Color(232, 232, 232) }
        };

        protected Vector2 Size = new Vector2(0, 0);
        protected int Timer = 0;
        protected int Team = 0;
        protected bool Remove = false;
        protected int Remove_Timer = 0;
        public bool finished = false;

        #region Accessors
        public int team
        {
            set
            {
                int team = (int)MathHelper.Clamp(value, 1, Constants.Team.NUM_TEAMS);
                if (team != Team)
                {
                    Team = team;
                    team_recolor();
                }
            }
        }
        #endregion

        protected void initialize()
        {
            texture = get_texture();
            Src_Rect = new Rectangle(0, 0, (int)Size.X, (int)Size.Y);
            loc.Y = 21;
        }

        protected void team_recolor()
        {
            Texture2D base_texture = get_texture();
            if (base_texture == null)
                return;
            if (Team - 1 < TEAM_COLORS.Length)
            {
                List<Color> color_list = TEAM_COLORS[Team - 1];
                Color[] data = new Color[base_texture.Width * base_texture.Height];

                base_texture.GetData<Color>(data);

                for (int y = 0; y < base_texture.Height; y++)
                    for (int x = 0; x < base_texture.Width; x++)
                        for (int i = 0; i < color_list.Count; i++)
                        {
                            Color color_from = DEFAULT_COLORS[i];
                            Color color_to = color_list[i];
                            if (data[x + y * base_texture.Width] == color_from)
                                data[x + y * base_texture.Width] = color_to;
                        }
                texture = (Global.Content as ContentManagers.ThreadSafeContentManager).texture_from_size(base_texture.Width, base_texture.Height);
                texture.SetData<Color>(data);
                Global.Battle_Textures.Add(texture);
            }
            else
                texture = base_texture;
        }

        protected abstract Texture2D get_texture();

        public override void update()
        {
            update_frame();
            if (Remove)
            {
                Remove_Timer--;
                if (Remove_Timer <= 0)
                    finished = true;
            }

        }

        protected virtual void update_frame() { }

        public virtual void remove() { }
        public virtual void remove(int time) { }
    }
}
