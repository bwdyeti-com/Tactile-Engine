using Microsoft.Xna.Framework;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.UserInterface.Command
{
    class MapSpriteUINode : TextUINode
    {
        private Character_Sprite MapSprite;

        internal MapSpriteUINode(string helpLabel, FE_Text text, int width) :
            base(helpLabel, text, width)
        {
            MapSprite = new Character_Sprite();
            MapSprite.draw_offset = new Vector2(8, 16);
            MapSprite.facing_count = 3;
            MapSprite.frame_count = 3;

            Text.draw_offset += new Vector2(16, 0);
        }

        public void set_map_sprite(string filename, int team)
        {
            MapSprite.texture = Scene_Map.get_team_map_sprite(
                team, filename);
            MapSprite.mirrored = Constants.Team.flipped_map_sprite(team);

            if (MapSprite.texture != null)
                MapSprite.offset = new Vector2(
                    (MapSprite.texture.Width / MapSprite.frame_count) / 2,
                    (MapSprite.texture.Height / MapSprite.facing_count) - 8);
            MapSprite.frame = Global.game_system.unit_anim_idle_frame;
        }
        public void clear_map_sprite()
        {
            MapSprite.texture = null;
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);
            MapSprite.update();
            MapSprite.frame = Global.game_system.unit_anim_idle_frame;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            MapSprite.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
