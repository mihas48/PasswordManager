using PasswordManager.Core.Models;
using System.Collections.ObjectModel;

namespace PasswordManager.Core.Interfaces
{
    public interface IExportImportService
    {
        void AddEntry(PasswordEntry entry);
        void UpdateEntry(Guid id, PasswordEntry newEntry);
        void DeleteEntry(Guid id);
        ObservableCollection<PasswordEntry> GetAll();
        void SaveData(byte[] key);
        ObservableCollection<PasswordEntry> LoadData(byte[] key);
    }
}
