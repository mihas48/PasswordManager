using PasswordManager.Core.Interfaces;
using System.Security.Cryptography;

namespace PasswordManager.Services
{
    public class CryptoService : ICryptoService
    {
        public byte[] DeriveKey(string masterPassword, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(masterPassword, salt, 100000, HashAlgorithmName.SHA256, 32);
        }

        public byte[] Encrypt(string plainText, byte[] key)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");

            byte[] encrypted;
            byte[] iv;

            using (Aes aes = Aes.Create())
            {
                aes.GenerateIV();

                iv = aes.IV;

                aes.Key = key;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            byte[] result = new byte[iv.Length + encrypted.Length];

            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

            return result;
        }

        public string Decrypt(byte[] cipherTextWithIv, byte[] key)
        {
            if (cipherTextWithIv == null || cipherTextWithIv.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");

            byte[] iv = new byte[16];
            byte[] cipherText = new byte[cipherTextWithIv.Length - 16];
            Buffer.BlockCopy(cipherTextWithIv, 0, iv, 0, 16);
            Buffer.BlockCopy(cipherTextWithIv, 16, cipherText, 0, cipherTextWithIv.Length - 16);

            string plainText = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plainText;
        }
    }
}
