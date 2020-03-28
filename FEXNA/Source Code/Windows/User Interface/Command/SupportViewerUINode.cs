using Microsoft.Xna.Framework;

namespace FEXNA.Windows.UserInterface.Command
{
    class SupportViewerUINode : SupportUINode
    {
        internal SupportViewerUINode(
                string helpLabel,
                int actorId,
                int targetActorId,
                string str,
                int width)
            : base(helpLabel, actorId, targetActorId, str, width)
        {
            Rank.text = "";
        }

        protected override bool DisplayedActor(int actorId)
        {
            return Global.progress.recruitedActors.Contains(actorId);
        }

        protected override bool SupportEnabled(int actorId, int targetActorId)
        {
            return true;
        }
        protected override string MapSpriteName(int actorId)
        {
            var actorData = Global.data_actors[actorId];
            var classData = Global.data_classes[actorData.ClassId];
            return Game_Actors.get_map_sprite_name(classData.Name, actorData.ClassId, actorData.Gender);
        }
        protected override int GetAffinity(int actorId)
        {
            return (int)Global.data_actors[actorId].Affinity;
        }
        protected override string RankText(int actorId, int targetActorId)
        {
            return Constants.Support.SUPPORT_LETTERS[0];
        }

        protected override void set_map_sprite_texture(bool deployed, string name)
        {
            MapSprite.texture = Scene_Map.get_team_map_sprite(
                Constants.Team.PLAYER_TEAM, name);
            if (MapSprite.texture != null)
                MapSprite.offset = new Vector2(
                    (MapSprite.texture.Width / MapSprite.frame_count) / 2,
                    (MapSprite.texture.Height / MapSprite.facing_count) - 8);
            int alpha = deployed ? 255 : 128;
            MapSprite.tint = new Color(alpha, alpha, alpha, 255);
        }
    }
}
