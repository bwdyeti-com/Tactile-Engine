using System.IO;

namespace TactileLibrary.Config
{
    public class ConfigData : TactileDataContent
    {
        public ActorConfig Actor = new ActorConfig();
        public BattleSceneConfig BattleScene = new BattleSceneConfig();
        public BgmConfig Bgm = new BgmConfig();

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static ConfigData GetEmptyInstance()
        {
            return new ConfigData();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var config = (ConfigData)other;

            Actor = new ActorConfig(config.Actor);
            BattleScene = new BattleSceneConfig(config.BattleScene);
            Bgm = new BgmConfig(config.Bgm);
        }

        public static ConfigData ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            _Read(input);
        }

        public override void Write(BinaryWriter output)
        {
            _Write(output);
        }

        private void _Read(BinaryReader input)
        {
            Encryption.ContentEncryption.DecryptStream(input, Decrypt);
        }
        private void Decrypt(BinaryReader input)
        {
            Actor = ActorConfig.ReadContent(input);
            BattleScene = BattleSceneConfig.ReadContent(input);
            Bgm = BgmConfig.ReadContent(input);
        }

        private void _Write(BinaryWriter output)
        {
            Encryption.ContentEncryption.EncryptStream(output, Encrypt);
        }
        private void Encrypt(BinaryWriter output)
        {
            Actor.Write(output);
            BattleScene.Write(output);
            Bgm.Write(output);
        }
        #endregion

        #region ICloneable
        public override object Clone()
        {
            return new ConfigData(this);
        }
        #endregion

        public ConfigData() { }
        public ConfigData(ConfigData config)
        {
            CopyFrom(config);
        }
    }
}
