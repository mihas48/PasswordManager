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

        public HashBlock(byte[] currentHash, byte[] previousHash, Guid entryId, byte[] entryHash, int blockId, OperationType operation)
        {
            CurrentHash = currentHash;
            PreviousHash = previousHash;
            Date = DateTime.Now;
            EntryId = entryId;
            EntryHash = entryHash;
            BlockId = blockId;
            Operation = operation;
        }
    }
}
