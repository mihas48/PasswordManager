namespace PasswordManager.Core.Interfaces
{
    public interface IPasswordGeneratorService
    {
        string Generate(bool useUpperCase, bool useLowerCase, bool useNumbers, bool useSymbols, int passwordLength);
    }
}
