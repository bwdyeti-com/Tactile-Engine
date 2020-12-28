using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EnumExtension;

namespace Tactile
{
    enum Minimap_Attack_Range
    {
        None = 0,
        Attack_All = 1 << 0,
        Attack = 1 << 1,
        Staff_All = 1 << 2,
        Staff = 1 << 3
    }
    class Window_Minimap
    {
        enum Directions { L, R, U, D }

        const int MINIMAP_TILE_SIZE = 4;
        const int TIMER = 20, UNIT_TIME_MAX = 64, VIEW_TIME_MAX = 32;
        const int GREY_ALPHA = 64;
        private bool Active = false, Closing = false;
        private int Timer = TIMER;
        private int View_Timer = 0, Unit_Timer = 0;
        private int View_Alpha = 0, Unit_Alpha = 0;
        private Texture2D Minimap_Texture, Sprite_Texture;
        private Vector2 Map_Offset, View_Offset, View_Area;
        private static int[,] Data;
        private static bool[,] Fow;
        private List<Minimap_Unit> Units;
        private Minimap_Attack_Range[,] Attack_Range;
        Sprite Grey_Fill;

        #region Accessors
        public bool active { get { return Active; } }

        public bool closing { get { return Closing; } }

        public float angle
        {
            get
            {
                if (Active)
                    return 0;
                return MathHelper.PiOver2 * (Closing ? -1 * (TIMER - Timer) : Timer) / ((float)TIMER);
            }
        }
        public float scale
        {
            get
            {
                if (Active)
                    return 1;
                return (Closing ? Timer : TIMER - Timer) / ((float)TIMER);
            }
        }
        #endregion

        public static void clear()
        {
            Data = null;
            Fow = null;
        }

        public Window_Minimap()
        {
            setup();
            update();
        }

