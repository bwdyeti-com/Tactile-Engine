using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Title_Background : Sprite
    {
        const int BG_TEXTURE_COLUMNS = 5;
        const int BG_UPDATE = 2;
        const int BG_FRAMES = 16;
        const int COLUMN_UPDATE = 2;
        const int INITIAL_DISTANCE = -260;
        const float SPACING = 2;
        const int INTERVAL = (int)(335 * SPACING);
        const float DISTANCE_RATIO = (INTERVAL * 2 / SPACING) * ((float)COLUMN_UPDATE / (float)BG_UPDATE);
        const int COLUMN_COUNT = (int)(12 / SPACING);

        int Frame = 0;
        int Timer = -1;
        List<Title_Column> Columns = new List<Title_Column>();

        public Title_Background()
        {
            textures = new List<Texture2D> { Global.Content.Load<Texture2D>(@"Graphics/Titles/Title BG"),
                Global.Content.Load<Texture2D>(@"Graphics/Pictures/Column") };
            for (int i = 1; i <= COLUMN_COUNT; i++)
                add_column(i * INTERVAL + INITIAL_DISTANCE, i == COLUMN_COUNT ? 0 : 255);
            update();
        }

        public override void update()
        {
            Timer++;
            if (Timer % COLUMN_UPDATE == 0)
            {
                foreach (Title_Column column in Columns)
                {
                    column.distance -= ((DISTANCE_RATIO * COLUMN_UPDATE / (BG_FRAMES * BG_UPDATE)));
                    copy_stereo(column);
                    column.update();
                }
                if (Columns[0].distance <= 0)
                {
                    float offset = Columns[Columns.Count - 1].distance;
                    Columns.RemoveAt(0);
                    if (!Title_Column.SINGLE_PILLAR)
                        Columns.RemoveAt(0);
                    add_column(INTERVAL + offset, 0);
                }
            }
            if (Timer % BG_UPDATE == 0)
                Frame = (Frame + 1) % BG_FRAMES;
        }

        protected void add_column(float distance, int opacity)
        {
            Columns.Add(new Title_Column(textures[1]));
            Columns[Columns.Count - 1].distance = distance;
            Columns[Columns.Count - 1].opacity = opacity;
            if (!Title_Column.SINGLE_PILLAR)
            {
                Columns.Add(new Title_Column_Other(textures[1]));
                Columns[Columns.Count - 1].distance = distance;
                Columns[Columns.Count - 1].opacity = opacity;
            }
        }

        public override Rectangle src_rect
        {
            get
            {
                int height = (int)Math.Ceiling(BG_FRAMES / (float)BG_TEXTURE_COLUMNS);
                return new Rectangle((Frame % BG_TEXTURE_COLUMNS) * (textures[0].Width / BG_TEXTURE_COLUMNS),
                    (Frame / BG_TEXTURE_COLUMNS) * (textures[0].Height / height),
                    textures[0].Width / BG_TEXTURE_COLUMNS, textures[0].Height / height);
            }
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (textures.Count > 0)
                if (visible)
                {
                    // Background
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(textures[0], this.loc + draw_vector(),
                        src_rect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    sprite_batch.End();
                    // Columns
                    Effect column_shader = Global.effect_shader();
                    if (column_shader != null)
                        column_shader.CurrentTechnique = column_shader.Techniques["Technique2"];
                    for (int i = Columns.Count - 1; i >= 0; i--)
                    {
                        Title_Column column = Columns[i];
                        if (column_shader != null)
                            column_shader.Parameters["color_shift"].SetValue(column.color.ToVector4());
                        sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, column_shader);
                        column.draw(sprite_batch);
                        sprite_batch.End();
                    }
                }
        }
    }
}
