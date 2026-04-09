namespace PasswordManager.Core.Models
{
    public class PasswordEntry
    {
        private readonly Guid _id;
        private string _serviceName;
        private string _login;
        private string _password;
        private string _category;
        private readonly DateTime _createdAt;
        private DateTime _updatedAt;

        public string ServiceName
        {
            get => _serviceName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ошибка! Значение имени сервиса не может быть пустым!");

                _serviceName = value;
                _updatedAt = DateTime.Now;
            }
        }
        public string Login
        {
            get => _login;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ошибка! Значение имени пользователя не может быть пустым!");

                _login = value;
                _updatedAt = DateTime.Now;
            }
        }
        public string Password 
        { 
            get => _password;
            set 
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ошибка! Значение пароля не может быть пустым!");

                _password = value;
                _updatedAt = DateTime.Now;
            }                
        }
        public string Category {
            get => _category;
            set
            {
                _category = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }
        public Guid Id => _id;
        public DateTime CreatedAt => _createdAt;
        public DateTime UpdatedAt => _updatedAt;

        public PasswordEntry(string serviceName, string login, string password)
        {
            ServiceName = serviceName;
            Login = login;
            Password = password;

            _createdAt = DateTime.Now;
            _updatedAt = _createdAt;

            _id = Guid.NewGuid();
        }
    }
}
