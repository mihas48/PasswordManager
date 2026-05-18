namespace PasswordManager.Core.Interfaces
{
    public interface ILoggerService
    {
        void Log(string message);
        IReadOnlyList<string> GetLogs();
        void ClearLogs();
    }
}