using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Windows.Command
{
    class Window_Command_Support : Window_Command
    {
        const int WIDTH = 128;
        const int LINES = 9;
        //const int HEIGHT = LINES * 16 + 24; //Debug

        private int ActorId;
        private Support_Command_Components Header;

        #region Accessors
        private Game_Actor actor { get { return Global.game_actors[ActorId]; } }

        public int TargetId { get { return this.actor.support_candidates()[this.index]; } }
        #endregion

        public Window_Command_Support(int actorId, Vector2 loc)
        {
            ActorId = actorId;
            List<string> strs = new List<string>();
            Header = new Support_Command_Components(LINES, this.actor.supports_remaining);

            foreach (int actor_id in this.actor.support_candidates())
            {

                if (Global.battalion.actors.Contains(actor_id))
                {
                    Game_Actor other_actor = Global.game_actors[actor_id];
                    strs.Add(other_actor.name);
                }
                else
                {
                    strs.Add("-----");
                }
            }

            initialize(loc, WIDTH, strs);
            Bar_Offset = new Vector2(0, 0);
            Window_Img.set_lines(LINES, (int)Size_Offset.Y + 8);
            //Window_Img.height = HEIGHT; //Debug
        }

        protected override void initialize_window()
        {
            Window_Img = new Prepartions_Item_Window(true);
        }

        protected Texture2D map_sprite_texture(int actor_id, bool deployed)
        {
            return Scene_Map.get_team_map_sprite(
                    deployed ? Constants.Team.PLAYER_TEAM : 0, Global.game_actors.get_map_sprite_name(actor_id));
        }

        protected override void add_commands(List<string> strs)
        {
            var nodes = new List<CommandUINode>();
            var supports = this.actor.support_candidates();
            for (int i = 0; i < supports.Count; i++)
            {
                var text_node = item(strs[i], i);
                nodes.Add(text_node);
            }

            set_nodes(nodes);
        }

        protected override CommandUINode item(object value, int i)
        {
            var text_node = new SupportUINode(
                "", ActorId, this.actor.support_candidates()[i],
                value as string, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }

        protected override void draw_text(SpriteBatch sprite_batch)
        {
            Header.draw(sprite_batch, -(loc + draw_vector()));
            base.draw_text(sprite_batch);
        }
    }

    class Support_Command_Components : Sprite
    {
        const string FILENAME = @"Graphics/Windowskins/Support_Components";
        private int Lines;
        private FE_Text Remaining_Label, X_Label, Remaining_Count;

        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Remaining_Label.stereoscopic = value;
                X_Label.stereoscopic = value;
                Remaining_Count.stereoscopic = value;
            }
        }

        public Support_Command_Components(int lines, int remaining)
        {
            texture = Global.Content.Load<Texture2D>(FILENAME);
            Lines = lines;

            Remaining_Label = new FE_Text();
            Remaining_Label.loc = new Vector2(24, 0);
            Remaining_Label.Font = "FE7_Text";
            Remaining_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + (remaining == 0 ? "Grey" : "White"));
            Remaining_Label.text = "Remaining";
            X_Label = new FE_Text();
            X_Label.loc = new Vector2(72, 0);
            X_Label.Font = "FE7_Text";
            X_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + (remaining == 0 ? "Grey" : "White"));
            X_Label.text = "x";
            Remaining_Count = new FE_Text_Int();
            Remaining_Count.loc = new Vector2(96, 0);
            Remaining_Count.Font = "FE7_Text";
            Remaining_Count.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + (remaining == 0 ? "Grey" : "Blue"));
            Remaining_Count.text = remaining.ToString();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    // Header
                    sprite_batch.Draw(texture, (loc + new Vector2(16, -8) + draw_vector()) - draw_offset,
                        new Rectangle(0, (Global.game_options.window_color + 1) * 16, 104, 16), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Header Label
                    sprite_batch.Draw(texture, (loc + new Vector2(16, -8) + draw_vector()) - draw_offset,
                        new Rectangle(0, 0, 104, 16), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Footer
                    Vector2 footer_loc = new Vector2(Config.WINDOW_WIDTH - 104, loc.Y - draw_offset.Y + Lines * 16 + 16);
                    sprite_batch.Draw(texture, (footer_loc + draw_vector()),
                        new Rectangle(0, 80, 104, 16), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    Remaining_Label.draw(sprite_batch, -footer_loc);
                    X_Label.draw(sprite_batch, -footer_loc);
                    Remaining_Count.draw(sprite_batch, -footer_loc);
                }
        }
    }
}
