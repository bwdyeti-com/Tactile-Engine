using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA
{
    partial class Game_Map
    {
        public int terrain_id(Vector2 loc)
        {
            FEXNA_Library.Data_Terrain terrain = terrain_data(loc);
            return terrain.Id;
        }

        public int terrain_avo_bonus(Vector2 loc)
        {
            FEXNA_Library.Data_Terrain terrain = terrain_data(loc);
            if (Global.game_system.In_Arena)
                return 0;
            return terrain.Avoid; // 0 if arena/some other cases //Yeti
        }

        public int terrain_def_bonus(Vector2 loc)
        {
            FEXNA_Library.Data_Terrain terrain = terrain_data(loc);
            if (Global.game_system.In_Arena)
                return 0;
            return terrain.Def; // 0 if arena/some other cases //Yeti
        }

        public int terrain_res_bonus(Vector2 loc)
        {
            FEXNA_Library.Data_Terrain terrain = terrain_data(loc);
            if (Global.game_system.In_Arena)
                return 0;
            return terrain.Res; // 0 if arena/some other cases //Yeti
        }

        internal int terrain_cost(Game_Unit unit, Vector2 loc)
        {
            return terrain_cost(unit.actor.move_type, terrain_tag(loc));
        }
        internal int terrain_cost(FEXNA_Library.MovementTypes move_type, Vector2 loc)
        {
            return terrain_cost((int)move_type, terrain_tag(loc));
        }
        internal int terrain_cost(Game_Unit unit, int terrainTag)
        {
            return terrain_cost((int)unit.actor.move_type, terrainTag);
        }

        internal int terrain_cost(int move_type, int terrainTag)
        {
            FEXNA_Library.Data_Terrain terrain = terrain_data(terrainTag);
            return terrain.Move_Costs[Global.game_state.weather][move_type]; // This might need to be a Game_Unit method //Yeti
        }

        public int terrain_step_sound_group(Vector2 loc)
        {
            FEXNA_Library.Data_Terrain terrain = terrain_data(loc);
            return terrain.Step_Sound_Group;
        }

        public int terrain_dust_type(Vector2 loc)
        {
            if (Global.game_system.In_Arena)
                return 0;
            FEXNA_Library.Data_Terrain terrain = terrain_data(loc);
            return terrain.Dust_Type;
        }

        public bool terrain_fire_through(Vector2 loc)
        {
            FEXNA_Library.Data_Terrain terrain = terrain_data(loc);
            return terrain.Fire_Through;
        }

        public bool terrain_heals(Vector2 loc)
        {
            FEXNA_Library.Data_Terrain terrain = terrain_data(loc);
            return terrain.Heal != null && terrain.Heal[0] > 0;
        }

        public int terrain_healing_amount(Vector2 loc)
        {
            FEXNA_Library.Data_Terrain terrain = terrain_data(loc);
            if (terrain.Heal == null)
                return 0;
            return terrain.Heal[0];
        }

        public HashSet<Vector2> healing_terrain()
        {
            HashSet<Vector2> result = new HashSet<Vector2>();
            FEXNA_Library.Data_Terrain terrain;
            int width = this.width - edge_offset_right, height = this.height - edge_offset_bottom;
            for (int y = edge_offset_top; y < height; y++)
                for (int x = edge_offset_left; x < width; x++)
                {
                    if (terrain_heals(new Vector2(x, y)))
                        result.Add(new Vector2(x, y));
                }
            return result;
        }
    }
}
