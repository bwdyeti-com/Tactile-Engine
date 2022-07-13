using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;

namespace Tactile.Windows.UserInterface.Options
{
    class UnitScreenUINode : UINode
    {
        protected Character_Sprite MapSprite;
        protected TextSprite Name;

        protected override IEnumerable<Inputs> ValidInputs
        {
            get
            {
                yield return Inputs.A;
                yield return Inputs.R;
            }
        }
        protected override bool RightClickActive { get { return true; } }

        internal UnitScreenUINode(string name)
        {
            Name = new TextSprite();
            Name.draw_offset = new Vector2(16, 0);
            Name.SetFont(Config.UI_FONT, Global.Content, "White");
            Name.text = name;
            // Map Sprite
            MapSprite = new Character_Sprite();
            MapSprite.facing_count = 3;
            MapSprite.frame_count = 3;
            MapSprite.draw_offset = new Vector2(8, 16);
            MapSprite.mirrored = Constants.Team.flipped_map_sprite(Constants.Team.PLAYER_TEAM);

            MapSprite.frame = Global.game_system.unit_anim_idle_frame;
            MapSprite.update();
        }

        protected override void update_graphics(bool activeNode)
        {
            Name.update();
            MapSprite.frame = Global.game_system.unit_anim_idle_frame;
            MapSprite.update();
        }

        internal void set_map_sprite_texture(int team, string name)
        {
            MapSprite.texture = Scene_Map.get_team_map_sprite(team, name);
            if (MapSprite.texture != null)
                MapSprite.offset = new Vector2(
                    (MapSprite.texture.Width / MapSprite.frame_count) / 2,
                    (MapSprite.texture.Height / MapSprite.facing_count) - 8);
        }
        internal void set_name_texture(string color)
        {
            Name.SetColor(Global.Content, color);
        }

        protected override void mouse_off_graphic()
        {
            MapSprite.tint = Color.White;
            Name.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            MapSprite.tint = Color.White;
            Name.tint = Color.White;
        }
        protected override void mouse_click_graphic()
        {
            MapSprite.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
            Name.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            MapSprite.draw(sprite_batch, draw_offset - loc);
            Name.draw(sprite_batch, draw_offset - loc);
        }
    }
}
