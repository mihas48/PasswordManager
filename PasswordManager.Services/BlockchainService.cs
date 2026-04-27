using Newtonsoft.Json;
using PasswordManager.Core.Interfaces;
using PasswordManager.Core.Models;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using static System.Net.Mime.MediaTypeNames;
using PasswordManager.Core.Models;

namespace PasswordManager.Services
{
    public class BlockchainService : IBlockchainService
    {
        public void VerifyIntegrity(ObservableCollection<PasswordEntry> currentEntries, List<HashBlock> blockchain)
        {
            Dictionary<Guid, HashBlock> entryBlockPairs = new Dictionary<Guid, HashBlock>();

            //строим словарь "Id → последний блок"
            foreach (HashBlock currentBlock in blockchain)
            {
                HashBlock block = null;

                //нет элемента в словаре - добавление нового элемента в словарь
                if (!entryBlockPairs.TryGetValue(currentBlock.EntryId, out block))
                {
                    entryBlockPairs.Add(currentBlock.EntryId, currentBlock);
                }

                //оставляем в словаре запись с бОльшим индексом блока
                else if (currentBlock.BlockId > block.BlockId)
                {
                    entryBlockPairs.Remove(currentBlock.EntryId);
                    entryBlockPairs.Add(currentBlock.EntryId, currentBlock);
                }
            }

            //поиск в словаре для каждой записи её блока по id
            foreach (PasswordEntry entry in currentEntries)
            {
                //блок, которому соответствует запись из словаря, ключём для поиска которого является id текущей записи для проверки
                HashBlock block = null;

                if (!entryBlockPairs.TryGetValue(entry.Id, out block))
                    throw new Exception("");

                if (block.Operation == HashBlock.OperationType.Deleted && IsEntryExists(currentEntries, block.EntryId))
                    throw new Exception("");

                //блок есть и тип его Created или Updated
                string serializedEntry = JsonConvert.SerializeObject(entry);
                byte[] entryBytes = Encoding.UTF8.GetBytes(serializedEntry);

                if (!SHA256.HashData(entryBytes).SequenceEqual(block.EntryHash))
                    throw new Exception("");
            }
        }

        private bool IsEntryExists(ObservableCollection<PasswordEntry> currentEntries, Guid id)
        {
            foreach (PasswordEntry entry in currentEntries)
            {
                if (entry.Id == id)
                    return true;
            }

            return false;
        }
    }
}
