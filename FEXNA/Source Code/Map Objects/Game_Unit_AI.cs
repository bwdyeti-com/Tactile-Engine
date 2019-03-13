using Microsoft.Xna.Framework;

namespace FEXNA
{
    partial class Game_Unit
    {
        protected Vector2 Ai_Base_Loc;
        protected int Ai_Base_Temp_Moved;
        protected bool Saving_Ai_Loc;

        #region Accessors
        public bool saving_ai_loc { get { return Saving_Ai_Loc; } }
        #endregion

        public void set_ai_base_loc(Vector2 new_loc, int move_distance)
        {
            Ai_Base_Loc = Loc;
            Ai_Base_Temp_Moved = Temp_Moved;

            Loc = new_loc;
            Temp_Moved = Temp_Moved + move_distance;

            Saving_Ai_Loc = true;
        }

        public void reset_ai_loc()
        {
            if (Saving_Ai_Loc)
            {
                Loc = Ai_Base_Loc;
                Temp_Moved = Ai_Base_Temp_Moved;

                Saving_Ai_Loc = false;
            }
        }
    }
}
