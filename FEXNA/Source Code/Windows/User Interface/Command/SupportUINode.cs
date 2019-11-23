using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.UserInterface.Command
{
    class SupportUINode : CommandUINode
    {
        protected Character_Sprite MapSprite;
        protected FE_Text Text;
        private Sprite Affinity;
        protected FE_Text_Int Rank;

        internal SupportUINode(
                string helpLabel,
                int actorId,
                int targetActorId,
                string str,
                int width)
            : base(helpLabel)
        {
            Size = new Vector2(width, 16);

            Text = new FE_Text();
            Text.draw_offset = new Vector2(16, 0);
            Text.Font = "FE7_Text";
            Text.texture = Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_White");
            Text.text = str;

            // Map Sprite
            MapSprite = new Character_Sprite();
            MapSprite.facing_count = 3;
            MapSprite.frame_count = 3;
            MapSprite.draw_offset = new Vector2(8, 16);
            MapSprite.mirrored = Constants.Team.flipped_map_sprite(Constants.Team.PLAYER_TEAM);

            Rank = new FE_Text_Int();
            Rank.draw_offset = new Vector2(104, 0);
            Rank.Font = "FE7_TextL";
            
            if (DisplayedActor(targetActorId))
            {
                set_map_sprite_texture(true, MapSpriteName(targetActorId));

                Text.texture = Global.Content.Load<Texture2D>(
                    string.Format(@"Graphics\Fonts\FE7_Text_{0}",
                    SupportEnabled(actorId, targetActorId) ? "White" : "Grey"));

                Rank.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" +
                    (SupportEnabled(actorId, targetActorId) ? "White" : "Grey"));
                Rank.text = RankText(actorId, targetActorId);

                Icon_Sprite icon = new Icon_Sprite();
                icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Affinity Icons");
                icon.size = new Vector2(16, 16);
                icon.index = GetAffinity(targetActorId);
                Affinity = icon;
            }
            else
            {
                set_map_sprite_texture(false, MapSpriteName(targetActorId));

                Text.texture = Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_Grey");

                Rank.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
                Rank.text = Constants.Support.SUPPORT_LETTERS[0];

                FE_Text affinity = new FE_Text();
                affinity.Font = "FE7_Text";
                affinity.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
                affinity.text = "--";
                Affinity = affinity;
            }
            MapSprite.frame = Global.game_system.unit_anim_idle_frame;

            Affinity.draw_offset = new Vector2(64, 0);
        }

        protected virtual bool DisplayedActor(int actorId)
        {
            return Global.battalion.actors.Contains(actorId);
        }

        protected virtual bool SupportEnabled(int actorId, int targetActorId)
        {
            var actor = Global.game_actors[actorId];
            return actor.is_support_ready(targetActorId);
        }
        protected virtual string MapSpriteName(int actorId)
        {
            return Global.game_actors[actorId].map_sprite_name;
        }
        protected virtual int GetAffinity(int actorId)
        {
            return (int)Global.game_actors[actorId].affin;
        }
        protected virtual string RankText(int actorId, int targetActorId)
        {
            var actor = Global.game_actors[actorId];
            return actor.supports.ContainsKey(targetActorId) ?
                Constants.Support.SUPPORT_LETTERS[actor.supports[targetActorId]] :
                Constants.Support.SUPPORT_LETTERS[0];
        }

        internal override void set_text_color(string color)
        {
            //Text.change_text_color(color); //Debug
        }

        protected override void update_graphics(bool activeNode)
        {
            Text.update();
            MapSprite.frame = Global.game_system.unit_anim_idle_frame;
            MapSprite.update();
        }

        protected virtual void set_map_sprite_texture(bool deployed, string name)
        {
            int team = deployed ? Constants.Team.PLAYER_TEAM : 0;
            MapSprite.texture = Scene_Map.get_team_map_sprite(team, name);
            if (MapSprite.texture != null)
                MapSprite.offset = new Vector2(
                    (MapSprite.texture.Width / MapSprite.frame_count) / 2,
                    (MapSprite.texture.Height / MapSprite.facing_count) - 8);
        }

        protected override void mouse_off_graphic()
        {
            Text.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            Text.tint = FEXNA.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            Text.tint = FEXNA.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            MapSprite.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Text.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Rank.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Affinity.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
