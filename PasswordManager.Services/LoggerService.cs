using PasswordManager.Core.Interfaces;

namespace PasswordManager.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly List<string> _logs = new List<string>();
        private readonly object _lock = new object();

        public void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            string fullMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] - {message}";
            
            lock (_lock)
            {
                _logs.Add(fullMessage);
            }
        }

        public IReadOnlyList<string> GetLogs()
        {
            lock (_lock)
            {
                return _logs.ToList().AsReadOnly();
            }
        }

        public void ClearLogs()
        {
            lock (_lock)
            {
                _logs.Clear();
            }
        }
    }
}