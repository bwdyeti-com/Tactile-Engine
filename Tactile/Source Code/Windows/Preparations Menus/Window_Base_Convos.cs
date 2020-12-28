using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Menus;
using Tactile.Windows.Command;
using TactileLibrary;

namespace Tactile.Windows
{
    //@Debug: the stars should still be part of the text items instead of drawn on top
    class Window_Base_Convos : Window_Command
    {
        private List<Sprite> Priority_Stars;

        public Window_Base_Convos(Vector2 loc) :
            base(loc, 200, Global.game_state.base_event_names())
        {
            this.text_offset = new Vector2(16, 0);
            this.stereoscopic = Config.PREPMAIN_TALK_DEPTH;

            initialize_sprites();
        }

        private void initialize_sprites()
        {
            Texture2D star_texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Ranking_Star");
            Priority_Stars = new List<Sprite>();
            List<int> priorities = Global.game_state.base_event_priorities();
            for (int i = 0; i < priorities.Count; i++)
            {
                bool ready = true;
                if (!Config.BASE_EVENT_ACTIVATED_INVISIBLE)
                    if (!Global.game_state.base_event_ready(i))
                    {
                        set_text_color(i, "Grey");
                        ready = false;
                    }
                for (int j = 0; j < priorities[i]; j++)
                {
                    Priority_Stars.Add(new Sprite(star_texture));
                    Priority_Stars[Priority_Stars.Count - 1].draw_offset = new Vector2(128 + j * 16, i * 16 + 8);
                    Priority_Stars[Priority_Stars.Count - 1].stereoscopic = Config.PREPMAIN_TALK_DEPTH;
                    if (!ready)
                        Priority_Stars[Priority_Stars.Count - 1].tint = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            base.draw(spriteBatch);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            foreach (Sprite star in Priority_Stars)
                star.draw(spriteBatch, -this.loc);
            spriteBatch.End();
        }
    }
}
