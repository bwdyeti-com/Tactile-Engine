using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TactileLibrary;

namespace Tactile
{
    class Tilemap : Stereoscopic_Graphic_Object
    {
        const int FRAME_COUNT = 16; // Ticks per frame of terrain animation
        const int CHANGING_TILE_TIME = 16;

        protected Texture2D Map_Texture, Map_Grid_Texture;
        protected List<Texture2D> Animated_Map_Textures;
        protected List<Rectangle> Animated_Tile_Data;
        protected Dictionary<Vector2, int> TileAnimationIndices;
        protected List<int> Frame_Maxes = new List<int>();
        protected List<int> Frames = new List<int>();
        protected int Tileset_Width;
        protected int Tileset_Height;
        protected int Timer = 0;
        protected List<Changing_Tile_Data> Changing_Tiles = new List<Changing_Tile_Data>();

        protected static int TILE_SIZE { get { return Constants.Map.TILE_SIZE; } }
        protected int TILESET_TILE_SIZE { get { return Constants.Map.TILESET_TILE_SIZE; } }

        #region Accessors
        public HashSet<Vector2> roof_tiles
        {
            get
            {
                return new HashSet<Vector2>(Changing_Tiles
                    .Where(x => x.roof)
                    .SelectMany(x => x.key)
                    .Select(x => new Vector2(x[0], x[1])));
            }
        }
        #endregion

        public Tilemap(Texture2D map_texture, List<Texture2D> animated_textures, List<Rectangle> animated_tileset_data)
        {
            Map_Texture = map_texture;
            Map_Grid_Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Map_Grid");
            Tileset_Width = Map_Texture.Width / TILESET_TILE_SIZE;
            Tileset_Height = Map_Texture.Height / TILESET_TILE_SIZE;
            Animated_Map_Textures = animated_textures;
            Animated_Tile_Data = animated_tileset_data;
            TileAnimationIndices = new Dictionary<Vector2, int>();
            for (int i = 0; i < Animated_Map_Textures.Count; i++)
            {
                Texture2D texture = Animated_Map_Textures[i];
                Rectangle texture_data = Animated_Tile_Data[i];
                Frame_Maxes.Add(texture.Width / (TILESET_TILE_SIZE * texture_data.Width));
                Frames.Add(0);

                for (int y = texture_data.Top; y < texture_data.Bottom; y++)
                    for (int x = texture_data.Left; x < texture_data.Right; x++)
                        TileAnimationIndices.Add(new Vector2(x, y), i);
            }
        }

        public void update()
        {
            Timer = (Timer + 1) % FRAME_COUNT;
            if (Timer == 0)
            {
                for (int i = 0; i < Frames.Count; i++)
                    Frames[i] = (Frames[i] + 1) % Frame_Maxes[i];
            }
            int j = 0;
            while (j < Changing_Tiles.Count)
            {
                Changing_Tiles[j].value--;
                if (Changing_Tiles[j].value <= 0)
                    Changing_Tiles.RemoveAt(j);
                else
                    j++;
            }
        }

        public void change_tile(Vector2 loc, int id, bool roof)
        {
            if (Changing_Tiles.Count == 0 || Changing_Tiles[Changing_Tiles.Count - 1].value < CHANGING_TILE_TIME)
            {
                Changing_Tiles.Add(new Changing_Tile_Data { key = new List<int[]>(), value = CHANGING_TILE_TIME, roof = roof });
            }
            Changing_Tiles[Changing_Tiles.Count - 1].key.Add(new int[] { (int)loc.X, (int)loc.Y, id });
        }

        public static Rectangle view_area()
        {
            return view_area(false);
        }
        public static Rectangle view_area(bool rotated)
        {
            if (rotated)
                return new Rectangle(0, 0, Global.game_map.width, Global.game_map.height);
            else
            {
                int start_x = Math.Max(0, Global.game_map.display_x / TILE_SIZE - 1);
                int start_y = Math.Max(0, Global.game_map.display_y / TILE_SIZE);
                //Rectangle result = new Rectangle(start_x, start_y,
                //    Math.Min(start_x + 1 + (Config.WINDOW_WIDTH / TILE_SIZE), Global.game_map.width),
                //    Math.Min(start_y + 1 + (Config.WINDOW_HEIGHT / TILE_SIZE), Global.game_map.height));
                Rectangle result = new Rectangle(start_x, start_y,
                    Math.Min(2 + (Config.WINDOW_WIDTH / TILE_SIZE), Global.game_map.width - start_x),
                    Math.Min(1 + (Config.WINDOW_HEIGHT / TILE_SIZE), Global.game_map.height - start_y));
                return result;
            }
        }

