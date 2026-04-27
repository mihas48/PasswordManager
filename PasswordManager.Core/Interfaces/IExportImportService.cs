using PasswordManager.Core.Models;
using System.Collections.ObjectModel;

namespace PasswordManager.Core.Interfaces
{
    public interface IExportImportService
    {
        public ObservableCollection<PasswordEntry> Entries { get; }
        public List<HashBlock> Blockchain { get; }
        void AddEntry(PasswordEntry entry);
        void UpdateEntry(Guid id, PasswordEntry newEntry);
        void RemoveEntry(Guid id);
        ObservableCollection<PasswordEntry> GetAll();
        void SaveData();
        void LoadData(byte[] key);
    }
}
