using PasswordManager.Core.Interfaces;
using System.Security.Cryptography;

namespace PasswordManager.Services
{
    public class PasswordGeneratorService : IPasswordGeneratorService
    {
        public string Generate(bool useUpperCase, bool useLowerCase, bool useNumbers, bool useSymbols, int passwordLength)
        {
            string chars = "";
            string generatedPassword;

            if (useUpperCase)
                chars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (useLowerCase)
                chars += "abcdefghijklmnopqrstuvwxyz";

            if (useNumbers)
                chars += "0123456789";

            if (useSymbols)
                chars += "!@#$%^&*()_-+=";

            char[] result = new char[passwordLength];
            byte[] randomBytes = new byte[passwordLength];
            RandomNumberGenerator.Fill(randomBytes);

            for (int i = 0; i < passwordLength; i++)
            {
                result[i] = chars[randomBytes[i] % chars.Length];
            }

            generatedPassword = new string(result);

            return generatedPassword;
        }
    }
}
