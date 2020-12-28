using System;
using System.ComponentModel;
using System.IO;

namespace TactileLibrary.Config
{
    public enum StatLabelColoring
    {
        None,
        Averages, // Stat labels are colored based on how far from the average the stat value is
        Growths // Stat labels are colored based on the unit's growth in that stat
    }

    public class ActorConfig : TactileDataContent
    {
        // Name
        [Category("Name"), Description(
            "Actor names will be split using this character,\n" +
            "and only the part before the first instance\n" +
            "will be shown as the ingame name.")]
        public char ActorNameDelimiter = '_';
        [Category("Name"), Description(
            "Generic face sprites will use the class name, then\n" +
            "this character, then the number of the unit's build.")]
        public char BuildNameDelimiter = '-';

        // Caps
        [Category("Caps"), Description(
            "Expected maximum HP.\n" +
            "Used for the length of gauges.")]
        public int HpMax = 80;
        [Category("Caps"), Description(
            "Expected maximum for primary stats.\n" +
            "Used for the length of gauges.")]
        public int PrimaryStatMax = 30;
        [Category("Caps"), Description(
            "The value of HP compared to other stats.\n" +
            "Used for conserving growths, semifixed levels, etc.")]
        public float HpValue = 0.5f;
        [Category("Caps"), Description(
            "Maximum Luck.")]
        public int LuckCap = 30;
        [Category("Caps"), Description(
            "The threshold for a unit to be at critical health.\n" +
            "AI can behave differently when at risk.")]
        public float LowHealthRate = 0.4f;//0.33f;

        // Levels
        [Category("Levels"), Description(
            "Maximum Level.")]
        public int LvlCap = 20;
        [Category("Levels"), Description(
            "Maximum Level for trainees.")]
        public int Tier0LvlCap = 10;
        [Category("Levels"), Description(
            "The minimum level units can use a promotion item.")]
        public int PromotionLvl = 10;
        [Category("Levels"), Description(
            "The lowest tier used by any class.")]
        public int LowestTier = 0;
        [Category("Levels"), Description(
            "Level is reset to 1 upon promotion.")]
        public bool ResetLevelOnPromotion = true;
        [Category("Levels"), Description(
            "Select which class to promote to from a menu,\n" +
            "for classes with multiple choices. If false,\n" +
            "one choice will be randomly selected.")]
        public bool PromotionSelectionMenu = true;
        [Category("Levels"), Description(
            "How much exp a unit needs to level up.")]
        public int ExpToLvl = 100;
        [Category("Levels"), Description(
            "The total amount of exp that can be gained\n" +
            "from one enemy. Use -1 for no limit.")]
        public int ExpPerEnemy = 100;
        [Category("Levels"), Description(
            "Exp Per Enemy is ignored on kill.")]
        public bool ExpPerEnemyKillException = true;
        [Category("Levels"), Description(
            "After a stat is capped, its growth rate will be\n" +
            "divided up among the other uncapped stats.")]
        public bool ConserveWastedGrowths = true;
        [Category("Levels"), Description(
            "The maximum amount a growth rate can be\n" +
            "boosted by conserving wasted growths.")]
        public int ConservedGrowthMaxPerStat = 50;
        [Category("Levels"), Description(
            "When a unit's max HP is increased,\n" +
            "they gain that much current HP as well.")]
        public bool ActorGainedHpHeal = true;
        [Category("Levels"), Description(
            "Allied NPC units can gain exp.\n" +
            "They will cap out one exp before level up.")]
        public bool CitizensGainExp = true;
        [Category("Levels"), Description(
            "Levels gained at the home based, from BExp/etc\n" +
            "will use the semi-fixed level up formulae.")]
        public bool SemifixedLevelsAtPreparations = true;
        [Category("Levels"), Description(
            "The first level up an actor gains will use the\n" +
            "semi-fixed level up formulae, with the total\n" +
            "points gained correlating with the growth total.")]
        public bool SemifixedFirstLevelUp = true;
        [Category("Levels"), Description(
            "If a unit hasn't capped any stats yet, level\n" +
            "ups will always give at least one point.")]
        public bool NoEmptyLevels = true;

