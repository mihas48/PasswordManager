namespace PasswordManager.Core.Interfaces
{
    public interface ICryptoService
    {
        public byte[] DeriveKey(string masterPassword, byte[] salt);
        public byte[] Encrypt(string plainText, byte[] key);
        public string Decrypt(byte[] cipherTextWithIv, byte[] key);
    }
}
