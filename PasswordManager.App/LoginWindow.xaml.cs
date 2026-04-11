using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Core.Interfaces;
using PasswordManager.Services;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace PasswordManager.App.Views
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string password = MasterPasswordBox.Password;

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль");
                return;
            }

            // 1. Проверяем, существует ли мастер-пароль
            if (!_authService.IsMasterPasswordSet())
            {
                // Режим регистрации
                string confirm = ConfirmPasswordBox.Password;
                if (string.IsNullOrEmpty(confirm) || password != confirm)
                {
                    MessageBox.Show("Пароли не совпадают");
                    return;
                }
                _authService.CreateMasterPassword(password);
                MessageBox.Show("Мастер-пароль создан. Теперь войдите.");
                return; // не открываем главное окно, нужно повторно ввести пароль
            }

            // 2. Режим входа
            if (!_authService.ValidateMasterPassword(password))
            {
                MessageBox.Show("Неверный пароль");
                return;
            }

            // 3. Получаем ключ
            byte[] encryptionKey = _authService.GetEncryptionKey();

            // 4. Загружаем данные
            try
            {
                _exportImportService.LoadData(encryptionKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
                return;
            }

            // 5. Открываем главное окно
            var mainWindow = new MainWindow(_exportImportService, _passwordGeneratorService, _cryptoService, _loggerService, encryptionKey);
            mainWindow.Show();
            this.Close();
        }
    }
}
