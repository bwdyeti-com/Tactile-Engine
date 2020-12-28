using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile.Windows.Command.Items
{
    class StealStatsPanel : StatsPanel
    {
        private Window_Steal_Panel FaceBg;
        protected TextSprite Name;

        public StealStatsPanel(Game_Actor actor)
            : base(actor)
        {
            // Background
            FaceBg = new Window_Steal_Panel();
            FaceBg.loc = Vector2.Zero;
            // Name
            Name = new TextSprite();
            //Name.loc = Face_Bg.loc + new Vector2(12, 8); //Debug
            Name.loc = FaceBg.loc + new Vector2(32, 8);
            Name.SetFont(Config.UI_FONT, Global.Content, "White");
            Name.text += actor.name;
            Name.offset = new Vector2((int)(Font_Data.text_width(Name.text) * 0.4f), 0);
        }

        protected override void set_face(Game_Actor actor)
        {
            Face = new Status_Face(actor);
            Face.loc = new Vector2(48, 96);
        }

        public override void refresh_info(Game_Actor actor, Data_Equipment item,
            int[] statValues, int[] baseStatValues)
        {
            // No item or weapon
            if (item == null || is_weapon_highlighted(item))
            {
                Item_Description.text = "";
            }
            // Item
            else
            {
                Item_Description.text = "";
                if (item.is_weapon)
                    Item_Description.SetColor(Global.Content, "Grey");
                else
                    Item_Description.SetColor(Global.Content,
                        Combat.can_use_item(actor, item.Id, false) ?
                        "White" : "Grey");
                string[] desc_ary = item.Quick_Desc.Split('|');
                for (int i = 0; i < desc_ary.Length; i++)
                    Item_Description.text += desc_ary[i] + "\n";
            }
        }

        public override void Draw(SpriteBatch sprite_batch)
        {
            Vector2 data_draw_offset = loc + draw_vector();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            draw_window(sprite_batch, data_draw_offset);
            Name.draw(sprite_batch, -data_draw_offset);
            sprite_batch.End();

            Face.draw(sprite_batch, -data_draw_offset);
        }

        protected override void draw_window(
            SpriteBatch sprite_batch, Vector2 data_draw_offset)
        {
            FaceBg.draw(sprite_batch, -data_draw_offset);
        }
    }

    class Window_Steal_Panel : Sprite
    {
        readonly static Vector2 SIZE = new Vector2(16, 16);
        const int HEADER_HEIGHT = 24;
        private int Width, Height;

        public int width
        {
            get { return Width + 16; }
            set { Width = value; }
        }
        public int height
        {
            get { return Height + 16; }
            set { Height = value; }
        }

        public Window_Steal_Panel()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Steal_Window");
            width = (int)Face_Sprite_Data.STATUS_FACE_SIZE.X;
            height = (int)Face_Sprite_Data.STATUS_FACE_SIZE.Y;
        }

        new public virtual Rectangle src_rect(int frame, int width, int height)
        {
            int x = 0;
            int y = HEADER_HEIGHT;
            switch (frame)
            {
                // Bottom
                case 1:
                    return new Rectangle(x + 0, y + 32, width, height);
                case 2:
                    return new Rectangle(x + 16, y + 32, width, height);
                case 3:
                    return new Rectangle(x + 32, y + 32, width, height);
                // Middle
                case 4:
                    return new Rectangle(x + 0, y + 16, width, height);
                case 5:
                    return new Rectangle(x + 16, y + 16, width, height);
                case 6:
                    return new Rectangle(x + 32, y + 16, width, height);
                // Top row
                case 7:
                    return new Rectangle(x + 0, y + 0, width, height);
                case 8:
                    return new Rectangle(x + 16, y + 0, width, height);
                case 9:
                    return new Rectangle(x + 32, y + 0, width, height);
            }
            if (frames.Count == 0)
                return new Rectangle(0, 0, texture.Width, texture.Height);
            else if (current_frame < 0)
                return new Rectangle(0, 0, 0, 0);
            else
                return frames[current_frame];
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (!(texture == null))
                if (visible)
                {
                    // Draw main panel
                    Vector2 offset = this.offset;
                    Rectangle src_rect;
                    int y = 0;
                    int temp_height;
                    while (y < height)
                    {
                        temp_height = (int)SIZE.Y;
                        if (height - (y + (int)SIZE.Y) < (int)SIZE.Y && height - (y + (int)SIZE.Y) != 0)
                            temp_height = height - (y + (int)SIZE.Y);
                        int x = 0;
                        int temp_width;
                        while (x < width)
                        {
                            temp_width = (int)SIZE.X;
                            if (width - (x + (int)SIZE.X) < (int)SIZE.X && width - (x + (int)SIZE.X) != 0)
                                temp_width = width - (x + (int)SIZE.X);
                            if (x == 0)
                            {
                                src_rect = this.src_rect((y == 0 ? 7 : (height - y <= (int)SIZE.X ? 1 : 4)),
                                    temp_width, temp_height);
                                if (mirrored) offset.X = src_rect.Width - offset.X;
                                sprite_batch.Draw(texture, (loc + new Vector2(0, HEADER_HEIGHT - 8) + draw_vector()) - draw_offset, src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                            }
                            else if (width - x <= (int)SIZE.X)
                            {
                                src_rect = this.src_rect((y == 0 ? 9 : (height - y <= (int)SIZE.X ? 3 : 6)),
                                    temp_width, temp_height);
                                if (mirrored) offset.X = src_rect.Width - offset.X;
                                sprite_batch.Draw(texture, (loc + new Vector2(0, HEADER_HEIGHT - 8) + draw_vector()) - draw_offset, src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                            }
                            else
                            {
                                src_rect = this.src_rect((y == 0 ? 8 : (height - y <= (int)SIZE.X ? 2 : 5)),
                                    temp_width, temp_height);
                                if (mirrored) offset.X = src_rect.Width - offset.X;
                                sprite_batch.Draw(texture, (loc + new Vector2(0, HEADER_HEIGHT - 8) + draw_vector()) - draw_offset, src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                            }
                            x += temp_width;
                        }
                        y += temp_height;
                    }
                    // Draw header
                    sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset, new Rectangle(0, 0, 80, HEADER_HEIGHT),
                            tint, angle, offset - Vector2.Zero, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
