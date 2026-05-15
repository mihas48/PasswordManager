using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PasswordManager.Core.Interfaces;
using PasswordManager.Core.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

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
                _loggerService.Log($"Добавлена запись");
            }
            catch (Exception ex)
            {
                _loggerService.Log($"Ошибка при добавлении записи: {ex.Message}");
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                _loggerService.Log($"Удалена запись");
            }
            catch (Exception ex)
            {
                _loggerService.Log($"Ошибка при удалении записи: {ex.Message}");
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
                _loggerService.Log($"Обновлена запись");
            }
            catch (Exception ex)
            {
                _loggerService.Log($"Ошибка при обновлении записи: {ex.Message}");
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        [RelayCommand]
        public void GenerateLogReport()
        {
            var logs = _loggerService.GetLogs();
            if (logs.Count == 0)
            {
                MessageBox.Show("Нет логов для отображения.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "HTML files (*.html)|*.html",
                DefaultExt = ".html",
                FileName = $"LogReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"
            };

            if (saveDialog.ShowDialog() == true)
            {
                string htmlContent = BuildHtmlReport(logs);
                File.WriteAllText(saveDialog.FileName, htmlContent);
                MessageBox.Show($"Отчёт сохранён: {saveDialog.FileName}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //Вспомогательные методы
        private string BuildHtmlReport(IReadOnlyList<string> logs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>Журнал событий Password Manager</title>");
            sb.AppendLine(@"
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #F0F9F0;
            margin: 40px;
        }
        h1 {
            color: #2E7D32;
            border-bottom: 2px solid #81C784;
            padding-bottom: 10px;
        }
        .log-container {
            background: white;
            border-radius: 12px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            overflow: hidden;
        }
        table {
            width: 100%;
            border-collapse: collapse;
        }
        th {
            background: #2E7D32;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: 600;
        }
        td {
            padding: 10px 12px;
            border-bottom: 1px solid #E8F5E9;
            font-family: 'Consolas', monospace;
            font-size: 13px;
        }
        tr:hover {
            background-color: #F1F8E9;
        }
        .footer {
            margin-top: 20px;
            font-size: 12px;
            color: #558B2F;
            text-align: center;
        }
    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1> Журнал действий Password Manager</h1>");
            sb.AppendLine("<div class='log-container'>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>#</th><th>Дата и время</th><th>Сообщение</th></tr>");

            int index = 1;
            foreach (string log in logs)
            {
                int firstBracket = log.IndexOf(']');
                if (firstBracket > 0)
                {
                    string timestamp = log.Substring(1, firstBracket - 1); // без первой скобки
                    string message = log.Substring(firstBracket + 3); // пропускаем "] - "
                    sb.AppendLine($"<tr><td>{index++}</td><td>{timestamp}</td><td>{EscapeHtml(message)}</td></tr>");
                }
                else
                {
                    // fallback
                    sb.AppendLine($"<tr><td>{index++}</td><td>—</td><td>{EscapeHtml(log)}</td></tr>");
                }
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<div class='footer'>Сгенерировано: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string EscapeHtml(string text)
        {
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
