using System;
using System.ComponentModel;
using System.IO;

namespace TactileLibrary.Config
{
    public class BgmConfig : TactileDataContent
    {
        // Maybe make these part of chapters instead of global...? //@Debug
        [Category("Bgm"), Description(
            "Title Theme")]
        public string TitleTheme = "Main Theme";
        [Category("Bgm"), Description(
            "Preparations and Home Base Theme")]
        public string PreparationsTheme = "Prepare to Charge";
        [Category("Bgm"), Description(
            "Chapter Transition Jingle")]
        public string ChapterTransitionTheme = "Chapter Transition Humming";
        [Category("Bgm"), Description(
            "Arena Battle Theme")]
        public string ArenaBattleTheme = "Arena Battle";
        [Category("Bgm"), Description(
            "Staff Theme for healing, restoration")]
        public string StaffTheme = "Curing";
        [Category("Bgm"), Description(
            "Staff Theme for status infliction")]
        public string AttackStaffTheme = "Healing";
        [Category("Bgm"), Description(
            "Dance or Play Theme")]
        public string DanceTheme = "Poem of Breeze";
        [Category("Bgm"), Description(
            "Promotion Theme")]
        public string PromotionTheme = "To the Heights";
        [Category("Bgm"), Description(
            "Near Victory Theme")]
        public string VictoryTheme = "Winning Road";
        [Category("Bgm"), Description(
            "Game Over")]
        public string GameOverTheme = "Game Over";
        [Category("Bgm"), Description(
            "PC Killed Theme")]
        public string AllyDeathTheme = "Within Sadness";

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static BgmConfig GetEmptyInstance()
        {
            return new BgmConfig();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var config = (BgmConfig)other;
            TitleTheme = config.TitleTheme;
            PreparationsTheme = config.PreparationsTheme;
            ChapterTransitionTheme = config.ChapterTransitionTheme;
            ArenaBattleTheme = config.ArenaBattleTheme;
            StaffTheme = config.StaffTheme;
            AttackStaffTheme = config.AttackStaffTheme;
            DanceTheme = config.DanceTheme;
            PromotionTheme = config.PromotionTheme;
            VictoryTheme = config.VictoryTheme;
            GameOverTheme = config.GameOverTheme;
            AllyDeathTheme = config.AllyDeathTheme;
        }

        public static BgmConfig ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            TitleTheme = input.ReadString();
            PreparationsTheme = input.ReadString();
            ChapterTransitionTheme = input.ReadString();
            ArenaBattleTheme = input.ReadString();
            StaffTheme = input.ReadString();
            AttackStaffTheme = input.ReadString();
            DanceTheme = input.ReadString();
            PromotionTheme = input.ReadString();
            VictoryTheme = input.ReadString();
            GameOverTheme = input.ReadString();
            AllyDeathTheme = input.ReadString();
        }

        public override void Write(BinaryWriter output)
        {
            output.Write(TitleTheme);
            output.Write(PreparationsTheme);
            output.Write(ChapterTransitionTheme);
            output.Write(ArenaBattleTheme);
            output.Write(StaffTheme);
            output.Write(AttackStaffTheme);
            output.Write(DanceTheme);
            output.Write(PromotionTheme);
            output.Write(VictoryTheme);
            output.Write(GameOverTheme);
            output.Write(AllyDeathTheme);
        }
        #endregion

        #region ICloneable
        public override object Clone()
        {
            return new BgmConfig(this);
        }
        #endregion

        public BgmConfig() { }
        public BgmConfig(BgmConfig config)
        {
            CopyFrom(config);
        }
    }
}
