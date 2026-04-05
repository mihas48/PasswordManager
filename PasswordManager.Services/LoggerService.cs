using PasswordManager.Core.Interfaces;

namespace PasswordManager.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly string _path;

        public string Path { get => _path; }

        public LoggerService(string path) { _path = path; }


        public void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            string fullMessage = $"[{DateTime.Now}] - {message}.";

            File.AppendAllText(_path, fullMessage + "\n");
        }
    }
}
