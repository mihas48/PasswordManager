using Newtonsoft.Json;
using PasswordManager.Core.Interfaces;
using PasswordManager.Core.Models;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Services
{
    public class BlockchainService : IBlockchainService
    {
        public void VerifyIntegrity(ObservableCollection<PasswordEntry> currentEntries, List<HashBlock> blockchain)
        {
            // 1. Проверка связности цепочки и корректности CurrentHash
            byte[] prevHash = null;
            for (int i = 0; i < blockchain.Count; i++)
            {
                var block = blockchain[i];

                // Пересчитываем хеш блока (должен совпадать с block.CurrentHash)
                using var stream = new MemoryStream();
                stream.Write(block.PreviousHash, 0, block.PreviousHash.Length);
                stream.Write(block.EntryId.ToByteArray(), 0, 16);
                stream.Write(block.EntryHash, 0, block.EntryHash.Length);
                stream.Write(BitConverter.GetBytes(block.BlockId), 0, 4);
                stream.WriteByte((byte)block.Operation);
                stream.Write(BitConverter.GetBytes(block.Date.Ticks), 0, 8);

                byte[] recomputedHash = SHA256.HashData(stream.ToArray());
                if (!recomputedHash.SequenceEqual(block.CurrentHash))
                    throw new InvalidOperationException("Blockchain corrupted: CurrentHash mismatch.");

                if (i > 0)
                {
                    if (!block.PreviousHash.SequenceEqual(prevHash))
                        throw new InvalidOperationException("Blockchain corrupted: PreviousHash mismatch.");
                }
                else
                {
                    if (block.BlockId != 0)
                        throw new InvalidOperationException("First block must have BlockId 0.");
                    byte[] zeroHash = new byte[32];
                    if (!block.PreviousHash.SequenceEqual(zeroHash))
                        throw new InvalidOperationException("First block must have zero PreviousHash.");
                }

                prevHash = block.CurrentHash;
            }

            // 2. Словарь последних блоков для каждой записи
            Dictionary<Guid, HashBlock> lastBlockForEntry = new Dictionary<Guid, HashBlock>();
            foreach (HashBlock block in blockchain)
            {
                if (lastBlockForEntry.TryGetValue(block.EntryId, out var existing) && existing.BlockId >= block.BlockId)
                    continue;
                lastBlockForEntry[block.EntryId] = block;
            }

            // 3. Проверка, что каждая существующая запись соответствует своему последнему блоку
            foreach (PasswordEntry entry in currentEntries)
            {
                if (!lastBlockForEntry.TryGetValue(entry.Id, out var block))
                    throw new InvalidOperationException($"No block found for entry {entry.Id}.");

                if (block.Operation == HashBlock.OperationType.Deleted)
                    throw new InvalidOperationException($"Entry {entry.Id} exists but last block is Deleted.");

                string serializedEntry = JsonConvert.SerializeObject(entry);
                byte[] entryBytes = Encoding.UTF8.GetBytes(serializedEntry);
                byte[] entryHash = SHA256.HashData(entryBytes);
                if (!entryHash.SequenceEqual(block.EntryHash))
                    throw new InvalidOperationException($"Entry {entry.Id} hash mismatch.");
            }

            // 4. Проверка, что для всех блоков Created/Updated запись существует, а для Deleted – отсутствует
            foreach (var kv in lastBlockForEntry)
            {
                var block = kv.Value;
                bool entryExists = currentEntries.Any(e => e.Id == block.EntryId);
                if (block.Operation == HashBlock.OperationType.Deleted && entryExists)
                    throw new InvalidOperationException($"Deleted block found but entry {block.EntryId} still exists.");
                if (block.Operation != HashBlock.OperationType.Deleted && !entryExists)
                    throw new InvalidOperationException($"Entry {block.EntryId} does not exist but last block operation is {block.Operation}.");
            }
        }
    }
}