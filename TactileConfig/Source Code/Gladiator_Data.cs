using TactileLibrary;

namespace Tactile
{
    public class Gladiator_Data
    {
        public int Class_Id { get; protected set; }
        public int Weapon_Type { get; protected set; }
        public int Gender { get; protected set; }
        public int Con { get; protected set; }

        internal Gladiator_Data(int class_id, int weapon, int gender, int con = -1)
        {
            Class_Id = class_id;
            Weapon_Type = weapon;
            Gender = gender;
            Con = con;
        }
    }
}
