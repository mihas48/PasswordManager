using PasswordManager.Core.Interfaces;
using System.Security.Cryptography;

namespace PasswordManager.Services
{
    public class AuthService : IAuthService
    {
        private readonly ICryptoService _cryptoService;
        private byte[] _currentKey;

        public AuthService(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public bool IsMasterPasswordSet()
            => (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "master.dat")));

        public void CreateMasterPassword(string password)
        {
            byte[] salt = new byte[32];
            RandomNumberGenerator.Fill(salt);

            byte[] key = _cryptoService.DeriveKey(password, salt);

            byte[] data = new byte[salt.Length + key.Length];
            Buffer.BlockCopy(salt, 0, data, 0, salt.Length);
            Buffer.BlockCopy(key, 0, data, salt.Length, key.Length);
            File.WriteAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "master.dat"), data);
        }

        public bool ValidateMasterPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("Ошибка! Пароль не может быть пустым!");

            byte[] fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "master.dat"));

            byte[] salt = new byte[32];
            byte[] masterPassword = new byte[32];

            Buffer.BlockCopy(fileData, 0, salt, 0, 32);
            Buffer.BlockCopy(fileData, 32, masterPassword, 0, 32);

            byte[] passwordHash = _cryptoService.DeriveKey(password, salt);

            _currentKey = passwordHash;

            return masterPassword.SequenceEqual(passwordHash);
        }

        public byte[] GetEncryptionKey()
        {
            if (_currentKey == null)
                throw new InvalidOperationException("Сначала выполните вход");
            return _currentKey;
        }
    }
}
