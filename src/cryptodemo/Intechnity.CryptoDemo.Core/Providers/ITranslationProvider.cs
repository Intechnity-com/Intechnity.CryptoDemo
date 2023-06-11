namespace Intechnity.CryptoDemo.Core.Providers;

public interface ITranslationProvider
{
    string Translate(string input, params object?[] args);
}