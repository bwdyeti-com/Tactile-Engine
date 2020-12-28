using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Windows.Title
{
    class WindowSupportViewerActorList : Preparations.WindowPrepActorList
    {
        const int COLUMNS = 4;
        const int ROW_SIZE = 16;

        private Dictionary<int, Dictionary<int, string>> SupportLevels;
        private SupportViewerFooter Footer;

        public WindowSupportViewerActorList()
        {
            loc.Y = Config.WINDOW_HEIGHT - (this.Height + 16);
            this.ColorOverride = 0;
            Footer = new SupportViewerFooter();
        }

        #region WindowPrepActorList Abstract
        protected override int Columns { get { return COLUMNS; } }
        protected override int VisibleRows { get { return (Config.WINDOW_HEIGHT - (Global.ActorConfig.NumItems + 1) * 16) / ROW_SIZE; } }
        protected override int RowSize { get { return ROW_SIZE; } }

        protected override List<int> GetActorList()
        {
            HashSet<int> actorIds = new HashSet<int>();
            foreach (var pair in Global.data_supports)
            {
                var supportData = pair.Value;
                actorIds.Add(supportData.Id1);
                actorIds.Add(supportData.Id2);
            }
            
            // Use data actors set as the base collection
            return Global.data_actors
                .Select(x => x.Key)
                // Actor has been recruited
                .Where(x => Global.progress.recruitedActors.Contains(x))
                // Actor has a support
                .Where(x => actorIds.Contains(x))
                .ToList();
        }

        protected override string ActorName(int actorId)
        {
            return Global.data_actors[actorId].Name.Split(Global.ActorConfig.ActorNameDelimiter)[0];
        }
        protected override string ActorMapSpriteName(int actorId)
        {
            var actorData = Global.data_actors[actorId];
            var classData = Global.data_classes[actorData.ClassId];
            return Game_Actors.get_map_sprite_name(classData.Name, actorData.ClassId, actorData.Gender);
        }

        protected override void refresh_font(int i)
        {
            int actorId = ActorList[i];
            bool forced = false, available;
            available = SupportLevels.ContainsKey(actorId) &&
                SupportLevels[actorId].Any(x => Global.progress.supports[x.Value] > 0);
            if (available)
            {
                forced = true;
                foreach (var tuple in Global.data_actors[actorId].SupportPartners(Global.data_supports, Global.data_actors))
                {
                    if (Global.progress.recruitedActors.Contains(tuple.Item2) &&
                        Global.progress.supports.ContainsKey(tuple.Item1))
                    {
                        if (Global.progress.supports[tuple.Item1] == Global.data_supports[tuple.Item1].MaxLevel)
                            continue;
                    }
                    
                    forced = false;
                    break;
                }

            }
            refresh_font(i, forced, available);
        }
        #endregion

        protected override void initialize()
        {
            SupportLevels = new Dictionary<int, Dictionary<int, string>>();
            foreach (var pair in Global.progress.supports)
            {
                var supportData = Global.data_supports[pair.Key];

                if (!SupportLevels.ContainsKey(supportData.Id1))
                    SupportLevels.Add(supportData.Id1, new Dictionary<int, string>());
                if (!SupportLevels.ContainsKey(supportData.Id2))
                    SupportLevels.Add(supportData.Id2, new Dictionary<int, string>());

                SupportLevels[supportData.Id1][supportData.Id2] = pair.Key;
                SupportLevels[supportData.Id2][supportData.Id1] = pair.Key;
            }

            base.initialize();
        }

        public override void update(bool active)
        {
            base.update(active);
            Footer.update();
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            base.draw_window(sprite_batch);
            Footer.draw(sprite_batch);
        }
    }

    class SupportViewerFooter : Sprite
    {
        const string FILENAME = @"Graphics/Windowskins/Support_Components";
        private TextSprite SuccessLabel, PercentLabel, SuccessCount;

        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                SuccessLabel.stereoscopic = value;
                PercentLabel.stereoscopic = value;
                SuccessCount.stereoscopic = value;
            }
        }
        
        public SupportViewerFooter()
        {
            this.texture = Global.Content.Load<Texture2D>(FILENAME);

            int supportProgress = Global.progress.SupportPercent;

            string labelColor = "White";
            string valueColor = "Blue";
            if (supportProgress == 100)
            {
                labelColor = "Green";
                valueColor = "Green";
            }

            SuccessLabel = new TextSprite();
            SuccessLabel.loc = new Vector2(24, 0);
            SuccessLabel.SetFont(Config.UI_FONT, Global.Content, labelColor);
            SuccessLabel.text = "Success";
            SuccessCount = new RightAdjustedText();
            SuccessCount.loc = new Vector2(88, 0);
            SuccessCount.SetFont(Config.UI_FONT, Global.Content, valueColor);
            SuccessCount.text = supportProgress.ToString();
            PercentLabel = new TextSprite();
            PercentLabel.loc = new Vector2(88, 0);
            PercentLabel.SetFont(Config.UI_FONT, Global.Content, labelColor);
            PercentLabel.text = "%";
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    // Footer
                    Vector2 footer_loc = new Vector2(
                        Config.WINDOW_WIDTH - 104,
                        Config.WINDOW_HEIGHT - 16 - draw_offset.Y);
                    sprite_batch.Draw(texture, (footer_loc + draw_vector()),
                        new Rectangle(0, 80, 104, 16), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    SuccessLabel.draw(sprite_batch, -footer_loc);
                    PercentLabel.draw(sprite_batch, -footer_loc);
                    SuccessCount.draw(sprite_batch, -footer_loc);
                }
        }
    }
}