        public void draw(SpriteBatch sprite_batch, bool rotated, bool fow)
        {
            if (fow && !Global.game_map.fow)
                return;

            Rectangle area = view_area(rotated);
            int start_x = area.X;
            int start_y = area.Y;
            int width = area.Width + start_x;
            int height = area.Height + start_y;

            int gran = 1;// Game_Map.ALPHA_GRANULARITY; //Yeti
            int gran_size = TILE_SIZE / gran;

            Vector2 draw_vector = this.draw_vector() - Global.game_map.display_loc;
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            // Draw static tiles
            for (int y = start_y; y < height; y++)
                for (int x = start_x; x < width; x++)
                {
                    if (!fow_tile_draw(x, y, fow))
                        continue;

                    draw_tile(sprite_batch, x, y, gran, gran_size, draw_vector);
                }
            sprite_batch.End();
            // Draw animated tiles over that, if there are any
            if (Animated_Tile_Data.Count != 0)
            {
                sprite_batch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                for (int y = start_y; y < height; y++)
                    for (int x = start_x; x < width; x++)
                    {
                        if (!fow_tile_draw(x, y, fow))
                            continue;
                        draw_animated_tile(sprite_batch, x, y, gran, gran_size, draw_vector);
                    }
                sprite_batch.End();
            }
        }

        private void draw_tile(SpriteBatch sprite_batch, int x, int y, int gran, int gran_size, Vector2 draw_vector, Maybe<int> changing_tile_value = default(Maybe<int>))
        {
            int tile_x, tile_y;
            tile_data(x, y, out tile_x, out tile_y);

            draw_tile(sprite_batch, new Vector2(x, y), tile_x, tile_y, gran, gran_size, draw_vector, changing_tile_value);
        }
        private void draw_tile(SpriteBatch sprite_batch, Vector2 loc, int tile_x, int tile_y, int gran, int gran_size, Vector2 draw_vector, Maybe<int> changing_tile_value = default(Maybe<int>))
        {
            loc = loc * TILE_SIZE + draw_vector;
            Color tint = Color.White;
            if (changing_tile_value.IsSomething)
            {
                tint.R = (byte)((tint.R * changing_tile_value) / CHANGING_TILE_TIME);
                tint.G = (byte)((tint.G * changing_tile_value) / CHANGING_TILE_TIME);
                tint.B = (byte)((tint.B * changing_tile_value) / CHANGING_TILE_TIME);
                tint.A = (byte)((tint.A * changing_tile_value) / CHANGING_TILE_TIME);
            }
            for (int oy = 0; oy < gran; oy++)
                for (int ox = 0; ox < gran; ox++)
                {

                    //tint = Global.game_map.get_tint(x * gran + ox, y * gran + oy);
                    sprite_batch.Draw(Map_Texture, //Debug
                        loc + new Vector2(ox, oy) * gran_size,
                        new Rectangle((tile_x) * TILESET_TILE_SIZE + (TILESET_TILE_SIZE - TILE_SIZE) / 2 + (ox * gran_size),
                                        (tile_y) * TILESET_TILE_SIZE + (TILESET_TILE_SIZE - TILE_SIZE) / 2 + (oy * gran_size),
                        gran_size, gran_size), tint);
                }
        }

