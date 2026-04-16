using PasswordManager.Core.Interfaces;
using PasswordManager.Services;
using System.Windows;
using System.IO;

namespace PasswordManager.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Создаём экземпляры сервисов
            ICryptoService cryptoService = new CryptoService();
            IAuthService authService = new AuthService(cryptoService);
            IExportImportService exportImportService = new ExportImportService(cryptoService, AppDomain.CurrentDomain.BaseDirectory.ToString());
            ILoggerService loggerService = new LoggerService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"));
            IPasswordGeneratorService passwordGeneratorService = new PasswordGeneratorService();

            // Проверяем, создан ли профиль
            if (authService.IsMasterPasswordSet())
            {
                // Создаём окно логина и передаём ему сервисы
                var loginWindow = new LoginWindow(authService, exportImportService, cryptoService, loggerService, passwordGeneratorService);
                loginWindow.Show();
            }
            else
            {
                // Создаём окно регистрации и передаём ему сервисы
                var registrationWindow = new RegistrationWindow(authService, exportImportService, cryptoService, loggerService, passwordGeneratorService);
                registrationWindow.Show();
            }
        }
    }
}
