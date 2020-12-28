using System;
using System.ComponentModel;
using System.IO;

namespace TactileLibrary.Config
{
    public class BattleSceneConfig : TactileDataContent
    {
        // Hp
        [Category("Hp"), Description(
            "Digits in the HP counter during battle.")]
        public int HpCounterValues = 2;
        [Category("Hp"), Description(
            "Digits in the HP counter outside battle:\n" +
            "Status screen, Unit screen, combat preview, etc.")]
        public int StatusHpCounterValues = 2;
        [Category("Hp"), Description(
            "HP gauge rows during battle scenes.")]
        public int MaxHpRows = 2;
        [Category("Hp"), Description(
            "HP per row of the health gauge\n" +
            "during battle scenes.")]
        public int HpTabsPerRow = 50;
        [Category("Hp"), Description(
            "Width of the ticks on the HP gauge.")]
        public int HpGaugeTabWidth = 3;
        [Category("Hp"), Description(
            "Height of the ticks on the HP gauge.")]
        public int HpGaugeTabHeight = 6;

        // Battle Scene
        [Category("Battle Scene"), Description(
            "Time in ticks to transition to the battle scene.")]
        public int BattleTransitionTime = 30;
        [Category("Battle Scene"), Description(
            "Minimum scaling of battle sprites during the\n" +
            "battle scene transition as they zoom in from\n" +
            "their unit's position on the map.")]
        public float BattlerMinScale = 0.33f;
        [Category("Battle Scene"), Description(
            "Fullscreen backgrounds are always shown\n" +
            "during battle scenes.")]
        public bool BattleBgAlwaysVisible = true;

        // Battle Scene Tone
        [Category("Battle Scene Tone"), Description(
            "How strongly the screen tone is applied\n" +
            "to the backtround during battle scenes.\n" +
            "Out of 255.")]
        public int ActionBackgroundToneWeight = 192;
        [Category("Battle Scene Tone"), Description(
            "How strongly the screen tone is applied\n" +
            "to platforms during battle scenes.\n" +
            "Out of 255.")]
        public int ActionPlatformToneWeight = 160;
        [Category("Battle Scene Tone"), Description(
            "How strongly the screen tone is applied\n" +
            "to battlers during battle scenes.\n" +
            "Out of 255.")]
        public int ActionBattlerToneWeight = 64;

        // Arena Background
        [Category("Arena Background"), Description(
            "Arena background animation frame duration in ticks.")]
        public int ArenaBgTime = 30;
        [Category("Arena Background"), Description(
            "Arena background animation number of frames.")]
        public int ArenaBgFrames = 3;

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static BattleSceneConfig GetEmptyInstance()
        {
            return new BattleSceneConfig();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var config = (BattleSceneConfig)other;
            HpCounterValues = config.HpCounterValues;
            StatusHpCounterValues = config.StatusHpCounterValues;
            MaxHpRows = config.MaxHpRows;
            HpTabsPerRow = config.HpTabsPerRow;
            HpGaugeTabWidth = config.HpGaugeTabWidth;
            HpGaugeTabHeight = config.HpGaugeTabHeight;

            BattleTransitionTime = config.BattleTransitionTime;
            BattlerMinScale = config.BattlerMinScale;
            BattleBgAlwaysVisible = config.BattleBgAlwaysVisible;

            ActionBackgroundToneWeight = config.ActionBackgroundToneWeight;
            ActionPlatformToneWeight = config.ActionPlatformToneWeight;
            ActionBattlerToneWeight = config.ActionBattlerToneWeight;

            ArenaBgTime = config.ArenaBgTime;
            ArenaBgFrames = config.ArenaBgFrames;
        }

        public static BattleSceneConfig ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            HpCounterValues = input.ReadInt32();
            StatusHpCounterValues = input.ReadInt32();
            MaxHpRows = input.ReadInt32();
            HpTabsPerRow = input.ReadInt32();
            HpGaugeTabWidth = input.ReadInt32();
            HpGaugeTabHeight = input.ReadInt32();

            BattleTransitionTime = input.ReadInt32();
            BattlerMinScale = input.ReadSingle();
            BattleBgAlwaysVisible = input.ReadBoolean();

            ActionBackgroundToneWeight = input.ReadInt32();
            ActionPlatformToneWeight = input.ReadInt32();
            ActionBattlerToneWeight = input.ReadInt32();

            ArenaBgTime = input.ReadInt32();
            ArenaBgFrames = input.ReadInt32();
        }

        public override void Write(BinaryWriter output)
        {
            output.Write(HpCounterValues);
            output.Write(StatusHpCounterValues);
            output.Write(MaxHpRows);
            output.Write(HpTabsPerRow);
            output.Write(HpGaugeTabWidth);
            output.Write(HpGaugeTabHeight);

            output.Write(BattleTransitionTime);
            output.Write(BattlerMinScale);
            output.Write(BattleBgAlwaysVisible);

            output.Write(ActionBackgroundToneWeight);
            output.Write(ActionPlatformToneWeight);
            output.Write(ActionBattlerToneWeight);

            output.Write(ArenaBgTime);
            output.Write(ArenaBgFrames);
        }
        #endregion

        #region ICloneable
        public override object Clone()
        {
            return new BattleSceneConfig(this);
        }
        #endregion

        public BattleSceneConfig() { }
        public BattleSceneConfig(BattleSceneConfig config)
        {
            CopyFrom(config);
        }
    }
}
