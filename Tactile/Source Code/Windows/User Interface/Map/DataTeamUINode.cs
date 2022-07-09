using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Windows.UserInterface.Map
{
    class DataTeamUINode : UINode
    {
        bool Active = false;
        private Data_Team_Window Window;
        private List<TextSprite> GroupNames, GroupCounts;

        #region Accessors
        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Window.stereoscopic = value;
            }
        }

        protected override IEnumerable<Inputs> ValidInputs
        {
            get
            {
                yield break;
            }
        }
        protected override bool RightClickActive { get { return false; } }
        #endregion

        internal DataTeamUINode(int team, List<string> names, List<int> counts)
        {
            int count = Math.Min(names.Count, counts.Count);

            Window = new Data_Team_Window(team, count);

            GroupNames = new List<TextSprite>();
            GroupCounts = new List<TextSprite>();
            for (int i = 0; i < count; i++)
            {
                GroupNames.Add(new TextSprite());
                GroupNames[GroupNames.Count - 1].loc = new Vector2(4, 12 + i * 16);
                GroupNames[GroupNames.Count - 1].SetFont(Config.UI_FONT, Global.Content, "White");
                GroupNames[GroupNames.Count - 1].text = names[i];

                GroupCounts.Add(new RightAdjustedText());
                GroupCounts[GroupCounts.Count - 1].loc = new Vector2(112, 12 + i * 16);
                GroupCounts[GroupCounts.Count - 1].SetFont(Config.UI_FONT, Global.Content, "Blue");
                GroupCounts[GroupCounts.Count - 1].text = counts[i].ToString();
            }

            Size = new Vector2(Data_Team_Window.WIDTH - 4, 16 + 16 * count);
        }

        protected override void update_graphics(bool activeNode)
        {
            Active = activeNode;

            Window.update();
            foreach (var name in GroupNames)
                name.update();
            foreach (var count in GroupCounts)
                count.update();
        }

        protected override void mouse_off_graphic() { }
        protected override void mouse_highlight_graphic() { }
        protected override void mouse_click_graphic() { }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = (draw_offset + this.offset) -
                (this.loc + this.draw_offset + stereo_offset());

            // Use effect shader if this node is active
            Effect background_shader = Global.effect_shader();
            if (Active)
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, background_shader);
            else
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Window.draw(sprite_batch, loc);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            foreach (var name in GroupNames)
                name.draw(sprite_batch, loc);
            foreach (var count in GroupCounts)
                count.draw(sprite_batch, loc);
            sprite_batch.End();
        }
    }
}
