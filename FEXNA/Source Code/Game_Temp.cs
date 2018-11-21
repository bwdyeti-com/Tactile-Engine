using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA
{
    public class Game_Temp
    {
        public int highlighted_unit_id = -1;
        public int dying_unit_id = -1;
        public string message_text = null;
        public bool boss_theme = false;
        // Menu Calls
        public bool menuing = false;
        public bool discard_menuing = false;
        public bool menu_call = false;
        public bool map_menu_call = false;
        public bool end_turn_highlit = false;
        public bool unit_menu_call = false;
        public bool status_menu_call = false;
        internal bool shop_call { get; private set; }
        internal bool secret_shop_call { get; private set; }
        public bool discard_menu_call = false;
        public bool force_send_to_convoy = false;
        public bool minimap_call = false;
        public bool map_save_call = false;
        public bool rankings_call = false;
        public int status_unit_id;
        public int status_team;
        public bool scripted_battle = false;
        public int preparations_item_index;
        //Sparring
        public bool sparring = false;
        internal Scripted_Combat_Script scripted_battle_stats;
        // Ranges
        public HashSet<Vector2> temp_attack_range = new HashSet<Vector2>();
        public HashSet<Vector2> temp_staff_range = new HashSet<Vector2>();
        public HashSet<Vector2> temp_talk_range = new HashSet<Vector2>();
        // Skills:
        public Dictionary<string, HashSet<Vector2>> temp_skill_ranges = new Dictionary<string, HashSet<Vector2>>();
        public Dictionary<string, HashSet<Vector2>> temp_skill_move_ranges = new Dictionary<string, HashSet<Vector2>>();

#if DEBUG
        internal bool chapter_skipped = false;
#endif

        public void clear_temp_range()
        {
            temp_attack_range.Clear();
            temp_staff_range.Clear();
            temp_talk_range.Clear();
            
            temp_skill_ranges.Clear();
            temp_skill_move_ranges.Clear();
        }

        internal void call_shop(bool secret = false)
        {
            shop_call = true;
            secret_shop_call = secret;
        }

        internal void reset_shop_call()
        {
            shop_call = false;
            secret_shop_call = false;
        }
    }
}
