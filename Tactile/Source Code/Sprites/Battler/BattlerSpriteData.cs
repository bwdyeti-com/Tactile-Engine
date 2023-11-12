using System.Linq;
using TactileLibrary;
using TactileWeaponExtension;

namespace Tactile
{
    class BattlerSpriteData
    {
        private Game_Unit _Unit;
        private Maybe<int> _ClassId;

        public BattlerSpriteData(Game_Unit unit)
        {
            _Unit = unit;
        }
        public BattlerSpriteData(Game_Unit unit, int classId)
        {
            _Unit = unit;
            _ClassId = classId;
        }

        public Game_Unit Unit { get { return _Unit; } }

        public override string ToString()
        {
            return string.Format("BattlerSpriteData: {0}, {1}, {2}",
                this.NameFull, this.ClassNameFull,
                this.Weapon == null ? "[Unarmed]" : this.Weapon.Name);
        }

        public string Name { get { return _Unit.actor.name; } }
        public string NameFull { get { return _Unit.actor.name_full; } }

        public string BattlerName()
        {
            return BattlerName(this.UsedWeaponType);
        }
        public string BattlerName(string weaponType)
        {
            int gender = this.Gender;
            string className = this.ClassNameFull;

            string name;
            // Check if the unit has a personal sprite
            name = this.NameFull + "-" + className + gender.ToString();
            if (!Global.content_exists(@"Graphics/Animations/" + name + "-" + weaponType))
            {
                // Check for a generic sprite based on class/gender
                name = className + gender.ToString();
                if (!Global.content_exists(@"Graphics/Animations/" + name + '-' + weaponType))
                {
                    // Use generic base gender sprite based on class/base gender
                    gender %= 2;
                    name = className + gender.ToString();
                    if (!Global.content_exists(@"Graphics/Animations/" + name + '-' + weaponType))
                        return "";
                }
            }
            return name;
        }

        public Anim_Sprite_Data AnimData(bool dance)
        {
            return (dance && _Unit.id == Global.game_state.dancer_id) ?
                   AnimName(Global.weapon_types[0].AnimName) :
                   AnimName();
        }
        public Anim_Sprite_Data AnimName()
        {
            return AnimName(this.UsedWeaponType);
        }
        public Anim_Sprite_Data AnimName(string weaponType)
        {
            int gender = this.Gender;
            if (string.IsNullOrEmpty(weaponType))
                return new Anim_Sprite_Data { name = "", gender = gender };

            string className = this.ClassNameFull;

            string name;
            // Check if the unit has a personal sprite
            //name = unit.actor.name_full + "-" + className + //Debug
            //    gender.ToString() + "-" + weapon;
            name = string.Format("{0}-{1}{2}-{3}", this.NameFull, className, gender, weaponType);
            if (!Global.content_exists(@"Graphics/Animations/" + name))
            {
                // Check for a generic sprite based on class/gender
                name = className + gender.ToString() +
                    '-' + weaponType;
                if (!Global.content_exists(@"Graphics/Animations/" + name))
                {
                    // Use generic base gender sprite based on class/base gender
                    gender %= 2;
                    name = className + gender.ToString() +
                        '-' + weaponType;
                    if (!Global.content_exists(@"Graphics/Animations/" + name))
                    {
                        // Try one last time with the other gender, to at least show something
                        gender = 1 - gender;
                        name = className + gender.ToString() +
                            '-' + weaponType;
                        if (!Global.content_exists(@"Graphics/Animations/" + name))
                            return new Anim_Sprite_Data { name = "", gender = gender };
                    }
                }
            }
            return new Anim_Sprite_Data { name = name, gender = gender };
        }

        public int ClassId
        {
            get
            {
                if (_ClassId.IsSomething)
                    return _ClassId;
                return _Unit.actor.class_id;
            }
        }
        public Data_Class ClassData { get { return Global.data_classes[this.ClassId]; } }
        public string ClassNameFull { get { return this.ClassData.Name; } }

        public int ActualGender { get { return _Unit.actor.gender; } }
        public int Gender
        {
            get
            {
                int gender = this.ActualGender;
                if (TactileBattlerImage.Single_Gender_Battle_Sprite.Contains(this.ClassId))
                    gender = (gender / 2) * 2;
                return gender;
            }
        }

        public bool SkillEffect
        {
            get { return _Unit.actor.skill_activated && !_Unit.skip_skill_effect; }
        }

        public int WeaponId
        {
            get
            {
                int weaponId = _Unit.actor.weapon_id;

                // Tries to get the weapon of the weapon id
                Data_Weapon weapon = GetWeapon(weaponId);

                // If it's null, return 0
                if (weapon == null)
                    return 0;
                // Else if can't equip this weapon type
                else if (_Unit.actor.max_weapon_level(weapon.main_type(), true) <= 0)
                    return 0;

                return weaponId;
            }
        }
        public Data_Weapon Weapon { get { return GetWeapon(this.WeaponId); } }

        private Data_Weapon GetWeapon(int weaponId)
        {
            if (!Global.HasWeapon(weaponId))
                return null;
            return weaponId == 0 ? null : Global.GetWeapon(weaponId);
        }

        public string UsedWeaponType
        {
            get
            {
                var weapon = this.Weapon;
                // This does need to handle unique anims somehow, though //Yeti
                string weaponType = Global.weapon_types[0].AnimName;
                if (weapon == null)
                {
                    // If no weapon equipped, and an equippable staff in inventory
                    if (_Unit.actor.can_staff())
                        foreach (Item_Data item_data in _Unit.actor.items)
                            if (item_data.is_weapon && item_data.to_weapon.is_staff() &&
                                _Unit.actor.is_equippable(item_data.to_weapon))
                            {
                                weaponType = item_data.to_weapon.main_type().AnimName;
                                break;
                            }
                }
                else if (weapon.Ballista())
                    weaponType = "";
                //else if (this.weapon.Unique()) // Need to add this tag to weapons, or come up with a more useful system //Yeti
                //    weapon_type = (int)Anim_Types.Unique;
                else
                {
                    weaponType = weapon.main_type().AnimName;
                    if (weapon.Thrown() && !string.IsNullOrEmpty(weapon.main_type().ThrownAnimName))
                        weaponType = weapon.main_type().ThrownAnimName;
                }
                return weaponType;
            }
        }

        public bool MWeapon(int distance)
        {
            if (this.Weapon == null)
                return false;

            return (distance >= 2 && this.Weapon.is_ranged_magic()) ||
                this.Weapon.is_always_magic();
        }

        //@Yeti: anima stuff shouldn't be hardcoded
        public int WeaponAnimaType { get { return this.Weapon.anima_type(); } }

        public bool MagicAttack { get { return _Unit.magic_attack; } }

        public int AnimationGroupOffset
        {
            get
            {
                string name = string.Format("{0}-{1}",
                    this.ClassNameFull, this.UsedWeaponType);
                return Global.animation_group(name);
            }
        }
    }

    struct Anim_Sprite_Data
    {
        public string name;
        public int gender;

        public bool ValidAnim { get { return !string.IsNullOrEmpty(name); } }
    }
}
