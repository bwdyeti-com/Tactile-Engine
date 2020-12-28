using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Arena_Background : Sprite
    {
        readonly static int[][] ARENA_DEPTH = new int[][]
        {
            new int[] { 28, 8 },
            new int[] { 12, 7 },
            new int[] { 40, 6 },
            new int[] { 10, 5 },
            new int[] { 10, 4 },
            new int[] { 10, 3 },
            new int[] { 10, 2 },
            new int[] { 20, 1 },
            new int[] { 20, 0 },
        };

        protected int Bg_Timer;

        public Arena_Background(Texture2D texture) : base(texture) { }

        public override void update()
        {
            stereoscopic = Config.BATTLE_PLATFORM_BASE_DEPTH; //@Yeti // should be in a few layers
            base.update();
            Bg_Timer = (Bg_Timer + 1) % Global.BattleSceneConfig.ArenaBgTime;
            src_rect = new Rectangle(
                0,
                160 * (Bg_Timer / (Global.BattleSceneConfig.ArenaBgTime /
                    Global.BattleSceneConfig.ArenaBgFrames)),
                Config.WINDOW_WIDTH,
                160);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    int y = 160 * (Bg_Timer / (Global.BattleSceneConfig.ArenaBgTime /
                        Global.BattleSceneConfig.ArenaBgFrames));
                    int oy = 0;
                    for (int i = 0; i < ARENA_DEPTH.Length; i++)
                    {
                        stereoscopic = ARENA_DEPTH[i][1];
                        Rectangle src_rect = new Rectangle(0, y + oy, Config.WINDOW_WIDTH, ARENA_DEPTH[i][0]);
                        Vector2 offset = this.offset;
                        sprite_batch.Draw(texture,
                            (loc + draw_vector() + new Vector2(0, oy)) - draw_offset,
                            src_rect, tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        oy += ARENA_DEPTH[i][0];
                    }
                }
        }
    }
}
