using PasswordManager.App.ViewModels;
using PasswordManager.Core.Interfaces;
using System.Windows;

namespace PasswordManager.App
{
    public partial class MainWindow : Window
    {
        public MainWindow(IExportImportService exportImportService, IPasswordGeneratorService passwordGeneratorService,
            ICryptoService cryptoService, ILoggerService loggerService, byte[] encryptionKey)
        {
            InitializeComponent();
            var vm = new MainViewModel(loggerService, passwordGeneratorService, cryptoService, exportImportService, encryptionKey);
            DataContext = vm;
        }
    }
}