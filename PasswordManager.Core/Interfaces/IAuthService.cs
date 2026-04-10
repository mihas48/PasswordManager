namespace PasswordManager.Core.Interfaces
{
    public interface IAuthService
    {
        bool IsMasterPasswordSet();
        void CreateMasterPassword(string password);
        bool ValidateMasterPassword(string password);
        byte[] GetEncryptionKey();
    }
}
