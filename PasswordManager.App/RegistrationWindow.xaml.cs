using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Core.Interfaces;
using PasswordManager.Services;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace PasswordManager.App
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        IAuthService _authService;
        IExportImportService _exportImportService;
        ICryptoService _cryptoService;
        ILoggerService _loggerService;
        IPasswordGeneratorService _passwordGeneratorService;

        public RegistrationWindow(IAuthService authService, IExportImportService exportImportService, ICryptoService cryptoService,
            ILoggerService loggerService, IPasswordGeneratorService passwordGeneratorService)
        {
            InitializeComponent();

            _authService = authService;
            _exportImportService = exportImportService;
            _cryptoService = cryptoService;
            _loggerService = loggerService;
            _passwordGeneratorService = passwordGeneratorService;
        }

        private void ButtonRegistration_Click(object sender, RoutedEventArgs e)
        {
            string password = MasterPasswordBox.Password;

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль");
                return;
            }

            string confirm = ConfirmPasswordBox.Password;
            if (string.IsNullOrEmpty(confirm) || password != confirm)
            {
                MessageBox.Show("Пароли не совпадают");
                _loggerService.Log($"Ошибка регистрации. Пароли не совпадают");
                return;
            }
            _authService.CreateMasterPassword(password);
            MessageBox.Show("Мастер-пароль создан. Теперь войдите.");

            _loggerService.Log($"Создан мастер пароль");

            // Открываем окно входа в профиль
            var loginWindow = new LoginWindow(_authService, _exportImportService, _cryptoService, _loggerService, _passwordGeneratorService);
            loginWindow.Show();
            this.Close();
        }
    }
}
