using PasswordManager.Core.Models;
using System.Collections.ObjectModel;

namespace PasswordManager.Core.Interfaces
{
    public interface IBlockchainService
    {
        void VerifyIntegrity(ObservableCollection<PasswordEntry> currentEntries, List<HashBlock> blockchain);
    }
}
