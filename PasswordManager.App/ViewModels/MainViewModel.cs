using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Core.Interfaces;
using System.Numerics;

namespace PasswordManager.App.ViewModels
{
    partial class MainViewModel : ObservableObject
    {
        //Сервисы
        private readonly ILoggerService _loggerService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;

        public MainViewModel(ILoggerService loggerService, IPasswordGeneratorService passwordGeneratorService)
        {
            _loggerService = loggerService;
            _passwordGeneratorService = passwordGeneratorService;
        }

        //Свойства
        [ObservableProperty]
        private bool _useUpperCase = false;

        [ObservableProperty]
        private bool _useLowerCase = false;

        [ObservableProperty]
        private bool _useNumbers = false;

        [ObservableProperty]
        private bool _useSymbols = false;

        [ObservableProperty]
        private int _passwordLength = 8;

        [ObservableProperty]
        private string _passwordLengthText = "8";

        [ObservableProperty]
        private string _generatedPassword = "";

        //Команды

        [RelayCommand]
        public void Generate(object sender)
        {
            GeneratedPassword = _passwordGeneratorService.Generate(UseUpperCase, UseLowerCase, UseNumbers, UseSymbols, PasswordLength);
        }

        [RelayCommand]
        public void Log(object sender)
        {
            _loggerService.Log("Сообщение");
        }
    }
}
