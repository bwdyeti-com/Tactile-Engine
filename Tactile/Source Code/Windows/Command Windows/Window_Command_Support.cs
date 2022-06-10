using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface.Command;

namespace Tactile.Windows.Command
{
    class Window_Command_Support : Window_Command_Scrollbar
    {
        const int WIDTH = 128;
        const int LINES = 9;

        protected int ActorId;
        private Support_Command_Components Header;

        #region Accessors
        private Game_Actor actor { get { return Global.game_actors[ActorId]; } }
        
        public int TargetId { get { return this.SupportPartners[this.index]; } }

        protected virtual int SupportsRemaining { get { return this.actor.supports_remaining; } }

        protected virtual List<int> SupportPartners { get { return this.actor.support_candidates(); } }
        #endregion

        public Window_Command_Support(int actorId, Vector2 loc)
        {
            Rows = LINES;

            ActorId = actorId;
            Header = new Support_Command_Components(LINES, this.SupportsRemaining);

            List<string> strs = GetNames();

            initialize(loc, WIDTH, strs);
            Bar_Offset = new Vector2(0, 0);
            Window_Img.set_lines(LINES, (int)Size_Offset.Y + 4);
        }

        protected override void set_default_offsets(int width)
        {
            this.text_offset = new Vector2(0, 0);
            this.glow_width = width - (24 + (int)(Text_Offset.X * 2));
            Bar_Offset = new Vector2(0, 0);
            Size_Offset = new Vector2(0, 0);
        }

        protected virtual List<string> GetNames()
        {
            List<string> strs = new List<string>();
            foreach (int actor_id in this.SupportPartners)
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
            return strs;
        }
        
        protected override void initialize_window()
        {
            Window_Img = new Prepartions_Item_Window(true);
        }
        
        protected override CommandUINode item(object value, int i)
        {
            var text_node = new SupportUINode(
                "", ActorId, this.SupportPartners[i],
                value as string, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            base.draw_window(sprite_batch);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Header.draw(sprite_batch, -(loc + draw_vector()));

            // Draw the top entry of the list, so it overlaps the header
            DrawFirstVisibleRow(sprite_batch);
            sprite_batch.End();
        }

        protected override void draw_text(SpriteBatch sprite_batch)
        {
            DrawRangeText(sprite_batch);
        }
    }

    class Support_Command_Components : Sprite
    {
        const string FILENAME = @"Graphics/Windowskins/Support_Components";
        private int Lines;
        private int Color_Override = -1;
        private TextSprite Remaining_Label, X_Label, Remaining_Count;

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

        public int color_override
        {
            set
            {
                Color_Override = (int)MathHelper.Clamp(value, -1, Constants.Team.NUM_TEAMS - 1);
            }
        }
        public int window_color
        {
            get
            {
                return Color_Override != -1 ? Color_Override : Global.game_options.window_color;
            }
        }

        public Support_Command_Components(int lines, int remaining, bool noRemainingPositive = false)
        {
            texture = Global.Content.Load<Texture2D>(FILENAME);
            Lines = lines;
            
            string labelColor = "White";
            string valueColor = "Blue";
            if (remaining == 0)
            {
                if (noRemainingPositive)
                    valueColor = "Green";
                else
                {
                    labelColor = "Grey";
                    valueColor = "Grey";
                }
            }

            Remaining_Label = new TextSprite();
            Remaining_Label.loc = new Vector2(24, 0);
            Remaining_Label.SetFont(Config.UI_FONT, Global.Content, labelColor);
            Remaining_Label.text = "Remaining";
            X_Label = new TextSprite();
            X_Label.loc = new Vector2(72, 0);
            X_Label.SetFont(Config.UI_FONT, Global.Content, labelColor);
            X_Label.text = "x";
            Remaining_Count = new RightAdjustedText();
            Remaining_Count.loc = new Vector2(96, 0);
            Remaining_Count.SetFont(Config.UI_FONT, Global.Content, valueColor);
            Remaining_Count.text = remaining.ToString();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    // Header
                    sprite_batch.Draw(texture, (loc + new Vector2(16, -8) + draw_vector()) - draw_offset,
                        new Rectangle(0, (this.window_color + 1) * 16, 104, 16), tint, angle, offset, scale,
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
