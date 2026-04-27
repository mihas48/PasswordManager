using Newtonsoft.Json;
using PasswordManager.Core.Interfaces;
using PasswordManager.Core.Models;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Services
{
    public class ExportImportService : IExportImportService
    {
        public ObservableCollection<PasswordEntry> Entries { get; private set; }
        public List<HashBlock> Blockchain { get; private set; }
        private ICryptoService _cryptoService;
        private IBlockchainService _blockchainService;
        private readonly string _dataFilePath;
        private readonly string _blockchainFilePath;
        private byte[] _currentKey;

        public ExportImportService(IBlockchainService blockchainService, ICryptoService cryptoService, string dataFilePath, string blockchainFilePath)
        {
            _blockchainService = blockchainService;
            _cryptoService = cryptoService;
            _dataFilePath = dataFilePath;
            _blockchainFilePath = blockchainFilePath;

            Entries = new ObservableCollection<PasswordEntry>();
            LoadBlockchain();
        }

        private void SaveBlockchain()
        {
            string json = JsonConvert.SerializeObject(Blockchain);
            File.WriteAllText(_blockchainFilePath, json);
        }

        private void LoadBlockchain()
        {
            if (File.Exists(_blockchainFilePath))
            {
                string json = File.ReadAllText(_blockchainFilePath);
                Blockchain = JsonConvert.DeserializeObject<List<HashBlock>>(json) ?? new List<HashBlock>();
            }
            else
            {
                Blockchain = new List<HashBlock>();
            }
        }

        private void AddBlock(Guid entryId, byte[] entryHash, HashBlock.OperationType operation)
        {
            int newBlockId = Blockchain.Count;
            byte[] previousHash = Blockchain.Count > 0 ? Blockchain.Last().CurrentHash : null;
            var newBlock = new HashBlock(previousHash, entryId, entryHash, newBlockId, operation);
            Blockchain.Add(newBlock);
            SaveBlockchain();
        }

        public void AddEntry(PasswordEntry entry)
        {
            Entries.Add(entry);
            SaveData();

            byte[] entryHash = ComputeEntryHash(entry);
            AddBlock(entry.Id, entryHash, HashBlock.OperationType.Created);
        }

        public void UpdateEntry(Guid id, PasswordEntry newEntry)
        {
            var entry = Entries.FirstOrDefault(e => e.Id == id);
            if (entry == null)
                throw new ArgumentException("Entry not found for update.");

            int index = Entries.IndexOf(entry);
            Entries[index] = newEntry;
            SaveData();

            byte[] entryHash = ComputeEntryHash(newEntry);
            AddBlock(id, entryHash, HashBlock.OperationType.Updated);
        }

        public void RemoveEntry(Guid id)
        {
            var entry = Entries.FirstOrDefault(e => e.Id == id);
            if (entry == null)
                throw new ArgumentException("Entry not found for removal.");

            Entries.Remove(entry);
            SaveData();

            byte[] entryHash = ComputeEntryHash(entry);
            AddBlock(id, entryHash, HashBlock.OperationType.Deleted);
        }

        public ObservableCollection<PasswordEntry> GetAll() => Entries;

        private byte[] ComputeEntryHash(PasswordEntry entry)
        {
            string serialized = JsonConvert.SerializeObject(entry);
            byte[] bytes = Encoding.UTF8.GetBytes(serialized);
            return SHA256.HashData(bytes);
        }

        public void SaveData()
        {
            string json = JsonConvert.SerializeObject(Entries);
            byte[] encrypted = _cryptoService.Encrypt(json, _currentKey);
            File.WriteAllBytes(_dataFilePath, encrypted);
        }

        public void LoadData(byte[] key)
        {
            _currentKey = key;
            if (!File.Exists(_dataFilePath))
            {
                Entries = new ObservableCollection<PasswordEntry>();
                return;
            }

            byte[] encryptedData = File.ReadAllBytes(_dataFilePath);
            string json = _cryptoService.Decrypt(encryptedData, _currentKey);
            Entries = JsonConvert.DeserializeObject<ObservableCollection<PasswordEntry>>(json) ?? new ObservableCollection<PasswordEntry>();

            if (Blockchain.Count > 0)
                _blockchainService.VerifyIntegrity(Entries, Blockchain);
        }
    }
}