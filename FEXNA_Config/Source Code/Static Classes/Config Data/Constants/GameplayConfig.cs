namespace FEXNA.Constants
{
    public class Gameplay
    {
        public readonly static int[] MAIN_CHARACTER = { 1 };
        public readonly static int[] LOSS_ON_DEATH = { 1, 26, 41 }; //FEGame

        public const bool BLOCK_FIRE_THROUGH_WALLS_DEFAULT = true;
        public const bool BLOCK_VISION_THROUGH_WALLS = false;
        public const bool TALKING_IS_FREE_ACTION = true;
        public const bool MOVE_ARROW_WIGGLING = true; // Does the movement arrow redraw use rns, like in GBA FE?

        public const bool CANCEL_GREEN_DROP_ON_ANY_STEAL = false; // Does stealing from a unit that normally drops something on death cancel that drop?
        public const bool REPAIR_DROPPED_ITEM = false;
        public const bool CITIZENS_GET_DROPS = true; // Non-generic player allied AI can acquire green drops

        public const bool MASTERIES_CHARGE_AT_TURN_END = true; // Do masteries charge at the end of a turn, rather than the start

        public const int CONVOY_SIZE = 150;
        public const Convoy_Stack_Types CONVOY_ITEMS_STACK = Convoy_Stack_Types.Use;
        public const bool CONVOY_SOLD_ITEMS_REPAIR = true; // Do items sold to the convoy show up again repaired in the next chapter
        public const bool TEAM_LEADER_CONVOY = false; // Can team leaders always access the convoy

        public const bool SIEGE_RELOADING = true; // Siege engines are disabled for a turn after firing
        public const bool SIEGE_MANUAL_RELOADING = false; // Siege engines are manually reloaded // Not implemented //Yeti
    }
}
