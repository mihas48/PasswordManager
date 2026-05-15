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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        IAuthService _authService;
        IExportImportService _exportImportService;
        ICryptoService _cryptoService;
        ILoggerService _loggerService;
        IPasswordGeneratorService _passwordGeneratorService;

        public LoginWindow(IAuthService authService, IExportImportService exportImportService, ICryptoService cryptoService,
            ILoggerService loggerService, IPasswordGeneratorService passwordGeneratorService)
        {
            InitializeComponent();

            _authService = authService;
            _exportImportService = exportImportService;
            _cryptoService = cryptoService;
            _loggerService = loggerService;
            _passwordGeneratorService = passwordGeneratorService;
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            string password = MasterPasswordBox.Password;

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль");
                return;
            }

            // Режим входа
            if (!_authService.ValidateMasterPassword(password))
            {
                MessageBox.Show("Неверный пароль");
                _loggerService.Log($"Ошибка авторизации. Неверный пароль");
                return;
            }

            _loggerService.Log($"Выполнена авторизация");

            // Получаем ключ
            byte[] encryptionKey = _authService.GetEncryptionKey();

            // Загружаем данные
            try
            {
                _exportImportService.LoadData(encryptionKey);
            }
            catch (Exception ex)
            {
                _loggerService.Log($"Ошибка загрузки данных: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
                return;
            }

            // Открываем главное окно
            var mainWindow = new MainWindow(_exportImportService, _passwordGeneratorService, _cryptoService, _loggerService, encryptionKey);
            mainWindow.Show();
            this.Close();
        }
    }
}
