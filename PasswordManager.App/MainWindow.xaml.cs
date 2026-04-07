using System.IO;
using System.Windows;
using PasswordManager.App.ViewModels;
using PasswordManager.Services;

namespace PasswordManager.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var logger = new LoggerService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"));
            var passwordGenerator = new PasswordGeneratorService();
            var crypto = new CryptoService();

            DataContext = new MainViewModel(logger, passwordGenerator, crypto);
        }
    }
}