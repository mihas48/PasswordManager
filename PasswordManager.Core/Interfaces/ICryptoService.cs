namespace PasswordManager.Core.Interfaces
{
    public interface ICryptoService
    {
        byte[] DeriveKey(string masterPassword, byte[] salt);
        byte[] Encrypt(string plainText, byte[] key);
        string Decrypt(byte[] cipherText, byte[] key);
    }
}
