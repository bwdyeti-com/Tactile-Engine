using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Status;

namespace FEXNA
{
    abstract class Status_Page : Graphic_Object
    {
        protected UINodeSet<StatusUINode>StatusPageNodes;
        // BG Design
        protected Sprite Window_Design;

        public void init_design()
        {
            // Window Design
            Window_Design = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Pictures/Status_Window_Design"));
            Window_Design.loc = new Vector2(14, 90);
            Window_Design.src_rect = new Rectangle(0, 96 * Global.game_options.window_color, 292, 96);
            Window_Design.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH; //Yeti
        }

        public abstract void set_images(Game_Unit unit);

        public virtual void update()
        {
            Window_Design.update();
        }

        public void refresh(Game_Unit unit)
        {
            foreach (StatusUINode node in StatusPageNodes)
            {
                node.refresh(unit);
            }
        }

        public IEnumerable<StatusUINode> node_union(IEnumerable<StatusUINode> nodes)
        {
            return nodes.Union(StatusPageNodes);
        }

        public abstract void draw(SpriteBatch sprite_batch, Vector2 draw_offset);
    }
}
