using System;
using System.IO;
using System.Security.Cryptography;

namespace TactileLibrary.Encryption
{
    public static class StreamEncryption
    {
        const int IV_SIZE = 16;

        public static void EncryptStream(byte[] key, BinaryWriter writer, MemoryStream stream)
        {
            _EncryptStream(key, writer, stream);
        }
        private static void _EncryptStream(byte[] key, BinaryWriter writer, MemoryStream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Encrypts onto the memory stream and gets the IV used
                byte[] iv = _EncryptStream(key, ms, stream);

                // Write the length of the encrypted stream also encrypted
                using (MemoryStream lengthMs = GetEncryptedLength(key, iv, ms.Length))
                {
                    // Write the length of the encrypted length
                    byte lengthLength = (byte)lengthMs.Length;
                    writer.Write(lengthLength);

                    // Write IV
                    writer.Write(iv);

                    // Write the length of the encrypted portion
                    lengthMs.Seek(0, SeekOrigin.Begin);
                    lengthMs.CopyTo(writer.BaseStream);
                }

                // Write the encrypted stream
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(writer.BaseStream);
            }
        }
        private static byte[] _EncryptStream(byte[] key, MemoryStream targetStream, MemoryStream sourceStream)
        {
            byte[] iv = new byte[IV_SIZE];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(iv);
            }

            _EncryptStream(key, iv, targetStream, sourceStream);

            return iv;
        }
        private static void _EncryptStream(byte[] key, byte[] iv, MemoryStream targetStream, MemoryStream sourceStream)
        {
            using (Rijndael rijndael = GetRijndael(key))
            {
                CryptoStream encryptor = new CryptoStream(
                    targetStream,
                    rijndael.CreateEncryptor(key, iv),
                    CryptoStreamMode.Write);
                sourceStream.CopyTo(encryptor);
                encryptor.FlushFinalBlock();
            }
        }

        private static MemoryStream GetEncryptedLength(byte[] key, byte[] iv, long length)
        {
            MemoryStream encryptedLengthMs = new MemoryStream();

            using (MemoryStream lengthMs = new MemoryStream())
            {
                // Write the length to get it as a memory stream
                BinaryWriter lengthWriter = new BinaryWriter(lengthMs);
                lengthWriter.Write(length);
                lengthWriter.Flush();
                lengthMs.Seek(0, SeekOrigin.Begin);

                _EncryptStream(key, iv, encryptedLengthMs, lengthMs);
            }

            return encryptedLengthMs;
        }

        public static BinaryReader DecryptStream(byte[] key, BinaryReader reader)
        {
            return _DecryptStream(key, reader);
        }
        private static BinaryReader _DecryptStream(byte[] key, BinaryReader reader)
        {
            // Get length of length
            byte lengthLength = reader.ReadByte();

            // Get IV
            byte[] iv = new byte[IV_SIZE];

            if (reader.Read(iv, 0, iv.Length) != iv.Length)
            {
                throw new ApplicationException("Failed to read IV from stream.");
            }

            // Get length
            long encryptedLength;
            using (BinaryReader lengthReader = GetDecryptedPartialStream(key, iv, reader, lengthLength))
            {
                encryptedLength = lengthReader.ReadInt64();
            }

            // Get the data
            return GetDecryptedPartialStream(key, iv, reader, encryptedLength);
        }

        private static BinaryReader GetDecryptedPartialStream(byte[] key, byte[] iv, BinaryReader reader, long length)
        {
            // Copy the encrypted section of the reader to a memory stream
            byte[] partialStreamBuffer = new byte[length];
            reader.BaseStream.Read(partialStreamBuffer, 0, partialStreamBuffer.Length);

            // Create a memory stream from the bytes and decrypt it
            using (MemoryStream ms = new MemoryStream(partialStreamBuffer))
            {
                using (Rijndael rijndael = GetRijndael(key))
                {
                    CryptoStream decryptor = new CryptoStream(
                        ms,
                        rijndael.CreateDecryptor(key, iv),
                        CryptoStreamMode.Read);

                    MemoryStream resultStream = new MemoryStream();
                    decryptor.CopyTo(resultStream);
                    decryptor.Flush();

                    resultStream.Seek(0, SeekOrigin.Begin);
                    return new BinaryReader(resultStream);
                }
            }
        }

        private static Rijndael GetRijndael(byte[] key)
        {
            Rijndael rijndael = new RijndaelManaged();
            rijndael.KeySize = key.Length * sizeof(byte) * 8;
            return rijndael;
        }
    }
}
