using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Turn_Change_Effect : Sprite
    {
        const bool TRACK_TILES = false;

        static int MAX_OPACITY = 118;
        static int MIDDLE_ROW = (Config.WINDOW_HEIGHT / Constants.Map.TILE_SIZE) / 2;
        const int INTRO_TIME = 8;
        const int MAIN_TIME = 60;
        const int OUTRO_TIME = 22;
        const int TIMER_MAX = INTRO_TIME + MAIN_TIME + OUTRO_TIME;
        protected int Timer = TIMER_MAX;
        protected int Turn;
        protected int Effect_Opacity = 0;

        protected int TILE_SIZE { get { return Constants.Map.TILE_SIZE; } }

        #region Accessors
        private int time_into_intro { get { return TIMER_MAX - Timer; } }
        private int time_into_main { get { return (TIMER_MAX - INTRO_TIME) - Timer; } }
        private int time_until_main_midpoint { get { return MAIN_TIME / 2 - time_into_main; } }
        private int time_into_outro { get { return OUTRO_TIME - Timer; } }
        #endregion

        public Turn_Change_Effect()
        {
            Turn = 1;
            initialize();
        }

        public Turn_Change_Effect(int turn)
        {
            Turn = turn;
            initialize();
        }

        protected virtual void initialize()
        {
            loc.X = Config.WINDOW_WIDTH / 2 - (TILE_SIZE * 7 - (TILE_SIZE * 3 / 4));
            loc.Y = MIDDLE_ROW * TILE_SIZE - (TILE_SIZE * 3 / 4);
            update();
        }

        public bool is_finished { get { return Timer < 0; } }

        public override void update()
        {
            if (time_into_intro == INTRO_TIME)
                Global.game_system.play_se(System_Sounds.Turn_Change);

            offset.X = ((int)((time_until_main_midpoint > 0 ? -1 : 1) *
                Math.Pow(Math.Abs(time_until_main_midpoint / 10.0), 5)));
            int opacity = Math.Min(255, 256 - (int)(Math.Pow(Math.Abs(time_until_main_midpoint / 10.0), 5) / 25 * 256));
            tint = new Color(opacity, opacity, opacity, opacity);
            Timer--;
            // Squares fading in
            if (time_until_main_midpoint > 0)
                Effect_Opacity = (int)(MAX_OPACITY * (((TIMER_MAX - (INTRO_TIME - 1)) - Timer) / ((MAIN_TIME) / 2.0f)));
            // Squares fading out
            else if (Timer > OUTRO_TIME)
                Effect_Opacity = (int)(MAX_OPACITY * ((Timer - OUTRO_TIME) / ((MAIN_TIME) / 2.0f)));
        }

        public override Rectangle src_rect
        {
            get
            {
                return new Rectangle(0, 16 + (Turn - 1) * 24, 256, 24);
            }
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            bool track_tiles = TRACK_TILES && Global.game_options.controller != 2;
            if (!(texture == null))
            {
                // Draws turn text
                if (visible)
                {
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(texture, loc, src_rect, tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
                // Draws change effects
                Vector2 size = new Vector2(Config.WINDOW_WIDTH / TILE_SIZE, Config.WINDOW_HEIGHT / TILE_SIZE);
                if (track_tiles)
                    size = size + new Vector2(1, 1);
                Rectangle effect_rect = new Rectangle(64, 0, TILE_SIZE, TILE_SIZE);
                Color effect_tint = new Color(Effect_Opacity, Effect_Opacity, Effect_Opacity, Effect_Opacity);
                for (int y = 0; y < size.Y; y++)
                    for (int x = 0; x < size.X; x++)
                    {
                        Vector2 pos = new Vector2(x * TILE_SIZE, y * TILE_SIZE);
                        if (track_tiles)
                        {
                            Vector2 offset = new Vector2(Global.game_map.target_display_loc.X % 16,
                                Global.game_map.target_display_loc.Y % 16);
                            if (offset.X == 0)
                                offset.X = TILE_SIZE;
                            if (offset.Y == 0)
                                offset.Y = TILE_SIZE;
                            pos -= new Vector2(TILE_SIZE - offset.X, TILE_SIZE - offset.Y);
                        }
                        if (y == MIDDLE_ROW || y + 1 == MIDDLE_ROW)
                            continue;
                        int frame = 0;
                        // Squares fading in
                        if (time_until_main_midpoint > 0)
                            frame = (int)MathHelper.Clamp(((TIMER_MAX - INTRO_TIME) - (TILE_SIZE - (int)size.X) -
                                (Timer + x - y)) / 2, 0, 8);
                        // Squares fading out
                        else if (Timer > OUTRO_TIME)
                            frame = (int)MathHelper.Clamp(((Timer + x - y) + (41 + (int)size.Y) -
                                (TIMER_MAX - INTRO_TIME)) / 2, 0, 8);
                        int cell_height = TILE_SIZE;
                        if (y > MIDDLE_ROW)
                        {
                            cell_height = ((y * TILE_SIZE) - ((int)size.Y * (TILE_SIZE / 2))) - ((int)Math.Pow((time_until_main_midpoint / 5.0f), 2));
                            cell_height = (int)MathHelper.Clamp(cell_height, 0, TILE_SIZE);
                            pos.Y -= cell_height - TILE_SIZE;
                            effect_rect = new Rectangle(frame * TILE_SIZE, TILE_SIZE - cell_height, TILE_SIZE, cell_height);
                        }
                        else
                        {
                            cell_height = (MIDDLE_ROW - 1) * TILE_SIZE - ((int)Math.Pow((time_until_main_midpoint / 5.0f), 2)) - y * TILE_SIZE;
                            cell_height = (int)MathHelper.Clamp(cell_height, 0, TILE_SIZE);
                            effect_rect = new Rectangle(frame * TILE_SIZE, 0, TILE_SIZE, cell_height);
                        }
                        sprite_batch.Draw(texture, pos, effect_rect, effect_tint);
                    }
            }
        }
    }
}
