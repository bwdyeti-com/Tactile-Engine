using System;
using System.IO;

namespace TactileLibrary.Encryption
{
    class ContentEncryption
    {
        private static readonly byte[] CONTENT_KEY = new byte[]
        {
            0x1e, 0x12, 0xe5, 0xba, 0xa0, 0xed, 0x37, 0xa2,
            0x8a, 0x3b, 0x37, 0x3a, 0xb8, 0x5c, 0xe4, 0x4c
        };

        internal static void EncryptStream(BinaryWriter writer, Action<BinaryWriter> save)
        {
            // Create a temporary memory stream to hold the unencrypted data
            using (MemoryStream ms = new MemoryStream())
            {
                // Write to the memory stream
                BinaryWriter memoryWriter = new BinaryWriter(ms);
                save(memoryWriter);
                memoryWriter.Flush();

                // Reset to the start of the memory stream, then copy it encrypted
                ms.Seek(0, SeekOrigin.Begin);
                StreamEncryption
                    .EncryptStream(CONTENT_KEY, writer, ms);
            }
        }

        internal static void DecryptStream(
            BinaryReader reader,
            Action<BinaryReader> read)
        {
            using (BinaryReader decryptedReader =
                StreamEncryption.DecryptStream(CONTENT_KEY, reader))
            {
                read(decryptedReader);
            }
        }
    }
}
