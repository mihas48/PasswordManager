using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PasswordManager.Core.Models
{
    public class PasswordEntry : INotifyPropertyChanged
    {
        private readonly Guid _id;
        private string _title;
        private string _login;
        private string _password;
        private string _category;
        private string _notes;
        private readonly DateTime _createdAt;
        private DateTime _updatedAt;
        private bool _isPasswordVisible;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [JsonIgnore]
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (_isPasswordVisible != value)
                {
                    _isPasswordVisible = value;
                    OnPropertyChanged(nameof(IsPasswordVisible));
                    OnPropertyChanged(nameof(DisplayPassword));
                }
            }
        }

        [JsonIgnore]
        public string DisplayPassword
        {
            get
            {
                if (IsPasswordVisible)
                    return Password;
                return new string('*', _password?.Length ?? 0);
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                if (_category != value)
                {
                    _category = string.IsNullOrWhiteSpace(value) ? null : value;
                    OnPropertyChanged(nameof(Category));
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ошибка! Значение имени сервиса не может быть пустым!");
                if (_title != value)
                {
                    _title = value;
                    _updatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(UpdatedAt));
                }
            }
        }

        public string Login
        {
            get => _login;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ошибка! Значение имени пользователя не может быть пустым!");
                if (_login != value)
                {
                    _login = value;
                    _updatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Login));
                    OnPropertyChanged(nameof(UpdatedAt));
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ошибка! Значение пароля не может быть пустым!");
                if (_password != value)
                {
                    _password = value;
                    _updatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Password));
                    OnPropertyChanged(nameof(UpdatedAt));
                    OnPropertyChanged(nameof(DisplayPassword));
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ошибка! Значение заметок не может быть пустым!");
                if (_notes != value)
                {
                    _notes = value;
                    _updatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Notes));
                    OnPropertyChanged(nameof(UpdatedAt));
                }
            }
        }

        public Guid Id => _id;
        public DateTime CreatedAt => _createdAt;
        public DateTime UpdatedAt => _updatedAt;

        public PasswordEntry(string title, string login, string password, string notes)
        {
            Title = title;
            Login = login;
            Password = password;
            Notes = notes;

            _createdAt = DateTime.Now;
            _updatedAt = _createdAt;
            _id = Guid.NewGuid();
        }
    }
}