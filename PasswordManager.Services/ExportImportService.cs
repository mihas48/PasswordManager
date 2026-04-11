using Newtonsoft.Json;
using PasswordManager.Core.Interfaces;
using PasswordManager.Core.Models;
using System.Collections.ObjectModel;

namespace PasswordManager.Services
{
    public class ExportImportService : IExportImportService
    {
        private List<PasswordEntry> _entries = new();
        private ICryptoService _cryptoService;
        private readonly string _filePath; //файл с зашифрованными данными
        private byte[] _currentKey;

        public ExportImportService(ICryptoService cryptoService, string filePath)
        {
            _cryptoService = cryptoService;
            _filePath = filePath;
        }

        public void AddEntry(PasswordEntry entry)
        {
            _entries.Add(entry);
            SaveData();
        }

        public void UpdateEntry(Guid id, PasswordEntry newEntry)
        {
            var entry = _entries.FirstOrDefault(_entries => _entries.Id == id);

            if (entry == null)
                throw new ArgumentException("Ошибка! В списке нет соответствующего элемента для замены!");

            int index = _entries.IndexOf(entry);

            _entries[index] = newEntry;

            SaveData();
        }

        public void RemoveEntry(Guid id)
        {
            var entry = _entries.FirstOrDefault(_entries => _entries.Id == id);

            if (entry == null)
                throw new ArgumentException("Ошибка! В списке нет соответствующего элемента для удаления!");

            _entries.Remove(entry);
            SaveData();
        }

        public List<PasswordEntry> GetAll()
            => _entries;

        public void SaveData()
        {
            string json = JsonConvert.SerializeObject(_entries);

            byte[] encrypted = _cryptoService.Encrypt(json, _currentKey);

            File.WriteAllBytes(_filePath, encrypted);
        }

        public void LoadData(byte[] key)
        {
            _currentKey = key;
            if (!File.Exists(_filePath))
            {
                _entries = new List<PasswordEntry>();
                return;
            }

            byte[] encryptedData = File.ReadAllBytes(_filePath);
            string json = _cryptoService.Decrypt(encryptedData, _currentKey);
            _entries = JsonConvert.DeserializeObject<List<PasswordEntry>>(json) ?? new List<PasswordEntry>();
        }
    }
}