        private void setup()
        {
            Window_Minimap.clear();
            if (Data == null)
            {
                if (Global.game_map.width > 0)
                    setup_minimap();
                else
                    return;
            }
            setup_units();
            Map_Offset = (new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) -
                new Vector2(Global.game_map.width, Global.game_map.height) * MINIMAP_TILE_SIZE) / 2;
            View_Area = (new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) * MINIMAP_TILE_SIZE) /
                Constants.Map.TILE_SIZE - new Vector2(8) + new Vector2(2);
            Minimap_Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Minimap_Tiles");
            Sprite_Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Minimap_Sprites");
            Grey_Fill = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            Grey_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Grey_Fill.tint = new Color(0, 0, 0, 0);
        }

        private void setup_minimap()
        {
            Data = new int[Global.game_map.width, Global.game_map.height];
            Fow = new bool[Global.game_map.width, Global.game_map.height];
            for(int y = 0; y < Global.game_map.height; y++)
                for (int x = 0; x < Global.game_map.width; x++)
                {
                    int tag = Global.game_map.terrain_tag(x, y);
                    int tile_id = 0;
                    if (Global.data_terrains.ContainsKey(tag))
                         tile_id = Global.data_terrains[tag].Minimap;
                    switch (tile_id)
                    {
                        // Doors
                        case 7:
                            if (Global.game_map.terrain_tag(x + 1, y) == tag)
                                tile_id = 22;
                            else if (Global.game_map.terrain_tag(x - 1, y) == tag)
                                tile_id = 23;
                            break;
                        // Bridge
                        case 16:
                            check_bridge(x, y, tag, ref tile_id);
                            break;
                        // Stairs
                        case 17:
                            check_stairs(x, y, tag, ref tile_id);
                            break;
                        // Walls/Rivers/Lakes/Roads
                        case 32:
                        case 48:
                        case 64:
                        case 80:
                            int mod = 0;
                            foreach (Tuple<Vector2, int> offset in new Tuple<Vector2, int>[]
                                {
                                    new Tuple<Vector2, int>(new Vector2(-1,  0), 1),
                                    new Tuple<Vector2, int>(new Vector2( 1,  0), 2),
                                    new Tuple<Vector2, int>(new Vector2( 0, -1), 4),
                                    new Tuple<Vector2, int>(new Vector2( 0,  1), 8),
                                })
                            {
                                if (Global.data_terrains[tag].Minimap_Group.Contains(Global.game_map.terrain_tag(new Vector2(x, y) + offset.Item1)))
                                    mod += offset.Item2;
                                else if (Global.game_map.terrain_tag(new Vector2(x, y) + offset.Item1) == tag)
                                    mod += offset.Item2;
                            }
                            tile_id += mod;
                            break;
                        // Cliffs
                        case 96:
                            check_cliff(x, y, tag, ref tile_id);
                            break;
                    }
                    Data[x, y] = tile_id;
                    Fow[x, y] = Global.game_map.fow &&
                        !Global.game_map.fow_visibility[Constants.Team.PLAYER_TEAM].Contains(new Vector2(x, y));
                }
        }

        private void check_bridge(int x, int y, int tag, ref int tile_id)
        {
            // Check for adjacent bridges
            foreach (Vector2 offset in new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0) })
                if (Global.game_map.terrain_tag(new Vector2(x, y) + offset) == tag)
                {
                    tile_id = 16;
                    return;
                }
            foreach (Vector2 offset in new Vector2[] { new Vector2(0, -1), new Vector2(0, 1) })
                if (Global.game_map.terrain_tag(new Vector2(x, y) + offset) == tag)
                {
                    tile_id = 24;
                    return;
                }
            // Check for adjacent rivers
            foreach (Vector2 offset in new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0) })
                if (new List<int> { 16 }.Contains(Global.game_map.terrain_tag(new Vector2(x, y) + offset)))
                {
                    tile_id = 24;
                    return;
                }
            foreach (Vector2 offset in new Vector2[] { new Vector2(0, -1), new Vector2(0, 1) })
                if (new List<int> { 16 }.Contains(Global.game_map.terrain_tag(new Vector2(x, y) + offset)))
                {
                    tile_id = 16;
                    return;
                }
        }

        private void check_stairs(int x, int y, int tag, ref int tile_id)
        {
            foreach (Vector2 offset in new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) })
                if (Global.game_map.terrain_tag(new Vector2(x, y) + offset) == tag)
                {
                    tile_id = 18;
                    return;
                }
            HashSet<Directions> adjacent_walls = new HashSet<Directions>(
                new Tuple<Vector2, Directions>[]
                        {
                            new Tuple<Vector2, Directions>(new Vector2(-1,  0), Directions.L),
                            new Tuple<Vector2, Directions>(new Vector2( 1,  0), Directions.R),
                            new Tuple<Vector2, Directions>(new Vector2( 0, -1), Directions.U),
                            new Tuple<Vector2, Directions>(new Vector2( 0,  1), Directions.D),
                        }
                    .Where(v => Global.data_terrains[Global.game_map.terrain_tag(new Vector2(x, y) + v.Item1)].Minimap == 32)
                    .Select(v => v.Item2));
            if (adjacent_walls.Count == 2)
                if ((adjacent_walls.Contains(Directions.L) && adjacent_walls.Contains(Directions.R)) ||
                    (adjacent_walls.Contains(Directions.U) && adjacent_walls.Contains(Directions.D)))
                {
                    tile_id = 18;
                    return;
                }
        }

        private void check_cliff(int x, int y, int tag, ref int tile_id)
        {
            bool water = check_for_water(x, y, tag, tile_id);
            // If bordering water
            if (water)
            {
                List<int> dir = new List<int>();
                foreach (Tuple<Vector2, Directions> offset in new Tuple<Vector2, Directions>[]
                        {
                            new Tuple<Vector2, Directions>(new Vector2(-1,  0), Directions.L),
                            new Tuple<Vector2, Directions>(new Vector2( 1,  0), Directions.R),
                            new Tuple<Vector2, Directions>(new Vector2( 0, -1), Directions.U),
                            new Tuple<Vector2, Directions>(new Vector2( 0,  1), Directions.D),
                        })
                    if (new List<int> { 16, 21, 22 }.Contains(Global.game_map.terrain_tag(new Vector2(x, y) + offset.Item1)))
                        dir.Add((int)offset.Item2);
                if (dir.Count != 4)
                {
                    if (dir.Count == 3)
                    {
                        if (dir.Except(new List<int> { (int)Directions.L, (int)Directions.R }).Count() == 1)
                            dir = dir.Except(new List<int> { (int)Directions.L, (int)Directions.R }).ToList();
                        else
                            dir = dir.Except(new List<int> { (int)Directions.U, (int)Directions.D }).ToList();
                    }
                    if (dir.Count == 2)
                    {
                        List<int> ary = new List<int>();
                        foreach (Tuple<Vector2, Directions> offset in new Tuple<Vector2, Directions>[]
                                {
                                    new Tuple<Vector2, Directions>(new Vector2(-1,  0), Directions.L),
                                    new Tuple<Vector2, Directions>(new Vector2( 1,  0), Directions.R),
                                    new Tuple<Vector2, Directions>(new Vector2( 0, -1), Directions.U),
                                    new Tuple<Vector2, Directions>(new Vector2( 0,  1), Directions.D),
                                })
                            if (Global.game_map.terrain_tag(new Vector2(x, y) + offset.Item1) == tag)
                                ary.Add((int)offset.Item2);
                        if (ary.Count == 1)
                            dir = dir.Except(ary).ToList();
                    }
                    int mod;
                    if (dir.Contains((int)Directions.L))
                    {
                        if (dir.Contains((int)Directions.U))
                            mod = 5;
                        else if (dir.Contains((int)Directions.D))
                            mod = 7;
                        else
                            mod = 6;
                    }
                    else if (dir.Contains((int)Directions.R))
                    {
                        if (dir.Contains((int)Directions.U))
                            mod = 3;
                        else if (dir.Contains((int)Directions.D))
                            mod = 1;
                        else
                            mod = 2;
                    }
                    else
                    {
                        if (dir.Contains((int)Directions.U))
                            mod = 4;
                        else
                            mod = 0;
                    }
                    tile_id += mod;
                }
            }
            // Check for whatever else cliffs check for
            else
            {
                //Yeti
                tile_id += 8;
            }
        }

        private bool check_for_water(int x, int y, int tag, int tile_id)
        {
            foreach (Vector2 offset in new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) })
                if (new List<int> { 16, 21, 22 }.Contains(Global.game_map.terrain_tag(new Vector2(x, y) + offset)))
                    return true;
            return false;
        }

        private void setup_units()
        {
            Units = new List<Minimap_Unit>();
            foreach (Game_Unit unit in Global.game_map.units.Values)
                if (unit.visible_by() && !Global.game_map.is_off_map(unit.loc))
                    Units.Add(new Minimap_Unit { Loc = unit.loc * MINIMAP_TILE_SIZE, Team = unit.team, Ready = unit.ready });
            Attack_Range = new Minimap_Attack_Range[
                Global.game_map.width, Global.game_map.height];
            foreach (var pair in Global.game_map.all_enemy_displayed_attack)
                Attack_Range[(int)pair.Key.X, (int)pair.Key.Y] |= Minimap_Attack_Range.Attack_All;
            foreach (var pair in Global.game_map.enemy_displayed_attack)
                Attack_Range[(int)pair.Key.X, (int)pair.Key.Y] |= Minimap_Attack_Range.Attack;
            foreach (var pair in Global.game_map.all_enemy_displayed_staff)
                Attack_Range[(int)pair.Key.X, (int)pair.Key.Y] |= Minimap_Attack_Range.Staff_All;
            foreach (var pair in Global.game_map.enemy_displayed_staff)
                Attack_Range[(int)pair.Key.X, (int)pair.Key.Y] |= Minimap_Attack_Range.Staff;
        }

        public void update()
        {
            // View box flash
            if (View_Timer < VIEW_TIME_MAX / 2)
                View_Alpha = Math.Max(128 - View_Timer * 8, 0);
            else
                View_Alpha = Math.Max(128 - ((VIEW_TIME_MAX - View_Timer) * 8), 0);
            View_Timer = (View_Timer + 1) % VIEW_TIME_MAX;
            // Unit dot flash
            switch (Unit_Timer)
            {
                case 0:
                    Unit_Alpha = 12 * 16;
                    break;
                case 4:
                    Unit_Alpha = 21 * 16;
                    break;
                case 8:
                    Unit_Alpha = 18 * 16;
                    break;
                case 12:
                    Unit_Alpha = 15 * 16;
                    break;
                case 16:
                    Unit_Alpha = 12 * 16;
                    break;
                case 20:
                    Unit_Alpha = 9 * 16;
                    break;
                case 24:
                    Unit_Alpha = 6 * 16;
                    break;
                case 32:
                    Unit_Alpha = 3 * 16;
                    break;
                case 44:
                    Unit_Alpha = 0 * 16;
                    break;
            }
            Unit_Timer = (Unit_Timer + 1) % UNIT_TIME_MAX;

            View_Offset = (Global.game_map.display_loc * MINIMAP_TILE_SIZE) / Constants.Map.TILE_SIZE - new Vector2(1);
            View_Area = (new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) * MINIMAP_TILE_SIZE) / Constants.Map.TILE_SIZE - new Vector2(8) + new Vector2(2);
            if (Timer >= 0)
            {
                Timer--;
                Grey_Fill.TintA =
                    (byte)((Closing ? Timer : (TIMER - Timer)) * GREY_ALPHA / TIMER);
                if (Timer <= 0)
                    Active = true;
            }
        }

        public void close()
        {
            Closing = true;
            Active = false;
            Timer = TIMER;
        }

        #region Draw
        public void draw_background(SpriteBatch sprite_batch)
        {
            if (Data != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Grey_Fill.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        public void draw_map(SpriteBatch sprite_batch)
        {
            if (Data != null)
            {
                // Map
                int width = Minimap_Texture.Width / MINIMAP_TILE_SIZE;
                // Fow tiles
                Color fog_color = Color.White;

                Effect unit_shader = Global.effect_shader();
                if (unit_shader != null)
                {
                    unit_shader.CurrentTechnique = unit_shader.Techniques["Tone"];
                    unit_shader.Parameters["tone"].SetValue(Global.game_map.fow_color.to_vector_4());
                }
                else
                    fog_color = new Color(168, 168, 168, 255);

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, unit_shader);
                for (int y = 0; y < Data.GetLength(1); y++)
                    for (int x = 0; x < Data.GetLength(0); x++)
                        if (Fow[x, y])
                            sprite_batch.Draw(Minimap_Texture, new Vector2(x * MINIMAP_TILE_SIZE, y * MINIMAP_TILE_SIZE) + Map_Offset,
                                new Rectangle((Data[x, y] % width) * MINIMAP_TILE_SIZE, (Data[x, y] / width) * MINIMAP_TILE_SIZE, MINIMAP_TILE_SIZE, MINIMAP_TILE_SIZE), fog_color);
                sprite_batch.End();

                // Visible tiles
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Color outline_color = new Color(0f, 0f, 0f, 0.5f);
                byte outline_frame_y, outline_frame_x;
                bool within_vertical_edges;
                for (int y = 0; y < Data.GetLength(1); y++)
                {
                    within_vertical_edges = y + 1 >= Global.game_map.edge_offset_top && y <= Data.GetLength(1) - Global.game_map.edge_offset_bottom;
                    if (y + 1 == Global.game_map.edge_offset_top)
                        outline_frame_y = 0;
                    else if (y == Data.GetLength(1) - Global.game_map.edge_offset_bottom)
                        outline_frame_y = 2;
                    else
                        outline_frame_y = 1;
                    for (int x = 0; x < Data.GetLength(0); x++)
                    {
                        // Draw tile
                        if (!Fow[x, y])
                            sprite_batch.Draw(Minimap_Texture, new Vector2(x * MINIMAP_TILE_SIZE, y * MINIMAP_TILE_SIZE) + Map_Offset,
                                new Rectangle((Data[x, y] % width) * MINIMAP_TILE_SIZE, (Data[x, y] / width) * MINIMAP_TILE_SIZE, MINIMAP_TILE_SIZE, MINIMAP_TILE_SIZE), Color.White);
                        // Draw attack range
                        if (Attack_Range[x, y] != Minimap_Attack_Range.None)
                        {
                            // Using null for the src rect, make sure that's fine
                            if (Attack_Range[x, y].HasEnumFlag(Minimap_Attack_Range.Staff_All) ||
                                    Attack_Range[x, y].HasEnumFlag(Minimap_Attack_Range.Staff))
                                sprite_batch.Draw(Global.Content.Load<Texture2D>(@"Graphics/White_Square"),
                                    new Vector2(x * MINIMAP_TILE_SIZE, y * MINIMAP_TILE_SIZE) + Map_Offset,
                                    null,
                                    Attack_Range[x, y].HasEnumFlag(Minimap_Attack_Range.Staff) ? new Color(32, 96, 32, 128) : new Color(32, 64, 64, 128),
                                    0f, Vector2.Zero, MINIMAP_TILE_SIZE / 16f, SpriteEffects.None, 0f);
                            if (Attack_Range[x, y].HasEnumFlag(Minimap_Attack_Range.Attack_All) ||
                                    Attack_Range[x, y].HasEnumFlag(Minimap_Attack_Range.Attack))
                                sprite_batch.Draw(Global.Content.Load<Texture2D>(@"Graphics/White_Square"),
                                    new Vector2(x * MINIMAP_TILE_SIZE, y * MINIMAP_TILE_SIZE) + Map_Offset,
                                    null,
                                    Attack_Range[x, y].HasEnumFlag(Minimap_Attack_Range.Attack) ? new Color(96, 32, 32, 128) : new Color(64, 32, 64, 128),
                                    0f, Vector2.Zero, MINIMAP_TILE_SIZE / 16f, SpriteEffects.None, 0f);
                        }
                        // Draw playable edge
                        if (within_vertical_edges && x + 1 >= Global.game_map.edge_offset_left && x <= Data.GetLength(0) - Global.game_map.edge_offset_right)
                        {
                            if (x + 1 == Global.game_map.edge_offset_left)
                                outline_frame_x = 0;
                            else if (x == Data.GetLength(0) - Global.game_map.edge_offset_right)
                                outline_frame_x = 2;
                            else
                                outline_frame_x = 1;

                            if (outline_frame_x != 1 || outline_frame_y != 1)
                                sprite_batch.Draw(Sprite_Texture, new Vector2(x * MINIMAP_TILE_SIZE, y * MINIMAP_TILE_SIZE) + Map_Offset,
                                    new Rectangle(outline_frame_x * MINIMAP_TILE_SIZE, 20 + outline_frame_y * MINIMAP_TILE_SIZE, MINIMAP_TILE_SIZE, MINIMAP_TILE_SIZE), outline_color);
                        }
                    }
                }
                sprite_batch.End();
            }
        }

        public void draw_sprites(SpriteBatch sprite_batch)
        {
            if (Data != null)
            {
                // Units
                Effect unit_shader = Global.effect_shader();
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, unit_shader);
                if (unit_shader != null)
                {
                    unit_shader.CurrentTechnique = unit_shader.Techniques["Technique2"];
                    unit_shader.Parameters["color_shift"].SetValue(new Color(255, 255, 255, Unit_Alpha).ToVector4());
                }
                foreach (Minimap_Unit unit in Units)
                {
                    int tint = unit.Ready ? 255 : 128;
                    sprite_batch.Draw(Sprite_Texture, unit.Loc + Map_Offset,
                        new Rectangle((unit.Team - 1) * MINIMAP_TILE_SIZE, 0, MINIMAP_TILE_SIZE, MINIMAP_TILE_SIZE), new Color(tint, tint, tint, 255));
                }
                sprite_batch.End();
            }
        }

        public void draw_view(SpriteBatch sprite_batch)
        {
            if (Active && Data != null)
            {
                // View Area
                Effect unit_shader = Global.effect_shader();
                if (unit_shader != null)
                {
                    unit_shader.CurrentTechnique = unit_shader.Techniques["Technique2"];
                    unit_shader.Parameters["color_shift"].SetValue(new Color(0, 0, 0, View_Alpha).ToVector4());
                }
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, unit_shader);
                sprite_batch.Draw(Sprite_Texture, View_Offset + Map_Offset,
                    new Rectangle(0, 4, 8, 8), Color.White);
                sprite_batch.Draw(Sprite_Texture, View_Offset + Map_Offset + new Vector2(View_Area.X, 0),
                    new Rectangle(8, 4, 8, 8), Color.White);
                sprite_batch.Draw(Sprite_Texture, View_Offset + Map_Offset + new Vector2(0, View_Area.Y),
                    new Rectangle(0, 12, 8, 8), Color.White);
                sprite_batch.Draw(Sprite_Texture, View_Offset + Map_Offset + View_Area,
                    new Rectangle(8, 12, 8, 8), Color.White);
                sprite_batch.End();
            }
        }
        #endregion
    }

    struct Minimap_Unit
    {
        public Vector2 Loc;
        public int Team;
        public bool Ready;
    }
}
