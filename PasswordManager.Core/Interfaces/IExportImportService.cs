using PasswordManager.Core.Models;
using System.Collections.ObjectModel;

namespace PasswordManager.Core.Interfaces
{
    public interface IExportImportService
    {
        void AddEntry(PasswordEntry entry);
        void UpdateEntry(Guid id, PasswordEntry newEntry);
        void RemoveEntry(Guid id);
        List<PasswordEntry> GetAll();
        void SaveData();
        void LoadData(byte[] key);
    }
}
