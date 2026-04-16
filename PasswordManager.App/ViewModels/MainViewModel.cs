using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Core.Interfaces;
using PasswordManager.Core.Models;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;

namespace PasswordManager.App.ViewModels
{
    partial class MainViewModel : ObservableObject
    {
        //Сервисы
        private readonly ILoggerService _loggerService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;
        private readonly ICryptoService _cryptoService;
        private readonly IExportImportService _exportImportService;
        private readonly byte[] _encryptionKey;

        public MainViewModel(ILoggerService loggerService, IPasswordGeneratorService passwordGeneratorService, 
            ICryptoService cryptoService, IExportImportService exportImportService, byte[] encryptionKey)
        {
            _loggerService = loggerService;
            _passwordGeneratorService = passwordGeneratorService;
            _cryptoService = cryptoService;
            _exportImportService = exportImportService;
            _encryptionKey = encryptionKey;
        }

        //Свойства
        public ObservableCollection<PasswordEntry> Entries => _exportImportService.Entries;

        private byte[] _cipherText;
        private byte[] _key;

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

        // Свойства для добавления новой записи
        [ObservableProperty]
        private string _entryTitle = "";

        [ObservableProperty]
        private string _entryLogin = "";

        [ObservableProperty]
        private string _entryPassword = "";

        [ObservableProperty]
        private string _entryUrl = "";

        [ObservableProperty]
        private string _entryNotes = "";

        //Команды
        [RelayCommand]
        public void Generate()
        {
            if (!UseUpperCase && !UseLowerCase && !UseNumbers && !UseSymbols)
                MessageBox.Show("Выберете как минимум один из \"флажков\" для генерации. Например, Нижний регистр  (a – z).");

            else
                GeneratedPassword = _passwordGeneratorService.Generate(UseUpperCase, UseLowerCase, UseNumbers, UseSymbols, PasswordLength);
        }

        [RelayCommand]
        public void CopyInClipboard()
        {
            Clipboard.SetText(GeneratedPassword);
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

            _cipherText = _cryptoService.Encrypt(TextForEncrypt, key);

            EncryptedText = System.Convert.ToBase64String(_cipherText); 
        }

        [RelayCommand]
        public void Decrypt()
        {
            if (EncryptedText == "")
                throw new ArgumentNullException("Ошибка! Сначала зашифруйте текст");

            DecryptedText =  _cryptoService.Decrypt(_cipherText, _key);
        }

        [RelayCommand]
        public void SaveEntry()
        {
            PasswordEntry newEntry = new PasswordEntry(EntryTitle, EntryLogin, EntryPassword, EntryNotes);

            _exportImportService.AddEntry(newEntry);
        }

        [RelayCommand]
        public void TogglePasswordVisibility(PasswordEntry currentEntry)
        {
            currentEntry.IsPasswordVisible = !currentEntry.IsPasswordVisible;
        }

        [RelayCommand]
        public void CopyPassword(PasswordEntry currentEntry)
        {
            Clipboard.SetText(currentEntry.Password);
        }
    }
}
