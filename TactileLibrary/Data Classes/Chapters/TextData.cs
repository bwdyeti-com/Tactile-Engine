using System.Collections.Generic;
using System.IO;
using System.Linq;
using DictionaryExtension;

namespace TactileLibrary.Chapters
{
    public class TextData : TactileDataContent
    {
        public Dictionary<string, string> Text = new Dictionary<string, string>();

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static TextData GetEmptyInstance()
        {
            return new TextData();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var text = (TextData)other;

            Text = text.Text.ToDictionary(p => p.Key, p => p.Value);
        }

        public static TextData ReadContent(BinaryReader reader)
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
            Text.read(input);
        }

        private void _Write(BinaryWriter output)
        {
            Encryption.ContentEncryption.EncryptStream(output, Encrypt);
        }
        private void Encrypt(BinaryWriter output)
        {
            Text.write(output);
        }
        #endregion

        #region ICloneable
        public override object Clone()
        {
            return new TextData(this);
        }
        #endregion

        public TextData() { }
        public TextData(TextData text)
        {
            CopyFrom(text);
        }

        public void AddText(string key, string value)
        {
            Text.Add(key, value);
        }
    }
}