        // Generics
        [Category("Generics"), Description(
            "The percentage of a generic unit's levels that\n" +
            "should be normal, random level ups. The remainder\n" +
            "will use fixed level ups (levels * growth rate).")]
        public float GenericRandomLevelPercent = 1f / 8;
        [Category("Generics"), Description(
            "The maximum value of Generic Unit Random Level Ratio.")]
        public int GenericRandomLevelMax = 4;
        [Category("Generics"), Description(
            "Generics gain enough WExp to use all weapons in their\n" +
            "starting inventorythey have the potential to equip.")]
        public bool GenericAutoWexp = true;
        [Category("Generics"), Description(
            "Generics have randomly assigned affinities,\n" +
            "instead of having no affinity.")]
        public bool GenericActorRandomAffinities = true;

        // Stat Labels
        [Category("Stat Labels"), Description(
            "Coloration mode for primary stat labels on the status screen.\n\n" +
            "Averages: Stats that are above their average will be greener, and below will be redder.\n" +
            "Growths: Stats with high growth rates will be greener, and low will be redder.")]
        public StatLabelColoring StatLabelColoring = StatLabelColoring.Averages;
        [Category("Stat Labels"), Description(
            "Stat label colors are only applied during\n" +
            "preparations and on the world map.")]
        public bool StatColorsOnlyInPrep = false;
        [Category("Stat Labels"), Description(
            "Stat label colors are only used for PCs.")]
        public bool OnlyPcStatColors = true;
        [Category("Stat Labels"), Description(
            "When using growths coloring, the threshold value for full red.")]
        public int GrowthAverageColorMin = 0;
        [Category("Stat Labels"), Description(
            "When using growths coloring, the middle point\n" +
            "value that have no modification.")]
        public int GrowthAverageColorMed = 30;
        [Category("Stat Labels"), Description(
            "When using growths coloring, the threshold value for full green.")]
        public int GrowthAverageColorMax = 60;

        // Weapon Types
        [Category("Weapon Types"), Description(
            "Having ranks in a child weapon type\n" +
            "allows using all of its ancestors")]
        public bool ChildWeaponTypeAllowsParent = true;
        [Category("Weapon Types"), Description(
            "Having ranks in a parent weapon type\n" +
            "allows using all of its descendants")]
        public bool ParentWeaponTypeAllowsChild = true;
        [Category("Weapon Types"), Description(
            "Hidden weapon types will be dynamically added\n" +
            "to the status screen for actors that have them.")]
        public bool ShowAllActorWeaponTypes = false;

        // Items
        [Category("Items"), Description(
            "Inventory size.")]
        public int NumItems = 6;
        [Category("Items"), Description(
            "Unequip weapon option shows up in the item menu.")]
        public bool AllowUnequip = true;
        [Category("Items"), Description(
            "Each actor can only get an S rank in one weapon type.")]
        public bool OneSRank = true;
        [Category("Items"), Description(
            "The Hit and Crt bonus for having an S rank.")]
        public int SRankBonus = 5;