        private bool draw_animated_tile(SpriteBatch sprite_batch, int x, int y, int gran, int gran_size, Vector2 draw_vector, Maybe<int> changing_tile_value = default(Maybe<int>))
        {
            int tile_x, tile_y;
            tile_data(x, y, out tile_x, out tile_y);

            return draw_animated_tile(sprite_batch, new Vector2(x, y), tile_x, tile_y, gran, gran_size, draw_vector, changing_tile_value);
        }
        private bool draw_animated_tile(SpriteBatch sprite_batch, Vector2 loc, int tile_x, int tile_y, int gran, int gran_size, Vector2 draw_vector, Maybe<int> changing_tile_value = default(Maybe<int>))
        {
            Vector2 tile_id_loc = new Vector2(tile_x, tile_y);
            if (TileAnimationIndices.ContainsKey(tile_id_loc))
            //if (Animated_Tile_Data.Any(rect => rect.Contains(tile_x, tile_y))) //Debug
            {
                int animated_index = TileAnimationIndices[tile_id_loc];
                //int animated_index = Animated_Tile_Data.FindIndex(rect => rect.Contains(tile_x, tile_y)); //Debug
                var animation_data = Animated_Tile_Data[animated_index];
                tile_x += -animation_data.X + (Frames[animated_index] * animation_data.Width);
                tile_y += -animation_data.Y;

                loc = loc * TILE_SIZE + draw_vector;
                Color tint = Color.White;
                if (changing_tile_value.IsSomething)
                {
                    tint.R = (byte)((tint.R * changing_tile_value) / CHANGING_TILE_TIME);
                    tint.G = (byte)((tint.G * changing_tile_value) / CHANGING_TILE_TIME);
                    tint.B = (byte)((tint.B * changing_tile_value) / CHANGING_TILE_TIME);
                    tint.A = (byte)((tint.A * changing_tile_value) / CHANGING_TILE_TIME);
                }
                for (int oy = 0; oy < gran; oy++)
                    for (int ox = 0; ox < gran; ox++)
                    {
                        sprite_batch.Draw(Animated_Map_Textures[animated_index],
                                loc + new Vector2(ox, oy) * gran_size,
                            //new Rectangle((tile_x) * TILE_SIZE + (ox * gran_size), (tile_y) * TILE_SIZE + (oy * gran_size),
                                new Rectangle((tile_x) * TILESET_TILE_SIZE + (TILESET_TILE_SIZE - TILE_SIZE) / 2 + (ox * gran_size),
                                                (tile_y) * TILESET_TILE_SIZE + (TILESET_TILE_SIZE - TILE_SIZE) / 2 + (oy * gran_size),
                                gran_size, gran_size), tint);
                    }
                return true;
            }
            return false;
        }

        private bool fow_tile_draw(int x, int y, bool fow)
        {
            bool tile_visible = !Global.game_map.fow || Global.game_map.fow_visibility[Constants.Team.PLAYER_TEAM].Contains(new Vector2(x, y));
            return tile_visible ^ fow;
            //return !((!Global.game_map.fow || Global.game_map.fow_visibility[Config.PLAYER_TEAM].Contains(new Vector2(x, y))) ^ !fow); //Debug
        }

        private bool tile_data(int x, int y, out int tile_x, out int tile_y)
        {
            int id = Global.game_map.map_data.GetValue(x, y);
            tile_data(id, out tile_x, out tile_y);
            return true;
        }
        private bool tile_data(int id, out int tile_x, out int tile_y)
        {
            tile_x = id % Tileset_Width;
            tile_y = id / Tileset_Width;
            return true;
        }

        public void draw_changing_tiles(SpriteBatch sprite_batch, bool rotated, bool fow, bool roof)
        {
            Rectangle area = view_area(rotated);
            int start_x = area.X;
            int start_y = area.Y;
            int width = area.Width + start_x;
            int height = area.Height + start_y;

            int gran = 1;// Game_Map.ALPHA_GRANULARITY;
            int gran_size = TILE_SIZE / gran;

            Vector2 draw_vector = this.draw_vector() - Global.game_map.display_loc;
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            foreach (Changing_Tile_Data tile_datas in Changing_Tiles)
            {
                if (tile_datas.roof == roof)
                    foreach (int[] data in tile_datas.key)
                    {
                        int x = data[0];
                        int y = data[1];
                        if (x < start_x || x >= width || y < start_y || y >= height)
                            continue;
                        if (!fow_tile_draw(x, y, fow))
                            continue;
                        int id = data[2];
                        int tile_x, tile_y;
                        tile_data(id, out tile_x, out tile_y);

                        bool animated_tile = draw_animated_tile(
                            sprite_batch, new Vector2(x, y),
                            tile_x, tile_y, gran, gran_size, draw_vector, tile_datas.value);
                        if (!animated_tile)
                        {
                            draw_tile(sprite_batch, new Vector2(x, y),
                                tile_x, tile_y, gran, gran_size, draw_vector, tile_datas.value);
                        }
                    }
            }
            sprite_batch.End();
        }

