using Newtonsoft.Json;
using PasswordManager.Core.Interfaces;
using PasswordManager.Core.Models;
using System.Collections.ObjectModel;

namespace PasswordManager.Services
{
    public class ExportImportService : IExportImportService
    {
        private ObservableCollection<PasswordEntry> _entries = new ObservableCollection<PasswordEntry>();
        private ICryptoService _cryptoService;
        private readonly string _filePath; //файл с зашифрованными данными

        public ExportImportService(ICryptoService cryptoService, string filePath)
        {
            _cryptoService = cryptoService;
            _filePath = filePath;
        }

        public void AddEntry(PasswordEntry entry)
        {
            _entries.Add(entry);
        }

        public void UpdateEntry(Guid id, PasswordEntry newEntry)
        {
            var entry = _entries.FirstOrDefault(_entries => _entries.Id == id);

            if (entry == null)
                throw new ArgumentException("Ошибка! В списке нет соответствующего элемента для замены!");

            int index = _entries.IndexOf(entry);

            _entries[index] = newEntry;
        }

        public void DeleteEntry(Guid id)
        {
            var entry = _entries.FirstOrDefault(_entries => _entries.Id == id);

            if (entry == null)
                throw new ArgumentException("Ошибка! В списке нет соответствующего элемента для удаления!");

            _entries.Remove(entry);
        }

        public ObservableCollection<PasswordEntry> GetAll()
            => _entries;

        public void SaveData(byte[] key)
        {
            string json = JsonConvert.SerializeObject(_entries);

            byte[] encrypted = _cryptoService.Encrypt(json, key);

            File.WriteAllBytes(_filePath, encrypted);
        }

        public ObservableCollection<PasswordEntry> LoadData(byte[] key)
        {
            byte[] encrypted = File.ReadAllBytes(_filePath);
            string plainText = _cryptoService.Decrypt(encrypted, key);

            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("Ошибка! Файл не содержит данных!");

            ObservableCollection<PasswordEntry> newEntries = new ObservableCollection<PasswordEntry>(JsonConvert.DeserializeObject<ObservableCollection<PasswordEntry>>(plainText));

            return newEntries;
        }
    }
}
