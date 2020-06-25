using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using EnumExtension;
using ListExtension;

namespace FEXNA_Library.Battler
{
    [Flags]
    public enum AttackAnims : short
    {
        Attack = 1 << 0,
        Hold = 1 << 1,
        Return = 1 << 2,

        None = 0,
        Hit = 1 << 4,
        Crit = 1 << 5,
        CausedDamage = 1 << 6,
        SkillActive = 1 << 7,

        HitDefined = 1 << 10,
        CritDefined = 1 << 11,
        DamageDefined = 1 << 12,
        SkillDefined = 1 << 13,
    }
    public class Battle_Animation_Association_Data : IFEXNADataContent
    {
        public List<int> Idle = new List<int>();
        public List<int> Avoid = new List<int>(), AvoidReturn = new List<int>();
        public List<int> GetHit = new List<int>(), GetCrit = new List<int>();
        public List<int> Dance = new List<int>(), DanceHold = new List<int>(), DanceReturn = new List<int>();
        public List<int> PreFight = new List<int>();
        public List<Battle_Animation_Attack_Set> AttackAnimations =
            new List<Battle_Animation_Attack_Set>();

        #region Accessors
        [ContentSerializerIgnore] // For some reason even though this is readonly it wants to be written to the xml file???
        // Oh actually it's writing to the xml improperly because it's readable, but then failing to write back to the data
        public List<int> get_crit { get { return GetCrit.Any() ? GetCrit : GetHit; } }
        #endregion

        #region IFEXNADataContent
        public IFEXNADataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static Battle_Animation_Association_Data GetEmptyInstance()
        {
            return new Battle_Animation_Association_Data();
        }

        public static Battle_Animation_Association_Data ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public void Read(BinaryReader input)
        {
            Idle.read(input);
            Avoid.read(input);
            AvoidReturn.read(input);
            GetHit.read(input);
            GetCrit.read(input);
            Dance.read(input);
            DanceHold.read(input);
            DanceReturn.read(input);
            PreFight.read(input);

            AttackAnimations.read(input);
        }

        public void Write(BinaryWriter output)
        {
            Idle.write(output);
            Avoid.write(output);
            AvoidReturn.write(output);
            GetHit.write(output);
            GetCrit.write(output);
            Dance.write(output);
            DanceHold.write(output);
            DanceReturn.write(output);
            PreFight.write(output);

            AttackAnimations.write(output);
        }
        #endregion

        public Battle_Animation_Association_Data() { }
        public Battle_Animation_Association_Data(Battle_Animation_Association_Data source)
        {
            Idle = new List<int>(source.Idle);
            Avoid = new List<int>(source.Avoid);
            AvoidReturn = new List<int>(source.AvoidReturn);
            GetHit = new List<int>(source.GetHit);
            GetCrit = new List<int>(source.GetCrit);
            Dance = new List<int>(source.Dance);
            DanceHold = new List<int>(source.DanceHold);
            DanceReturn = new List<int>(source.DanceReturn);
            PreFight = new List<int>(source.PreFight);

            AttackAnimations = source.AttackAnimations
                .Select(x => new Battle_Animation_Attack_Set(x))
                .ToList();
        }

        public override string ToString()
        {
            return string.Format("Animation Ids");
        }

        public List<int> attack_animation(AnimTypeKeys type, int distance, AttackAnims anim)
        {
            var attack_set = AttackAnimations
                .FirstOrDefault(x => x.Type.attack_type_match(type, distance));
            // If no match, for thrown weapons at 1 range, default down to normal attack if possible
            if (attack_set == null && type.HasEnumFlag(AnimTypeKeys.ThrownWeapon) && distance == 1)
            {
                attack_set = AttackAnimations
                    .FirstOrDefault(x => x.Type.attack_type_match(
                        (type & ~AnimTypeKeys.ThrownWeapon) | AnimTypeKeys.NormalAttack, distance));
            }
            if (attack_set != null)
            {
                return attack_set.attack_animation(anim);
            }

            return null;
        }

        /*public List<int> attack_animation(AttackAnims anim)
        {
            AttackAnims base_key;
            if (anim.Overlaps(AttackAnims.Attack))
                base_key = AttackAnims.Attack;
            else if (anim.Overlaps(AttackAnims.Hold))
                base_key = AttackAnims.Hold;
            else if (anim.Overlaps(AttackAnims.Return))
                base_key = AttackAnims.Return;
            else
                throw new ArgumentException();


            List<int> result;
            if (anim.Overlaps(AttackAnims.Attack))
            {
                if (anim.Overlaps(AttackAnims.Crit))
                {
                    if (try_attack_anim(base_key | AttackAnims.MissCrit, out result))
                        return result;
                    else if (try_attack_anim(base_key | AttackAnims.NoDamageCrit, out result))
                        return result;
                    else if (try_attack_anim(base_key | AttackAnims.HitCrit, out result))
                        return result;
                    else
                        return AttackAnimations[base_key | AttackAnims.Hit];
                }
                else
                {
                    if (try_attack_anim(base_key | AttackAnims.Miss, out result))
                        return result;
                    else if (try_attack_anim(base_key | AttackAnims.NoDamageHit, out result))
                        return result;
                    else
                        return AttackAnimations[base_key | AttackAnims.Hit];
                }
            }

            throw new ArgumentException();
        }

        private bool try_attack_anim(AttackAnims anim, out List<int> result)
        {
#if DEBUG
            if (!AttackAnimations.ContainsKey(anim))
                throw new ArgumentException(
                    string.Format("Animation Data is missing a key for \"{0}\"", anim));
#endif
            if (AttackAnimations[anim].Any())
            {
                result = AttackAnimations[anim];
                return true;
            }
            result = null;
            return false;
        }*/

        public bool compare(Battle_Animation_Association_Data other)
        {
            // Default animations
            if (!Idle.compare(other.Idle))
                return false;
            if (!Avoid.compare(other.Avoid))
                return false;
            if (!AvoidReturn.compare(other.AvoidReturn))
                return false;
            if (!GetHit.compare(other.GetHit))
                return false;
            if (!GetCrit.compare(other.GetCrit))
                return false;
            if (!PreFight.compare(other.PreFight))
                return false;
            if (!Dance.compare(other.Dance))
                return false;
            if (!DanceHold.compare(other.DanceHold))
                return false;
            if (!DanceReturn.compare(other.DanceReturn))
                return false;
            // Attack animations
            if (AttackAnimations.Count != other.AttackAnimations.Count)
                return false;
            for (int i = 0; i < AttackAnimations.Count; i++)
                if (!AttackAnimations[i].compare(other.AttackAnimations[i]))
                    return false;

            return true;
        }

        public object Clone()
        {
            return new Battle_Animation_Association_Data(this);
        }
    }
}
