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

        private PasswordEntry _selectedEntry;

        public PasswordEntry SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                if (_selectedEntry != value)
                {
                    _selectedEntry = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        EntryTitle = value.Title;
                        EntryLogin = value.Login;
                        EntryPassword = value.Password;
                        EntryNotes = value.Notes;
                    }
                    else
                    {
                        EntryTitle = EntryLogin = EntryPassword = EntryNotes = "";
                    }
                }
            }
        }

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

        //команда для демонстрации шифрования
        [RelayCommand]
        public void Encrypt()
        {
            try
            {
                if (TextForEncrypt == "")
                {
                    throw new ArgumentNullException("Текс для шифрования");
                }

                byte[] salt = new byte[32];
                RandomNumberGenerator.Fill(salt);

                //предопределённый пароль
                string masterPassword = "VeryStrongPassword";

                byte[] key = _cryptoService.DeriveKey(masterPassword, salt);
                _key = key;

                _cipherText = _cryptoService.Encrypt(TextForEncrypt, key);

                EncryptedText = System.Convert.ToBase64String(_cipherText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        //команда для демонстрации дешифрования
        [RelayCommand]
        public void Decrypt()
        {
            try
            {
                if (EncryptedText == "")
                {
                    throw new ArgumentException("Сначала зашифруйте текст");
                }

                DecryptedText = _cryptoService.Decrypt(_cipherText, _key);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public void SaveEntry()
        {
            try
            {
                PasswordEntry newEntry = new PasswordEntry(EntryTitle, EntryLogin, EntryPassword, EntryNotes);
                newEntry.UpdatedAt = DateTime.Now;

                _exportImportService.AddEntry(newEntry);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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

        [RelayCommand]
        public void RemoveEntry()
        {
            try
            {
            if (SelectedEntry == null)
            {
                throw new ArgumentException("Перед удалением выберете элемент из списка");
            }

            _exportImportService.RemoveEntry(SelectedEntry.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        [RelayCommand]
        public void UpdateEntry()
        {
            try
            {
                if (SelectedEntry == null)
                {
                    throw new ArgumentException("Перед редактированием выберете элемент из списка");
                }

                PasswordEntry newEntry = new PasswordEntry(EntryTitle, EntryLogin, EntryPassword, EntryNotes);
                newEntry.UpdatedAt = DateTime.Now;

                if (SelectedEntry.Title == EntryTitle && SelectedEntry.Login == EntryLogin &&
                    SelectedEntry.Password == EntryPassword && SelectedEntry.Notes == EntryNotes)
                {
                    throw new ArgumentException("Новые данные полностью соответствуют исходным");
                }

                _exportImportService.UpdateEntry(SelectedEntry.Id, newEntry);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
