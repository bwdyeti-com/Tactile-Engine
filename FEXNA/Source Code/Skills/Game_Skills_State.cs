using System.IO;

namespace FEXNA.State
{
    class Game_Skills_State : Game_State_Component
    {
        internal Game_Dance_State Dance_State { get; private set; }
        internal Game_Sacrifice_State Sacrifice_State { get; private set; }
        internal Game_Steal_State Steal_State { get; private set; }

        internal override void write(BinaryWriter writer)
        {
            Dance_State.write(writer);
            Sacrifice_State.write(writer);
            Steal_State.write(writer);
        }

        internal override void read(BinaryReader reader)
        {
            Dance_State.read(reader);
            Sacrifice_State.read(reader);
            Steal_State.read(reader);
        }

        internal Game_Skills_State()
        {
            Dance_State = new Game_Dance_State();
            Sacrifice_State = new Game_Sacrifice_State();
            Steal_State = new Game_Steal_State();
        }

        internal override void update()
        {
            Dance_State.update();
            Sacrifice_State.update();
            Steal_State.update();
        }

        internal bool is_skill_ready()
        {
            if (dance_active) return false;
            if (sacrifice_active) return false;
            if (steal_active) return false;
            return true;
        }

        public void skip_battle_scene()
        {
            Dance_State.skip_battle_scene();
        }

        internal bool dance_active { get { return Dance_State.dance_calling || Dance_State.in_dance; } }
        internal bool sacrifice_active { get { return Sacrifice_State.sacrifice_calling || Sacrifice_State.in_sacrifice; } }
        internal bool steal_active { get { return Steal_State.steal_calling || Steal_State.in_steal; } }
    }
}
