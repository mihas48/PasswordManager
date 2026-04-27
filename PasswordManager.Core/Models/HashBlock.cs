using System.Security.Cryptography;

namespace PasswordManager.Core.Models
{
    public class HashBlock
    {
        public enum OperationType
        {
            Created,
            Updated,
            Deleted
        }

        public byte[] CurrentHash { get; private set; }
        public byte[] PreviousHash { get; private set; }
        public DateTime Date { get; private set; }
        public Guid EntryId { get; private set; }
        public byte[] EntryHash { get; private set; }
        public int BlockId { get; private set; }
        public OperationType Operation { get; private set; }

        public HashBlock(byte[] previousHash, Guid entryId, byte[] entryHash, int blockId, OperationType operation)
        {
            PreviousHash = previousHash ?? new byte[32];
            EntryId = entryId;
            EntryHash = entryHash;
            BlockId = blockId;
            Operation = operation;
            Date = DateTime.UtcNow;

            using var stream = new MemoryStream();

            stream.Write(PreviousHash, 0, PreviousHash.Length);
            stream.Write(entryId.ToByteArray(), 0, 16);
            stream.Write(entryHash, 0, entryHash.Length);
            stream.Write(BitConverter.GetBytes(blockId), 0, 4);
            stream.WriteByte((byte)operation);
            stream.Write(BitConverter.GetBytes(Date.Ticks), 0, 8);

            CurrentHash = SHA256.HashData(stream.ToArray());
        }
    }
}