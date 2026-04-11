using PasswordManager.Core.Interfaces;
using PasswordManager.Services;
using PasswordManager.App.Views;
using System.Windows;
using System.IO;

namespace PasswordManager.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Создаём экземпляры сервисов
            ICryptoService cryptoService = new CryptoService();
            IAuthService authService = new AuthService(cryptoService);
            IExportImportService exportImportService = new ExportImportService(cryptoService, AppDomain.CurrentDomain.BaseDirectory.ToString());
            ILoggerService loggerService = new LoggerService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"));
            IPasswordGeneratorService passwordGeneratorService = new PasswordGeneratorService();

            // 2. Создаём окно логина и передаём ему сервисы
            var loginWindow = new LoginWindow(authService, exportImportService, cryptoService, loggerService, passwordGeneratorService);
            loginWindow.Show();
        }
    }
}
