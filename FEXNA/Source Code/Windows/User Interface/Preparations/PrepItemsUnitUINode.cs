using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.UserInterface.Preparations
{
    class PrepItemsUnitUINode : UINode
    {
        protected Character_Sprite MapSprite;
        protected FE_Text Name;
        protected Unit_Line_Cursor GlowingLine;

        protected override IEnumerable<Inputs> ValidInputs
        {
            get
            {
                yield return Inputs.A;
                yield return Inputs.R;
            }
        }
        protected override bool RightClickActive { get { return true; } }

        internal PrepItemsUnitUINode(string name)
        {
            Name = new FE_Text();
            Name.draw_offset = new Vector2(16, 0);
            Name.Font = "FE7_Text";
            Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Name.text = name;
            // Map Sprite
            MapSprite = new Character_Sprite();
            MapSprite.facing_count = 3;
            MapSprite.frame_count = 3;
            MapSprite.draw_offset = new Vector2(8, 16);
            MapSprite.mirrored = Constants.Team.flipped_map_sprite(Constants.Team.PLAYER_TEAM);

            GlowingLine = new Unit_Line_Cursor(64);
            GlowingLine.draw_offset = new Vector2(0, 8);
            GlowingLine.visible = false;
        }

        protected override void update_graphics(bool activeNode)
        {
            Name.update();
            MapSprite.frame = Global.game_system.unit_anim_idle_frame;
            MapSprite.update();

            GlowingLine.update();
            GlowingLine.visible = activeNode;
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
            Name.texture = Global.Content.Load<Texture2D>(
                string.Format(@"Graphics/Fonts/FE7_Text_{0}", color));
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
            GlowingLine.draw(sprite_batch, draw_offset - loc);

            MapSprite.draw(sprite_batch, draw_offset - loc);
            Name.draw(sprite_batch, draw_offset - loc);
        }
    }
}