        public void draw_grid(SpriteBatch sprite_batch, bool rotated)
        {
            Rectangle area = view_area(rotated);
            int start_x = area.X;
            int start_y = area.Y;
            int width = area.Width + start_x;
            int height = area.Height + start_y;

            int map_width = Global.game_map.width;
            int map_height = Global.game_map.height;
            int x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            if (Global.game_options.controller != 2)
            {
                x1 = Global.game_map.edge_offset_left;
                x2 = Global.game_map.edge_offset_right;
                y1 = Global.game_map.edge_offset_top;
                y2 = Global.game_map.edge_offset_bottom;
            };
            bool outline = x1 > 0 || x2 > 0 || x2 > 0 || y2 > 0;
            int ox, oy;
            int texture_cell_width = Map_Grid_Texture.Width / 3;
            Color tint1 = new Color(
                Global.game_map.grid_opacity, Global.game_map.grid_opacity, Global.game_map.grid_opacity, Global.game_map.grid_opacity);
            Color tint2 = new Color(128, 128, 128, 128);
            Vector2 draw_vector = this.draw_vector() - Global.game_map.display_loc;
            for (int y = start_y; y < height; y++)
                for (int x = start_x; x < width; x++)
                {
                    // If outside playable area, next
                    if (x < x1 || (x + x2) >= map_width)
                        continue;
                    if (y < y1 || (y + y2) >= map_height)
                        continue;
                    // Draw grid
                    if (Global.game_options.controller == 2)
                    {
                        ox = 1;
                        oy = 1;
                    }
                    else
                    {
                        ox = !outline ? 1 : (x == x1 ? 0 : x == map_width - (x2 + 1) ? 2 : 1);
                        oy = !outline ? 1 : (y == y1 ? 0 : y == map_height - (y2 + 1) ? 2 : 1);
                    }
                    sprite_batch.Draw(Map_Grid_Texture,
                        new Vector2(x, y) * TILE_SIZE + draw_vector,
                        new Rectangle(
                            ox * texture_cell_width,
                            oy * texture_cell_width + (Map_Grid_Texture.Height / 2),
                            texture_cell_width, texture_cell_width), tint1);
                    // Draw outline
                    //if (Global.game_options.controller != 2 && outline && (ox != 1 || oy != 1))
                    //{
                    //    sprite_batch.Draw(Map_Grid_Texture,
                    //        new Vector2(x, y) * TILE_SIZE + draw_vector() - display_loc,
                    //        new Rectangle(ox * TILE_SIZE, oy * TILE_SIZE,
                    //        TILE_SIZE, TILE_SIZE), tint2);
                    //}
                }
        }
        public void draw_grid_outline(SpriteBatch sprite_batch, bool rotated)
        {
            // This seems easier than the below? //Debug
            if (Global.game_options.controller == 2)
                return;
            Rectangle area = view_area(rotated);
            int start_x = area.X;
            int start_y = area.Y;
            int width = area.Width + start_x;
            int height = area.Height + start_y;

            int map_width = Global.game_map.width;
            int map_height = Global.game_map.height;
            int x1 = Global.game_map.edge_offset_left;
            int x2 = Global.game_map.edge_offset_right;
            int y1 = Global.game_map.edge_offset_top;
            int y2 = Global.game_map.edge_offset_bottom;
            bool outline = x1 > 0 || x2 > 0 || x2 > 0 || y2 > 0;
            if (!outline)
                return;
            int ox, oy;
            int texture_cell_width = Map_Grid_Texture.Width / 3;
            Color tint1 = new Color(
                Global.game_map.grid_opacity, Global.game_map.grid_opacity, Global.game_map.grid_opacity, Global.game_map.grid_opacity);
            Color tint2 = new Color(128, 128, 128, 128);
            Vector2 draw_vector = this.draw_vector() - Global.game_map.display_loc;
            if (Global.game_options.controller != 2 && outline)
                for (int y = start_y; y < height; y++)
                    for (int x = start_x; x < width; x++)
                    {
                        // If outside playable area, next
                        if (x < x1 || (x + x2) >= map_width)
                            continue;
                        if (y < y1 || (y + y2) >= map_height)
                            continue;
                        // Draw grid
                        if (Global.game_options.controller == 2)
                        {
                            ox = 1;
                            oy = 1;
                        }
                        else
                        {
                            ox = !outline ? 1 : (x == x1 ? 0 : x == map_width - (x2 + 1) ? 2 : 1);
                            oy = !outline ? 1 : (y == y1 ? 0 : y == map_height - (y2 + 1) ? 2 : 1);
                        }
                        // Draw outline
                        if (ox != 1 || oy != 1)
                        {
                            sprite_batch.Draw(Map_Grid_Texture,
                                new Vector2(x, y) * TILE_SIZE + draw_vector,
                                new Rectangle(
                                    ox * texture_cell_width,
                                    oy * texture_cell_width,
                                    texture_cell_width, texture_cell_width), tint2);
                        }
                    }
        }
    }

    class Changing_Tile_Data
    {
        public List<int[]> key;
        public int value;
        public bool roof;
    }
}
