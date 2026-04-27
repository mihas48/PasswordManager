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

            ICryptoService cryptoService = new CryptoService();
            IAuthService authService = new AuthService(cryptoService);
            IBlockchainService blockchainService = new BlockchainService();
            IExportImportService exportImportService = new ExportImportService(
                blockchainService,
                cryptoService,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "passwords.dat"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blockchain.dat")
            );
            ILoggerService loggerService = new LoggerService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"));
            IPasswordGeneratorService passwordGeneratorService = new PasswordGeneratorService();

            if (authService.IsMasterPasswordSet())
            {
                var loginWindow = new LoginWindow(authService, exportImportService, cryptoService, loggerService, passwordGeneratorService);
                loginWindow.Show();
            }
            else
            {
                var registrationWindow = new RegistrationWindow(authService, exportImportService, cryptoService, loggerService, passwordGeneratorService);
                registrationWindow.Show();
            }
        }
    }
}