        // Lives
        //@Debug: should be infinite with 0
        [Category("Lives"), Description(
            "The number of times a PC can die in casual mode\n" +
            "before they're permanently lost. Important characters\n" +
            "still only have one life. Use 0 for infinite.")]
        public int CasualModeLives = 3;

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static ActorConfig GetEmptyInstance()
        {
            return new ActorConfig();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var config = (ActorConfig)other;
            ActorNameDelimiter = config.ActorNameDelimiter;
            BuildNameDelimiter = config.BuildNameDelimiter;

            HpMax = config.HpMax;
            PrimaryStatMax = config.PrimaryStatMax;
            HpValue = config.HpValue;
            LuckCap = config.LuckCap;
            LowHealthRate = config.LowHealthRate;

            LvlCap = config.LvlCap;
            Tier0LvlCap = config.Tier0LvlCap;
            PromotionLvl = config.PromotionLvl;
            LowestTier = config.LowestTier;
            ResetLevelOnPromotion = config.ResetLevelOnPromotion;
            PromotionSelectionMenu = config.PromotionSelectionMenu;
            ExpToLvl = config.ExpToLvl;
            ExpPerEnemy = config.ExpPerEnemy;
            ExpPerEnemyKillException = config.ExpPerEnemyKillException;
            ConserveWastedGrowths = config.ConserveWastedGrowths;
            ConservedGrowthMaxPerStat = config.ConservedGrowthMaxPerStat;
            ActorGainedHpHeal = config.ActorGainedHpHeal;
            CitizensGainExp = config.CitizensGainExp;
            SemifixedLevelsAtPreparations = config.SemifixedLevelsAtPreparations;
            SemifixedFirstLevelUp = config.SemifixedFirstLevelUp;
            NoEmptyLevels = config.NoEmptyLevels;

            GenericRandomLevelPercent = config.GenericRandomLevelPercent;
            GenericRandomLevelMax = config.GenericRandomLevelMax;
            GenericAutoWexp = config.GenericAutoWexp;
            GenericActorRandomAffinities = config.GenericActorRandomAffinities;

            StatLabelColoring = config.StatLabelColoring;
            StatColorsOnlyInPrep = config.StatColorsOnlyInPrep;
            OnlyPcStatColors = config.OnlyPcStatColors;
            GrowthAverageColorMin = config.GrowthAverageColorMin;
            GrowthAverageColorMed = config.GrowthAverageColorMed;
            GrowthAverageColorMax = config.GrowthAverageColorMax;

            ChildWeaponTypeAllowsParent = config.ChildWeaponTypeAllowsParent;
            ParentWeaponTypeAllowsChild = config.ParentWeaponTypeAllowsChild;
            ShowAllActorWeaponTypes = config.ShowAllActorWeaponTypes;

            NumItems = config.NumItems;
            AllowUnequip = config.AllowUnequip;
            OneSRank = config.OneSRank;
            SRankBonus = config.SRankBonus;

            CasualModeLives = config.CasualModeLives;
        }

        public static ActorConfig ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            ActorNameDelimiter = input.ReadChar();
            BuildNameDelimiter = input.ReadChar();

            HpMax = input.ReadInt32();
            PrimaryStatMax = input.ReadInt32();
            HpValue = input.ReadSingle();
            LuckCap = input.ReadInt32();
            LowHealthRate = input.ReadSingle();

            LvlCap = input.ReadInt32();
            Tier0LvlCap = input.ReadInt32();
            PromotionLvl = input.ReadInt32();
            LowestTier = input.ReadInt32();
            ResetLevelOnPromotion = input.ReadBoolean();
            PromotionSelectionMenu = input.ReadBoolean();
            ExpToLvl = input.ReadInt32();
            ExpPerEnemy = input.ReadInt32();
            ExpPerEnemyKillException = input.ReadBoolean();
            ConserveWastedGrowths = input.ReadBoolean();
            ConservedGrowthMaxPerStat = input.ReadInt32();
            ActorGainedHpHeal = input.ReadBoolean();
            CitizensGainExp = input.ReadBoolean();
            SemifixedLevelsAtPreparations = input.ReadBoolean();
            SemifixedFirstLevelUp = input.ReadBoolean();
            NoEmptyLevels = input.ReadBoolean();

            GenericRandomLevelPercent = input.ReadSingle();
            GenericRandomLevelMax = input.ReadInt32();
            GenericAutoWexp = input.ReadBoolean();
            GenericActorRandomAffinities = input.ReadBoolean();

            StatLabelColoring = (StatLabelColoring)input.ReadInt32();
            StatColorsOnlyInPrep = input.ReadBoolean();
            OnlyPcStatColors = input.ReadBoolean();
            GrowthAverageColorMin = input.ReadInt32();
            GrowthAverageColorMed = input.ReadInt32();
            GrowthAverageColorMax = input.ReadInt32();

