using System;
using System.IO;
using EnumExtension;

namespace FEXNA_Library.Battler
{
    [Flags]
    public enum AnimTypeKeys : byte
    {
        NormalAttack = 1 << 0,
        ThrownWeapon = 1 << 1,
        MWeapon = 1 << 2,
        DistanceDefined = 1 << 7
    }
    public class Battle_Animation_Attack_Type
    {
        public AnimTypeKeys AttackType;
        public int MinDistance = -1, MaxDistance = -1;

        #region Serialization
        public static Battle_Animation_Attack_Type Read(BinaryReader input)
        {
            Battle_Animation_Attack_Type result = new Battle_Animation_Attack_Type();

            result.AttackType = (AnimTypeKeys)input.ReadByte();
            result.MinDistance = input.ReadInt32();
            result.MaxDistance = input.ReadInt32();

            return result;
        }

        public void Write(BinaryWriter output)
        {
            output.Write((byte)AttackType);
            output.Write(MinDistance);
            output.Write(MaxDistance);
        }
        #endregion

        public Battle_Animation_Attack_Type() { }
        public Battle_Animation_Attack_Type(Battle_Animation_Attack_Type other)
        {
            AttackType = other.AttackType;
            MinDistance = other.MinDistance;
            MaxDistance = other.MaxDistance;
        }

        public override string ToString()
        {
            return string.Format("Attack Type: {0}; {1} to {2}",
                AttackType & ~AnimTypeKeys.DistanceDefined,
                MinDistance, MaxDistance);
        }

        public bool attack_type_match(AnimTypeKeys type, int distance)
        {
            if ((type & ~AnimTypeKeys.DistanceDefined) ==
                (AttackType & ~AnimTypeKeys.DistanceDefined))
            {
                // If this anim type cares about distance
                if (AttackType.HasEnumFlag(AnimTypeKeys.DistanceDefined))
                {
                    if (MinDistance > 0 && distance < MinDistance)
                        return false;
                    else if (MaxDistance > 0 && distance > MaxDistance)
                        return false;
                }
                return true;
            }
            return false;
        }

        internal string distance_string()
        {
            if (MinDistance > -1 && MaxDistance > -1)
                return string.Format("{0} to {1}",
                    MinDistance, MaxDistance);
            else if (MinDistance > -1)
                return string.Format("{0}+", MinDistance);
            else if (MaxDistance > -1)
                return string.Format("{0}-", MaxDistance);

            return "";
        }
    }
}
