using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnumExtension;
using ListExtension;

namespace TactileLibrary.Battler
{
    public class Battle_Animation_Attack_Set
    {
        public Battle_Animation_Attack_Type Type;
        public List<Battle_Animation_Variable_Set> Attack;
        public List<Battle_Animation_Variable_Set> Hold;
        public List<Battle_Animation_Variable_Set> Return;

        #region Serialization
        public static Battle_Animation_Attack_Set Read(BinaryReader input)
        {
            Battle_Animation_Attack_Set result = new Battle_Animation_Attack_Set();

            result.Type = Battle_Animation_Attack_Type.Read(input);
            result.Attack.read(input);
            result.Hold.read(input);
            result.Return.read(input);

            return result;
        }

        public void Write(BinaryWriter output)
        {
            Type.Write(output);
            Attack.write(output);
            Hold.write(output);
            Return.write(output);
        }
        #endregion

        public Battle_Animation_Attack_Set()
        {
            Type = new Battle_Animation_Attack_Type();
            Attack = new List<Battle_Animation_Variable_Set> {
                new Battle_Animation_Variable_Set() };
            Hold = new List<Battle_Animation_Variable_Set> {
                new Battle_Animation_Variable_Set() };
            Return = new List<Battle_Animation_Variable_Set> {
                new Battle_Animation_Variable_Set() };
        }
        public Battle_Animation_Attack_Set(Battle_Animation_Attack_Set other)
        {
            Type = new Battle_Animation_Attack_Type(other.Type);
            Attack = other.Attack
                .Select(x => new Battle_Animation_Variable_Set(x))
                .ToList();
            Hold = other.Hold
                .Select(x => new Battle_Animation_Variable_Set(x))
                .ToList();
            Return = other.Return
                .Select(x => new Battle_Animation_Variable_Set(x))
                .ToList();
        }

        public override string ToString()
        {
            string distance = Type.distance_string();
            return string.Format("Attack Set: {0}{1}",
                Type.AttackType & ~AnimTypeKeys.DistanceDefined,
                distance.Any() ? "; " + distance : "");
        }

        internal List<int> attack_animation(AttackAnims anim)
        {
            List<Battle_Animation_Variable_Set> anim_set;
            if (anim.HasEnumFlag(AttackAnims.Attack))
                anim_set = Attack;
            else if (anim.HasEnumFlag(AttackAnims.Hold))
                anim_set = Hold;
            else //if (anim.Overlaps(AttackAnims.Return))
                anim_set = Return;

            for (int i = anim_set.Count - 1; i >= 0; i--)
            {
                var animation = anim_set[i];
                // Check for hit, crit, damage caused
                if (!animation.test_attack_flag_match(anim,
                        AttackAnims.HitDefined, AttackAnims.Hit))
                    continue;
                if (!animation.test_attack_flag_match(anim,
                        AttackAnims.CritDefined, AttackAnims.Crit))
                    continue;
                if (!animation.test_attack_flag_match(anim,
                        AttackAnims.DamageDefined, AttackAnims.CausedDamage))
                    continue;
                if (!animation.test_attack_flag_match(anim,
                        AttackAnims.SkillDefined, AttackAnims.SkillActive))
                    continue;

                // All checks passed
                return animation.Animation;
            }

            // Should never hit this, but just in case
            return null;
        }

        internal bool compare(Battle_Animation_Attack_Set other)
        {
            if (Type != other.Type)
            // Attack
                if (Attack.Count != other.Attack.Count)
                return false;
            for (int i = 0; i < Attack.Count; i++)
                if (!Attack[i].compare(other.Attack[i]))
                    return false;
            // Hold
            if (Hold.Count != other.Hold.Count)
                return false;
            for (int i = 0; i < Hold.Count; i++)
                if (!Hold[i].compare(other.Hold[i]))
                    return false;
            // Return
            if (Return.Count != other.Return.Count)
                return false;
            for (int i = 0; i < Return.Count; i++)
                if (!Return[i].compare(other.Return[i]))
                    return false;

            return true;
        }

        public IEnumerable<int> all_anim_ids()
        {
            foreach (int id in Attack.SelectMany(x => x.Animation))
                yield return id;
            foreach (int id in Hold.SelectMany(x => x.Animation))
                yield return id;
            foreach (int id in Return.SelectMany(x => x.Animation))
                yield return id;
        }
    }
}
