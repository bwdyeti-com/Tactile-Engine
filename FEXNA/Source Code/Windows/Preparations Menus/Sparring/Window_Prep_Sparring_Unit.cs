//Sparring
using System.Collections.Generic;
using System.Linq;
using FEXNA.Windows.UserInterface.Preparations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Window_Prep_Sparring_Unit : Window_Prep_Items_Unit
    {
        const int COLUMNS = 4;
        const int ROW_SIZE = 20;
        readonly static int ROWS = (Config.WINDOW_HEIGHT - (Constants.Actor.NUM_ITEMS + 1) * 16) / ROW_SIZE;

        private Hand_Cursor Healer_Cursor, Battler_1_Cursor;

        #region Accessors
        new internal int index
        {
            get { return base.index; }
            set { base.index = value; }
        }

        public bool healer_set
        {
            set
            {
                if (value)
                {
                    Healer_Cursor.visible = true;
                    Healer_Cursor.loc = cursor_loc() + new Vector2(8, 4 + Scroll * row_size());
                }
                else
                    Healer_Cursor.visible = false;
            }
        }

        public bool battler_1_set
        {
            set
            {
                if (value)
                {
                    Battler_1_Cursor.visible = true;
                    Battler_1_Cursor.loc = cursor_loc() + new Vector2(8, 4 + Scroll * row_size());
                }
                else
                    Battler_1_Cursor.visible = false;
            }
        }
        #endregion

        public Window_Prep_Sparring_Unit() { }

        protected override void initialize()
        {
            WIDTH = unit_spacing() * COLUMNS + 32 + 8;
            HEIGHT = ROWS * ROW_SIZE + 8;
            loc = new Vector2((Config.WINDOW_WIDTH - WIDTH) / 2, 0);
            Unit_Scissor_Rect = new Rectangle((int)loc.X, (int)loc.Y + 4, WIDTH, HEIGHT - 8);
            initialize_sprites();
            initialize_index();
        }

        protected override void initialize_sprites()
        {
            base.initialize_sprites();
            Healer_Cursor = new Hand_Cursor();
            Healer_Cursor.tint = new Color(192, 192, 192, 255);
            Healer_Cursor.visible = false;
            Battler_1_Cursor = new Hand_Cursor();
            Battler_1_Cursor.tint = new Color(192, 192, 192, 255);
            Battler_1_Cursor.visible = false;
        }

        protected override PrepItemsUnitUINode unit_node(int actorId)
        {
            // Spar points
            int points = 0, staff_points = 0;
            if (Global.game_actors[actorId].can_arena() ||
                    Global.game_actors[actorId].can_oversee_sparring())
                points = Global.battalion.sparring_readiness(actorId);
            staff_points = Global.battalion.overseer_uses(actorId);

            var node = new PrepSparringUnitUINode(actorId, points, staff_points);
            node.Size = new Vector2(unit_spacing(), ROW_SIZE);
            return node;
        }

        public void refresh_map_sprites()
        {
            for (int i = 0; i < UnitNodes.Count(); i++)
                refresh_map_sprite(i);
        }

        protected override bool map_sprite_ready(int index)
        {
            if (Global.battalion.actors[index] == Window_Sparring.Healer_Id ||
                    Global.battalion.actors[index] == Window_Sparring.Battler_1_Id ||
                    Global.battalion.actors[index] == Window_Sparring.Battler_2_Id)
                return true;
            // Selecting healer
            if (Window_Sparring.Healer_Id == -1)
            {
                if (!Global.game_actors[Global.battalion.actors[index]].can_oversee_sparring())
                    return false;
            }
            // Selecting first battler
            else if (Window_Sparring.Battler_1_Id == -1)
            {
                if (!Global.game_actors[Global.battalion.actors[index]].can_arena())
                    return false;
            }
            // Selecting second battler
            else if (Window_Sparring.Battler_2_Id == -1)
            {
                if (!Global.game_actors[Global.battalion.actors[index]].can_arena())
                    return false;
                if (Window_Sparring.sparring_range(Window_Sparring.Battler_1_Id, Global.battalion.actors[index]) == -1)
                    return false;
            }
            else
                return false;
            //if (Global.game_actors[Global.battalion.actors[index]].can_arena() ||
            //        Global.game_actors[Global.battalion.actors[index]].can_oversee_sparring())
            return Global.battalion.can_spar(Global.battalion.actors[index], Window_Sparring.Healer_Id == -1);
        }

        public override void update(bool active)
        {
            base.update(active);

            float pip_opacity = 1f;
            int loops = 2;
            if (Global.game_map.icon_loops % loops < 2)
            {
                float pip_timer = Global.game_map.icon_loops % loops == 0 ? Global.game_map.icon_timer : 1 - Global.game_map.icon_timer;
                //if (pip_timer > 0.5f)
                //    pip_timer = 1 - pip_timer;
                //pip_opacity -= 1f * (pip_timer * 2);
                pip_opacity -= pip_timer;
            }
            for (int i = 0; i < Global.battalion.actors.Count; i++)
            {
                bool healer = Window_Sparring.Healer_Id == Global.battalion.actors[i] ||
                    (Window_Sparring.Healer_Id == -1 && this.actor_id == Global.battalion.actors[i] && map_sprite_ready(i));
                bool battler = Window_Sparring.Healer_Id != -1 &&
                    ((Window_Sparring.Battler_1_Id == Global.battalion.actors[i] || Window_Sparring.Battler_2_Id == Global.battalion.actors[i]) ||
                    (Window_Sparring.Battler_2_Id == -1 && this.actor_id == Global.battalion.actors[i] && map_sprite_ready(i)));

                int pips;
                if (healer)
                    pips = Global.battalion.spar_expense(Global.battalion.actors[i], true);
                else if (battler)
                    pips = Global.battalion.spar_expense(Global.battalion.actors[i], false);
                else
                    pips = 0;
                (UnitNodes[i] as PrepSparringUnitUINode).set_active_pips(pips);
                (UnitNodes[i] as PrepSparringUnitUINode).set_pip_opacity(pip_opacity);
            }
        }

        protected override int rows()
        {
            return ROWS;
        }
        protected override int row_size()
        {
            return ROW_SIZE;
        }

        protected override void draw_selected_cursor(SpriteBatch sprite_batch)
        {
            base.draw_selected_cursor(sprite_batch);
            Healer_Cursor.draw(sprite_batch, Offset - draw_vector());
            Battler_1_Cursor.draw(sprite_batch, Offset - draw_vector());
        }
    }
}
