using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Core.Interfaces;
using PasswordManager.Services;
using System.Numerics;
using System.Security.Cryptography;

namespace PasswordManager.App.ViewModels
{
    partial class MainViewModel : ObservableObject
    {
        //Сервисы
        private readonly ILoggerService _loggerService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;
        private readonly ICryptoService _cryptoService;

        public MainViewModel(ILoggerService loggerService, IPasswordGeneratorService passwordGeneratorService, ICryptoService cryptoService)
        {
            _loggerService = loggerService;
            _passwordGeneratorService = passwordGeneratorService;
            _cryptoService = cryptoService;
        }

        //Свойства
        [ObservableProperty]
        private bool _useUpperCase = false;

        [ObservableProperty]
        private bool _useLowerCase = false;

        [ObservableProperty]
        private bool _useNumbers = false;

        [ObservableProperty]
        private bool _useSymbols = false;

        [ObservableProperty]
        private int _passwordLength = 8;

        [ObservableProperty]
        private string _passwordLengthText = "8";

        [ObservableProperty]
        private string _generatedPassword = "";

        [ObservableProperty]
        private string _textForEncrypt = "";

        [ObservableProperty]
        private string _encryptedText = "";

        [ObservableProperty]
        private string _decryptedText = "";

        private byte[] _cipherText;
        private byte[] _key;
        private byte[] _iv;

        //Команды
        [RelayCommand]
        public void Generate()
        {
            GeneratedPassword = _passwordGeneratorService.Generate(UseUpperCase, UseLowerCase, UseNumbers, UseSymbols, PasswordLength);
        }

        [RelayCommand]
        public void Log()
        {
            _loggerService.Log("Сообщение");
        }

        [RelayCommand]
        public void Encrypt()
        {
            if (TextForEncrypt == "")
                throw new ArgumentNullException("Ошибка! Поле текста для шифрования пусто!");

            byte[] salt = new byte[32];
            RandomNumberGenerator.Fill(salt);

            string masterPassword = "VeryStrongPassword";

            byte[] key = _cryptoService.DeriveKey(masterPassword, salt);
            _key = key;

            byte[] data = _cryptoService.Encrypt(TextForEncrypt, key);

            byte[] iv = new byte[16];
            _iv = iv;
            byte[] cipher = new byte[16];

            Buffer.BlockCopy(data, 0, iv, 0, 16);
            Buffer.BlockCopy(data, 16, cipher, 0, 16);

            _cipherText = cipher;

            EncryptedText = System.Convert.ToBase64String(cipher); 
        }

        [RelayCommand]
        public void Decrypt()
        {
            if (EncryptedText == "")
                throw new ArgumentNullException("Ошибка! Сначала зашифруйте текст");

            DecryptedText =  _cryptoService.Decrypt(_cipherText, _key, _iv);
        }
    }
}
