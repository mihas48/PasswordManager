using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PasswordManager.Core.Interfaces;
using PasswordManager.Core.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace PasswordManager.App.ViewModels
{
    partial class MainViewModel : ObservableObject
    {
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

        //СВОЙСТВА 
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

        [ObservableProperty] private bool _useUpperCase = false;
        [ObservableProperty] private bool _useLowerCase = false;
        [ObservableProperty] private bool _useNumbers = false;
        [ObservableProperty] private bool _useSymbols = false;
        [ObservableProperty] private int _passwordLength = 8;
        [ObservableProperty] private string _passwordLengthText = "8";
        [ObservableProperty] private string _generatedPassword = "";
        [ObservableProperty] private string _textForEncrypt = "";
        [ObservableProperty] private string _encryptedText = "";
        [ObservableProperty] private string _decryptedText = "";
        [ObservableProperty] private string _entryTitle = "";
        [ObservableProperty] private string _entryLogin = "";
        [ObservableProperty] private string _entryPassword = "";
        [ObservableProperty] private string _entryUrl = "";
        [ObservableProperty] private string _entryNotes = "";

        //КОМАНДЫ (существующие)
        [RelayCommand]
        public void Generate()
        {
            if (!UseUpperCase && !UseLowerCase && !UseNumbers && !UseSymbols)
                MessageBox.Show("Выберите как минимум один из \"флажков\" для генерации.");
            else
                GeneratedPassword = _passwordGeneratorService.Generate(UseUpperCase, UseLowerCase, UseNumbers, UseSymbols, PasswordLength);
        }

        [RelayCommand] public void CopyInClipboard() => Clipboard.SetText(GeneratedPassword);

        [RelayCommand]
        public void Encrypt()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TextForEncrypt))
                    throw new Exception("Поле текста для шифрования не может быть пустым!");
                byte[] salt = new byte[32];
                RandomNumberGenerator.Fill(salt);
                string masterPassword = "VeryStrongPassword";
                _key = _cryptoService.DeriveKey(masterPassword, salt);
                _cipherText = _cryptoService.Encrypt(TextForEncrypt, _key);
                EncryptedText = Convert.ToBase64String(_cipherText);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public void Decrypt()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EncryptedText))
                    throw new ArgumentException("Сначала зашифруйте текст");
                DecryptedText = _cryptoService.Decrypt(_cipherText, _key);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public void SaveEntry()
        {
            try
            {
                var entry = new PasswordEntry(EntryTitle, EntryLogin, EntryPassword, EntryNotes) { UpdatedAt = DateTime.Now };
                _exportImportService.AddEntry(entry);
                _loggerService.Log("Добавлена запись");
            }
            catch (Exception ex)
            {
                _loggerService.Log($"Ошибка при добавлении: {ex.Message}");
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand] public void TogglePasswordVisibility(PasswordEntry entry) => entry.IsPasswordVisible = !entry.IsPasswordVisible;
        [RelayCommand] public void CopyPassword(PasswordEntry entry) => Clipboard.SetText(entry.Password);

        [RelayCommand]
        public void RemoveEntry()
        {
            try
            {
                if (SelectedEntry == null) throw new ArgumentException("Перед удалением выберите элемент из списка");
                _exportImportService.RemoveEntry(SelectedEntry.Id);
                _loggerService.Log("Удалена запись");
            }
            catch (Exception ex)
            {
                _loggerService.Log($"Ошибка при удалении: {ex.Message}");
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public void UpdateEntry()
        {
            try
            {
                if (SelectedEntry == null) throw new ArgumentException("Перед редактированием выберите элемент из списка");
                var newEntry = new PasswordEntry(EntryTitle, EntryLogin, EntryPassword, EntryNotes) { UpdatedAt = DateTime.Now };
                if (SelectedEntry.Title == EntryTitle && SelectedEntry.Login == EntryLogin &&
                    SelectedEntry.Password == EntryPassword && SelectedEntry.Notes == EntryNotes)
                    throw new ArgumentException("Новые данные полностью соответствуют исходным");
                _exportImportService.UpdateEntry(SelectedEntry.Id, newEntry);
                _loggerService.Log("Обновлена запись");
            }
            catch (Exception ex)
            {
                _loggerService.Log($"Ошибка при обновлении: {ex.Message}");
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        [RelayCommand]
        public void GenerateSingleEntryReport()
        {
            if (SelectedEntry == null)
            {
                MessageBox.Show("Сначала выберите запись в таблице.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var dlg = new SaveFileDialog { Filter = "HTML files (*.html)|*.html", DefaultExt = ".html", FileName = $"Entry_{SelectedEntry.Title}.html" };
            if (dlg.ShowDialog() == true)
            {
                string html = BuildSingleEntryHtml(SelectedEntry);
                File.WriteAllText(dlg.FileName, html);
                MessageBox.Show($"Отчёт сохранён: {dlg.FileName}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        public void GenerateSelectedEntriesReport()
        {
            var selected = Entries.Where(e => e.IsSelected).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Не отмечено ни одной записи. Отметьте записи галочками в таблице.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var dlg = new SaveFileDialog { Filter = "HTML files (*.html)|*.html", DefaultExt = ".html", FileName = $"SelectedEntries_{DateTime.Now:yyyyMMdd}.html" };
            if (dlg.ShowDialog() == true)
            {
                string html = BuildSelectedEntriesHtml(selected);
                File.WriteAllText(dlg.FileName, html);
                MessageBox.Show($"Отчёт сохранён: {dlg.FileName}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        public void GenerateStatisticsReport()
        {
            if (Entries.Count == 0)
            {
                MessageBox.Show("Нет записей для формирования статистики.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var dlg = new SaveFileDialog { Filter = "HTML files (*.html)|*.html", DefaultExt = ".html", FileName = $"Statistics_{DateTime.Now:yyyyMMdd}.html" };
            if (dlg.ShowDialog() == true)
            {
                string html = BuildStatisticsHtml();
                File.WriteAllText(dlg.FileName, html);
                MessageBox.Show($"Отчёт сохранён: {dlg.FileName}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string BuildSingleEntryHtml(PasswordEntry entry)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Карточка записи</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI;background:#F0F9F0;margin:40px}h1{color:#2E7D32;border-bottom:2px solid #81C784}.card{background:white;border-radius:12px;padding:20px;box-shadow:0 2px 8px rgba(0,0,0,0.1);margin-top:20px}td{padding:8px 16px}</style>");
            sb.AppendLine("</head><body><h1>🔐 Карточка учётной записи</h1>");
            sb.AppendLine("<div class='card'><table>");
            sb.AppendLine($"<tr><td><b>Название:</b></td><td>{EscapeHtml(entry.Title)}</td></tr>");
            sb.AppendLine($"<tr><td><b>Логин:</b></td><td>{EscapeHtml(entry.Login)}</td></tr>");
            sb.AppendLine($"<tr><td><b>Пароль:</b></td><td>{new string('*', entry.Password?.Length ?? 0)}</td></tr>");
            sb.AppendLine($"<tr><td><b>Заметки:</b></td><td>{EscapeHtml(entry.Notes ?? "-")}</td></tr>");
            sb.AppendLine($"<tr><td><b>Дата обновления:</b></td><td>{entry.UpdatedAt:yyyy-MM-dd HH:mm}</td></tr>");
            sb.AppendLine("</table></div>");

            // История из блокчейна
            var blocks = _exportImportService.Blockchain?.Where(b => b.EntryId == entry.Id).OrderBy(b => b.BlockId).ToList();
            if (blocks != null && blocks.Count > 0)
            {
                sb.AppendLine("<h2>📜 История операций</h2><div class='card'><table>");
                sb.AppendLine("<tr><th>Дата</th><th>Операция</th></tr>");
                foreach (var b in blocks)
                    sb.AppendLine($"<tr><td>{b.Date:yyyy-MM-dd HH:mm}</td><td>{b.Operation}</td></tr>");
                sb.AppendLine("</table></div>");
            }
            sb.AppendLine($"<p style='margin-top:20px;color:#558B2F'>Сгенерировано: {DateTime.Now:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private string BuildSelectedEntriesHtml(List<PasswordEntry> entries)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Сводка выбранных записей</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI;background:#F0F9F0;margin:40px}h1{color:#2E7D32}table{width:100%;border-collapse:collapse;background:white}th{background:#2E7D32;color:white;padding:10px}td{border-bottom:1px solid #E8F5E9;padding:8px}</style>");
            sb.AppendLine("</head><body><h1>📋 Сводка выбранных записей</h1>");
            sb.AppendLine("<table><tr><th>Название</th><th>Логин</th><th>Пароль</th><th>Заметки</th><th>Обновлено</th></tr>");
            foreach (var e in entries)
            {
                sb.AppendLine($"<tr><td>{EscapeHtml(e.Title)}</td><td>{EscapeHtml(e.Login)}</td><td>{new string('*', e.Password?.Length ?? 0)}</td><td>{EscapeHtml(e.Notes ?? "")}</td><td>{e.UpdatedAt:yyyy-MM-dd}</td></tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine($"<p>Всего записей: {entries.Count}</p>");
            sb.AppendLine($"<p style='color:#558B2F'>Сгенерировано: {DateTime.Now:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private string BuildStatisticsHtml()
        {
            var entries = Entries.ToList();
            int total = entries.Count;
            int weakPwdCount = entries.Count(e => e.Password?.Length < 8);
            double avgLength = entries.Any() ? entries.Average(e => e.Password?.Length ?? 0) : 0;
            var oldest = entries.OrderBy(e => e.UpdatedAt).FirstOrDefault();
            var newest = entries.OrderByDescending(e => e.UpdatedAt).FirstOrDefault();
            var outdated = entries.Count(e => (DateTime.Now - e.UpdatedAt).TotalDays > 90);
            var blockchainOps = _exportImportService.Blockchain?.GroupBy(b => b.Operation).ToDictionary(g => g.Key, g => g.Count());
            int created = blockchainOps?.GetValueOrDefault(HashBlock.OperationType.Created, 0) ?? 0;
            int updated = blockchainOps?.GetValueOrDefault(HashBlock.OperationType.Updated, 0) ?? 0;
            int deleted = blockchainOps?.GetValueOrDefault(HashBlock.OperationType.Deleted, 0) ?? 0;
            var logs = _loggerService.GetLogs();
            string firstLog = logs.FirstOrDefault() ?? "—";
            string lastLog = logs.LastOrDefault() ?? "—";

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Статистика хранилища</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI;background:#F0F9F0;margin:40px}h1{color:#2E7D32;border-bottom:2px solid #81C784}.card{background:white;border-radius:12px;padding:20px;box-shadow:0 2px 8px rgba(0,0,0,0.1);margin:20px 0}td{padding:8px 16px}</style>");
            sb.AppendLine("</head><body><h1>📊 Статистика хранилища паролей</h1>");
            sb.AppendLine("<div class='card'><table>");
            sb.AppendLine($"<tr><td><b>Общее количество записей:</b></td><td>{total}</td></tr>");
            sb.AppendLine($"<tr><td><b>Средняя длина паролей:</b></td><td>{avgLength:F1} симв.</td></tr>");
            sb.AppendLine($"<tr><td><b>Слабых паролей (&lt;8 симв.):</b></td><td>{weakPwdCount}</td></tr>");
            sb.AppendLine($"<tr><td><b>Самая старая запись:</b></td><td>{oldest?.UpdatedAt:yyyy-MM-dd} ({oldest?.Title})</td></tr>");
            sb.AppendLine($"<tr><td><b>Самая новая запись:</b></td><td>{newest?.UpdatedAt:yyyy-MM-dd} ({newest?.Title})</td></tr>");
            sb.AppendLine($"<tr><td><b>Записей без обновлений &gt;90 дней:</b></td><td>{outdated}</td></tr>");
            sb.AppendLine("</table></div>");

            sb.AppendLine("<h2>🔗 Блокчейн-статистика</h2><div class='card'><table>");
            sb.AppendLine($"<tr><td>Создано (Created):</td><td>{created}</td></tr>");
            sb.AppendLine($"<tr><td>Обновлено (Updated):</td><td>{updated}</td></tr>");
            sb.AppendLine($"<tr><td>Удалено (Deleted):</td><td>{deleted}</td></tr>");
            sb.AppendLine($"<tr><td>Всего блоков:</td><td>{created + updated + deleted}</td></tr>");
            sb.AppendLine("</table></div>");

            sb.AppendLine("<h2>📄 Журнал действий</h2><div class='card'><table>");
            sb.AppendLine($"<tr><td>Первая запись в логе:</td><td>{firstLog}</td></tr>");
            sb.AppendLine($"<tr><td>Последняя запись в логе:</td><td>{lastLog}</td></tr>");
            sb.AppendLine($"<tr><td>Всего записей в логе:</td><td>{logs.Count}</td></tr>");
            sb.AppendLine("</table></div>");

            sb.AppendLine($"<p style='color:#558B2F;margin-top:20px'>Сгенерировано: {DateTime.Now:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string EscapeHtml(string text) =>
            text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}