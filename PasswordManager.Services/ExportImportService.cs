using Newtonsoft.Json;
using PasswordManager.Core.Interfaces;
using PasswordManager.Core.Models;
using System.Collections.ObjectModel;

namespace PasswordManager.Services
{
    public class ExportImportService : IExportImportService
    {
        public ObservableCollection<PasswordEntry> Entries { get; private set; }
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
            Entries.Add(entry);
            SaveData();
        }

        public void UpdateEntry(Guid id, PasswordEntry newEntry)
        {
            var entry = Entries.FirstOrDefault(Entries => Entries.Id == id);

            if (entry == null)
                throw new ArgumentException("Ошибка! В списке нет соответствующего элемента для замены!");

            int index = Entries.IndexOf(entry);

            Entries[index] = newEntry;

            SaveData();
        }

        public void RemoveEntry(Guid id)
        {
            var entry = Entries.FirstOrDefault(Entries => Entries.Id == id);

            if (entry == null)
                throw new ArgumentException("Ошибка! В списке нет соответствующего элемента для удаления!");

            Entries.Remove(entry);
            SaveData();
        }

        public ObservableCollection<PasswordEntry> GetAll()
            => Entries;

        public void SaveData()
        {
            string json = JsonConvert.SerializeObject(Entries);

            byte[] encrypted = _cryptoService.Encrypt(json, _currentKey);

            File.WriteAllBytes(_filePath, encrypted);
        }

        public void LoadData(byte[] key)
        {
            _currentKey = key;
            if (!File.Exists(_filePath))
            {
                Entries = new ObservableCollection<PasswordEntry>();
                return;
            }

            byte[] encryptedData = File.ReadAllBytes(_filePath);
            string json = _cryptoService.Decrypt(encryptedData, _currentKey);
            Entries = JsonConvert.DeserializeObject<ObservableCollection<PasswordEntry>>(json) ?? new ObservableCollection<PasswordEntry>();
        }
    }
}