            ChildWeaponTypeAllowsParent = input.ReadBoolean();
            ParentWeaponTypeAllowsChild = input.ReadBoolean();
            ShowAllActorWeaponTypes = input.ReadBoolean();

            NumItems = input.ReadInt32();
            AllowUnequip = input.ReadBoolean();
            OneSRank = input.ReadBoolean();
            SRankBonus = input.ReadInt32();

            CasualModeLives = input.ReadInt32();
    }

        public override void Write(BinaryWriter output)
        {
            output.Write(ActorNameDelimiter);
            output.Write(BuildNameDelimiter);

            output.Write(HpMax);
            output.Write(PrimaryStatMax);
            output.Write(HpValue);
            output.Write(LuckCap);
            output.Write(LowHealthRate);

            output.Write(LvlCap);
            output.Write(Tier0LvlCap);
            output.Write(PromotionLvl);
            output.Write(LowestTier);
            output.Write(ResetLevelOnPromotion);
            output.Write(PromotionSelectionMenu);
            output.Write(ExpToLvl);
            output.Write(ExpPerEnemy);
            output.Write(ExpPerEnemyKillException);
            output.Write(ConserveWastedGrowths);
            output.Write(ConservedGrowthMaxPerStat);
            output.Write(ActorGainedHpHeal);
            output.Write(CitizensGainExp);
            output.Write(SemifixedLevelsAtPreparations);
            output.Write(SemifixedFirstLevelUp);
            output.Write(NoEmptyLevels);

            output.Write(GenericRandomLevelPercent);
            output.Write(GenericRandomLevelMax);
            output.Write(GenericAutoWexp);
            output.Write(GenericActorRandomAffinities);

            output.Write((int)StatLabelColoring);
            output.Write(StatColorsOnlyInPrep);
            output.Write(OnlyPcStatColors);
            output.Write(GrowthAverageColorMin);
            output.Write(GrowthAverageColorMed);
            output.Write(GrowthAverageColorMax);

            output.Write(ChildWeaponTypeAllowsParent);
            output.Write(ParentWeaponTypeAllowsChild);
            output.Write(ShowAllActorWeaponTypes);

            output.Write(NumItems);
            output.Write(AllowUnequip);
            output.Write(OneSRank);
            output.Write(SRankBonus);

            output.Write(CasualModeLives);
        }
        #endregion

        #region ICloneable
        public override object Clone()
        {
            return new ActorConfig(this);
        }
        #endregion

        public ActorConfig() { }
        public ActorConfig(ActorConfig config)
        {
            CopyFrom(config);
        }

        public int LevelCap(int tier)
        {
            if (ResetLevelOnPromotion)
                return RawLevelCap(tier);
            else
            {
                int cap = 0;
                for (int currentTier = LowestTier; currentTier <= tier; currentTier++)
                {
                    cap += RawLevelCap(currentTier);
                }
                return cap;
            }
        }

        //private
        public int RawLevelCap(int tier)
        {
            if (tier == 0)
                return Tier0LvlCap;
            else
                return LvlCap;
        }

        public int ActualLevel(int tier, int level)
        {
            if (ResetLevelOnPromotion)
                return level + LevelsBeforeTier(tier);
            else
                return level;
        }

        public int LevelsBeforeTier(int tier)
        {
            int level = 0;
            for (int currentTier = LowestTier; currentTier < tier; currentTier++)
                level += RawLevelCap(currentTier);

            return level;
        }

        public int PromotionLevel(int tier)
        {
            return Math.Min(RawLevelCap(tier), PromotionLvl);
        }

        /// <summary>
        /// Assumed promotion levels for each tier, for coloring stat label averages.
        /// </summary>
        public int AveragePromotionLevel(int tier)
        {
            switch (tier)
            {
                case 0:
                    return Tier0LvlCap;
                case 1:
                default:
                    return LvlCap;
            }
        }
    }
}
